using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_TtitleScene : UI_Scene
{
    #region Enum
    enum Images
    {
        Panel,
        LoadingIcon,
        TitleLogo,
    }

    enum Buttons
    {
        StartButton,
        SettingButton,
        QuitButton,
    }

    enum Texts
    {
        StartText,
        SettingText,
        QuitText,
        VersionText,
    }

    enum GameObjects
    {
        Slider,
    }
    #endregion

    // 언어별 로고 — 씬에 직접 물려 있어 빌드에 포함된다(Tools/Release/타이틀 화면 구성).
    [SerializeField] Sprite _logoKo;
    [SerializeField] Sprite _logoEn;

    const string TITLE_BGM = "BGM_BreezeForest";

    bool isPreload = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        var slider = GetObject((int)GameObjects.Slider).GetComponent<Slider>();
        slider.value = 0;
        slider.interactable = false;    // 진행 표시일 뿐 — 손잡이를 끌 수 있으면 안 된다

        GetButton((int)Buttons.StartButton).gameObject.BindEvent(() =>
        {
            if (isPreload)
                Managers.Scene.LoadScene(Define.Scene.GameScene, transform);
        });
        GetButton((int)Buttons.SettingButton).gameObject.BindEvent(() =>
        {
            if (isPreload && Managers.UI.GetPopupCount() == 0)
                Managers.UI.ShowPopupUI<UI_SettingPopup>();
        });
        GetButton((int)Buttons.QuitButton).gameObject.BindEvent(QuitGame);

        // 프리로드가 끝나기 전에는 누를 게 없다.
        GetButton((int)Buttons.StartButton).gameObject.SetActive(false);
        GetButton((int)Buttons.SettingButton).gameObject.SetActive(false);
        GetButton((int)Buttons.QuitButton).gameObject.SetActive(false);

        GetText((int)Texts.VersionText).text = "v" + Application.version;

        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    void RefreshTexts()
    {
        SetTextLocalized(Texts.StartText, LocalizeKey.StartText);
        SetTextLocalized(Texts.SettingText, "SettingText");
        SetTextLocalized(Texts.QuitText, "QuitGameText");

        // 로고: 사전 준비 전(Awake)에도 Language 는 PlayerPrefs·기기 언어로 바로 알 수 있다.
        bool korean = Managers.Game == null
            ? Application.systemLanguage == SystemLanguage.Korean
            : Managers.Game.Language == Language.Korean;
        var logo = GetImage((int)Images.TitleLogo);
        if (logo != null && _logoKo != null && _logoEn != null)
            logo.sprite = korean ? _logoKo : _logoEn;
    }

    void SetTextLocalized(Texts text, string key)
    {
        var tmp = GetText((int)text);
        if (tmp == null) return;
        ApplyFont(tmp, FontType.UI);
        tmp.text = Util.Localize(key);
    }

    static void QuitGame()
    {
        Managers.Game?.FlushSave();
        Application.Quit();
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        // 로고는 로딩 중에도 살짝 떠 있게 — 첫 화면이 멈춰 보이지 않도록.
        var logoRT = GetImage((int)Images.TitleLogo).rectTransform;
        logoRT.DOAnchorPosY(logoRT.anchoredPosition.y + 4f, 1.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject).Play();

        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            GetObject((int)GameObjects.Slider).GetComponent<Slider>().value = (float)count / totalCount;
            if (count == totalCount)
            {
                isPreload = true;
                Managers.Data.Init();
                Managers.Game.Init();
                Managers.Font.Init();
                Managers.Sound.Init();
                Managers.Quest.Init();
                Managers.SubQuest.Init();

                // ⚠️ 로컬라이즈 사전은 **지금** 준비된다(Managers.Data.Init 이후).
                //  Init()은 Awake에서 도는터라 그때 불러도 사전이 비어 있어
                //  Util.Localize 가 **키를 그대로 돌려준다** — 실제로 빌드된 게임의
                //  시작 버튼에 "StartText" 라는 키가 그대로 찍혀 나왔다.
                //  Managers.Game 도 Awake 시점엔 null 이라 언어 변경 구독도 못 걸었다.
                Managers.Game.OnLanguageChanged -= RefreshTexts;   // 타이틀 재진입 시 중복 구독 방지
                Managers.Game.OnLanguageChanged += RefreshTexts;
                RefreshTexts();

                ShowMenu();
            }
        });
    }

    // 로딩 표시(스피너·진행바)를 치우고 메뉴를 띄운다. 종료 버튼은 PC 에만 — 모바일은 OS 가 앱을 닫는다.
    void ShowMenu()
    {
        GetImage((int)Images.LoadingIcon).gameObject.SetActive(false);
        GetObject((int)GameObjects.Slider).SetActive(false);

        GetButton((int)Buttons.StartButton).gameObject.SetActive(true);
        GetButton((int)Buttons.SettingButton).gameObject.SetActive(true);
        GetButton((int)Buttons.QuitButton).gameObject.SetActive(!Application.isMobilePlatform);
        if (Application.isMobilePlatform)
        {
            // 종료 버튼이 없으면 설정 버튼을 가운데로
            var settingRT = GetButton((int)Buttons.SettingButton).GetComponent<RectTransform>();
            settingRT.anchoredPosition = new Vector2(0f, settingRT.anchoredPosition.y);
        }

        GetText((int)Texts.StartText).DOFade(0.25f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCubic).SetLink(gameObject).Play();
        Managers.Sound.PlayBgm(TITLE_BGM);
    }
}
