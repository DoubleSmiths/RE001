# [슬라이드 26] 마을 MID UI #2 — TYPE 1 / TYPE 2 구분

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 26
- 첨부 이미지 (`기획 ppt/slide_images/slide26/`, 총 2개):
  - TYPE 1 (건물 밖) / TYPE 2 (건물 안) 각 레이아웃 예시

먼저 `기획 ppt/slide_images/slide26/` 두 이미지를 Read로 비교해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 마을 MID UI 종류 2가지
- **TYPE 1** — 건물 **밖**에 있을 시 (마을 로비 기본).
  - 예: Guild / Alchemy / Monastery / Travel / Exit / Bag 등 건물 진입 버튼들.
- **TYPE 2** — 건물 **안**에 있을 시 (길드/연금공방 등 내부).
  - 예: Talk / CustomOrder / Nothing(빈 슬롯) / Exit 등 해당 건물 내 액션 버튼들.

### 전환 조건
- 건물 진입(Guild/Alchemy 등) 버튼 클릭 시 TYPE 1 → TYPE 2 로 전환.
- 나가기(Exit) 버튼 클릭 시 TYPE 2 → TYPE 1 로 복귀.

### 애니메이션 세부
- TYPE 1 버튼 애니: 슬라이드 28 참조.
- TYPE 2 버튼 애니: 슬라이드 29 참조.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` 에서 `HideAllGroup`, `Village1MainBtnGroup` / `Village1GuildBtnGroup` / `Village1AlchemyBtnGroup` / `Village1MonasteryBtnGroup` 구조 확인.
   - `LastVillage1Group` 상태 (마지막 장소 기억) 로직 확인 (memory session_resume.md에 언급된 기존 구현).
   - TYPE 1/2 전환 시 BG prefab 교체 로직 (SwitchBG 헬퍼) 확인.

2. **체크리스트**
   - [ ] TYPE 1 버튼 그룹 (Main 로비) 존재?
   - [ ] TYPE 2 버튼 그룹 (Guild/Alchemy/Monastery 각각) 존재?
   - [ ] 건물 진입 시 TYPE 1 Hide → TYPE 2 Show 전환?
   - [ ] Exit 시 TYPE 2 Hide → TYPE 1 Show 복귀?
   - [ ] LastVillage1Group 저장 (다른 Popup 열고 닫을 때 현재 그룹 유지)?
   - [ ] BG prefab 교체 (SwitchBG) 전환 타이밍 적절?

3. **누락분만 구현**
   - 빠진 TYPE 2 그룹 있으면 추가.
   - 전환 로직에 FadeInOut/PlaceNameAnim 연계 (슬라이드 27).

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
