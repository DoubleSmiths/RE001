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
        __1,
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
        In_Frame,
        SettingPopup,
        SettingPopupLanguage,
        SettingPopupSound,
        SettingPopupEffects,
    }

    #endregion

    static readonly Color _lgButtonColor = new Color32(0xC5, 0x3F, 0x2A, 0xFF); // C53F2A
    static readonly Color _greenColor = new Color32(0x14, 0xFF, 0x00, 0xFF); // 14FF00

    private void Awake() { Init(); }

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

        _frameRT = GetObject((int)GameObjects.SettingPopup).GetComponent<RectTransform>();
        _openFromY = 350f;
        _closeToY = 350f;

        GetButton((int)Buttons.SettingPopupBackground).gameObject.BindEvent(() => OnClickBG());

        GetButton((int)Buttons.KorBtn).gameObject.BindEvent(OnClickKorean);
        GetButton((int)Buttons.EngBtn).gameObject.BindEvent(OnClickEnglish);

        // 버튼 클릭 애니메이션 (스펙: Scale 0.8 / 0.07s / OutQuad / Yoyo×2)
        AddClickAnim(GetButton((int)Buttons.KorBtn));
        AddClickAnim(GetButton((int)Buttons.EngBtn));
        AddClickAnim(GetButton((int)Buttons.SettingPopupBGMONBtn));
        AddClickAnim(GetButton((int)Buttons.SettingPopupSFXONBtn));
        AddClickAnim(GetButton((int)Buttons.SettingPopupPopupONBtn));
        AddClickAnim(GetButton((int)Buttons.SettingPopupShakeONBtn));

        GetButton((int)Buttons.SettingPopupBGMONBtn).gameObject.BindEvent(OnClickBGMSoundON);
        GetButton((int)Buttons.SettingPopupSFXONBtn).gameObject.BindEvent(OnClickSFXSoundON);
        GetButton((int)Buttons.SettingPopupPopupONBtn).gameObject.BindEvent(OnClickPopupOn);
        GetButton((int)Buttons.SettingPopupShakeONBtn).gameObject.BindEvent(OnClickShakeOn);

        RefreshButtons();
        RefreshLanguageButtons();

        OpenAnimation();

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
            group.alpha = selected ? 1f : 0.0f;
        else
            btn.image.color = selected ? _lgButtonColor : new Color(1f, 1f, 1f, 0.0f);
    }

    public void RefreshButtons()
    {
        OnClickBGMSoundON();
        OnClickSFXSoundON();
        OnClickPopupOn();
        OnClickShakeOn();
    }

    public void OnClickBGMSoundON()
    {
        Managers.Game.IsBGSoundOn = !Managers.Game.IsBGSoundOn;
        GetButton((int)Buttons.SettingPopupBGMONBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(Managers.Game.IsBGSoundOn ? Define.MYTHIC_BUTTON_CHECK : Define.MYTHIC_BUTTON_X);
        GetButton((int)Buttons.SettingPopupBGMONBtn).GetComponent<Image>().color = Managers.Game.IsBGSoundOn ? _greenColor : Color.white;
    }

    public void OnClickSFXSoundON()
    {
        Managers.Game.IsSFXOn = !Managers.Game.IsSFXOn;
        GetButton((int)Buttons.SettingPopupSFXONBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(Managers.Game.IsSFXOn ? Define.MYTHIC_BUTTON_CHECK : Define.MYTHIC_BUTTON_X);
        GetButton((int)Buttons.SettingPopupSFXONBtn).GetComponent<Image>().color = Managers.Game.IsSFXOn ? _greenColor : Color.white;
    }

    public void OnClickPopupOn()
    {
        Managers.Game.IsCreateAtkTextOn = !Managers.Game.IsCreateAtkTextOn;
        GetButton((int)Buttons.SettingPopupPopupONBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(Managers.Game.IsCreateAtkTextOn ? Define.MYTHIC_BUTTON_CHECK : Define.MYTHIC_BUTTON_X);
            GetButton((int)Buttons.SettingPopupPopupONBtn).GetComponent<Image>().color = Managers.Game.IsCreateAtkTextOn ? _greenColor : Color.white;
    }

    public void OnClickShakeOn()
    {
        Managers.Game.IsShakeOn = !Managers.Game.IsShakeOn;
        GetButton((int)Buttons.SettingPopupShakeONBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(Managers.Game.IsShakeOn ? Define.MYTHIC_BUTTON_CHECK : Define.MYTHIC_BUTTON_X);
        GetButton((int)Buttons.SettingPopupShakeONBtn).GetComponent<Image>().color = Managers.Game.IsShakeOn ? _greenColor : Color.white;
    }

}