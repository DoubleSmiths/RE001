<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# SubItem

## Purpose
부모 팝업/씬 UI가 동적으로 생성해 리스트·그리드·HUD에 채워 넣는 재사용 UI 조각(슬롯, 버튼, 바, 아이콘) 모음. 프로젝트에 `UI_SubItem` 클래스는 없으며, 이 디렉터리의 클래스는 대부분 `UI_Base`(`../UI_Base.cs`)를 직접 상속하고, 예외적으로 `UI_BattleBtnGroup`/`UI_Village1BtnGroup` 두 개는 `UI_AreaBtnGroup`(`../Scene/UI_AreaBtnGroup.cs`, `UI_Base` 파생)을 상속하는 "구역(Area) 단위 버튼 그룹"으로, UI_GameScene이 마을↔전투 구역 전환 시 통째로 스폰/파괴한다. 각 클래스는 `SetInfo(...)`로 데이터를 받아 자신을 갱신하며, 생성은 `Managers.UI.MakeSubItem<T>()` 또는 `Managers.Resource.Instantiate()`를 통한다.

## Key Files
| File | Description |
|------|-------------|
| `UI_BattleBtnGroup.cs` | 전투 구역 전체 UI 그룹(`UI_AreaBtnGroup` 상속). ComboManager 이벤트 구독으로 콤보 게이지/Rank 색/버프 표시, 전투 기록(mm:ss.ss 타이머 + Smooth 보간 피해/회복량), 드롭 아이템 리스트(`MonsterController.OnItemDropped` → `UI_BattleDropItem` 생성/등급순 정렬), 몬스터 스폰 코루틴(`CoBattleLoop`), 귀환(2단계 확인)/여정 버튼. `PlayEnterAnimation()` 오버라이드에서 `StartBattle()` 호출, `BeginExitTransit()`으로 퇴장 연출. v2 스테이지: 처치마다 `GameManager.AddStageKill`, 처치 수가 차고 자동 도전이면 `CoStageBoss`(보스 스폰·30초 제한·첫 돌파 시 다음 스테이지로 자동 이동, 실패 시 퇴각). v2 요일 던전(ADR0034): `StartBattle` 에서 `Managers.Dungeon.Begin()` 이 참이면 `CoDungeon`(가깝게·빨리 한 마리씩 스폰, 스테이지 처치에 안 셈, 시간 초과 시 남은 몬스터 퇴각 → `Finish` → 그 스테이지 사냥 이어감). `EndBattle` 은 진행 중 던전을 정산한다 `StartBattle` 은 예약된 판을 시련(`CoPromotion`) → 심연(`CoAbyss`) → 요일 던전(`CoDungeon`) 순으로 보고 없으면 사냥(ADR0037). 보스 한 판은 `CoFightBoss`(스테이지 보스·시련·심연 보스 공용 — 알림·보스 곡·체력 채움·HP 바·맞붙은 뒤 제한 시간·지면 물러남, 결과는 `_bossKilled`/`_bossPlayerDown`). `EndBattle` 은 던전·시련·심연 중 실행 중인 판을 끝낸다. |
| `UI_BattleDropItem.cs` | 전투 중 드롭 아이템 슬롯 1칸. `SetInfo(dataId, initialCount)`로 아이콘/등급(Glow 색·Exceptional+ 파티클)/카운트 세팅, `AddCount(by)`로 중복 획득 시 카운트 증가 + prefab DOTweenAnimation 재생. `Grade`/`DataId` 프로퍼티는 부모(UI_BattleBtnGroup)의 등급순 정렬에 사용 |
| `UI_Bestiary.cs` | 마물 도감의 "지역 한 덩어리"(지역명 + 몬스터 박스 그리드). `SetInfo(regionName, List<CreatureData>, onClickMonster)`가 기존 자식 제거 후 `MakeSubItem<UI_BestiaryBox>`로 박스를 채움. UI_BestiaryPopup의 Content 안에서 지역 수만큼 생성됨 |
| `UI_BestiaryBox.cs` | 도감 몬스터 1칸. `SetInfo(CreatureData, onClick)`이 기록도(`UI_BestiaryPopup.GetRecordRate`) 기준 3분류: 0%="?"잠금(클릭 불가), 10%미만=검정 실루엣+초록 Glow, 10%이상=풀컬러+Rarity Glow. 별등급 무한 DOPunchRotation, 동행 미니몬이면 frame 색 FF00CB + "Active Ef" prefab 부착(`RefreshCompanionVisual`) |
| `UI_Buff.cs` | UI_GameScene의 BuffDisplayer에 붙는 강화 버프 아이콘. `SetInfo(PlayerController.BuffInfo, onExpired)`. timed 버프는 Update에서 fillAmount 0→1 진행 + 잔여시간에 따라 점멸 가속(30/60/90% 구간), 만료 시 `onExpired(key)` 콜백 후 FadeOut 자멸. `Apply(b)`로 재발동 갱신, `Remove()`로 외부 해제 |
| `UI_FieldSelectButton.cs` | UI_TravelPopup의 사냥터(필드) 선택 버튼. `SetInfo(TravelNodeData)` → `NodeId` 저장 + 이름 Localize, 클릭 시 `OnClickFieldSelect?.Invoke(nodeId)`. `SetSelected`(EFrame 토글), `RefreshQuestVFX(HashSet<string>)`로 노드의 ClickBtnId/EnterBtnId가 활성 notify에 있으면 VFX 표시 |
| `UI_FieldTypeButton.cs` | UI_TravelPopup의 필드 대분류(타입) 버튼. 현재 골격만 존재 — `SetInfo()`는 빈 구현이고 클릭/선택/VFX 로직은 전부 TODO 주석 상태. `NodeId`/`OnClickFieldType` 필드만 준비됨 |
| `UI_BattleInput.cs` | v2 전투 입력(ADR0033) — UI_GameScene `In-Frame/BattleInput`(첫 자식)에 붙는다(빌더 `Tools/Remake V2/Build Battle Input`, 그림 `_tools/gen_battle_buttons.py`). 자식 셋은 **형제**: `BattleTapArea`(투명, 누르면 `PlayerController.RequestAttack` — PC Space·Z), `BattleAutoBtn`(AUTO 토글 `GameManager.AutoBattle` — 첫 스테이지 돌파 전엔 숨김, 돌파하면 `ClearStage` 가 열고 켠다(ADR0038, `AutoUnlocked`)), `BattleGuardBtn`(PointerDown → `TryGuard`, PC X·Shift, `BattleGuardCool` 원형 덮개 = `GuardCooldownNormalized`). 버튼이 탭 칸의 자식이면 누름이 탭 칸까지 올라가 막기+공격이 된다. 마을에선 버튼 숨김·탭 칸 레이캐스트 끔. 첫 맞붙음(`MonsterController.OnAnyEngaged`)부터 **한 번이라도 누를 때까지** 4초마다 '눌러서 공격'(`TapHintSeen` 은 첫 탭에 켜진다, AUTO 중엔 쉼), 첫 예고(`OnAttackTelegraph`)에 막기 도움말(`GuardHintSeen`), 예고마다 방패 버튼 들썩임 방패 버튼은 평소 반투명(`GUARD_IDLE_ALPHA` 0.55 — 큰 몬스터·보스를 가리지 않게), 공격 예고 때만 또렷해졌다가 `GUARD_ALERT_HOLD` 뒤 돌아간다(CanvasGroup). |
| `UI_ImportantSlot.cs` | 중요 아이템(퀵슬롯형) 슬롯. `SetInfo(InventoryItem, ItemData)`로 아이콘/개수(2개 이상만)/NEW 라벨 표시, 클릭 시 `ClearItemNew` 후 `UI_ItemPopup` 오픈. `PlayOpenAnim(delay)`: Scale (0,1,0)→1, 0.15s OutQuad 등장 연출 |
| `UI_InvSlot.cs` | 가방(인벤토리) 그리드 슬롯. `SetInfo(InventoryItem, ItemData)`로 아이콘/개수/등급 Glow(`Util.GetGradeColor`)/NEW 라벨 세팅, 클릭 시 DOPunchScale + `UI_ItemPopup` 오픈. 빈 슬롯 상태는 `SetOpenBlock()`(열림, Glow 흰색 α40) / `SetBlock()`(잠김, BlockImg + Glow 0F0F0F) |
| `UI_MapSelectButton.cs` | UI_TravelPopup의 지도(대륙/맵) 선택 버튼. `SetInfo(TravelNodeData, isCurrentLocation)` — 현재 위치면 `Button.interactable=false` + 클릭 무시. `SetSelected`/`RefreshQuestVFX(HashSet<string>)`는 UI_FieldSelectButton과 동일 패턴 |
| `UI_MonsterHPBar.cs` | 화면 상단 몬스터 HP 바. `SetInfo(MonsterController)`가 이름 Localize + `monster.OnHpChanged` 구독(HP 0 이하 시 `PlayCloseAnimation`=DOPlayBackwards 후 Destroy). 희귀도별 색(FFD400/FF4300)·스케일(1.15/1.3)·Material(Path_Image_Knight/BeastLord) 적용. UI_BattleBtnGroup이 몬스터 조우(OnEngaged) 시 `MakeSubItem`으로 생성 |
| `UI_MonsterItemDropListBox.cs` | 미니몬 기록(UI_MonsterDetailPopup)의 전리품 1칸. `SetInfo(ItemData, count, chance, unlocked)` — 미해금이면 "?" + Glow 0F0F0F, 해금이면 아이콘 + 등급 Glow + `x{count} {chance:0.#}%` 표기 |
| `UI_MissionShortcut.cs` | v2 HUD 재화 줄의 미션 버튼(`CurrencyBar/MissionBtn`, 아이콘 Quest Icon). 누르면 UI_MissionPopup, 받을 것이 있으면 빨간 알림 점(`MissionBadge`, 0.5초마다 `MissionManager.HasClaimable` 확인) 미션 버튼 점에는 오늘 무료 입장이 남은 요일 던전(`DungeonManager.HasFreeEntry`)도 포함. 같은 컴포넌트가 상점 버튼(`ShopBtn`, Shop_Icon, 탭 Shop)에도 붙는다 — 상점 버튼의 점은 `ShopManager.HasFreeReward`(무료 크리스탈·월정액 매일·오프라인 2배). 알림 점은 이름이 ...Badge 인 자식. 도전할 수 있는 시련(`PromotionManager.CanChallenge`, ADR0037)도 알림 점을 켠다. |
| `UI_SkillBookSkill.cs` | 능력 목록(UI_SkillBookPopup)의 능력 아이콘 1칸. `SetInfo(abilityId, onClick)` — `Managers.Ability.IsTalent/IsEquipped/GetLevel`로 재능·기억중이면 "기억중" 배지 + Active_Ef, 레벨은 rich-text로 숫자만 색(최대=#FF00FF, 미만=#B9B900). 클릭 시 루트 DOTweenAnimation 재생 후 콜백 |
| `UI_StageBar.cs` | v2 스테이지 막대(ADR0030) — UI_GameScene `In-Frame/StageBar`에 붙는다(빌더 `Tools/Remake V2/Build Stage Bar`). 전투 구역에서만 보이며(`GameManager.InBattleArea`) `3-2 지역명` + 처치 N/10, 보스전엔 남은 시간(빨간 채움). 보스에게 져서 자동 도전이 꺼지면 금색으로 깜빡이고 누르면 `BossAutoChallenge=true`. 돌파한 스테이지는 이름이 금색. 요일 던전 중(ADR0034)에는 던전 이름 + `남은 N초 · M마리` + 남은 시간 빨간 채움. 값이 바뀐 칸만 다시 쓴다 심연(ADR0037) 중엔 '심연 N층' + 그 층 처치 수(보스 층은 보스 타이머), 시련 중엔 '시련 N단계' + 보스 타이머. |
| `UI_TownSelectButton.cs` | UI_TravelPopup의 마을 선택 버튼. `SetInfo(TravelNodeData, isCurrentLocation)` — 현재 위치면 interactable off + "현재 위치" 라벨 표시. `SetSelected`는 EFrame 토글 + 배경색 스왑(FFC500 선택/959595 기본). `RefreshQuestVFX` 동일 패턴. Init에 TODO 다수 잔존 |
| `UI_TrainingPanel.cs` | v2 훈련 패널(ADR0030) — UI_CharacterPopup `TrainingFrame`에 붙는다(빌더 `Tools/Remake V2/Build Training Panel`). 줄을 누르면 `GameManager.TryTrain` 1단계, 누르고 있으면 0.35초 뒤 0.07초마다 연속. 골드 부족·상한이면 줄을 흐리게 |
| `UI_Village1BtnGroup.cs` | 마을(Village1) 구역 전체 UI 그룹(`UI_AreaBtnGroup` 상속). Main/Guild/Alchemy/Monastery 4개 하위 버튼 그룹을 DOTween Sequence(페이드 + 버튼 순차 등장)로 전환하며 `SwitchBG`로 배경 prefab 교체, `Managers.Game.LastVillage1Group`으로 마지막 그룹 복원. 퀘스트 notify는 `s_notifyBindings` 정적 테이블(BtnID→VFX/NewTxt 매핑)로 일괄 갱신, 버튼 해금은 `OnBtnUnlocked`→`UnlockButtonAnim`(TravelButton/AlchemyButton) 또는 silent 토글(CustomOrder). NPC 대화(`UI_DialoguePopup`)/제작의뢰(`UI_CustomOrderPopup`)/여정(`UI_TravelPopup`) 팝업 진입점 건물 [대화](협회·연금공방·대장간·수도원·상점)는 볼 차례인 호감도 이야기가 있으면 잡담 대신 그것을 튼다(`TryPlayBondScene` → `BondManager.FinishScene`, ADR0042). 퀘스트 대사가 우선. |

## For AI Agents

### Working In This Directory
- **상속 베이스**: 15개 클래스는 `UI_Base` 직접 상속. `UI_BattleBtnGroup`·`UI_Village1BtnGroup`만 `UI_AreaBtnGroup`(../Scene/) 상속 — 이 둘은 리스트 아이템이 아니라 UI_GameScene이 구역 전환 시 통째로 갈아끼우는 화면 하단 버튼 그룹이며, `Setup(UI_GameScene)`으로 `_gameScene` 참조를 받고 `PlayEnterAnimation()` 진입 콜백을 오버라이드할 수 있다. 프로젝트에 `UI_SubItem` 클래스는 존재하지 않는다.
- **Init 계약**: `Init()`은 `if (base.Init() == false) return false;` 가드로 시작하고 `BindImage/BindText/BindButton/BindObject(typeof(enum))`으로 prefab 자식을 enum 이름으로 바인딩한다. **enum 멤버 이름 = prefab GameObject 이름**이므로 enum 순서/이름 변경은 prefab과 동기화 필수 (`UI_BattleBtnGroup.ApplyBattleTexts`는 enum 인덱스 하드코딩까지 있음: `i == 0 || i == 1 || ...`).
- **SetInfo 패턴**: 생성 직후 부모가 `SetInfo(...)`를 호출해 데이터를 주입한다. Awake보다 먼저 불릴 수 있어 `SetInfo` 첫 줄에서 `Init()`을 재호출하는 파일이 많다 (`UI_FieldSelectButton`, `UI_MapSelectButton`, `UI_TownSelectButton`, `UI_BattleDropItem`은 `if (!_init) Init()`).
- **생성 경로**: 리스트 아이템은 `Managers.UI.MakeSubItem<T>(parent)`(예: UI_Bestiary→UI_BestiaryBox, UI_BattleBtnGroup→UI_MonsterHPBar) 또는 `Managers.Resource.Instantiate("prefab명", parent)`(UI_BattleDropItem). 부모 컨테이너는 prefab에 박힌 샘플 자식을 먼저 제거하고 채우는 관례가 있다 (UI_Bestiary, UI_BattleBtnGroup.BindDropListUI).
- **클릭 이벤트**: `Button.onClick`이 아니라 `gameObject.BindEvent(콜백)` 확장 메서드 사용. **BindEvent는 `Button.interactable`을 무시**하므로 클릭 차단은 코드 가드로 직접 한다 (`UI_BestiaryBox._interactable`, `UI_MapSelectButton._isCurrent`, `UI_Village1BtnGroup.OnClickTravel`의 UnlockedBtnIds 체크).
- **이벤트 구독 해제**: `Managers.Game.OnLanguageChanged`, `Managers.Quest.OnNotifyChanged`, `Managers.Combo.*`, `MonsterController.OnItemDropped` 등 구독한 것은 반드시 `OnDestroy()`(override, `base.OnDestroy()` 호출)에서 해제. Tween도 `_tween?.Kill()`로 정리 (UI_BestiaryBox, UI_BattleBtnGroup).
- **미커밋 변경분 (현재 작업 트리)**: `UI_FieldSelectButton` / `UI_MapSelectButton` / `UI_TownSelectButton`에 `RefreshQuestVFX(HashSet<string> activeIds)` 메서드가 추가된 상태(TravelNodeData의 ClickBtnId/EnterBtnId 완전일치 매칭). `UI_Village1BtnGroup`은 그룹별 if문 나열이던 `RefreshNotify`가 `s_notifyBindings` 정적 매핑 테이블 + `NotifyBinding` struct 방식으로 리팩터링된 상태다. 이 4개 파일을 수정할 때는 커밋본과 다름을 인지할 것.
- 미완성 파일 주의: `UI_FieldTypeButton`은 `SetInfo()`가 빈 몸통이고 핵심 로직이 TODO. `UI_TownSelectButton.Init`에도 TODO 다수.

### Common Patterns
- **enum 4종 바인딩**: `enum Images/Texts/Buttons/GameObjects` → `Bind*(typeof(...))` → `GetImage/GetText/GetButton/GetObject((int)Enum.멤버)` 접근.
- **로컬라이즈/폰트**: `Util.Localize(키)` + `ApplyFont(tmp, Define.FontType.UI|Battle)`. 언어 전환은 `Managers.Game.OnLanguageChanged += RefreshTexts` 구독으로 대응. 동적 텍스트(숫자 등)는 폰트만 적용하고 값은 SetInfo/Update에서 채움.
- **색 유틸**: `Util.HexToColor("FF00CB")`, `Util.GetGradeColor(EquipGrade)`(아이템 등급 Glow), 미해금 Glow는 `0F0F0F` 컨벤션.
- **DOTween 이원화**: (1) prefab에 저작된 `DOTweenAnimation` 컴포넌트를 코드에서 `DORestart()`/`DOPlayBackwards()`로 트리거 (UI_BattleDropItem, UI_MonsterHPBar, UI_SkillBookSkill, UI_Buff의 Flash), (2) 코드 생성 tween — 클릭 `DOPunchScale`(UI_InvSlot, UI_BestiaryBox), `DOVirtual.DelayedCall`, `DOTween.Sequence()`의 AppendCallback/AppendInterval 타임라인(UI_Village1BtnGroup 그룹 전환). 팝업 위 연출은 `.SetUpdate(true)`로 timeScale 무시.
- **퀘스트 notify VFX**: BtnID(string) 완전일치 매칭. Travel 계열 버튼은 `RefreshQuestVFX(activeIds)`, Village1은 `s_notifyBindings` 테이블. 새 notify 대상 버튼 추가 시 테이블에 항목만 추가하면 됨.
- **데이터 접근**: `Managers.Data.ItemDic/CreatureDic/SkillDic.TryGetValue(dataId, ...)` — DataId 기반 딕셔너리 조회. sprite는 `Managers.Resource.Load<Sprite>(이름)` 후 null 가드(실패 시 prefab 기본 스프라이트 유지).

## Dependencies

### Internal
- `Managers` 정적 진입점: `Managers.UI`(MakeSubItem, ShowPopupUI), `Managers.Resource`(Load/Instantiate/Destroy), `Managers.Data`(ItemDic/CreatureDic/SkillDic/DialogueScriptDic), `Managers.Game`(OnLanguageChanged, UnlockedBtnIds, LastVillage1Group, ActivationManager, CompanionMinMonId, Player), `Managers.Quest`/`Managers.SubQuest`(notify·해금·대화 ID), `Managers.Combo`(UI_BattleBtnGroup 전용), `Managers.Ability`(UI_SkillBookSkill 전용), `Managers.Object`(몬스터 스폰)
- `../UI_Base.cs`(Bind/Get 헬퍼, ApplyFont, `_init`), `../Scene/UI_AreaBtnGroup.cs`(Setup, PlayEnterAnimation, UnlockButtonAnim, ButtonMoveAnim/PlaceNameAnim), `../Scene/UI_GameScene.cs`(FadeInOut, EnterAreaWithFade)
- Data 클래스: `TravelNodeData`, `CreatureData`, `ItemData`, `SkillData`, `InventoryItem`, `QuestNotifyData` (`Data` 네임스페이스)
- 컨트롤러: `MonsterController`(OnItemDropped/OnHpChanged/OnEngaged), `PlayerController`(BuffInfo, OnHitLanded, StartBattle/StopBattle)
- 팝업(열기 대상): `UI_TravelPopup`, `UI_ItemPopup`, `UI_DialoguePopup`, `UI_CustomOrderPopup`, `UI_UnlockButtonPopup`, `UI_PotionDetailPopup`; 정적 조회: `UI_BestiaryPopup.GetRecordRate/GetStarGrade`
- 유틸: `Util`(Localize, HexToColor, GetGradeColor, SetTMPUnderlay, IntToBool), `Define`(FontType, LocalizeKey, State, AreaType, EquipGrade, CreatureRarity, Village1Group, BtnID 상수), `BackgroundScroller`

### External
- UnityEngine / UnityEngine.UI (uGUI: Image, Button, Slider, Outline, ParticleSystem, Rigidbody2D)
- TMPro (TMP_Text)
- DG.Tweening (DOTween Pro — `DOTweenAnimation` 컴포넌트, Sequence, DOVirtual)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- `UI_Bestiary`는 지역 헤더와 전체 마물 수에 맞는 Grid 높이를 항상 유지한다. 화면 근처에서만 `SetBoxesVisible(true)`로 칸을 만들고, 벗어나면 Tween 정리·비활성화 후 Resource.Destroy한다. 높이는 저작된 cell/spacing/padding 및 실제 폭에서 계산하며, 재진입한 칸은 현재 기록도·별·동행과 언어 폰트를 다시 적용한다.

- `UI_BestiaryBox.prefab`의 저작 DOTweenAnimation 4개는 `autoGenerate=false`로 유지한다. 클릭·별·등급 연출은 `UI_BestiaryBox` 코드가 재생한다. 미재생 저작 Tween을 자동 생성하면 도감 503칸에서 대기 Tween 2012개가 추가된다.

- `UI_Village1BtnGroup`의 Sequence/DelayedCall은 그룹을 target/link로 소유한다. `OnDestroy`에서 `base.OnDestroy()`를 호출해 자식 Tween도 종료한다. 지역 교체 뒤 이전 버튼 등장·알림·지연 해금 콜백이 실행되면 안 된다.

- `UI_SkillBar`: 전투 기록 아래에 수동 능력 6칸과 선택 능력의 이름/상태를 표시한다. 전투 밖에서는 CanvasGroup 표시·입력을 함께 끄고, 팝업이 열리면 능력 입력을 차단한다. 스프라이트 없는 Filled Image는 채움 비율을 무시하므로 재사용 덮개는 실제 RectTransform 높이로 제어한다. 숫자는 덮개의 자식으로 넣지 않는다. 배치 상수·프레임·번역 키는 Define, 검증은 `_tools/verify_skillbar.cs`와 ADR 0021 참고.
