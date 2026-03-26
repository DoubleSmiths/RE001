using UnityEngine;

public class UI_InvSlot : UI_Base
{
    #region Enum

    enum Images
    {
        GameObject,
        Frame,
        Glow,
        Img,
        BlockImg,
    }

    enum Texts
    {
        AmountTxt,
        InvSlotNewTxt,
    }

    enum Buttons
    {
        UI_InvSlot,
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

    public void SetInfo()
    {

    }

}
