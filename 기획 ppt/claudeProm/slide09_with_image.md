# [슬라이드 9] 환경설정 Popup (SETTING)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 9
- 첨부 이미지:
  - `기획 ppt/slide_images/slide9/slide9_shape003_Google_Shape_145_p5.gif` — 환경설정 버튼 클릭 → Popup 열리는 전체 연출 GIF (OutBack 올라오는 느낌)
  - `기획 ppt/slide_images/slide9/slide9_shape008_Google_Shape_150_p5.png` — `Frame_square_256` (팝업 메인 프레임)
  - `기획 ppt/slide_images/slide9/slide9_shape010_Google_Shape_152_p5.png` — `Lobby_CharacterInfo_RedGlow` (상단 붉은 글로우)
  - `기획 ppt/slide_images/slide9/slide9_shape011_Google_Shape_153_p5.png` — 구분선 (가로 얇은 흰 라인, 섹션 타이틀 아래)
  - `기획 ppt/slide_images/slide9/slide9_shape013_Google_Shape_155_p5.png` — `Mythic_Button_(29)` (체크 O 아이콘, 어두운 배경에 흰 체크)
  - `기획 ppt/slide_images/slide9/slide9_shape016_Google_Shape_158_p5.png` — `Mythic_Button_(28)` (X 아이콘)
  - `기획 ppt/slide_images/slide9/slide9_shape017_Google_Shape_159_p5.png` — `fx_glow` 텍스처 (흰 원형 블러)
  - `기획 ppt/slide_images/slide9/slide9_shape021_Google_Shape_163_p5.gif` — Popup 내부 상세 연출 (언어 전환 Fx_glow)
  - `기획 ppt/slide_images/slide9/slide9_shape022_Google_Shape_164_p5.gif` — Mythic Button 클릭 시 Sprite 교체 GIF (체크 ↔ X)

먼저 Read 도구로 위 이미지들을 모두 열어 시각적으로 확인한 뒤 작업을 시작해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 용도
- 기본적인 게임 설정을 할 수 있는 Popup 창.
- 상단 재화 BAR 우측 **환경설정 버튼** 클릭으로 호출 (슬라이드 5 연계).

### 구성
- **타이틀**: `SETTING` (붉은 배경 + Lobby_CharacterInfo_RedGlow 장식)
- **섹션 1 — 언어설정**: `한국어` / `ENG` 두 라벨 (텍스트 버튼)
- **섹션 2 — 소리**: `배경음` / `효과음` — 각각 체크/X 토글 버튼
- **섹션 3 — 화면효과**: `피해숫자 생성` / `화면 흔들림` — 각각 체크/X 토글 버튼
- **닫기(X) 버튼**: 우상단
- **언어 선택 폰트 예외**: 슬라이드 3 스펙 — "한국어"/"ENG" 라벨은 **언어 무관 고정 폰트** (UI 언어 바뀌어도 두 라벨은 그대로).

### 배경 리소스
- `Frame_square_256` (메인 팝업 프레임, 256 정사각 프레임)
- `Lobby_CharacterInfo_RedGlow` (상단 붉은 글로우)
- `message_popup_bg_default` (내부 배경)

### 버튼 리소스
- `Mythic_Button_(29)` — 체크 O 스프라이트
- `Mythic_Button_(28)` — X 스프라이트

### 글로우 리소스
- `fx_glow` (원형 블러 스프라이트)
- Color: `#14FF00` (녹색 — 선택/ON 상태)
- Color: `#C53F2A` (적갈색 — 비선택 상태 가능성, 또는 언어 버튼 선택 표시)

### 애니메이션 스펙

#### Popup Open / Close (DOTweenAnimation 컴포넌트)
- Type: `Move`
- Duration: `0.5s`
- Delay: `0`
- Ease: `OutBack`
- Loops: `1`
- From: `(x=0, y=350, z=0)` (화면 아래에서 튀어 올라오는 느낌)
- **OnActive → Restart** (열림): From y=350 → 원위치로 이동
- **OnDisable → Rewind** (닫힘): 역재생하여 y=350 (아래)로 내려가며 사라짐

#### Button Click Anim (DOTweenAnimation 컴포넌트, 각 Mythic 버튼 / 언어 버튼)
- Type: `Scale`
- Duration: `0.07s`
- Delay: `0`
- Ease: `OutQuad`
- Loops: `2`
- Loop Type: `Yoyo`
- TO: `0.8` (Uniform Scale)
- 트리거: `OnClick`
- 의미: 0.07s × 2 Yoyo = 클릭 시 순간 수축→복귀.

