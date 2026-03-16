using Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

[Serializable]
public class StageClearInfo
{
    public int StageIndex = 1;
    public int MaxWaveIndex = 0;
    public bool isOpenFirstBox = false;
    public bool isOpenSecondBox = false;
    public bool isOpenThirdBox = false;
    public bool isClear = false;
}

[Serializable]
public class MissionInfo
{
    public int Progress;
    public bool IsRewarded;
}
/// <summary>
/// 계정에관한 모든 정보 
[Serializable]
public class GameData
{
    public int UserLevel = 1;
    public string UserName = "Player";

    public int Stamina = Define.MAX_STAMINA;
    public int Gold = 0;
    public int Dia = 0;

    #region 업적
    public int CommonGachaOpenCount = 0;
    public int AdvancedGachaOpenCount = 0;
    public int FastRewardCount = 0;
    public int OfflineRewardCount = 0;
    public int TotalMonsterKillCount = 0;
    public int TotalEliteKillCount = 0;
    public int TotalBossKillCount = 0;
    public List<Data.AchievementData> Achievements = new List<AchievementData>();  // 업적 목록

    #endregion

    #region 하루마다 초기화
    public int GachaCountAdsAnvanced = 1;
    public int GachaCountAdsCommon = 1;
    public int GoldCountAds = 1;
    public int RebirthCountAds = 3;
    public int DiaCountAds = 3;
    public int StaminaCountAds = 1;
    public int FastRewardCountAds = 1;
    public int FastRewardCountStamina = 3;
    public int SkillRefreshCountAds = 3;
    public int RemainsStaminaByDia = 3;
    public int BronzeKeyCountAds = 1;
    #endregion

    public bool[] AttendanceReceived = new bool[30];
    public bool BGMOn = true;
    public bool EffectSoundOn = true;
    public Define.JoystickType JoystickType = Define.JoystickType.Flexible;
    public List<Character> Characters = new List<Character>();
    public List<Equipment> OwnedEquipments = new List<Equipment>();
    public ContinueData ContinueInfo = new ContinueData();
    public StageData CurrentStage = new StageData();
    public Dictionary<int, int> ItemDictionary = new Dictionary<int, int>();//<ID, 갯수>
    public Dictionary<EquipmentType, Equipment> EquippedEquipments = new Dictionary<EquipmentType, Equipment>();
    public Dictionary <int, StageClearInfo> DicStageClearInfo = new Dictionary<int, StageClearInfo>();
    public Dictionary<MissionTarget, MissionInfo> DicMission = new Dictionary<MissionTarget, MissionInfo>()
    {
        {MissionTarget.StageEnter, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.StageClear, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.EquipmentLevelUp, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.OfflineRewardGet, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.EquipmentMerge, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.MonsterKill, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.EliteMonsterKill, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.GachaOpen, new MissionInfo() { Progress = 0, IsRewarded = false }},
        {MissionTarget.ADWatchIng, new MissionInfo() { Progress = 0, IsRewarded = false }},
    };
}

[Serializable]
public class ContinueData
{
    public bool isContinue { get { return SavedBattleSkill.Count > 0; } }
    public int PlayerDataId;
    public float Hp;
    public float MaxHp;
    public float MaxHpBonusRate = 1;
    public float HealBonusRate = 1;
    public float HpRegen;
    public float Atk;
    public float AttackRate = 1;
    public float Def;
    public float DefRate;
    public float MoveSpeed;
    public float MoveSpeedRate = 1;
    public float TotalExp;
    public int Level = 1;
    public float Exp;
    public float CriRate;
    public float CriDamage = 1.5f;
    public float DamageReduction;
    public float ExpBonusRate = 1;
    public float SoulBonusRate = 1;
    public float CollectDistBonus = 1;
    public int KillCount;
    public int SkillRefreshCount = 3;
    public float SoulCount;

