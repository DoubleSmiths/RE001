using DG.Tweening;
using System.Collections.Generic;
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

            // t=0 : 버튼 OutBack 역재생 + 페이드 시작 (FadeInOut: 0.4 out + 0.15 wait + 0.5 in)
            seq.AppendCallback(() =>
            {
                Managers.Quest.CheckButtonClick(GUILD_EXIT_QUEST_VFX);
                GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");

                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildTalkButton).GetComponent<RectTransform>(), -53f, 113f, -100f, 113f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildQuestButton).GetComponent<RectTransform>(), 0f, 186f, 75f, 186f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton1).GetComponent<RectTransform>(), 51f, 186f, 75f, 186f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton2).GetComponent<RectTransform>(), 0f, 133f, 75f, 133f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildNothingButton3).GetComponent<RectTransform>(), 51f, 133f, 120f, 133f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1GuildExitButton).GetComponent<RectTransform>(), 0f, 59f, 0f, 0f, Ease.InBack);

                _gameScene.FadeInOut();
            });

            // t=0.4 : 화면이 검게 덮인 순간 BG 교체 + NPC 제거 + 메인 그룹 전환 (시각적 팝 제거)
            seq.AppendInterval(0.4f);
            seq.AppendCallback(() =>
            {
                Managers.Game.LastVillage1Group = Village1Group.Main;
                HideAllGroup();
                ShowGroup(GameObjects.Village1MainBtnGroup);

                GetButton((int)Buttons.GuildButton).gameObject.SetActive(false);
                GetButton((int)Buttons.AlchemyButton).gameObject.SetActive(false);
                GetButton((int)Buttons.MonasteryButton).gameObject.SetActive(false);
                GetButton((int)Buttons.TempButton1).gameObject.SetActive(false);
                GetButton((int)Buttons.TempButton2).gameObject.SetActive(false);
                GetButton((int)Buttons.TravelButton).gameObject.SetActive(false);

                SwitchBG("Village1_Main_BG");
                Managers.Game.Player?.ReturnHomeIdle();   // 암전 중 원위치+Idle (페이드 인 시 잔여 이동 방지)
            });

            // t=0.55 : 페이드 인 시작 시점에 장소 텍스트 1회만 재생
            seq.AppendInterval(0.15f);
            seq.AppendCallback(() =>
            {
                PlaceNameAnim(GetText((int)Texts.Village1NameText).GetComponent<RectTransform>());
            });

            // t=1.05 : 페이드 인 완료 직후 TYPE 1 버튼 순차 등장 (0.07s 간격)
            seq.AppendInterval(0.5f);
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

            seq.AppendCallback(TryPlayPendingUnlock);
        });
        GetButton((int)Buttons.Village1AlchemyExitButton).gameObject.BindEvent(() =>
        {
            Sequence seq = DOTween.Sequence();

            // t=0 : 하위 버튼 OutBack 역재생 + 페이드 시작
            seq.AppendCallback(() =>
            {
                Managers.Quest.CheckButtonClick(ALCHEMY_EXIT_QUEST_VFX);
                GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");

                ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyTalkButton).GetComponent<RectTransform>(), -53f, 113f, -100f, 113f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.CustomOrderButton).GetComponent<RectTransform>(), 0f, 186f, 75f, 186f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyNothingButton1).GetComponent<RectTransform>(), 51f, 186f, 75f, 186f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyNothingButton2).GetComponent<RectTransform>(), 0f, 133f, 75f, 133f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyNothingButton3).GetComponent<RectTransform>(), 51f, 133f, 120f, 133f, Ease.InBack);
                ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyExitButton).GetComponent<RectTransform>(), 0f, 59f, 0f, 0f, Ease.InBack);

                _gameScene.FadeInOut();
            });

            // t=0.4 : 검게 덮인 순간 BG 교체 + 메인 그룹 전환
            seq.AppendInterval(0.4f);
            seq.AppendCallback(() =>
            {
                Managers.Game.LastVillage1Group = Village1Group.Main;
                HideAllGroup();
                ShowGroup(GameObjects.Village1MainBtnGroup);

                GetButton((int)Buttons.GuildButton).gameObject.SetActive(false);
                GetButton((int)Buttons.AlchemyButton).gameObject.SetActive(false);
                GetButton((int)Buttons.MonasteryButton).gameObject.SetActive(false);
                GetButton((int)Buttons.TempButton1).gameObject.SetActive(false);
                GetButton((int)Buttons.TempButton2).gameObject.SetActive(false);
                GetButton((int)Buttons.TravelButton).gameObject.SetActive(false);

                SwitchBG("Village1_Main_BG");
                Managers.Game.Player?.ReturnHomeIdle();   // 암전 중 원위치+Idle (페이드 인 시 잔여 이동 방지)
            });

            // t=0.55 : 페이드 인 시점에 장소 텍스트 재생
            seq.AppendInterval(0.15f);
            seq.AppendCallback(() =>
            {
                PlaceNameAnim(GetText((int)Texts.Village1NameText).GetComponent<RectTransform>());
            });

            // t=1.05 : TYPE 1 버튼 순차 등장
            seq.AppendInterval(0.5f);
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

            seq.AppendCallback(TryPlayPendingUnlock);
        });
        GetButton((int)Buttons.Village1MonasteryExitButton).gameObject.BindEvent(() =>
        {
            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            OnClickMain();
        });
        GetButton((int)Buttons.Village1GuildTalkButton).gameObject.BindEvent(OnClickVillage1GuildTalkButton);
        GetButton((int)Buttons.Village1GuildQuestButton).gameObject.BindEvent(OnClickVillage1GuildQuestButton);
        GetButton((int)Buttons.Village1AlchemyTalkButton).gameObject.BindEvent(OnClickVillage1AlchemyTalkButton);
        GetButton((int)Buttons.CustomOrderButton).gameObject.BindEvent(OnClickCustomOrder);
        GetButton((int)Buttons.MonasteryClassButton).gameObject.BindEvent(OnClickMonasteryClass);
        GetButton((int)Buttons.TravelButton).gameObject.BindEvent(OnClickTravel);

        Managers.Quest.OnNotifyChanged += RefreshNotify;
        Managers.Quest.OnBtnUnlocked   += OnBtnUnlocked;
        // 서브퀘스트 전이 시 조용히 해금되는 버튼(예: CustomOrderButton) 시각 상태 갱신
        if (Managers.SubQuest != null)
            Managers.SubQuest.OnSubQuestChanged += OnSubQuestChanged;
        RefreshNotify();

        SetInfo();

        // 저장된 해금 상태 복원 (애니메이션 없이)
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_TRAVEL_BUTTON))
        {
            GetImage((int)Images.TravelButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.TravelButtonText).gameObject.SetActive(true);
        }
        RefreshCustomOrderButton();

        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Quest != null)
        {
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
            Managers.Quest.OnBtnUnlocked   -= OnBtnUnlocked;
        }
        if (Managers.SubQuest != null)
            Managers.SubQuest.OnSubQuestChanged -= OnSubQuestChanged;
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        if (Managers.UI != null) Managers.UI.OnPopupClosed -= OnPopupClosedForUnlock;
    }

    // enum Texts 모든 entry — 폰트(UI) + 한영 일괄 처리.
    void RefreshTexts()
    {
        // 마을 이름
        SetTextLocalized(Texts.Village1NameText, LocalizeKey.PlaceNameTownAsdalText);

        // 메인 그룹 버튼 라벨
        SetTextLocalized(Texts.GuildButtonText, LocalizeKey.GuildText);
        SetTextLocalized(Texts.AlchemyButtonText, LocalizeKey.AlchemyText);
        SetTextLocalized(Texts.AlchemyButtonNewTxt, LocalizeKey.AlchemyButtonNewTxt);
        SetTextLocalized(Texts.MonasteryButtonText, LocalizeKey.MonasteryText);
        SetTextLocalized(Texts.MonasteryButtonNewTxt, LocalizeKey.MonasteryButtonNewTxt);
        SetTextLocalized(Texts.TravelButtonText, LocalizeKey.TravelBtnText);
        SetTextLocalized(Texts.TravelButtonNewTxt, LocalizeKey.TravelButtonNewTxt);

        // 길드 그룹
        SetTextLocalized(Texts.Village1GuildNameText, LocalizeKey.GuildText);
        SetTextLocalized(Texts.Village1GuildTalkButtonText, LocalizeKey.TalkBtnText);
        SetTextLocalized(Texts.Village1GuildQuestButtonText, LocalizeKey.QuestText);
        JustApplyFont(Texts.Village1GuildNothingButton1Text);
        JustApplyFont(Texts.Village1GuildNothingButton2Text);
        JustApplyFont(Texts.Village1GuildNothingButton3Text);
        SetTextLocalized(Texts.Village1GuildExitButtonText, LocalizeKey.ExitBtnText);

        // 연금공방 그룹
        SetTextLocalized(Texts.Village1AlchemyNameText, LocalizeKey.AlchemyText);
        SetTextLocalized(Texts.Village1AlchemyTalkButtonText, LocalizeKey.TalkBtnText);
        SetTextLocalized(Texts.CustomOrderButtonText, LocalizeKey.CommissionText);
        SetTextLocalized(Texts.AlchemyCustomOrderButtonNewTxt, LocalizeKey.AlchemyCustomOrderButtonNewTxt);
        JustApplyFont(Texts.Village1AlchemyNothingButton1Text);
        JustApplyFont(Texts.Village1AlchemyNothingButton2Text);
        JustApplyFont(Texts.Village1AlchemyNothingButton3Text);
        SetTextLocalized(Texts.Village1AlchemyExitButtonText, LocalizeKey.ExitBtnText);

        // 수도원 그룹
        SetTextLocalized(Texts.Village1MonasteryNameText, LocalizeKey.MonasteryText);
        SetTextLocalized(Texts.Village1MonasteryTalkButtonText, LocalizeKey.TalkBtnText);
        SetTextLocalized(Texts.MonasteryClassButtonText, LocalizeKey.AnointmentText);
        SetTextLocalized(Texts.MonasteryCustomOrderButtonNewTxt, LocalizeKey.MonasteryCustomOrderButtonNewTxt);
        JustApplyFont(Texts.Village1MonasteryNothingButton1Text);
        JustApplyFont(Texts.Village1MonasteryNothingButton2Text);
        JustApplyFont(Texts.Village1MonasteryNothingButton3Text);
        SetTextLocalized(Texts.Village1MonasteryExitButtonText, LocalizeKey.ExitBtnText);
    }

    void SetTextLocalized(Texts texts, string locKey)
    {
        var tmp = GetText((int)texts);
        if (tmp == null) return;
        ApplyFont(tmp, FontType.UI);
        tmp.text = Util.Localize(locKey);
    }

    void JustApplyFont(Texts texts)
    {
        var tmp = GetText((int)texts);
        if (tmp != null) ApplyFont(tmp, FontType.UI);
    }

    void OnSubQuestChanged(int _)
    {
        // 서브퀘스트 전이로 조용히 해금되는 버튼 상태를 즉시 반영
        RefreshCustomOrderButton();
    }

    /// <summary>
    /// CustomOrder(제작의뢰) 버튼은 SubQuest 2 진입 시 조용히 해금됨.
    /// 해금 전엔 아이콘/텍스트 비활성화(빈 프레임만 보임), 해금 후에는 표시.
    /// 별도 UnlockButtonAnim 없이 즉시 토글 (user 요청: "해금 애니메이션과 해금 팝업 없음").
    /// </summary>
    void RefreshCustomOrderButton()
    {
        bool unlocked = Managers.Game.UnlockedBtnIds.Contains(BTN_COMMISION);
        var icon = GetImage((int)Images.CustomOrderButtonIcon);
        var text = GetText((int)Texts.CustomOrderButtonText);
        if (icon != null) icon.gameObject.SetActive(unlocked);
        if (text != null) text.gameObject.SetActive(unlocked);
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
        // CustomOrder 버튼은 silent 해금 — 애니 없이 즉시 아이콘/텍스트만 노출
        if (btnId == BTN_COMMISION)
        {
            RefreshCustomOrderButton();
            return;
        }

        // UnlockButtonAnim 대상 ID만 pending 으로 저장.
        // (서브퀘스트가 발행하는 UNLOCK_POTION_BUTTON 등은 여기서 무시 — UI_GameScene이 처리)
        if (!IsUnlockAnimTarget(btnId))
            return;

        // 해금 이벤트 수신 즉시 느낌표/NEW 억제 시작.
        // (연출 시작은 0.5s 지연되므로, 여기서 걸지 않으면 그 사이 [!]가 잠깐 표시됨)
        BeginUnlockNotifySuppression();

        _pendingUnlockBtnId = btnId;

        // 메인 그룹이 이미 화면에 떠있으면(=해금 대상 버튼이 보임) 즉시 연출 시작.
        // Guild/Alchemy/Monastery 하위 그룹에 들어가 있는 상태면 그룹 종료 시점에 재생되도록 대기.
        if (GetObject((int)GameObjects.Village1MainBtnGroup).activeSelf)
            TryPlayPendingUnlock();
    }

    static bool IsUnlockAnimTarget(string btnId)
    {
        return btnId == UNLOCK_TRAVEL_BUTTON || btnId == UNLOCK_ALCHEMY_BUTTON;
    }

    void TryPlayPendingUnlock()
    {
        if (string.IsNullOrEmpty(_pendingUnlockBtnId)) return;
        string unlockId = _pendingUnlockBtnId;
        _pendingUnlockBtnId = null;
        DOVirtual.DelayedCall(0.5f, () => TryPlayUnlockAnim(unlockId));
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

        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_ALCHEMY_BUTTON))
        {
            GetImage((int)Images.AlchemyButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.AlchemyButtonText).gameObject.SetActive(true);
        }

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
                SwitchBG("Village1_Guild_BG");
                ShowGroup(GameObjects.Village1GuildBtnGroup);
                break;
            case Village1Group.Alchemy:
                HideAllGroup();
                SwitchBG("Village1_Alchemy_BG");
                ShowGroup(GameObjects.Village1AlchemyBtnGroup);
                break;
            case Village1Group.Monastery:
                HideAllGroup();
                SwitchBG("Village1_Main_BG");
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

    /// <summary>
    /// 배경 prefab(Village1_Main_BG / Village1_Guild_BG)을 전체 교체.
    /// 씬 초기 BackgroundImage와 이전 BG prefab 인스턴스를 제거하고 새 prefab 스폰.
    /// ResourceManager.Instantiate가 인스턴스 이름에서 "(Clone)"을 제거하므로 찾을 때도 원본 키로 찾아야 함.
    /// </summary>
    void SwitchBG(string prefabKey)
    {
        // 씬 초기 BackgroundImage(스프라이트 1개짜리) 비활성화 - 더 이상 사용 안 함
        var legacyBG = GameObject.Find("BackgroundImage");
        if (legacyBG != null) legacyBG.SetActive(false);

        // 기존 BG prefab 인스턴스 제거 (씬 루트에서 모두 탐색, 중복 스폰도 정리)
        foreach (var key in new[] { "Village1_Main_BG", "Village1_Guild_BG", "Village1_Alchemy_BG" })
        {
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == key || go.name == key + "(Clone)") Destroy(go);
            }
        }
        // 구버전 잔재 정리
        var oldNpc = GameObject.Find("GuildNPC") ?? GameObject.Find("GuildNPC(Clone)");
        if (oldNpc != null) Destroy(oldNpc);
        var oldFloor = GameObject.Find("GuildFloor") ?? GameObject.Find("GuildFloor(Clone)");
        if (oldFloor != null) Destroy(oldFloor);

        if (string.IsNullOrEmpty(prefabKey)) return;
        var bg = Managers.Resource.Instantiate(prefabKey);
        ApplyTownBackground(bg, prefabKey);

        ApplyPlayerBuildingScale(prefabKey);
    }

    // 마을 배경 프리팹은 하나(Village1_*)를 5개 마을이 공유한다.
    //  한동안 **색조만** 바꿔 구분했는데, 색만 도는 같은 그림이라 "그 마을 배경이 아니다"는
    //  지적이 그대로였다. 마을마다 그림 자체(VillageBG_{nodeId})를 만들어 갈아끼운다.
    //  버튼(모험가협회/연금공방/여정)은 아래 패널의 UI라 그림이 바뀌어도 조작에 영향이 없다.
    //  전용 그림이 없으면 원본을 그대로 둔다.
    const string MAIN_BG_KEY = "Village1_Main_BG";
    static Sprite s_mainBgOriginal;      // 풀에서 재사용된 인스턴스에 남은 그림을 되돌리기 위해

    void ApplyTownBackground(GameObject bg, string prefabKey)
    {
        if (bg == null) return;
        // 마을 전경만 대상. 길드·연금공방 내부는 실내라 마을 그림으로 바꾸면 안 된다.
        if (prefabKey != MAIN_BG_KEY) return;

        SpriteRenderer target = null;
        foreach (var sr in bg.GetComponentsInChildren<SpriteRenderer>(true))
            if (sr.name.EndsWith("_BG_Img")) { target = sr; break; }
        if (target == null) return;

        if (s_mainBgOriginal == null) s_mainBgOriginal = target.sprite;

        int nodeId = Managers.Game != null ? Managers.Game.CurrentTravelAreaId : 0;
        Sprite sp = Managers.Resource?.Load<Sprite>($"VillageBG_{nodeId}");

        // 배경 판은 풀에서 재사용된다 — 전용 그림이 없는 마을이면 **원본으로 되돌려야**
        //  직전 마을 그림이 그대로 남지 않는다.
        target.sprite = sp != null ? sp : s_mainBgOriginal;
        target.color = Color.white;          // 예전 색조 방식의 잔재가 남으면 새 그림까지 물든다
    }

    // 기획 수정안(2026-07-05): 건물 내부(길드/연금 등)에서는 플레이어를 확대해 '건물에 들어간 느낌'을 준다.
    // (NPC 스프라이트를 플레이어 크기로 줄이면 도트가 뭉개져서, 반대로 플레이어를 키우는 방식)
    // SwitchBG가 진입/퇴장/복원 모든 경로의 단일 지점이라 여기서 일괄 처리 — 암전(Fade) 중에 호출되므로 팝 없음.
    const float PLAYER_BUILDING_SCALE = 1.5f;   // 건물 내부 확대 배율 (기획 튜닝값)
    Vector3? _playerBaseScale;

    void ApplyPlayerBuildingScale(string bgKey)
    {
        var player = Managers.Game.Player;
        if (player == null) return;

        if (_playerBaseScale == null) _playerBaseScale = player.transform.localScale;

        bool insideBuilding = bgKey != "Village1_Main_BG";
        player.transform.localScale = _playerBaseScale.Value * (insideBuilding ? PLAYER_BUILDING_SCALE : 1f);
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
            SwitchBG("Village1_Main_BG");
            Managers.Game.Player?.ReturnHomeIdle();   // 원위치+Idle (페이드 인 시 잔여 이동 방지)
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

        seq.AppendCallback(TryPlayPendingUnlock);
    }

    // ── Unlock 연출 동안 느낌표/NEW 억제 ──
    //  기획 수정안(2026-07-05): 해금 연출·팝업 중에는 [!] Quest Mark를 표시하지 않고,
    //  해금 팝업이 닫힌 뒤 [NEW] Text와 함께 일괄 표시한다.
    bool _suppressNotifyForUnlock;

    void BeginUnlockNotifySuppression()
    {
        if (_suppressNotifyForUnlock) return;   // 이미 억제 중 (중복 구독 방지)
        _suppressNotifyForUnlock = true;
        RefreshNotify();   // 이미 켜져 있는 느낌표/NEW도 끔
        Managers.UI.OnPopupClosed += OnPopupClosedForUnlock;
    }

    void OnPopupClosedForUnlock(System.Type popupType)
    {
        if (popupType != typeof(UI_UnlockButtonPopup)) return;
        Managers.UI.OnPopupClosed -= OnPopupClosedForUnlock;
        _suppressNotifyForUnlock = false;
        RefreshNotify();   // 팝업 닫힘 → [!] Quest Mark + [NEW] Text 동시 표시
    }

    void TryPlayUnlockAnim(string btnId)
    {
        switch (btnId)
        {
            case UNLOCK_TRAVEL_BUTTON:
                BeginUnlockNotifySuppression();
                UnlockButtonAnim(
                    GetButton((int)Buttons.TravelButton).gameObject,
                    GetImage((int)Images.TravelButtonIcon),
                    GetText((int)Texts.TravelButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo("Travel_Icon", LocalizeKey.TravelBtnText, LocalizeKey.TravelUnlockText)); // 해금 대상 = [여정] 버튼
                break;

            case UNLOCK_ALCHEMY_BUTTON:
                BeginUnlockNotifySuppression();
                UnlockButtonAnim(
                    GetButton((int)Buttons.AlchemyButton).gameObject,
                    GetImage((int)Images.AlchemyButtonIcon),
                    GetText((int)Texts.AlchemyButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo("Alchemy_Icon", LocalizeKey.AlchemyText, LocalizeKey.AlchemyUnlockText));
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
                SwitchBG("Village1_Guild_BG");
                Managers.Game.Player?.ReturnHomeIdle();   // 암전 중 원위치+Idle (페이드 인 시 잔여 이동 방지)
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
        Sequence seq = DOTween.Sequence();

        // t=0 : 로비 TYPE 1 버튼 OutBack 역재생
        seq.AppendCallback(() =>
        {
            ButtonMoveAnim(GetButton((int)Buttons.GuildButton).GetComponent<RectTransform>(), -51.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.AlchemyButton).GetComponent<RectTransform>(), 0.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.MonasteryButton).GetComponent<RectTransform>(), 52.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.TempButton1).GetComponent<RectTransform>(), -51.5f, -21.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.TempButton2).GetComponent<RectTransform>(), 0.5f, -21.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.TravelButton).GetComponent<RectTransform>(), 52.5f, -21.5f, 0, 0, Ease.InBack);
        });

        seq.AppendInterval(0.3f);

        // t=0.3 : 그룹 전환 + 하위 버튼 비활성화 + Player Escape + Fade + 장소 텍스트
        seq.AppendCallback(() =>
        {
            HideAllGroup();
            ShowGroup(GameObjects.Village1AlchemyBtnGroup);

            GetButton((int)Buttons.Village1AlchemyTalkButton).gameObject.SetActive(false);
            GetButton((int)Buttons.CustomOrderButton).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1AlchemyNothingButton1).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1AlchemyNothingButton2).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1AlchemyNothingButton3).gameObject.SetActive(false);
            GetButton((int)Buttons.Village1AlchemyExitButton).gameObject.SetActive(false);

            Managers.Quest.CheckButtonClick(VILLAGE1_ALCHEMY_VFX_SUB1);

            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            PlaceNameAnim(GetText((int)Texts.Village1AlchemyNameText).GetComponent<RectTransform>());
        });

        // t=0.8 : 페이드 극중에 BG 교체 (시각적 팝 제거)
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
        {
            Managers.Game.LastVillage1Group = Village1Group.Alchemy;
            SwitchBG("Village1_Alchemy_BG");
            Managers.Game.Player?.ReturnHomeIdle();   // 암전 중 원위치+Idle (페이드 인 시 잔여 이동 방지)
        });

        seq.AppendInterval(0.5f);

        // t=1.3 : TYPE 2 버튼 순차 등장 (슬라이드 29 pos)
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyTalkButton).GetComponent<RectTransform>(), -100f, 113f, -53f, 113f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.CustomOrderButton).GetComponent<RectTransform>(), 75f, 186f, 0f, 186f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyNothingButton1).GetComponent<RectTransform>(), 75f, 186f, 51f, 186f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyNothingButton2).GetComponent<RectTransform>(), 75f, 133f, 0f, 133f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyNothingButton3).GetComponent<RectTransform>(), 120f, 133f, 51f, 133f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.Village1AlchemyExitButton).GetComponent<RectTransform>(), 0f, 0f, 0f, 59f));
    }

    void OnClickTravel()
    {
        // 여정은 UNLOCK_TRAVEL_BUTTON 해금 후에만 동작.
        // (BindEvent는 Button.interactable을 무시하므로 코드 가드 필요)
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_TRAVEL_BUTTON) == false)
            return;

        // 여정 팝업은 자체 Background 버튼이 배경 클릭을 차단하므로 Village1 버튼 그룹을 숨기면 안 됨
        // (팝업 닫힐 때 복원 로직이 없어 버튼이 계속 사라져 보이는 버그 방지)
        Managers.UI.ShowPopupUI<UI_TravelPopup>().SetInfo();
    }

    void OnClickCustomOrder()
    {
        // 슬라이드 34: 제작의뢰 팝업.
        // SubQuest 2 완료 시 BTN_COMMISION이 UnlockedBtnIds에 추가됨 → 그 전엔 차단.
        if (Managers.Game.UnlockedBtnIds.Contains(BTN_COMMISION) == false)
            return;

        Managers.UI.ShowPopupUI<UI_CustomOrderPopup>().SetInfo();
    }

    void OnClickMonasteryClass()
    {
        // 수도원 [축성] 버튼 — 직업(전직) 능력 전용 창.
        // (수도원 그룹 '진입' 연출은 미구현 — MonasteryButtonIcon 클릭 바인딩 없음. 현재는 RestoreLastGroup 경로로만 그룹 표시됨)
        Managers.UI.ShowPopupUI<UI_ClassPopup>().SetInfo();
    }

    void OnClickVillage1GuildTalkButton()
    {
        // 슬라이드 30: 메인 퀘스트 [1] 시에만 [대화] 버튼으로 퀘스트 진행.
        // 그 외에는 GuildNormalText에서 한 줄 랜덤 선택하여 잡담 대사 재생.
        bool isFirstQuest = Managers.Quest.CurrentQuestID == 1;

        string groupKey = isFirstQuest
            ? Managers.Quest.GetCurrentDialogueID()   // "GuildQuest1Text"
            : "GuildNormalText";

        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: groupKey,
            speakerName: "접수원 아가씨",
            portrait: null,
            onFinished: isFirstQuest
                ? (System.Action)(() => Managers.Quest.CheckButtonClick(VILLAGE1_GUILD_TALK_VFX))
                : null,
            randomSingleLine: !isFirstQuest);
    }

    void OnClickVillage1AlchemyTalkButton()
    {
        // 서브퀘스트 1(연금공방 첫 방문) 활성 상태면 전용 대사 + 종료 시 BTN_ALCHEMY_TALK 트리거.
        // 그 외에는 AlchemyNormalText에서 한 줄 랜덤.
        bool isSubQuestActive = Managers.SubQuest != null && Managers.SubQuest.IsActive;
        int  subQuestBefore   = Managers.SubQuest != null ? Managers.SubQuest.CurrentSubQuestID : 0;
        string subDlg = isSubQuestActive ? Managers.SubQuest.GetCurrentDialogueID() : null;

        // 서브퀘스트 전용 대사가 데이터시트에 등록돼 있으면 사용, 아니면 Normal로 폴백
        string groupKey = (!string.IsNullOrEmpty(subDlg)
                           && Managers.Data.DialogueScriptDic.ContainsKey(subDlg))
            ? subDlg
            : "AlchemyNormalText";

        bool useFullDialogue = groupKey != "AlchemyNormalText";

        // 서브퀘스트 라인(퀘스트 관련)은 "alch_happy", 일상 대사는 "alch_normal"
        string animState = isSubQuestActive ? "alch_happy" : "alch_normal";

        Debug.Log($"[AlchemyTalk] SubQuestActive={isSubQuestActive} SubQuestID={Managers.SubQuest?.CurrentSubQuestID} subDlg={subDlg} → groupKey={groupKey}");

        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: groupKey,
            speakerName: "연금공방 아가씨",
            portrait: null,
            onFinished: () =>
            {
                Managers.Quest.CheckButtonClick(BTN_ALCHEMY_TALK);

                // SubQuest 1 → 2 전이가 막 일어났다면 HP 포션 "획득 안내" 팝업 표시 (설명 전용)
                if (subQuestBefore == 1
                    && Managers.SubQuest != null
                    && Managers.SubQuest.CurrentSubQuestID != 1)
                {
                    DG.Tweening.DOVirtual.DelayedCall(0.05f, () =>
                    {
                        Managers.UI.ShowPopupUI<UI_PotionDetailPopup>()
                            .SetInfo(Managers.Game.CurrentHPPotionDataId, fromCustomOrder: false);
                    }).SetUpdate(true);
                }
            },
            randomSingleLine: !useFullDialogue,
            portraitAnimState: animState);
    }

    void OnClickVillage1GuildQuestButton()
    {
        // 완료 보고 퀘스트 진행 중이면 퀘스트 전용 대사 + 종료 시 진행 트리거,
        // 그 외에는 잡담 대사(GuildNormalText) 한 줄 랜덤.
        //
        // ⚠️ 예전엔 `CurrentQuestID == 4`로 박아 뒀다. 그래서 같은 [임무] 버튼으로 보고하는
        //    퀘7(2급 승급 보고)에서는 잡담만 나오고 CheckButtonClick이 불리지 않아
        //    **마지막 퀘스트를 인게임에서 영영 끝낼 수 없었다**. 번호가 아니라 데이터로 판정한다 —
        //    "이 버튼으로 완료되는 퀘스트인가"가 진짜 조건이고, 보고 퀘스트가 늘어도 따라간다.
        bool isReportQuest =
            Managers.Data.QuestDic.TryGetValue(Managers.Quest.CurrentQuestID, out var curQuest)
            && curQuest.CompleteType == Define.QuestCompleteType.BTN_CLICK
            && curQuest.CompleteTarget == BTN_GUILD_QUEST;

        string groupKey = isReportQuest
            ? Managers.Quest.GetCurrentDialogueID()   // QuestData.DialogueID = "GuildQuest2Text"
            : "GuildNormalText";

        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: groupKey,
            speakerName: "접수원 아가씨",
            portrait: null,
            onFinished: isReportQuest
                ? (System.Action)(() =>
                {
                    Managers.Quest.CheckButtonClick(BTN_GUILD_QUEST);
                    // 기획 수정안(2026-07-05): 완료 보고 대화 종료 → 랭크업 연출 (Player Rank Up Ef, 프리팹 저작 속도 그대로)
                    if (Managers.Game.Player != null)
                        Managers.Object?.ShowRankUpEffectOn(Managers.Game.Player.transform);
                    // 랭크업 토스트 배너 (지역명 토스트와 동일 경로)
                    Managers.UI.ShowToast("RANK UP");
                })
                : null,
            randomSingleLine: !isReportQuest);
    }

    // BtnID -> notify 대상 매핑 테이블(s_notifyBindings) 기반으로 일괄 갱신.
    // 버튼이 늘어나면 s_notifyBindings에 항목만 추가하면 된다. (기존: 그룹별 if문 나열)
    void RefreshNotify()
    {
        // Unlock 연출~해금 팝업 종료까지는 느낌표/NEW 전부 끔 (팝업 닫힌 뒤 일괄 표시)
        if (_suppressNotifyForUnlock)
        {
            foreach (NotifyBinding binding in s_notifyBindings)
                SetNotifyActive(binding, false);
            return;
        }

        List<Data.QuestNotifyData> list = Managers.Quest.GetCurrentNotifyList();

        HashSet<string> activeIds = new HashSet<string>();
        foreach (Data.QuestNotifyData n in list)
            activeIds.Add(n.BtnID);

        foreach (NotifyBinding binding in s_notifyBindings)
        {
            bool on = false;
            foreach (string id in binding.BtnIds)
            {
                if (activeIds.Contains(id))
                {
                    on = true;
                    break;
                }
            }
            SetNotifyActive(binding, on);
        }
    }

    // notify 대상 하나(느낌표 VFX 오브젝트 또는 New 텍스트)를 켜고/끈다.
    void SetNotifyActive(NotifyBinding binding, bool on)
    {
        switch (binding.Kind)
        {
            case NotifyTargetKind.Object:
                GetObject(binding.Index)?.SetActive(on);
                break;
            case NotifyTargetKind.Text:
                GetText(binding.Index)?.gameObject?.SetActive(on);
                break;
        }
    }

    enum NotifyTargetKind { Object, Text }

    // BtnIds 중 하나라도 현재 notify 리스트에 있으면 (Kind, Index) 대상이 켜진다.
    struct NotifyBinding
    {
        public readonly NotifyTargetKind Kind;
        public readonly int Index;
        public readonly string[] BtnIds;

        public NotifyBinding(NotifyTargetKind kind, int index, params string[] btnIds)
        {
            Kind = kind;
            Index = index;
            BtnIds = btnIds;
        }
    }

    // 각 notify 대상과 이를 켜는 BtnID 목록. (오타/누락 방지를 위해 여기 한 곳에 모아둠)
    static readonly NotifyBinding[] s_notifyBindings =
    {
        // ---- Village1MainBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1GuildVFX,        VILLAGE1_GUILD_VFX, "BTN_GUILD"),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.AlchemyButtonNewTxt,           ALCHEMY_BUTTON_NEW_TXT),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyVFX_Sub1, VILLAGE1_ALCHEMY_VFX_SUB1, BTN_ALCHEMY),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.MonasteryButtonNewTxt,         MONASTERY_BUTTON_NEW_TXT),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.MonasteryVFX_Sub1,       MONASTERY_VFX_SUB1),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.TravelButtonNewTxt,            TRAVEL_BUTTON_NEW_TXT),
        // 여정 버튼: VFX 이름(VILLAGE1_TRAVEL_QUEST_VFX)과 BTN_JOURNEY 액션 ID 둘 다 인정.
        // (Quest3=VFX명 사용, Quest6=BTN_JOURNEY 사용 → 둘 다 표시되도록. 길드/연금과 동일 패턴)
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1TravelQuestVFX,  VILLAGE1_TRAVEL_QUEST_VFX, BTN_JOURNEY),

        // ---- Village1GuildBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1GuildTalkVFX,    VILLAGE1_GUILD_TALK_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1GuildQuestVFX,   VILLAGE1_GUILD_QUEST_VFX, BTN_GUILD_QUEST),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.GuildExitQuestVFX,       GUILD_EXIT_QUEST_VFX),

        // ---- Village1AlchemyBtnGroup ---- (서브퀘스트 notify는 _Sub1 VFX)
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyTalkVFX,      VILLAGE1_ALCHEMY_TALK_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyTalkVFX_Sub1, VILLAGE1_ALCHEMY_TALK_VFX_SUB1, BTN_ALCHEMY_TALK),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.AlchemyCustomOrderButtonNewTxt,    VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyQuestVFX,     VILLAGE1_ALCHEMY_QUEST_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.AlchemyExitQuestVFX,         ALCHEMY_EXIT_QUEST_VFX),

        // ---- Village1MonasteryBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1MonasteryTalkVFX,      VILLAGE1_MONASTERY_TALK_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1MonasteryTalkVFX_Sub1, VILLAGE1_MONASTERY_TALK_VFX_SUB1),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.MonasteryCustomOrderButtonNewTxt,    VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1MonasteryQuestVFX,     VILLAGE1_MONASTERY_QUEST_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.MonasteryExitQuestVFX,         MONASTERY_EXIT_QUEST_VFX),
    };
}
