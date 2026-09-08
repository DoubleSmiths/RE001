<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Scenes

## Purpose
씬 루트에 붙는 진입점 컴포넌트 디렉터리. `BaseScene`(abstract)이 EventSystem 보장과 `SceneType` 노출을 담당하고, 실제 게임 부트스트랩은 `GameScene.Awake()`에서 일어난다 — Addressables "PreLoad" 라벨 전체 로드 완료 후 Data → Game → Font → Sound → Quest → SubQuest → Combo 순으로 매니저를 초기화하고 `UI_GameScene`을 띄운다. 씬 전환 자체는 SceneManagerEx가 아니라 (현 구조상) GameScene 하나에서 Area 전환(Village1/Battle)으로 처리되며, 이 디렉터리는 초기화 순서의 단일 진실 공급원이다.

## Key Files
| File | Description |
|------|-------------|
| `BaseScene.cs` | abstract 베이스. `Awake()→Init()`, EventSystem 없으면 `Managers.Resource.Instantiate("UI/EventSystem")`로 생성해 `@EventSystem` 명명. `Define.Scene SceneType` 프로퍼티, `Clear()` abstract |
| `GameScene.cs` | 메인 씬. 에디터 Awake의 PreLoad 완료 콜백에서 매니저 초기화 후 BindPlayer 호출. 빌드는 TitleScene 프리로드를 사용하고 Start에서 BindPlayer 호출. 공통 연결은 Player 지정 → Combo.Init → RefreshStats → HUD 연결 → 방치 보상 순서 |
| `TitleScene.cs` | `SceneType = Define.Scene.TitleScene`만 설정하는 최소 구현 |

## For AI Agents

### Working In This Directory
- **초기화 순서가 계약이다**: `Managers.Data.Init()` → `Managers.Game.Init()`(LoadGame) → `Font/Sound/Quest/SubQuest.Init()` → `Init()`(UI 생성) → `Combo.Init()/AttachPlayer()` → `_player.RefreshStats()`. 새 매니저 Init을 추가할 때 이 체인 안의 올바른 위치에 넣어야 한다 (예: Quest는 Game 이후).
- `GameScene.Awake()`의 PreLoad 콜백 안에서만 매니저가 준비된다 — Addressables에 "PreLoad" 라벨이 없는 에셋은 이 시점에 로드되지 않는다 (Editor/SoundAddressableRegistrar, EquipPrefabAddressableRegistrar가 라벨을 붙이는 이유).
- `RefreshStats()` 재호출 주석 주의: `BaseController.Awake → RefreshStats`가 Data/Game 초기화 **전에** 1회 실행되므로, 초기화 완료 후 반드시 다시 호출해 세이브된 Level/PlayerStatusData를 반영한다. Player 초기화 관련 수정 시 이 이중 호출을 깨지 말 것.
- 씬 UI는 `Managers.UI.ShowSceneUI<UI_GameScene>()`으로만 생성 (직접 Instantiate 금지).
- 새 씬 추가 시: `Define.Scene` enum에 값 추가 → `BaseScene` 상속 → `Init()`에서 `SceneType` 설정 + `Clear()` 구현.
- 빌드는 TitleScene 프리로드에서 매니저를 초기화한다. GameScene.Start에서 공통 BindPlayer를 호출해 모든 Awake가 끝난 후 플레이어·HUD·콤보·방치 보상을 연결한다. 에디터 직접 진입은 프리로드 완료 콜백에서 같은 메서드를 호출한다.

### Common Patterns
- `Awake() → Init()` 템플릿 메서드 패턴: 자식은 `protected override Init()`에서 `base.Init()` 호출 후 자기 설정.
- 비동기 프리로드 콜백 패턴: `LoadAllAsync<Object>("PreLoad", (key, count, totalCount) => { if (count == totalCount) {...} })`.
- `#if UNITY_EDITOR / #else` 로 에디터 전용 부트스트랩 분리.

## Dependencies

### Internal
- `Managers` (Resource, Data, Game, Font, Sound, Quest, SubQuest, UI, Combo)
- `Define.Scene` enum (Utils/Define.cs)
- `UI_GameScene`, `PlayerController`

### External
- UnityEngine (`MonoBehaviour`, `GameObject.FindObjectOfType`, `FindAnyObjectByType`)
- UnityEngine.EventSystems (`EventSystem`)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
