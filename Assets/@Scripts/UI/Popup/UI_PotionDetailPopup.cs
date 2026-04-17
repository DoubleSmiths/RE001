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

    // 재료 보유량 스텁 (실제 인벤토리 연동 시 교체)
    static readonly Dictionary<int, int> _inventoryStub = new Dictionary<int, int>
    {
        { 1001, 5 },  // HP Step 2는 1001 2개 필요 → 충족 | Step 3은 10개 필요 → 부족
        { 1002, 0 },
    };

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

        // 하위 구조에 dot(.)을 포함한 이름이 있어 enum 바인딩 대신 transform.Find 사용
        _frame   = transform.Find("FIXED/In_Frame/PotionDetailPopup/PotionDetailPopupFrame");
        _myFrameRT = _frame.GetComponent<RectTransform>();
        _bg      = transform.Find("FIXED/In_Frame/PotionDetailPopup/PotionDetailPopupBackground").GetComponent<Image>();
        _potionImg = _frame.Find("FrameImg/Img/PotionDetailPopupPotionImg").GetComponent<Image>();
        _nameText  = _frame.Find("FrameImg/PotionDetailPopupTitle/Mask/PotionDetailPopupPotionName").GetComponent<TMP_Text>();
        _rankText     = _frame.Find("FrameImg/Guage/RankText").GetComponent<TMP_Text>();
        _nextRankText = _frame.Find("FrameImg/Guage_NextLvl/NextLevel_RankText").GetComponent<TMP_Text>();

        var hpG = _frame.Find("FrameImg/HPPotion_Texts");
        _hpTextsGroup = hpG.gameObject;
        _flatHeal   = hpG.Find("FlatHealAmountTxt").GetComponent<TMP_Text>();
        _flatHealN  = hpG.Find("FlatHealAmountTxt_NextLvl").GetComponent<TMP_Text>();
        _pHeal      = hpG.Find("P.HealAmountTxt").GetComponent<TMP_Text>();
        _pHealN     = hpG.Find("P.HealAmountTxt_NextLvl").GetComponent<TMP_Text>();
        _totalHeal  = hpG.Find("TotalHealAmountTxt").GetComponent<TMP_Text>();
        _totalHealN = hpG.Find("TotalHealAmountTxt_NextLvl").GetComponent<TMP_Text>();

        _spdTextsGroup = _frame.Find("FrameImg/SPDPotion_Texts").gameObject;
        _cooldown  = _frame.Find("FrameImg/PotionCooltimeTxt").GetComponent<TMP_Text>();
        _cooldownN = _frame.Find("FrameImg/PotionCooltimeTxt_NextLvl").GetComponent<TMP_Text>();
        _descText  = _frame.Find("FrameImg/DescText").GetComponent<TMP_Text>();

        _switchPanelBtn = _frame.Find("FrameImg/SwitchPanelBtn").gameObject;
        _switchBtnLabel = FindFirstTMPInChildren(_switchPanelBtn.transform);

        _reqItemGroup = _frame.Find("FrameImg/ReqItemGroup").gameObject;
        _reqItemList  = _reqItemGroup.transform.Find("ReqItemList").gameObject;
        _reqItemTitle = _reqItemGroup.transform.Find("GameObject").gameObject; // "REQ. ITEMS" title
        _reqDecorLine = _reqItemGroup.transform.Find("__1").gameObject;        // Decor_description_line
        var titleTmp = _reqItemTitle.GetComponentInChildren<TMP_Text>(true);
        if (titleTmp != null) titleTmp.text = "필요 재료";
        _maxLvlTxt    = _reqItemGroup.transform.Find("MaxLvlTxt").GetComponent<TMP_Text>();
        _cantCraftBtn = _reqItemGroup.transform.Find("CantCraftBtn").gameObject;
        _craftBtn     = _reqItemGroup.transform.Find("CraftBtn").gameObject;
        _nextLvlBtn   = _reqItemGroup.transform.Find("NextLvlBtn").gameObject;
        _backBtn      = _reqItemGroup.transform.Find("BackBtn").gameObject;
        _nextLvlBtnLabel = FindFirstTMPInChildren(_nextLvlBtn.transform);

        // ReqItemBox1_1 ~ ReqItemBox1_5
        var list = new List<RectTransform>();
        for (int i = 1; i <= 5; i++)
        {
            var box = _reqItemList.transform.Find($"ReqItemBox1_{i}");
            if (box != null) list.Add(box.GetComponent<RectTransform>());
        }
        _reqItemBoxes = list.ToArray();

        // 이벤트 바인딩
        _switchPanelBtn.BindEvent(() => { if (!_craftBusy) OnClickSwitchPanel(); });
        _nextLvlBtn.BindEvent(()     => { if (!_craftBusy) OnClickNextLvl(); });
        _backBtn.BindEvent(()        => { if (!_craftBusy) OnClickBack(); });
        _craftBtn.BindEvent(()       => { if (!_craftBusy) OnClickCraft(); });
        _bg.gameObject.BindEvent(()  => { if (!_craftBusy) OnClickClose(); });
        // X 버튼이 있으면 바인딩
        var xBtn = transform.Find("FIXED/In_Frame/PotionDetailPopup/Xbutton");
        if (xBtn != null) xBtn.gameObject.BindEvent(() => { if (!_craftBusy) OnClickClose(); });

        return true;
    }

    TMP_Text FindFirstTMPInChildren(Transform t)
    {
        foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
            return tmp;
        return null;
    }

    public int PotionId => _currentDataId;

    /// <summary>
    /// dataId: HPPotionDic의 DataId. (-1이면 기본 30001)
    /// </summary>
    public void SetInfo(int dataId = 30001, bool fromCustomOrder = true)
    {
        _currentDataId  = dataId > 0 ? dataId : 30001;
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

        ApplyData(data, isPreview: false);
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

    void ApplyData(HPPotionData data, bool isPreview)
    {
        // HP 계열만 노출 (SPD 텍스트 그룹 숨김)
        _hpTextsGroup.SetActive(true);
        _spdTextsGroup.SetActive(false);

        _nameText.text = Localize(data.Name);
        _rankText.text = $"{data.Step}단계";

        int maxHp = 1000; // 표기용 기준 체력 (실제 플레이어 HP 연동은 후속)
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

        // 필요 재료
        UpdateReqItems(data);

        // 이 물약이 HP 계열의 마지막 단계면 MAX LEVEL (다음 등급으로 승급 로직은 후속)
        var nextData = GetData(_currentDataId + 1);
        bool isMaxLevel = nextData == null;
        _maxLvlTxt.gameObject.SetActive(isMaxLevel);

        // 제작의뢰에서 열었을 때만 [재료부족/제작]/[다음단계 정보] 표시
        bool showCraftUI = _fromCustomOrder && !isMaxLevel;
        _cantCraftBtn.SetActive(false); // 재료 체크 후 결정
        _craftBtn.SetActive(false);
        _nextLvlBtn.SetActive(false);
        _backBtn.SetActive(false);

        if (showCraftUI)
        {
            bool canCraft = HasEnoughMaterials(data);
            _craftBtn.SetActive(canCraft);
            _cantCraftBtn.SetActive(!canCraft);
            _nextLvlBtn.SetActive(true);
        }
    }

    void UpdateReqItems(HPPotionData data)
    {
        int n = data.NeededItemIDs != null ? data.NeededItemIDs.Count : 0;
        for (int i = 0; i < _reqItemBoxes.Length; i++)
        {
            var box = _reqItemBoxes[i];
            bool use = i < n;
            box.gameObject.SetActive(use);
            if (!use) continue;

            int itemId = data.NeededItemIDs[i];
            int required = data.NeededItemAmounts[i];
            int have = _inventoryStub.TryGetValue(itemId, out var h) ? h : 0;

            // Amount: 보유/필요. 부족하면 빨강.
            var amount = box.Find("AmountTxt")?.GetComponent<TMP_Text>();
            if (amount != null)
            {
                amount.text = $"{have}/{required}";
                amount.color = have >= required ? Color.white : new Color32(0xFF, 0x55, 0x55, 0xFF);
            }

            // 아이콘: ItemDic에 해당 DataId 있으면 SpriteName 기반 로드. 없으면 prefab default 유지.
            var img = box.Find("Img")?.GetComponent<Image>();
            if (img != null && img.gameObject.activeSelf == false)
                img.gameObject.SetActive(true);
            if (img != null && Managers.Data.ItemDic.TryGetValue(itemId, out var itemData))
            {
                var sp = Managers.Resource.Load<Sprite>(itemData.SpriteName);
                if (sp != null) img.sprite = sp;
            }
        }
    }

    /// <summary>모든 필요 재료를 충족하는지 체크 (스텁 인벤토리 기준).</summary>
    bool HasEnoughMaterials(HPPotionData data)
    {
        if (data == null || data.NeededItemIDs == null) return true;
        for (int i = 0; i < data.NeededItemIDs.Count; i++)
        {
            int itemId = data.NeededItemIDs[i];
            int required = data.NeededItemAmounts[i];
            int have = _inventoryStub.TryGetValue(itemId, out var h) ? h : 0;
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
            _reqItemList.SetActive(false);
            _reqItemTitle.SetActive(false);
            if (_reqDecorLine != null) _reqDecorLine.SetActive(false);
            _cantCraftBtn.SetActive(false);
            _craftBtn.SetActive(false);
            _nextLvlBtn.SetActive(false);
            _backBtn.SetActive(false);
            _maxLvlTxt.gameObject.SetActive(false);
            _descText.gameObject.SetActive(true);
            if (_switchBtnLabel != null) _switchBtnLabel.text = "필요재료";
        }
        else
        {
            _descText.gameObject.SetActive(false);
            _reqItemList.SetActive(true);
            _reqItemTitle.SetActive(true);
            if (_reqDecorLine != null) _reqDecorLine.SetActive(true);
            if (_switchBtnLabel != null) _switchBtnLabel.text = "설명";

            // 재료/버튼 상태는 ApplyData가 결정하므로 여기서 다시 재계산
            var data = GetData(_currentDataId);
            if (data != null)
            {
                var next = GetData(_currentDataId + 1);
                bool isMaxLevel = next == null;
                _maxLvlTxt.gameObject.SetActive(isMaxLevel);

                bool showCraftUI = _fromCustomOrder && !isMaxLevel;
                if (_previewNext)
                {
                    // S39 preview 상태: 제작/재료부족 숨김, 다음단계 → [뒤로]로 바뀜
                    _craftBtn.SetActive(false);
                    _cantCraftBtn.SetActive(false);
                    _nextLvlBtn.SetActive(false);
                    _backBtn.SetActive(showCraftUI);
                }
                else if (showCraftUI)
                {
                    bool canCraft = HasEnoughMaterials(data);
                    _craftBtn.SetActive(canCraft);
                    _cantCraftBtn.SetActive(!canCraft);
                    _nextLvlBtn.SetActive(true);
                    _backBtn.SetActive(false);
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
        var next = GetData(_currentDataId + 1);
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
        int maxHp = 1000;
        int pCur  = curr.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * curr.MaxHPRecov / 100f);
        int pNext = next.MaxHPRecov == 0 ? 0 : Mathf.RoundToInt(maxHp * next.MaxHPRecov / 100f);
        int totalCur  = curr.FlatRecov + pCur;
        int totalNext = next.FlatRecov + pNext;

        _flatHeal.text  = FmtStat("기본 회복량",       curr.FlatRecov,   next.FlatRecov);
        _pHeal.text     = FmtStat("최대 체력 회복량", curr.MaxHPRecov,  next.MaxHPRecov, suffix: "%");
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
        var next = GetData(_currentDataId + 1);
        if (next == null) return; // 최대 레벨은 제작 불가

        _craftBusy = true;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_myFrameRT.DOShakeAnchorPos(1.1f, strength: new Vector3(1.5f, 0f, 0f), vibrato: 21, randomness: 0f).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => SpawnCraftFX());
        seq.Append(_myFrameRT.DOPunchScale(new Vector3(0.2f, 0.2f, 1f), 0.3f, vibrato: 4, elasticity: 1f).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            _currentDataId = _currentDataId + 1;
            _previewNext = false;
            RefreshAll();
            _craftBusy = false;
        });
    }

    /// <summary>S40: 제작 시 FX 스폰 — 중앙 splash + 물약 아이콘 upgrade sparkle.</summary>
    void SpawnCraftFX()
    {
        // 팝업 중앙과 아이콘의 월드 좌표. Canvas(Screen Space - Camera)의 plane distance를 벗어나면
        // UI 뒤로 묻히므로 카메라 방향으로 z를 살짝 당겨준다.
        SpawnFXAt("FX_splash_spark_floor", _myFrameRT.position);
        SpawnFXAt("FX_splash_spark_air",   _potionImg.rectTransform.position);
    }

    void SpawnFXAt(string key, Vector3 worldPos, float lifetime = 2f)
    {
        var go = Managers.Resource.Instantiate(key);
        if (go == null) return;

        // UI canvas에 parent시키되 world position 유지. z를 카메라 쪽으로 당겨 UI 앞에 그리도록.
        go.transform.SetParent(_frame, worldPositionStays: true);
        go.transform.position = worldPos + new Vector3(0f, 0f, -1f); // UI 앞쪽

        // 자체 Canvas + overrideSorting으로 팝업 Canvas(sortingOrder=12)보다 위에 렌더.
        var canvas = go.AddComponent<UnityEngine.Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9999;

        // Particle renderer sortingOrder도 같이 상향.
        foreach (var psr in go.GetComponentsInChildren<UnityEngine.ParticleSystemRenderer>(true))
        {
            psr.sortingOrder = 9999;
        }

        // 명시적 Play + 즉시 Emit (prefab의 rateOverTime/burst 설정 미흡 방어).
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
            ps.Play(true);
            ps.Emit(30);
        }

        Destroy(go, lifetime);
    }

    void PunchBtnScale(Transform btn)
    {
        btn.DOKill();
        btn.localScale = Vector3.one;
        btn.DOScale(0.9f, 0.07f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
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
