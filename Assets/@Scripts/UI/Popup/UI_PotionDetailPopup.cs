using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data;
using Sequence = DG.Tweening.Sequence;

/// <summary>
/// 슬라이드 36~40: 물약 상세정보 팝업.
/// S36: 호출 진입 애님 / S37: 기본 데이터 바인딩 / S38: 설명 ↔ 필요재료 토글 /
/// S39: 다음 단계 정보 미리보기 + 하이라이트 / S40: 제작 버튼 애님 + 갱신.
/// </summary>
public class UI_PotionDetailPopup : UI_Popup
{
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
        Img_Guage_02,
        Img_Guage_04,
        Img_Guage_01,
        background,
        Img_Guage_NextLvl_02,
        Img_Guage_NextLvl_01,
        Img_Guage_NextLvl_03,
        Img_Guage_NextLvl_04,
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

    Vector2 _frameOrigin;

    // 현재 표시 중인 물약 DataId (HPPotionDic 기준). -1 = 미설정.
    int _currentDataId = -1;
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
        if (titleTmp != null) titleTmp.text = "필요 재료";
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

        // 이벤트 바인딩
        _switchPanelBtn?.BindEvent(() => { if (!_craftBusy) OnClickSwitchPanel(); });
        _nextLvlBtn?.BindEvent(()     => { if (!_craftBusy) OnClickNextLvl(); });
        _backBtn?.BindEvent(()        => { if (!_craftBusy) OnClickBack(); });
        _craftBtn?.BindEvent(()       => { if (!_craftBusy) OnClickCraft(); });
        _bg?.gameObject.BindEvent(()  => { if (!_craftBusy) OnClickClose(); });
        GetImage((int)Images.Xbutton)?.gameObject.BindEvent(() => { if (!_craftBusy) OnClickClose(); });

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
    /// dataId: HPPotionDic의 DataId. (-1이면 기본 30001)
    /// </summary>
    public void SetInfo(int dataId = -1, bool fromCustomOrder = true)
    {
        _currentDataId  = ResolvePotionDataId(dataId);
        _fromCustomOrder = fromCustomOrder;

        // 팝업을 새로 열 때는 항상 재료 목록 / 현재 단계 상태로 시작 (S38 스펙)
        _showDesc = false;
        _previewNext = false;
        _craftBusy = false;

        _frameOrigin = _myFrameRT.anchoredPosition;

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
        var data = GetData(_currentDataId);
        if (data == null) return;

        StopBlink();
        ApplyData(data);
        RefreshPanelMode();
    }

    // ────────────────────────────────────────
    //  S37: Data binding
    // ────────────────────────────────────────
    HPPotionData GetData(int dataId)
    {
        if (Managers.Data.HPPotionDic.TryGetValue(dataId, out var d)) return d;
        return null;
    }

    string Localize(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (!Managers.Data.LocalizationDic.TryGetValue(key, out var entry)) return key;
        return Managers.Game.Language == Define.Language.Korean ? entry.Kor : entry.Eng;
    }

    void ApplyData(HPPotionData data)
    {
        // HP 계열만 노출 (SPD 텍스트 그룹 숨김)
        _hpTextsGroup.SetActive(true);
        _spdTextsGroup.SetActive(false);

        _nameText.text = Localize(data.Name);
        _rankText.text = $"{data.Step}단계";
        _rankText.color = Color.white;

        int maxHp = Managers.Game.GetPlayerMaxHp();
        int pHealAmount = data.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * data.MaxHPRecov / 100f);
        int totalHeal   = data.FlatRecov + pHealAmount;

        _flatHeal.text  = $"기본 회복량: {data.FlatRecov}";
        _pHeal.text     = $"최대 체력 회복량: {data.MaxHPRecov}% ({pHealAmount})";
        _totalHeal.text = $"총 회복량: {totalHeal}";
        _cooldown.text  = $"재생산시간: {data.Cooldown}초";

        _descText.text = Localize(data.Description);

        // 아이콘: data.SpriteName (spriteloader는 후속)
        // ex. "HP_Potion1" 키가 로드돼있으면 교체
        var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
        if (sp != null) _potionImg.sprite = sp;

        var nextData = GetNextData(_currentDataId);

        // 필요 재료는 다음 단계 업그레이드 기준으로 표시
        UpdateReqItems(nextData);

        // 이 물약이 HP 계열의 마지막 단계면 MAX LEVEL
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

