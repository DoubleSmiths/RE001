<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Managers

## Purpose
게임 전체 서비스 레이어의 단일 진입점. `Managers.cs`(MonoBehaviour 싱글톤)가 Core(범용 프레임워크) 7종과 Contents(게임 로직) 7종의 매니저 인스턴스를 필드로 소유하고, 외부에는 `Managers.XXX` static property로만 노출한다. `@Managers` GameObject에 부착되어 `DontDestroyOnLoad`로 유지되며, Awake에서 씬 재진입 시 중복 인스턴스를 제거하고(콤보 Tick이 잘못된 인스턴스로 가는 버그 방지), Update에서 `_combo.Tick(Time.deltaTime)`을 매 프레임 호출한다.

## Key Files
| File | Description |
|------|-------------|
| `Managers.cs` | 싱글톤 루트. lazy `Init()`(씬에서 `@Managers` 찾거나 생성), Awake 중복 제거, Update에서 ComboManager.Tick 구동, `Clear()`(Sound/Scene/UI/Pool/Object 정리). `Instance`는 private — 외부 접근은 static property만 가능 |

### 등록된 매니저 (Managers.cs 기준)
| 구분 | Static Property | 타입 | 비고 |
|------|----------------|------|------|
| Core | `Managers.Data` | `DataManager` | JSON → Dictionary 캐시 |
| Core | `Managers.Font` | `FontManager` | 언어/용도별 TMP 폰트 |
| Core | `Managers.Pool` | `PoolManager` | IObjectPool 래핑 |
| Core | `Managers.Resource` | `ResourceManager` | Addressables 로드/캐시/Instantiate |
| Core | `Managers.Scene` | `SceneManagerEx` | 씬 전환 + Managers.Clear |
| Core | `Managers.Sound` | `SoundManager` | BGM 크로스페이드/SFX |
| Core | `Managers.UI` | `UIManager` | 팝업 스택/토스트/SceneUI |
| Contents | `Managers.Game` | `GameManager` | GameData 세이브/로드, 재화·장비·인벤토리 |
| Contents | `Managers.Object` | `ObjectManager` | 몬스터/동행/데미지폰트/FX 스폰 |
| Contents | `Managers.MinMon` | `MinMonManager` | 미니몬 포획/등급/공명도/도감 |
| Contents | `Managers.Ability` | `AbilityManager` | 능력(스킬) 학습/기억(장착)/스탯 적용 |
| Contents | `Managers.Quest` | `QuestManager` | 메인퀘스트 진행/Notify/Unlock |
| Contents | `Managers.SubQuest` | `SubQuestManager` | 서브퀘스트 상태머신 |
| Contents | `Managers.Combo` | `ComboManager` | 콤보 Rank/타이머/버프 (Managers.Update가 Tick) |

`TimeManager`는 주석 처리되어 미등록. `CoroutineManager`(Contents/ 폴더 소재)는 Managers.cs에 등록되지 않은 독립 static MonoBehaviour이다.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Core/` | 게임 장르와 무관하게 재사용 가능한 범용 시스템 (리소스/풀/UI/사운드/씬/데이터/폰트). 상세: `Core/AGENTS.md` |
| `Contents/` | RE001 게임 로직 전용 매니저 (세이브데이터/퀘스트/미니몬/능력/콤보/오브젝트 스폰). 상세: `Contents/AGENTS.md` |

## For AI Agents

### Working In This Directory
- 매니저 접근은 반드시 `Managers.XXX` static property 경유. 매니저를 직접 `new` 하거나 필드 참조 금지 (인스턴스는 Managers.cs가 `new`로 소유).
- 모든 static property는 `Instance?._xxx` 형태 — Instance가 null이면 null 반환 가능하므로 초기화 이전 시점 호출부는 null 체크 필요 (`Managers.Game?.Player` 패턴이 코드 전반에 존재).
- **새 매니저 추가 절차**: ① Core/Contents 판단 후 해당 폴더에 클래스 생성(일반 C# 클래스, MonoBehaviour 아님) ② Managers.cs의 해당 #region에 `XxxManager _xxx = new XxxManager();` 필드 추가 ③ `public static XxxManager Xxx { get { return Instance?._xxx; } }` property 추가 ④ Init/Clear가 필요하면 호출 지점 연결(자동 호출 아님 — 아래 참고).
- **라이프사이클 주의**: `Managers.Clear()`는 Sound/Scene/UI/Pool/Object의 Clear만 호출한다. Data/Game/Quest/SubQuest/Combo/Font의 Init()은 Managers.cs가 아닌 씬 초기화 코드 등 외부에서 호출된다 — 새 매니저의 Init/Clear를 Managers.Clear에 자동으로 넣지 말고 기존 호출 흐름을 확인할 것.
- Core/Contents 구분 기준: 다른 게임에 그대로 가져갈 수 있으면 Core, RE001 게임 규칙(GameData, 퀘스트, 미니몬 등)을 알면 Contents.
- Awake의 중복 제거 로직과 Update의 `_combo?.Tick()`은 콤보 버그 수정 이력이 주석으로 남아있음 — 수정 시 씬 재진입 시나리오를 반드시 고려.

### Common Patterns
- 싱글톤 + lazy Init: `Instance { get { Init(); return s_instance; } }`, `GameObject.Find("@Managers")` 후 없으면 생성.
- 매니저는 MonoBehaviour가 아닌 순수 C# 클래스 (Managers.cs와 CoroutineManager만 MonoBehaviour). 코루틴이 필요하면 `CoroutineManager.StartCoroutine()` static 사용.
- 매니저 간 참조도 `Managers.XXX` 경유 (예: DataManager → `Managers.Resource.Load`).
- C# `event Action` 기반 Observer — 각 매니저가 상태 변경 이벤트를 발행하고 UI가 구독.

## Dependencies

### Internal
- `Contents/`, `Core/` 하위 매니저 전부 (필드로 소유)
- `Define`(상수/열거형), `Util`(GetOrAddComponent 등), UI 클래스 계층(UI_Popup 등), Controllers(PlayerController 등)

### External
- UnityEngine (MonoBehaviour, DontDestroyOnLoad)
- 하위 매니저 경유: Newtonsoft.Json, Unity Addressables, DOTween, TextMeshPro, UnityEngine.Pool

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
