# UI_TravelPopup.cs 설명서

여정(Travel) 팝업 — 마을/탐험 카테고리에서 목적지를 골라 씬을 전환하는 3단계(Root → Category → Detail) UI.
경로: `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
관련 prefab: `UI_TravelPopupON.prefab` (또는 `UI_MapSelectPopup.prefab` 계열)
관련 데이터: `TravelAreaData` / `TravelAreaConditionData` / `TravelAreaNotifyData` (Data.Contents.cs)

---

## 1. 한눈에 보는 화면 구조

```
┌──────────────────────────────────┐
│ TitleBar               [X]        │  ← 닫기
├──────────────────────────────────┤
│  [지도 이미지]                    │  ← 현재 위치(또는 선택한 목적지) 미리보기
│   PlaceName                       │
├──────────────────────────────────┤
│ [TOWN] [EXPLORE]                  │  ← Root 버튼 (대분류)
│ ┌── Town Layer ─────┐             │
│ │ [아스달] [다음마을]│             │  ← TOWN 중분류 = 행선지 자체
│ └────────────────────┘             │
│ ┌── Field Layer ────┐             │
│ │ [숲] [사막] […]   │             │  ← EXPLORE 중분류 = 그룹 (소분류로 진입)
│ └────────────────────┘             │
│ ┌── BWF Layer ─────┐               │
│ │ [던전1] [던전2]   │              │  ← EXPLORE 소분류 = 실제 진입지
│ └────────────────────┘             │
├──────────────────────────────────┤
│ Description                       │  ← 잠금 사유 / 설명 / 현재 위치 라벨
├──────────────────────────────────┤
│ [< 뒤로]            [이동(GO) >]  │
└──────────────────────────────────┘
```

3단계 깊이를 enum `TravelDepth`로 관리:
- `Root` — 대분류만 보임 (TOWN/EXPLORE)
- `Category` — 중분류 Layer 전개 (TOWN은 여기서 종료, EXPLORE는 한 단계 더)
- `Detail` — EXPLORE 전용. 소분류(BWF) Layer까지 전개

---

## 2. 데이터 모델 (3단 트리)

`Managers.Data.TravelAreaDic` (key=DataId)에서 모든 노드를 받아 트리를 재구성.

| 필드 | 의미 |
|---|---|
| `Depth` | 1=Root / 2=중분류 / 3=소분류 |
| `Category` | "TOWN" 또는 "EXPLORE" — Root 노드의 부류 |
| `RootId` / `ParentId` | 트리 부모 — 정렬·하위탐색용 |
| `Order` | 같은 깊이 내 표시 순서 |
| `Name` / `Description` / `UnlockSummary` | 라벨/설명 — 로컬라이즈 키일 수도 있음 |
| `SpriteName` | 지도 이미지 스프라이트 키 |
| `SceneName` | 씬 진입용 `AreaType` 매핑 (EXPLORE는 무조건 Battle) |
| `DefaultBtnId` / `EntryBtnId` | 퀘스트 트리거(`Managers.Quest.CheckButtonClick`) ID |
| `IsSelectable` / `IsEnterable` | 0이면 기본 규칙(Depth≤2 선택, Depth==3 진입) 적용 |

**잠금 조건**은 별도 테이블 `TravelAreaConditionDic`(key=DataId → List)에 저장.
지원 ConditionType (UPPER):
- `NONE` — 항상 통과
- `QUEST_CLEAR` — `CurrentQuestID > Target(int)`
- `BTN_UNLOCKED` — `Game.UnlockedBtnIds.Contains(Target)`
- `ITEM_REQUIRED` — `GetItemCount(Target) >= Value`
- `LEVEL_REQUIRED` — `Game.Level >= Value`
- 나머지 → `false` (모름 → 잠금)

`TravelAreaNotifyDic`은 노드별 추가 알림 BtnId 매핑(서브트리 N뱃지 계산용).

---

## 3. 라이프사이클

```
Awake → Init()       ← Bind 4종 + 폰트/패널 캐시 + 이벤트 구독
SetInfo() (외부)     ← 트리 빌드 + 버튼 인스턴스화 + OpenAnimation
… 사용자 조작 …
OnDestroy            ← 이벤트 4건 해제 (Quest 3건 + Game 2건)
```

### Init() (172~230)
1. `BindImage/BindText/BindButton/BindObject` — enum 4종을 prefab 자식에 자동 바인드
2. `_frameRT`를 `MapSelectPopup`으로 지정 + `_openFromY/_closeToY = -120` (UI_Popup의 OutBack 슬라이드 인 애님)
3. `CachePanel`로 TownType/FieldType/BWFType 패널의 anchoredPosition을 "원점"으로 stash
4. 사용 안 하는 노드 영구 비활성: `TitleTextFrame`, `MapSelectBWF_EntryBtn`(이동 버튼과 중복)
5. `EquipedMat` 머티리얼 비동기 로드 (선택 테두리용)
6. Click 핸들러 일괄 바인딩 (`BindClickWithAnim` — 클릭 시 0.95 yoyo 펀치)
7. **이벤트 구독**:
   - `Managers.Quest.OnNotifyChanged` → `RefreshAll`
   - `Managers.Quest.OnQuestChanged` → `OnQuestChanged`
   - `Managers.Quest.OnBtnUnlocked` → `OnButtonUnlocked`
   - `Managers.Game.OnItemChanged` → `OnInventoryChanged`
   - `Managers.Game.OnLanguageChanged` → `OnLanguageChanged`

### SetInfo() (250~280)
팝업이 열릴 때 외부에서 호출하는 진입점.
1. `_allNodes`를 Depth/Order/DataId 순으로 정렬해 캐싱
2. `_rootNodes` = Depth==1 노드만 추출
3. `RestoreSelection` — 항상 Root depth로 시작 (스펙상 마지막 선택 복원하지 않음). `_selectedRootId`만 현재 위치 기반으로 결정
4. `BuildMiddleButtons` / `BuildDetailButtons` — 데이터 기반 버튼 인스턴스화
5. `RefreshStaticTexts` — 고정 텍스트(Title/Back/GO) 다국어 적용
6. `_prevDepth/_prevSelectedRootId/_prevSelectedGroupId` 초기화
7. `RefreshAll` → 패널/버튼/텍스트 일괄 갱신
8. `OpenAnimation()` — UI_Popup이 제공하는 슬라이드 인

---

## 4. 버튼 인스턴스화 패턴

`BuildButtonGroup` (369~416)이 핵심 헬퍼.

```
templateButton = prefab의 첫 번째 버튼
nodes.Count==0   → templateRoot SetActive(false), 끝
nodes.Count>=1   → templateRoot SetActive(true), 0번째는 그대로
nodes.Count>=2   → 1번째부터 Instantiate(templateRoot, parentRoot)
                   name = $"{templateRoot.name}_{node.DataId}"
                   localScale=1로 강제(부모가 다르면 스케일이 튀는 것 방지)
                   _clonedXxxButtons에 등록 (재빌드 시 Destroy용)
