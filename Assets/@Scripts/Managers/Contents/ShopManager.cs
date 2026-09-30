using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;
#if UNITY_ANDROID && !UNITY_EDITOR
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
#endif

// 보상형 광고(설계서 4.6, ADR0044). 광고 제거를 샀으면 광고 없이 바로 보상.
//  안드로이드 기기: AdMob 보상형 — 타이틀에서 Init() 이 UMP 동의(필요한 나라만 양식) → SDK 초기화 → 한 편 미리 로드.
//   광고가 **닫힌 뒤** onDone(보상 받았는가)을 부른다(비동기). 광고 단위가 테스트 ID 인 채면 출시 빌드는 Available=false.
//  에디터·그 밖의 플랫폼: 모의 — 에디터·개발 빌드는 바로 성공, 출시 빌드(스팀 PC)는 광고 없음.
public static class AdService
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static bool s_started, s_initStarted, s_ready, s_loading, s_showing;
    static RewardedAd s_ad;
    static DateTime s_loadedAt;
    const double AD_FRESH_MINUTES = 55;   // 미리 받아 둔 광고는 1시간이 지나면 못 띄운다(CanShowAd 는 만료를 모른다) — 그 전에 새로
    static bool IsTestUnit => ADMOB_REWARDED_ANDROID.StartsWith(ADMOB_TEST_PUBLISHER);

    public static bool Available => Managers.Game.ShopSave.AdFree || (s_ready && (Debug.isDebugBuild || !IsTestUnit));
    public static bool IsShowing => s_showing;
    // 유럽 등 동의가 필요한 곳은 설정을 다시 고를 자리를 둬야 한다(상점 탭 맨 아래 줄)
    public static bool PrivacyOptionsRequired
        => s_started && ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

    public static void Init()
    {
        if (s_started) return;
        // 광고 단위가 아직 구글 테스트 ID 면 출시 빌드는 SDK 를 깨우지 않는다 — 광고 없음·수집 없음(스토어 데이터 보안 A안) 그대로
        if (IsTestUnit && !Debug.isDebugBuild) return;
        s_started = true;
        try
        {
            // 콜백을 메인 스레드로 돌리는 실행기는 MobileAds.Initialize 가 처음 만든다 — 동의(UMP) 콜백은 그보다 먼저 오므로
            //  직접 만들어 둔다. 없으면 새 설치(동의 전)에서 동의 콜백이 큐에만 쌓여 광고가 끝내 시작되지 않는다.
            MobileAdsEventExecutor.Initialize();
#pragma warning disable 0618   // 플러그인 11.x 에서 폐기 예고 — 대체 API(ExecuteInUpdate)를 이 플래그가 대신 불러 준다
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
#pragma warning restore 0618
            ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
            {
                if (updateError != null) Debug.LogWarning("[Ad] 동의 정보 갱신 실패: " + updateError.Message);
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning("[Ad] 동의 양식 실패: " + formError.Message);
                    StartAds();
                });
            });
            StartAds();   // 지난번에 받은 동의로 바로 시작할 수 있으면 기다리지 않는다
        }
        catch (Exception e)
        {
            Debug.LogError("[Ad] 광고 SDK 시작 실패 — 광고 없이 계속한다: " + e);   // 타이틀 로딩을 멈추지 않게
        }
    }

    static void StartAds()
    {
        if (s_initStarted || !ConsentInformation.CanRequestAds()) return;
        s_initStarted = true;
        MobileAds.Initialize(_ => { s_ready = true; Load(); });
    }

    static void Load()
    {
        if (!s_ready || s_loading || s_ad != null) return;
        s_loading = true;
        RewardedAd.Load(ADMOB_REWARDED_ANDROID, new AdRequest(), (ad, error) =>
        {
            s_loading = false;
            if (error != null || ad == null) { Debug.LogWarning("[Ad] 보상형 광고 로드 실패: " + error?.GetMessage()); return; }
            s_ad = ad;
            s_loadedAt = DateTime.UtcNow;
        });
    }

    // 광고를 띄웠으면 true(보상은 닫힌 뒤 onDone), 띄우지 못했으면 false(onDone 은 부르지 않는다 — 부른 쪽이 '불러오는 중'을 알린다)
    public static bool ShowRewarded(string placement, Action<bool> onDone)
    {
        if (Managers.Game.ShopSave.AdFree) { onDone?.Invoke(true); return true; }
        if (s_ad != null && !s_showing && (DateTime.UtcNow - s_loadedAt).TotalMinutes > AD_FRESH_MINUTES)
        {
            s_ad.Destroy();   // 오래 둔 광고 — 띄우면 조용히 실패하므로 버리고 새로 받는다
            s_ad = null;
        }
        if (!Available || s_showing || s_ad == null || !s_ad.CanShowAd()) { Load(); return false; }
        var ad = s_ad;
        s_ad = null;
        s_showing = true;
        bool earned = false, finished = false;
        void Finish()
        {
            if (finished) return;
            finished = true;
            s_showing = false;
            ad.Destroy();
            Load();   // 다음 편을 미리
            onDone?.Invoke(earned);
        }
        ad.OnAdFullScreenContentClosed += Finish;
        ad.OnAdFullScreenContentFailed += e => { Debug.LogWarning("[Ad] 보상형 광고 표시 실패: " + e?.GetMessage()); Finish(); };
        Debug.Log($"[Ad] 보상형 광고 — {placement}");
        ad.Show(_ => earned = true);
        return true;
    }

    public static void ShowPrivacyOptions(Action onDone)
        => ConsentForm.ShowPrivacyOptionsForm(e =>
        {
            if (e != null) Debug.LogWarning("[Ad] 개인정보 설정 양식 실패: " + e.Message);
            onDone?.Invoke();
        });
