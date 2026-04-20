using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static Define;

// ──────────────────────────────────────────────
//  런타임 데이터 클래스
// ──────────────────────────────────────────────
[Serializable]
public class InventoryItem
{
    public int DataId;
    public int Count;
    public bool isNew;
    public bool IsNew
    {
        get => isNew;
        set
        {
            if (isNew != value)
            {
                isNew = value;
                Managers.Game.OnInventoryNewChanged?.Invoke();
                Managers.Game.SaveGame();
            }
        }
    }
}

[Serializable]
public class Equipment
{
    public int DataId;
    public EquipSlotType SlotType;
    public int EnhanceLevel;
    public int Durability;
    public bool IsEquipped;

    public Equipment() { }
    public Equipment(int dataId)
    {
        DataId = dataId;
        if (Managers.Data.EquipmentDic.TryGetValue(dataId, out var data))
        {
            SlotType   = data.SlotType;
            Durability = data.MaxDurability;
        }
    }

    public Data.EquipmentData EquipmentData => Managers.Data.EquipmentDic.TryGetValue(DataId, out var d) ? d : null;
}

[Serializable]
public class MinMonSaveData
{
    public int DataId;
    public int TameCount;
    public bool IsCompanion;
}

// ──────────────────────────────────────────────
//  세이브 데이터
// ──────────────────────────────────────────────
[Serializable]
public class GameData
{
    // 기본
    public string PlayerName = "여행자";
    public int    Level      = 1;
    public float  Exp        = 0f;
    public int    Gold        = 0;
    public int    Crystal     = 0;
    public int    StatusPoint = 0;
    public int    Elixir      = 0;

    // 환경설정
    public bool _isBGSoundOn        = true;
    public bool _isSFXOn            = true;
    public bool _isCreateAtkTextOn    = true;
    public bool _isShakeOn          = true;

    // 장비
    public List<Equipment>                    OwnedEquipments    = new List<Equipment>();
    public Dictionary<EquipSlotType, Equipment> EquippedEquipments = new Dictionary<EquipSlotType, Equipment>();

    // 소지품
    public List<InventoryItem> Inventory = new List<InventoryItem>();

    // 미니몬
    public List<MinMonSaveData> OwnedMinMons    = new List<MinMonSaveData>();
    public int                  CompanionMinMonId = -1;

    // 해금
    public List<int>    ActivationManager    = new List<int>( new int[Enum.GetValues(typeof(ActivationKey)).Length]);

    // 마물 도감
    public List<int> DiscoveredMonsterIds = new List<int>();

    // 퀘스트
    public int          CurrentQuestID = 1;
    public List<string> UnlockedBtnIds = new List<string>();
    public int          CurrentHPPotionDataId = POTION_ID_MIN + 1;
    public int          CurrentTravelAreaId = 0;

    // 마지막 장소
    public Village1Group LastVillage1Group = Village1Group.Main;


}

// ──────────────────────────────────────────────
//  GameManager
// ──────────────────────────────────────────────
public class GameManager
{
    GameData _gameData = new GameData();
    public PlayerController Player { get; set; }
    string   _path;

    // ── 재화 ──
    public int Gold
    {
        get => _gameData.Gold;
        set { _gameData.Gold = value; OnResourceChanged?.Invoke(); SaveGame(); }
    }

    public int Crystal
    {
        get => _gameData.Crystal;
        set { _gameData.Crystal = value; OnResourceChanged?.Invoke(); SaveGame(); }
    }

    public int StatusPoint
    {
        get => _gameData.StatusPoint;
        set { _gameData.StatusPoint = value; SaveGame(); }
    }

    public int Elixir
    {
        get => _gameData.Elixir;
        set { _gameData.Elixir = value; SaveGame(); }
    }

    // ── 환경설정 ──
    public bool IsBGSoundOn
    {
        get => _gameData._isBGSoundOn;
        set { _gameData._isBGSoundOn = value; SaveGame(); }
    }

