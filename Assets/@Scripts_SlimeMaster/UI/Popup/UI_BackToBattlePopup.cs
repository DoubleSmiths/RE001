using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_BackToBattlePopup : UI_Popup
{
    #region UI 기능 리스트
    // 로컬라이징
    // BackToBattleTitleText : 이어서 하기
    // BackToBattleContentText : 진행중인 전투가 있습니다.\n계속하시겠습니까?
    // ConfirmText : OK
    // CancelText : 취소

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
    }
    enum Buttons
    {
        ConfirmButton,
        CancelButton,
    }

    enum Texts
    {
        BackToBattleTitleText,
        BackToBattleContentText,
        ConfirmText,
        CancelText,
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

        GetButton((int)Buttons.ConfirmButton).gameObject.BindEvent(OnClickConfirmButton);
        GetButton((int)Buttons.ConfirmButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.CancelButton).gameObject.BindEvent(OnClickCancelButton);
        GetButton((int)Buttons.CancelButton).GetOrAddComponent<UI_ButtonAnimation>();


        // 테스트용
#if UNITY_EDITOR

        //TextBindTest();
#endif
        #endregion

        return true;
    }

    public void SetInfo()
    {

        Refresh();
    }

    void Refresh()
    {

    }

    void OnClickConfirmButton()
    {
        Managers.Sound.PlayButtonClick();
        // 이전 플레이하던 게임으로 되돌아가기\
        Managers.Scene.LoadScene(Define.Scene.GameScene, transform);

    }
    void OnClickCancelButton()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.ClearContinueData();
        Managers.UI.ClosePopupUI(this);
    }

}
