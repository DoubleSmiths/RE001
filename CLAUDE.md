
스크립트 위치
Assets/@Scripts

# SlimeMaster - Unity 방치형 게임 코딩 가이드

이 문서는 SlimeMaster 프로젝트에서 사용된 코딩 패턴과 아키텍처를 정리한 것으로,
다른 방치형 게임 개발 시에도 동일하게 적용할 수 있는 기술 가이드입니다.

---

## 1. 전체 아키텍처: Manager Pattern + MVC

### 폴더 구조 원칙
```
Assets/@Scripts/
├── Managers/       ← 싱글톤 서비스 레이어 (Core + Contents 분리)
├── Controllers/    ← 엔티티 동작 (상속 계층 구성)
├── Contents/       ← 게임 로직 (Skill, Character, Map 등)
├── UI/             ← UI 팝업 스택 시스템
├── Data/           ← 데이터 직렬화 클래스
└── Utils/          ← Define(상수/열거형), 공용 유틸
```

### 핵심 원칙
- 모든 시스템은 **Managers.cs** 하나를 통해서만 접근
- `DontDestroyOnLoad`는 Managers 루트 오브젝트 하나에만 적용
- 매니저는 **Core**(범용 프레임워크)와 **Contents**(게임 로직)로 구분

---

## 2. Managers 싱글톤 패턴

```csharp
// Managers.cs - 모든 매니저의 진입점
public class Managers : MonoBehaviour
{
    static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    // Core (재사용 가능한 범용 시스템)
    DataManager _data = new DataManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();

    // Contents (게임 로직 전용)
    GameManager _game = new GameManager();
    ObjectManager _object = new ObjectManager();
    TimeManager _time = new TimeManager();

    // 외부 접근은 static property로만
    public static DataManager Data { get { return Instance._data; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static GameManager Game { get { return Instance._game; } }
    // ...
}
```

**규칙:**
- 매니저 내부 접근은 `_data`, 외부 접근은 `Managers.Data`
- 매니저 간 의존성은 항상 `Managers.XXX`를 통해 참조
- 매니저 인스턴스를 직접 new 하지 않음 (Managers가 소유)

---

## 3. 데이터 드리븐 설계 (JSON + ILoader)

### 데이터 클래스 구조
```csharp
// Data.Contents.cs - 데이터 직렬화 클래스
[Serializable]
public class SkillData
{
    public int DataId;
    public string Name;
    public float CoolTime;
    public float Damage;
    // ...
}

// ILoader 인터페이스로 딕셔너리 변환
public class SkillDataLoader : ILoader<int, SkillData>
{
    public List<SkillData> Skills = new List<SkillData>();
    public Dictionary<int, SkillData> MakeDict()
    {
        return Skills.ToDictionary(x => x.DataId);
    }
}
```

### DataManager 로딩 패턴
```csharp
public class DataManager
{
    public Dictionary<int, SkillData> SkillDic { get; private set; }

    public void Init()
    {
        // 게임 시작 시 한 번만 로드, 이후 딕셔너리로 캐싱
        SkillDic = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
    }

    T LoadJson<T>(string path) where T : class
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<T>(textAsset.text);
    }
}
```

**규칙:**
- 게임 수치(데미지, 쿨타임, 확률 등)는 코드에 하드코딩하지 않고 JSON으로 관리
- 모든 데이터는 `DataId`(int)를 기본 키로 사용
- 로드 후에는 딕셔너리로 O(1) 접근

---

## 4. 세이브/로드 시스템

```csharp
// GameData - 전체 계정 상태를 하나의 클래스로 직렬화
[Serializable]
public class GameData
{
    public int UserLevel;
    public int Gold;
    public int Dia;
    public List<Equipment> OwnedEquipments = new List<Equipment>();
    public Dictionary<int, StageClearInfo> DicStageClearInfo;
    public ContinueData ContinueInfo;  // 전투 중 상태 스냅샷
    // ...
}

public class GameManager
{
    const string SAVE_PATH = "/SaveData.json";

    public void SaveGame()
    {
        string json = JsonConvert.SerializeObject(_gameData);
        File.WriteAllText(Application.persistentDataPath + SAVE_PATH, json);
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + SAVE_PATH;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            _gameData = JsonConvert.DeserializeObject<GameData>(json);
        }
    }
}
```

