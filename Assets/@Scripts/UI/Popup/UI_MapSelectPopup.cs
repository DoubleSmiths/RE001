using UnityEngine;
using static Define;

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

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.MapSelectBackground).gameObject.BindEvent(ClosePopupUI);
        GetButton((int)Buttons.Buttonto_FieldType).gameObject.BindEvent(OnClickFieldType);
        GetButton((int)Buttons.ButtonBWFType).gameObject.BindEvent(OnClickBWFType);
        GetButton((int)Buttons.MapSelectBWF_EntryBtn).gameObject.BindEvent(OnClickBWFEntry);
        GetButton((int)Buttons.ButtonMapSelectGO).gameObject.BindEvent(OnClickGo);
        GetButton((int)Buttons.ButtonMapSelectBack).gameObject.BindEvent(OnClickBack);

        Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
    }

    public void SetInfo()
    {
        GetObject((int)GameObjects.TownTypeContent).SetActive(false);
        GetObject((int)GameObjects.BWFTypeContent).SetActive(false);
    }

    void RefreshNotify()
    {
        var list = Managers.Quest.GetCurrentNotifyList();

        bool journeyOrExploreNotify = list.Exists(n => n.BtnID == BTN_JOURNEY)
                                   || list.Exists(n => n.BtnID == BTN_EXPLORE);
        bool mapNotify   = list.Exists(n => n.BtnID == BTN_MAP_BREEZING_WIND_FOREST);
        bool stageNotify = list.Exists(n => n.BtnID == BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE);

        GetObject((int)GameObjects.to_FieldQuestVFX)?.SetActive(journeyOrExploreNotify);
        GetObject((int)GameObjects.ButtontoBWFVFX)?.SetActive(mapNotify);
        GetObject((int)GameObjects.MapSelectBWF_EntryVFX)?.SetActive(stageNotify);
    }

    void OnClickFieldType()
    {
        Managers.Quest.CheckButtonClick(BTN_EXPLORE);
        GetObject((int)GameObjects.TownTypeContent).SetActive(false);
        GetObject((int)GameObjects.BWFTypeContent).SetActive(true);
    }

    void OnClickBWFType()
    {
        Managers.Quest.CheckButtonClick(BTN_MAP_BREEZING_WIND_FOREST);
    }

    void OnClickBWFEntry()
    {
        Managers.Quest.CheckButtonClick(BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE);
    }

    void OnClickGo()
    {
        Managers.Quest.CheckButtonClick(BTN_JOURNEY);
        //ClosePopupUI();
        Managers.UI.CloseAllPopupUI();

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene?.EnterAreaWithFade(AreaType.Battle);
    }

    void OnClickBack()
    {
        GetObject((int)GameObjects.TownTypeContent).SetActive(false);
        GetObject((int)GameObjects.BWFTypeContent).SetActive(false);
    }
}
