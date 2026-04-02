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

    // TODO : 그냥 여기서 전투까지 하기.

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.TravelButton).gameObject.BindEvent(OnClickTravel);

        Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();
        ChangeMap();

        for (int i = 0; i < 6; i++)
        {
            //if (i == (int)Buttons.TravelButton) continue;
            var tempBtn = GetButton(i).gameObject;
            tempBtn.SetActive(false);
        }

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
    }

    public override void PlayEnterAnimation()
    {
        // TODO : DOTweenAnimation이랑 같이 쓰는게 맞는지 모르겠음. DOTweenAnimation은 애니메이션이 끝나면 자동으로 꺼지는게 아니라서, 애니메이션이 끝난 후에 버튼을 활성화하는 방식으로 해야할듯.
        // TODO : DOTweenAnimation가 있는 자식들을 다 가져오기

        //var btn = GetButton((int)Buttons.TravelButton).gameObject;
        //btn.SetActive(false);

        // Toast Test
        Managers.UI.ShowToast("산들바람 숲(잎구)");

        Sequence seq = DOTween.Sequence();

        // TODO : 버튼들이 퍼지는 애니메이션 추가, 아래가 그거긴함. 버튼에 이미지가 없던데 뭐가 들어가는 지 모름.
        for (int i = 0; i < 6; i++)
        {
            //if (i == (int)Buttons.TravelButton) continue;
            var tempBtn = GetButton(i).gameObject;

            seq.AppendCallback(() => tempBtn.SetActive(true));
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() => tempBtn.GetComponent<DOTweenAnimation>()?.DORestart());
        }
        //seq.AppendInterval(0.2f);
        //seq.AppendCallback(() => btn.SetActive(true));
        //seq.AppendCallback(() => btn.GetComponent<DOTweenAnimation>()?.DORestart());
        // TODO : 던전 이름 나오는 애니메이션 추가

        // 캐릭터 애니메이션은 여기서 달리기로 바꿔야하나?
    }

    void RefreshNotify()
    {
        var list = Managers.Quest.GetCurrentNotifyList();
        bool travelNotify = list.Exists(n => n.BtnID == VILLAGE1_TRAVEL_QUEST_VFX);
        GetObject((int)GameObjects.TravelButtonQuestVFX)?.SetActive(travelNotify);
    }

    void OnClickTravel()
    {
        GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        Destroy(GameObject.Find("CH1BG")); // TODO : 맵마다 배경이 다르니까 데이터로 관리해야할듯

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        // TODO : Village1이 아니라 마지막 마을로 해야 됨. 아직은 데이터 없음.
        gameScene?.EnterAreaWithFade(AreaType.Village1);
    }

    void ChangeMap()
    {
        GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        Managers.Resource.Instantiate("CH1BG"); // TODO : 맵마다 배경이 다르니까 데이터로 관리해야할듯
    }
}
