using UnityEngine;

public class UI_AttactedPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        AttactedPopupImage1,
        AttactedPopupImage2,
        AttactedPopupMask,
        AttactedPopupGlow,
        AttactedPopupImage3,
    }

    enum Texts
    {
        AttactedPopupText,
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        AttactedPopup,
    }

    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }
}
