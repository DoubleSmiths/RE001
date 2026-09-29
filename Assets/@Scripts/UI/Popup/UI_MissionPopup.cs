using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

// v2 미션·출석 창(ADR0030, 설계서 4.5) — HUD 재화 줄의 미션 버튼으로 연다(빌더: Tools/Remake V2/Build Mission Popup).
//  틀은 마을 상점 창(UI_ShopPopup)을 복제했고, 줄은 상태 창의 긴 버튼을 복제한 MissionRowTemplate 을 필요한 만큼 찍는다.
//  탭: 일일 / 주간 / 가이드(받지 않은 첫 단계 + 다음 두 단계 미리보기) / 출석(7칸).
//  줄을 누르면 받는다(받을 수 있을 때만). 진행·주기 초기화는 MissionManager 가 맡는다.
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

    public enum Tab { Daily, Weekly, Guide, Attendance }

    static readonly string[] TAB_KEYS = { "MissionDailyTabText", "MissionWeeklyTabText", "MissionGuideTabText", "MissionAttendanceTabText" };
    static readonly Color TAB_ON      = new Color32(0xFF, 0xD2, 0x4A, 0xFF);
    static readonly Color TAB_OFF     = new Color32(0xB8, 0xB8, 0xB8, 0xFF);
    static readonly Color TEXT_READY  = new Color32(0xFF, 0xD2, 0x4A, 0xFF);   // 받기
    static readonly Color TEXT_REWARD = new Color32(0x7F, 0xE0, 0xFF, 0xFF);   // 진행 중 보상
    static readonly Color TEXT_DONE   = new Color32(0x8A, 0x8A, 0x8A, 0xFF);   // 받음
    const float ROW_DONE_ALPHA   = 0.55f;
    const int   GUIDE_PREVIEW    = 2;       // 가이드 다음 단계 미리보기 수

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
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += Refresh;
        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Mission != null) Managers.Mission.OnChanged -= Refresh;
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= Refresh;
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

        GetText((int)Texts.title).text = Util.Localize("MissionTitleText");
        GetText((int)Texts.ClaimAllTxt).text = Util.Localize("MissionClaimAllText");
        for (int i = 0; i < _tabTexts.Count; i++)
        {
            _tabTexts[i].text  = Util.Localize(TAB_KEYS[i]);
            _tabTexts[i].color = (int)_tab == i ? TAB_ON : TAB_OFF;
        }

        int n = 0;
        switch (_tab)
        {
            case Tab.Daily:  foreach (var m in mission.List(MissionManager.DAILY))  MissionRow(n++, m); break;
            case Tab.Weekly: foreach (var m in mission.List(MissionManager.WEEKLY)) MissionRow(n++, m); break;
            case Tab.Guide:  n = GuideRows(); break;
            case Tab.Attendance:
                for (int d = 0; d < MissionManager.ATTENDANCE_DAYS; d++) AttendanceRow(n++, d);
                break;
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
