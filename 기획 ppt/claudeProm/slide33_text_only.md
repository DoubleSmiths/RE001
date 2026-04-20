# [슬라이드 33] 연금공방 (Alchemy)

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 씬 구성
- `Background_Alchemy` 배경
- `Alchemist_idle_Sheet` + Animator `Town_NPC` / Clip `Alchemist`
- `Main_lev_build` (`Main_lev_build_53`)
- `JungleTileset` (`JungleTilset_8`)
- 대화 Sprite: `alchemist_face_1`
- 제작의뢰 버튼 아이콘: `Potion_Icon2`

### 텍스트
- `CommissionText` — "제작의뢰" 라벨

## 작업 지시
1. **조사**
   - `Village1_Alchemy_BG.prefab` 구성.
   - `UI_Village1BtnGroup.OnClickAlchemy` / `OnClickVillage1AlchemyExitButton` 흐름.
   - Animator + Alchemist Clip.
   - Potion_Icon2 CustomOrder 버튼 적용.

2. **체크리스트**
   - [ ] Alchemy BG prefab 구성?
   - [ ] OnClickAlchemy/Exit 정상?
   - [ ] Alchemist Animator Idle?
   - [ ] CommissionText Localize?
   - [ ] Potion_Icon2 적용?
   - [ ] alchemist_face_1 연결?

3. **누락분만 구현**
   - Animator Controller 신설.
   - NpcId → face Sprite 매핑.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/Map/Village1_Alchemy_BG.prefab`
- `Assets/@Resources/Animator/Town_NPC.controller`
- `Assets/@Resources/Sprite/` 관련
