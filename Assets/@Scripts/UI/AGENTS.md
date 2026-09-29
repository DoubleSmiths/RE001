<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# UI

## Purpose
게임 전체 UI 프레임워크의 루트 디렉토리. 모든 UI 클래스의 공통 조상인 `UI_Base`(enum 기반 Bind/Get 바인딩, 이벤트 바인딩, 폰트/로컬라이징 관리, 팝업/페이드 연출 헬퍼)와 UGUI 포인터 이벤트를 C# `Action`으로 라우팅하는 `UI_EventHandler`, world-space 데미지 텍스트 팝업(`DamageFont`), UI Canvas 위 fake particle 버스트 유틸(`UIFx`)을 담는다. 실제 화면 UI는 하위 디렉토리(Popup/, Scene/, SubItem/, Toast/)에 계층별로 분리되어 있다.

## Key Files
| File | Description |
|------|-------------|
| `UI_Base.cs` | 모든 UI의 abstract 베이스. enum 이름으로 자식 오브젝트를 찾아 바인딩하는 `Bind<T>`/`Get<T>` 시스템, static `BindEvent`(UI_EventHandler 연결), `ApplyFont`(언어 변경 시 자동 폰트 갱신), `PopupOpenAnimation`/`FadeIn`/`FadeOut`/`FadeInOut` DOTween 연출 헬퍼 제공 |
| `UI_EventHandler.cs` | `IPointerClickHandler` 등 6종 UGUI 인터페이스 구현체. 포인터 이벤트를 `OnClickHandler` 등 public `Action` 필드로 전달하며, 클릭 시 `Managers.Sound.PlayButtonClick()` SFX를 자동 재생. `_pressed` 상태로 `OnPressedHandler`를 Update마다 호출 |
| `DamageFont.cs` | world-space `TextMeshPro` 데미지 팝업. `DamageFontMode` 5종(Normal/Critical/Miss/CriticalCallout/Heal)별 vertex gradient 색상 + outline 적용, DOTween으로 Fade-in+Scale(0.3s) → 상승+Fade-out(1.5s) 후 `Managers.Resource.Destroy`로 풀 반환. Critical 시 글로우 sprite(`CritAdditive`)를 텍스트 좌상단에 lazy 생성 |
| `UIFx.cs` | static 클래스. World-space ParticleSystem이 Screen Space Camera Canvas에 렌더되지 않는 한계를 우회해 RectTransform+Image+DOTween으로 구현한 fake burst(`PlayBurst`: Splash/Sparkle/Heal 3종)와 `Coffee.UIExtensions.UIParticle`로 실제 파티클 prefab을 UI 위에 렌더하는 `PlayParticle(parent, localPos, prefabKey, lifetime, scale)` 제공. `scale` 파라미터는 호스트 RectTransform의 localScale 배율(기본 1) — 월드용 프리팹이 UI에서 작게 보일 때 확대용 (해금 연출은 `UI_AreaBtnGroup.UNLOCK_FX_SCALE`=2.25배 사용) |
| `UI_Test.cs` | RectTransform 위치를 잡는 임시 테스트 스크립트 (영혼 상점 초기 상태) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Popup/` | 모달 팝업 UI (`UI_Popup` 계열, UIManager 팝업 스택 관리) — `Popup/AGENTS.md` 참조 |
| `Scene/` | 씬 고정 HUD (`UI_Scene` 계열: 타이틀/게임 허브/배틀 씬, 구역 버튼 그룹) — `Scene/AGENTS.md` 참조 |
| `SubItem/` | 리스트 아이템/버튼 템플릿 (`SetInfo()`로 자신을 갱신하는 재사용 단위) — `SubItem/AGENTS.md` 참조 |
| `Toast/` | 화면 상단 토스트 배너 (지역명 표시 등) — `Toast/AGENTS.md` 참조 |

## For AI Agents

### Working In This Directory
- **UI 클래스 계층**: `UI_Base` → `UI_Scene`(씬 HUD) / `UI_Popup`(모달 팝업) / `UI_SubItem`(리스트 아이템) / `UI_Toast`. 새 UI는 반드시 이 계층 중 하나를 상속한다.
- **Bind/Get 시스템**: 각 UI 클래스는 `enum Images/Texts/Buttons/GameObjects/Toggles`를 선언하고 `Init()`에서 `BindImage(typeof(Images))` 식으로 바인딩한다. **enum 멤버 이름은 프리팹 하이어라키의 자식 오브젝트 이름과 완전일치**해야 한다(`Util.FindChild` 재귀 검색). 바인딩 실패 시 `Failed to bind(이름)` 로그만 남고 계속 진행되므로 이름 오타에 주의.
- **Init() 규약**: `Init()`은 `_init` 플래그로 idempotent하게 작성한다 (`if (base.Init() == false) return false;` 패턴 필수). `UI_Base.Start()`가 자동 호출하지만 많은 클래스가 `Awake()`에서도 선제 호출한다.
- **이벤트 바인딩**: `Button.onClick` 대신 `gameObject.BindEvent(action)` 확장(내부적으로 `UI_Base.BindEvent` → `UI_EventHandler` GetOrAdd)을 사용. `-=` 후 `+=` 하므로 중복 등록이 방지된다. Drag 계열은 `Action<BaseEventData>`를 넘긴다.
- **팝업 열기/닫기**: 반드시 `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()`를 통해서만. 직접 `SetActive` 금지.
- **폰트/로컬라이징**: TMP 텍스트에 폰트를 줄 때는 `ApplyFont(text, FontType.XX)`를 사용 — 첫 등록 시 `Managers.Game.OnLanguageChanged`를 구독해 언어 변경 시 자동 갱신된다. `OnDestroy()` override 시 반드시 `base.OnDestroy()`를 호출해 구독 해제를 유지할 것.
- **클릭 SFX**: `UI_EventHandler.OnPointerClick`이 자동으로 `Managers.Sound.PlayButtonClick()`을 호출하므로 개별 버튼에 클릭 사운드를 추가로 넣지 않는다.
- **UI 위 파티클**: world-space `ParticleSystem`은 Screen Space Camera Canvas 위에 직접 렌더되지 않는다(구조적 제약). UI 위에 실제 파티클 prefab을 재생할 때는 반드시 `UIFx.PlayParticle()`(Coffee.UIExtensions `UIParticle` 래퍼) 경유 — 사용처: `UI_AreaBtnGroup.UnlockButtonAnim`(Star_01_Anim_13_a/GlowDot_01, scale 2.25), `UI_GameScene.PlayFollowDoneSequence` 완료 연출 등. 크기가 작게 보이면 프리팹을 고치지 말고 `scale` 인자로 배율 지정.
- **오브젝트 생성/삭제**: `Managers.Resource.Instantiate()` / `Managers.Resource.Destroy()` 사용 (풀 반환 포함). `DamageFont`처럼 풀에서 재사용되는 오브젝트는 `SetInfo` 시작에서 `DOTween.Kill(this)`로 잔상 트윈을 정리한다.

### Common Patterns
- `enum Buttons/Texts/Images/GameObjects` 선언 → `BindXxx(typeof(...))` → `GetButton((int)Buttons.X)` / `GetText((int)Texts.X)` / `GetImage((int)Images.X)` / `GetObject((int)GameObjects.X)` 접근.
- `SetInfo(...)` 메서드로 외부 데이터를 받아 자신을 갱신 (`DamageFont.SetInfo`, SubItem 계열 공통 패턴).
- DOTween `Sequence` 연출 + `SetUpdate(true)`(timeScale 무시, 팝업 일시정지 중에도 동작) + `OnComplete` 콜백에서 정리/풀 반환.
- Time-scale 독립 UI 연출: `PopupOpenAnimation`(0.8→1.0 scale), `FadeInOut`(Image alpha), `OnClickButtonAnim`(`DOTweenAnimation.DORestart`).
- 색상은 `Util.HexToColor("FF0000")` hex 문자열로 지정, TMP outline은 `Util.SetTMPOutline`.

### 스펙 출처 (기획서 매핑)
이 폴더 파일이 구현하는 스펙의 기획서 슬라이드 매핑. 덱 원본은 구글 슬라이드이며, 에디터 내 스펙 미러(검증 메뉴)는 `Editor/QuestVerifyMenu.cs` 참조.

| File | 기획서 슬라이드 |
|------|----------------|
| `UI_Base.cs` | 기획서1 슬27 |
| `DamageFont.cs` | 기획서1 슬64 |

대화 연출 스크립트 `DialogueAnimator.cs`는 `@Scripts` 루트 소속(기획서1 슬31·32, 수정안 슬1·2) — UI 문서에서 대화 연출을 참조할 때는 수정안 반영된 최신 시퀀스 기준으로 볼 것.

## Dependencies

### Internal
- `Managers` (UI, Resource, Game, Sound, Font, Object) — 매니저 싱글톤 진입점
- `Util` — `FindChild`, `GetOrAddComponent`, `HexToColor`, `SetTMPOutline`, `Localize`
- `Define` — `UIEvent`, `FontType`, `LocalizeKey` 등 enum/상수

### External
- DOTween (`DG.Tweening`) — 모든 UI 연출
- TextMeshPro (`TMPro`) — 텍스트 렌더링 (`TMP_Text`, `TextMeshPro`, `VertexGradient`)
- Coffee.UIExtensions (ParticleEffectForUGUI) — `UIParticle` (UIFx.PlayParticle)
- Unity UGUI (`UnityEngine.UI`, `UnityEngine.EventSystems`)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- `UI_ItemLinkHandler`는 TMP 본문의 재료 링크를 아이템 상세 팝업으로 연결하는 클릭 전용 이벤트 수신기다. 스크롤 본문에서는 드래그 인터페이스가 없는 이 컴포넌트를 써서 상위 ScrollRect의 드래그 입력을 유지한다.
