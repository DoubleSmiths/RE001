# [슬라이드 20] 귀중품(Requisite) Popup

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 20
- 첨부 이미지 (`기획 ppt/slide_images/slide20/`, 총 8개):
  - 귀중품 Popup 레이아웃, RequisiteTitleText 샘플, Icon_inventory_bag 2레이어 색 구성, Frame_bg_big_2, Open Anim 등

먼저 `기획 ppt/slide_images/slide20/` 내 모든 파일을 Read로 열어 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 용도
- **컨텐츠 관련 아이템**이 보관되는 창 (귀중품 카테고리).

### 타이틀
- `RequisiteTitleText` ("귀중품")
- Outline: `#9F8320` (x=0.1, y=0.1)

### Icon 2 레이어 구성 (예시 A — 노란색)
- Layer1: `Icon_inventory_bag` → Color `#FDE500`
- Layer2: `Icon_inventory_bag` → Color `#FFFFFF`

### Icon 2 레이어 구성 (예시 B — 마젠타)
- Layer1: `Icon_inventory_bag` → Color `#FFFFFF`
- Layer2: `Icon_inventory_bag` → Color `#FF00C0`
- (두 레이어 중첩으로 아이템별 고유 색감 연출)

### 배경
- `Frame_bg_big_2` — Color `#FBFF00` (노란 프레임 배경)

### 중요 규칙
- **귀중품 Item Slot에는 `shop_frame_glow`가 사용되지 않음** (소지품 Popup과의 차이점).

### Open Anim (Scale 트윈, DOTweenAnimation)
- Type: `Scale`
- Duration: `0.15s`
- Ease: `OutQuad`
- FROM: `(x=0, y=1, z=0)` (세로 방향으로 펼쳐지듯 열림)

### 획득 순서
- 왼쪽 위 → 오른쪽 순서대로 획득.

## 작업 지시
1. **조사**
   - `UI_ImportantSlot.cs` / 귀중품 Popup (존재 여부 확인, 없으면 `UI_RequisitePopup.cs` 신설 필요).
   - `Icon_inventory_bag` 2레이어 구조 (같은 스프라이트 2회 다른 색) 구현 여부.
   - `RequisiteTitleText` Outline 설정.
   - `Frame_bg_big_2` 색 `#FBFF00` 적용 여부.
   - Open Anim prefab DOTweenAnimation 유무.
   - shop_frame_glow **미사용** 확인.

2. **체크리스트**
   - [ ] `UI_RequisitePopup` (또는 해당 Popup) 존재 + 귀중품 카테고리 필터?
   - [ ] RequisiteTitleText + Outline `#9F8320 (0.1, 0.1)`?
   - [ ] Icon 2레이어 (Icon_inventory_bag 2회) 구성 — Layer1/Layer2 Color 개별 제어?
   - [ ] Frame_bg_big_2 색 `#FBFF00`?
   - [ ] shop_frame_glow 미사용?
   - [ ] Open Anim Scale 0.15s OutQuad From (0, 1, 0) DOTweenAnimation?
   - [ ] 획득 순서 좌→우, 상→하?
   - [ ] Slot 클릭 → 상세정보 Popup?

3. **누락분만 구현**
   - Popup 없으면 `UI_RequisitePopup.cs` + prefab 신설 (소지품 Popup 구조 참고).
   - Icon 2레이어: UI_ItemIcon에 Layer1/Layer2 `Image` 필드 + `SetLayerColors(Color l1, Color l2)` 헬퍼.
   - RequisiteTitleText TMP Outline 설정.
   - Frame_bg_big_2 색 상수화 + Image 적용.
   - Open Anim은 prefab의 DOTweenAnimation Scale (AutoPlay Off, Link OnEnable).

4. **결과 보고**
   - 체크리스트 결과 + 수정/신규 파일·prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_ImportantSlot.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs` (레이아웃 참고)
- `Assets/@Resources/Prefab/UI/Popup/UI_RequisitePopup.prefab` (존재 여부 확인)
- `Assets/@Resources/Sprite/` (Icon_inventory_bag, Frame_bg_big_2)
