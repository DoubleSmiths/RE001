using DG.Tweening;
using UnityEditor.U2D;
using UnityEngine;
using static Define;
using static Util;

public class UI_Village1BtnGroup : UI_AreaBtnGroup
{
    #region Enum

    enum Images
    {
        GuildButtonIcon,
        AlchemyButtonIcon,
        MonasteryButtonIcon,
        TravelButtonIcon,
    }

    enum Texts
    {
        Village1NameText,
        GuildButtonText,
        AlchemyButtonText,
        AlchemyButtonNewTxt,
        MonasteryButtonText,
        MonasteryButtonNewTxt,
        TravelButtonText,
        TravelButtonNewTxt,
        Village1GuildNameText,
    }

    enum Buttons
    {
        GuildButton,
        AlchemyButton,
        MonasteryButton,
        TempButton1,
        TempButton2,
        TravelButton,
        Village1GuildTalkButton,
        Village1GuildQuestButton,
        Village1GuildExitButton,
        Village1AlchemyTalkButton,
        CustomOrderButton,
        Village1AlchemyExitButton,
        Village1MonasteryTalkButton,
        MonasteryClassButton,
        Village1MonasteryExitButton,
    }

    enum GameObjects
    {
        Village1MainBtnGroup,
        Village1GuildVFX,
        Village1AlchemyVFX_Sub1,
        MonasteryVFX_Sub1,
        Village1TravelQuestVFX,
        Village1GuildBtnGroup,
        Village1GuildTalkVFX,
        Village1AlchemyBtnGroup,
        Village1MonasteryBtnGroup,
    }

    #endregion



    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetImage((int)Images.GuildButtonIcon).gameObject.BindEvent(OnClickGuild);
        GetImage((int)Images.AlchemyButtonIcon).gameObject.BindEvent(OnClickAlchemy);
        GetImage((int)Images.TravelButtonIcon).gameObject.BindEvent(OnClickTravel);
        GetButton((int)Buttons.Village1GuildExitButton).gameObject.BindEvent(OnClickMain);
        GetButton((int)Buttons.Village1AlchemyExitButton).gameObject.BindEvent(OnClickMain);
        GetButton((int)Buttons.Village1MonasteryExitButton).gameObject.BindEvent(OnClickMain);
        GetButton((int)Buttons.Village1GuildTalkButton).gameObject.BindEvent(OnClickVillage1GuildTalkButton);

