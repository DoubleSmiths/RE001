using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_HubScene : UI_Scene
{
    #region Enum
    enum Images
    {
        PlayerHPBarGuage,
        PlayerEXPBarGuage,
        PortraitImg,
        CurrencyBar,
        CharacterBtnNewIcon,
        BestiaryBtnNewIcon,
    }

    enum Texts
    {
        MainGoldText,
        MainCrystalText,
        PlayerHPBarText,
        PlayerEXPTextPercent,
        PlayerNameText,
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
        EnterBattleButton,
    }

    enum GameObjects
    {
        PlayerBar,
        BTMButtonGroup,
    }
    #endregion

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.OptionBtn).gameObject.BindEvent(() => OnClickOption());
        GetButton((int)Buttons.CharacterButton).gameObject.BindEvent(() => OnClickCharacter());
        GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(() => OnClickInventory());
        GetButton((int)Buttons.EquipmentButton).gameObject.BindEvent(() => OnClickEquipment());
        GetButton((int)Buttons.SkillButton).gameObject.BindEvent(() => OnClickSkill());
        GetButton((int)Buttons.BestiaryButton).gameObject.BindEvent(() => OnClickBestiary());
        GetButton((int)Buttons.EnterBattleButton).gameObject.BindEvent(() => OnClickEnterBattle());

        RefreshUI();

        Managers.Game.OnResourceChanged += RefreshCurrency;

        // 잠긴 탭 비활성화
        RefreshTabLocks();

        return true;
    }

    void OnDestroy()
    {
        if (Managers.Game != null)
            Managers.Game.OnResourceChanged -= RefreshCurrency;
    }

    public void RefreshUI()
    {
        RefreshCurrency();
        RefreshPlayerBar();
        RefreshPlayerName();
    }

    void RefreshCurrency()
    {
        GetText((int)Texts.MainGoldText).text    = Managers.Game.Gold.ToString("N0");
        GetText((int)Texts.MainCrystalText).text = Managers.Game.Crystal.ToString("N0");
    }

    void RefreshPlayerBar()
    {
        // HP / EXP 바는 PlayerController 이벤트 구독 후 갱신됨
        GetImage((int)Images.PlayerHPBarGuage).fillAmount  = 1f;
        GetImage((int)Images.PlayerEXPBarGuage).fillAmount = 0f;
    }

    void RefreshPlayerName()
    {
        GetText((int)Texts.PlayerNameText).text  = Managers.Game.PlayerName;
        GetText((int)Texts.PlayerLevelText).text = $"Lv.{Managers.Game.Level}";
    }

    void RefreshTabLocks()
    {
        // 잠긴 탭은 비활성화 (UnlockedTabs에 없는 탭)
        bool inventoryUnlocked  = Managers.Quest.IsTabUnlocked(TabType.Bag.ToString());
        bool equipmentUnlocked  = Managers.Quest.IsTabUnlocked(TabType.Equipment.ToString());
        bool skillUnlocked      = Managers.Quest.IsTabUnlocked(TabType.Skill.ToString());
        bool bestiaryUnlocked   = Managers.Quest.IsTabUnlocked(TabType.Bestiary.ToString());

        GetButton((int)Buttons.InventoryButton).interactable  = inventoryUnlocked;
        GetButton((int)Buttons.EquipmentButton).interactable  = equipmentUnlocked;
        GetButton((int)Buttons.SkillButton).interactable      = skillUnlocked;
        GetButton((int)Buttons.BestiaryButton).interactable   = bestiaryUnlocked;
    }

    // ── 버튼 핸들러 ──
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

    void OnClickEnterBattle()
    {
        Managers.Scene.LoadScene(Scene.BattleScene, transform);
    }

    // ── PlayerController 이벤트 연결 ──
    public void BindPlayer(PlayerController player)
    {
        player.OnHpChanged  += (cur, max) =>
        {
            GetImage((int)Images.PlayerHPBarGuage).fillAmount = cur / max;
            GetText((int)Texts.PlayerHPBarText).text = $"{(int)cur} / {(int)max}";
        };

        player.OnExpChanged += (cur, req) =>
        {
            GetImage((int)Images.PlayerEXPBarGuage).fillAmount = cur / req;
            GetText((int)Texts.PlayerEXPTextPercent).text = $"{(int)(cur / req * 100)}%";
        };

        player.OnLevelUp += (lv) =>
        {
            GetText((int)Texts.PlayerLevelText).text = $"Lv.{lv}";
        };
    }
}
