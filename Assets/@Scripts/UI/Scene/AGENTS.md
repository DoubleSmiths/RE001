<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# UI/Scene

## Purpose
씬에 고정되는 HUD UI 계층. `UI_Scene`(베이스: `Managers.UI.SetCanvas` 등록)을 상속한 타이틀 씬(`UI_TtitleScene`), 메인 허브 HUD(`UI_GameScene`), 배틀 씬 HUD(`UI_BattleScene`)와, `UI_GameScene`이 구역(마을/전투)마다 교체 장착하는 구역별 버튼 그룹의 abstract 베이스(`UI_AreaBtnGroup`)를 담는다. `UI_GameScene`은 이 게임의 중심 HUD로 재화/HP/EXP 표시, 하단 6탭 버튼(잠금·NEW 뱃지·하이라이트), 퀘스트 팔로우 UI, 구역 전환 페이드 연출을 모두 담당한다.

## Key Files
| File | Description |
|------|-------------|
| `UI_Scene.cs` | 씬 HUD 베이스. `Init()`에서 `Managers.UI.SetCanvas(gameObject, true)`로 Canvas를 씬 UI로 등록하는 것이 전부 |
| `UI_GameScene.cs` | 메인 허브 HUD (~1350줄). 재화 Lerp 표시+증감 색 플래시, HP/EXP 바(`UpdateHP`/`UpdateEXP`), 레벨업 연출(`OnLevelUp`), 하단 6탭(상태/가방/물약/장비/기술/도감) 클릭→팝업 열기+하이라이트+NEW 뱃지(Potion/Equipment/Skill은 GameManager에 저장·복원), 구역 전환(`EnterArea`/`EnterAreaWithFade`: FadeOut→`SwapAreaBtnGroup`→FadeIn→`PlayEnterAnimation`, Battle 진입 시 지역명 Toast+BG 스크롤 시작), 퀘스트 팔로우 UI(`RefreshQuestFollow`), 버프 아이콘(`UI_Buff`) 관리, `BindPlayer`로 PlayerController 이벤트 연결. HUD 이름 자리(`PlayerRankText`)는 다는 칭호(업적, ADR0040)가 있으면 금색 칭호 — `Mission.OnChanged` 구독(`RefreshPlayerName`). UNITY_EDITOR 전용 테스트 단축키/ContextMenu 다수 |
| `UI_AreaBtnGroup.cs` | 구역별 버튼 그룹 abstract 베이스. `Setup(UI_GameScene)`으로 부모 참조 주입 후 Init, virtual `PlayEnterAnimation()`(구역 진입 연출 hook), `UnlockButtonAnim`(버튼 해금 연출 — EventSystem 비활성화 + sortingOrder 9999 투명 입력 blocker 이중 방어로 연출 중 입력 차단), `PlaceNameAnim`, `ButtonMoveAnim` 공용 연출 제공 |
| `UI_BattleScene.cs` | 별도 배틀 씬용 HUD. 플레이어 HP 바(`BindPlayer`), 보스 HP 바(`ShowBossHP`/`UpdateBossHP`, 기본 숨김), 킬 카운트(`AddKillCount`), 구역 이름(`SetAreaName`), Pause(timeScale 토글)/ReturnHub(`Scene.HubScene` 로드) 버튼. `UI_GameScene`의 Battle 구역(BtnGroup 교체 방식)과는 별개 씬 |
| `UI_TtitleScene.cs` | 타이틀 씬 (파일명 오타 그대로 `Ttitle`). `Start()`에서 Addressables `"PreLoad"` 라벨 전체 로드 후 Slider로 진행률 표시, 완료 시 `Managers.Data/Game/Font/Sound/Quest/SubQuest.Init()` 순차 초기화 + StartButton 활성화 → 클릭 시 `Define.Scene.GameScene` 로드. StartText는 무한 Yoyo Fade |

## For AI Agents