**규칙:**
- 세이브 데이터는 하나의 `GameData` 클래스에 집중
- 배틀 도중 상태는 `ContinueData`로 별도 스냅샷 관리 (이어하기 지원)
- 시간 기반 데이터(오프라인 보상 등)는 `PlayerPrefs`에 timestamp만 저장

---

## 5. UI 팝업 스택 시스템

```csharp
public class UIManager
{
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        GameObject go = Managers.Resource.Instantiate(name ?? typeof(T).Name);
        T popup = go.GetOrAddComponent<T>();
        _popupStack.Push(popup);
        // 팝업이 열리면 게임 일시정지
        Time.timeScale = 0f;
        return popup;
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0) return;
        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        // 모든 팝업이 닫히면 재개
        if (_popupStack.Count == 0)
            Time.timeScale = 1f;
    }
}
```

**UI 클래스 계층:**
```
UI_Base
  ├─ UI_Scene      ← 씬 고정 HUD (배틀/로비/타이틀)
  ├─ UI_Popup      ← 모달 팝업 (스택 관리)
  └─ UI_SubItem    ← 리스트 아이템 템플릿
```

**규칙:**
- 팝업은 반드시 UIManager를 통해서만 열고 닫음 (직접 SetActive 금지)
- UI_SubItem은 데이터를 받아 자신을 갱신하는 `SetInfo()` 메서드를 가짐
- Canvas SortingOrder는 UIManager가 팝업마다 자동으로 증가시킴

---

## 6. 오브젝트 풀링

```csharp
// PoolManager - Unity IObjectPool<T> 래핑
public class PoolManager
{
    Dictionary<string, IObjectPool<GameObject>> _pools = new();

    public GameObject Pop(GameObject prefab)
    {
        string key = prefab.name;
        if (!_pools.ContainsKey(key))
            CreatePool(prefab);
        return _pools[key].Get();
    }

    public void Push(GameObject obj)
    {
        string key = obj.name;
        _pools[key].Release(obj);
    }
}

// ResourceManager에서 풀링 여부를 선택적으로 사용
public GameObject Instantiate(string path, bool pooling = false)
{
    if (pooling)
        return Managers.Pool.Pop(prefab);
    return Object.Instantiate(prefab);
}
```

**규칙:**
- 자주 생성/삭제되는 오브젝트(총알, 몬스터, 데미지 폰트 등)는 반드시 풀링
- `Destroy()` 대신 `Managers.Resource.Destroy(go)` 사용 (풀 반환 처리 포함)
- 풀 키는 prefab 이름(string)으로 관리

---

## 7. 크리처 상속 계층

```
BaseController (GetOrAddComponent 헬퍼 등 공용 유틸)
  └─ CreatureController (체력/공격/스탯/상태머신 - abstract)
      ├─ PlayerController   (레벨업, 경험치, 스킬 장착)
      ├─ MonsterController  (AI 이동, 공격, 드롭)
      ├─ EliteController    (MonsterController 확장)
      └─ BossController     (MonsterController 확장)
```

### 상태 머신 패턴
```csharp
public enum CreatureState { Idle, Moving, Skill, OnDamaged, Dead }

public CreatureState CreatureState
{
    get { return _creatureState; }
    set
    {
        _creatureState = value;
        UpdateAnimation();   // 상태 변경 시 애니메이션 자동 전환
    }
}

// Update에서 상태별 함수 호출
void Update()
{
    switch (CreatureState)
    {
        case CreatureState.Idle:    UpdateIdle(); break;
        case CreatureState.Moving:  UpdateMoving(); break;
        case CreatureState.Skill:   UpdateSkill(); break;
    }
}
```

**규칙:**
- 상태 전환은 항상 property setter를 통해서만 (직접 필드 변경 금지)
- abstract 메서드(`OnDead`, `UpdateAnimation`)로 자식 클래스 구현 강제

---

## 8. 스킬 시스템 (Sequence / Repeat 이분법)

```
SkillBase (데이터 로드, 데미지 계산)
  ├─ SequenceSkill  ← 순서대로 실행, 콜백으로 체이닝
  │   └─ BasicAttack, ComboShot, Charging, CircleShot, SpinShot
  └─ RepeatSkill    ← 쿨타임마다 자동 반복 (코루틴)
      └─ ArrowShot, PoisonField, Meteor, ChainLightning...
```

