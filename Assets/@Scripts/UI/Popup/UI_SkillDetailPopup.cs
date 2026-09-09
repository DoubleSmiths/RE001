using System;
using System.Collections;
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


    const float FRAME_FROM_Y = -120f;   // 슬43: From y -120 (스펙)
    Vector2 _frameOrigin;
    Vector2 _baseOrigin;   // prefab 저작 위치(클램프 누적 방지용 기준)

    public int SkillId { get; private set; } = -1;

    int _reqMinionId;   // #6: 현재 능력의 게이팅 미니몬 CreatureId (Source=Minion일 때)
    int _reqItemId;     // 슬50: 현재 능력의 요구 재료 ItemData DataId (Source=Npc일 때)
    int _reqEquipId;    // 슬49: 현재 능력의 요구 장비 DataId (Source=Equip일 때)
    bool _badgeBaseCached; float _badgeBaseX;   // 슬45: 상태 배지(완벽숙달/기억중) 기본 x (둘 다일 때 좌우 분리용)
    bool _previewNext;  // 슬51 버튼2: 다음 등급 미리보기 토글 상태

    // 슬46: 설명 텍스트 양에 따른 정보창 세로 자동 사이즈 — 프레임/설명 캐시 + base 값(누적 방지).
    RectTransform _autoFrame;       // Img_LvEffectTxt_Size_01 (설명에 stretch로 묶인 프레임)
    TMPro.TMP_Text _autoDescTmp;    // LvEffectTxt_Real (설명 본문)
    bool  _autoSizeCached;
    float _autoFrameBaseSize, _autoFrameBasePos;

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

        // 슬43: 진입 Move 애니메이션 기준점(_frameOrigin) 캡처 — 종료 역재생 위치 계산용.
        //  (이전엔 미설정으로 (0,0) 고정 버그 → OnClickClose 종료 위치 오류)
        var mainGo = GetObject((int)GameObjects.SkillDetailPopupMain);
        if (mainGo != null)
        {
            var mainRT = mainGo.GetComponent<RectTransform>();
            if (mainRT != null) { _frameOrigin = mainRT.anchoredPosition; _baseOrigin = _frameOrigin; }
            // [DOTweenAnimation 규약 예외 — 의도적 코드 대체] 진입 점프 방지.
            //  프리팹 진입 DOTweenAnimation(Main/부모)의 isFrom이 Start에서 _baseOrigin으로 슬라이드인 →
            //  0.55s 후 ClampPopupToCenter가 위로 올려 '훅 점프'가 생겼다. → 컴포넌트를 끄고 코드가 처음부터
            //  중앙 위치로 슬라이드인. (컴포넌트화하려면 prefab anim의 isFrom 스냅/클램프 충돌부터 해결 필요.)
            foreach (var anim in mainGo.GetComponentsInParent<DG.Tweening.DOTweenAnimation>(true))
                anim.enabled = false;
        }

        GetButton((int)Buttons.SkillDetailPopupBackground).gameObject.BindEvent(OnClickBG);
        GetButton((int)Buttons.MemorizeBtn).gameObject.BindEvent(OnClickMemorize);
        // 능력 정보창 #6: 미니몬 요구 박스 클릭 → 해당 미니몬 [미니몬 기록] 호출
        GetButton((int)Buttons.ReqMinionBox).gameObject.BindEvent(OnClickReqMinion);
        // 슬51 버튼2: 다음 등급 정보 미리보기 토글
        GetButton((int)Buttons.NextLvlBtn).gameObject.BindEvent(OnClickNextLvl);
        // 슬50: 재료 박스 클릭 → 해당 아이템 상세 (정보 팝업 있으면)
        GetButton((int)Buttons.ReqItemBox).gameObject.BindEvent(OnClickReqItem);
        // 슬49: 장비 박스 클릭 → 해당 장비 상세
        GetButton((int)Buttons.ReqItemBox1_1).gameObject.BindEvent(OnClickReqEquip);

        // 슬43: 우상단 X 버튼으로 닫기 (Images.Xbutton 노드 — Buttons에 없어 Image 게임오브젝트에 직접 바인드)
        var xGo = GetImageGo((int)Images.Xbutton);
        if (xGo != null) xGo.BindEvent(OnClickXbutton);

        return true;
    }

    // 슬43: X버튼/닫기 — 종료 애니(진입 Move 역재생) 후 팝업 닫기.
    void OnClickXbutton() => OnClickClose();

    // skillId = 능력(SkillData) DataId. 이름/설명/단계를 한/영으로 채운다.
    public void SetInfo(int skillId = -1)
    {
        SkillId = skillId;

        // 슬43: BG 딤 — #000000 alpha 130 (이전 220 → 스펙 130/255)
        var bg = GetButton((int)Buttons.SkillDetailPopupBackground).GetComponent<Image>();
        if (bg != null)
        {
            bg.DOKill();
            Color c = bg.color; c.r = c.g = c.b = 0f; c.a = 0f; bg.color = c;
            bg.DOFade(130f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        // 슬43: 진입 Move 애니메이션 (From y-120 OutBack 0.5).
        //  유효 능력이면 사이즈/중앙정렬 확정 후 ClampPopupToCenter에서 진입 애니를 재생(위치 보정 반영). 빈 팝업(skillId<0)만 여기서.
        if (skillId < 0 || Managers.Ability == null) { PlayOpenAnim(); return; }

        // 슬43: 타이틀 "능력 정보" (prefab Localize 미연결 대비 코드에서도 세팅)
        SetText(Texts.Txt_Title_03, Util.Localize(Define.LocalizeKey.AbilityDetails_TitleText));

        // 이름 — 한/영 (NameEng 폴백). 스킬 이름 노드는 ItemPopupItemName.
        SetText(Texts.ItemPopupItemName, Managers.Ability.GetName(skillId));
        // 능력 단계 "【 LEVEL: lv/max 】" — RefreshDescLayout이 미리보기(_previewNext) 인지하여 생성(슬46/51).
        _previewNext = false;  // 슬51: 능력 바뀌면 미리보기 해제

        RefreshMemorizeState();
        RefreshRoleIcons();
        RefreshMidState();     // #3 중단 상태(최대등급/기억중/재능/습득필요)
        RefreshDescLayout();   // #4 이야기/등급/설명 레이아웃
        RefreshSourceUI();     // #5/#6 하단 출처별 UI (#7 장비/#8 재료 포함)
        RefreshBottomButtons();// 슬51 최하단 3버튼(버튼1 NPC장소 / 버튼2 다음등급 / 버튼3 기억)
        ApplySkillIcon();      // 능력별 아이콘 반영(없으면 프리팹 기본 유지)
        ApplyEquipMaterial();  // 슬52: 기억중/재능이면 이름 Frame Sprite에 Material_Equip
        RefreshAutoSize();     // 슬46: 설명 길이에 따라 정보창 세로 사이즈 자동 변화
        ApplyBottomMargin();   // 슬51 피드백: 하단 UI를 SubFrame 밑으로 정렬
        ClampPopupToCenter();  // 피드백: 콘텐츠(타이틀~게이지)를 화면 사용영역 중심에 맞춰 위치 → 하단 버튼이 화면 밖/HUD와 겹치지 않게
    }

    // 피드백(버튼 화면밖 / 진입 점프): 사이즈 확정 후, 콘텐츠 상단(타이틀)~하단(게이지) 중심을 화면 사용영역 중심에 맞춰
    //  '중앙 위치'를 계산하고, 그 위치로 직접 진입 슬라이드인(From y-120). 프리팹 진입 애니는 Init에서 꺼서 점프 없음.
    //  팝업 길이가 능력마다 달라도 항상 가운데. (_baseOrigin 기준 재계산이라 누적 없음)
    const float SAFE_CENTER_Y = -0.4f;   // 화면 사용영역 중심(상/하 HUD 고려, world). 약간 아래로(피드백)
    void ClampPopupToCenter()
    {
        var main = System.Array.Find(GetComponentsInChildren<Transform>(true), t => t.name == "SkillDetailPopupMain") as RectTransform;
        if (main == null) return;
        var titleRT = System.Array.Find(GetComponentsInChildren<Transform>(true), t => t.name == "Title") as RectTransform;
        var gaugeRT = System.Array.Find(GetComponentsInChildren<Transform>(true), t => t.name == "MemoryDisplayGroup") as RectTransform
                   ?? GetButton((int)Buttons.MemorizeBtn)?.GetComponent<RectTransform>();
        if (titleRT == null || gaugeRT == null) return;

        // 중앙 위치 계산을 위해 _baseOrigin에 두고 레이아웃 확정 후 측정 (진입 애니 꺼서 측정 신뢰 가능)
        DG.Tweening.DOTween.Kill(main, false);
        main.anchoredPosition = _baseOrigin;
        Canvas.ForceUpdateCanvases();   // 처음 열 때 첫 프레임 world 좌표 확정 (RefreshAutoSize와 동일 이유)
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(main);
        var tc = new Vector3[4]; titleRT.GetWorldCorners(tc); float topY = tc[2].y;
        var gc = new Vector3[4]; gaugeRT.GetWorldCorners(gc); float botY = gc[0].y;
        float center = (topY + botY) * 0.5f;
        float scale = main.lossyScale.y;
        if (scale <= 0.0001f) { _frameOrigin = _baseOrigin; main.anchoredPosition = _frameOrigin; return; }

        _frameOrigin = _baseOrigin + new Vector2(0f, (SAFE_CENTER_Y - center) / scale);
        // 중앙 위치로 직접 진입 슬라이드인 (점프 없음)
        main.anchoredPosition = _frameOrigin + new Vector2(0f, FRAME_FROM_Y);
        main.DOAnchorPos(_frameOrigin, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // 슬51 피드백: 하단 UI(출처+버튼=ReqGroup)는 SubFrame(장식 패널) '밑'에 정렬.
    //  RefreshAutoSize가 설명량에 맞춰 프레임(→SubFrame)을 줄이/늘리므로, 여기선 ReqGroup 상단을 SubFrame 바닥 바로 아래로만 정렬.
    //  (이전엔 프레임을 PAD만큼 연장했으나 설명 rect가 그만큼 빈 공간으로 늘어나 팝업이 길어져 제거 — 길이 조절 피드백.)
    //  RefreshAutoSize 이후 호출. SubFrame/ReqGroup 모두 팝업 루트 하위라 진입 애니 중에도 상대 Δ 일관.
    const float SUBFRAME_CLEAR = 0.06f; // SubFrame 바닥 밑 간격(world)
    RectTransform _subFrameRT;
    void ApplyBottomMargin()
    {
        var rg = GetObject((int)GameObjects.ReqGroup)?.GetComponent<RectTransform>();
        if (rg == null) return;

        if (_subFrameRT == null)
            _subFrameRT = System.Array.Find(GetComponentsInChildren<Transform>(true), t => t.name == "SubFrame") as RectTransform;
        if (_subFrameRT == null) return;
        Canvas.ForceUpdateCanvases();   // 처음 열 때 첫 프레임 world 좌표 확정 (RefreshAutoSize와 동일 이유)
        var sc = new Vector3[4]; _subFrameRT.GetWorldCorners(sc); float subBottom = sc[0].y;
        var rc = new Vector3[4]; rg.GetWorldCorners(rc);          float rgTop     = rc[2].y;
        float scaleY = rg.lossyScale.y;
        if (scaleY <= 0.0001f) return;
        float deltaLocal = ((subBottom - SUBFRAME_CLEAR) - rgTop) / scaleY;
        rg.anchoredPosition = new Vector2(rg.anchoredPosition.x, rg.anchoredPosition.y + deltaLocal);
    }

    // [능력 정보창 #1] 능력 종류에 따라 아이콘 표시. 기획서 매핑:
    //  기술=Sword01 / 마법=Staff01(미존재 → Hud_icon_magic 대체) / 수비=role_icon_gladiator / 지원=role_icon_darkmage.
    //  "2가지 종류 이상이면 여러 개" — 현재 데이터는 StatType 1개라 종류 1개. 데이터 확장 시 자동으로 가로 나열.
    static string RoleIconKey(AbilityManager.Role r)
    {
        switch (r)
        {
            case AbilityManager.Role.Skill: return "Sword01";
            case AbilityManager.Role.Spell: return "Staff01";                // 슬44: 마법=Staff01 (에셋 존재)
            case AbilityManager.Role.Def:   return "role_icon_gladiator";
            default:                        return "role_icon_darkmage";   // Util(지원)
        }
    }

    // ── 슬39: 발동조건/효과 설명 ──
    string BuildTriggerDesc(int id)
    {
        if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) return "";
        if (d.TriggerType == Define.AbilityTrigger.None) return "";
        // 미니몬 액티브 설명은 효과 비율·공유 쿨타임까지 데이터에 표시한다.
        if (d.TriggerType == Define.AbilityTrigger.Active && d.Source == Define.AbilitySource.Minion) return "";

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
            case Define.AbilityCondition.AfterAttack: return "공격 후";
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

    // 슬44: 능력 종류(기술/마법/방어/지원)를 능력 아이콘 '왼쪽'에 [역할 아이콘(뒤) + BtmText 라벨(앞)] 겹쳐서 표시.
    //  2가지 종류 이상이면 흐름 배치(2개씩, 다음 줄). 단일 종류면 1개.
    void RefreshRoleIcons()
    {
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
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-33.5f, 0f);   // 능력 아이콘 왼쪽 영역 중앙 (IconSlot 로컬, scale 0.05). -30이면 우측 쏠려 아이콘과 겹침 → -33.5로 양쪽 여백 균형
        }

        for (int i = _roleIconRow.childCount - 1; i >= 0; i--)
            Destroy(_roleIconRow.GetChild(i).gameObject);

        if (SkillId < 0 || Managers.Ability == null) return;

        var roles = Managers.Ability.GetRoles(SkillId);
        int n = roles.Count;
        const int perRow = 2;
        int rows = (n + perRow - 1) / perRow;
        const float gapX = 16f, gapY = 14f, iconSize = 16f;

        for (int i = 0; i < n; i++)
        {
            int row = i / perRow;
            int itemsInRow = Mathf.Min(perRow, n - row * perRow);   // 마지막 행은 적을 수 있음
            int colInRow = i % perRow;
            // 각 행을 가로 중앙 정렬 + 전체를 세로 중앙 정렬 (2가지=가로, 3가지=2+1중앙, 4가지=2x2)
            float cx = (colInRow - (itemsInRow - 1) * 0.5f) * gapX;
            float cy = ((rows - 1) * 0.5f - row) * gapY;

            var cell = new GameObject("Role_" + roles[i], typeof(RectTransform));
            var crt = (RectTransform)cell.transform;
            crt.SetParent(_roleIconRow, false);
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(iconSize, iconSize);
            crt.anchoredPosition = new Vector2(cx, cy);

            // 역할 아이콘(뒤). role_icon_gladiator/darkmage는 꽉 찬 스프라이트(74/64px)라 sword01/staff01(32px, 여백 큼)보다
            //  같은 셀에서 더 커 보임 → Def/Util은 0.72배로 축소해 시각 크기를 sword/staff에 맞춤.
            var sp = Managers.Resource.Load<Sprite>(RoleIconKey(roles[i]));
            if (sp != null)
            {
                var igo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var irt = (RectTransform)igo.transform;
                irt.SetParent(crt, false);
                irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                float disp = (roles[i] == AbilityManager.Role.Def || roles[i] == AbilityManager.Role.Util) ? iconSize * 0.72f : iconSize;
                irt.sizeDelta = new Vector2(disp, disp);
                irt.anchoredPosition = Vector2.zero;
                var im = igo.GetComponent<Image>();
                im.sprite = sp; im.preserveAspect = true; im.raycastTarget = false;
            }
            // BtmText 라벨(앞, 겹침)
            var tgo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            var trt = (RectTransform)tgo.transform;
            trt.SetParent(crt, false);
            trt.anchorMin = new Vector2(0.5f, 0.5f); trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(gapX + 8f, gapY);
            trt.anchoredPosition = Vector2.zero;
            var tmp = tgo.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = Util.Localize(RoleLabelKey(roles[i]));
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 8f;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;
            ApplyFont(tmp, Define.FontType.UI);
        }
    }

    // 슬44: 종류별 BtmText 라벨 로컬키 (기술/마법/방어/지원)
    static string RoleLabelKey(AbilityManager.Role r)
    {
        switch (r)
        {
            case AbilityManager.Role.Skill: return Define.LocalizeKey.AbilityCodex_BtmText_Skill;
            case AbilityManager.Role.Spell: return Define.LocalizeKey.AbilityCodex_BtmText_Spell;
            case AbilityManager.Role.Def:   return Define.LocalizeKey.AbilityCodex_BtmText_Def;
            default:                        return Define.LocalizeKey.AbilityCodex_BtmText_Util;
        }
    }

    // 슬43: 기억/망각 상태 + 마나(기억력) 점유량 표시.
    void RefreshMemorizeState()
    {
        bool owned    = Managers.Ability.IsLearned(SkillId);
        bool talent   = Managers.Ability.IsTalent(SkillId);
        bool equipped = Managers.Ability.IsEquipped(SkillId);
        int cap = Managers.Data.SkillDic.TryGetValue(SkillId, out var d) ? d.MemoryCapacity : 0;
        if (Managers.Ability.IsCompanionGranted(SkillId) && !Managers.Game.EquippedAbilities.Contains(SkillId))
            cap = 0;

        // 슬44: 기억력 점유량 (재능=0이면 "?" 색 #FDF6A6). 외곽선(C0AA70)은 prefab 머티리얼 프리셋 소관.
        SetText(Texts.Txt_GotMEM_01, talent ? "<color=#FDF6A6>?</color>" : cap.ToString());

        // 슬44: 재능 텍스트(TalentText) — 외곽선 7073C0은 prefab 머티리얼 프리셋 소관.
        if (talent) SetText(Texts.NoMEM, Util.Localize(Define.LocalizeKey.TalentText));

        // 슬52: 기억력 게이지 — 색은 기억 가능 여부(가능/기억중=#00FF00, 부족=red).
        //  기억중(equipped)이면 점유가 이미 used에 포함 → "used/max"(mockup image152). 미기억이면 "used(-cap)/max"(image146/150).
        int used = Managers.Ability.UsedMemory, max = Managers.Ability.MaxMemory;
        bool canAfford = equipped || Managers.Ability.FreeMemory >= cap;
        string col = (owned && !talent && !canAfford) ? "red" : "#00FF00";   // 'lime' TMP 미지원 → #00FF00
        string gauge = equipped ? $"<color={col}>{used}</color>/{max}" : $"<color={col}>{used}(-{cap})</color>/{max}";
        SetText(Texts.Txt_MemoryDisplayGroup_02, gauge);

        // 출처별 버튼3 분기 — 장비부여(무점유) / NPC 습득 / 축성(전직)
        var src = Managers.Ability.GetSource(SkillId);
        bool equipGranted = Managers.Ability.IsEquipGranted(SkillId);

        string btnLabel;
        bool interactable;
        if (Managers.Ability.IsCompanionGranted(SkillId))
        {
            btnLabel = Util.Localize(Define.COMPANION_ABILITY_BORROWED_TEXT);
            interactable = false;
        }
        else if (equipGranted)
        {
            // 장비 착용 부여(무점유) — 망각 불가 → "기억중" 표기 + 비활성.
            //  (게이지 점유 표기는 UsedMemory 기반이라 무점유가 자동 반영됨)
            btnLabel = Util.Localize(Define.LocalizeKey.Memorized_Text_Full);
            interactable = false;
        }
        else if (!owned && src == Define.AbilitySource.Npc)
        {
            // NPC 출처 미보유 → "습득"(재료 소모). 재료 부족이면 비활성 — 재료색은 RefreshItemReq가 담당.
            btnLabel = Util.Localize("Ability_Learn_Text");
            interactable = Managers.Ability.CanLearnFromNpc(SkillId);
        }
        else if (!owned && src == Define.AbilitySource.Class)
        {
            // Class 출처 미보유 → "축성"(숙련도 100 소진 습득). 숙련도 부족이면 비활성.
            btnLabel = Util.Localize(Define.LocalizeKey.AnointmentText);
            interactable = Managers.Ability.CanAnoint(SkillId);
        }
        else
        {
            // 슬51/52: 기억 버튼 라벨 — 미보유:습득 필요 / 재능:항시 적용 / 기억중:- 기억중 - / 미기억&가능:기억하기 / 미기억&부족:기억력 부족
            btnLabel = !owned ? "습득 필요"
                     : talent ? "항시 적용"
                     : equipped ? $"- {Util.Localize(Define.LocalizeKey.Memorized_Text_Full)} -"   // 기억중 → "- 기억중 -" (mockup image152)
                     : canAfford ? Util.Localize(Define.LocalizeKey.MemorizeBtnText)
                     : Util.Localize(Define.LocalizeKey.Lack_Memory_Text);
            // 슬52: Interactive — 보유&비재능 & (기억중 or 여유>=점유). 부족 시 X.
            interactable = owned && !talent && (equipped || canAfford);
        }
        SetText(Texts.Txt_MemorizeBtn_01, btnLabel);
        var btn = GetButton((int)Buttons.MemorizeBtn);
        if (btn != null) btn.interactable = interactable;

        // 슬52: 부족 표시(LackMemoryImg) — 미기억 & 기억력 부족 시
        SetActive(GetImageGo((int)Images.LackMemoryImg), owned && !talent && !equipped && !canAfford);

        // 슬52: 버튼3 글로우색 — 상호작용 가능(기억/잊기/습득/축성)=0036A6(파랑), 그 외(부족/장비부여/재능)=000000.
        SetButtonGlow(Images.Img_Item_12, interactable ? "0036A6" : "000000");
    }

    // 슬43/41/52: 기억/망각 토글 + 미보유 습득(NPC 재료/축성) (정보창에서). 재능/장비부여/기억력 부족은 불가.
    void OnClickMemorize()
    {
        // 장비 착용 부여(무점유)는 망각 불가 (BindEvent는 Button.interactable을 무시하므로 코드 가드)
        if (Managers.Ability.IsEquipGranted(SkillId) || Managers.Ability.IsCompanionGranted(SkillId)) return;

        // 미보유 → 출처별 습득 시도 (NPC 재료 소모 / 축성)
        if (!Managers.Ability.IsLearned(SkillId)) { TryAcquire(); return; }
        if (Managers.Ability.IsTalent(SkillId)) return;

        bool wasEquipped = Managers.Ability.IsEquipped(SkillId);
        if (wasEquipped) Managers.Ability.Forget(SkillId);                               // 버튼3-2: 잊기
        else if (!Managers.Ability.Memorize(SkillId))                                    // 버튼3-1: 기억(기억력 부족이면 실패)
        {
            Debug.Log($"[SkillDetail] 기억력 부족 — {SkillId} 기억 실패");
            return;
        }

        // 슬52: 클릭 연출 — 버튼 Anim(양쪽). 흰색 플래시 뱅은 '기억하기'(memorize)일 때만 (기억중→잊기엔 없음).
        PlayMemorizeButtonPunch();
        if (!wasEquipped) PlayMemorizeFlash();

        // 스탯 재적용 + 열려있는 능력 목록/디테일 갱신
        var player = FindObjectOfType<PlayerController>();
        if (player != null) player.RefreshStats();
        FindObjectOfType<UI_SkillBookPopup>()?.RefreshUI();
        RefreshMemorizeState();
        RefreshMidState();      // 슬52: Active_Ef(아이콘 배경 빛) 추가/제거 반영
        ApplyEquipMaterial();   // 슬52: 능력 이름 Frame Sprite에 Material_Equip 추가/제거
    }

    // 미보유 능력 습득 — NPC 출처: 재료 소모(TryLearnFromNpc) / Class 출처: 축성(TryAnoint, 숙련도 100 소진).
    //  성공 시 정보창 전면 갱신(재료 박스 수량 포함) + 기억하기와 동일 연출.
    void TryAcquire()
    {
        var src = Managers.Ability.GetSource(SkillId);
        bool ok = src == Define.AbilitySource.Npc   ? Managers.Ability.TryLearnFromNpc(SkillId)
                : src == Define.AbilitySource.Class ? Managers.Ability.TryAnoint(SkillId)
                : false;
        if (!ok) return;

        // 습득 연출 — 기억하기와 동일 (버튼 Punch + 흰색 플래시)
        PlayMemorizeButtonPunch();
        PlayMemorizeFlash();

        // 스탯 재적용(재능=점유0은 습득 즉시 적용) + 열려있는 능력 목록 갱신. 직업창은 OnAbilityChanged 구독으로 자동 갱신.
        Managers.Game?.Player?.RefreshStats(fullHeal: false);
        FindAnyObjectByType<UI_SkillBookPopup>()?.RefreshUI();

        RefreshAllStates();
    }

    // 습득/축성 후 정보창 전면 갱신 — 상태 배지/설명/출처(재료 수량·색)/버튼/사이즈까지 SetInfo와 동일 순서(애니 제외).
    void RefreshAllStates()
    {
        RefreshMemorizeState();
        RefreshMidState();
        RefreshDescLayout();     // 미보유 "- 습득 필요 -" → 설명 표시 전환
        RefreshSourceUI();       // 슬50: 재료 박스 보유 수량/색 갱신
        RefreshBottomButtons();
        ApplyEquipMaterial();
        RefreshAutoSize();
        ApplyBottomMargin();
    }

    // 슬52: 기억/망각 버튼 Punch (양쪽 공통).
    void PlayMemorizeButtonPunch()
    {
        var btn = GetButton((int)Buttons.MemorizeBtn);
        if (btn == null) return;
        var t = btn.transform;
        t.DOKill(); t.localScale = Vector3.one;
        t.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.25f, 8, 1f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    // 슬52: '기억하기'(memorize) 시 흰색 "플래시 뱅". 사용자 요구:
    //  ① 기억중→잊기엔 안 나옴(OnClickMemorize에서 memorize일 때만 호출). ② SubFrame 크기로만(SubFrame에 stretch).
    //  ③ 배경만 깜박 — SubFrame의 '첫 자식'(테두리/콘텐츠보다 뒤)에 둬서 글자/이미지는 위에 유지.
    //  ※ Flash_USETHIS 프리팹은 Animator+ScriptMachine이 런타임 인스턴스서 재생 안 돼(알파 0 리셋) 순수 흰 Image+코루틴 Lerp로 구동
    //    (이 프로젝트는 Image.DOFade도 알파 갱신 안 됨 — 트랜스폼 트윈만 정상).
    void PlayMemorizeFlash()
    {
        if (_subFrameRT == null)
            _subFrameRT = System.Array.Find(GetComponentsInChildren<Transform>(true), x => x.name == "SubFrame") as RectTransform;
        if (_subFrameRT == null) return;

        var flashGo = new GameObject("MemoFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        var flashRT = (RectTransform)flashGo.transform;
        flashRT.SetParent(_subFrameRT, false);
        flashRT.anchorMin = Vector2.zero; flashRT.anchorMax = Vector2.one;   // SubFrame 크기로 채움
        flashRT.offsetMin = Vector2.zero; flashRT.offsetMax = Vector2.zero;
        flashRT.localScale = Vector3.one;
        flashRT.SetAsFirstSibling();   // SubFrame 테두리/콘텐츠보다 뒤 → 배경만 깜박

        var flashImg = flashGo.GetComponent<UnityEngine.UI.Image>();
        flashImg.color = new Color(1f, 1f, 1f, 0f);
        flashImg.raycastTarget = false;
        StartCoroutine(CoMemorizeFlash(flashImg, flashGo));
    }

    // 슬52: 흰색 플래시 "뱅" — 알파 0→0.85(번쩍) → 0(사그라듦). 프레임 무관(unscaledDeltaTime)으로 진행 후 파괴.
    IEnumerator CoMemorizeFlash(UnityEngine.UI.Image img, GameObject go)
    {
        const float peak = 0.85f, inDur = 0.06f, outDur = 0.34f;
        float e = 0f;
        while (e < inDur) { e += Time.unscaledDeltaTime; SetImgAlpha(img, Mathf.Lerp(0f, peak, e / inDur)); yield return null; }
        e = 0f;
        while (e < outDur) { e += Time.unscaledDeltaTime; SetImgAlpha(img, Mathf.Lerp(peak, 0f, e / outDur)); yield return null; }
        if (go != null) Destroy(go);
    }

    static void SetImgAlpha(UnityEngine.UI.Image img, float a)
    {
        if (img == null) return;
        var c = img.color; c.a = a; img.color = c;
    }

    // 능력 아이콘(WIcon)을 SkillData.IconLabel로 교체.
    //  이 처리가 없어서 상세정보창은 어떤 능력을 열든 **프리팹에 저작된 아이콘 하나**만 보여줬다
    //  (능력 65종이 각자 다른 아이콘을 갖고 있는데도 전부 같아 보이던 원인).
    void ApplySkillIcon()
    {
        var icon = GetImage((int)Images.WIcon);
        if (icon == null) return;
        if (!Managers.Data.SkillDic.TryGetValue(SkillId, out var d)) return;
        if (string.IsNullOrEmpty(d.IconLabel)) return;

        var sp = Managers.Resource.Load<Sprite>(d.IconLabel);
        if (sp != null) icon.sprite = sp;   // 로드 실패 시 프리팹 기본 아이콘 유지
    }

    static Material _equipMat;   // 슬52: Material_Equip 캐시(IMPORTS → Addressable 등록 필요. 미로드 시 no-op)
    // 슬52: 기억중이면 능력 이름 Frame의 Sprite에 Material_Equip(샤인) 추가, 망각/미기억이면 제거.
    void ApplyEquipMaterial()
    {
        if (_equipMat == null) _equipMat = Managers.Resource.Load<Material>("Material_Equip");
        var glow = GetImage((int)Images.Glow);   // 능력 이름 Frame(SkillDetailPopupTitle>Mask>Glow)의 Sprite
        if (glow == null) return;
        bool on = Managers.Ability.IsEquipped(SkillId) || Managers.Ability.IsTalent(SkillId);
        // Material_Equip이 로드되면 적용, 아니면 기본 머티리얼 유지(플래그)
        if (_equipMat != null) glow.material = on ? _equipMat : null;
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

        // 슬45: 이름 바 위 상태 배지(MasterAndMEMGroup) — 최대등급=완벽숙달(FF00F5) / 기억중=기억중 / 재능=재능(둘 다 파랑 3BA6FF).
        //  최대등급과 기억중(재능)은 독립 조건 → 둘 다 성립하면 둘 다 표시(좌우로 분리). 둘 다 같은 위치라 겹침 방지.
        bool showMaster = atMax;
        bool showMemBadge = talent || (owned && equipped &&
            (!Managers.Ability.IsCompanionGranted(SkillId) || Managers.Game.EquippedAbilities.Contains(SkillId)));
        var mIcon   = GetImage((int)Images.MasterIcon)?.rectTransform;
        var memIcon = GetImage((int)Images.MemorizedIcon)?.rectTransform;
        if (!_badgeBaseCached && mIcon != null) { _badgeBaseX = mIcon.anchoredPosition.x; _badgeBaseCached = true; }

        SetActive(GetImageGo((int)Images.MasterIcon),    showMaster);
        SetActive(GetImageGo((int)Images.MemorizedIcon), showMemBadge);
        if (showMaster)
            SetText(Texts.Txt_MasterIcon_02, Util.Localize(Define.LocalizeKey.AbilityMaster_Text));   // "완벽숙달"
        if (showMemBadge)
        {
            // 재능이면 "재능"(TalentText), 아니면 "기억중"(Memorized_Text_Full). 프레임 파랑 3BA6FF.
            SetText(Texts.Txt_MemorizedIcon_01, Util.Localize(talent ? Define.LocalizeKey.TalentText : Define.LocalizeKey.Memorized_Text_Full));
            var memFrame = GetImage((int)Images.Img_MemorizedIcon_01);
            if (memFrame != null) memFrame.color = Util.HexToColor("3BA6FF");
        }
        // 둘 다 표시 시 좌우로 분리(완벽숙달 왼쪽 / 기억중 오른쪽), 하나만이면 기본 위치 중앙.
        float off = (showMaster && showMemBadge) ? 12f : 0f;
        if (mIcon   != null) mIcon.anchoredPosition   = new Vector2(_badgeBaseX - off, mIcon.anchoredPosition.y);
        if (memIcon != null) memIcon.anchoredPosition = new Vector2(_badgeBaseX + off, memIcon.anchoredPosition.y);

        // 점유량(GotMEM) / 재능(NoMEM) — 점유량 0이면 재능 표기
        SetActive(GetObject((int)GameObjects.GotMEM), (owned || sd?.Source == Define.AbilitySource.Class) && !talent);
        SetActive(GetTextGo((int)Texts.NoMEM),        talent);

        // 포획과 축성은 습득 전에 효과를 비교할 수 있다.
        SetActive(GetTextGo((int)Texts.Lv0Text), !owned
            && sd?.Source != Define.AbilitySource.Minion && sd?.Source != Define.AbilitySource.Class);

        // 슬43/45: 능력이 [재능]이거나 기억(사용)중이면 아이콘 배경 Active_Ef(빛 sweep 루프) 활성.
        //  (모션/색은 prefab authored 이펙트 유지 — 코드는 토글만. UI_SkillBookSkill.cs와 동일 패턴)
        bool activeEf = talent || (owned && equipped);
        SetActive(GetObject((int)GameObjects.SkillDetailActiveEf), activeEf);
        // 슬45: 최대 등급 글로우(shop_frame_glow #636363) — prefab 노드는 MasterIcon로 대표(별도 노드 부재 가능 → MasterIcon 토글로 처리됨)
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
            // 슬46: 이야기 = Italic + 중앙 정렬 (폰트 크기 상대규칙은 prefab authored 값 유지)
            if (has)
            {
                storyT.text = story;
                storyT.fontStyle = TMPro.FontStyles.Italic;
                storyT.alignment = TMPro.TextAlignmentOptions.Center;
                ApplyFont(storyT, Define.FontType.UI);
            }
        }

        // 슬46/51: 능력 등급 텍스트(중앙 정렬). 버튼2 미리보기(_previewNext) 시 "현재 ▶ 다음"(다음=#00FF00), 평소 "현재/최대"(최대도달=#FF00FF).
        var lvT = GetText((int)Texts.SkillLvTxt);
        if (lvT != null)
        {
            lvT.alignment = TMPro.TextAlignmentOptions.Center;
            int glv  = Managers.Ability.GetLevel(SkillId);
            int gmax = Managers.Data.SkillDic.TryGetValue(SkillId, out var gd) ? Mathf.Max(1, gd.MaxLevel) : Mathf.Max(1, glv);
            string lvlKey = Util.Localize(Define.LocalizeKey.LevelText);
            if (_previewNext && glv < gmax)
                lvT.text = $"【 {lvlKey}: <color=yellow>{glv}</color> ▶ <color=#00FF00>{glv + 1}</color> 】";
            else
            {
                string gradeCol = (glv >= gmax) ? "#FF00FF" : "yellow";
                lvT.text = $"【 {lvlKey}: <color={gradeCol}>{glv}</color>/{gmax} 】";
            }
            ApplyFont(lvT, Define.FontType.UI);
        }

        // 포획·축성의 보상을 습득 전에 확인할 수 있다.
        bool showDesc = Managers.Ability.IsLearned(SkillId) ||
            (Managers.Data.SkillDic.TryGetValue(SkillId, out var data)
                && (data.Source == Define.AbilitySource.Minion || data.Source == Define.AbilitySource.Class));
        var realT = GetText((int)Texts.LvEffectTxt_Real);
        if (realT != null)
        {
            realT.gameObject.SetActive(showDesc);
            if (showDesc)
            {
                string desc = Managers.Ability.GetDesc(SkillId);
                string trig = BuildTriggerDesc(SkillId);
                if (!string.IsNullOrEmpty(trig)) desc = string.IsNullOrEmpty(desc) ? trig : desc + "\n\n" + trig;
                // 슬51 버튼2: 다음 등급 미리보기 토글 시 "현재 ▶ 다음" 한 줄 추가
                if (_previewNext) desc += BuildNextLevelPreview();
                realT.text = desc;
                // 슬46: 능력 설명 = 중앙 + 상단 정렬. 상단정렬이라 길어지면 아래로만 넘쳐 위(능력 등급)와 겹치지 않음.
                realT.alignment = TMPro.TextAlignmentOptions.Top;
                // 슬46: 가로 넘침 방지 — ContentSizeFitter horizontal(=PreferredSize면 텍스트 자연폭으로 늘어나 팝업 밖으로 넘침)을
                //  Unconstrained로 끄고, 폭을 팝업 안(부모폭-26)으로 고정해 wrap시킨다.
                var csf = realT.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (csf != null) csf.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                var drt = realT.rectTransform;
                drt.sizeDelta = new Vector2(-26f, drt.sizeDelta.y);
                ApplyFont(realT, Define.FontType.UI);
            }
        }
    }

    // ── 슬46: 능력 설명 텍스트 양에 따라 정보창 세로 사이즈 자동 변화 ──
    //  프레임(Img_LvEffectTxt_Size_01)에는 설명(LvEffectTxt_Real)이 stretch로 묶여 있고,
    //  이름/아이콘은 상단 앵커, 출처(ReqGroup)/버튼은 하단 앵커에 묶여 있다.
    //  → 프레임을 D 키우고(sizeDelta.y) D/2 아래로 내리면(anchoredPos.y):
    //     상단(이름/아이콘) 고정 · 설명영역 아래로 D 확장 · 하단(출처·버튼) D 하강.
    //  D = 설명 preferredHeight − 기준(base) 영역높이. 재오픈/미리보기 토글 누적 방지 위해 항상 base 리셋→측정→성장.
    void RefreshAutoSize()
    {
        if (_autoFrame == null)   _autoFrame   = GetImage((int)Images.Img_LvEffectTxt_Size_01)?.rectTransform;
        if (_autoDescTmp == null) _autoDescTmp = GetText((int)Texts.LvEffectTxt_Real);
        if (_autoFrame == null || _autoDescTmp == null) return;

        // 설명 프레임의 **가로 폭** 보정.
        //  프레임은 stretch(0~1) 앵커인데 부모(LvEffectTxt_Size)의 폭이 0이라
        //  프레임 40 → SubFrame 32 → 본문 14픽셀까지 좁아졌다. 한 글자도 안 들어가
        //  설명이 "물리 / 공격 / 력을 / 증가 / 시킨 / 다." 처럼 **세로로 쪼개져** 보였다.
        //  팝업 기준 폭(FIXED = 180)에서 좌우 여백을 뺀 값으로 고정한다.
        const float DESC_WIDTH = 150f;
        if (!Mathf.Approximately(_autoFrame.sizeDelta.x, DESC_WIDTH))
            _autoFrame.sizeDelta = new Vector2(DESC_WIDTH, _autoFrame.sizeDelta.y);

        if (!_autoSizeCached)
        {
            _autoFrameBaseSize = _autoFrame.sizeDelta.y;
            _autoFrameBasePos  = _autoFrame.anchoredPosition.y;
            _autoSizeCached = true;
        }

        // 항상 base로 리셋(누적 방지)
        _autoFrame.sizeDelta        = new Vector2(_autoFrame.sizeDelta.x, _autoFrameBaseSize);
        _autoFrame.anchoredPosition = new Vector2(_autoFrame.anchoredPosition.x, _autoFrameBasePos);

        // 설명 미표시(미보유 = "- 습득 필요 -")면 기본 사이즈 유지
        if (!_autoDescTmp.gameObject.activeInHierarchy) return;

        if (_subFrameRT == null)
            _subFrameRT = System.Array.Find(GetComponentsInChildren<Transform>(true), t => t.name == "SubFrame") as RectTransform;
        if (_subFrameRT == null) return;

        // base 상태로 레이아웃 확정 후, SubFrame(장식 패널) 바닥이 '설명 바닥 + 약간'에 오도록 프레임 높이를 줄이/늘림.
        //  (설명 rect엔 CSF가 있어 frame과 무관하게 텍스트 크기로 잡힘 → 빈 공간의 실제 원인은 SubFrame이 설명보다 한참 아래까지
        //   내려가는 것. 그래서 설명 rect 높이가 아니라 'SubFrame 바닥 vs 설명 바닥'을 기준으로 조절. 짧으면 축소/길면 확장.)
        // 처음 열 때(팝업 Instantiate 첫 프레임)는 캔버스가 아직 배치 전이라 GetWorldCorners가 부정확 → 강제 갱신.
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_autoFrame);
        _autoDescTmp.ForceMeshUpdate();
        float scaleY = _autoFrame.lossyScale.y;
        if (scaleY <= 0.0001f) return;
        // 설명 rect엔 CSF가 있어 긴 텍스트는 rect 밖으로 넘침(rect 바닥≠텍스트 바닥) → 텍스트 실제 바닥 = rect상단 − preferredHeight.
        var dc = new Vector3[4]; _autoDescTmp.rectTransform.GetWorldCorners(dc);
        float descScaleY = _autoDescTmp.rectTransform.lossyScale.y;
        float descBottom = dc[2].y - _autoDescTmp.preferredHeight * descScaleY;
        var sc = new Vector3[4]; _subFrameRT.GetWorldCorners(sc);                float subBottom  = sc[0].y;
        const float DESIRED_GAP = 0.45f;   // SubFrame 바닥이 설명 바닥보다 이만큼 아래(world)
        float moveUpWorld = (descBottom - DESIRED_GAP) - subBottom;   // 양수=SubFrame을 위로(프레임 축소) / 음수=확장
        float deltaLocal  = -moveUpWorld / scaleY;
        // 설명 rect의 CSF+앵커로 측정이 길어질수록 과대 산출되어 긴 설명에서 화면 밖으로 넘침 →
        //  성장은 화면에 맞게 cap, 축소는 충분히 허용. (정확한 중앙배치는 ClampPopupToCenter가 보정)
        deltaLocal = Mathf.Clamp(deltaLocal, -130f, 55f);
        if (Mathf.Abs(deltaLocal) < 1f) return;

        _autoFrame.sizeDelta        = new Vector2(_autoFrame.sizeDelta.x, _autoFrameBaseSize + deltaLocal);
        _autoFrame.anchoredPosition = new Vector2(_autoFrame.anchoredPosition.x, _autoFrameBasePos - deltaLocal * 0.5f);
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

        // 슬47: 출처별 헤더 TEXT + 색 (prefab 하드코딩 → 한/영 연동 + spec hex 보정)
        switch (src)
        {
            case Define.AbilitySource.Class:
                SetTextLoc(Texts.Txt_ClassGroup_05, Define.LocalizeKey.Class_Ability_Text, "3DC4FF");
                // 하단 직업명 — 전직 시스템 미구현이라 현재 플레이어 직업명(기본 "여행자") 유지. 색 F3FF00.
                SetTextColorHex(Texts.Txt_ClassGroup_04, "F3FF00");
                break;
            case Define.AbilitySource.Minion:
                SetTextLoc(Texts.Txt_ReqMinionGroup_02, Define.LocalizeKey.Req_Minimon_Text, "ECCC6C");
                break;
            case Define.AbilitySource.Equip:
                SetTextLoc(Texts.Txt_ReqEquipGroup_06, Define.LocalizeKey.Req_Equip_Text, "902EC8");
                break;
            case Define.AbilitySource.Npc:
                SetTextLoc(Texts.Txt_ReqItemGroup_07, Define.LocalizeKey.Req_Item_Text, "8CEC6C");
                break;
            case Define.AbilitySource.Unlimited:
                SetTextLoc(Texts.Txt_UnlimitedPotenGroup_08, Define.LocalizeKey.Unlimited_Potential_Text, "EC706C");
                SetTextLoc(Texts.Txt_UnlimitedPotenGroup_03, Define.LocalizeKey.Unlimited_Potential_Desc_Text, "C8C8C8");
                break;
        }

        _reqMinionId = 0;
        if (src == Define.AbilitySource.Minion)
            RefreshMinionReq(Managers.Ability.GetRequiredMinionId(SkillId));

        _reqItemId = 0;
        if (src == Define.AbilitySource.Npc)
            RefreshItemReq();   // 슬50 필요 재료

        if (src == Define.AbilitySource.Equip)
            RefreshEquipReq();  // 슬49 필요 장비
    }

    // ── 능력 정보창 #7 (슬49): 필요 장비 박스 (강화/내구 상태 → 능력단계). 첫 박스만 사용. ──
    //  prefab 박스(ReqItemBox1_1)는 텍스트 1·체크 1개라, 강화/내구 2줄 rich-text + 통합 체크 1개로 표기(플래그).
    void RefreshEquipReq()
    {
        // #Panel_ReqEquipGroup_02는 prefab 기본 비활성 → 박스 컨테이너 활성화
        var equipPanel = GetObject((int)GameObjects.Panel_ReqEquipGroup_02);
        SetActive(equipPanel, true);

        // 슬49(폭 보정): 박스 컨테이너 HLG가 childForceExpandWidth=true라 단일 박스가 좌측 정렬됨 → 끄면 UpperCenter로 가운데 정렬(스펙 "가운데 정렬").
        var equipHlg = equipPanel != null ? equipPanel.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>() : null;
        if (equipHlg != null) equipHlg.childForceExpandWidth = false;

        // 장비 박스 1_2~1_5 숨김 (요구 장비 1종)
        SetActive(GetButton((int)Buttons.ReqItemBox1_2).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqItemBox1_3).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqItemBox1_4).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqItemBox1_5).gameObject, false);

        var box0 = GetButton((int)Buttons.ReqItemBox1_1);
        SetActive(box0.gameObject, true);

        if (!Managers.Data.SkillDic.TryGetValue(SkillId, out var d)) return;
        _reqEquipId = d.RequiredEquipId;
        if (_reqEquipId <= 0 || !Managers.Data.EquipmentDic.TryGetValue(_reqEquipId, out var ed)) return;

        // 능력 최대 레벨 도달 시 "- 최다 등급 -"(Ability_MaxLevel_Text)만 표기, 박스 숨김(상호배타).
        int abilLv = Managers.Ability.GetLevel(SkillId);
        bool abilityMax = Managers.Ability.IsLearned(SkillId) && abilLv >= Mathf.Max(1, d.MaxLevel);
        var maxLvlT = GetText((int)Texts.Txt_ReqEquipGroup_03);
        if (maxLvlT != null)
        {
            SetActive(maxLvlT.gameObject, abilityMax);
            if (abilityMax) { maxLvlT.text = Util.Localize(Define.LocalizeKey.Ability_MaxLevel_Text); ApplyFont(maxLvlT, Define.FontType.UI); }
        }
        SetActive(box0.gameObject, !abilityMax);
        if (abilityMax) return;

        // 최대 강화도 = 해당 장비 EquipEnhance 중 최대 Level (없으면 +9 기본).
        int maxEnh = 0;
        foreach (var kv in Managers.Data.EquipEnhanceDic)
            if (kv.Value.EquipDataId == _reqEquipId && kv.Value.Level > maxEnh) maxEnh = kv.Value.Level;
        if (maxEnh <= 0) maxEnh = 9;

        var owned = Managers.Game?.OwnedEquipments?.Find(e => e.DataId == _reqEquipId);
        bool has = owned != null;
        bool atMaxEnh = has && owned.EnhanceLevel >= maxEnh;
        bool atMaxDur = has && owned.Durability >= ed.MaxDurability;
        //  (슬49 표: 보유 시 1단계, 강화최대/내구최대 각 +1. 능력 레벨은 AbilityManager.GetLevel이 장비 상태에서
        //   동적 산출(1+최대강화+최대내구)하므로 【LEVEL: lv/max】표기는 자동 반영. 여기선 강화/내구 상태 표시만.)

        // 장비 아이콘
        var icon = GetImage((int)Images.Img_ReqItemBox1_1_04);
        if (icon != null && !string.IsNullOrEmpty(ed.SpriteName))
        {
            var sp = Managers.Resource.Load<Sprite>(ed.SpriteName);
            if (sp != null) { icon.sprite = sp; icon.preserveAspect = true; }
        }

        // 슬49: Max_Enh_Text(최대 강화도 시 #00FF00, 그외 909090) + MAX_Dur_Text(보유&최대 내구도 시 #00FF00, 그외 909090).
        //  박스 텍스트 노드 1개 → 2줄 rich-text로 함께 표기.
        string enhCol = atMaxEnh ? "#00FF00" : "#909090";
        string durCol = atMaxDur ? "#00FF00" : "#909090";
        var txt = GetText((int)Texts.Txt_ReqItemBox1_1_06);
        if (txt != null)
        {
            txt.text = $"<color={enhCol}>{Util.Localize(Define.LocalizeKey.Max_Enh_Text)}</color>\n<color={durCol}>{Util.Localize(Define.LocalizeKey.MAX_Dur_Text)}</color>";
            SetActive(txt.gameObject, true);
            ApplyFont(txt, Define.FontType.UI);
            // 슬49(오버플로 보정): 텍스트에 CSF(PreferredSize)가 있어 rect가 텍스트 폭(≈23)으로 늘어남.
            //  박스가 좌측 정렬이면 "최대 내구도"가 팝업 끝까지 넘쳤음 → 위 equipHlg.childForceExpandWidth=false로 박스를 중앙정렬해 해결.
            //  (autosize는 CSF와 충돌해 폰트가 과축소되므로 사용하지 않음.)
        }

        // 슬49: 우상단 체크 ICON 2개 — 조건마다 하나씩 #0AFF00 점등, 기본 #BEBEBE(a150).
        //  1. 강화도 최대 / 2. 보유 & 내구도 최대. prefab 장비박스엔 체크 노드가 없어 런타임 생성(EnsureEquipChecks).
        EnsureEquipChecks();
        if (_equipChecks != null)
        {
            Color32 chkOn  = new Color32(0x0A, 0xFF, 0x00, 0xFF);
            Color32 chkOff = new Color32(0xBE, 0xBE, 0xBE, 150);
            _equipChecks[0].color = atMaxEnh ? (Color)chkOn : (Color)chkOff;   // 1. 강화도 최대
            _equipChecks[1].color = atMaxDur ? (Color)chkOn : (Color)chkOff;   // 2. 보유 & 내구도 최대 (atMaxDur는 보유 포함)
        }

        // 슬49: 해금된 장비면 항상 Interactive O(보유무관). 해금 전이면 X — 시드 장비는 해금 상태.
        box0.interactable = true;
    }

    Image[] _equipChecks;   // 슬49: 장비박스 런타임 체크마크 2개 캐시
    // 슬49: 장비 박스 체크 ICON 2개 런타임 생성 (prefab 부재). Checkmark_01은 Resource.Load 미프리로드라
    //  미니몬 박스 체크 노드(Img_ReqMinionBox_06)의 baked 스프라이트를 차용. 위치/크기는 미니몬 박스와 동일(✓✓ 겹침).
    void EnsureEquipChecks()
    {
        if (_equipChecks != null) return;
        var box = GetButton((int)Buttons.ReqItemBox1_1)?.transform;
        if (box == null) return;
        var srcSprite = GetImage((int)Images.Img_ReqMinionBox_06)?.sprite;
        _equipChecks = new Image[2];
        Vector2[] pos = { new Vector2(5f, 5f), new Vector2(2f, 5f) };
        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject("Img_EquipCheck_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(box, false);
            rt.SetAsLastSibling();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(9.73f, 9.73f);
            rt.anchoredPosition = pos[i];
            var im = go.GetComponent<Image>();
            im.sprite = srcSprite; im.raycastTarget = false; im.preserveAspect = true;
            _equipChecks[i] = im;
        }
    }

    void OnClickReqEquip()
    {
        if (_reqEquipId <= 0) return;
        Debug.Log($"[SkillDetail] 장비 박스 클릭 — equipId={_reqEquipId} (장비 상세 팝업 연결 대기)");
    }

    // ── 능력 정보창 #8 (슬50): 필요 재료 박스 게이팅 (보유/필요 + 색). 첫 박스만 사용. ──
    void RefreshItemReq()
    {
        // 재료 박스 1~4 숨김 (요구 재료 1종 — 데이터 확장 시 박스 추가 가능)
        SetActive(GetButton((int)Buttons.ReqItemBox_1).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqItemBox_2).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqItemBox_3).gameObject, false);
        SetActive(GetButton((int)Buttons.ReqItemBox_4).gameObject, false);

        var box0 = GetButton((int)Buttons.ReqItemBox);
        SetActive(box0.gameObject, true);

        if (!Managers.Data.SkillDic.TryGetValue(SkillId, out var d)) return;
        _reqItemId = d.RequiredItemId;
        int need = Mathf.Max(0, d.RequiredItemCount);

        int lv = Managers.Ability.GetLevel(SkillId);
        int maxLv = Mathf.Max(1, d.MaxLevel);
        bool atMax = Managers.Ability.IsLearned(SkillId) && lv >= maxLv;

        // 슬50: 최대치 도달 시 "- 최다 등급 -"(Ability_MaxLevel_Text)로 변경, 박스/수량 숨김
        var maxT = GetText((int)Texts.Txt_ReqGroup_01);
        if (maxT != null)
        {
            SetActive(maxT.gameObject, atMax);
            if (atMax) { maxT.text = Util.Localize(Define.LocalizeKey.Ability_MaxLevel_Text); ApplyFont(maxT, Define.FontType.UI); }
        }
        SetActive(box0.gameObject, !atMax);
        if (atMax) return;

        // 재료 아이콘
        var icon = GetImage((int)Images.Img_ReqItemBox_03);
        var itemData = Managers.Data.ItemDic.TryGetValue(_reqItemId, out var it) ? it
                     : Managers.Data.RequisiteItemDic.TryGetValue(_reqItemId, out var rit) ? rit : null;
        if (icon != null && itemData != null && !string.IsNullOrEmpty(itemData.SpriteName))
        {
            var sp = Managers.Resource.Load<Sprite>(itemData.SpriteName);
            if (sp != null) { icon.sprite = sp; icon.preserveAspect = true; }
        }

        // 슬50: 현재 보유량/필요량 + 색 (0=red / 0<x<필요=orange / >=필요=#00FF00)
        int have = Managers.Game != null ? Managers.Game.GetItemCount(_reqItemId) : 0;
        string col = have <= 0 ? "red" : have >= need ? "#00FF00" : "orange";
        var cntT = GetText((int)Texts.Txt_ReqItemBox_03);
        if (cntT != null) { cntT.text = $"<color={col}>{have}</color>/{need}"; SetActive(cntT.gameObject, true); ApplyFont(cntT, Define.FontType.UI); }

        box0.interactable = true;   // 슬50: 획득 여부 무관 항상 Interactive O
    }

    void OnClickReqItem()
    {
        if (_reqItemId <= 0) return;
        // 슬50: 누를 시 해당 아이템 상세정보창 — 전용 아이템 상세 팝업 시스템 정비 후 연결(현재 로그).
        Debug.Log($"[SkillDetail] 재료 박스 클릭 — itemId={_reqItemId} (아이템 상세 팝업 연결 대기)");
    }

    // 로컬 텍스트 + hex 색을 함께 적용 (슬47 헤더).
    void SetTextLoc(Texts id, string locKey, string hex)
    {
        var t = GetText((int)id);
        if (t == null) return;
        t.text = Util.Localize(locKey);
        if (ColorUtility.TryParseHtmlString("#" + hex, out var c)) t.color = c;
        ApplyFont(t, Define.FontType.UI);
    }

    void SetTextColorHex(Texts id, string hex)
    {
        var t = GetText((int)id);
        if (t != null && ColorUtility.TryParseHtmlString("#" + hex, out var c)) t.color = c;
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
        int sync = Mathf.RoundToInt(Managers.MinMon.GetSyncRate(minionId) * Define.ABILITY_PERCENT_BASE);
        // 슬48: 상호작용 = 기록도>0 또는 보유 (0%+미보유만 X). 텍스트/잠금 노출은 동일 기준.
        bool unlocked = rec > 0 || owned;

        // 영구 습득 조건은 처치 기록도가 아니라 포획 공명도다.
        string rc = sync <= 0 ? "red" : sync >= Define.ABILITY_PERCENT_BASE ? "#00FF00" : "orange";
        var recT = GetText((int)Texts.Txt_ReqMinionBox_07);
        if (recT != null) { recT.text = $"<color={rc}>{sync}%</color>"; SetActive(recT.gameObject, unlocked); }

        // 현재 진행도가 어떤 조건인지 짧게 표시한다.
        var subT = GetText((int)Texts.Txt_ReqMinionBox_05);
        if (subT != null)
        {
            SetActive(subT.gameObject, unlocked);
            subT.text = Util.Localize(Define.LocalizeKey.LinkText);
            SetTextColor(subT, sync >= Define.ABILITY_PERCENT_BASE ? new Color32(0x00, 0xFF, 0x00, 0xFF) : new Color32(0x90, 0x90, 0x90, 0xFF));
        }

        // 슬48: 미니몬 이미지 = 도감 ICON과 동일(CreatureData.IconLabel) 데이터 연결.
        //  스프라이트 미프리로드면 sprite=null → 흰색 박스(사용자 승인: 이미지 준비 전까지 허용. 라비슬라임 등도 흰색 OK).
        //  ※ 이전 버그: 이 노드(Img_GlowMask_30)를 allSS '체크'로 착색 → 비AllSS 미니몬 이미지가 회색틴트됨. 제거.
        var minImg = GetImage((int)Images.Img_GlowMask_30);
        if (minImg != null)
        {
            Sprite sp = null;
            if (Managers.Data.CreatureDic.TryGetValue(minionId, out var cd) && !string.IsNullOrEmpty(cd.IconLabel))
                sp = Managers.Resource.Load<Sprite>(cd.IconLabel);
            minImg.sprite = sp;
            if (sp != null) minImg.preserveAspect = true;
            minImg.color = Color.white;
        }

        // 슬48: 우상단 체크 ICON 2개(Checkmark_01) — 조건 성립마다 하나씩 #0AFF00 점등, 기본 #BEBEBE(a150).
        //  Img_ReqMinionBox_06 = 공명도 100% / Img_ReqMinionBox_04 = 능력치 모두 SS.
        Color32 chkOn  = new Color32(0x0A, 0xFF, 0x00, 0xFF);
        Color32 chkOff = new Color32(0xBE, 0xBE, 0xBE, 150);
        var chk1 = GetImage((int)Images.Img_ReqMinionBox_06);
        var chk2 = GetImage((int)Images.Img_ReqMinionBox_04);
        if (chk1 != null) { SetActive(chk1.gameObject, unlocked); chk1.color = (sync >= Define.ABILITY_PERCENT_BASE) ? (Color)chkOn : (Color)chkOff; }
        if (chk2 != null) { SetActive(chk2.gameObject, unlocked); chk2.color = allSS ? (Color)chkOn : (Color)chkOff; }

        // 잠금 오버레이("?" + "미니몬 또는 기록도 필요") — 미해금 시 표시
        SetActive(GetImageGo((int)Images.Img_ReqMinionBox_14), !unlocked);     // "?" 프레임
        SetActive(GetObject((int)GameObjects.Panel_ReqMinionBox_02), !unlocked); // NeedMinion 텍스트
        // 슬48: 잠금 라벨 문구 주입 (기존 loc MinimonSkillLockedText)
        if (!unlocked) SetText(Texts.Txt_BlockImg_05, Util.Localize("MinimonSkillLockedText"));

        box0.interactable = unlocked;
    }

    void OnClickReqMinion()
    {
        if (_reqMinionId <= 0) return;
        Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(_reqMinionId);
    }

    // ── 슬51: 능력 정보창 최하단 3버튼 (버튼1 NPC장소 / 버튼2 다음등급 / 버튼3 기억) ──
    //  prefab의 물약 잔재 버튼(Back/Craft/CantCraft/NotInPace)은 숨기고, 의미를 재매핑.
    //  버튼3(기억=MemorizeBtn)은 RefreshMemorizeState가 담당 → 여기선 항상 표시 유지.
    void RefreshBottomButtons()
    {
        // 물약 제작 잔재 버튼 숨김 (CraftBtn은 버튼1=NPC장소로 재활용 → 여기서 숨기지 않음)
        SetActive(GetButton((int)Buttons.BackBtn).gameObject, false);
        SetActive(GetImageGo((int)Images.CantCraftBtn), false);
        SetActive(GetImageGo((int)Images.NotInPaceCraftBtn), false);

        bool owned  = Managers.Ability.IsLearned(SkillId);
        int  lv     = Managers.Ability.GetLevel(SkillId);
        int  maxLv  = Managers.Data.SkillDic.TryGetValue(SkillId, out var d) ? Mathf.Max(1, d.MaxLevel) : lv;
        bool hasNext = owned && lv < maxLv;   // 슬51 버튼2: 다음 등급이 존재할 경우만 표시

        // 슬51 버튼2(우상): 다음 등급 정보 미리보기 (NextLvlBtn). 누르기 전=다음등급정보 / 누른 후=뒤로(BackBtnText). 글로우 8E5E00.
        SetActive(GetButton((int)Buttons.NextLvlBtn).gameObject, hasNext);
        if (hasNext)
        {
            string label = _previewNext ? Util.Localize(Define.LocalizeKey.BackBtnText) : Util.Localize(Define.LocalizeKey.NextLevelInfoBtnText);
            SetText(Texts.Txt_NextLvlBtn_07, label);
            SetButtonGlow(Images.Img_Item_11, "8E5E00");   // 슬51 버튼2 글로우색
        }
        else _previewNext = false;

        // 슬51 버튼1(좌상): NPC 학습 장소 (Source=Npc + NpcLocation 있을 때만). CraftBtn 재활용(버튼2/3과 동일 UI). 글로우 000000.
        //  (구 우상단 라벨 WhereToFindTxtFrame은 숨김 — 위치가 버튼행으로 '이동')
        SetActive(GetImageGo((int)Images.WhereToFindTxtFrame), false);
        SetActive(GetTextGo((int)Texts.WhereToFindTxt), false);
        string loc = (d != null && Managers.Ability.GetSource(SkillId) == Define.AbilitySource.Npc) ? d.NpcLocation : null;
        bool hasLoc = !string.IsNullOrEmpty(loc);
        SetActive(GetButton((int)Buttons.CraftBtn).gameObject, hasLoc);
        if (hasLoc)
        {
            SetText(Texts.Txt_CraftBtn_05, loc);
            SetButtonGlow(Images.Img_Item_08, "000000");   // 슬51 버튼1 글로우색
        }
    }

    // 슬51/52: 버튼 Shop_frame_glow(Img_Item_*) 색 지정. (버튼1 000000 / 버튼2 8E5E00 / 버튼3 0036A6·000000)
    void SetButtonGlow(Images glowImg, string hex)
    {
        var im = GetImage((int)glowImg);
        if (im != null && ColorUtility.TryParseHtmlString("#" + hex, out var c)) im.color = c;
    }

    // 슬51 버튼2: 다음 등급 미리보기 토글 → 설명 텍스트에 "현재 ▶ 다음" 표기.
    void OnClickNextLvl()
    {
        _previewNext = !_previewNext;
        RefreshDescLayout();
        RefreshBottomButtons();
        RefreshAutoSize();      // 슬46: 미리보기 줄 추가/제거에 맞춰 세로 사이즈 재조정
        ApplyBottomMargin();    // 슬51: 하단 UI를 SubFrame 밑으로 재정렬
    }

    // 슬51 버튼2: 다음 등급 수치 미리보기 한 줄 (StatBonus×Level 파생). 높으면 #00FF00, 낮으면 red.
    string BuildNextLevelPreview()
    {
        if (!Managers.Data.SkillDic.TryGetValue(SkillId, out var d)) return "";
        int lv = Managers.Ability.GetLevel(SkillId);
        if (Mathf.Approximately(d.StatBonus, 0f)) return "";   // 수치형 능력만 미리보기(효과형은 생략)
        float cur  = d.StatBonus * lv;
        float next = d.StatBonus * (lv + 1);
        string col = next > cur ? "#00FF00" : next < cur ? "red" : "yellow";
        return $"\n\n<color=yellow>{StatKor(d.StatType)} +{cur:0} ▶ <color={col}>+{next:0}</color></color>";
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

    // 슬43: 진입 Move 애니메이션 (From y-120 OutBack 0.5). SkillDetailPopupMain 프레임을 _frameOrigin 기준으로.
    void PlayOpenAnim()
    {
        var mainGo = GetObject((int)GameObjects.SkillDetailPopupMain);
        if (mainGo == null) return;
        var rt = mainGo.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.DOKill();
        rt.anchoredPosition = _frameOrigin + new Vector2(0f, FRAME_FROM_Y);
        rt.DOAnchorPos(_frameOrigin, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // 슬43: 종료 — 진입 Move 역재생(DoPlayBackwards 상응) 후 닫기.
    void OnClickClose()
    {
        if (_isClosing) return;
        _isClosing = true;

        var mainGo = GetObject((int)GameObjects.SkillDetailPopupMain);
        var rt = mainGo != null ? mainGo.GetComponent<RectTransform>() : null;
        if (rt == null) { ClosePopupUI(); return; }

        rt.DOKill();
        rt.DOAnchorPos(_frameOrigin + new Vector2(0f, FRAME_FROM_Y), 0.5f)
          .SetEase(Ease.InBack)
          .SetUpdate(true)
          .OnComplete(() => ClosePopupUI());
    }
}
