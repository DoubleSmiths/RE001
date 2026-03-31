using System.Collections.Generic;
using UnityEngine;
using Data;

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

    // 번호에 따라 장비, 소비, 퀘스트, 기타로 구분 (지금은 테스트로 9000 이상은 중요 아이템으로 간주)
    private const int IMPORTANT_ITEM_ID_MIN = 9000;
    private const int IMPORTANT_SLOT_COUNT  = 5;
    private const int NORMAL_SLOT_COUNT     = 15;

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
        GetButton((int)Buttons.InventorySortBtn).gameObject.BindEvent(OnClickInventorySortBtn);

        SetInfo();

        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnClickInventorySortBtn();
        }
    }

    public void SetInfo()
    {
        // 기존 슬롯 제거
        ClearChildren(GetObject((int)GameObjects.Content).transform);
        ClearChildren(GetObject((int)GameObjects.ImportantThingsList).transform);

        // 인벤토리를 일반/중요 아이템으로 분류
        List<InventoryItem> normalItems    = new List<InventoryItem>();
        List<InventoryItem> importantItems = new List<InventoryItem>();

        foreach (InventoryItem item in Managers.Game.Inventory)
        {
            Debug.Log($"Inventory Item: DataId={item.DataId}, Count={item.Count}");

            if (item.DataId >= IMPORTANT_ITEM_ID_MIN)
                importantItems.Add(item);
            else
                normalItems.Add(item);
        }

        // 중요 아이템 슬롯 세팅
        Transform importantParent = GetObject((int)GameObjects.ImportantThingsList).transform;
        for (int i = 0; i < IMPORTANT_SLOT_COUNT; i++)
        {
            UI_ImportantSlot slot = Managers.UI.MakeSubItem<UI_ImportantSlot>(importantParent);
            slot.gameObject.name = $"ImportantSlot_{i}";
            if (i < importantItems.Count)
            {
                ItemData data = GetItemData(importantItems[i].DataId);
                slot.SetInfo(importantItems[i], data);
            }
            else
            {
                slot.SetInfo();
            }
        }

        // 일반 아이템 슬롯 세팅
        Transform contentParent = GetObject((int)GameObjects.Content).transform;
        for (int i = 0; i < NORMAL_SLOT_COUNT; i++)
        {
            UI_InvSlot invSlot = Managers.UI.MakeSubItem<UI_InvSlot>(contentParent);
            invSlot.gameObject.name = $"InvSlot_{i}";
            if (i < normalItems.Count)
            {
                ItemData data = GetItemData(normalItems[i].DataId);
                invSlot.SetInfo(normalItems[i], data);
            }
            else
            {
                invSlot.SetInfo();
            }
        }
    }

    private ItemData GetItemData(int dataId)
    {
        ItemData data;
        Managers.Data.ItemDic.TryGetValue(dataId, out data);
        return data;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(parent.GetChild(i).gameObject);
    }

    public void OnClickInventoryPopupBackground()
    {
        ClosePopupUI();
    }

    public void OnClickInventorySortBtn()
    {
        Managers.Game.Inventory.Sort((a, b) => a.DataId.CompareTo(b.DataId));
        SetInfo();
    }
}
