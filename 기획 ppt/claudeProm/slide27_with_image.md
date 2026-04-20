# [슬라이드 27] 마을 MID UI #3 — Anim 흐름 (Fade In/Out + 장소 Text)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 27
- 첨부 이미지 (`기획 ppt/slide_images/slide27/`, 총 3개):
  - Anim 흐름 예시, 장소 Text Anim, 상단 화면 페이드 효과

먼저 `기획 ppt/slide_images/slide27/` 내 모든 파일 + 기존 `Documents/기획안/slide27_images/` 폴더도 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### Anim 흐름 (두 케이스)

**Case A — 게임 시작 시**
- STEP 1
  - a. 페이드 인 Anim
  - b. 장소 Text Anim
- STEP 2
  - c. 버튼 ON Anim

**Case B — 마을 내 장소 이동 시**
- STEP 1
  - a. 페이드 아웃 Anim
  - b. 장소 Text Anim
  - c. 버튼 OFF Anim
- STEP 2
  - c. 버튼 ON Anim

### 장소 Text Anim (DOTweenAnimation)
`[OnEnable: Restart]`
- **1st anim (Fade In)**
  - Type: `Move`
  - Duration: `0.25s` / Delay: `0` / Ease: `OutQuad` / Loops: `1`
  - FROM: `(x=-150, y=12, z=0)`
- **Wait 0.6 sec**
- **2nd anim (Fade Out Slide)**
  - Type: `Move`
  - Duration: `0.15s` / Delay: `0` / Ease: `OutQuad` / Loops: `1`
  - TO: `(x=150, y=12, z=0)`

### 장소 Text Fade 보조 (`0.4초 Fade Out → 0.15s 대기 → 0.5s Fade In`)
- Ease: `InQuad`
- 텍스트가 바뀌는 순간 부드럽게 교체하는 트윈.

### 상단 화면 페이드 인/아웃
- 씬의 상단 게임 뷰 (배경/캐릭터) 영역을 어두운 레이어로 덮는 페이드.
- Ease: `InQuad` (장소 Text Fade와 동일 계열).

## 현재 구현 힌트 (memory 기록)
- `UI_Base.FadeInOut(Image, Action)` — InQuad 적용됨.
- `UI_GameScene.EnterAreaWithFade` — 0.15s 중간 대기 포함.
- `UI_AreaBtnGroup.PlaceNameAnim` — OutQuad 1st 0.25s FROM(-150, 12), 2nd 0.15s TO(150, 12).

## 작업 지시
1. **조사**
   - `UI_Base.FadeInOut` 읽고 InQuad 사용 여부 확인.
   - `UI_GameScene.EnterAreaWithFade` 로직 확인.
   - `UI_AreaBtnGroup.PlaceNameAnim` 또는 `UI_Village1BtnGroup.PlaceNameAnim` DOTweenAnimation 설정.
   - prefab의 장소 Text 노드에 Move DOTweenAnimation 2개 (FROM(-150,12) / TO(150,12)) 유무.

2. **체크리스트**
   - [ ] 게임 시작 시 Case A (Fade In + PlaceName + 버튼 ON) 흐름?
   - [ ] 장소 이동 시 Case B (Fade Out + PlaceName + 버튼 OFF → 새 장소 Fade In + 버튼 ON)?
   - [ ] 장소 Text 1st Move 0.25s OutQuad FROM(-150, 12)?
   - [ ] Wait 0.6s?
   - [ ] 2nd Move 0.15s OutQuad TO(150, 12)?
   - [ ] Fade In/Out 0.4/0.15/0.5 InQuad?
   - [ ] 상단 화면 어두운 페이드 레이어?
   - [ ] prefab DOTweenAnimation으로 구성 (코드 Sequence 이관)?

3. **누락분만 구현**
   - PlaceNameAnim 구성이 코드 Sequence이면 prefab DOTweenAnimation 2개로 분할 (Link OnEnable: Restart).
   - Fade는 InQuad Ease로 통일.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/UI_Base.cs` (FadeInOut)
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (EnterAreaWithFade L236~250)
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs` (PlaceNameAnim)
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
