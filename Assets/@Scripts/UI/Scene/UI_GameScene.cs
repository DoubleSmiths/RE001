using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Util;

public class UI_GameScene : UI_Scene
{
    #region Enum

    enum Images
    {
        CurrencyBar,
        MainGoldImg,
        MainCrystalImg,
        ButtomDisplayer,
        ButtonDisplayerBackground,
        Frame,
        Frame2,
        CharacterButtonIcon,
        CharacterBtnNewIcon,
        InventoryButtonIcon,
        PotionButtonIcon,
        EquipmentButtonIcon,
        SkillButtonIcon,
        BestiaryButtonIcon,
        BestiaryBtnNewIcon,
        BestiaryBtnFollowingBox,
        PlayerHPBarGroup,
        PlayerHPBarFrame,
        PlayerHPBarGuage,
        PlayerEXPBarGroup,
        PlayerEXPBarFrame,
        PlayerEXPBarGuage,
        PlayerPortraitFrame,
        PortraitMask,
        PortraitImg,
        Main_QuestFollow,
        Main_QuestFollowColor,
        ForFadeImage,
    }

    enum Texts
    {
        MainGoldText,
        MainCrystalText,
        CharacterButtonText,
        InventoryButtonText,
        InventoryButtonNewTxt,
        PotionButtonText,
        PotionButtonNewTxt,
        EquipmentButtonText,
        EquipmentButtonNewTxt,
        SkillButtonText,
        SkillButtonNewTxt,
        BestiaryButtonText,
        BestiaryBtnFollowingNumTxt,
        BestiaryBtnNewTxt,
        RankTxt,
        PlayerHPBarText,
        PlayerEXPTextFlat,
        PlayerEXPTextPercent,
        PlayerRankText,
        PlayerLevelText,
        Quest_NeedItemNameTxt,
        Quest_NeedItemAmountTxt,
    }

    enum Buttons
    {
        OptionBtn,
        GuildButton,
        GuildButton1,
        GuildButton2,
        GuildButton3,
        GuildButton4,
        TravelButtonOnBattle,
        CharacterButton,
        InventoryButton,
        PotionButton,
        EquipmentButton,
        SkillButton,
        BestiaryButton,
        ExitButton,
    }

    enum GameObjects
    {
        FIXED,
        InFrame,
        MainGoldShow,
        MainCrystalShow,
        AreaBtnGroupSlot,
        OnBattleBtns,
        OnBattleMainBtnGroup,
        TravelButtonOnBattleQuestVFX,
        BTMButtonGroup,
        BTMButtonUnlockHelper,
        Layout,
        Dust_01,
        Ring_01,
        ExitQuestVFX,
        PlayerBar,
        PlayerHPBarGuageBox,
        PlayerEXPBarGuageBox,
        PlayerPortraitGroup,
        QuestFollow,
        TxtGroup,
        Quest_Done_Effect,
        GlowDot_Lens_01,
        GlowDot_Lens_02,
        GlowDot_02,
    }

    #endregion


    UI_AreaBtnGroup _currentBtnGroup;
    int _followActiveQuestId = -1;
    bool _isAreaTransitioning;

    private void Awake() { Init(); }

    // 재화 Lerp용 표시값
    float _displayGold;
    float _displayCrystal;

    // HP/EXP 이전값 추적
    float _prevHp  = -1f;
    float _prevExp = -1f;

    static readonly Color _barWhite = Color.white;
    static readonly Color _barHurt  = new Color32(0xFF, 0x00, 0x00, 0xFF); // FF0000
    static readonly Color _barHeal  = new Color32(0x58, 0xFF, 0x00, 0xFF); // 58FF00

    static readonly Color _goldColor = Color.white;                                  // #FFFFFF
    static readonly Color _crystalColor = new Color(0f, 0.953f, 1f, 80f / 225f);        // #00F3FF
    static readonly Color _increaseColor = new Color(0f, 1f, 0f);                       // #00FF00
    static readonly Color _decreaseColor = new Color(1f, 0f, 0f);                       // #FF0000


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 폰트 등록 (숫자 텍스트는 Inspector에서 개별 지정)
        ApplyFont(GetText((int)Texts.CharacterButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.InventoryButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.InventoryButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.PotionButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.PotionButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.EquipmentButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.EquipmentButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.SkillButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.SkillButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.BestiaryButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.BestiaryBtnNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.PlayerRankText), FontType.UI);
        ApplyFont(GetText((int)Texts.PlayerLevelText), FontType.UI);

