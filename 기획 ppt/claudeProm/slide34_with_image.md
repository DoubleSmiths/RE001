# [슬라이드 34] 제작의뢰(CustomOrder) Popup #1 — 기본 열기 Anim

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 34
- 첨부 이미지 (`기획 ppt/slide_images/slide34/`, 총 6개):
  - Popup 전체 화면, `Frame_square_768`, `Decor_description_line`, `BAR_Basic`, `alchemist_face_2` (Mask 처리), CommissionText + WhichOneText 예시

먼저 `기획 ppt/slide_images/slide34/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 리소스
- `Frame_square_768` — 메인 프레임
- `BAR_Basic`, `Decor_description_line` — 구분 장식
- `alchemist_face_2` — 캐릭터 (Mask 처리)

### Popup Open Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: `(x=0, y=-120, z=0)`
- OnActive → Restart, OnDisable → Rewind

### `alchemist_face_2` Anim (추가 마스크 슬라이드)
- Type: Move / Duration: 0.4s / Ease: OutBack
- From: `(x=0, y=-1, z=0)` (주의: y=-1 이므로 미세 슬라이드)
  - **실제 기존 구현**: memory 에 따라 "-150 x offset" 으로 해석되어 있음. 기획서 원문은 `From x=0, y=-1`이지만 실제 prefab 시연상 가로 슬라이드가 확인되므로 **구현 시 기획자와 재확인 필요**. 현 코드 기준 유지.
- OnDisable → PlayBackwards

### Text
- `CommissionText` — "제작의뢰" 제목
- `WhichOneText` — "무엇을 만들어드릴까요?"
- Anim: Dialogue Text Anim처럼 타이핑 효과.

## 현재 구현 힌트 (memory)
- `UI_CustomOrderPopup.cs` 작성됨.
- SetInfo에서 Frame OutBack 0.5s, Alch_Img_mask OutBack 0.4s (-150 x offset), BlackBG fade 0.3s OutQuad, TalkText 0.5s 지연 타이핑.
- prefab 루트 MonoBehaviour fileID 7310000000000000001 추가 + Addressables 등록 완료.

## 작업 지시
1. **조사**
   - `UI_CustomOrderPopup.cs` 전체 읽기 (SetInfo, OnClickClose).
   - prefab DOTweenAnimation 구성 확인 (Move, Mask slide, BlackBG Fade).
   - CommissionText/WhichOneText 타이핑 연동 확인.

2. **체크리스트**
   - [ ] Popup Move Anim (from y=-120 OutBack 0.5s) prefab DOTweenAnimation?
   - [ ] alchemist_face_2 Move 0.4s OutBack (Mask 처리 + 슬라이드 인)?
   - [ ] OnDisable Rewind / PlayBackwards?
   - [ ] BlackBG_Layer 공통 처리 (슬라이드 8)?
   - [ ] CommissionText + WhichOneText 타이핑 (DialogueAnimator.PlayTyping)?
   - [ ] prefab DOTweenAnimation 기반? (현재 코드 DOAnchorPos/DOFade 사용 중이면 이관 고려)

3. **누락분만 구현**
   - 코드 DOAnchorPos → prefab DOTweenAnimation Move 이관 (가능한 경우).
   - 타이핑 Anim은 DialogueAnimator 재사용.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CustomOrderPopup.prefab`
- `Assets/@Scripts/UI/DialogueAnimator.cs` (PlayTyping)
