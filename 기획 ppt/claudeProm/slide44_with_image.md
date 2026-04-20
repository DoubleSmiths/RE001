# [슬라이드 44] 여정 Popup #2 — 3 레이어 구조

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 44
- 첨부 이미지 (`기획 ppt/slide_images/slide44/`, 총 3개):
  - 모든 Layer 활성 예시, 기본(대분류만 활성) 예시, 중/소분류 Layer Size 표시

먼저 `기획 ppt/slide_images/slide44/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 여정 Popup 3 Layer 구조
1. **대분류 Layer** — [마을] / [탐험] 등 최상위 분류 버튼.
2. **중분류 Layer** — 선택된 대분류 아래의 해금된 마을/지역들.
3. **소분류 Layer** — 중분류 선택 시 나타나는 세부 장소들 ([마을] 경우만).

### 초기 화면
- Popup 켜졌을 때 **대분류 Layer만 활성**.
- 중/소분류 Layer는 비활성.

### 각 Layer Size
- 기획서에 이미지로만 명시 (실제 RectTransform Size는 이미지 참조).
- 중분류 Layer Size / 소분류 Layer Size 각각 이미지로 제공됨.

## 작업 지시
1. **조사**
   - `UI_TravelPopup.cs` 또는 `UI_MapSelectPopup.cs` 에서 3 레이어 오브젝트 바인딩.
   - 각 Layer SetActive 제어 로직.
   - Mask 처리 (슬라이드 45 연계).

2. **체크리스트**
   - [ ] 대분류 / 중분류 / 소분류 3 Layer 오브젝트 존재?
   - [ ] Popup 초기 상태: 대분류만 활성?
   - [ ] 대분류 [마을] 클릭 → 중분류 활성?
   - [ ] 중분류 [세부장소] 클릭 → 소분류 활성 ([마을] 경로만)?
   - [ ] 대분류 [탐험] 클릭 → 중분류만 활성 (소분류 없음)?

3. **누락분만 구현**
   - 3 Layer prefab 구조 구성.
   - `ExpandLayer(layerIndex)` / `CollapseLayers()` 헬퍼.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
