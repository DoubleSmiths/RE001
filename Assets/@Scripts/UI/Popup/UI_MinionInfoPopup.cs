using UnityEngine;

public class UI_MinionInfoPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        MinionInfoPopupBlock,
        shineeffectImage1,
        shineeffectImage2,
        MinionInfoPopupFrame,
        EFrame,
        MinionInfoPopupMonsterBox,
        MinionInfoPopupMonsterBoxMask,
        MinimonImage,
        BlockImg,
        AllSStar,
        MinionInfoPopupSkillBox,
        MinionUsingSkillFrame,
        MinionUsingSkillBackground,
        MinionInfoPopupInfoBox,
        MinionInfoPopupNameFrame,
        MinionInfoPopupNameFrameMask,
        MinionInfoPopupNameFrameImage,
        MinionInfoPopupDmgFrame,
        MinionInfoPopupDmgFrameMask,
        DamageRankImage,
        MinionInfoPopupASFrame,
        MinionInfoPopupASFrameMask,
        ASRankImage,
        MinionInfoPopupCritFrame,
        MinionInfoPopupCritFrameMask,
        CritRankImage,
        MinionSummonButtonImage,
        MinionSummonButtonMask,
        Img_Item_03,
        Img_Item_06,
        MinionSummonedButtonImage,
        MinionSummonedButtonMask,
        Img_Item_04,
        Img_Item_01,
        MinionCantSummonButton,
        MinionCantSummonButtonImage,
        MinionCantSummonButtonMask,
        Img_Item_02,
        Img_Item_05,
        Xbutton,
    }

    enum Texts
    {
        MinionInfoPopupTitle,
        BlockImgText,
        MinionNumberText,
        SYNCText,
        SkillNameText,
        MinionNotUseSkillText,
        MinionInfoPopupNameText,
        DamageStringText,
        DamageNumberText,
        ASStringText,
        ASNumberText,
        CritStringText,
        CritNumberText,
        MinionSummonButtonText,
        MinionSummonedButtonText,
        MinionCantSummonButtonText,
        MinionNewText,
        MinionATKNewText,
        MinionASNewText,
        MinionCritNewText,
        NoImprovedText,
        GuranteedText,
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        MinionInfoPopupBackground,
        MinionUsingSkillIcon,
        MinionSummonButton,
        MinionSummonedButton,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        MinionInfoPopup,
        MinionInfoPopupPanel,
        shineeffect,
        ActiveEf,
        Star_GlowDot,
        VFX_Star_GlowDot_02,
        VFX_Star_GlowDot_04,
        SYNC_GlowDot,
        VFX_SYNC_GlowDot_01,
        VFX_SYNC_GlowDot_03,
        MinionUseSkill,
        MinionUsingSkill,
        MinionNotUseSkill,
        MinionInfoPopupDmgUpgradeSFX,
        VFX_MinionInfoPopupDmgUpgrad_05,
        VFX_MinionInfoPopupDmgUpgrad_04,
        VFX_MinionInfoPopupDmgUpgrad_02,
        VFX_MinionInfoPopupDmgUpgrad_01,
        VFX_GlowDot_02_End_01,
        MinionInfoPopupASUpgradeSFX,
        VFX_MinionInfoPopupASUpgrade_04,
        VFX_MinionInfoPopupASUpgrade_03,
        VFX_MinionInfoPopupASUpgrade_01,
        VFX_MinionInfoPopupASUpgrade_02,
        VFX_GlowDot_02_End_03,
        MinionInfoPopupCritUpgradeSFX,
        VFX_MinionInfoPopupCritUpgra_07,
        VFX_MinionInfoPopupCritUpgra_06,
        VFX_MinionInfoPopupCritUpgra_03,
        VFX_MinionInfoPopupCritUpgra_05,
        VFX_GlowDot_02_End_02,
        MinionInfoPopupButtonGroup,
    }

    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;


        return true;
    }
}
