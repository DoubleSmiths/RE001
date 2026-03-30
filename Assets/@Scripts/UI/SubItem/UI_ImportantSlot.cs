using UnityEngine;

public class UI_ImportantSlot : UI_Base
{
    #region Enum

    enum Images
    {
        GameObject,
        Frame,
        Img,
    }

    enum Texts
    {
        AmountTxt,
        ImportantSlotNewTxt,
    }

    enum Buttons
    {
        UI_ImportantSlot,
    }

    enum GameObjects
    {
    }

    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        #endregion
        return true;
    }

}
