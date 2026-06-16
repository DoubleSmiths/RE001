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
    public int KillCount;       // 사냥 성공 횟수 — 기록도(분석률) = KillCount/MaxKillCount. 포획 안 해도 누적.
    public int TameCount;       // 시도 횟수(천장 카운터) — 성공/실패 모두 증가. "n/10번째에 확정"
    public int CaughtCount;     // 실제 포획 성공 횟수 — 보유 판정/재포획 등급 롤/공명도 기준
    public bool IsCompanion;
    // 능력치 등급 (0~5 = E~S, 슬18/20). -1 = 미포획(첫 포획 시 RollGrade로 결정)
    public int AtkGrade      = -1;
    public int AtkSpeedGrade = -1;
    public int CritGrade     = -1;
}

// ──────────────────────────────────────────────
//  세이브 데이터
// ──────────────────────────────────────────────
[Serializable]
public class GameData
{
    // 기본
    public string PlayerName = "여행자";
    public int    AdventurerRank = 0;   // 슬7: 모험가 등급 0~99 (0=BLANK)
    public int    Level      = 1;
    public float  Exp        = 0f;
    public int    Gold        = 0;
    public int    Crystal     = 0;
    public int    StatusPoint = 0;
    public int    Elixir      = 0;
    public int    Mandrake    = 0;   // 슬31/37: [가랏! 만드라고라!] 확정 포획용 소비 아이템 보유 수
    // 캐릭터창 스탯 분배 누적치(영구). 0=STR(물공) 1=VIT(HP) 2=DEX(명중) 3=MAG(마공)
    public int[]  StatInvest  = new int[4];

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
    public List<int>            InterestedMinMons = new List<int>(); // 슬23: 포획 가능 미니몬 창고(저장 순서)
    public List<int>            MaxStarGranted    = new List<int>(); // 슬2: 보라별(3단계) 최초 달성으로 완성도 등급 부여한 몬스터(중복 방지)

    // 슬2: 도감 완성도 4스킬 상태 (좌→우: 종을초월교감/조련사기술/오다주웠/기억확장). 기본 해금은 idx3(기억확장)만.
    public bool[] BestiarySkillUnlocked = new bool[] { false, false, false, true };
    public int[]  BestiarySkillGrade    = new int[4];
    public bool[] BestiarySkillNew      = new bool[4];
    public int    BestiarySkillRotPtr   = 0;   // 좌→우 순환 포인터(다음 등급 오를 스킬 idx)
    public int    BestiaryCompletion    = 0;   // 보라별 달성 누적 수(완성도 숫자)

    // 능력(슬38): 보유한 능력 / 기억(장착)중인 능력 / 능력별 단계(Level)
    public List<int>            LearnedAbilities  = new List<int>();
    public List<int>            EquippedAbilities = new List<int>();
    public Dictionary<int,int>  AbilityLevels     = new Dictionary<int,int>();

    // 해금
    public List<int>    ActivationManager    = new List<int>( new int[Enum.GetValues(typeof(ActivationKey)).Length]);

    // 마물 도감
    public List<int> DiscoveredMonsterIds = new List<int>();

    // 퀘스트
    public int          CurrentQuestID = 1;
    public int          CurrentSubQuestID = 0;  // 0 = 활성 서브퀘스트 없음
    public List<string> UnlockedBtnIds = new List<string>();
    public int          CurrentHPPotionDataId = POTION_ID_MIN + 1;
    public int          CurrentSpeedPotionDataId = SPEED_POTION_ID_MIN + 1;
    public int          CurrentTravelAreaId = 0;

    // 마지막 장소
    public Village1Group LastVillage1Group = Village1Group.Main;

    // 최하단 탭(상태/가방/물약/장비/기술/도감) 해금 비트마스크. 슬라이드 10.
    // 기본값 1 = [상태]만 해금. 나머지 퀘스트 진행에 따라 해금.
    public int UnlockedBtmButtons = 1;

