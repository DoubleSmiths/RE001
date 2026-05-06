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
    }

    enum Buttons
    {
        StartButton,
    }

    enum Texts
    {
        StartText
    }

    enum GameObjects
    {
        Slider,
    }
    #endregion

    bool isPreload = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.Slider).GetComponent<Slider>().value = 0;
        // 테스트
        GetButton((int)Buttons.StartButton).gameObject.BindEvent(() =>
        {
            if (isPreload)
                Managers.Scene.LoadScene(Define.Scene.GameScene, transform);
        });
        GetButton((int)Buttons.StartButton).gameObject.SetActive(false);

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
        var startTmp = GetText((int)Texts.StartText);
        if (startTmp != null)
        {
            ApplyFont(startTmp, FontType.UI);
            startTmp.text = Util.Localize(LocalizeKey.StartText);
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            GetObject((int)GameObjects.Slider).GetComponent<Slider>().value = (float)count / totalCount;
            if (count == totalCount)
            {
                GetButton((int)Buttons.StartButton).gameObject.SetActive(true);
                isPreload = true;
                Managers.Data.Init();
                Managers.Game.Init();
                Managers.Font.Init();
                Managers.Sound.Init();
                Managers.Quest.Init();
                Managers.SubQuest.Init();
                StartAnimation();
            }
        });
    }

    void StartAnimation()
    {
        GetText((int)Texts.StartText).DOFade(0, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCubic).Play();
        //GetImage((int)Images.LoadingIcon).gameObject.SetActive(false);
        //GetImage((int)Images.Panel).DOFade(0, 1f).OnComplete(
        //    () => { GetImage((int)Images.Panel).gameObject.SetActive(false); });
    }
}
