<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Popup

## Purpose
UIManager 팝업 스택 위에서 동작하는 모달/비모달 팝업 20종이 모인 디렉토리. 모든 클래스는 `UI_Popup`(→ `UI_Base`)을 상속하며, enum 기반 Bind로 프리팹 노드를 잡고 `SetInfo(...)`로 데이터를 주입받는다. 도감(Bestiary/MinMon), 물약/능력 제작(연금공방), 장비, 인벤토리, 여정(이동), NPC 대화, 설정 등 게임의 거의 모든 콘텐츠 시스템의 상세 UI가 여기에 있다. 팝업 열림 중에는 `Time.timeScale = 0`이므로 연출은 전부 unscaled 시간(`WaitForSecondsRealtime`, DOTween `SetUpdate(true)`) 기반이다.

## Key Files
| File | Description |
|------|-------------|
| `UI_Popup.cs` | 모든 팝업의 base 클래스. `Managers.UI.SetCanvas`/`ClosePopupUI(this)`, `_frameRT` 기준 `OpenAnimation()`(DOAnchorPosY, OutBack, `_openFromY/_closeToY`로 방향 커스텀), `OnClickBG()`(역방향 애니 후 닫기), `ClosePanelWithAnim()` 제공 |
| `UI_AttactedPopup.cs` | [미니몬 관심] 비모달 Side 알림. `SetInfo(int dataId, bool storageFull)` — 우측 슬라이드인 → 3초 → 슬라이드아웃 후 `Managers.Resource.Destroy`로 자기파괴. 팝업 스택/timeScale 미사용, `Managers.UI.ShowAttractedPopup`로 직접 생성 |
| `UI_BestiaryPopup.cs` | 마물 도감 팝업. 지역별 몬스터 그리드(`REGION_STUB` 스텁 데이터), 완성도 + 도감 스킬 4종(HybridKinship/MasterHandler/Retrievion/MemoryExpansion). 기록도/공명도/등급은 `Managers.MinMon`으로 위임하는 static 헬퍼(`GetRecordRate`/`GetSync` 등) 제공. `SetInfo()`, `PlayGradeUp(int dataId, int newStar)`, `RefreshCompanionVisuals()` |
| `UI_CharacterPopup.cs` | 캐릭터 상태(스탯) 팝업. STR/VIT/DEX/MAG 투자 포인트 + PATK/MATK/HP/DEF의 기본·증가·부스트 분해 표시와 상세(치명타/명중/저항 등) 토글. `SetInfo(PlayerController player)` |
| `UI_CustomOrderPopup.cs` | 연금공방 제작의뢰(CUSTOM ORDER) 팝업. 물약/기술 버튼 그룹 토글(`_currentGroup`), DialogueAnimator 타이핑 대사, 물약 프레임 클릭 → `UI_PotionDetailPopup` 연계. `SetInfo()` |
| `UI_DialoguePopup.cs` | NPC 대화창. `Managers.Data.DialogueScriptDic`의 groupKey로 라인 목록을 받아 DialogueAnimator로 타이핑/스킵/다음 진행. `SetInfo(string groupKey, string speakerName, Sprite portrait = null, Action onFinished = null, bool randomSingleLine = false, string portraitAnimState = "recep_normal")` |
| `UI_EquipmentPopup.cs` | 장비 팝업 1차 구현 — 7슬롯(Weapon/Helmet/Cloak/Armor/Glove/Boots/Ring) 리스트 네비게이션(한 번에 한 슬롯 패널만 표시)만 구현. 장착/강화/상세는 후속. enum `OpenHelemtListBtn` 오타는 프리팹 이름 호환을 위해 의도적으로 유지 |
| `UI_InventoryPopup.cs` | 소지품 인벤토리 팝업. `Managers.Game.Inventory`를 일반(25슬롯)/귀중품(5슬롯)으로 분류해 `UI_InvSlot`/`UI_ImportantSlot`을 `MakeSubItem`으로 생성. 정렬 버튼, NEW 뱃지(`OnInventoryNewChanged` 구독), 닫을 때 `ClearAllItemNew()`. `SetInfo()` |
| `UI_ItemPopup.cs` | 아이템 상세 팝업. `ItemDic`/`RequisiteItemDic`에서 이름·등급(등급색 Glow)·발견장소·설명·보유수량 표시, 사용 버튼 그룹은 항상 숨김. `SetInfo(int id)` |
| `UI_MinionInfoPopup.cs` | [나의 미니몬] 팝업. 포획한 미니몬의 능력치 등급(outline)/공명도/사용 스킬/동행(부르기·동행중) 토글. `SetInfo(int dataId)`, 포획 결과 연출 `PlayCatchResult(bool first, bool improved)` |
| `UI_MonsterDetailPopup.cs` | 도감 몬스터 상세 팝업. 능력치 14종·드롭 아이템·몬스터 스킬 잠금 표시, [내 미니몬] 버튼 → `UI_MinionInfoPopup`. `SetInfo(int dataId)`, `RefreshCompanionState()` |
| `UI_PotionDetailPopup.cs` | 물약 상세정보 팝업(슬36~40). HP/SPD 물약을 내부 `PotionView`로 정규화, 설명 ↔ 필요재료 토글, 다음 단계 미리보기, 제작 버튼 처리. `SetInfo(int dataId = -1, bool fromCustomOrder = true)` |
| `UI_PotionPopup.cs` | 물약창 팝업(슬41~42). HP/Speed 슬롯 + 쿨타임 슬라이더(위→아래) + Finish Anim + 자동물약 On/Off와 HP Ratio 슬라이더(기억 전/후 분기). `SetInfo()`, `MarkAutoPotionMemorized()` |
| `UI_SettingPopup.cs` | 설정 팝업. 언어(한/영) 선택과 BGM/SFX/데미지텍스트/카메라쉐이크 토글 — 전부 `Managers.Game`의 플래그(`IsBGSoundOn` 등)를 읽고 쓴다 |
| `UI_SkillBookPopup.cs` | [능력 목록] 팝업. AbilityManager 기반 보유 능력 아이콘(`UI_SkillBookSkill` 동적 생성) + 기억력(used/max) 표시, 능력 종류 필터 5버튼(프리팹 authored DOTweenAnimation 트리거). 아이콘 클릭 → 능력 정보창. `SetInfo()`, `RefreshUI()` |
| `UI_SkillDetailPopup.cs` | [능력 정보창] — 디렉토리 최대 파일. 능력 상세 + 기억(Memorize)/망각, 다음 등급 미리보기 토글, 요구 재료·장비·미니몬 게이팅 박스(클릭 시 각 상세 팝업 연계), 설명 길이에 따른 프레임 자동 사이즈. `SetInfo(int skillId = -1)`, `SkillId` 프로퍼티. `Managers.Ability` 의존 |
| `UI_TamePopup.cs` | [미니몬 잡기!] 포획 미니게임 팝업. 로딩→등장→길들이기/만드라고라 분기→성공/실패 연출 시퀀스, 확률/천장 판정은 `Managers.MinMon.TryCatch`. 전용 BGM `PushBgm` 후 OnDestroy에서 복귀. `SetInfo(int dataId)` |
| `UI_TravelPopup.cs` | 여정(장소 이동) 팝업. `TravelNodeData` 기반 TOWN/EXPLORE 대분류 → 마을/필드 중분류 → 목적지 소분류 버튼 트리(`UI_TownSelectButton`/`UI_FieldSelectButton`/`UI_MapSelectButton` 동적 생성), 퀘스트 목적지 VFX(`Managers.Quest`), 현재 위치/행선지 색상 규칙. `SetInfo()`, `RefreshUI()` override |
| `UI_UnlockButtonPopup.cs` | 기능(버튼) 해금 튜토리얼 팝업. TMP 글자별 알파 typewriter 연출(LateUpdate에서 vertex color 강제), 클릭 시 타이핑 중이면 스킵·완료 상태면 닫기. `SetInfo(string iconSprite, string iconNameId, string iconDescId)` |
| `UI_WarningPopup.cs` | [포획 창고 가득] 비모달 경고. `SetInfo(int dataId)` — AttactedPopup과 동일한 우측 슬라이드인/아웃 후 자기파괴, 버튼 없음. `Managers.UI.ShowWarningPopup(dataId)`로 표시 |

