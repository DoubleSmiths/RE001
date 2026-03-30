using UnityEngine;

public class UI_ItemPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        _0,
        _2,
        Image_4,
        Image_5,
        ItemPopupFrame,
        ItemImg,
        dot,
        dot_1,
        __1,
        ScrollNotHaveBtn,
        SOMNotHaveBtn,
        MORNotHaveBtn,
        DuriShowPanel,
        Flash_USETHIS,
        Xbutton,
    }

    enum Texts
    {
        AmountText,
        ItemPopupItemName,
        raretext,
        ItemPopupItemDesc_Rare,
        wherefoundtext,
        ItemPopupItemDesc_Found,
        ItemPopupItemDesc,
        Text_Legacy,
        Text_Legacy_1,
        GameObject,
    }

    enum Buttons
    {
        ItemPopupBackground,
        _1,
        ScrollUseBtn,
        SOMUseBtn,
        MORUseBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        ItemPopup,
        ItemPopupBackground_SOMGroup,
        ItemPopupMain,
        shineeffect,
        ItemPopupImgGroup,
        ItemPopupTitle,
        ButtonGroup,
        ScrollBtnGroup,
        SOMBtnGroup,
        MORBtnGroup,
        ItemPopupFlash,
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

        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.ItemPopupBackground).gameObject.BindEvent(OnClickItemPopupBackground);

        return true;
    }


    public void SetInfo(int id)
    {
        // TODO : ID에 맞는 아이템 팝업 열기.
        
    }

    void OnClickItemPopupBackground()
    {
        ClosePopupUI();
    }

}
