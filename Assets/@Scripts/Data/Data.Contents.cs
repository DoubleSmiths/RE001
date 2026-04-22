using System;
using System.Collections.Generic;
using static Define;

namespace Data
{
    #region CreatureData
    [Serializable]
    public class CreatureData
    {
        public int DataId;
        public string Name;
        public string PrefabLabel;
        public string IconLabel;
        public float MaxHp;
        public float PhysicalAtk;
        public float MagicAtk;
        public float CritRate;
        public float CritDamage     = 150f;
        public float Accuracy       = 80f;
        public float AtkSpeed       = 100f;
        public float Def;
        public float PhysicalResist;
        public float MagicResist;
        public float Evasion;
        public float RecoveryRate;
        public float MoveSpeed;
        public float TotalExp;
        public int RewardGold;
        // 드롭 아이템 (DropItemId == 0 이면 드롭 없음)
        public int DropItemId;
        public int DropCount;
        public float DropChance; // 0~1
    }

    [Serializable]
    public class CreatureDataLoader : ILoader<int, CreatureData>
    {
        public List<CreatureData> creatures = new List<CreatureData>();
        public Dictionary<int, CreatureData> MakeDict()
        {
            var dict = new Dictionary<int, CreatureData>();
            foreach (var item in creatures)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region EquipmentData
    [Serializable]
    public class EquipmentData
    {
        public int DataId;
        public string Name;
        public EquipSlotType SlotType;
        public EquipGrade Grade;
        public string SpriteName;
        public int MaxDurability;
        public int BaseAtk;
        public float BaseCritRate;
        public int BaseMagicAtk;
    }

    [Serializable]
    public class EquipmentDataLoader : ILoader<int, EquipmentData>
    {
        public List<EquipmentData> equipments = new List<EquipmentData>();
        public Dictionary<int, EquipmentData> MakeDict()
        {
            var dict = new Dictionary<int, EquipmentData>();
            foreach (var item in equipments)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region EquipEnhanceData
    [Serializable]
    public class EquipEnhanceData
    {
        public int DataId;
        public int EquipDataId;
        public int Level;           // +1, +2 ...
        public int AtkBonus;
        public float CritBonus;
        public int MagicAtkBonus;
        public int RequiredMaterialId;
        public int RequiredMaterialCount;
    }

    [Serializable]
    public class EquipEnhanceDataLoader : ILoader<int, EquipEnhanceData>
    {
        public List<EquipEnhanceData> enhances = new List<EquipEnhanceData>();
        public Dictionary<int, EquipEnhanceData> MakeDict()
        {
            var dict = new Dictionary<int, EquipEnhanceData>();
            foreach (var item in enhances)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region SkillData
    [Serializable]
    public class SkillData
    {
        public int DataId;
        public string Name;
        public string Description;
        public string IconLabel;
        public SkillCategory Category;
        public SkillActiveType SkillActiveType;
        public string StatType;
        public float StatBonus;
        public int MaxLevel;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();
        public Dictionary<int, SkillData> MakeDict()
        {
            var dict = new Dictionary<int, SkillData>();
            foreach (var item in skills)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region ItemData
    // InventoryItem / RequisiteItem / Equipment 시트 공용
    [Serializable]
    public class ItemData
    {
        public int DataId;
        public string Name;
        public EquipGrade Grade;
        public string Description;
        public string SpriteName;
        public string FoundLocation;
    }

    [Serializable]
    public class ItemDataLoader : ILoader<int, ItemData>
    {
        public List<ItemData> items = new List<ItemData>();
        public Dictionary<int, ItemData> MakeDict()
        {
            var dict = new Dictionary<int, ItemData>();
            foreach (var item in items)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region HPPotionData
    [Serializable]
    public class HPPotionData
    {
        public int DataId;
        public string Name;
        public EquipGrade Grade;
        public string Description;
        public string SpriteName;
        public string WhereTo;
        public int Step;
        public int FlatRecov;
        public int MaxHPRecov;
        public int Cooldown;
        public List<int> NeededItemIDs     = new List<int>();
        public List<int> NeededItemAmounts = new List<int>();
    }

    [Serializable]
    public class HPPotionDataLoader : ILoader<int, HPPotionData>
    {
        public List<HPPotionData> potions = new List<HPPotionData>();
        public Dictionary<int, HPPotionData> MakeDict()
        {
            var dict = new Dictionary<int, HPPotionData>();
            foreach (var p in potions)
                dict.Add(p.DataId, p);
            return dict;
        }
    }
    #endregion

    #region SpeedPotionData
    [Serializable]
    public class SpeedPotionData
    {
        public int DataId;
        public string Name;
        public EquipGrade Grade;
        public string Description;
        public string SpriteName;
        public string WhereTo;
        public int Step;
        public float AtkSpeedBonus;
        public int Duration;
        public int Cooldown;
        public List<int> NeededItemIDs     = new List<int>();
        public List<int> NeededItemAmounts = new List<int>();
    }

    [Serializable]
    public class SpeedPotionDataLoader : ILoader<int, SpeedPotionData>
    {
        public List<SpeedPotionData> potions = new List<SpeedPotionData>();
        public Dictionary<int, SpeedPotionData> MakeDict()
        {
            var dict = new Dictionary<int, SpeedPotionData>();
            foreach (var p in potions)
                dict.Add(p.DataId, p);
            return dict;
        }
    }
    #endregion

    #region AreaData
    [Serializable]
    public class AreaData
    {
        public int DataId;
        public string Name;
        public string Description;
        public string BackgroundLabel;
        public List<int> MonsterIds = new List<int>();
    }

    [Serializable]
    public class AreaDataLoader : ILoader<int, AreaData>
    {
        public List<AreaData> areas = new List<AreaData>();
        public Dictionary<int, AreaData> MakeDict()
        {
            var dict = new Dictionary<int, AreaData>();
            foreach (var item in areas)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region TravelAreaData
    [Serializable]
    public class TravelAreaData
    {
        public int DataId;
        public string Name;
        public string Description;
        public string SpriteName;
        public string SceneName;
        public int RecommendPower;
        public List<int> PreviewMonsterIds = new List<int>();
        public List<int> PreviewDropItemIds = new List<int>();
        public int Order;
        public string DefaultBtnId;
        public string EntryBtnId;
        public string UnlockSummary;
        public string Category;
        public int Depth;
        public int ParentId;
        public int RootId;
        public string NodeType;
        public int IsSelectable;
        public int IsEnterable;
        public string PanelType;
        public string Note;
    }

    [Serializable]
    public class TravelAreaDataLoader : ILoader<int, TravelAreaData>
    {
        public List<TravelAreaData> areas = new List<TravelAreaData>();
        public Dictionary<int, TravelAreaData> MakeDict()
        {
            var dict = new Dictionary<int, TravelAreaData>();
            foreach (var item in areas)
                dict.Add(item.DataId, item);
            return dict;
        }
    }

    [Serializable]
    public class TravelAreaConditionData
    {
        public int DataId;
        public int Order;
        public string ConditionType;
        public string Target;
        public int Value;
        public string FailMessage;
        public string Scope;
        public string Note;
    }

    [Serializable]
    public class TravelAreaConditionLoader : ILoader<int, List<TravelAreaConditionData>>
    {
        public List<TravelAreaConditionData> conditions = new List<TravelAreaConditionData>();
        public Dictionary<int, List<TravelAreaConditionData>> MakeDict()
        {
            var dict = new Dictionary<int, List<TravelAreaConditionData>>();
            foreach (var item in conditions)
            {
                if (!dict.ContainsKey(item.DataId))
                    dict[item.DataId] = new List<TravelAreaConditionData>();
                dict[item.DataId].Add(item);
            }
            foreach (var list in dict.Values)
                list.Sort((a, b) => a.Order.CompareTo(b.Order));
            return dict;
        }
    }

    [Serializable]
    public class TravelAreaNotifyData
    {
        public int DataId;
        public int Order;
        public string BtnId;
        public string NotifyType;
        public string Note;
    }

    [Serializable]
    public class TravelAreaNotifyLoader : ILoader<int, List<TravelAreaNotifyData>>
    {
        public List<TravelAreaNotifyData> notifies = new List<TravelAreaNotifyData>();
        public Dictionary<int, List<TravelAreaNotifyData>> MakeDict()
        {
            var dict = new Dictionary<int, List<TravelAreaNotifyData>>();
            foreach (var item in notifies)
            {
                if (!dict.ContainsKey(item.DataId))
                    dict[item.DataId] = new List<TravelAreaNotifyData>();
                dict[item.DataId].Add(item);
            }
            foreach (var list in dict.Values)
                list.Sort((a, b) => a.Order.CompareTo(b.Order));
            return dict;
        }
    }
    #endregion

    #region MinMonData
    [Serializable]
    public class MinMonData
    {
        public int DataId;
        public string Name;
        public string IconLabel;
        public string PrefabLabel;
        public int MonsterDataId;
        public float BaseAtk;
        public float BaseAtkSpeed;
        public float BaseCritRate;
        public MinMonGrade AtkGrade;
        public MinMonGrade AtkSpeedGrade;
        public MinMonGrade CritGrade;
        public int MaxTameCount;
        public float CatchChance;
    }

    [Serializable]
    public class MinMonDataLoader : ILoader<int, MinMonData>
    {
        public List<MinMonData> minMons = new List<MinMonData>();
        public Dictionary<int, MinMonData> MakeDict()
        {
            var dict = new Dictionary<int, MinMonData>();
            foreach (var item in minMons)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region ForgeRecipeData
    [Serializable]
    public class ForgeRecipeData
    {
        public int DataId;
        public int EquipDataId;
        public int Level;
        public int RequiredMaterialId;
        public int RequiredMaterialCount;
    }

    [Serializable]
    public class ForgeRecipeDataLoader : ILoader<int, ForgeRecipeData>
    {
        public List<ForgeRecipeData> recipes = new List<ForgeRecipeData>();
        public Dictionary<int, ForgeRecipeData> MakeDict()
        {
            var dict = new Dictionary<int, ForgeRecipeData>();
            foreach (var item in recipes)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region CraftRecipeData
    [Serializable]
    public class CraftMaterial
    {
        public int ItemId;
        public int Count;
    }

    [Serializable]
    public class CraftRecipeData
    {
        public int DataId;
        public CraftCategory Category;
        public int ResultItemId;
        public List<CraftMaterial> Materials = new List<CraftMaterial>();
    }

    [Serializable]
    public class CraftRecipeDataLoader : ILoader<int, CraftRecipeData>
    {
        public List<CraftRecipeData> recipes = new List<CraftRecipeData>();
        public Dictionary<int, CraftRecipeData> MakeDict()
        {
            var dict = new Dictionary<int, CraftRecipeData>();
            foreach (var item in recipes)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region LevelData
    [Serializable]
    public class LevelData
    {
        public int Level;
        public float RequiredExp;
    }

    [Serializable]
    public class LevelDataLoader : ILoader<int, LevelData>
    {
        public List<LevelData> levels = new List<LevelData>();
        public Dictionary<int, LevelData> MakeDict()
        {
            var dict = new Dictionary<int, LevelData>();
            foreach (var item in levels)
                dict.Add(item.Level, item);
            return dict;
        }
    }
    #endregion

    #region PlayerStatusData
    [Serializable]
    public class PlayerStatusData
    {
        public int DataId;
        public string Class;
        public int ClassProficiency;
        public int AdvRank;
        public int Level;
        public int NowExp;
        public int StartHP;
        public int StatusPoint;
        public int Elixir;
        public int StartStrength;
        public int StartVital;
        public int StartAgility;
        public int StartMagic;
        // ── 상세 능력치 초기값 (슬라이드 18) ──
        public float StartCritRate      = 0f;
        public float StartCritDamage    = 150f;
        public float StartAccuracy      = 80f;
        public float StartAtkSpeed      = 100f;
        public float StartPhysicalResist = 0f;
        public float StartMagicResist   = 0f;
        public float StartEvasion       = 0f;
        public float StartRecoveryRate  = 0f;
        public float StartGrowthRate    = 100f;
        public float StartDropRate      = 100f;
        public List<string> StartSkills;
        public List<int> StartSkillLevels;
    }

    [Serializable]
    public class PlayerStatusDataLoader : ILoader<int, PlayerStatusData>
    {
        public List<PlayerStatusData> playerStatuses = new List<PlayerStatusData>();
        public Dictionary<int, PlayerStatusData> MakeDict()
        {
            var dict = new Dictionary<int, PlayerStatusData>();
            foreach (var item in playerStatuses)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region DialogueScriptData
    // DialogueData001.xlsx 파싱 결과
    // 시트명(GroupKey) → 대사 라인 목록으로 접근
    [Serializable]
    public class DialogueScriptLine
    {
        public int    TextKey;
        public string Kor;
        public string Eng;
    }

    [Serializable]
    public class DialogueScriptGroup
    {
        public string GroupKey;  // 시트명 (예: "GuildQuest1Text")
        public List<DialogueScriptLine> Lines = new List<DialogueScriptLine>();
    }

    [Serializable]
    public class DialogueScriptLoader : ILoader<string, DialogueScriptGroup>
    {
        public List<DialogueScriptGroup> groups = new List<DialogueScriptGroup>();
        public Dictionary<string, DialogueScriptGroup> MakeDict()
        {
            var dict = new Dictionary<string, DialogueScriptGroup>();
            foreach (var g in groups)
                dict[g.GroupKey] = g;
            return dict;
        }
    }
    #endregion

    #region LocalizationData
    // LocalizationData001.xlsx 파싱 결과
    // TextKey → 언어별 텍스트로 접근
    [Serializable]
    public class LocalizationEntry
    {
        public string TextKey;
        public string Kor;
        public string Eng;
    }

    [Serializable]
    public class LocalizationLoader : ILoader<string, LocalizationEntry>
    {
        public List<LocalizationEntry> entries = new List<LocalizationEntry>();
        public Dictionary<string, LocalizationEntry> MakeDict()
        {
            var dict = new Dictionary<string, LocalizationEntry>();
            foreach (var e in entries)
            {
                if (!dict.ContainsKey(e.TextKey))
                    dict[e.TextKey] = e;
            }
            return dict;
        }
    }
    #endregion

    #region QuestData

    [Serializable]
    public class QuestData
    {
        public int    QuestID;
        public string QuestName;
        public QuestCompleteType CompleteType;
        public string CompleteTarget;  // ITEM_ACQUIRE/ITEM_COUNT: int DataId as string | ANIM_END: animId string | MULTI: empty
        public int    CompleteCount;   // ITEM_COUNT only
        public string FollowItemID;    // int DataId as string, empty = no Follow UI
        public string FollowItemName;  // display text
        public int    FollowCount;
        public string AnimationID;     // empty = no animation on quest start
        public bool   AnimBlocking;
        public string DialogueID;      // empty = use NPC default dialogue
    }

    [Serializable]
    public class QuestDataLoader : ILoader<int, QuestData>
    {
        public List<QuestData> quests = new List<QuestData>();
        public Dictionary<int, QuestData> MakeDict()
        {
            var dict = new Dictionary<int, QuestData>();
            foreach (var q in quests)
                dict[q.QuestID] = q;
            return dict;
        }
    }

    [Serializable]
    public class QuestConditionData
    {
        public int    QuestID;
        public int    Order;
        public QuestConditionType ConditionType;
        public string Target;  // BTN_CLICK: btnId | ITEM_ACQUIRE/ITEM_COUNT: int DataId as string
        public int    Count;   // ITEM_COUNT only
    }

    [Serializable]
    public class QuestConditionLoader : ILoader<int, List<QuestConditionData>>
    {
        public List<QuestConditionData> conditions = new List<QuestConditionData>();
        public Dictionary<int, List<QuestConditionData>> MakeDict()
        {
            var dict = new Dictionary<int, List<QuestConditionData>>();
            foreach (var c in conditions)
            {
                if (!dict.ContainsKey(c.QuestID))
                    dict[c.QuestID] = new List<QuestConditionData>();
                dict[c.QuestID].Add(c);
            }
            foreach (var list in dict.Values)
                list.Sort((a, b) => a.Order.CompareTo(b.Order));
            return dict;
        }
    }

    [Serializable]
    public class QuestNotifyData
    {
        public int    QuestID;
        public int    Order;
        public string BtnID;
    }

    [Serializable]
    public class QuestNotifyLoader : ILoader<int, List<QuestNotifyData>>
    {
        public List<QuestNotifyData> notifies = new List<QuestNotifyData>();
        public Dictionary<int, List<QuestNotifyData>> MakeDict()
        {
            var dict = new Dictionary<int, List<QuestNotifyData>>();
            foreach (var n in notifies)
            {
                if (!dict.ContainsKey(n.QuestID))
                    dict[n.QuestID] = new List<QuestNotifyData>();
                dict[n.QuestID].Add(n);
            }
            foreach (var list in dict.Values)
                list.Sort((a, b) => a.Order.CompareTo(b.Order));
            return dict;
        }
    }

    [Serializable]
    public class QuestUnlockData
    {
        public int    QuestID;
        public QuestUnlockType UnlockType;
        public string TargetID;
    }

    [Serializable]
    public class QuestUnlockLoader : ILoader<int, List<QuestUnlockData>>
    {
        public List<QuestUnlockData> unlocks = new List<QuestUnlockData>();
        public Dictionary<int, List<QuestUnlockData>> MakeDict()
        {
            var dict = new Dictionary<int, List<QuestUnlockData>>();
            foreach (var u in unlocks)
            {
                if (!dict.ContainsKey(u.QuestID))
                    dict[u.QuestID] = new List<QuestUnlockData>();
                dict[u.QuestID].Add(u);
            }
            return dict;
        }
    }

    // ─── SubQuest ─────────────────────────────────────────────────────────
    [Serializable]
    public class SubQuestData
    {
        public int                  SubQuestID;
        public string               SubQuestName;
        public SubQuestCompleteType CompleteType;
        public string               CompleteParam;  // ex. "HPPotion_Rank:1"
        public string               DialogueID;     // empty = NPC 기본 대사
    }

    [Serializable]
    public class SubQuestDataLoader : ILoader<int, SubQuestData>
    {
        public List<SubQuestData> subQuests = new List<SubQuestData>();
        public Dictionary<int, SubQuestData> MakeDict()
        {
            var dict = new Dictionary<int, SubQuestData>();
            foreach (var q in subQuests)
                dict[q.SubQuestID] = q;
            return dict;
        }
    }

    [Serializable]
    public class SubQuestNotifyData
    {
        public int                 SubQuestID;
        public string              TargetID;
        public SubQuestShowTiming  ShowTiming;
    }

    [Serializable]
    public class SubQuestNotifyLoader : ILoader<int, List<SubQuestNotifyData>>
    {
        public List<SubQuestNotifyData> notifies = new List<SubQuestNotifyData>();
        public Dictionary<int, List<SubQuestNotifyData>> MakeDict()
        {
            var dict = new Dictionary<int, List<SubQuestNotifyData>>();
            foreach (var n in notifies)
            {
                if (!dict.ContainsKey(n.SubQuestID))
                    dict[n.SubQuestID] = new List<SubQuestNotifyData>();
                dict[n.SubQuestID].Add(n);
            }
            return dict;
        }
    }

    [Serializable]
    public class SubQuestUnlockData
    {
        public int             SubQuestID;
        public string          QuestType;      // "SUB" 등 확장용 메타데이터
        public QuestUnlockType UnlockType;
        public string          TargetID;
        public string          UnlockValue;    // "TRUE" 등 확장용 메타데이터
    }

    [Serializable]
    public class SubQuestUnlockLoader : ILoader<int, List<SubQuestUnlockData>>
    {
        public List<SubQuestUnlockData> unlocks = new List<SubQuestUnlockData>();
        public Dictionary<int, List<SubQuestUnlockData>> MakeDict()
        {
            var dict = new Dictionary<int, List<SubQuestUnlockData>>();
            foreach (var u in unlocks)
            {
                if (!dict.ContainsKey(u.SubQuestID))
                    dict[u.SubQuestID] = new List<SubQuestUnlockData>();
                dict[u.SubQuestID].Add(u);
            }
            return dict;
        }
    }

    // ─── Combo (슬라이드 48) ───────────────────────────────────────────────
    [Serializable]
    public class ComboStageData
    {
        public int    Level;           // 0 ~ 6
        public string NameKey;         // "ComboLv0Text" 등 Localization 키
        public float  MaxTimer;        // 이 단계에서의 게이지 최대값(초)
        public float  HitGain;         // 타격당 추가되는 시간(초)
        public int    HitsToPromote;   // 현 단계에서 이 만큼 타격 누적 시 상위 단계 승격
        public float  AtkSpeedBuff;    // 공격속도 증가 %
        public float  DmgBoostBuff;    // 피해증폭률 %
    }

    [Serializable]
    public class ComboStageDataLoader : ILoader<int, ComboStageData>
    {
        public List<ComboStageData> stages = new List<ComboStageData>();
        public Dictionary<int, ComboStageData> MakeDict()
        {
            var dict = new Dictionary<int, ComboStageData>();
            foreach (var s in stages)
                dict[s.Level] = s;
            return dict;
        }
    }

    #endregion
}
