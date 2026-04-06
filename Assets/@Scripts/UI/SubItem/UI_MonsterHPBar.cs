using UnityEngine;

public class UI_MonsterHPBar : UI_Base
{
    #region Enum

    enum Images
    {
        Frame,
        MonsterHPBar,
        MonsterHPBarGuage,
    }

    enum Texts
    {
        MonsterNameDisplay,
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
