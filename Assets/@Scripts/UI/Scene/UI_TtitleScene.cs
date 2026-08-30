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

                // ⚠️ 로컬라이즈 사전은 **지금** 준비된다(Managers.Data.Init 이후).
                //  Init()은 Awake에서 도는터라 그때 불러도 사전이 비어 있어
                //  Util.Localize 가 **키를 그대로 돌려준다** — 실제로 빌드된 게임의
                //  시작 버튼에 "StartText" 라는 키가 그대로 찍혀 나왔다.
                //  Managers.Game 도 Awake 시점엔 null 이라 언어 변경 구독도 못 걸었다.
                Managers.Game.OnLanguageChanged -= RefreshTexts;   // 타이틀 재진입 시 중복 구독 방지
                Managers.Game.OnLanguageChanged += RefreshTexts;
                RefreshTexts();

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
