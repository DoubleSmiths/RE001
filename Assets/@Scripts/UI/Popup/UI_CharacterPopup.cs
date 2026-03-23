using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        CharacterPopupFrame,
        ContentsStats,
        STRStatGroup,
        VITStatGroup,
        DEXStatGroup,
        MAGStatGroup,
        Mask,
        StatDetailFrame,
        CCDetailGroup,
        ACCDetailGroup,
        PRESDetailGroup,
        EVADetailGroup,
        EXPRATEDetailGroup,
        CDMGDetailGroup,
        ASDetailGroup,
        MRESDetailGroup,
        RECOVDetailGroup,
        DROPRATEDetailGroup,
        StatDetailFrame1,
        STRDetailGroup1,
        DEXDetailGroup1,
        Xbutton,
    }

    enum Texts
    {
        title,
        StatPointsTxt,
        ElixirPointsTxt,
        STRAmountTxt,
        STRInvPoint,
        VITAmountTxt,
        VITInvPoint,
        DEXAmountTxt,
        DEXInvPoint,
        MAGAmountTxt,
        MAGInvPoint,
        Text,
        Text2,
    }

    enum Buttons
    {
        CharacterPopupBackground,
        STRDePointBtn,
        STRInPointBtn,
        VITDePointBtn,
        VITInPointBtn,
        DEXDePointBtn,
        DEXInPointBtn,
        MAGDePointBtn,
        MAGInPointBtn,
        StatResetBtn,
        StatDetailBtn,
        StatConfirmBtn,
        PATKDetailGroup,
        HPDetailGroup,
        MATKDetailGroup,
        DEFDetailGroup,
    }

    enum GameObjects
    {
        FIXED,
        InFrame,
        CharacterPopup,
        Contents,
        PointsandElixirBar,
        TopLayout,
        BtmLayout,
        Left,
        SubTitleATK,
        _3,
        SubTitleDEF,
        _4,
        SubTitleUTIL,
        Right,
        _1,
        _2,
        Layout3,
    }

    #endregion

    Tween _popupTween;

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

        GetButton((int)Buttons.CharacterPopupBackground).gameObject.BindEvent(() => OnClickBG());

        OpenAnimation();
        return true;
    }

    void OpenAnimation()
    {
        DOTweenAnimation popupAnim = GetButton((int)Buttons.CharacterPopupBackground).gameObject.GetComponent<DOTweenAnimation>();
        popupAnim.DOKill();
        popupAnim.CreateTween();
        _popupTween = popupAnim.tween;
        _popupTween.SetAutoKill(false);
        _popupTween.SetUpdate(true);
        _popupTween.OnRewind(() =>
        {
            ClosePopupUI();
        });

        _popupTween.Restart();
    }

    void OnClickBG()
    {
        _popupTween.Goto(_popupTween.Duration(), false);
        _popupTween.PlayBackwards();
    }

}