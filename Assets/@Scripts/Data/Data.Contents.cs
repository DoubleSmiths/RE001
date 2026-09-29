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
        // 범용 몬스터 프리팹(PrefabLabel="Monster") 사용 시, 외형/애니를 데이터로 분리하기 위한
        // 애니메이터 컨트롤러 Addressable 주소. 비어있으면 프리팹에 박힌 기본 컨트롤러 유지(슬라임).
        // 예: "Goblin" → Goblin.controller 런타임 교체. (MonsterController.SetInfo에서 적용)
        public string AnimController = "";
        // 슬8: 미니몬 기록 [서식지] — LocalizationData의 TextKey를 저장(값까지 한/영 지원).
        // 예: "BreezingWindForestNameText" → 산들바람 숲 / BREEZEWOOD FOREST. 비어있으면 "-".
        // (범용 스폰 풀 필터에도 사용: 현재 전투 지역의 Habitat과 일치하는 몬스터만 등장)
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

        // 몬스터별 SFX (데이터 드리븐). 어드레서블 클립 주소(파일명). 비우면 기본 슬라임 사운드.
        //  MonsterController가 공격/피격/사망 시 재생. 신규 몬스터는 데이터로 자기 사운드를 지정.
        public string AttackSfx = "Slime_Attack";
        public string OnHitSfx  = "Slime_OnHit";
        public string DieSfx    = "Slime_Die";
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
        public string Name;          // LocalizationData 키 (예 Skill5008NameText). 표시 시 Util.Localize로 한/영.
        public string Description;   // LocalizationData 키 (예 Skill5008DescText).
        public string IconLabel;
        public SkillCategory Category;
        public SkillActiveType SkillActiveType;
        public string StatType;        // 버프 대상: PhysicalAtk/MagicAtk/CritRate/MaxHp/AtkSpeed/Accuracy/Def/MaxMemory
        public float StatBonus;        // 버프 수치
        // 다중 스탯(병렬 리스트) — 비어있으면 StatType/StatBonus 단일 fallback (RoleTypes와 동일 하위호환 패턴)
        public List<string> StatTypes  = new List<string>();
        public List<float>  StatBonuses = new List<float>();
        public int MaxLevel;
        public int MemoryCapacity;     // 슬38: 기억력 점유량. 0이면 [재능](항시 적용, 점유 없음)

        // ── 능력 정보창 #4/#5/#6 ──
        public string Story;             // 능력 이야기(flavor) — LocalizationData 키(예 Skill5002StoryText). 비면 표시 안 함.
        public AbilitySource Source;     // 능력 출처(획득 경로). 기본 Unlimited. (#5 하단 UI 분기)
        public int RequiredMinionId;     // Source=Minion일 때 게이팅 미니몬 CreatureId. 0이면 (id-4000)로 파생.

        // ── 슬49/50/51: 출처별 요구 데이터 ──
        public int RequiredEquipId;      // 슬49 Source=Equip일 때 요구 장비 DataId (장비 시스템 가동 후 사용)
        public int RequiredItemId;       // 슬50 Source=Npc일 때 요구 재료 ItemData DataId
        public int RequiredItemCount;    // 슬50 요구 재료 개수
        public string NpcLocation;       // 슬51 버튼1: Source=Npc일 때 학습 NPC가 있는 장소

        // 슬44: 능력 종류(여러 개 가능). 비어있으면 StatType→단일 Role 파생. 값: Skill/Spell/Def/Util
        public List<string> RoleTypes = new List<string>();

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
        public float EffectValue;                                                // 효과 수치 (발동 시 능력 단계 × 배율 적용)
        public float EffectDuration;                                             // Buff 지속(초), 0=전투 종료까지
        public string EffectVfxLabel;                                            // 발동 VFX 프리팹 주소. 비면 효과별 기본 매핑
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

    #region BuffData
    // 버프(강화/물약) 표시 카탈로그 — 좌상단 버프 아이콘(UI_Buff)이 참조한다.
    // 현재는 신속(스피드) 버프뿐이지만, 버프 종류가 늘어나면 여기에 행을 추가한다.
    [Serializable]
    public class BuffData
    {
        public int DataId;
        public string Name;        // LocalizationData 키
        public string Description; // LocalizationData 키
        public string IconLabel;   // 버프 아이콘 스프라이트 주소
        public bool Timed;         // true=지속시간형(게이지/점멸/FadeOut) / false=인게임지속(정적)
        public float Duration;     // 기본 지속(초), Timed=false면 미사용
    }

    [Serializable]
    public class BuffDataLoader : ILoader<int, BuffData>
    {
        public List<BuffData> buffs = new List<BuffData>();
        public Dictionary<int, BuffData> MakeDict()
        {
            var dict = new Dictionary<int, BuffData>();
            foreach (var b in buffs)
                dict.Add(b.DataId, b);
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
        public string PetAnimLabel;   // 동행(펫) 전용 AnimatorController Addressable 주소.
                                      //  비면 몬스터 프리팹 기본 컨트롤러 유지. (펫 애니는 몬스터 anim 폴더 하위 Pet_* 에 제작)
        public int MonsterDataId;
        public float BaseAtk;
        public float BaseAtkSpeed;
        public float BaseCritRate;
        public MinMonGrade AtkGrade;
        public MinMonGrade AtkSpeedGrade;
        public MinMonGrade CritGrade;
        public int MaxTameCount;
        public float CatchChance;
        // 첫 동행 체험까지의 대기만 제한한다. 0이면 기존 확률을 사용한다.
        public int FirstInterestKillCount;
        public float FirstCatchChance;
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
    // 대장간 장비 제작 레시피 (_tools/gen_forge_recipes.py 생성).
    //  장비 60종의 유일한 획득 경로 — 재료 3종 + 골드 + 요구 레벨.
    //  재료를 3종 요구하는 이유: 한 종류만 갈면 나머지 29종이 또 쓸 데 없는 아이템이 된다.
    public class ForgeRecipeData
    {
        public int DataId;
        public int EquipDataId;     // 결과 장비
        public int ReqLevel;        // 요구 플레이어 레벨
        public int Gold;
        public int Mat1Id, Mat1Count;
        public int Mat2Id, Mat2Count;
        public int Mat3Id, Mat3Count;
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

    #region BalanceConfig
    // v2 밸런스 스칼라 값(키·값). 코드에 수치를 박지 않고 여기서 읽는다 — 키는 Define.BalanceKey (ADR0030).
    [Serializable]
    public class BalanceConfigData
    {
        public string Key;
        public float  Value;
        public string Desc;
    }

    [Serializable]
    public class BalanceConfigLoader : ILoader<string, BalanceConfigData>
    {
        public List<BalanceConfigData> items = new List<BalanceConfigData>();
        public Dictionary<string, BalanceConfigData> MakeDict()
        {
            var dict = new Dictionary<string, BalanceConfigData>();
            foreach (var d in items)
                if (!string.IsNullOrEmpty(d.Key)) dict[d.Key] = d;
            return dict;
        }
    }
    #endregion

    #region MissionData / ProductData (v2 미션·출석·인앱 상품 — _tools/gen_missions.py, ADR0030)
    [Serializable]
    public class MissionData
    {
        public int    DataId;
        public string Kind;          // Daily / Weekly / Guide / Attendance
        public int    Order;
        public string Event;         // Define.MissionEvent 이름
        public int    Goal;
        public string RewardType;    // Crystal / Mandrake / Gold
        public int    RewardValue;
        public string TitleKey;      // {0} = Goal
    }

    [Serializable]
    public class MissionDataLoader : ILoader<int, MissionData>
    {
        public List<MissionData> missions = new List<MissionData>();
        public Dictionary<int, MissionData> MakeDict()
        {
            var dict = new Dictionary<int, MissionData>();
            foreach (var m in missions) dict[m.DataId] = m;
            return dict;
        }
    }

    [Serializable]
    public class ProductData
    {
        public int    DataId;
        public string ProductKey;    // 스토어 상품 ID
        public string Kind;          // Pack / StepUp / Monthly / AdFree / Crystal
        public int    PriceKRW;
        public int    Crystal;
        public int    Mandrake;
        public string NameKey;
    }

    [Serializable]
    public class ProductDataLoader : ILoader<int, ProductData>
    {
        public List<ProductData> products = new List<ProductData>();
        public Dictionary<int, ProductData> MakeDict()
        {
            var dict = new Dictionary<int, ProductData>();
            foreach (var p in products) dict[p.DataId] = p;
            return dict;
        }
    }
    #endregion

    #region RelicData (v2 유물 30종 — _tools/gen_relics.py, ADR0035)
    [Serializable]
    public class RelicData
    {
        public int    DataId;
        public string NameKey;
        public string Grade;       // Common / Rare / Epic / Legendary
        public string StatType;    // Atk / Hp / Def / CritDmg / Gold / Exp / Drop / Interest
        public float  Base;        // 1레벨 값
        public float  PerLevel;    // 레벨당 값
        public string IconLabel;
    }

    [Serializable]
    public class RelicDataLoader : ILoader<int, RelicData>
    {
        public List<RelicData> relics = new List<RelicData>();
        public Dictionary<int, RelicData> MakeDict()
        {
            var dict = new Dictionary<int, RelicData>();
            foreach (var r in relics) dict[r.DataId] = r;
            return dict;
        }
    }
    #endregion

    #region ShopItemData
    [Serializable]
    // 마을 상점 판매 목록 (_tools/gen_shop_items.py 생성).
    //  골드 소비처가 강화(GameManager:888)와 주문제작(:1202) 둘뿐이라 사냥으로 쌓인 골드가
    //  갈 곳이 없었다. 상점은 **장비를 팔지 않는다** — 장비 획득은 대장간 제작으로 몰아야
    //  재료가 쌓이기만 하는 문제가 풀리고, 두 건물이 서로 역할을 먹지 않는다.
    public class ShopItemData
    {
        public int DataId;
        public int ItemId;          // 파는 물건 (재료/물약/주문서 — ItemDic 또는 포션 사전 기준)
        public int Price;           // 구매가(골드)
        public int SellPrice;       // 되팔기가. 0이면 되팔 수 없다
        public int UnlockLevel;     // 이 레벨부터 목록에 뜬다
    }

    [Serializable]
    public class ShopItemDataLoader : ILoader<int, ShopItemData>
    {
        public List<ShopItemData> items = new List<ShopItemData>();
        public Dictionary<int, ShopItemData> MakeDict()
        {
            var dict = new Dictionary<int, ShopItemData>();
            foreach (var item in items)
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
        public int   StartDef           = 0;    // 레벨별 방어력 (StartDropRate 뒤 신규 컬럼)
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
    
        // 이 알림이 **서브퀘스트**에서 왔는가. 느낌표 색이 여기서 갈린다
        //  (노랑 FFE300 = 메인 / 초록 14FF00 = 서브). 시트에는 없는 런타임 전용 값이라
        //  직렬화에서 뺀다 — JSON에 없는 필드를 넣으면 파서가 어긋난다.
        [NonSerialized] public bool IsSubQuest;
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
