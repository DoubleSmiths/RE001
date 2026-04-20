# [슬라이드 16] 상태정보(Status) Popup #6 — 능력점수 올리기 / 되돌리기

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 케이스 1: 능력점수 있음 + `<` / `>` 클릭
- 할당된 능력과 상세정보 창의 **변동된 Text 색** → `#00FF00`.
- 능력점수 복귀 시 본래 색으로 복귀.

### 케이스 2: 능력점수 할당됨 + `V` 확정 클릭
- 변동된 부분을 **적용 확정** + 본래 색으로 복귀.
- **Particle: `Quest_Done_Effect`** 재생 (제안).

### 케이스 3: 능력점수 없음 + `>` 클릭
- Text 색 `#B70000` 으로 **0.3초**.
- **DOTween Shake/Position**: Duration 0.3 / TO x=0.8 / Vibrato 50 / Randomness 90.

### 케이스 4: 능력점수 할당됨 + `되돌리기` 클릭
- 해당 능력에 할당된 모든 점수를 원래대로 복귀.

## 작업 지시
1. **조사**
   - `UI_CharacterPopup.cs` `<`/`>`/`V`/`되돌리기` 버튼 바인딩.
   - pending vs applied 분리 구조 유무.
   - Text 색 전환 로직.
   - Shake 애니메이션 유무 (prefab DOTweenAnimation 또는 코드).
   - `Quest_Done_Effect` 파티클 prefab 존재 여부.

2. **체크리스트**
   - [ ] `<`/`>` 핸들러 + Text `#00FF00`?
   - [ ] `V` 확정 + Quest_Done_Effect + 본래 색?
   - [ ] `>` 거부 (0 점수): Text `#B70000` 0.3s + Shake?
   - [ ] `되돌리기` → pending 전량 롤백?
   - [ ] V 전에는 Save 반영 X?
   - [ ] Popup 닫을 때 pending 롤백?
   - [ ] Shake/색 애니메이션 prefab DOTweenAnimation 구성?

3. **누락분만 구현**
   - pending/applied 분리: `_pending[]` + `Apply()`/`Revert()`.
   - Text 색은 DOTweenAnimation Color (DOPlayById).
   - Shake는 prefab DOTweenAnimation (Duration 0.3, Vibrato 50, Rand 90) + 코드 DOPlay.
   - Quest_Done_Effect Instantiate 위치 결정.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Prefab/FX/Quest_Done_Effect.prefab`
