<!-- Parent: ../../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# @Scripts

## Purpose
RE001의 모든 런타임/에디터 C# 소스 코드 루트. Manager 싱글톤 서비스 레이어, 엔티티 컨트롤러 계층, UI 팝업 스택 시스템, JSON 데이터 클래스, 에디터 도구로 구성된다. 아키텍처 원칙: 모든 시스템은 `Managers.cs` 하나를 통해서만 접근하고, Core(범용 프레임워크)와 Contents(게임 로직)를 분리한다.

## Key Files
| File | Description |
|------|-------------|
| `DialogueAnimator.cs` | NPC 대화 텍스트 연출용 컴포넌트 (루트에 위치한 단독 스크립트) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Managers/` | 싱글톤 서비스 레이어 — 진입점 `Managers.cs`, Core/Contents 분리 (see `Managers/AGENTS.md`) |
| `Controllers/` | 엔티티 동작 — 플레이어/몬스터/투사체/카메라 등 상속 계층 (see `Controllers/AGENTS.md`) |
| `UI/` | UI 시스템 — enum 바인딩 기반 UI_Base, 팝업 스택, 씬 HUD, 서브아이템 (see `UI/AGENTS.md`) |
| `Data/` | JSON 직렬화 데이터 클래스 + ILoader (see `Data/AGENTS.md`) |
| `Scenes/` | 씬 라이프사이클 스크립트 (see `Scenes/AGENTS.md`) |
| `Utils/` | Define(상수/열거형), 확장 메서드, 공용 유틸 (see `Utils/AGENTS.md`) |
| `Editor/` | 에디터 전용 도구 — 데이터 파서, 테스트 메뉴, Addressable 등록기 (see `Editor/AGENTS.md`) |

## For AI Agents

### Working In This Directory
- 매니저 접근은 반드시 `Managers.XX` static property 경유. 매니저 인스턴스를 직접 `new` 하지 않는다.
- 오브젝트 생성은 `Managers.Resource.Instantiate()`, 삭제는 `Managers.Resource.Destroy()` (풀 반환 포함). 자주 생성/삭제되는 오브젝트는 풀링 필수.
- 팝업은 `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()`로만 열고 닫는다 (직접 SetActive 금지).
- 상태 전환은 property setter를 통해서만 (직접 필드 변경 금지).
- 이벤트 구독은 `OnDestroy()`에서 반드시 해제.
- 게임 수치는 JSON 데이터(`Assets/@Resources/Data`)로 관리 — 코드 하드코딩 금지. `DataId`(int)가 기본 키.
- 모든 스크립트는 UTF-8 (BOM 없음).

### Testing Requirements
- `Editor/`의 테스트 메뉴(BattleTestMenu, PopupTestMenu, IntegrationTestMenu, QuestVerifyMenu 등)와 Unity 플레이 모드로 검증.

### Common Patterns
- `GetOrAddComponent<T>()` 확장 메서드 (Extension.cs)
- `ILoader<Key, Value>` 인터페이스로 JSON → Dictionary 캐싱
- C# `Action` 이벤트로 컨트롤러 ↔ UI 느슨한 결합 (컨트롤러가 UI를 직접 참조하지 않음)

## Dependencies

### Internal
- `Assets/@Resources/Data` — 런타임에 로드되는 JSON 데이터
- `Assets/@Resources/Prefab` — `Managers.Resource`로 로드되는 프리팹

### External
- Newtonsoft.Json, DOTween, TextMesh Pro, Addressables, DamageNumbersPro

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
