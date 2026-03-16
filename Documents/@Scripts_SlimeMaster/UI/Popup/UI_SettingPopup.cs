using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_SettingPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // UseIDValueText : 유저 아이디
    // VersionValueText : 현재 클라이언트의 버전 표시

    // 버튼
    // SoundEffectOffButton, SoundEffectOnButton : 효과음 온오프 기능 (상태 저장)
    // BackgroundSoundOffButton, BackgroundSoundOnButton : BGM 온오프 기능 (상태 저장)
    // JoystickFixedOffButton, JoystickFixedOnButton : 조이스틱 고정 온오프 기능 (상태 저장)

    // 로컬라이징 텍스트
    // SettingTlileText : 설정
    // UserInfoText : 유저 정보
    // SoundEffectText : 효과음
    // BackgroundSoundText : 배경음
    // JoystickText : 조이스틱
    // LanguageText : 언어
    // TermsOfServiceButtonText : 서비스 이용약관
    // PrivacyPolicyButtonText : 개인정보 처리방침
    // FeedbackButtonText : Feedback
    // ErrorFixButtonText : Error Fix

    #endregion


    #region Enum
    enum GameObjects
    {
        ContentObject,
        //LanguageObject
    }
    enum Buttons
    {
        BackgroundButton,
        SoundEffectOffButton,
        SoundEffectOnButton,
        BackgroundSoundOffButton,
        BackgroundSoundOnButton,
        JoystickFixedOffButton,
        JoystickFixedOnButton,
        //LanguageButton,
        //TermsOfServiceButton,
        //PrivacyPolicyButton,
    }

    enum Texts
    {
        SettingTlileText,
        //UserInfoText,
        //UseIDValueText,
        SoundEffectText,
        BackgroundSoundText,
        JoystickText,
        //LanguageText,
        //LanguageValueText,
        //TermsOfServiceButtonText,
        //PrivacyPolicyButtonText,
        VersionValueText
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

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

        GetButton((int)Buttons.SoundEffectOffButton).gameObject.BindEvent(EffectSoundOn);
        GetButton((int)Buttons.SoundEffectOnButton).gameObject.BindEvent(EffectSoundOff);

        GetButton((int)Buttons.BackgroundSoundOffButton).gameObject.BindEvent(BackgroundSoundOn);
        GetButton((int)Buttons.BackgroundSoundOnButton).gameObject.BindEvent(BackgroundSoundOff);

        GetButton((int)Buttons.JoystickFixedOffButton).gameObject.BindEvent(OnCllickJoystickFixed);
        GetButton((int)Buttons.JoystickFixedOnButton).gameObject.BindEvent(OnCllickJoystickNonFixed);

        GetText((int)Texts.VersionValueText).text = $"버전 : {Application.version}";

        if (Managers.Game.BGMOn == false)
        {
            BackgroundSoundOff();
        } 
        else
        {
            BackgroundSoundOn();
        }

        if (Managers.Game.EffectSoundOn == false)
        {
            EffectSoundOff();
        }
        else
        {
            EffectSoundOn();
        }

        if (Managers.Game.JoystickType == Define.JoystickType.Fixed)
        {
            OnCllickJoystickFixed();
        }
        else
        {
            OnCllickJoystickNonFixed();
        }


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

    void EffectSoundOn()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.EffectSoundOn = true;
        GetButton((int)Buttons.SoundEffectOnButton).gameObject.SetActive(true);
        GetButton((int)Buttons.SoundEffectOffButton).gameObject.SetActive(false);
    }

    void EffectSoundOff()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.EffectSoundOn = false;
        GetButton((int)Buttons.SoundEffectOnButton).gameObject.SetActive(false);
        GetButton((int)Buttons.SoundEffectOffButton).gameObject.SetActive(true);
    }

    void BackgroundSoundOn()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.BGMOn = true;
        GetButton((int)Buttons.BackgroundSoundOnButton).gameObject.SetActive(true);
        GetButton((int)Buttons.BackgroundSoundOffButton).gameObject.SetActive(false);
    }

    void BackgroundSoundOff()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.BGMOn = false;
        GetButton((int)Buttons.BackgroundSoundOnButton).gameObject.SetActive(false);
        GetButton((int)Buttons.BackgroundSoundOffButton).gameObject.SetActive(true);
    }

    void OnCllickJoystickFixed()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.JoystickType = Define.JoystickType.Fixed;
        GetButton((int)Buttons.JoystickFixedOnButton).gameObject.SetActive(true);
        GetButton((int)Buttons.JoystickFixedOffButton).gameObject.SetActive(false);
    }

    void OnCllickJoystickNonFixed()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.JoystickType = Define.JoystickType.Flexible;
        GetButton((int)Buttons.JoystickFixedOnButton).gameObject.SetActive(false);
        GetButton((int)Buttons.JoystickFixedOffButton).gameObject.SetActive(true);
    }

    void OnClickBackgroundButton() // 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);
    }

}
