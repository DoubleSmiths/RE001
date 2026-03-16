using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_AccountPassPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // AccountPassScrollContentObject :  UI_AccountPassItem이 들어갈 부모 개체

    // 로컬라이징
    // BackgroundText : 터치하여 닫기
    // AchievementTitleText : 업적
    // AccountPassDescriptionText : 일정 계정 레벨을 올릴 때마다 보상을 받을 수 있습니다.
    // FreePassTitleText : Free
    // RarePassButtonText : 레어 패스
    // RarePassPriceText : 레어 패스 가격
    // EpicPassButtonText : 에픽 패스
    // EpicPassPriceText : 에픽 패스 가격
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        AccountPassScrollContentObject,
    }
    enum Buttons
    {
        BackButton,

        RarePassButton,
        EpicPassButton,
    }
    enum Texts
    {
        BackgroundText,
        AchievementTitleText,
        AccountPassDescriptionText,

        FreePassTitleText,
        RarePassButtonText,
        RarePassPriceText,
        EpicPassButtonText,
        EpicPassPriceText,
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

        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnClickBackgroundButton);

        GetButton((int)Buttons.RarePassButton).gameObject.BindEvent(OnClickRarePassButton);
        GetButton((int)Buttons.RarePassButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.EpicPassButton).gameObject.BindEvent(OnClickEpicPassButton);
        GetButton((int)Buttons.EpicPassButton).GetOrAddComponent<UI_ButtonAnimation>();



        // 테스트용
#if UNITY_EDITOR

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

    void OnClickRarePassButton()
    {
        // 레어 패스 구매, 결제모듈 호출
    }
    void OnClickEpicPassButton()
    {
        // 에픽 패스 구매, 결제모듈 호출
    }

    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.UI.ClosePopupUI(this);
    }

}
