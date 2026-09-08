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
        Minion,     // 동행 미니몬(컴패니언) — 공격 시 DMG 폰트/타격은 플레이어와 동일 취급
    }

    public enum CreatureRarity
    {
        Common,
        Rare,
        Boss,
    }

    public enum AttackType
    {
        Physical,
        Magic,
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

    // 슬38 능력 종류. Talent(재능=점유량0) / Memory(기억) 는 기존 값이라 순서를 유지하고,
    // 기획서가 정의한 3분류(전투·유틸리티·편의성)를 뒤에 추가한다.
    public enum SkillCategory
    {
        Talent,
        Memory,
        Combat,        // 전투: 전투 능력치·기술
        Utility,       // 유틸리티: 포획 확률, 경험치 획득량 등
        Convenience,   // 편의성: 자동물약, 관심 최대 수 등
    }

    public enum SkillActiveType
    {
        Passive,
        Sequence,
        Repeat,
    }

    // ── 슬39: 능력 전투 발동조건 ──
    // 능력 발동 방식. None = 패시브(항시 스탯 가산, 기존 동작).
    public enum AbilityTrigger
    {
        None,         // 패시브 (스탯만 가산)
        Probability,  // 확률 발동: 조건 충족 시 ProcChance% 확률로 발동
        Time,         // 시간 발동: ActiveContext 충족 시 Cooldown마다 발동 (쿨타임 스킬)
        Active,       // 액티브: 플레이어가 스킬바에서 직접 눌러 발동. Cooldown 소모.
    }

    // 액티브 스킬바 슬롯 수 (기획서 퀵슬롯 6칸).
    public const int ACTIVE_SKILL_SLOTS = 6;

    // 확률 발동 조건 (슬39).
    public enum AbilityCondition
    {
        BattleStart,      // 전투 시작 시
        OnAttack,         // 공격 시
        AfterAttack,      // 공격 후 (명중/빗나감 판정이 끝난 뒤)
        OnHit,            // 타격(명중) 시
        OnCrit,           // 치명타 시
        OnMiss,           // 타격이 빗나갈 시 (순수 빗나감 — 상대 회피 제외)
        SelfEvade,        // 자신이 회피 시
        OnDamaged,        // 피격 시
        OnHeal,           // 체력 회복 시
        LowHp,            // 본인 HP가 ConditionValue% 이하가 될 시 (전투당 1회)
        // ── 슬39 stateful 조건 (AbilityRunner가 카운터/1회 추적) ──
        EnemyLowHp,       // 상대 HP가 ConditionValue% 이하가 될 시 (전투당 1회)
        FatalDamage,      // 사망에 이르는 공격을 받은 경우 (HP 차감 직전 통지 — Heal 효과면 생존 가능)
        ConsecutiveCrit,  // 연속 n번(ConditionValue) 타격이 치명타일 시
        ConsecutiveEvade, // 연속 n번(ConditionValue) 회피할 시
        EnemyEvade,       // 상대방이 회피할 시
        AfterAbilityUse,  // 특정 능력(ConditionValue=DataId, 0=아무 능력) 발동 후
    }

    // 시간 발동 맥락 (슬39).
    public enum AbilityContext
    {
        InBattle,     // 전투 중 발동
        OutOfBattle,  // 비전투 중 발동
        Always,       // 항시 발동
    }

    // 능력 발동 효과.
    public enum AbilityEffect
    {
        None,
        Heal,         // 본인 HP 회복 (EffectValue)
        BonusDamage,  // 현재 타겟에게 추가 피해 (EffectValue)
        Buff,         // 본인 스탯 일시 증가 (EffectStat += EffectValue, EffectDuration초; 0이면 영구/전투종료까지)
    }

    // ── 능력 정보창 #5: 능력 출처(획득 경로) ── 하단 UI 5종 분기에 사용.
    public enum AbilitySource
    {
        Unlimited,    // 무한한 잠재력 (2가지 이상 또는 다른 방식 — 도감 별 작업 등). 기본값.
        Class,        // 전직
        Minion,       // 미니몬 (공명도 100%) — RequiredMinionId의 미니몬 도감 게이팅(#6)
        Equip,        // 장비 강화 (+9)
        Npc,          // NPC에게 재료 제공
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
        Forge,
        Shop,
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
        Battle,    // 전투 UI 고정 (RiaSans-Bold) — 슬라이드 47
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
        ITEM_REMOVE,    // 퀘스트 완료 시 아이템 회수
    }

    public enum SubQuestCompleteType
    {
        NONE,           // 종료 상태 (더 이상 완료 조건 없음)
        LEVEL_COUNT,    // 지정 대상의 레벨/랭크 달성
        BTN_CLICK,      // 지정 버튼 클릭 (CompleteParam = BtnID)
    }

    public enum SubQuestShowTiming
    {
        ON_START,       // 서브퀘스트 시작 시부터 표시
    }

    public enum QuestConditionType
    {
        BTN_CLICK,
        ITEM_ACQUIRE,
        ITEM_COUNT,
    }
    #endregion

    #region LocalizeKey
    // LocalizationData의 TextKey와 1:1 대응 (자동 생성 - Tools/GenerateLocalizeKeys)
    public static class LocalizeKey
    {
        // ── Localization ──
        public const string TravelBtnText = "TravelBtnText";
        public const string ExitBtnText = "ExitBtnText";
        public const string TalkBtnText = "TalkBtnText";
        public const string ItemNeededText = "ItemNeededText";
        public const string NeedMatsText = "NeedMatsText";
        public const string DescriptionText = "DescriptionText";
        public const string NextInfoText = "NextInfoText";
        public const string BackBtnText = "BackBtnText";
        public const string CraftText = "CraftText";
        public const string RankText = "RankText";
        public const string LevelText = "LevelText";
        public const string StartText = "StartText";
        public const string BtmCharacterBtnText = "BtmCharacterBtnText";
        public const string BtmInventoryBtnText = "BtmInventoryBtnText";
        public const string BtmPotionBtnText = "BtmPotionBtnText";
        public const string BtmEquipmentBtnText = "BtmEquipmentBtnText";
        public const string BtmSkillBtnText = "BtmSkillBtnText";
        public const string BtmMinimonCodexBtnText = "BtmMinimonCodexBtnText";
        public const string GuildText = "GuildText";
        public const string QuestText = "QuestText";
        public const string AlchemyText = "AlchemyText";
        public const string CommissionText = "CommissionText";
        public const string MonasteryText = "MonasteryText";
        public const string AnointmentText = "AnointmentText";
        public const string SmithyText = "SmithyText";
        public const string ForgeText = "ForgeText";
        public const string StrengthText = "StrengthText";
        public const string VitialityText = "VitialityText";
        public const string AgilityText = "AgilityText";
        public const string MagicText = "MagicText";
        public const string AttackPowerText = "AttackPowerText";
        public const string PhysicalAttackText = "PhysicalAttackText";
        public const string MagicalAttackText = "MagicalAttackText";
        public const string CriticalText = "CriticalText";
        public const string CriticalDamageText = "CriticalDamageText";
        public const string AccuracyText = "AccuracyText";
        public const string AttackSpeedText = "AttackSpeedText";
        public const string MaxHitPointText = "MaxHitPointText";
        public const string DefenseText = "DefenseText";
        public const string PhysicalResistanceText = "PhysicalResistanceText";
        public const string MagicalResistanceText = "MagicalResistanceText";
        public const string EvadeText = "EvadeText";
        public const string RecoveryText = "RecoveryText";
        public const string ExperienceGainText = "ExperienceGainText";
        public const string DropRateText = "DropRateText";
        public const string FlatRecovText = "FlatRecovText";
        public const string MaxHPRecovText = "MaxHPRecovText";
        public const string TotalRecovText = "TotalRecovText";
        public const string PotionCDText = "PotionCDText";
        public const string BaseStatText = "BaseStatText";
        public const string SPDBonusText = "SPDBonusText";
        public const string DurationText = "DurationText";
        public const string SecondsUnitText = "SecondsUnitText";
        public const string IncreseStatText = "IncreseStatText";
        public const string BoostStatText = "BoostStatText";
        public const string DmgBoostText = "DmgBoostText";
        public const string PlayerLvPrefix = "PlayerLvPrefix";
        public const string CharPopupTitleText = "CharPopupTitleText";
        public const string CharPopupStatPointText = "CharPopupStatPointText";
        public const string CharPopupElixirText = "CharPopupElixirText";
        public const string CharPopupAttributeDetailText = "CharPopupAttributeDetailText";
        public const string CharPopupOffenseText = "CharPopupOffenseText";
        public const string CharPopupDefenseText = "CharPopupDefenseText";
        public const string CharPopupUtilityText = "CharPopupUtilityText";
        public const string TownText = "TownText";
        public const string ExploreText = "ExploreText";
        public const string CurrentLocationText = "CurrentLocationText";
        public const string HeadOutText = "HeadOutText";
        public const string AbilityCodex_TitleText = "AbilityCodex_TitleText";
        public const string AbilityCodex_BtmText_All = "AbilityCodex_BtmText_All";
        public const string AbilityCodex_BtmText_Skill = "AbilityCodex_BtmText_Skill";
        public const string AbilityCodex_BtmText_Spell = "AbilityCodex_BtmText_Spell";
        public const string AbilityCodex_BtmText_Def = "AbilityCodex_BtmText_Def";
        public const string AbilityCodex_BtmText_Util = "AbilityCodex_BtmText_Util";
        public const string AbilityCodex_TopText_All = "AbilityCodex_TopText_All";
        public const string AbilityCodex_TopText_Skill = "AbilityCodex_TopText_Skill";
        public const string AbilityCodex_TopText_Spell = "AbilityCodex_TopText_Spell";
        public const string AbilityCodex_TopText_Def = "AbilityCodex_TopText_Def";
        public const string AbilityCodex_TopText_Util = "AbilityCodex_TopText_Util";
        public const string Memorized_Text_Short = "Memorized_Text_Short";
        public const string Memorized_Text_Full = "Memorized_Text_Full";
        public const string AbilityMaster_Text = "AbilityMaster_Text";
        public const string AbilityDetails_TitleText = "AbilityDetails_TitleText";
        public const string TalentText = "TalentText";
        public const string Unlimited_Potential_Text = "Unlimited_Potential_Text";
        public const string Unlimited_Potential_Desc_Text = "Unlimited_Potential_Desc_Text";
        public const string Req_Minimon_Text = "Req_Minimon_Text";
        public const string Req_Minimon_SubText = "Req_Minimon_SubText";
        public const string Minimon_MaxRank_Text = "Minimon_MaxRank_Text";
        public const string Req_Equip_Text = "Req_Equip_Text";
        public const string Req_Item_Text = "Req_Item_Text";
        public const string Class_Ability_Text = "Class_Ability_Text";
        public const string Max_Enh_Text = "Max_Enh_Text";
        public const string MAX_Dur_Text = "MAX_Dur_Text";
        public const string Ability_MaxLevel_Text = "Ability_MaxLevel_Text";
        public const string Next_Level_Info_Text = "Next_Level_Info_Text";
        public const string MemorizeText = "MemorizeText";
        public const string Lack_Memory_Text = "Lack_Memory_Text";
        public const string Ability_Learn_Text = "Ability_Learn_Text";
        public const string Lack_Proficiency_Text = "Lack_Proficiency_Text";
        public const string NextLevelInfoBtnText = "NextLevelInfoBtnText";
        public const string MinimonCodexText = "MinimonCodexText";
        public const string CompletionText = "CompletionText";
        public const string CatchMinimonTitleText = "CatchMinimonTitleText";
        public const string CatchMinimonStartText1 = "CatchMinimonStartText1";
        public const string CatchMinimonStartText2 = "CatchMinimonStartText2";
        public const string LureBtnText = "LureBtnText";
        public const string CatchSuccessRateText = "CatchSuccessRateText";
        public const string CatchSuccessAtText = "CatchSuccessAtText";
        public const string GuaranteedCatchText = "GuaranteedCatchText";
        public const string GoMandrakeBtnText = "GoMandrakeBtnText";
        public const string ReqMandrakeText = "ReqMandrakeText";
        public const string TryLureText = "TryLureText";
        public const string LureNotEffectiveText = "LureNotEffectiveText";
        public const string LureEffectiveText = "LureEffectiveText";
        public const string LureFailText = "LureFailText";
        public const string LureSuccessText1 = "LureSuccessText1";
        public const string LureSuccessText2 = "LureSuccessText2";
        public const string GoMandrakeText = "GoMandrakeText";
        public const string EatMandrakeText = "EatMandrakeText";
        public const string MinimonEntryText = "MinimonEntryText";
        public const string LinkText = "LinkText";
        public const string LinkSkillText = "LinkSkillText";
        public const string Link100RequiredText = "Link100RequiredText";
        public const string InfoAndStatsText = "InfoAndStatsText";
        public const string MinimonNameText = "MinimonNameText";
        public const string HabitatText = "HabitatText";
        public const string EXPText = "EXPText";
        public const string EncounterText = "EncounterText";
        public const string AttentionRateText = "AttentionRateText";
        public const string LogText = "LogText";
        public const string LogCompletionText = "LogCompletionText";
        public const string LootText = "LootText";
        public const string MyMinimonText = "MyMinimonText";
        public const string RequireMinimonText = "RequireMinimonText";
        public const string MinimonSkillLockedText = "MinimonSkillLockedText";
        public const string ParticalGetText = "ParticalGetText";
        public const string CompleteGetText = "CompleteGetText";
        public const string FollowText = "FollowText";
        public const string FollowingText = "FollowingText";
        public const string MinimonAbilityText = "MinimonAbilityText";
        public const string FullGrowthInText = "FullGrowthInText";
        public const string MinimonGrowthFailText = "MinimonGrowthFailText";
        public const string SummonText = "SummonText";
        public const string MemorizeBtnText = "MemorizeBtnText";
        public const string MyMinimon = "MyMinimon";
        public const string PlaceEverywhereText = "PlaceEverywhereText";
        public const string PlaceNameTownAsdalText = "PlaceNameTownAsdalText";
        public const string BreezingWindForestNameText = "BreezingWindForestNameText";
        public const string BreezingWindForestEntranceText = "BreezingWindForestEntranceText";
        public const string WhisperingCaveNameText = "WhisperingCaveNameText";
        public const string WhisperingCaveEntranceText = "WhisperingCaveEntranceText";
        public const string RuinedSanctumNameText = "RuinedSanctumNameText";
        public const string RuinedSanctumEntranceText = "RuinedSanctumEntranceText";
        public const string PlaceNameBWF0Text = "PlaceNameBWF0Text";
        public const string PlaceNameBWF1Text = "PlaceNameBWF1Text";
        public const string PlaceNameBWF2Text = "PlaceNameBWF2Text";
        public const string PlaceNameBWF3Text = "PlaceNameBWF3Text";
        public const string RarityBasicText = "RarityBasicText";
        public const string RarityAbnormalText = "RarityAbnormalText";
        public const string RarityRareText = "RarityRareText";
        public const string RarityExceptionalText = "RarityExceptionalText";
        public const string RarityLegendaryText = "RarityLegendaryText";
        public const string RarityMysticText = "RarityMysticText";
        public const string RarityREQUISITEText = "RarityREQUISITEText";
        public const string RequisiteBagText = "RequisiteBagText";
        public const string ItemJellyPongNameText = "ItemJellyPongNameText";
        public const string ItemRabiJellyNameText = "ItemRabiJellyNameText";
        public const string ItemPongyaPearlNameText = "ItemPongyaPearlNameText";
        public const string ItemScrollOfENH1NameText = "ItemScrollOfENH1NameText";
        public const string ItemScrollOfENH2NameText = "ItemScrollOfENH2NameText";
        public const string ItemScrollOfENH3NameText = "ItemScrollOfENH3NameText";
        public const string ItemScrollOfENH4NameText = "ItemScrollOfENH4NameText";
        public const string ItemMORNameText = "ItemMORNameText";
        public const string ItemMastersSigil1NameText = "ItemMastersSigil1NameText";
        public const string ItemMastersSigil2NameText = "ItemMastersSigil2NameText";
        public const string ItemMastersSigil3NameText = "ItemMastersSigil3NameText";
        public const string ItemTempPermitNameText = "ItemTempPermitNameText";
        public const string ItemRegularPermitNameText = "ItemRegularPermitNameText";
        public const string HPPotion1NameText = "HPPotion1NameText";
        public const string HPPotion2NameText = "HPPotion2NameText";
        public const string SPDPotion1NameText = "SPDPotion1NameText";
        public const string SPDPotion2NameText = "SPDPotion2NameText";
        public const string ItemJellyPongDescText = "ItemJellyPongDescText";
        public const string ItemRabiJellyDescText = "ItemRabiJellyDescText";
        public const string ItemPongyaPearlDescText = "ItemPongyaPearlDescText";
        public const string ItemScrollOfENH1DescText = "ItemScrollOfENH1DescText";
        public const string ItemScrollOfENH2DescText = "ItemScrollOfENH2DescText";
        public const string ItemScrollOfENH3DescText = "ItemScrollOfENH3DescText";
        public const string ItemScrollOfENH4DescText = "ItemScrollOfENH4DescText";
        public const string ItemMORDescText = "ItemMORDescText";
        public const string ItemMastersSigil1DescText = "ItemMastersSigil1DescText";
        public const string ItemMastersSigil2DescText = "ItemMastersSigil2DescText";
        public const string ItemMastersSigil3DescText = "ItemMastersSigil3DescText";
        public const string ItemTempPermitDescText = "ItemTempPermitDescText";
        public const string ItemRegularPermitDescText = "ItemRegularPermitDescText";
        public const string HPPotion1DescText = "HPPotion1DescText";
        public const string HPPotion2DescText = "HPPotion2DescText";
        public const string SPDPotion0DescText = "SPDPotion0DescText";
        public const string SPDPotion1DescText = "SPDPotion1DescText";
        public const string SPDPotion2DescText = "SPDPotion2DescText";
        public const string ComboText = "ComboText";
        public const string ComboLv0Text = "ComboLv0Text";
        public const string ComboLv1Text = "ComboLv1Text";
        public const string ComboLv2Text = "ComboLv2Text";
        public const string ComboLv3Text = "ComboLv3Text";
        public const string ComboLv4Text = "ComboLv4Text";
        public const string ComboLv5Text = "ComboLv5Text";
        public const string ComboLv6Text = "ComboLv6Text";
        public const string BattleLogText = "BattleLogText";
        public const string BattleTimeText = "BattleTimeText";
        public const string DmgDealtText = "DmgDealtText";
        public const string DmgTakenText = "DmgTakenText";
        public const string HPRestoredText = "HPRestoredText";
        public const string ReturnText = "ReturnText";
        public const string ReturnConfirmText = "ReturnConfirmText";
        public const string CritTextPopupText = "CritTextPopupText";
        public const string BattleMissText = "BattleMissText";
        public const string MinimonInterestSuccessText = "MinimonInterestSuccessText";
        public const string MinimonInterestFullText = "MinimonInterestFullText";
        public const string TravelUnlockText = "TravelUnlockText";
        public const string AlchemyUnlockText = "AlchemyUnlockText";
        public const string LanguageText = "LanguageText";
        public const string SoundText = "SoundText";
        public const string BGMText = "BGMText";
        public const string SFXText = "SFXText";
        public const string EffectsTitle = "EffectsTitle";
        public const string PopupText = "PopupText";
        public const string CameraShakeText = "CameraShakeText";
        public const string InvSlotNewTxt = "InvSlotNewTxt";
        public const string ImportantSlotNewTxt = "ImportantSlotNewTxt";
        public const string AlchemyButtonNewTxt = "AlchemyButtonNewTxt";
        public const string MonasteryButtonNewTxt = "MonasteryButtonNewTxt";
        public const string TravelButtonNewTxt = "TravelButtonNewTxt";
        public const string AlchemyCustomOrderButtonNewTxt = "AlchemyCustomOrderButtonNewTxt";
        public const string MonasteryCustomOrderButtonNewTxt = "MonasteryCustomOrderButtonNewTxt";
        public const string BeastiaryText = "BeastiaryText";
        public const string SlimeNameText = "SlimeNameText";
        public const string RabiSlimeNameText = "RabiSlimeNameText";
        public const string GoblinNameText = "GoblinNameText";
        public const string Skill5009NameText = "Skill5009NameText";
        public const string Skill5009DescText = "Skill5009DescText";
        public const string Skill5001NameText = "Skill5001NameText";
        public const string Skill5001DescText = "Skill5001DescText";
        public const string Skill5002NameText = "Skill5002NameText";
        public const string Skill5002DescText = "Skill5002DescText";
        public const string Skill5002StoryText = "Skill5002StoryText";
        public const string Skill5003NameText = "Skill5003NameText";
        public const string Skill5003DescText = "Skill5003DescText";
        public const string Skill5003StoryText = "Skill5003StoryText";
        public const string Skill5004NameText = "Skill5004NameText";
        public const string Skill5004DescText = "Skill5004DescText";
        public const string Skill5004StoryText = "Skill5004StoryText";
        public const string Skill7000NameText = "Skill7000NameText";
        public const string Skill7000DescText = "Skill7000DescText";
        public const string Skill7001NameText = "Skill7001NameText";
        public const string Skill7001DescText = "Skill7001DescText";
        public const string Skill7002NameText = "Skill7002NameText";
        public const string Skill7002DescText = "Skill7002DescText";
        public const string Skill6001NameText = "Skill6001NameText";
        public const string Skill6001DescText = "Skill6001DescText";
        public const string Skill6001StoryText = "Skill6001StoryText";
        public const string Skill6002NameText = "Skill6002NameText";
        public const string Skill6002DescText = "Skill6002DescText";
        public const string Skill5005NameText = "Skill5005NameText";
        public const string Skill5005DescText = "Skill5005DescText";
        public const string Skill5006NameText = "Skill5006NameText";
        public const string Skill5006DescText = "Skill5006DescText";
        public const string Skill5007NameText = "Skill5007NameText";
        public const string Skill5007DescText = "Skill5007DescText";
        public const string Skill5008NameText = "Skill5008NameText";
        public const string Skill5008DescText = "Skill5008DescText";
        public const string Skill5012NameText = "Skill5012NameText";
        public const string Skill5012DescText = "Skill5012DescText";
        public const string Skill5012StoryText = "Skill5012StoryText";
        public const string Skill5010NameText = "Skill5010NameText";
        public const string Skill5010DescText = "Skill5010DescText";
        public const string Skill5010StoryText = "Skill5010StoryText";
        public const string Skill5011NameText = "Skill5011NameText";
        public const string Skill5011DescText = "Skill5011DescText";
        public const string Skill5011StoryText = "Skill5011StoryText";
        public const string Skill5013NameText = "Skill5013NameText";
        public const string Skill5013DescText = "Skill5013DescText";
        public const string Skill5013StoryText = "Skill5013StoryText";
        public const string Skill5014NameText = "Skill5014NameText";
        public const string Skill5014DescText = "Skill5014DescText";
        public const string Skill5014StoryText = "Skill5014StoryText";
        public const string Skill5015NameText = "Skill5015NameText";
        public const string Skill5015DescText = "Skill5015DescText";
        public const string Skill5015StoryText = "Skill5015StoryText";
        public const string UniqueSkillText = "UniqueSkillText";
        public const string Equip30001NameText = "Equip30001NameText";
        public const string Minmon2001NameText = "Minmon2001NameText";
        public const string Minmon2002NameText = "Minmon2002NameText";
        public const string ItemRank2PermitNameText = "ItemRank2PermitNameText";
        public const string ItemRank2PermitDescText = "ItemRank2PermitDescText";
        public const string Monster2101NameText = "Monster2101NameText";
        public const string Monster2102NameText = "Monster2102NameText";
        public const string Monster2103NameText = "Monster2103NameText";
        public const string Monster2104NameText = "Monster2104NameText";
        public const string Monster2105NameText = "Monster2105NameText";
        public const string Monster2106NameText = "Monster2106NameText";
        public const string Monster2107NameText = "Monster2107NameText";
        public const string Monster2108NameText = "Monster2108NameText";
        public const string Monster2109NameText = "Monster2109NameText";
        public const string Monster2110NameText = "Monster2110NameText";
        public const string Monster2111NameText = "Monster2111NameText";
        public const string Monster2112NameText = "Monster2112NameText";
        public const string Monster2113NameText = "Monster2113NameText";
        public const string Monster2114NameText = "Monster2114NameText";
        public const string Monster2115NameText = "Monster2115NameText";
        public const string Monster2116NameText = "Monster2116NameText";
        public const string Monster2117NameText = "Monster2117NameText";
        public const string Monster2118NameText = "Monster2118NameText";
        public const string Monster2119NameText = "Monster2119NameText";
        public const string Monster2120NameText = "Monster2120NameText";
        public const string Monster2121NameText = "Monster2121NameText";
        public const string Monster2122NameText = "Monster2122NameText";
        public const string Monster2123NameText = "Monster2123NameText";
        public const string Monster2124NameText = "Monster2124NameText";
        public const string Monster2125NameText = "Monster2125NameText";
        public const string Monster2126NameText = "Monster2126NameText";
        public const string Monster2127NameText = "Monster2127NameText";
        public const string Monster2128NameText = "Monster2128NameText";
        public const string Monster2129NameText = "Monster2129NameText";
        public const string Monster2130NameText = "Monster2130NameText";
        public const string Monster2131NameText = "Monster2131NameText";
        public const string Monster2132NameText = "Monster2132NameText";
        public const string Monster2133NameText = "Monster2133NameText";
        public const string Monster2134NameText = "Monster2134NameText";
        public const string Monster2135NameText = "Monster2135NameText";
        public const string Monster2136NameText = "Monster2136NameText";
        public const string Monster2137NameText = "Monster2137NameText";
        public const string Monster2138NameText = "Monster2138NameText";
        public const string Monster2139NameText = "Monster2139NameText";
        public const string Monster2140NameText = "Monster2140NameText";
        public const string Monster2141NameText = "Monster2141NameText";
        public const string Monster2142NameText = "Monster2142NameText";
        public const string Monster2143NameText = "Monster2143NameText";
        public const string Monster2144NameText = "Monster2144NameText";
        public const string Monster2145NameText = "Monster2145NameText";
        public const string Monster2146NameText = "Monster2146NameText";
        public const string Monster2147NameText = "Monster2147NameText";
        public const string Monster2148NameText = "Monster2148NameText";
        public const string Monster2149NameText = "Monster2149NameText";
        public const string Monster2150NameText = "Monster2150NameText";
        public const string Monster2151NameText = "Monster2151NameText";
        public const string Monster2152NameText = "Monster2152NameText";
        public const string Monster2153NameText = "Monster2153NameText";
        public const string Monster2154NameText = "Monster2154NameText";
        public const string Monster2155NameText = "Monster2155NameText";
        public const string Monster2156NameText = "Monster2156NameText";
        public const string Monster2157NameText = "Monster2157NameText";
        public const string Monster2158NameText = "Monster2158NameText";
        public const string Monster2159NameText = "Monster2159NameText";
        public const string Monster2160NameText = "Monster2160NameText";
        public const string Monster2161NameText = "Monster2161NameText";
        public const string Monster2162NameText = "Monster2162NameText";
        public const string Monster2163NameText = "Monster2163NameText";
        public const string Monster2164NameText = "Monster2164NameText";
        public const string Monster2165NameText = "Monster2165NameText";
        public const string Monster2166NameText = "Monster2166NameText";
        public const string Monster2167NameText = "Monster2167NameText";
        public const string Monster2168NameText = "Monster2168NameText";
        public const string Monster2169NameText = "Monster2169NameText";
        public const string Monster2170NameText = "Monster2170NameText";
        public const string Monster2171NameText = "Monster2171NameText";
        public const string Monster2172NameText = "Monster2172NameText";
        public const string Monster2173NameText = "Monster2173NameText";
        public const string Monster2174NameText = "Monster2174NameText";
        public const string Monster2175NameText = "Monster2175NameText";
        public const string Monster2176NameText = "Monster2176NameText";
        public const string Monster2177NameText = "Monster2177NameText";
        public const string Monster2178NameText = "Monster2178NameText";
        public const string Monster2179NameText = "Monster2179NameText";
        public const string Monster2180NameText = "Monster2180NameText";
        public const string Monster2181NameText = "Monster2181NameText";
        public const string Monster2182NameText = "Monster2182NameText";
        public const string Monster2183NameText = "Monster2183NameText";
        public const string Monster2184NameText = "Monster2184NameText";
        public const string Monster2185NameText = "Monster2185NameText";
        public const string Monster2186NameText = "Monster2186NameText";
        public const string Monster2187NameText = "Monster2187NameText";
        public const string Monster2188NameText = "Monster2188NameText";
        public const string Monster2189NameText = "Monster2189NameText";
        public const string Monster2190NameText = "Monster2190NameText";
        public const string Monster2191NameText = "Monster2191NameText";
        public const string Monster2192NameText = "Monster2192NameText";
        public const string Monster2193NameText = "Monster2193NameText";
        public const string Monster2194NameText = "Monster2194NameText";
        public const string Monster2195NameText = "Monster2195NameText";
        public const string Monster2196NameText = "Monster2196NameText";
        public const string Monster2197NameText = "Monster2197NameText";
        public const string Monster2198NameText = "Monster2198NameText";
        public const string Monster2199NameText = "Monster2199NameText";
        public const string Monster2200NameText = "Monster2200NameText";
        public const string Monster2201NameText = "Monster2201NameText";
        public const string Monster2202NameText = "Monster2202NameText";
        public const string Monster2203NameText = "Monster2203NameText";
        public const string Monster2204NameText = "Monster2204NameText";
        public const string Monster2205NameText = "Monster2205NameText";
        public const string Monster2206NameText = "Monster2206NameText";
        public const string Monster2207NameText = "Monster2207NameText";
        public const string Monster2208NameText = "Monster2208NameText";
        public const string Monster2209NameText = "Monster2209NameText";
        public const string Monster2210NameText = "Monster2210NameText";
        public const string Monster2211NameText = "Monster2211NameText";
        public const string Monster2212NameText = "Monster2212NameText";
        public const string Monster2213NameText = "Monster2213NameText";
        public const string Monster2214NameText = "Monster2214NameText";
        public const string Monster2215NameText = "Monster2215NameText";
        public const string Monster2216NameText = "Monster2216NameText";
        public const string Monster2217NameText = "Monster2217NameText";
        public const string Monster2218NameText = "Monster2218NameText";
        public const string Monster2219NameText = "Monster2219NameText";
        public const string Monster2220NameText = "Monster2220NameText";
        public const string Monster2221NameText = "Monster2221NameText";
        public const string Monster2222NameText = "Monster2222NameText";
        public const string Monster2223NameText = "Monster2223NameText";
        public const string Monster2224NameText = "Monster2224NameText";
        public const string Monster2225NameText = "Monster2225NameText";
        public const string Monster2226NameText = "Monster2226NameText";
        public const string Monster2227NameText = "Monster2227NameText";
        public const string Monster2228NameText = "Monster2228NameText";
        public const string Monster2229NameText = "Monster2229NameText";
        public const string Monster2230NameText = "Monster2230NameText";
        public const string Monster2231NameText = "Monster2231NameText";
        public const string Monster2232NameText = "Monster2232NameText";
        public const string Monster2233NameText = "Monster2233NameText";
        public const string Monster2234NameText = "Monster2234NameText";
        public const string Monster2235NameText = "Monster2235NameText";
        public const string Monster2236NameText = "Monster2236NameText";
        public const string Monster2237NameText = "Monster2237NameText";
        public const string Monster2238NameText = "Monster2238NameText";
        public const string Monster2239NameText = "Monster2239NameText";
        public const string Monster2240NameText = "Monster2240NameText";
        public const string Monster2241NameText = "Monster2241NameText";
        public const string Monster2242NameText = "Monster2242NameText";
        public const string Monster2243NameText = "Monster2243NameText";
        public const string Monster2244NameText = "Monster2244NameText";
        public const string Monster2245NameText = "Monster2245NameText";
        public const string Monster2246NameText = "Monster2246NameText";
        public const string Monster2247NameText = "Monster2247NameText";
        public const string Monster2248NameText = "Monster2248NameText";
        public const string Monster2249NameText = "Monster2249NameText";
        public const string Monster2250NameText = "Monster2250NameText";
        public const string Monster2251NameText = "Monster2251NameText";
        public const string Monster2252NameText = "Monster2252NameText";
        public const string Monster2253NameText = "Monster2253NameText";
        public const string Monster2254NameText = "Monster2254NameText";
        public const string Monster2255NameText = "Monster2255NameText";
        public const string Monster2256NameText = "Monster2256NameText";
        public const string Monster2257NameText = "Monster2257NameText";
        public const string Monster2258NameText = "Monster2258NameText";
        public const string Monster2259NameText = "Monster2259NameText";
        public const string Monster2260NameText = "Monster2260NameText";
        public const string Monster2261NameText = "Monster2261NameText";
        public const string Monster2262NameText = "Monster2262NameText";
        public const string Monster2263NameText = "Monster2263NameText";
        public const string Monster2264NameText = "Monster2264NameText";
        public const string Monster2265NameText = "Monster2265NameText";
        public const string Monster2266NameText = "Monster2266NameText";
        public const string Monster2267NameText = "Monster2267NameText";
        public const string Monster2268NameText = "Monster2268NameText";
        public const string Monster2269NameText = "Monster2269NameText";
        public const string Monster2270NameText = "Monster2270NameText";
        public const string Monster2271NameText = "Monster2271NameText";
        public const string Monster2272NameText = "Monster2272NameText";
        public const string Monster2273NameText = "Monster2273NameText";
        public const string Monster2274NameText = "Monster2274NameText";
        public const string Monster2275NameText = "Monster2275NameText";
        public const string Monster2276NameText = "Monster2276NameText";
        public const string Monster2277NameText = "Monster2277NameText";
        public const string Monster2278NameText = "Monster2278NameText";
        public const string Monster2279NameText = "Monster2279NameText";
        public const string Monster2280NameText = "Monster2280NameText";
        public const string Monster2281NameText = "Monster2281NameText";
        public const string Monster2282NameText = "Monster2282NameText";
        public const string Monster2283NameText = "Monster2283NameText";
        public const string Monster2284NameText = "Monster2284NameText";
        public const string Monster2285NameText = "Monster2285NameText";
        public const string Monster2286NameText = "Monster2286NameText";
        public const string Monster2287NameText = "Monster2287NameText";
        public const string Monster2288NameText = "Monster2288NameText";
        public const string Monster2289NameText = "Monster2289NameText";
        public const string Monster2290NameText = "Monster2290NameText";
        public const string Monster2291NameText = "Monster2291NameText";
        public const string Monster2292NameText = "Monster2292NameText";
        public const string Monster2293NameText = "Monster2293NameText";
        public const string Monster2294NameText = "Monster2294NameText";
        public const string Monster2295NameText = "Monster2295NameText";
        public const string Monster2296NameText = "Monster2296NameText";
        public const string Monster2297NameText = "Monster2297NameText";
        public const string Monster2298NameText = "Monster2298NameText";
        public const string Monster2299NameText = "Monster2299NameText";
        public const string Monster2300NameText = "Monster2300NameText";
        public const string Monster2301NameText = "Monster2301NameText";
        public const string Monster2302NameText = "Monster2302NameText";
        public const string Monster2303NameText = "Monster2303NameText";
        public const string Monster2304NameText = "Monster2304NameText";
        public const string Monster2305NameText = "Monster2305NameText";
        public const string Monster2306NameText = "Monster2306NameText";
        public const string Monster2307NameText = "Monster2307NameText";
        public const string Monster2308NameText = "Monster2308NameText";
        public const string Monster2309NameText = "Monster2309NameText";
        public const string Monster2310NameText = "Monster2310NameText";
        public const string Monster2311NameText = "Monster2311NameText";
        public const string Monster2312NameText = "Monster2312NameText";
        public const string Monster2313NameText = "Monster2313NameText";
        public const string Monster2314NameText = "Monster2314NameText";
        public const string Monster2315NameText = "Monster2315NameText";
        public const string Monster2316NameText = "Monster2316NameText";
        public const string Monster2317NameText = "Monster2317NameText";
        public const string Monster2318NameText = "Monster2318NameText";
        public const string Monster2319NameText = "Monster2319NameText";
        public const string Monster2320NameText = "Monster2320NameText";
        public const string Monster2321NameText = "Monster2321NameText";
        public const string Monster2322NameText = "Monster2322NameText";
        public const string Monster2323NameText = "Monster2323NameText";
        public const string Monster2324NameText = "Monster2324NameText";
        public const string Monster2325NameText = "Monster2325NameText";
        public const string Monster2326NameText = "Monster2326NameText";
        public const string Monster2327NameText = "Monster2327NameText";
        public const string Monster2328NameText = "Monster2328NameText";
        public const string Monster2329NameText = "Monster2329NameText";
        public const string Monster2330NameText = "Monster2330NameText";
        public const string Monster2331NameText = "Monster2331NameText";
        public const string Monster2332NameText = "Monster2332NameText";
        public const string Monster2333NameText = "Monster2333NameText";
        public const string Monster2334NameText = "Monster2334NameText";
        public const string Monster2335NameText = "Monster2335NameText";
        public const string Monster2336NameText = "Monster2336NameText";
        public const string Monster2337NameText = "Monster2337NameText";
        public const string Monster2338NameText = "Monster2338NameText";
        public const string Monster2339NameText = "Monster2339NameText";
        public const string Monster2340NameText = "Monster2340NameText";
        public const string Monster2341NameText = "Monster2341NameText";
        public const string Monster2342NameText = "Monster2342NameText";
        public const string Monster2343NameText = "Monster2343NameText";
        public const string Monster2344NameText = "Monster2344NameText";
        public const string Monster2345NameText = "Monster2345NameText";
        public const string Monster2346NameText = "Monster2346NameText";
        public const string Monster2347NameText = "Monster2347NameText";
        public const string Monster2348NameText = "Monster2348NameText";
        public const string Monster2349NameText = "Monster2349NameText";
        public const string Monster2350NameText = "Monster2350NameText";
        public const string Monster2351NameText = "Monster2351NameText";
        public const string Monster2352NameText = "Monster2352NameText";
        public const string Monster2353NameText = "Monster2353NameText";
        public const string Monster2354NameText = "Monster2354NameText";
        public const string Monster2355NameText = "Monster2355NameText";
        public const string Monster2356NameText = "Monster2356NameText";
        public const string Monster2357NameText = "Monster2357NameText";
        public const string Monster2358NameText = "Monster2358NameText";
        public const string Monster2359NameText = "Monster2359NameText";
        public const string Monster2360NameText = "Monster2360NameText";
        public const string Monster2361NameText = "Monster2361NameText";
        public const string Monster2362NameText = "Monster2362NameText";
        public const string Monster2363NameText = "Monster2363NameText";
        public const string Monster2364NameText = "Monster2364NameText";
        public const string Monster2365NameText = "Monster2365NameText";
        public const string Monster2366NameText = "Monster2366NameText";
        public const string Monster2367NameText = "Monster2367NameText";
        public const string Monster2368NameText = "Monster2368NameText";
        public const string Monster2369NameText = "Monster2369NameText";
        public const string Monster2370NameText = "Monster2370NameText";
        public const string Monster2371NameText = "Monster2371NameText";
        public const string Monster2372NameText = "Monster2372NameText";
        public const string Monster2373NameText = "Monster2373NameText";
        public const string Monster2374NameText = "Monster2374NameText";
        public const string Monster2375NameText = "Monster2375NameText";
        public const string Monster2376NameText = "Monster2376NameText";
        public const string Monster2377NameText = "Monster2377NameText";
        public const string Monster2378NameText = "Monster2378NameText";
        public const string Monster2379NameText = "Monster2379NameText";
        public const string Monster2380NameText = "Monster2380NameText";
        public const string Monster2381NameText = "Monster2381NameText";
        public const string Monster2382NameText = "Monster2382NameText";
        public const string Monster2383NameText = "Monster2383NameText";
        public const string Monster2384NameText = "Monster2384NameText";
        public const string Monster2385NameText = "Monster2385NameText";
        public const string Monster2386NameText = "Monster2386NameText";
        public const string Monster2387NameText = "Monster2387NameText";
        public const string Monster2388NameText = "Monster2388NameText";
        public const string Monster2389NameText = "Monster2389NameText";
        public const string Monster2390NameText = "Monster2390NameText";
        public const string Monster2391NameText = "Monster2391NameText";
        public const string Monster2392NameText = "Monster2392NameText";
        public const string Monster2393NameText = "Monster2393NameText";
        public const string Monster2394NameText = "Monster2394NameText";
        public const string Monster2395NameText = "Monster2395NameText";
        public const string Monster2396NameText = "Monster2396NameText";
        public const string Monster2397NameText = "Monster2397NameText";
        public const string Monster2398NameText = "Monster2398NameText";
        public const string Monster2399NameText = "Monster2399NameText";
        public const string Monster2400NameText = "Monster2400NameText";
        public const string Monster2401NameText = "Monster2401NameText";
        public const string Monster2402NameText = "Monster2402NameText";
        public const string Monster2403NameText = "Monster2403NameText";
        public const string Monster2404NameText = "Monster2404NameText";
        public const string Monster2405NameText = "Monster2405NameText";
        public const string Monster2406NameText = "Monster2406NameText";
        public const string Monster2407NameText = "Monster2407NameText";
        public const string Monster2408NameText = "Monster2408NameText";
        public const string Monster2409NameText = "Monster2409NameText";
        public const string Monster2410NameText = "Monster2410NameText";
        public const string Monster2411NameText = "Monster2411NameText";
        public const string Monster2412NameText = "Monster2412NameText";
        public const string Monster2413NameText = "Monster2413NameText";
        public const string Monster2414NameText = "Monster2414NameText";
        public const string Monster2415NameText = "Monster2415NameText";
        public const string Monster2416NameText = "Monster2416NameText";
        public const string Monster2417NameText = "Monster2417NameText";
        public const string Monster2418NameText = "Monster2418NameText";
        public const string Monster2419NameText = "Monster2419NameText";
        public const string Monster2420NameText = "Monster2420NameText";
        public const string Monster2421NameText = "Monster2421NameText";
        public const string Monster2422NameText = "Monster2422NameText";
        public const string Monster2423NameText = "Monster2423NameText";
        public const string Monster2424NameText = "Monster2424NameText";
        public const string Monster2425NameText = "Monster2425NameText";
        public const string Monster2426NameText = "Monster2426NameText";
        public const string Monster2427NameText = "Monster2427NameText";
        public const string Monster2428NameText = "Monster2428NameText";
        public const string Monster2429NameText = "Monster2429NameText";
        public const string Monster2430NameText = "Monster2430NameText";
        public const string Monster2431NameText = "Monster2431NameText";
        public const string Monster2432NameText = "Monster2432NameText";
        public const string Monster2433NameText = "Monster2433NameText";
        public const string Monster2434NameText = "Monster2434NameText";
        public const string Monster2435NameText = "Monster2435NameText";
        public const string Monster2436NameText = "Monster2436NameText";
        public const string Monster2437NameText = "Monster2437NameText";
        public const string Monster2438NameText = "Monster2438NameText";
        public const string Monster2439NameText = "Monster2439NameText";
        public const string Monster2440NameText = "Monster2440NameText";
        public const string Monster2441NameText = "Monster2441NameText";
        public const string Monster2442NameText = "Monster2442NameText";
        public const string Monster2443NameText = "Monster2443NameText";
        public const string Monster2444NameText = "Monster2444NameText";
        public const string Monster2445NameText = "Monster2445NameText";
        public const string Monster2446NameText = "Monster2446NameText";
        public const string Monster2447NameText = "Monster2447NameText";
        public const string Monster2448NameText = "Monster2448NameText";
        public const string Monster2449NameText = "Monster2449NameText";
        public const string Monster2450NameText = "Monster2450NameText";
        public const string Monster2451NameText = "Monster2451NameText";
        public const string Monster2452NameText = "Monster2452NameText";
        public const string Monster2453NameText = "Monster2453NameText";
        public const string Monster2454NameText = "Monster2454NameText";
        public const string Monster2455NameText = "Monster2455NameText";
        public const string Monster2456NameText = "Monster2456NameText";
        public const string Monster2457NameText = "Monster2457NameText";
        public const string Monster2458NameText = "Monster2458NameText";
        public const string Monster2459NameText = "Monster2459NameText";
        public const string Monster2460NameText = "Monster2460NameText";
        public const string Monster2461NameText = "Monster2461NameText";
        public const string Monster2462NameText = "Monster2462NameText";
        public const string Monster2463NameText = "Monster2463NameText";
        public const string Monster2464NameText = "Monster2464NameText";
        public const string Monster2465NameText = "Monster2465NameText";
        public const string Monster2466NameText = "Monster2466NameText";
        public const string Monster2467NameText = "Monster2467NameText";
        public const string Monster2468NameText = "Monster2468NameText";
        public const string Monster2469NameText = "Monster2469NameText";
        public const string Monster2470NameText = "Monster2470NameText";
        public const string Monster2471NameText = "Monster2471NameText";
        public const string Monster2472NameText = "Monster2472NameText";
        public const string Monster2473NameText = "Monster2473NameText";
        public const string Monster2474NameText = "Monster2474NameText";
        public const string Monster2475NameText = "Monster2475NameText";
        public const string Monster2476NameText = "Monster2476NameText";
        public const string Monster2477NameText = "Monster2477NameText";
        public const string Monster2478NameText = "Monster2478NameText";
        public const string Monster2479NameText = "Monster2479NameText";
        public const string Monster2480NameText = "Monster2480NameText";
        public const string Monster2481NameText = "Monster2481NameText";
        public const string Monster2482NameText = "Monster2482NameText";
        public const string Monster2483NameText = "Monster2483NameText";
        public const string Monster2484NameText = "Monster2484NameText";
        public const string Monster2485NameText = "Monster2485NameText";
        public const string Monster2486NameText = "Monster2486NameText";
        public const string Monster2487NameText = "Monster2487NameText";
        public const string Monster2488NameText = "Monster2488NameText";
        public const string Monster2489NameText = "Monster2489NameText";
        public const string Monster2490NameText = "Monster2490NameText";
        public const string Monster2491NameText = "Monster2491NameText";
        public const string Monster2492NameText = "Monster2492NameText";
        public const string Monster2493NameText = "Monster2493NameText";
        public const string Monster2494NameText = "Monster2494NameText";
        public const string Monster2495NameText = "Monster2495NameText";
        public const string Monster2496NameText = "Monster2496NameText";
        public const string Monster2497NameText = "Monster2497NameText";
        public const string Monster2498NameText = "Monster2498NameText";
        public const string Monster2499NameText = "Monster2499NameText";
        public const string Monster2500NameText = "Monster2500NameText";
        public const string Monster2501NameText = "Monster2501NameText";
        public const string Monster2502NameText = "Monster2502NameText";
        public const string Monster2503NameText = "Monster2503NameText";
        public const string Monster2504NameText = "Monster2504NameText";
        public const string Monster2505NameText = "Monster2505NameText";
        public const string Monster2506NameText = "Monster2506NameText";
        public const string Monster2507NameText = "Monster2507NameText";
        public const string Monster2508NameText = "Monster2508NameText";
        public const string Monster2509NameText = "Monster2509NameText";
        public const string Monster2510NameText = "Monster2510NameText";
        public const string Monster2511NameText = "Monster2511NameText";
        public const string Monster2512NameText = "Monster2512NameText";
        public const string Monster2513NameText = "Monster2513NameText";
        public const string Monster2514NameText = "Monster2514NameText";
        public const string Monster2515NameText = "Monster2515NameText";
        public const string Monster2516NameText = "Monster2516NameText";
        public const string Monster2517NameText = "Monster2517NameText";
        public const string Monster2518NameText = "Monster2518NameText";
        public const string Monster2519NameText = "Monster2519NameText";
        public const string Monster2520NameText = "Monster2520NameText";
        public const string Monster2521NameText = "Monster2521NameText";
        public const string Monster2522NameText = "Monster2522NameText";
        public const string Monster2523NameText = "Monster2523NameText";
        public const string Monster2524NameText = "Monster2524NameText";
        public const string Monster2525NameText = "Monster2525NameText";
        public const string Monster2526NameText = "Monster2526NameText";
        public const string Monster2527NameText = "Monster2527NameText";
        public const string Monster2528NameText = "Monster2528NameText";
        public const string Monster2529NameText = "Monster2529NameText";
        public const string Monster2530NameText = "Monster2530NameText";
        public const string Monster2531NameText = "Monster2531NameText";
        public const string Monster2532NameText = "Monster2532NameText";
        public const string Monster2533NameText = "Monster2533NameText";
        public const string Monster2534NameText = "Monster2534NameText";
        public const string Monster2535NameText = "Monster2535NameText";
        public const string Monster2536NameText = "Monster2536NameText";
        public const string Monster2537NameText = "Monster2537NameText";
        public const string Monster2538NameText = "Monster2538NameText";
        public const string Monster2539NameText = "Monster2539NameText";
        public const string Monster2540NameText = "Monster2540NameText";
        public const string Monster2541NameText = "Monster2541NameText";
        public const string Monster2542NameText = "Monster2542NameText";
        public const string Monster2543NameText = "Monster2543NameText";
        public const string Monster2544NameText = "Monster2544NameText";
        public const string Monster2545NameText = "Monster2545NameText";
        public const string Monster2546NameText = "Monster2546NameText";
        public const string Monster2547NameText = "Monster2547NameText";
        public const string Monster2548NameText = "Monster2548NameText";
        public const string Monster2549NameText = "Monster2549NameText";
        public const string Monster2550NameText = "Monster2550NameText";
        public const string Monster2551NameText = "Monster2551NameText";
        public const string Monster2552NameText = "Monster2552NameText";
        public const string Monster2553NameText = "Monster2553NameText";
        public const string Monster2554NameText = "Monster2554NameText";
        public const string Monster2555NameText = "Monster2555NameText";
        public const string Monster2556NameText = "Monster2556NameText";
        public const string Monster2557NameText = "Monster2557NameText";
        public const string Monster2558NameText = "Monster2558NameText";
        public const string Monster2559NameText = "Monster2559NameText";
        public const string Monster2560NameText = "Monster2560NameText";
        public const string Monster2561NameText = "Monster2561NameText";
        public const string Monster2562NameText = "Monster2562NameText";
        public const string Monster2563NameText = "Monster2563NameText";
        public const string Monster2564NameText = "Monster2564NameText";
        public const string Monster2565NameText = "Monster2565NameText";
        public const string Monster2566NameText = "Monster2566NameText";
        public const string Monster2567NameText = "Monster2567NameText";
        public const string Monster2568NameText = "Monster2568NameText";
        public const string Monster2569NameText = "Monster2569NameText";
        public const string Monster2570NameText = "Monster2570NameText";
        public const string Monster2571NameText = "Monster2571NameText";
        public const string Monster2572NameText = "Monster2572NameText";
        public const string Monster2573NameText = "Monster2573NameText";
        public const string Monster2574NameText = "Monster2574NameText";
        public const string Monster2575NameText = "Monster2575NameText";
        public const string Monster2576NameText = "Monster2576NameText";
        public const string Monster2577NameText = "Monster2577NameText";
        public const string Monster2578NameText = "Monster2578NameText";
        public const string Monster2579NameText = "Monster2579NameText";
        public const string Monster2580NameText = "Monster2580NameText";
        public const string Monster2581NameText = "Monster2581NameText";
        public const string Monster2582NameText = "Monster2582NameText";
        public const string Monster2583NameText = "Monster2583NameText";
        public const string Monster2584NameText = "Monster2584NameText";
        public const string Monster2585NameText = "Monster2585NameText";
        public const string Monster2586NameText = "Monster2586NameText";
        public const string Monster2587NameText = "Monster2587NameText";
        public const string Monster2588NameText = "Monster2588NameText";
        public const string Monster2589NameText = "Monster2589NameText";
        public const string Monster2590NameText = "Monster2590NameText";
        public const string Monster2591NameText = "Monster2591NameText";
        public const string Monster2592NameText = "Monster2592NameText";
        public const string Monster2593NameText = "Monster2593NameText";
        public const string Monster2594NameText = "Monster2594NameText";
        public const string Monster2595NameText = "Monster2595NameText";
        public const string Monster2596NameText = "Monster2596NameText";
        public const string Monster2597NameText = "Monster2597NameText";
        public const string Monster2598NameText = "Monster2598NameText";
        public const string Monster2599NameText = "Monster2599NameText";
        public const string Monster2600NameText = "Monster2600NameText";
        public const string BreezingWindForestDepth1Text = "BreezingWindForestDepth1Text";
        public const string BreezingWindForestDepth2Text = "BreezingWindForestDepth2Text";
        public const string BreezingWindForestDepth3Text = "BreezingWindForestDepth3Text";
        public const string BreezingWindForestDepth4Text = "BreezingWindForestDepth4Text";
        public const string BreezingWindForestDepth5Text = "BreezingWindForestDepth5Text";
        public const string BreezingWindForestDepth6Text = "BreezingWindForestDepth6Text";
        public const string BreezingWindForestDepth7Text = "BreezingWindForestDepth7Text";
        public const string BreezingWindForestDepth8Text = "BreezingWindForestDepth8Text";
        public const string BreezingWindForestDepth9Text = "BreezingWindForestDepth9Text";
        public const string BreezingWindForestDepth10Text = "BreezingWindForestDepth10Text";
        public const string WhisperingCaveDepth1Text = "WhisperingCaveDepth1Text";
        public const string WhisperingCaveDepth2Text = "WhisperingCaveDepth2Text";
        public const string WhisperingCaveDepth3Text = "WhisperingCaveDepth3Text";
        public const string WhisperingCaveDepth4Text = "WhisperingCaveDepth4Text";
        public const string WhisperingCaveDepth5Text = "WhisperingCaveDepth5Text";
        public const string WhisperingCaveDepth6Text = "WhisperingCaveDepth6Text";
        public const string WhisperingCaveDepth7Text = "WhisperingCaveDepth7Text";
        public const string WhisperingCaveDepth8Text = "WhisperingCaveDepth8Text";
        public const string WhisperingCaveDepth9Text = "WhisperingCaveDepth9Text";
        public const string WhisperingCaveDepth10Text = "WhisperingCaveDepth10Text";
        public const string RuinedSanctumDepth1Text = "RuinedSanctumDepth1Text";
        public const string RuinedSanctumDepth2Text = "RuinedSanctumDepth2Text";
        public const string RuinedSanctumDepth3Text = "RuinedSanctumDepth3Text";
        public const string RuinedSanctumDepth4Text = "RuinedSanctumDepth4Text";
        public const string RuinedSanctumDepth5Text = "RuinedSanctumDepth5Text";
        public const string RuinedSanctumDepth6Text = "RuinedSanctumDepth6Text";
        public const string RuinedSanctumDepth7Text = "RuinedSanctumDepth7Text";
        public const string RuinedSanctumDepth8Text = "RuinedSanctumDepth8Text";
        public const string RuinedSanctumDepth9Text = "RuinedSanctumDepth9Text";
        public const string RuinedSanctumDepth10Text = "RuinedSanctumDepth10Text";
        public const string Equip30002NameText = "Equip30002NameText";
        public const string Equip30003NameText = "Equip30003NameText";
        public const string Equip30004NameText = "Equip30004NameText";
        public const string Equip30005NameText = "Equip30005NameText";
        public const string Equip30006NameText = "Equip30006NameText";
        public const string Equip30007NameText = "Equip30007NameText";
        public const string Equip30008NameText = "Equip30008NameText";
        public const string Equip30009NameText = "Equip30009NameText";
        public const string Equip30010NameText = "Equip30010NameText";
        public const string Equip30011NameText = "Equip30011NameText";
        public const string Equip30012NameText = "Equip30012NameText";
        public const string Equip30013NameText = "Equip30013NameText";
        public const string Equip30014NameText = "Equip30014NameText";
        public const string Equip30015NameText = "Equip30015NameText";
        public const string Equip30016NameText = "Equip30016NameText";
        public const string Equip30017NameText = "Equip30017NameText";
        public const string Equip30018NameText = "Equip30018NameText";
        public const string Equip30019NameText = "Equip30019NameText";
        public const string Equip30020NameText = "Equip30020NameText";
        public const string Equip30021NameText = "Equip30021NameText";
        public const string Equip30022NameText = "Equip30022NameText";
        public const string Equip30023NameText = "Equip30023NameText";
        public const string Equip30024NameText = "Equip30024NameText";
        public const string Equip30025NameText = "Equip30025NameText";
        public const string Equip30026NameText = "Equip30026NameText";
        public const string Equip30027NameText = "Equip30027NameText";
        public const string Equip30028NameText = "Equip30028NameText";
        public const string Equip30029NameText = "Equip30029NameText";
        public const string Equip30030NameText = "Equip30030NameText";
        public const string Equip30031NameText = "Equip30031NameText";
        public const string Equip30032NameText = "Equip30032NameText";
        public const string Equip30033NameText = "Equip30033NameText";
        public const string Equip30034NameText = "Equip30034NameText";
        public const string Equip30035NameText = "Equip30035NameText";
        public const string Equip30036NameText = "Equip30036NameText";
        public const string Equip30037NameText = "Equip30037NameText";
        public const string Equip30038NameText = "Equip30038NameText";
        public const string Equip30039NameText = "Equip30039NameText";
        public const string Equip30040NameText = "Equip30040NameText";
        public const string Equip30041NameText = "Equip30041NameText";
        public const string Equip30042NameText = "Equip30042NameText";
        public const string Equip30043NameText = "Equip30043NameText";
        public const string Equip30044NameText = "Equip30044NameText";
        public const string Equip30045NameText = "Equip30045NameText";
        public const string Equip30046NameText = "Equip30046NameText";
        public const string Equip30047NameText = "Equip30047NameText";
        public const string Equip30048NameText = "Equip30048NameText";
        public const string Equip30049NameText = "Equip30049NameText";
        public const string Equip30050NameText = "Equip30050NameText";
        public const string Equip30051NameText = "Equip30051NameText";
        public const string Equip30052NameText = "Equip30052NameText";
        public const string Equip30053NameText = "Equip30053NameText";
        public const string Equip30054NameText = "Equip30054NameText";
        public const string Equip30055NameText = "Equip30055NameText";
        public const string Equip30056NameText = "Equip30056NameText";
        public const string Equip30057NameText = "Equip30057NameText";
        public const string Equip30058NameText = "Equip30058NameText";
        public const string Equip30059NameText = "Equip30059NameText";
        public const string Equip30060NameText = "Equip30060NameText";
        public const string Skill5100NameText = "Skill5100NameText";
        public const string Skill5100DescText = "Skill5100DescText";
        public const string Skill5101NameText = "Skill5101NameText";
        public const string Skill5101DescText = "Skill5101DescText";
        public const string Skill5102NameText = "Skill5102NameText";
        public const string Skill5102DescText = "Skill5102DescText";
        public const string Skill5103NameText = "Skill5103NameText";
        public const string Skill5103DescText = "Skill5103DescText";
        public const string Skill5104NameText = "Skill5104NameText";
        public const string Skill5104DescText = "Skill5104DescText";
        public const string Skill5105NameText = "Skill5105NameText";
        public const string Skill5105DescText = "Skill5105DescText";
        public const string Skill5106NameText = "Skill5106NameText";
        public const string Skill5106DescText = "Skill5106DescText";
        public const string Skill5107NameText = "Skill5107NameText";
        public const string Skill5107DescText = "Skill5107DescText";
        public const string Skill5108NameText = "Skill5108NameText";
        public const string Skill5108DescText = "Skill5108DescText";
        public const string Skill5109NameText = "Skill5109NameText";
        public const string Skill5109DescText = "Skill5109DescText";
        public const string Skill5110NameText = "Skill5110NameText";
        public const string Skill5110DescText = "Skill5110DescText";
        public const string Skill5111NameText = "Skill5111NameText";
        public const string Skill5111DescText = "Skill5111DescText";
        public const string Skill5112NameText = "Skill5112NameText";
        public const string Skill5112DescText = "Skill5112DescText";
        public const string Skill5113NameText = "Skill5113NameText";
        public const string Skill5113DescText = "Skill5113DescText";
        public const string Skill5114NameText = "Skill5114NameText";
        public const string Skill5114DescText = "Skill5114DescText";
        public const string Skill5115NameText = "Skill5115NameText";
        public const string Skill5115DescText = "Skill5115DescText";
        public const string Skill5116NameText = "Skill5116NameText";
        public const string Skill5116DescText = "Skill5116DescText";
        public const string Skill5117NameText = "Skill5117NameText";
        public const string Skill5117DescText = "Skill5117DescText";
        public const string Skill5118NameText = "Skill5118NameText";
        public const string Skill5118DescText = "Skill5118DescText";
        public const string Skill5119NameText = "Skill5119NameText";
        public const string Skill5119DescText = "Skill5119DescText";
        public const string Skill5120NameText = "Skill5120NameText";
        public const string Skill5120DescText = "Skill5120DescText";
        public const string Skill5121NameText = "Skill5121NameText";
        public const string Skill5121DescText = "Skill5121DescText";
        public const string Skill5122NameText = "Skill5122NameText";
        public const string Skill5122DescText = "Skill5122DescText";
        public const string Skill5123NameText = "Skill5123NameText";
        public const string Skill5123DescText = "Skill5123DescText";
        public const string Skill5124NameText = "Skill5124NameText";
        public const string Skill5124DescText = "Skill5124DescText";
        public const string Skill5125NameText = "Skill5125NameText";
        public const string Skill5125DescText = "Skill5125DescText";
        public const string Skill5126NameText = "Skill5126NameText";
        public const string Skill5126DescText = "Skill5126DescText";
        public const string Skill5127NameText = "Skill5127NameText";
        public const string Skill5127DescText = "Skill5127DescText";
        public const string Skill5128NameText = "Skill5128NameText";
        public const string Skill5128DescText = "Skill5128DescText";
        public const string Skill5129NameText = "Skill5129NameText";
        public const string Skill5129DescText = "Skill5129DescText";
        public const string Skill5130NameText = "Skill5130NameText";
        public const string Skill5130DescText = "Skill5130DescText";
        public const string Skill5131NameText = "Skill5131NameText";
        public const string Skill5131DescText = "Skill5131DescText";
        public const string Skill5132NameText = "Skill5132NameText";
        public const string Skill5132DescText = "Skill5132DescText";
        public const string Skill5133NameText = "Skill5133NameText";
        public const string Skill5133DescText = "Skill5133DescText";
        public const string Skill5134NameText = "Skill5134NameText";
        public const string Skill5134DescText = "Skill5134DescText";
        public const string Skill5135NameText = "Skill5135NameText";
        public const string Skill5135DescText = "Skill5135DescText";
        public const string Skill5136NameText = "Skill5136NameText";
        public const string Skill5136DescText = "Skill5136DescText";
        public const string Skill5137NameText = "Skill5137NameText";
        public const string Skill5137DescText = "Skill5137DescText";
        public const string Skill5138NameText = "Skill5138NameText";
        public const string Skill5138DescText = "Skill5138DescText";
        public const string Skill5139NameText = "Skill5139NameText";
        public const string Skill5139DescText = "Skill5139DescText";
        public const string Skill5140NameText = "Skill5140NameText";
        public const string Skill5140DescText = "Skill5140DescText";
        public const string Skill5141NameText = "Skill5141NameText";
        public const string Skill5141DescText = "Skill5141DescText";
        public const string Skill5142NameText = "Skill5142NameText";
        public const string Skill5142DescText = "Skill5142DescText";
        public const string Skill5143NameText = "Skill5143NameText";
        public const string Skill5143DescText = "Skill5143DescText";
        public const string Skill5144NameText = "Skill5144NameText";
        public const string Skill5144DescText = "Skill5144DescText";
        public const string PlaceAsdalText = "PlaceAsdalText";
        public const string PlaceWindportText = "PlaceWindportText";
        public const string PlaceAshmineText = "PlaceAshmineText";
        public const string PlaceFrostkeepText = "PlaceFrostkeepText";
        public const string PlaceDawnspireText = "PlaceDawnspireText";
        public const string SunkenGroveNameText = "SunkenGroveNameText";
        public const string HollowBurrowNameText = "HollowBurrowNameText";
        public const string SaltMarshNameText = "SaltMarshNameText";
        public const string TidalCavernNameText = "TidalCavernNameText";
        public const string BrokenPierNameText = "BrokenPierNameText";
        public const string StormCliffNameText = "StormCliffNameText";
        public const string DriftwoodWoodNameText = "DriftwoodWoodNameText";
        public const string CinderPitNameText = "CinderPitNameText";
        public const string DeepShaftNameText = "DeepShaftNameText";
        public const string RustHallNameText = "RustHallNameText";
        public const string EmberVeinNameText = "EmberVeinNameText";
        public const string CollapsedTunnelNameText = "CollapsedTunnelNameText";
        public const string FrostWoodNameText = "FrostWoodNameText";
        public const string GlacierCaveNameText = "GlacierCaveNameText";
        public const string SilentChapelNameText = "SilentChapelNameText";
        public const string WhiteWasteNameText = "WhiteWasteNameText";
        public const string FrozenVaultNameText = "FrozenVaultNameText";
        public const string DawnGardenNameText = "DawnGardenNameText";
        public const string VoidStairNameText = "VoidStairNameText";
        public const string StarSanctumNameText = "StarSanctumNameText";
        public const string LastRampartNameText = "LastRampartNameText";
        public const string ThroneOfDawnNameText = "ThroneOfDawnNameText";
        public const string BreezingWindForestD1Text = "BreezingWindForestD1Text";
        public const string BreezingWindForestD2Text = "BreezingWindForestD2Text";
        public const string BreezingWindForestD3Text = "BreezingWindForestD3Text";
        public const string BreezingWindForestD4Text = "BreezingWindForestD4Text";
        public const string BreezingWindForestD5Text = "BreezingWindForestD5Text";
        public const string WhisperingCaveD1Text = "WhisperingCaveD1Text";
        public const string WhisperingCaveD2Text = "WhisperingCaveD2Text";
        public const string WhisperingCaveD3Text = "WhisperingCaveD3Text";
        public const string WhisperingCaveD4Text = "WhisperingCaveD4Text";
        public const string WhisperingCaveD5Text = "WhisperingCaveD5Text";
        public const string RuinedSanctumD1Text = "RuinedSanctumD1Text";
        public const string RuinedSanctumD2Text = "RuinedSanctumD2Text";
        public const string RuinedSanctumD3Text = "RuinedSanctumD3Text";
        public const string RuinedSanctumD4Text = "RuinedSanctumD4Text";
        public const string RuinedSanctumD5Text = "RuinedSanctumD5Text";
        public const string SunkenGroveD1Text = "SunkenGroveD1Text";
        public const string SunkenGroveD2Text = "SunkenGroveD2Text";
        public const string SunkenGroveD3Text = "SunkenGroveD3Text";
        public const string SunkenGroveD4Text = "SunkenGroveD4Text";
        public const string SunkenGroveD5Text = "SunkenGroveD5Text";
        public const string HollowBurrowD1Text = "HollowBurrowD1Text";
        public const string HollowBurrowD2Text = "HollowBurrowD2Text";
        public const string HollowBurrowD3Text = "HollowBurrowD3Text";
        public const string HollowBurrowD4Text = "HollowBurrowD4Text";
        public const string HollowBurrowD5Text = "HollowBurrowD5Text";
        public const string SaltMarshD1Text = "SaltMarshD1Text";
        public const string SaltMarshD2Text = "SaltMarshD2Text";
        public const string SaltMarshD3Text = "SaltMarshD3Text";
        public const string SaltMarshD4Text = "SaltMarshD4Text";
        public const string SaltMarshD5Text = "SaltMarshD5Text";
        public const string TidalCavernD1Text = "TidalCavernD1Text";
        public const string TidalCavernD2Text = "TidalCavernD2Text";
        public const string TidalCavernD3Text = "TidalCavernD3Text";
        public const string TidalCavernD4Text = "TidalCavernD4Text";
        public const string TidalCavernD5Text = "TidalCavernD5Text";
        public const string BrokenPierD1Text = "BrokenPierD1Text";
        public const string BrokenPierD2Text = "BrokenPierD2Text";
        public const string BrokenPierD3Text = "BrokenPierD3Text";
        public const string BrokenPierD4Text = "BrokenPierD4Text";
        public const string BrokenPierD5Text = "BrokenPierD5Text";
        public const string StormCliffD1Text = "StormCliffD1Text";
        public const string StormCliffD2Text = "StormCliffD2Text";
        public const string StormCliffD3Text = "StormCliffD3Text";
        public const string StormCliffD4Text = "StormCliffD4Text";
        public const string StormCliffD5Text = "StormCliffD5Text";
        public const string DriftwoodWoodD1Text = "DriftwoodWoodD1Text";
        public const string DriftwoodWoodD2Text = "DriftwoodWoodD2Text";
        public const string DriftwoodWoodD3Text = "DriftwoodWoodD3Text";
        public const string DriftwoodWoodD4Text = "DriftwoodWoodD4Text";
        public const string DriftwoodWoodD5Text = "DriftwoodWoodD5Text";
        public const string CinderPitD1Text = "CinderPitD1Text";
        public const string CinderPitD2Text = "CinderPitD2Text";
        public const string CinderPitD3Text = "CinderPitD3Text";
        public const string CinderPitD4Text = "CinderPitD4Text";
        public const string CinderPitD5Text = "CinderPitD5Text";
        public const string DeepShaftD1Text = "DeepShaftD1Text";
        public const string DeepShaftD2Text = "DeepShaftD2Text";
        public const string DeepShaftD3Text = "DeepShaftD3Text";
        public const string DeepShaftD4Text = "DeepShaftD4Text";
        public const string DeepShaftD5Text = "DeepShaftD5Text";
        public const string RustHallD1Text = "RustHallD1Text";
        public const string RustHallD2Text = "RustHallD2Text";
        public const string RustHallD3Text = "RustHallD3Text";
        public const string RustHallD4Text = "RustHallD4Text";
        public const string RustHallD5Text = "RustHallD5Text";
        public const string EmberVeinD1Text = "EmberVeinD1Text";
        public const string EmberVeinD2Text = "EmberVeinD2Text";
        public const string EmberVeinD3Text = "EmberVeinD3Text";
        public const string EmberVeinD4Text = "EmberVeinD4Text";
        public const string EmberVeinD5Text = "EmberVeinD5Text";
        public const string CollapsedTunnelD1Text = "CollapsedTunnelD1Text";
        public const string CollapsedTunnelD2Text = "CollapsedTunnelD2Text";
        public const string CollapsedTunnelD3Text = "CollapsedTunnelD3Text";
        public const string CollapsedTunnelD4Text = "CollapsedTunnelD4Text";
        public const string CollapsedTunnelD5Text = "CollapsedTunnelD5Text";
        public const string FrostWoodD1Text = "FrostWoodD1Text";
        public const string FrostWoodD2Text = "FrostWoodD2Text";
        public const string FrostWoodD3Text = "FrostWoodD3Text";
        public const string FrostWoodD4Text = "FrostWoodD4Text";
        public const string FrostWoodD5Text = "FrostWoodD5Text";
        public const string GlacierCaveD1Text = "GlacierCaveD1Text";
        public const string GlacierCaveD2Text = "GlacierCaveD2Text";
        public const string GlacierCaveD3Text = "GlacierCaveD3Text";
        public const string GlacierCaveD4Text = "GlacierCaveD4Text";
        public const string GlacierCaveD5Text = "GlacierCaveD5Text";
        public const string SilentChapelD1Text = "SilentChapelD1Text";
        public const string SilentChapelD2Text = "SilentChapelD2Text";
        public const string SilentChapelD3Text = "SilentChapelD3Text";
        public const string SilentChapelD4Text = "SilentChapelD4Text";
        public const string SilentChapelD5Text = "SilentChapelD5Text";
        public const string WhiteWasteD1Text = "WhiteWasteD1Text";
        public const string WhiteWasteD2Text = "WhiteWasteD2Text";
        public const string WhiteWasteD3Text = "WhiteWasteD3Text";
        public const string WhiteWasteD4Text = "WhiteWasteD4Text";
        public const string WhiteWasteD5Text = "WhiteWasteD5Text";
        public const string FrozenVaultD1Text = "FrozenVaultD1Text";
        public const string FrozenVaultD2Text = "FrozenVaultD2Text";
        public const string FrozenVaultD3Text = "FrozenVaultD3Text";
        public const string FrozenVaultD4Text = "FrozenVaultD4Text";
        public const string FrozenVaultD5Text = "FrozenVaultD5Text";
        public const string DawnGardenD1Text = "DawnGardenD1Text";
        public const string DawnGardenD2Text = "DawnGardenD2Text";
        public const string DawnGardenD3Text = "DawnGardenD3Text";
        public const string DawnGardenD4Text = "DawnGardenD4Text";
        public const string DawnGardenD5Text = "DawnGardenD5Text";
        public const string VoidStairD1Text = "VoidStairD1Text";
        public const string VoidStairD2Text = "VoidStairD2Text";
        public const string VoidStairD3Text = "VoidStairD3Text";
        public const string VoidStairD4Text = "VoidStairD4Text";
        public const string VoidStairD5Text = "VoidStairD5Text";
        public const string StarSanctumD1Text = "StarSanctumD1Text";
        public const string StarSanctumD2Text = "StarSanctumD2Text";
        public const string StarSanctumD3Text = "StarSanctumD3Text";
        public const string StarSanctumD4Text = "StarSanctumD4Text";
        public const string StarSanctumD5Text = "StarSanctumD5Text";
        public const string LastRampartD1Text = "LastRampartD1Text";
        public const string LastRampartD2Text = "LastRampartD2Text";
        public const string LastRampartD3Text = "LastRampartD3Text";
        public const string LastRampartD4Text = "LastRampartD4Text";
        public const string LastRampartD5Text = "LastRampartD5Text";
        public const string ThroneOfDawnD1Text = "ThroneOfDawnD1Text";
        public const string ThroneOfDawnD2Text = "ThroneOfDawnD2Text";
        public const string ThroneOfDawnD3Text = "ThroneOfDawnD3Text";
        public const string ThroneOfDawnD4Text = "ThroneOfDawnD4Text";
        public const string ThroneOfDawnD5Text = "ThroneOfDawnD5Text";
        public const string Mat11001NameText = "Mat11001NameText";
        public const string Mat11001DescText = "Mat11001DescText";
        public const string Mat11002NameText = "Mat11002NameText";
        public const string Mat11002DescText = "Mat11002DescText";
        public const string Mat11003NameText = "Mat11003NameText";
        public const string Mat11003DescText = "Mat11003DescText";
        public const string Mat11004NameText = "Mat11004NameText";
        public const string Mat11004DescText = "Mat11004DescText";
        public const string Mat11005NameText = "Mat11005NameText";
        public const string Mat11005DescText = "Mat11005DescText";
        public const string Mat11006NameText = "Mat11006NameText";
        public const string Mat11006DescText = "Mat11006DescText";
        public const string Mat11007NameText = "Mat11007NameText";
        public const string Mat11007DescText = "Mat11007DescText";
        public const string Mat11008NameText = "Mat11008NameText";
        public const string Mat11008DescText = "Mat11008DescText";
        public const string Mat11009NameText = "Mat11009NameText";
        public const string Mat11009DescText = "Mat11009DescText";
        public const string Mat11010NameText = "Mat11010NameText";
        public const string Mat11010DescText = "Mat11010DescText";
        public const string Mat11011NameText = "Mat11011NameText";
        public const string Mat11011DescText = "Mat11011DescText";
        public const string Mat11012NameText = "Mat11012NameText";
        public const string Mat11012DescText = "Mat11012DescText";
        public const string Mat11013NameText = "Mat11013NameText";
        public const string Mat11013DescText = "Mat11013DescText";
        public const string Mat11014NameText = "Mat11014NameText";
        public const string Mat11014DescText = "Mat11014DescText";
        public const string Mat11015NameText = "Mat11015NameText";
        public const string Mat11015DescText = "Mat11015DescText";
        public const string Mat11016NameText = "Mat11016NameText";
        public const string Mat11016DescText = "Mat11016DescText";
        public const string Mat11017NameText = "Mat11017NameText";
        public const string Mat11017DescText = "Mat11017DescText";
        public const string Mat11018NameText = "Mat11018NameText";
        public const string Mat11018DescText = "Mat11018DescText";
        public const string Mat11019NameText = "Mat11019NameText";
        public const string Mat11019DescText = "Mat11019DescText";
        public const string Mat11020NameText = "Mat11020NameText";
        public const string Mat11020DescText = "Mat11020DescText";
        public const string Mat11021NameText = "Mat11021NameText";
        public const string Mat11021DescText = "Mat11021DescText";
        public const string Mat11022NameText = "Mat11022NameText";
        public const string Mat11022DescText = "Mat11022DescText";
        public const string Mat11023NameText = "Mat11023NameText";
        public const string Mat11023DescText = "Mat11023DescText";
        public const string Mat11024NameText = "Mat11024NameText";
        public const string Mat11024DescText = "Mat11024DescText";
        public const string Mat11025NameText = "Mat11025NameText";
        public const string Mat11025DescText = "Mat11025DescText";
        public const string Mat11026NameText = "Mat11026NameText";
        public const string Mat11026DescText = "Mat11026DescText";
        public const string Mat11027NameText = "Mat11027NameText";
        public const string Mat11027DescText = "Mat11027DescText";
        public const string Mat11028NameText = "Mat11028NameText";
        public const string Mat11028DescText = "Mat11028DescText";
        public const string Mat11029NameText = "Mat11029NameText";
        public const string Mat11029DescText = "Mat11029DescText";
        public const string Mat11030NameText = "Mat11030NameText";
        public const string Mat11030DescText = "Mat11030DescText";
        public const string ForgeCantRecoverText = "ForgeCantRecoverText";
        public const string ForgeCantEnhanceText = "ForgeCantEnhanceText";
        public const string ForgeDurabilityText = "ForgeDurabilityText";
        public const string TestKey = "TestKey";
        public const string Skill5200NameText = "Skill5200NameText";
        public const string Skill5200DescText = "Skill5200DescText";
        public const string Skill5201NameText = "Skill5201NameText";
        public const string Skill5201DescText = "Skill5201DescText";
        public const string Skill5202NameText = "Skill5202NameText";
        public const string Skill5202DescText = "Skill5202DescText";
        public const string Skill5203NameText = "Skill5203NameText";
        public const string Skill5203DescText = "Skill5203DescText";
        public const string Skill5204NameText = "Skill5204NameText";
        public const string Skill5204DescText = "Skill5204DescText";
        public const string Skill5205NameText = "Skill5205NameText";
        public const string Skill5205DescText = "Skill5205DescText";
        public const string ForgePotentialText = "ForgePotentialText";
        public const string ForgeOrText = "ForgeOrText";
        public const string ForgeReqMaterialText = "ForgeReqMaterialText";
        public const string ItemSOM1NameText = "ItemSOM1NameText";
        public const string ItemSOM1DescText = "ItemSOM1DescText";
        public const string ItemSOM2NameText = "ItemSOM2NameText";
        public const string ItemSOM2DescText = "ItemSOM2DescText";
        public const string ItemSOM3NameText = "ItemSOM3NameText";
        public const string ItemSOM3DescText = "ItemSOM3DescText";
        public const string ItemBMORNameText = "ItemBMORNameText";
        public const string ItemBMORDescText = "ItemBMORDescText";
        public const string MagicAttackText = "MagicAttackText";
        public const string EnhanceLevelText = "EnhanceLevelText";
        public const string EnhanceSucceededText = "EnhanceSucceededText";
        public const string EnhanceFailedText = "EnhanceFailedText";
        public const string EnhancePreviewText = "EnhancePreviewText";
        public const string MonasteryUnlockText = "MonasteryUnlockText";
        public const string MonasteryNothingText = "MonasteryNothingText";
        public const string ForgeUnlockText = "ForgeUnlockText";
        public const string ForgeCraftBtnText = "ForgeCraftBtnText";
        public const string ForgeEnhanceBtnText = "ForgeEnhanceBtnText";
        public const string ForgeCraftTitleText = "ForgeCraftTitleText";
        public const string ForgeNoCraftableText = "ForgeNoCraftableText";
        public const string ForgeCraftedText = "ForgeCraftedText";
        public const string ForgeOwnedText = "ForgeOwnedText";
        public const string ForgeNoEquipText = "ForgeNoEquipText";
        public const string ShopText = "ShopText";
        public const string ShopUnlockText = "ShopUnlockText";
        public const string ShopBuyBtnText = "ShopBuyBtnText";
        public const string ShopSellBtnText = "ShopSellBtnText";
        public const string ShopBuyTitleText = "ShopBuyTitleText";
        public const string ShopSellTitleText = "ShopSellTitleText";
        public const string ShopNotEnoughGoldText = "ShopNotEnoughGoldText";
        public const string ShopNothingToSellText = "ShopNothingToSellText";
        public const string ShopLockedItemText = "ShopLockedItemText";
        public const string ShopPurchasedText = "ShopPurchasedText";
        public const string ShopSoldText = "ShopSoldText";
        public const string ShopSellPriceText = "ShopSellPriceText";
        public const string ShopOwnedCountText = "ShopOwnedCountText";
        public const string ConfirmBtnText = "ConfirmBtnText";
        public const string CancelBtnText = "CancelBtnText";
        public const string ShopConfirmBuyTitleText = "ShopConfirmBuyTitleText";
        public const string ShopConfirmSellTitleText = "ShopConfirmSellTitleText";
        public const string MonSkillSlimeName = "MonSkillSlimeName";
        public const string MonSkillSlimeDesc = "MonSkillSlimeDesc";
        public const string MonSkillGoblinName = "MonSkillGoblinName";
        public const string MonSkillGoblinDesc = "MonSkillGoblinDesc";
        public const string MonSkillForestSpiderName = "MonSkillForestSpiderName";
        public const string MonSkillForestSpiderDesc = "MonSkillForestSpiderDesc";
        public const string MonSkillWildWolfName = "MonSkillWildWolfName";
        public const string MonSkillWildWolfDesc = "MonSkillWildWolfDesc";
        public const string MonSkillToadstoolName = "MonSkillToadstoolName";
        public const string MonSkillToadstoolDesc = "MonSkillToadstoolDesc";
        public const string MonSkillWindSpriteName = "MonSkillWindSpriteName";
        public const string MonSkillWindSpriteDesc = "MonSkillWindSpriteDesc";
        public const string MonSkillBoarName = "MonSkillBoarName";
        public const string MonSkillBoarDesc = "MonSkillBoarDesc";
        public const string MonSkillCrowName = "MonSkillCrowName";
        public const string MonSkillCrowDesc = "MonSkillCrowDesc";
        public const string MonSkillCreeperName = "MonSkillCreeperName";
        public const string MonSkillCreeperDesc = "MonSkillCreeperDesc";
        public const string MonSkillFoxName = "MonSkillFoxName";
        public const string MonSkillFoxDesc = "MonSkillFoxDesc";
        public const string MonSkillHornetName = "MonSkillHornetName";
        public const string MonSkillHornetDesc = "MonSkillHornetDesc";
        public const string MonSkillCaveBatName = "MonSkillCaveBatName";
        public const string MonSkillCaveBatDesc = "MonSkillCaveBatDesc";
        public const string MonSkillMoleName = "MonSkillMoleName";
        public const string MonSkillMoleDesc = "MonSkillMoleDesc";
        public const string MonSkillMinerWraithName = "MonSkillMinerWraithName";
        public const string MonSkillMinerWraithDesc = "MonSkillMinerWraithDesc";
        public const string MonSkillCrystalGolemName = "MonSkillCrystalGolemName";
        public const string MonSkillCrystalGolemDesc = "MonSkillCrystalGolemDesc";
        public const string MonSkillCentipedeName = "MonSkillCentipedeName";
        public const string MonSkillCentipedeDesc = "MonSkillCentipedeDesc";
        public const string MonSkillMossHorrorName = "MonSkillMossHorrorName";
        public const string MonSkillMossHorrorDesc = "MonSkillMossHorrorDesc";
        public const string MonSkillStoneSentryName = "MonSkillStoneSentryName";
        public const string MonSkillStoneSentryDesc = "MonSkillStoneSentryDesc";
        public const string MonSkillShadeName = "MonSkillShadeName";
        public const string MonSkillShadeDesc = "MonSkillShadeDesc";
        public const string MonSkillSalamanderName = "MonSkillSalamanderName";
        public const string MonSkillSalamanderDesc = "MonSkillSalamanderDesc";
        public const string MonSkillGlowWormName = "MonSkillGlowWormName";
        public const string MonSkillGlowWormDesc = "MonSkillGlowWormDesc";
        public const string MonSkillRevenantName = "MonSkillRevenantName";
        public const string MonSkillRevenantDesc = "MonSkillRevenantDesc";
        public const string MonSkillStoneKnightName = "MonSkillStoneKnightName";
        public const string MonSkillStoneKnightDesc = "MonSkillStoneKnightDesc";
        public const string MonSkillFallenPriestName = "MonSkillFallenPriestName";
        public const string MonSkillFallenPriestDesc = "MonSkillFallenPriestDesc";
        public const string MonSkillChimeraName = "MonSkillChimeraName";
        public const string MonSkillChimeraDesc = "MonSkillChimeraDesc";
        public const string MonSkillGargoyleName = "MonSkillGargoyleName";
        public const string MonSkillGargoyleDesc = "MonSkillGargoyleDesc";
        public const string MonSkillWardenName = "MonSkillWardenName";
        public const string MonSkillWardenDesc = "MonSkillWardenDesc";
        public const string MonSkillPhantasmName = "MonSkillPhantasmName";
        public const string MonSkillPhantasmDesc = "MonSkillPhantasmDesc";
        public const string MonSkillTaintedSpiritName = "MonSkillTaintedSpiritName";
        public const string MonSkillTaintedSpiritDesc = "MonSkillTaintedSpiritDesc";
        public const string MonSkillAltarWardName = "MonSkillAltarWardName";
        public const string MonSkillAltarWardDesc = "MonSkillAltarWardDesc";
        public const string MonSkillVoidThingName = "MonSkillVoidThingName";
        public const string MonSkillVoidThingDesc = "MonSkillVoidThingDesc";
        public const string EquipAlreadyOwnedText = "EquipAlreadyOwnedText";
        public const string EquipCannotCraftText = "EquipCannotCraftText";
        public const string RequiredLevelFormat = "RequiredLevelFormat";
        public const string RequiredGoldFormat = "RequiredGoldFormat";
        public const string RequiredItemFormat = "RequiredItemFormat";
        public const string MissingItemFormat = "MissingItemFormat";
        public const string EquipSlotWeaponText = "EquipSlotWeaponText";
        public const string EquipSlotHelmetText = "EquipSlotHelmetText";
        public const string EquipSlotArmorText = "EquipSlotArmorText";
        public const string EquipSlotGlovesText = "EquipSlotGlovesText";
        public const string EquipSlotLegsText = "EquipSlotLegsText";
        public const string EquipSlotAccessoryText = "EquipSlotAccessoryText";
        public const string PlayerLevelLabelText = "PlayerLevelLabelText";
        public const string GoldLabelText = "GoldLabelText";
        public const string DurabilityFullText = "DurabilityFullText";
        public const string InsufficientMaterialsText = "InsufficientMaterialsText";
        public const string EnhanceMaxText = "EnhanceMaxText";
        public const string DurabilityZeroText = "DurabilityZeroText";
        public const string SelectScrollText = "SelectScrollText";
        public const string SelectScrollFirstText = "SelectScrollFirstText";
        public const string EnhanceGoldShortFormat = "EnhanceGoldShortFormat";
        public const string EquipActionText = "EquipActionText";
        public const string EquippedText = "EquippedText";
        public const string EquipmentStatsText = "EquipmentStatsText";
        public const string ForgeRestoreTitleText = "ForgeRestoreTitleText";
        public const string ForgeRestoreActionText = "ForgeRestoreActionText";
        public const string SkipEnhanceAnimationText = "SkipEnhanceAnimationText";
        public const string EnhanceDurabilityRiskText = "EnhanceDurabilityRiskText";

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

    // UI_GameScene 하단 (나가기 / 전투 중 여정 버튼 느낌표)
    public const string EXIT_QUEST_VFX = "EXIT_QUEST_VFX";
    public const string TRAVEL_BUTTON_ON_BATTLE_QUEST_VFX = "TRAVEL_BUTTON_ON_BATTLE_QUEST_VFX";

    // Travel UI (미구현)
    public const string BTN_JOURNEY = "BTN_JOURNEY";
    public const string BTN_EXPLORE = "BTN_EXPLORE";
    public const string BTN_TOWN = "BTN_TOWN";   // 여정 팝업 마을 카테고리 버튼 느낌표 (BTN_EXPLORE와 대칭)
    public const string BTN_MAP_BREEZING_WIND_FOREST = "BTN_MAP_BREEZING_WIND_FOREST";
    public const string BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE = "BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE";

    // Village1 Build Btn Group
    public const string VILLAGE1_GUILD_TALK_VFX = "VILLAGE1_GUILD_TALK_VFX";
    public const string VILLAGE1_GUILD_QUEST_VFX = "VILLAGE1_GUILD_QUEST_VFX";
    public const string GUILD_EXIT_QUEST_VFX = "GUILD_EXIT_QUEST_VFX";
    public const string BTN_GUILD_QUEST = "BTN_GUILD_QUEST";   // 길드 [퀘스트] 버튼 클릭 액션 키 (QuestData.CompleteTarget 매칭)

    // Village1 Alchemy Btn Group
    public const string VILLAGE1_ALCHEMY_TALK_VFX = "VILLAGE1_ALCHEMY_TALK_VFX";
    public const string VILLAGE1_ALCHEMY_TALK_VFX_SUB1 = "VILLAGE1_ALCHEMY_TALK_VFX_SUB1";
    public const string VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT = "VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT";
    public const string VILLAGE1_ALCHEMY_QUEST_VFX = "VILLAGE1_ALCHEMY_QUEST_VFX";
    public const string ALCHEMY_EXIT_QUEST_VFX = "ALCHEMY_EXIT_QUEST_VFX";

    // SubQuest notify / unlock — SubQuest_Notify.TargetID / SubQuest_Unlock_Table.TargetID 와 대응
    public const string BTN_ALCHEMY       = "BTN_ALCHEMY";        // Village1 연금공방 버튼 유도
    public const string BTN_ALCHEMY_TALK  = "BTN_ALCHEMY_TALK";   // 연금공방 NPC 대화 버튼 유도
    public const string BTN_COMMISION     = "BTN_COMMISION";      // 제작의뢰 버튼 해금 (시트 표기 유지)

    // Village1 Monastery Btn Group
    public const string VILLAGE1_MONASTERY_TALK_VFX = "VILLAGE1_MONASTERY_TALK_VFX";
    public const string VILLAGE1_MONASTERY_TALK_VFX_SUB1 = "VILLAGE1_MONASTERY_TALK_VFX_SUB1";
    public const string VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT = "VILLAGE1_MONASTERY_CUSTOM_ORDER_BUTTON_NEX_TXT";
    public const string VILLAGE1_MONASTERY_QUEST_VFX = "VILLAGE1_MONASTERY_QUEST_VFX";
    public const string MONASTERY_EXIT_QUEST_VFX = "MONASTERY_EXIT_QUEST_VFX";

    // 대장간 / 상점
    public const string VILLAGE1_FORGE_VFX       = "VILLAGE1_FORGE_VFX";
    public const string VILLAGE1_FORGE_TALK_VFX  = "VILLAGE1_FORGE_TALK_VFX";
    // _SUB1 = 서브퀘스트용 **초록** 느낌표. 접미사 없는 쪽이 메인 퀘스트 **노랑**(FFE300/14FF00).
    public const string VILLAGE1_FORGE_TALK_VFX_SUB1 = "VILLAGE1_FORGE_TALK_VFX_SUB1";
    public const string FORGE_EXIT_QUEST_VFX     = "FORGE_EXIT_QUEST_VFX";
    public const string BTN_FORGE_CRAFT          = "BTN_FORGE_CRAFT";
    public const string BTN_FORGE_ENHANCE        = "BTN_FORGE_ENHANCE";
    public const string VILLAGE1_SHOP_VFX        = "VILLAGE1_SHOP_VFX";
    public const string VILLAGE1_SHOP_TALK_VFX   = "VILLAGE1_SHOP_TALK_VFX";
    public const string VILLAGE1_SHOP_TALK_VFX_SUB1 = "VILLAGE1_SHOP_TALK_VFX_SUB1";
    public const string SHOP_EXIT_QUEST_VFX      = "SHOP_EXIT_QUEST_VFX";
    public const string BTN_SHOP_BUY             = "BTN_SHOP_BUY";
    public const string BTN_SHOP_SELL            = "BTN_SHOP_SELL";
    #endregion

    #region UnlockKey
    // QuestUnlockData.TargetID (UnlockType = UI_BUTTON) 과 1:1 대응
    // UI_GameScene 하단 탭 버튼
    public const string UNLOCK_INVENTORY_BUTTON = "UNLOCK_INVENTORY_BUTTON";
    public const string UNLOCK_POTION_BUTTON    = "UNLOCK_POTION_BUTTON";

    // UI_Village1BtnGroup 이동 버튼
    public const string UNLOCK_TRAVEL_BUTTON    = "UNLOCK_TRAVEL_BUTTON";

    // UI_Village1BtnGroup 연금공방 버튼
    public const string UNLOCK_ALCHEMY_BUTTON   = "UNLOCK_ALCHEMY_BUTTON";

    // UI_Village1BtnGroup 마을 건물 — 수도원/대장간/상점.
    //  수도원은 그룹·[축성]까지 저작돼 있었는데 메인 버튼에 클릭 바인딩이 없어 들어갈 방법이 없었다.
    //  대장간/상점은 TempButton1/TempButton2 자리를 쓴다.
    public const string UNLOCK_MONASTERY_BUTTON = "UNLOCK_MONASTERY_BUTTON";
    public const string UNLOCK_FORGE_BUTTON     = "UNLOCK_FORGE_BUTTON";
    public const string UNLOCK_SHOP_BUTTON      = "UNLOCK_SHOP_BUTTON";
    #endregion

    // Mythic_Button
    public const string MYTHIC_BUTTON_X = "Mythic_Button_X.sprite";
    public const string MYTHIC_BUTTON_CHECK = "Mythic_Button_Check.sprite";
    public const string MYTHIC_BUTTON_RETURN = "Mythic_Button_Return.sprite";

    public const int ITEM_ID_MIN = 10000;
    public const int EQUIP_ID_MIN = 50000;
    public const int POTION_ID_MIN = 30000;
    public const int SPEED_POTION_ID_MIN = 40000;
    public const int IMPORTANT_ITEM_ID_MIN = 20000;

    #region Ground (접지)
    // 캐릭터의 **발이 닿는** 월드 Y. 지면 타일(JungleTileset) 윗면 높이다.
    //  플레이어·몬스터·동행 미니몬 전부 이 값 하나를 쓴다. 아트마다 다르던 바닥 여백은
    //  스프라이트 pivot이 흡수한다(Tools/Ground Pivot 정렬). 여백을 코드에서 보정하지 말 것 —
    //  아트가 바뀔 때마다 상수가 늘어나고, 그러다 "누구는 뜨고 누구는 파묻히는" 상태로 돌아간다.
    public const float GROUND_Y = 2.33f;
    #endregion

    #region Forge (대장간/장비 강화 — VS 원작 Enhance Chance Cal 발굴 수치)
    // 강화 최대 단계 (+9). 도달 시 장비 능력 영구화(슬38).
    public const int ENHANCE_MAX_LEVEL = 9;

    // 강화 성공확률(%) — 인덱스 = 현재 강화도(+0..+8). Low=잠재력 0 / High=잠재력 만충.
    public static readonly float[] ENHANCE_CHANCE_LOW  = { 90f, 75f, 50f, 25f, 10f, 6f, 4f, 2f, 1f };
    public static readonly float[] ENHANCE_CHANCE_HIGH = { 99f, 90f, 75f, 50f, 35f, 25f, 15f, 10f, 5f };

    // 강화 성공 시 잠재력 증가량 — 인덱스 = EquipGrade(Basic~Mystic). REQUISITE는 Mystic 취급.
    public static readonly int[] ENHANCE_POTEN_BY_GRADE = { 4, 10, 20, 40, 80, 150 };

    // ponytail: 원작 잠재력→확률 보간 수식 미해독 — 만충 기준 100, 초과분은 +0.2%/포인트로 근사.
    public const float ENHANCE_POTEN_FULL = 100f;            // Low→High 만충 기준 잠재력
    public const float ENHANCE_POTEN_OVER_BONUS = 0.2f;      // 만충 초과 잠재력 1당 확률 +%

    // 강화 주문서 4종 (ItemData DataId) — 값 = 실패 시 내구도 감소량 (VS 원작 Amount).
    //  10014(이그누스의 인정)는 확정 강화 + 감소 0.
    public const int SCROLL_BASIC_ID    = 10011;   // 장비 강화 주문서 (실패 시 내구도 -20)
    public const int SCROLL_ADVANCED_ID = 10012;   // 고급 장비 강화 주문서 (-10)
    public const int SCROLL_ANCIENT_ID  = 10013;   // 고대 장비 강화 주문서 (-5)
    public const int SCROLL_IGNUS_ID    = 10014;   // 이그누스의 인정 (확정 강화, -0)
    public static readonly int[] ENHANCE_SCROLL_IDS       = { SCROLL_BASIC_ID, SCROLL_ADVANCED_ID, SCROLL_ANCIENT_ID, SCROLL_IGNUS_ID };
    public static readonly int[] ENHANCE_SCROLL_DURI_LOSS = { 20, 10, 5, 0 };

    // 복원의 거울(MOR) — 내구도 +25 복구 소비 아이템.
    public const int MOR_ITEM_ID = 10015;
    public const int MOR_RESTORE_AMOUNT = 25;

    // 장인의 인장(Sigil Of Master) 3종 + 축복의 거울.
    //  스프라이트(SOM1~3 / BrokenMOR)는 진작 있었는데 **아이템 데이터가 없어** 대장간 슬롯이
    //  빈 채로 dim 처리돼 있었다("스코프 제외" 주석). 데이터를 만들고 실제 보유 수를 표시한다.
    public static readonly int[] SOM_ITEM_IDS = { 10016, 10017, 10018 };
    public const int BMOR_ITEM_ID = 10019;
    public const int BMOR_RESTORE_AMOUNT = 999;   // 축복의 거울 = 내구도 완전 회복
    // 재료 복구(거울 없이): 젤리퐁 5개당 +25. ponytail: 원작 재료 규칙 미해독 — 시드값, 엑셀화 후속.
    public const int RESTORE_MATERIAL_ID = 10001;
    public const int RESTORE_MATERIAL_COUNT = 5;
    public const int RESTORE_MATERIAL_AMOUNT = 25;
    #endregion

    // 전직(Class): 직업 숙련도 최대치 — 도달 시 수도원 [축성]으로 직업 능력 습득 가능.
    public const int CLASS_PROFICIENCY_MAX = 100;

    // COLOR
    public const string DEFAULT_COLOR = "FFFFFF";
    public const string SHORTAGE_COLOR = "FEA400";
    public const string EMPTY_COLOR = "FF0000";
    public const string COMPLETE_COLOR = "00FF00";
    public const string QUEST_FOLLOW_DEFAULT_COLOR = "FFC84E";
}
