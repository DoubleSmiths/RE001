using static Define;

public abstract class UI_AreaBtnGroup : UI_Base
{
    protected UI_GameScene _gameScene;

    public void Setup(UI_GameScene gameScene)
    {
        _gameScene = gameScene;
        Init();
    }
}
