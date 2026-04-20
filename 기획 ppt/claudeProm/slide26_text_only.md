# [슬라이드 26] 마을 MID UI #2 — TYPE 1 / TYPE 2 구분

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 마을 MID UI 종류
- **TYPE 1**: 건물 **밖**에 있을 시 (로비). 건물 진입 버튼들.
- **TYPE 2**: 건물 **안**에 있을 시. 해당 건물 내 액션 버튼들 (Talk/CustomOrder/Exit 등).

### 전환 조건
- 건물 진입 버튼 클릭 → TYPE 1 → TYPE 2.
- Exit 클릭 → TYPE 2 → TYPE 1.

### 세부 애니메이션
- TYPE 1: 슬라이드 28 참조.
- TYPE 2: 슬라이드 29 참조.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` 그룹 구조.
   - `LastVillage1Group` 상태 유무.
   - `SwitchBG` 헬퍼 유무.

2. **체크리스트**
   - [ ] TYPE 1/2 그룹 존재?
   - [ ] TYPE 1→2 전환?
   - [ ] TYPE 2→1 복귀?
   - [ ] LastVillage1Group 저장?
   - [ ] BG prefab 교체 타이밍?

3. **누락분만 구현**
   - 빠진 TYPE 2 그룹 추가.
   - Fade/PlaceName 연계 (슬라이드 27).

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
