# [슬라이드 38] 물약 상세정보 Popup #2 — 설명/필요재료 토글

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 38
- 첨부 이미지 (`기획 ppt/slide_images/slide38/`, 총 4개):
  - [설명] 누르기 전/후 비교, ItemDesc 예시, 제작된 상태/제작 안된 상태 호출 차이

먼저 `기획 ppt/slide_images/slide38/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### [설명] 버튼 (SwitchPanelBtn) 동작
- 누르면:
  - [재료부족/제작] 버튼과 [다음단계 정보] 버튼이 **사라짐**.
  - 필요 재료 목록 부분이 **해당 물약의 부가 설명 Text**로 변함.
  - [설명] 버튼 라벨이 **[필요재료]** 버튼으로 변함.
- [필요재료] 누르면 → 원상태 복귀.

### Popup 호출 시 기본 상태
- 어느 상태에서 Popup이 꺼졌든, **항상 "필요재료 목록" 상태**로 켜짐.

### Button Anim (설명/필요재료 토글 공용, DOTweenAnimation)
- Type: Scale / Duration: 0.07s / Ease: OutQuad
- Loops: 2 / Loop Type: Yoyo / TO: 0.9

### ItemDesc
- 물약 제작 상태별로 텍스트 표시 분기:
  - **제작되어 있는 상태**에서 호출 → 특정 설명 텍스트.
  - **제작되어 있지 않은 상태**에서 호출 → `Potion0DescText` (예시 데이터 ID가 0이라고 추정)

## 현재 구현 힌트 (memory)
- `UI_PotionDetailPopup.SwitchPanelBtn` 클릭 → `_showDesc` 토글, `RefreshPanelMode` 재료↔설명 + 버튼 라벨 전환.
- 팝업 열 때 항상 `_showDesc=false` 초기화.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup.SwitchPanelBtn` 핸들러 + `RefreshPanelMode`.
   - `_showDesc` 초기화 로직 (`SetInfo` 내 false 리셋).
   - 설명 모드에서 [재료부족/제작], [다음단계] 버튼 숨김 처리.
   - SwitchPanelBtn prefab Scale DOTweenAnimation 유무.
   - 제작 상태별 ItemDesc 분기 로직.

2. **체크리스트**
   - [ ] [설명] 클릭 시 재료 → 설명 전환?
   - [ ] [재료부족/제작], [다음단계] 버튼 숨김?
   - [ ] 버튼 라벨 [설명] ↔ [필요재료] 전환?
   - [ ] Popup 열릴 때 항상 필요재료 상태?
   - [ ] Scale Click Anim (0.07s OutQuad Loops 2 Yoyo To 0.9) DOTweenAnimation?
   - [ ] 제작 상태별 ItemDesc 분기 (Potion0DescText 등)?

3. **누락분만 구현**
   - `_showDesc` 토글 존재 확인 후 누락 시 추가.
   - 제작 여부 체크 (`Managers.Game.HasPotion(dataId)`) 로직 추가.
   - prefab에 Scale DOTweenAnimation.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (SwitchPanelBtn, RefreshPanelMode)
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
