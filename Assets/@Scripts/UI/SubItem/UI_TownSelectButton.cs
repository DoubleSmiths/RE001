using UnityEngine;
using UnityEngine.UI;

public class UI_TownSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        TownSelectButtonIconFrame,
        TownSelectButtonIcon,
        TownSelectButtonQuestVFXIcon,
        EFrame,
    }

    enum Texts
    {
        TownSelectButtonText,
        TownSelectPopupCurrentLocationText,
    }

    enum Buttons
    {
        UI_TownSelectButton,
    }

    enum GameObjects
    {
        TownSelectButtonQuestVFX,
        TownSelectPopupActiveEf,
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
