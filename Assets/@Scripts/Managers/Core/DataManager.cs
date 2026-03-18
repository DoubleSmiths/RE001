using Data;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic ;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, CreatureData>      CreatureDic       { get; private set; } = new Dictionary<int, CreatureData>();
    public Dictionary<int, EquipmentData>     EquipmentDic      { get; private set; } = new Dictionary<int, EquipmentData>();
    public Dictionary<int, EquipEnhanceData>  EquipEnhanceDic   { get; private set; } = new Dictionary<int, EquipEnhanceData>();
    public Dictionary<int, SkillData>         SkillDic          { get; private set; } = new Dictionary<int, SkillData>();
    public Dictionary<int, ItemData>          ItemDic           { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, AreaData>          AreaDic           { get; private set; } = new Dictionary<int, AreaData>();
    public Dictionary<int, QuestData>         QuestDic          { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, DialogueData>      DialogueDic       { get; private set; } = new Dictionary<int, DialogueData>();
    public Dictionary<int, MinMonData>        MinMonDic         { get; private set; } = new Dictionary<int, MinMonData>();
    public Dictionary<int, ForgeRecipeData>   ForgeRecipeDic    { get; private set; } = new Dictionary<int, ForgeRecipeData>();
    public Dictionary<int, CraftRecipeData>   CraftRecipeDic    { get; private set; } = new Dictionary<int, CraftRecipeData>();
    public Dictionary<int, LevelData>         LevelDic          { get; private set; } = new Dictionary<int, LevelData>();

    public void Init()
    {
        //CreatureDic     = LoadJson<CreatureDataLoader,     int, CreatureData>    ("CreatureData").MakeDict();
        //EquipmentDic    = LoadJson<EquipmentDataLoader,    int, EquipmentData>   ("EquipmentData").MakeDict();
        //EquipEnhanceDic = LoadJson<EquipEnhanceDataLoader, int, EquipEnhanceData>("EquipEnhanceData").MakeDict();
        //SkillDic        = LoadJson<SkillDataLoader,        int, SkillData>       ("SkillData").MakeDict();
        //ItemDic         = LoadJson<ItemDataLoader,         int, ItemData>        ("ItemData").MakeDict();
        //AreaDic         = LoadJson<AreaDataLoader,         int, AreaData>        ("AreaData").MakeDict();
        //QuestDic        = LoadJson<QuestDataLoader,        int, QuestData>       ("QuestData").MakeDict();
        //DialogueDic     = LoadJson<DialogueDataLoader,     int, DialogueData>    ("DialogueData").MakeDict();
        //MinMonDic       = LoadJson<MinMonDataLoader,       int, MinMonData>      ("MinMonData").MakeDict();
        //ForgeRecipeDic  = LoadJson<ForgeRecipeDataLoader,  int, ForgeRecipeData> ("ForgeRecipeData").MakeDict();
        //CraftRecipeDic  = LoadJson<CraftRecipeDataLoader,  int, CraftRecipeData> ("CraftRecipeData").MakeDict();
        //LevelDic        = LoadJson<LevelDataLoader,        int, LevelData>       ("LevelData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
