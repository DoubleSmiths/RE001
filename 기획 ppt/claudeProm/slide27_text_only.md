# [슬라이드 27] 마을 MID UI #3 — Anim 흐름 (Fade In/Out + 장소 Text)

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### Anim 흐름

**Case A — 게임 시작**
- STEP 1: 페이드 인 + 장소 Text Anim
- STEP 2: 버튼 ON

**Case B — 장소 이동**
- STEP 1: 페이드 아웃 + 장소 Text Anim + 버튼 OFF
- STEP 2: 버튼 ON

### 장소 Text Anim (DOTweenAnimation, OnEnable Restart)
- **1st Move**: Duration 0.25s / OutQuad / Loops 1 / FROM (-150, 12, 0)
- Wait 0.6s
- **2nd Move**: Duration 0.15s / OutQuad / Loops 1 / TO (150, 12, 0)

### 장소 Text Fade (교체 순간)
- 0.4s Fade Out (InQuad) → 0.15s 대기 → 0.5s Fade In (InQuad)

### 상단 화면 페이드
- Ease: InQuad

## 작업 지시
1. **조사**
   - `UI_Base.FadeInOut` InQuad 사용.
   - `UI_GameScene.EnterAreaWithFade` 흐름.
   - `UI_Village1BtnGroup.PlaceNameAnim` 구성.
   - prefab 장소 Text Move DOTweenAnimation 2개 유무.

2. **체크리스트**
   - [ ] Case A 흐름?
   - [ ] Case B 흐름?
   - [ ] 1st Move 0.25 OutQuad FROM(-150, 12)?
   - [ ] Wait 0.6s?
   - [ ] 2nd Move 0.15 OutQuad TO(150, 12)?
   - [ ] Fade In/Out 0.4/0.15/0.5 InQuad?
   - [ ] 상단 페이드 레이어?
   - [ ] prefab DOTweenAnimation 구성?

3. **누락분만 구현**
   - 코드 Sequence → prefab DOTweenAnimation 이관.
   - Fade Ease InQuad 통일.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/UI_Base.cs`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
