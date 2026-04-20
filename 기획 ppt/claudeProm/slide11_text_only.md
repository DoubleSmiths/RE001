# [슬라이드 11] 상태정보(Status) Popup #1 — 기본 구성

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다.
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 용도
- 플레이어의 스탯과 능력치를 확인하는 Popup.
- 최하단 [상태] 버튼 클릭 시 호출 (슬라이드 10 연계).

### 리소스
- 프레임: `Frame_square_256`
- 상단 글로우: `Lobby_CharacterInfo_RedGlow`
- 배경: `message_popup_bg_default`
- 히어로 프레임: `pick_hero_frame_2`
- 좌우 화살표: `SwitchSelect_ArrowLeft`, `SwitchSelect_ArrowRight`
- 엘릭서 아이콘: `Elixir`
- Mythic Button: `Mythic_Button_(24)`, `Mythic_Button_(29)`
- 글로우: `shop_frame_glow`, **Mask 처리**
- 하단 상세정보 배경: `Frame_bg_big`
- Glow color: `#934A00` alpha `200`, `#458000`

### Popup Open Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: (x=0, y=-120, z=0) — 화면 아래에서 올라옴
- OnActive → Restart, OnDisable → Rewind

### Open & Close 추가 연출 (Popup 프레임)
1. **Punch/Scale**
   - Duration: 0.3s / Ease: OutQuad
   - TO: (x=-0.25, y=0.15, z=0), Vibrato: 10, Elasticity: 1
2. **Scale (Yoyo)**
   - Duration: 0.05s / Ease: OutQuad
   - Loops: 2, Yoyo / TO: 0.9 (Uniform)

## 작업 지시
1. **현재 구현 상태 조사**
   - `UI_CharacterPopup.cs` 전체 읽기.
   - prefab DOTweenAnimation 3종 유무 (Move / Punch-Scale / Scale-Yoyo).
   - shop_frame_glow Mask 처리 유무.
   - SwitchSelect_Arrow 핸들러 유무.
   - `Managers.Game.StatPoint` / `Elixir` Property 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] Move Open Anim (from y=-120 OutBack 0.5s)?
   - [ ] OnDisable Rewind?
   - [ ] Punch/Scale DOTweenAnimation?
   - [ ] Scale Yoyo DOTweenAnimation?
   - [ ] pick_hero_frame_2 + 초상화 + 좌우 화살표?
   - [ ] SwitchSelect_Arrow 핸들러?
   - [ ] Elixir 아이콘 + 수량 텍스트?
   - [ ] shop_frame_glow Mask?
   - [ ] Glow color `#934A00 alpha 200` / `#458000`?
   - [ ] BlackBG_Layer 공통 처리 적용?

3. **누락분만 구현**
   - 코드 DOPunchScale → prefab DOTweenAnimation 이관.
   - 히어로 프레임/화살표 구조 없으면 스텁 추가.
   - Mask: Frame_bg_big에 RectMask2D, glow 자식.
   - Elixir property 없으면 스텁.

4. **결과 보고**
   - 체크리스트 결과 + 수정 prefab/파일 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Scripts/UI/UI_Popup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Sprite/` (Frame_square_256, Lobby_CharacterInfo_RedGlow, message_popup_bg_default, Elixir, pick_hero_frame_2, SwitchSelect_ArrowLeft/Right, Mythic_Button_24/29, shop_frame_glow, Frame_bg_big)
