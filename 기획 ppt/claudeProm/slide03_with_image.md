# [슬라이드 3] Font 설정 구현/점검

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 3
- 첨부 이미지 (슬라이드에서 추출됨):
  - `기획 ppt/slide_images/slide3/slide3_shape004_Google_Shape_61_p2.png` — Cafe24 폰트 샘플 ("안녕하세요")
  - `기획 ppt/slide_images/slide3/slide3_shape005_Google_Shape_62_p2.png` — DOSSaemmul 폰트 샘플 ("WARNING")
  - `기획 ppt/slide_images/slide3/slide3_shape006_Google_Shape_63_p2.png` — PF_Stardust 폰트 샘플 ("모험가협회")
  - `기획 ppt/slide_images/slide3/slide3_shape018_Google_Shape_75_p2.png` — 숫자 전용 폰트 예시 ("65")
  - `기획 ppt/slide_images/slide3/slide3_shape019_Google_Shape_76_p2.png` — 숫자 전용 폰트 예시 ("[일반]")
  - `기획 ppt/slide_images/slide3/slide3_shape022_Google_Shape_79_p2.png` — Player Bar 예외 (모험가/LV.1/65/65/30.0000%)
  - `기획 ppt/slide_images/slide3/slide3_shape026_Google_Shape_83_p2.png` — Outline 색상 4종 (기본 FFFFFF / 부족 FEA400 / 없음·경고 FF0000 / 있음·충족 00FF00)
  - `기획 ppt/slide_images/slide3/slide3_shape027_Google_Shape_84_p2.gif` — 환경설정 Popup (언어설정 한국어/ENG 고정 표기)

먼저 Read 도구로 위 이미지들을 모두 열어 시각적으로 확인한 뒤 작업을 시작해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약
1. **INGAME 사용 폰트 3종**
   - `Cafe24Klassic`: NPC Dialogue 전용. **영/한 무관 항상 고정**.
   - `DOSSaemmul`: **언어=영어**일 때 UI 전체 (메뉴명/아이템명/설명 등), 대화·숫자 제외.
   - `PF_Stardust`: **언어=한국어**일 때 UI 전체, 대화·숫자 제외.
2. **숫자 전용 고정 폰트**
   - INGAME의 모든 숫자 TEXT는 언어 선택과 무관하게 **지정된 동일 폰트**로 고정.
   - 예시 이미지의 "65", "[일반]" 스타일 (두꺼운 픽셀 폰트).
3. **Outline 규약**
   - 모든 TEXT는 **기본 검정 Outline** 적용.
   - 다른 색 Outline이 필요한 경우는 **각 개별 기획서에서 명시** → 구현 시 해당 슬라이드 지시를 따른다.
   - 상태 색상 팔레트:
     - `기본`: FFFFFF
     - `부족`: FEA400
     - `없음/경고`: FF0000
     - `있음/충족`: 00FF00
4. **예외처리 (언어 무관 고정)**
   - **Player Bar의 Level 정보 및 모험가 등급**: 언어 선택과 상관없이 `DOSSaemmul` 고정. Level은 항상 `LV.#` 형식.
   - **환경설정 Popup의 언어 선택 부분** (`한국어` / `ENG`): 고정 표기.

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `Assets/@Scripts/Managers/Core/FontManager.cs` 읽기
   - `Assets/@Scripts/Utils/Define.cs`의 `FontType` enum 확인
   - `Assets/@Resources/Font/` 디렉터리에 폰트 에셋 3종이 모두 있는지 확인
   - `Grep`으로 `FontType`, `FontManager`, `GetFont` 호출부를 모두 찾아 어디서 어떻게 쓰이는지 파악
2. **기획서 대비 누락 점검 (필수)**
   - [ ] `FontType.Number` (또는 `FontType.Fixed`) 정의 여부 — 숫자 전용 고정 폰트가 enum에 있는가?
   - [ ] 숫자용 폰트 에셋이 Resource로 로드되는가? (현재는 3종만 Init 됨)
   - [ ] HP/MP/EXP/Gold/Dia/Lv 표기 등 **숫자가 들어가는 TMP_Text**가 실제로 숫자 폰트를 참조하는가?
   - [ ] Player Bar(`UI_GameScene` HUD)의 Level 텍스트가 언어 무관 DOSSaemmul 고정인가?
   - [ ] 환경설정 Popup의 "한국어"/"ENG" 라벨이 언어 무관 고정 폰트인가? (해당 Popup 존재 여부부터 확인)
   - [ ] TMP Outline 기본 설정 (검정)이 모든 TMP 에셋의 Material Preset 또는 Outline 속성에 일괄 적용되어 있는가?
   - [ ] Outline 색상 상수(FFFFFF/FEA400/FF0000/00FF00)가 `Define.cs` 또는 별도 ColorUtil에 상수로 정의되어 있는가?
3. **누락분만 구현 (이미 있는 것 재작업 금지)**
   - 누락 항목 각각에 대해 공통 규약을 준수해 구현.
   - 숫자 폰트 에셋이 없으면 먼저 에셋 존재 여부만 보고하고 구현 중단.
   - Outline 기본값을 일괄 변경해야 하는 경우 영향 범위를 먼저 사용자에게 보고.
4. **결과 보고**
   - 점검 체크리스트 결과 + 실제 수정한 파일 목록을 간결히 요약.

## 관련 파일 힌트
- `Assets/@Scripts/Managers/Core/FontManager.cs`
- `Assets/@Scripts/Utils/Define.cs` (FontType, Language)
- `Assets/@Scripts/Managers/GameManager.cs` (Language 저장/스위치)
- `Assets/@Resources/Font/*.asset`
- `Assets/AddressableAssetsData/AssetGroups/Fonts.asset`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (Player Bar HUD — Lv.# 표기 위치)