### 상호작용 규칙
- **언어 버튼**: 한국어/ENG 중 현재 선택된 쪽에 `fx_glow` ON, 반대편은 `fx_glow` OFF. 전환 시 fx_glow가 켜지고 꺼지는 트랜지션.
- **Mythic Button**: 클릭 시 **Sprite Change** (체크 O ↔ X) — 토글.
- 토글 상태는 `Managers.Game.IsBGSoundOn` / `IsSFXOn` / `IsCreateAtkTextOn` / `IsShakeOn` 과 연동 (이미 있음).
- 언어 선택은 `Managers.Game.Language` 와 연동 (이미 있음).

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs` 전체 읽기 (이미 대부분 구현됨).
   - `UI_SettingPopup.prefab` 에서 `SettingPopup` 노드의 DOTweenAnimation Move 설정 확인 (Open from y=350 OutBack 0.5s).
   - 각 버튼에 DOTweenAnimation Scale 0.07/OutQuad/Yoyo×2/To=0.8 컴포넌트가 prefab에 있는지 확인 (현재 코드에서 `AddClickAnim`이 코드로 처리하고 있으면 이관 대상).
   - prefab에서 언어 버튼(KorBtn/EngBtn)의 `fx_glow` 자식 오브젝트 존재 여부 확인.
   - `_lgButtonColor`/`_greenColor` 상수 = `#C53F2A`/`#14FF00` 스펙 일치 확인.
   - `Managers.Game.Language`, `IsBGSoundOn`, `IsSFXOn`, `IsCreateAtkTextOn`, `IsShakeOn` Property 및 저장 로직 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] Popup Open Anim이 DOTweenAnimation Move (from y=350, OutBack, 0.5s) 로 prefab에 구성되어 있는가? 코드 구현이면 이관.
   - [ ] OnDisable Rewind 동작 — 닫을 때 역재생으로 내려가는가?
   - [ ] 모든 클릭 대상 버튼(KorBtn, EngBtn, BGM, SFX, Popup/AtkText, Shake, Xbutton)에 Scale Click DOTweenAnimation 있는가?
   - [ ] 언어 버튼 선택 표시가 `fx_glow` **On/Off** 방식인가? (현재 `SetButtonSelected`는 CanvasGroup alpha 또는 button color alpha 조정 — fx_glow 오브젝트 토글로 변경 필요할 수 있음)
   - [ ] fx_glow 자식 오브젝트가 언어 버튼 prefab에 있고 Color `#14FF00` (또는 `#C53F2A`) 으로 설정되어 있는가?
   - [ ] Mythic Button 클릭 시 Sprite가 체크 ↔ X 로 교체되는가? (현재 `UpdateToggleButton`에 구현됨 — 스프라이트 경로 Define.MYTHIC_BUTTON_CHECK/X 확인)
   - [ ] "한국어" / "ENG" 라벨은 언어 무관 고정 폰트인가? (슬라이드 3 예외 처리 연계)
   - [ ] 닫기(Xbutton) 클릭 / BlackBG 클릭 모두 Close Anim 후 `ClosePopupUI` 호출되는가?
   - [ ] BlackBG_Layer 공통 처리(슬라이드 8)가 이 Popup에도 적용되는가?

3. **누락분만 구현**
   - Open/Close 애니메이션이 코드 `DOTween.To` 등으로 구현되어 있으면 **prefab의 DOTweenAnimation**(Move, From y=350, OutBack, 0.5s, AutoPlay Off, Link OnEnable/OnDisable)으로 이관.
   - Button Click Scale 애니메이션은 각 버튼 오브젝트에 **DOTweenAnimation Scale**(Duration 0.07, Ease OutQuad, Loops 2 Yoyo, To 0.8, Trigger OnClick) 추가하고, `AddClickAnim` 코드는 제거.
   - 언어 버튼 선택 표시를 fx_glow 방식으로 바꿔야 하면 prefab에 fx_glow 자식 Image(Color #14FF00 또는 #C53F2A) 추가 + `RefreshLanguageButtons`에서 해당 fx_glow만 `SetActive` 토글.
   - "한국어"/"ENG" 라벨 TMP 폰트가 현재 UI 폰트 규칙을 따르고 있으면 슬라이드 3 스펙에 맞춰 고정 폰트로 교체.

4. **결과 보고**
   - 점검 체크리스트 결과 + 수정 prefab/파일 목록 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs` (L55~ 색상 상수, L72 _openFromY, L82~ AddClickAnim, L105~ 언어 전환, L127~ SetButtonSelected)
- `Assets/@Scripts/UI/UI_Popup.cs` (OpenAnimation / Close)
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (Language, IsBGSoundOn, IsSFXOn, IsCreateAtkTextOn, IsShakeOn)
- `Assets/@Scripts/Utils/Define.cs` (MYTHIC_BUTTON_CHECK, MYTHIC_BUTTON_X 경로 상수)
- `Assets/@Resources/Prefab/UI/Popup/UI_SettingPopup.prefab`
- `Assets/@Resources/Sprite/` (Frame_square_256, Lobby_CharacterInfo_RedGlow, message_popup_bg_default, Mythic_Button_28/29, fx_glow)
