using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _ui;
    GameManager _game;

    PlayerController _player;

#if UNITY_EDITOR
    bool isPreload = false;

    private void Awake()
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            //GetObject((int)GameObjects.Slider).GetComponent<Slider>().value = (float)count / totalCount;
            if (count == totalCount)
            {
                //GetButton((int)Buttons.StartButton).gameObject.SetActive(true);
                isPreload = true;
                Managers.Data.Init();
                Managers.Game.Init();
                Managers.Font.Init();
                Managers.Sound.Init();
                Managers.Quest.Init();
                Managers.SubQuest.Init();
                Init();
                BindPlayer();
                //StartAnimation();
            }
        });
    }
#else
    private void Awake()
    {

        Init();

        // 데이터는 TitleScene의 프리로드에서 준비된다. 플레이어 연결은 Awake 완료 후 Start에서 한다.

        // Fade 효과를 UI_Scene에 배치?

    }
#endif

    protected override void Init()
    {
        Debug.Log("@>> GameScene Init()");
        base.Init();

        SceneType = Define.Scene.GameScene;

        _ui = Managers.UI.ShowSceneUI<UI_GameScene>();

        // TODO : 배경 사운드
    }

    void Start()
    {
#if !UNITY_EDITOR
        BindPlayer();
#endif
    }

    void BindPlayer()
    {
        _game = Managers.Game;
        _player = FindAnyObjectByType<PlayerController>();
        _game.Player = _player;
        Managers.Combo.Init(); // 내부에서 Player.OnHitLanded를 구독한다.
        if (_player == null) return;

        // 데이터 로드 전에 실행된 Awake의 스탯을 저장된 진행도로 다시 계산한다.
        _player.RefreshStats();
        _ui.BindPlayer(_player);
        var reward = _game.ClaimOfflineReward();
        if (reward.Kills > 0)
            _ui.ShowOfflineReward(reward);
        Managers.Shop?.RestoreHotBuffs();   // 켜져 있던 핫타임 아이콘(버프 줄)을 다시 띄운다
    }

    void Update()
    {
        
    }

    public override void Clear()
    {

    }
}