```csharp
// RepeatSkill - 코루틴 기반 자동 반복
public abstract class RepeatSkill : SkillBase
{
    protected override void ActivateSkill()
    {
        StartCoroutine(CoActivateSkill());
    }

    IEnumerator CoActivateSkill()
    {
        while (true)
        {
            DoSkillJob();
            yield return new WaitForSeconds(SkillData.CoolTime);
        }
    }

    protected abstract void DoSkillJob();
}

// SequenceSkill - 콜백 체이닝
public abstract class SequenceSkill : SkillBase
{
    public abstract void DoSkill(Action callback);
}
```

### SkillBook - 스킬 관리 컴포넌트
```csharp
// CreatureController에 조합으로 부착
public class SkillBook : MonoBehaviour
{
    public List<SkillBase> SkillList = new();
    public List<SequenceSkill> SequenceSkills = new();
    public List<RepeatSkill> RepeatSkills = new();

    public void AddSkill(SkillType skillType, int level)
    {
        // prefab 인스턴스화 또는 컴포넌트 추가 후 레벨 세팅
    }
}
```

**규칙:**
- 스킬 데이터(데미지, 쿨타임, 범위)는 JSON에서 로드
- 스킬 오브젝트는 풀링으로 관리
- `SupportSkill`: 스탯 버프 전용 별도 분류, 스킬 로직 없이 수치만 가짐

---

## 9. 이벤트(Observer) 패턴

```csharp
// C# Action으로 느슨한 결합
public class PlayerController : CreatureController
{
    public Action OnPlayerLevelUp;
    public Action OnPlayerDead;
    public Action OnPlayerDataUpdated;
    public Action OnPlayerMove;
}

// 구독 예시
void Start()
{
    player.OnPlayerLevelUp += HandleLevelUp;
    player.OnPlayerDead    += HandleGameOver;
}

// 호출 예시
void LevelUp()
{
    Level++;
    OnPlayerLevelUp?.Invoke();
}
```

**규칙:**
- UI 갱신, 사운드, 이펙트 트리거는 이벤트로 연결 (컨트롤러가 UI를 직접 참조하지 않음)
- 이벤트 구독 해제는 `OnDestroy()`에서 반드시 처리

---

## 10. 방치형 게임 핵심 시스템

### 오프라인 보상
```csharp
// TimeManager
DateTime lastRewardTime = DateTime.Parse(PlayerPrefs.GetString("LastRewardTime"));
TimeSpan elapsed = DateTime.Now - lastRewardTime;
int offlineMinutes = (int)elapsed.TotalMinutes;
int reward = CalculateGoldPerMinute() * offlineMinutes;
```

### 스태미나 (시간 회복)
```csharp
// 5분마다 1 회복, 최대 50
const int MAX_STAMINA = 50;
const int REGEN_MINUTES = 5;

int GetCurrentStamina()
{
    DateTime last = DateTime.Parse(PlayerPrefs.GetString("LastStaminaTime"));
    int recovered = (int)(DateTime.Now - last).TotalMinutes / REGEN_MINUTES;
    return Mathf.Min(savedStamina + recovered, MAX_STAMINA);
}
```

### 웨이브 기반 배틀
- 스테이지 = N개의 웨이브 + 보스
- 웨이브 = 고정 시간(60초) + 몬스터 스폰 테이블 JSON 정의
- 각 웨이브 HP 배율(`HpIncreaseRate`)로 난이도 스케일링

### 이어하기 (ContinueData)
```csharp
// 전투 중 상태를 스냅샷으로 저장
[Serializable]
public class ContinueData
{
    public int PlayerDataId;
    public float Hp;
    public float MaxHp;
    public int Level;
    public int WaveIndex;
    public List<SkillSaveInfo> Skills;
    // ...
}
```

---

## 11. 제네릭 패턴 모음

### GetOrAddComponent
```csharp
public static T GetOrAddComponent<T>(this GameObject go) where T : Component
{
    T component = go.GetComponent<T>();
    if (component == null)
        component = go.AddComponent<T>();
    return component;
}
```

### 제네릭 팩토리 스폰
```csharp
public T Spawn<T>(Vector3 position) where T : BaseController
{
    GameObject go = Managers.Resource.Instantiate(typeof(T).Name, pooling: true);
    T controller = go.GetOrAddComponent<T>();
    controller.transform.position = position;
    return controller;
}
```

