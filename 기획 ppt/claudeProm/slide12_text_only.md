# [슬라이드 12] 상태정보(Status) Popup #2 — Text 설정

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약**: 기존 DOTweenAnimation 재사용 우선, 코드 Sequence 지양.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (텍스트 전용)

상태정보 Popup에 표기되어야 할 Text 필드 목록 (Texts enum에 포함 필요):

| Enum 필드 | 한글 라벨 | 용도 |
|---|---|---|
| `CharPopupTitleText` | 상태정보 | Popup 상단 타이틀 |
| `CharPopupStatPointText` | 능력점수 n | 보유 능력점수 표시 |
| `CharPopupElixirText` | (엘릭서 수량) | 엘릭서 재화 수량 |
| `StrengthText` | 근력 | 스탯 박스 라벨 |
| `MagicText` | 마력 | 스탯 박스 라벨 |
| `AgilityText` | 민첩 | 스탯 박스 라벨 |
| `VitialityText` | 체력 | 스탯 박스 라벨 (기획서 철자 Vitiality — 코드는 기획 표기 유지) |
| `CharPopupAttributeDetailText` | 능력 상세정보 | 버튼 라벨 |

## 작업 지시
1. **조사**
   - `UI_CharacterPopup.cs` Texts enum 전체 확인 (현재 `STRStatNameTxt` 등 네이밍 다를 수 있음).
   - prefab TMP_Text 바인딩 확인.
   - Localization 키 존재 여부 확인.

2. **체크리스트**
   - [ ] 8개 Text 필드 모두 Texts enum에 존재?
   - [ ] TMP_Text 오브젝트와 바인딩?
   - [ ] Localization에 한/영 둘 다 키 정의?
   - [ ] "체력/Vitality" 표시 문자열 확인 (코드 철자는 Vitiality 유지)?

3. **누락분만 구현**
   - 빠진 필드는 Texts enum + prefab TMP + Localize 호출 추가.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Data/LocalizationData*.json`
