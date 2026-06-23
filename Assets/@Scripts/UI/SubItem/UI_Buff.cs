using UnityEngine;

public class UI_Buff : UI_Base
{
    #region Enum

    enum Images
    {
        Frame,
        Buff_Icon,
        Duri_Anim,
        Flash_Anim,
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

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        return true;
    }
}
