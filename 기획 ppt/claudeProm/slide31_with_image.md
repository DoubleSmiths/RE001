# [슬라이드 31] 대화창 #1 — 구성 리소스 + 페이드

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 31
- 첨부 이미지 (`기획 ppt/slide_images/slide31/`, 총 6개):
  - `Gradient01`, `dia_box2`, `Arrow_down`, `alchemist_face_1`, `guild_npc_idle_profile` 캐릭터 Sprite 등

먼저 `기획 ppt/slide_images/slide31/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 리소스 & 색상
- `Gradient01` — None (색 `#000000`) 기반 그래디언트
- `dia_box2` — 색 `#111111`
- `Arrow_down` — 색 `#A1A1A1`, **지속적 페이드 아웃/인 (alpha 255 ↔ 100)**
- 캐릭터 Sprite: `alchemist_face_1`, `guild_npc_idle_profile` 등 대화 상대에 따라 변동
- Animation clip 지정 없으면 **항시 Idle clip 재생**

### DOTween 페이드 (각 요소별)
- **Gradient + Sprite(None)**:
  - Fade / Duration 0.5s / Ease OutQuad
- **dia_box2**:
  - Fade / Duration 0.5s / Ease OutQuad
- **캐릭터 Sprite 활성 후 0.2s 페이드 인**:
  - Fade / Duration 0.2s / Ease OutQuad

### Arrow_down 루프
- Alpha 255 ↔ 100 무한 Yoyo (DOTweenAnimation 또는 코드 Yoyo)

## 현재 구현 힌트 (memory)
- `UI_DialoguePopup` prefab에 `GradientImage`, `dia_box2` 등 구성 존재.
- `DialogueAnimator.PlayOpenSequence` 에서 STEP 1~3 구성됨 (Gradient+Panel 0.5s, dia_box2 0.5s, 0.2s 대기 후 portrait/name).
- `PlayArrowLoop` — alpha 1 ↔ 100/255 0.2s OutQuad Yoyo 무한.

## 작업 지시
1. **조사**
   - `UI_DialoguePopup.cs` Images enum의 GradientImage / PanelImage / dia_box2 / Arrow_down 위치.
   - `DialogueAnimator.PlayOpenSequence` / `PlayArrowLoop` 로직.
   - prefab의 각 요소 초기 알파 0 설정.

2. **체크리스트**
   - [ ] GradientImage + Panel Fade 0.5s OutQuad?
   - [ ] dia_box2 Fade 0.5s OutQuad?
   - [ ] Character Sprite Fade 0.2s OutQuad?
   - [ ] Arrow_down 알파 255↔100 Yoyo 무한?
   - [ ] 캐릭터 Animator Idle Clip 기본 재생?
   - [ ] 색상값 (Gradient #000000, dia_box2 #111111, Arrow_down #A1A1A1)?
   - [ ] 대화 상대별 Sprite 교체 API?

3. **누락분만 구현**
   - Fade는 DOTweenAnimation 또는 DialogueAnimator 내부 DOFade (코드인 경우 유지 — 현재 이미 잘 구현됨).
   - Arrow_down Yoyo는 DOTweenAnimation (Loops -1 Yoyo)으로도 구성 가능.
   - 캐릭터 Sprite 교체 헬퍼 `SetPortrait(Sprite s, string animatorClip = null)`.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_DialoguePopup.cs`
- `Assets/@Scripts/UI/DialogueAnimator.cs` (PlayOpenSequence, PlayArrowLoop)
- `Assets/@Resources/Prefab/UI/Popup/UI_DialoguePopup.prefab`
