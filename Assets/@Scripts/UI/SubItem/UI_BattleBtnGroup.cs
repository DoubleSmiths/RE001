using DG.Tweening;
using UnityEngine;
using static Define;

public class UI_BattleBtnGroup : UI_AreaBtnGroup
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
    }

    enum Buttons
    {
        TempButton1,
        TempButton2,
        TempButton3,
        TempButton4,
        TempButton5,
        TravelButton,
    }

    enum GameObjects
    {
        Village1MainBtnGroup,
        Village1GuildVFX,
        Village1AlchemyVFX_Sub1,
        MonasteryVFX_Sub1,
        TravelButtonQuestVFX,
    }

    #endregion


    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.TravelButton).gameObject.BindEvent(OnClickTravel);

        Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
    }

    public override void PlayEnterAnimation()
    {
        var btn = GetButton((int)Buttons.TravelButton).gameObject;
        btn.SetActive(false);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() => btn.SetActive(true));
        seq.AppendCallback(() => btn.GetComponent<DOTweenAnimation>()?.DORestart());
        // TODO : 버튼들이 퍼지는 애니메이션 추가
        // TODO : 던전 이름 나오는 애니메이션 추가
    }

    void RefreshNotify()
    {
        var list = Managers.Quest.GetCurrentNotifyList();
        bool travelNotify = list.Exists(n => n.BtnID == VILLAGE1_TRAVEL_QUEST_VFX);
        GetObject((int)GameObjects.TravelButtonQuestVFX)?.SetActive(travelNotify);
    }

    void OnClickTravel()
    {
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        // TODO : Village1이 아니라 마지막 마을로 해야 됨. 아직은 데이터 없음.
        gameScene?.EnterAreaWithFade(AreaType.Village1);
    }
}