### ILoader 인터페이스
```csharp
public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}
```

---

## 12. 코딩 규칙 요약

| 항목 | 규칙 |
|------|------|
| 매니저 접근 | `Managers.XX` static property 통해서만 |
| 오브젝트 생성 | `Managers.Resource.Instantiate()` 사용 |
| 오브젝트 삭제 | `Managers.Resource.Destroy()` 사용 (풀 반환 포함) |
| 팝업 열기/닫기 | `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()` |
| 게임 수치 | JSON 데이터로 관리, 코드 하드코딩 금지 |
| 상태 전환 | 반드시 property setter 통해서만 |
| 이벤트 구독 | `OnDestroy()`에서 반드시 해제 |
| 인코딩 | 모든 C# 스크립트는 UTF-8 (BOM 없음) |
| 스킬 데이터 | DataId로 JSON 참조, 스킬 클래스에 하드코딩 금지 |

# RE001 게임 분석 문서

## 1. 장르 및 개요

- **장르**: 2D 사이드뷰 액션 RPG (모바일 세로 화면)
- **시점**: 2D 횡스크롤
- **화면 구성**: 상단 게임 뷰 + 하단 UI 패널 분리 구조
- **아트 스타일**: 픽셀 아트 배경 + 픽셀 캐릭터

---

## 2. 화면 레이아웃

```
┌─────────────────────────┐
│  [골드] [재화]    [메뉴]  │  ← 최상단 HUD (상시 표시)
│  [퀘스트 진행도]          │  ← 우측 상단 (예: 젤리 풍 0/1)
├─────────────────────────┤
│                         │
│      게임 뷰 영역        │  ← 배경 + 캐릭터 + 전투
│   (픽셀 아트 배경)        │
│                         │
├─────────────────────────┤
│  [초상화] 캐릭터명 : Lv.  │
│  ████████ HP / HP       │  ← 캐릭터 상태창
│  ████ EXP%              │
├─────────────────────────┤
│  [슬롯][슬롯][슬롯]       │
│  [슬롯][슬롯][슬롯]       │  ← 퀵슬롯 (6칸, NPC/퀘스트 아이콘)
├─────────────────────────┤
│ [상태][가방][물약][장비][기술][도감] │  ← 하단 탭 메뉴
└─────────────────────────┘
```

---

## 3. 씬 구성

| 씬 | 설명 |
|---|---|
| 마을 (시작 마을) | 픽셀 마을 배경, 캐릭터 대기 상태, 모험가협회 등 NPC 이동 가능 |
| 신들바람 숲 | 전투 스테이지, 슬라임 등 몬스터 등장 |
| 시들어가는 사막 | 마물 도감에 등장하는 다음 구역 (잠금 상태) |
| 도서관 (모험가협회) | NPC 대화 공간, 퀘스트 수주 |
| 연금공방 | 물약/아이템 제작 NPC |
| 대장간 | 장비 강화 연출 씬 |

---

## 4. 핵심 게임플레이

### 4-1. 이동 및 탐험
- 캐릭터가 마을/던전을 횡스크롤로 이동
- **여정(부츠 아이콘)** : 통행증 소지 여부에 따라 이동 가능 지역 해금
- 퀵슬롯 우측 하단에 **여정** 버튼으로 장소 이동

### 4-2. 전투 시스템
- 2D 횡스크롤 실시간 전투
- 플레이어 공격 시 흰색 검기(슬래시) 이펙트 발생
- 몬스터 HP 바가 화면 상단에 표시 (이름: 풍 등)
- 몬스터가 플레이어를 공격하면 플레이어 HP 감소
- 데미지 숫자가 몬스터 위에 팝업으로 표시 (흰색 숫자)
- 치명타 별도 표시 가능

### 4-3. 캐릭터 성장
- **레벨업**: 경험치(EXP%) 게이지 → 100% 도달 시 레벨업
- **기본 직업**: 여행자 (Lv.1 시작)
- **스탯**: 공격력, 공격속도, 치명타 (등급: E, D, C 표기)

