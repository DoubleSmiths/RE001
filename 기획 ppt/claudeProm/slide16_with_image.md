# [슬라이드 16] 상태정보(Status) Popup #6 — 능력점수 올리기 / 되돌리기

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 16
- 첨부 이미지:
  - `기획 ppt/slide_images/slide16/slide16_shape004_그림_8.gif` — 능력점수 할당 (<, > 버튼 + 색 변경)
  - `기획 ppt/slide_images/slide16/slide16_shape005_그림_12.gif` — V 버튼 확정 (Quest_Done_Effect 파티클)
  - `기획 ppt/slide_images/slide16/slide16_shape006_그림_18.gif` — 능력점수 부족 시 Shake 피드백 (빨강 + 흔들림)
  - `기획 ppt/slide_images/slide16/slide16_shape007_그림_20.gif` — 되돌리기 버튼

먼저 Read로 4개 gif를 모두 열어 각 상호작용 흐름을 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 케이스 1: 능력점수 있음 + `<` / `>` 버튼 클릭
- 할당된 능력(근력/체력/민첩/마력)과 능력 상세정보 창의 **변동된 부분 Text 색**을 `#00FF00` 로 변경.
- **능력점수 복귀 시** (되돌리기/V 확정) 본래 색으로 복귀.

### 케이스 2: 능력점수 할당되어 있음 + `V` 버튼 클릭 (확정)
- 할당된 능력과 능력 상세정보 창의 변동된 부분을 **적용 확정**하고 본래 색으로 복귀.
- **Particle: `Quest_Done_Effect`** 재생 (제안 — "넣으면 좋을 듯?" 기획서 메모).

### 케이스 3: 능력점수 없음 + `>` 버튼 클릭 (할당 불가)
- 해당 Text 색을 `#B70000` 로 **0.3초간** 변경.
- **DOTween Shake/Position**:
  - Duration: `0.3s`
  - TO: `x=0.8`
  - Vibrato: `50`
  - Randomness: `90`

### 케이스 4: 능력점수 할당되어 있음 + `되돌리기` 버튼 클릭
- 해당 능력에 할당되어 있던 **모든 능력점수를 원래대로 복귀**.

## 작업 지시
1. **조사**
   - `UI_CharacterPopup.cs` 에 `<`/`>` 할당 버튼, `V` 확정 버튼, `되돌리기` 버튼 바인딩 유무.
   - 임시 할당(pending)과 확정(applied) 분리 구조 유무 — 예: `_pendingStatPoints[0..3]` vs `Managers.Game.StrStat` 등.
   - Text 색 `#00FF00` / `#B70000` 전환 로직.
   - Shake 애니메이션: prefab DOTweenAnimation 또는 코드 `DOShakePosition`.
   - `Quest_Done_Effect` 파티클 prefab 존재 여부.

2. **체크리스트**
   - [ ] `<` 버튼 핸들러: 할당 차감 + 해당 스탯/상세정보 Text 색 `#00FF00`?
   - [ ] `>` 버튼 핸들러: 능력점수>0이면 할당 + Text 색 `#00FF00`?
   - [ ] `V` 버튼 핸들러: pending → applied 확정 + Quest_Done_Effect 파티클 + 본래 색 복귀?
   - [ ] 능력점수=0 + `>` 클릭: Text `#B70000` 0.3s + Shake/Position (Duration 0.3, TO 0.8, Vib 50, Rand 90)?
   - [ ] `되돌리기` 버튼: pending 전량 롤백?
   - [ ] pending 상태에서 V 눌러야만 GameData/Save 반영?
   - [ ] pending 상태에서 Popup 닫으면 롤백되는가? (기획서 불명확 — 방어적으로 롤백 권장)
   - [ ] Shake 및 색 복귀 애니메이션이 prefab DOTweenAnimation 구성인가?

3. **누락분만 구현**
   - pending/applied 분리: `_pending[]` 배열 + `Apply()` / `Revert()` 메서드.
   - Text 색 애니메이션은 DOTweenAnimation Color (OnEnable/OnDisable Link 또는 코드 DOPlayById)로.
   - Shake는 prefab에 DOTweenAnimation Shake Position (Duration 0.3, Vibrato 50, Randomness 90) 추가, 코드에선 DOPlay.
   - Quest_Done_Effect 파티클 Instantiate 위치/부모 결정 후 호출.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (StatPoint 저장)
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Prefab/FX/Quest_Done_Effect.prefab` (존재 여부 확인)
