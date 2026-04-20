# [슬라이드 29] 마을 MID UI #5 — TYPE 2 Button Animation

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 29
- 첨부 이미지 (`기획 ppt/slide_images/slide29/`, 총 4개):
  - TYPE 2 배치 예시, 기준점, 번호 라벨 표시

먼저 `기획 ppt/slide_images/slide29/` 내 4개 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### TYPE 2 Button Animation (모든 버튼 공통)

**DOTweenAnimation (OnActive)**
- Type: `Move`
- Duration: `0.3s` / Delay: `0` / Ease: `OutBack` / Loops: `1`

### 순차 등장 RULE
- 1번 → **0.07s Wait** → 2번 → 0.07s Wait → ... (TYPE 1보다 훨씬 빠른 간격)
- 나가기 클릭 시 `DOPlayBackwards`

### 버튼 위치 & FROM (최종 위치 / 시작 위치)
| 번호 | 최종 X, Y | FROM X, Y |
|---|---|---|
| 1 | -53, 113 | -100, 113 |
| 2 | 0, 186 | 75, 186 |
| 3 | 51, 186 | 51, 186 (움직이지 않음) |
| 4 | 0, 133 | 75, 133 |
| 5 | 51, 133 | 120, 133 |
| 6 | 0, 59 | 0, 0 |

### 레이아웃 (기획서 배치)
```
  1 ┐
   │    2  3
   └─  4  5
         6
```
즉 좌상단 1, 중상단 2-3, 중단 4-5, 하단 6 구조.

### 현재 구현 힌트 (memory session_resume.md)
- UI_Village1BtnGroup L152 `Village1GuildExitButton` 핸들러가 구조화되어 있고, BG swap 타이밍 (t=0.4), 버튼 순차 등장 (Talk (-53,113) CustomOrder (0,186) Nothing1 (51,186) Nothing2 (0,133) Nothing3 (51,133) Exit (0,59)) 이 이미 구현됨.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` 의 OnClickGuild / OnClickAlchemy / OnClickMonastery 함수의 TYPE 2 버튼 순차 활성 로직.
   - 각 TYPE 2 버튼의 Move DOTweenAnimation 설정 (Duration 0.3 OutBack, FROM/TO 값).
   - Exit 핸들러의 DOPlayBackwards 역순.

2. **체크리스트**
   - [ ] 6개 TYPE 2 버튼에 Move DOTweenAnimation (0.3 OutBack Loops 1)?
   - [ ] 각 버튼의 FROM/TO 좌표가 표와 일치? (특히 3번은 정지)
   - [ ] 순차 등장 0.07s 간격?
   - [ ] Exit DOPlayBackwards 역순?
   - [ ] prefab DOTweenAnimation 기반?

3. **누락분만 구현**
   - FROM/TO 좌표가 코드에 하드코딩되어 있고 스펙과 다르면 값 교정.
   - 특히 3번 버튼은 **FROM=TO (움직이지 않음)** 확인.
   - prefab DOTweenAnimation 이관.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs` (L152 Village1GuildExitButton, L435/471 button3 관련)
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
