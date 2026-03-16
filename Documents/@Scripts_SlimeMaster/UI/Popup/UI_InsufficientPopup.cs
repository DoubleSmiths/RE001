using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_InsufficientPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // InsufficientItemImage : 모자란 재화의 아이콘
    // InsufficientCostValueText : 모자란 재화의 수, 빨간색(#F3614D)으로 보여준다

    // 로컬라이징 텍스트
    // InsufficientPopupTitleText
    // InsufficientText
    // GoDiamondsText
    // GoNowText

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        InsufficientCostObject,
    }
    enum Buttons
    {
        BackgroundButton,
        GoNowButton,
    }

    enum Texts
    {
        BackgroundText,
        InsufficientPopupTitleText,
        InsufficientText,
        InsufficientCostValueText,
        GoDiamondsText,
        GoNowText,
    }

    enum Images
    {
        InsufficientItemImage, // 모자란 코스트 아이콘
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



#if UNITY_EDITOR

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
       
        // 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.InsufficientCostObject).GetComponent<RectTransform>());
    }

    void OnClickBackgroundButton() // 배경 눌러 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);

    }
    void OnClickGoNowButton() // 지금 이동 버튼
    {
        // GoDiaButton : 상점 페이지의 다이아 구매 탭으로 이동

    }

}
