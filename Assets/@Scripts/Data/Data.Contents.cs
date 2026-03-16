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
        public float Atk;
        public float Def;
        public float MoveSpeed;
        public float TotalExp;
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

    #region AreaData
    [Serializable]
    public class AreaData
    {
        public int DataId;
        public string Name;
        public string Description;
        public string BackgroundLabel;
        public List<int> MonsterIds = new List<int>();
        public int UnlockQuestId;
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

    #region QuestData
    [Serializable]
    public class QuestData
    {
        public int DataId;
        public QuestType Type;
        public int TargetId;
        public int TargetCount;
        public int RewardGold;
        public int RewardCrystal;
        public string UnlockTabName;
        public int UnlockAreaId;
        public int NextQuestId;
    }

    [Serializable]
    public class QuestDataLoader : ILoader<int, QuestData>
    {
        public List<QuestData> quests = new List<QuestData>();
        public Dictionary<int, QuestData> MakeDict()
        {
            var dict = new Dictionary<int, QuestData>();
            foreach (var item in quests)
                dict.Add(item.DataId, item);
            return dict;
        }
    }
    #endregion

    #region DialogueData
    [Serializable]
    public class DialogueLine
    {
        public string Text;
        public int UnlockQuestId;
    }

    [Serializable]
    public class DialogueData
    {
        public int DataId;
        public string NpcName;
        public string NpcIllustration;
        public List<DialogueLine> Lines = new List<DialogueLine>();
    }

    [Serializable]
    public class DialogueDataLoader : ILoader<int, DialogueData>
    {
        public List<DialogueData> dialogues = new List<DialogueData>();
        public Dictionary<int, DialogueData> MakeDict()
        {
            var dict = new Dictionary<int, DialogueData>();
            foreach (var item in dialogues)
                dict.Add(item.DataId, item);
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
        public int UnlockQuestId;
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
}