        // 버튼 이벤트
        GetButton((int)Buttons.OptionBtn).gameObject.BindEvent(OnClickOption);
        GetButton((int)Buttons.CharacterButton).gameObject.BindEvent(OnClickCharacter);
        GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventory);
        GetButton((int)Buttons.PotionButton).gameObject.BindEvent(OnClickPotion);
        GetButton((int)Buttons.EquipmentButton).gameObject.BindEvent(OnClickEquipment);
        GetButton((int)Buttons.SkillButton).gameObject.BindEvent(OnClickSkill);
        GetButton((int)Buttons.BestiaryButton).gameObject.BindEvent(OnClickBestiary);

        // Camera Setting
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;

        // 초기화 시 퀘스트 동행 UI는 비활성화 상태로 시작 (필요 시 활성화)
        GetObject((int)GameObjects.QuestFollow).gameObject.SetActive(false);

        // Fade
        //FadeInOut(GetImage((int)Images.ForFadeImage));

        Managers.Quest.OnFollowUIChanged += RefreshQuestFollow;
        Managers.Quest.OnBtnUnlocked += OnBtnUnlocked;
        RefreshQuestFollow(Managers.Quest.CurrentQuestID);

        // 슬라이드 10: 최하단 탭 버튼 하이라이트 되돌림을 위한 Popup 닫힘 구독
        Managers.UI.OnPopupClosed += OnBtmPopupClosed;

        SetInfo();
        RefreshTabLocks();
        RefreshBTMButtonGroup();
        RefreshBtmButtonsLockState();

        // TODO : 플레이어의 현재 구역에 따라 EnterArea 호출
        EnterArea(AreaType.Village1);

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Managers.Game != null)
        {
            Managers.Game.OnResourceChanged -= RefreshCurrency;
            Managers.Game.OnLanguageChanged -= RefreshTexts;
            Managers.Game.OnInventoryNewChanged -= RefreshBTMButtonGroup;
            Managers.Game.OnItemChanged -= OnFollowItemChanged;
            Managers.Game.OnStatusPointChanged -= RefreshCharacterNewIcon;
        }

        if (Managers.Quest != null)
        {
            Managers.Quest.OnFollowUIChanged -= RefreshQuestFollow;
            Managers.Quest.OnBtnUnlocked -= OnBtnUnlocked;
        }

        if (Managers.UI != null)
            Managers.UI.OnPopupClosed -= OnBtmPopupClosed;
    }

    public void FadeInOut()
    {
        FadeInOut(GetImage((int)Images.ForFadeImage));
    }

    // ────────────────────────────────────────
    //  구역 전환
    // ────────────────────────────────────────

    /// <summary>페이드 없이 즉시 구역 전환 (씬 초기 로드용)</summary>
    public void EnterArea(AreaType area)
    {
        SwapAreaBtnGroup(area);
    }

    /// <summary>
    /// FadeOut → onBlack(배경 교체) → BtnGroup 교체 → FadeIn → PlayEnterAnimation 순으로 구역 전환.
    /// </summary>
    public bool CanEnterAreaTransition(AreaType area)
    {
        if (_isAreaTransitioning)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: already transitioning to another area. requested={area}");
            return false;
        }

        if (GetImage((int)Images.ForFadeImage) == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: fade image missing. requested={area}");
            return false;
        }

        if (GetObject((int)GameObjects.AreaBtnGroupSlot) == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: AreaBtnGroupSlot missing. requested={area}");
            return false;
        }

        string prefabName = $"UI_{area}BtnGroup";
        GameObject prefab = Managers.Resource.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: prefab missing. prefab={prefabName}");
            return false;
        }

        if (prefab.GetComponent<UI_AreaBtnGroup>() == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: UI_AreaBtnGroup missing on prefab. prefab={prefabName}");
            return false;
        }

        return true;
    }

    // 슬라이드 55 — Battle Scene 진입 시 Fade Out 0.5 / Fade In 1.0.
    // Fade In 절반(t=1.0)에 [지역명 UI] 표시. 배너 In Animation 종료(t=2.0)에 BG scroll + Player Idle→Run + 배틀 루프 시작.
    public bool EnterAreaWithFade(AreaType area, System.Action onBlack = null)
    {
        if (CanEnterAreaTransition(area) == false)
            return false;

        Image fade = GetImage((int)Images.ForFadeImage);
        if (fade == null)
            return false;

        _isAreaTransitioning = true;

        // 마을(Village*) 복귀 시 CurrentTravelAreaId 갱신 (HOME 버튼/사망 등 Travel popup 거치지 않는 경로).
        // Travel popup의 "현재 위치" 표시가 마지막 전투 행선지로 남아있던 버그 차단.
        // AreaType enum 이름과 TravelNode.AreaType 문자열을 매칭 — Village2/3 추가되어도 코드 수정 불필요.
        // Battle은 노드가 여러 개라 자동 매칭 X (TravelPopup.TravelToNode가 정확한 노드 id를 별도로 set함).
        if (area != AreaType.Battle)
        {
            string areaName = area.ToString();
            if (Managers.Data?.TravelNodeDic != null)
            {
                foreach (var node in Managers.Data.TravelNodeDic.Values)
                {
                    if (node.AreaType == areaName)
                    {
                        Managers.Game.CurrentTravelAreaId = node.Id;
                        break;
                    }
                }
            }
        }

        // 슬라이드 69 — Battle 씬에서 나갈 때 transit 효과:
        //   monster Idle 고정 + BG scroll OFF + Player Run + 화면 밖 이동.
        //   Fade Out 동안 진행되며, SwapAreaBtnGroup(검은 화면 시점)에서 UI_BattleBtnGroup destroy → EndBattle 정리.
        var battleBtn = _currentBtnGroup as UI_BattleBtnGroup;
        if (battleBtn != null) battleBtn.BeginExitTransit();

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // Fade Out 0.5s
        seq.Append(fade.DOFade(1f, 0.5f).SetEase(Ease.InQuad).SetUpdate(true));

        // 검은 화면에서 영역 교체
        seq.AppendCallback(() =>
        {
            onBlack?.Invoke();
            if (SwapAreaBtnGroup(area) == false)
                Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed during transition. area={area}");
        });

        // Fade In 1.0s
        seq.Append(fade.DOFade(0f, 1.0f).SetEase(Ease.InQuad).SetUpdate(true));

        if (area == AreaType.Battle)
        {
            // Battle 진입 — Fade In 50% (절대 t=1.0)에 UI_Toast(지역명 배너) 표시.
            // Toast prefab의 DOTweenAnimation Move 종료(슬라이드 55: 1초)에서 진입 완료 처리.
            seq.InsertCallback(1.0f, () => Managers.UI.ShowToast(GetCurrentAreaName()));
            seq.InsertCallback(2.0f, () =>
            {
                StartBattleSceneScroll();
                _currentBtnGroup?.PlayEnterAnimation();
            });
            seq.OnComplete(() => _isAreaTransitioning = false);
        }
        else
        {
            // 비-Battle (마을 등) — 기존 동작: fade 끝나면 바로 PlayEnterAnimation
            seq.OnComplete(() =>
            {
                _isAreaTransitioning = false;
                _currentBtnGroup?.PlayEnterAnimation();
            });
        }
        seq.OnKill(() => _isAreaTransitioning = false);
        return true;
    }

    string GetCurrentAreaName()
    {
        int id = Managers.Game?.CurrentTravelAreaId ?? 0;
        if (Managers.Data?.TravelNodeDic != null &&
            Managers.Data.TravelNodeDic.TryGetValue(id, out var node))
            return Util.Localize(node.Name);
        return string.Empty;
    }

    void StartBattleSceneScroll()
    {
        // 슬라이드 55/57/58 — 배경 스크롤 시작 + Player Idle→Run
        BackgroundScroller scroller = null;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == "CH1BG" || go.name == "CH1BG(Clone)")
            {
                scroller = go.GetComponent<BackgroundScroller>();
                if (scroller != null) break;
            }
        }
        if (scroller != null) scroller.IsScrolling = true;

        var player = Managers.Game?.Player;
        if (player != null && player.State != Define.State.Dead)
            player.State = Define.State.Moving;
    }

    bool SwapAreaBtnGroup(AreaType area)
    {
        if (_currentBtnGroup != null)
            Managers.Resource.Destroy(_currentBtnGroup.gameObject);

        GameObject slotObject = GetObject((int)GameObjects.AreaBtnGroupSlot);
        if (slotObject == null)
        {
            Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed: AreaBtnGroupSlot missing. area={area}");
            _currentBtnGroup = null;
            return false;
        }

        Transform slot = slotObject.transform;
        string prefabName = $"UI_{area}BtnGroup";

        GameObject go = Managers.Resource.Instantiate(prefabName, slot);
        if (go == null)
        {
            Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed: instantiate returned null. prefab={prefabName}");
            _currentBtnGroup = null;
            return false;
        }

        _currentBtnGroup = go.GetComponent<UI_AreaBtnGroup>();
        if (_currentBtnGroup == null)
        {
            Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed: UI_AreaBtnGroup missing. prefab={prefabName}");
            Managers.Resource.Destroy(go);
            return false;
        }

        _currentBtnGroup.Setup(this);
        Debug.Log($"[UI_GameScene] SwapAreaBtnGroup success. area={area}, prefab={prefabName}");
        return true;
    }

    // ────────────────────────────────────────
    //  SetInfo – 씬 진입 시 전체 갱신
    // ────────────────────────────────────────
    public void SetInfo()
    {
        // 재화 초기값 즉시 표시 (Lerp 없이)
        _displayGold = Managers.Game.Gold;
        _displayCrystal = Managers.Game.Crystal;
        GetText((int)Texts.MainGoldText).text = Managers.Game.Gold.ToString("N0");
        GetText((int)Texts.MainCrystalText).text = Managers.Game.Crystal.ToString("N0");

        RefreshPlayerBar();
        RefreshBestiaryFollowing();
        RefreshTexts();
        RefreshBTMButtonGroup();

        Managers.Game.OnResourceChanged += RefreshCurrency;
        Managers.Game.OnLanguageChanged += RefreshTexts;
        Managers.Game.OnInventoryNewChanged += RefreshBTMButtonGroup;
        Managers.Game.OnItemChanged += OnFollowItemChanged;
    }

    // ────────────────────────────────────────
    //  재화
    // ────────────────────────────────────────
    void RefreshCurrency()
    {
        int newGold = Managers.Game.Gold;
        int newCrystal = Managers.Game.Crystal;

        // 증감 색상 플래시
        if (newGold != (int)_displayGold)
            FlashCurrencyText(GetText((int)Texts.MainGoldText), newGold > _displayGold, _goldColor);
        if (newCrystal != (int)_displayCrystal)
            FlashCurrencyText(GetText((int)Texts.MainCrystalText), newCrystal > _displayCrystal, _crystalColor);

        // Lerp 갱신
        DOTween.To(() => _displayGold, x =>
        {
            _displayGold = x;
            GetText((int)Texts.MainGoldText).text = ((int)x).ToString("N0");
        }, newGold, 0.5f).SetEase(Ease.OutQuad);

        DOTween.To(() => _displayCrystal, x =>
        {
            _displayCrystal = x;
            GetText((int)Texts.MainCrystalText).text = ((int)x).ToString("N0");
        }, newCrystal, 0.5f).SetEase(Ease.OutQuad);
    }

    void FlashCurrencyText(TMP_Text text, bool increased, Color originalColor)
    {
        Color flash = increased ? _increaseColor : _decreaseColor;
        text.DOKill();
        text.DOColor(flash, 0.1f)
            .OnComplete(() => text.DOColor(originalColor, 0.3f));
    }

    // ────────────────────────────────────────
    //  플레이어 바 (HP / EXP / 레벨)
    // ────────────────────────────────────────
    void RefreshPlayerBar()
    {
        int lv = Managers.Game.Level;
        GetText((int)Texts.PlayerLevelText).text = $"Lv.{lv}";
        GetText((int)Texts.PlayerRankText).text = Managers.Game.PlayerName;

        // TODO: 직업에 따라 PortraitImg 스프라이트 교체
        // Managers.Resource.LoadAsync<Sprite>($"Portrait_{jobType}", sprite =>
        //     GetImage((int)Images.PortraitImg).sprite = sprite);
    }

    // ────────────────────────────────────────
    //  텍스트 로컬라이징
    // ────────────────────────────────────────
    void RefreshTexts()
    {
        GetText((int)Texts.CharacterButtonText).text = Localize(LocalizeKey.BtmCharacterBtnText);
        GetText((int)Texts.InventoryButtonText).text = Localize(LocalizeKey.BtmInventoryBtnText);
        GetText((int)Texts.PotionButtonText).text = Localize(LocalizeKey.BtmPotionBtnText);
        GetText((int)Texts.EquipmentButtonText).text = Localize(LocalizeKey.BtmEquipmentBtnText);
        GetText((int)Texts.SkillButtonText).text = Localize(LocalizeKey.BtmSkillBtnText);
        GetText((int)Texts.BestiaryButtonText).text = Localize(LocalizeKey.BtmBeastiaryBtnText);
    }

    public void RefreshBTMButtonGroup()
    {
        // TODO : 일반 재료 아이템이랑 중요 아이템만 확인하게 해야 됨.
        bool flag = false;
        foreach (InventoryItem item in Managers.Game.Inventory)
        {
            // TODO : 아이템 ID 범위는 예시입니다. 실제로는 게임 디자인에 맞게 조정해야 합니다.
            if (item.DataId > Define.ITEM_ID_MIN && item.DataId < Define.ITEM_ID_MIN + 1000 || item.DataId > Define.IMPORTANT_ITEM_ID_MIN && item.DataId < Define.IMPORTANT_ITEM_ID_MIN + 1000 && item.IsNew)
            {
                flag = true;
                GetText((int)Texts.InventoryButtonNewTxt)?.gameObject?.SetActive(true);
                break;
            }
        }

        if (!flag) GetText((int)Texts.InventoryButtonNewTxt)?.gameObject?.SetActive(false);
    }

    public void UpdateHP(float current, float max)
    {
        // bar: fill Lerp
        Image bar = GetImage((int)Images.PlayerHPBarGuage);
        bar.DOKill();
        bar.DOFillAmount(max > 0 ? current / max : 0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        // 텍스트 갱신
        TMP_Text hpText = GetText((int)Texts.PlayerHPBarText);
        hpText.text = $"{(int)current} / {(int)max}";

        // 텍스트 색상 플래시 (초기 설정 시 스킵)
        if (_prevHp >= 0f)
        {
            bool isHurt = current < _prevHp;
            Color flash = isHurt ? _barHurt : _barHeal;

            hpText.DOKill();
            hpText.DOColor(flash, 0.17f).SetUpdate(true)
                  .OnComplete(() => hpText.DOColor(_barWhite, 0.17f).SetUpdate(true));

            // 회복 시 텍스트 Scale 증감
            if (!isHurt)
                hpText.transform.DOScale(1.05f, 0.17f).SetUpdate(true)
                      .OnComplete(() => hpText.transform.DOScale(1f, 0.17f).SetUpdate(true));
        }

        _prevHp = current;
    }

    public void UpdateEXP(float current, float required)
    {
        // bar: fill Lerp
        Image bar = GetImage((int)Images.PlayerEXPBarGuage);
        bar.DOKill();
        bar.DOFillAmount(required > 0 ? current / required : 0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        // 퍼센트 텍스트 (##.####% 형식)
        TMP_Text expText = GetText((int)Texts.PlayerEXPTextPercent);
        expText.text = required > 0 ? $"{current / required * 100f:F4}%" : "0.0000%";

        // EXP 증가 시 텍스트 플래시 + Scale
        if (_prevExp >= 0f && current > _prevExp)
        {
            expText.DOKill();
            expText.DOColor(_barHeal, 0.17f).SetUpdate(true)
                   .OnComplete(() => expText.DOColor(_barWhite, 0.17f).SetUpdate(true));

            expText.transform.DOScale(1.05f, 0.17f).SetUpdate(true)
                   .OnComplete(() => expText.transform.DOScale(1f, 0.17f).SetUpdate(true));
        }

        _prevExp = current;
    }

    // 슬라이드 68 — Level Up Event
    //   2-1. Player Level Up Text 팝업 (UI_PlayerLevelUpTextPopup.prefab)
    //   2-2. Player Level Up Effect (Player Level Up Ef.prefab)
    //   2-3. HP Now → MaxHp 회복 (PlayerController.RefreshStats에서 이미 처리) + 초록 회복 텍스트
    //   2-4/2-5. EXP/Level UI 갱신 (OnExpChanged/PlayerLevelText 핸들러가 처리)
    //   2-6. [상태] Button [+] icon Punch/Scale (Duration 0.3 OutQuad TO 0.2 Vibrato 10 Elasticity 1)
    public void OnLevelUp(int newLevel, int regenHp)
    {
        GetText((int)Texts.PlayerLevelText).text = $"Lv.{newLevel}";

        PlayerController player = Managers.Game?.Player;
        if (player != null)
        {
            Vector3 pos = player.transform.position;

            // 2-1. Level Up Text 팝업 — RectTransform 기반 UI prefab. Canvas(SceneUI) 하위로 parent + local 좌표 리셋.
            // prefab 내부 DOTweenAnimation 최대 anim 시간 ≈ 1.5s. 2s 후 자체 정리.
            var textPopup = Managers.Resource.Instantiate("UI_PlayerLevelUpTextPopup");
            if (textPopup != null)
            {
                textPopup.transform.SetParent(transform, worldPositionStays: false);
                var rt = textPopup.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = Vector2.zero;
                textPopup.transform.localScale = Vector3.one;
                Destroy(textPopup, 2f);
            }

            // 2-2. Level Up Effect — ParticleSystem 기반 world-space VFX. 3s 후 정리.
            var effect = Managers.Resource.Instantiate("Player Level Up Ef");
            if (effect != null)
            {
                effect.transform.position = pos;
                Destroy(effect, 3f);
            }

            // 2-3. HP 회복 텍스트 (초록). 회복량 0이면 popup 생략.
            if (regenHp > 0)
                Managers.Object.ShowHealFont(pos, regenHp, player.transform);
        }

        // 2-6. [상태] 버튼 [+] icon — active + Punch/Scale
        GameObject icon = GetImage((int)Images.CharacterBtnNewIcon)?.gameObject;
        if (icon != null)
        {
            icon.SetActive(true);
            icon.transform.DOKill();   // 진행 중인 punch 있으면 정리 (Restart)
            icon.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 10, 1f).SetEase(Ease.OutQuad);
        }
    }

    void RefreshCharacterNewIcon()
    {
        GetImage((int)Images.CharacterBtnNewIcon).gameObject.SetActive(Managers.Game.StatusPoint > 0);
    }

    // ────────────────────────────────────────
    //  도감 – 동행 중인 미니몬 수
    // ────────────────────────────────────────
    void RefreshBestiaryFollowing()
    {
        int companionId = Managers.Game.CompanionMinMonId;
        bool hasCompanion = companionId != -1;

        GetObject((int)GameObjects.BTMButtonGroup); // 참조 확인용
        GetImage((int)Images.BestiaryBtnFollowingBox).gameObject.SetActive(hasCompanion);
        if (hasCompanion)
            GetText((int)Texts.BestiaryBtnFollowingNumTxt).text = "1";
    }

    // ────────────────────────────────────────
    //  탭 잠금
    // ────────────────────────────────────────
    void RefreshTabLocks()
    {
        // RefreshBtmButtonsLockState에서 해금 상태 기반으로 활성/비활성 처리하므로 별도 작업 불필요
    }

    void OnFollowItemChanged(int dataId)
    {
        Data.QuestData quest = Managers.Quest.GetCurrentFollowUI();
        if (quest == null) return;
        if (!int.TryParse(quest.FollowItemID, out int followId)) return;
        if (followId != dataId) return;

        RefreshQuestFollow(Managers.Quest.CurrentQuestID);
    }

    void RefreshQuestFollow(int questId)
    {
        Data.QuestData quest = Managers.Quest.GetCurrentFollowUI();
        GameObject followObj = GetObject((int)GameObjects.QuestFollow);

        if (quest == null)
        {
            followObj.SetActive(false);
            _followActiveQuestId = -1;
            return;
        }

        int itemId = int.Parse(quest.FollowItemID);
        int current = Managers.Game.GetItemCount(itemId);

        GetText((int)Texts.Quest_NeedItemNameTxt).text = quest.FollowItemName;
        GetText((int)Texts.Quest_NeedItemAmountTxt).text = $"{current}/{quest.FollowCount}";

        bool wasInactive = !followObj.activeSelf;
        if (!followObj.activeSelf)
        {
            followObj.SetActive(true);
            GetImage((int)Images.Main_QuestFollow).GetComponent<DOTweenAnimation>().DORestart();
        }
        else followObj.SetActive(true);

        bool isDone = current >= quest.FollowCount;
        ColorUtility.TryParseHtmlString($"#{(isDone ? Define.COMPLETE_COLOR : Define.QUEST_FOLLOW_DEFAULT_COLOR)}", out Color color);
        GetImage((int)Images.Main_QuestFollowColor).color = color;

        if (isDone && _followActiveQuestId != questId)
            GetObject((int)GameObjects.Quest_Done_Effect).GetComponent<ParticleSystem>().Play();

        _followActiveQuestId = questId;

        if (wasInactive)
            GetImage((int)Images.Main_QuestFollow).GetComponent<DOTweenAnimation>().DORestart();
    }

    void OnBtnUnlocked(string btnId)
    {
        if (btnId == UNLOCK_INVENTORY_BUTTON)
        {
            GetButton((int)Buttons.InventoryButton).gameObject.SetActive(true);
            GetButton((int)Buttons.InventoryButton).interactable = true;
            Managers.Game.UnlockBtmButton(TabType.Bag);
            RefreshBtmButtonsLockState();
            ShowNewBadge(TabType.Bag);
        }
        else if (btnId == UNLOCK_POTION_BUTTON)
        {
            // 서브퀘스트 완료로 silent 해금 — 해금 애니메이션/팝업 없이 하단 Potion 탭 활성화
            GetButton((int)Buttons.PotionButton).gameObject.SetActive(true);
            GetButton((int)Buttons.PotionButton).interactable = true;
            Managers.Game.UnlockBtmButton(TabType.Potion);
            RefreshBtmButtonsLockState();
            ShowNewBadge(TabType.Potion);
        }
    }

    void SetTabInteractable(Buttons btn, bool interactable)
    {
        var button = GetButton((int)btn);
        button.interactable = interactable;
        switch (btn)
        {
            case Buttons.CharacterButton:
                GetButton((int)Buttons.CharacterButton).gameObject.SetActive(interactable);
                GetImage((int)Images.CharacterBtnNewIcon).gameObject.SetActive(false);
                break;
            case Buttons.InventoryButton:
                GetButton((int)Buttons.InventoryButton).gameObject.SetActive(interactable);
                GetText((int)Texts.InventoryButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.PotionButton:
                GetButton((int)Buttons.PotionButton).gameObject.SetActive(interactable);
                GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.EquipmentButton:
                //GetButton((int)Buttons.EquipmentButton).gameObject.SetActive(interactable);
                GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.SkillButton:
                //GetButton((int)Buttons.SkillButton).gameObject.SetActive(interactable);
                GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.BestiaryButton:
                GetButton((int)Buttons.BestiaryButton).gameObject.SetActive(interactable);
                GetText((int)Texts.BestiaryBtnNewTxt).gameObject.SetActive(false);
                GetImage((int)Images.BestiaryBtnNewIcon).gameObject.SetActive(false);
                break;
        }
    }

    // ────────────────────────────────────────
    //  New 뱃지
    // ────────────────────────────────────────
    public void ShowNewBadge(TabType tab)
    {
        switch (tab)
        {
            case TabType.Status:
                RefreshCharacterNewIcon();
                break;
            case TabType.Bag:
                GetText((int)Texts.InventoryButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Potion:
                GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Equipment:
                GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Skill:
                GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Bestiary:
                GetText((int)Texts.BestiaryBtnNewTxt).gameObject.SetActive(true);
                GetImage((int)Images.BestiaryBtnNewIcon).gameObject.SetActive(true);
                break;
        }
    }

    // ────────────────────────────────────────
    //  PlayerController 이벤트 연결
    // ────────────────────────────────────────
    public void BindPlayer(PlayerController player)
    {
        player.OnHpChanged += (cur, max) => UpdateHP(cur, max);
        player.OnExpChanged += (cur, req) => UpdateEXP(cur, req);
        player.OnLevelUp += (lv, regen) => OnLevelUp(lv, regen);
        UpdateHP(player.Hp, player.MaxHp);
        Managers.Game.OnStatusPointChanged += RefreshCharacterNewIcon;
        RefreshCharacterNewIcon();
    }

    // ────────────────────────────────────────
    //  버튼 핸들러
    // ────────────────────────────────────────
    void OnClickOption()
    {
        // 기획서 스펙: Scale / Duration 0.07 / OutQuad / Loops 2 / Yoyo / To 0.75
        GetButton((int)Buttons.OptionBtn).transform
            .DOScale(0.75f, 0.07f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);

        Managers.UI.ShowPopupUI<UI_SettingPopup>();
    }

    // ────────────────────────────────────────
    //  최하단 탭 버튼 (슬라이드 10)
    //  - 클릭 시 해당 Popup 열기 + 하이라이트(#FFDA01 Color + 1.1 Scale, 0.3s OutQuad)
    //  - 잠긴 버튼은 무반응
    //  - Popup 닫힘 시 하이라이트 되돌림
    // ────────────────────────────────────────
    static readonly Color BTM_HL_COLOR = new Color32(0xFF, 0xDA, 0x01, 0xFF);
    const float BTM_HL_SCALE = 1.1f;
    const float BTM_HL_DURATION = 0.3f;

    Color[] _btmBtnOriginalColor;
    System.Type _btmActivePopupType;
    int _btmActiveIdx = -1;

    void OnClickCharacter()
    {
        HandleBtmClick(TabType.Status, Buttons.CharacterButton, typeof(UI_CharacterPopup),
            () => Managers.UI.ShowPopupUI<UI_CharacterPopup>());
    }

    void OnClickInventory()
    {
        HandleBtmClick(TabType.Bag, Buttons.InventoryButton, typeof(UI_InventoryPopup),
            () => Managers.UI.ShowPopupUI<UI_InventoryPopup>().SetInfo());
    }

    void OnClickPotion()
    {
        HandleBtmClick(TabType.Potion, Buttons.PotionButton, typeof(UI_PotionPopup),
            () => Managers.UI.ShowPopupUI<UI_PotionPopup>().SetInfo());
    }

    void OnClickEquipment()
    {
        HandleBtmClick(TabType.Equipment, Buttons.EquipmentButton, null,
            () => Managers.UI.ShowToast("장비 팝업은 준비 중입니다."));
    }

    void OnClickSkill()
    {
        HandleBtmClick(TabType.Skill, Buttons.SkillButton, null,
            () => Managers.UI.ShowToast("기술 팝업은 준비 중입니다."));
    }

    void OnClickBestiary()
    {
        HandleBtmClick(TabType.Bestiary, Buttons.BestiaryButton, null,
            () => Managers.UI.ShowToast("도감 팝업은 준비 중입니다."));
    }

    void HandleBtmClick(TabType tab, Buttons btnId, System.Type popupType, System.Action showAction)
    {
        if (Managers.Game.IsBtmButtonUnlocked(tab) == false)
            return; // 잠긴 탭은 무반응

        int idx = (int)btnId;
        if (popupType != null)
        {
            PlayBtmHighlight(btnId);
            _btmActivePopupType = popupType;
            _btmActiveIdx = idx;
        }
        showAction?.Invoke();
    }

    // 각 Btm 버튼의 아이콘 Image (Button.colors가 안 덮어쓰는 별도 Image)
    Image GetBtmIcon(Buttons btnId)
    {
        switch (btnId)
        {
            case Buttons.CharacterButton: return GetImage((int)Images.CharacterButtonIcon);
            case Buttons.InventoryButton: return GetImage((int)Images.InventoryButtonIcon);
            case Buttons.PotionButton:    return GetImage((int)Images.PotionButtonIcon);
            case Buttons.EquipmentButton: return GetImage((int)Images.EquipmentButtonIcon);
            case Buttons.SkillButton:     return GetImage((int)Images.SkillButtonIcon);
            case Buttons.BestiaryButton:  return GetImage((int)Images.BestiaryButtonIcon);
        }
        return null;
    }

    void PlayBtmHighlight(Buttons btnId)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null) return;
        Transform tr = btn.transform;
        Image icon = GetBtmIcon(btnId);
        CacheBtmOriginalColor(btnId, icon);

        tr.DOKill();
        tr.DOScale(BTM_HL_SCALE, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        if (icon != null)
        {
            icon.DOKill();
            icon.DOColor(BTM_HL_COLOR, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    void RestoreBtmHighlight(Buttons btnId)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null) return;
        Transform tr = btn.transform;
        Image icon = GetBtmIcon(btnId);

        tr.DOKill();
        tr.DOScale(1f, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        if (icon != null && _btmBtnOriginalColor != null)
        {
            int idx = (int)btnId;
            if (idx >= 0 && idx < _btmBtnOriginalColor.Length && _btmBtnOriginalColor[idx].a > 0f)
            {
                icon.DOKill();
                icon.DOColor(_btmBtnOriginalColor[idx], BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
            }
        }
    }

    void CacheBtmOriginalColor(Buttons btnId, Image img)
    {
        if (img == null) return;
        int count = System.Enum.GetValues(typeof(Buttons)).Length;
        if (_btmBtnOriginalColor == null || _btmBtnOriginalColor.Length < count)
            _btmBtnOriginalColor = new Color[count];
        int idx = (int)btnId;
        if (idx >= _btmBtnOriginalColor.Length) return;
        // 아이콘은 투명하지 않은 색상을 원본으로 캐시 (이미 하이라이트 진행 중인 상태 저장 방지)
        if (_btmBtnOriginalColor[idx].a <= 0f)
            _btmBtnOriginalColor[idx] = img.color;
    }

    void OnBtmPopupClosed(System.Type closedType)
    {
        if (_btmActivePopupType == null || closedType != _btmActivePopupType)
            return;
        if (_btmActiveIdx < 0)
            return;
        RestoreBtmHighlight((Buttons)_btmActiveIdx);
        _btmActivePopupType = null;
        _btmActiveIdx = -1;
    }

    void RefreshBtmButtonsLockState()
    {
        ApplyBtmLock(Buttons.CharacterButton, TabType.Status);
        ApplyBtmLock(Buttons.InventoryButton, TabType.Bag);
        ApplyBtmLock(Buttons.PotionButton, TabType.Potion);
        ApplyBtmLock(Buttons.EquipmentButton, TabType.Equipment);
        ApplyBtmLock(Buttons.SkillButton, TabType.Skill);
        ApplyBtmLock(Buttons.BestiaryButton, TabType.Bestiary);
    }

    void ApplyBtmLock(Buttons btnId, TabType tab)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null) return;
        bool unlocked = Managers.Game.IsBtmButtonUnlocked(tab);
        // 잠긴 버튼은 완전히 숨김 (NEW 뱃지 등 포함된 루트 오브젝트 전체)
        btn.gameObject.SetActive(unlocked);
    }

#if UNITY_EDITOR
    // ────────────────────────────────────────
    //  [테스트] 재화 증감 애니메이션 확인
    //  Inspector에서 호출하거나 Update에서 키 입력으로 사용
    // ────────────────────────────────────────
    void Update()
    {


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1)) TestGoldIncrease();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TestGoldDecrease();
        if (Input.GetKeyDown(KeyCode.Alpha3)) TestCrystalIncrease();
        if (Input.GetKeyDown(KeyCode.Alpha4)) TestCrystalDecrease();

        if (Input.GetKeyDown(KeyCode.Alpha5)) TestHPDamage();
        if (Input.GetKeyDown(KeyCode.Alpha6)) TestHPHeal();
        if (Input.GetKeyDown(KeyCode.Alpha7)) TestEXPGain();

        if (Input.GetKeyDown(KeyCode.U)) TestUnlockAll();

        if (Input.GetKeyDown(KeyCode.T))
        {
            Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>().SetInfo("Travel_Icon", LocalizeKey.GuildText, LocalizeKey.TestKey);
        }

        if (Input.GetKeyDown(KeyCode.F1)) TestJumpToQuest(1);
        if (Input.GetKeyDown(KeyCode.F2)) TestJumpToQuest(2);
        if (Input.GetKeyDown(KeyCode.F3)) TestJumpToQuest(3);

        if (Input.GetKeyDown(KeyCode.A))
        {
            Managers.Game.AddItem(10001, 1); // 예시: 아이템 ID 10001을 1개 추가
        }
        if (Input.GetKeyDown(KeyCode.S))
            Managers.UI.ShowToast("산들바람 숲(잎구)");
        if (Input.GetKeyDown(KeyCode.E))
        {
            Managers.Game.AddItem(20001, 1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Managers.Game.RemoveItem(10001, 1);
            Managers.Game.RemoveItem(20001, 1);

        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(10001);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            // TODO : 전투로 이동
            //Managers.Quest.CheckButtonClick(BTN_JOURNEY);
            //ClosePopupUI();
            Managers.UI.CloseAllPopupUI();

            UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
            gameScene?.EnterAreaWithFade(AreaType.Battle);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Managers.Game.Player.State == State.Idle)
                Managers.Game.Player.State = State.Moving;
            else if (Managers.Game.Player.State == State.Moving)
                Managers.Game.Player.State = State.Attack;
            else if (Managers.Game.Player.State == State.Attack)
                Managers.Game.Player.State = State.Idle;
        }

#endif
    }

    [ContextMenu("Test/Unlock All")]
    public void TestUnlockAll()
    {
        // 하단 탭 버튼 전체 해금
        foreach (TabType tab in System.Enum.GetValues(typeof(TabType)))
            Managers.Game.UnlockBtmButton(tab);

        // 마을 이동/연금공방 버튼 해금
        Managers.Game.AddUnlockedBtn(UNLOCK_TRAVEL_BUTTON);
        Managers.Game.AddUnlockedBtn(UNLOCK_ALCHEMY_BUTTON);

        // 하단 탭 UI 즉시 갱신
        RefreshBtmButtonsLockState();

        // Village1 버튼 그룹 재생성 (연금공방 버튼 반영)
        EnterArea(AreaType.Village1);
    }

    [ContextMenu("Test/Jump to Quest 1")]
    public void TestJumpToQuest1() => TestJumpToQuest(1);

    [ContextMenu("Test/Jump to Quest 2")]
    public void TestJumpToQuest2() => TestJumpToQuest(2);

    [ContextMenu("Test/Jump to Quest 3")]
    public void TestJumpToQuest3() => TestJumpToQuest(3);

    void TestJumpToQuest(int questId)
    {
        Managers.Quest.JumpToQuest(questId);
    }

    [ContextMenu("Test/Gold +500")]
    public void TestGoldIncrease() => Managers.Game.Gold += 500;

    [ContextMenu("Test/Gold -500")]
    public void TestGoldDecrease() => Managers.Game.Gold = Mathf.Max(0, Managers.Game.Gold - 500);

    [ContextMenu("Test/Crystal +100")]
    public void TestCrystalIncrease() => Managers.Game.Crystal += 100;

    [ContextMenu("Test/Crystal -100")]
    public void TestCrystalDecrease() => Managers.Game.Crystal = Mathf.Max(0, Managers.Game.Crystal - 100);

    // ── HP / EXP 바 애니메이션 테스트 ──────────────────────────
    float _testHp  = 100f;
    float _testExp = 0f;
    const float TEST_MAX_HP  = 100f;
    const float TEST_MAX_EXP = 100f;

    [ContextMenu("Test/HP -30 (Hurt)")]
    public void TestHPDamage()
    {
        _testHp = Mathf.Max(0f, _testHp - 30f);
        UpdateHP(_testHp, TEST_MAX_HP);
    }

    [ContextMenu("Test/HP +20 (Heal)")]
    public void TestHPHeal()
    {
        _testHp = Mathf.Min(TEST_MAX_HP, _testHp + 20f);
        UpdateHP(_testHp, TEST_MAX_HP);
    }

    [ContextMenu("Test/EXP +25")]
    public void TestEXPGain()
    {
        _testExp = Mathf.Min(TEST_MAX_EXP, _testExp + 25f);
        UpdateEXP(_testExp, TEST_MAX_EXP);
    }
#endif
}