    public bool IsSFXOn
    {
        get => _gameData._isSFXOn;
        set { _gameData._isSFXOn = value; SaveGame(); }
    }

    public bool IsCreateAtkTextOn
    {
        get => _gameData._isCreateAtkTextOn;
        set { _gameData._isCreateAtkTextOn = value; SaveGame(); }
    }

    public bool IsShakeOn
    {
        get => _gameData._isShakeOn;
        set { _gameData._isShakeOn = value; SaveGame(); }
    }

    // ── 캐릭터 ──
    public string PlayerName
    {
        get => _gameData.PlayerName;
        set { _gameData.PlayerName = value; }
    }

    public int Level
    {
        get => _gameData.Level;
        set { _gameData.Level = value; }
    }

    public float Exp
    {
        get => _gameData.Exp;
        set { _gameData.Exp = value; }
    }

    // ── 장비 ──
    public List<Equipment> OwnedEquipments
    {
        get => _gameData.OwnedEquipments;
        set { _gameData.OwnedEquipments = value; EquipInfoChanged?.Invoke(); }
    }

    public Dictionary<EquipSlotType, Equipment> EquippedEquipments
    {
        get => _gameData.EquippedEquipments;
        set { _gameData.EquippedEquipments = value; EquipInfoChanged?.Invoke(); }
    }

    // ── 소지품 ──
    public List<InventoryItem> Inventory => _gameData.Inventory;

    // ── 미니몬 ──
    public List<MinMonSaveData> OwnedMinMons     => _gameData.OwnedMinMons;
    public int                  CompanionMinMonId
    {
        get => _gameData.CompanionMinMonId;
        set { _gameData.CompanionMinMonId = value; SaveGame(); }
    }

    // ── 해금 ──
    public List<int> ActivationManager
    {
        get { return _gameData.ActivationManager; }
        set { _gameData.ActivationManager = value; SaveGame(); }
    }

    // ── 마물 도감 ──
    public List<int> DiscoveredMonsterIds => _gameData.DiscoveredMonsterIds;

    // ── 언어 ──
    public Define.Language Language
    {
        get => (Define.Language)PlayerPrefs.GetInt("Language", (int)Define.Language.Korean);
        set { PlayerPrefs.SetInt("Language", (int)value); OnLanguageChanged?.Invoke(); }
    }

    // ── 퀘스트 ──
    public int CurrentQuestID
    {
        get => _gameData.CurrentQuestID;
        set { 
            _gameData.CurrentQuestID = value; 
            Debug.Log($"CurrentQuestID set to {value}");
        }
    }

    public List<string> UnlockedBtnIds => _gameData.UnlockedBtnIds;
    public int CurrentHPPotionDataId
    {
        get => _gameData.CurrentHPPotionDataId <= 0 ? POTION_ID_MIN + 1 : _gameData.CurrentHPPotionDataId;
        set
        {
            int next = value <= 0 ? POTION_ID_MIN + 1 : value;
            if (_gameData.CurrentHPPotionDataId == next)
                return;

            _gameData.CurrentHPPotionDataId = next;
            SaveGame();
            OnHPPotionChanged?.Invoke(next);
        }
    }

