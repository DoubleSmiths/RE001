# [슬라이드 11] 상태정보(Status) Popup #1 — 기본 구성

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 11
- 첨부 이미지:
  - `기획 ppt/slide_images/slide11/slide11_shape006_Google_Shape_249_p8.png` — `Frame_square_256` (팝업 메인 프레임)
  - `기획 ppt/slide_images/slide11/slide11_shape008_Google_Shape_251_p8.png` — `Lobby_CharacterInfo_RedGlow` (상단 붉은 글로우)
  - `기획 ppt/slide_images/slide11/slide11_shape009_Google_Shape_252_p8.png` — 구분선
  - `기획 ppt/slide_images/slide11/slide11_shape012_Google_Shape_255_p8.png` — Elixir 아이콘
  - `기획 ppt/slide_images/slide11/slide11_shape014_Google_Shape_257_p8.png` — `pick_hero_frame_2` (히어로 초상화 프레임)
  - `기획 ppt/slide_images/slide11/slide11_shape017_Google_Shape_260_p8.png` — `SwitchSelect_ArrowLeft`
  - `기획 ppt/slide_images/slide11/slide11_shape018_Google_Shape_261_p8.png` — `SwitchSelect_ArrowRight`
  - `기획 ppt/slide_images/slide11/slide11_shape019_Google_Shape_262_p8.png` — `Mythic_Button_(24)` 계열
  - `기획 ppt/slide_images/slide11/slide11_shape020_Google_Shape_263_p8.png` — `Mythic_Button_(29)` 체크 버튼
  - `기획 ppt/slide_images/slide11/slide11_shape024_Google_Shape_267_p8.png` — `shop_frame_glow`
  - `기획 ppt/slide_images/slide11/slide11_shape026_Google_Shape_269_p8.png` — `Frame_bg_big`
  - `기획 ppt/slide_images/slide11/slide11_shape034_Google_Shape_277_p8.gif` — Popup 열림 전체 GIF (하단에서 OutBack 올라옴)

먼저 Read 도구로 위 이미지들을 모두 열어 시각적으로 확인한 뒤 작업을 시작해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다.
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 용도
- 플레이어의 스탯과 능력치를 확인하는 Popup 창.
- 최하단 버튼 그룹의 **[상태] Button** 클릭 시 호출 (슬라이드 10 연계).

### 리소스
- 프레임: `Frame_square_256` / 상단 글로우: `Lobby_CharacterInfo_RedGlow` / 내부 배경: `message_popup_bg_default`
- 히어로 프레임: `pick_hero_frame_2`, 좌우 화살표: `SwitchSelect_ArrowLeft`/`Right`
- 엘릭서: `Elixir` 아이콘
- Mythic Button: `(24)`, `(29)`
- 글로우: `shop_frame_glow` (**Mask 처리**), 하단 상세 배경: `Frame_bg_big`
- Glow color 1: `#934A00` alpha `200`
- Glow color 2: `#458000` (녹색)

### Popup Open Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: `(x=0, y=-120, z=0)` (화면 아래에서 올라옴)
- OnActive → Restart, OnDisable → Rewind

### Open & Close 추가 연출 (Popup 프레임)
1. **Punch/Scale**
   - Duration: 0.3s / Ease: OutQuad
   - TO: `(x=-0.25, y=0.15, z=0)` / Vibrato: 10 / Elasticity: 1
2. **Scale (Yoyo)**
   - Duration: 0.05s / Ease: OutQuad
   - Loops: 2 / Yoyo / TO: 0.9 (Uniform)

## 작업 지시
1. **현재 구현 상태 조사**
   - `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs` 전체 (이미 OpenAnimation 호출).
   - prefab의 루트/프레임 DOTweenAnimation 3종 유무 (Move / Punch-Scale / Scale-Yoyo).
   - shop_frame_glow Mask 처리 유무.
   - SwitchSelect_Arrow 핸들러 유무.
   - `Managers.Game.StatPoint`, `Elixir` Property 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] Move Open Anim (from y=-120 OutBack 0.5s) prefab DOTweenAnimation?
   - [ ] OnDisable Rewind?
   - [ ] Punch/Scale (0.3s OutQuad, (-0.25, 0.15, 0), v10 e1) DOTweenAnimation?
   - [ ] Scale Yoyo (0.05s OutQuad Loops 2 Yoyo To 0.9) DOTweenAnimation?
   - [ ] pick_hero_frame_2 + 초상화 + 좌우 화살표 구성?
   - [ ] SwitchSelect_Arrow 핸들러?
   - [ ] Elixir 아이콘 + 수량 텍스트 바인딩?
   - [ ] shop_frame_glow Mask 처리 (Frame_bg_big 내부만)?
   - [ ] Glow color `#934A00 alpha 200` / `#458000`?
   - [ ] 슬라이드 8 BlackBG_Layer 공통 처리 적용?

3. **누락분만 구현**
   - 코드 `DOPunchScale` (L386/L397)을 prefab DOTweenAnimation으로 이관.
   - pick_hero_frame_2 + 화살표 구조 없으면 신규 추가 (동작 1차 스텁).
   - shop_frame_glow Mask: Frame_bg_big에 RectMask2D, glow는 자식.
   - Elixir property 없으면 스텁.

4. **결과 보고**
   - 체크리스트 결과 + 수정 prefab/파일 + 이관 DOTweenAnimation 목록.
   - 슬라이드 12~13은 별도 프롬프트. 본 11번은 프레임/Open Anim/배경 리소스 중심.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs` (L116~ Texts, L256~ Underlay, L299 OpenAnimation, L343 StatPoint/Elixir, L386/L397 DOPunchScale 이관 대상)
- `Assets/@Scripts/UI/UI_Popup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Sprite/` (Frame_square_256, Lobby_CharacterInfo_RedGlow, message_popup_bg_default, Elixir, pick_hero_frame_2, SwitchSelect_ArrowLeft/Right, Mythic_Button_24/29, shop_frame_glow, Frame_bg_big)