    public List<SupportSkillData> SoulShopList = new List<SupportSkillData>();
    public List<SupportSkillData> SavedSupportSkill = new List<SupportSkillData>();
    public Dictionary<Define.SkillType, int> SavedBattleSkill = new Dictionary<Define.SkillType, int>();

    public int WaveIndex;
    public void Clear()
    {
        // 각 변수의 초기값 설정
        PlayerDataId = 0;
        Hp = 0f;
        MaxHp = 0f;
        MaxHpBonusRate = 1f;
        HealBonusRate = 1f;
        HpRegen = 0f;
        Atk = 0f;
        AttackRate = 1f;
        Def = 0f;
        DefRate = 0f;
        MoveSpeed = 0f;
        MoveSpeedRate = 1f;
        TotalExp = 0f;
        Level = 1;
        Exp = 0f;
        CriRate = 0f;
        CriDamage = 1.5f;
        DamageReduction = 0f;
        ExpBonusRate = 1f;
        SoulBonusRate = 1f;
        CollectDistBonus = 1f;
        
        KillCount = 0;
        SoulCount = 0f;
        SkillRefreshCount = 3;

        SoulShopList.Clear();
        SavedSupportSkill.Clear();
        SavedBattleSkill.Clear();

    }
}

public class GameManager
{
    #region GameData
    public GameData _gameData = new GameData();
    public List<Equipment> OwnedEquipments
    {
        get { return _gameData.OwnedEquipments; }
        set
        {
            _gameData.OwnedEquipments = value;
            //갱신이 빈번하게 발생하여 렉 발생, Sorting시 무한루프 발생으로 인하여 주석처리
            //EquipInfoChanged?.Invoke();
        }
    }
    public List<SupportSkillData> SoulShopList
    {
        get { return _gameData.ContinueInfo.SoulShopList; }
        set
        {
            _gameData.ContinueInfo.SoulShopList = value;
            SaveGame();
        }
    }

    public Dictionary<int, int> ItemDictionary
    {
        get { return _gameData.ItemDictionary; }
        set
        {
            _gameData.ItemDictionary = value;
        }
    }
    public Dictionary<MissionTarget, MissionInfo> DicMission
    {
        get { return _gameData.DicMission; }
        set
        {
            _gameData.DicMission = value;
        }
    }
    public List<Character> Characters
    {
        get { return _gameData.Characters; }
        set
        {
            _gameData.Characters = value;
            EquipInfoChanged?.Invoke();
        }
    }
    public Dictionary<EquipmentType, Equipment> EquippedEquipments
    {
        get { return _gameData.EquippedEquipments; }
        set
        {
            _gameData.EquippedEquipments = value;
            EquipInfoChanged?.Invoke();
        }
    }
    public Dictionary<int, StageClearInfo> DicStageClearInfo
    {
        get { return _gameData.DicStageClearInfo; }
        set
        {
            _gameData.DicStageClearInfo = value;
            Managers.Achievement.StageClear();
            SaveGame();
        }
    }
    public int UserLevel
    {
        get { return _gameData.UserLevel; }
        set { _gameData.UserLevel = value; }
    }
    public string UserName
    {
        get { return _gameData.UserName; }
        set { _gameData.UserName = value; }
    }
    public int Stamina
    {
        get { return _gameData.Stamina; }
        set 
        { 
            _gameData.Stamina = value;
            SaveGame();
            OnResourcesChagned?.Invoke();
        }
    }
    public int Gold
    {
        get { return _gameData.Gold; }
        set 
        {
            _gameData.Gold = value;
            SaveGame();
            OnResourcesChagned?.Invoke();
        }
    }
    public int Dia
    {
        get { return _gameData.Dia; }
        set 
        { 
            _gameData.Dia = value;
            SaveGame();
            OnResourcesChagned?.Invoke();
        }
    }

