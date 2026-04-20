# [슬라이드 45] 여정 Popup #3 — Layer Animation

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 1. 초기 화면
- 대분류 Layer만 활성, 중·소분류 비활성.

### 2. Mask
- 중·소분류 Mask 처리.

### 3. 중·소분류 활성 Anim (DOTweenAnimation)
- Move / 0.3s / OutQuad
- FROM (0, -150, 0)

### [뒤로]
- 소 → 중분류 순 DOPlayBackwards.

### 스크롤
- 중분류 ScrollBar + Viewport (해금된 마을만).
- 소분류 ScrollBar + Viewport (해금된 세부장소만).
- [탐험] 경로는 소분류 없음.

### 4. 모든 버튼 Anim (DOTweenAnimation Scale)
- 0.07s / OutQuad / Loops 2 / Yoyo / TO 0.95.

## 작업 지시
1. **조사**
   - Layer 활성 로직.
   - Move DOTweenAnimation.
   - [뒤로] 핸들러.
   - ScrollRect/Mask.
   - 버튼 Scale DOTweenAnimation.

2. **체크리스트**
   - [ ] 초기 대분류만?
   - [ ] 중·소분류 Mask?
   - [ ] Move from y=-150 OutQuad 0.3s?
   - [ ] [뒤로] 소→중 DOPlayBackwards?
   - [ ] ScrollRect 구조?
   - [ ] 해금된 장소만?
   - [ ] 버튼 Scale (0.07s Loops 2 Yoyo To 0.95)?

3. **누락분만 구현**
   - Layer Move DOTweenAnimation.
   - `_activeLayerStack.Pop()` + DOPlayBackwards.
   - 버튼 Scale DOTweenAnimation 상속.
   - UnlockedPlaces 필터링.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
