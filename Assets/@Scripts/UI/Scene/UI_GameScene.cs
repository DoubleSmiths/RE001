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
    }
    #endregion

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

        // 퀘스트 이벤트
        Managers.Quest.OnTabUnlocked += OnTabUnlocked;

        SetInfo();
        RefreshTabLocks();

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
    //  SetInfo – 씬 진입 시 전체 갱신
    // ────────────────────────────────────────
    public void SetInfo()
    {
        RefreshCurrency();
        RefreshPlayerBar();
        RefreshBestiaryFollowing();

        Managers.Game.OnResourceChanged += RefreshCurrency;
    }

    // ────────────────────────────────────────
    //  재화
    // ────────────────────────────────────────
    void RefreshCurrency()
    {
        GetText((int)Texts.MainGoldText).text    = Managers.Game.Gold.ToString("N0");
        GetText((int)Texts.MainCrystalText).text = Managers.Game.Crystal.ToString("N0");
    }

    // ────────────────────────────────────────
    //  플레이어 바 (HP / EXP / 레벨)
    // ────────────────────────────────────────
    void RefreshPlayerBar()
    {
        int lv = Managers.Game.Level;
        GetText((int)Texts.RankTxt).text = $"Lv.{lv}";
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
        GetText((int)Texts.RankTxt).text = $"Lv.{newLevel}";
    }

    // ────────────────────────────────────────
    //  도감 – 동행 중인 미니몬 수
    // ────────────────────────────────────────
    void RefreshBestiaryFollowing()
    {
        int companionId = Managers.Game.CompanionMinMonId;
        bool hasCompanion = companionId != -1;

        GetObject((int)GameObjects.BTMButtonGroup); // 참조 확인용
        // 동행 박스: 동행 중일 때만 표시
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
        // New 뱃지 숨김 처리
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
        // TODO: UI_SettingsPopup 열기
    }

    void OnClickCharacter()
    {
        // TODO: UI_CharacterPopup 열기
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
}
