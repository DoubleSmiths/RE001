using DG.Tweening;
using TMPro;
using UnityEngine;
using static Define;

public class UI_GameScene : UI_Scene
{
    #region Enum
    enum Images
    {
        CharacterBtnNewIcon,
        BestiaryBtnNewIcon,
        BestiaryBtnFollowingBox,
        CurrencyBar,
        PlayerHPBarGuage,
        PlayerEXPBarGuage,
        PortraitImg,
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
        PlayerEXPTextPercent,
        PlayerRankText,
        PlayerLevelText,
    }

    enum Buttons
    {
        OptionBtn,
        CharacterButton,
        InventoryButton,
        PotionButton,
        EquipmentButton,
        SkillButton,
        BestiaryButton,
    }

    enum GameObjects
    {
        PlayerBar,
        BTMButtonGroup,
        QuestFollow,
        AreaBtnGroupSlot,
    }
    #endregion

    UI_AreaBtnGroup _currentBtnGroup;

    // 재화 Lerp용 표시값
    float _displayGold;
    float _displayCrystal;

    static readonly Color _goldColor    = Color.white;                      // #FFFFFF
    static readonly Color _crystalColor = Color.white;                      // #FFFFFF
    static readonly Color _increaseColor = new Color(0f, 1f, 0f);           // #00FF00
    static readonly Color _decreaseColor = new Color(1f, 0f, 0f);           // #FF0000

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

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

        // Fade
        FadeInOut(GetImage((int)Images.ForFadeImage));

        // 퀘스트 이벤트
        Managers.Quest.OnTabUnlocked += OnTabUnlocked;

        SetInfo();
        RefreshTabLocks();

        // TODO : 플레이어의 현재 구역에 따라 EnterArea 호출
        EnterArea(AreaType.Village1);

