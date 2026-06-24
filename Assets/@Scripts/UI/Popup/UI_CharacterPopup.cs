using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_CharacterPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        CharacterPopupFrame,
        ContentsStats,
        STRStatGroup,
        VITStatGroup,
        DEXStatGroup,
        MAGStatGroup,
        StatResetBtnGrow,
        Mask,
        StatDetailBtnGrow,
        StatConfirmBtnGrow,
        StatDetailFrame,
        CCDetailGroup,
        ACCDetailGroup,
        P_RESDetailGroup,
        EVADetailGroup,
        EXP_RATEDetailGroup,
        CDMGDetailGroup,
        ASDetailGroup,
        M_RESDetailGroup,
        RECOVDetailGroup,
        DROP_RATEDetailGroup,
        Xbutton,
        StatDetailBtnMask,
        PATKUnderArrowImage,
        MATKUnderArrowImage,
        HPUnderArrowImage,
        DEFUnderArrowImage,
    }

    enum Texts
    {
        title,
        STRAmountTxt,
        STRInvPoint,
        VITAmountTxt,
        VITInvPoint,
        DEXAmountTxt,
        DEXInvPoint,
        MAGAmountTxt,
        MAGInvPoint,
        SubTitle_ATKText,
        PATKBaseStatString,
        PATKBaseStatNum,
        PATKIncreaseStatString,
        PATKIncreaseStatNum,
        PATKBoostStatString,
        PATKBoostStatNum,
        PATKEqualTxt,
        PATKStatString,
        PATKStatNum,
        PATKUnderArrowText,
        CCStatString,
        CCStatNum,
        ACCStatString,
        ACCStatNum,
        SubTitle_DEFText,
        HPBaseStatString,
        HPBaseStatNum,
        HPIncreaseStatString,
        HPIncreaseStatNum,
        HPBoostStatString,
        HPBoostStatNum,
        HPEqualTxt,
        HPStatString,
        HPStatNum,
        HPUnderArrowText,
        P_RESStatString,
        P_RESStatNum,
        EVAStatString,
        EVAStatNum,
        SubTitle_UTILText,
        EXP_RATEStatString,
        EXP_RATEStatNum,
        MATKBaseStatString,
        MATKBaseStatNum,
        MATKIncreaseStatString,
        MATKIncreaseStatNum,
        MATKBoostStatString,
        MATKBoostStatNum,
        MATKEqualTxt,
        MATKStatString,
        MATKStatNum,
        MATKUnderArrowText,
        CDMGStatString,
        CDMGStatNum,
        ASStatString,
        ASStatNum,
        DEFBaseStatString,
        DEFBaseStatNum,
        DEFIncreaseStatString,
        DEFIncreaseStatNum,
        DEFBoostStatString,
        DEFBoostStatNum,
        DEFEqualTxt,
        DEFStatString,
        DEFStatNum,
        DEFUnderArrowText,
        M_RESStatString,
        M_RESStatNum,
        RECOVStatString,
        RECOVStatNum,
        DROP_RATEStatString,
        DROP_RATEStatNum,
        PointsText,
        PointsStatPointsTxt,
        ElixirText,
        ElixirPointsTxt,
        STRStatNameTxt,
        VITStatNameTxt,
        DEXStatNameTxt,
        MAGStatNameTxt,
    }

    enum Buttons
    {
        CharacterPopupBackground,
        STRDePointBtn,
        STRInPointBtn,
        VITDePointBtn,
        VITInPointBtn,
        DEXDePointBtn,
        DEXInPointBtn,
        MAGDePointBtn,
        MAGInPointBtn,
        StatResetBtn,
        StatDetailBtn,
        StatConfirmBtn,
        PATKDetailGroup,
        HPDetailGroup,
        MATKDetailGroup,
        DEFDetailGroup,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        CharacterPopup,
        Contents,
        PointsandElixirBar,
        TopLayout,
        BtmLayout,
        Left,
        SubTitle_ATK,
        PATKSubTexts,
        PATKBase,
        PATKIncrease,
        PATKBoost,
        PATKMainTexts,
        CCMainTexts,
        ACCMainTexts,
        __3,
        SubTitle_DEF,
        HPSubTexts,
        HPBase,
        HPIncrease,
        HPBoost,
        HPMainTexts,
        P_RESMainTexts,
        EVAMainTexts,
        __4,
        SubTitle_UTIL,
        EXP_RATEMainTexts,
        Right,
        __1,
        MATKSubTexts,
        MATKBase,
        MATKIncrease,
        MATKBoost,
        MATKMainTexts,
        CDMGMainTexts,
        ASMainTexts,
        DEFSubTexts,
        DEFBase,
        DEFIncrease,
        DEFBoost,
        DEFMainTexts,
        M_RESMainTexts,
        RECOVMainTexts,
        __2,
        DROP_RATEMainTexts,
    }

    #endregion


    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
        {
            Managers.Game.StatusPoint++;
        }
