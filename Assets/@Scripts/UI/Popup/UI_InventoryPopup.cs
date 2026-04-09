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
        ImportantBagBtnText,
    }

    enum Buttons
    {
        InventoryPopupBackground,
        UI_InvSlot,
        InventorySortBtn,
        ImportantBagBtn,
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
    private const int IMPORTANT_SLOT_COUNT = 5;
    private const int NORMAL_SLOT_COUNT = 25;
    private const int OPEN_SLOT_COUNT = 15;


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
        _frameRT = GetObject((int)GameObjects.InventoryPopup).GetComponent<RectTransform>();

        GetButton((int)Buttons.InventoryPopupBackground).gameObject.BindEvent(OnClickBG);
        GetButton((int)Buttons.InventorySortBtn).gameObject.BindEvent(OnClickInventorySortBtn);
        GetButton((int)Buttons.ImportantBagBtn).gameObject.BindEvent(OnClickImportantBagBtn);
        GetImage((int)Images.ImportantThingsBox).gameObject.SetActive(false);
        RefreshImportantBagNewTxt();

        SetInfo();
        SetText();

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

        // 인벤토리를 일반/중요 아이템으로 분류
        List<InventoryItem> normalItems = new List<InventoryItem>();

        foreach (InventoryItem item in Managers.Game.Inventory)
        {
            Debug.Log($"Inventory Item: DataId={item.DataId}, Count={item.Count}");

            if (item.DataId > Define.ITEM_ID_MIN && item.DataId < Define.ITEM_ID_MIN + 1000)
                normalItems.Add(item);

            // TODO : 아이템을 클릭하면 아이템 정보 팝업이 뜨고, NEW 안나오게 설정하기.
            //item.IsNew = false;
        }

        // 일반 아이템 슬롯 세팅
        Transform contentParent = GetObject((int)GameObjects.Content).transform;
        for (int i = 0; i < NORMAL_SLOT_COUNT; i++)
        {
            UI_InvSlot invSlot = Managers.UI.MakeSubItem<UI_InvSlot>(contentParent, pooling: false);
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

            if (i < OPEN_SLOT_COUNT)
            {
                // 슬롯이 열려있는 경우
                invSlot.SetOpenBlock();
            }

        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Managers.Game.OnInventoryNewChanged -= RefreshImportantBagNewTxt;
    }

    public void SetText()
    {
        // TODO : 언어에 따라 텍스트 변경하기
        //GetText((int)Texts.ImportantBagBtnText).text = Util.Localize("ImportantBagBtnText");

    }

    private ItemData GetItemData(int dataId)
    {
        ItemData data;
        if (!Managers.Data.ItemDic.TryGetValue(dataId, out data) && !Managers.Data.RequisiteItemDic.TryGetValue(dataId, out data))
            return null;
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

    public void OnClickImportantBagBtn()
    {
        GetImage((int)Images.ImportantThingsBox).gameObject.SetActive(true);

        // 여기서 중요 아이템만
        ClearChildren(GetObject((int)GameObjects.ImportantThingsList).transform);

        List<InventoryItem> importantItems = new List<InventoryItem>();

        foreach (InventoryItem item in Managers.Game.Inventory)
        {
            Debug.Log($"Inventory Item: DataId={item.DataId}, Count={item.Count}");

            if (item.DataId > Define.IMPORTANT_ITEM_ID_MIN && item.DataId < Define.IMPORTANT_ITEM_ID_MIN + 1000)
                importantItems.Add(item);

            // TODO : 아이템을 클릭하면 아이템 정보 팝업이 뜨고, NEW 안나오게 설정하기.
            //item.IsNew = false;
        }

        // 중요 아이템 슬롯 세팅
        Transform importantParent = GetObject((int)GameObjects.ImportantThingsList).transform;
        for (int i = 0; i < IMPORTANT_SLOT_COUNT; i++)
        {
            UI_ImportantSlot slot = Managers.UI.MakeSubItem<UI_ImportantSlot>(importantParent, pooling: false);
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

            Managers.Game.OnInventoryNewChanged += RefreshImportantBagNewTxt;
        }
    }

    public void RefreshImportantBagNewTxt()
    {
        if (this == null) return; // Destroy 예약 후 OnDestroy 전에 호출될 수 있으므로 방어

        // TODO : 중요 아이템만 확인하게 해야 됨.
        bool flag = false;
        foreach (InventoryItem item in Managers.Game.Inventory)
        {
            if (item.DataId > Define.IMPORTANT_ITEM_ID_MIN && item.DataId < Define.IMPORTANT_ITEM_ID_MIN + 1000 && item.IsNew)
            {
                flag = true;
                GetText((int)Texts.ImportantBagNewTxt)?.gameObject?.SetActive(true);
                break;
            }
        }

        if (!flag) GetText((int)Texts.ImportantBagNewTxt)?.gameObject?.SetActive(false);
    }
}
