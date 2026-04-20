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

    Coroutine _battleCoroutine;

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

        //Set();
        ChangeMap();
        Managers.Game.Player.State = State.Moving;
        Managers.Game.Player.StartBattle(); // _inBattle = true (PlayEnterAnimation 미호출 환경 대비)

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
        EndBattle();
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
    }

    public void Set()
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1.0f);
        seq.AppendCallback(() =>
        {
            ChangeMap();
            Managers.Game.Player.State = State.Moving;
        });
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

        StartBattle();
    }

    void StartBattle()
    {
        PlayerController player = Managers.Game.Player;
        if (player == null) return;

        player.StartBattle();
        _battleCoroutine = StartCoroutine(CoBattleLoop());
    }

    System.Collections.IEnumerator CoBattleLoop()
    {
        PlayerController player = Managers.Game.Player;
        Transform monsterRoot = Managers.Object.MonsterTransform;
        Vector3 spawnPos = new Vector3(5, 2, 0);

        while (true)
        {
            MonsterController mc = Managers.Object.SpawnMonster(2001, spawnPos); // TODO : 일단 2001로 고정인데, 나중에 데이터로 관리해야할듯
            if (mc != null)
            {
                mc.OnEngaged += () =>
                {
                    Managers.UI.MakeSubItem<UI_MonsterHPBar>(Managers.UI.SceneUI.transform).SetInfo(mc);
                };
            }
            yield return new WaitUntil(() => monsterRoot.childCount == 0);
            yield return new WaitForSeconds(1f);
        }
    }

    void EndBattle()
    {
        Managers.Game.Player?.StopBattle();

        if (_battleCoroutine != null)
        {
            StopCoroutine(_battleCoroutine);
            _battleCoroutine = null;
        }

        // 남아있는 몬스터 전부 제거
        Transform monsterRoot = Managers.Object.MonsterTransform;
        for (int i = monsterRoot.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(monsterRoot.GetChild(i).gameObject);

        // 남아있는 몬스터 HP바 전부 제거
        if (Managers.UI.SceneUI != null)
        {
            foreach (var bar in Managers.UI.SceneUI.GetComponentsInChildren<UI_MonsterHPBar>(true))
                Managers.Resource.Destroy(bar.gameObject);
        }
    }

    void RefreshNotify()
    {
        var list = Managers.Quest.GetCurrentNotifyList();
        bool travelNotify = list.Exists(n => n.BtnID == VILLAGE1_TRAVEL_QUEST_VFX);
        GetObject((int)GameObjects.TravelButtonQuestVFX)?.SetActive(travelNotify);
    }

    void OnClickTravel()
    {
        EndBattle();

        // Battle BG 제거
        var ch1 = GameObject.Find("CH1BG") ?? GameObject.Find("CH1BG(Clone)");
        if (ch1 != null)
            Destroy(ch1);

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        // TODO : Village1이 아니라 마지막 마을로 해야 됨. 아직은 데이터 없음.
        gameScene?.EnterAreaWithFade(AreaType.Village1);
    }

    void ChangeMap()
    {
        // Village1→Battle 전환 시 SwitchBG가 legacy BackgroundImage를 이미 비활성화했을 수 있음
        var legacyBG = GameObject.Find("BackgroundImage");
        if (legacyBG != null)
        {
            var sr = legacyBG.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1, 1, 1, 0);
        }

        // Village1 BG prefab 인스턴스 정리 (마을 배경이 Battle 배경 뒤로 비치는 것 방지)
        foreach (var key in new[] { "Village1_Main_BG", "Village1_Guild_BG", "Village1_Alchemy_BG" })
        {
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == key || go.name == key + "(Clone)")
                    Destroy(go);
            }
        }

        Managers.Resource.Instantiate("CH1BG"); // TODO : 맵마다 배경이 다르니까 데이터로 관리해야할듯

        // 바닥은 마을 바닥(Village1_Main_Ground)을 재사용 — Village1_Main_BG prefab 스폰 후 배경 이미지만 비활성
        var villageBG = Managers.Resource.Instantiate("Village1_Main_BG");
        if (villageBG != null)
        {
            foreach (var t in villageBG.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Village1_Main_BG_Img")
                {
                    t.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }
}
