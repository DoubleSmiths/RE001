# [슬라이드 29] 마을 MID UI #5 — TYPE 2 Button Animation

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### TYPE 2 버튼 Anim (모든 버튼 공통)
**DOTweenAnimation (OnActive)**
- Move / Duration 0.3s / Ease OutBack / Loops 1

### 순차 등장 RULE
- 1번 → 0.07s Wait → 2번 → ... (TYPE 1보다 빠름)
- Exit 클릭 시 DOPlayBackwards.

### 버튼 좌표
| # | TO (X, Y) | FROM (X, Y) |
|---|---|---|
| 1 | -53, 113 | -100, 113 |
| 2 | 0, 186 | 75, 186 |
| 3 | 51, 186 | 51, 186 (정지) |
| 4 | 0, 133 | 75, 133 |
| 5 | 51, 133 | 120, 133 |
| 6 | 0, 59 | 0, 0 |

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` TYPE 2 순차 활성 로직.
   - 각 버튼 Move DOTweenAnimation.
   - Exit 역재생.

2. **체크리스트**
   - [ ] 6개 TYPE 2 버튼 Move DOTweenAnimation (0.3 OutBack)?
   - [ ] FROM/TO 좌표 일치? (3번 정지)
   - [ ] 순차 0.07s?
   - [ ] Exit DOPlayBackwards?
   - [ ] prefab DOTweenAnimation 기반?

3. **누락분만 구현**
   - 스펙 좌표 교정.
   - prefab DOTweenAnimation 이관.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
