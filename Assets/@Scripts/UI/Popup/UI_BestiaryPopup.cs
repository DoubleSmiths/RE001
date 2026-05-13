using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class UI_BestiaryPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        Xbutton,
    }

    enum Buttons
    {
        BestiaryPopupBackground,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        BestiaryPopup,
        BestiaryPopupFrame,
    }

    #endregion

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        _frameRT = GetObject((int)GameObjects.BestiaryPopup).GetComponent<RectTransform>();
        GetButton((int)Buttons.BestiaryPopupBackground).gameObject.BindEvent(OnClickBG);

        return true;
    }

    public void SetInfo()
    {

    }

}
