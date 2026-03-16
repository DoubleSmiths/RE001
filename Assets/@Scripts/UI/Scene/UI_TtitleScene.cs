using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
        // �׽�Ʈ��
        GetButton((int)Buttons.StartButton).gameObject.BindEvent(() =>
        {
            if (isPreload)
                Managers.Scene.LoadScene(Define.Scene.HubScene, transform);
        });
        GetButton((int)Buttons.StartButton).gameObject.SetActive(false);

        return true;
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
