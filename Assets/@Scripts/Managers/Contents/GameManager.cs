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
    public int    Gold       = 0;
    public int    Crystal    = 0;

    // 장비
    public List<Equipment>                    OwnedEquipments    = new List<Equipment>();
    public Dictionary<EquipSlotType, Equipment> EquippedEquipments = new Dictionary<EquipSlotType, Equipment>();

    // 소지품
    public List<InventoryItem> Inventory = new List<InventoryItem>();

    // 미니몬
    public List<MinMonSaveData> OwnedMinMons    = new List<MinMonSaveData>();
    public int                  CompanionMinMonId = -1;

    // 퀘스트 / 해금
    public List<int>    CompletedQuestIds    = new List<int>();
    public List<string> UnlockedTabs         = new List<string>();
    public List<int>    UnlockedAreaIds      = new List<int>();

    // 마물 도감
    public List<int> DiscoveredMonsterIds = new List<int>();
}

// ──────────────────────────────────────────────
//  GameManager
// ──────────────────────────────────────────────
public class GameManager
{
    GameData _gameData = new GameData();
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

    // ── 퀘스트 / 해금 ──
    public List<int>    CompletedQuestIds    => _gameData.CompletedQuestIds;
    public List<string> UnlockedTabs         => _gameData.UnlockedTabs;
    public List<int>    UnlockedAreaIds      => _gameData.UnlockedAreaIds;

    // ── 마물 도감 ──
    public List<int> DiscoveredMonsterIds => _gameData.DiscoveredMonsterIds;

    // ── 언어 ──
    public Define.Language Language
    {
        get => (Define.Language)PlayerPrefs.GetInt("Language", (int)Define.Language.Korean);
        set { PlayerPrefs.SetInt("Language", (int)value); OnLanguageChanged?.Invoke(); }
    }

    // ── 이벤트 ──
    public event Action OnLanguageChanged;
    public event Action OnResourceChanged;
    public event Action EquipInfoChanged;
    public event Action<Equipment> OnEquipmentForged;

    // ────────────────────────────────────────
    //  Init
    // ────────────────────────────────────────
    public void Init()
    {
        _path = Application.persistentDataPath + "/SaveData.json";

        if (LoadGame())
            return;

        PlayerPrefs.SetInt("ISFIRST", 1);
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
            existing.Count += count;
        else
            Inventory.Add(new InventoryItem { DataId = dataId, Count = count });
        SaveGame();
    }

    public void RemoveItem(int dataId, int count = 1)
    {
        var existing = Inventory.Find(i => i.DataId == dataId);
        if (existing == null) return;
        existing.Count -= count;
        if (existing.Count <= 0)
            Inventory.Remove(existing);
        SaveGame();
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
            return false;
        }

        if (!File.Exists(_path))
            return false;

        GameData data = JsonConvert.DeserializeObject<GameData>(File.ReadAllText(_path));
        if (data != null)
            _gameData = data;

        return true;
    }
}
