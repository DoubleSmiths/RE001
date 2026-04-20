# [슬라이드 20] 귀중품(Requisite) Popup

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 용도
- 컨텐츠 관련 아이템 보관 창.

### 타이틀
- `RequisiteTitleText`
- Outline: `#9F8320` (x=0.1, y=0.1)

### Icon 2 레이어 구성
- 같은 스프라이트 `Icon_inventory_bag` 을 2회 중첩하여 색으로 구분:
  - 예시 A: Layer1 `#FDE500`, Layer2 `#FFFFFF`
  - 예시 B: Layer1 `#FFFFFF`, Layer2 `#FF00C0`
- 아이템별 Layer1/Layer2 색 조합이 고유 색감을 만듦.

### 배경 프레임
- `Frame_bg_big_2` — Color `#FBFF00`

### 중요
- **귀중품 Slot에는 `shop_frame_glow` 미사용** (소지품과의 차이점).

### Open Anim (DOTweenAnimation Scale)
- Duration 0.15s / Ease OutQuad
- FROM: (x=0, y=1, z=0) — 세로 펼치기

### 획득 순서
- 왼쪽 위 → 오른쪽, 상→하.

## 작업 지시
1. **조사**
   - `UI_ImportantSlot.cs` / 귀중품 Popup 존재 여부.
   - 아이콘 2레이어 구조.
   - 타이틀 Outline 설정.
   - Frame_bg_big_2 색.
   - Open Anim prefab DOTweenAnimation.
   - shop_frame_glow 미사용 여부.

2. **체크리스트**
   - [ ] 귀중품 Popup + 필터?
   - [ ] 타이틀 Outline `#9F8320 (0.1, 0.1)`?
   - [ ] Icon 2레이어 (Layer1/Layer2 개별 색)?
   - [ ] Frame_bg_big_2 `#FBFF00`?
   - [ ] shop_frame_glow 미사용?
   - [ ] Open Anim Scale 0.15s OutQuad From (0, 1, 0)?
   - [ ] 획득 순서 좌→우, 상→하?
   - [ ] Slot 클릭 → 상세 Popup?

3. **누락분만 구현**
   - `UI_RequisitePopup.cs` + prefab 신설 (없을 경우).
   - 아이콘 2레이어 구조: `SetLayerColors(Color l1, Color l2)` 헬퍼.
   - 타이틀 TMP Outline.
   - Frame_bg_big_2 색 상수화.
   - Open Anim prefab DOTweenAnimation Scale.

4. **결과 보고**
   - 체크리스트 결과 + 수정/신규 파일·prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_ImportantSlot.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_RequisitePopup.prefab` (존재 여부 확인)
- `Assets/@Resources/Sprite/` (Icon_inventory_bag, Frame_bg_big_2)
