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
        Village1GuildTalkButtonImage,
        Village1GuildTalkButtonIcon,
        Village1GuildQuestButtonIcon,
        Village1GuildNothingButton1Icon,
        Village1GuildNothingButton2Icon,
        Village1GuildNothingButton3Icon,
        Village1GuildExitButtonIcon,
        Village1AlchemyTalkButtonImage,
        Village1AlchemyTalkButtonIcon,
        Village1AlchemyTalkButtonImage2,
        Village1AlchemyTalkButtonIcon2,
        CustomOrderButtonIcon,
        Village1AlchemyNothingButton1Icon,
        Village1AlchemyNothingButton2Icon,
        Village1AlchemyNothingButton3Icon,
        Village1AlchemyExitButtonIcon,
        Village1MonasteryTalkButtonImage,
        Village1MonasteryTalkButtonIcon,
        Village1MonasteryTalkButtonImage2,
        Village1MonasteryTalkButtonIcon2,
        MonasteryClassButtonIcon,
        Village1MonasteryNothingButton1Icon,
        Village1MonasteryNothingButton2Icon,
        Village1MonasteryNothingButton3Icon,
        Village1MonasteryExitButtonIcon,
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
        Village1GuildTalkButtonText,
        Village1GuildQuestButtonText,
        Village1GuildNothingButton1Text,
        Village1GuildNothingButton2Text,
        Village1GuildNothingButton3Text,
        Village1GuildExitButtonText,
        Village1AlchemyNameText,
        Village1AlchemyTalkButtonText,
        CustomOrderButtonText,
        AlchemyCustomOrderButtonNewTxt,
        Village1AlchemyNothingButton1Text,
        Village1AlchemyNothingButton2Text,
        Village1AlchemyNothingButton3Text,
        Village1AlchemyExitButtonText,
        Village1MonasteryNameText,
        Village1MonasteryTalkButtonText,
        MonasteryClassButtonText,
        MonasteryCustomOrderButtonNewTxt,
        Village1MonasteryNothingButton1Text,
        Village1MonasteryNothingButton2Text,
        Village1MonasteryNothingButton3Text,
        Village1MonasteryExitButtonText,
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
        Village1GuildNothingButton1,
        Village1GuildNothingButton2,
        Village1GuildNothingButton3,
        Village1GuildExitButton,
        Village1AlchemyTalkButton,
        CustomOrderButton,
        Village1AlchemyNothingButton1,
        Village1AlchemyNothingButton2,
        Village1AlchemyNothingButton3,
        Village1AlchemyExitButton,
        Village1MonasteryTalkButton,
        MonasteryClassButton,
        Village1MonasteryNothingButton1,
        Village1MonasteryNothingButton2,
        Village1MonasteryNothingButton3,
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


    string _pendingUnlockBtnId = null;

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        TextOutlines();

        GetImage((int)Images.GuildButtonIcon).gameObject.BindEvent(OnClickGuild);
        GetImage((int)Images.AlchemyButtonIcon).gameObject.BindEvent(OnClickAlchemy);
        GetImage((int)Images.TravelButtonIcon).gameObject.BindEvent(OnClickTravel);
        GetButton((int)Buttons.Village1GuildExitButton).gameObject.BindEvent(() =>
        {
            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() =>
            {
                Managers.Quest.CheckButtonClick(GUILD_EXIT_QUEST_VFX);
                GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");

                // 버튼 애님.
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildTalkButton).GetComponent<RectTransform>(), -53f, 113f, -100f, 113f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildQuestButton).GetComponent<RectTransform>(), 0f, 186f, 75f, 186f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton1).GetComponent<RectTransform>(), 51f, 186f, 120f, 186f, Ease.InBack); // toX => 120f?
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton2).GetComponent<RectTransform>(), 0f, 133f, 75f, 133f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton3).GetComponent<RectTransform>(), 51f, 133f, 120f, 133f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildExitButton).GetComponent<RectTransform>(), 0f, 59f, 0f, 0f, Ease.InBack);
                _gameScene.FadeInOut();
            });

            seq.AppendInterval(0.3f);

            seq.AppendCallback(() => { PlaceNameAnim(GetText((int)Texts.Village1NameText).rectTransform); });

            seq.AppendInterval(0.5f);

            seq.AppendCallback(() => { OnClickMain(); });
            
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
        GetButton((int)Buttons.TravelButton).gameObject.BindEvent(OnClickTravel);

        Managers.Quest.OnNotifyChanged += RefreshNotify;
        Managers.Quest.OnBtnUnlocked   += OnBtnUnlocked;
        RefreshNotify();

        SetInfo();

        // 저장된 해금 상태 복원 (애니메이션 없이)
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_TRAVEL_BUTTON))
        {
            GetImage((int)Images.TravelButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.TravelButtonText).gameObject.SetActive(true);
        }

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Quest != null)
        {
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
            Managers.Quest.OnBtnUnlocked   -= OnBtnUnlocked;
        }
    }

    void TextOutlines()
    {
        Util.SetTMPUnderlay(GetText((int)Texts.Village1NameText), new Color32(0xCF, 0xA9, 0x36, 0x76), 0.2f, -0.2f);
        Util.SetTMPUnderlay(GetText((int)Texts.Village1GuildNameText), new Color32(0xCF, 0xA9, 0x36, 0x76), 0.2f, -0.2f);
        Util.SetTMPUnderlay(GetText((int)Texts.Village1AlchemyNameText), new Color32(0xCF, 0xA9, 0x36, 0x76), 0.2f, -0.2f);
        Util.SetTMPUnderlay(GetText((int)Texts.Village1MonasteryNameText), new Color32(0xCF, 0xA9, 0x36, 0x76), 0.2f, -0.2f);
        
    }

    void OnBtnUnlocked(string btnId)
    {
        _pendingUnlockBtnId = btnId;
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

        RestoreLastGroup();
    }

    void RestoreLastGroup()
    {
        switch (Managers.Game.LastVillage1Group)
        {
            case Village1Group.Guild:
                HideAllGroup();
                GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Background_Guild");
                Managers.Resource.Instantiate("GuildNPC").GetComponent<Animator>().Play("Receptionist");
                ShowGroup(GameObjects.Village1GuildBtnGroup);
                break;
            case Village1Group.Alchemy:
                HideAllGroup();
                ShowGroup(GameObjects.Village1AlchemyBtnGroup);
                break;
            case Village1Group.Monastery:
                HideAllGroup();
                ShowGroup(GameObjects.Village1MonasteryBtnGroup);
                break;
            default:
                OnClickMain();
                break;
        }
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
        Managers.Game.LastVillage1Group = Village1Group.Main;
        HideAllGroup();

        //GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
        //_gameScene.FadeInOut();
        PlaceNameAnim(GetText((int)Texts.Village1NameText).GetComponent<RectTransform>());
        ShowGroup(GameObjects.Village1MainBtnGroup);

        GetButton((int)Buttons.GuildButton).gameObject.SetActive(false);
        GetButton((int)Buttons.AlchemyButton).gameObject.SetActive(false);
        GetButton((int)Buttons.MonasteryButton).gameObject.SetActive(false);
        GetButton((int)Buttons.TempButton1).gameObject.SetActive(false);
        GetButton((int)Buttons.TempButton2).gameObject.SetActive(false);
        GetButton((int)Buttons.TravelButton).gameObject.SetActive(false);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Background_Village1");

            // TODO NPC들 제거
            GameObject guildNPC = GameObject.Find("GuildNPC");
            if (guildNPC != null) Destroy(guildNPC);
        });

        seq.AppendInterval(1f);

        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.GuildButton).GetComponent<RectTransform>(), 0, 0, -51.5f, 45.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.AlchemyButton).GetComponent<RectTransform>(), 0, 0, 0.5f, 45.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.MonasteryButton).GetComponent<RectTransform>(), 0, 0, 52.5f, 45.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.TempButton1).GetComponent<RectTransform>(), 0, 0, -51.5f, -21.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.TempButton2).GetComponent<RectTransform>(), 0, 0, 0.5f, -21.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.TravelButton).GetComponent<RectTransform>(), 0, 0, 52.5f, -21.5f));


        seq.AppendCallback(() =>
        {
            if (string.IsNullOrEmpty(_pendingUnlockBtnId)) return;
            string unlockId = _pendingUnlockBtnId;
            _pendingUnlockBtnId = null;

            DOVirtual.DelayedCall(0.5f, () => TryPlayUnlockAnim(unlockId));
        });
    }

    void TryPlayUnlockAnim(string btnId)
    {
        switch (btnId)
        {
            case UNLOCK_TRAVEL_BUTTON:
                UnlockButtonAnim(
                    GetButton((int)Buttons.TravelButton).gameObject,
                    GetImage((int)Images.TravelButtonIcon),
                    GetText((int)Texts.TravelButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo("Travel_Icon", LocalizeKey.ItemTempPermitNameText, LocalizeKey.TravelUnlockText)); // TODO : 정상적인 텍스트로 변경.
                break;
        }
    }

    void OnClickGuild()
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            // ButtonOffAnim 로비의 버튼 백 애님.
            ButtonMoveAnim(GetButton((int)Buttons.GuildButton).GetComponent<RectTransform>(), -51.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.AlchemyButton).GetComponent<RectTransform>(), 0.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.MonasteryButton).GetComponent<RectTransform>(), 52.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.TempButton1).GetComponent<RectTransform>(), -51.5f, -21.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.TempButton2).GetComponent<RectTransform>(), 0.5f, -21.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.TravelButton).GetComponent<RectTransform>(), 52.5f, -21.5f, 0, 0, Ease.InBack);
        });
        
        seq.AppendInterval(0.3f);

        seq.AppendCallback(() =>
        {
            HideAllGroup();

            ShowGroup(GameObjects.Village1GuildBtnGroup);

            // Active false 설정
            GetButton((int)Buttons.Village1GuildTalkButton).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1GuildQuestButton).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1GuildNothingButton1).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1GuildNothingButton2).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1GuildNothingButton3).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1GuildExitButton).gameObject.SetActive(false);

            Managers.Quest.CheckButtonClick(VILLAGE1_GUILD_VFX);

            // TODO : Player Move Anim, Fade In Out, Chang BG + Add NPC Sprite
            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            PlaceNameAnim(GetText((int)Texts.Village1GuildNameText).GetComponent<RectTransform>());
        });
        
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
            {
                Managers.Game.LastVillage1Group = Village1Group.Guild;
                GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Background_Guild");
                Managers.Resource.Instantiate("GuildNPC").GetComponent<Animator>().Play("Receptionist");
            });

        seq.AppendInterval(0.5f);

        // 버튼 애님.
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1GuildTalkButton).GetComponent<RectTransform>(), -100f, 113f, -53f, 113f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1GuildQuestButton).GetComponent<RectTransform>(), 75f, 186f, 0f, 186f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton1).GetComponent<RectTransform>(), 75f, 186f, 51f, 186f)); // fromx => 75f?
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton2).GetComponent<RectTransform>(), 75f, 133f, 0f, 133f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton3).GetComponent<RectTransform>(), 120f, 133f, 51f, 133f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1GuildExitButton).GetComponent<RectTransform>(), 0f, 0f, 0f, 59f));
        //seq.AppendInterval(0.07f);
    }

    void OnClickAlchemy()
    {
        Managers.Game.LastVillage1Group = Village1Group.Alchemy;
        HideAllGroup();

        ShowGroup(GameObjects.Village1AlchemyBtnGroup);
    }

    void OnClickTravel()
    {
        HideAllGroup();

        //TODO : 던전 진입 팝업 띄우기.
        Managers.UI.ShowPopupUI<UI_MapSelectPopup>().SetInfo();
    }

    void OnClickVillage1GuildTalkButton()
    {
        string dialogueId = Managers.Quest.GetCurrentDialogueID();

        // TODO : 대사 ID가 null이거나 빈 문자열이면 NPC 기본 대사 보여주기. 현재는 임시로 "GuildReceptionistDefaultDialogue"로 설정해둠. 데이터 시트에 추가 예정.
        // TODO : 길드 접수원이 아닌 Dialogue도 추가될 수 있으니, Dialogue ID에 따라 NPC 이름, 초상화도 변경할 수 있도록 수정해야할듯.
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
        //bool village1TravelQuestVfxNotify = list.Exists(n => n.BtnID == BTN_EXIT);

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
