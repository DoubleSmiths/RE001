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
        SettingPopupEtcTitle,
        SettingPopupFullscreenText,
        SettingPopupCreditsText,
        SettingPopupQuitText,
        Txt_SettingPopupTitle_02,
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
        SettingPopupFullscreenONBtn,
        SettingPopupCreditsBtn,
        SettingPopupQuitBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        SettingPopup,
        SettingPopupLanguage,
        SettingPopupSound,
        SettingPopupEffects,
        SettingPopupEtc,
        SettingPopupFrame,
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

        // 시스템: 크레딧·게임 종료·(PC) 전체 화면 — Tools/Release 로 프리팹에 추가된 줄
        GetButton((int)Buttons.SettingPopupCreditsBtn).gameObject.BindEvent(() => Managers.UI.ShowPopupUI<UI_CreditsPopup>());
        GetButton((int)Buttons.SettingPopupQuitBtn).gameObject.BindEvent(OnClickQuit);
        GetButton((int)Buttons.SettingPopupFullscreenONBtn).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.SettingPopupFullscreenONBtn)));
        GetButton((int)Buttons.SettingPopupFullscreenONBtn).gameObject.BindEvent(OnClickFullscreen);
        if (Application.isMobilePlatform)
        {
            // 모바일엔 창 모드가 없다 — 전체 화면 줄을 숨기고 그만큼 창을 줄인다(내용은 위쪽 기준 배치).
            GetText((int)Texts.SettingPopupFullscreenText).gameObject.SetActive(false);
            GetButton((int)Buttons.SettingPopupFullscreenONBtn).gameObject.SetActive(false);
            var frame = GetObject((int)GameObjects.SettingPopupFrame).GetComponent<RectTransform>();
            frame.sizeDelta -= new Vector2(0f, FULLSCREEN_ROW_HEIGHT);
        }

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
        SetTextLocalized(Texts.Txt_SettingPopupTitle_02, "SettingText");
        SetTextLocalized(Texts.SettingPopupEtcTitle, "SystemText");
        SetTextLocalized(Texts.SettingPopupFullscreenText, "FullscreenText");
        SetTextLocalized(Texts.SettingPopupCreditsText, "CreditsText");
        SetTextLocalized(Texts.SettingPopupQuitText, "QuitGameText");
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
        UpdateToggleButton((int)Buttons.SettingPopupFullscreenONBtn, Screen.fullScreenMode != FullScreenMode.Windowed);
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

    const float FULLSCREEN_ROW_HEIGHT = 22f;

    // PC: 전체 화면(테두리 없는 창, 좌우 검은 여백) ↔ 세로 창. Unity 가 다음 실행에도 기억한다.
    void OnClickFullscreen()
    {
        bool toFullscreen = Screen.fullScreenMode == FullScreenMode.Windowed;
        if (toFullscreen)
        {
            var display = Screen.currentResolution;
            Screen.SetResolution(display.width, display.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            int h = Mathf.RoundToInt(Display.main.systemHeight * 0.85f);
            Screen.SetResolution(Mathf.RoundToInt(h * FixedResolution.TARGET_ASPECT), h, FullScreenMode.Windowed);
        }
        UpdateToggleButton((int)Buttons.SettingPopupFullscreenONBtn, toFullscreen);
    }

    void OnClickQuit()
    {
        Managers.Game?.FlushSave();
        Application.Quit();
    }

}