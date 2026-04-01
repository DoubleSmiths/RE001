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
        PlaceTypeTownImage,
        AdventureImage,
        TownType,
        TownImage,
        EFrame,
        FieldType,
        FieldTypeAdventureImage,
        BWFType,
        BWFTypeFrame,
        BWFTypeTitle,
        TitleTextFrame,
        MapSelectBWF_EntryBtnFrame,
        MapSelectBWF_EntryBtnImage,
        MapSelectBWFBtnFrame,
        MapSelectBWFBtnImage,
        BtmGroup,
    }

    enum Texts
    {
        XbuttonText,
        MapSelectPopupTitleText,
        MapSelectPopupMapNameText,
        PlaceTypeTownText,
        AdventrueText,
        TownTypeTownText,
        MapSelectAsdalBtnText,
        MapSelectPopup_CurrentLocationTxt,
        FieldTypeAdventureText,
        ButtonBWFTypeText,
        BWFTypeTitleText,
        MapSelectBWF_EntryBtnText,
        MapSelectBWFBtnText,
        ButtonMapSelectBackText,
        ButtonMapSelectGOText,
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
        TownTypeContent,
        AsdalButtonQuestVFX,
        MapSelectPopupActiveEf,
        Content,
        ButtontoBWFVFX,
        BWFTypeContent,
        MapSelectBWF_EntryVFX,
    }

    #endregion

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        #endregion

        return true;
    }

    public void SetInfo()
    {
        // TODO : 맵 선택 팝업에 필요한 정보들 세팅
    }
}
