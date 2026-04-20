# [슬라이드 46] 여정 Popup #4 — 버튼 상태별 색상 / 상호작용

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 버튼 상태별 색 / Interactive
| 조건 | Color | Interactive |
|---|---|---|
| 중·소분류 Layer 켜짐 (기본) | `#FFFFFF` | O |
| 활성 Layer 없음 | `#5E5E5E alpha 200` | X |
| 행선지 선택 시 [마을] 중분류 / [탐험] 소분류 | `#FFA300` | O |
| [뒤로] 클릭 or 그 외 | `#5E5E5E alpha 200` | X |
| 현재 플레이어 장소 | `#237B00` | X |
| 선택(Highlight) | `#FFC500` | O (+ UIMask EquipedMat 테두리) |
| 세부장소 일반 | `#959595` | O |

### Text 필드
- `TownText`, `ExploreText`, `PlaceNameTownAsdalText`, `CurrentLocationText`
- `PlaceNameBWF0Text`, `PlaceNameBWF1Text`, `PlaceNameBWF2Text`

## 작업 지시
1. **조사**
   - 버튼 상태 분기 로직.
   - UIMask EquipedMat.
   - CurrentPlaceId 비교.
   - 6종 Text 바인딩.

2. **체크리스트**
   - [ ] 기본 #FFFFFF?
   - [ ] 비활성 #5E5E5E alpha 200?
   - [ ] 행선지 #FFA300?
   - [ ] 뒤로/기타 Disabled?
   - [ ] 현재 장소 #237B00?
   - [ ] 선택 #FFC500 + UIMask EquipedMat?
   - [ ] 세부장소 #959595?
   - [ ] 6종 Text 바인딩?

3. **누락분만 구현**
   - `TravelButtonState` enum.
   - `ApplyState(state)` 헬퍼.
   - CurrentPlaceId 비교 강제 Current.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Material/EquipedMat.mat`