        return true;
    }

    void OnDestroy()
    {
        if (Managers.Quest != null)
            Managers.Quest.OnTabUnlocked -= OnTabUnlocked;
        if (Managers.Game != null)
            Managers.Game.OnResourceChanged -= RefreshCurrency;
    }

    // ────────────────────────────────────────
    //  구역 전환
    // ────────────────────────────────────────
    public void EnterArea(AreaType area)
    {
        if (_currentBtnGroup != null)
            Managers.Resource.Destroy(_currentBtnGroup.gameObject);

        Transform slot = GetObject((int)GameObjects.AreaBtnGroupSlot).transform;
        string prefabName = $"UI_{area}BtnGroup";

        GameObject go = Managers.Resource.Instantiate(prefabName, slot);
        _currentBtnGroup = go.GetComponent<UI_AreaBtnGroup>();
        _currentBtnGroup.Setup(this);
    }

    // ────────────────────────────────────────
    //  SetInfo – 씬 진입 시 전체 갱신
    // ────────────────────────────────────────
    public void SetInfo()
    {
        // 재화 초기값 즉시 표시 (Lerp 없이)
        _displayGold    = Managers.Game.Gold;
        _displayCrystal = Managers.Game.Crystal;
        GetText((int)Texts.MainGoldText).text    = Managers.Game.Gold.ToString("N0");
        GetText((int)Texts.MainCrystalText).text = Managers.Game.Crystal.ToString("N0");

        RefreshPlayerBar();
        RefreshBestiaryFollowing();

        Managers.Game.OnResourceChanged += RefreshCurrency;
    }

    // ────────────────────────────────────────
    //  재화
    // ────────────────────────────────────────
    void RefreshCurrency()
    {
        int newGold    = Managers.Game.Gold;
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
        GetText((int)Texts.PlayerRankText).text  = Managers.Game.PlayerName;

        // TODO: 직업에 따라 PortraitImg 스프라이트 교체
        // Managers.Resource.LoadAsync<Sprite>($"Portrait_{jobType}", sprite =>
        //     GetImage((int)Images.PortraitImg).sprite = sprite);
    }

    public void UpdateHP(float current, float max)
    {
        GetImage((int)Images.PlayerHPBarGuage).fillAmount = max > 0 ? current / max : 0f;
        GetText((int)Texts.PlayerHPBarText).text = $"{(int)current} / {(int)max}";
    }

    public void UpdateEXP(float current, float required)
    {
        GetImage((int)Images.PlayerEXPBarGuage).fillAmount = required > 0 ? current / required : 0f;
        GetText((int)Texts.PlayerEXPTextPercent).text =
            required > 0 ? $"{(int)(current / required * 100)}%" : "0%";
    }

    public void OnLevelUp(int newLevel)
    {
        GetText((int)Texts.PlayerLevelText).text = $"Lv.{newLevel}";
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
        SetTabInteractable(Buttons.InventoryButton,  Managers.Quest.IsTabUnlocked(TabType.Bag.ToString()));
        SetTabInteractable(Buttons.PotionButton,     Managers.Quest.IsTabUnlocked(TabType.Potion.ToString()));
        SetTabInteractable(Buttons.EquipmentButton,  Managers.Quest.IsTabUnlocked(TabType.Equipment.ToString()));
        SetTabInteractable(Buttons.SkillButton,      Managers.Quest.IsTabUnlocked(TabType.Skill.ToString()));
        SetTabInteractable(Buttons.BestiaryButton,   Managers.Quest.IsTabUnlocked(TabType.Bestiary.ToString()));
    }

    void SetTabInteractable(Buttons btn, bool interactable)
    {
        var button = GetButton((int)btn);
        button.interactable = interactable;
        switch (btn)
        {
            case Buttons.InventoryButton:
                GetText((int)Texts.InventoryButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.PotionButton:
                GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.EquipmentButton:
                GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.SkillButton:
                GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.BestiaryButton:
                GetText((int)Texts.BestiaryBtnNewTxt).gameObject.SetActive(false);
                GetImage((int)Images.BestiaryBtnNewIcon).gameObject.SetActive(false);
                break;
        }
    }

    void OnTabUnlocked(string tabName)
    {
        RefreshTabLocks();
    }

    // ────────────────────────────────────────
    //  New 뱃지
    // ────────────────────────────────────────
    public void ShowNewBadge(TabType tab)
    {
        switch (tab)
        {
            case TabType.Bag:
                GetText((int)Texts.InventoryButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Potion:
                GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Equipment:
                GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(true);
                GetImage((int)Images.CharacterBtnNewIcon).gameObject.SetActive(true);
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
        player.OnHpChanged  += (cur, max) => UpdateHP(cur, max);
        player.OnExpChanged += (cur, req)  => UpdateEXP(cur, req);
        player.OnLevelUp    += (lv)        => OnLevelUp(lv);
        UpdateHP(player.Hp, player.MaxHp);
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

    void OnClickCharacter()
    {
        // TODO: UI_CharacterPopup 열기
        Managers.UI.ShowPopupUI<UI_CharacterPopup>();
    }

    void OnClickInventory()
    {
        // TODO: UI_InventoryPopup 열기
    }

    void OnClickPotion()
    {
        // TODO: UI_PotionPopup 열기
    }

    void OnClickEquipment()
    {
        // TODO: UI_EquipmentPopup 열기
    }

    void OnClickSkill()
    {
        // TODO: UI_SkillPopup 열기
    }

    void OnClickBestiary()
    {
        // TODO: UI_BestiaryPopup 열기
    }

#if UNITY_EDITOR
    // ────────────────────────────────────────
    //  [테스트] 재화 증감 애니메이션 확인
    //  Inspector에서 호출하거나 Update에서 키 입력으로 사용
    // ────────────────────────────────────────
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) TestGoldIncrease();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TestGoldDecrease();
        if (Input.GetKeyDown(KeyCode.Alpha3)) TestCrystalIncrease();
        if (Input.GetKeyDown(KeyCode.Alpha4)) TestCrystalDecrease();
    }

    [ContextMenu("Test/Gold +500")]
    public void TestGoldIncrease()  => Managers.Game.Gold += 500;

    [ContextMenu("Test/Gold -500")]
    public void TestGoldDecrease()  => Managers.Game.Gold = Mathf.Max(0, Managers.Game.Gold - 500);

    [ContextMenu("Test/Crystal +100")]
    public void TestCrystalIncrease() => Managers.Game.Crystal += 100;

    [ContextMenu("Test/Crystal -100")]
    public void TestCrystalDecrease() => Managers.Game.Crystal = Mathf.Max(0, Managers.Game.Crystal - 100);
#endif
}
