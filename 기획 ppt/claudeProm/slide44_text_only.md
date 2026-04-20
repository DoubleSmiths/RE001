# [슬라이드 44] 여정 Popup #2 — 3 레이어 구조

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 3 Layer 구조
1. 대분류 Layer — [마을] / [탐험].
2. 중분류 Layer — 선택된 대분류 하위 (마을/지역).
3. 소분류 Layer — 중분류 선택 시 세부 장소 ([마을] 경로만).

### 초기 화면
- 대분류만 활성.

### Layer Size
- 이미지에 따로 명시 (RectTransform Size는 이미지 참조).

## 작업 지시
1. **조사**
   - `UI_TravelPopup.cs` 3 레이어 바인딩.
   - SetActive 제어.
   - Mask (슬라이드 45).

2. **체크리스트**
   - [ ] 3 Layer 오브젝트?
   - [ ] 초기 대분류만?
   - [ ] [마을] → 중분류?
   - [ ] 중분류 → 소분류?
   - [ ] [탐험] → 중분류만 (소분류 없음)?

3. **누락분만 구현**
   - 3 Layer 구조 + `ExpandLayer`/`CollapseLayers` 헬퍼.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
