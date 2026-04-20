# [슬라이드 32] 대화창 #2 — Animation STEP 1~4

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 32
- 첨부 이미지 (`기획 ppt/slide_images/slide32/`, 총 1개):
  - 대화창 열기 전체 GIF (STEP 1~4 통합)

먼저 `기획 ppt/slide_images/slide32/slide32_shape004_그림_4.gif` 를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### Animation 단계
- **STEP 1**: Gradient01 + Sprite(None) 페이드 인 (0.5s OutQuad 공통).
- **STEP 2**: **0.5초 뒤** dia_box2 페이드 인.
- **STEP 3**: **0.2초 뒤** 캐릭터 Sprite ON + 캐릭터 이름 표시 + **Text Show Start**.
- **STEP 4**: Text Show 완료 → `Arrow_down` 활성화.

### 특이사항
1. **Text Show 중 화면 클릭**:
   - 현재 진행 중인 Text Show Anim 중단 + 모든 Text 즉시 표시.
2. **다음 Dialogue Text 존재 시 클릭**:
   - 대화창 꺼지지 않고 **다음 Text 표시**.
3. **다음 Dialogue Text 없는 상태 클릭**:
   - 대화창 꺼짐.

## 현재 구현 힌트 (memory)
- `DialogueAnimator.PlayOpenSequence(gradient, panel, box, portrait, name, onTextShowStart)` — STEP 1~3 구현.
- `ShowLine` 의 `onTypingComplete` 콜백에서 `PlayArrowLoop` 시작 — STEP 4 대응.

## 작업 지시
1. **조사**
   - `UI_DialoguePopup.cs` 클릭 감지 (`SkipTyping()` / `NextLine()` 등) 로직.
   - `DialogueAnimator.ShowLine` 의 타이핑 중단 / 즉시 표시 처리.
   - Arrow_down 활성화가 타이핑 완료 후 트리거되는지.
   - 마지막 줄인지 체크 후 닫기 vs 다음 줄 전환.

2. **체크리스트**
   - [ ] STEP 1 Gradient+Panel Fade 0.5s OutQuad?
   - [ ] STEP 2 dia_box2 Fade (STEP 1 후 0.5s 대기)?
   - [ ] STEP 3 0.2s 뒤 캐릭터+이름+Text Show?
   - [ ] STEP 4 Text Show 완료 시 Arrow_down Yoyo 시작?
   - [ ] 클릭 중 타이핑 중: 즉시 전체 표시?
   - [ ] 클릭 시 다음 Text 있음: 다음 줄로?
   - [ ] 클릭 시 다음 Text 없음: 대화창 닫기?

3. **누락분만 구현**
   - `OnClickDialogueArea()` 헬퍼: 현재 타이핑 상태 → SkipTyping, 완료 + 다음 있음 → NextLine, 없음 → Close.
   - Arrow_down 활성화 타이밍 보정.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_DialoguePopup.cs`
- `Assets/@Scripts/UI/DialogueAnimator.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_DialoguePopup.prefab`