#endif
    }


    // ── 스탯 분배 상태 ──
    // 순서: 0=STR, 1=VIT, 2=DEX, 3=MAG
    readonly int[] _statDeltas     = new int[4];  // 이번에 투자할 포인트
    readonly int[] _statBaseValues = new int[4];  // 팝업 열릴 때의 기준값
    static readonly Color s_ColGreen = new Color32(0x00, 0xFF, 0x00, 0xFF); // 증가 표시
    static readonly Color s_ColShake = new Color32(0xB7, 0x00, 0x00, 0xFF); // 포인트 부족

    bool _detailOpen = false;

    PlayerController _player;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        #endregion

        _frameRT = GetObject((int)GameObjects.CharacterPopup).gameObject.GetComponent<RectTransform>();

        // ── 배경/X 클릭 닫기 = 저장 안 하고 닫음 → 임시 분배 롤백 ──
        GetButton((int)Buttons.CharacterPopupBackground).gameObject.BindEvent(OnClickCloseRollback);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickCloseRollback);

        // ── 스탯 포인트 +/- ──
        //GetButton((int)Buttons.STRInPointBtn).gameObject.BindEvent(() => AddStat(0));
        //GetButton((int)Buttons.STRDePointBtn).gameObject.BindEvent(() => RemoveStat(0));
        //GetButton((int)Buttons.VITInPointBtn).gameObject.BindEvent(() => AddStat(1));
        //GetButton((int)Buttons.VITDePointBtn).gameObject.BindEvent(() => RemoveStat(1));
        //GetButton((int)Buttons.DEXInPointBtn).gameObject.BindEvent(() => AddStat(2));
        //GetButton((int)Buttons.DEXDePointBtn).gameObject.BindEvent(() => RemoveStat(2));
        //GetButton((int)Buttons.MAGInPointBtn).gameObject.BindEvent(() => AddStat(3));
        //GetButton((int)Buttons.MAGDePointBtn).gameObject.BindEvent(() => RemoveStat(3));

        // ── 상세/초기화/확인 ──
        GetButton((int)Buttons.StatDetailBtn).gameObject.BindEvent(OnClickStatDetail);
        GetButton((int)Buttons.StatResetBtn).gameObject.BindEvent(OnClickStatReset);
        GetButton((int)Buttons.StatResetBtn).gameObject.BindEvent(() => OnClickStatRestAndConfirmAnim(GetButton((int)Buttons.StatResetBtn).gameObject.transform));
        GetButton((int)Buttons.StatConfirmBtn).gameObject.BindEvent(OnClickStatConfirm);
        GetButton((int)Buttons.StatConfirmBtn).gameObject.BindEvent(() => OnClickStatRestAndConfirmAnim(GetButton((int)Buttons.StatConfirmBtn).gameObject.transform));

        // TextMat
        Util.SetTMPUnderlay(GetText((int)Texts.PointsText), new Color32(0xFF, 0x00, 0x00, 0xFF), 0.1f, -0.1f);
        Util.SetTMPUnderlay(GetText((int)Texts.PointsStatPointsTxt), new Color32(0xFF, 0x00, 0x00, 0xFF), 0.1f, -0.1f);
        Util.SetTMPUnderlay(GetText((int)Texts.STRStatNameTxt), new Color32(0xFF, 0xC0, 0x00, 0xFF), 0.2f, -0.2f);
        Util.SetTMPUnderlay(GetText((int)Texts.VITStatNameTxt), new Color32(0xFF, 0xC0, 0x00, 0xFF), 0.2f, -0.2f);
        Util.SetTMPUnderlay(GetText((int)Texts.DEXStatNameTxt), new Color32(0xFF, 0xC0, 0x00, 0xFF), 0.2f, -0.2f);
        Util.SetTMPUnderlay(GetText((int)Texts.MAGStatNameTxt), new Color32(0xFF, 0xC0, 0x00, 0xFF), 0.2f, -0.2f);

        //Material PointsTextMat = GetText((int)Texts.PointsText).fontMaterial;
        //PointsTextMat.EnableKeyword("UNDERLAY_ON");
        //PointsTextMat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color32(0xFF, 0x00, 0x00, 0xFF));
        //PointsTextMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.1f);
        //PointsTextMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.1f);
        //Material PointsStatPointsTxtMat = GetText((int)Texts.PointsStatPointsTxt).fontMaterial;
        //PointsStatPointsTxtMat.EnableKeyword("UNDERLAY_ON");
        //PointsStatPointsTxtMat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color32(0xFF, 0x00, 0x00, 0xFF));
        //PointsStatPointsTxtMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.1f);
        //PointsStatPointsTxtMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.1f);

        BindStatButtons();


        // ── 메인 스탯 그룹 클릭 (각각 따로 토글: 누른 쪽 프레임+세부스탯만 열림)
        GetButton((int)Buttons.PATKDetailGroup).gameObject.BindEvent(() => OnClickMainStatGroup((int)Buttons.PATKDetailGroup, (int)GameObjects.PATKSubTexts));
        GetButton((int)Buttons.HPDetailGroup).gameObject.BindEvent(()   => OnClickMainStatGroup((int)Buttons.HPDetailGroup, (int)GameObjects.HPSubTexts));
        GetButton((int)Buttons.MATKDetailGroup).gameObject.BindEvent(() => OnClickMainStatGroup((int)Buttons.MATKDetailGroup, (int)GameObjects.MATKSubTexts));
        GetButton((int)Buttons.DEFDetailGroup).gameObject.BindEvent(()  => OnClickMainStatGroup((int)Buttons.DEFDetailGroup, (int)GameObjects.DEFSubTexts));

        // ── 초기 상태 ──
        GetImage((int)Images.StatDetailBtnMask).gameObject.SetActive(false);
        GetObject((int)GameObjects.PATKSubTexts).gameObject.SetActive(false);
        GetObject((int)GameObjects.MATKSubTexts).gameObject.SetActive(false);
        GetObject((int)GameObjects.HPSubTexts).gameObject.SetActive(false);
        GetObject((int)GameObjects.DEFSubTexts).gameObject.SetActive(false);
        GetImage((int)Images.StatDetailFrame).gameObject.SetActive(false);

        SetLocalization();

        _player = Managers.Game.Player;
        if (_player != null)
        {
            CacheBaseValues();
            RefreshUI();
        }

        OpenAnimation();
        return true;
    }

    // ────────────────────────────────────────────────────
    //  슬라이드 16: 모든 정적 라벨 텍스트를 LocalizationData로 연동
    // ────────────────────────────────────────────────────
    void SetLocalization()
    {
        // 팝업 타이틀
        GetText((int)Texts.title).text = Util.Localize(LocalizeKey.CharPopupTitleText);

        // 4대 스탯 그룹 라벨 (첫 번째 TMP_Text = 라벨)
        SetGroupLabel(GetImage((int)Images.STRStatGroup).gameObject, Util.Localize(LocalizeKey.StrengthText));
        SetGroupLabel(GetImage((int)Images.VITStatGroup).gameObject, Util.Localize(LocalizeKey.VitialityText));
        SetGroupLabel(GetImage((int)Images.DEXStatGroup).gameObject, Util.Localize(LocalizeKey.AgilityText));
        SetGroupLabel(GetImage((int)Images.MAGStatGroup).gameObject, Util.Localize(LocalizeKey.MagicText));

        // 메인 스탯 버튼 그룹 라벨
        SetGroupLabel(GetButton((int)Buttons.PATKDetailGroup).gameObject, Util.Localize(LocalizeKey.PhysicalAttackText));
        SetGroupLabel(GetButton((int)Buttons.HPDetailGroup).gameObject,   Util.Localize(LocalizeKey.MaxHitPointText));
        SetGroupLabel(GetButton((int)Buttons.MATKDetailGroup).gameObject, Util.Localize(LocalizeKey.MagicalAttackText));
        SetGroupLabel(GetButton((int)Buttons.DEFDetailGroup).gameObject,  Util.Localize(LocalizeKey.DefenseText));

        // 섹션 헤더 (SubTitle GameObjects 첫 번째 TMP_Text)
        SetGroupLabel(GetObject((int)GameObjects.SubTitle_ATK).gameObject,  Util.Localize(LocalizeKey.CharPopupOffenseText));
        SetGroupLabel(GetObject((int)GameObjects.SubTitle_DEF).gameObject,  Util.Localize(LocalizeKey.CharPopupDefenseText));
        SetGroupLabel(GetObject((int)GameObjects.SubTitle_UTIL).gameObject, Util.Localize(LocalizeKey.CharPopupUtilityText));

        // 상세 스탯 그룹 라벨
        SetGroupLabel(GetImage((int)Images.CCDetailGroup).gameObject,       Util.Localize(LocalizeKey.CriticalText));
        SetGroupLabel(GetImage((int)Images.CDMGDetailGroup).gameObject,     Util.Localize(LocalizeKey.CriticalDamageText));
        SetGroupLabel(GetImage((int)Images.ACCDetailGroup).gameObject,      Util.Localize(LocalizeKey.AccuracyText));
        SetGroupLabel(GetImage((int)Images.ASDetailGroup).gameObject,       Util.Localize(LocalizeKey.AttackSpeedText));
        SetGroupLabel(GetImage((int)Images.P_RESDetailGroup).gameObject,     Util.Localize(LocalizeKey.PhysicalResistanceText));
        SetGroupLabel(GetImage((int)Images.M_RESDetailGroup).gameObject,     Util.Localize(LocalizeKey.MagicalResistanceText));
        SetGroupLabel(GetImage((int)Images.EVADetailGroup).gameObject,      Util.Localize(LocalizeKey.EvadeText));
        SetGroupLabel(GetImage((int)Images.RECOVDetailGroup).gameObject,    Util.Localize(LocalizeKey.RecoveryText));
        SetGroupLabel(GetImage((int)Images.EXP_RATEDetailGroup).gameObject,  Util.Localize(LocalizeKey.ExperienceGainText));
        SetGroupLabel(GetImage((int)Images.DROP_RATEDetailGroup).gameObject, Util.Localize(LocalizeKey.DropRateText));

        // 스탯포인트 / 엘릭서 라벨 (PointsandElixirBar 내 첫/두번째 TMP_Text가 라벨)
        TMP_Text[] barTexts = GetObject((int)GameObjects.PointsandElixirBar)
                              .gameObject.GetComponentsInChildren<TMP_Text>();
        // PointsandElixirBar는 [StatPointLabel, StatPointValue, ElixirLabel, ElixirValue] 순 가정
        if (barTexts.Length >= 1) barTexts[0].text = Util.Localize(LocalizeKey.CharPopupStatPointText);
        if (barTexts.Length >= 3) barTexts[2].text = Util.Localize(LocalizeKey.CharPopupElixirText);
    }

    // ────────────────────────────────────────────────────
    //  스탯 포인트 분배
    // ────────────────────────────────────────────────────
    void CacheBaseValues()
    {
        _statBaseValues[0] = (int)_player.PhysicalAtk;
        _statBaseValues[1] = (int)_player.MaxHp;
        _statBaseValues[2] = (int)_player.Accuracy;
        _statBaseValues[3] = (int)_player.MagicAtk;
        for (int i = 0; i < 4; i++) _statDeltas[i] = 0;
    }

    bool AddStat(int idx)
    {
        if (Managers.Game.StatusPoint <= 0) return false;
        Managers.Game.StatusPoint--;
        _statDeltas[idx]++;
        //PlayStatBtnAnim(idx, true);
        RefreshUI();
        return true;
    }

    bool RemoveStat(int idx)
    {
        if (_statDeltas[idx] <= 0) return false;
        Managers.Game.StatusPoint++;
        _statDeltas[idx]--;
        //PlayStatBtnAnim(idx, false);
        RefreshUI();
        return true;
    }

    void SelectArrowStatButtonAnim(Transform transform)
    {
        // anim
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(-0.25f, 0.15f, 0f), 0.3f, 10, 1f)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);
    }

    // 슬라이드 15: DOTween PunchScale Duration 0.3, Ease OutQuad,
    //              TO: x-0.25, y 0.15, z 0, Vibrato 10, Elasticity 1
    void PlayDetailOpenAnim(Transform t)
    {
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOPunchScale(new Vector3(-0.25f, 0.15f, 0f), 0.3f, 10, 1f)
         .SetEase(Ease.OutQuad)
         .SetUpdate(true);
    }

    void BindStatAddButton(Buttons buttonType, int statIdx)
    {
        GameObject btnObj = GetButton((int)buttonType).gameObject;

        btnObj.BindEvent(() =>
        {
            if (!AddStat(statIdx))
            {
                PlayNoPointFeedback(btnObj.transform, statIdx);
                return;
            }

            SelectArrowStatButtonAnim(btnObj.transform);
        });
    }

    void BindStatRemoveButton(Buttons buttonType, int statIdx)
    {
        GameObject btnObj = GetButton((int)buttonType).gameObject;

        btnObj.BindEvent(() =>
        {
            if (!RemoveStat(statIdx))
                return;

            SelectArrowStatButtonAnim(btnObj.transform);
        });
    }

    void BindStatButtons()
    {
        BindStatAddButton(Buttons.STRInPointBtn, 0);
        BindStatAddButton(Buttons.VITInPointBtn, 1);
        BindStatAddButton(Buttons.DEXInPointBtn, 2);
        BindStatAddButton(Buttons.MAGInPointBtn, 3);

        BindStatRemoveButton(Buttons.STRDePointBtn, 0);
        BindStatRemoveButton(Buttons.VITDePointBtn, 1);
        BindStatRemoveButton(Buttons.DEXDePointBtn, 2);
        BindStatRemoveButton(Buttons.MAGDePointBtn, 3);
    }

    // 슬라이드 15: Scale TO:0.9, Duration 0.05, Ease OutQuad, Loops 2, Yoyo
    void PlayStatBtnAnim(int idx, bool isAdd)
    {
        int btnEnum = isAdd
            ? (int)Buttons.STRInPointBtn + idx * 2
            : (int)Buttons.STRDePointBtn + idx * 2;
        Transform t = GetButton(btnEnum).transform;
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOScale(0.9f, 0.05f)
         .SetEase(Ease.OutQuad)
         .SetLoops(2, LoopType.Yoyo)
         .SetUpdate(true);
    }

    // ────────────────────────────────────────────────────
    //  상세 스탯 패널 토글 (StatDetailFrame)
    // ────────────────────────────────────────────────────
    void OnClickStatDetail()
    {
        _detailOpen = !_detailOpen;
        GameObject frame = GetImage((int)Images.StatDetailFrame).gameObject;
        frame.SetActive(_detailOpen);

        if (_detailOpen)
        {
            PlayDetailOpenAnim(frame.transform);
            GetImage((int)Images.StatDetailBtnMask).gameObject.SetActive(true);
            GetButton((int)Buttons.StatDetailBtn).GetComponent<Image>().color = new Color32(0xFF, 0xD0, 0x00, 0xFF);
            GetImage((int)Images.StatDetailBtnMask).color = new Color32(0xFF, 0xBA, 0x00, 0xA0);
            GetImage((int)Images.StatDetailBtnGrow).color = new Color32(0x00, 0x00, 0x00, 0xFF);
        }
        else
        {
            GetImage((int)Images.StatDetailBtnMask).gameObject.SetActive(false);
            GetButton((int)Buttons.StatDetailBtn).GetComponent<Image>().color = new Color32(0xFF, 0xFF, 0xFF, 0xB4);
            GetImage((int)Images.StatDetailBtnMask).color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            GetImage((int)Images.StatDetailBtnGrow).color = new Color32(0x00, 0x00, 0x00, 0xFF);
        }
    }

    // ────────────────────────────────────────────────────
    //  메인 스탯 그룹 클릭
    // ────────────────────────────────────────────────────

    void OnClickMainStatGroup(int frameIdx, int TextBoxIdx)
    {
        bool open = !GetObject(TextBoxIdx).activeSelf;   // 현재 상태 토글
        SetMainStatGroup(frameIdx, TextBoxIdx, open);
    }

    // 누른 쪽 프레임 높이(16↔46)만 확장/축소 + 해당 세부스탯만 표시.
    void SetMainStatGroup(int frameIdx, int TextBoxIdx, bool open)
    {
        GetObject(TextBoxIdx).SetActive(open);
        RectTransform rt = GetButton(frameIdx).GetComponent<RectTransform>();
        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOSizeDelta(new Vector2(60f, open ? 46f : 16f), 0.3f).SetEase(Ease.Linear);
    }

    // ────────────────────────────────────────────────────
    //  초기화 / 확인
    // ────────────────────────────────────────────────────
    // StatResetBtn = 롤백 버튼: 임시 투자분(델타)을 취소하고 포인트를 환불.
    void OnClickStatReset() => RollbackDeltas();

    // 저장 안 한 임시 분배를 롤백 — 투자한 포인트를 환불하고 델타 0으로.
    void RollbackDeltas()
    {
        for (int i = 0; i < 4; i++)
        {
            Managers.Game.StatusPoint += _statDeltas[i];
            _statDeltas[i] = 0;
        }
        RefreshUI();
    }

    // 저장(확정) 없이 닫을 때 — 롤백 후 팝업 닫기.
    void OnClickCloseRollback()
    {
        RollbackDeltas();
        OnClickBG();
    }

    // StatConfirmBtn = 스탯 적용 + 저장 전용. 팝업은 닫지 않는다.
    void OnClickStatConfirm()
    {
        if (_player == null) return;
        bool nothingToApply = true;
        for (int i = 0; i < 4; i++) if (_statDeltas[i] != 0) nothingToApply = false;
        if (nothingToApply) return;

        // 분배 델타를 GameData에 누적(영구 저장 — 세이브/로드는 GameManager가 관리)
        int[] inv = Managers.Game.StatInvest;
        for (int i = 0; i < 4; i++) { inv[i] += _statDeltas[i]; _statDeltas[i] = 0; }
        Managers.Game.SaveGame();

        // 실제 캐릭터에 적용 + PlayerBar 갱신 (RefreshStats가 OnHpChanged 발화 → HP 바/텍스트 즉시 갱신)
        _player.RefreshStats();
        CacheBaseValues();   // 적용 후 새 기준값 재캐싱
        RefreshUI();
        // 팝업은 닫지 않음 (사용자 요구)
    }

    void OnClickStatRestAndConfirmAnim(Transform t)
    {
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOScale(0.9f, 0.05f)
         .SetEase(Ease.OutQuad)
         .SetLoops(2, LoopType.Yoyo)
         .SetUpdate(true);
    }

    // ────────────────────────────────────────────────────
    //  UI 갱신
    // ────────────────────────────────────────────────────
    void RefreshUI()
    {
        if (_player == null) return;

        GetText((int)Texts.PointsStatPointsTxt).text = Managers.Game.StatusPoint.ToString();
        GetText((int)Texts.ElixirPointsTxt).text     = Managers.Game.Elixir.ToString();

        // ── 미리보기용 기본값 (스탯 포인트 투자분 반영) ──
        float patkBase = _player.PhysicalAtk + _statDeltas[0] * 2f;
        float hpBase   = _player.MaxHp       + _statDeltas[1] * 20f;
        float matkBase = _player.MagicAtk    + _statDeltas[3] * 2f;
        float defBase  = _player.Def;

        // ── 최종 = 기본 * (1 + 증가율/100) * 증폭률 ──
        float patkFinal = patkBase * (1f + _player.PhysicalAtkRate / 100f) * _player.PhysicalAtkAmp;
        float hpFinal   = hpBase   * (1f + _player.MaxHpRate        / 100f) * _player.MaxHpAmp;
        float matkFinal = matkBase * (1f + _player.MagicAtkRate     / 100f) * _player.MagicAtkAmp;
        float defFinal  = defBase  * (1f + _player.DefRate           / 100f) * _player.DefAmp;

        // ── 버튼 (접힌 상태) → 최종 능력치 ──
        SetGroupValue(GetButton((int)Buttons.PATKDetailGroup).gameObject, Util.FormatStat(patkFinal));
        SetGroupValue(GetButton((int)Buttons.HPDetailGroup).gameObject,   Util.FormatStat(hpFinal));
        SetGroupValue(GetButton((int)Buttons.MATKDetailGroup).gameObject, Util.FormatStat(matkFinal));
        SetGroupValue(GetButton((int)Buttons.DEFDetailGroup).gameObject,  Util.FormatStat(defFinal));

        // ── 확장 서브텍스트 → 기본 / +증가율% / x증폭률 / 최종 ──
        RefreshStatSubTexts(
            Texts.PATKBaseStatNum, Texts.PATKIncreaseStatNum, Texts.PATKBoostStatNum, Texts.PATKStatNum,
            patkBase, _player.PhysicalAtkRate, _player.PhysicalAtkAmp, patkFinal);

        RefreshStatSubTexts(
            Texts.HPBaseStatNum, Texts.HPIncreaseStatNum, Texts.HPBoostStatNum, Texts.HPStatNum,
            hpBase, _player.MaxHpRate, _player.MaxHpAmp, hpFinal);

        RefreshStatSubTexts(
            Texts.MATKBaseStatNum, Texts.MATKIncreaseStatNum, Texts.MATKBoostStatNum, Texts.MATKStatNum,
            matkBase, _player.MagicAtkRate, _player.MagicAtkAmp, matkFinal);

        RefreshStatSubTexts(
            Texts.DEFBaseStatNum, Texts.DEFIncreaseStatNum, Texts.DEFBoostStatNum, Texts.DEFStatNum,
            defBase, _player.DefRate, _player.DefAmp, defFinal);

        // 능력 점수 올림에 따라서 텍스 변함.
        if (_statDeltas[0] != 0)
        {
            GetText((int)Texts.STRAmountTxt).color = s_ColGreen;
            GetText((int)Texts.PATKBaseStatNum).color = s_ColGreen;
            GetText((int)Texts.PATKStatNum).color = s_ColGreen;   
        }
        else
        {
            GetText((int)Texts.STRAmountTxt).color = Color.white;
            GetText((int)Texts.PATKBaseStatNum).color = Color.white;
            GetText((int)Texts.PATKStatNum).color = Color.white;
        }

        if (_statDeltas[1] != 0)
        {
            GetText((int)Texts.VITAmountTxt).color = s_ColGreen;
            GetText((int)Texts.HPBaseStatNum).color = s_ColGreen;
            GetText((int)Texts.HPStatNum).color = s_ColGreen;
        }
        else
        {
            GetText((int)Texts.VITAmountTxt).color = Color.white;
            GetText((int)Texts.HPBaseStatNum).color = Color.white;
            GetText((int)Texts.HPStatNum).color = Color.white;
        }

        if (_statDeltas[2] != 0)
        {
            GetText((int)Texts.DEXAmountTxt).color = s_ColGreen;
        }
        else
        {
            GetText((int)Texts.DEXAmountTxt).color = Color.white;
        }

        if (_statDeltas[3] != 0)
        {
            GetText((int)Texts.MAGAmountTxt).color = s_ColGreen;
            GetText((int)Texts.MATKBaseStatNum).color = s_ColGreen;
            GetText((int)Texts.MATKStatNum).color = s_ColGreen;
        }
        else
        {
            GetText((int)Texts.MAGAmountTxt).color = Color.white;
            GetText((int)Texts.MATKBaseStatNum).color = Color.white;
            GetText((int)Texts.MATKStatNum).color = Color.white;
        }

        // ── STR/VIT/DEX/MAG 투자 포인트 ──
        GetText((int)Texts.STRInvPoint).text = _statDeltas[0].ToString();
        GetText((int)Texts.VITInvPoint).text = _statDeltas[1].ToString();
        GetText((int)Texts.DEXInvPoint).text = _statDeltas[2].ToString();
        GetText((int)Texts.MAGInvPoint).text = _statDeltas[3].ToString();

        // ── STR/VIT/DEX/MAG 현재 합산값 ──
        GetText((int)Texts.STRAmountTxt).text = Util.FormatStat(_player.PhysicalAtk + _statDeltas[0] * 2f);
        GetText((int)Texts.VITAmountTxt).text = Util.FormatStat(_player.MaxHp       + _statDeltas[1] * 20f);
        GetText((int)Texts.DEXAmountTxt).text = Util.FormatStat(_player.Accuracy    + _statDeltas[2] * 1f);
        GetText((int)Texts.MAGAmountTxt).text = Util.FormatStat(_player.MagicAtk    + _statDeltas[3] * 2f);

        // ── 상세 스탯 ──
        SetGroupValue(GetImage((int)Images.CCDetailGroup).gameObject,        Util.FormatStat(_player.CritRate,       2, percent: true));
        SetGroupValue(GetImage((int)Images.CDMGDetailGroup).gameObject,      Util.FormatStat(_player.CritDamage,     0, percent: true));
        SetGroupValue(GetImage((int)Images.ACCDetailGroup).gameObject,       Util.FormatStat(_player.Accuracy,       1, percent: true));
        SetGroupValue(GetImage((int)Images.ASDetailGroup).gameObject,        Util.FormatStat(_player.AtkSpeed,       0, percent: true));
        SetGroupValue(GetImage((int)Images.P_RESDetailGroup).gameObject,     Util.FormatStat(_player.PhysicalResist, 2, percent: true));
        SetGroupValue(GetImage((int)Images.M_RESDetailGroup).gameObject,     Util.FormatStat(_player.MagicResist,    2, percent: true));
        SetGroupValue(GetImage((int)Images.EVADetailGroup).gameObject,       Util.FormatStat(_player.Evasion,        2, percent: true));
        SetGroupValue(GetImage((int)Images.RECOVDetailGroup).gameObject,     Util.FormatStat(_player.RecoveryRate,   1, percent: true));
        SetGroupValue(GetImage((int)Images.EXP_RATEDetailGroup).gameObject,  Util.FormatStat(_player.GrowthRate,     0, percent: true));
        SetGroupValue(GetImage((int)Images.DROP_RATEDetailGroup).gameObject, Util.FormatStat(_player.DropRate,       0, percent: true));
    }

    // 기본 / +증가율% / x증폭률 / 최종을 각 Texts enum에 채움
    void RefreshStatSubTexts(Texts baseNum, Texts rateNum, Texts ampNum, Texts finalNum,
                             float baseVal, float rate, float amp, float finalVal)
    {
        GetText((int)baseNum).text  = Util.FormatStat(baseVal);
        GetText((int)rateNum).text  = "+" + rate.ToString("F0") + "%";
        GetText((int)ampNum).text   = "x" + amp.ToString("G4");
        GetText((int)finalNum).text = Util.FormatStat(finalVal);
    }

    public void SetInfo(PlayerController player)
    {
        _player = player;
        CacheBaseValues();
        RefreshUI();
    }

    // 그룹 내 마지막 TMP_Text = 값 표시
    void SetGroupValue(GameObject group, string value)
    {
        TMP_Text[] texts = group.GetComponentsInChildren<TMP_Text>();
        if (texts.Length > 0)
            texts[texts.Length - 1].text = value;
    }

    // 그룹 내 첫 번째 TMP_Text = 라벨 (슬라이드 16: LocalizationData 연동)
    void SetGroupLabel(GameObject group, string label)
    {
        TMP_Text[] texts = group.GetComponentsInChildren<TMP_Text>();
        if (texts.Length > 0)
            texts[0].text = label;
    }

    // 능력점수 없을 때 > Btn — B70000 0.3s 플래시 + Shake
    void PlayNoPointFeedback(Transform btnTransform, int statIdx)
    {
        TMP_Text valueTxt = GetText((int)Texts.PointsStatPointsTxt);
        if (valueTxt == null) return;

        valueTxt.DOKill();
        valueTxt.transform.DOKill();

        //해당 그룹 값 텍스트 B70000 플래시
        valueTxt.color = s_ColShake;

        valueTxt.DOColor(Color.white, 0.15f)
            .SetDelay(0.3f)
            .SetUpdate(true);

        // 버튼 Shake
        valueTxt.transform.DOShakePosition(0.3f, new Vector3(0.8f, 0f, 0f), 50, 90)
                    .SetUpdate(true);
    }
}