    public int CurrentTravelAreaId
    {
        get => _gameData.CurrentTravelAreaId;
        set
        {
            if (_gameData.CurrentTravelAreaId == value)
                return;

            _gameData.CurrentTravelAreaId = value;

            // EXPLORE(배틀) 지역은 persist 하지 않음 — 게임 종료 후 재시작 시 마지막 마을로 복귀
            if (Managers.Data != null &&
                Managers.Data.TravelAreaDic != null &&
                Managers.Data.TravelAreaDic.TryGetValue(value, out var node) &&
                string.Equals(node.Category, "EXPLORE", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            SaveGame();
        }
    }

    public Village1Group LastVillage1Group
    {
        get => _gameData.LastVillage1Group;
        set { _gameData.LastVillage1Group = value; SaveGame(); }
    }

    public void AddUnlockedBtn(string btnId)
    {
        if (!_gameData.UnlockedBtnIds.Contains(btnId))
        {
            _gameData.UnlockedBtnIds.Add(btnId);
            SaveGame();
        }
    }

    // ── 이벤트 ──
    public event Action OnLanguageChanged;
    public event Action OnResourceChanged;
    public event Action EquipInfoChanged;
    public event Action<Equipment> OnEquipmentForged;
    public event Action<int> OnItemAdded;         // dataId
    public event Action<int> OnItemChanged;       // dataId (추가/제거 모두)
    public Action      OnInventoryNewChanged; // IsNew 상태 변경
    public event Action<int> OnHPPotionChanged;   // current potion dataId

    // ────────────────────────────────────────
    //  Init
    // ────────────────────────────────────────
    public void Init()
    {
        _path = Application.persistentDataPath + "/SaveData.json";

        if (LoadGame())
        {
            EnsurePotionState();
            return;
        }

        // 할때마다 처음상태로 하고싶으면 1로 바꿔야하는 것.
        PlayerPrefs.SetInt("ISFIRST", 0);
        EnsurePotionState();
        SaveGame();
    }

    // ────────────────────────────────────────
    //  장비
    // ────────────────────────────────────────
    public void EquipItem(Equipment equipment)
    {
        if (EquippedEquipments.TryGetValue(equipment.SlotType, out var old))
        {
            old.IsEquipped = false;
            EquippedEquipments.Remove(equipment.SlotType);
        }

        EquippedEquipments[equipment.SlotType] = equipment;
        equipment.IsEquipped = true;
        EquipInfoChanged?.Invoke();
        SaveGame();
    }

    public void UnEquipItem(Equipment equipment)
    {
        if (EquippedEquipments.ContainsKey(equipment.SlotType))
        {
            EquippedEquipments[equipment.SlotType].IsEquipped = false;
            EquippedEquipments.Remove(equipment.SlotType);
            EquipInfoChanged?.Invoke();
            SaveGame();
        }
    }

    public bool ForgeEquipment(Equipment equip)
    {
        int nextLevel = equip.EnhanceLevel + 1;
        Data.ForgeRecipeData recipe = null;

        foreach (var r in Managers.Data.ForgeRecipeDic.Values)
        {
            if (r.EquipDataId == equip.DataId && r.Level == nextLevel)
            {
                recipe = r;
                break;
            }
        }

        if (recipe == null)
            return false;

        if (!HasItem(recipe.RequiredMaterialId, recipe.RequiredMaterialCount))
            return false;

        RemoveItem(recipe.RequiredMaterialId, recipe.RequiredMaterialCount);
        equip.EnhanceLevel++;
        if (equip.EquipmentData != null)
            equip.Durability = equip.EquipmentData.MaxDurability;

        SaveGame();
        OnEquipmentForged?.Invoke(equip);
        return true;
    }

    // ────────────────────────────────────────
    //  소지품
    // ────────────────────────────────────────
    public void AddItem(int dataId, int count = 1)
    {
        var existing = Inventory.Find(i => i.DataId == dataId);
        if (existing != null)
        {
            existing.Count += count;
            existing.IsNew = true;
        }
        else
            Inventory.Add(new InventoryItem { DataId = dataId, Count = count, IsNew = true });
        SaveGame();
        OnItemAdded?.Invoke(dataId);
        OnItemChanged?.Invoke(dataId);
    }

    public void ClearItemNew(int dataId)
    {
        var item = Inventory.Find(i => i.DataId == dataId);
        if (item == null || !item.IsNew) return;
        item.IsNew = false;
        OnInventoryNewChanged?.Invoke();
    }

    public void RemoveItem(int dataId, int count = 1)
    {
        var existing = Inventory.Find(i => i.DataId == dataId);
        if (existing == null) return;
        existing.Count -= count;
        if (existing.Count <= 0)
            Inventory.Remove(existing);
        SaveGame();
        OnItemChanged?.Invoke(dataId);
    }

    public bool HasItem(int dataId, int count = 1)
    {
        var existing = Inventory.Find(i => i.DataId == dataId);
        return existing != null && existing.Count >= count;
    }

    public int GetItemCount(int dataId)
    {
        var existing = Inventory.Find(i => i.DataId == dataId);
        return existing?.Count ?? 0;
    }

    public int GetPlayerMaxHp()
    {
        if (Player != null && Player.MaxHp > 0f)
            return Mathf.RoundToInt(Player.MaxHp);

        Data.PlayerStatusData status = null;
        if (Managers.Data.PlayerStatusDic.TryGetValue(Level, out var levelStatus))
            status = levelStatus;
        else
        {
            foreach (var entry in Managers.Data.PlayerStatusDic.Values)
            {
                status = entry;
                break;
            }
        }

        return status?.StartHP ?? 1000;
    }

    public int GetNextHPPotionDataId(int currentDataId)
    {
        int nextId = -1;

        foreach (var dataId in Managers.Data.HPPotionDic.Keys)
        {
            if (dataId <= currentDataId)
                continue;

            if (nextId == -1 || dataId < nextId)
                nextId = dataId;
        }

        return nextId;
    }

    public bool CanCraftHPPotion(int currentDataId)
    {
        int nextDataId = GetNextHPPotionDataId(currentDataId);
        if (nextDataId < 0 || !Managers.Data.HPPotionDic.TryGetValue(nextDataId, out var next))
            return false;

        for (int i = 0; i < next.NeededItemIDs.Count; i++)
        {
            int itemId = next.NeededItemIDs[i];
            int required = i < next.NeededItemAmounts.Count ? next.NeededItemAmounts[i] : 0;
            if (!HasItem(itemId, required))
                return false;
        }

        return true;
    }

    public bool TryCraftHPPotion(int currentDataId, out int craftedDataId)
    {
        craftedDataId = currentDataId;

        int nextDataId = GetNextHPPotionDataId(currentDataId);
        if (nextDataId < 0 || !Managers.Data.HPPotionDic.TryGetValue(nextDataId, out var next))
            return false;

        if (!CanCraftHPPotion(currentDataId))
            return false;

        for (int i = 0; i < next.NeededItemIDs.Count; i++)
        {
            int itemId = next.NeededItemIDs[i];
            int required = i < next.NeededItemAmounts.Count ? next.NeededItemAmounts[i] : 0;
            if (required > 0)
                RemoveItem(itemId, required);
        }

        CurrentHPPotionDataId = nextDataId;
        craftedDataId = nextDataId;
        return true;
    }

    // ────────────────────────────────────────
    //  세이브 / 로드
    // ────────────────────────────────────────
    public void SaveGame()
    {
        if (_path == null) return;
        File.WriteAllText(_path, JsonConvert.SerializeObject(_gameData));
    }

    public bool LoadGame()
    {
        if (PlayerPrefs.GetInt("ISFIRST", 1) == 1)
        {
            string path = Application.persistentDataPath + "/SaveData.json";
            if (File.Exists(path))
                File.Delete(path);

            FirstSetting();

            return false;
        }

        if (!File.Exists(_path))
            return false;

        GameData data = JsonConvert.DeserializeObject<GameData>(File.ReadAllText(_path));
        if (data != null)
            _gameData = data;

        EnsurePotionState();

        return true;
    }

    public void FirstSetting()
    {
        // 1이면 켜야하는 것.
        ActivationManager[(int)ActivationKey.Village1GuildVFX] = 1;
    }

    void EnsurePotionState()
    {
        if (_gameData.CurrentHPPotionDataId <= 0)
            _gameData.CurrentHPPotionDataId = POTION_ID_MIN + 1;
    }
}
