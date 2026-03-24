using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

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

        GetButton((int)Buttons.KorBtn).gameObject.BindEvent(OnClickKorean);
        GetButton((int)Buttons.EngBtn).gameObject.BindEvent(OnClickEnglish);

        RefreshLanguageButtons();

        return true;
    }

    // ────────────────────────────────────────
    //  언어 선택
    // ────────────────────────────────────────
    void OnClickKorean()
    {
        if (Managers.Game.Language == Language.Korean) return;
        Managers.Game.Language = Language.Korean;
        RefreshLanguageButtons();
    }

    void OnClickEnglish()
    {
        if (Managers.Game.Language == Language.English) return;
        Managers.Game.Language = Language.English;
        RefreshLanguageButtons();
    }

    // 선택된 버튼: 불투명 / 미선택: 반투명
    void RefreshLanguageButtons()
    {
        bool isKorean = Managers.Game.Language == Language.Korean;
        SetButtonSelected(GetButton((int)Buttons.KorBtn), isKorean);
        SetButtonSelected(GetButton((int)Buttons.EngBtn), !isKorean);
    }

    void SetButtonSelected(Button btn, bool selected)
    {
        var group = btn.transform.GetComponent<CanvasGroup>();
        if (group != null)
            group.alpha = selected ? 1f : 0.4f;
        else
            btn.image.color = selected ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }

    // ────────────────────────────────────────
    //  팝업 애니메이션
    // ────────────────────────────────────────
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