## For AI Agents

### Working In This Directory
- 새 팝업은 `UI_Popup` 상속 + `Awake() => Init();` + `base.Init()` 체크가 기본 골격. `Init()`에서 `BindImage/BindText/BindButton/BindObject(typeof(enum))`으로 프리팹 노드를 바인딩한다 — enum 멤버 이름은 프리팹 GameObject 이름과 완전 일치해야 하므로 프리팹의 오타(`OpenHelemtListBtn` 등)도 enum에 그대로 유지한다.
- 열기/닫기는 `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()`로만. 예외는 비모달 알림 2종(`UI_AttactedPopup`, `UI_WarningPopup`)으로, 전투 중 게임을 멈추지 않기 위해 스택/timeScale을 쓰지 않고 `Managers.UI.ShowAttractedPopup`/`ShowWarningPopup`으로 직접 생성 후 스스로 `Managers.Resource.Destroy`한다.
- 데이터 주입은 팝업 생성 직후 `SetInfo(...)` 호출 컨벤션. 시그니처는 팝업마다 다르다(위 표 참조).
- 팝업이 열리면 `Time.timeScale = 0` — 모든 코루틴 대기는 `WaitForSecondsRealtime`, 모든 DOTween은 `.SetUpdate(true)`를 붙여야 동작한다.
- 진입/닫기 애니메이션: `_frameRT`에 프레임 RectTransform을 넣고 `OpenAnimation()` 호출, 배경 버튼에 `BindEvent(OnClickBG)` 연결(역방향 애니 후 닫기). 등장 방향은 `_openFromY/_closeToY`로 조정(예: SettingPopup은 +350).
- 언어 변경 대응: `Managers.Game.OnLanguageChanged += RefreshTexts` 구독 후 반드시 `OnDestroy()`에서 해제. 텍스트는 `Util.Localize(key)` + `ApplyFont(tmp, FontType.UI)`.
- 연출은 프리팹에 authored된 `DOTweenAnimation`을 코드에서 트리거(`DORestart`/`DORewind`/`DOPlayBackwards`)하는 것이 프로젝트 관례. 코드로 대체할 때는 `[DOTweenAnimation 규약 예외 — 의도적 코드 대체]` 주석으로 사유를 남긴다(UI_AttactedPopup, UI_SkillDetailPopup 참고).
- .cs는 UTF-8(BOM 없음). 게임 수치는 JSON(`Managers.Data.*Dic`)에서 로드하고 하드코딩하지 않는다.

