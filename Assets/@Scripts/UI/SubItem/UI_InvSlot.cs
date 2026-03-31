using Data;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class UI_InvSlot : UI_Base
{
    #region Enum

    enum Images
    {
        GameObject,
        Frame,
        Glow,
        Img,
        BlockImg,
    }

    enum Texts
    {
        AmountTxt,
        InvSlotNewTxt,
    }

    enum Buttons
    {
        //UI_InvSlot,
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

        // 등급 글로우
        GetImage((int)Images.Glow).gameObject.SetActive(hasItem);
        if (hasItem)
            GetImage((int)Images.Glow).color = GetGradeColor(itemData.Grade);

        // 새 아이템 표시
        GetText((int)Texts.InvSlotNewTxt).gameObject.SetActive(hasItem && inventoryItem.IsNew);

        GetImage((int)Images.GameObject).gameObject.BindEvent( () => { OnClickGameObject(inventoryItem); });
    }

    private Color GetGradeColor(EquipGrade grade)
    {
        switch (grade)
        {
            //case EquipGrade.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
            case EquipGrade.Uncommon: return new Color(0.6f, 0.2f, 0.8f);
            case EquipGrade.Rare:     return new Color(0.2f, 0.4f, 1f);
            case EquipGrade.Epic:     return new Color(0.6f, 0.2f, 0.8f);
            default:                  return new Color(0.7f, 0.7f, 0.7f);
        }
    }

    public void SetOpenBlock()
    {
        GetImage((int)Images.BlockImg).gameObject.SetActive(false);
    }

    void OnClickGameObject(InventoryItem inventoryItem)
    {
        inventoryItem.IsNew = false; // 클릭 시 "New" 상태 해제
        GetText((int)Texts.InvSlotNewTxt).gameObject.SetActive(inventoryItem.IsNew);

        // 아이템 상세 정보 UI 열기 (예시)
        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(inventoryItem.DataId);
    }
}
