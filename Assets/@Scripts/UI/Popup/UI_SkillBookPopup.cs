using System.Collections.Generic;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [능력 목록] 팝업 (기획서 06.05 슬40~42 + 능력 시스템 슬38~39).
// 보유 능력 목록 + 기억력(used/max) 표시. 슬롯 클릭 → [능력 정보창](기억/망각은 거기서).
//  - 능력 데이터/회계는 AbilityManager. 슬롯은 prefab 정적(SkillBookSkill_0~6).
//  - 슬42: 필터 = 능력 종류(전체/기술/마법/수비/지원). 잠금(미보유)은 목록에 없음(슬41).
public class UI_SkillBookPopup : UI_Popup
{
    #region Enum

    enum Texts
    {
        title,
        SubtitleText,
        MemoryAmountTxt_1,   // 기억력 게이지 "used/max"
    }

    enum Buttons
    {
        SkillPopupBackground,
        Btn_SkillBookSkill_0_07,
        Btn_SkillBookSkill_1_02,
        Btn_SkillBookSkill_2_09,
        Btn_SkillBookSkill_3_10,
        Btn_SkillBookSkill_4_05,
        Btn_SkillBookSkill_5_01,
        Btn_SkillBookSkill_6_03,
        SkillPopup_ShowAll_Btn,
        SkillPopup_ShowTalent_Btn,
        SkillPopup_ShowOnMemory_Btn,
        SkillPopup_ShowNotOnMemory_Btn,
        SkillPopup_ShowLocked_Btn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        SkillBookPopup,
        Content,
        SkillBookSkill_0,
        SkillBookSkill_1,
        SkillBookSkill_2,
        SkillBookSkill_3,
        SkillBookSkill_4,
        SkillBookSkill_5,
        SkillBookSkill_6,
    }

    #endregion

    const int SLOT_COUNT = 7;
    static readonly int[] SLOT_ROOT =
    {
        (int)GameObjects.SkillBookSkill_0, (int)GameObjects.SkillBookSkill_1,
        (int)GameObjects.SkillBookSkill_2, (int)GameObjects.SkillBookSkill_3,
        (int)GameObjects.SkillBookSkill_4, (int)GameObjects.SkillBookSkill_5,
        (int)GameObjects.SkillBookSkill_6,
    };
    static readonly int[] SLOT_BTN =
    {
        (int)Buttons.Btn_SkillBookSkill_0_07, (int)Buttons.Btn_SkillBookSkill_1_02,
        (int)Buttons.Btn_SkillBookSkill_2_09, (int)Buttons.Btn_SkillBookSkill_3_10,
        (int)Buttons.Btn_SkillBookSkill_4_05, (int)Buttons.Btn_SkillBookSkill_5_01,
        (int)Buttons.Btn_SkillBookSkill_6_03,
    };

    // 슬42: 능력 종류 필터 (전체/기술/마법/수비/지원)
    enum Filter { All, Skill, Spell, Def, Util }
    Filter _filter = Filter.All;

