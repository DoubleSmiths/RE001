# TravelPopup 구현 판정 결과

## 미구현 항목별 구현 계획

### 현재 위치 복원과 실제 구역 상태 일치

- 현재 상태
  - `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`의 `RestoreSelection`, `GetPreferredRootId`, `TryApplySavedSelection`는 `Managers.Game.CurrentTravelAreaId`를 기준으로 팝업 선택 상태를 복원한다.
  - 하지만 `Assets/@Scripts/UI/Scene/UI_GameScene.cs`는 초기화 시 `EnterArea(AreaType.Village1)`를 고정 호출한다.
- 왜 미구현인지
  - 팝업 내부 표시만 저장값을 따르고, 실제 씬의 현재 구역은 저장값을 무시한다.
  - 그래서 현재 위치 텍스트와 실제 활성 `UI_AreaBtnGroup`이 어긋날 수 있다.
- 수정 대상 파일
  - `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
  - `Assets/@Scripts/Managers/Contents/GameManager.cs`
  - `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- 추가/수정할 함수
  - `UI_GameScene.Init()`
  - `UI_GameScene.EnterArea(...)`
  - 저장값에서 시작 구역을 해석하는 `ResolveInitialAreaType(...)` 또는 동등 함수
  - 필요 시 `GameManager`에 현재 구역 해석 헬퍼
- 필요한 데이터/이벤트/저장 처리
  - 기존 `CurrentTravelAreaId`를 실제 초기 구역 복원에 사용해야 한다.
  - 저장값이 없는 경우만 `Village1` 기본값을 허용해야 한다.
- 테스트 항목
  - 저장된 `CurrentTravelAreaId=1110`일 때 마을 진입 복원
  - `2110`일 때 탐험 진입 복원
  - 잘못된 ID 저장 시 기본값 fallback
  - `TravelPopup` 표시와 실제 활성 구역 일치

### 탐험 상세 지역의 실제 진입 연결

- 현재 상태
  - `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`의 `OnClickEnterSelectedDetail()`는 `TryResolveAreaType()`로 `SceneName`을 `AreaType`으로 파싱한 뒤 `Assets/@Scripts/UI/Scene/UI_GameScene.cs`의 `EnterAreaWithFade()`를 호출한다.
  - 그런데 `Assets/@Resources/Data/JsonData/TravelAreaData.json`의 탐험 상세 `SceneName`은 `Stage_BreezingWindForest_Entrance` 등이고, `Assets/@Scripts/Utils/Define.cs`의 `AreaType`는 `Village1`, `Battle` 뿐이다.
- 왜 미구현인지
  - 탐험 상세 노드는 버튼과 선택은 되지만 실제 진입 시 `Enum.TryParse`에서 막힌다.
  - 즉 UI만 있고 진입 완료가 아니다.
- 수정 대상 파일
  - `Assets/@Scripts/Utils/Define.cs`
  - `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
  - `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
  - 필요 시 `Assets/@Scripts/Data/Data.Contents.cs`
- 추가/수정할 함수
  - `UI_TravelPopup.TryResolveAreaType(...)`
  - `UI_GameScene.CanEnterAreaTransition(...)`
  - `UI_GameScene.SwapAreaBtnGroup(...)`
- 필요한 데이터/이벤트/저장 처리
  - `TravelAreaData.SceneName`과 실제 전환 대상의 1:1 매핑 정책이 필요하다.
  - `AreaType`를 탐험 상세까지 확장하거나, 별도 매핑 테이블을 둬야 한다.
  - `UI_{AreaType}BtnGroup` 프리팹과 `UI_AreaBtnGroup` 컴포넌트도 해당 지역 수만큼 맞춰야 한다.
- 테스트 항목
  - `2110`, `2210`, `2310` 진입 성공
  - 존재하지 않는 프리팹/매핑 누락 시 오류 메시지
  - 마을 노드와 탐험 노드 모두 동일한 진입 루트 사용 확인

### 전투 종료 후 복귀 위치의 하드코딩 제거

- 현재 상태
  - `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs`의 `OnClickTravel()`은 전투 종료 후 `gameScene?.EnterAreaWithFade(AreaType.Village1);`를 고정 호출한다.
  - 주석에도 마지막 마을로 가야 함 TODO가 남아 있다.
- 왜 미구현인지
  - `TravelPopup`에서 어떤 상세 지역으로 들어왔든 복귀는 항상 `Village1`로 고정된다.
  - 현재 위치 저장과 Travel 구조를 실제 게임 흐름에 연결하지 못한다.
