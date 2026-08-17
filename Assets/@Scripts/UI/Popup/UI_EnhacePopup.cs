using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 강화 실행 팝업(모루/망치 씬) — UI_ForgePopup에서 주문서 선택 후 [강화]로 진입.
/// 성공확률/실패 리스크(내구도 감소량)/잠재력 게이지 표시 + [강화] 실행.
///  · [영상 건너뛰기] 체크: 결과 플래시(Flash_Success/Fail)만 재생하고 팝업 유지(연속 강화).
///  · 미체크: UI_EnhanceProgressPopup(결과 연출)로 진행.
/// 잠재 슬롯(PotenSelectPopup)/주문서 재선택(EnhScrollSelectPopup)/SOM(돌파)은 이번 스코프 제외.
/// </summary>
public class UI_EnhacePopup : UI_Popup
{
    #region Enum

    enum Images
    {
        WIcon,                  // 장비 아이콘 (CraftEquipIcon 하위)
    }

    enum Texts
    {
        ENumber,                // "+N"
        PotenForceBarBarText,   // 잠재력 수치
        ChanceTxt,              // 성공확률 "99.999%"
        RiskNum,                // 실패 시 내구도 감소량
        EnhEquipNameText,       // "+N 장비명"
        DurabilityText,         // "내구도" 라벨
        DurabilityRealText,     // "cur/max"
    }

    enum Buttons
    {
        Enhance_Btn,
        EnhancePopupCloseBtn,   // 투명 전체화면 닫기 (prefab interactable=False → 코드 활성)
    }

    enum GameObjects
    {
        EnhacePopup,            // 프레임 루트
        PotenForceBarGroup,     // 잠재력 슬라이더
        Holder,                 // 슬라이더 Fill 홀더 (prefab INACTIVE → 활성)
        PotenSelectPopup,       // 잠재 슬롯 선택 — 스코프 제외(비활성)
        EnhScrollSelectPopup,   // 주문서 재선택 — 스코프 제외(ForgePopup에서 선택 완료)
        PotentialBox,           // 잠재 "+" 박스 — 스코프 제외
        Btn_EnhacePopup_01,     // 주문서 "+" 박스 — 스코프 제외
        RiskGroup,
        text,                   // RiskGroup 라벨 "필요/실패 시 내구도 감소량" (prefab INACTIVE)
        isSkipBtn,              // 영상 건너뛰기 Toggle
        Flash_Success,
        Flash_Fail,
        HammerNormalAnim,
    }

    #endregion

    Equipment _equip;
    int _scrollId;
    Toggle _skipToggle;
    bool _resolving;   // 강화 연출 중 중복 클릭 방지

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 이번 스코프 제외 요소 정리
        GetObject((int)GameObjects.PotenSelectPopup)?.SetActive(false);
        GetObject((int)GameObjects.EnhScrollSelectPopup)?.SetActive(false);
        GetObject((int)GameObjects.PotentialBox)?.SetActive(false);
        GetObject((int)GameObjects.Btn_EnhacePopup_01)?.SetActive(false);
        GetObject((int)GameObjects.Holder)?.SetActive(true);
        GetObject((int)GameObjects.text)?.SetActive(true);

        GetButton((int)Buttons.Enhance_Btn).onClick.AddListener(OnClickEnhance);

        var closeBtn = GetButton((int)Buttons.EnhancePopupCloseBtn);
        closeBtn.interactable = true;
        closeBtn.onClick.AddListener(OnClickClose);

