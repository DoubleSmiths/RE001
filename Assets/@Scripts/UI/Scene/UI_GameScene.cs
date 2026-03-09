using UnityEngine;

public class UI_GameScene : UI_Scene
{
    #region Enum
    enum Images
    {

    }

    enum Texts
    {

    }

    enum Buttons
    {

    }

    enum GameObjects
    {

    }

    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        return true;
    }

    public void SetInfo()
    {
        // TODO Set Player Info
    }


}
