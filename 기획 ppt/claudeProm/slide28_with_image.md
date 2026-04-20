# [슬라이드 28] 마을 MID UI #4 — TYPE 1 Button Animation

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 28
- 첨부 이미지 (`기획 ppt/slide_images/slide28/`, 총 3개):
  - TYPE 1 버튼 배치 예시 (6개 버튼), 기준점 표시, 초기 위치 상세

먼저 `기획 ppt/slide_images/slide28/` 내 3개 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### TYPE 1 Button Animation (모든 버튼 동일)

**DOTweenAnimation (OnEnable: Restart)**
- Type: `Move`
- Duration: `0.3s` / Delay: `0` / Ease: `OutBack` / Loops: `1`
- FROM: `(x=0, y=0, z=0)` (기준점)

### 순차 등장 RULE
- 1번 버튼 Active → **0.7s Wait** → 2번 버튼 Active → 0.7s Wait → ... (순차 등장)
- 나가기 버튼 누를 시 `DOPlayBackwards` (역재생)

### 초기 위치 (버튼 최종 위치 — FROM (0,0,0)에서 이 좌표로 이동)
| 번호 | X | Y |
|---|---|---|
| 1 | -51.5 | 45.5 |
| 2 | 0.5 | 45.5 |
| 3 | 52.5 | 45.5 |
| 4 | -51.5 | -21.5 |
| 5 | 0.5 | -21.5 |
| 6 | 52.5 | -21.5 |

### 레이아웃
- 2행 3열 그리드 (위 3개 / 아래 3개).
- 가로 간격 ~52 pt, 세로 간격 ~67 pt.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` TYPE 1 (로비) 버튼 6개 오브젝트 바인딩.
   - 순차 등장 코루틴 `CoActivateButtons(yield 0.7f)` 또는 DOTweenAnimation Delay 계단식.
   - 각 버튼의 prefab Move DOTweenAnimation 설정 (Duration 0.3 OutBack FROM (0,0,0)).
   - 나가기 시 DOPlayBackwards 호출 여부.

2. **체크리스트**
   - [ ] 6개 버튼 각각 Move DOTweenAnimation (0.3s OutBack Loops 1 FROM (0,0,0))?
   - [ ] 순차 등장 0.7s 간격?
   - [ ] 각 버튼 최종 위치가 표의 좌표와 일치?
   - [ ] Exit 클릭 시 DOPlayBackwards 역재생?
   - [ ] DOTweenAnimation (prefab 컴포넌트) 기반 구현? (코드 DOTween 이관)

3. **누락분만 구현**
   - prefab의 각 버튼에 Move DOTweenAnimation (AutoPlay Off, OnEnable Restart, Duration 0.3, OutBack, FROM (0,0,0)) 추가.
   - 순차 등장은 별도 코루틴으로 `btn.SetActive(true)` 호출 (OnEnable 시 DOTween 자동 재생).
   - Exit 핸들러에서 역순으로 `DOPlayBackwards()` 호출.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
