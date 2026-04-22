using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data;
using static Define;
using Sequence = DG.Tweening.Sequence;

/// <summary>
/// 슬라이드 36~40: 물약 상세정보 팝업.
/// S36: 호출 진입 애님 / S37: 기본 데이터 바인딩 / S38: 설명 ↔ 필요재료 토글 /
/// S39: 다음 단계 정보 미리보기 + 하이라이트 / S40: 제작 버튼 애님 + 갱신.
/// </summary>
public class UI_PotionDetailPopup : UI_Popup
{
    public enum PotionKind { HP, Speed }

    /// <summary>HP/SPD 양쪽 데이터를 공통 뷰로 정규화. 표시 분기용.</summary>
    class PotionView
    {
        public PotionKind Kind;
        public int DataId;
        public string Name;
        public EquipGrade Grade;
        public string Description;
        public string SpriteName;
        public int Step;
        public int Cooldown;
        public List<int> NeededItemIDs;
        public List<int> NeededItemAmounts;

        // HP 전용
        public int FlatRecov;
        public int MaxHPRecov;

        // SPD 전용
        public float AtkSpeedBonus;
        public int Duration;
    }

    #region Enum

    enum Images
    {
        Img_PotionDetailPopupshineef_01,
        Image_5,
        FrameImg,
        WhereToFindTxtFrame_1,
        GlowMask_1,
        Img_GlowMask_1_07,
        PotionDetailPopupPotionImg,
        Img_PotionDetailPopupTitle_01,
        Mask,
        Img_Mask_03,
        Glow,
        Img_Mask_04,
        Guage,
        Img_Guage_Step1,        // 좌 — Step >= 1 일 때 표시
        Img_Guage_Step2,        // 중 — Step >= 2 일 때 표시
        Img_Guage_Step3,        // 우 — Step >= 3 일 때 표시
        background,
        Img_Guage_NextLvl_BG,   // Guage_NextLvl 전체 배경 (stretch)
        Img_Guage_NextLvl_Step1,
        Img_Guage_NextLvl_Step2,
        Img_Guage_NextLvl_Step3,
        Img_SwitchPanelBtn_01,
        __1,
        CantCraftBtn,
        Img_CantCraftBtn_05,
        Mask_CantCraftBtn_10,
        Img_Item_02,
        Img_Item_04,
        Img_CraftBtn_04,
        Mask_CraftBtn_02,
        Img_Item_01,
        Img_Item_06,
        Img_NextLvlBtn_02,
        Mask_NextLvlBtn_04,
        Img_Item_08,
        Img_Item_05,
        Img_BackBtn_03,
        Mask_BackBtn_03,
        Img_Item_07,
        Img_Item_03,
        Img_ReqItemBox1_1_08,
        Mask_ReqItemBox1_1_05,
        Img_GlowMask_14,
        Img_ReqItemBox1_1_04,
        Img_ReqItemBox1_1_06,
        Img_ReqItemBox1_2_04,
        Mask_ReqItemBox1_2_02,
        Img_GlowMask_11,
        Img_ReqItemBox1_2_03,
        Img_ReqItemBox1_2_05,
        Img_ReqItemBox1_3_03,
        Mask_ReqItemBox1_3_01,
        Img_GlowMask_13,
        Img_ReqItemBox1_3_05,
        Img_ReqItemBox1_3_02,
        Img_ReqItemBox1_4_05,
        Mask_ReqItemBox1_4_03,
        Img_GlowMask_12,
        Img_ReqItemBox1_4_02,
        Img_ReqItemBox1_4_01,
        Img_ReqItemBox1_5_02,
        Mask_ReqItemBox1_5_04,
        Img_GlowMask_15,
        Img_ReqItemBox1_5_06,
        Img_ReqItemBox1_5_03,
        Xbutton,
    }

    enum Texts
    {
        WhereToFindTxt,
        PotionDetailPopupPotionName,
        RankText,
        NextLevel_RankText,
        FlatHealAmountTxt_NextLvl,
        FlatHealAmountTxt,
        P_HealAmountTxt_NextLvl,
        P_HealAmountTxt,
        TotalHealAmountTxt_NextLvl,
        TotalHealAmountTxt,
        SPDAmountTxt_NextLvl,
        SPDAmountTxt,
        DURIAmountTxt_NextLvl,
        DURIAmountTxt,
        PotionCooltimeTxt_NextLvl,
        PotionCooltimeTxt,
        SPDPotionUnlockTxtKOR,
        SPDPotionUnlockTxtENG,
        DescText,
        Txt_SwitchPanelBtn_01,
        Txt_ReqItemGroup_02,
        MaxLvlTxt,
        Txt_CantCraftBtn_08,
        Txt_CraftBtn_06,
        Txt_NextLvlBtn_05,
        Txt_BackBtn_09,
        Txt_ReqItemBox1_1_05,
        Txt_ReqItemBox1_2_03,
        Txt_ReqItemBox1_3_01,
        Txt_ReqItemBox1_4_04,
        Txt_ReqItemBox1_5_02,
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        PotionDetailPopupBackground,
        Btn_FrameImg_01,
        SwitchPanelBtn,
        CraftBtn,
        NextLvlBtn,
        BackBtn,
        ReqItemBox1_1,
        ReqItemBox1_2,
        ReqItemBox1_3,
        ReqItemBox1_4,
        ReqItemBox1_5,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        PotionDetailPopup,
        SpeedPotionNewBtnHelper,
        PotionDetailPopupFrame,
        PotionDetailPopupshineeffect,
        PotionDetailPopupTitle,
        Guage_NextLvl,
        HPPotion_Texts,
        SPDPotion_Texts,
        SPDPotionUnlockTxtGroup,
        ReqItemGroup,
        ReqItemList,
    }

