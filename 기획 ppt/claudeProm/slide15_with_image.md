# [슬라이드 15] 상태정보(Status) Popup #5 — 증가율 / 증폭률 UI 확장

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 15
- 첨부 이미지:
  - `기획 ppt/slide_images/slide15/slide15_shape004_그림_1.png` — 증가율/증폭률 포함한 능력치 박스 예시 (확장 후 상태)
  - `기획 ppt/slide_images/slide15/slide15_shape012_그림_13.gif` — UI 확장 애니메이션 GIF (박스 클릭 시 너비/높이 늘어나며 텍스트 등장)

먼저 Read로 위 2개 파일을 열어 확장 전/후 레이아웃과 애니메이션 흐름을 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 증가율(+n%) / 증폭률(×n) 적용 대상 (4종만)
1. **물리공격력**
2. **마법공격력**
3. **최대체력**
4. **방어력**

### 공식
```
기본 * (증가율 + 1) * 증폭률 = 최종 능력치
```
- **확장 전 표기는 "최종 능력치"** (계산된 결과만 보임).
- 박스 **클릭 시 UI 확장** → 증가율과 증폭률 각각 확인 가능.

### 능력점수 (근력/체력/민첩/마력) 상승 효과
- 능력점수로 오르는 수치는 **[기본]** 수치가 오르는 것 (증가율/증폭률이 아님).

### UI 확장 Anim (박스 클릭 시)
- **UIWidthHeight (또는 RectTransform sizeDelta 트윈)**
  - Duration: `0.3s`
  - Ease: `Linear`
  - TO: `(x=60, y=46)` (확장된 사이즈, 단위 픽셀)
- **다시 클릭 시**: `DOPlayBackwards` 로 축소 (Rewind 또는 역재생)

### 텍스트 등장 Anim (확장 시)
- **Fade**
  - Duration: `0.4s`
  - Delay:
    - 기본 text: `0.1s`
    - 증가율 text: `0.2s`
    - 증폭률 text: `0.3s`
  - Ease: `OutQuad`
  - From: `0` (알파 0에서 시작 → 1)
- **다시 클릭 시**: 텍스트 오브젝트 `SetActive(false)` (페이드 아웃 없이 즉시 비활성)

## 작업 지시
1. **조사**
   - `CreatureController` / `StatData`에 `IncreaseRate` / `Amplify` (증가율/증폭률) 필드 존재 여부.
   - 공식 `기본 * (1+증가율) * 증폭률` 코드 경로 확인.
   - `UI_CharacterPopup`에서 4종 박스 클릭 핸들러 / 확장 상태 필드 유무.
   - prefab에서 박스 오브젝트에 **UIWidthHeight** (`RectTransform sizeDelta`) DOTweenAnimation 유무.
   - 증가율/증폭률 TMP_Text 자식 오브젝트 + Fade DOTweenAnimation 유무.

2. **체크리스트**
   - [ ] 4종 스탯에 증가율/증폭률 필드?
   - [ ] 최종 능력치 = 기본×(1+증가율)×증폭률 공식?
   - [ ] 4종 박스 클릭 핸들러?
   - [ ] 확장 애님: sizeDelta Linear 0.3s TO(60, 46)?
   - [ ] 축소: DOPlayBackwards?
   - [ ] 기본/증가율/증폭률 TMP 3종에 Fade 0.4s OutQuad, delay (0.1/0.2/0.3)?
   - [ ] 축소 시 TMP SetActive(false)?
   - [ ] 능력점수 상승은 "기본" 수치만 오르는가?

3. **누락분만 구현**
   - 4종 필드(`물리공격력`/`마법공격력`/`최대체력`/`방어력`)에 `IncreaseRate`/`Amplify` 속성 추가.
   - `CalcFinalStat()` 유틸 신설.
   - 박스 클릭 핸들러 → prefab DOTweenAnimation `DOPlayForward`/`DOPlayBackwards`.
   - TMP Fade 3종을 DOTweenAnimation으로 (delay 0.1/0.2/0.3).
   - 능력점수 할당 시 증가율/증폭률 대신 **기본값**만 증가.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab 요약.

## 관련 파일 힌트
- `Assets/@Scripts/Controllers/CreatureController.cs`
- `Assets/@Scripts/Controllers/PlayerController.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
