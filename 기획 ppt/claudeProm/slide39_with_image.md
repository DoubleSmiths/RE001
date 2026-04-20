# [슬라이드 39] 물약 상세정보 Popup #3 — 다음단계 정보 미리보기

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 39
- 첨부 이미지 (`기획 ppt/slide_images/slide39/`, 총 4개):
  - [다음단계 정보] 버튼 클릭 전후 비교, BackBtnText, 3단계→다음 등급 1단계 전환 예시

먼저 `기획 ppt/slide_images/slide39/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### [다음단계 정보] 버튼 동작
- Button Anim (DOTweenAnimation Scale): 0.07s OutQuad Loops 2 Yoyo TO 0.9
- 변하는 부분이 **하이라이트** 되며 **지속 점멸** (알파 1↔0.45 또는 유사):
  1. 물약 명 Text
  2. 단계 Text
  3. 단계 아이콘
  4. 능력치 (`▶` 표기 추가)
- **필요 재료 표기는 변하지 않음**.

### 능력치 색
- 능력치 향상: `#00FF00`
- 능력치 하락: `#FF0000`

### 버튼 라벨 전환
- [다음단계 정보] → [뒤로] 로 변환.
- [뒤로] 클릭 시 원상태 복귀.

### 3단계에서 클릭 시 (다음 등급 1단계 미리보기)
1. 물약 아이콘 (다음 등급)
2. Glow 색 (다음 등급색)
3. 물약 명 Text
4. 단계 `1단계`로 표기하되 **`#00FF00` 색**으로 표기 (향상 표시)
5. 능력치

### 등급 변경 Scale Anim (DOTweenAnimation Scale)
- Duration 0.3s / Ease OutBack / From 3 (3배에서 축소되며 등장)

### Text
- `BackBtnText` — Outline `#F5A200` alpha 146

## 현재 구현 힌트 (memory)
- `UI_PotionDetailPopup.NextLvlBtn` / `BackBtn` 구현.
- `FmtStat(label, cur, next, suffix, lowerIsBetter)` — ▶ 표기 + 색상 분기.
- `StartBlink` — 이름/단계/능력치 알파 1↔0.45 InOutSine 무한 Yoyo.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup.NextLvlBtn` 핸들러 + `BackBtn` 핸들러.
   - `FmtStat` / `StartBlink` 로직.
   - 3단계 → 다음 등급 1단계 미리보기 로직.
   - prefab Button DOTweenAnimation Scale / 미리보기 Scale from 3.

2. **체크리스트**
   - [ ] 미리보기 4요소 점멸 (이름/단계/단계아이콘/능력치)?
   - [ ] 능력치 ▶ 표기 + 색 분기 (#00FF00 / #FF0000)?
   - [ ] 필요 재료는 변하지 않음?
   - [ ] [다음단계] ↔ [뒤로] 버튼 라벨 전환?
   - [ ] 3단계에서 다음 등급 1단계 전환 미리보기?
   - [ ] 단계 `1단계` 색 #00FF00 (등급 향상 표시)?
   - [ ] 등급 변경 Scale Anim (Duration 0.3 OutBack From 3)?
   - [ ] BackBtnText Outline #F5A200 alpha 146?

3. **누락분만 구현**
   - 점멸은 `DOFade` Yoyo 또는 DOTweenAnimation Fade Yoyo.
   - 등급 변경 Scale은 prefab DOTweenAnimation (AutoPlay Off, 트리거는 코드 DOPlay).
   - BackBtnText TMP Underlay.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (NextLvlBtn, BackBtn, FmtStat, StartBlink)
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
