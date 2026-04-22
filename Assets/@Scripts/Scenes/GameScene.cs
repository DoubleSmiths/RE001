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
                _game = Managers.Game;
                _player = FindAnyObjectByType<PlayerController>();
                Managers.Game.Player = _player;
                Managers.Combo.Init();              // 콤보 매니저 초기화
                Managers.Combo.AttachPlayer(_player); // Player.OnHitLanded 구독
                if (_player != null)
                    _ui.BindPlayer(_player);
                //StartAnimation();
            }
        });
    }
#else
    private void Awake()
    {

        Init();

        // TODO : 게임 데이터 불러오기
         _game = Managers.Game;

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
        
    }

    void Update()
    {
        
    }

    public override void Clear()
    {

    }
}