        _skipToggle = GetObject((int)GameObjects.isSkipBtn)?.GetComponent<Toggle>();

        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged += OnEquipInfoChanged;

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged -= OnEquipInfoChanged;
    }

    void OnEquipInfoChanged()
    {
        if (_equip != null) RefreshUI();
    }

    // ────────────────────────────────────────
    //  SetInfo
    // ────────────────────────────────────────
    public void SetInfo(Equipment equip, int scrollId)
    {
        _equip = equip;
        _scrollId = scrollId;
        if (_equip == null) return;
        RefreshUI();
    }

    public override void RefreshUI()
    {
        if (_equip == null || _equip.EquipmentData == null) return;
        var data = _equip.EquipmentData;

        // 장비 아이콘 — EquipIcon Animator가 sprite를 덮어쓰므로 코드 세팅 시 정지 (ForgePopup과 동일 처리)
        var wIcon = GetImage((int)Images.WIcon);
        Sprite sp = Managers.Resource.Load<Sprite>(data.SpriteName);
        if (wIcon != null && sp != null)
        {
            var animator = wIcon.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            wIcon.sprite = sp;
            // 애니메이터가 먹여 둔 회전/스케일이 그대로 굳는다(대장간에서 아이콘이 z=90으로 누워 있었다).
            wIcon.rectTransform.localRotation = Quaternion.identity;
            wIcon.rectTransform.localScale = Vector3.one;
        }

        GetText((int)Texts.ENumber).text = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel}" : "";

        string nm = Util.Localize(data.Name);
        var nameTxt = GetText((int)Texts.EnhEquipNameText);
        nameTxt.gameObject.SetActive(true);
        nameTxt.text = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel} {nm}" : nm;
        ApplyFont(nameTxt, FontType.UI);

        // 내구도
        GetText((int)Texts.DurabilityText).gameObject.SetActive(true);
        var duriTxt = GetText((int)Texts.DurabilityRealText);
        duriTxt.gameObject.SetActive(true);
        duriTxt.text = $"{_equip.Durability}/{data.MaxDurability}";

        // 성공확률 / 리스크
        bool maxed = _equip.EnhanceLevel >= ENHANCE_MAX_LEVEL;
        GetText((int)Texts.ChanceTxt).text = maxed ? "MAX" : $"{Managers.Game.GetEnhanceChance(_equip):F3}%";

        int idx = System.Array.IndexOf(ENHANCE_SCROLL_IDS, _scrollId);
        var riskTxt = GetText((int)Texts.RiskNum);
        riskTxt.gameObject.SetActive(true);
        riskTxt.text = (idx >= 0 ? ENHANCE_SCROLL_DURI_LOSS[idx] : ENHANCE_SCROLL_DURI_LOSS[0]).ToString();

        // 잠재력 게이지 — 만충(ENHANCE_POTEN_FULL) 기준 비율
        var barGroup = GetObject((int)GameObjects.PotenForceBarGroup);
        var slider = barGroup != null ? barGroup.GetComponent<Slider>() : null;
        if (slider != null) slider.value = Mathf.Clamp01(_equip.Potential / ENHANCE_POTEN_FULL);
        GetText((int)Texts.PotenForceBarBarText).text = _equip.Potential.ToString();

        // 강화 가능 여부
        GetButton((int)Buttons.Enhance_Btn).interactable =
            !_resolving && Managers.Game.CanEnhance(_equip, _scrollId);
    }

    // ────────────────────────────────────────
    //  강화 실행
    // ────────────────────────────────────────
    void OnClickEnhance()
    {
        if (_resolving || _equip == null) return;

        // 골드 부족은 버튼만 막으면 이유를 알 수 없다 — 필요 금액을 알려준다.
        int cost = Managers.Game.GetEnhanceCost(_equip);
        if (Managers.Game.Gold < cost)
        {
            Managers.UI.ShowToast($"골드가 부족합니다\n강화 비용 {cost:N0} (보유 {Managers.Game.Gold:N0})");
            return;
        }
        if (!Managers.Game.CanEnhance(_equip, _scrollId)) return;

        var result = Managers.Game.EnhanceEquipment(_equip, _scrollId);
        if (result == null) return;

        Managers.Sound?.PlaySFX("SFX_EnhanceHammer");   // 망치질

        bool skip = _skipToggle != null && _skipToggle.isOn;
        if (skip)
        {
            // 결과 플래시만 — 팝업 유지, 연속 강화 가능
            StartCoroutine(CoSkipFlash(result.Success));
        }
        else
        {
            // 대장장이 모션(성공/실패)을 보여준 뒤 결과 창으로 넘긴다.
            //  ⚠️ 프리팹에 Forge Smith 애니메이터(Normal/Success/Fail 3.0~5.0초)가 저작돼 있는데
            //     **코드가 한 번도 재생하지 않았다** — 강화를 눌러도 망치질 한 번 없이 결과창만 떴다.
            StartCoroutine(CoSmithMotion(result));
        }
        RefreshUI();   // 주문서 소진/확률/내구도/잠재력 즉시 갱신
    }

    /// 대장장이 모션 → 결과 창. 모션 도중에는 중복 클릭을 막는다.
    IEnumerator CoSmithMotion(GameManager.EnhanceResult result)
    {
        _resolving = true;

        var smith = GetObject((int)GameObjects.HammerNormalAnim);
        var anim = smith != null ? smith.GetComponent<Animator>() : null;
        float wait = 0.4f;

        if (anim != null)
        {
            string state = result.Success ? "Success" : "Fail";
            anim.Play(state, 0, 0f);
            foreach (var c in anim.runtimeAnimatorController.animationClips)
                if (c.name == state) { wait = c.length; break; }
        }

        // 모션 끝머리에 결과 플래시를 겹쳐 타격감을 준다.
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, wait - 0.5f));
        var flash = GetObject(result.Success ? (int)GameObjects.Flash_Success : (int)GameObjects.Flash_Fail);
        if (flash != null) { flash.SetActive(false); flash.SetActive(true); }
        yield return new WaitForSecondsRealtime(0.5f);
        if (flash != null) flash.SetActive(false);

        if (anim != null) anim.Play("Normal", 0, 0f);   // 대기 자세로 복귀
        _resolving = false;

        Managers.UI.ShowPopupUI<UI_EnhanceProgressPopup>().SetInfo(result, _equip);
        RefreshUI();
    }

    IEnumerator CoSkipFlash(bool success)
    {
        _resolving = true;
        var flash = GetObject(success ? (int)GameObjects.Flash_Success : (int)GameObjects.Flash_Fail);
        if (flash != null)
        {
            flash.SetActive(false);
            flash.SetActive(true);   // Animator(Flash) 재생
        }
        yield return new WaitForSecondsRealtime(0.6f);
        if (flash != null) flash.SetActive(false);
        _resolving = false;
        RefreshUI();
    }

    void OnClickClose()
    {
        if (_isClosing || _resolving) return;
        _isClosing = true;
        Managers.UI.ClosePopupUI();
    }
}
