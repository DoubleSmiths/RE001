using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

/// <summary>
/// 슬라이드 36: 제작의뢰 팝업에서 해금된 기술 클릭 시 호출되는 상세정보 팝업.
/// 진입 애니메이션(From y=-130 OutBack 0.5s)과 닫기 동작만 최소 구현.
/// 슬라이드 37~40 상세 내용은 후속 작업.
/// </summary>
public class UI_SkillDetailPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        Img_LvEffectTxt_Size_01,
        Title,
        SubFrame,
        Img_SubFrame_01,
        EFrame,
        Img_NotChanging_01,
        WhereToFindTxtFrame,
        Img_ReqItemGroup_02,
        Img_ReqItemBox_10,
        Mask_ReqItemBox_06,
        Img_GlowMask_16,
        Img_ReqItemBox_03,
        Img_ReqItemBox_07,
        Img_ReqItemBox_1_16,
        Mask_ReqItemBox_1_07,
        Img_GlowMask_27,
        Img_ReqItemBox_1_06,
        Img_ReqItemBox_1_04,
        Img_ReqItemBox_2_15,
        Mask_ReqItemBox_2_05,
        Img_GlowMask_24,
        Img_ReqItemBox_2_12,
        Img_ReqItemBox_2_10,
        Img_ReqItemBox_3_17,
        Mask_ReqItemBox_3_08,
        Img_GlowMask_22,
        Img_ReqItemBox_3_02,
        Img_ReqItemBox_3_15,
        Img_ReqItemBox_4_11,
        Mask_ReqItemBox_4_09,
        Img_GlowMask_13,
        Img_ReqItemBox_4_07,
        Img_ReqItemBox_4_05,
        Img_ReqEquipGroup_06,
        Img_ReqItemBox1_1_09,
        Mask_ReqItemBox1_1_14,
        Img_GlowMask_21,
        Img_ReqItemBox1_1_04,
        Img_ReqItemBox1_1_08,
        Img_ReqItemBox1_2_13,
        Mask_ReqItemBox1_2_04,
        Img_GlowMask_15,
        Img_ReqItemBox1_2_08,
        Img_ReqItemBox1_2_11,
        Img_ReqItemBox1_3_06,
        Mask_ReqItemBox1_3_15,
        Img_GlowMask_14,
        Img_ReqItemBox1_3_01,
        Img_ReqItemBox1_3_16,
        Img_ReqItemBox1_4_05,
        Mask_ReqItemBox1_4_13,
        Img_GlowMask_20,
        Img_ReqItemBox1_4_07,
        Img_ReqItemBox1_4_02,
        Img_ReqItemBox1_5_18,
        Mask_ReqItemBox1_5_03,
        Img_GlowMask_17,
        Img_ReqItemBox1_5_11,
        Img_ReqItemBox1_5_09,
        Img_ReqMinionGroup_03,
        Img_ReqMinionBox_12,
        Mask_ReqMinionBox_10,
        Img_GlowMask_18,
        Img_GlowMask_30,
        Img_ReqMinionBox_14,
        Img_ReqMinionBox_06,
        Img_ReqMinionBox_04,
        Img_ReqMinionBox_1_04,
        Mask_ReqMinionBox_1_12,
        Img_GlowMask_26,
        Img_GlowMask_11,
        Img_ReqMinionBox_1_13,
        Img_ReqMinionBox_1_01,
        Img_ReqMinionBox_1_02,
        Img_ReqMinionBox_2_07,
        Mask_ReqMinionBox_2_02,
        Img_GlowMask_23,
        Img_GlowMask_29,
        Img_ReqMinionBox_2_12,
        Img_ReqMinionBox_2_05,
        Img_ReqMinionBox_2_03,
        Img_ReqMinionBox_3_08,
        Mask_ReqMinionBox_3_11,
        Img_GlowMask_19,
        Img_GlowMask_28,
        Img_ReqMinionBox_3_03,
        Img_ReqMinionBox_3_02,
        Img_ReqMinionBox_3_05,
        Img_ReqMinionBox_4_14,
        Mask_ReqMinionBox_4_01,
        Img_GlowMask_25,
        Img_GlowMask_12,
        Img_ReqMinionBox_4_06,
        Img_ReqMinionBox_4_03,
        Img_ReqMinionBox_4_01,
        Img_UnlimitedPotenGroup_05,
        Img_ClassGroup_01,
        Img_NextLvlBtn_06,
        Mask_NextLvlBtn_06,
        Img_Item_04,
        Img_Item_11,
        Img_BackBtn_01,
        Mask_BackBtn_02,
        Img_Item_03,
        Img_Item_07,
        CantCraftBtn,
        Img_CantCraftBtn_02,
        Mask_CantCraftBtn_08,
        Img_Item_01,
        Img_Item_10,
        Img_CraftBtn_04,
        Mask_CraftBtn_10,
        Img_Item_02,
        Img_Item_08,
        NotInPaceCraftBtn,
        Img_NotInPaceCraftBtn_05,
        Mask_NotInPaceCraftBtn_09,
        Img_Item_05,
        yoyo,
        Img_Item_09,
        Img_MemorizeBtn_03,
        Mask_MemorizeBtn_04,
        Img_Item_06,
        Img_Item_12,
        Deactimg,
        Img_Deactimg_02,
        Img_Deactimg_04,
        LackMemoryImg,
        Img_LackMemoryImg_01,
        Img_LackMemoryImg_03,
        Img_MemoryIcon_05,
        Img_SkillDetailPopupTitle_03,
        Mask,
        Img_Mask_03,
        Glow,
        Img_SkillDetailPopupIcon_02,
        Img_SkillDetailPopupIcon_04,
        WIcon,
        Img_SkillDetailPopupIcon_01,
        Img_BlockImg_01,
        MasterIcon,
        Img_MasterIcon_02,
        MemorizedIcon,
        Img_MemorizedIcon_01,
        FX_SkillUpgrade,
        __2,
        __3,
        Img_MemoryIcon_04,
        Xbutton,
    }

    enum Texts
    {
        LvEffectTxt_Size,
        Txt_Title_03,
        WhereToFindTxt,
        Txt_ReqItemGroup_07,
        Txt_ReqItemBox_03,
        Txt_ReqItemBox_1_09,
        Txt_ReqItemBox_2_04,
        Txt_ReqItemBox_3_08,
        Txt_ReqItemBox_4_07,
        Txt_ReqEquipGroup_06,
        Txt_ReqEquipGroup_03,
        Txt_ReqItemBox1_1_06,
        Txt_ReqItemBox1_2_02,
        Txt_ReqItemBox1_3_05,
        Txt_ReqItemBox1_4_10,
        Txt_ReqItemBox1_5_01,
        Txt_ReqMinionGroup_02,
        Txt_ReqMinionGroup_04,
        Txt_BlockImg_05,
        Txt_NeedMinionTxtGroup_09,
        Txt_NeedMinionTxtGroup_01,
        Txt_ReqMinionBox_07,
        Txt_ReqMinionBox_05,
        Txt_BlockImg_02,
        Txt_NeedMinionTxtGroup_08,
        Txt_NeedMinionTxtGroup_05,
        Txt_ReqMinionBox_1_04,
        Txt_ReqMinionBox_1_02,
        Txt_BlockImg_04,
        Txt_NeedMinionTxtGroup_07,
        Txt_NeedMinionTxtGroup_04,
        Txt_ReqMinionBox_2_02,
        Txt_ReqMinionBox_2_01,
        Txt_BlockImg_01,
        Txt_NeedMinionTxtGroup_06,
        Txt_NeedMinionTxtGroup_03,
        Txt_ReqMinionBox_3_05,
        Txt_ReqMinionBox_3_03,
        Txt_BlockImg_03,
        Txt_NeedMinionTxtGroup_10,
        Txt_NeedMinionTxtGroup_02,
        Txt_ReqMinionBox_4_01,
        Txt_ReqMinionBox_4_04,
        Txt_UnlimitedPotenGroup_08,
        Txt_UnlimitedPotenGroup_03,
        Txt_UnlimitedPotenGroup_01,
        Txt_ClassGroup_05,
        Txt_ClassGroup_04,
        Txt_ClassGroup_02,
        Txt_ReqGroup_01,
        Txt_NextLvlBtn_07,
        Txt_BackBtn_12,
        Txt_CantCraftBtn_11,
        Txt_CraftBtn_05,
        Txt_NotInPaceCraftBtn_03,
        Txt_NotInPaceCraftBtn_04,
        Txt_MemorizeBtn_01,
        Txt_MemoryDisplayGroup_02,
        Txt_Image_02,
        ItemPopupItemName,
        Txt_MasterIcon_02,
        Txt_MemorizedIcon_01,
        SkillDescText,
        SkillLvTxt,
        LvEffectTxt_Real,
        Lv0Text,
        Txt_GotMEM_01,
        Txt_Image_01,
        NoMEM,
        Txt_NoMEM_04,
        Txt_TalentHelpPopup_09,
        GameObject_1,
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        SkillDetailPopupBackground,
        ReqItemBox,
        ReqItemBox_1,
        ReqItemBox_2,
        ReqItemBox_3,
        ReqItemBox_4,
        ReqItemBox1_1,
        ReqItemBox1_2,
        ReqItemBox1_3,
        ReqItemBox1_4,
        ReqItemBox1_5,
        ReqMinionBox,
        ReqMinionBox_1,
        ReqMinionBox_2,
        ReqMinionBox_3,
        ReqMinionBox_4,
        NextLvlBtn,
        BackBtn,
        CraftBtn,
        MemorizeBtn,
        Btn_MemoryAmountTxt_02,
        Btn_GotMEM_01,
        Btn_NoMEM_02,
        TalentHelpPopup,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        SkillDetailPopup,
        SkillDetailPopupMain,
        NotChanging,
        SkillDetailActiveEf,
        FX_splash_spark_floor2,
        floor,
        particle,
        dust_spread,
        center_up,
        ReqGroup,
        ReqItemGroup,
        Panel_ReqItemGroup_01,
        ReqEquipGroup,
        Panel_ReqEquipGroup_02,
        ReqMinionGroup,
        ReqMinionList,
        Panel_ReqMinionBox_02,
        Panel_ReqMinionBox_1_03,
        Panel_ReqMinionBox_2_04,
        Panel_ReqMinionBox_3_01,
        Panel_ReqMinionBox_4_05,
        UnlimitedPotenGroup,
        ClassGroup,
        MemoryDisplayGroup,
        SkillDetailPopupTitle,
        SkillDetailPopupIcon,
        MasterAndMEMGroup,
        Star_01_Start,
        GlowDot_01_Start,
        GlowDot_02_Start,
        GlowDot_01_End,
        GlowDot_02_End,
        GlowDot_02_End_Light,
        NeedMemoryGroup,
        GotMEM,
    }

    #endregion


    const float FRAME_FROM_Y = -130f;
    Vector2 _frameOrigin;

    public int SkillId { get; private set; } = -1;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.SkillDetailPopup).GetComponent<RectTransform>();

        GetButton((int)Buttons.SkillDetailPopupBackground).gameObject.BindEvent(OnClickBG);
        GetButton((int)Buttons.MemorizeBtn).gameObject.BindEvent(OnClickMemorize);

        return true;
    }

    // skillId = 능력(SkillData) DataId. 이름/설명/단계를 한/영으로 채운다.
    public void SetInfo(int skillId = -1)
    {
        SkillId = skillId;

        var bg = GetButton((int)Buttons.SkillDetailPopupBackground).GetComponent<Image>();
        if (bg != null)
        {
            bg.DOKill();
            Color c = bg.color; c.a = 0f; bg.color = c;
            bg.DOFade(220f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        if (skillId < 0 || Managers.Ability == null) return;

        // 이름 — 한/영 (NameEng 폴백). 스킬 이름 노드는 ItemPopupItemName.
        SetText(Texts.ItemPopupItemName, Managers.Ability.GetName(skillId));
        // 설명 — 한/영 (DescEng 폴백)
        SetText(Texts.SkillDescText, Managers.Ability.GetDesc(skillId));
        // 능력 단계 "【 LEVEL: lv/max 】"
        int lv = Mathf.Max(1, Managers.Ability.GetLevel(skillId));
        int maxLv = Managers.Data.SkillDic.TryGetValue(skillId, out var sd) ? Mathf.Max(1, sd.MaxLevel) : lv;
        SetText(Texts.SkillLvTxt, $"【 LEVEL: <color=yellow>{lv}</color>/{maxLv} 】");

        RefreshMemorizeState();
    }

    // 슬43: 기억/망각 상태 + 마나(기억력) 점유량 표시.
    void RefreshMemorizeState()
    {
        bool owned    = Managers.Ability.IsLearned(SkillId);
        bool talent   = Managers.Ability.IsTalent(SkillId);
        bool equipped = Managers.Ability.IsEquipped(SkillId);
        int cap = Managers.Data.SkillDic.TryGetValue(SkillId, out var d) ? d.MemoryCapacity : 0;

        // 기억력 점유량 (재능=0이면 "?")
        SetText(Texts.Txt_GotMEM_01, talent ? "?" : cap.ToString());

        // 기억력 게이지 "<color>used(-cost)</color>/max"
        int used = Managers.Ability.UsedMemory, max = Managers.Ability.MaxMemory;
        string col = used == 0 ? "red" : used >= max ? "lime" : "yellow";
        SetText(Texts.Txt_MemoryDisplayGroup_02, $"<color={col}>{used}(-{cap})</color>/{max}");

        // 기억 버튼 라벨 — 미보유: 습득 필요 / 재능: 항시 적용 / 기억중: 망각 / 미기억: 기억
        string btnLabel = !owned ? "습득 필요" : talent ? "항시 적용" : equipped ? "망각" : "기억";
        SetText(Texts.Txt_MemorizeBtn_01, btnLabel);
        var btn = GetButton((int)Buttons.MemorizeBtn);
        if (btn != null) btn.interactable = owned && !talent;
    }

    // 슬43/41: 기억/망각 토글 (정보창에서). 재능/미보유는 불가.
    void OnClickMemorize()
    {
        if (!Managers.Ability.IsLearned(SkillId) || Managers.Ability.IsTalent(SkillId)) return;

        if (Managers.Ability.IsEquipped(SkillId)) Managers.Ability.Forget(SkillId);
        else if (!Managers.Ability.Memorize(SkillId))
        {
            Debug.Log($"[SkillDetail] 기억력 부족 — {SkillId} 기억 실패");
            return;
        }

        // 스탯 재적용 + 열려있는 능력 목록/디테일 갱신
        var player = FindObjectOfType<PlayerController>();
        if (player != null) player.RefreshStats();
        FindObjectOfType<UI_SkillBookPopup>()?.RefreshUI();
        RefreshMemorizeState();
    }

    void SetText(Texts id, string value)
    {
        var t = GetText((int)id);
        if (t == null) return;
        t.text = value;
        ApplyFont(t, Define.FontType.UI);
    }

    void OnClickClose()
    {
        var frameRT = GetObject((int)GameObjects.SkillDetailPopupMain).GetComponent<RectTransform>();
        frameRT.DOKill();

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(frameRT.DOAnchorPos(_frameOrigin + new Vector2(0f, FRAME_FROM_Y), 0.3f).SetEase(Ease.InBack));
        seq.OnComplete(() => Managers.UI.ClosePopupUI());
    }
}
