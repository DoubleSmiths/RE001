using UnityEngine;

public class UI_MapSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        MapSelectButtonIconFrame,
        MapSelectButtonIcon,
        MapSelectButtonQuestVFXIcon,
    }

    enum Texts
    {
        MapSelectButtonText,
    }

    enum Buttons
    {
        UI_MapSelectButton,
    }

    enum GameObjects
    {
        MapSelectButtonQuestVFX,
    }

    #endregion


    private void Awake()
    {
        Init();
    }
    override public bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        return true;
    }


}
