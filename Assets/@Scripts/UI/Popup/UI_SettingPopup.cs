using DG.Tweening;
using UnityEngine;

public class UI_SettingPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        SettingPopupFrame,
        _1,
        Xbutton,
    }

    enum Texts
    {
        SettingPopupBGMText,
        SettingPopupSFXText,
        SettingPopupEffectsTitle,
        SettingPopupPopupText,
        SettingPopupShakeText,
        GameObject,
    }

    enum Buttons
    {
        SettingPopupBackground,
        KorBtn,
        EngBtn,
        SettingPopupBGMONBtn,
        SettingPopupBGMOFFBtn,
        SettingPopupSFXONBtn,
        SettingPopupSFXOFFBtn,
        SettingPopupPopupONBtn,
        SettingPopupPopupOFFBtn,
        SettingPopupShakeONBtn,
        SettingPopupShakeOFFBtn,
        Image,
    }

    enum GameObjects
    {
        FIXED,
        InFrame,
        SettingPopup,
        SettingPopupLanguage,
        SettingPopupSound,
        SettingPopupEffects,
    }

    #endregion

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

        DOTweenAnimation popupAnim = GetButton((int)Buttons.SettingPopupBackground).gameObject.GetComponent<DOTweenAnimation>();
        OpenAnimation(popupAnim);
        GetButton((int)Buttons.SettingPopupBackground).gameObject.BindEvent(() => OnClickBG(popupAnim));

        return true;
    }

    public void OpenAnimation(DOTweenAnimation doanim)
    {
        doanim.DOKill();
        doanim.CreateTween();
        doanim.tween.SetAutoKill(false);
        doanim.tween.OnRewind(() =>
        {
            ClosePopupUI();
        });

        doanim.tween.Restart();
    }

    public void OnClickBG(DOTweenAnimation doanim)
    {
        doanim.tween.Goto(doanim.tween.Duration(), false);
        doanim.tween.PlayBackwards();
    }
}