#else
    public static bool Available => Managers.Game.ShopSave.AdFree || Application.isEditor || Debug.isDebugBuild;
    public static bool IsShowing => false;
    public static bool PrivacyOptionsRequired => false;

    public static void Init() { }

    public static bool ShowRewarded(string placement, Action<bool> onDone)
    {
        if (!Available) return false;
        if (!Managers.Game.ShopSave.AdFree) Debug.Log($"[Ad] 모의 보상형 광고 — {placement}");
        onDone?.Invoke(true);
        return true;
    }

    public static void ShowPrivacyOptions(Action onDone) => onDone?.Invoke();
#endif
}

// 인앱 결제(설계서 4.7). 스토어 결제(Google Play Billing·Steamworks)는 계정이 생긴 뒤(P3).
//  지금은 에디터·개발 빌드만 모의 구매로 즉시 지급하고, 출시 빌드에서는 상품 줄을 숨긴다(Available=false).
public static class IapService
{
    public static bool Available => Application.isEditor || Debug.isDebugBuild;

    public static void Purchase(string productKey, Action<bool> onDone)
    {
        if (!Available) { onDone?.Invoke(false); return; }
        Debug.Log($"[IAP] 모의 구매 — {productKey}");
        onDone?.Invoke(true);
    }
}

// v2 상점·핫타임·월정액(ADR0030, 설계서 4.6·4.7). 표: ProductData(_tools/gen_missions.py), 수치: BalanceConfig.
//  포획 확률을 올리는 상품은 팔지 않는다 — 만드라고라는 크리스탈로도 사지만 무과금 출석·미션에서도 나온다.
//  세이브: GameData.Shop(ShopSaveData). 광고 횟수는 기기 현지 날짜로 하루마다 비운다.
public class ShopManager
{
    public const string AD_HOT = "hot_", AD_FREE_CRYSTAL = "free_crystal", AD_OFFLINE = "offline_double", AD_DUNGEON = "dungeon_", AD_RELIC = "relic";
    const int BUFF_KEY_HOT = -200;   // 버프 아이콘 키 — 물약(-100)과 겹치지 않게, 종류별 -200/-201/-202
    const int HOT_KINDS = 3;

    public event Action OnChanged;

    ShopSaveData Save => Managers.Game.ShopSave;
    static float B(string key, float fallback) => Managers.Data.Balance(key, fallback);

