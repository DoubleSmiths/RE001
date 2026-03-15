using UnityEngine;

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
        PlayerRankText,
        PlayerLevelText,
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

        return true;
    }

    public void SetInfo()
    {
        // TODO Set Player Info
    }


}
