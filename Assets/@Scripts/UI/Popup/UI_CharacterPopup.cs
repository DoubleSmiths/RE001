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
        Mask,
        StatDetailFrame,
        CCDetailGroup,
        ACCDetailGroup,
        PRESDetailGroup,
        EVADetailGroup,
        EXPRATEDetailGroup,
        CDMGDetailGroup,
        ASDetailGroup,
        MRESDetailGroup,
        RECOVDetailGroup,
        DROPRATEDetailGroup,
        StatDetailFrame_1,
        STRDetailGroup1,
        DEXDetailGroup1,
        Xbutton,
        StatResetBtnGrow,
        StatDetailBtnGrow,
        StatConfirmBtnGrow,
    }

    enum Texts
    {
        title,
        StatPointsTxt,
        ElixirPointsTxt,
        STRAmountTxt,
        STRInvPoint,
        VITAmountTxt,
        VITInvPoint,
        DEXAmountTxt,
        DEXInvPoint,
        MAGAmountTxt,
        MAGInvPoint,
        Text,
        Text2,
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
        InFrame,
        CharacterPopup,
        Contents,
        PointsandElixirBar,
        TopLayout,
        BtmLayout,
        Left,
        SubTitleATK,
        _3,
        SubTitleDEF,
        _4,
        SubTitleUTIL,
        Right,
        _1,
        _2,
        Layout_3,
    }

    #endregion

    // ── 스탯 분배 상태 ──
    // 순서: 0=STR, 1=VIT, 2=DEX, 3=MAG
    readonly int[] _statDeltas     = new int[4];  // 이번에 투자할 포인트
    readonly int[] _statBaseValues = new int[4];  // 팝업 열릴 때의 기준값

    bool _detailOpen = false;
    int  _selectedMain = -1; // PATKDetailGroup 등 선택된 메인스탯 인덱스

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

        // ── 배경 클릭 닫기 ──
        GetButton((int)Buttons.CharacterPopupBackground).gameObject.BindEvent(() => OnClickBG());

        // ── 스탯 포인트 +/- ──
        GetButton((int)Buttons.STRInPointBtn).gameObject.BindEvent(() => AddStat(0));
        GetButton((int)Buttons.STRDePointBtn).gameObject.BindEvent(() => RemoveStat(0));
        GetButton((int)Buttons.VITInPointBtn).gameObject.BindEvent(() => AddStat(1));
        GetButton((int)Buttons.VITDePointBtn).gameObject.BindEvent(() => RemoveStat(1));
        GetButton((int)Buttons.DEXInPointBtn).gameObject.BindEvent(() => AddStat(2));
        GetButton((int)Buttons.DEXDePointBtn).gameObject.BindEvent(() => RemoveStat(2));
        GetButton((int)Buttons.MAGInPointBtn).gameObject.BindEvent(() => AddStat(3));
        GetButton((int)Buttons.MAGDePointBtn).gameObject.BindEvent(() => RemoveStat(3));

        // ── 상세/초기화/확인 ──
        GetButton((int)Buttons.StatDetailBtn).gameObject.BindEvent(OnClickStatDetail);
        GetButton((int)Buttons.StatResetBtn).gameObject.BindEvent(OnClickStatReset);
        GetButton((int)Buttons.StatConfirmBtn).gameObject.BindEvent(OnClickStatConfirm);

        // ── 메인 스탯 그룹 클릭 → StatDetailFrame_1 / Layout_3 ──
        GetButton((int)Buttons.PATKDetailGroup).gameObject.BindEvent(() => OnClickMainStatGroup(0));
        GetButton((int)Buttons.HPDetailGroup).gameObject.BindEvent(()   => OnClickMainStatGroup(1));
        GetButton((int)Buttons.MATKDetailGroup).gameObject.BindEvent(() => OnClickMainStatGroup(2));
        GetButton((int)Buttons.DEFDetailGroup).gameObject.BindEvent(()  => OnClickMainStatGroup(3));

        // ── 초기 상태 ──
        GetImage((int)Images.StatDetailFrame).gameObject.SetActive(false);
        GetImage((int)Images.StatDetailFrame_1).gameObject.SetActive(false);
        GetObject((int)GameObjects.Layout_3).gameObject.SetActive(false);

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
        SetGroupLabel(GetObject((int)GameObjects.SubTitleATK).gameObject,  Util.Localize(LocalizeKey.CharPopupOffenseText));
        SetGroupLabel(GetObject((int)GameObjects.SubTitleDEF).gameObject,  Util.Localize(LocalizeKey.CharPopupDefenseText));
        SetGroupLabel(GetObject((int)GameObjects.SubTitleUTIL).gameObject, Util.Localize(LocalizeKey.CharPopupUtilityText));

        // 상세 스탯 그룹 라벨
        SetGroupLabel(GetImage((int)Images.CCDetailGroup).gameObject,       Util.Localize(LocalizeKey.CriticalText));
        SetGroupLabel(GetImage((int)Images.CDMGDetailGroup).gameObject,     Util.Localize(LocalizeKey.CriticalDamageText));
        SetGroupLabel(GetImage((int)Images.ACCDetailGroup).gameObject,      Util.Localize(LocalizeKey.AccuracyText));
        SetGroupLabel(GetImage((int)Images.ASDetailGroup).gameObject,       Util.Localize(LocalizeKey.AttackSpeedText));
        SetGroupLabel(GetImage((int)Images.PRESDetailGroup).gameObject,     Util.Localize(LocalizeKey.PhysicalResistanceText));
        SetGroupLabel(GetImage((int)Images.MRESDetailGroup).gameObject,     Util.Localize(LocalizeKey.MagicalResistanceText));
        SetGroupLabel(GetImage((int)Images.EVADetailGroup).gameObject,      Util.Localize(LocalizeKey.EvadeText));
        SetGroupLabel(GetImage((int)Images.RECOVDetailGroup).gameObject,    Util.Localize(LocalizeKey.RecoveryText));
        SetGroupLabel(GetImage((int)Images.EXPRATEDetailGroup).gameObject,  Util.Localize(LocalizeKey.ExperienceGainText));
        SetGroupLabel(GetImage((int)Images.DROPRATEDetailGroup).gameObject, Util.Localize(LocalizeKey.DropRateText));

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

    void AddStat(int idx)
    {
        if (Managers.Game.StatusPoint <= 0) return;
        Managers.Game.StatusPoint--;
        _statDeltas[idx]++;
        PlayStatBtnAnim(idx, true);
        RefreshUI();
    }

    void RemoveStat(int idx)
    {
        if (_statDeltas[idx] <= 0) return;
        Managers.Game.StatusPoint++;
        _statDeltas[idx]--;
        PlayStatBtnAnim(idx, false);
        RefreshUI();
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
            PlayDetailOpenAnim(frame.transform);
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

    // ────────────────────────────────────────────────────
    //  메인 스탯 그룹 클릭 → StatDetailFrame_1 / Layout_3
    // ────────────────────────────────────────────────────
    void OnClickMainStatGroup(int idx)
    {
        GameObject frame1   = GetImage((int)Images.StatDetailFrame_1).gameObject;
        GameObject layout3  = GetObject((int)GameObjects.Layout_3).gameObject;

        bool alreadySelected = (_selectedMain == idx && frame1.activeSelf);
        if (alreadySelected)
        {
            frame1.SetActive(false);
            layout3.SetActive(false);
            _selectedMain = -1;
            return;
        }

        _selectedMain = idx;
        frame1.SetActive(true);
        layout3.SetActive(true);
        PlayDetailOpenAnim(frame1.transform);
    }

    // ────────────────────────────────────────────────────
    //  초기화 / 확인
    // ────────────────────────────────────────────────────
    void OnClickStatReset()
    {
        // 투자한 포인트를 돌려줌
        for (int i = 0; i < 4; i++)
            Managers.Game.StatusPoint += _statDeltas[i];
        for (int i = 0; i < 4; i++) _statDeltas[i] = 0;
        RefreshUI();
    }

    void OnClickStatConfirm()
    {
        if (_player == null) return;

        // 델타 적용 (1포인트 = STR:2공격, VIT:20HP, DEX:1명중, MAG:2마법공격)
        _player.PhysicalAtk += _statDeltas[0] * 2f;
        _player.MaxHp       += _statDeltas[1] * 20f;
        _player.Accuracy    += _statDeltas[2] * 1f;
        _player.MagicAtk   += _statDeltas[3] * 2f;

        Managers.Game.SaveGame();
        ClosePopupUI();
    }

    // ────────────────────────────────────────────────────
    //  UI 갱신
    // ────────────────────────────────────────────────────
    void RefreshUI()
    {
        if (_player == null) return;

        GetText((int)Texts.StatPointsTxt).text  = Managers.Game.StatusPoint.ToString();
        GetText((int)Texts.ElixirPointsTxt).text = Managers.Game.Elixir.ToString();

        // 4대 기본 스탯 그룹
        SetGroupValue(GetButton((int)Buttons.PATKDetailGroup).gameObject, Util.FormatStat(_player.PhysicalAtk + _statDeltas[0] * 2f));
        SetGroupValue(GetButton((int)Buttons.HPDetailGroup).gameObject,   Util.FormatStat(_player.MaxHp       + _statDeltas[1] * 20f));
        SetGroupValue(GetButton((int)Buttons.MATKDetailGroup).gameObject, Util.FormatStat(_player.MagicAtk    + _statDeltas[3] * 2f));
        SetGroupValue(GetButton((int)Buttons.DEFDetailGroup).gameObject,  Util.FormatStat(_player.Def));

        // STR/VIT/DEX/MAG 투자 포인트 표시
        GetText((int)Texts.STRInvPoint).text = _statDeltas[0].ToString();
        GetText((int)Texts.VITInvPoint).text = _statDeltas[1].ToString();
        GetText((int)Texts.DEXInvPoint).text = _statDeltas[2].ToString();
        GetText((int)Texts.MAGInvPoint).text = _statDeltas[3].ToString();

        // STR/VIT/DEX/MAG 현재 합산값
        GetText((int)Texts.STRAmountTxt).text = Util.FormatStat(_player.PhysicalAtk + _statDeltas[0] * 2f);
        GetText((int)Texts.VITAmountTxt).text = Util.FormatStat(_player.MaxHp       + _statDeltas[1] * 20f);
        GetText((int)Texts.DEXAmountTxt).text = Util.FormatStat(_player.Accuracy    + _statDeltas[2] * 1f);
        GetText((int)Texts.MAGAmountTxt).text = Util.FormatStat(_player.MagicAtk    + _statDeltas[3] * 2f);

        // 상세 스탯
        SetGroupValue(GetImage((int)Images.CCDetailGroup).gameObject,       Util.FormatStat(_player.CritRate,      2, percent: true));
        SetGroupValue(GetImage((int)Images.CDMGDetailGroup).gameObject,     Util.FormatStat(_player.CritDamage,    0, percent: true));
        SetGroupValue(GetImage((int)Images.ACCDetailGroup).gameObject,      Util.FormatStat(_player.Accuracy,      1, percent: true));
        SetGroupValue(GetImage((int)Images.ASDetailGroup).gameObject,       Util.FormatStat(_player.AtkSpeed,      0, percent: true));
        SetGroupValue(GetImage((int)Images.PRESDetailGroup).gameObject,     Util.FormatStat(_player.PhysicalResist,2, percent: true));
        SetGroupValue(GetImage((int)Images.MRESDetailGroup).gameObject,     Util.FormatStat(_player.MagicResist,   2, percent: true));
        SetGroupValue(GetImage((int)Images.EVADetailGroup).gameObject,      Util.FormatStat(_player.Evasion,       2, percent: true));
        SetGroupValue(GetImage((int)Images.RECOVDetailGroup).gameObject,    Util.FormatStat(_player.RecoveryRate,  1, percent: true));
        SetGroupValue(GetImage((int)Images.EXPRATEDetailGroup).gameObject,  Util.FormatStat(_player.GrowthRate,    0, percent: true));
        SetGroupValue(GetImage((int)Images.DROPRATEDetailGroup).gameObject, Util.FormatStat(_player.DropRate,      0, percent: true));
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
}
