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

    // ── 슬39: 능력 전투 발동조건 ──
    // 능력 발동 방식. None = 패시브(항시 스탯 가산, 기존 동작).
    public enum AbilityTrigger
    {
        None,         // 패시브 (스탯만 가산)
        Probability,  // 확률 발동: 조건 충족 시 ProcChance% 확률로 발동
        Time,         // 시간 발동: ActiveContext 충족 시 Cooldown마다 발동 (쿨타임 스킬)
    }

    // 확률 발동 조건 (슬39 — 연속 n회/사망조건 등 stateful 조건은 후속).
    public enum AbilityCondition
    {
        BattleStart,  // 전투 시작 시
        OnAttack,     // 공격 시
        OnHit,        // 타격(명중) 시
        OnCrit,       // 치명타 시
        OnMiss,       // 타격이 빗나갈 시
        SelfEvade,    // 자신이 회피 시
        OnDamaged,    // 피격 시
        OnHeal,       // 체력 회복 시
        LowHp,        // 본인 HP가 ConditionValue% 이하가 될 시 (전투당 1회)
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
        // ── GlobalButtons ──
        public const string TravelBtnText = "TravelBtnText";
        public const string ExitBtnText = "ExitBtnText";
        public const string TalkBtnText = "TalkBtnText";
        public const string ItemNeededText = "ItemNeededText";
        public const string DescriptionText = "DescriptionText";
        public const string NextInfoText = "NextInfoText";
        public const string CraftText = "CraftText";
        public const string RankText = "RankText";
        public const string TestKey = "TestKey";

        // ── ButtomButtons ──
        public const string BtmCharacterBtnText = "BtmCharacterBtnText";
        public const string BtmInventoryBtnText = "BtmInventoryBtnText";
        public const string BtmPotionBtnText = "BtmPotionBtnText";
        public const string BtmEquipmentBtnText = "BtmEquipmentBtnText";
        public const string BtmSkillBtnText = "BtmSkillBtnText";
        public const string BtmMinimonCodexBtnText = "BtmMinimonCodexBtnText";

        // ── Battle ──
        public const string BattleMissText = "BattleMissText";
        public const string PlayerLvPrefix = "PlayerLvPrefix";
        public const string DmgBoostText = "DmgBoostText";
        public const string ReturnText = "ReturnText";
        public const string ReturnConfirmText = "ReturnConfirmText";
        public const string ComboLv0Text = "ComboLv0Text";
        public const string ComboLv1Text = "ComboLv1Text";
        public const string ComboLv2Text = "ComboLv2Text";
        public const string ComboLv3Text = "ComboLv3Text";
        public const string ComboLv4Text = "ComboLv4Text";
        public const string ComboLv5Text = "ComboLv5Text";
        public const string ComboLv6Text = "ComboLv6Text";
        public const string BattleTimeText = "BattleTimeText";
        public const string BattleLogText = "BattleLogText";
        public const string DmgDealtText = "DmgDealtText";
        public const string DmgTakenText = "DmgTakenText";
        public const string HPRestoredText = "HPRestoredText";

        // ── Travel ──
        public const string TownText = "TownText";
        public const string ExploreText = "ExploreText";
        public const string BackBtnText = "BackBtnText";
        public const string HeadOutText = "HeadOutText";
        public const string CurrentLocationText = "CurrentLocationText";
        public const string BreezingWindForestNameText = "BreezingWindForestNameText";
        public const string BreezingWindForestEntranceText = "BreezingWindForestEntranceText";

        // ── Battle (추가) ──
        public const string ComboText = "ComboText";
        public const string CompletionText = "CompletionText";
        public const string BeastiaryText = "BeastiaryText"; // 슬1: 도감 팝업 타이틀 "마물 도감"
        public const string CritTextPopupText = "CritTextPopupText";
        public const string EXPText = "EXPText";
        public const string LevelText = "LevelText";
        public const string LogText = "LogText";
        public const string LogCompletionText = "LogCompletionText";
        public const string NeedMatsText = "NeedMatsText";

        // ── Bestiary / Minimon ──
        public const string AttentionRateText = "AttentionRateText";
        public const string EncounterText = "EncounterText";
        public const string HabitatText = "HabitatText";
        public const string InfoAndStatsText = "InfoAndStatsText";
        public const string MinimonCodexText = "MinimonCodexText";
        public const string MinimonEntryText = "MinimonEntryText";
        public const string MinimonNameText = "MinimonNameText";
        public const string MyMinimon = "MyMinimon";
        public const string UniqueSkillText = "UniqueSkillText";
        public const string ParticalGetText = "ParticalGetText"; // 슬10: 부분획득
        public const string CompleteGetText = "CompleteGetText"; // 슬11: 완벽획득
        public const string RequireMinimonText = "RequireMinimonText"; // 슬12: 미포획 시 버튼 "포획 필요"
        public const string MyMinimonText = "MyMinimonText"; // 슬12: 포획 시 버튼 "내 미니몬"(창 제목 MyMinimon="나의 미니몬"과 별개)
        public const string FollowingText = "FollowingText"; // 슬13: 동행 중 하단 UI "동행중"
        public const string SummonText = "SummonText";       // 슬19: [부르기] 버튼 ("부르기"/"SUMMON")
        public const string MinimonGrowthFailText = "MinimonGrowthFailText"; // 슬22: 재포획 성장 실패 문구
        public const string CatchMinimonTitleText = "CatchMinimonTitleText"; // 슬28: [미니몬 잡기] 버튼/팝업 제목
        // 슬31/32: 잡기 미니게임 대사/수치 텍스트 (한/영)
        public const string CatchMinimonStartText1 = "CatchMinimonStartText1";
        public const string CatchMinimonStartText2 = "CatchMinimonStartText2";
        public const string TryLureText = "TryLureText";
        public const string LureEffectiveText = "LureEffectiveText";
        public const string LureNotEffectiveText = "LureNotEffectiveText";
        public const string LureSuccessText1 = "LureSuccessText1";
        public const string LureSuccessText2 = "LureSuccessText2";
        public const string LureFailText = "LureFailText";
        public const string GoMandrakeText = "GoMandrakeText";
        public const string EatMandrakeText = "EatMandrakeText";
        public const string CatchSuccessRateText = "CatchSuccessRateText";
        public const string CatchSuccessAtText = "CatchSuccessAtText";
        public const string GuaranteedCatchText = "GuaranteedCatchText";
        public const string ReqMandrakeText = "ReqMandrakeText";
        public const string AbilityCodex_TitleText = "AbilityCodex_TitleText"; // 슬40: 능력 목록 팝업 제목
        // 슬42: 능력 목록 하단 필터 버튼(=상단 TopText) 라벨 — 능력 종류(전체/기술/마법/수비/지원)
        public const string AbilityCodex_BtmText_All = "AbilityCodex_BtmText_All";
        public const string AbilityCodex_BtmText_Skill = "AbilityCodex_BtmText_Skill";
        public const string AbilityCodex_BtmText_Spell = "AbilityCodex_BtmText_Spell";
        public const string AbilityCodex_BtmText_Def = "AbilityCodex_BtmText_Def";
        public const string AbilityCodex_BtmText_Util = "AbilityCodex_BtmText_Util";
        public const string MinimonInterestSuccessText = "MinimonInterestSuccessText"; // 슬24: 관심 알림(이름 뒤)
        public const string MinimonInterestFullText = "MinimonInterestFullText"; // 슬25: 관심+창고가득 알림(이름 뒤)

        // ── Misc ──
        public const string LinkText = "LinkText";
        public const string Link100RequiredText = "Link100RequiredText";
        public const string LootText = "LootText";
        public const string SPDPotion0DescText = "SPDPotion0DescText";

        // ── Setting Popup (JSON 키 추가 필요) ──
        public const string LanguageText = "LanguageText";
        public const string SoundText = "SoundText";
        public const string BGMText = "BGMText";
        public const string SFXText = "SFXText";
        public const string EffectsTitle = "EffectsTitle";
        public const string PopupText = "PopupText";
        public const string CameraShakeText = "CameraShakeText";

        // ── Title Scene (JSON 키 추가 필요) ──
        public const string StartText = "StartText";

        // ── Inventory Slot (JSON 키 추가 필요) ──
        public const string InvSlotNewTxt = "InvSlotNewTxt";
        public const string ImportantSlotNewTxt = "ImportantSlotNewTxt";

        // ── Village1 NEW 배지 (JSON 키 추가 필요 — 모두 "NEW" 표시) ──
        public const string AlchemyButtonNewTxt = "AlchemyButtonNewTxt";
        public const string MonasteryButtonNewTxt = "MonasteryButtonNewTxt";
        public const string TravelButtonNewTxt = "TravelButtonNewTxt";
        public const string AlchemyCustomOrderButtonNewTxt = "AlchemyCustomOrderButtonNewTxt";
        public const string MonasteryCustomOrderButtonNewTxt = "MonasteryCustomOrderButtonNewTxt";

        // ── TownAsdalButtons ──
        public const string GuildText = "GuildText";
        public const string QuestText = "QuestText";
        public const string AlchemyText = "AlchemyText";
        public const string CommissionText = "CommissionText";
        public const string MonasteryText = "MonasteryText";
        public const string AnointmentText = "AnointmentText";
        public const string SmithyText = "SmithyText";
        public const string ForgeText = "ForgeText";

        // ── StatText ──
        public const string StrengthText = "StrengthText";
        public const string VitialityText = "VitialityText";
        public const string AgilityText = "AgilityText";
        public const string MagicText = "MagicText";
        public const string AttackPowerText = "AttackPowerText";   // "공격력" (미니몬 능력치 라벨)
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
        public const string SPDBonusText = "SPDBonusText";
        public const string DurationText = "DurationText";
        public const string SecondsUnitText = "SecondsUnitText";
        public const string BaseStatText = "BaseStatText";
        public const string IncreseStatText = "IncreseStatText";
        public const string BoostStatText = "BoostStatText";

        // ── CharacterPopup ──
        public const string CharPopupTitleText = "CharPopupTitleText";
        public const string CharPopupStatPointText = "CharPopupStatPointText";
        public const string CharPopupElixirText = "CharPopupElixirText";
        public const string CharPopupAttributeDetailText = "CharPopupAttributeDetailText";
        public const string CharPopupOffenseText = "CharPopupOffenseText";
        public const string CharPopupDefenseText = "CharPopupDefenseText";
        public const string CharPopupUtilityText = "CharPopupUtilityText";

        // ── ItemRarity ──
        public const string RarityBasicText = "RarityBasicText";
        public const string RarityAbnormalText = "RarityAbnormalText";
        public const string RarityRareText = "RarityRareText";
        public const string RarityExceptionalText = "RarityExceptionalText";
        public const string RarityLegendaryText = "RarityLegendaryText";
        public const string RarityMysticText = "RarityMysticText";
        public const string RarityREQUISITEText = "RarityREQUISITEText";

        // ── ItemName ──
        public const string ItemJellyPongNameText = "ItemJellyPongNameText";
        public const string ItemRabiJellyNameText = "ItemRabiJellyNameText";
        public const string ItemPongyaPearlNameText = "ItemPongyaPearlNameText";
        public const string ItemScrollOfENH1NameText = "ItemScrollOfENH1NameText";
        public const string ItemScrollOfENH2NameText = "ItemScrollOfENH2NameText";
        public const string ItemScrollOfENH3NameText = "ItemScrollOfENH3NameText";
        public const string ItemScrollOfENH4NameText = "ItemScrollOfENH4NameText";
        public const string ItemMastersSigil1NameText = "ItemMastersSigil1NameText";
        public const string ItemMastersSigil2NameText = "ItemMastersSigil2NameText";
        public const string ItemMastersSigil3NameText = "ItemMastersSigil3NameText";
        public const string ItemTempPermitNameText = "ItemTempPermitNameText";
        public const string ItemRegularPermitNameText = "ItemRegularPermitNameText";
        public const string HPPotion1NameText = "HPPotion1NameText";
        public const string HPPotion2NameText = "HPPotion2NameText";
        public const string SPDPotion1NameText = "SPDPotion1NameText";
        public const string SPDPotion2NameText = "SPDPotion2NameText";

        // ── ItemDesc ──
        public const string ItemJellyPongDescText = "ItemJellyPongDescText";
        public const string ItemRabiJellyDescText = "ItemRabiJellyDescText";
        public const string ItemPongyaPearlDescText = "ItemPongyaPearlDescText";
        public const string ItemScrollOfENH1DescText = "ItemScrollOfENH1DescText";
        public const string ItemScrollOfENH2DescText = "ItemScrollOfENH2DescText";
        public const string ItemScrollOfENH3DescText = "ItemScrollOfENH3DescText";
        public const string ItemScrollOfENH4DescText = "ItemScrollOfENH4DescText";
        public const string ItemMastersSigil1DescText = "ItemMastersSigil1DescText";
        public const string ItemMastersSigil2DescText = "ItemMastersSigil2DescText";
        public const string ItemMastersSigil3DescText = "ItemMastersSigil3DescText";
        public const string ItemTempPermitDescText = "ItemTempPermitDescText";
        public const string ItemRegularPermitDescText = "ItemRegularPermitDescText";
        public const string HPPotion1DescText = "HPPotion1DescText";
        public const string HPPotion2DescText = "HPPotion2DescText";
        public const string SPDPotion1DescText = "SPDPotion1DescText";
        public const string SPDPotion2DescText = "SPDPotion2DescText";

        // ── PlaceName ──
        public const string PlaceEverywhereText = "PlaceEverywhereText";
        public const string PlaceNameTownAsdalText = "PlaceNameTownAsdalText";
        public const string PlaceNameBWF0Text = "PlaceNameBWF0Text";
        public const string PlaceNameBWF1Text = "PlaceNameBWF1Text";
        public const string PlaceNameBWF2Text = "PlaceNameBWF2Text";
        public const string PlaceNameBWF3Text = "PlaceNameBWF3Text";

        // ── UnlockText ──
        public const string TravelUnlockText = "TravelUnlockText";
        public const string AlchemyUnlockText = "AlchemyUnlockText";

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

    // COLOR
    public const string DEFAULT_COLOR = "FFFFFF";
    public const string SHORTAGE_COLOR = "FEA400";
    public const string EMPTY_COLOR = "FF0000";
    public const string COMPLETE_COLOR = "00FF00";
    public const string QUEST_FOLLOW_DEFAULT_COLOR = "FFC84E";
}
