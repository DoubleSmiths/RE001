# [슬라이드 12] 상태정보(Status) Popup #2 — Text 설정

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 12
- 첨부 이미지:
  - `기획 ppt/slide_images/slide12/slide12_shape005_Google_Shape_284_p9.png` — 상태정보 Popup 전체 레이아웃 (상단: 상태정보 타이틀, 능력점수 10, 근력/체력/민첩/마력 박스, [능력 상세정보] 버튼, 공격/수비/보조 섹션)

먼저 Read 도구로 위 이미지를 열어 각 텍스트 라벨의 위치와 크기를 확인해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약**:
  - 기존 `DOTweenAnimation` 컴포넌트 있으면 재사용 (`DOPlay`/`DOPlayById`/`DORestart`).
  - 코드 `DOTween.Sequence()` 지양. 단일 트윈은 prefab DOTweenAnimation 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 필요 Text 필드 (Texts enum에 반드시 포함)
- `CharPopupTitleText` — "상태정보" 타이틀
- `CharPopupStatPointText` — "능력점수 n" 표시
- `CharPopupElixirText` — 엘릭서 수량 표시
- `StrengthText` — "근력"
- `MagicText` — "마력" (Magic)
- `AgilityText` — "민첩"
- `VitialityText` — "체력" (주의: 철자 `Vitiality`로 기획서 명시)
- `CharPopupAttributeDetailText` — "능력 상세정보" 버튼 라벨

### 현재 구현 힌트
- `UI_CharacterPopup.cs` Texts enum에 `PointsStatPointsTxt`, `ElixirPointsTxt`, `STRStatNameTxt` 등이 이미 존재. 기획 스펙 네이밍(`CharPopupStatPointText` 등)과 통일 여부 점검.

## 작업 지시
1. **조사**
   - `UI_CharacterPopup.cs` Texts enum 전체 나열.
   - prefab의 대응 TMP_Text 오브젝트 위치/폰트 확인.
   - `LocalizationDic`에 `CharPopupTitleText`, `CharPopupStatPointText`, `CharPopupElixirText` 키 존재 여부 확인.

2. **체크리스트**
   - [ ] Texts enum에 8종 필드가 존재하는가? (네이밍 다를 수 있음)
   - [ ] 각 필드가 대응 TMP_Text 오브젝트와 바인딩되어 있는가?
   - [ ] Localization 키(`Util.Localize(LocalizeKey.CharPopupXxxText)`) 호출로 갱신되는가?
   - [ ] 기획 라벨 문자열 ("상태정보", "능력점수", "근력", "민첩", "마력", "체력", "능력 상세정보") 한/영 둘 다 Localization에 정의?
   - [ ] "체력"의 영문 표기가 "Vitality"인지 "Vitiality"인지 스펙과 일치?

3. **누락분만 구현**
   - 빠진 Text 필드는 Texts enum + 대응 prefab TMP_Text + Localize 호출 추가.
   - "체력" 오타(Vitiality) 관련은 일단 기획서 표기 그대로 코드에 유지, Localization 표시 문자열만 "체력/Vitality" 로.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs` (Texts enum)
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Scripts/Utils/Util.cs` 또는 `Define.cs` (LocalizeKey)
- `Assets/@Resources/Data/LocalizationData*.json`