    public int CommonGachaOpenCount
    {
        get { return _gameData.CommonGachaOpenCount; }
        set { 
            _gameData.CommonGachaOpenCount = value;
            Managers.Achievement.CommonOpen();
        }
    }
    public int AdvancedGachaOpenCount
    {
        get { return _gameData.AdvancedGachaOpenCount; }
        set { 
            _gameData.AdvancedGachaOpenCount = value;
            Managers.Achievement.AdvancedOpen();
        }
    }
    public int FastRewardCount
    {
        get { return _gameData.FastRewardCount; }
        set { 
            _gameData.FastRewardCount = value;
            Managers.Achievement.FastReward();
        }
    }
    public int OfflineRewardCount
    {
        get { return _gameData.OfflineRewardCount; }
        set { 
            _gameData.OfflineRewardCount = value;
            Managers.Achievement.OfflineReward();
        }
    }
    public int TotalMonsterKillCount
    {
        get { return _gameData.TotalMonsterKillCount; }
        set 
        { 
            _gameData.TotalMonsterKillCount = value;
            if (value % 100 == 0)
                Managers.Achievement.MonsterKill();
        }
    }
    public int TotalEliteKillCount
    {
        get { return _gameData.TotalEliteKillCount; }
        set { 
            _gameData.TotalEliteKillCount = value;
            Managers.Achievement.EliteKill();
        }
    }
    public int TotalBossKillCount
    {
        get { return _gameData.TotalBossKillCount; }
        set { 
            _gameData.TotalBossKillCount = value;
            Managers.Achievement.BossKill();
        }
    }
    public List<Data.AchievementData> Achievements
    {
        get { return _gameData.Achievements; }
        set
        {
            _gameData.Achievements = value;
        }
    }
    public ContinueData ContinueInfo
    {
        get { return _gameData.ContinueInfo; }
        set 
        {
            _gameData.ContinueInfo = value;
        }
    }
    public StageData CurrentStageData
    {
        get { return _gameData.CurrentStage; }
        set { _gameData.CurrentStage = value; }
    }
    public WaveData CurrentWaveData
    {
        get { return CurrentStageData.WaveArray[CurrentWaveIndex]; }
    }
    public int CurrentWaveIndex
    {
        get { return _gameData.ContinueInfo.WaveIndex; }
        set { _gameData.ContinueInfo.WaveIndex = value; }
    }
    public Map CurrentMap { get; set; }

    public int GachaCountAdsAnvanced
    {
        get { return _gameData.GachaCountAdsAnvanced; }
        set
        {
            _gameData.GachaCountAdsAnvanced = value;
        }
    }
    public int GachaCountAdsCommon
    {
        get { return _gameData.GachaCountAdsCommon; }
        set
        {
            _gameData.GachaCountAdsCommon = value;
        }
    }
    public int GoldCountAds 
    {
        get { return _gameData.GoldCountAds; }
        set
        {
            _gameData.GoldCountAds = value;
        }
    }
    public int RebirthCountAds
    {
        get { return _gameData.RebirthCountAds; }
        set
        {
            _gameData.RebirthCountAds = value;
        }
    }
    public int DiaCountAds
    {
        get { return _gameData.DiaCountAds; }
        set
        {
            _gameData.DiaCountAds = value;
        }
    }
    public int StaminaCountAds
    {
        get { return _gameData.StaminaCountAds; }
        set
        {
            _gameData.StaminaCountAds = value;
        }
    }
    public int FastRewardCountAds
    {
        get { return _gameData.FastRewardCountAds; }
        set
        {
            _gameData.FastRewardCountAds = value;
        }
    }
    public int SkillRefreshCountAds
    {
        get { return _gameData.SkillRefreshCountAds; }
        set
        {
            _gameData.SkillRefreshCountAds = value;
        }
    }
    public int RemainsStaminaByDia
    {
        get { return _gameData.RemainsStaminaByDia; }
        set
        {
            _gameData.RemainsStaminaByDia = value;
        }
    }
    public int BronzeKeyCountAds
    {
        get { return _gameData.BronzeKeyCountAds; }
        set 
        {
            _gameData.BronzeKeyCountAds = value;
        }
    }
    public int FastRewardCountStamina
    {
        get { return _gameData.FastRewardCountStamina; }
        set
        {
            _gameData.FastRewardCountStamina = value;
        }
    }

