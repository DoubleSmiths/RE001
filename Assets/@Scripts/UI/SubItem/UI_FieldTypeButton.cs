using UnityEngine;

public class UI_FieldTypeButton : UI_Base
{
    #region Enum

    enum Images
    {
        FieldTypeButtonQuestVFXIcon,
    }

    enum Texts
    {
        FieldTypeButtonText,
    }

    enum Buttons
    {
        UI_FieldTypeButton,
    }

    enum GameObjects
    {
        FieldTypeButtonQuestVFX,
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
