using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

/// <summary>
/// 슬라이드 41~42: 물약창 Popup.
/// S41: 해금된 물약 슬롯 + 자동물약 On/Off + HP Ratio Slider(기억 전/후 분기) + 슬롯 클릭 → 상세 팝업
/// S42: 물약 사용 쿨타임 슬라이더(위→아래) + Finish Anim(alpha 80→0 InBack 0.3s) + 자동물약 조건 트리거(스텁)
/// 진입 애님: Move OutBack 0.5s, From (0, +40)
/// </summary>
public class UI_PotionPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        PotionBoxGroup,
        Title,
        HPPotionSlotFrame,
        HPPotionSlotBackground,
        HPPotionSlotPotionImg,
        Img_Fill_Area_03,
        HPPotionCooldownFinishAnim,
        SpeedPotionSlotFrame,
        SpeedPotionSlotBackground,
        SpeedPotionSlotPotionImg,
        Img_Fill_Area_02,
        SpeedPotionCooldownFinishAnim,
        Img_AutoBtn_01,
        Image_1,
        AutoBtnToggleImg,
        Background,
        Img_Fill_Area_01,
        Handle,
        AutoPotionGuageLock,
        Img_AutoPotion_Guage_Lock_02,
    }

    enum Texts
    {
        Txt_Title_02,
        HPPotionLeftTxt,
        Txt_Button_02,
        SpeedPotionLeftTxt,
        Txt_Button_1_01,
        Txt_AutoBtn_03,
        Txt_Slider_01,
        AutoBtnNewTxt,
    }

    enum Buttons
    {
        PotionPopupBackground,
        HPPotionSlot,
        Button,
        SpeedPotionSlot,
        Button_1,
        AutoBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        PotionPopupON,
        PotionLayout,
        HPPotionCooldownBar,
        Panel_HP_Potion_Cooldown_Bar_03,
        SpeedPotionCooldownBar,
        Panel_Speed_Potion_Cooldown_Ba_02,
        Slider,
        Panel_Slider_01,
        HandleSlideArea,
    }

    #endregion


    // ────────────────────────────────────────
    //  Slot 데이터
    // ────────────────────────────────────────
    class Slot
    {
        public string name;             // "HP" / "Speed"
        public Button button;
        public Image potionImg;
        public Slider cooldownBar;       // fill: 1 = 가득, 0 = 쿨타임 끝 (위→아래 감소)
        public GameObject finishAnim;    // Finish Anim 이미지(alpha 80 → 0)
        public Image finishAnimImg;
        public TMP_Text leftTxt;
        public int dataId;               // HPPotion/SPDPotion DataId (30001, 40001 등)
        public float cooldown;           // 초 단위 남은 쿨타임
        public float cooldownMax;        // 초기 쿨타임
        public int count;                // 보유 수량 (스텁)
        public bool unlocked;
        public Image lockBg;             // 잠금 오버레이 (runtime 생성)
        public Image lockIcon;           // Chat_Icon_Lock (runtime 생성)
    }

    // 잠금 비주얼 스펙(S35/S41 공통): Sprite None 000000 alpha 180/255, Chat_Icon_Lock FFFFFF alpha 80/255
    static readonly Color LOCK_BG_COLOR   = new Color(0f, 0f, 0f, 180f / 255f);
    static readonly Color LOCK_ICON_COLOR = new Color(1f, 1f, 1f,  80f / 255f);

    Slot _hpSlot;
    Slot _spdSlot;
    List<Slot> _slots = new List<Slot>();

    // ────────────────────────────────────────
    //  일반 캐시
    // ────────────────────────────────────────
    Transform _popupRoot;                // PotionPopupON
    Image _bg;                           // Potion Popup Background
    RectTransform _boxGroupRT;           // Potion Box Group (실제 프레임)
    TMP_Text _titleText;

    Button _autoBtn;                     // 자동물약 On/Off 토글
    Animator _autoToggleAnimator;        // AutoBtnToggleImg
    GameObject _autoBtnNewTxt;           // NEW 뱃지

    Slider _hpRatioSlider;               // HP Ratio (자동 트리거 기준)
    TMP_Text _hpRatioText;
    GameObject _autoGuageLock;           // 잠금 오버레이 (기억 전)

    Vector2 _boxOrigin;
    bool _autoOn;
    bool _autoMemorized;
    float _hpRatio = 0.5f;

    float _boxOffScreenY;  // 열릴 때 시작 / 닫힐 때 종료 y (화면 완전 밖)

    // Prefab의 anchoredPosition(0, -85)은 In_Frame 바닥 아래(오프스크린)라 보이지 않음.
    // 기획서 GIF 기준 화면 하단에 떠있는 모양 — In_Frame bottom에서 위로 120 정도.
    static readonly Vector2 BOX_ORIGIN = new Vector2(0f, 45f);

    const string PREF_AUTO_ON   = "PotionPopup.AutoOn";
    const string PREF_HP_RATIO  = "PotionPopup.HpRatio";
    const string PREF_MEMORIZED = "PotionPopup.Memorized";

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        #region Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        #endregion

        _popupRoot  = GetObject((int)GameObjects.PotionPopupON).transform;
        _bg         = GetButton((int)Buttons.PotionPopupBackground).GetComponent<Image>();
        _boxGroupRT = GetImage((int)Images.PotionBoxGroup).GetComponent<RectTransform>();
        _titleText  = GetText((int)Texts.Txt_Title_02);

        _frameRT = _boxGroupRT;

        // 슬롯 바인딩 (HP / Speed)
        _hpSlot  = BindHpSlot();
        _spdSlot = BindSpeedSlot();
        _slots.Add(_hpSlot);
        _slots.Add(_spdSlot);

        // AutoBtn
        _autoBtn            = GetButton((int)Buttons.AutoBtn);
        _autoToggleAnimator = GetImage((int)Images.AutoBtnToggleImg).GetComponent<Animator>();
        _autoBtnNewTxt      = GetText((int)Texts.AutoBtnNewTxt).gameObject;

        // HP Ratio Slider
        _hpRatioSlider = GetObject((int)GameObjects.Slider).GetComponent<Slider>();
        _hpRatioText   = GetText((int)Texts.Txt_Slider_01);
        _autoGuageLock = GetImage((int)Images.AutoPotionGuageLock).gameObject;

        // 이벤트 바인딩
        _bg.gameObject.BindEvent(OnClickClose);
        _autoBtn.onClick.AddListener(OnClickAutoToggle);
        foreach (var s in _slots)
            if (s != null && s.button != null)
                s.button.onClick.AddListener(() => OnClickSlot(s));

        _hpRatioSlider.onValueChanged.AddListener(OnHpRatioChanged);

        return true;
    }

    Slot BindHpSlot()
    {
        var s = new Slot { name = "HP" };
        s.button        = GetButton((int)Buttons.HPPotionSlot);
        s.potionImg     = GetImage((int)Images.HPPotionSlotPotionImg);
        s.cooldownBar   = GetObject((int)GameObjects.HPPotionCooldownBar).GetComponent<Slider>();
        s.finishAnimImg = GetImage((int)Images.HPPotionCooldownFinishAnim);
        s.finishAnim    = s.finishAnimImg.gameObject;
        s.leftTxt       = GetText((int)Texts.HPPotionLeftTxt);

        // 잠금 오버레이 + 패드락 아이콘 runtime 생성 (prefab에 미포함)
        CreateLockOverlay(s.button.transform, s);
        return s;
    }

    Slot BindSpeedSlot()
    {
        var s = new Slot { name = "Speed" };
        s.button        = GetButton((int)Buttons.SpeedPotionSlot);
        s.potionImg     = GetImage((int)Images.SpeedPotionSlotPotionImg);
        s.cooldownBar   = GetObject((int)GameObjects.SpeedPotionCooldownBar).GetComponent<Slider>();
        s.finishAnimImg = GetImage((int)Images.SpeedPotionCooldownFinishAnim);
        s.finishAnim    = s.finishAnimImg.gameObject;
        s.leftTxt       = GetText((int)Texts.SpeedPotionLeftTxt);

        CreateLockOverlay(s.button.transform, s);
        return s;
    }

    /// <summary>슬롯에 잠금 오버레이(검은 사각형) + 패드락 아이콘을 동적으로 생성.</summary>
    void CreateLockOverlay(Transform parent, Slot s)
    {
        // Lock BG (검은 오버레이, 슬롯 전체 덮음)
        var bgGo = new GameObject("LockBg", typeof(RectTransform));
        bgGo.transform.SetParent(parent, worldPositionStays: false);
        var bgRT = bgGo.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        s.lockBg = bgGo.AddComponent<Image>();
        s.lockBg.color = LOCK_BG_COLOR;
        s.lockBg.raycastTarget = false;

        // Lock Icon (Chat_Icon_Lock, 중앙)
        var iconGo = new GameObject("LockIcon", typeof(RectTransform));
        iconGo.transform.SetParent(parent, worldPositionStays: false);
        var iconRT = iconGo.GetComponent<RectTransform>();
        iconRT.anchorMin = iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = Vector2.zero;
        iconRT.sizeDelta = new Vector2(14f, 18f);
        s.lockIcon = iconGo.AddComponent<Image>();
        var lockSprite = Managers.Resource.Load<Sprite>("Chat_Icon_Lock");
        if (lockSprite != null) s.lockIcon.sprite = lockSprite;
        s.lockIcon.color = LOCK_ICON_COLOR;
        s.lockIcon.raycastTarget = false;
        s.lockIcon.preserveAspect = true;
    }

    // ────────────────────────────────────────
    //  Open / Close
    // ────────────────────────────────────────
    public void SetInfo()
    {
        // 저장값 복원
        _autoOn          = PlayerPrefs.GetInt(PREF_AUTO_ON, 0) == 1;
        _autoMemorized   = PlayerPrefs.GetInt(PREF_MEMORIZED, 0) == 1;
        _hpRatio         = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_HP_RATIO, 0.5f));

        if (_titleText != null) _titleText.text = "물약";

        // 슬롯 상태 (인벤토리 미연동 → HP만 해금 가정, 수량 5개)
        if (_hpSlot != null)
        {
            _hpSlot.unlocked = true;
            _hpSlot.dataId = 30001;
            _hpSlot.cooldownMax = 5f;
            _hpSlot.count = 5;
            _hpSlot.cooldown = 0f;
            RefreshSlotVisual(_hpSlot);
        }
        bool spdAnimNeeded = false;
        if (_spdSlot != null)
        {
            // SPD는 연금공방 NPC가 해금한 시점(SpdPotionRevealed=true)부터 사용 가능
            _spdSlot.unlocked = Managers.Game.SpdPotionRevealed;
            _spdSlot.dataId = Managers.Game.CurrentSpeedPotionDataId;
            _spdSlot.cooldownMax = 10f;
            _spdSlot.count = 0;
            _spdSlot.cooldown = 0f;
            RefreshSlotVisual(_spdSlot);

            // 최초 1회만 해금 애니 재생 — RefreshSlotVisual이 lockBg/lockIcon을 감췄으므로 다시 노출
            spdAnimNeeded = _spdSlot.unlocked && !Managers.Game.SpdPotionPopupAnimPlayed;
            if (spdAnimNeeded)
            {
                if (_spdSlot.lockBg  != null) { _spdSlot.lockBg .color = LOCK_BG_COLOR;   _spdSlot.lockBg .gameObject.SetActive(true); }
                if (_spdSlot.lockIcon!= null) { _spdSlot.lockIcon.color = LOCK_ICON_COLOR; _spdSlot.lockIcon.gameObject.SetActive(true); }
            }
        }

        // 자동물약 UI 갱신
        RefreshAutoUI();
        if (_autoBtnNewTxt != null) _autoBtnNewTxt.SetActive(false);

        // 진입 애님: 화면 완전 밖(아래) → BOX_ORIGIN. OutBack 0.5s.
        _boxOrigin = BOX_ORIGIN;
        var canvasRT = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        _boxOffScreenY = _boxOrigin.y - canvasRT.rect.height - _boxGroupRT.rect.height;
        _boxGroupRT.DOKill();
        _boxGroupRT.anchoredPosition = new Vector2(0f, _boxOffScreenY);
        _boxGroupRT.DOAnchorPos(_boxOrigin, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        // BlackBG 페이드
        if (_bg != null)
        {
            _bg.DOKill();
            Color c = _bg.color; c.a = 0f; _bg.color = c;
            _bg.DOFade(220f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        // SPD 해금 1회 연출 (물약 팝업 첫 진입)
        if (spdAnimNeeded)
            PlaySpdSlotRevealAnim();
    }

    /// <summary>
    /// SPD 포션 슬롯 해금 1회 연출.
    /// 팝업 박스 진입(0.5s)이 끝난 뒤 Shake+Scale+LockFade → 완료 시 플래그 저장.
    /// </summary>
    void PlaySpdSlotRevealAnim()
    {
        if (_spdSlot == null || _spdSlot.button == null) return;
        var slotTr = _spdSlot.button.transform;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        // 팝업 박스 OutBack 0.5s가 끝날 때까지 대기
        seq.AppendInterval(0.6f);
        seq.AppendCallback(() =>
        {
            slotTr.DOKill();
            slotTr.localScale = Vector3.one;
            slotTr.DOShakePosition(0.7f, new Vector3(3f, 3f, 0f), vibrato: 20, randomness: 60f).SetUpdate(true);
            slotTr.DOScale(1.15f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        });
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            slotTr.DOScale(1f, 0.2f).SetEase(Ease.InOutBack).SetUpdate(true);
            if (_spdSlot.lockBg  != null) _spdSlot.lockBg .DOFade(0f, 0.4f).SetUpdate(true);
            if (_spdSlot.lockIcon!= null) _spdSlot.lockIcon.DOFade(0f, 0.4f).SetUpdate(true);
        });
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() =>
        {
            if (_spdSlot.lockBg  != null) _spdSlot.lockBg .gameObject.SetActive(false);
            if (_spdSlot.lockIcon!= null) _spdSlot.lockIcon.gameObject.SetActive(false);
            RefreshSlotVisual(_spdSlot);
            Managers.Game.SpdPotionPopupAnimPlayed = true;
        });
    }

    void OnClickClose()
    {
        _boxGroupRT.DOKill();
        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_boxGroupRT.DOAnchorPos(new Vector2(0f, _boxOffScreenY), 0.25f).SetEase(Ease.InBack));
        seq.OnComplete(() => Managers.UI.ClosePopupUI());
    }

    // ────────────────────────────────────────
    //  슬롯 클릭 / 사용 / 쿨타임
    // ────────────────────────────────────────
    void OnClickSlot(Slot s)
    {
        if (s == null) return;
        if (!s.unlocked) return;              // 잠긴 슬롯
        if (s.cooldown > 0f) return;          // 쿨타임 중

        // 수량 체크 — 없으면 상세 팝업만 보여줌
        if (s.count <= 0)
        {
            OpenDetailFor(s);
            return;
        }

        // 실제 사용
        UsePotion(s);
    }

    void OpenDetailFor(Slot s)
    {
        if (s.name == "HP")
            Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(s.dataId, fromCustomOrder: false);
        // SPD/Luck 상세는 별도 팝업 필요 — 현재 UI_PotionDetailPopup이 HP 전용이므로 스킵
    }

    void UsePotion(Slot s)
    {
        s.count = Mathf.Max(0, s.count - 1);
        s.cooldown = s.cooldownMax;

        if (s.button != null) s.button.interactable = false;
        if (s.cooldownBar != null)
        {
            s.cooldownBar.gameObject.SetActive(true);
            s.cooldownBar.value = 1f; // 1=가득 → Update에서 0으로 감소 (fill 위→아래)
        }

        // 물약 사용 FX (캐릭터 몸 중앙) — 에셋 있으면 스폰. HP Potion Ef 프리팹 미확인이라 스킵 가능.
        SpawnUseFXForSlot(s);

        RefreshSlotVisual(s);
    }

    void SpawnUseFXForSlot(Slot s)
    {
        if (s == null || s.button == null) return;

        // S42: 물약 사용 시 즉각 사용된 물약에 해당되는 이펙트 표시.
        // 기획서 원본은 HP Potion Ef를 "캐릭터 몸 한 가운데"에 스폰하지만,
        // PlayerController HP 연동이 아직 없어서 우선 물약 슬롯 중앙에 UI-native FX 재생.
        var slotRT = s.button.GetComponent<RectTransform>();
        if (slotRT == null) return;

        // Box Group을 parent로 (Slot 내부에 parent하면 cooldown bar 등과 겹쳐서 레이어 꼬임)
        var parent = _boxGroupRT;
        Vector3 slotWorld = slotRT.position;
        Vector3 slotLocal = parent.InverseTransformPoint(slotWorld);
        Vector2 localPos = new Vector2(slotLocal.x, slotLocal.y);

        // 타입별 FX: HP는 Heal(녹색 플러스), Speed는 Sparkle(반짝이)
        var fx = (s.name == "HP") ? UIFx.FxType.Heal : UIFx.FxType.Sparkle;
        UIFx.PlayBurst(parent, localPos, fx);
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        // 쿨타임 진행
        foreach (var s in _slots)
        {
            if (s == null) continue;
            if (s.cooldown > 0f)
            {
                s.cooldown = Mathf.Max(0f, s.cooldown - Time.unscaledDeltaTime);
                if (s.cooldownBar != null)
                    s.cooldownBar.value = s.cooldown / s.cooldownMax;

                if (s.cooldown <= 0f)
                    OnCooldownFinished(s);
            }
        }

        // 자동물약 트리거 (HP 연동 미구현 → 스펙 스켈레톤만)
        // TODO: Managers.Game.Player 등에서 현재 HP/MaxHP 읽어 _hpRatio 이하일 때 UsePotion(_hpSlot)
    }

    void OnCooldownFinished(Slot s)
    {
        if (s.button != null) s.button.interactable = s.unlocked && s.count > 0;
        if (s.cooldownBar != null) s.cooldownBar.gameObject.SetActive(false);

        // S42 Finish Anim: Image (Sprite None, color FFFFFF alpha 80) → DOFade to 0 over 0.3s InBack
        if (s.finishAnim != null && s.finishAnimImg != null)
        {
            s.finishAnim.SetActive(true);
            Color c = s.finishAnimImg.color; c.a = 80f / 255f; s.finishAnimImg.color = c;
            s.finishAnimImg.DOKill();
            s.finishAnimImg.DOFade(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => s.finishAnim.SetActive(false));
        }

        RefreshSlotVisual(s);
    }

    void RefreshSlotVisual(Slot s)
    {
        if (s == null) return;
        if (s.button != null)
            s.button.interactable = s.unlocked && s.count > 0 && s.cooldown <= 0f;
        if (s.leftTxt != null)
            s.leftTxt.text = s.unlocked ? s.count.ToString() : "";
        if (s.potionImg != null)
            s.potionImg.color = s.unlocked ? Color.white : new Color(1f, 1f, 1f, 0.25f);
        if (s.cooldownBar != null)
            s.cooldownBar.gameObject.SetActive(s.cooldown > 0f);
        if (s.finishAnim != null && s.cooldown > 0f)
            s.finishAnim.SetActive(false);

        // 잠금 오버레이 + 패드락 아이콘: 미해금일 때만 표시
        if (s.lockBg != null)   s.lockBg.gameObject.SetActive(!s.unlocked);
        if (s.lockIcon != null) s.lockIcon.gameObject.SetActive(!s.unlocked);
    }

    // ────────────────────────────────────────
    //  자동물약 (S41/S42 토글)
    // ────────────────────────────────────────
    void OnClickAutoToggle()
    {
        if (!_autoMemorized)
        {
            // 아직 자동물약 미기억 — 토글 불가 (잠겨있음)
            return;
        }
        _autoOn = !_autoOn;
        PlayerPrefs.SetInt(PREF_AUTO_ON, _autoOn ? 1 : 0);
        PlayerPrefs.Save();
        RefreshAutoUI();
    }

    void RefreshAutoUI()
    {
        // AutoBtnToggleImg Animator state: "On" / "Off" 클립 가정 (prefab 설정에 의존)
        if (_autoToggleAnimator != null)
            _autoToggleAnimator.Play(_autoOn ? "On" : "Off", 0, 0f);

        // 슬라이더: 기억 전엔 잠금, 기억 후엔 조절 가능
        if (_hpRatioSlider != null)
        {
            _hpRatioSlider.interactable = _autoMemorized;
            _hpRatioSlider.value = _hpRatio;
        }
        if (_autoGuageLock != null)
            _autoGuageLock.SetActive(!_autoMemorized);
        if (_hpRatioText != null)
            _hpRatioText.text = _autoMemorized ? $"{Mathf.RoundToInt(_hpRatio * 100)}%" : "";
    }

    void OnHpRatioChanged(float v)
    {
        if (!_autoMemorized) return;
        _hpRatio = v;
        PlayerPrefs.SetFloat(PREF_HP_RATIO, _hpRatio);
        if (_hpRatioText != null)
            _hpRatioText.text = $"{Mathf.RoundToInt(_hpRatio * 100)}%";
    }

    /// <summary>외부에서 "자동물약 기억"(물약 주입기 스킬 해금) 호출용.</summary>
    public void MarkAutoPotionMemorized()
    {
        _autoMemorized = true;
        PlayerPrefs.SetInt(PREF_MEMORIZED, 1);
        if (_autoBtnNewTxt != null) _autoBtnNewTxt.SetActive(true);
        RefreshAutoUI();
    }
}