- 수정 대상 파일
  - `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs`
  - `Assets/@Scripts/Managers/Contents/GameManager.cs`
  - `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- 추가/수정할 함수
  - `UI_BattleBtnGroup.OnClickTravel()`
  - 마지막 비전투 거점/마을을 해석하는 `GameManager` 헬퍼
  - 또는 `UI_GameScene`의 공통 복귀 함수
- 필요한 데이터/이벤트/저장 처리
  - 현재 `CurrentTravelAreaId` 외에 마지막 안전 거점 또는 마지막 마을 상세 노드 저장값이 필요할 수 있다.
  - 최소 구현이면 탐험 상세에서 입장할 때 대응 마을/허브 복귀 목적지를 함께 저장해야 한다.
- 테스트 항목
  - 탐험 지역 진입 후 전투 종료 시 원래 거점 복귀
  - 마을 내 전환 후 전투 진입/복귀
  - 저장 후 재실행 뒤 복귀 기준 유지

### Village1 Travel 해금 표시의 실제 상태 반영

- 현재 상태
  - `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`에 `UNLOCK_TRAVEL_BUTTON` 처리와 notify 갱신은 있으나, `SetInfo()` 주변에 해금된 아이콘/텍스트 `SetActive(true)` 처리 TODO가 남아 있다.
- 왜 미구현인지
  - 데이터상 해금과 notify는 있지만, Village1 메인 버튼의 아이콘/텍스트/NEW/VFX 표시 정책이 완전히 닫히지 않았다.
  - 해금되었는데도 시각 상태가 누락될 수 있다.
- 수정 대상 파일
  - `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
  - 필요 시 `Assets/@Scripts/UI/Popup/UI_UnlockButtonPopup.cs`
- 추가/수정할 함수
  - `SetInfo()`
  - `OnBtnUnlocked(...)`
  - `TryPlayUnlockAnim(...)`
  - Travel 버튼 표시 상태를 갱신하는 전용 함수
- 필요한 데이터/이벤트/저장 처리
  - 기존 `UnlockedBtnIds` 재사용 가능
  - 추가 저장은 필요 없고, 해금 후 즉시 UI 반영만 정리하면 된다.
- 테스트 항목
  - `UNLOCK_TRAVEL_BUTTON` 획득 직후 아이콘/텍스트 노출
  - 재접속 후 해금 상태 유지
  - notify와 unlock 표시가 서로 충돌하지 않는지 확인

## 거짓 완료 판정 방지

### 현재 코드에서 이미 있는 것

- `UI_TravelPopup`의 3계층 버튼 생성
- 선택/입장 분리
- 조건 평가
- 선택 상태 저장/복원
- 진입 버튼에서 `UI_GameScene.EnterAreaWithFade()` 호출 시도

### 아직 없는 것

- 탐험 상세 `SceneName`을 실제 진입 가능한 `AreaType` 또는 프리팹 체계로 연결하는 구현
- 씬 시작 시 저장된 현재 구역 복원
- 전투 종료 복귀 목적지 일반화
- `Village1` Travel 해금 표시 정리

### 이름만 있고 실제 동작은 없는 것

- `TryResolveAreaType()`의 탐험 상세 진입
- `UI_GameScene`의 현재 구역 복원
- `UI_BattleBtnGroup`의 마지막 마을 복귀
- `UI_Village1BtnGroup`의 해금된 Travel 버튼 시각 상태 완결 처리

## 복붙용 요약

```text
TravelPopup 구현 판정 결과 요약:

1. 현재 위치 복원과 실제 구역 상태 일치: 미구현
- 팝업 선택 상태는 CurrentTravelAreaId를 복원하지만, 실제 씬은 항상 Village1로 시작함
- 수정 대상: UI_GameScene.cs, GameManager.cs, UI_TravelPopup.cs

2. 탐험 상세 지역의 실제 진입 연결: 미구현
- SceneName은 Stage_BreezingWindForest_Entrance 같은 문자열인데 AreaType은 Village1, Battle뿐이라 실제 진입 불가
- 수정 대상: Define.cs, UI_TravelPopup.cs, UI_GameScene.cs

3. 전투 종료 후 복귀 위치의 하드코딩 제거: 미구현
- 전투 종료 시 항상 Village1로 복귀함
- 수정 대상: UI_BattleBtnGroup.cs, GameManager.cs, UI_GameScene.cs

4. Village1 Travel 해금 표시의 실제 상태 반영: 부분구현
- 해금 데이터와 notify는 있으나 실제 버튼 시각 상태 반영 TODO가 남아 있음
- 수정 대상: UI_Village1BtnGroup.cs
```
