using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [기술책] 팝업 (기획서 v3 슬40~41 + 능력 시스템 슬38~39).
// 보유 능력 목록 + 기억력(used/max) 표시. 슬롯 클릭으로 기억(장착)/망각 토글.
//  - 능력 데이터/회계는 AbilityManager. 슬롯은 prefab 정적(SkillBookSkill_0~6).
//  - 잠금(미보유) / 기억중(장착) / 미기억 오버레이를 슬롯별로 표시.
//  - 필터(전체/재능/기억중/미기억/잠금) 지원.
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

    enum Filter { All, Talent, OnMemory, NotOnMemory, Locked }
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

        BindFilter(Buttons.SkillPopup_ShowAll_Btn,         Filter.All);
        BindFilter(Buttons.SkillPopup_ShowTalent_Btn,      Filter.Talent);
        BindFilter(Buttons.SkillPopup_ShowOnMemory_Btn,    Filter.OnMemory);
        BindFilter(Buttons.SkillPopup_ShowNotOnMemory_Btn, Filter.NotOnMemory);
        BindFilter(Buttons.SkillPopup_ShowLocked_Btn,      Filter.Locked);

        return true;
    }

    void BindFilter(Buttons btn, Filter f)
    {
        var b = GetButton((int)btn);
        if (b != null) b.gameObject.BindEvent(() => { _filter = f; Refresh(); });
    }

    public void SetInfo()
    {
        var title = GetText((int)Texts.title);
        if (title != null) ApplyFont(title, FontType.UI);
        Refresh();
    }

    void Refresh()
    {
        RefreshMemoryGauge();

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

    bool PassFilter(int id)
    {
        bool learned = Managers.Ability.IsLearned(id);
        bool equipped = Managers.Ability.IsEquipped(id);
        bool talent = Managers.Ability.IsTalent(id);
        switch (_filter)
        {
            case Filter.Talent:      return talent;
            case Filter.OnMemory:    return equipped;
            case Filter.NotOnMemory: return learned && !equipped && !talent;
            case Filter.Locked:      return !learned;
            default:                 return true; // All
        }
    }

    // 슬롯 내부 오버레이를 이름 부분일치로 찾아 상태 반영 (prefab 정적 슬롯이라 자식 탐색)
    void RefreshSlot(GameObject root, int abilityId)
    {
        bool learned  = Managers.Ability.IsLearned(abilityId);
        bool equipped = Managers.Ability.IsEquipped(abilityId);

        SetChild(root, "LockedIcon",    !learned);            // 미보유 = 잠금
        SetChild(root, "MemorizedIcon", equipped);            // 기억(장착) 중
        // Lvl/New 등은 데이터(능력 레벨) 도입 후. 아이콘 sprite는 IconLabel 비어있어 prefab 기본 유지.
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
        txt.text = $"{Managers.Ability.UsedMemory}/{Managers.Ability.MaxMemory}";
        ApplyFont(txt, FontType.UI);
    }

    // 슬롯 클릭: 미보유=무시, 재능=상세만, 기억중=망각, 미기억=기억(용량 가능 시)
    void OnClickSlot(int slot)
    {
        if (slot >= _shown.Count) return;
        int id = _shown[slot];

        if (!Managers.Ability.IsLearned(id))
        {
            Debug.Log($"[SkillBook] 미보유 능력 {id} — 잠금");
            return;
        }
        if (Managers.Ability.IsTalent(id))
        {
            Debug.Log($"[SkillBook] 재능 {id} — 항시 적용(망각 불가)");
            return;
        }
        if (Managers.Ability.IsEquipped(id)) Managers.Ability.Forget(id);
        else if (!Managers.Ability.Memorize(id))
            Debug.Log($"[SkillBook] 기억력 부족 — {id} 기억 실패 (free={Managers.Ability.FreeMemory})");

        // 능력 변경 → 플레이어 스탯 즉시 재적용
        var player = FindObjectOfType<PlayerController>();
        if (player != null) player.RefreshStats();

        Refresh();
    }
}
