# [슬라이드 19] 소지품(Inventory) Popup

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 19
- 첨부 이미지 (`기획 ppt/slide_images/slide19/`, 총 17개):
  - `slide19_shape003_그림_3.gif` — Open 애니메이션 예시
  - `slide19_shape007_그림_8.png` (BAR_Bag), `slide19_shape009_그림_11.png` (Icon_inventory_bag)
  - `slide19_shape011_그림_14.png` (shop_frame_glow), `slide19_shape013_그림_17.png` (Chat_Icon_Lock)
  - `slide19_shape015_그림_20.png` (gui_atlas slider_button01), `slide19_shape017_그림_23.png` (T_gray_button)
  - `slide19_shape019_그림_26.png` (Stencils 32), `slide19_shape021_그림_29.png` (InventoryTitleText)
  - `slide19_shape026/027/028_그림_37/39/41.png` — 사용가능 / 사용중 / 잠긴 Slot 색 샘플
  - `slide19_shape032_그림_47.png` — 획득 순서
  - `slide19_shape036_그림_53.gif` — [정리] 버튼 애니
  - `slide19_shape042_그림_61.gif` — 사용중 Slot Punch/Scale
  - `slide19_shape045_그림_2.gif`, `slide19_shape048_그림_16.png` — NEW Text 조건

먼저 `기획 ppt/slide_images/slide19/` 내 모든 파일 `ls` 후 핵심 gif/png 를 Read로 열어 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 용도
- 모든 재료와 사용 아이템(물약 제외)이 보관되는 창.

### 리소스
- `BAR_Bag`, `Icon_inventory_bag`, `shop_frame_glow`, `Chat_Icon_Lock`
- `gui_atlas (slider_button01)` — 스크롤 슬라이더 핸들
- `T_gray_button` — 그레이 버튼
- `Stencils 32 (65)` — 스텐실(Mask) 프레임
- `InventoryTitleText` — "소지품" 타이틀 TMP

### Popup Open Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: `(x=0, y=-120, z=0)`
- OnActive → Restart, OnDisable → Rewind

### Slot 구조
- 제한 **없음** — ScrollView 처리.
- **초기 사용 가능 Slot: 16칸**. 나머지는 **잠금 처리**.

### Slot 상태별 시각
| 상태 | Glow color | 상호작용 |
|---|---|---|
| 사용가능 (획득 가능) | `#FFFFFF` alpha 40 | X |
| 사용중 (획득 불가) | 해당 아이템 **등급색** | O |
| 잠긴 (획득 불가) | `#0F0F0F`, 자물쇠 `#FFFFFF` alpha 150 | X |

### 획득 순서
- 가장 **왼쪽 위**부터 **오른쪽**으로 순서대로 획득 (좌→우, 상→하).

### [정리] 버튼
- 클릭 시 왼쪽 위부터 오른쪽으로 순서대로 재정리.
- (NEW 표시는 버그로 명시됨 — 무시)
- **Button Anim (DOTweenAnimation Scale)**:
  - Duration: 0.05s / Ease: OutQuad
  - Loops: 2 / Loop Type: Yoyo / TO: 0.9

### 사용중 Slot Anim (DOTweenAnimation Punch/Scale)
- Duration: 0.3s / Ease: OutQuad / Loops: 1
- TO: (x=-0.25, y=0.15, z=1)
- Vibrato: 10, Elasticity: 1

### New Text 조건
- **새로운 종류**의 아이템을 **최초 획득**했을 때 표시.
- 표시 위치: [가방] 버튼 + 소지품 Popup의 해당 Slot.
- **수량 증가**는 NEW 대상이 **아님**.

## 작업 지시
1. **조사**
   - `UI_InventoryPopup.cs` 바인딩 + `UI_InvSlot.cs` 구조.
   - Slot 3상태(사용가능/사용중/잠김) 시각 분기 유무.
   - 초기 16칸 / 잠금 처리 로직.
   - [정리] 버튼 핸들러 + Scale DOTweenAnimation 유무.
   - 사용중 Slot Punch/Scale DOTweenAnimation 유무.
   - NEW Text 조건 (최초 획득만, 수량 증가 제외) 로직.
   - Open Anim prefab DOTweenAnimation Move 유무.

2. **체크리스트**
   - [ ] ScrollView로 무제한 Slot?
   - [ ] 초기 16칸 사용가능 / 나머지 잠금?
   - [ ] 사용가능 Glow `#FFFFFF alpha 40`?
   - [ ] 사용중 Glow = 등급색?
   - [ ] 잠긴 Glow `#0F0F0F` + 자물쇠 `#FFFFFF alpha 150`?
   - [ ] 획득 순서 좌→우, 상→하?
   - [ ] [정리] 버튼 구현 + Scale DOTweenAnimation (0.05s OutQuad Loops 2 Yoyo To 0.9)?
   - [ ] 사용중 Slot Punch/Scale DOTweenAnimation (0.3s OutQuad, (-0.25, 0.15, 1), v10 e1)?
   - [ ] NEW 조건: 최초 획득만, 수량 증가 제외?
   - [ ] 아이템 Slot 클릭 → 상세정보 Popup 호출?
   - [ ] Open Anim Move (from y=-120 OutBack 0.5s) prefab DOTweenAnimation?

3. **누락분만 구현**
   - Slot 상태 enum + Glow 색 매핑.
   - 초기 16칸 + 잠금 제어 로직.
   - [정리] 버튼 Scale DOTweenAnimation, Sort 함수.
   - 사용중 Slot DOTweenAnimation Punch/Scale.
   - NEW 조건: `_firstAcquiredItemIds` 집합 관리 (아이템 최초 획득 시 추가, Popup 열기/클릭으로 소거).

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (Inventory, FirstAcquired 추적)
- `Assets/@Resources/Prefab/UI/Popup/UI_InventoryPopup.prefab`
- `Assets/@Resources/Sprite/` (BAR_Bag, Icon_inventory_bag, shop_frame_glow, Chat_Icon_Lock, gui_atlas slider_button01, T_gray_button, Stencils 32)
