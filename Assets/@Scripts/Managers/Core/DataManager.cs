using Data;
using Newtonsoft.Json;
using UnityEngine;

public interface ILoader<Key, Value>
{
    System.Collections.Generic.Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public System.Collections.Generic.Dictionary<int, Data.CreatureData>      CreatureDic       { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.CreatureData>();
    public System.Collections.Generic.Dictionary<int, Data.EquipmentData>     EquipmentDic      { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.EquipmentData>();
    public System.Collections.Generic.Dictionary<int, Data.EquipEnhanceData>  EquipEnhanceDic   { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.EquipEnhanceData>();
    public System.Collections.Generic.Dictionary<int, Data.SkillData>         SkillDic          { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.SkillData>();
    public System.Collections.Generic.Dictionary<int, Data.ItemData>          ItemDic           { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.ItemData>();
    public System.Collections.Generic.Dictionary<int, Data.AreaData>          AreaDic           { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.AreaData>();
    public System.Collections.Generic.Dictionary<int, Data.QuestData>         QuestDic          { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.QuestData>();
    public System.Collections.Generic.Dictionary<int, Data.DialogueData>      DialogueDic       { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.DialogueData>();
    public System.Collections.Generic.Dictionary<int, Data.MinMonData>        MinMonDic         { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.MinMonData>();
    public System.Collections.Generic.Dictionary<int, Data.ForgeRecipeData>   ForgeRecipeDic    { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.ForgeRecipeData>();
    public System.Collections.Generic.Dictionary<int, Data.CraftRecipeData>   CraftRecipeDic    { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.CraftRecipeData>();
    public System.Collections.Generic.Dictionary<int, Data.LevelData>         LevelDic          { get; private set; } = new System.Collections.Generic.Dictionary<int, Data.LevelData>();

    public void Init()
    {
        CreatureDic     = LoadJson<Data.CreatureDataLoader,     int, Data.CreatureData>    ("CreatureData").MakeDict();
        EquipmentDic    = LoadJson<Data.EquipmentDataLoader,    int, Data.EquipmentData>   ("EquipmentData").MakeDict();
        EquipEnhanceDic = LoadJson<Data.EquipEnhanceDataLoader, int, Data.EquipEnhanceData>("EquipEnhanceData").MakeDict();
        SkillDic        = LoadJson<Data.SkillDataLoader,        int, Data.SkillData>       ("SkillData").MakeDict();
        ItemDic         = LoadJson<Data.ItemDataLoader,         int, Data.ItemData>        ("ItemData").MakeDict();
        AreaDic         = LoadJson<Data.AreaDataLoader,         int, Data.AreaData>        ("AreaData").MakeDict();
        QuestDic        = LoadJson<Data.QuestDataLoader,        int, Data.QuestData>       ("QuestData").MakeDict();
        DialogueDic     = LoadJson<Data.DialogueDataLoader,     int, Data.DialogueData>    ("DialogueData").MakeDict();
        MinMonDic       = LoadJson<Data.MinMonDataLoader,       int, Data.MinMonData>      ("MinMonData").MakeDict();
        ForgeRecipeDic  = LoadJson<Data.ForgeRecipeDataLoader,  int, Data.ForgeRecipeData> ("ForgeRecipeData").MakeDict();
        CraftRecipeDic  = LoadJson<Data.CraftRecipeDataLoader,  int, Data.CraftRecipeData> ("CraftRecipeData").MakeDict();
        LevelDic        = LoadJson<Data.LevelDataLoader,        int, Data.LevelData>       ("LevelData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