### Common Patterns
- **enum Bind 패턴**: `#region Enum`에 Images/Texts/Buttons/GameObjects enum 선언 → `Bind*(typeof(...))` → `GetImage/GetText/GetButton/GetObject((int)Enum.Member)`로 접근. 클릭은 `gameObject.BindEvent(handler)`.
- **SubItem 동적 생성**: 리스트형 UI는 `Managers.UI.MakeSubItem<T>(parent)`로 `UI_SubItem`을 생성하고 각 아이템의 `SetInfo(...)`로 채운다(Inventory→UI_InvSlot, SkillBook→UI_SkillBookSkill, Travel→UI_TownSelectButton 등). 기존 자식은 `Managers.Resource.Destroy`로 정리.
- **타이핑(typewriter) 연출**: NPC 대사류는 공용 `DialogueAnimator` 컴포넌트(`GetOrAddComponent`)의 `PlayTyping`/`SkipTyping` 사용(Dialogue/Tame/CustomOrder). UnlockButtonPopup은 TMP vertex alpha를 직접 조작하는 자체 구현. 공통 UX: 타이핑 중 클릭=전체 표시 스킵, 완료 후 클릭=다음/닫기.
- **비모달 자기소멸 알림**: 슬라이드인 → `WaitForSecondsRealtime` 유지 → 슬라이드아웃 → `OnComplete(() => Managers.Resource.Destroy(gameObject))`.
- **등급 색상/이름**: `Util.GetGradeColor(EquipGrade)` + 한/영 등급명 스위치(UI_ItemPopup의 `GetGradeName` 참고).
- **미니몬 데이터 접근**: 기록도/공명도/등급 조회는 `UI_BestiaryPopup`의 static 헬퍼를 경유(내부적으로 `Managers.MinMon` 위임) — `UI_MinionInfoPopup`이 `using static UI_BestiaryPopup`으로 재사용.

## Dependencies

### Internal
- **Managers**: `Managers.UI`(팝업 스택/SetCanvas/MakeSubItem), `Managers.Data`(CreatureDic, ItemDic, RequisiteItemDic, DialogueScriptDic, HP/SPD 물약 등 JSON 딕셔너리), `Managers.Game`(Inventory, Language, 설정 플래그, OnLanguageChanged/OnInventoryNewChanged 이벤트), `Managers.Resource`(Load/Destroy), `Managers.Sound`(PlaySFX/PushBgm), `Managers.MinMon`(도감/포획), `Managers.Ability`(능력 목록/기억), `Managers.Quest`(여정 퀘스트 VFX)
- **Data 클래스**: `CreatureData`, `ItemData`, `MinMonData`, `TravelNodeData`, `DialogueScriptLine`, `InventoryItem`
- **UI 계층**: `UI_Base`(Bind/Get/BindEvent), SubItem들(`UI_InvSlot`, `UI_ImportantSlot`, `UI_SkillBookSkill`, `UI_TownSelectButton`, `UI_FieldSelectButton`, `UI_MapSelectButton`)
- **기타**: `Define`(LocalizeKey, FontType, EquipGrade, Language, 아이템 ID 상수), `Util`(Localize, ApplyFont, GetGradeColor, FindChild), `DialogueAnimator`, `PlayerController`(CharacterPopup)

### External
- **DOTween** (`DG.Tweening`) — 전 팝업의 진입/닫기/연출 트윈, `DOTweenAnimation` 프리팹 트리거
- **TextMeshPro** (`TMPro`) — 텍스트 표시 및 vertex color 조작(typewriter)
- **UnityEngine.UI** — Image/Button/Slider/ScrollRect

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
