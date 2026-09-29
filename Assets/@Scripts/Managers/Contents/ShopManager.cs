using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// 보상형 광고(설계서 4.6). 지금은 모의다 — 에디터·개발 빌드는 바로 성공한다.
//  출시 빌드는 광고 SDK 를 붙이기 전까지 광고 자리를 숨긴다(Available=false). 광고 제거를 샀으면 광고 없이 즉시 보상.
public static class AdService
{
    public static bool Available => Managers.Game.ShopSave.AdFree || Application.isEditor || Debug.isDebugBuild;

    public static void ShowRewarded(string placement, Action<bool> onDone)
    {
        if (!Available) { onDone?.Invoke(false); return; }
        if (!Managers.Game.ShopSave.AdFree) Debug.Log($"[Ad] 모의 보상형 광고 — {placement}");
        onDone?.Invoke(true);
    }
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
    public const string AD_HOT = "hot_", AD_FREE_CRYSTAL = "free_crystal", AD_OFFLINE = "offline_double", AD_DUNGEON = "dungeon_";
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

    // 광고를 보고 보상 — 한도가 남아 있을 때만. 모의 광고는 즉시 끝나 결과를 바로 돌려준다(실제 SDK 는 OnChanged 로 본다).
    //  요일 던전의 광고 입장(DungeonManager)도 이 길을 쓴다 — 하루 횟수 세기·광고 제거 상품이 한곳에서 처리된다.
    public bool WatchAd(string placement, int perDay, Action onReward)
    {
        if (!AdService.Available || AdLeft(placement, perDay) <= 0) return false;
        bool rewarded = false;
        AdService.ShowRewarded(placement, success =>
        {
            if (!success) return;
            Save.AdViews.TryGetValue(placement, out int seen);
            Save.AdViews[placement] = seen + 1;
            rewarded = true;
            onReward();
            Managers.Game.SaveGame();
            OnChanged?.Invoke();
        });
        return rewarded;
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
            case "Pack":    return !IsBought(p);
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