    #endregion

    // 슬라이드 37 스펙: From y = -130
    const float FRAME_FROM_Y = -130f;

    // Cached transforms
    Transform _frame;
    RectTransform _myFrameRT;
    Image _bg;
    Image _potionImg;
    TMP_Text _nameText;
    TMP_Text _rankText, _nextRankText;
    TMP_Text _flatHeal, _flatHealN;
    TMP_Text _pHeal, _pHealN;
    TMP_Text _totalHeal, _totalHealN;
    TMP_Text _cooldown, _cooldownN;
    TMP_Text _spdAmount, _spdAmountN;
    TMP_Text _duration, _durationN;
    TMP_Text _descText;
    TMP_Text _maxLvlTxt;
    TMP_Text _switchBtnLabel;
    TMP_Text _nextLvlBtnLabel;
    GameObject _hpTextsGroup;
    GameObject _spdTextsGroup;
    GameObject _reqItemGroup;
    GameObject _reqItemList;
    GameObject _reqItemTitle;
    GameObject _reqDecorLine;        // Decor_description_line (긴 막대)
    GameObject _switchPanelBtn;
    GameObject _cantCraftBtn;
    GameObject _craftBtn;
    GameObject _nextLvlBtn;
    GameObject _backBtn;
    RectTransform[] _reqItemBoxes;   // ReqItemBox1_1 ~ 5
    Image[]         _reqItemIcons;   // 각 박스의 아이템 아이콘 Image (캐시)

    // Gauge 세그먼트 (단계 표시): left-to-right 정렬
    Image[]       _guageSegments;          // Guage/Img_Guage_02, _04, _01
    Color[]       _guageSegmentDefaultColor;
    Image[]       _guageNextLvlSegments;   // Guage_NextLvl/Img_Guage_NextLvl_01~04
    GameObject    _guageNextLvlGo;

    // S39 등급 변경 Scale 애님 대상 (OutBack 0.3s From 3)
    RectTransform _potionImgRT;
    static readonly Color GUAGE_PREVIEW_COLOR = new Color(0f, 1f, 0f, 1f); // #00FF00

    Vector2 _frameOrigin;

    // 현재 표시 중인 물약 DataId (HPPotionDic/SpeedPotionDic 기준). -1 = 미설정.
    int _currentDataId = -1;
    PotionKind _kind = PotionKind.HP;
    bool _showDesc = false;       // S38: true면 설명 표시, false면 재료 표시
    bool _previewNext = false;    // S39: 다음 단계 미리보기 모드
    bool _craftBusy = false;      // S40: 제작 애님 중에는 모든 버튼 비활성
    // 제작의뢰로 열었을 때만 [재료부족/제작], [다음단계 정보] 버튼 표시
    bool _fromCustomOrder = true; // 기본값은 true (슬라이드 36 진입 경로)

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 프레임 / 배경
        _frame     = GetObject((int)GameObjects.PotionDetailPopupFrame).transform;
        _myFrameRT = _frame.GetComponent<RectTransform>();
        _bg        = GetButton((int)Buttons.PotionDetailPopupBackground).GetComponent<Image>();

        // 물약 이미지 & 헤더
        _potionImg    = GetImage((int)Images.PotionDetailPopupPotionImg);
        _nameText     = GetText((int)Texts.PotionDetailPopupPotionName);
        _rankText     = GetText((int)Texts.RankText);
        _nextRankText = GetText((int)Texts.NextLevel_RankText);

        // HP 수치 텍스트 (원래 P.HealAmountTxt → prefab에서 P_HealAmountTxt로 저장)
        _hpTextsGroup = GetObject((int)GameObjects.HPPotion_Texts);
        _flatHeal     = GetText((int)Texts.FlatHealAmountTxt);
        _flatHealN    = GetText((int)Texts.FlatHealAmountTxt_NextLvl);
        _pHeal        = GetText((int)Texts.P_HealAmountTxt);
        _pHealN       = GetText((int)Texts.P_HealAmountTxt_NextLvl);
        _totalHeal    = GetText((int)Texts.TotalHealAmountTxt);
        _totalHealN   = GetText((int)Texts.TotalHealAmountTxt_NextLvl);

