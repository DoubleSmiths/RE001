using System.Collections.Generic;
using UnityEngine;
using static Util;

public class Define
{
    #region Enum
    public enum CreatureState
    {
        Idle,
        Moving,
        Attack,
        OnDamaged,
        Dead,
    }

    public enum ObjectType
    {
        Player,
        Monster,
        Projectile,
    }

    public enum EquipSlotType
    {
        Weapon,
        Helmet,
        Armor,
        Gloves,
        Legs,
        Accessory,
    }

    public enum EquipGrade
    {
        Common,
        Uncommon,
        Rare,
        Epic,
    }

    public enum QuestType
    {
        KillMonster,
        ReachArea,
        GetItem,
        TalkNpc,
    }

    public enum SkillCategory
    {
        Talent,
        Memory,
    }

    public enum SkillActiveType
    {
        Passive,
        Sequence,
        Repeat,
    }

    public enum CraftCategory
    {
        Potion,
        Skill,
    }

    public enum Scene
    {
        Unknown,
        GameScene,
        TitleScene,
        HubScene,
        BattleScene,
    }

    public enum AreaType
    {
        Village1,
        Battle,
    }

    public enum TabType
    {
        Status,
        Bag,
        Equipment,
        Skill,
        Sword,
        Potion,
        Bestiary,
    }

    public enum MinMonGrade
    {
        E,
        D,
        C,
        B,
        A,
        S,
    }

    public enum Language
    {
        Korean,
        English,
    }

    public enum FontType
    {
        UI,        // 언어에 따라 폰트 변경 (KR: DOSSaemmul / EN: PF_Stardust)
        Dialogue,  // 항상 고정 (Cafe24 Klassic)
    }

    public enum Sound
    {
        Bgm,
        SubBgm,
        Effect,
        Max,
    }

    public enum UIEvent
    {
        Click,
        Pressed,
        PointerDown,
        PointerUp,
        BeginDrag,
        Drag,
        EndDrag,
    }

    public enum ActivationKey
    {
        Village1GuildVFX,
        AlchemyButtonNewTxt,
        Village1AlchemyVFX_Sub1,
        MonasteryButtonNewTxt,
        MonasteryVFX_Sub1,
        Village1TravelQuestVFX,
        TravelButtonNewTxt,
    }
    #endregion

    #region LocalizeKey
    // LocalizationData001.xlsx의 TextKey와 1:1 대응
    // 오타 방지 및 자동완성 지원
    public static class LocalizeKey
    {
        // ── GlobalButtons ──
        public const string TravelBtnText = "TravelBtnText";
        public const string ExitBtnText   = "ExitBtnText";
        public const string TalkBtnText   = "TalkBtnText";

        // ── ButtomButtons (하단 탭) ──
        public const string CharacterText  = "CharacterText";
        public const string InventoryText  = "InventoryText";
        public const string PotionText     = "PotionText";
        public const string EquipmentText  = "EquipmentText";
        public const string SkillText      = "SkillText";
        public const string BestiaryText   = "BestiaryText";

        // ── TownAsdalButtons (마을 버튼) ──
        public const string GuildText     = "GuildText";
        public const string AlchemyText   = "AlchemyText";
        public const string MonasteryText = "MonasteryText";

        // ── SettingPopup ──
        public const string SettingBGMText    = "SettingBGMText";
        public const string SettingSFXText    = "SettingSFXText";
        public const string SettingEffectText = "SettingEffectText";
        public const string SettingPopupText  = "SettingPopupText";
        public const string SettingShakeText  = "SettingShakeText";
    }
    #endregion

}
