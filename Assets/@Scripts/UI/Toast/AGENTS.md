<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# UI/Toast

## Purpose
화면에 잠시 표시되고 자동으로 사라지는 토스트 배너 UI. 배너는 `UI_Toast` 하나로, 지역명(전투 구역 진입 페이드 연출 중)과 보상·실패·결과 알림이 모두 `Managers.UI.ShowToast(msg)` 경로로 표시된다 (UI_GameScene.EnterAreaWithFade의 t=1.0 시점 콜백 참조). 프리팹에 심어진 `DOTweenAnimation`(Move, 약 1초)으로 등장한 뒤 1.1초 후 1.5초에 걸쳐 페이드아웃한다.

## Key Files
| File | Description |
|------|-------------|
| `UI_Toast.cs` | 지역명 배너 토스트. `UI_Base` 직접 상속(팝업 스택 미사용). `SetInfo(string msg)`로 `StageTitle_Txt` 텍스트 교체 + scale 리셋, `OnEnable()`에서 프리팹의 `DOTweenAnimation.DORestart()` → 1.1s 대기 → `Fade()`(StageTitle 이미지 3장 + 텍스트를 1.5s DOFade(0)). `ApplyFont(FontType.UI)`로 언어 변경 시 폰트 자동 갱신 |

## For AI Agents

### Working In This Directory
- `UI_Toast`는 `UI_Popup`이 아닌 `UI_Base` 직상속 — UIManager 팝업 스택(`ShowPopupUI`/`ClosePopupUI`)을 타지 않는다. 표시는 `Managers.UI.ShowToast(msg)`를 통해서만 한다.
- 표시 수명이 코드에 하드코딩되어 있다: `OnEnable`의 DOTween Sequence가 `DOTweenAnimation` 재시작 → 1.1s interval → `Fade()`(1.5s). 등장 연출 자체는 프리팹의 `DOTweenAnimation` 컴포넌트에 있으므로 타이밍 변경 시 프리팹과 코드 양쪽을 확인할 것.
- 재사용(재표시) 시 `SetInfo`가 `transform.localScale = Vector3.one`으로 리셋하지만 `Fade()`로 내려간 이미지/텍스트 alpha는 코드에서 복구하지 않는다 — alpha 복원은 프리팹 DOTweenAnimation 또는 호출 측 책임임에 유의.
- enum 멤버 이름(`StageTitle`, `StageTitle_1`, `StageTitle_2`, `StageTitle_Txt`)은 프리팹 자식 오브젝트 이름과 완전일치해야 한다 (UI_Base Bind 규약).
- `OnDestroy` override 시 `base.OnDestroy()` 호출 유지 (`OnLanguageChanged` 구독 해제).
- **그리는 순서·입력**: `UIManager.ShowToast` 가 캔버스 순서를 토스트 전용(`TOAST_ORDER_BASE` 2000+, HUD·팝업 위)으로 올리고 `GraphicRaycaster` 를 끈다. 프리팹 값(100) 그대로면 화면 UI(1001)·큰 팝업에 가려졌고, 레이캐스트가 켜진 채 올리면 3초 동안 아래 탭 공격·버튼을 막는다.
- **긴 문장**: `StageTitle_Txt` 의 ContentSizeFitter(가로 = 선호 크기)를 `SetInfo` 에서 끈다. 켜 두면 칸이 글자만큼 늘어나 자동 크기(최소 0.5배)가 한 번도 줄이지 못하고 화면 밖으로 넘쳤다. 지금은 띠 폭(164) 안에서 두 줄까지 줄어든다.
- **확인할 때**: `unity-cli exec` 안에서 바로 띄운 토스트는 exec 컴파일로 멈춘 프레임(수 초) 때문에 트윈이 한 번에 끝나 안 보인다 — 코루틴에서 0.5초 기다린 뒤 띄워 찍는다.

### Common Patterns
- `enum Images/Texts` + `BindImage`/`BindText` → `GetImage`/`GetText` 접근 (UI_Base 표준 바인딩).
- `SetInfo(msg)` 패턴으로 외부에서 표시 내용 주입.
- `Awake()`에서 `Init()` 선제 호출 + `_init` 가드.
- 동적 텍스트는 내용 로컬라이즈 없이 `ApplyFont`로 폰트만 관리 (지역명 문자열은 호출 측에서 이미 Localize되어 들어옴).

## Dependencies

### Internal
- `UI_Base` — Bind/Get, ApplyFont, Init 가드
- `Managers.Game.OnLanguageChanged` — 폰트 갱신 이벤트
- 호출 측: `Managers.UI.ShowToast` (UIManager), `UI_GameScene.EnterAreaWithFade` (전투 진입 배너)
- `Define.FontType`

### External
- DOTween (`DG.Tweening` — Sequence, DOFade, DOTweenAnimation)
- TextMeshPro (UI_Base 경유 `TMP_Text`)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