    #region 핫타임 — 광고(하루 3회) 또는 크리스탈로 30분(실제 시간), 켜져 있으면 이어 붙는다
    readonly bool[] _hotWasOn = new bool[HOT_KINDS];

    public bool IsHot(HotTime k) => Save.HotUntil[(int)k] > DateTime.UtcNow.Ticks;
    public TimeSpan HotLeft(HotTime k) => IsHot(k) ? TimeSpan.FromTicks(Save.HotUntil[(int)k] - DateTime.UtcNow.Ticks) : TimeSpan.Zero;
    public float GoldMult => IsHot(HotTime.Gold) ? B(BalanceKey.HOT_GOLD_MULT, 2f) : 1f;
    public float ExpMult  => IsHot(HotTime.Exp)  ? B(BalanceKey.HOT_EXP_MULT, 2f) : 1f;
    public int   HotAdsLeft(HotTime k) => AdLeft(AD_HOT + k, (int)B(BalanceKey.HOTTIME_ADS_PER_DAY, 3f));
    public int   HotCrystalCost => (int)B(BalanceKey.HOTTIME_CRYSTAL_COST, 50f);
    public int   HotMinutes     => (int)B(BalanceKey.HOTTIME_MINUTES, 30f);

    // PlayerController.RefreshStats — 훈련 뒤, 임시 버프 앞
    public void ApplyHotTime(PlayerController p)
    {
        if (p == null || !IsHot(HotTime.Atk)) return;
        float mul = 1f + B(BalanceKey.HOT_ATK_PCT, 20f) / 100f;
        p.PhysicalAtk *= mul;
        p.MagicAtk    *= mul;
    }

    public bool HotByAd(HotTime k) => WatchAd(AD_HOT + k, (int)B(BalanceKey.HOTTIME_ADS_PER_DAY, 3f), () => StartHot(k));

    public bool HotByCrystal(HotTime k)
    {
        if (!SpendCrystal(HotCrystalCost)) return false;
        StartHot(k);
        return true;
    }

    void StartHot(HotTime k)
    {
        long from = Math.Max(DateTime.UtcNow.Ticks, Save.HotUntil[(int)k]);
        Save.HotUntil[(int)k] = from + TimeSpan.FromMinutes(B(BalanceKey.HOTTIME_MINUTES, 30f)).Ticks;
        Managers.Game.SaveGame();
        RaiseHotBuff(k);
        if (k == HotTime.Atk) RefreshPlayer();
        OnChanged?.Invoke();
    }

    // Managers.Update — 끝나는 순간 공격력을 되돌리고 버프 아이콘을 거둔다
    public void Tick()
    {
        if (Managers.Game?.ShopSave == null) return;
        for (int i = 0; i < HOT_KINDS; i++)
        {
            bool on = IsHot((HotTime)i);
            if (on == _hotWasOn[i]) continue;
            _hotWasOn[i] = on;
            if (on) continue;
            var p = Managers.Game.Player;
            if (p != null) p.RaiseBuffCleared(BUFF_KEY_HOT - i);
            if (i == (int)HotTime.Atk) RefreshPlayer();
            OnChanged?.Invoke();
        }
    }

    // GameScene 이 플레이어를 붙인 뒤 — 켜져 있는 핫타임 아이콘을 다시 띄운다
    public void RestoreHotBuffs()
    {
        for (int i = 0; i < HOT_KINDS; i++)
            if (IsHot((HotTime)i)) RaiseHotBuff((HotTime)i);
    }

    void RaiseHotBuff(HotTime k)
    {
        var p = Managers.Game.Player;
        if (p == null) return;
        p.RaiseBuffApplied(new PlayerController.BuffInfo(BUFF_KEY_HOT - (int)k, HOT_TIME_ICONS[(int)k], (float)HotLeft(k).TotalSeconds, true));
    }

    static void RefreshPlayer()
    {
        var p = Managers.Game.Player;   // Player 는 Unity == 로 파괴 여부까지 본다
        if (p != null) p.RefreshStats(fullHeal: false);
    }
    #endregion