### 4-4. 미니언(펫) 시스템
- 미니언 탭에서 포획한 몬스터를 동반자로 등록
- 미니언 정보: NO.001 하수인 (슬라임 계열)
- **동기화 수치** 표시 (예: 16%)
- 미니언 스탯: 공격력(E), 공격속도(D), 치명타(C)
- 최대 성장 조건 표시 (예: 9번 더 길들일 시)
- "풍에게는 효과가 없는 듯 하다" → 몬스터별 미니언 효과 유무 존재

---

## 5. UI 시스템

### 5-1. 하단 탭 메뉴
| 탭 | 아이콘 | 기능 |
|---|---|---|
| 상태 | 캐릭터 | 캐릭터 스탯/정보 |
| 가방 | 가방 | 소지품 인벤토리 |
| 물약 | 물약병 | 물약 관리 |
| 장비 | 검+방패 | 장착 장비 관리 |
| 기술 | 검 교차 | 스킬 목록 |
| 도감 | 날개(새) | 마물 도감 |

### 5-2. 소지품 (인벤토리)
- 그리드 형태 (3열 다중행)
- 아이템 클릭 시 상세 팝업 표시
- 아이템 정보: 이름, 등급(색상 테두리), 발견장소, 설명

### 5-3. 장비 시스템 (EQUIPMENT)
- 장비 슬롯: 무기, 투구, 갑옷, 장갑, 신발, 기타
- 등급: E(보라) → G(초록) → B(파랑) → ? 순 업그레이드
- 장비 정보: 내구도, 고유능력, 능력치(물리공격+, 치명타%, 마법공격+)
- 장착 / 대장간(강화) 버튼
- 강화 UI (T1 → T2 → T3 티어 업그레이드 트리 표시)
- 강화 연출: 대장간 씬에서 망치로 두드리는 애니메이션

### 5-4. 퀵슬롯 패널 (메인 화면 중단)
- 6칸 슬롯에 NPC/장소/아이템 단축 아이콘 배치
- 아이콘에 `!` 뱃지 → 새 알림/퀘스트 있음을 표시

---

## 6. NPC 및 대화 시스템

| NPC | 장소 | 역할 |
|---|---|---|
| 접수원 아가씨 | 모험가협회(도서관) | 퀘스트 수주, 신규 등록 |
| 연금공방 아가씨 | 연금공방 | 물약/기술 제작 의뢰 (CUSTOM ORDER) |

- NPC 대화 시 하단 절반이 대화창으로 전환
- 대화창 구성: `| NPC이름`, 대사, 선택지 버튼들

### CUSTOM ORDER (연금공방)
- 제작 카테고리: 물약, 기술 (추가 슬롯 잠금)
- 제작의뢰 버튼으로 접근

---

## 7. 퀘스트 시스템

- 모험가협회에서 퀘스트 수주
- 퀘스트 아이콘 목록 (퀵슬롯 패널에 표시)
- 우측 상단 HUD에 현재 퀘스트 진행도 표시 (예: `젤리 풍 0/1`, `품직 젤리 00/5`)

---

## 8. 마물 도감

- 지역별 몬스터 목록 (신들바람 숲, 시들어가는 사막 등)
- 포획 여부에 따라 슬롯 잠금/해제
- 완성도 게이지 및 등급별 보상 잠금 표시
- 몬스터 번호 표기 (001, 002...)

---

## 9. 재화 시스템

| 재화 | 아이콘 | 용도 |
|---|---|---|
| 골드 | 황금 동전 | 기본 화폐 |
| 젬(보석) | 파란 보석 | 프리미엄 화폐 |

- 최상단 HUD 좌측에 항상 표시

---

## 10. 구현 우선순위 제안

1. **씬 구조**: 마을 씬, 전투 씬, NPC 대화 씬
2. **핵심 UI**: HUD(HP/EXP/재화), 하단 탭 메뉴, 퀵슬롯 패널
3. **전투**: 횡스크롤 이동 + 근접 공격 + 데미지 팝업
4. **캐릭터 시스템**: HP, EXP, 레벨, 장비 슬롯
5. **NPC 대화**: 대화창 + 선택지 UI
6. **인벤토리 & 장비**: 그리드 인벤토리, 장비 팝업
7. **퀘스트**: 수주 → 진행도 HUD 표시 → 완료
8. **미니언**: 포획 → 도감 등록 → 전투 동반
9. **제작 시스템**: 연금공방 CUSTOM ORDER
10. **강화 시스템**: 대장간 T1→T2→T3 강화 트리
