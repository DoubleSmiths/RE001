using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    //InventoryScrollContentObject : MaterialInfoButton 아이템을 넣는 부모 개체
    // 인벤토리 탭은 추후 추가 예정

    // 로컬라이징
    // BackgroundText
    // InventoryPopupTitleText

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        InventoryScrollContentObject,
    }
    enum Buttons
    {
        BackgroundButton,
    }

    enum Texts
    {
        BackgroundText,
        InventoryPopupTitleText,
    }
    #endregion

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

        // 테스트용
#if UNITY_EDITOR
        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);


        
        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    public void SetInfo()
    {

        Refresh();
    }

    void Refresh()
    {

    }

    void OnClickBackgroundButton() // 배경 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);

    }
}
