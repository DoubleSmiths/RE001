using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

// v2 미션·출석 창(ADR0030, 설계서 4.5) — HUD 재화 줄의 미션 버튼으로 연다(빌더: Tools/Remake V2/Build Mission Popup).
//  틀은 마을 상점 창(UI_ShopPopup)을 복제했고, 줄은 상태 창의 긴 버튼을 복제한 MissionRowTemplate 을 필요한 만큼 찍는다.
//  탭: 일일 / 주간 / 가이드(받지 않은 첫 단계 + 다음 두 단계 미리보기) / 출석(7칸) / 상점.
//  줄을 누르면 받는다(받을 수 있을 때만). 진행·주기 초기화는 MissionManager, 상점·광고·핫타임은 ShopManager 가 맡는다.
//  상점 탭: 핫타임 3종(광고가 남았으면 광고, 아니면 크리스탈), 무료 크리스탈·오프라인 2배(광고), 월정액 매일 보상,
//  만드라고라(크리스탈), 인앱 상품. 광고·결제 줄은 모의가 되는 빌드(에디터·개발)에서만 보인다(AdService/IapService).
//  던전 탭(ADR0034): 요일 던전 3종 — 무료 입장이 남았으면 입장, 없으면 광고, 그다음 크리스탈. 누르면 창을 닫고 전투로 간다.
public class UI_MissionPopup : UI_Popup
{
    #region Enum
    enum Images
    {
        MissionPopupFrame,
        Xbutton,
    }

    enum Texts
    {
        title,
        ClaimAllTxt,
    }

    enum Buttons
    {
        MissionPopupBackground,
        ClaimAllBtn,
    }

    enum GameObjects
    {
        Content,
        MissionTabs,
        MissionRowTemplate,
    }
    #endregion

    public enum Tab { Daily, Weekly, Guide, Attendance, Shop, Dungeon }   // 끝에만 붙인다 — 바로가기 버튼이 번호로 저장한다

    static readonly string[] TAB_KEYS = { "MissionDailyTabText", "MissionWeeklyTabText", "MissionGuideTabText", "MissionAttendanceTabText", "MissionShopTabText", "MissionDungeonTabText" };
    static readonly string[] HOT_KEYS = { "HotGoldText", "HotExpText", "HotAtkText" };   // HotTime 순서
    static readonly Color TAB_ON      = new Color32(0xFF, 0xD2, 0x4A, 0xFF);
    static readonly Color TAB_OFF     = new Color32(0xB8, 0xB8, 0xB8, 0xFF);
    static readonly Color TEXT_READY  = new Color32(0xFF, 0xD2, 0x4A, 0xFF);   // 받기
    static readonly Color TEXT_REWARD = new Color32(0x7F, 0xE0, 0xFF, 0xFF);   // 진행 중 보상
    static readonly Color TEXT_DONE   = new Color32(0x8A, 0x8A, 0x8A, 0xFF);   // 받음
    const float ROW_DONE_ALPHA   = 0.55f;
    const int   GUIDE_PREVIEW    = 2;       // 가이드 다음 단계 미리보기 수
    const float SHOP_TICK        = 1f;      // 상점 탭: 핫타임 남은 시간 갱신 주기(초)
    const string TAB_ALERT       = "<color=#FF6A6A>!</color>";
    float _shopTick;

    Tab _tab;
    GameObject _template;
    readonly List<GameObject> _rows = new List<GameObject>();
    readonly List<Action> _rowActions = new List<Action>();
    readonly List<TMP_Text> _tabTexts = new List<TMP_Text>();

