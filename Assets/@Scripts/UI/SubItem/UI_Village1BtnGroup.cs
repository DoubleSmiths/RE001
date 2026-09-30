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

        // 대장간
        ForgeButtonIcon,
        Village1ForgeTalkButtonImage,
        Village1ForgeTalkButtonIcon,
        Village1ForgeTalkButtonImage2,
        Village1ForgeTalkButtonIcon2,
        ForgeCraftButtonIcon,
        ForgeEnhanceButtonIcon,
        Village1ForgeNothingButton2Icon,
        Village1ForgeNothingButton3Icon,
        Village1ForgeExitButtonIcon,

        // 상점
        ShopButtonIcon,
        Village1ShopTalkButtonImage,
        Village1ShopTalkButtonIcon,
        Village1ShopTalkButtonImage2,
        Village1ShopTalkButtonIcon2,
        ShopBuyButtonIcon,
        ShopSellButtonIcon,
        Village1ShopNothingButton2Icon,
        Village1ShopNothingButton3Icon,
        Village1ShopExitButtonIcon,
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

        // 대장간
        ForgeButtonText,
        Village1ForgeNameText,
        Village1ForgeTalkButtonText,
        ForgeCraftButtonText,
        ForgeCraftButtonNewTxt,
        ForgeEnhanceButtonText,
        Village1ForgeNothingButton2Text,
        Village1ForgeNothingButton3Text,
        Village1ForgeExitButtonText,

        // 상점
        ShopButtonText,
        Village1ShopNameText,
        Village1ShopTalkButtonText,
        ShopBuyButtonText,
        ShopBuyButtonNewTxt,
        ShopSellButtonText,
        Village1ShopNothingButton2Text,
        Village1ShopNothingButton3Text,
        Village1ShopExitButtonText,
    }

    enum Buttons
    {
        GuildButton,
        AlchemyButton,
        MonasteryButton,
        ForgeButton,
        ShopButton,
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

        // 대장간
        Village1ForgeTalkButton,
        ForgeCraftButton,
        ForgeEnhanceButton,
        Village1ForgeNothingButton2,
        Village1ForgeNothingButton3,
        Village1ForgeExitButton,

        // 상점
        Village1ShopTalkButton,
        ShopBuyButton,
        ShopSellButton,
        Village1ShopNothingButton2,
        Village1ShopNothingButton3,
        Village1ShopExitButton,
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

        // 대장간
        Village1ForgeBtnGroup,
        Village1ForgeTalkVFX,
        Village1ForgeTalkVFX_Sub1,
        Village1ForgeQuestVFX,
        ForgeExitQuestVFX,

        // 상점
        Village1ShopBtnGroup,
        Village1ShopTalkVFX,
        Village1ShopTalkVFX_Sub1,
        Village1ShopQuestVFX,
        ShopExitQuestVFX,
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
        GetImage((int)Images.MonasteryButtonIcon).gameObject.BindEvent(OnClickMonastery);
        GetImage((int)Images.ForgeButtonIcon).gameObject.BindEvent(OnClickForge);
        GetImage((int)Images.ShopButtonIcon).gameObject.BindEvent(OnClickShop);
        GetImage((int)Images.TravelButtonIcon).gameObject.BindEvent(OnClickTravel);
        GetButton((int)Buttons.Village1GuildExitButton).gameObject.BindEvent(() =>
        {
            Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

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
                GetButton((int)Buttons.ForgeButton).gameObject.SetActive(false);
                GetButton((int)Buttons.ShopButton).gameObject.SetActive(false);
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
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.ForgeButton).GetComponent<RectTransform>(), 0, 0, -51.5f, -21.5f));
            seq.AppendInterval(0.07f);
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.ShopButton).GetComponent<RectTransform>(), 0, 0, 0.5f, -21.5f));
            seq.AppendInterval(0.07f);
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.TravelButton).GetComponent<RectTransform>(), 0, 0, 52.5f, -21.5f));

            seq.AppendCallback(TryPlayPendingUnlock);
        });
        GetButton((int)Buttons.Village1AlchemyExitButton).gameObject.BindEvent(() =>
        {
            Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

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
                GetButton((int)Buttons.ForgeButton).gameObject.SetActive(false);
                GetButton((int)Buttons.ShopButton).gameObject.SetActive(false);
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
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.ForgeButton).GetComponent<RectTransform>(), 0, 0, -51.5f, -21.5f));
            seq.AppendInterval(0.07f);
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.ShopButton).GetComponent<RectTransform>(), 0, 0, 0.5f, -21.5f));
            seq.AppendInterval(0.07f);
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.TravelButton).GetComponent<RectTransform>(), 0, 0, 52.5f, -21.5f));

            seq.AppendCallback(TryPlayPendingUnlock);
        });
        GetButton((int)Buttons.Village1MonasteryExitButton).gameObject.BindEvent(
            () => ExitBuilding(MONASTERY_EXIT_QUEST_VFX, MONASTERY_SUB_BTNS));
        GetButton((int)Buttons.Village1ForgeExitButton).gameObject.BindEvent(
            () => ExitBuilding(FORGE_EXIT_QUEST_VFX, FORGE_SUB_BTNS));
        GetButton((int)Buttons.Village1ShopExitButton).gameObject.BindEvent(
            () => ExitBuilding(SHOP_EXIT_QUEST_VFX, SHOP_SUB_BTNS));
        GetButton((int)Buttons.Village1GuildTalkButton).gameObject.BindEvent(OnClickVillage1GuildTalkButton);
        GetButton((int)Buttons.Village1GuildQuestButton).gameObject.BindEvent(OnClickVillage1GuildQuestButton);
        GetButton((int)Buttons.Village1AlchemyTalkButton).gameObject.BindEvent(OnClickVillage1AlchemyTalkButton);
        GetButton((int)Buttons.CustomOrderButton).gameObject.BindEvent(OnClickCustomOrder);
        GetButton((int)Buttons.MonasteryClassButton).gameObject.BindEvent(OnClickMonasteryClass);
        GetButton((int)Buttons.Village1MonasteryTalkButton).gameObject.BindEvent(OnClickVillage1MonasteryTalkButton);
        GetButton((int)Buttons.ForgeCraftButton).gameObject.BindEvent(OnClickForgeCraft);
        GetButton((int)Buttons.ForgeEnhanceButton).gameObject.BindEvent(OnClickForgeEnhance);
        GetButton((int)Buttons.ShopBuyButton).gameObject.BindEvent(OnClickShopBuy);
        GetButton((int)Buttons.ShopSellButton).gameObject.BindEvent(OnClickShopSell);
        GetButton((int)Buttons.Village1ForgeTalkButton).gameObject.BindEvent(OnClickVillage1ForgeTalkButton);
        GetButton((int)Buttons.Village1ShopTalkButton).gameObject.BindEvent(OnClickVillage1ShopTalkButton);
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
        base.OnDestroy();
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
        SetTextLocalized(Texts.ForgeButtonText, LocalizeKey.ForgeText);
        SetTextLocalized(Texts.ShopButtonText, LocalizeKey.ShopText);
        SetTextLocalized(Texts.Village1ForgeNameText, LocalizeKey.ForgeText);
        SetTextLocalized(Texts.Village1ForgeTalkButtonText, LocalizeKey.TalkBtnText);
        SetTextLocalized(Texts.ForgeCraftButtonText, LocalizeKey.ForgeCraftBtnText);
        SetTextLocalized(Texts.ForgeEnhanceButtonText, LocalizeKey.ForgeEnhanceBtnText);
        SetTextLocalized(Texts.Village1ForgeExitButtonText, LocalizeKey.ExitBtnText);
        SetTextLocalized(Texts.Village1ShopNameText, LocalizeKey.ShopText);
        SetTextLocalized(Texts.Village1ShopTalkButtonText, LocalizeKey.TalkBtnText);
        SetTextLocalized(Texts.ShopBuyButtonText, LocalizeKey.ShopBuyBtnText);
        SetTextLocalized(Texts.ShopSellButtonText, LocalizeKey.ShopSellBtnText);
        SetTextLocalized(Texts.Village1ShopExitButtonText, LocalizeKey.ExitBtnText);
        SetTextLocalized(Texts.Village1MonasteryNameText, LocalizeKey.MonasteryText);
        SetTextLocalized(Texts.Village1MonasteryTalkButtonText, LocalizeKey.TalkBtnText);
        SetTextLocalized(Texts.Village1MonasteryExitButtonText, LocalizeKey.ExitBtnText);
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
        return btnId == UNLOCK_TRAVEL_BUTTON || btnId == UNLOCK_ALCHEMY_BUTTON
            || btnId == UNLOCK_FORGE_BUTTON || btnId == UNLOCK_SHOP_BUTTON
            || btnId == UNLOCK_MONASTERY_BUTTON;
    }

    void TryPlayPendingUnlock()
    {
        if (string.IsNullOrEmpty(_pendingUnlockBtnId)) return;
        string unlockId = _pendingUnlockBtnId;
        _pendingUnlockBtnId = null;
        DOVirtual.DelayedCall(0.5f, () => TryPlayUnlockAnim(unlockId)).SetTarget(this).SetLink(gameObject);
    }

    void SetInfo()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1MainBtnGroup);

        //GetImage((int)Images.GuildButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.AlchemyButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.MonasteryButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.ForgeButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.ShopButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.TravelButtonIcon).gameObject.SetActive(false);
        //GetText((int)Texts.GuildButtonText).gameObject.SetActive(false);
        GetText((int)Texts.AlchemyButtonText).gameObject.SetActive(false);
        GetText((int)Texts.AlchemyButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonText).gameObject.SetActive(false);
        GetText((int)Texts.ForgeButtonText).gameObject.SetActive(false);
        GetText((int)Texts.ShopButtonText).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonText).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonNewTxt).gameObject.SetActive(false);

        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_ALCHEMY_BUTTON))
        {
            GetImage((int)Images.AlchemyButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.AlchemyButtonText).gameObject.SetActive(true);
        }

        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_MONASTERY_BUTTON))
        {
            GetImage((int)Images.MonasteryButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.MonasteryButtonText).gameObject.SetActive(true);
        }

        // 여정도 여기서 켠다. 예전엔 Init()에서만 복원해서, SetInfo가 한 번 더 불리면
        //  (그룹 갱신 등) 해금돼 있는데도 여정 칸만 빈 채로 남았다.
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_TRAVEL_BUTTON))
        {
            GetImage((int)Images.TravelButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.TravelButtonText).gameObject.SetActive(true);
        }

        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_FORGE_BUTTON))
        {
            GetImage((int)Images.ForgeButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.ForgeButtonText).gameObject.SetActive(true);
        }

        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_SHOP_BUTTON))
        {
            GetImage((int)Images.ShopButtonIcon).gameObject.SetActive(true);
            GetText((int)Texts.ShopButtonText).gameObject.SetActive(true);
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
                SwitchBG("Village1_Monastery_BG");
                ShowGroup(GameObjects.Village1MonasteryBtnGroup);
                break;
            case Village1Group.Forge:
                HideAllGroup();
                SwitchBG("Village1_Forge_BG");
                ShowGroup(GameObjects.Village1ForgeBtnGroup);
                break;
            case Village1Group.Shop:
                HideAllGroup();
                SwitchBG("Village1_Shop_BG");
                ShowGroup(GameObjects.Village1ShopBtnGroup);
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
        GetObject((int)GameObjects.Village1ForgeBtnGroup).SetActive(group == GameObjects.Village1ForgeBtnGroup);
        GetObject((int)GameObjects.Village1ShopBtnGroup).SetActive(group == GameObjects.Village1ShopBtnGroup);
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
        foreach (var key in new[] { "Village1_Main_BG", "Village1_Guild_BG", "Village1_Alchemy_BG",
                                    "Village1_Monastery_BG", "Village1_Forge_BG", "Village1_Shop_BG" })
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
        GetObject((int)GameObjects.Village1MonasteryBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1ForgeBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1ShopBtnGroup).gameObject.SetActive(false);
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
        GetButton((int)Buttons.ForgeButton).gameObject.SetActive(false);
        GetButton((int)Buttons.ShopButton).gameObject.SetActive(false);
        GetButton((int)Buttons.TravelButton).gameObject.SetActive(false);

        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

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
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.ForgeButton).GetComponent<RectTransform>(), 0, 0, -51.5f, -21.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.ShopButton).GetComponent<RectTransform>(), 0, 0, 0.5f, -21.5f));
        seq.AppendInterval(0.07f);
        seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)Buttons.TravelButton).GetComponent<RectTransform>(), 0, 0, 52.5f, -21.5f));

        // 로비로 돌아왔을 때도 현재 퀘스트 기준으로 느낌표를 다시 계산한다.
        //  이게 빠져서 마을로 나오면 길드 느낌표가 사라진 채였다.
        seq.AppendCallback(RefreshNotify);
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
                        .SetInfo(GetImage((int)Images.TravelButtonIcon).sprite, LocalizeKey.TravelBtnText, LocalizeKey.TravelUnlockText)); // 해금 대상 = [여정] 버튼
                break;

            case UNLOCK_FORGE_BUTTON:
                BeginUnlockNotifySuppression();
                UnlockButtonAnim(
                    GetButton((int)Buttons.ForgeButton).gameObject,
                    GetImage((int)Images.ForgeButtonIcon),
                    GetText((int)Texts.ForgeButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo(GetImage((int)Images.ForgeButtonIcon).sprite, LocalizeKey.ForgeText, LocalizeKey.ForgeUnlockText));
                break;

            case UNLOCK_SHOP_BUTTON:
                BeginUnlockNotifySuppression();
                UnlockButtonAnim(
                    GetButton((int)Buttons.ShopButton).gameObject,
                    GetImage((int)Images.ShopButtonIcon),
                    GetText((int)Texts.ShopButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo(GetImage((int)Images.ShopButtonIcon).sprite, LocalizeKey.ShopText, LocalizeKey.ShopUnlockText));
                break;

            case UNLOCK_MONASTERY_BUTTON:
                BeginUnlockNotifySuppression();
                UnlockButtonAnim(
                    GetButton((int)Buttons.MonasteryButton).gameObject,
                    GetImage((int)Images.MonasteryButtonIcon),
                    GetText((int)Texts.MonasteryButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo(GetImage((int)Images.MonasteryButtonIcon).sprite, LocalizeKey.MonasteryText, LocalizeKey.MonasteryUnlockText));
                break;

            case UNLOCK_ALCHEMY_BUTTON:
                BeginUnlockNotifySuppression();
                UnlockButtonAnim(
                    GetButton((int)Buttons.AlchemyButton).gameObject,
                    GetImage((int)Images.AlchemyButtonIcon),
                    GetText((int)Texts.AlchemyButtonText),
                    onComplete: () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()
                        .SetInfo(GetImage((int)Images.AlchemyButtonIcon).sprite, LocalizeKey.AlchemyText, LocalizeKey.AlchemyUnlockText));
                break;
        }
    }

    #region 건물 진입/퇴장 공용 연출

    // 로비(메인) 버튼 6칸은 3x2 고정 배치다. 순서 = 등장 순서.
    static readonly Buttons[] MAIN_BTNS =
    {
        Buttons.GuildButton, Buttons.AlchemyButton, Buttons.MonasteryButton,
        Buttons.ForgeButton, Buttons.ShopButton, Buttons.TravelButton,
    };
    static readonly Vector2[] MAIN_BTN_POS =
    {
        new Vector2(-51.5f, 45.5f), new Vector2(0.5f, 45.5f), new Vector2(52.5f, 45.5f),
        new Vector2(-51.5f, -21.5f), new Vector2(0.5f, -21.5f), new Vector2(52.5f, -21.5f),
    };

    // 건물 안 버튼 6칸도 협회·연금공방·수도원이 전부 같은 자리를 쓴다 (슬29 레이아웃).
    //  [대화] / 기능1 / 기능2 / 기능3 / 기능4 / [나가기] 순서.
    //  FROM은 화면 밖 대기 위치 — 진입은 FROM→POS, 퇴장은 POS→FROM(역재생)이다.
    static readonly Vector2[] SUB_BTN_POS =
    {
        new Vector2(-53f, 113f), new Vector2(0f, 186f), new Vector2(51f, 186f),
        new Vector2(0f, 133f), new Vector2(51f, 133f), new Vector2(0f, 59f),
    };
    static readonly Vector2[] SUB_BTN_FROM =
    {
        new Vector2(-100f, 113f), new Vector2(75f, 186f), new Vector2(75f, 186f),
        new Vector2(75f, 133f), new Vector2(120f, 133f), new Vector2(0f, 0f),
    };

    // 수도원 하위 버튼 — SUB_BTN_POS와 같은 순서여야 한다.
    static readonly Buttons[] MONASTERY_SUB_BTNS =
    {
        Buttons.Village1MonasteryTalkButton, Buttons.MonasteryClassButton,
        Buttons.Village1MonasteryNothingButton1, Buttons.Village1MonasteryNothingButton2,
        Buttons.Village1MonasteryNothingButton3, Buttons.Village1MonasteryExitButton,
    };

    void RetractMainButtons()
    {
        for (int i = 0; i < MAIN_BTNS.Length; i++)
            ButtonMoveAnim(GetButton((int)MAIN_BTNS[i]).GetComponent<RectTransform>(),
                MAIN_BTN_POS[i].x, MAIN_BTN_POS[i].y, 0f, 0f, Ease.InBack);
    }

    void AppendMainButtonEntrance(Sequence seq)
    {
        for (int i = 0; i < MAIN_BTNS.Length; i++)
        {
            int idx = i;   // 클로저 캡처
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)MAIN_BTNS[idx]).GetComponent<RectTransform>(),
                0f, 0f, MAIN_BTN_POS[idx].x, MAIN_BTN_POS[idx].y));
            if (idx < MAIN_BTNS.Length - 1) seq.AppendInterval(0.07f);
        }
    }

    /// <summary>마을 로비 → 건물 진입 연출. 협회·연금공방이 각자 복사해 쓰던 흐름을 하나로 모았다.
    /// subBtns는 SUB_BTN_POS와 같은 순서([대화]…[나가기])여야 한다.</summary>
    void EnterBuilding(Village1Group group, GameObjects groupObj, string bgKey, Texts nameText,
                       string enterVfxId, Buttons[] subBtns)
    {
        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

        // t=0 : 로비 버튼 OutBack 역재생
        seq.AppendCallback(RetractMainButtons);

        seq.AppendInterval(0.3f);

        // t=0.3 : 그룹 전환 + 하위 버튼 숨김 + Player Escape + 페이드 + 장소 텍스트
        seq.AppendCallback(() =>
        {
            HideAllGroup();
            ShowGroup(groupObj);

            foreach (Buttons b in subBtns) GetButton((int)b).gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(enterVfxId)) Managers.Quest.CheckButtonClick(enterVfxId);

            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");
            _gameScene.FadeInOut();
            PlaceNameAnim(GetText((int)nameText).GetComponent<RectTransform>());
        });

        // t=0.8 : 암전 극중에 BG 교체 (시각적 팝 제거)
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
        {
            Managers.Game.LastVillage1Group = group;
            SwitchBG(bgKey);
            Managers.Game.Player?.ReturnHomeIdle();   // 암전 중 원위치+Idle
        });

        seq.AppendInterval(0.5f);

        seq.AppendCallback(RefreshNotify);   // 방금 켠 그룹의 느낌표를 현재 퀘스트 기준으로 다시 계산

        // t=1.3 : 하위 버튼 순차 등장
        for (int i = 0; i < subBtns.Length; i++)
        {
            int idx = i;
            seq.AppendCallback(() => ButtonMoveAnim(GetButton((int)subBtns[idx]).GetComponent<RectTransform>(),
                SUB_BTN_FROM[idx].x, SUB_BTN_FROM[idx].y, SUB_BTN_POS[idx].x, SUB_BTN_POS[idx].y));
            if (idx < subBtns.Length - 1) seq.AppendInterval(0.07f);
        }
    }

    /// <summary>건물 → 마을 로비 복귀 연출.</summary>
    void ExitBuilding(string exitVfxId, Buttons[] subBtns)
    {
        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

        // t=0 : 하위 버튼 역재생 + 페이드 시작
        seq.AppendCallback(() =>
        {
            if (!string.IsNullOrEmpty(exitVfxId)) Managers.Quest.CheckButtonClick(exitVfxId);
            GameObject.Find("Player")?.GetComponent<Animator>().Play("Escape");

            for (int i = 0; i < subBtns.Length; i++)
                ButtonMoveAnim(GetButton((int)subBtns[i]).GetComponent<RectTransform>(),
                    SUB_BTN_POS[i].x, SUB_BTN_POS[i].y, SUB_BTN_FROM[i].x, SUB_BTN_FROM[i].y, Ease.InBack);

            _gameScene.FadeInOut();
        });

        // t=0.4 : 화면이 검게 덮인 순간 BG 교체 + 메인 그룹 전환
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() =>
        {
            Managers.Game.LastVillage1Group = Village1Group.Main;
            HideAllGroup();
            ShowGroup(GameObjects.Village1MainBtnGroup);

            foreach (Buttons b in MAIN_BTNS) GetButton((int)b).gameObject.SetActive(false);

            SwitchBG("Village1_Main_BG");
            Managers.Game.Player?.ReturnHomeIdle();
        });

        // t=0.55 : 페이드 인 시작 시점에 장소 텍스트 1회
        seq.AppendInterval(0.15f);
        seq.AppendCallback(() => PlaceNameAnim(GetText((int)Texts.Village1NameText).GetComponent<RectTransform>()));

        // t=1.05 : 로비 버튼 순차 등장
        seq.AppendInterval(0.5f);
        AppendMainButtonEntrance(seq);

        seq.AppendCallback(RefreshNotify);
        seq.AppendCallback(TryPlayPendingUnlock);
    }

    // 대장간 하위 버튼 — SUB_BTN_POS와 같은 순서([대화]/기능1/기능2/빈칸/빈칸/[나가기]).
    static readonly Buttons[] FORGE_SUB_BTNS =
    {
        Buttons.Village1ForgeTalkButton, Buttons.ForgeCraftButton, Buttons.ForgeEnhanceButton,
        Buttons.Village1ForgeNothingButton2, Buttons.Village1ForgeNothingButton3,
        Buttons.Village1ForgeExitButton,
    };

    static readonly Buttons[] SHOP_SUB_BTNS =
    {
        Buttons.Village1ShopTalkButton, Buttons.ShopBuyButton, Buttons.ShopSellButton,
        Buttons.Village1ShopNothingButton2, Buttons.Village1ShopNothingButton3,
        Buttons.Village1ShopExitButton,
    };

    void OnClickForge()
    {
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_FORGE_BUTTON) == false)
            return;

        EnterBuilding(Village1Group.Forge, GameObjects.Village1ForgeBtnGroup,
            "Village1_Forge_BG", Texts.Village1ForgeNameText,
            VILLAGE1_FORGE_VFX, FORGE_SUB_BTNS);
    }

    void OnClickShop()
    {
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_SHOP_BUTTON) == false)
            return;

        EnterBuilding(Village1Group.Shop, GameObjects.Village1ShopBtnGroup,
            "Village1_Shop_BG", Texts.Village1ShopNameText,
            VILLAGE1_SHOP_VFX, SHOP_SUB_BTNS);
    }

    // 대장간 [제작] — 새 목록 UI를 만들지 않는다.
    //  장비 팝업이 이미 **미보유 장비까지** 회색+자물쇠로 보여주고, 누르면 상세창의 [제작]으로 간다
    //  (UI_EquipmentPopup.cs:285 / UI_EquipDetailPopup.cs:186). 대장간은 그 경로의 입구다.
    void OnClickForgeCraft()
    {
        Managers.Quest.CheckButtonClick(BTN_FORGE_CRAFT);
        Managers.UI.ShowPopupUI<UI_EquipmentPopup>();
    }

    // 대장간 [강화] — 손질할 장비를 하나 정해서 강화창을 연다.
    //  착용 중인 무기가 1순위(가장 자주 손대는 것), 없으면 아무 보유 장비.
    void OnClickForgeEnhance()
    {
        Managers.Quest.CheckButtonClick(BTN_FORGE_ENHANCE);

        Equipment target = null;
        if (Managers.Game.EquippedEquipments.TryGetValue(EquipSlotType.Weapon, out var weapon))
            target = weapon;
        if (target == null && Managers.Game.OwnedEquipments.Count > 0)
            target = Managers.Game.OwnedEquipments[0];

        if (target == null)
        {
            Managers.UI.ShowToast(Util.Localize(LocalizeKey.ForgeNoEquipText));
            return;
        }

        Managers.UI.ShowPopupUI<UI_ForgePopup>().SetInfo(target);
    }

    void OnClickShopBuy()
    {
        Managers.Quest.CheckButtonClick(BTN_SHOP_BUY);
        Managers.UI.ShowPopupUI<UI_ShopPopup>().SetInfo(sellMode: false);
    }

    void OnClickShopSell()
    {
        Managers.Quest.CheckButtonClick(BTN_SHOP_SELL);
        Managers.UI.ShowPopupUI<UI_ShopPopup>().SetInfo(sellMode: true);
    }

    // 수도원 — 그룹·[축성]·나가기까지 저작돼 있었는데 메인 버튼에 클릭 바인딩이 없어
    //  RestoreLastGroup 경로 말고는 들어갈 방법이 없었다.
    void OnClickMonastery()
    {
        if (Managers.Game.UnlockedBtnIds.Contains(UNLOCK_MONASTERY_BUTTON) == false)
            return;

        EnterBuilding(Village1Group.Monastery, GameObjects.Village1MonasteryBtnGroup,
            "Village1_Monastery_BG", Texts.Village1MonasteryNameText,
            MONASTERY_VFX_SUB1, MONASTERY_SUB_BTNS);
    }

    #endregion

    void OnClickGuild()
    {
        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

        seq.AppendCallback(() =>
        {
            // ButtonOffAnim 로비의 버튼 백 애님.
            ButtonMoveAnim(GetButton((int)Buttons.GuildButton).GetComponent<RectTransform>(), -51.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.AlchemyButton).GetComponent<RectTransform>(), 0.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.MonasteryButton).GetComponent<RectTransform>(), 52.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.ForgeButton).GetComponent<RectTransform>(), -51.5f, -21.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.ShopButton).GetComponent<RectTransform>(), 0.5f, -21.5f, 0, 0, Ease.InBack);
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
        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

        // t=0 : 로비 TYPE 1 버튼 OutBack 역재생
        seq.AppendCallback(() =>
        {
            ButtonMoveAnim(GetButton((int)Buttons.GuildButton).GetComponent<RectTransform>(), -51.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.AlchemyButton).GetComponent<RectTransform>(), 0.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.MonasteryButton).GetComponent<RectTransform>(), 52.5f, 45.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.ForgeButton).GetComponent<RectTransform>(), -51.5f, -21.5f, 0, 0, Ease.InBack);
            ButtonMoveAnim(GetButton((int)Buttons.ShopButton).GetComponent<RectTransform>(), 0.5f, -21.5f, 0, 0, Ease.InBack);
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
        bool isFirstQuest = Managers.Quest.CurrentQuestID == FIRST_GUILD_QUEST_ID;
        if (!isFirstQuest && TryPlayBondScene(BondManager.Npc.Receptionist, Util.Localize(LocalizeKey.NpcReceptionistNameText), null, "recep_normal"))
            return;

        string groupKey = isFirstQuest
            ? Managers.Quest.GetCurrentDialogueID()   // "GuildQuest1Text"
            : "GuildNormalText";

        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: groupKey,
            speakerName: Util.Localize(LocalizeKey.NpcReceptionistNameText),
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
        if (!isSubQuestActive && TryPlayBondScene(BondManager.Npc.Alchemist, Util.Localize(LocalizeKey.NpcAlchemistNameText), null, "alch_happy",
                () => Managers.Quest.CheckButtonClick(BTN_ALCHEMY_TALK)))   // 잡담처럼 '말 걸기' 신호는 그대로
            return;
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
            speakerName: Util.Localize(LocalizeKey.NpcAlchemistNameText),
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
                    }).SetTarget(this).SetLink(gameObject).SetUpdate(true);
                }
            },
            randomSingleLine: !useFullDialogue,
            portraitAnimState: animState);
    }

    // 대장간·상점 NPC — 협회/연금공방과 같은 방식(시트-당-그룹). 퀘스트 전용 대사가 없으면
    //  잡담 시트에서 한 줄만 뽑는다.
    void OnClickVillage1ForgeTalkButton()
    {
        Managers.Quest.CheckButtonClick(VILLAGE1_FORGE_TALK_VFX);
        if (TryPlayBondScene(BondManager.Npc.Blacksmith, Util.Localize(LocalizeKey.NpcBlacksmithNameText), GetImage((int)Images.Village1ForgeTalkButtonIcon)?.sprite, null))
            return;
        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: "ForgeNormalText",
            speakerName: Util.Localize(LocalizeKey.NpcBlacksmithNameText),
            portrait: GetImage((int)Images.Village1ForgeTalkButtonIcon)?.sprite,
            onFinished: null,
            randomSingleLine: true);
    }

    void OnClickVillage1MonasteryTalkButton()
    {
        Managers.Quest.CheckButtonClick(VILLAGE1_MONASTERY_TALK_VFX);
        if (TryPlayBondScene(BondManager.Npc.Priest, Util.Localize(LocalizeKey.NpcMonasteryPriestNameText), GetImage((int)Images.Village1MonasteryTalkButtonIcon)?.sprite, null))
            return;
        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: "MonasteryNormalText",
            speakerName: Util.Localize(LocalizeKey.NpcMonasteryPriestNameText),
            portrait: GetImage((int)Images.Village1MonasteryTalkButtonIcon)?.sprite,
            onFinished: null,
            randomSingleLine: true);
    }

    void OnClickVillage1ShopTalkButton()
    {
        Managers.Quest.CheckButtonClick(VILLAGE1_SHOP_TALK_VFX);
        if (TryPlayBondScene(BondManager.Npc.Shopkeeper, Util.Localize(LocalizeKey.NpcShopkeeperNameText), GetImage((int)Images.Village1ShopTalkButtonIcon)?.sprite, null))
            return;
        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: "ShopNormalText",
            speakerName: Util.Localize(LocalizeKey.NpcShopkeeperNameText),
            portrait: GetImage((int)Images.Village1ShopTalkButtonIcon)?.sprite,
            onFinished: null,
            randomSingleLine: true);
    }

    // 호감도 이야기 장면(ADR0042) — 볼 차례인 이야기가 있으면 잡담 대신 처음부터 끝까지 튼다. 다 보면 BondManager.FinishScene(크리스탈).
    //  portrait 가 null 이면 대화창 애니메이터(animState)로 얼굴을 그린다(접수원·연금술사).
    bool TryPlayBondScene(BondManager.Npc npc, string speaker, Sprite portrait, string animState, System.Action after = null)
    {
        var bm = Managers.Bond;
        int scene = bm != null ? bm.PendingScene(npc) : 0;
        if (scene <= 0) return false;
        string key = bm.SceneKey(npc, scene);
        if (Managers.Data.DialogueScriptDic == null || !Managers.Data.DialogueScriptDic.ContainsKey(key)) return false;
        Managers.UI.ShowPopupUI<UI_DialoguePopup>().SetInfo(
            groupKey: key,
            speakerName: speaker,
            portrait: portrait,
            onFinished: () => { bm.FinishScene(npc, scene); after?.Invoke(); },
            randomSingleLine: false,
            portraitAnimState: animState);
        return true;
    }

    void OnClickVillage1GuildQuestButton()
    {
        // 처음 임무를 받으러 온 플레이어가 [임무]를 눌러도 같은 안내를 시작한다.
        if (Managers.Quest.CurrentQuestID == FIRST_GUILD_QUEST_ID)
        {
            OnClickVillage1GuildTalkButton();
            return;
        }

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
            speakerName: Util.Localize(LocalizeKey.NpcReceptionistNameText),
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

        // 느낌표 색은 **알림이 어디서 왔는가**로 갈린다 — 노랑(FFE300)=메인 퀘스트,
        //  초록(14FF00)=서브 퀘스트. 같은 BtnID가 양쪽에서 올 수 있으므로 집합을 나눠 둔다.
        //  (예전엔 하나로 합쳐서, 서브 알림이 메인 색 느낌표를 켜는 것을 막을 수 없었다)
        HashSet<string> mainIds = new HashSet<string>();
        HashSet<string> subIds  = new HashSet<string>();
        foreach (Data.QuestNotifyData n in list)
        {
            if (n.IsSubQuest) subIds.Add(n.BtnID);
            else              mainIds.Add(n.BtnID);
        }

        foreach (NotifyBinding binding in s_notifyBindings)
        {
            HashSet<string> src = binding.IsSubQuestMark ? subIds : mainIds;

            bool on = false;
            foreach (string id in binding.BtnIds)
            {
                if (src.Contains(id))
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

        /// <summary>초록(서브퀘스트) 느낌표인가. 프리팹 이름 규약 `*_Sub1`이 곧 초록이다
        /// — 실제 색을 재 보니 _Sub1만 14FF00이고 나머지는 전부 FFE300이었다.</summary>
        public readonly bool IsSubQuestMark;

        public NotifyBinding(NotifyTargetKind kind, int index, params string[] btnIds)
        {
            Kind = kind;
            Index = index;
            IsSubQuestMark = false;
            BtnIds = btnIds;
        }

        NotifyBinding(NotifyTargetKind kind, int index, bool isSub, string[] btnIds)
        {
            Kind = kind;
            Index = index;
            IsSubQuestMark = isSub;
            BtnIds = btnIds;
        }

        /// <summary>서브퀘스트(초록) 느낌표 바인딩.</summary>
        public static NotifyBinding Sub(NotifyTargetKind kind, int index, params string[] btnIds)
            => new NotifyBinding(kind, index, true, btnIds);
    }

    // 각 notify 대상과 이를 켜는 BtnID 목록. (오타/누락 방지를 위해 여기 한 곳에 모아둠)
    static readonly NotifyBinding[] s_notifyBindings =
    {
        // ---- Village1MainBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1GuildVFX,        VILLAGE1_GUILD_VFX, "BTN_GUILD"),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.AlchemyButtonNewTxt,           ALCHEMY_BUTTON_NEW_TXT),
        NotifyBinding.Sub(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyVFX_Sub1, VILLAGE1_ALCHEMY_VFX_SUB1, BTN_ALCHEMY),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.MonasteryButtonNewTxt,         MONASTERY_BUTTON_NEW_TXT),
        NotifyBinding.Sub(NotifyTargetKind.Object, (int)GameObjects.MonasteryVFX_Sub1,       MONASTERY_VFX_SUB1),
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
        NotifyBinding.Sub(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyTalkVFX_Sub1, VILLAGE1_ALCHEMY_TALK_VFX_SUB1, BTN_ALCHEMY_TALK),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.AlchemyCustomOrderButtonNewTxt,    VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1AlchemyQuestVFX,     VILLAGE1_ALCHEMY_QUEST_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.AlchemyExitQuestVFX,         ALCHEMY_EXIT_QUEST_VFX),

        // ---- Village1MonasteryBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1MonasteryTalkVFX,      VILLAGE1_MONASTERY_TALK_VFX),
        NotifyBinding.Sub(NotifyTargetKind.Object, (int)GameObjects.Village1MonasteryTalkVFX_Sub1, VILLAGE1_MONASTERY_TALK_VFX_SUB1),
        new NotifyBinding(NotifyTargetKind.Text,   (int)Texts.MonasteryCustomOrderButtonNewTxt,    VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1MonasteryQuestVFX,     VILLAGE1_MONASTERY_QUEST_VFX),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.MonasteryExitQuestVFX,         MONASTERY_EXIT_QUEST_VFX),

        // ---- Village1ForgeBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1ForgeTalkVFX,      VILLAGE1_FORGE_TALK_VFX),
        NotifyBinding.Sub(NotifyTargetKind.Object, (int)GameObjects.Village1ForgeTalkVFX_Sub1, VILLAGE1_FORGE_TALK_VFX_SUB1),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1ForgeQuestVFX,     VILLAGE1_FORGE_VFX, BTN_FORGE_CRAFT, BTN_FORGE_ENHANCE),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.ForgeExitQuestVFX,         FORGE_EXIT_QUEST_VFX),

        // ---- Village1ShopBtnGroup ----
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1ShopTalkVFX,       VILLAGE1_SHOP_TALK_VFX),
        NotifyBinding.Sub(NotifyTargetKind.Object, (int)GameObjects.Village1ShopTalkVFX_Sub1,  VILLAGE1_SHOP_TALK_VFX_SUB1),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.Village1ShopQuestVFX,      VILLAGE1_SHOP_VFX, BTN_SHOP_BUY, BTN_SHOP_SELL),
        new NotifyBinding(NotifyTargetKind.Object, (int)GameObjects.ShopExitQuestVFX,          SHOP_EXIT_QUEST_VFX),
    };
}
