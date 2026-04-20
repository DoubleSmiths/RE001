# [슬라이드 38] 물약 상세정보 Popup #2 — 설명/필요재료 토글

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### [설명] 버튼 동작
- 클릭 시:
  - [재료부족/제작], [다음단계 정보] 버튼 사라짐.
  - 필요 재료 목록 → 부가 설명 Text 전환.
  - [설명] 라벨 → [필요재료] 로 변환.
- [필요재료] 클릭 → 원상태.

### Popup 기본 상태
- Popup 열 때 항상 "필요재료 목록" 상태.

### Button Anim (DOTweenAnimation Scale)
- Duration 0.07s / OutQuad / Loops 2 / Yoyo / TO 0.9.

### ItemDesc
- 제작된 상태 vs 제작 안 된 상태별 텍스트 분기 (`Potion0DescText` 등).

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup.SwitchPanelBtn` + `RefreshPanelMode`.
   - `_showDesc` 초기화.
   - 설명 모드 시 버튼 숨김.
   - Scale DOTweenAnimation.
   - 제작 상태별 Desc 분기.

2. **체크리스트**
   - [ ] 재료 ↔ 설명 전환?
   - [ ] [재료부족/제작], [다음단계] 숨김?
   - [ ] 라벨 토글?
   - [ ] Popup 기본 "필요재료"?
   - [ ] Scale DOTweenAnimation (0.07s Loops 2 Yoyo To 0.9)?
   - [ ] 제작 상태별 Desc 분기?

3. **누락분만 구현**
   - 토글 누락 시 추가.
   - `HasPotion(dataId)` 체크.
   - prefab Scale DOTweenAnimation.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
