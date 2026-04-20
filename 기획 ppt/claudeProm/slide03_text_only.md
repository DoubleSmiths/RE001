# [슬라이드 3] Font 설정 구현/점검

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 1. INGAME 사용 폰트 3종
| 폰트 | 용도 | 언어 분기 |
|---|---|---|
| `Cafe24Klassic` | NPC Dialogue Texts (대화창) | 없음 — 항상 고정 |
| `DOSSaemmul` | 메뉴명/아이템명/설명 등 UI 전반 | 언어=영어일 때 사용 |
| `PF_Stardust` | 메뉴명/아이템명/설명 등 UI 전반 | 언어=한국어일 때 사용 |

- 단, **숫자 TEXT**는 위 규칙에서 제외 (아래 2번).

### 2. 숫자 전용 고정 폰트
- INGAME에 표시되는 **모든 숫자 텍스트**는 언어 선택과 무관하게 동일 폰트로 고정.
- 예: HP `65/65`, EXP `30.0000%`, 아이템 수량 `5`, 라벨 `[일반]`, Level 숫자 등.
- 시각적 특성: 두꺼운 픽셀 스타일 (기획서 샘플 기준).

### 3. Outline 규약
- 기본 규칙: **모든 TEXT에 검정 Outline 적용**.
- 예외: 해당 기획서에서 다른 색 Outline을 명시한 경우에만 변경.
- 상태 색상 팔레트 (텍스트 본색상 기준):
  - `기본` = `#FFFFFF`
  - `부족` = `#FEA400`
  - `없음/경고` = `#FF0000`
  - `있음/충족` = `#00FF00`

### 4. 예외처리 (언어 무관 고정)
- **Player Bar**: Level 정보와 모험가 등급은 언어 무관 `DOSSaemmul` 고정.
  - Level 표기는 항상 `LV.#` 형식 (예: `LV.1`).
- **환경설정 Popup의 언어선택 항목**: "한국어" / "ENG" 라벨은 고정 표기 (언어 바뀌어도 변하지 않음).

## 작업 지시
1. **현재 구현 상태 조사**
   - `Assets/@Scripts/Managers/Core/FontManager.cs` 읽기
   - `Assets/@Scripts/Utils/Define.cs`의 `FontType` enum 확인
   - `Assets/@Resources/Font/` 폰트 에셋 확인
   - `Grep`으로 `FontType`, `FontManager`, `GetFont` 호출부 전수 조사

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] `FontType.Number`(혹은 Fixed) enum 항목이 존재하는가?
   - [ ] 숫자 전용 폰트 에셋이 `FontManager.Init()`에서 로드되는가?
   - [ ] HP/EXP/Gold/Dia/Lv 등 숫자 TMP_Text가 숫자 폰트를 사용하는가?
   - [ ] Player Bar의 Level 텍스트가 언어 무관 DOSSaemmul 고정인가?
   - [ ] 환경설정 Popup의 "한국어"/"ENG" 라벨이 언어 무관 고정 폰트인가? (Popup 자체 존재 여부부터 확인)
   - [ ] TMP 에셋 전체에 검정 Outline 기본 적용이 되어 있는가?
   - [ ] Outline 상태 색상 상수(FFFFFF/FEA400/FF0000/00FF00)가 `Define.cs` 또는 별도 ColorUtil에 정의되어 있는가?

3. **누락분만 구현**
   - 이미 구현된 부분은 건드리지 말 것.
   - 공통 규약 준수.
   - 폰트 파일 자체가 없는 경우 보고 후 중단.
   - TMP Material Preset 일괄 변경 같은 광범위 수정은 사전 보고 후 진행.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일 목록 요약.

## 관련 파일 힌트
- `Assets/@Scripts/Managers/Core/FontManager.cs`
- `Assets/@Scripts/Utils/Define.cs`
- `Assets/@Scripts/Managers/GameManager.cs`
- `Assets/@Resources/Font/` (Cafe24Klassic SDF, DOSSaemmul SDF, PFStardust SDF)
- `Assets/AddressableAssetsData/AssetGroups/Fonts.asset`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (Player Bar HUD)
