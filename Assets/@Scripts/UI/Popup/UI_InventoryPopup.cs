using UnityEngine;

public class UI_InventoryPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        InventoryPopupFrame,
        ScrollView,
        Viewport,
        ScrollbarVertical,
        Handle,
        Image_1,
        ImportantThingsBox,
        Xbutton,
    }

    enum Texts
    {
        title,
        InvSlotNewTxt,
        ImportantBagNewTxt,
        ImportantSlotNewTxt,
        ImportantSlotNewTxt_1,
        ImportantSlotNewTxt_2,
        ImportantSlotNewTxt_3,
        ImportantSlotNewTxt_4,
    }

    enum Buttons
    {
        InventoryPopupBackground,
        UI_InvSlot,
        InventorySortBtn,
        ImportantBagBtn,
        ImportantSlot,
        ImportantSlot_1,
        ImportantSlot_2,
        ImportantSlot_3,
        ImportantSlot_4,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        InventoryPopup,
        InventoryPopupTitle,
        InventoryList,
        Content,
        SlidingArea,
        ImportantThingsList,
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

        GetButton((int)Buttons.InventoryPopupBackground).gameObject.BindEvent(OnClickInventoryPopupBackground);

        SetInfo();

        return true;
    }

    public void SetInfo()
    {
        
       
        
        return;
    }

    public void OnClickInventoryPopupBackground()
    {
        ClosePopupUI();
    }

}
