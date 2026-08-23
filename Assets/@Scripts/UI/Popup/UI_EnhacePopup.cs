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

        RefreshPreview(data);

        // 강화 가능 여부
        GetButton((int)Buttons.Enhance_Btn).interactable =
            !_resolving && Managers.Game.CanEnhance(_equip, _scrollId);
    }

    // ────────────────────────────────────────
    //  강화 성공 시 능력치 변화 (미리보기)
    // ────────────────────────────────────────
    //  영상에는 강화를 누르기 **전에** "강화 성공 시 / 능력치 변화" 블록이 떠서, 무엇이 얼마나
    //  오르는지 보고 결정할 수 있다. 우리 강화창엔 이 화면이 통째로 없었다 —
    //  확률과 내구도 손실만 보이고 정작 얻는 것이 안 보였다.
    //  영상처럼 **변하는 스탯에만** 화살표를 붙이고, 안 변하는 것은 현재 값만 적는다.
    TMPro.TMP_Text _previewTxt;

    void RefreshPreview(Data.EquipmentData data)
    {
        if (_previewTxt == null)
        {
            // 확률 텍스트를 복제해 서식을 물려받되, 자리는 **강화 버튼 아래** 빈 공간에 잡는다.
            //  확률 아래에 두었더니 버튼과 [영상 건너뛰기] 토글 위에 겹쳐 찍혔다.
            var anchor = GetText((int)Texts.ChanceTxt);
            if (anchor == null) return;
            var go = Instantiate(anchor.gameObject, anchor.transform.parent);
            go.name = "Txt_EnhancePreview";
            _previewTxt = go.GetComponent<TMPro.TMP_Text>();

            var rt = _previewTxt.rectTransform;
            var btn = GetButton((int)Buttons.Enhance_Btn);
            var btnRt = btn != null ? btn.transform as RectTransform : null;
            if (btnRt != null && btnRt.parent == rt.parent)
                rt.anchoredPosition = btnRt.anchoredPosition + new Vector2(0f, -34f);
            else
                rt.anchoredPosition = anchor.rectTransform.anchoredPosition + new Vector2(0f, -120f);
            rt.sizeDelta = new Vector2(120f, 0f);
            _previewTxt.fontSize = Mathf.Max(5f, anchor.fontSize * 0.62f);
            _previewTxt.alignment = TMPro.TextAlignmentOptions.Top;
            _previewTxt.color = Color.white;
        }

        bool maxed = _equip.EnhanceLevel >= ENHANCE_MAX_LEVEL;
        if (maxed) { _previewTxt.gameObject.SetActive(false); return; }
        _previewTxt.gameObject.SetActive(true);

        var cur  = Managers.Game.GetEnhanceBonus(_equip.DataId, _equip.EnhanceLevel);
        var next = Managers.Game.GetEnhanceBonus(_equip.DataId, _equip.EnhanceLevel + 1);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=#F5DD6C>{Util.Localize("EnhancePreviewText")}</color>");
        sb.AppendLine(Row(Util.Localize(LocalizeKey.EnhanceLevelText),
                          $"+{_equip.EnhanceLevel}", $"+{_equip.EnhanceLevel + 1}", true));
        sb.AppendLine(Row(Util.Localize(LocalizeKey.PhysicalAttackText),
                          $"+{data.BaseAtk + cur.atk}", $"+{data.BaseAtk + next.atk}", next.atk != cur.atk));
        sb.AppendLine(Row(Util.Localize(LocalizeKey.CriticalText),
                          $"+{data.BaseCritRate + cur.crit:0.#}%", $"+{data.BaseCritRate + next.crit:0.#}%",
                          !Mathf.Approximately(next.crit, cur.crit)));
        sb.Append(Row(Util.Localize(LocalizeKey.MagicAttackText),
                      $"+{data.BaseMagicAtk + cur.magic}", $"+{data.BaseMagicAtk + next.magic}",
                      next.magic != cur.magic));

        _previewTxt.text = sb.ToString();
        ApplyFont(_previewTxt, FontType.UI);
    }

    // 변하는 줄만 "현재 ▷ 이후", 안 변하면 현재 값만 (영상과 같은 규칙)
    static string Row(string name, string before, string after, bool changed)
        => changed
            ? $"{name}  {before} <color=#FF4C4C>▷</color> <color=#00FF00>{after}</color>"
            : $"{name}  {before}";

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

        // 망치 소리는 여기서 한 번 울리지 않는다 — 클립(Forge Smith/Success.anim)이
        //  t=0.08 / 1.08 / 2.08 에 SmashSound 이벤트를 갖고 있어 **타격마다** 울린다.
        //  (원본 영상도 1.0초 간격 3연타다. 예전엔 여기 한 번뿐이라 첫 타만 소리가 났다)

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

        HookSmithEvents();

        var smith = GetObject((int)GameObjects.HammerNormalAnim);
        var anim = smith != null ? smith.GetComponent<Animator>() : null;
        float wait = 0.4f;

        if (anim != null)
        {
            string state = result.Success ? "Success" : "Fail";
            _ceremony = true;
            anim.Play(state, 0, 0f);
            foreach (var c in anim.runtimeAnimatorController.animationClips)
                if (c.name == state) { wait = c.length; break; }
        }

        // 결과 섬광은 클립의 Success/Fail 이벤트가 정확한 시점에 터뜨린다(OnSmithAnimEvent).
        //  여기서는 모션이 끝날 때까지만 기다린다.
        yield return new WaitForSecondsRealtime(wait);

        _ceremony = false;                             // 대기 자세의 망치 소리는 듣지 않는다
        if (anim != null) anim.Play("Normal", 0, 0f);   // 대기 자세로 복귀
        _resolving = false;

        Managers.UI.ShowPopupUI<UI_EnhanceProgressPopup>().SetInfo(result, _equip);
        RefreshUI();
    }

    #region 강화 연출 대본 (클립 AnimationEvent)

    // 대장간 강화 클립은 stringParameter로 **연출 대본**을 들고 있다.
    //  원본(비주얼 스크립팅)의 그래프를 그대로 옮긴 것 — 이름/시각은 클립이 정한다.
    //
    //    Success.anim(5.02s)  SmashSound 0.08 / Curious 0.25 / Shake 0.33
    //                         SmashSound 1.08 / Shake 1.33
    //                         SmashSound 2.08 / ChargeSuccess 2.33
    //                         Serious 3.75 / Success 3.83 / SuccessDone 5.00
    //    Fail.anim(5.02s)     … ChargeFail 2.33 / Serious 3.75 / Fail 3.85
    //                         FailSound 3.90 / FailDone 5.00
    //
    //  소리 이름은 원본 그래프의 PlaySFX 인자를 우리 클립으로 옮긴 것이다.
    static readonly (string ev, string sfx)[] EVENT_SFX =
    {
        ("SmashSound",  "SFX_EnhanceHammer"),     // 망치 타격 (3연타)
        ("Curious",     "SFX_CuriousCaution2"),   // 첫 타 직후 — 대장장이가 살피는 소리
        ("Serious",     "SFX_Serious"),           // 마지막 담금질 직전 긴장
        ("Success",     "SFX_Success(Base)"),     // 성공 폭발 (영상에서 가장 큰 소리)
        ("FailSound",   "SFX_Fail"),
    };

    // 실패 마무리의 불꽃 소리는 이벤트와 **동시에** 나지 않는다.
    //  원본 그래프에서 FailDone -> Timer(0.3) -> ... -> Timer(1.1) -> Timer(0.2) -> PlaySFX(FlameOn)
    //  이라 합계 1.6초 뒤다. 담금질한 쇠를 물에 넣는 타이밍이라 이 지연이 곧 연출이다.
    const float FLAME_DELAY = 1.6f;

    AnimEventSink _smithSink;

    // 대기 자세(Normal.anim)에도 SmashSound가 3개 박혀 있다 — 대장장이가 늘 망치질하는
    //  루프라서다. 그대로 두면 강화창을 열어 둔 내내 망치 소리가 계속 난다(원본 영상은 조용하다).
    //  **강화 연출(Success/Fail) 중에만** 대본을 듣는다.
    bool _ceremony;

    /// <summary>망치 애니메이터의 이벤트를 구독한다. 중복 구독은 하지 않는다.</summary>
    void HookSmithEvents()
    {
        var smith = GetObject((int)GameObjects.HammerNormalAnim);
        if (smith == null) return;

        var sink = smith.GetComponent<AnimEventSink>();
        if (sink == null) sink = smith.AddComponent<AnimEventSink>();
        if (ReferenceEquals(sink, _smithSink)) return;   // 이미 이 sink를 듣고 있다

        if (_smithSink != null) _smithSink.OnAnimEvent -= OnSmithAnimEvent;
        _smithSink = sink;
        _smithSink.OnAnimEvent += OnSmithAnimEvent;
    }

    void OnSmithAnimEvent(string id)
    {
        if (!_ceremony || string.IsNullOrEmpty(id)) return;

        foreach (var (ev, sfx) in EVENT_SFX)
            if (ev == id) { Managers.Sound?.PlaySFX(sfx); break; }

        switch (id)
        {
            case "FailDone":
                DOVirtual.DelayedCall(FLAME_DELAY, () => Managers.Sound?.PlaySFX("SFX_FlameOn"))
                         .SetUpdate(true);
                break;

            // 타격 — 흔들림 + 모루 위 섬광/불티. 원본도 Shake에서 FX 오브젝트를 켠다.
            case "Shake":
                ShakeFrame(0.22f, 6f);
                PopImpactFx();
                break;

            case "ShakeLast":
                ShakeFrame(0.30f, 8f);
                PopImpactFx();
                break;

            case "ChargeSuccess":                                    // 담금질 — 길게 떨린다
            case "ChargeFail":
                ShakeFrame(0.90f, 4f);
                break;

            // 결과 섬광은 **이 순간**이다(클립 t=3.83). 예전엔 코루틴이 "클립길이-0.5"로
            //  어림잡아 터뜨려 0.7초쯤 늦었다 — 소리(Success)와 빛이 어긋나 보였다.
            case "Success":
                ShakeFrame(0.45f, 12f);
                PopResultFlash(true);
                break;

            case "Fail":
                ShakeFrame(0.35f, 8f);
                PopResultFlash(false);
                break;
        }
    }

    /// <summary>타격 순간의 섬광·불티. 껐다 켜야 authored 연출이 처음부터 다시 돈다.</summary>
    void PopImpactFx()
    {
        foreach (string n in new[] { "Flash", "GlowDot_01_1" })
        {
            var go = Util.FindChild(gameObject, n, true);
            if (go == null) continue;
            go.SetActive(false);
            go.SetActive(true);
        }
    }

    /// <summary>성공/실패 전체화면 섬광(Animator "Flash").</summary>
    void PopResultFlash(bool success)
    {
        var flash = GetObject(success ? (int)GameObjects.Flash_Success : (int)GameObjects.Flash_Fail);
        if (flash == null) return;

        flash.SetActive(false);
        flash.SetActive(true);
        DOVirtual.DelayedCall(FLASH_HOLD, () => { if (flash != null) flash.SetActive(false); })
                 .SetUpdate(true);
    }

    const float FLASH_HOLD = 0.5f;

    /// <summary>강화창 프레임을 흔든다. 원본의 HammerSmash / HammerSmashLong 자리.
    /// 위치를 되돌려 놓고 끝내야 다음 연출이 어긋난 자리에서 시작하지 않는다.</summary>
    void ShakeFrame(float duration, float strength)
    {
        var rt = _frameRT != null ? _frameRT : GetObject((int)GameObjects.EnhacePopup)?.GetComponent<RectTransform>();
        if (rt == null) return;

        Vector2 home = rt.anchoredPosition;
        rt.DOKill();
        rt.DOShakeAnchorPos(duration, strength, 18, 90f, false, false)
          .SetEase(Ease.OutQuad)
          .SetUpdate(true)
          .OnComplete(() => { if (rt != null) rt.anchoredPosition = home; });
    }

    #endregion

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