        // SPD / 쿨다운 / 설명
        _spdTextsGroup = GetObject((int)GameObjects.SPDPotion_Texts);
        _spdAmount     = GetText((int)Texts.SPDAmountTxt);
        _spdAmountN    = GetText((int)Texts.SPDAmountTxt_NextLvl);
        _duration      = GetText((int)Texts.DURIAmountTxt);
        _durationN     = GetText((int)Texts.DURIAmountTxt_NextLvl);
        _cooldown      = GetText((int)Texts.PotionCooltimeTxt);
        _cooldownN     = GetText((int)Texts.PotionCooltimeTxt_NextLvl);
        _descText      = GetText((int)Texts.DescText);

        // 패널 전환 버튼
        _switchPanelBtn = GetButton((int)Buttons.SwitchPanelBtn)?.gameObject;
        _switchBtnLabel = GetText((int)Texts.Txt_SwitchPanelBtn_01);

        // 재료 그룹 (GameObject였던 Txt_ReqItemGroup_02는 타이틀 텍스트)
        _reqItemGroup  = GetObject((int)GameObjects.ReqItemGroup);
        _reqItemList   = GetObject((int)GameObjects.ReqItemList);
        _reqItemTitle  = GetText((int)Texts.Txt_ReqItemGroup_02)?.gameObject;
        var titleTmp   = GetText((int)Texts.Txt_ReqItemGroup_02);
        if (titleTmp != null) titleTmp.text = Localize(LocalizeKey.ItemNeededText);
        _reqDecorLine  = GetImage((int)Images.__1)?.gameObject;
        _maxLvlTxt     = GetText((int)Texts.MaxLvlTxt);
        _cantCraftBtn  = GetImage((int)Images.CantCraftBtn)?.gameObject;
        _craftBtn      = GetButton((int)Buttons.CraftBtn)?.gameObject;
        _nextLvlBtn    = GetButton((int)Buttons.NextLvlBtn)?.gameObject;
        _backBtn       = GetButton((int)Buttons.BackBtn)?.gameObject;
        _nextLvlBtnLabel = GetText((int)Texts.Txt_NextLvlBtn_05);

        // ReqItemBox1_1 ~ 5 및 아이콘 캐시
        var list     = new List<RectTransform>();
        var iconList = new List<Image>();
        for (int i = 1; i <= 5; i++)
        {
            var box = _reqItemList != null ? _reqItemList.transform.Find($"ReqItemBox1_{i}") : null;
            if (box == null) continue;
            var rt = box.GetComponent<RectTransform>();
            list.Add(rt);
            iconList.Add(FindIconImg(rt));
        }
        _reqItemBoxes = list.ToArray();
        _reqItemIcons = iconList.ToArray();

        // Gauge 세그먼트: 좌→우 순서 (Step1/2/3) — UI 바인드로 직접 획득
        _guageSegments = new[]
        {
            GetImage((int)Images.Img_Guage_Step1),
            GetImage((int)Images.Img_Guage_Step2),
            GetImage((int)Images.Img_Guage_Step3),
        };
        _guageSegmentDefaultColor = new Color[_guageSegments.Length];
        for (int i = 0; i < _guageSegments.Length; i++)
            if (_guageSegments[i] != null) _guageSegmentDefaultColor[i] = _guageSegments[i].color;

        _guageNextLvlGo = GetObject((int)GameObjects.Guage_NextLvl);
        _guageNextLvlSegments = new[]
        {
            GetImage((int)Images.Img_Guage_NextLvl_Step1),
            GetImage((int)Images.Img_Guage_NextLvl_Step2),
            GetImage((int)Images.Img_Guage_NextLvl_Step3),
        };

        _potionImgRT = _potionImg?.rectTransform;

        // 이벤트 바인딩
        _switchPanelBtn?.BindEvent(() => { if (!_craftBusy) OnClickSwitchPanel(); });
        _nextLvlBtn?.BindEvent(()     => { if (!_craftBusy) OnClickNextLvl(); });
        _backBtn?.BindEvent(()        => { if (!_craftBusy) OnClickBack(); });
        _craftBtn?.BindEvent(()       => { if (!_craftBusy) OnClickCraft(); });
        _bg?.gameObject.BindEvent(()  => { if (!_craftBusy) OnClickClose(); });
        GetImage((int)Images.Xbutton)?.gameObject.BindEvent(() => { if (!_craftBusy) OnClickClose(); });

        // SetInfo 전에 보이면 안 되는 오브젝트 초기 숨김
        _descText?.gameObject.SetActive(false);
        _maxLvlTxt?.gameObject.SetActive(false);
        _cantCraftBtn?.SetActive(false);
        _craftBtn?.SetActive(false);
        _nextLvlBtn?.SetActive(false);
        _backBtn?.SetActive(false);
        _spdTextsGroup?.SetActive(false);
        GetObject((int)GameObjects.Guage_NextLvl)?.SetActive(false);
        GetObject((int)GameObjects.SpeedPotionNewBtnHelper)?.SetActive(false);
        GetObject((int)GameObjects.SPDPotionUnlockTxtGroup)?.SetActive(false);
        // NextLvl 비교 텍스트는 다음 단계 미리보기 진입 시에만 표시
        _flatHealN?.gameObject.SetActive(false);
        _pHealN?.gameObject.SetActive(false);
        _totalHealN?.gameObject.SetActive(false);
        _cooldownN?.gameObject.SetActive(false);
        _spdAmountN?.gameObject.SetActive(false);
        _durationN?.gameObject.SetActive(false);

