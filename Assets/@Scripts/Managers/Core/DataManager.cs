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
    public Dictionary<int, ItemData>          RequisiteItemDic  { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, HPPotionData>      HPPotionDic       { get; private set; } = new Dictionary<int, HPPotionData>();
    public Dictionary<int, SpeedPotionData>   SpeedPotionDic    { get; private set; } = new Dictionary<int, SpeedPotionData>();
    public Dictionary<int, AreaData>          AreaDic           { get; private set; } = new Dictionary<int, AreaData>();
    public Dictionary<int, TravelNodeData>                TravelNodeDic       { get; private set; } = new Dictionary<int, TravelNodeData>();
    public Dictionary<int, List<TravelConditionData>>     TravelConditionDic  { get; private set; } = new Dictionary<int, List<TravelConditionData>>();
    public Dictionary<int, MinMonData>        MinMonDic         { get; private set; } = new Dictionary<int, MinMonData>();
    public Dictionary<int, ForgeRecipeData>   ForgeRecipeDic    { get; private set; } = new Dictionary<int, ForgeRecipeData>();
    public Dictionary<int, CraftRecipeData>   CraftRecipeDic    { get; private set; } = new Dictionary<int, CraftRecipeData>();
    public Dictionary<int, LevelData>          LevelDic          { get; private set; } = new Dictionary<int, LevelData>();
    public Dictionary<int, PlayerStatusData>   PlayerStatusDic   { get; private set; } = new Dictionary<int, PlayerStatusData>();
    public Dictionary<string, DialogueScriptGroup>       DialogueScriptDic { get; private set; } = new Dictionary<string, DialogueScriptGroup>();
    public Dictionary<string, LocalizationEntry>         LocalizationDic   { get; private set; } = new Dictionary<string, LocalizationEntry>();
    public Dictionary<int, QuestData>                QuestDic          { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, List<QuestConditionData>> QuestConditionDic { get; private set; } = new Dictionary<int, List<QuestConditionData>>();
    public Dictionary<int, List<QuestNotifyData>>    QuestNotifyDic    { get; private set; } = new Dictionary<int, List<QuestNotifyData>>();
    public Dictionary<int, List<QuestUnlockData>>    QuestUnlockDic    { get; private set; } = new Dictionary<int, List<QuestUnlockData>>();
    public Dictionary<int, SubQuestData>                SubQuestDic         { get; private set; } = new Dictionary<int, SubQuestData>();
    public Dictionary<int, List<SubQuestNotifyData>>    SubQuestNotifyDic   { get; private set; } = new Dictionary<int, List<SubQuestNotifyData>>();
    public Dictionary<int, List<SubQuestUnlockData>>    SubQuestUnlockDic   { get; private set; } = new Dictionary<int, List<SubQuestUnlockData>>();
    public Dictionary<int, ComboStageData>              ComboStageDic       { get; private set; } = new Dictionary<int, ComboStageData>();
    public Dictionary<int, BuffData>                    BuffDic             { get; private set; } = new Dictionary<int, BuffData>();

    public void Init()
    {
        CreatureDic       = LoadJson<CreatureDataLoader,      int,    CreatureData>       ("CreatureData").MakeDict();
        EquipmentDic      = LoadJson<EquipmentDataLoader,     int,    EquipmentData>      ("EquipmentData").MakeDict();   // 슬49: 장비 데이터 로드 활성화(필요 장비 UI)
        EquipEnhanceDic   = LoadJson<EquipEnhanceDataLoader,  int,    EquipEnhanceData>   ("EquipEnhanceData").MakeDict();
        SkillDic          = LoadJson<SkillDataLoader,         int,    SkillData>          ("SkillData").MakeDict();
        ItemDic           = LoadJson<ItemDataLoader,          int,    ItemData>           ("InventoryItem").MakeDict();
        RequisiteItemDic  = LoadJson<ItemDataLoader,          int,    ItemData>           ("RequisiteItem").MakeDict();
        HPPotionDic       = LoadJson<HPPotionDataLoader,      int,    HPPotionData>       ("HPPotion").MakeDict();
        SpeedPotionDic    = LoadJson<SpeedPotionDataLoader,   int,    SpeedPotionData>    ("SpeedPotion").MakeDict();
        //AreaDic           = LoadJson<AreaDataLoader,          int,    AreaData>           ("AreaData").MakeDict();
        TravelNodeDic       = LoadJson<TravelNodeDataLoader,      int, TravelNodeData>              ("TravelNodeData").MakeDict();
        TravelConditionDic  = LoadJson<TravelConditionDataLoader, int, List<TravelConditionData>>   ("TravelConditionData").MakeDict();
        MinMonDic         = LoadJson<MinMonDataLoader,        int,    MinMonData>         ("MinMonData").MakeDict();
        ForgeRecipeDic    = LoadJson<ForgeRecipeDataLoader,   int,    ForgeRecipeData>    ("ForgeRecipeData").MakeDict();
        //CraftRecipeDic    = LoadJson<CraftRecipeDataLoader,   int,    CraftRecipeData>    ("CraftRecipeData").MakeDict();
        LevelDic          = LoadJson<LevelDataLoader,         int,    LevelData>          ("LevelData").MakeDict();
        PlayerStatusDic   = LoadJson<PlayerStatusDataLoader,  int,    PlayerStatusData>   ("PlayerStatusData").MakeDict();
        DialogueScriptDic = LoadJson<DialogueScriptLoader,    string, DialogueScriptGroup>("DialogueScriptData").MakeDict();
        LocalizationDic   = LoadJson<LocalizationLoader,      string, LocalizationEntry>  ("LocalizationData").MakeDict();
        QuestDic          = LoadJson<QuestDataLoader,      int, QuestData>                ("QuestData").MakeDict();
        QuestConditionDic = LoadJson<QuestConditionLoader, int, List<QuestConditionData>> ("QuestConditionData").MakeDict();
        QuestNotifyDic    = LoadJson<QuestNotifyLoader,    int, List<QuestNotifyData>>    ("QuestNotifyData").MakeDict();
        QuestUnlockDic    = LoadJson<QuestUnlockLoader,    int, List<QuestUnlockData>>    ("QuestUnlockData").MakeDict();
        SubQuestDic       = LoadJson<SubQuestDataLoader,   int, SubQuestData>             ("SubQuestMainData").MakeDict();
        SubQuestNotifyDic = LoadJson<SubQuestNotifyLoader, int, List<SubQuestNotifyData>> ("SubQuestNotifyData").MakeDict();
        SubQuestUnlockDic = LoadJson<SubQuestUnlockLoader, int, List<SubQuestUnlockData>> ("SubQuestUnlockData").MakeDict();
        ComboStageDic     = LoadJson<ComboStageDataLoader, int, ComboStageData>          ("ComboData").MakeDict();
        BuffDic           = LoadJson<BuffDataLoader,       int, BuffData>                ("BuffData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        if (textAsset == null)
        {
            UnityEngine.Debug.LogError($"[DataManager] JSON load 실패: '{path}' — Addressable PreLoad 그룹에 등록돼있는지 확인 필요");
            return System.Activator.CreateInstance<Loader>();
        }
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
