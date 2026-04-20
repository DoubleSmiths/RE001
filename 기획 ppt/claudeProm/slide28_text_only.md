# [슬라이드 28] 마을 MID UI #4 — TYPE 1 Button Animation

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### TYPE 1 버튼 Anim (모든 버튼 공통)
**DOTweenAnimation (OnEnable: Restart)**
- Move / Duration 0.3s / Ease OutBack / Loops 1
- FROM: (x=0, y=0, z=0)

### 순차 등장 RULE
- 1번 Active → 0.7s Wait → 2번 Active → 0.7s Wait → ... 반복.
- 나가기 클릭 시 `DOPlayBackwards`.

### 버튼 최종 좌표 (FROM (0,0,0) → 이동)
| # | X | Y |
|---|---|---|
| 1 | -51.5 | 45.5 |
| 2 | 0.5 | 45.5 |
| 3 | 52.5 | 45.5 |
| 4 | -51.5 | -21.5 |
| 5 | 0.5 | -21.5 |
| 6 | 52.5 | -21.5 |

2행 3열.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` TYPE 1 버튼 바인딩.
   - 순차 등장 코루틴/Delay 구성.
   - prefab 각 버튼 Move DOTweenAnimation.
   - Exit 역재생 여부.

2. **체크리스트**
   - [ ] 6개 버튼에 Move DOTweenAnimation (0.3s OutBack FROM(0,0,0))?
   - [ ] 순차 0.7s 간격?
   - [ ] 최종 위치 6좌표 일치?
   - [ ] Exit DOPlayBackwards?
   - [ ] prefab DOTweenAnimation 구현?

3. **누락분만 구현**
   - 각 버튼 Move DOTweenAnimation (AutoPlay Off, OnEnable Restart).
   - 순차 코루틴으로 `SetActive(true)`.
   - Exit 역순 DOPlayBackwards.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
