# [슬라이드 32] 대화창 #2 — Animation STEP 1~4

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### Animation 단계
- **STEP 1**: Gradient01 + Sprite(None) 페이드 인 (0.5s OutQuad).
- **STEP 2**: 0.5s 뒤 dia_box2 페이드 인.
- **STEP 3**: 0.2s 뒤 캐릭터 Sprite ON + 이름 표시 + Text Show 시작.
- **STEP 4**: Text Show 완료 → Arrow_down 활성.

### 특이사항
1. Text Show 중 화면 클릭 → 즉시 전체 Text 표시.
2. 다음 Dialogue 있음 + 클릭 → 다음 Text.
3. 다음 Dialogue 없음 + 클릭 → 대화창 꺼짐.

## 작업 지시
1. **조사**
   - `UI_DialoguePopup.cs` 클릭 감지.
   - `DialogueAnimator.ShowLine` 타이핑 처리.
   - Arrow_down 활성 타이밍.
   - 마지막 줄 체크.

2. **체크리스트**
   - [ ] STEP 1 Gradient+Panel Fade 0.5s?
   - [ ] STEP 2 dia_box2 0.5s 뒤?
   - [ ] STEP 3 0.2s 뒤 캐릭터+이름+Text Show?
   - [ ] STEP 4 Arrow_down Yoyo?
   - [ ] 타이핑 중 클릭 → 전체 표시?
   - [ ] 다음 Text → 전환?
   - [ ] 마지막 Text → 닫기?

3. **누락분만 구현**
   - `OnClickDialogueArea()` 상태 분기.
   - Arrow_down 타이밍 보정.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_DialoguePopup.cs`
- `Assets/@Scripts/UI/DialogueAnimator.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_DialoguePopup.prefab`