    // 제작의뢰 팝업 내 SPD 포션 해금 연출이 이미 재생되었는지.
    // false = 아직 미공개 (잠긴 상태로 시작 → 팝업 오픈 시 1회 해금 애니)
    // true  = 공개됨 (바로 해금 상태로 표시)
    public bool SpdPotionRevealed = false;

    // UI_PotionPopup(하단 탭) 내 SPD 슬롯 해금 연출이 재생되었는지.
    // 제작의뢰 해금 후 물약 팝업을 첫 번째로 열 때 1회 연출.
    public bool SpdPotionPopupAnimPlayed = false;
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
        set { _gameData.StatusPoint = value; OnStatusPointChanged?.Invoke(); SaveGame(); }
    }

    public int Elixir
    {
        get => _gameData.Elixir;
        set { _gameData.Elixir = value; SaveGame(); }
    }

    // 캐릭터창 스탯 분배 누적치(영구). 배열 참조 반환 — 원소 수정 후 SaveGame()으로 저장.
    public int[] StatInvest => _gameData.StatInvest;

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
    public int AdventurerRank
    {
        get => _gameData.AdventurerRank;
        set { _gameData.AdventurerRank = Mathf.Clamp(value, 0, 99); SaveGame(); }
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
    public List<int>            InterestedMinMons => _gameData.InterestedMinMons;
    public List<int>            MaxStarGranted    => _gameData.MaxStarGranted;

    // 슬2: 도감 완성도 4스킬 상태 (세이브/로드는 GameManager 단일소스)
    public bool[] BestiarySkillUnlocked => _gameData.BestiarySkillUnlocked;
    public int[]  BestiarySkillGrade    => _gameData.BestiarySkillGrade;
    public bool[] BestiarySkillNew      => _gameData.BestiarySkillNew;
    public int    BestiarySkillRotPtr { get => _gameData.BestiarySkillRotPtr; set => _gameData.BestiarySkillRotPtr = value; }
    public int    BestiaryCompletion  { get => _gameData.BestiaryCompletion;  set => _gameData.BestiaryCompletion  = value; }

    // ── 능력 ──
    public List<int>            LearnedAbilities  => _gameData.LearnedAbilities;
    public List<int>            EquippedAbilities => _gameData.EquippedAbilities;
    public Dictionary<int,int>  AbilityLevels     => _gameData.AbilityLevels;
    public int                  CompanionMinMonId
    {
        get => _gameData.CompanionMinMonId;
        set { _gameData.CompanionMinMonId = value; SaveGame(); }
    }

    // 슬31/37: 만드라고라 보유 수 (확정 포획 소비 아이템)
    public int MandrakeCount
    {
        get => _gameData.Mandrake;
        set { _gameData.Mandrake = Mathf.Max(0, value); SaveGame(); }
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

    public int CurrentSubQuestID
    {
        get => _gameData.CurrentSubQuestID;
        set { _gameData.CurrentSubQuestID = value; }
    }

    public List<string> UnlockedBtnIds => _gameData.UnlockedBtnIds;

    public bool SpdPotionRevealed
    {
        get => _gameData.SpdPotionRevealed;
        set { _gameData.SpdPotionRevealed = value; SaveGame(); }
    }

    public bool SpdPotionPopupAnimPlayed
    {
        get => _gameData.SpdPotionPopupAnimPlayed;
        set { _gameData.SpdPotionPopupAnimPlayed = value; SaveGame(); }
    }
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

    public int CurrentSpeedPotionDataId
    {
        get => _gameData.CurrentSpeedPotionDataId <= 0 ? SPEED_POTION_ID_MIN + 1 : _gameData.CurrentSpeedPotionDataId;
        set
        {
            int next = value <= 0 ? SPEED_POTION_ID_MIN + 1 : value;
            if (_gameData.CurrentSpeedPotionDataId == next)
                return;

            _gameData.CurrentSpeedPotionDataId = next;
            SaveGame();
            OnSpeedPotionChanged?.Invoke(next);
        }
    }

    // 런타임 전용 현재 위치(EXPLORE 포함). 0이면 persist된 마지막 마을을 사용.
    int _runtimeCurrentTravelAreaId;

    public int CurrentTravelAreaId
    {
        get => _runtimeCurrentTravelAreaId != 0
            ? _runtimeCurrentTravelAreaId
            : _gameData.CurrentTravelAreaId;
        set
        {
            _runtimeCurrentTravelAreaId = value;

            // EXPLORE(배틀) 지역은 persist 하지 않음 — 게임 종료 후 재시작 시 마지막 마을로 복귀.
            // 다른 곳의 SaveGame() 호출이 EXPLORE 진입 중에 발생해도 _gameData에는 마지막 TOWN이
            // 그대로 남도록 _gameData.CurrentTravelAreaId 자체를 EXPLORE 값으로 덮어쓰지 않는다.
            if (Managers.Data != null &&
                Managers.Data.TravelNodeDic != null &&
                Managers.Data.TravelNodeDic.TryGetValue(value, out var node) &&
                string.Equals(node.Category, "EXPLORE", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (_gameData.CurrentTravelAreaId == value)
                return;

            _gameData.CurrentTravelAreaId = value;
            SaveGame();
        }
    }

    public Village1Group LastVillage1Group
    {
        get => _gameData.LastVillage1Group;
        set { _gameData.LastVillage1Group = value; SaveGame(); }
    }

    public bool IsBtmButtonUnlocked(Define.TabType tab)
    {
        return (_gameData.UnlockedBtmButtons & (1 << (int)tab)) != 0;
    }

    public void UnlockBtmButton(Define.TabType tab)
    {
        int mask = 1 << (int)tab;
        if ((_gameData.UnlockedBtmButtons & mask) != 0) return;
        _gameData.UnlockedBtmButtons |= mask;
        SaveGame();
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
    public event Action OnStatusPointChanged;
    public event Action EquipInfoChanged;
    public event Action<Equipment> OnEquipmentForged;
    public event Action<int> OnItemAdded;         // dataId
    public event Action<int> OnItemChanged;       // dataId (추가/제거 모두)
    public Action      OnInventoryNewChanged; // IsNew 상태 변경
    public event Action<int> OnHPPotionChanged;   // current potion dataId
    public event Action<int> OnSpeedPotionChanged;// current SPD potion dataId

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

    public int GetNextSpeedPotionDataId(int currentDataId)
    {
        int nextId = -1;

        foreach (var dataId in Managers.Data.SpeedPotionDic.Keys)
        {
            if (dataId <= currentDataId)
                continue;

            if (nextId == -1 || dataId < nextId)
                nextId = dataId;
        }

        return nextId;
    }

    public bool CanCraftSpeedPotion(int currentDataId)
    {
        int nextDataId = GetNextSpeedPotionDataId(currentDataId);
        if (nextDataId < 0 || !Managers.Data.SpeedPotionDic.TryGetValue(nextDataId, out var next))
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

    public bool TryCraftSpeedPotion(int currentDataId, out int craftedDataId)
    {
        craftedDataId = currentDataId;

        int nextDataId = GetNextSpeedPotionDataId(currentDataId);
        if (nextDataId < 0 || !Managers.Data.SpeedPotionDic.TryGetValue(nextDataId, out var next))
            return false;

        if (!CanCraftSpeedPotion(currentDataId))
            return false;

        for (int i = 0; i < next.NeededItemIDs.Count; i++)
        {
            int itemId = next.NeededItemIDs[i];
            int required = i < next.NeededItemAmounts.Count ? next.NeededItemAmounts[i] : 0;
            if (required > 0)
                RemoveItem(itemId, required);
        }

        CurrentSpeedPotionDataId = nextDataId;
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
        if (_gameData.CurrentSpeedPotionDataId <= 0)
            _gameData.CurrentSpeedPotionDataId = SPEED_POTION_ID_MIN + 1;
    }
}
