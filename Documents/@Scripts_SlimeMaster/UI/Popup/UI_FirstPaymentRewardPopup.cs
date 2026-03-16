using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_FirstPaymentRewardPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // 보상의 내용을 테이블로 만들기엔 너무 작은양이라 Define의 DicFirstPayReward에 기입함 
    // FirstRewardBackgroundImage : 보상의 등급에따라 색상 변경
    // FirstRewardImage : 보상의 아이콘
    // FirstRewardCountText : 보상 개수

    // SecondRewardBackgroundImage : 보상의 등급에따라 색상 변경
    // SecondRewardImage : 보상의 아이콘
    // SecondRewardCountText : 보상 개수

    // ThirdRewardBackgroundImage : 보상의 등급에따라 색상 변경
    // ThirdRewardImage : 보상의 아이콘
    // ThirdRewardCountText : 보상 개수

    // FourthRewardBackgroundImage : 보상의 등급에따라 색상 변경
    // FourthRewardImage : 보상의 아이콘
    // FourthRewardCountText : 보상 개수

    // 로컬라이징
    // BackgroundText : 터치하여 닫기
    // FirstPaymentRewardPopupTitleText : 첫 결재 보상
    // FirstPaymentRewardDescriptionText : 최초 결제 시 다음 보상을 1회 지급합니다.
    // GoNowText : 바로 가기

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
    }
    enum Buttons
    {
        BackgroundButton,
        GoNowButton,
    }
    enum Texts
    {
        BackgroundText,
        FirstPaymentRewardPopupTitleText,
        FirstPaymentRewardDescriptionText,

        FirstRewardCountText,
        SecondRewardCountText,
        ThirdRewardCountText,
        FourthRewardCountText,

        GoNowText,
    }

    enum Images
    {
        FirstRewardBackgroundImage,
        FirstRewardImage,
        SecondRewardBackgroundImage,
        SecondRewardImage,
        ThirdRewardBackgroundImage,
        ThirdRewardImage,
        FourthRewardBackgroundImage,
        FourthRewardImage,

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
        BindImage(typeof(Images));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.GoNowButton).gameObject.BindEvent(OnClickGoNowButton);
        GetButton((int)Buttons.GoNowButton).GetOrAddComponent<UI_ButtonAnimation>();

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
    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.UI.ClosePopupUI(this);
    }

    // 상점으로 이동 버튼
    void OnClickGoNowButton()
    {
        // 모든 팝업을 닫고 상점 탭으로 이동
    }
}
