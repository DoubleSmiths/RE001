using System.Collections.Generic;
using UnityEngine;
using static Util;

public class Define
{
    #region Enum
    public enum State
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
        Basic,
        Abnormal,
        Rare,
        Exceptional,
        Legendary,
        Mystic,
        REQUISITE,
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

    public enum Village1Group
    {
        Main,
        Guild,
        Alchemy,
        Monastery,
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

    public enum DialogueEmotion
    {
        Normal,
        Angry,
        Scared,
        Happy,
        Sad,
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

    public enum QuestCompleteType
    {
        ITEM_ACQUIRE,
        ITEM_COUNT,
        ANIM_END,
        MULTI,
        BTN_CLICK,      // 단일 버튼 클릭으로 완료
    }

    public enum QuestUnlockType
    {
        UI_BUTTON,
        SUBQUEST_START,
        ITEM_REWARD,    // 퀘스트 완료 시 아이템 지급
    }

    public enum QuestConditionType
    {
        BTN_CLICK,
        ITEM_ACQUIRE,
        ITEM_COUNT,
    }
    #endregion

    #region LocalizeKey
    // LocalizationData001.xlsx의 TextKey와 1:1 대응
    // 오타 방지 및 자동완성 지원
    public static class LocalizeKey
    {
        public const string TestKey = "TestKey";

        // ── GlobalButtons ──
        public const string TravelBtnText = "TravelBtnText";
        public const string ExitBtnText = "ExitBtnText";
        public const string TalkBtnText = "TalkBtnText";

        // ── ButtomButtons (하단 탭) ──
        public const string CharacterText = "CharacterText";
        public const string InventoryText = "InventoryText";
        public const string PotionText = "PotionText";
        public const string EquipmentText = "EquipmentText";
        public const string SkillText = "SkillText";
        public const string BestiaryText = "BestiaryText";

        // ── TownAsdalButtons (마을 버튼) ──
        public const string GuildText = "GuildText";
        public const string AlchemyText = "AlchemyText";
        public const string MonasteryText = "MonasteryText";

        // ── SettingPopup ──
        public const string SettingBGMText = "SettingBGMText";
        public const string SettingSFXText = "SettingSFXText";
        public const string SettingEffectText = "SettingEffectText";
        public const string SettingPopupText = "SettingPopupText";
        public const string SettingShakeText = "SettingShakeText";
    }
    #endregion

    #region NotifyKey
    // QuestNotifyData.BtnID / QuestConditionData.Target(BTN_CLICK) 과 1:1 대응
    // 오타 방지 및 자동완성 지원

    // Village1 Main Btn Group
    //public const string BTN_GUILD      = "BTN_GUILD";
    //public const string BTN_GUILD_TALK = "BTN_GUILD_TALK";
    //public const string BTN_EXIT       = "BTN_EXIT";
    public const string VILLAGE1_GUILD_VFX = "VILLAGE1_GUILD_VFX";
    public const string ALCHEMY_BUTTON_NEW_TXT = "ALCHEMY_BUTTON_NEW_TXT";
    public const string VILLAGE1_ALCHEMY_VFX_SUB1 = "VILLAGE1_ALCHEMY_VFX_SUB1";
    public const string MONASTERY_BUTTON_NEW_TXT = "MONASTERY_BUTTON_NEW_TXT";
    public const string MONASTERY_VFX_SUB1 = "MONASTERY_VFX_SUB1";
    public const string TRAVEL_BUTTON_NEW_TXT = "TRAVEL_BUTTON_NEW_TXT";
    public const string VILLAGE1_TRAVEL_QUEST_VFX = "VILLAGE1_TRAVEL_QUEST_VFX";

    public const string BTN_EXIT = "BTN_EXIT";

    // Travel UI (미구현)
    public const string BTN_JOURNEY = "BTN_JOURNEY";
    public const string BTN_EXPLORE = "BTN_EXPLORE";
    public const string BTN_MAP_BREEZING_WIND_FOREST = "BTN_MAP_BREEZING_WIND_FOREST";
    public const string BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE = "BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE";

    // Village1 Build Btn Group
    public const string VILLAGE1_GUILD_TALK_VFX = "VILLAGE1_GUILD_TALK_VFX";
    public const string VILLAGE1_GUILD_QUEST_VFX = "VILLAGE1_GUILD_QUEST_VFX";
    public const string GUILD_EXIT_QUEST_VFX = "GUILD_EXIT_QUEST_VFX";

    // Village1 Alchemy Btn Group
    public const string VILLAGE1_ALCHEMY_TALK_VFX = "VILLAGE1_ALCHEMY_TALK_VFX";
    public const string VILLAGE1_ALCHEMY_TALK_VFX_SUB1 = "VILLAGE1_ALCHEMY_TALK_VFX_SUB1";
    public const string VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT = "VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT";
    public const string VILLAGE1_ALCHEMY_QUEST_VFX = "VILLAGE1_ALCHEMY_QUEST_VFX";
    public const string ALCHEMY_EXIT_QUEST_VFX = "ALCHEMY_EXIT_QUEST_VFX";

    // Village1 Monastery Btn Group
    public const string VILLAGE1_MONASTERY_TALK_VFX = "VILLAGE1_MONASTERY_TALK_VFX";
    public const string VILLAGE1_MONASTERY_TALK_VFX_SUB1 = "VILLAGE1_MONASTERY_TALK_VFX_SUB1";
    public const string VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT = "VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT";
    public const string VILLAGE1_MONASTERY_QUEST_VFX = "VILLAGE1_MONASTERY_QUEST_VFX";
    public const string MONASTERY_EXIT_QUEST_VFX = "MONASTERY_EXIT_QUEST_VFX";
    #endregion

    #region UnlockKey
    // QuestUnlockData.TargetID (UnlockType = UI_BUTTON) 과 1:1 대응
    // UI_GameScene 하단 탭 버튼
    public const string UNLOCK_INVENTORY_BUTTON = "UNLOCK_INVENTORY_BUTTON";

    // UI_Village1BtnGroup 이동 버튼
    public const string UNLOCK_TRAVEL_BUTTON    = "UNLOCK_TRAVEL_BUTTON";
    #endregion

    // Mythic_Button
    public const string MYTHIC_BUTTON_X = "Mythic_Button_X.sprite";
    public const string MYTHIC_BUTTON_CHECK = "Mythic_Button_Check.sprite";
    public const string MYTHIC_BUTTON_RETURN = "Mythic_Button_Return.sprite";

    public const int ITEM_ID_MIN = 1000;
    public const int EQUIP_ID_MIN = 2000;
    public const int POTION_ID_MIN = 3000;
    public const int IMPORTANT_ITEM_ID_MIN = 9000;

    // COLOR
    public const string DEFAULT_COLOR = "FFFFFF";
    public const string SHORTAGE_COLOR = "FEA400";
    public const string EMPTY_COLOR = "FF0000";
    public const string COMPLETE_COLOR = "00FF00";
    public const string QUEST_FOLLOW_DEFAULT_COLOR = "FFC84E";
}
