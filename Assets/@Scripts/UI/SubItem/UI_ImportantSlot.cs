using UnityEngine;
using Data;

public class UI_ImportantSlot : UI_Base
{
    #region Enum

    enum Images
    {
        GameObject,
        Frame,
        Img,
    }

    enum Texts
    {
        AmountTxt,
        ImportantSlotNewTxt,
    }

    enum Buttons
    {
        UI_ImportantSlot,
    }

    enum GameObjects
    {
    }

    #endregion

    private void Awake() { Init(); }

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

    public void SetInfo(InventoryItem inventoryItem = null, ItemData itemData = null)
    {
        bool hasItem = inventoryItem != null && itemData != null;

        // 아이템 아이콘
        Sprite sprite = hasItem ? Managers.Resource.Load<Sprite>(itemData.SpriteName) : null;
        GetImage((int)Images.Img).sprite = sprite;
        GetImage((int)Images.Img).gameObject.SetActive(sprite != null);

        // 개수 (2개 이상일 때만 표시)
        bool showCount = hasItem && inventoryItem.Count > 1;
        GetText((int)Texts.AmountTxt).text = showCount ? inventoryItem.Count.ToString() : "";
        GetText((int)Texts.AmountTxt).gameObject.SetActive(showCount);
    }
}