```

각 버튼은 `TravelNodeButtonView`(100~108)로 wrap:
- `Root` GameObject
- `Button` 컴포넌트
- `Text` (자식에서 이름 매칭, 없으면 첫 번째 TMP_Text)
- `Notify` GO (퀘스트 N뱃지)
- `Frame` (Image — 색상 틴트 대상)

뷰는 두 dict에 보관:
- `_middleButtonViews`: TOWN+EXPLORE 중분류 통합 (key=DataId)
- `_detailButtonViews`: EXPLORE 소분류만

그리고 `RebindClickEvent`(1629~1645)로 click을 새로 맺는다 — `UI_EventHandler`를 GetOrAddComponent해 기존 핸들러를 **null로 비우고** 새로 등록(중복 호출 방지). `Button.interactable=false`면 yoyo도 액션도 스킵.

> ⚠ **클로저 캡쳐 주의**: `int capturedId = node.DataId;`를 만든 뒤 람다에서 capturedId 사용 — for 루프 변수를 직접 캡쳐하면 모든 람다가 마지막 값을 보는 버그를 막음.

---

## 5. 상태 (필드 모음)

```
TravelDepth _depth       (현재 깊이)
int _selectedRootId      (TOWN/EXPLORE 중 어느 쪽인지)
int _selectedGroupId     (중분류 — TOWN이면 행선지, EXPLORE면 그룹)
int _selectedDetailId    (소분류 — EXPLORE 전용 행선지)

