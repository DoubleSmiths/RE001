using UnityEngine;

public class UI_WarningPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        TameSlotMaxWarningPopupImage1,
        TameSlotMaxWarningPopupImage2,
        TameSlotMaxWarningPopupImage3,
    }

    enum Texts
    {
        TameSlotMaxWarningPopupText,
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        WarningPopup,
        TameSlotMaxWarningPopup,
    }

    #endregion

    override public bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        return true;
    }
}
