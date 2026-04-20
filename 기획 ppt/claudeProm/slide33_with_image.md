# [슬라이드 33] 연금공방 (Alchemy)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 33
- 첨부 이미지 (`기획 ppt/slide_images/slide33/`, 총 7개):
  - 연금공방 씬 전체, Background_Alchemy, Alchemist_idle_Sheet, Main_lev_build, JungleTileset, alchemist_face_1, Potion_Icon2 (제작의뢰 버튼 아이콘)

먼저 `기획 ppt/slide_images/slide33/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 씬 구성
- `Background_Alchemy` — 연금공방 배경
- `Alchemist_idle_Sheet` + Animator: `Town_NPC`, Clip: `Alchemist`
- `Main_lev_build` (`Main_lev_build_53`)
- `JungleTileset` (`JungleTilset_8`)
- 대화 Sprite: `alchemist_face_1`
- 제작의뢰 버튼 아이콘: `Potion_Icon2`

### 텍스트
- `CommissionText` — "제작의뢰" 라벨 (연금공방 TYPE 2 버튼 텍스트로 추정)

## 현재 구현 힌트 (memory)
- `Village1_Alchemy_BG.prefab` 이 Addressable로 등록됨 (Village1_Alchemy_Alchemist NPC, Village1_Alchemy_BG_Img, _Ground 포함).
- `UI_Village1BtnGroup.OnClickAlchemy` / `OnClickVillage1AlchemyExitButton` 전환 로직 재구성 완료 (슬라이드 29 TYPE 2 연계).

## 작업 지시
1. **조사**
   - `Village1_Alchemy_BG.prefab` 내 구성 확인 (Alchemist NPC, BG_Img, Ground).
   - `UI_Village1BtnGroup.OnClickAlchemy` — 슬라이드 29 TYPE 2 버튼 전환 + BG switch 확인.
   - Animator `Town_NPC` + `Alchemist` Clip 존재 확인.
   - CustomOrder 버튼 아이콘 `Potion_Icon2` 적용 여부.

2. **체크리스트**
   - [ ] Alchemy BG prefab 구성 (배경, NPC, Tileset)?
   - [ ] OnClickAlchemy 전환 흐름 정상?
   - [ ] OnClickVillage1AlchemyExitButton 복귀 정상?
   - [ ] Alchemist Animator Idle Clip 재생?
   - [ ] CommissionText 바인딩 + Localize?
   - [ ] Potion_Icon2 CustomOrder 버튼에 적용?
   - [ ] alchemist_face_1 대화 Sprite 연결 (DialoguePopup에서 호출)?

3. **누락분만 구현**
   - Animator Controller 없으면 신설.
   - 대화 Sprite 매핑 테이블 (NpcId → face Sprite).

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/Map/Village1_Alchemy_BG.prefab`
- `Assets/@Resources/Animator/Town_NPC.controller`
- `Assets/@Resources/Sprite/` (Background_Alchemy, Alchemist_idle_Sheet, alchemist_face_1, Potion_Icon2)