TravelDepth _prevDepth
int _prevSelectedRootId
int _prevSelectedGroupId   (패널 In/Out 애니메이션 트리거용)

bool _isEntering           (이중 진입 가드)
string _selectionFeedbackMessage  (잠금 사유를 하단 description에 표시)
```

`_selectedDetailId`는 prev를 두지 않음 — 소분류는 항상 해당 그룹 안에서만 의미를 가지므로 그룹 변화로 충분.

---

## 6. 클릭 플로우

### 대분류 클릭 → `OnClickRootCategory(category)` (954~970)
- `_selectedRootId = root.DataId`
- `_selectedGroupId = 0`, `_selectedDetailId = 0` (사용자가 명시 클릭하기 전엔 선택 없음)
- `_depth = Category`
- EXPLORE 클릭 시 퀘스트 트리거 `BTN_EXPLORE` 발화
- `RefreshAll`

### 중분류 클릭 → `OnClickGroupNode(dataId)` (972~1015)
1. **현재 위치(또는 그 조상)면 무시** — `IsCurrentPlaceOrAncestor`로 차단 (자기 자신으로의 이동 방지)
2. `IsSelectableNode` 통과해야 함
3. `TryGetSelectionBlockReason` → 잠겼으면 `_selectionFeedbackMessage`에 사유 저장하고 RefreshAll만 호출 (선택 자체를 거부)
4. **TOWN과 EXPLORE 분기**:
   - **TOWN**: 중분류 자체가 행선지. 첫 번째 자식 detail을 `_selectedDetailId`로 자동 잡고 `_depth = Category` 유지 (소분류 Layer 안 열림)
   - **EXPLORE**: `_selectedDetailId = 0`, `_depth = Detail`로 진입. 소분류 Layer가 슬라이드 인
5. `node.DefaultBtnId` 있으면 퀘스트 트리거
6. `BuildDetailButtons` (그룹 바뀌면 소분류도 재빌드)
7. `RefreshAll`

### 소분류 클릭 → `OnClickDetailNode(dataId)` (1017~1044)
- 동일하게 현재위치 가드 → 선택 가능 검사 → 잠금 검사
- 통과 시 `_selectedDetailId` 갱신만 (depth는 그대로)
- `DefaultBtnId` 트리거
- `RefreshAll`

### 이동 버튼 → `OnClickEnterSelectedDetail` (1046~1110)
1. `_isEntering` 가드 (씬 전환 중 더블탭 무력화)
2. `TryGetValidatedSelectedDetail` — `_selectedDetailId`가 유효하고 `CanEnterNode` 통과해야 함
3. `UI_GameScene` 활성 확인
4. `TryResolveAreaType` — `Category=="EXPLORE"`면 `AreaType.Battle` 강제, 그 외엔 `Enum.TryParse(SceneName)`
5. `gameScene.CanEnterAreaTransition(areaType)` 검증
6. **여기까지 통과**:
   - `_isEntering = true`
   - `EntryBtnId` 퀘스트 트리거
   - `gameScene.EnterAreaWithFade(areaType)` 호출 — false면 가드 풀고 종료
   - **성공 시**: `Managers.Game.CurrentTravelAreaId = detail.DataId` 저장
   - `BTN_JOURNEY` 퀘스트 트리거
   - `Managers.UI.CloseAllPopupUI()` — 팝업 전부 닫고 페이드로 씬 진입

각 실패 분기는 `Managers.UI.ShowToast`로 사용자 안내 + Debug.LogWarning.

### 뒤로 → `OnClickBack` (1112~1132)
- `Detail` → `Category` (소분류 선택 해제)
- `Category` → `Root` (중분류 선택 해제는 안 함, 패널만 닫음)
- `Root` → `ClosePopupUI` (팝업 닫기)

---

## 7. RefreshAll 흐름

`RefreshAll` (439~451)이 모든 시각 갱신의 단일 진입점.

```
NormalizeSelectionState   ← 데이터 변경(잠금/삭제)으로 선택이 깨졌는지 검사·복구
RefreshVisiblePanels      ← Town/Field/BWF Layer In/Out 애니메이션
RefreshRootCategoryButtons ← 대분류 버튼 색·VFX·텍스트
RefreshMiddleButtons      ← 중분류 버튼 (TOWN+EXPLORE)
RefreshDetailButtons      ← 소분류 버튼 (EXPLORE만)
RefreshSelectionPanel     ← 지도/이름/설명/이동·뒤로 버튼
_prev* = 현재 상태 stash  ← 다음 호출의 패널 애님 비교 기준
```

### 7-1. NormalizeSelectionState (1451~1503)
선택값이 더 이상 유효하지 않은지(노드가 삭제됨/그룹의 ParentId가 바뀜/잠금이 걸림) 검사하고, 깨졌으면 0/Root depth로 강제 리셋. 외부 이벤트(아이템 변화 등)로 잠금 상태가 변할 수 있어서 매 Refresh마다 호출.

### 7-2. RefreshVisiblePanels + AnimatePanel (831~902)
**핵심: 이번 Refresh 시점에 보여야 하는지 vs 직전엔 보였는지**의 4가지 조합으로 애니메이션 분기:

| now | was | 동작 |
|---|---|---|
| true | false | `+ LAYER_FROM_OFFSET(0,-150)` 위치에서 origin으로 슬라이드 인 (OutQuad 0.3s) |
| false | true | origin에서 `+ FROM_OFFSET`으로 슬라이드 아웃 (InQuad 0.3s) → SetActive(false) + pos 복원 |
| true | true | 위치 고정 (이미 열려있는데 다시 트윈 안 걸어 떨림 방지) |
| false | false | SetActive(false) |

`SetUpdate(true)` — `Time.timeScale=0` 상태(팝업 열려있을 때)에서도 진행되도록.

### 7-3. RefreshRootCategoryButtons (541~595)
- 항상 표시되는 두 버튼(TOWN/EXPLORE) 텍스트·VFX·색·테두리 갱신
- `_depth==Root`면 둘 다 `COLOR_ROOT_IDLE(흰색)` + 테두리 OFF
- `_depth>=Category`이고 선택된 쪽이면 `COLOR_GROUP_SELECT(FFC500)` + 테두리 ON, 반대쪽은 `SetActive(false)` (Layer가 열린 동안엔 다른 카테고리 버튼 숨김)
- 아이콘 색은 항상 `FFFFFF alpha 80` (상태 무관)

### 7-4. RefreshMiddleButtons (597~662)
중분류 버튼 16~30개를 한 루프로 갱신. 색상 결정 우선순위:
1. **layer 안 열림 / 잠김** → `COLOR_DISABLED(5E5E5E α200)`, interactable=false
2. **현재 위치 또는 그 조상** → `COLOR_CURRENT(237B00 녹)`, interactable=false
3. **선택 상태**:
   - TOWN 중분류 + `_depth==Category` → `COLOR_DESTINATION(FFA300 주)` (행선지)
   - EXPLORE 중분류 + `_depth==Detail` → `COLOR_GROUP_SELECT(FFC500 노)` + 테두리 ON
4. **나머지** → `COLOR_DEFAULT(흰)`, interactable=`IsSelectableNode`

텍스트는 Bold/Normal로 선택 표시. `FitTextToButton`으로 길어지면 폰트 자동 축소.
N뱃지는 `HasNotifyInSubtree` — 본인 또는 자손에 알림 있으면 표시.

### 7-5. RefreshDetailButtons (664~720)
소분류 전용. 비슷하지만 `_selectedDetailId`만 보고 선택 판정. 색상은 Disabled / Current / Destination(선택) / Default 4종.
`Notify`는 `HasEntryNotify` — `EntryBtnId`로만 매칭(서브트리 검사 아님).

### 7-6. RefreshSelectionPanel (722~829)
지도 이미지·이름·설명·이동/뒤로 버튼 갱신.

`displayNode` 우선순위: `detail` > `currentLocation` > `group` > `root`
- **상세 선택 상태에선 선택된 목적지를 preview** (스펙상 "유동적"이지만 명시 선택 시엔 미리보기)
- 그 외엔 현재 위치를 보여줌

`MapImage`는 `Managers.Resource.Load<Sprite>` 동기 시도 → 캐시 미스면 `LoadAsync` 비동기 보충.

설명 텍스트(`MapSelectPopup_CurrentLocationTxt`):
- `_depth==Detail`: 진입 가능하면 그룹/디테일 description, 불가면 `enterFailMessage`
- `_depth==Category`:
  - 그룹이 현재 위치(또는 조상)면 "현재 위치" 라벨
  - TOWN 카테고리에서 그룹 미선택이면 "현재 위치" 라벨
  - 그 외엔 `ResolveCategoryInfo` (잠금 사유 우선, 없으면 description)
- `_depth==Root`: 루트 노드 description
- `_selectionFeedbackMessage`가 있으면 위 모든 결과를 덮어씀 (잠금 사유를 직접 보여주기 위함)

이동 버튼(GO):
- TOWN + `_depth==Category` + `_selectedGroupId != 0` + 현재위치 아님 → 후보
- EXPLORE + `_depth==Detail` + `_selectedDetailId != 0` + 현재위치 아님 → 후보
- 후보 + `canEnter` + `detail != null` → interactable=true

뒤로 버튼: `_depth!=Root`이면 활성+흰색, Root이면 비활성+회색.

---

## 8. 색상 팔레트

| 상수 | 값 | 의미 |
|---|---|---|
| `COLOR_DEFAULT` | `FFFFFF` | 평상시 |
| `COLOR_DISABLED` | `5E5E5E α200` | 잠금 / Layer 미오픈 |
| `COLOR_DESTINATION` | `FFA300` | 선택된 행선지 (TOWN 중분류, EXPLORE 소분류) |
| `COLOR_CURRENT` | `237B00` | 현재 플레이어 위치 (또는 조상) |
| `COLOR_GROUP_SELECT` | `FFC500` | EXPLORE 중분류가 그룹으로 선택돼 소분류 Layer를 연 상태 |
| `COLOR_ROOT_IDLE` | `FFFFFF` | 대분류 버튼 본체 |
| `COLOR_ROOT_ICON` | `FFFFFF α80` | 대분류 아이콘 (상태 무관) |

선택 테두리는 `EquipedMat` 머티리얼 + `UISprite`(Unity 기본 SlicedSprite)로 런타임에 `BorderOverlay_EquipedMat` 자식을 만들어 토글.

---

## 9. 외부 의존

| 의존 | 용도 |
|---|---|
| `Managers.Data.TravelAreaDic` | 노드 본체 |
| `Managers.Data.TravelAreaConditionDic` | 잠금 조건 |
| `Managers.Data.TravelAreaNotifyDic` | 추가 알림 BtnId |
| `Managers.Game.CurrentTravelAreaId` | 현재 위치(저장) |
| `Managers.Game.UnlockedBtnIds` | BTN_UNLOCKED 조건 |
| `Managers.Game.GetItemCount` | ITEM_REQUIRED 조건 |
| `Managers.Game.Level` | LEVEL_REQUIRED 조건 |
| `Managers.Quest.CurrentQuestID` | QUEST_CLEAR 조건 |
| `Managers.Quest.GetCurrentNotifyList()` | N뱃지 |
| `Managers.Quest.CheckButtonClick(btnId)` | 진입/그룹/디테일 클릭 시 퀘스트 트리거 |
| `Managers.UI.SceneUI as UI_GameScene` | 씬 전환 위임 |
| `gameScene.CanEnterAreaTransition` / `EnterAreaWithFade` | 실제 씬 전환 |
| `Managers.UI.ShowToast` | 실패 사유 토스트 |
| `Managers.UI.CloseAllPopupUI` | 진입 성공 시 모든 팝업 닫기 |
| `Managers.Resource.Load[Async]<T>` | 지도 스프라이트 / EquipedMat |

이벤트 5건을 OnDestroy에서 반드시 해제 (CLAUDE.md 규약).

---

## 10. 자주 헷갈리는 포인트

1. **TOWN과 EXPLORE는 깊이가 다르다**
   - TOWN: 1(Root) → 2(중분류=행선지) — 소분류 없음
   - EXPLORE: 1(Root) → 2(중분류=그룹) → 3(소분류=행선지)
   - 그래서 `OnClickGroupNode`에서 TOWN이면 첫 자식 detail을 자동 선택해 `_selectedDetailId`를 채우고 `_depth=Category`로 멈춘다.

2. **현재 위치 자기 자신은 클릭 불가**
   - `IsCurrentPlaceOrAncestor` — ParentId 체인을 타고 올라가며 매칭. 던전 안에 있을 때 그 던전의 부모 그룹/루트도 모두 "현재"로 친다.
   - 마을 안에 있을 때 마을 버튼이 녹색으로 잠긴 모습이 이 로직.

3. **잠금 vs 선택 차단 vs 진입 차단**
   - `IsNodeUnlocked`: 조건 평가만 (조건이 아예 없으면 true)
   - `TryGetSelectionBlockReason`: 자기 자신 + (Depth==3이면) 부모 그룹까지 한꺼번에 잠금 검사
   - `CanEnterNode`: 위 + selectable/enterable + AreaType 매핑까지

4. **`_selectionFeedbackMessage`의 동작**
   - 잠긴 버튼을 클릭했을 때 RefreshAll만 호출하고 description을 사유로 덮어씀
   - 다음 정상 액션(다른 버튼 클릭, 외부 이벤트 등)이 들어오면 빈 문자열로 리셋

5. **패널 애니메이션은 prev 비교 기반**
   - "지금 보여야 하는가"만으로는 In/Out을 결정 못 함 → 직전 상태와 비교해 transition만 트리거
   - `_prev*` 필드를 RefreshAll 끝에서 갱신하는 이유

6. **버튼 yoyo는 Time.timeScale=0에서도 작동해야 함**
   - 팝업 열리면 GameManager가 `timeScale=0` 만들 수 있어 `SetUpdate(true)` 필수 (DOTween의 unscaled time)

7. **소분류 재빌드 타이밍**
   - 그룹이 바뀔 때만(`OnClickGroupNode`) 호출. RefreshAll 자체는 재빌드 안 함
   - 그래서 그룹 바뀔 때마다 `BuildDetailButtons` 명시 호출

8. **TitleTextFrame / MapSelectBWF_EntryBtn은 영구 비활성**
   - prefab엔 있지만 기획에서 제거된 노드. Init에서 SetActive(false)로 무력화

---

## 11. 확장 시 체크리스트

- [ ] **새 카테고리 추가** (예: "DUNGEON"): RootNode 데이터 추가 + `BuildMiddleButtons`에 호출 1줄 추가 + `RefreshRootCategoryButtons`에 매핑 + `OnClickRootCategory` 케이스 + `TryResolveAreaType` 분기.
- [ ] **새 잠금 조건 타입**: `EvaluateCondition`의 switch에 case 추가만 하면 됨. `default: return false`이므로 안전.
- [ ] **버튼 색 변경**: `COLOR_*` 상수 한 곳에서 수정. 새 색이 추가되면 `RefreshMiddleButtons`/`RefreshDetailButtons`의 우선순위 if-else도 같이 손봐야 함.
- [ ] **소분류 Layer를 TOWN에도 적용**: `OnClickGroupNode`의 TOWN 분기에서 `_depth = Detail` 줄 + `RefreshVisiblePanels`의 `bwfVisible` 조건 변경.
- [ ] **현재 위치 매핑 변경**: `ResolveCurrentLocationNode`(기본값=TOWN root의 첫 Depth==2). 시작 마을이 바뀌면 여기.
- [ ] **N뱃지 로직 변경**: `HasNotifyInSubtree` / `HasEntryNotify` / `HasNotifyForNode` 3종.

---

## 12. 알려진 제약

- **선택 상태가 세션을 넘기지 않음** — `RestoreSelection`이 항상 Root depth로 시작 (의도된 스펙).
- **AreaType이 enum이라 데이터 SceneName과의 매핑이 정적** — 새 씬 추가 시 enum도 같이 늘어나야 함.
- **클로즈 애니메이션이 따로 없음** — `ClosePopupUI`는 `UI_Popup`의 기본 동작(close trigger)에 의존.
- **`_clonedDetailButtons`은 부모를 BWFTypeContent로만 가정** — TOWN에 소분류 Layer를 추가하려면 별도 부모 분기 필요.