        return true;
    }

    // 박스 직접 자식 Image 중 아이콘을 반환.
    // Mask 컴포넌트 있는 것(GlowMask), white 스프라이트(BlockImg), Icon_inventory 스프라이트(Frame) 제외.
    Image FindIconImg(RectTransform box)
    {
        foreach (Transform child in box)
        {
            var img = child.GetComponent<Image>();
            if (img == null) continue;
            if (child.GetComponent<Mask>() != null) continue;
            var spName = img.sprite != null ? img.sprite.name : null;
            if (spName == "white" || spName == "Icon_inventory") continue;
            return img;
        }
        return null;
    }

    public int PotionId => _currentDataId;

    /// <summary>
    /// dataId: HPPotionDic 또는 SpeedPotionDic의 DataId.
    /// 범위로 종류 자동 판정 (40000+ = SPD, 그 외 = HP).
    /// -1이면 플레이어의 현재 HP 물약 단계 사용.
    /// </summary>
    public void SetInfo(int dataId = -1, bool fromCustomOrder = true)
    {
        _kind           = InferKind(dataId);
        _currentDataId  = ResolvePotionDataId(dataId, _kind);
        _fromCustomOrder = fromCustomOrder;

        // 팝업을 새로 열 때는 항상 재료 목록 / 현재 단계 상태로 시작 (S38 스펙)
        _showDesc = false;
        _previewNext = false;
        _craftBusy = false;

        // 프리팹에 -130 이 저장돼 있어도 실제 휴식 위치는 (0,0)
        _frameOrigin = Vector2.zero;

        // 팝업 진입: From y=-130, OutBack 0.5s
        _myFrameRT.DOKill();
        _myFrameRT.anchoredPosition = _frameOrigin + new Vector2(0f, FRAME_FROM_Y);
        _myFrameRT.DOAnchorPos(_frameOrigin, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        if (_bg != null)
        {
            _bg.DOKill();
            Color c = _bg.color; c.a = 0f; _bg.color = c;
            _bg.DOFade(220f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        RefreshAll();
    }

    void RefreshAll()
    {
        var data = GetView(_currentDataId);
        if (data == null) return;

        StopBlink();
        ApplyData(data);
        RefreshPanelMode();
    }

    // ────────────────────────────────────────
    //  S37: Data binding
    // ────────────────────────────────────────
    PotionView GetView(int dataId)
    {
        if (IsSpeedPotionId(dataId))
        {
            if (!Managers.Data.SpeedPotionDic.TryGetValue(dataId, out var s)) return null;
            return new PotionView
            {
                Kind = PotionKind.Speed,
                DataId = s.DataId, Name = s.Name, Grade = s.Grade,
                Description = s.Description, SpriteName = s.SpriteName,
                Step = s.Step, Cooldown = s.Cooldown,
                NeededItemIDs = s.NeededItemIDs, NeededItemAmounts = s.NeededItemAmounts,
                AtkSpeedBonus = s.AtkSpeedBonus, Duration = s.Duration,
            };
        }
        if (Managers.Data.HPPotionDic.TryGetValue(dataId, out var h))
        {
            return new PotionView
            {
                Kind = PotionKind.HP,
                DataId = h.DataId, Name = h.Name, Grade = h.Grade,
                Description = h.Description, SpriteName = h.SpriteName,
                Step = h.Step, Cooldown = h.Cooldown,
                NeededItemIDs = h.NeededItemIDs, NeededItemAmounts = h.NeededItemAmounts,
                FlatRecov = h.FlatRecov, MaxHPRecov = h.MaxHPRecov,
            };
        }
        return null;
    }

    static bool IsSpeedPotionId(int dataId)
    {
        return dataId >= SPEED_POTION_ID_MIN && dataId < SPEED_POTION_ID_MIN + 10000;
    }

    static PotionKind InferKind(int dataId)
    {
        return IsSpeedPotionId(dataId) ? PotionKind.Speed : PotionKind.HP;
    }

    string Localize(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (!Managers.Data.LocalizationDic.TryGetValue(key, out var entry)) return key;
        return Managers.Game.Language == Define.Language.Korean ? entry.Kor : entry.Eng;
    }

    void ApplyData(PotionView data)
    {
        // 종류에 맞춰 텍스트 그룹 전환
        bool isHP = data.Kind == PotionKind.HP;
        _hpTextsGroup.SetActive(isHP);
        _spdTextsGroup.SetActive(!isHP);

        _nameText.text = Localize(data.Name);
        _rankText.text = $"{data.Step}{Localize(LocalizeKey.RankText)}";
        _rankText.color = Color.white;

        // 현재 단계에 맞춰 Gauge 채우기 (S37 스펙) — 미리보기 아니므로 초록 없음
        UpdateGauge(filledCount: data.Step, greenFromIndex: data.Step);
        _guageNextLvlGo?.SetActive(false);

        if (isHP)
            ApplyHPStats(data, preview: false);
        else
            ApplySpeedStats(data, preview: false);

        _descText.text = Localize(data.Description);

        var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
        if (sp != null) _potionImg.sprite = sp;

        var nextData = GetNextView(_currentDataId);

        // 필요 재료는 다음 단계 업그레이드 기준으로 표시
        UpdateReqItems(nextData);

        // 이 물약이 마지막 단계면 MAX LEVEL
        bool isMaxLevel = nextData == null;
        _maxLvlTxt?.gameObject.SetActive(isMaxLevel);

        // 제작의뢰에서 열었을 때만 [재료부족/제작]/[다음단계 정보] 표시
        bool showCraftUI = _fromCustomOrder && !isMaxLevel;
        _cantCraftBtn?.SetActive(false); // 재료 체크 후 결정
        _craftBtn?.SetActive(false);
        _nextLvlBtn?.SetActive(false);
        _backBtn?.SetActive(false);

        if (showCraftUI)
        {
            bool canCraft = HasEnoughMaterials(nextData);
            _craftBtn?.SetActive(canCraft);
            _cantCraftBtn?.SetActive(!canCraft);
            _nextLvlBtn?.SetActive(true);
        }
    }

    /// <summary>Gauge 세그먼트 표시.
    /// filledCount: 채워질 칸 수 (1~3)
    /// greenFromIndex: 이 index부터 채워지는 칸은 초록색(#00FF00)으로 표시 (S39 미리보기 — 새로 추가되는 단계만).
    ///   normal 모드에서는 greenFromIndex == filledCount 로 호출하여 초록 없음.</summary>
    void UpdateGauge(int filledCount, int greenFromIndex)
    {
        if (_guageSegments == null) return;
        filledCount    = Mathf.Clamp(filledCount,    0, _guageSegments.Length);
        greenFromIndex = Mathf.Clamp(greenFromIndex, 0, _guageSegments.Length);

        for (int i = 0; i < _guageSegments.Length; i++)
        {
            var seg = _guageSegments[i];
            if (seg == null) continue;
            bool filled = i < filledCount;
            seg.gameObject.SetActive(filled);
            if (filled)
                seg.color = (i >= greenFromIndex) ? GUAGE_PREVIEW_COLOR : _guageSegmentDefaultColor[i];
        }
    }

    /// <summary>NextLvl Gauge(초록) — 다음 단계 개수만큼 채운 상태로 표시.</summary>
    void UpdateGaugeNextLvl(int step)
    {
        if (_guageNextLvlGo != null) _guageNextLvlGo.SetActive(true);
        if (_guageNextLvlSegments == null) return;
        int filledCount = Mathf.Clamp(step, 0, _guageNextLvlSegments.Length);
        for (int i = 0; i < _guageNextLvlSegments.Length; i++)
        {
            var seg = _guageNextLvlSegments[i];
            if (seg == null) continue;
            bool filled = i < filledCount;
            seg.gameObject.SetActive(filled);
            if (filled) seg.color = GUAGE_PREVIEW_COLOR;
        }
    }

    void ApplyHPStats(PotionView data, bool preview)
    {
        int maxHp = Managers.Game.GetPlayerMaxHp();
        int pHealAmount = data.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * data.MaxHPRecov / 100f);
        int totalHeal   = data.FlatRecov + pHealAmount;
        string sec = Localize(LocalizeKey.SecondsUnitText);

        _flatHeal.text  = $"{Localize(LocalizeKey.FlatRecovText)}: {data.FlatRecov}";
        _pHeal.text     = $"{Localize(LocalizeKey.MaxHPRecovText)}: {data.MaxHPRecov}% ({pHealAmount})";
        _totalHeal.text = $"{Localize(LocalizeKey.TotalRecovText)}: {totalHeal}";
        _cooldown.text  = $"{Localize(LocalizeKey.PotionCDText)}: {data.Cooldown}{sec}";
    }

    void ApplySpeedStats(PotionView data, bool preview)
    {
        int bonusPct = Mathf.RoundToInt(data.AtkSpeedBonus * 100f);
        string sec = Localize(LocalizeKey.SecondsUnitText);

        _spdAmount.text = $"{Localize(LocalizeKey.SPDBonusText)}: +{bonusPct}%";
        _duration.text  = $"{Localize(LocalizeKey.DurationText)}: {data.Duration}{sec}";
        _cooldown.text  = $"{Localize(LocalizeKey.PotionCDText)}: {data.Cooldown}{sec}";
    }

    void UpdateReqItems(PotionView data)
    {
        if (data == null)
        {
            foreach (var box in _reqItemBoxes)
                box.gameObject.SetActive(false);
            return;
        }

        int n = data.NeededItemIDs != null ? data.NeededItemIDs.Count : 0;
        for (int i = 0; i < _reqItemBoxes.Length; i++)
        {
            var box = _reqItemBoxes[i];
            bool use = i < n;
            box.gameObject.SetActive(use);
            if (!use) continue;

            int itemId = data.NeededItemIDs[i];
            int required = data.NeededItemAmounts[i];
            int have = Managers.Game.GetItemCount(itemId);

            // Amount: 보유/필요. 부족하면 빨강.
            var amount = box.GetComponentInChildren<TMP_Text>(true);
            if (amount != null)
            {
                amount.text = $"{have}/{required}";
                amount.color = have >= required ? Color.white : new Color32(0xFF, 0x55, 0x55, 0xFF);
            }

            // 아이콘: Init()에서 캐시한 아이콘 Image 사용.
            var img = i < _reqItemIcons.Length ? _reqItemIcons[i] : null;
            if (img != null)
            {
                if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
                if (TryGetItemData(itemId, out var itemData))
                {
                    var sp = Managers.Resource.Load<Sprite>(itemData.SpriteName);
                    if (sp != null) img.sprite = sp;
                }
            }
        }
    }

    /// <summary>모든 필요 재료를 충족하는지 체크.</summary>
    bool HasEnoughMaterials(PotionView data)
    {
        if (data == null || data.NeededItemIDs == null) return true;
        for (int i = 0; i < data.NeededItemIDs.Count; i++)
        {
            int itemId = data.NeededItemIDs[i];
            int required = data.NeededItemAmounts[i];
            int have = Managers.Game.GetItemCount(itemId);
            if (have < required) return false;
        }
        return true;
    }

    // ────────────────────────────────────────
    //  S38: 설명 ↔ 필요재료 토글
    // ────────────────────────────────────────
    void OnClickSwitchPanel()
    {
        _showDesc = !_showDesc;
        RefreshPanelMode();
        PunchBtnScale(_switchPanelBtn.transform);
    }

    /// <summary>
    /// S38: _showDesc에 따라 재료 영역 ↔ 설명 영역 전환.
    /// 설명 모드일 때는 [재료부족/제작], [다음단계 정보] 숨김 + 설명Text 표시.
    /// SwitchPanelBtn 라벨도 DESCRIP ↔ REQ. ITEMS 토글.
    /// </summary>
    void RefreshPanelMode()
    {
        if (_showDesc)
        {
            _reqItemList?.SetActive(false);
            _reqItemTitle?.SetActive(false);
            if (_reqDecorLine != null) _reqDecorLine.SetActive(false);
            _cantCraftBtn?.SetActive(false);
            _craftBtn?.SetActive(false);
            _nextLvlBtn?.SetActive(false);
            _backBtn?.SetActive(false);
            _maxLvlTxt?.gameObject.SetActive(false);
            _descText.gameObject.SetActive(true);
            if (_switchBtnLabel != null) _switchBtnLabel.text = Localize(LocalizeKey.ItemNeededText);
        }
        else
        {
            _descText.gameObject.SetActive(false);
            _reqItemList?.SetActive(true);
            _reqItemTitle?.SetActive(true);
            if (_reqDecorLine != null) _reqDecorLine.SetActive(true);
            if (_switchBtnLabel != null) _switchBtnLabel.text = Localize(LocalizeKey.DescriptionText);

            // 재료/버튼 상태는 ApplyData가 결정하므로 여기서 다시 재계산
            var data = GetView(_currentDataId);
            if (data != null)
            {
                var next = GetNextView(_currentDataId);
                bool isMaxLevel = next == null;
                _maxLvlTxt.gameObject.SetActive(isMaxLevel);

                bool showCraftUI = _fromCustomOrder && !isMaxLevel;
                if (_previewNext)
                {
                    // S39 preview 상태: [재료부족/제작] 버튼은 현재 단계 기준으로 그대로 유지
                    // (다음단계 정보 → [뒤로]로만 교체)
                    bool canCraft = HasEnoughMaterials(next);
                    _craftBtn?.SetActive(showCraftUI && canCraft);
                    _cantCraftBtn?.SetActive(showCraftUI && !canCraft);
                    _nextLvlBtn?.SetActive(false);
                    _backBtn?.SetActive(showCraftUI);
                }
                else if (showCraftUI)
                {
                    bool canCraft = HasEnoughMaterials(next);
                    _craftBtn?.SetActive(canCraft);
                    _cantCraftBtn?.SetActive(!canCraft);
                    _nextLvlBtn?.SetActive(true);
                    _backBtn?.SetActive(false);
                }
            }
        }
    }

    // ────────────────────────────────────────
    //  S39: 다음 단계 정보 미리보기
    // ────────────────────────────────────────
    void OnClickNextLvl()
    {
        PunchBtnScale(_nextLvlBtn.transform);
        var next = GetNextView(_currentDataId);
        if (next == null) return;

        _previewNext = true;
        ApplyPreview(next);
        RefreshPanelMode();
    }

    void OnClickBack()
    {
        PunchBtnScale(_backBtn.transform);
        _previewNext = false;
        RefreshAll(); // 원래 데이터로 복귀
    }

    void ApplyPreview(PotionView next)
    {
        var curr = GetView(_currentDataId);
        if (curr == null) return;

        // 이름/단계/아이콘은 다음 값으로 교체. 단, 변경부 하이라이트 및 점멸 적용.
        _nameText.text = Localize(next.Name);
        _rankText.text = $"{next.Step}{Localize(LocalizeKey.RankText)}";
        _rankText.color = next.Grade != curr.Grade
            ? new Color32(0x00, 0xFF, 0x00, 0xFF)
            : Color.white;

        var sp = Managers.Resource.Load<Sprite>(next.SpriteName);
        if (sp != null) _potionImg.sprite = sp;

        // S39 Gauge:
        //   같은 등급에서 단계업: 새로 추가되는 칸(curr.Step..next.Step-1)만 초록.
        //     예) 1단계→2단계 : 2단계 칸만 초록, 1단계 칸은 기본색 유지.
        //   등급 변경: 현재 Gauge는 그대로 + NextLvl Gauge에 새 등급 단계 표시 (전부 초록).
        if (next.Grade == curr.Grade)
        {
            UpdateGauge(filledCount: next.Step, greenFromIndex: curr.Step);
            _guageNextLvlGo?.SetActive(false);
        }
        else
        {
            UpdateGauge(filledCount: curr.Step, greenFromIndex: curr.Step); // 현 단계 유지 + 초록 없음
            UpdateGaugeNextLvl(next.Step);
        }

        // S39 Scale Anim: 등급 변경 시 아이콘을 from 3 OutBack 0.3s로 팝업
        if (next.Grade != curr.Grade && _potionImgRT != null)
        {
            _potionImgRT.DOKill();
            _potionImgRT.localScale = Vector3.one * 3f;
            _potionImgRT.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        string sec = Localize(LocalizeKey.SecondsUnitText);

        if (curr.Kind == PotionKind.HP)
        {
            int maxHp = Managers.Game.GetPlayerMaxHp();
            int pCur  = curr.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * curr.MaxHPRecov / 100f);
            int pNext = next.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * next.MaxHPRecov / 100f);
            int totalCur  = curr.FlatRecov + pCur;
            int totalNext = next.FlatRecov + pNext;

            _flatHeal.text  = FmtStat(Localize(LocalizeKey.FlatRecovText),  curr.FlatRecov,   next.FlatRecov);
            _pHeal.text     = FmtStat(Localize(LocalizeKey.MaxHPRecovText), pCur,             pNext);
            _totalHeal.text = FmtStat(Localize(LocalizeKey.TotalRecovText), totalCur,         totalNext);
        }
        else
        {
            int spdCur  = Mathf.RoundToInt(curr.AtkSpeedBonus * 100f);
            int spdNext = Mathf.RoundToInt(next.AtkSpeedBonus * 100f);
            _spdAmount.text = FmtStat(Localize(LocalizeKey.SPDBonusText), spdCur,        spdNext,        suffix: "%", prefix: "+");
            _duration.text  = FmtStat(Localize(LocalizeKey.DurationText), curr.Duration, next.Duration, suffix: sec);
        }
        // 재생산시간은 값이 작을수록 좋으므로 색상 방향 반전 (공통)
        _cooldown.text  = FmtStat(Localize(LocalizeKey.PotionCDText), curr.Cooldown,    next.Cooldown,   suffix: sec, lowerIsBetter: true);

        StartBlink();
    }

    string FmtStat(string label, int cur, int next, string suffix = "", bool lowerIsBetter = false, string prefix = "")
    {
        bool improved = lowerIsBetter ? (next < cur) : (next > cur);
        bool worsened = lowerIsBetter ? (next > cur) : (next < cur);
        string colorHex = improved ? "00FF00" : (worsened ? "FF0000" : "FFFFFF");
        return $"{label}: {prefix}{cur}{suffix} <color=#{colorHex}>▶ {prefix}{next}{suffix}</color>";
    }

    Sequence _blinkSeq;

    Graphic[] BlinkTargets()
    {
        if (_kind == PotionKind.Speed)
            return new Graphic[] { _nameText, _rankText, _spdAmount, _duration, _cooldown };
        return new Graphic[] { _nameText, _rankText, _flatHeal, _pHeal, _totalHeal, _cooldown };
    }

    void StartBlink()
    {
        StopBlink();
        _blinkSeq = DOTween.Sequence().SetUpdate(true).SetLoops(-1);
        // 이름/단계/능력치 라벨 알파 점멸 (OutSine 0.5s in / 0.5s out)
        var targets = BlinkTargets();
        foreach (var t in targets)
        {
            if (t == null) continue;
            _blinkSeq.Join(t.DOFade(0.45f, 0.4f).SetEase(Ease.InOutSine));
        }
        _blinkSeq.AppendInterval(0.1f);
        foreach (var t in targets)
        {
            if (t == null) continue;
            _blinkSeq.Join(t.DOFade(1f, 0.4f).SetEase(Ease.InOutSine));
        }
    }

    void StopBlink()
    {
        if (_blinkSeq != null && _blinkSeq.IsActive()) _blinkSeq.Kill();
        _blinkSeq = null;
        var targets = BlinkTargets();
        foreach (var t in targets)
        {
            if (t == null) continue;
            Color c = t.color; c.a = 1f; t.color = c;
        }
        if (_rankText != null) _rankText.color = Color.white;
    }

    // ────────────────────────────────────────
    //  S40: 제작 버튼 애님
    // ────────────────────────────────────────
    void OnClickCraft()
    {
        PunchBtnScale(_craftBtn.transform);
        var next = GetNextView(_currentDataId);
        if (next == null) return; // 최대 레벨은 제작 불가

        bool canCraft = _kind == PotionKind.Speed
            ? Managers.Game.CanCraftSpeedPotion(_currentDataId)
            : Managers.Game.CanCraftHPPotion(_currentDataId);
        if (!canCraft) return;

        _craftBusy = true;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_myFrameRT.DOShakeAnchorPos(1.1f, strength: new Vector3(1.5f, 0f, 0f), vibrato: 21, randomness: 0f).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => SpawnCraftFX());
        seq.Append(_myFrameRT.DOPunchScale(new Vector3(0.2f, 0.2f, 1f), 0.3f, vibrato: 4, elasticity: 1f).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            bool ok;
            int craftedDataId;
            if (_kind == PotionKind.Speed)
                ok = Managers.Game.TryCraftSpeedPotion(_currentDataId, out craftedDataId);
            else
                ok = Managers.Game.TryCraftHPPotion(_currentDataId, out craftedDataId);

            if (ok)
                _currentDataId = craftedDataId;

            _previewNext = false;
            RefreshAll();
            _craftBusy = false;
        });
    }

    /// <summary>
    /// S40: 제작 시 FX 스폰 — 중앙 splash + 물약 아이콘 upgrade sparkle.
    /// Coffee.UIExtensions.UIParticle 패키지로 ParticleSystem prefab을 UI Canvas 위에 렌더.
    /// </summary>
    void SpawnCraftFX()
    {
        // 팝업 프레임 중앙: FX_splash_spark_floor
        UIFx.PlayParticle(_myFrameRT, Vector2.zero, "FX_splash_spark_floor");

        // 물약 아이콘 위치: FX_splash_spark_air
        Vector3 iconWorld = _potionImg.rectTransform.position;
        Vector3 iconLocal = _myFrameRT.InverseTransformPoint(iconWorld);
        UIFx.PlayParticle(_myFrameRT, new Vector2(iconLocal.x, iconLocal.y), "FX_splash_spark_air");
    }

    void PunchBtnScale(Transform btn)
    {
        btn.DOKill();
        btn.localScale = Vector3.one;
        btn.DOScale(0.9f, 0.07f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
    }

    int ResolvePotionDataId(int requestedDataId, PotionKind kind)
    {
        if (kind == PotionKind.Speed)
        {
            if (requestedDataId > 0 && Managers.Data.SpeedPotionDic.ContainsKey(requestedDataId))
                return requestedDataId;

            int cur = Managers.Game.CurrentSpeedPotionDataId;
            if (Managers.Data.SpeedPotionDic.ContainsKey(cur))
                return cur;

            return SPEED_POTION_ID_MIN + 1;
        }

        if (requestedDataId > 0 && Managers.Data.HPPotionDic.ContainsKey(requestedDataId))
            return requestedDataId;

        int currentDataId = Managers.Game.CurrentHPPotionDataId;
        if (Managers.Data.HPPotionDic.ContainsKey(currentDataId))
            return currentDataId;

        return POTION_ID_MIN + 1;
    }

    PotionView GetNextView(int currentDataId)
    {
        int nextDataId = _kind == PotionKind.Speed
            ? Managers.Game.GetNextSpeedPotionDataId(currentDataId)
            : Managers.Game.GetNextHPPotionDataId(currentDataId);
        return nextDataId > 0 ? GetView(nextDataId) : null;
    }

    bool TryGetItemData(int itemId, out ItemData itemData)
    {
        if (Managers.Data.ItemDic.TryGetValue(itemId, out itemData))
            return true;

        if (Managers.Data.RequisiteItemDic.TryGetValue(itemId, out itemData))
            return true;

        itemData = null;
        return false;
    }

    // ────────────────────────────────────────
    //  Close
    // ────────────────────────────────────────
    void OnClickClose()
    {
        StopBlink();
        _myFrameRT.DOKill();

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_myFrameRT.DOAnchorPos(_frameOrigin + new Vector2(0f, FRAME_FROM_Y), 0.3f).SetEase(Ease.InBack));
        seq.OnComplete(() => Managers.UI.ClosePopupUI());
    }

    public override void OnDestroy()
    {
        StopBlink();
        base.OnDestroy();
    }
}
