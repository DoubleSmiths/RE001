using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_LanguageSelectPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // 현재 설정된 언어가 선택 되어야함

    // 로컬라이징
    // BackgroundText : 탭하여 닫기
    // ConfirmButtonText : 확인

    // KoreanText : 고유 언어로 각각 표시
    // EnglishText 
    // JapaneseText 
    // SimplifiedText
    // TraditionalText
    // FranceText

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
    }
    enum Buttons
    {
        ConfirmButton,
        BackgroundButton,
    }
    enum Toggles
    {
        KoreanToggle,
        EnglishToggle,
        JapaneseToggle,
        SimplifiedToggle,
        TraditionalToggle,
        FranceToggle,
    }
    enum Texts
    {
        BackgroundText,
        ConfirmButtonText,

        KoreanText,
        EnglishText,
        JapaneseText,
        SimplifiedText,
        TraditionalText,
        FranceText,
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
        BindToggle(typeof(Toggles));

        GetToggle((int)Toggles.KoreanToggle).gameObject.BindEvent(OnClickKoreanToggle);
        GetToggle((int)Toggles.KoreanToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.EnglishToggle).gameObject.BindEvent(OnClickEnglishToggle);
        GetToggle((int)Toggles.EnglishToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.JapaneseToggle).gameObject.BindEvent(OnClickJapaneseToggle);
        GetToggle((int)Toggles.JapaneseToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.SimplifiedToggle).gameObject.BindEvent(OnClickSimplifiedToggle);
        GetToggle((int)Toggles.SimplifiedToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.TraditionalToggle).gameObject.BindEvent(OnClickTraditionalToggle);
        GetToggle((int)Toggles.TraditionalToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.FranceToggle).gameObject.BindEvent(OnClickFranceToggle);
        GetToggle((int)Toggles.FranceToggle).GetOrAddComponent<UI_ButtonAnimation>();


        GetButton((int)Buttons.ConfirmButton).gameObject.BindEvent(OnClickConfirmButton);
        GetButton((int)Buttons.ConfirmButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);


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

    #region Toggles
    void OnClickKoreanToggle() // 한국어
    {
        // 한국어 선택 (이후 확인버튼 누르면 언어 변경)
    }
    void OnClickEnglishToggle() // 영어
    {
        // 영어 선택 (이후 확인버튼 누르면 언어 변경)
    }
    void OnClickJapaneseToggle() // 일본어
    {
        // 일본어 선택 (이후 확인버튼 누르면 언어 변경)
    }
    void OnClickSimplifiedToggle() // 중국어 간체
    {
        // 중국어 간체 선택 (이후 확인버튼 누르면 언어 변경)
    }
    void OnClickTraditionalToggle() // 중국어 번체
    {
        // 중국어 번체 선택 (이후 확인버튼 누르면 언어 변경)
    }
    void OnClickFranceToggle() // 프랑스어
    {
        // 프랑스어 선택 (이후 확인버튼 누르면 언어 변경)
    }
    #endregion



    void OnClickConfirmButton() // 확인 버튼
    {
        // 선택한 언어를 저장 하고 팝업 닫기
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickBackgroundButton() // 터치하여 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);
    }
}
