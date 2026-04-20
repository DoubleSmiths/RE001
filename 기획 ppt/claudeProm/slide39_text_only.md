# [슬라이드 39] 물약 상세정보 Popup #3 — 다음단계 정보 미리보기

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### [다음단계 정보] 버튼 동작
- Button Anim: Scale 0.07s OutQuad Loops 2 Yoyo TO 0.9.
- 변하는 부분 하이라이트 + 지속 점멸:
  1. 물약 명
  2. 단계
  3. 단계 아이콘
  4. 능력치 (`▶` 표기 추가)
- 필요 재료는 변하지 않음.

### 능력치 색
- 향상: `#00FF00`, 하락: `#FF0000`.

### 버튼 라벨 전환
- [다음단계 정보] → [뒤로].
- [뒤로] → 원상태 복귀.

### 3단계 미리보기 (다음 등급 1단계)
1. 물약 아이콘 (다음 등급)
2. Glow 다음 등급색
3. 물약 명
4. 단계 `1단계`, 색 `#00FF00`
5. 능력치

### 등급 변경 Scale Anim
- DOTweenAnimation Scale / 0.3s / OutBack / From 3.

### BackBtnText
- Outline `#F5A200` alpha 146.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup.NextLvlBtn`/`BackBtn`/`FmtStat`/`StartBlink`.
   - 3단계 → 다음 등급 전환.
   - prefab Button Scale / 미리보기 Scale from 3.

2. **체크리스트**
   - [ ] 4요소 점멸?
   - [ ] 능력치 ▶ + 색 분기?
   - [ ] 재료 변동 X?
   - [ ] 라벨 토글?
   - [ ] 3단계 → 다음 등급 미리보기?
   - [ ] 단계 `1단계` #00FF00?
   - [ ] Scale from 3 OutBack 0.3s?
   - [ ] BackBtnText Outline #F5A200 alpha 146?

3. **누락분만 구현**
   - 점멸 DOFade Yoyo.
   - 등급 Scale prefab DOTweenAnimation.
   - BackBtnText TMP Underlay.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