    void Awake() => Init();

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.MissionPopupBackground)?.gameObject.BindEvent(OnClickClose);
        GetImage((int)Images.Xbutton)?.gameObject.BindEvent(OnClickClose);
        GetButton((int)Buttons.ClaimAllBtn).gameObject.BindEvent(OnClickClaimAll);
        ApplyFont(GetText((int)Texts.title), FontType.UI);
        ApplyFont(GetText((int)Texts.ClaimAllTxt), FontType.UI);

        Transform tabs = GetObject((int)GameObjects.MissionTabs).transform;
        for (int i = 0; i < tabs.childCount; i++)
        {
            var tab = (Tab)i;
            tabs.GetChild(i).gameObject.BindEvent(() => SelectTab(tab));
            var text = tabs.GetChild(i).GetComponentInChildren<TMP_Text>(true);
            ApplyFont(text, FontType.UI);
            _tabTexts.Add(text);
        }

        _template = GetObject((int)GameObjects.MissionRowTemplate);
        _template.SetActive(false);

        if (Managers.Mission != null) Managers.Mission.OnChanged += Refresh;
        if (Managers.Shop != null) Managers.Shop.OnChanged += Refresh;
        if (Managers.Dungeon != null) Managers.Dungeon.OnChanged += Refresh;
        if (Managers.Game != null)
        {
            Managers.Game.OnLanguageChanged += Refresh;
            Managers.Game.OnResourceChanged += Refresh;   // 크리스탈이 모자라다/충분하다 표시
        }
        return true;
    }

    void Update()
    {
        if (_tab != Tab.Shop) return;
        _shopTick -= Time.unscaledDeltaTime;
        if (_shopTick > 0f) return;
        _shopTick = SHOP_TICK;
        Refresh();   // 핫타임 남은 시간
    }

    public override void OnDestroy()
    {
        if (Managers.Mission != null) Managers.Mission.OnChanged -= Refresh;
        if (Managers.Shop != null) Managers.Shop.OnChanged -= Refresh;
        if (Managers.Dungeon != null) Managers.Dungeon.OnChanged -= Refresh;
        if (Managers.Game != null)
        {
            Managers.Game.OnLanguageChanged -= Refresh;
            Managers.Game.OnResourceChanged -= Refresh;
        }
        base.OnDestroy();
    }

    public void SetInfo(Tab tab)
    {
        _tab = tab;
        Refresh();
    }

    void SelectTab(Tab tab)
    {
        if (_tab == tab) return;
        _tab = tab;
        Managers.Sound?.PlaySFX("SFX_BtnSelect");
        Refresh();
    }

    void OnClickClose() => Managers.UI.ClosePopupUI(this);

    void Refresh()
    {
        var mission = Managers.Mission;
        if (mission == null) return;

        GetText((int)Texts.title).text = Util.Localize(_tab == Tab.Shop ? "ShopTitleText" : _tab == Tab.Dungeon ? "DungeonTitleText" : "MissionTitleText");
        GetText((int)Texts.ClaimAllTxt).text = Util.Localize("MissionClaimAllText");
        // 상점·던전 탭에는 '모두 받기'가 없다 — 줄 동작이 광고 보기·크리스탈 소비·결제·입장이라 한꺼번에 누르면 안 된다
        GetButton((int)Buttons.ClaimAllBtn).gameObject.SetActive(_tab != Tab.Shop && _tab != Tab.Dungeon);
        for (int i = 0; i < _tabTexts.Count && i < TAB_KEYS.Length; i++)
        {
            _tabTexts[i].text  = Util.Localize(TAB_KEYS[i]);
            _tabTexts[i].color = (int)_tab == i ? TAB_ON : TAB_OFF;
        }
        // 오늘 무료 입장이 남은 던전이 있으면 탭 이름 옆에 빨간 '!'
        if ((int)Tab.Dungeon < _tabTexts.Count && Managers.Dungeon != null && Managers.Dungeon.HasFreeEntry)
            _tabTexts[(int)Tab.Dungeon].text += TAB_ALERT;

        int n = 0;
        switch (_tab)
        {
            case Tab.Daily:  foreach (var m in mission.List(MissionManager.DAILY))  MissionRow(n++, m); break;
            case Tab.Weekly: foreach (var m in mission.List(MissionManager.WEEKLY)) MissionRow(n++, m); break;
            case Tab.Guide:  n = GuideRows(); break;
            case Tab.Attendance:
                for (int d = 0; d < MissionManager.ATTENDANCE_DAYS; d++) AttendanceRow(n++, d);
                break;
            case Tab.Shop: n = ShopRows(); break;
            case Tab.Dungeon: n = DungeonRows(); break;
        }
        for (int i = n; i < _rows.Count; i++) _rows[i].SetActive(false);

        // 받을 것이 없으면 '모두 받기'를 흐리게
        bool any = false;
        for (int i = 0; i < n; i++) any |= _rowActions[i] != null;
        Util.GetOrAddComponent<CanvasGroup>(GetButton((int)Buttons.ClaimAllBtn).gameObject).alpha = any ? 1f : ROW_DONE_ALPHA;
    }

    // 지금 탭에서 받을 수 있는 줄을 전부 받는다(가이드는 받으면 다음 단계가 열리니 더 없을 때까지).
    void OnClickClaimAll()
    {
        if (_tab == Tab.Shop || _tab == Tab.Dungeon) return;
        bool got = false;
        for (int guard = 0; guard < 64; guard++)
        {
            Action next = null;
            for (int i = 0; i < _rowActions.Count && i < _rows.Count; i++)
                if (_rows[i].activeSelf && _rowActions[i] != null) { next = _rowActions[i]; break; }
            if (next == null) break;
            next();   // 받으면 OnChanged → Refresh 로 줄이 다시 그려진다
            got = true;
        }
        if (!got) Managers.Sound?.PlaySFX("SFX_Fail");
    }

    void MissionRow(int index, Data.MissionData m)
    {
        var mission = Managers.Mission;
        int progress = Mathf.Min(mission.Progress(m), m.Goal);
        bool claimed = mission.IsClaimed(m), done = mission.IsDone(m);
        string left = $"{Title(m)}  ({progress}/{m.Goal})";
        if (claimed)   SetRow(index, left, Util.Localize("MissionDoneText"), TEXT_DONE, ROW_DONE_ALPHA, null);
        else if (done) SetRow(index, left, Util.Localize("MissionClaimText"), TEXT_READY, 1f, () => Claim(m));
        else           SetRow(index, left, Reward(m.RewardType, m.RewardValue), TEXT_REWARD, 1f, null);
    }

    int GuideRows()
    {
        var mission = Managers.Mission;
        var current = mission.CurrentGuide;
        if (current == null)
        {
            SetRow(0, Util.Localize("MissionGuideDoneText"), "", TEXT_DONE, 1f, null);
            return 1;
        }
        MissionRow(0, current);
        int n = 1;
        foreach (var m in mission.List(MissionManager.GUIDE))
        {
            if (n > GUIDE_PREVIEW) break;
            if (m.Order <= current.Order) continue;
            SetRow(n++, $"{Util.Localize("MissionNextText")}  {Title(m)}", Reward(m.RewardType, m.RewardValue), TEXT_DONE, ROW_DONE_ALPHA, null);
        }
        return n;
    }

    void AttendanceRow(int index, int day)
    {
        var mission = Managers.Mission;
        var m = mission.AttendanceAt(day);
        if (m == null) return;
        string left = $"{string.Format(Util.Localize(m.TitleKey), day + 1)}  {Reward(m.RewardType, m.RewardValue)}";
        int next = mission.AttendanceIndex;
        bool canAttend = mission.CanAttend;
        if (day == next && canAttend) SetRow(index, left, Util.Localize("MissionClaimText"), TEXT_READY, 1f, Attend);
        else if (day < next)          SetRow(index, left, Util.Localize("AttendanceClaimedText"), TEXT_DONE, ROW_DONE_ALPHA, null);
        else if (day == next)         SetRow(index, left, Util.Localize("AttendanceTomorrowText"), TEXT_REWARD, 1f, null);
        else                          SetRow(index, left, "", TEXT_REWARD, 1f, null);
    }

    #region 상점 탭
    int ShopRows()
    {
        var shop = Managers.Shop;
        int n = 0;

        foreach (HotTime k in Enum.GetValues(typeof(HotTime)))
        {
            // 꺼져 있으면 '골드 2배 30분', 켜져 있으면 '골드 2배 (남은 29:51)' — 영어 줄이 너무 작아지지 않게
            string left = Util.Localize(HOT_KEYS[(int)k]) + " " + (shop.IsHot(k)
                ? string.Format(Util.Localize("HotLeftFormat"), Clock(shop.HotLeft(k)))
                : string.Format(Util.Localize("HotDurationFormat"), shop.HotMinutes));
            int ads = shop.HotAdsLeft(k);
            var kind = k;
            if (AdService.Available && ads > 0)
                SetRow(n++, left, string.Format(Util.Localize("AdWatchFormat"), ads), TEXT_READY, 1f, () => Done(shop.HotByAd(kind)));
            else
                SetRow(n++, left, Crystal(shop.HotCrystalCost), CrystalColor(shop.HotCrystalCost), 1f, () => Done(shop.HotByCrystal(kind)));
        }

        if (AdService.Available)
        {
            int free = shop.FreeCrystalAdsLeft;
            string freeLeft = string.Format(Util.Localize("FreeCrystalFormat"), shop.FreeCrystalAmount);
            if (free > 0) SetRow(n++, freeLeft, string.Format(Util.Localize("AdWatchFormat"), free), TEXT_READY, 1f, () => Done(shop.FreeCrystalByAd()));
            else          SetRow(n++, freeLeft, Util.Localize("AttendanceTomorrowText"), TEXT_DONE, ROW_DONE_ALPHA, null);
            if (shop.OfflineDoubleAvailable)
                SetRow(n++, Util.Localize("OfflineDoubleText"), string.Format(Util.Localize("AdWatchFormat"), 1), TEXT_READY, 1f, () => Done(shop.DoubleOffline()));
        }

        if (shop.MonthlyActive)
        {
            string left = string.Format(Util.Localize("MonthlyDailyFormat"), shop.MonthlyDailyCrystal, shop.MonthlyDaysLeft);
            if (shop.CanClaimMonthly) SetRow(n++, left, Util.Localize("MissionClaimText"), TEXT_READY, 1f, () => Done(shop.ClaimMonthly()));
            else                      SetRow(n++, left, Util.Localize("AttendanceTomorrowText"), TEXT_DONE, ROW_DONE_ALPHA, null);
        }

        SetRow(n++, string.Format(Util.Localize("MandrakeBuyFormat"), Managers.Game.MandrakeCount),
               Crystal(shop.MandrakeCost), CrystalColor(shop.MandrakeCost), 1f, () => Done(shop.BuyMandrake()));

        if (IapService.Available)
            foreach (var p in shop.Products) ProductRow(n++, p);
        return n;
    }

    void ProductRow(int index, Data.ProductData p)
    {
        var shop = Managers.Shop;
        string name = p.Kind == "Crystal" ? string.Format(Util.Localize(p.NameKey), p.Crystal) : Util.Localize(p.NameKey);
        var parts = new List<string>();
        if (p.Kind != "Crystal" && p.Crystal > 0) parts.Add(Reward("Crystal", p.Crystal));
        if (p.Mandrake > 0) parts.Add(Reward("Mandrake", p.Mandrake));
        if (shop.IsFirstBuyBonus(p)) parts.Add(Util.Localize("FirstBuyBonusText"));
        string left = parts.Count > 0 ? $"{name}  ·  {string.Join(" · ", parts)}" : name;

        if (!shop.CanBuy(p))
        {
            string state = p.Kind == "Monthly" && shop.MonthlyActive ? string.Format(Util.Localize("MonthlyLeftFormat"), shop.MonthlyDaysLeft)
                         : shop.IsBought(p) || p.Kind == "AdFree"          ? Util.Localize("OwnedText")
                         :                                                   Util.Localize("StepLockedText");   // 스텝업 — 앞 단계를 먼저
            SetRow(index, left, state, TEXT_DONE, ROW_DONE_ALPHA, null);
            return;
        }
        SetRow(index, left, string.Format(Util.Localize("PriceFormat"), p.PriceKRW), TEXT_READY, 1f, () => Buy(p));
    }

    void Buy(Data.ProductData p)
    {
        Managers.Shop.Purchase(p, ok =>
        {
            if (!ok) { Managers.UI.ShowToast(Util.Localize("ShopNotReadyText")); return; }
            Managers.Sound?.PlaySFX("SFX_Success2");
            string name = p.Kind == "Crystal" ? string.Format(Util.Localize(p.NameKey), p.Crystal) : Util.Localize(p.NameKey);
            Managers.UI.ShowToast(string.Format(Util.Localize("ShopBoughtFormat"), name));
        });
    }

    // 상점 줄을 누른 결과 — 크리스탈이 모자라 실패하면 알린다
    void Done(bool ok)
    {
        if (ok) { Managers.Sound?.PlaySFX("SFX_Success2"); return; }
        Managers.Sound?.PlaySFX("SFX_Fail");
        Managers.UI.ShowToast(Util.Localize("NotEnoughCrystalText"));
    }

    static string Crystal(int cost) => string.Format(Util.Localize("RewardCrystalFormat"), cost);
    static Color CrystalColor(int cost) => Managers.Game.Crystal >= cost ? TEXT_REWARD : TEXT_DONE;
    static string Clock(TimeSpan t) => t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes:00}:{t.Seconds:00}";
    #endregion

    #region 던전 탭 (ADR0034)
    int DungeonRows()
    {
        var d = Managers.Dungeon;
        int n = 0;
        foreach (DungeonKind k in Enum.GetValues(typeof(DungeonKind)))
        {
            string left = Util.Localize(DungeonManager.NAME_KEYS[(int)k]);
            if (k == DungeonManager.Today) left += "  " + string.Format(Util.Localize("DungeonTodayFormat"), d.Mult(k));
            if (d.Best(k) > 0)             left += "  " + string.Format(Util.Localize("DungeonBestFormat"), d.Best(k));
            var kind = k;
            int free = d.FreeLeft(k), ads = d.AdLeft(k);
            if (free > 0)
                SetRow(n++, left, string.Format(Util.Localize("DungeonFreeFormat"), free, d.FreePerDay), TEXT_READY, 1f, () => EnterDungeon(kind, DungeonManager.Pay.Free));
            else if (AdService.Available && ads > 0)
                SetRow(n++, left, string.Format(Util.Localize("AdWatchFormat"), ads), TEXT_READY, 1f, () => EnterDungeon(kind, DungeonManager.Pay.Ad));
            else
                SetRow(n++, left, Crystal(d.CrystalCost), CrystalColor(d.CrystalCost), 1f, () => EnterDungeon(kind, DungeonManager.Pay.Crystal));
        }
        SetRow(n++, string.Format(Util.Localize("DungeonHelpFormat"), Mathf.RoundToInt(d.TimeLimit)), "", TEXT_DONE, 1f, null);
        return n;
    }

    // 값을 치르고 창을 닫은 뒤 전투로 — 새 전투 그룹이 뜨면서 던전이 시작된다(UI_BattleBtnGroup.CoDungeon)
    void EnterDungeon(DungeonKind k, DungeonManager.Pay pay)
    {
        var scene = Managers.UI.SceneUI as UI_GameScene;
        if (scene == null || !scene.CanEnterAreaTransition(AreaType.Battle) || !Managers.Dungeon.TryEnter(k, pay))
        {
            Managers.Sound?.PlaySFX("SFX_Fail");
            Managers.UI.ShowToast(Util.Localize(pay == DungeonManager.Pay.Crystal && Managers.Game.Crystal < Managers.Dungeon.CrystalCost
                ? "NotEnoughCrystalText" : "DungeonBusyText"));
            return;
        }
        Managers.Sound?.PlaySFX("SFX_Success2");
        Managers.UI.CloseAllPopupUI();
        scene.EnterAreaWithFade(AreaType.Battle);
    }
    #endregion

    void Claim(Data.MissionData m)
    {
        if (Managers.Mission.TryClaim(m)) Managers.Sound?.PlaySFX("SFX_Success2");
    }

    void Attend()
    {
        if (Managers.Mission.TryAttend()) Managers.Sound?.PlaySFX("SFX_Success2");
    }

    static string Title(Data.MissionData m) => string.Format(Util.Localize(m.TitleKey), m.Goal);

    public static string Reward(string type, int value) => type switch
    {
        "Mandrake" => string.Format(Util.Localize("RewardMandrakeFormat"), value),
        "Gold"     => string.Format(Util.Localize("RewardGoldFormat"), value),
        _          => string.Format(Util.Localize("RewardCrystalFormat"), value),
    };

    // 줄은 템플릿 복제본을 재사용한다. 누름은 줄마다 한 번만 걸고, 그 줄의 현재 동작(_rowActions)을 부른다.
    void SetRow(int index, string left, string right, Color rightColor, float alpha, Action onClick)
    {
        while (_rows.Count <= index)
        {
            int i = _rows.Count;
            var go = Instantiate(_template, GetObject((int)GameObjects.Content).transform);
            go.BindEvent(() => _rowActions[i]?.Invoke());
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true)) ApplyFont(t, FontType.UI);
            _rows.Add(go);
            _rowActions.Add(null);
        }

        var row = _rows[index];
        row.SetActive(true);
        var texts = row.GetComponentsInChildren<TMP_Text>(true);   // [0] 왼쪽(제목·진행) [1] 오른쪽(상태·보상)
        texts[0].text = left;
        texts[1].text = right;
        texts[1].color = rightColor;
        Util.GetOrAddComponent<CanvasGroup>(row).alpha = alpha;
        _rowActions[index] = onClick;
    }
}
