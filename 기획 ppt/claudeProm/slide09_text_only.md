# [슬라이드 9] 환경설정 Popup (SETTING)

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다.
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 용도
- 게임의 기본 설정을 조정하는 Popup.
- 상단 재화 BAR 우측 환경설정 버튼 클릭으로 호출 (슬라이드 5 연계).

### 구성 요소
| 섹션 | 항목 | 컨트롤 |
|---|---|---|
| 타이틀 | `SETTING` | (Lobby_CharacterInfo_RedGlow 장식) |
| 언어설정 | 한국어 / ENG | 텍스트 버튼 (선택 시 fx_glow On) |
| 소리 | 배경음 / 효과음 | Mythic 토글 버튼 (체크 ↔ X) |
| 화면효과 | 피해숫자 생성 / 화면 흔들림 | Mythic 토글 버튼 (체크 ↔ X) |
| 닫기 | X 버튼 (우상단) | — |

### 리소스
- 프레임: `Frame_square_256`
- 상단 글로우: `Lobby_CharacterInfo_RedGlow`
- 내부 배경: `message_popup_bg_default`
- 체크 버튼: `Mythic_Button_(29)`
- X 버튼: `Mythic_Button_(28)`
- 글로우: `fx_glow`, Color `#14FF00` (선택/ON) / `#C53F2A` (비선택/언어 버튼)

### 폰트 예외 (슬라이드 3 연계)
- 언어 선택의 "한국어" / "ENG" 라벨은 **언어 무관 고정 폰트**.

### Popup Open / Close Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: (x=0, y=350, z=0)
- OnActive → Restart (열림)
- OnDisable → Rewind (닫힘, 역재생)

### Button Click Anim (각 버튼에 DOTweenAnimation)
- Type: Scale / Duration: 0.07s / Delay: 0 / Ease: OutQuad
- Loops: 2 / Loop Type: Yoyo / TO: 0.8 (Uniform)
- Trigger: OnClick

### 상호작용 규칙
- 언어 버튼: 현재 선택된 쪽 fx_glow ON, 반대편 OFF. 전환 시 트랜지션.
- Mythic Button: 클릭 시 Sprite Change (체크 ↔ X).
- 토글 상태 → `Managers.Game.IsBGSoundOn`, `IsSFXOn`, `IsCreateAtkTextOn`, `IsShakeOn` 연동.
- 언어 선택 → `Managers.Game.Language` 연동.

## 작업 지시
1. **현재 구현 상태 조사**
   - `UI_SettingPopup.cs` 전체 읽기 (대부분 구현됨).
   - prefab에서 SettingPopup 노드의 Move DOTweenAnimation (from y=350, OutBack, 0.5s) 설정 확인.
   - 각 버튼 Scale Click DOTweenAnimation 존재 유무.
   - 언어 버튼의 fx_glow 자식 오브젝트 유무.
   - `_lgButtonColor`/`_greenColor` 상수 = `#C53F2A`/`#14FF00` 일치 확인.
   - Language/IsBGSoundOn/IsSFXOn/IsCreateAtkTextOn/IsShakeOn 연동 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] Popup Open Anim이 DOTweenAnimation (Move from y=350 OutBack 0.5s) prefab 구성?
   - [ ] OnDisable Rewind로 닫힐 때 역재생?
   - [ ] 모든 버튼에 Scale Click DOTweenAnimation 있음?
   - [ ] 언어 버튼 선택 표시가 fx_glow On/Off?
   - [ ] fx_glow 자식 Image (Color #14FF00 또는 #C53F2A) 존재?
   - [ ] Mythic 버튼 클릭 시 Sprite 체크 ↔ X 교체?
   - [ ] "한국어"/"ENG" 라벨 고정 폰트?
   - [ ] Xbutton / BlackBG 모두 Close Anim 후 ClosePopupUI 호출?
   - [ ] 슬라이드 8 BlackBG_Layer 공통 처리 적용?

3. **누락분만 구현**
   - Open/Close를 코드 DOTween으로 한다면 prefab DOTweenAnimation 이관 (AutoPlay Off, Link OnEnable/OnDisable).
   - Button Click Scale은 각 버튼에 DOTweenAnimation (Duration 0.07, OutQuad, Loops 2 Yoyo, To 0.8, Trigger OnClick) 추가, `AddClickAnim` 코드 제거.
   - 언어 선택 표시를 fx_glow 토글 방식으로: prefab에 fx_glow 자식 Image 추가 + `RefreshLanguageButtons`에서 SetActive.
   - "한국어"/"ENG" 라벨 TMP 폰트 고정.

4. **결과 보고**
   - 체크리스트 결과 + 수정 prefab/파일 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs` (L55~ 색상, L72 _openFromY, L82~ AddClickAnim, L105~ 언어 전환, L127~ SetButtonSelected)
- `Assets/@Scripts/UI/UI_Popup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/Utils/Define.cs` (MYTHIC_BUTTON_CHECK, MYTHIC_BUTTON_X)
- `Assets/@Resources/Prefab/UI/Popup/UI_SettingPopup.prefab`
- `Assets/@Resources/Sprite/` (Frame_square_256, Lobby_CharacterInfo_RedGlow, message_popup_bg_default, Mythic_Button_28/29, fx_glow)
