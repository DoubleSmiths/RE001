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
        SettingPopupLanguageText,
        SettingPopupSoundText,
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

        GetButton((int)Buttons.KorBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.KorBtn)));
        GetButton((int)Buttons.EngBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.EngBtn)));
        GetButton((int)Buttons.SettingPopupBGMONBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.SettingPopupBGMONBtn)));
        GetButton((int)Buttons.SettingPopupSFXONBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.SettingPopupSFXONBtn)));
        GetButton((int)Buttons.SettingPopupPopupONBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.SettingPopupPopupONBtn)));
        GetButton((int)Buttons.SettingPopupShakeONBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.SettingPopupShakeONBtn)));

        GetButton((int)Buttons.SettingPopupBGMONBtn).gameObject.BindEvent(OnClickBGMSoundON);
        GetButton((int)Buttons.SettingPopupSFXONBtn).gameObject.BindEvent(OnClickSFXSoundON);
        GetButton((int)Buttons.SettingPopupPopupONBtn).gameObject.BindEvent(OnClickPopupOn);
        GetButton((int)Buttons.SettingPopupShakeONBtn).gameObject.BindEvent(OnClickShakeOn);

        RefreshButtons();
        RefreshLanguageButtons();
        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        OpenAnimation();

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // enum Texts 모든 entry — 폰트 + 한영 localization 일괄 처리.
    void RefreshTexts()
    {
        SetTextLocalized(Texts.SettingPopupLanguageText, LocalizeKey.LanguageText);
        SetTextLocalized(Texts.SettingPopupSoundText, LocalizeKey.SoundText);
        SetTextLocalized(Texts.SettingPopupBGMText, LocalizeKey.BGMText);
        SetTextLocalized(Texts.SettingPopupSFXText, LocalizeKey.SFXText);
        SetTextLocalized(Texts.SettingPopupEffectsTitle, LocalizeKey.EffectsTitle);
        SetTextLocalized(Texts.SettingPopupPopupText, LocalizeKey.PopupText);
        SetTextLocalized(Texts.SettingPopupShakeText, LocalizeKey.CameraShakeText);
        // GameObject — enum 잘못 박힌 entry로 보임. 텍스트 용도 아님 → 폰트만 적용.
        var dummyTmp = GetText((int)Texts.GameObject);
        if (dummyTmp != null) ApplyFont(dummyTmp, FontType.UI);
    }

    void SetTextLocalized(Texts texts, string locKey)
    {
        var tmp = GetText((int)texts);
        if (tmp == null) return;
        ApplyFont(tmp, FontType.UI);
        tmp.text = Util.Localize(locKey);
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
        UpdateToggleButton((int)Buttons.SettingPopupBGMONBtn, Managers.Game.IsBGSoundOn);
        UpdateToggleButton((int)Buttons.SettingPopupSFXONBtn, Managers.Game.IsSFXOn);
        UpdateToggleButton((int)Buttons.SettingPopupPopupONBtn, Managers.Game.IsCreateAtkTextOn);
        UpdateToggleButton((int)Buttons.SettingPopupShakeONBtn, Managers.Game.IsShakeOn);
    }

    void UpdateToggleButton(int buttonType, bool isOn)
    {
        Image image = GetButton(buttonType).GetComponent<Image>();
        image.sprite = Managers.Resource.Load<Sprite>(isOn ? Define.MYTHIC_BUTTON_CHECK : Define.MYTHIC_BUTTON_X);
        image.color = isOn ? _greenColor : Color.white;
    }

    public void OnClickBGMSoundON()
    {
        Managers.Game.IsBGSoundOn = !Managers.Game.IsBGSoundOn;
        UpdateToggleButton((int)Buttons.SettingPopupBGMONBtn, Managers.Game.IsBGSoundOn);
    }

    public void OnClickSFXSoundON()
    {
        Managers.Game.IsSFXOn = !Managers.Game.IsSFXOn;
        UpdateToggleButton((int)Buttons.SettingPopupSFXONBtn, Managers.Game.IsSFXOn);
    }

    public void OnClickPopupOn()
    {
        Managers.Game.IsCreateAtkTextOn = !Managers.Game.IsCreateAtkTextOn;
        UpdateToggleButton((int)Buttons.SettingPopupPopupONBtn, Managers.Game.IsCreateAtkTextOn);
    }

    public void OnClickShakeOn()
    {
        Managers.Game.IsShakeOn = !Managers.Game.IsShakeOn;
        UpdateToggleButton((int)Buttons.SettingPopupShakeONBtn, Managers.Game.IsShakeOn);
    }

}