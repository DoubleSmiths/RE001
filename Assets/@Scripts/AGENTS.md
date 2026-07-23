<!-- Parent: ../../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# @Scripts

## Purpose
RE001의 모든 런타임/에디터 C# 소스 코드 루트. Manager 싱글톤 서비스 레이어, 엔티티 컨트롤러 계층, UI 팝업 스택 시스템, JSON 데이터 클래스, 에디터 도구로 구성된다. 아키텍처 원칙: 모든 시스템은 `Managers.cs` 하나를 통해서만 접근하고, Core(범용 프레임워크)와 Contents(게임 로직)를 분리한다.

## Key Files
| File | Description |
|------|-------------|
| `DialogueAnimator.cs` | NPC 대화 연출 컴포넌트 (루트 단독 스크립트). 오프닝 시퀀스 `PlayOpenSequence` — 클릭 즉시 Dialogue Box+초상화 0.3s / 배경(Gradient+Panel) 1.0s **동시** 페이드(OutQuad), 0.3s 시점에 이름 ON+타이핑 시작 콜백. 글자별 알파 페이드 타이핑(TMP 메시 재갱신 대응으로 LateUpdate에서 강제 적용, `SkipTyping` 지원), 감정 연출(DOShake/Punch — `_textOriginPos`로 프리팹 저작 텍스트 위치 보존·복원), 초상화 발화 강조, Arrow 점멸 루프 |

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
- **UI 캔버스 위 파티클**: 월드 ParticleSystem은 Screen Space Camera 캔버스 위에 직접 렌더되지 않는다 — 반드시 `UIFx.PlayParticle()`(UIParticle 래퍼) 경유 (UI/UIFx.cs 참조).
- 게임 수치는 JSON 데이터(`Assets/@Resources/Data`)로 관리 — 코드 하드코딩 금지. `DataId`(int)가 기본 키.
- 모든 스크립트는 UTF-8 (BOM 없음).

### 스펙 출처 (기획서 매핑)
게임 스펙의 원전은 구글 슬라이드 기획서 2종 — **기획서1**(70장: UI/연출/전투), **기획서2**(54장: 도감/미니몬/능력) + 수정안. 코드 주석의 "슬N"/"슬라이드 N" 표기가 이 슬라이드 번호를 가리키며, 에디터 미러는 `Editor/QuestVerifyMenu.cs`(슬라이드별 시연 메뉴). 파일별 상세 매핑은 각 하위 디렉터리 AGENTS.md의 동명 소절 참조.

| 파일 | 기획서 슬라이드 |
|------|----------------|
| `DialogueAnimator.cs` | 기획서1 슬31·32 (대화 연출), 수정안 슬1·2 (오프닝 페이드 타이밍) |

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
