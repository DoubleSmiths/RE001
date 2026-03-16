using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UI_MergeAllResultPopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        MergeAlIScrollContentObject,
        ContentObject,
    }

    enum Buttons
    {
        BackgroundButton,
    }

    enum Texts
    {
        MergeAllPopupTitleText,
        BackgroundText
    }
    #endregion

    List<Equipment> _items = new List<Equipment>();

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

        #endregion
        Refresh();

        return true;
    }
    public void SetInfo(List<Equipment> items)
    {
        _items = items;

        Refresh();
    }

    void Refresh()
    {
        GameObject container = GetObject((int)GameObjects.MergeAlIScrollContentObject);
        container.DestroyChilds();

        foreach (Equipment item in _items)
        {
            UI_EquipItem equipItem = Managers.Resource.Instantiate("UI_EquipItem", pooling: true).GetOrAddComponent<UI_EquipItem>();
            equipItem.transform.SetParent(container.transform);
            equipItem.SetInfo(item, UI_ItemParentType.EquipInventoryGroup);
        }

    }


    void OnClickBackgroundButton() // 화면 터치하여 닫기
    {
        Managers.UI.ClosePopupUI(this);
        //gameObject.SetActive(false);
    }
}