    readonly List<int> _shown = new List<int>(); // 슬롯 i에 매핑된 abilityId

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.SkillBookPopup).GetComponent<RectTransform>();
        OpenAnimation();

        GetButton((int)Buttons.SkillPopupBackground).gameObject.BindEvent(OnClickBG);

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            int slot = i;
            GetButton(SLOT_BTN[i]).gameObject.BindEvent(() => OnClickSlot(slot));
        }

        // 슬42: 하단 5버튼 = 능력 종류 (좌→우: 전체/기술/마법/수비/지원). prefab 버튼 이름은 구버전이라 위치로 매핑.
        BindFilter(Buttons.SkillPopup_ShowAll_Btn,         Filter.All);
        BindFilter(Buttons.SkillPopup_ShowTalent_Btn,      Filter.Skill);
        BindFilter(Buttons.SkillPopup_ShowOnMemory_Btn,    Filter.Spell);
        BindFilter(Buttons.SkillPopup_ShowNotOnMemory_Btn, Filter.Def);
        BindFilter(Buttons.SkillPopup_ShowLocked_Btn,      Filter.Util);

        return true;
    }

    void BindFilter(Buttons btn, Filter f)
    {
        var b = GetButton((int)btn);
        if (b != null) b.gameObject.BindEvent(() => { _filter = f; Refresh(); });
    }

    public void SetInfo()
    {
        // 슬40: 제목 = AbilityCodex_TitleText("능력 목록"). prefab 영어("SKILL LIST") 하드코딩 교체.
        var title = GetText((int)Texts.title);
        if (title != null)
        {
            title.text = Util.Localize(Define.LocalizeKey.AbilityCodex_TitleText);
            ApplyFont(title, FontType.UI);
        }
        // 슬42: 하단 필터 버튼 라벨 한/영 (전체/기술/마법/수비/지원).
        foreach (Filter f in System.Enum.GetValues(typeof(Filter))) SetFilterLabel(f);

        // 슬42: 능력 목록 호출 시 항상 [전체] 선택
        _filter = Filter.All;
        Refresh();
    }

    // 슬42: 활성 필터에 대응하는 하단 버튼 enum (위치 매핑: 전체/기술/마법/수비/지원)
    int FilterButton(Filter f)
    {
        switch (f)
        {
            case Filter.Skill: return (int)Buttons.SkillPopup_ShowTalent_Btn;
            case Filter.Spell: return (int)Buttons.SkillPopup_ShowOnMemory_Btn;
            case Filter.Def:   return (int)Buttons.SkillPopup_ShowNotOnMemory_Btn;
            case Filter.Util:  return (int)Buttons.SkillPopup_ShowLocked_Btn;
            default:           return (int)Buttons.SkillPopup_ShowAll_Btn;
        }
    }

    // 슬42: 필터별 라벨 로컬라이즈 키
    string FilterLocKey(Filter f)
    {
        switch (f)
        {
            case Filter.Skill: return Define.LocalizeKey.AbilityCodex_BtmText_Skill;
            case Filter.Spell: return Define.LocalizeKey.AbilityCodex_BtmText_Spell;
            case Filter.Def:   return Define.LocalizeKey.AbilityCodex_BtmText_Def;
            case Filter.Util:  return Define.LocalizeKey.AbilityCodex_BtmText_Util;
            default:           return Define.LocalizeKey.AbilityCodex_BtmText_All;
        }
    }

    // 하단 필터 버튼의 라벨 텍스트를 로컬라이즈 값으로 설정
    void SetFilterLabel(Filter f)
    {
        var btn = GetButton(FilterButton(f));
        var lbl = btn != null ? btn.GetComponentInChildren<TMPro.TMP_Text>(true) : null;
        if (lbl != null) { lbl.text = Util.Localize(FilterLocKey(f)); ApplyFont(lbl, FontType.UI); }
    }

    // 외부(능력 정보창 기억/망각)에서 필터 유지한 채 갱신
    public void RefreshUI() => Refresh();

    void Refresh()
    {
        RefreshMemoryGauge();

        // 슬40: 상단 TopText(SubtitleText) = 현재 눌린 하단 필터 버튼의 라벨과 동일하게 표기
        var sub = GetText((int)Texts.SubtitleText);
        if (sub != null)
        {
            var btn = GetButton(FilterButton(_filter));
            var lbl = btn != null ? btn.GetComponentInChildren<TMPro.TMP_Text>(true) : null;
            if (lbl != null) { sub.text = lbl.text; ApplyFont(sub, FontType.UI); }
        }

        // 필터에 맞는 능력 목록 수집 (DataId 정렬)
        _shown.Clear();
        var ids = new List<int>(Managers.Data.SkillDic.Keys);
        ids.Sort();
        foreach (int id in ids)
            if (PassFilter(id)) _shown.Add(id);

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            var root = GetObject(SLOT_ROOT[i]);
            if (root == null) continue;

            if (i >= _shown.Count) { root.SetActive(false); continue; }
            root.SetActive(true);
            RefreshSlot(root, _shown[i]);
        }
    }

    // 슬41: 잠김(미보유) 능력은 [능력 목록]에 없음(미니몬 도감에서만) → 보유 능력만.
    // 슬42: 필터 = 능력 종류(All/기술/마법/수비/지원).
    bool PassFilter(int id)
    {
        if (!Managers.Ability.IsLearned(id)) return false;
        if (_filter == Filter.All) return true;
        var role = Managers.Ability.GetRole(id);
        switch (_filter)
        {
            case Filter.Skill: return role == AbilityManager.Role.Skill;
            case Filter.Spell: return role == AbilityManager.Role.Spell;
            case Filter.Def:   return role == AbilityManager.Role.Def;
            case Filter.Util:  return role == AbilityManager.Role.Util;
            default:           return true;
        }
    }

    // 슬롯 내부 오버레이를 이름 부분일치로 찾아 상태 반영 (prefab 정적 슬롯이라 자식 탐색)
    void RefreshSlot(GameObject root, int abilityId)
    {
        bool learned  = Managers.Ability.IsLearned(abilityId);
        bool equipped = Managers.Ability.IsEquipped(abilityId);

        SetChild(root, "LockedIcon",    !learned);            // 미보유 = 잠금
        SetChild(root, "MemorizedIcon", equipped);            // 기억(장착) 중

        // 능력 단계(Lv) 표기 — 슬롯 내 LvlIcon 텍스트
        int lv = Managers.Ability.GetLevel(abilityId);
        foreach (var tmp in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
            if (tmp.name.Contains("LvlIcon")) { tmp.text = "Lv." + Mathf.Max(1, lv); ApplyFont(tmp, FontType.UI); }
        // 아이콘 sprite는 IconLabel 비어있어 prefab 기본 유지.
    }

    void SetChild(GameObject root, string nameContains, bool on)
    {
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
        {
            if (tr.name.Contains(nameContains)) tr.gameObject.SetActive(on);
        }
    }

    void RefreshMemoryGauge()
    {
        var txt = GetText((int)Texts.MemoryAmountTxt_1);
        if (txt == null) return;
        int used = Managers.Ability.UsedMemory;
        int max  = Managers.Ability.MaxMemory;
        txt.text = $"{used}/{max}";
        // 슬40: 현재 기억력 색 — 0=red / <max=yellow / =max=lime
        txt.color = used == 0      ? Util.HexToColor("FF0000")
                  : used >= max    ? Util.HexToColor("00FF00")
                  :                  Util.HexToColor("FFFF00");
        ApplyFont(txt, FontType.UI);
    }

    // 슬롯 클릭: 미보유=무시, 재능=상세만, 기억중=망각, 미기억=기억(용량 가능 시)
    // 슬41: 능력 ICON 클릭 → Punch/Scale Anim과 함께 [능력 정보창] 호출. (기억/망각은 정보창에서)
    void OnClickSlot(int slot)
    {
        if (slot >= _shown.Count) return;
        int id = _shown[slot];

        var root = GetObject(SLOT_ROOT[slot]);
        if (root != null)
        {
            root.transform.DOKill();
            root.transform.localScale = Vector3.one;
            // 슬41: Punch/Scale Dur 0.3 OutQuad TO(-0.25,0.15) Vibrato 10 Elasticity 1
            root.transform.DOPunchScale(new Vector3(-0.25f, 0.15f, 0f), 0.3f, 10, 1f)
                .SetEase(Ease.OutQuad).SetUpdate(true);
        }

        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(id);
    }
}