    public bool[] AttendanceReceived 
    {
        get { return _gameData.AttendanceReceived;}
        set { _gameData.AttendanceReceived = value;}
    }

    #region Option
    public bool BGMOn
    {
        get { return _gameData.BGMOn; }
        set 
        {
            if (_gameData.BGMOn == value)
                return;
            _gameData.BGMOn = value;
            if (_gameData.BGMOn == false)
            {
                Managers.Sound.Stop(Sound.Bgm);
            }
            else
            {
                string name = "Bgm_Lobby";
                if (Managers.Scene.CurrentScene.SceneType == Define.Scene.GameScene)
                    name = "Bgm_Game";

                Managers.Sound.Play(Define.Sound.Bgm, name);
            }
        }
    }

    public bool EffectSoundOn
    {
        get { return _gameData.EffectSoundOn; }
        set { _gameData.EffectSoundOn = value; }
    }

    public Define.JoystickType JoystickType
    {
        get { return _gameData.JoystickType; }
        set { _gameData.JoystickType = value; }
    }

    #endregion

    #endregion

    #region Character
    public Character CurrentCharacter
    {
        get
        {
            return _gameData.Characters.Find(c => c.isCurrentCharacter == true);
        }
    }
    #endregion

    #region Player
    public PlayerController Player { get; set; }
    Vector2 _moveDir;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            OnMoveDirChanged?.Invoke(_moveDir);
        }
    }
    #endregion

    #region Action
    public event Action<Vector2> OnMoveDirChanged;
    public event Action EquipInfoChanged;
    public event Action OnResourcesChagned;
    //public Action OnMonsterDataUpdated;
    #endregion

    public float TimeRemaining = 60;
    public Vector3 SoulDestination { get; set; }
    public bool IsLoaded = false;
    public bool IsGameEnd = false;
    public CameraController CameraController { get; set; }

    public void Init()
    {
        _path = Application.persistentDataPath + "/SaveData.json";

        if (LoadGame())
            return;

        PlayerPrefs.SetInt("ISFIRST", 1);

        Character character = new Character();
        character.SetInfo(CHARACTER_DEFAULT_ID);
        character.isCurrentCharacter = true;

        Characters = new List<Character>();
        Characters.Add(character);

        CurrentStageData = Managers.Data.StageDic[1];
        
        foreach (Data.StageData stage in Managers.Data.StageDic.Values)
        {
            StageClearInfo info = new StageClearInfo 
            {
                StageIndex = stage.StageIndex,
                MaxWaveIndex = 0,
                isOpenFirstBox = false,
                isOpenSecondBox = false,
                isOpenThirdBox = false,
            };
            _gameData.DicStageClearInfo.Add(stage.StageIndex, info);
        }

        Managers.Time.LastRewardTime = DateTime.Now;
        Managers.Time.LastGeneratedStaminaTime = DateTime.Now;

        SetBaseEquipment();

        Managers.Achievement.Init();

        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_BRONZE_KEY], 10);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD_KEY], 30);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_DIA], 1000);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD], 100000);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_WEAPON_SCROLL], 15);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GLOVES_SCROLL], 15);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_RING_SCROLL], 15);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_BELT_SCROLL], 15);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_ARMOR_SCROLL], 15);
        ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_BOOTS_SCROLL], 15);
        ////테스트용
        //for (int i = 50401; i < 50460; i++)
        //{ 
        //    AddMaterialItem(i, 5);
        //}
        IsLoaded = true;
        SaveGame();
    }

    public void ExchangeMaterial(MaterialData data, int count)
    {
        switch (data.MaterialType)
        {
            case MaterialType.Dia:
                Dia += count;
                break;
            case MaterialType.Gold:
                Gold += count;
                break;
            case MaterialType.Stamina:
                Stamina += count;
                break;
            case MaterialType.BronzeKey:
            case MaterialType.SilverKey:
            case MaterialType.GoldKey:
                AddMaterialItem(data.DataId, count);
                break;
            case MaterialType.RandomScroll:
                int randScroll = UnityEngine.Random.Range(50101, 50106);
                AddMaterialItem(randScroll, count);
                break;
            case MaterialType.WeaponScroll:
                AddMaterialItem(Define.ID_WEAPON_SCROLL, count);
                break;
            case MaterialType.GlovesScroll:
                AddMaterialItem(Define.ID_GLOVES_SCROLL, count);
                break;
            case MaterialType.RingScroll:
                AddMaterialItem(Define.ID_RING_SCROLL, count);
                break;
            case MaterialType.BeltScroll:
                AddMaterialItem(Define.ID_BELT_SCROLL, count);
                break;
            case MaterialType.ArmorScroll:
                AddMaterialItem(Define.ID_ARMOR_SCROLL, count);
                break;
            case MaterialType.BootsScroll:
                AddMaterialItem(Define.ID_BOOTS_SCROLL, count);
                break;
            default: 
                //TODO 
                break;
        }
        
    }

    public void SetNextStage()
    { 
        CurrentStageData = Managers.Data.StageDic[CurrentStageData.StageIndex + 1];
    }

    public int GetMaxStageIndex()
    {
        foreach(StageClearInfo clearInfo in _gameData.DicStageClearInfo.Values) 
        {
            if (clearInfo.MaxWaveIndex != 10)
                return clearInfo.StageIndex;
        }
        return 0;
    }

    public int GetMaxStageClearIndex()
    {
        int MaxStageClearIndex = 0;

        foreach (StageClearInfo stageClearInfo in Managers.Game.DicStageClearInfo.Values)
        {
            if (stageClearInfo.isClear == true)
                MaxStageClearIndex = Mathf.Max(MaxStageClearIndex, stageClearInfo.StageIndex);
        }
        return MaxStageClearIndex;
    }

    public void AddMaterialItem(int id, int quantity)
    {
        if (ItemDictionary.ContainsKey(id))
        {
            ItemDictionary[id] += quantity;
        }
        else
        {
            ItemDictionary[id] = quantity;
        }
        SaveGame();
    }
    
    public void RemovMaterialItem(int id, int quantity)
    {
        if (ItemDictionary.ContainsKey(id))
        {
            ItemDictionary[id] -= quantity;
            SaveGame();
        }
    }

    #region InGame

    public GemInfo GetGemInfo()
    {
        float smallGemChance = CurrentWaveData.SmallGemDropRate;
        float greenGemChance = CurrentWaveData.GreenGemDropRate + smallGemChance;
        float blueGemChance = CurrentWaveData.BlueGemDropRate + greenGemChance;
        float yellowGemChance = CurrentWaveData.YellowGemDropRate + blueGemChance;
        float rand = Random.value;

        if (rand < smallGemChance)
            return new GemInfo(GemInfo.GemType.Small, new Vector3(0.65f, 0.65f, 0.65f));
        else if (rand < greenGemChance)
            return new GemInfo(GemInfo.GemType.Green, Vector3.one);
        else if (rand < blueGemChance)
            return new GemInfo(GemInfo.GemType.Blue, Vector3.one);
        else if (rand < yellowGemChance)
            return new GemInfo(GemInfo.GemType.Yellow, Vector3.one);

        return null;
    }
    public GemInfo GetGemInfo(GemInfo.GemType type)
    {
        if(type == GemInfo.GemType.Small)
            return new GemInfo(GemInfo.GemType.Small, new Vector3(0.65f, 0.65f, 0.65f));

        return new GemInfo(type, Vector3.one);
    }
    public void GameOver()
    {
        IsGameEnd = true;
        Player.StopAllCoroutines();
        Managers.UI.ShowPopupUI<UI_GameoverPopup>().SetInfo();
    }

    public (int hp, int atk) GetCurrentChracterStat()
    {
        int hpBonus = 0;
        int AtkBonus = 0;
        var (equipHpBonus, equipAtkBonus) = GetEquipmentBonus();

        Character ch = CurrentCharacter;

        hpBonus = (equipHpBonus);
        AtkBonus = (equipAtkBonus);

        return (hpBonus, AtkBonus);
    }


    #endregion

    #region Equipment

    public void SetBaseEquipment()
    {
        //초기아이템 설정
        Equipment weapon = new Equipment(WEAPON_DEFAULT_ID);
        Equipment gloves = new Equipment(GLOVES_DEFAULT_ID);
        Equipment ring = new Equipment(RING_DEFAULT_ID);
        Equipment belt = new Equipment(BELT_DEFAULT_ID);
        Equipment armor = new Equipment(ARMOR_DEFAULT_ID);
        Equipment boots = new Equipment(BOOTS_DEFAULT_ID);

        OwnedEquipments = new List<Equipment>
            {
                weapon,
                gloves,
                ring,
                belt,
                armor,
                boots
            };

        EquippedEquipments = new Dictionary<EquipmentType, Equipment>();
        EquipItem(EquipmentType.Weapon, weapon);
        EquipItem(EquipmentType.Gloves, gloves);
        EquipItem(EquipmentType.Ring, ring);
        EquipItem(EquipmentType.Belt, belt);
        EquipItem(EquipmentType.Armor, armor);
        EquipItem(EquipmentType.Boots, boots);
    }

    public void EquipItem(EquipmentType type, Equipment equipment)
    {
        if (EquippedEquipments.ContainsKey(type))
        {
            EquippedEquipments[type].IsEquipped = false;
            EquippedEquipments.Remove(type);
        }

        // 새로운 장비를 착용
        EquippedEquipments.Add(type, equipment);
        equipment.IsEquipped = true;
        equipment.IsConfirmed = true;

        // 장비변경 이벤트 호출
        EquipInfoChanged?.Invoke();
    }

    public void UnEquipItem(Equipment equipment)
    {
        // 착용중인 장비를 제거한다.
        if (EquippedEquipments.ContainsKey(equipment.EquipmentData.EquipmentType))
        {
            EquippedEquipments[equipment.EquipmentData.EquipmentType].IsEquipped = false;
            EquippedEquipments.Remove(equipment.EquipmentData.EquipmentType);

        }
        // 장비변경 이벤트 호출
        EquipInfoChanged?.Invoke();
    }

    public Equipment AddEquipment(string key)
    {
        if (key.Equals("None"))
            return null;

        Equipment equip = new Equipment(key);
        equip.IsConfirmed = false;
        OwnedEquipments.Add(equip);
        EquipInfoChanged?.Invoke();

        return equip;
    }

    public Equipment MergeEquipment(Equipment equipment, Equipment mergeEquipment1, Equipment mergeEquipment2, bool isAllMerge = false)
    {
        equipment = OwnedEquipments.Find(equip => equip == equipment);
        if (equipment == null)
            return null;
        mergeEquipment1 = OwnedEquipments.Find(equip => equip == mergeEquipment1);
        if (mergeEquipment1 == null)
            return null;

        if (mergeEquipment2 != null)
        {
            mergeEquipment2 = OwnedEquipments.Find(equip => equip == mergeEquipment2);
            if (mergeEquipment2 == null)
                return null;
        }

        int level = equipment.Level;
        bool isEquipped = equipment.IsEquipped;// || mergeEquipment1.IsEquipped || mergeEquipment2.IsEquipped;
        string mergedItemCode = equipment.EquipmentData.MergedItemCode;
        Equipment newEquipment = AddEquipment(mergedItemCode);
        newEquipment.Level = level;
        newEquipment.IsEquipped = isEquipped;

        OwnedEquipments.Remove(equipment);
        OwnedEquipments.Remove(mergeEquipment1);
        OwnedEquipments.Remove(mergeEquipment2);

        if (Managers.Game.DicMission.TryGetValue(MissionTarget.EquipmentMerge, out MissionInfo mission))
            mission.Progress++;

        //자동합성인 경우는 SAVE게임 하지않고 다끝난후에 한번에 한다.
        if(isAllMerge == false)
            SaveGame();

        Debug.Log(newEquipment.EquipmentData.EquipmentGrade);
        return newEquipment;
    }

    public void SortEquipment(EquipmentSortType sortType)
    {
        if (sortType == EquipmentSortType.Grade)
        {
            //OwnedEquipments = OwnedEquipments.OrderBy(item => item.EquipmentGrade).ThenBy(item => item.Level).ThenBy(item => item.EquipmentType).ToList();
            OwnedEquipments = OwnedEquipments.OrderBy(item => item.EquipmentData.EquipmentGrade).ThenBy(item => item.IsEquipped).ThenBy(item => item.Level).ThenBy(item => item.EquipmentData.EquipmentType).ToList();

        }
        else if (sortType == EquipmentSortType.Level)
        {
            OwnedEquipments = OwnedEquipments.OrderBy(item => item.Level).ThenBy(item => item.IsEquipped).ThenBy(item => item.EquipmentData.EquipmentGrade).ThenBy(item => item.EquipmentData.EquipmentType).ToList();
        }
    }

    public void GenerateRandomEquipment()
    {
        //N 0 
        //장비타입
        //
        EquipmentType type = Util.GetRandomEnumValue<EquipmentType>();
        GachaRarity rarity = Util.GetRandomEnumValue<GachaRarity>();
        EquipmentGrade grade = Util.GetRandomEnumValue<EquipmentGrade>();
        string itemNum = Random.Range(1, 4).ToString("D2");
        string gradeNum = ((int)grade).ToString("D2");


        string key = $"{rarity.ToString()[0]}{type.ToString()[0]}{itemNum}{gradeNum}";

        if (Managers.Data.EquipDataDic.ContainsKey(key))
        {
            AddEquipment(key);

        }
        //AddEquipment("N00101");
    }

    //public void GenerateRandomMaterials()
    //{
    //    EquipmentType type = Util.GetRandomEnumValue<EquipmentType>();
    //    GachaRarity rarity = Util.GetRandomEnumValue<GachaRarity>();
    //    EquipmentGrade grade = Util.GetRandomEnumValue<EquipmentGrade>();
    //    string itemNum = Random.Range(1, 4).ToString("D2");
    //    string gradeNum = ((int)grade).ToString("D2");


    //    string key = $"{rarity.ToString()[0]}{type.ToString()[0]}{itemNum}{gradeNum}";

    //    if (Managers.Data.EquipDataDic.ContainsKey(key))
    //    {
    //        AddEquipment(key);
    //    }
    //}

    public void GenerateRandomMaterials()
    {
        //임시
        List<Data.MaterialData> list = Managers.Data.MaterialDic.Values.ToList();
        for (int i = 0; i < 5; i++)
        { 
            AddMaterialItem(list[UnityEngine.Random.Range(11, list.Count)].DataId, 30);
        }
    }


    public (int hp, int atk) GetEquipmentBonus()
    {
        int hpBonus = 0;
        int atkBonus = 0;

        foreach (KeyValuePair<EquipmentType, Equipment> pair in EquippedEquipments)
        {
            hpBonus += pair.Value.MaxHpBonus;
            atkBonus += pair.Value.AttackBonus;
        }
        return (hpBonus, atkBonus);
    }

    #endregion

    #region Gacha
    public List<Equipment> DoGacha(GachaType gachaType, int count = 1)
    {
        List<Equipment> ret = new List<Equipment>();

        for (int i = 0; i < count; i++)
        {

            EquipmentGrade grade = GetRandomGrade(Define.PICKUP_GACHA_GRADE_PROB);
            switch (gachaType)
            {
                case GachaType.CommonGacha:
                    grade = GetRandomGrade(Define.COMMON_GACHA_GRADE_PROB);
                    CommonGachaOpenCount++;
                    break;
                case GachaType.PickupGacha:
                    grade = GetRandomGrade(Define.PICKUP_GACHA_GRADE_PROB);
                    break;
                case GachaType.AdvancedGacha:
                    grade = GetRandomGrade(Define.ADVENCED_GACHA_GRADE_PROB);
                    AdvancedGachaOpenCount++;
                    break;
            }

            List<GachaRateData> list = Managers.Data.GachaTableDataDic[gachaType].GachaRateTable.Where(item => item.EquipGrade == grade).ToList();

            int index = UnityEngine.Random.Range(0, list.Count);
            string key = list[index].EquipmentID;

            if (Managers.Data.EquipDataDic.ContainsKey(key))
            {
                ret.Add(AddEquipment(key));
            }
        }

        return ret;
    }

    public static EquipmentGrade GetRandomGrade(float[] prob)
    {
        float randomValue = UnityEngine.Random.value;
        if (randomValue < prob[(int)EquipmentGrade.Common])
        {
            return EquipmentGrade.Common;
        }
        else if (randomValue < prob[(int)EquipmentGrade.Common] + prob[(int)EquipmentGrade.Uncommon])
        {
            return EquipmentGrade.Uncommon;
        }
        else if (randomValue < prob[(int)EquipmentGrade.Common] + prob[(int)EquipmentGrade.Uncommon] + prob[(int)EquipmentGrade.Rare])
        {
            return EquipmentGrade.Rare;
        }
        else if (randomValue < prob[(int)EquipmentGrade.Common] + prob[(int)EquipmentGrade.Uncommon] + prob[(int)EquipmentGrade.Rare] + prob[(int)EquipmentGrade.Epic])
        {
            return EquipmentGrade.Epic;
        }

        return EquipmentGrade.Common;
    }
    #endregion

    #region Save&Load
    string _path;
    
    public void SaveGame()
    {
        if (Player != null )
        {
            _gameData.ContinueInfo.SavedBattleSkill = Player.Skills?.SavedBattleSkill;
            _gameData.ContinueInfo.SavedSupportSkill = Player.Skills?.SupportSkills;
        }
        string jsonStr = JsonConvert.SerializeObject(_gameData);
        File.WriteAllText(_path, jsonStr);
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

        if (File.Exists(_path) == false)
            return false;

        string fileStr = File.ReadAllText(_path);
        GameData data = JsonConvert.DeserializeObject<GameData>(fileStr);
        if (data != null)
            _gameData = data;

        EquippedEquipments = new Dictionary<EquipmentType, Equipment>();
        for (int i = 0; i < OwnedEquipments.Count; i++)
        {
            if (OwnedEquipments[i].IsEquipped)
            {
                EquipItem(OwnedEquipments[i].EquipmentData.EquipmentType, OwnedEquipments[i]);
            }
        }
        IsLoaded = true;
        return true;
    }

    public void ClearContinueData()
    {
        Managers.Game.SoulShopList.Clear();
        ContinueInfo.Clear();
        CurrentWaveIndex = 0;
        SaveGame();
    }
  
    public float GetTotalDamage()
    {
        float result = 0;

        foreach (SkillBase skill in Player.Skills.SkillList)
        {
            result += skill.TotalDamage;
        }

        return result;
    }
    #endregion
}

