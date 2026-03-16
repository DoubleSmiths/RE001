using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BeginnerSupportRewardPopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        ContentObject,
    }
    enum Buttons
    {
        BackgroundButton,
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

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

        #endregion

        Refresh();
        return true;
    }

    void Refresh()
    {

    }

    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.UI.ClosePopupUI(this);
    }
}