    #region 광고
    public int AdLeft(string placement, int perDay)
    {
        RollAdDay();
        Save.AdViews.TryGetValue(placement, out int seen);
        return Math.Max(0, perDay - seen);
    }

    // 광고를 보고 보상 — 한도가 남아 있을 때만. 돌려주는 값은 '광고를 띄웠는가'이고 보상(onReward)은 광고가 **닫힌 뒤** 온다
    //  (AdMob 은 비동기, 모의·광고 제거는 그 자리에서). 요일 던전 입장·유물 소환도 onReward 안에서 진행한다(ADR0044).
    //  하루 횟수 세기·광고 제거 상품이 한곳에서 처리된다.
    public bool WatchAd(string placement, int perDay, Action onReward)
    {
        if (!AdService.Available || AdService.IsShowing || AdLeft(placement, perDay) <= 0) return false;
        return AdService.ShowRewarded(placement, success =>
        {
            if (!success) return;   // 끝까지 보지 않고 닫았다
            Save.AdViews.TryGetValue(placement, out int seen);
            Save.AdViews[placement] = seen + 1;
            onReward();
            Managers.Game.SaveGame();
            Managers.Sound?.PlaySFX("SFX_Success2");
            OnChanged?.Invoke();
        });
    }

    public int  FreeCrystalAmount  => (int)B(BalanceKey.FREE_CRYSTAL_AD, 30f);
    public int  FreeCrystalAdsLeft => AdLeft(AD_FREE_CRYSTAL, (int)B(BalanceKey.FREE_CRYSTAL_ADS_PER_DAY, 1f));
    public bool FreeCrystalByAd() => WatchAd(AD_FREE_CRYSTAL, (int)B(BalanceKey.FREE_CRYSTAL_ADS_PER_DAY, 1f),
                                             () => Managers.Game.Crystal += FreeCrystalAmount);

    void RollAdDay()
    {
        int today = MissionManager.DayKey(DateTime.Now);
        if (Save.AdDayKey == today) return;
        Save.AdDayKey = today;
        Save.AdViews.Clear();
    }
    #endregion

    #region 오프라인 보상 2배(광고) — 정산마다 한 번
    GameManager.OfflineReward _lastOffline;
    bool _offlineDoubled = true;

    public bool OfflineDoubleAvailable => AdService.Available && !_offlineDoubled && _lastOffline.Kills > 0;

    public void OnOfflineClaimed(GameManager.OfflineReward r)   // GameManager.ClaimOfflineReward
    {
        _lastOffline = r;
        _offlineDoubled = false;
        OnChanged?.Invoke();
    }

    public bool DoubleOffline()
    {
        if (!OfflineDoubleAvailable) return false;
        return WatchAd(AD_OFFLINE, int.MaxValue, () =>
        {
            _offlineDoubled = true;
            float extra = B(BalanceKey.OFFLINE_AD_MULT, 2f) - 1f;
            Managers.Game.Gold += Mathf.RoundToInt(_lastOffline.Gold * extra);
            var p = Managers.Game.Player;
            if (p != null) p.GainExp(_lastOffline.Exp * extra);
            // 비번 일정(ADR0041) — 채집한 재료·마을 일 호감도도 같은 배수
            if (_lastOffline.Items != null)
                foreach (var kv in _lastOffline.Items) Managers.Game.AddItem(kv.Key, Mathf.RoundToInt(kv.Value * extra));
            if (_lastOffline.BondEach > 0) Managers.Bond?.AddToAll(Mathf.RoundToInt(_lastOffline.BondEach * extra));
        });
    }
    #endregion

    #region 크리스탈 사용
    public int MandrakeCost => (int)B(BalanceKey.MANDRAKE_CRYSTAL_COST, 50f);

    public bool BuyMandrake()
    {
        if (!SpendCrystal(MandrakeCost)) return false;
        Managers.Game.MandrakeCount += 1;
        OnChanged?.Invoke();
        return true;
    }

