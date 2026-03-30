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
        // TODO : 인벤토리 팝업에 필요한 정보 세팅 (인벤토리 슬롯, 중요 아이템 슬롯 등)
        // 기본적으로 15개가 사용할 수 있는 슬롯이고, 10개는 해금을 해야 쓸 수 있는 슬롯, 5개는 중요 아이템 슬롯이다.

        // 인벤토리 슬롯 세팅
        // 일반적인 아이템은 UI_InvSlot 프리팹을 사용하여 슬롯을 생성하고, 중요 아이템은 UI_ImportantSlot 프리팹을 사용하여 슬롯을 생성.

        // UI_ImportantSlot은 ImportantThingsList에 자식으로 생성되어야 하며, UI_InvSlot은 InventoryList의 Content에 자식으로 생성되어야 함.

        for (int i = 0; i < 5; i++)
        {
            // 중요 아이템 슬롯 생성
            UI_ImportantSlot importantSlot = Managers.UI.MakeSubItem<UI_ImportantSlot>(GetObject((int)GameObjects.ImportantThingsList).transform);
            // TODO : 중요 아이템 슬롯에 아이템 정보 세팅 (아이템 아이콘, 이름 등)
        }

        for (int i = 0; i < 15; i++)
        {
            // 일반 아이템 슬롯 생성
            UI_InvSlot invSlot = Managers.UI.MakeSubItem<UI_InvSlot>(GetObject((int)GameObjects.Content).transform);
            // TODO : 일반 아이템 슬롯에 아이템 정보 세팅 (아이템 아이콘, 이름 등)
        }

        return;
    }

    public void OnClickInventoryPopupBackground()
    {
        ClosePopupUI();
    }

}
