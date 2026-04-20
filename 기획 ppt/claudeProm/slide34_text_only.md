# [슬라이드 34] 제작의뢰(CustomOrder) Popup #1 — 기본 열기 Anim

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 리소스
- `Frame_square_768`, `BAR_Basic`, `Decor_description_line`, `alchemist_face_2` (Mask 처리)

### Popup Open Anim (DOTweenAnimation)
- Move / 0.5s / OutBack / Loops 1
- From (0, -120, 0)
- OnActive Restart, OnDisable Rewind

### alchemist_face_2 Anim
- Move / 0.4s / OutBack
- From (0, -1, 0) (기획서 원문 — 구현 현황은 -150 x offset. 재확인 필요)
- OnDisable PlayBackwards

### Text
- `CommissionText` ("제작의뢰")
- `WhichOneText` ("무엇을 만들어드릴까요?")
- Anim: Dialogue 타이핑처럼

## 작업 지시
1. **조사**
   - `UI_CustomOrderPopup.cs` SetInfo / OnClickClose.
   - prefab DOTweenAnimation 구성.
   - Text 타이핑 연동.

2. **체크리스트**
   - [ ] Popup Move (from y=-120 OutBack 0.5s)?
   - [ ] alchemist_face_2 Move 0.4s OutBack?
   - [ ] OnDisable Rewind/PlayBackwards?
   - [ ] BlackBG_Layer 공통 처리?
   - [ ] Commission/WhichOne 타이핑?
   - [ ] prefab DOTweenAnimation 기반?

3. **누락분만 구현**
   - 코드 DOAnchorPos → prefab DOTweenAnimation 이관.
   - 타이핑은 DialogueAnimator 재사용.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CustomOrderPopup.prefab`
- `Assets/@Scripts/UI/DialogueAnimator.cs`
