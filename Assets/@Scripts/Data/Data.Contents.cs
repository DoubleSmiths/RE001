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
        // 슬8: 미니몬 기록 [서식지] — LocalizationData의 TextKey를 저장(값까지 한/영 지원).
        // 예: "BreezingWindForestNameText" → 산들바람 숲 / BREEZEWOOD FOREST. 비어있으면 "-".
        public string Habitat = "";
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

        // ── 슬라이드 53/54 추가 필드 ──
        public float DmgAmp = 0f;            // 피해증폭 % (식: (1 + DmgAmp/100))
        public string Rarity = "Common";     // CreatureRarity 문자열 ("Common"/"Rare"/"Boss")
        public float MonsterAttackDelay = 1.5f; // 몬스터만 사용. Attack Anim 재생 전 대기 시간 (초)
        public float EncounterRate = 1f;     // 0~1 — 조우 확률
        public float InterestRate = 0f;      // 0~1 — 관심 확률
        public int MaxKillCount = 100;       // 도감 해금용 최대 킬 카운트

        // 몬스터 spawn 시 ground 높이 (sprite pivot 위치에 따라 prefab별로 다를 수 있음).
        // Slime 기준 2.3 — 다른 monster 추가 시 이 값으로 발 위치 조정.
        public float SpawnY = 2.3f;

        // 데미지 폰트가 transform.position 기준 위쪽으로 띄울 거리 (head 위치).
        // monster sprite 높이마다 다름. 기본 1.0 — 작은 monster는 0.5, 큰 monster는 2.0 등.
        public float DamageFontYOffset = 2.5f;

        // ── 슬라이드 79: 미니몬 고유기술 ──
        // 공명도 100% 달성 시 플레이어 기술로 영구 습득되는 몬스터별 고유기술.
        // 슬라이드 71의 4개 완성도 스킬과는 별개. 비어있으면 코드에서 "고유기술 #{id}" 폴백.
        public string UniqueSkillName = "";
        public string UniqueSkillDesc = "";
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
        public string Name;          // 한국어 이름 (기본/폴백)
        public string Description;   // 한국어 설명 (기본/폴백)
        public string NameEng;       // 영문 이름 (없으면 Name 폴백)
        public string DescEng;       // 영문 설명 (없으면 Description 폴백)
        public string IconLabel;
        public SkillCategory Category;
        public SkillActiveType SkillActiveType;
        public string StatType;        // 버프 대상: PhysicalAtk/MagicAtk/CritRate/MaxHp/AtkSpeed/Accuracy/Def/MaxMemory
        public float StatBonus;        // 버프 수치
        public int MaxLevel;
        public int MemoryCapacity;     // 슬38: 기억력 점유량. 0이면 [재능](항시 적용, 점유 없음)

        // ── 슬39: 전투 발동조건 (없으면 None = 패시브) ──
        public AbilityTrigger TriggerType;                                       // None / Probability / Time
        public List<AbilityCondition> Conditions = new List<AbilityCondition>(); // 확률 발동 조건(1개 이상)
        public float ProcChance = 100f;                                          // 확률 발동 n%
        public float Cooldown;                                                   // 시간 발동 재사용 대기(초)
        public AbilityContext ActiveContext;                                     // 시간 발동 맥락(InBattle/OutOfBattle/Always)
        public float ConditionValue;                                             // 조건 파라미터(LowHp = HP% 임계)
        // 발동 효과
        public AbilityEffect Effect;                                             // Heal / BonusDamage / Buff
        public string EffectStat;                                                // Buff 대상 스탯(PhysicalAtk 등)
        public float EffectValue;                                                // 효과 수치
        public float EffectDuration;                                             // Buff 지속(초), 0=전투 종료까지
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

    #region TravelNodeData
    [Serializable]
    public class TravelNodeData
    {
        public int Id;
        public int ParentId;
        public string Category;     // "TOWN" | "EXPLORE"
        public string Name;
        public string Sprite;
        public string AreaType;     // "Village1" | "Battle" — 행선지만 채움
        public string ClickBtnId;
        public string EnterBtnId;   // 행선지만 채움
    }

    [Serializable]
    public class TravelNodeDataLoader : ILoader<int, TravelNodeData>
    {
        public List<TravelNodeData> nodes = new List<TravelNodeData>();
        public Dictionary<int, TravelNodeData> MakeDict()
        {
            var dict = new Dictionary<int, TravelNodeData>();
            foreach (var item in nodes)
                dict.Add(item.Id, item);
            return dict;
        }
    }

    [Serializable]
    public class TravelConditionData
    {
        public int NodeId;
        public string Type;         // "QUEST_CLEAR" | "BTN_UNLOCKED" | "ITEM_REQUIRED" | "LEVEL_REQUIRED"
        public string Target;
        public int Value;
        public string FailMessage;
    }

    [Serializable]
    public class TravelConditionDataLoader : ILoader<int, List<TravelConditionData>>
    {
        public List<TravelConditionData> conditions = new List<TravelConditionData>();
        public Dictionary<int, List<TravelConditionData>> MakeDict()
        {
            var dict = new Dictionary<int, List<TravelConditionData>>();
            foreach (var item in conditions)
            {
                if (!dict.ContainsKey(item.NodeId))
                    dict[item.NodeId] = new List<TravelConditionData>();
                dict[item.NodeId].Add(item);
            }
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
    // VS(COMBOSYSTEM GameObject) 변수 구조와 1:1 대응:
    //   ComboName KOR/ENG → Localization(NameKey)
    //   Combo REQ        → HitsRequired (이 rank에 도달하기 위한 최소 누적 타격 수)
    //   Combo Time       → MaxTimer (이 rank 게이지 최대)
    //   Combo Time Inc   → HitGain  (타격당 타이머 추가)
    //   ASBoost          → AtkSpeedBuff (%)
    //   DMGBoost         → DmgBoostBuff (%)
    [Serializable]
    public class ComboStageData
    {
        public int    Level;           // 0 ~ 6
        public string NameKey;         // "ComboLv0Text" 등 Localization 키
        public int    HitsRequired;    // 누적 타격 수가 이 값 이상이면 이 Rank에 도달
        public float  MaxTimer;        // 이 단계에서의 게이지 최대값(초)
        public float  HitGain;         // 타격당 추가되는 시간(초)
        public float  AtkSpeedBuff;    // 공격속도 증가 %
        public float  DmgBoostBuff;    // 피해증폭률 %
        public string ColorHex;        // Rank별 텍스트/게이지 색 (#RRGGBB or #RRGGBBAA)
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
