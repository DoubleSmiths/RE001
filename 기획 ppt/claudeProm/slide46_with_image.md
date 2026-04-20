# [슬라이드 46] 여정 Popup #4 — 버튼 상태별 색상 / 상호작용

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 46
- 첨부 이미지 (`기획 ppt/slide_images/slide46/`, 총 13개):
  - 중·소분류 Layer 켜져 있을 때 예시, 행선지 선택 시 색, 현재 장소, 선택 시 테두리 UIMask EquipedMat 등

먼저 `기획 ppt/slide_images/slide46/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 버튼 상태별 색 / Interactive

**아무 중·소분류 Layer가 켜져 있을 때 (기본 활성)**
- Color: `#FFFFFF`
- Interactive: O

**그 외 (활성 Layer 없음)**
- Color: `#5E5E5E` alpha 200 (Button Disabled Color)
- Interactive: X

**행선지가 선택되었을 때 — [마을] 중분류 / [탐험] 소분류 버튼**
- Color: `#FFA300`
- Interactive: O
- 클릭 시 해당 장소로 이동.

**그 외 또는 [뒤로] 버튼이 눌렸을 때**
- Color: `#5E5E5E` alpha 200 (Disabled Color)
- Interactive: X

**현재 플레이어가 있는 장소**
- Color: `#237B00`
- Interactive: X (이미 그 장소에 있으므로 이동 불가)

**선택되었을 때 (Highlight)**
- Color: `#FFC500` (Disabled Color로 표기되어 있지만, Interactive: O)
- **테두리**에 Image `UIMask` + Mat `EquipedMat` 활성 (장착 스타일 하이라이트)

**그 외 (세부장소의 일반 상태)**
- Color: `#959595`
- Interactive: O

### Text 필드
- `TownText`, `ExploreText`, `PlaceNameTownAsdalText`, `CurrentLocationText`
- `PlaceNameBWF0Text`, `PlaceNameBWF1Text`, `PlaceNameBWF2Text` (BWF = 신들바람숲 계열?)

## 작업 지시
1. **조사**
   - `UI_TravelPopup` 각 버튼 상태 enum 및 색/Interactive 분기 로직.
   - `UIMask` + `EquipedMat` 적용 구조 (선택 시 테두리).
   - 현재 위치 `Managers.Game.CurrentPlaceId` 비교.
   - 6종 장소 텍스트 바인딩.

2. **체크리스트**
   - [ ] 기본 활성 #FFFFFF + Interactive O?
   - [ ] 활성 Layer 없을 때 #5E5E5E alpha 200 + Interactive X?
   - [ ] 행선지 선택 시 중분류(마을)/소분류(탐험) #FFA300 + Interactive O?
   - [ ] [뒤로] 클릭 시 비활성?
   - [ ] 현재 장소 #237B00 + Interactive X?
   - [ ] 선택(Highlight) #FFC500 + UIMask EquipedMat?
   - [ ] 세부장소 일반 #959595?
   - [ ] 6종 장소 Text 바인딩 + Localize?

3. **누락분만 구현**
   - 버튼 상태 enum (`TravelButtonState`): Default / Destination / Current / Selected / Disabled.
   - `ApplyState(state)` 헬퍼 → 색 + Interactive + UIMask 토글.
   - 현재 위치 비교 → Current 상태 강제 적용.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (CurrentPlaceId)
- `Assets/@Resources/Material/EquipedMat.mat` (존재 여부 확인)
