using DG.Tweening;
using UnityEngine;
using static Define;

public class UI_BattleBtnGroup : UI_AreaBtnGroup
{
    #region Enum

    enum Buttons
    {
        TravelButton,
    }

    enum GameObjects
    {
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
        gameScene?.EnterAreaWithFade(AreaType.Village1);
    }
}