### Working In This Directory
- 모든 씬 HUD는 `UI_Scene` 상속 + `Init()`에서 `base.Init()` 가드 후 `BindImage/BindText/BindButton/BindObject(typeof(enum))` 바인딩 (UI_Base의 enum 기반 시스템 — enum 이름은 프리팹 자식 이름과 완전일치 필수).
- **구역 전환은 `UI_GameScene.EnterAreaWithFade(AreaType)`만 사용**: prefab 이름 규약 `UI_{AreaType}BtnGroup`(예: `UI_Village1BtnGroup`, `UI_BattleBtnGroup`)으로 `AreaBtnGroupSlot`에 Instantiate → `Setup(this)` 호출. `CanEnterAreaTransition`이 사전 검증하고 `_isAreaTransitioning` 플래그로 중복 전환 차단. 비-Battle 구역 진입 시 `TravelNodeDic`의 `AreaType` 문자열 매칭으로 `CurrentTravelAreaId` 자동 갱신 (Battle은 TravelPopup이 노드 id를 직접 set).
- Battle 진입 타이밍: Fade Out 0.5s → 검은 화면에서 교체 → Fade In 1.0s, t=1.0에 `Managers.UI.ShowToast(지역명)`, t=2.0에 BG 스크롤 시작 + `PlayEnterAnimation`. 구역별 BGM도 `SwapAreaBtnGroup`에서 자동 전환(Battle=`BGM_BWF`, 그 외=`BGM_Village1`).
- 새 구역 버튼 그룹은 `UI_AreaBtnGroup`을 상속하고 진입 연출은 `PlayEnterAnimation()` override로 구현한다 (Fade In 완료 시점에 호출됨).
- 이벤트 구독은 `Init()`/`SetInfo()`/`BindPlayer()`에서 `+=`, **반드시 `OnDestroy()`에서 전부 `-=`** (UI_GameScene.OnDestroy가 Game/MinMon/Quest/UI 매니저 이벤트를 모두 해제하는 것이 기준 예시). `base.OnDestroy()` 호출 유지.
- 하단 탭 잠금은 `Managers.Game.IsBtmButtonUnlocked(TabType)` 기준 — 잠긴 버튼은 GameObject 자체를 숨김(`ApplyBtmLock`). 해금은 QuestManager의 `OnBtnUnlocked(btnId 문자열)` 이벤트로 통지받는다 (btnId는 `Define`의 `UNLOCK_*` 상수와 완전일치 매칭).
- NEW 뱃지 정책: Potion/Equipment/Skill은 `Managers.Game.SetTabNew/IsTabNew`로 저장·복원(탭 열면 해제+저장), Bag/Bestiary/Status는 데이터 파생(인벤토리 IsNew, MinMon 창고, StatusPoint)이라 각자 Refresh가 처리.

### Common Patterns
- `enum Images/Texts/Buttons/GameObjects` + `Get{Button,Text,Image,Object}((int)Enum.X)` 접근, `gameObject.BindEvent(handler)` 클릭 바인딩.
- 수치 표시 연출: DOTween `DOTween.To`로 재화 Lerp(0.5s), `DOFillAmount`로 바 게이지(0.2s), 증감 시 `DOColor` 플래시(증가 초록 #00FF00 / 감소 빨강 #FF0000, HP는 hurt #FF0000 / heal #58FF00) + Scale punch. 트윈 재시작 전 `DOKill()` 필수.
- `BindPlayer(PlayerController)` — `OnHpChanged`/`OnExpChanged`/`OnLevelUp`/`OnBuffApplied` 등 컨트롤러 이벤트를 HUD 갱신 메서드에 연결 (컨트롤러는 UI를 직접 참조하지 않는 Observer 패턴).
- 퀘스트 팔로우 수량 표기: `(<color=#XX>{보유}</color>/{필요})` — 0=빨강, 부족=주황, 충족=초록 rich text.
- 테스트 코드는 `#if UNITY_EDITOR` + `[ContextMenu("Test/...")]`로 격리 (숫자키 재화, F1-F3 퀘스트 점프, U 전체 해금, J 전투 진입 등).

## Dependencies

### Internal
- `Managers` — UI(ShowPopupUI/ShowToast/OnPopupClosed/SetCanvas/SceneUI), Game(재화/레벨/인벤토리/탭 해금/언어 이벤트), Resource(Load/Instantiate/Destroy/LoadAllAsync), Quest(OnFollowUIChanged/OnBtnUnlocked), MinMon(도감 NEW), Data(TravelNodeDic), Scene(LoadScene), Sound(PlayBgm), Object(ShowHealFont), SubQuest/Font(타이틀 Init)
- `UI_Base`/`UI_Scene` 상속 계층, `UI_GameScene` ↔ `UI_AreaBtnGroup`(및 `UI_BattleBtnGroup`) 상호 참조
- Popup 계열: `UI_SettingPopup`, `UI_CharacterPopup`, `UI_InventoryPopup`, `UI_PotionPopup`, `UI_EquipmentPopup`, `UI_SkillBookPopup`, `UI_BestiaryPopup`, `UI_UnlockButtonPopup`, `UI_ItemPopup`
- `PlayerController`(이벤트/BuffInfo), `UI_Buff`, `BackgroundScroller`, `Util`, `Define`(AreaType/TabType/State/LocalizeKey/UNLOCK_* 상수), `Data.QuestData`

### External
- DOTween (`DG.Tweening` — Sequence, DOFade, DOFillAmount, DOPunchScale, DOTweenAnimation, DOVirtual)
- TextMeshPro (`TMPro`)
- Unity UGUI (`UnityEngine.UI` — Image, Button, Slider, GraphicRaycaster), Addressables(ResourceManager 경유)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- `UI_AreaBtnGroup` 장소명·해금 연출은 그룹 수명에 연결한다. 해금 Tween의 정상 완료/중단 양쪽에서 입력 블로커와 EventSystem을 복원하며, 복원은 한 번만 실행해 완료 콜백이 시작한 후속 해금의 잠금을 보존한다.
