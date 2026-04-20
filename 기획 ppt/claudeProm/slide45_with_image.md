# [슬라이드 45] 여정 Popup #3 — Layer Animation

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 45
- 첨부 이미지 (`기획 ppt/slide_images/slide45/`, 총 7개):
  - 초기 화면, [마을] 클릭 시 중분류/소분류 예시, [탐험] 클릭 시 중분류만, ScrollBar/Viewport, 세부장소 버튼들

먼저 `기획 ppt/slide_images/slide45/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 1. 초기 화면 (Popup 켜졌을 때)
- 대분류 Layer만 활성, 중·소분류 비활성.

### 2. Mask 처리
- 중·소분류 Layer용 Mask 처리.

### 3. 중·소분류 Layer 활성 Anim (DOTweenAnimation)
- Type: Move / Duration: 0.3s / Ease: OutQuad
- FROM: `(x=0, y=-150, z=0)` (아래에서 올라옴)

### [뒤로] 버튼
- 소 → 중분류 순으로 클릭 한번 당 하나의 Layer를 `DOPlayBackwards`.

### 스크롤 구조
- 중분류 Layer는 ScrollBar + Viewport (해금된 마을만 보임).
- 소분류 Layer도 ScrollBar + Viewport (해금된 세부 장소만 보임).
- 단 [탐험] 대분류에서는 소분류 Layer 없음.

### 4. 모든 Button Animation (DOTweenAnimation Scale)
- Duration: 0.07s / Ease: OutQuad
- Loops: 2 / Loop Type: Yoyo / TO: 0.95

## 작업 지시
1. **조사**
   - `UI_TravelPopup` 3 Layer 활성 로직.
   - 각 Layer의 Move DOTweenAnimation 유무 (from y=-150 OutQuad 0.3s).
   - [뒤로] 버튼 핸들러 → 활성 Layer 감소.
   - ScrollRect / Viewport / Mask 설정.
   - 버튼별 Scale Click DOTweenAnimation (0.07s Loops 2 Yoyo To 0.95).

2. **체크리스트**
   - [ ] 초기 상태 대분류만?
   - [ ] 중·소분류 Mask 처리?
   - [ ] 중·소분류 활성 Move DOTweenAnimation (from y=-150 OutQuad 0.3s)?
   - [ ] [뒤로] 클릭 시 소 → 중 순서 DOPlayBackwards?
   - [ ] ScrollRect + Viewport 구조?
   - [ ] 해금된 장소만 표시?
   - [ ] 모든 버튼 Scale Click DOTweenAnimation (0.07s Loops 2 Yoyo To 0.95)?

3. **누락분만 구현**
   - Layer Move DOTweenAnimation prefab 구성 (AutoPlay Off, OnEnable Restart).
   - 뒤로 핸들러: `_activeLayerStack.Pop()` 후 해당 Layer DOPlayBackwards → SetActive(false).
   - 버튼들에 Scale DOTweenAnimation 일괄 추가 (공용 prefab 상속).
   - `UnlockedPlaces` 리스트로 필터링.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (UnlockedPlaces)
