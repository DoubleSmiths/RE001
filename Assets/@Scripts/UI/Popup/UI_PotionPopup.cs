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

    bool _autoOn;
    bool _autoMemorized;
    float _hpRatio = 0.5f;

    // PotionBoxGroup의 등장/퇴장 DOTweenAnimation (prefab: Move (0,-85)→(0,40) OutBack 0.5s, autoKill=False).
    //  열기 = DORestart(정방향), 닫기 = DOPlayBackwards(역재생).
    DG.Tweening.DOTweenAnimation _boxAnim;

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

        // PotionBoxGroup 등장/퇴장 DOTweenAnimation 캐시. 팝업 중 timeScale 0이어도 재생되도록 unscaled로.
        _boxAnim = GetImage((int)Images.PotionBoxGroup).GetComponent<DOTweenAnimation>();
        if (_boxAnim != null) _boxAnim.isIndependentUpdate = true;

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

        // 슬롯 하단 [정보] 버튼 → 상세 팝업(설명 전용 모드)
        GetButton((int)Buttons.Button).onClick.AddListener(() => OnClickInfo(_hpSlot));
        GetButton((int)Buttons.Button_1).onClick.AddListener(() => OnClickInfo(_spdSlot));

        _hpRatioSlider.onValueChanged.AddListener(OnHpRatioChanged);

        if (Managers.Game != null)
            Managers.Game.OnLanguageChanged += RefreshTexts;

        // 자동물약(PlayerController 상시 처리)이 발동하면 팝업이 열려 있을 때 HP 슬롯 쿨타임 바 동기화
        if (Managers.Game.Player != null)
            Managers.Game.Player.OnAutoPotionUsed += HandleAutoPotionUsed;

        return true;
    }

    void OnDestroy()
    {
        if (Managers.Game != null && Managers.Game.Player != null)
            Managers.Game.Player.OnAutoPotionUsed -= HandleAutoPotionUsed;
        if (Managers.Game != null)
            Managers.Game.OnLanguageChanged -= RefreshTexts;
    }

    // 자동 사용이 발동했을 때 팝업 HP 슬롯 쿨타임 바/버튼 상태를 맞춰준다(회복은 PlayerController가 이미 처리).
    void HandleAutoPotionUsed()
    {
        if (_hpSlot == null) return;
        _hpSlot.count = Mathf.Max(0, _hpSlot.count - 1);
        _hpSlot.cooldown = _hpSlot.cooldownMax;
        if (_hpSlot.button != null) _hpSlot.button.interactable = false;
        if (_hpSlot.cooldownBar != null) { _hpSlot.cooldownBar.gameObject.SetActive(true); _hpSlot.cooldownBar.value = 1f; }
        // FX(HP Potion Ef)는 PlayerController가 자동 사용 시 이미 생성 — 여기서 중복 생성 안 함.
        RefreshSlotVisual(_hpSlot);
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
        _autoOn        = Managers.Game.AutoPotionOn;
        _autoMemorized = Managers.Game.AutoPotionMemorized;
        _hpRatio       = Managers.Game.AutoPotionHpRatio;

        RefreshTexts();

        // 슬롯 상태 (인벤토리 미연동 → HP만 해금 가정, 수량 5개)
        if (_hpSlot != null)
        {
            _hpSlot.unlocked = true;
            _hpSlot.dataId = Managers.Game.CurrentHPPotionDataId;
            _hpSlot.cooldownMax = GetHpCooldown(_hpSlot.dataId);
            _hpSlot.count = 5;
            // 진행 중인 자동물약 쿨타임을 반영(쿨타임 도중 팝업을 열어도 바가 맞게 표시)
            var pl = Managers.Game.Player;
            _hpSlot.cooldown = pl != null ? pl.AutoPotionCooldownNormalized * _hpSlot.cooldownMax : 0f;
            RefreshSlotVisual(_hpSlot);
        }
        bool spdAnimNeeded = false;
        if (_spdSlot != null)
        {
            // SPD는 연금공방 NPC가 해금한 시점(SpdPotionRevealed=true)부터 사용 가능
            _spdSlot.unlocked = Managers.Game.SpdPotionRevealed;
            _spdSlot.dataId = Managers.Game.CurrentSpeedPotionDataId;
            _spdSlot.cooldownMax = GetSpdCooldown(_spdSlot.dataId);
            _spdSlot.count = Managers.Game.SpdPotionRevealed ? 5 : 0;
            // 진행 중인 쿨타임 복원 — 팝업을 껐다 켜도 남은 쿨타임 유지 (HP와 동일)
            var plSpd = Managers.Game.Player;
            _spdSlot.cooldown = plSpd != null ? plSpd.SpdPotionCooldownNormalized * _spdSlot.cooldownMax : 0f;
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

        // 등장 애님: PotionBoxGroup 컴포넌트의 DOTweenAnimation(Move 슬라이드인)을 재생. (코드 트윈 사용 안 함)
        _boxAnim?.DORestart();

        // 배경(PotionPopupBackground)은 항상 투명 유지 — 어두운 배경 표시 안 함.
        //  (클릭 닫기를 위해 raycastTarget은 prefab 설정 그대로 유지)
        if (_bg != null)
        {
            _bg.DOKill();
            Color c = _bg.color; c.a = 0f; _bg.color = c;
        }

        // SPD 해금 1회 연출 (물약 팝업 첫 진입)
        if (spdAnimNeeded)
            PlaySpdSlotRevealAnim();
    }

    /// <summary>
    /// SPD 포션 슬롯 해금 1회 연출 — LockIcon 전용.
    /// 타임라인:
    ///   t=0.0~0.5  대기 (Delay)
    ///   t=0.5~1.2  Shake/Position (Duration 0.7, Strength (1,1,0), Vibrato 48, Randomness 59, OutQuad)
    ///   t=1.2~1.7  Scale 1→2 (Duration 0.5, OutBack)
    ///   t=1.7~2.1  Lock overlay Fade out
    /// </summary>
    void PlaySpdSlotRevealAnim()
    {
        if (_spdSlot == null || _spdSlot.lockIcon == null) return;
        var lockIconTr = _spdSlot.lockIcon.rectTransform;
        Vector2 lockOriginPos = lockIconTr.anchoredPosition;

        // 초기 상태 정리
        lockIconTr.DOKill();
        lockIconTr.localScale = Vector3.one;
        lockIconTr.anchoredPosition = lockOriginPos;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // Delay 0.5s
        seq.AppendInterval(0.5f);

        // Shake/Position — 단독 재생
        seq.AppendCallback(() =>
        {
            lockIconTr
                .DOShakeAnchorPos(0.7f, new Vector3(1f, 1f, 0f), vibrato: 48, randomness: 59f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        });
        seq.AppendInterval(0.7f);

        // Scale 1→2
        seq.AppendCallback(() =>
        {
            lockIconTr
                .DOScale(2f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        });
        seq.AppendInterval(0.5f);

        // Lock overlay Fade out
        seq.AppendCallback(() =>
        {
            if (_spdSlot.lockBg != null) _spdSlot.lockBg.DOFade(0f, 0.4f).SetUpdate(true);
            _spdSlot.lockIcon.DOFade(0f, 0.4f).SetUpdate(true);
        });
        seq.AppendInterval(0.4f);

        seq.AppendCallback(() =>
        {
            if (_spdSlot.lockBg != null) _spdSlot.lockBg.gameObject.SetActive(false);
            _spdSlot.lockIcon.gameObject.SetActive(false);
            // 다음 번 잠금 상태 재사용을 위한 원복
            lockIconTr.anchoredPosition = lockOriginPos;
            lockIconTr.localScale = Vector3.one;
            RefreshSlotVisual(_spdSlot);
            Managers.Game.SpdPotionPopupAnimPlayed = true;
        });
    }

    bool _closing;
    void OnClickClose()
    {
        if (_closing) return;
        _closing = true;

        // 닫기: 등장 DOTweenAnimation을 역재생(rewind)한 뒤 팝업 닫기.
        if (_boxAnim != null)
        {
            _boxAnim.DOPlayBackwards();
            DG.Tweening.DOVirtual.DelayedCall(_boxAnim.duration, () => Managers.UI.ClosePopupUI(), ignoreTimeScale: true);
        }
        else
        {
            Managers.UI.ClosePopupUI();
        }
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

    // [정보] 버튼 / 수량 없는 슬롯 클릭 → 상세 팝업(설명 전용). 미해금 슬롯은 무반응.
    // UI_PotionDetailPopup은 DataId 범위(40000+=SPD)로 종류를 자동 판정하므로 HP/SPD 공용.
    void OnClickInfo(Slot s)
    {
        if (s == null || !s.unlocked) return;
        Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(s.dataId, fromCustomOrder: false);
    }

    void OpenDetailFor(Slot s) => OnClickInfo(s);

    // 언어별 정적 텍스트 갱신 — 타이틀 / [정보] 버튼 라벨.
    // (SPD 쪽 프리팹 라벨이 "INFO"로 저작돼 있어 언어 전환이 안 되던 문제 — 코드에서 일괄 세팅.
    //  '정보' 전용 LocalizeKey가 시트에 없어 코드 분기. 키 추가 시 Localize로 교체할 것.)
    void RefreshTexts()
    {
        if (_titleText != null) _titleText.text = Util.Localize(Define.LocalizeKey.BtmPotionBtnText);
        string infoLabel = Managers.Game.Language == Define.Language.Korean ? "정보" : "INFO";
        var hpInfoTxt  = GetText((int)Texts.Txt_Button_02);
        var spdInfoTxt = GetText((int)Texts.Txt_Button_1_01);
        if (hpInfoTxt  != null) hpInfoTxt.text  = infoLabel;
        if (spdInfoTxt != null) spdInfoTxt.text = infoLabel;
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

        // 슬42: 사용 즉시 실제 효과 적용 (회복물약=체력 회복+HP Potion Ef / 신속물약=공격속도 버프)
        ApplyPotionEffect(s);

        RefreshSlotVisual(s);
    }

    // 슬42: 물약 종류별 실제 효과 적용.
    void ApplyPotionEffect(Slot s)
    {
        var player = Managers.Game.Player;
        if (player == null) return;

        if (s.name == "HP")
        {
            Managers.Sound?.PlaySFX("SFX_RedPotion");   // 회복물약 사용
            // 회복물약: 고정 회복 + 최대체력 비율 회복 (ItemData/HPPotionData)
            if (Managers.Data.HPPotionDic.TryGetValue(s.dataId, out var hp))
            {
                float amount = hp.FlatRecov + player.MaxHp * (hp.MaxHPRecov / 100f);
                player.Heal(amount);
            }
            // 수동 사용도 자동 쿨타임을 공유 (직후 자동 중복 발동 방지)
            player.StartHpPotionCooldown(s.cooldownMax);
            // 슬42: HP Potion Ef를 캐릭터 몸에 생성
            player.SpawnHpPotionFx();
        }
        else if (s.name == "Speed")
        {
            Managers.Sound?.PlaySFX("SFX_SpeedPotion");   // 신속물약 사용
            // 신속물약: 일정 시간 공격속도 버프. 데이터 AtkSpeedBonus는 분수(0.2=20%)라 ×100(퍼센트 공식 단위)
            if (Managers.Data.SpeedPotionDic.TryGetValue(s.dataId, out var sp))
                player.ApplySpeedBuff(sp.AtkSpeedBonus * 100f, sp.Duration);
            // 쿨타임을 플레이어에 보관 — 팝업을 껐다 켜도 유지 (HP와 동일 패턴)
            player.StartSpdPotionCooldown(s.cooldownMax);
        }
    }

    float GetHpCooldown(int dataId)
        => Managers.Data.HPPotionDic.TryGetValue(dataId, out var d) && d.Cooldown > 0 ? d.Cooldown : 5f;
    float GetSpdCooldown(int dataId)
        => Managers.Data.SpeedPotionDic.TryGetValue(dataId, out var d) && d.Cooldown > 0 ? d.Cooldown : 10f;

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

        // 슬42: 자동물약은 PlayerController.UpdateAutoPotion이 전투 중 상시 처리(팝업 열림 무관).
        //  여기서는 처리하지 않는다.
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
        Managers.Game.AutoPotionOn = _autoOn;
        RefreshAutoUI();
    }

    void RefreshAutoUI()
    {
        // AutoBtnToggleImg(로딩 스피너)는 Animator에 On/Off 상태가 없어 Play가 무효였음.
        //  → 자동물약 작동 표시로, On일 때만 스피너 표시(활성=Animator 자동 재생), Off면 숨김.
        if (_autoToggleAnimator != null && _autoToggleAnimator.gameObject.activeSelf != _autoOn)
            _autoToggleAnimator.gameObject.SetActive(_autoOn);

        // 슬41: 자동물약 On이면 HP Ratio 슬라이더 열림(조절 가능), Off면 잠금.
        if (_hpRatioSlider != null)
        {
            _hpRatioSlider.interactable = _autoOn;
            _hpRatioSlider.value = _hpRatio;
        }
        if (_autoGuageLock != null)
            _autoGuageLock.SetActive(!_autoOn);
        if (_hpRatioText != null)
            _hpRatioText.text = _autoOn ? $"{Mathf.RoundToInt(_hpRatio * 100)}%" : "";
    }

    void OnHpRatioChanged(float v)
    {
        if (!_autoOn) return;   // 슬라이더는 auto On일 때만 조절
        _hpRatio = v;
        Managers.Game.AutoPotionHpRatio = _hpRatio;
        if (_hpRatioText != null)
            _hpRatioText.text = $"{Mathf.RoundToInt(_hpRatio * 100)}%";
    }

    /// <summary>외부에서 "자동물약 기억"(물약 주입기 스킬 해금) 호출용.</summary>
    public void MarkAutoPotionMemorized()
    {
        _autoMemorized = true;
        Managers.Game.AutoPotionMemorized = true;
        if (_autoBtnNewTxt != null) _autoBtnNewTxt.SetActive(true);
        RefreshAutoUI();
    }
}