    void UpdateReqItems(HPPotionData data)
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
    bool HasEnoughMaterials(HPPotionData data)
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
            if (_switchBtnLabel != null) _switchBtnLabel.text = "필요재료";
        }
        else
        {
            _descText.gameObject.SetActive(false);
            _reqItemList?.SetActive(true);
            _reqItemTitle?.SetActive(true);
            if (_reqDecorLine != null) _reqDecorLine.SetActive(true);
            if (_switchBtnLabel != null) _switchBtnLabel.text = "설명";

            // 재료/버튼 상태는 ApplyData가 결정하므로 여기서 다시 재계산
            var data = GetData(_currentDataId);
            if (data != null)
            {
                var next = GetNextData(_currentDataId);
                bool isMaxLevel = next == null;
                _maxLvlTxt.gameObject.SetActive(isMaxLevel);

                bool showCraftUI = _fromCustomOrder && !isMaxLevel;
                if (_previewNext)
                {
                    // S39 preview 상태: 제작/재료부족 숨김, 다음단계 → [뒤로]로 바뀜
                    _craftBtn?.SetActive(false);
                    _cantCraftBtn?.SetActive(false);
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
        var next = GetNextData(_currentDataId);
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

    void ApplyPreview(HPPotionData next)
    {
        var curr = GetData(_currentDataId);
        if (curr == null) return;

        // 이름/단계/아이콘은 다음 값으로 교체. 단, 변경부 하이라이트 및 점멸 적용.
        _nameText.text = Localize(next.Name);
        _rankText.text = $"{next.Step}단계";
        // 등급 변경(3단계 → 다음 등급 1단계)이면 단계 색을 00FF00으로
        _rankText.color = next.Grade != curr.Grade
            ? new Color32(0x00, 0xFF, 0x00, 0xFF)
            : Color.white;

        var sp = Managers.Resource.Load<Sprite>(next.SpriteName);
        if (sp != null) _potionImg.sprite = sp;

        // 능력치: ▶ 표기 + 변화 색상
        int maxHp = Managers.Game.GetPlayerMaxHp();
        int pCur  = curr.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * curr.MaxHPRecov / 100f);
        int pNext = next.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * next.MaxHPRecov / 100f);
        int totalCur  = curr.FlatRecov + pCur;
        int totalNext = next.FlatRecov + pNext;

        _flatHeal.text  = FmtStat("기본 회복량",       curr.FlatRecov,   next.FlatRecov);
        _pHeal.text     = FmtStat("최대 체력 회복량", pCur,             pNext);
        _totalHeal.text = FmtStat("총 회복량",         totalCur,         totalNext);
        // 재생산시간은 값이 작을수록 좋으므로 색상 방향 반전
        _cooldown.text  = FmtStat("재생산시간",        curr.Cooldown,    next.Cooldown,   suffix: "초", lowerIsBetter: true);

        StartBlink();
    }

    string FmtStat(string label, int cur, int next, string suffix = "", bool lowerIsBetter = false)
    {
        bool improved = lowerIsBetter ? (next < cur) : (next > cur);
        bool worsened = lowerIsBetter ? (next > cur) : (next < cur);
        string colorHex = improved ? "00FF00" : (worsened ? "FF0000" : "FFFFFF");
        return $"{label}: {cur}{suffix} <color=#{colorHex}>▶ {next}{suffix}</color>";
    }

    Sequence _blinkSeq;
    void StartBlink()
    {
        StopBlink();
        _blinkSeq = DOTween.Sequence().SetUpdate(true).SetLoops(-1);
        // 이름/단계/능력치 라벨 알파 점멸 (OutSine 0.5s in / 0.5s out)
        Graphic[] targets = { _nameText, _rankText, _flatHeal, _pHeal, _totalHeal, _cooldown };
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
        Graphic[] targets = { _nameText, _rankText, _flatHeal, _pHeal, _totalHeal, _cooldown };
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
        var next = GetNextData(_currentDataId);
        if (next == null) return; // 최대 레벨은 제작 불가
        if (!Managers.Game.CanCraftHPPotion(_currentDataId)) return;

        _craftBusy = true;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_myFrameRT.DOShakeAnchorPos(1.1f, strength: new Vector3(1.5f, 0f, 0f), vibrato: 21, randomness: 0f).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => SpawnCraftFX());
        seq.Append(_myFrameRT.DOPunchScale(new Vector3(0.2f, 0.2f, 1f), 0.3f, vibrato: 4, elasticity: 1f).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            if (Managers.Game.TryCraftHPPotion(_currentDataId, out var craftedDataId))
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

    int ResolvePotionDataId(int requestedDataId)
    {
        if (requestedDataId > 0 && Managers.Data.HPPotionDic.ContainsKey(requestedDataId))
            return requestedDataId;

        int currentDataId = Managers.Game.CurrentHPPotionDataId;
        if (Managers.Data.HPPotionDic.ContainsKey(currentDataId))
            return currentDataId;

        return 30001;
    }

    HPPotionData GetNextData(int currentDataId)
    {
        int nextDataId = Managers.Game.GetNextHPPotionDataId(currentDataId);
        return nextDataId > 0 ? GetData(nextDataId) : null;
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
