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
    // 값은 isNew 필드가 저장한다. 프로퍼티까지 직렬화하면 로드 중에 세터(저장·이벤트)가 돌 수 있다.
    [JsonIgnore]
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
    public int Potential;   // 잠재력 — 강화 성공마다 등급별 증가, 쌓일수록 성공확률 Low→High 보간(VS 원작)

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

    // ⚠️ 저장 금지 — 데이터표 행을 그대로 돌려주는 get-only 참조라, 저장하면 행 사본이 세이브에 남고
    //  로드 때 Newtonsoft 가 그 사본을 **공유 EquipmentDic 객체에 덮어써** 밸런스 수정이 되돌아갔다.
    [JsonIgnore]
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
    // 세이브 형식 버전 — 0 = 버전 필드 도입 전 세이브. 기록할 때 GameManager.SAVE_VERSION 으로 채운다.
    public int SaveVersion = 0;

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
    // v2 훈련(골드 강화) 단계. 0=공격% 1=체력% 2=방어% 3=치명피해%p (ADR0030)
    public int[]  TrainingLevels = new int[4];
    // v2 스테이지·보스(ADR0030). 처치 수는 StageNodeId 노드의 것 — 다른 노드로 가면 0부터 센다.
    //  돌파한 노드는 첫 돌파 크리스탈을 다시 주지 않는다. 보스에게 지면 자동 도전이 꺼진다(막대를 눌러 다시 켠다).
    public int       StageNodeId;
    public int       StageKillCount;
    public List<int> ClearedStages = new List<int>();
    public bool      BossAutoChallenge = true;
    // v2 가드·패링 도움말을 이미 보여 줬는가(첫 공격 예고 '!' 때 한 번)
    public bool      GuardHintSeen;

    // 환경설정
    public bool _isBGSoundOn        = true;
    public bool _isSFXOn            = true;
    public bool _isCreateAtkTextOn    = true;
    public bool _isShakeOn          = true;

    // 장비 — EquippedEquipments는 직렬화 제외. 로드 후 OwnedEquipments의 IsEquipped로 재구성
    //  (Json 역직렬화 시 두 컬렉션의 Equipment가 서로 다른 참조가 되어 강화가 한쪽만 갱신되는 버그 방지)
    public List<Equipment>                    OwnedEquipments    = new List<Equipment>();
    [JsonIgnore]
    public Dictionary<EquipSlotType, Equipment> EquippedEquipments = new Dictionary<EquipSlotType, Equipment>();

    // 전직(Class): 직업 숙련도 0~100. 몬스터 처치로 누적, 100 도달 시 수도원 [축성]으로 직업 능력 습득.
    public int ClassProficiency = 0;

    // 방치 보상: 마지막으로 게임을 본 시각(UTC Ticks)과 그때 머물던 전투 지역.
    //  0이면 최초 실행 — 보상을 주지 않는다(첫 실행에 1970년부터의 보상이 쏟아지는 것 방지).
    public long LastSeenTicks = 0;
    public int  LastBattleAreaId = 0;

    // 대장간 시드 아이템(주문서/거울) 최초 1회 지급 여부. ponytail: 정식 획득처(드롭/상점)는 후속.
    public bool ForgeItemsGranted = false;

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
    public List<int>    MetQuestConditions = new List<int>(); // MULTI 퀘스트의 충족 조건 Order 집합(현재 퀘스트 기준). 대화 중 종료 후 복원용.
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

    // 최하단 탭 NEW 뱃지 on/off 비트마스크 — 게임 재시작 후에도 유지.
    //  Bag/Bestiary/Status는 아이템 IsNew/관심/스탯포인트에서 파생되어 이미 저장되므로 제외.
    //  ShowNewBadge로 직접 켜는 Potion/Equipment/Skill만 여기 저장한다.
    public int NewBadgeTabs = 0;

    // 제작의뢰 팝업 내 SPD 포션 해금 연출이 이미 재생되었는지.
    // false = 아직 미공개 (잠긴 상태로 시작 → 팝업 오픈 시 1회 해금 애니)
    // true  = 공개됨 (바로 해금 상태로 표시)
    public bool SpdPotionRevealed = false;

    // UI_PotionPopup(하단 탭) 내 SPD 슬롯 해금 연출이 재생되었는지.
    // 제작의뢰 해금 후 물약 팝업을 첫 번째로 열 때 1회 연출.
    public bool SpdPotionPopupAnimPlayed = false;

    // 슬41/42: 자동물약 설정 (세이브 — 기존 PlayerPrefs에서 이관)
    public bool  AutoPotionMemorized = false;   // 물약주입기 기술 습득 시 true (자동물약 사용 가능)
    public bool  AutoPotionOn        = false;   // 자동물약 On/Off
    public float AutoPotionHpRatio   = 0.5f;    // 자동 사용 기준 HP 비율(0~1)

    // 슬35: 제작의뢰 행운물약 / 물약주입기 기술 해금 (신속물약 SpdPotionRevealed 패턴).
    //  *Unlocked = 컨텐츠(퀘스트/등급)로 해금됐는가(잠금 해제 게이트). *Revealed = 해금 모션을 봤는가(1회 재생).
    //  물약주입기 기술의 잠금 게이트는 AutoPotionMemorized(기술 습득)이며, 모션 1회 재생 여부만 별도 보관.
    public bool LuckPotionUnlocked       = false;
    public bool LuckPotionRevealed       = false;
    public bool AutoPotionSkillRevealed  = false;

    // [치트] 만능 키 — 모든 잠금 게이트를 무시한다. 세이브에 남겨 재시작해도 유지된다.
    //  (여정 노드 조건처럼 "데이터를 채우는" 게 아니라 "판정을 건너뛰는" 종류의 해금은 이 플래그로 처리)
    public bool AllUnlocked = false;

    // 마지막 협회 임무를 마친 뒤 엔딩(감사 인사 + 크레딧)을 한 번 봤는가
    public bool EndingSeen = false;
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
        set { _gameData._isBGSoundOn = value; SaveGame(); Managers.Sound?.ApplyBgmEnabled(value); }
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
        // 이름 입력 기능이 없어 저장값은 늘 기본값("여행자")이다 — 표시할 때 언어에 맞춘다.
        get => string.IsNullOrEmpty(_gameData.PlayerName) || _gameData.PlayerName == "여행자"
            ? Util.Localize(LocalizeKey.DefaultPlayerNameText)
            : _gameData.PlayerName;
        set { _gameData.PlayerName = value; }
    }
    public int AdventurerRank
    {
        get => _gameData.AdventurerRank;
        set
        {
            int next = Mathf.Clamp(value, 0, 99);
            if (_gameData.AdventurerRank == next) return;
            _gameData.AdventurerRank = next;
            SaveGame();
            OnAdventurerRankChanged?.Invoke();
        }
    }

    public event Action OnAdventurerRankChanged;

    // 전직: 직업 숙련도 (0~CLASS_PROFICIENCY_MAX). 몬스터 처치 시 AddClassProficiency로 누적.
    public int ClassProficiency => _gameData.ClassProficiency;
    public event Action OnClassProficiencyChanged;
    public void AddClassProficiency(int amount)
    {
        int next = Mathf.Clamp(_gameData.ClassProficiency + amount, 0, CLASS_PROFICIENCY_MAX);
        if (next == _gameData.ClassProficiency) return;
        _gameData.ClassProficiency = next;
        SaveGame();
        OnClassProficiencyChanged?.Invoke();
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

    // 슬41/42: 자동물약 설정
    public bool AutoPotionMemorized
    {
        get => _gameData.AutoPotionMemorized;
        set { _gameData.AutoPotionMemorized = value; SaveGame(); }
    }
    public bool AutoPotionOn
    {
        get => _gameData.AutoPotionOn;
        set { _gameData.AutoPotionOn = value; SaveGame(); }
    }
    public float AutoPotionHpRatio
    {
        get => Mathf.Clamp01(_gameData.AutoPotionHpRatio);
        set { _gameData.AutoPotionHpRatio = Mathf.Clamp01(value); SaveGame(); }
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
    //  처음 실행은 기기 언어를 따른다(한국어 기기만 한국어, 나머지는 영어).
    //  Util.Localize 가 문자열마다 읽으므로 PlayerPrefs(윈도우는 레지스트리)를 매번 읽지 않고 캐시한다.
    Define.Language? _language;
    public Define.Language Language
    {
        get
        {
            if (_language == null)
            {
                var systemDefault = Application.systemLanguage == SystemLanguage.Korean ? Define.Language.Korean : Define.Language.English;
                _language = (Define.Language)PlayerPrefs.GetInt("Language", (int)systemDefault);
            }
            return _language.Value;
        }
        set
        {
            _language = value;
            PlayerPrefs.SetInt("Language", (int)value);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
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

    // MULTI 퀘스트 충족 조건(Order) 영속 집합. QuestManager가 복원/저장에 사용.
    public List<int> MetQuestConditions => _gameData.MetQuestConditions;

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

    // 슬35: 행운물약/물약주입기 기술 해금 플래그
    public bool LuckPotionUnlocked
    {
        get => _gameData.LuckPotionUnlocked;
        set { _gameData.LuckPotionUnlocked = value; SaveGame(); }
    }
    public bool LuckPotionRevealed
    {
        get => _gameData.LuckPotionRevealed;
        set { _gameData.LuckPotionRevealed = value; SaveGame(); }
    }
    public bool AutoPotionSkillRevealed
    {
        get => _gameData.AutoPotionSkillRevealed;
        set { _gameData.AutoPotionSkillRevealed = value; SaveGame(); }
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

    /// <summary>
    /// 이미 손에 넣은 것에 해당하는 탭을 연다.
    ///
    /// 가방·물약은 퀘스트가 열어 주지만 **장비·능력·도감은 프로덕션에 여는 경로가 아예 없었다**.
    /// 메인 퀘스트를 끝까지 깨도 그 셋은 영영 잠겨 있었다(치트와 에디터 메뉴만 열 수 있었다).
    /// 소유 사실 자체를 조건으로 삼으면 별도 퀘스트 행 없이도 자연스럽게 열리고,
    /// 이미 진행한 세이브도 다음 갱신 때 스스로 복구된다.
    /// </summary>
    /// <returns>이번에 새로 열린 탭이 있으면 true</returns>
    public bool RefreshEarnedTabUnlocks()
    {
        int before = _gameData.UnlockedBtmButtons;

        if (_gameData.OwnedEquipments.Count > 0)      UnlockBtmButton(Define.TabType.Equipment);
        if (LearnedAbilities.Count > 0
            || _gameData.EquippedAbilities.Count > 0
            || Managers.Ability.IsCompanionGranted(Managers.Ability.MinionSkillId(CompanionMinMonId)))
            UnlockBtmButton(Define.TabType.Skill);
        if (DiscoveredMonsterIds.Count > 0)           UnlockBtmButton(Define.TabType.Bestiary);

        return before != _gameData.UnlockedBtmButtons;
    }

    // 최하단 탭 NEW 뱃지 저장 조회/설정(Potion/Equipment/Skill). Bag/Bestiary/Status는 보유 데이터에서 파생.
    public bool IsTabNew(Define.TabType tab)
    {
        return (_gameData.NewBadgeTabs & (1 << (int)tab)) != 0;
    }

    public void SetTabNew(Define.TabType tab, bool on)
    {
        int mask = 1 << (int)tab;
        bool cur = (_gameData.NewBadgeTabs & mask) != 0;
        if (cur == on) return;
        if (on) _gameData.NewBadgeTabs |= mask;
        else    _gameData.NewBadgeTabs &= ~mask;
        SaveGame();
    }

    public void AddUnlockedBtn(string btnId)
    {
        if (!_gameData.UnlockedBtnIds.Contains(btnId))
        {
            _gameData.UnlockedBtnIds.Add(btnId);
            // 수도원이 열리는 순간 첫 축성은 바로 할 수 있게 한다(ADR0029). 예전엔 해금 시점 숙련도가
            //  절반쯤(실측 48/100)이라 직업 능력을 보여 주고도 수십 마리를 더 잡아야 첫 선택을 할 수 있었다.
            if (btnId == UNLOCK_MONASTERY_BUTTON && !HasAnyClassAbility())
                AddClassProficiency(CLASS_PROFICIENCY_MAX);
            SaveGame();
        }
    }

    bool HasAnyClassAbility()
        => _gameData.LearnedAbilities.Exists(id =>
            Managers.Data.SkillDic.TryGetValue(id, out var skill) && skill.Source == AbilitySource.Class);

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
        FlushSave();    // 타이틀 재진입 등으로 다시 불릴 때 아직 안 쓴 변경을 먼저 기록한다
        _path = SavePath;

        if (LoadGame())
        {
            EnsurePotionState();
            EnsureStarterEquipment();   // 기존 세이브 마이그레이션(장비 없으면 시작 검 지급)
            return;
        }

        // 새 게임 — 세이브 파일이 없을 때만 온다(예전의 PlayerPrefs "ISFIRST" 판정은 폐기).
        FirstSetting();
        EnsurePotionState();
        EnsureStarterEquipment();
        SaveGameNow();
    }

    // ────────────────────────────────────────
    //  방치 보상 (오프라인 수익)
    // ────────────────────────────────────────
    //  방치형의 핵심 축. 껐다 켜는 사이에도 마지막으로 사냥하던 지역에서 계속 싸운 것으로 치고
    //  경험치·골드를 정산한다. 실제 전투보다 효율을 낮춰(OFFLINE_RATE) 직접 플레이할 이유를 남긴다.
    public const float OFFLINE_MAX_HOURS = 8f;    // 상한 — 하루 종일 켜둬야 이득인 구조를 피한다
    public const float OFFLINE_RATE      = 0.5f;  // 실제 사냥 대비 효율
    const float OFFLINE_KILL_INTERVAL    = 4f;    // 처치 1회당 초 (스폰 2s + 접근·교전)
    const float OFFLINE_MIN_MINUTES      = 1f;    // 이 미만이면 정산하지 않는다

    /// <summary>정산 결과. 아무것도 못 받으면 Seconds == 0.</summary>
    public struct OfflineReward
    {
        public double Seconds;
        public int    Kills;
        public float  Exp;
        public int    Gold;
        public bool   Capped;      // 상한에 걸렸는가 (UI에서 안내용)
    }

    public OfflineReward LastOfflineReward { get; private set; }

    /// <summary>마지막 접속 이후 경과분을 정산해 지급한다. Init에서 1회 호출.</summary>
    public OfflineReward ClaimOfflineReward()
    {
        var result = new OfflineReward();
        if (_gameData.LastSeenTicks <= 0)
        {
            MarkSeen();                       // 최초 실행 — 기준 시각만 남긴다
            return result;
        }

        var last = new System.DateTime(_gameData.LastSeenTicks, System.DateTimeKind.Utc);
        double seconds = (System.DateTime.UtcNow - last).TotalSeconds;
        if (seconds < OFFLINE_MIN_MINUTES * 60.0)
        {
            MarkSeen();
            return result;
        }

        double capped = System.Math.Min(seconds, OFFLINE_MAX_HOURS * 3600.0);
        result.Capped  = seconds > capped;
        result.Seconds = capped;

        // 마지막으로 있던 전투 지역의 몬스터 평균으로 정산.
        //  지역 정보가 없으면(전투를 한 번도 안 했으면) 지급 대상이 없다.
        if (!TryGetAreaAverage(_gameData.LastBattleAreaId, out float avgExp, out float avgGold))
        {
            MarkSeen();
            return result;
        }

        result.Kills = Mathf.FloorToInt((float)(capped / OFFLINE_KILL_INTERVAL) * OFFLINE_RATE);
        if (result.Kills <= 0)
        {
            MarkSeen();
            return result;
        }
        result.Exp  = avgExp * result.Kills;
        result.Gold = Mathf.RoundToInt(avgGold * result.Kills);

        Gold += result.Gold;                       // setter가 저장까지 처리
        Player?.GainExp(result.Exp);
        MarkSeen();

        LastOfflineReward = result;
        Debug.Log($"[방치보상] {capped / 3600.0:F1}시간 → {result.Kills}처치, EXP {result.Exp:N0}, 골드 {result.Gold:N0}");
        return result;
    }

    // ── 방치 누적 (게임을 켜둔 채 전투 구역 밖에 있을 때) ──
    //  전투 구역을 벗어나면 CoBattleLoop이 멈춰 진행이 완전히 정지한다.
    //  방치형에서 이건 결함이다 — 마을에 있든 메뉴를 보든 사냥은 계속돼야 한다.
    //  그래서 전투 밖에서는 오프라인과 같은 효율로 누적하고, 1마리분이 찰 때마다 지급한다.
    //  (전투 중에는 실제 전투가 100% 효율로 보상하므로 누적하지 않는다 — 이중 지급 방지)
    bool  _inBattleArea = false;
    float _idleKillProgress = 0f;

    /// <summary>전투 구역 안/밖 전환. UI_GameScene.EnterArea에서 호출.</summary>
    public void SetInBattleArea(bool inBattle)
    {
        _inBattleArea = inBattle;
        _idleKillProgress = 0f;
        OnStageChanged?.Invoke();   // 스테이지 막대는 전투 구역에서만 보인다
    }
    public bool InBattleArea => _inBattleArea;

    /// <summary>Managers.Update에서 매 프레임 호출. 전투 밖일 때만 누적한다.</summary>
    public void TickIdle(float deltaTime)
    {
        if (_inBattleArea || Player == null || _gameData == null) return;
        if (_gameData.LastBattleAreaId <= 0) return;      // 전투 경험이 없으면 누적 대상 없음

        _idleKillProgress += deltaTime / OFFLINE_KILL_INTERVAL * OFFLINE_RATE;
        _idleNotifyTimer += deltaTime;
        if (_idleKillProgress < 1f) { FlushIdleBatch(); return; }

        int kills = Mathf.FloorToInt(_idleKillProgress);
        _idleKillProgress -= kills;

        if (!TryGetAreaAverage(_gameData.LastBattleAreaId, out float avgExp, out float avgGold)) return;

        int gold = Mathf.RoundToInt(avgGold * kills);
        float exp = avgExp * kills;

        Gold += gold;                 // setter가 OnResourceChanged를 쏴 HUD 골드가 실시간으로 오른다
        Player.GainExp(exp);          // OnExpChanged로 EXP 바도 함께 오른다
        IdleKillCount += kills;
        OnIdleAccrued?.Invoke(kills);

        // 숫자만 오르면 왜 오르는지 알 수 없다 — 일정 주기마다 묶어서 알린다(매번 띄우면 스팸).
        _batchKills += kills; _batchGold += gold; _batchExp += exp;
        FlushIdleBatch();
    }

    const float IDLE_NOTIFY_INTERVAL = 60f;   // 방치 요약 알림 주기(초)
    float _idleNotifyTimer;
    int   _batchKills, _batchGold;
    float _batchExp;

    void FlushIdleBatch()
    {
        if (_idleNotifyTimer < IDLE_NOTIFY_INTERVAL) return;
        _idleNotifyTimer = 0f;
        if (_batchKills <= 0) return;

        string area = "";
        if (Managers.Data?.TravelNodeDic != null &&
            Managers.Data.TravelNodeDic.TryGetValue(_gameData.LastBattleAreaId, out var node))
            area = Util.Localize(node.Name);

        OnIdleBatch?.Invoke(new OfflineReward
        {
            Kills = _batchKills, Gold = _batchGold, Exp = _batchExp, Seconds = IDLE_NOTIFY_INTERVAL,
        }, area);

        _batchKills = 0; _batchGold = 0; _batchExp = 0f;
    }

    /// <summary>이번 세션에서 방치로 처치한 누적 수 (HUD 표기용).</summary>
    public int IdleKillCount { get; private set; }
    public event System.Action<int> OnIdleAccrued;
    /// <summary>방치 요약 알림 (주기적). 인자: 이번 구간 성과, 지역 이름.</summary>
    public event System.Action<OfflineReward, string> OnIdleBatch;

    /// <summary>해당 전투 노드에 출현하는 몬스터의 평균 경험치·골드.</summary>
    public bool TryGetAreaAverage(int nodeId, out float avgExp, out float avgGold)
    {
        avgExp = 0f; avgGold = 0f;
        if (Managers.Data?.TravelNodeDic == null) return false;
        if (!Managers.Data.TravelNodeDic.TryGetValue(nodeId, out var tn)) return false;

        int n = 0;
        foreach (var c in Managers.Data.CreatureDic.Values)
        {
            if (c == null || c.Habitat != tn.Name || c.EncounterRate <= 0f) continue;
            avgExp += c.TotalExp; avgGold += c.RewardGold; n++;
        }
        if (n == 0) return false;
        avgExp /= n; avgGold /= n;
        return true;
    }

    /// <summary>지금을 '마지막으로 본 시각'으로 기록. 일시정지/종료/지역이동 시 호출.</summary>
    public void MarkSeen()
    {
        _gameData.LastSeenTicks = System.DateTime.UtcNow.Ticks;
        SaveGameNow();  // 일시정지/종료 직전에 불리므로 모아 쓰기를 기다리지 않는다
    }

    /// <summary>전투 지역 진입 시 호출 — 방치 보상 정산 기준이 된다.</summary>
    public void MarkBattleArea(int travelNodeId)
    {
        if (travelNodeId <= 0 || _gameData.LastBattleAreaId == travelNodeId) return;
        _gameData.LastBattleAreaId = travelNodeId;
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
            Managers.Ability?.OnEquipChanged(old, false);     // 슬38: 이전 장비 능력 기억 정리
        }

        EquippedEquipments[equipment.SlotType] = equipment;
        equipment.IsEquipped = true;
        Managers.Ability?.OnEquipChanged(equipment, true);    // 슬38: 착용 = 장비 능력 무점유 자동 기억(점유 환불)
        EquipInfoChanged?.Invoke();
        SaveGame();
        Player?.RefreshStats(fullHeal: false);                               // 장비 기본/강화 스탯 반영
    }

    public void UnEquipItem(Equipment equipment)
    {
        if (EquippedEquipments.ContainsKey(equipment.SlotType))
        {
            EquippedEquipments[equipment.SlotType].IsEquipped = false;
            EquippedEquipments.Remove(equipment.SlotType);
            Managers.Ability?.OnEquipChanged(equipment, false);   // 슬38: 해제 = 장비 능력 소멸(+9는 잔존, 기억만 해제)
            EquipInfoChanged?.Invoke();
            SaveGame();
            Player?.RefreshStats(fullHeal: false);
        }
    }

    // 장비 획득 — 유일한 런타임 추가 경로. (퀘스트 보상/드롭 연결은 후속)
    public Equipment AddEquipment(int dataId)
    {
        var equip = new Equipment(dataId);
        OwnedEquipments.Add(equip);
        SetTabNew(TabType.Equipment, true);
        EquipInfoChanged?.Invoke();
        SaveGame();
        return equip;
    }

    // ── 대장간: 강화 (VS 원작 Enhance Chance Cal 이식) ──
    public class EnhanceResult
    {
        public bool  Success;
        public int   DuriLoss;      // 실패 시 내구도 감소량 (성공 시 0)
        public int   PotenGain;     // 성공 시 잠재력 증가량 (실패 시 0)
        public float ChancePct;     // 굴림에 사용된 성공확률(%)
        public int   PrevLevel;     // 연출용 before
        public int   PrevDurability;
        public int   GoldCost;      // 이번 시도에 소모한 골드
    }

    // 현재 강화도/잠재력 기준 성공확률(%). Low→High 보간 = 잠재력/만충, 만충 초과분은 +0.2%/pt.
    public float GetEnhanceChance(Equipment equip)
    {
        int lv = Mathf.Clamp(equip.EnhanceLevel, 0, ENHANCE_MAX_LEVEL - 1);
        float t = Mathf.Clamp01(equip.Potential / ENHANCE_POTEN_FULL);
        float chance = Mathf.Lerp(ENHANCE_CHANCE_LOW[lv], ENHANCE_CHANCE_HIGH[lv], t);
        float over = Mathf.Max(0f, equip.Potential - ENHANCE_POTEN_FULL);
        return Mathf.Min(100f, chance + over * ENHANCE_POTEN_OVER_BONUS);
    }

    // 강화 골드 비용. 방치로 쌓인 골드의 유일한 소비처다 —
    //  이게 없으면 골드가 순수 누적치가 되어 "방치 → 성장" 순환이 끊긴다.
    //  다음 단계일수록 가파르게(단계^1.8) 올려 후반 골드가 남아돌지 않게 한다.
    public const int ENHANCE_GOLD_BASE = 120;

    public int GetEnhanceCost(Equipment equip)
    {
        if (equip == null) return 0;
        int next = equip.EnhanceLevel + 1;                       // 다음 강화 단계 (1~9)
        int gradeIdx = equip.EquipmentData != null ? (int)equip.EquipmentData.Grade : 0;
        float gradeMul = 1f + gradeIdx * 0.6f;                   // 상위 등급일수록 비싸게
        return Mathf.RoundToInt(ENHANCE_GOLD_BASE * Mathf.Pow(next, 1.8f) * gradeMul);
    }

    public bool CanEnhance(Equipment equip, int scrollId)
        => equip != null
        && equip.EnhanceLevel < ENHANCE_MAX_LEVEL
        && equip.Durability > 0
        && HasItem(scrollId)
        && Gold >= GetEnhanceCost(equip)
        && HasEnhanceMaterial(equip);

    /// <summary>
    /// 강화 누적 보너스(물공/치명/마공). 장착 스탯·상세창·강화 결과창이 **같은 값**을 쓰게 하는 단일 출처.
    ///
    /// ⚠️ 예전엔 같은 루프가 세 군데(ApplyEquipmentStats / 상세창 SumEnhanceBonus / 결과창 SumAtkBonus)에
    ///    복제돼 있었고, 그중 결과창만 규칙이 달라 **같은 장비를 두 창이 다른 숫자로 보여줬다**.
    /// </summary>
    public (int atk, float crit, int magic) GetEnhanceBonus(int equipDataId, int level)
    {
        int atk = 0, magic = 0;
        float crit = 0f;
        if (Managers.Data?.EquipEnhanceDic == null) return (0, 0f, 0);

        foreach (var e in Managers.Data.EquipEnhanceDic.Values)
        {
            if (e.EquipDataId != equipDataId || e.Level > level) continue;
            atk   += e.AtkBonus;
            crit  += e.CritBonus;
            magic += e.MagicAtkBonus;
        }
        return (atk, crit, magic);
    }

    /// <summary>다음 단계 강화가 요구하는 재료 (없으면 (0,0) — 재료 없이 주문서만으로 강화).</summary>
    public (int itemId, int count) GetEnhanceMaterial(Equipment equip)
    {
        if (equip == null || Managers.Data?.EquipEnhanceDic == null) return (0, 0);
        int next = equip.EnhanceLevel + 1;
        foreach (var enh in Managers.Data.EquipEnhanceDic.Values)
            if (enh.EquipDataId == equip.DataId && enh.Level == next)
                return (enh.RequiredMaterialId, enh.RequiredMaterialCount);
        return (0, 0);
    }

    public bool HasEnhanceMaterial(Equipment equip)
    {
        var (id, need) = GetEnhanceMaterial(equip);
        return id <= 0 || need <= 0 || GetItemCount(id) >= need;
    }

    // 강화 실행: 주문서 1장 소모 → 확률 굴림 → 성공 시 +1단계/잠재력 증가, 실패 시 내구도 감소(주문서별).
    //  +9 도달 시 장비 능력 영구화(슬38). 결과는 UI 연출용으로 반환.
    public EnhanceResult EnhanceEquipment(Equipment equip, int scrollId)
    {
        if (!CanEnhance(equip, scrollId)) return null;

        var r = new EnhanceResult
        {
            ChancePct      = GetEnhanceChance(equip),
            PrevLevel      = equip.EnhanceLevel,
            PrevDurability = equip.Durability,
        };

        r.GoldCost = GetEnhanceCost(equip);
        Gold -= r.GoldCost;          // CanEnhance에서 잔액을 이미 확인함
        RemoveItem(scrollId, 1);

        // 요구 재료 소모. 데이터가 비어 있던 시절엔 강화가 골드와 주문서만 먹었다 —
        //  사냥으로 모은 재료가 강화로 이어지지 않아 성장 순환이 끊겨 있었다.
        var (matId, matNeed) = GetEnhanceMaterial(equip);
        if (matId > 0 && matNeed > 0) RemoveItem(matId, matNeed);

        bool guaranteed = scrollId == SCROLL_IGNUS_ID;   // 이그누스의 인정 = 확정 강화
        r.Success = guaranteed || UnityEngine.Random.value * 100f < r.ChancePct;

        if (r.Success)
        {
            equip.EnhanceLevel++;
            int gradeIdx = equip.EquipmentData != null
                ? Mathf.Clamp((int)equip.EquipmentData.Grade, 0, ENHANCE_POTEN_BY_GRADE.Length - 1)
                : 0;
            r.PotenGain = ENHANCE_POTEN_BY_GRADE[gradeIdx];
            equip.Potential += r.PotenGain;

            // +9 도달: 장비 능력 영구화 (기술창 잔존 — 슬38)
            if (equip.EnhanceLevel >= ENHANCE_MAX_LEVEL)
                Managers.Ability?.NotifyEquipMaxEnhanced(equip);
        }
        else
        {
            int idx = System.Array.IndexOf(ENHANCE_SCROLL_IDS, scrollId);
            r.DuriLoss = idx >= 0 ? ENHANCE_SCROLL_DURI_LOSS[idx] : ENHANCE_SCROLL_DURI_LOSS[0];
            equip.Durability = Mathf.Max(0, equip.Durability - r.DuriLoss);
        }

        SaveGame();
        if (r.Success) OnEquipmentForged?.Invoke(equip);
        EquipInfoChanged?.Invoke();
        if (equip.IsEquipped) Player?.RefreshStats(fullHeal: false);   // 강화/내구도 변화 → 스탯·능력단계 갱신
        return r;
    }

    // ── 대장간: 내구도 복구 — 거울(MOR) 또는 재료 소모로 +25. ──
    public bool RestoreDurability(Equipment equip, bool useMirror)
    {
        if (equip == null || equip.EquipmentData == null) return false;
        int max = equip.EquipmentData.MaxDurability;
        if (equip.Durability >= max) return false;

        if (useMirror)
        {
            // 축복의 거울이 있으면 그쪽을 먼저 쓴다 — 완전 회복이라 한 번에 끝난다.
            if (HasItem(BMOR_ITEM_ID))
            {
                RemoveItem(BMOR_ITEM_ID, 1);
                equip.Durability = max;
            }
            else
            {
                if (!HasItem(MOR_ITEM_ID)) return false;
                RemoveItem(MOR_ITEM_ID, 1);
                equip.Durability = Mathf.Min(max, equip.Durability + MOR_RESTORE_AMOUNT);
            }
        }
        else
        {
            if (!HasItem(RESTORE_MATERIAL_ID, RESTORE_MATERIAL_COUNT)) return false;
            RemoveItem(RESTORE_MATERIAL_ID, RESTORE_MATERIAL_COUNT);
            equip.Durability = Mathf.Min(max, equip.Durability + RESTORE_MATERIAL_AMOUNT);
        }

        SaveGame();
        EquipInfoChanged?.Invoke();
        if (equip.IsEquipped) Player?.RefreshStats(fullHeal: false);   // 최대 내구도 도달 → 능력단계 +1 반영(슬49)
        return true;
    }

    // 장착 장비의 기본 스탯 + 강화 보너스(EquipEnhanceData 단계별 증가분 누적) 가산.
    //  PlayerController.RefreshStats에서 호출.
    public void ApplyEquipmentStats(CreatureController c)
    {
        if (c == null || Managers.Data?.EquipmentDic == null) return;
        foreach (var equip in EquippedEquipments.Values)
        {
            var data = equip.EquipmentData;
            if (data == null) continue;
            c.PhysicalAtk += data.BaseAtk;
            c.CritRate    += data.BaseCritRate;
            c.MagicAtk    += data.BaseMagicAtk;

            if (Managers.Data.EquipEnhanceDic == null) continue;
            foreach (var enh in Managers.Data.EquipEnhanceDic.Values)
            {
                if (enh.EquipDataId != equip.DataId || enh.Level > equip.EnhanceLevel) continue;
                c.PhysicalAtk += enh.AtkBonus;
                c.CritRate    += enh.CritBonus;
                c.MagicAtk    += enh.MagicAtkBonus;
            }
        }
    }

    // 로드 후 EquippedEquipments를 OwnedEquipments의 IsEquipped 플래그로 재구성 (참조 일원화).
    void RebuildEquippedFromOwned()
    {
        _gameData.EquippedEquipments.Clear();
        foreach (var equip in _gameData.OwnedEquipments)
        {
            if (!equip.IsEquipped) continue;
            if (_gameData.EquippedEquipments.ContainsKey(equip.SlotType))
            {
                equip.IsEquipped = false;   // 슬롯 중복(구세이브) — 첫 항목만 유지
                continue;
            }
            _gameData.EquippedEquipments[equip.SlotType] = equip;
        }
    }

    // 시작 장비 지급 — 장비가 하나도 없으면 여행자의 검(30001) 지급 + 자동 장착. (기존 세이브 마이그레이션 겸용)
    void EnsureStarterEquipment()
    {
        // 시작 장비는 **6슬롯 T1 한 벌**을 지급한다.
        //  검 하나만 주면 장비창의 다른 슬롯·티어 목록이 전부 비어 있어
        //  "장비 팝업이 아무 동작도 안 한다"로 보인다(누를 대상 자체가 없다).
        //  장비 60종을 만들었지만 드롭·상점 같은 정식 획득 경로가 아직 없으므로
        //  최소한 한 벌은 손에 쥐고 시작하게 한다. ponytail: 정식 획득처는 후속.
        if (_gameData.OwnedEquipments.Count == 0 && Managers.Data?.EquipmentDic != null)
        {
            for (int id = 30001; id <= 30006; id++)     // T1 6슬롯(무기/투구/갑옷/장갑/각반/부적)
            {
                if (!Managers.Data.EquipmentDic.ContainsKey(id)) continue;

                var eq = new Equipment(id);
                _gameData.OwnedEquipments.Add(eq);
                if (!_gameData.EquippedEquipments.ContainsKey(eq.SlotType))
                {
                    eq.IsEquipped = true;
                    _gameData.EquippedEquipments[eq.SlotType] = eq;
                }
            }
            SaveGame();
        }

        // 대장간 시드 아이템 최초 1회 지급 — 주문서(기본10/고급3)와 복원의 거울(3).
        // ponytail: 정식 획득처(몬스터 드롭/제작)는 후속 — 지급량은 테스트 밸런스 시드.
        if (!_gameData.ForgeItemsGranted && Managers.Data?.ItemDic != null && Managers.Data.ItemDic.ContainsKey(SCROLL_BASIC_ID))
        {
            _gameData.ForgeItemsGranted = true;
            AddItem(SCROLL_BASIC_ID, 10);
            AddItem(SCROLL_ADVANCED_ID, 3);
            AddItem(MOR_ITEM_ID, 3);
        }
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

    // 가방을 열었다 닫을 때 — 일반/귀중품 모든 아이템 NEW 표시 일괄 해제.
    // (개별 확인 없이도 닫는 순간 가방 버튼/내부 아이템/귀중품 NEW가 모두 사라지도록)
    public void ClearAllItemNew()
    {
        bool changed = false;
        foreach (var item in Inventory)
        {
            if (item.isNew) { item.isNew = false; changed = true; }   // backing 필드 직접 — 이벤트/세이브 스팸 방지
        }
        if (changed)
        {
            OnInventoryNewChanged?.Invoke();
            SaveGame();
        }
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

    #region 상점 (Shop)
    //  골드 소비처가 강화와 주문제작 둘뿐이라 사냥으로 쌓인 골드가 갈 곳이 없었다.
    //  구매/판매 판정은 여기 한 곳에 둔다 — UI가 각자 검사하면 한쪽만 고쳐지는 일이 생긴다.

    /// <summary>지금 상점에 진열되는 목록. 해금 레벨을 넘긴 것만.</summary>
    public List<Data.ShopItemData> GetShopStock()
    {
        var list = new List<Data.ShopItemData>();
        if (Managers.Data.ShopItemDic == null) return list;

        foreach (var s in Managers.Data.ShopItemDic.Values)
            if (Level >= s.UnlockLevel)
                list.Add(s);

        list.Sort((a, b) => a.DataId.CompareTo(b.DataId));
        return list;
    }

    /// <summary>되팔 수 있는 것 중 **실제로 가진 것**만. 목록이 비면 판매 탭이 빈 화면이 된다.</summary>
    public List<Data.ShopItemData> GetSellableStock()
    {
        var list = new List<Data.ShopItemData>();
        if (Managers.Data.ShopItemDic == null) return list;

        foreach (var s in Managers.Data.ShopItemDic.Values)
            if (s.SellPrice > 0 && GetItemCount(s.ItemId) > 0)
                list.Add(s);

        list.Sort((a, b) => a.DataId.CompareTo(b.DataId));
        return list;
    }

    /// <summary>구매가 막힌 이유. 살 수 있으면 빈 문자열.</summary>
    public string GetBuyBlockReason(Data.ShopItemData shop, int count = 1)
    {
        if (shop == null) return Util.Localize(LocalizeKey.ShopNotForSaleText);
        if (Level < shop.UnlockLevel) return string.Format(Util.Localize(LocalizeKey.RequiredLevelFormat), shop.UnlockLevel);
        if (Gold < shop.Price * count) return Util.Localize(Define.LocalizeKey.ShopNotEnoughGoldText);
        return string.Empty;
    }

    public bool BuyShopItem(Data.ShopItemData shop, int count = 1)
    {
        if (count <= 0) return false;
        if (!string.IsNullOrEmpty(GetBuyBlockReason(shop, count))) return false;

        Gold -= shop.Price * count;
        AddItem(shop.ItemId, count);
        SaveGame();
        return true;
    }

    public bool SellShopItem(Data.ShopItemData shop, int count = 1)
    {
        if (shop == null || count <= 0) return false;
        if (shop.SellPrice <= 0) return false;
        if (GetItemCount(shop.ItemId) < count) return false;

        RemoveItem(shop.ItemId, count);
        Gold += shop.SellPrice * count;
        SaveGame();
        return true;
    }
    #endregion

    #region 훈련 (v2 — 골드 무한 강화, ADR0030)
    // 골드의 목적지. 30초마다 무언가를 올리게 하되, 상한(레벨 × 2)으로 지역 난이도를 지킨다.
    public const int TRAINING_STAT_COUNT = 4;
    public event Action OnTrainingChanged;

    public int TrainingLevel(TrainingStat stat) => _gameData.TrainingLevels[(int)stat];
    public int TrainingCap => Mathf.FloorToInt(Level * Managers.Data.Balance(BalanceKey.TRAIN_CAP_PER_LEVEL, 2f));

    // 다음 단계(nextLevel)로 올리는 비용 = BASE + COEF × nextLevel^POW
    public int TrainingCost(int nextLevel)
    {
        var d = Managers.Data;
        double cost = d.Balance(BalanceKey.TRAIN_COST_BASE, 10f)
                    + d.Balance(BalanceKey.TRAIN_COST_COEF, 0.8f) * Math.Pow(nextLevel, d.Balance(BalanceKey.TRAIN_COST_POW, 1.65f));
        return (int)Math.Min(int.MaxValue, Math.Round(cost));
    }

    // 단계당 가산(%, 치명피해는 %p)
    public float TrainingPerLevel(TrainingStat stat) => stat switch
    {
        TrainingStat.Attack     => Managers.Data.Balance(BalanceKey.TRAIN_ATK_PCT, 0.2f),
        TrainingStat.Health     => Managers.Data.Balance(BalanceKey.TRAIN_HP_PCT, 0.25f),
        TrainingStat.Defense    => Managers.Data.Balance(BalanceKey.TRAIN_DEF_PCT, 0.15f),
        _                       => Managers.Data.Balance(BalanceKey.TRAIN_CRITDMG_PCT, 0.5f),
    };

    public float TrainingBonus(TrainingStat stat) => TrainingLevel(stat) * TrainingPerLevel(stat);

    public bool TryTrain(TrainingStat stat)
    {
        int next = TrainingLevel(stat) + 1;
        if (next > TrainingCap) return false;
        int cost = TrainingCost(next);
        if (Gold < cost) return false;
        Gold -= cost;
        _gameData.TrainingLevels[(int)stat] = next;
        SaveGame();
        Player?.RefreshStats(fullHeal: false);
        OnTrainingChanged?.Invoke();
        return true;
    }

    // PlayerController.RefreshStats 가 장비·능력까지 더한 뒤 부른다 — 임시 버프보다 먼저 곱한다.
    public void ApplyTraining(PlayerController p)
    {
        if (p == null || _gameData.TrainingLevels == null) return;
        float atk = 1f + TrainingBonus(TrainingStat.Attack) / 100f;
        p.PhysicalAtk *= atk;
        p.MagicAtk    *= atk;
        p.MaxHp       *= 1f + TrainingBonus(TrainingStat.Health) / 100f;
        p.Def         *= 1f + TrainingBonus(TrainingStat.Defense) / 100f;
        p.CritDamage  += TrainingBonus(TrainingStat.CritDamage);
    }
    #endregion

    #region 스테이지·보스 (v2 — 10마리 → 보스 → 다음, ADR0030)
    // 사냥터(전투 노드 125곳) = 스테이지 '필드-층'(1-1 ~ 25-5). 정해진 수를 잡으면 보스, 보스를 잡으면 첫 돌파 크리스탈.
    //  규칙·저장은 여기, 전투 흐름(스폰·제한 시간)은 UI_BattleBtnGroup, 표시는 UI_StageBar.
    public event Action OnStageChanged;

    public int  StageKillsToBoss => Mathf.Max(1, Mathf.RoundToInt(Managers.Data.Balance(BalanceKey.STAGE_KILLS_TO_BOSS, 10f)));
    public int  StageKills(int nodeId) => _gameData.StageNodeId == nodeId ? _gameData.StageKillCount : 0;
    public bool IsBossReady(int nodeId) => StageKills(nodeId) >= StageKillsToBoss;
    public bool IsStageCleared(int nodeId) => _gameData.ClearedStages.Contains(nodeId);

    public bool BossAutoChallenge
    {
        get => _gameData.BossAutoChallenge;
        set
        {
            if (_gameData.BossAutoChallenge == value) return;
            _gameData.BossAutoChallenge = value;
            SaveGame();
            OnStageChanged?.Invoke();
        }
    }

    public void AddStageKill(int nodeId)
    {
        if (_gameData.StageNodeId != nodeId) { _gameData.StageNodeId = nodeId; _gameData.StageKillCount = 0; }
        if (_gameData.StageKillCount >= StageKillsToBoss) return;   // 보스를 기다리는 동안은 더 세지 않는다
        _gameData.StageKillCount++;
        SaveGame();
        OnStageChanged?.Invoke();
    }

    // 보스 처치 — 첫 돌파면 크리스탈을 주고 그 양을, 이미 돌파한 곳이면 0 을 돌려준다.
    public int ClearStage(int nodeId)
    {
        int crystals = 0;
        if (!_gameData.ClearedStages.Contains(nodeId))
        {
            _gameData.ClearedStages.Add(nodeId);
            crystals = StageFirstClearCrystals(nodeId);
            Crystal += crystals;
        }
        _gameData.StageKillCount = 0;
        SaveGame();
        OnStageChanged?.Invoke();
        return crystals;
    }

    // 시간 초과·사망 — 처치 수를 비우고 자동 도전을 끈다(같은 벽에 30초씩 계속 부딪히지 않게).
    //  스테이지 막대를 누르면 다시 켜진다 — 그 사이 훈련으로 강해지라는 신호다.
    public void FailStage(int nodeId)
    {
        if (_gameData.StageNodeId == nodeId) _gameData.StageKillCount = 0;
        _gameData.BossAutoChallenge = false;
        SaveGame();
        OnStageChanged?.Invoke();
    }

    public int StageFirstClearCrystals(int nodeId)
    {
        if (!TryGetStageNumber(nodeId, out int field, out _)) return 0;
        var d = Managers.Data;
        return Mathf.RoundToInt(d.Balance(BalanceKey.BOSS_FIRST_CRYSTAL_BASE, 20f)
                              + d.Balance(BalanceKey.BOSS_FIRST_CRYSTAL_PER_FIELD, 2f) * field);
    }

    // 보스전 진행 상태(저장 안 함) — UI_BattleBtnGroup 이 쓰고 UI_StageBar 가 읽는다.
    public bool  BossFightActive { get; private set; }
    public float BossTimeLeft    { get; private set; }
    public float BossTimeLimit   { get; private set; }

    public void SetBossFight(bool active, float left = 0f, float limit = 0f)
    {
        bool changed = BossFightActive != active;
        BossFightActive = active;
        BossTimeLeft    = left;
        BossTimeLimit   = limit;
        if (changed) OnStageChanged?.Invoke();
    }

    // 스테이지 순서 = 전투 노드 Id 오름차순(21010, 21020 … 25450). 같은 부모의 층은 Id 가 이어져 있다.
    List<int> _stageOrder;
    List<int> StageOrder
    {
        get
        {
            if (_stageOrder != null || Managers.Data?.TravelNodeDic == null) return _stageOrder ?? new List<int>();
            var order = new List<int>();
            foreach (var n in Managers.Data.TravelNodeDic.Values)
                if (string.Equals(n.AreaType, nameof(AreaType.Battle), StringComparison.OrdinalIgnoreCase)) order.Add(n.Id);
            order.Sort();
            return _stageOrder = order;
        }
    }

    // 필드 = 부모 노드가 몇 번째인지(1~25), 층 = 그 부모 안에서 몇 번째인지(1~5).
    public bool TryGetStageNumber(int nodeId, out int field, out int floor)
    {
        field = floor = 0;
        var nodes = Managers.Data?.TravelNodeDic;
        if (nodes == null) return false;
        int lastParent = int.MinValue;
        foreach (int id in StageOrder)
        {
            int parent = nodes[id].ParentId;
            if (parent != lastParent) { field++; floor = 0; lastParent = parent; }
            floor++;
            if (id == nodeId) return true;
        }
        field = floor = 0;
        return false;
    }

    public int NextStage(int nodeId)
    {
        var order = StageOrder;
        int i = order.IndexOf(nodeId);
        return i >= 0 && i + 1 < order.Count ? order[i + 1] : 0;
    }

    public bool GuardHintSeen
    {
        get => _gameData.GuardHintSeen;
        set { _gameData.GuardHintSeen = value; SaveGame(); }
    }

    public string StageLabel(int nodeId)
    {
        if (!TryGetStageNumber(nodeId, out int field, out int floor)) return "";
        string name = Managers.Data.TravelNodeDic.TryGetValue(nodeId, out var node) ? Util.Localize(node.Name) : "";
        return string.Format(Util.Localize("StageLabelFormat"), field, floor, name);
    }
    #endregion

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

    // 여정 노드 해금 판정 (기획서 p1-s47) — TravelConditionData의 조건을 전부 만족해야 해금.
    // 조건 행이 없는 노드는 항시 해금 (소분류 첫 장소 등). 통행증은 승급 시 회수·교체되므로
    // 지역 게이트는 ITEM_REQUIRED가 아닌 QUEST_CLEAR(승급 보고 퀘 완료)로 거는 것을 원칙으로 한다.
    public bool IsTravelNodeUnlocked(int nodeId) => string.IsNullOrEmpty(GetTravelLockReason(nodeId));

    // 잠금 사유를 돌려준다 (해금 상태면 빈 문자열).
    //  여정 팝업은 잠긴 노드를 **숨기지 않고 회색으로 표시**하므로 사유 문구가 필요하다.
    //  판정 로직을 여기 한 곳에만 두고 IsTravelNodeUnlocked는 이걸 감싼다.
    public string GetTravelLockReason(int nodeId)
    {
        if (AllUnlocked) return string.Empty;   // [치트] 만능 키

        if (Managers.Data.TravelConditionDic == null ||
            !Managers.Data.TravelConditionDic.TryGetValue(nodeId, out var conditions))
            return string.Empty;

        foreach (var cond in conditions)
        {
            switch (cond.Type)
            {
                case "QUEST_CLEAR":     // Target 퀘스트 완료 = 현재 퀘가 그 이후
                    if (int.TryParse(cond.Target, out int questId) && CurrentQuestID <= questId) return LockText(cond);
                    break;
                case "BTN_UNLOCKED":
                    if (!UnlockedBtnIds.Contains(cond.Target)) return LockText(cond);
                    break;
                case "ITEM_REQUIRED":
                    if (int.TryParse(cond.Target, out int itemId) && GetItemCount(itemId) < Mathf.Max(1, cond.Value)) return LockText(cond);
                    break;
                case "LEVEL_REQUIRED":
                    if (Level < cond.Value) return LockText(cond);
                    break;
                default:    // 데이터 오타 등 알 수 없는 타입은 개방이 아니라 잠금 유지 (fail-closed)
                    Debug.LogWarning($"[GameManager] 알 수 없는 TravelCondition Type: '{cond.Type}' (NodeId {nodeId})");
                    return LockText(cond);
            }
        }
        return string.Empty;
    }

    // ────────────────────────────────────────
    //  대장간 제작 — 장비의 유일한 획득 경로
    // ────────────────────────────────────────
    //  장비 60종을 만들어 놓고 얻을 방법이 없었다(시작 지급 6종이 전부).
    //  드롭으로 주려면 CreatureData.DropItemId가 이미 재료에 쓰여 컬럼을 늘려야 하고,
    //  확률 드롭은 방치형에서 스트레스가 된다. 반면 재료는 쌓이기만 하고 쓸 데가 없었다.
    //  → 재료 3종 + 골드 + 요구 레벨로 제작한다 (ForgeRecipeData).

    public Data.ForgeRecipeData GetRecipeFor(int equipDataId)
    {
        if (Managers.Data.ForgeRecipeDic == null) return null;
        foreach (var r in Managers.Data.ForgeRecipeDic.Values)
            if (r.EquipDataId == equipDataId) return r;
        return null;
    }

    public bool HasEquipment(int equipDataId)
        => _gameData.OwnedEquipments.Exists(e => e.DataId == equipDataId);

    /// <summary>제작이 막힌 이유. 만들 수 있으면 빈 문자열.</summary>
    public string GetCraftBlockReason(int equipDataId)
    {
        if (HasEquipment(equipDataId)) return Util.Localize(LocalizeKey.EquipAlreadyOwnedText);

        var r = GetRecipeFor(equipDataId);
        if (r == null) return Util.Localize(LocalizeKey.EquipCannotCraftText);
        if (Level < r.ReqLevel) return string.Format(Util.Localize(LocalizeKey.RequiredLevelFormat), r.ReqLevel);
        if (Gold < r.Gold) return string.Format(Util.Localize(LocalizeKey.RequiredGoldFormat), r.Gold);

        foreach (var (id, need) in RecipeMaterials(r))
        {
            if (id <= 0 || need <= 0) continue;
            int have = GetItemCount(id);
            if (have < need) return $"{Util.Localize(MaterialName(id))} {have}/{need}";
        }
        return string.Empty;
    }

    public bool CanCraft(int equipDataId) => string.IsNullOrEmpty(GetCraftBlockReason(equipDataId));

    public static IEnumerable<(int id, int count)> RecipeMaterials(Data.ForgeRecipeData r)
    {
        yield return (r.Mat1Id, r.Mat1Count);
        yield return (r.Mat2Id, r.Mat2Count);
        yield return (r.Mat3Id, r.Mat3Count);
    }

    static string MaterialName(int itemId)
        => Managers.Data.ItemDic != null && Managers.Data.ItemDic.TryGetValue(itemId, out var it) && !string.IsNullOrEmpty(it.Name)
            ? it.Name : itemId.ToString();

    /// <summary>제작 실행. 성공하면 장비를 보유 목록에 넣고 빈 슬롯이면 자동 착용.</summary>
    public bool CraftEquipment(int equipDataId)
    {
        if (!CanCraft(equipDataId)) return false;
        var r = GetRecipeFor(equipDataId);
        if (r == null) return false;

        foreach (var (id, need) in RecipeMaterials(r))
            if (id > 0 && need > 0) RemoveItem(id, need);
        Gold -= r.Gold;

        var eq = new Equipment(equipDataId);
        _gameData.OwnedEquipments.Add(eq);
        if (!_gameData.EquippedEquipments.ContainsKey(eq.SlotType))
        {
            eq.IsEquipped = true;
            _gameData.EquippedEquipments[eq.SlotType] = eq;
            Player?.RefreshStats(fullHeal: false);
        }

        SaveGame();
        EquipInfoChanged?.Invoke();
        return true;
    }

    // ────────────────────────────────────────
    //  [치트] 만능 키 — 모든 컨텐츠 해금
    // ────────────────────────────────────────
    public bool EndingSeen
    {
        get => _gameData.EndingSeen;
        set { _gameData.EndingSeen = value; SaveGame(); }
    }

    public bool AllUnlocked
    {
        get => _gameData.AllUnlocked;
        set { _gameData.AllUnlocked = value; SaveGame(); }
    }

    /// <summary>
    /// 잠겨 있는 모든 컨텐츠를 연다. 개발/검수용 치트.
    ///
    /// 해금에는 두 종류가 있어서 둘 다 처리한다.
    ///   · 판정을 건너뛰면 되는 것 (여정 노드 조건) → AllUnlocked 플래그
    ///   · 데이터를 실제로 채워야 하는 것 (장비 보유, 능력 습득, 도감 기록) → 여기서 직접 넣는다
    ///
    /// **레벨과 스탯은 건드리지 않는다.** 레벨업 자체가 컨텐츠이기도 하고,
    /// 올려버리면 되돌릴 수단이 없다. 고레벨 지역이 필요하면 EXP 치트(숫자 7)를 쓰면 된다.
    /// </summary>
    public void UnlockEverything()
    {
        var data = Managers.Data;

        // 1. 잠금 판정 전역 무시 (여정 25지역 x 5던전)
        _gameData.AllUnlocked = true;

        // 2. 최하단 탭 전부
        foreach (Define.TabType tab in System.Enum.GetValues(typeof(Define.TabType)))
            UnlockBtmButton(tab);

        // 3. 버튼 ID — 상수로 박힌 것 + 데이터가 요구하는 것 전부.
        //    데이터에서 긁어오므로 나중에 조건이 늘어도 치트가 따라간다.
        foreach (string id in new[] { UNLOCK_TRAVEL_BUTTON, UNLOCK_ALCHEMY_BUTTON,
                                      UNLOCK_INVENTORY_BUTTON, UNLOCK_POTION_BUTTON, BTN_COMMISION })
            AddUnlockedBtn(id);

        if (data.QuestUnlockDic != null)
            foreach (var list in data.QuestUnlockDic.Values)
                foreach (var u in list)
                    if (!string.IsNullOrEmpty(u.TargetID) && !int.TryParse(u.TargetID, out _))
                        AddUnlockedBtn(u.TargetID);

        if (data.TravelConditionDic != null)
            foreach (var list in data.TravelConditionDic.Values)
                foreach (var c in list)
                    if (c.Type == "BTN_UNLOCKED" && !string.IsNullOrEmpty(c.Target))
                        AddUnlockedBtn(c.Target);

        // 4. 능력 — 전부 습득
        if (data.SkillDic != null && Managers.Ability != null)
            foreach (int id in data.SkillDic.Keys)
                if (!Managers.Ability.IsLearned(id))
                    Managers.Ability.Learn(id);

        // 4-b. 액티브 스킬 — 습득만으론 스킬바에 안 뜬다(점유 능력은 [기억]해야 장착).
        //      기억력이 모자라면 Memorize가 조용히 실패하므로 스킬바가 빈다 → 직접 채운다.
        if (data.SkillDic != null && Managers.Ability != null)
            foreach (var d in data.SkillDic.Values)
                if (d.TriggerType == Define.AbilityTrigger.Active && !Managers.Ability.IsEquipped(d.DataId))
                    if (!_gameData.EquippedAbilities.Contains(d.DataId))
                        _gameData.EquippedAbilities.Add(d.DataId);

        // 5. 장비 — 전 종류 1개씩. 빈 슬롯은 자동 착용.
        if (data.EquipmentDic != null)
        {
            foreach (int id in data.EquipmentDic.Keys)
            {
                if (_gameData.OwnedEquipments.Exists(e => e.DataId == id)) continue;
                var eq = new Equipment(id);
                _gameData.OwnedEquipments.Add(eq);
                if (!_gameData.EquippedEquipments.ContainsKey(eq.SlotType))
                {
                    eq.IsEquipped = true;
                    _gameData.EquippedEquipments[eq.SlotType] = eq;
                }
            }
        }

        // 6. 아이템 — 재료/주문서/귀중품 전부 넉넉히 (강화·제작이 재료 부족으로 막히지 않게)
        const int CHEAT_ITEM_COUNT = 999;
        foreach (var dic in new[] { data.ItemDic, data.RequisiteItemDic })
        {
            if (dic == null) continue;
            foreach (int id in dic.Keys)
            {
                var have = Inventory.Find(i => i.DataId == id);
                if (have == null) Inventory.Add(new InventoryItem { DataId = id, Count = CHEAT_ITEM_COUNT });
                else if (have.Count < CHEAT_ITEM_COUNT) have.Count = CHEAT_ITEM_COUNT;
            }
        }

        // 7. 도감 — 전 몬스터 발견 + 기록도 만충(고유능력 해금 조건)
        if (data.CreatureDic != null)
        {
            foreach (var kv in data.CreatureDic)
            {
                if (!DiscoveredMonsterIds.Contains(kv.Key)) DiscoveredMonsterIds.Add(kv.Key);
                if (Managers.MinMon == null) continue;
                var save = Managers.MinMon.GetSaveData(kv.Key);
                save.KillCount = Mathf.Max(save.KillCount, Mathf.Max(1, kv.Value.MaxKillCount));
            }
        }

        // 8. 미니몬 — 전 종류 보유(등급 최고) + 동행 가능 상태.
        //    동행 선택(도감 [동행] 버튼)은 보유 여부로 갈리므로 CaughtCount가 곧 동행 해금이다.
        //    공명도 100%(4회)까지 채워 고유기술까지 열어 둔다.
        if (data.MinMonDic != null && Managers.MinMon != null)
        {
            foreach (int id in data.MinMonDic.Keys)
            {
                var save = Managers.MinMon.GetSaveData(id);
                save.CaughtCount   = Mathf.Max(save.CaughtCount, 4);   // 공명도 100% = 동행 + 고유기술
                save.AtkGrade      = Mathf.Max(save.AtkGrade,      5);
                save.AtkSpeedGrade = Mathf.Max(save.AtkSpeedGrade, 5);
                save.CritGrade     = Mathf.Max(save.CritGrade,     5);
            }
            // 동행이 비어 있으면 첫 미니몬을 붙여 준다 — "해금됐는데 아무도 안 따라온다"를 막는다.
            if (_gameData.CompanionMinMonId < 0)
                foreach (int id in data.MinMonDic.Keys) { Managers.MinMon.SetCompanion(id); break; }
        }

        // 9. 도감 스킬 슬롯 / 물약 게이트
        for (int i = 0; i < BestiarySkillUnlocked.Length; i++) BestiarySkillUnlocked[i] = true;
        _gameData.LuckPotionUnlocked      = true;
        _gameData.SpdPotionRevealed       = true;
        _gameData.AutoPotionMemorized     = true;

        // 10. 재화 — 강화/제작 비용이 막지 않을 만큼.
        //     만드라고라(확정 포획 소비품)는 인벤이 아니라 GameData 필드라 위 6번 루프가 못 채운다.
        _gameData.Gold     = Mathf.Max(_gameData.Gold, 9_999_999);
        _gameData.Crystal  = Mathf.Max(_gameData.Crystal, 99_999);
        _gameData.Mandrake = Mathf.Max(_gameData.Mandrake, 999);

        // 11. 퀘스트 — 마지막 번호로. (퀘스트가 하단 버튼/마을 UI 해금을 들고 있다)
        if (Managers.Quest != null && data.QuestDic != null && data.QuestDic.Count > 0)
        {
            int last = 0;
            foreach (int q in data.QuestDic.Keys) if (q > last) last = q;
            Managers.Quest.JumpToQuest(last);
        }

        SaveGame();
        Player?.RefreshStats(fullHeal: true);   // 장비/능력이 한꺼번에 붙었으니 스탯 재계산 + 만피

        Debug.Log($"[치트] 만능 키 — 여정 전체 해금 / 장비 {_gameData.OwnedEquipments.Count}종 / " +
                  $"능력 {LearnedAbilities.Count}종 / 도감 {DiscoveredMonsterIds.Count}종 / " +
                  $"미니몬 {_gameData.OwnedMinMons.Count}종 / 골드 {_gameData.Gold:N0}");
    }

    // FailMessage가 비어 있어도 잠금 사유는 반드시 비어있지 않은 문자열이어야 한다
    //  (호출부가 문자열 유무로 해금 여부를 판정하므로).
    //  FailMessage 는 한국어 문장 데이터(148행 전부 레벨 조건)라, 한국어가 아니면 키 문구로 대신한다 —
    //  예전엔 영어 모드에서도 잠긴 지역마다 '레벨 13 이상이어야 갈 수 있습니다.'가 붙었다.
    static string LockText(Data.TravelConditionData cond)
    {
        bool korean = Managers.Game != null && Managers.Game.Language == Define.Language.Korean;
        if (korean && !string.IsNullOrEmpty(cond.FailMessage)) return cond.FailMessage;
        if (cond.Type == "LEVEL_REQUIRED") return string.Format(Util.Localize(LocalizeKey.RequiredLevelFormat), cond.Value);
        return Util.Localize(LocalizeKey.TravelLockedText);
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
    //  저장은 "변경 표시 → 모아서 기록"이다. 세터마다 SaveGame()을 부르는 기존 패턴은 그대로 두고,
    //  실제 디스크 기록은 FlushSave 가 SAVE_FLUSH_INTERVAL 마다 한 번만 한다.
    //  (예전엔 처치 1회에 3~6번, 자동물약 슬라이더를 끄는 동안 매 프레임 90KB를 동기 기록했다)
    //  일시정지·종료(MarkSeen)와 새 게임 생성은 SaveGameNow 로 즉시 기록한다.
    //
    //  기록은 임시 파일에 다 쓴 뒤 이름을 바꾸고, 직전 정상본은 .bak 으로 남긴다.
    //  쓰는 도중 앱이 죽어도 온전한 파일 하나는 반드시 남는다.
    public const int SAVE_VERSION = 1;
    const float SAVE_FLUSH_INTERVAL = 3f;
    const int SAVE_READ_RETRIES = 3;

    public static string SavePath => Application.persistentDataPath + "/SaveData.json";

    // 기본값(Auto)은 필드 초기값이 든 리스트에 로드한 값을 **이어 붙인다** —
    //  ActivationManager(초기값 7칸)가 로드할 때마다 7칸씩 늘어나 해금 표시가 사라졌다.
    static readonly JsonSerializerSettings s_saveJson = new JsonSerializerSettings
    {
        ObjectCreationHandling = ObjectCreationHandling.Replace,
    };
    static readonly JsonSerializer s_saveSerializer = JsonSerializer.Create(s_saveJson);

    bool  _saveDirty;
    float _saveTimer;

    public void SaveGame()
    {
        if (_path == null) return;
        _saveDirty = true;
    }

    public void SaveGameNow()
    {
        SaveGame();
        FlushSave();
    }

    /// <summary>Managers.Update 에서 매 프레임. 변경이 있으면 주기마다 한 번 기록한다.</summary>
    public void TickSave(float unscaledDeltaTime)
    {
        if (!_saveDirty) return;
        _saveTimer += unscaledDeltaTime;
        if (_saveTimer >= SAVE_FLUSH_INTERVAL)
            FlushSave();
    }

    public void FlushSave()
    {
        _saveTimer = 0f;
        if (_path == null || !_saveDirty) return;
        _saveDirty = false;

        string tmp = _path + ".tmp", bak = _path + ".bak";
        try
        {
            _gameData.SaveVersion = SAVE_VERSION;
            // 이름을 바꾸기 전에 디스크까지 내린다(fsync). 안 하면 전원이 나갔을 때 방금 이름 바꾼 본 파일과
            //  2~3초 전 .bak 이 둘 다 빈 파일일 수 있다(ext4 지연 할당).
            // 파일에 바로 직렬화한다 — 예전엔 JSON 전체 문자열(UTF-16)과 바이트 배열을 만들어 저장 한 번에 수백 KB 를
            //  할당했고, 전투 중 3초마다 저장되니 게임 코드 가비지의 대부분이었다(프레임당 평균 3.2KB). 결과 바이트는 같다.
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false), 16 * 1024, leaveOpen: true))
                using (var jw = new JsonTextWriter(sw))
                    s_saveSerializer.Serialize(jw, _gameData);
                fs.Flush(true);
            }
            if (File.Exists(_path))
            {
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(_path, bak);
            }
            File.Move(tmp, _path);
        }
        catch (Exception e)
        {
            // 저장 실패가 게임 진행(보상 지급 등)을 끊으면 안 된다 — 기록하고 다음 주기에 다시 시도.
            _saveDirty = true;
            Debug.LogError($"[Save] 저장 실패: {e.Message}");
        }
    }

    /// <summary>세이브 파일 일체 삭제(에디터 초기화 메뉴용). 손상 격리본은 남긴다.
    /// 플레이 중이면 이번 세션의 저장도 멈춘다 — 안 그러면 모아 쓰기·종료 저장이 곧바로 다시 써서
    /// "다시 Play = 첫 실행" 이 성립하지 않는다.</summary>
    public static void DeleteSaveFiles()
    {
        if (Application.isPlaying) Managers.Game?.StopSavingThisSession();
        foreach (string f in new[] { SavePath, SavePath + ".tmp", SavePath + ".bak" })
            if (File.Exists(f)) File.Delete(f);
    }

    public void StopSavingThisSession()
    {
        _path = null;
        _saveDirty = false;
    }

    public bool LoadGame()
    {
        // 본 파일 → (본 파일이 이름 바꾸기 도중 사라졌다면) 임시 파일 → 직전 백업 순으로 읽는다.
        //  읽을 수 없는 파일은 지우지 않고 옆으로 치운다 — 예전엔 예외가 그대로 새어 나가
        //  빈 데이터가 손상된 세이브를 덮어썼다.
        if (_path == null) return false;

        GameData data = null;
        foreach (string candidate in new[] { _path, _path + ".tmp", _path + ".bak" })
        {
            if (!File.Exists(candidate)) continue;
            string json = ReadWithRetry(candidate);
            if (json == null)
            {
                // 파일은 있는데 못 읽는다(백신·클라우드 동기화가 잠금 등) — 손상이 아니다. 격리·새 게임 저장으로
                //  멀쩡한 세이브를 밀어내지 않도록 이번 세션은 저장하지 않는다. 다음 실행에서 정상적으로 읽힌다.
                Debug.LogError($"[Save] {Path.GetFileName(candidate)} 를 읽을 수 없어 이번 세션은 저장하지 않습니다.");
                StopSavingThisSession();
                return false;
            }
            try
            {
                data = JsonConvert.DeserializeObject<GameData>(json, s_saveJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] 읽기 실패 {Path.GetFileName(candidate)}: {e.Message}");
                data = null;
            }
            if (data != null)
            {
                if (candidate != _path) Debug.LogWarning($"[Save] {Path.GetFileName(candidate)} 에서 복구");
                break;
            }
            if (!candidate.EndsWith(".tmp")) QuarantineSave(candidate);
        }
        if (data == null)
            return false;

        _gameData = data;
        NormalizeSave();
        EnsurePotionState();
        RebuildEquippedFromOwned();   // 장착 딕셔너리를 보유 리스트 참조로 재구성
        Managers.Ability.RestoreMinionRewards();

        return true;
    }

    static string ReadWithRetry(string path)
    {
        for (int i = 0; i < SAVE_READ_RETRIES; i++)
        {
            try { return File.ReadAllText(path); }
            catch (IOException) { System.Threading.Thread.Sleep(100); }
            catch (UnauthorizedAccessException) { System.Threading.Thread.Sleep(100); }
        }
        return null;
    }

    static void QuarantineSave(string path)
    {
        try
        {
            string dest = $"{path}.corrupt-{DateTime.Now:yyyyMMdd-HHmmss}";
            File.Move(path, dest);
            Debug.LogError($"[Save] 읽을 수 없는 세이브를 보관: {Path.GetFileName(dest)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 손상 세이브 보관 실패: {e.Message}");
        }
    }

    // 옛 세이브·손으로 고친 세이브의 모양을 현재 코드가 기대하는 모양으로 맞춘다. 여러 번 돌려도 같다.
    void NormalizeSave()
    {
        var d = _gameData;
        if (d.SaveVersion > SAVE_VERSION)
            Debug.LogWarning($"[Save] 더 새 버전의 세이브({d.SaveVersion} > {SAVE_VERSION})");

        // v0: 로드마다 이어 붙어 늘어난 칸 정리 — 마지막 세션이 읽고 쓴 앞쪽 칸만 남긴다.
        int keys = Enum.GetValues(typeof(ActivationKey)).Length;
        d.ActivationManager ??= new List<int>();
        if (d.ActivationManager.Count > keys) d.ActivationManager.RemoveRange(keys, d.ActivationManager.Count - keys);
        while (d.ActivationManager.Count < keys) d.ActivationManager.Add(0);

        // 명시적 null 과 길이가 모자란 고정 배열(인덱스로 접근) 보정
        d.PlayerName           ??= "";
        d.OwnedEquipments      ??= new List<Equipment>();
        d.Inventory            ??= new List<InventoryItem>();
        d.OwnedMinMons         ??= new List<MinMonSaveData>();
        d.InterestedMinMons    ??= new List<int>();
        d.MaxStarGranted       ??= new List<int>();
        d.LearnedAbilities     ??= new List<int>();
        d.EquippedAbilities    ??= new List<int>();
        d.AbilityLevels        ??= new Dictionary<int, int>();
        d.DiscoveredMonsterIds ??= new List<int>();
        d.MetQuestConditions   ??= new List<int>();
        d.UnlockedBtnIds       ??= new List<string>();
        d.ClearedStages        ??= new List<int>();
        d.BestiarySkillUnlocked ??= new GameData().BestiarySkillUnlocked;   // 기본 해금(idx3) 유지
        d.StatInvest            = Fit(d.StatInvest, 4);
        d.TrainingLevels        = Fit(d.TrainingLevels, TRAINING_STAT_COUNT);   // v2 이전 세이브엔 없다
        d.BestiarySkillGrade    = Fit(d.BestiarySkillGrade, 4);
        d.BestiarySkillNew      = Fit(d.BestiarySkillNew, 4);
        d.BestiarySkillUnlocked = Fit(d.BestiarySkillUnlocked, 4);
        d.OwnedEquipments.RemoveAll(e => e == null);
        d.Inventory.RemoveAll(i => i == null);
        d.OwnedMinMons.RemoveAll(m => m == null);

        // 레벨 상한 도입 전에 상한을 넘긴 세이브(스탯 폴백 구간) 복구
        int maxLevel = Managers.Data != null ? Managers.Data.MaxPlayerLevel : int.MaxValue;
        d.Level = Mathf.Clamp(d.Level, 1, maxLevel);
    }

    static T[] Fit<T>(T[] arr, int length)
    {
        if (arr != null && arr.Length >= length) return arr;
        var fitted = new T[length];
        if (arr != null) Array.Copy(arr, fitted, arr.Length);
        return fitted;
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
