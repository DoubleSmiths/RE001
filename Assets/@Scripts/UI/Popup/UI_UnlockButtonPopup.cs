using UnityEngine;

public class UI_UnlockButtonPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        BlockImg,
        Content,
        Image4,
        Image5,
        TravelUnlockTutoIcon,
        Icon,
        TextBoxDialogueNextBtnImg,
    }

    enum Texts
    {
        Text,
        TextTMP,
    }

    enum Buttons
    {
        TravelUnlockTutoCloseBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        icon,
        shineeffect,
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

    // 어떤 버튼이 해금되는지에 따라 정보를 세팅하는 함수
    public void SetInfo()
    {
        
    }

    public void OnClickBlockImg()
    {
        ClosePopupUI();
    }
}
