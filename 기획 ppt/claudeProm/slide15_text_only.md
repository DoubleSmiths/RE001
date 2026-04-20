# [슬라이드 15] 상태정보(Status) Popup #5 — 증가율 / 증폭률 UI 확장

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 증가율(+n%) / 증폭률(×n) 적용 대상 (4종만)
- 물리공격력
- 마법공격력
- 최대체력
- 방어력

### 공식
```
기본 * (증가율 + 1) * 증폭률 = 최종 능력치
```
- 확장 전 표기는 **최종 능력치**.
- 박스 클릭 시 UI 확장 → 증가율/증폭률 각각 확인.

### 능력점수 상승 효과
- 근력/체력/민첩/마력 등 능력점수로 오르는 수치는 **[기본]** 수치가 오름.

### UI 확장 Anim (박스 클릭)
- **UIWidthHeight / sizeDelta**
  - Duration: 0.3s / Ease: Linear
  - TO: (x=60, y=46)
- 다시 클릭: `DOPlayBackwards`

### 텍스트 등장 Anim (Fade)
- Duration: 0.4s / Ease: OutQuad / From: 0
- Delay: 기본 0.1s / 증가율 0.2s / 증폭률 0.3s
- 다시 클릭 시: TMP SetActive(false) (즉시 비활성)

## 작업 지시
1. **조사**
   - `CreatureController`/`StatData`에 IncreaseRate/Amplify 필드 유무.
   - 공식 적용 코드 경로.
   - `UI_CharacterPopup` 4종 박스 클릭 핸들러/확장 상태 필드 유무.
   - prefab UIWidthHeight DOTweenAnimation / Fade DOTweenAnimation 유무.

2. **체크리스트**
   - [ ] 4종 스탯 IncreaseRate/Amplify?
   - [ ] 공식 `기본 × (1+증가율) × 증폭률`?
   - [ ] 박스 클릭 핸들러?
   - [ ] sizeDelta Linear 0.3s TO(60, 46)?
   - [ ] 축소 시 DOPlayBackwards?
   - [ ] Fade 0.4s OutQuad delay 0.1/0.2/0.3?
   - [ ] 축소 시 TMP SetActive(false)?
   - [ ] 능력점수 상승 = 기본 수치만 증가?

3. **누락분만 구현**
   - 4종 스탯에 IncreaseRate/Amplify 속성 추가.
   - `CalcFinalStat()` 유틸.
   - 박스 클릭 → prefab DOTweenAnimation 토글.
   - TMP Fade 3종 DOTweenAnimation (delay 0.1/0.2/0.3).
   - 능력점수 할당 시 기본값만 증가.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/Controllers/CreatureController.cs`, `PlayerController.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
