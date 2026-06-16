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

    int _reqMinionId;   // #6: 현재 능력의 게이팅 미니몬 CreatureId (Source=Minion일 때)

    // [능력 정보창 #1] 능력 종류 아이콘 — prefab에 전용 노드가 없어 런타임 생성(능력 ICON 우측 가로 나열).
    Transform _roleIconRow;
    const float ROLE_ICON_SIZE = 11f;   // "능력 ICON과 동일" 톤의 소형 아이콘
    const float ROLE_ICON_GAP  = 13f;

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
        // 능력 정보창 #6: 미니몬 요구 박스 클릭 → 해당 미니몬 [미니몬 기록] 호출
        GetButton((int)Buttons.ReqMinionBox).gameObject.BindEvent(OnClickReqMinion);

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
        // 능력 단계 "【 LEVEL: lv/max 】" (미보유=0, 이야기/설명은 RefreshDescLayout)
        int lv = Managers.Ability.GetLevel(skillId);   // 미보유 시 0
        int maxLv = Managers.Data.SkillDic.TryGetValue(skillId, out var sd) ? Mathf.Max(1, sd.MaxLevel) : Mathf.Max(1, lv);
        SetText(Texts.SkillLvTxt, $"【 LEVEL: <color=yellow>{lv}</color>/{maxLv} 】");

        RefreshMemorizeState();
        RefreshRoleIcons();
        RefreshMidState();     // #3 중단 상태(최대등급/기억중/재능/습득필요)
        RefreshDescLayout();   // #4 이야기/등급/설명 레이아웃
        RefreshSourceUI();     // #5/#6 하단 출처별 UI
        HidePotionButtons();   // 능력 정보창엔 물약 제작/다음단계 버튼 불필요
    }

    // [능력 정보창 #1] 능력 종류에 따라 아이콘 표시. 기획서 매핑:
    //  기술=Sword01 / 마법=Staff01(미존재 → Hud_icon_magic 대체) / 수비=role_icon_gladiator / 지원=role_icon_darkmage.
    //  "2가지 종류 이상이면 여러 개" — 현재 데이터는 StatType 1개라 종류 1개. 데이터 확장 시 자동으로 가로 나열.
    static string RoleIconKey(AbilityManager.Role r)
    {
        switch (r)
        {
            case AbilityManager.Role.Skill: return "Sword01";
            case AbilityManager.Role.Spell: return "Hud_icon_magic";
            case AbilityManager.Role.Def:   return "role_icon_gladiator";
            default:                        return "role_icon_darkmage";   // Util(지원)
        }
    }

    // ── 슬39: 발동조건/효과 설명 ──
    string BuildTriggerDesc(int id)
    {
        if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) return "";
        if (d.TriggerType == Define.AbilityTrigger.None) return "";

        string eff = EffectDesc(d);
        if (d.TriggerType == Define.AbilityTrigger.Probability)
        {
            string conds = "";
            if (d.Conditions != null)
                for (int i = 0; i < d.Conditions.Count; i++)
                    conds += (i > 0 ? " / " : "") + CondKor(d.Conditions[i], d.ConditionValue);
            return $"<color=#FFD24A>[{conds}]</color> {d.ProcChance:0}% 확률로 {eff}";
        }
        // Time
        return $"<color=#FFD24A>[{ContextKor(d.ActiveContext)}]</color> {d.Cooldown:0.#}초마다 {eff}";
    }

    static string CondKor(Define.AbilityCondition c, float p)
    {
        switch (c)
        {
            case Define.AbilityCondition.BattleStart: return "전투 시작 시";
            case Define.AbilityCondition.OnAttack:    return "공격 시";
            case Define.AbilityCondition.OnHit:       return "타격 시";
            case Define.AbilityCondition.OnCrit:      return "치명타 시";
            case Define.AbilityCondition.OnMiss:      return "공격이 빗나갈 시";
            case Define.AbilityCondition.SelfEvade:   return "회피 시";
            case Define.AbilityCondition.OnDamaged:   return "피격 시";
            case Define.AbilityCondition.OnHeal:      return "체력 회복 시";
            case Define.AbilityCondition.LowHp:       return $"HP {p:0}% 이하 시";
            default:                                  return c.ToString();
        }
    }

    static string ContextKor(Define.AbilityContext c)
    {
        switch (c)
        {
            case Define.AbilityContext.InBattle:    return "전투 중";
            case Define.AbilityContext.OutOfBattle: return "비전투 중";
            default:                                return "항시";
        }
    }

    static string EffectDesc(Data.SkillData d)
    {
        switch (d.Effect)
        {
            case Define.AbilityEffect.Heal:        return $"체력 {d.EffectValue:0} 회복";
            case Define.AbilityEffect.BonusDamage: return $"{d.EffectValue:0} 추가 피해";
            case Define.AbilityEffect.Buff:
                string dur = d.EffectDuration > 0 ? $" ({d.EffectDuration:0.#}초)" : "";
                return $"{StatKor(d.EffectStat)} +{d.EffectValue:0}{dur}";
            default:                               return "";
        }
    }

    static string StatKor(string s)
    {
        switch (s)
        {
            case "PhysicalAtk": return "물리공격력";
            case "MagicAtk":    return "마법공격력";
            case "CritRate":    return "치명타 확률";
            case "CritDamage":  return "치명타 피해";
            case "AtkSpeed":    return "공격속도";
            case "Accuracy":    return "명중률";
            case "Def":         return "방어력";
            case "MaxHp":       return "최대 체력";
            case "Evasion":     return "회피율";
            default:            return s;
        }
    }

    void RefreshRoleIcons()
    {
        // 부모: 능력 ICON 슬롯(SkillDetailPopupIcon) — front 레이어라 가려지지 않음.
        //  (상단 헤더에 붙이면 내부 패널 뒤로 들어가 가려짐)
        var parent = GetObject((int)GameObjects.SkillDetailPopupIcon)?.transform;
        if (parent == null) return;

        if (_roleIconRow == null)
        {
            var rowGo = new GameObject("RoleIconRow", typeof(RectTransform));
            _roleIconRow = rowGo.transform;
            var rt = (RectTransform)_roleIconRow;
            rt.SetParent(parent, false);
            rt.SetAsLastSibling();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(20f, -4f);   // 능력 아이콘 우측
        }

        for (int i = _roleIconRow.childCount - 1; i >= 0; i--)
            Destroy(_roleIconRow.GetChild(i).gameObject);

        if (SkillId < 0 || Managers.Ability == null) return;

        AbilityManager.Role[] roles = { Managers.Ability.GetRole(SkillId) };
        float x = 0f;
        foreach (var role in roles)
        {
            var sp = Managers.Resource.Load<Sprite>(RoleIconKey(role));
            if (sp == null) continue;

            var go = new GameObject("RoleIcon_" + role, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_roleIconRow, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(ROLE_ICON_SIZE, ROLE_ICON_SIZE);
            rt.anchoredPosition = new Vector2(x, 0f);

            var img = go.GetComponent<Image>();
            img.sprite = sp;
            img.preserveAspect = true;
            img.raycastTarget = false;
            x += ROLE_ICON_GAP;
        }
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

    // ── 능력 정보창 #3: 중단 상태 (최대등급 / 기억중 / 재능 / 습득필요) ──
    void RefreshMidState()
    {
        bool owned    = Managers.Ability.IsLearned(SkillId);
        bool talent   = Managers.Ability.IsTalent(SkillId);
        bool equipped = Managers.Ability.IsEquipped(SkillId);
        int  lv       = Managers.Ability.GetLevel(SkillId);
        int  maxLv    = Managers.Data.SkillDic.TryGetValue(SkillId, out var sd) ? Mathf.Max(1, sd.MaxLevel) : lv;
        bool atMax    = owned && lv >= maxLv;

        // 최대등급(MasterIcon) / 기억중(MemorizedIcon) — 능력 ICON 옆 뱃지
        SetActive(GetImageGo((int)Images.MasterIcon),    atMax);
        SetActive(GetImageGo((int)Images.MemorizedIcon), owned && equipped && !talent);

        // 점유량(GotMEM) / 재능(NoMEM) — 점유량 0이면 재능 표기
        SetActive(GetObject((int)GameObjects.GotMEM), owned && !talent);
        SetActive(GetTextGo((int)Texts.NoMEM),        talent);

        // 습득 필요(Lv0Text) — 미보유 시
        SetActive(GetTextGo((int)Texts.Lv0Text), !owned);
    }

    // ── 능력 정보창 #4: 능력 이야기 / 등급 / 설명 ──
    void RefreshDescLayout()
    {
        // 능력 이야기(flavor) — 데이터에 있으면 표시, 없으면 숨김
        string story = Managers.Ability.GetStory(SkillId);
        var storyT = GetText((int)Texts.SkillDescText);
        if (storyT != null)
        {
            bool has = !string.IsNullOrEmpty(story);
            storyT.gameObject.SetActive(has);
            if (has) { storyT.text = story; storyT.fontStyle = TMPro.FontStyles.Italic; ApplyFont(storyT, Define.FontType.UI); }
        }

        // 능력 설명(mechanics) — GetDesc + 발동조건 한 줄. 미보유 시 "- 습득 필요 -"(Lv0Text)가 대체하므로 숨김.
        bool owned = Managers.Ability.IsLearned(SkillId);
        var realT = GetText((int)Texts.LvEffectTxt_Real);
        if (realT != null)
        {
            realT.gameObject.SetActive(owned);
            if (owned)
            {
                string desc = Managers.Ability.GetDesc(SkillId);
                string trig = BuildTriggerDesc(SkillId);
                if (!string.IsNullOrEmpty(trig)) desc = string.IsNullOrEmpty(desc) ? trig : desc + "\n\n" + trig;
                realT.text = desc;
                ApplyFont(realT, Define.FontType.UI);
            }
        }
    }

    // ── 능력 정보창 #5/#6: 하단 출처별 UI ──
    void RefreshSourceUI()
    {
        var src = Managers.Ability.GetSource(SkillId);

        SetActive(GetObject((int)GameObjects.ClassGroup),          src == Define.AbilitySource.Class);
        SetActive(GetObject((int)GameObjects.ReqMinionGroup),      src == Define.AbilitySource.Minion);
        SetActive(GetObject((int)GameObjects.ReqEquipGroup),       src == Define.AbilitySource.Equip);
        SetActive(GetObject((int)GameObjects.ReqItemGroup),        src == Define.AbilitySource.Npc);
        SetActive(GetObject((int)GameObjects.UnlimitedPotenGroup), src == Define.AbilitySource.Unlimited);

        _reqMinionId = 0;
        if (src == Define.AbilitySource.Minion)
            RefreshMinionReq(Managers.Ability.GetRequiredMinionId(SkillId));
    }

    // #6: 미니몬 요구 박스 게이팅 (기록도/색/체크·잠금). 첫 박스만 사용, 나머지는 숨김.
    void RefreshMinionReq(int minionId)
    {
        _reqMinionId = minionId;

        // 박스 1~4 숨김 (요구 미니몬 1마리)
        SetActive(GetButton((int)Buttons.ReqMinionBox_1).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqMinionBox_2).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqMinionBox_3).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqMinionBox_4).gameObject, false);

        var box0 = GetButton((int)Buttons.ReqMinionBox);
        SetActive(box0.gameObject, true);

        if (minionId <= 0 || Managers.MinMon == null) return;

        int  rec     = Managers.MinMon.GetRecordRate(minionId);   // 0..100
        bool owned   = Managers.MinMon.IsOwned(minionId);
        bool allSS   = Managers.MinMon.IsAllSS(minionId);
        int  star    = Managers.MinMon.GetStarGrade(minionId);
        bool unlocked = rec >= 10 || owned;                       // 기록도 10%↑ 또는 보유 시 해금(상호작용)

        // 기록도 텍스트 + 색 (0=red / 0<x<100=orange / 100=lime)
        string rc = rec <= 0 ? "red" : rec >= 100 ? "lime" : "orange";
        var recT = GetText((int)Texts.Txt_ReqMinionBox_07);
        if (recT != null) { recT.text = $"<color={rc}>{rec}</color>/100"; SetActive(recT.gameObject, unlocked); }

        // 최상등급 표기 (필요 미니몬 별 2단계↑) — AllSS면 lime, 아니면 909090
        var maxRankT = GetText((int)Texts.Txt_ReqMinionBox_05);
        if (maxRankT != null)
        {
            SetActive(maxRankT.gameObject, unlocked && star >= 2);
            SetTextColor(maxRankT, allSS ? new Color32(0x0A, 0xFF, 0x00, 0xFF) : new Color32(0x90, 0x90, 0x90, 0xFF));
        }

        // 잠금 오버레이("?" + "하수인 또는 분석 필요") — 미해금 시 표시
        SetActive(GetImageGo((int)Images.Img_ReqMinionBox_14), !unlocked);     // "?" 프레임
        SetActive(GetObject((int)GameObjects.Panel_ReqMinionBox_02), !unlocked); // NeedMinion 텍스트

        box0.interactable = unlocked;
    }

    void OnClickReqMinion()
    {
        if (_reqMinionId <= 0) return;
        Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(_reqMinionId);
    }

    // 능력 정보창에선 물약 제작/다음단계 버튼군 불필요 — 숨김(기억 버튼만 유지).
    void HidePotionButtons()
    {
        SetActive(GetButton((int)Buttons.NextLvlBtn).gameObject, false);
        SetActive(GetButton((int)Buttons.BackBtn).gameObject,    false);
        SetActive(GetButton((int)Buttons.CraftBtn).gameObject,   false);
        SetActive(GetImageGo((int)Images.CantCraftBtn),          false);
        SetActive(GetImageGo((int)Images.NotInPaceCraftBtn),     false);
    }

    // ── helpers ──
    static void SetActive(GameObject go, bool on) { if (go != null && go.activeSelf != on) go.SetActive(on); }
    GameObject GetImageGo(int id) { var c = GetImage(id); return c != null ? c.gameObject : null; }
    GameObject GetTextGo(int id)  { var c = GetText(id);  return c != null ? c.gameObject : null; }
    static void SetTextColor(TMPro.TMP_Text t, Color c) { if (t != null) t.color = c; }

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