    public static bool SpendCrystal(int cost)
    {
        if (Managers.Game.Crystal < cost) return false;
        Managers.Game.Crystal -= cost;
        return true;
    }
    #endregion

    #region 인앱 상품
    public List<Data.ProductData> Products
    {
        get
        {
            var list = new List<Data.ProductData>(Managers.Data.ProductDic.Values);
            list.Sort((a, b) => a.DataId.CompareTo(b.DataId));
            return list;
        }
    }

    public bool IsBought(Data.ProductData p)       => Save.Purchased.Contains(p.ProductKey);
    public bool IsFirstBuyBonus(Data.ProductData p) => p.Kind == "Crystal" && !IsBought(p);

    public bool CanBuy(Data.ProductData p)
    {
        switch (p.Kind)
        {
            case "Pack":
            case "Pass":    return !IsBought(p);   // 성장 패스(ADR0039) — 한 번 사면 프리미엄 줄이 영구히 열린다
            case "StepUp":  return !IsBought(p) && PreviousStepBought(p);
            case "Monthly": return !MonthlyActive;
            case "AdFree":  return !Save.AdFree;
            default:        return true;
        }
    }

    bool PreviousStepBought(Data.ProductData p)
    {
        Data.ProductData prev = null;
        foreach (var q in Products)
            if (q.Kind == "StepUp" && q.DataId < p.DataId) prev = q;
        return prev == null || IsBought(prev);
    }

    public void Purchase(Data.ProductData p, Action<bool> onDone)
    {
        if (!CanBuy(p)) { onDone?.Invoke(false); return; }
        IapService.Purchase(p.ProductKey, ok =>
        {
            if (ok) Grant(p);
            onDone?.Invoke(ok);
        });
    }

    void Grant(Data.ProductData p)
    {
        int crystal = p.Crystal;
        if (IsFirstBuyBonus(p)) crystal = Mathf.RoundToInt(crystal * B(BalanceKey.CRYSTAL_FIRST_BUY_MULT, 2f));
        if (!Save.Purchased.Contains(p.ProductKey)) Save.Purchased.Add(p.ProductKey);
        if (crystal > 0)    Managers.Game.Crystal += crystal;
        if (p.Mandrake > 0) Managers.Game.MandrakeCount += p.Mandrake;
        if (p.Kind == "Monthly")
        {
            Save.MonthlyUntil    = DateTime.UtcNow.AddDays(B(BalanceKey.MONTHLY_DAYS, 30f)).Ticks;
            Save.MonthlyClaimKey = 0;
        }
        if (p.Kind == "AdFree") Save.AdFree = true;
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
    }
    #endregion

    #region 월정액 — 즉시 300 + 매일 100(30일) + 오프라인 정산 상한 +4시간
    public bool  MonthlyActive       => Save.MonthlyUntil > DateTime.UtcNow.Ticks;
    public int   MonthlyDaysLeft     => MonthlyActive ? (int)Math.Ceiling((new DateTime(Save.MonthlyUntil, DateTimeKind.Utc) - DateTime.UtcNow).TotalDays) : 0;
    public bool  CanClaimMonthly     => MonthlyActive && Save.MonthlyClaimKey != MissionManager.DayKey(DateTime.Now);
    public int   MonthlyDailyCrystal => (int)B(BalanceKey.MONTHLY_DAILY_CRYSTAL, 100f);
    public float OfflineBonusHours   => MonthlyActive ? B(BalanceKey.MONTHLY_OFFLINE_BONUS_HOURS, 4f) : 0f;

    public bool ClaimMonthly()
    {
        if (!CanClaimMonthly) return false;
        Save.MonthlyClaimKey = MissionManager.DayKey(DateTime.Now);
        Managers.Game.Crystal += MonthlyDailyCrystal;
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }
    #endregion

    // HUD 상점 버튼의 알림 — 공짜로 받을 것(무료 크리스탈 광고·월정액 매일·오프라인 2배)이 있으면 true
    public bool HasFreeReward => (AdService.Available && FreeCrystalAdsLeft > 0) || CanClaimMonthly || OfflineDoubleAvailable;
}
