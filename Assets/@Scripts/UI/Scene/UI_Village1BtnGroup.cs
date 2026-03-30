using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
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
        AlchemyCustomOrderButtonNewTxt,
        MonasteryCustomOrderButtonNewTxt,
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
        Village1GuildQuestVFX,
        GuildExitQuestVFX,
        Village1AlchemyBtnGroup,
        Village1AlchemyTalkVFX,
        Village1AlchemyTalkVFX_Sub1,
        Village1AlchemyQuestVFX,
        AlchemyExitQuestVFX,
        Village1MonasteryBtnGroup,
        Village1MonasteryTalkVFX,
        Village1MonasteryTalkVFX_Sub1,
        Village1MonasteryQuestVFX,
        MonasteryExitQuestVFX,
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
        GetButton((int)Buttons.Village1GuildExitButton).gameObject.BindEvent(() =>
        {
            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            OnClickMain();
        });
        GetButton((int)Buttons.Village1AlchemyExitButton).gameObject.BindEvent(() =>
        {
            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            OnClickMain();
        });
        GetButton((int)Buttons.Village1MonasteryExitButton).gameObject.BindEvent(() =>
        {
            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            OnClickMain();
        });
        GetButton((int)Buttons.Village1GuildTalkButton).gameObject.BindEvent(OnClickVillage1GuildTalkButton);

        Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();

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

        // TODO : 해금된 아이콘과 텍스트는 SetActive(true)로 해야됨.
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

        //GetObject((int)GameObjects.Village1GuildVFX).gameObject.SetActive(true);
        GetObject((int)GameObjects.Village1GuildVFX).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.Village1GuildVFX]));
        GetObject((int)GameObjects.Village1AlchemyVFX_Sub1).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.Village1AlchemyVFX_Sub1]));
        GetObject((int)GameObjects.MonasteryVFX_Sub1).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.MonasteryVFX_Sub1]));
        GetText((int)Texts.AlchemyButtonNewTxt).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.AlchemyButtonNewTxt]));
        GetText((int)Texts.MonasteryButtonNewTxt).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.MonasteryButtonNewTxt]));
        GetText((int)Texts.TravelButtonNewTxt).gameObject.SetActive(IntToBool(Managers.Game.ActivationManager[(int)ActivationKey.TravelButtonNewTxt]));

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

        //GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
        //_gameScene.FadeInOut();

        ShowGroup(GameObjects.Village1MainBtnGroup);

        GetButton((int)Buttons.GuildButton).gameObject.SetActive(false);
        GetButton((int)Buttons.AlchemyButton).gameObject.SetActive(false);
        GetButton((int)Buttons.MonasteryButton).gameObject.SetActive(false);
        GetButton((int)Buttons.TempButton1).gameObject.SetActive(false);
        GetButton((int)Buttons.TempButton2).gameObject.SetActive(false);
        GetButton((int)Buttons.TravelButton).gameObject.SetActive(false);

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

        seq.AppendCallback(() =>
        {
            GetButton((int)Buttons.GuildButton).gameObject.SetActive(true);
            GetButton((int)Buttons.AlchemyButton).gameObject.SetActive(true);
            GetButton((int)Buttons.MonasteryButton).gameObject.SetActive(true);
            GetButton((int)Buttons.TempButton1).gameObject.SetActive(true);
            GetButton((int)Buttons.TempButton2).gameObject.SetActive(true);
            GetButton((int)Buttons.TravelButton).gameObject.SetActive(true);
        });

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

        Managers.Quest.CheckButtonClick(VILLAGE1_GUILD_VFX);

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
        string dialogueId = Managers.Quest.GetCurrentDialogueID();

        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: dialogueId,
            speakerName: "접수원 아가씨",
            portrait: null,
            onFinished: () => Managers.Quest.CheckButtonClick(VILLAGE1_GUILD_TALK_VFX));
    }

    void RefreshNotify()
    {
        // TODO : UI_Village1BtnGroup에 있는 느낌표 오브젝트 다 연결하기. -> test1, test2는 임시로 만들어둔 것. 제거 예정. 데이터 시트에 아직 없음.

        // TODO : 퀘스트 알림이 필요한 버튼이 늘어날 때마다 if문 추가해야하는데, 이 부분은 어떻게 개선할 수 있을지 고민해보기.

        RefreshVillage1MainButtonGroupNotify();
        RefreshVillage1GuildButtonGroupNotify();
        RefreshVillage1AlchemyBtnGroupNotiry();
        RefreshVillage1MonasteryBtnGroupNotify();

    }

    void RefreshVillage1MainButtonGroupNotify()
    {
        List<Data.QuestNotifyData> list = Managers.Quest.GetCurrentNotifyList();
        // TODO Define으로 만들어서 오타 확률 줄이자.
        bool village1GuildNotify = list.Exists(n => n.BtnID == VILLAGE1_GUILD_VFX);
        bool alchemyButtonNewNotify = list.Exists(n => n.BtnID == ALCHEMY_BUTTON_NEW_TXT);
        bool village1AlchemyVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_ALCHEMY_VFX_SUB1);
        bool monasteryButtonNewNotify = list.Exists(n => n.BtnID == MONASTERY_BUTTON_NEW_TXT);
        bool monasteryVfxNotify = list.Exists(n => n.BtnID == MONASTERY_VFX_SUB1);
        bool TravelButtonNewNotify = list.Exists(n => n.BtnID == TRAVEL_BUTTON_NEW_TXT);
        bool village1TravelQuestVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_TRAVEL_QUEST_VFX);

        #region Village1MainBtnGroup
        GetObject((int)GameObjects.Village1GuildVFX)?.SetActive(village1GuildNotify);
        GetText((int)Texts.AlchemyButtonNewTxt)?.gameObject?.SetActive(alchemyButtonNewNotify);
        GetObject((int)GameObjects.Village1AlchemyVFX_Sub1)?.SetActive(village1AlchemyVfxNotify);
        GetText((int)Texts.MonasteryButtonNewTxt)?.gameObject?.SetActive(monasteryButtonNewNotify);
        GetObject((int)GameObjects.MonasteryVFX_Sub1)?.SetActive(monasteryVfxNotify);
        GetText((int)Texts.TravelButtonNewTxt)?.gameObject?.SetActive(TravelButtonNewNotify);
        GetObject((int)GameObjects.Village1TravelQuestVFX)?.SetActive(village1TravelQuestVfxNotify);
        #endregion
    }

    void RefreshVillage1GuildButtonGroupNotify()
    {
        List<Data.QuestNotifyData> list = Managers.Quest.GetCurrentNotifyList();
        bool village1GGuildTalkVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_GUILD_TALK_VFX);
        bool village1GuildQuestVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_GUILD_QUEST_VFX);
        bool guildExitQuestVfxNotify = list.Exists(n => n.BtnID == GUILD_EXIT_QUEST_VFX);

        #region Village1GuildBtnGroup
        GetObject((int)GameObjects.Village1GuildTalkVFX)?.SetActive(village1GGuildTalkVfxNotify);
        GetObject((int)GameObjects.Village1GuildQuestVFX)?.SetActive(village1GuildQuestVfxNotify);
        GetObject((int)GameObjects.GuildExitQuestVFX)?.SetActive(guildExitQuestVfxNotify);
        #endregion
    }

    void RefreshVillage1AlchemyBtnGroupNotiry()
    {
        List<Data.QuestNotifyData> list = Managers.Quest.GetCurrentNotifyList();
        bool village1AlchemyTalkVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_ALCHEMY_TALK_VFX);
        bool village1AlchemyTalkVfxSub1Notify = list.Exists(n => n.BtnID == VILLAGE1_ALCHEMY_TALK_VFX_SUB1);
        bool village1AlchemyCustomOrderButtonNewTxtNotify = list.Exists(n => n.BtnID == VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT);
        bool village1AlchemyQuestVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_ALCHEMY_QUEST_VFX);
        bool alchemyExitQuestVfxNotify = list.Exists(n => n.BtnID == ALCHEMY_EXIT_QUEST_VFX);


        #region Village1AlchemyBtnGroup
        GetObject((int)GameObjects.Village1AlchemyTalkVFX)?.SetActive(village1AlchemyTalkVfxNotify);
        GetObject((int)GameObjects.Village1AlchemyTalkVFX_Sub1)?.SetActive(village1AlchemyTalkVfxSub1Notify);
        GetText((int)Texts.AlchemyCustomOrderButtonNewTxt)?.gameObject?.SetActive(village1AlchemyCustomOrderButtonNewTxtNotify);
        GetObject((int)GameObjects.Village1AlchemyQuestVFX)?.SetActive(village1AlchemyQuestVfxNotify);
        GetObject((int)GameObjects.AlchemyExitQuestVFX)?.SetActive(alchemyExitQuestVfxNotify);
        #endregion
    }

    void RefreshVillage1MonasteryBtnGroupNotify()
    {
        List<Data.QuestNotifyData> list = Managers.Quest.GetCurrentNotifyList();
        bool village1MonasteryTalkVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_MONASTERY_TALK_VFX);
        bool village1MonasteryTalkVfxSub1Notify = list.Exists(n => n.BtnID == VILLAGE1_MONASTERY_TALK_VFX_SUB1);
        bool village1MonasteryCustomOrderButtonNewTxtNotify = list.Exists(n => n.BtnID == VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT);
        bool village1MonasteryQuestVfxNotify = list.Exists(n => n.BtnID == VILLAGE1_MONASTERY_QUEST_VFX);
        bool monasteryExitQuestVfxNotify = list.Exists(n => n.BtnID == MONASTERY_EXIT_QUEST_VFX);

        #region Village1MonasteryBtnGroup
        GetObject((int)GameObjects.Village1MonasteryTalkVFX)?.SetActive(village1MonasteryTalkVfxNotify);
        GetObject((int)GameObjects.Village1MonasteryTalkVFX_Sub1)?.SetActive(village1MonasteryTalkVfxSub1Notify);
        GetText((int)Texts.MonasteryCustomOrderButtonNewTxt)?.gameObject?.SetActive(village1MonasteryCustomOrderButtonNewTxtNotify);
        GetObject((int)GameObjects.Village1MonasteryQuestVFX)?.SetActive(village1MonasteryQuestVfxNotify);
        GetObject((int)GameObjects.MonasteryExitQuestVFX)?.SetActive(monasteryExitQuestVfxNotify);
        #endregion

    }
}
