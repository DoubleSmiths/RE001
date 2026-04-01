using UnityEngine;

public class UI_MapSelectPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        Xbutton,
        MapSelectPopupFrame,
        MapSelectPopupTitleBar,
        MapSelectPopupMapImage,
        MapSelectPopupMapImageFrame,
        MapSelectPopupMapNameFrame,
        Decor_description_line_Image,
        MapSelectPopupBtmFrame,
        PlaceType,
        TownType,
        EFrame,
        FieldType,
        Image_2,
        BWFType,
        TitleText_1,
        BtmGroup,
    }

    enum Texts
    {
        MapSelectPopupTitleText,
        MapSelectPopupMapNameText,
        MapSelectPopup_CurrentLocationTxt,
        GameObject_1,
    }

    enum Buttons
    {
        MapSelectBackground,
        Buttonto_TownType,
        Buttonto_FieldType,
        ButtonMapSelectBack_TOP_1,
        MapSelectAsdalBtn,
        ButtonMapSelectBack_TOP,
        ButtonBWFType,
        MapSelectBWF_EntryBtn,
        MapSelectBWFBtn,
        ButtonMapSelectBack,
        ButtonMapSelectGO,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        MapSelectPopup,
        to_TownButtonQuestVFX,
        to_FieldQuestVFX,
        AsdalButtonQuestVFX,
        MapSelectPopupActiveEf,
        ButtontoBWFVFX,
        MapSelectBWF_EntryVFX,
    }

    #endregion


    public override bool Init()
    {
        if (base.Init() == false)
            return false;


        return true;
    }

}
