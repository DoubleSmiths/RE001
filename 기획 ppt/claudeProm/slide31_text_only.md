# [슬라이드 31] 대화창 #1 — 구성 리소스 + 페이드

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 리소스 & 색상
- `Gradient01` 기반 None `#000000`
- `dia_box2` `#111111`
- `Arrow_down` `#A1A1A1`, 알파 255↔100 지속 페이드
- 캐릭터 Sprite: `alchemist_face_1`, `guild_npc_idle_profile` 등 대화 상대별 변경. Idle clip 기본.

### DOTween 페이드
- Gradient + Sprite(None): Fade 0.5s OutQuad
- dia_box2: Fade 0.5s OutQuad
- Character Sprite: Fade 0.2s OutQuad

### Arrow_down 루프
- 알파 1 ↔ 100/255 Yoyo 무한.

## 작업 지시
1. **조사**
   - `UI_DialoguePopup.cs` Images enum.
   - `DialogueAnimator.PlayOpenSequence` / `PlayArrowLoop`.
   - prefab 초기 알파 0 설정.

2. **체크리스트**
   - [ ] Gradient + Panel Fade 0.5s?
   - [ ] dia_box2 Fade 0.5s?
   - [ ] Character Fade 0.2s?
   - [ ] Arrow_down Yoyo 무한?
   - [ ] Animator Idle 기본?
   - [ ] 색상값 일치?
   - [ ] Sprite 교체 API?

3. **누락분만 구현**
   - DOFade 코드 또는 prefab DOTweenAnimation 우선.
   - Arrow_down Yoyo DOTweenAnimation.
   - 캐릭터 교체 헬퍼.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_DialoguePopup.cs`
- `Assets/@Scripts/UI/DialogueAnimator.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_DialoguePopup.prefab`
