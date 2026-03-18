using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _ui;
    GameManager _game;

    PlayerController _player;

    private void Awake()
    {
        Init();

        // TODO : 게임 데이터 불러오기
         _game = Managers.Game;

        // Fade 효과를 UI_Scene에 배치?

    }

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
