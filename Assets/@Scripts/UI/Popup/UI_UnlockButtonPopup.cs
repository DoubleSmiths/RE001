using UnityEngine;
using static Util;

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
        IconNameText,
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

    private void Awake()
    {
        Init();
    }

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

        GetImage((int)Images.Content).gameObject.BindEvent(OnClickContent);

        return true;
    }

    // 어떤 버튼이 해금되는지에 따라 정보를 세팅하는 함수
    public void SetInfo(string iconSprite, string iconNameId, string iconDescId)
    {
        GetImage((int)Images.Icon).sprite = Managers.Resource.Load<Sprite>(iconSprite);
        GetText((int)Texts.IconNameText).text = Localize(iconNameId);
        GetText((int)Texts.TextTMP).text = Localize(iconDescId);
    }

    public void OnClickContent()
    {
        ClosePopupUI();
    }
}