        SetInfo();

        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UnlockButtonAnim(GetButton((int)Buttons.AlchemyButton).gameObject, GetImage((int)Images.AlchemyButtonIcon), GetText((int)Texts.AlchemyButtonText));
        }
    }

    void SetInfo()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1MainBtnGroup);

        //GetImage((int)Images.GuildButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.AlchemyButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.MonasteryButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.TravelButtonIcon).gameObject.SetActive(false);
        //GetText((int)Texts.GuildButtonText).gameObject.SetActive(false);
        GetText((int)Texts.AlchemyButtonText).gameObject.SetActive(false);
        GetText((int)Texts.AlchemyButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonText).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonText).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonNewTxt).gameObject.SetActive(false);

        //GetObject((int)GameObjects.Village1GuildVFX).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.Village1GuildVFX]));
        GetObject((int)GameObjects.Village1GuildVFX).gameObject.SetActive(true);
        GetObject((int)GameObjects.Village1AlchemyVFX_Sub1).gameObject.SetActive(false);
        GetObject((int)GameObjects.MonasteryVFX_Sub1).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1TravelQuestVFX).gameObject.SetActive(false);

        GetText((int)Texts.AlchemyButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonNewTxt).gameObject.SetActive(false);


        // TODO : 해금된 버튼이 있으면 Image, Text 를 SetActive(true)로 변경.
        // TODO : 이벤트 ID 확인 후 VFX 켜기, NEW Text 켜기, 

        OnClickMain();
    }

    void ShowGroup(GameObjects group)
    {
        GetObject((int)GameObjects.Village1MainBtnGroup).SetActive(group == GameObjects.Village1MainBtnGroup);
        GetObject((int)GameObjects.Village1GuildBtnGroup).SetActive(group == GameObjects.Village1GuildBtnGroup);
        GetObject((int)GameObjects.Village1AlchemyBtnGroup).SetActive(group == GameObjects.Village1AlchemyBtnGroup);
        GetObject((int)GameObjects.Village1MonasteryBtnGroup).SetActive(group == GameObjects.Village1MonasteryBtnGroup);
    }

    void HideAllGroup()
    {
        GetObject((int)GameObjects.Village1MainBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1GuildBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1AlchemyBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1AlchemyBtnGroup).gameObject.SetActive(false);
    }

    void OnClickMain()
    {
        HideAllGroup();

        GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
        _gameScene.FadeInOut();

        ShowGroup(GameObjects.Village1MainBtnGroup);

        GetButton((int)Buttons.GuildButton).gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        GetButton((int)Buttons.AlchemyButton).gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        GetButton((int)Buttons.MonasteryButton).gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        GetButton((int)Buttons.TempButton1).gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        GetButton((int)Buttons.TempButton2).gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        GetButton((int)Buttons.TravelButton).gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);

        seq.AppendCallback(() =>
        {
            GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Background_Village1");

            // TODO NPC들 제거
            GameObject guildNPC = GameObject.Find("GuildNPC");
            if (guildNPC != null) Destroy(guildNPC);
        });

        seq.Append(GetText((int)Texts.Village1NameText).gameObject.GetComponent<RectTransform>()
            .DOAnchorPos(Vector2.zero, 0.25f)
            .From(new Vector2(-170f, 0f))
            .SetEase(Ease.OutQuad));

        seq.AppendInterval(0.1f);

        seq.Append(GetText((int)Texts.Village1NameText).gameObject.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(170f, 0f), 0.15f)
            .SetEase(Ease.OutQuad));

        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => GetButton((int)Buttons.GuildButton).gameObject.GetComponent<DOTweenAnimation>().DORestart());
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => GetButton((int)Buttons.AlchemyButton).gameObject.GetComponent<DOTweenAnimation>().DORestart());
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => GetButton((int)Buttons.MonasteryButton).gameObject.GetComponent<DOTweenAnimation>().DORestart());
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => GetButton((int)Buttons.TempButton1).gameObject.GetComponent<DOTweenAnimation>().DORestart());
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => GetButton((int)Buttons.TempButton2).gameObject.GetComponent<DOTweenAnimation>().DORestart());
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => GetButton((int)Buttons.TravelButton).gameObject.GetComponent<DOTweenAnimation>().DORestart());

        //seq.Append(GetText((int)Texts.Village1NameText).gameObject.GetComponent<RectTransform>()
        //    .DOAnchorPos(new Vector2(170f, 0f), 0.15f)
        //    .SetEase(Ease.OutQuad));

    }

    void OnClickGuild()
    {
        HideAllGroup();

        // TODO : Player Move Anim, Fade In Out, Chang BG + Add NPC Sprite
        GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
        _gameScene.FadeInOut();

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.AppendCallback(() =>
            {
                GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Background_Guild");
                Managers.Resource.Instantiate("GuildNPC").GetComponent<Animator>().Play("Receptionist");
                ShowGroup(GameObjects.Village1GuildBtnGroup);
            });
    }

    void OnClickAlchemy()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1AlchemyBtnGroup);
    }

    void OnClickTravel()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1MonasteryBtnGroup);
    }

    void OnClickVillage1GuildTalkButton()
    {
        // TODO : 길드 NPC와의 대화창 생성 
        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
                 groupKey: "GuildQuest1Text",
                 speakerName: "접수원 아가씨",
                 portrait: null,
                 onFinished: () => Debug.Log("대화 종료"));
    }
}
