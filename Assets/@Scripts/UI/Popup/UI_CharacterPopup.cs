using DG.Tweening;
using TMPro;
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
        StatDetailFrame_1,
        STRDetailGroup1,
        DEXDetailGroup1,
        Xbutton,
        StatResetBtnGrow,
        StatDetailBtnGrow,
        StatConfirmBtnGrow,
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
        Layout_3,
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

        //DOTweenAnimation popupAnim = GetButton((int)Buttons.CharacterPopupBackground).gameObject.GetComponent<DOTweenAnimation>();
        _frameRT = GetObject((int)GameObjects.CharacterPopup).gameObject.GetComponent<RectTransform>();
        GetButton((int)Buttons.CharacterPopupBackground).gameObject.BindEvent(() => OnClickBG());
        OpenAnimation();
        //OpenAnimation(popupAnim);

        GetImage((int)Images.StatDetailFrame).gameObject.SetActive(false);
        GetImage((int)Images.StatDetailFrame_1).gameObject.SetActive(false);
        GetObject((int)GameObjects.Layout_3).gameObject.SetActive(false);

        PlayerController player = Managers.Game.Player;
        if (player != null)
            SetInfo(player);

        return true;
    }

    public void SetInfo(PlayerController player)
    {
        // ── 기본 스탯 (4대 스탯) ──
        SetGroupValue(GetButton((int)Buttons.PATKDetailGroup).gameObject, Util.FormatStat(player.PhysicalAtk));
        SetGroupValue(GetButton((int)Buttons.MATKDetailGroup).gameObject, Util.FormatStat(player.MagicAtk));
        SetGroupValue(GetButton((int)Buttons.HPDetailGroup).gameObject,   Util.FormatStat(player.MaxHp));
        SetGroupValue(GetButton((int)Buttons.DEFDetailGroup).gameObject,  Util.FormatStat(player.Def));

        // ── 상세 능력치 ──
        SetGroupValue(GetImage((int)Images.CCDetailGroup).gameObject,       Util.FormatStat(player.CritRate,    2, percent: true));
        SetGroupValue(GetImage((int)Images.CDMGDetailGroup).gameObject,     Util.FormatStat(player.CritDamage,  0, percent: true));
        SetGroupValue(GetImage((int)Images.ACCDetailGroup).gameObject,      Util.FormatStat(player.Accuracy,    1, percent: true));
        SetGroupValue(GetImage((int)Images.ASDetailGroup).gameObject,       Util.FormatStat(player.AtkSpeed,    0, percent: true));
        SetGroupValue(GetImage((int)Images.PRESDetailGroup).gameObject,     Util.FormatStat(player.PhysicalResist, 2, percent: true));
        SetGroupValue(GetImage((int)Images.MRESDetailGroup).gameObject,     Util.FormatStat(player.MagicResist, 2, percent: true));
        SetGroupValue(GetImage((int)Images.EVADetailGroup).gameObject,      Util.FormatStat(player.Evasion,     2, percent: true));
        SetGroupValue(GetImage((int)Images.RECOVDetailGroup).gameObject,    Util.FormatStat(player.RecoveryRate,1, percent: true));
        SetGroupValue(GetImage((int)Images.EXPRATEDetailGroup).gameObject,  Util.FormatStat(player.GrowthRate,  0, percent: true));
        SetGroupValue(GetImage((int)Images.DROPRATEDetailGroup).gameObject, Util.FormatStat(player.DropRate,    0, percent: true));
    }

    // 그룹 내 마지막 TMP_Text를 값 표시 텍스트로 사용
    void SetGroupValue(GameObject group, string value)
    {
        TMP_Text[] texts = group.GetComponentsInChildren<TMP_Text>();
        if (texts.Length > 0)
            texts[texts.Length - 1].text = value;
    }


    public void OpenAnimation(DOTweenAnimation doanim)
    {
        doanim.DOKill();
        doanim.CreateTween();
        doanim.tween.SetAutoKill(false);
        doanim.tween.SetUpdate(true);
        doanim.tween.OnRewind(() =>
        {
            ClosePopupUI();
        });

        doanim.tween.Restart();
    }

    //public void OnClickBG(DOTweenAnimation doanim)
    //{
    //    doanim.tween.Goto(doanim.tween.Duration(), false);
    //    doanim.tween.PlayBackwards();
    //}

}