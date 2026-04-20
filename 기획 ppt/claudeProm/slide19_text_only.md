# [슬라이드 19] 소지품(Inventory) Popup

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 용도
- 모든 재료/사용 아이템(물약 제외) 보관 창.

### 리소스
- `BAR_Bag`, `Icon_inventory_bag`, `shop_frame_glow`, `Chat_Icon_Lock`
- `gui_atlas (slider_button01)` 스크롤 슬라이더
- `T_gray_button`, `Stencils 32 (65)`, `InventoryTitleText`

### Popup Open Anim (DOTweenAnimation)
- Move / Duration 0.5s / Ease OutBack / Loops 1
- From (x=0, y=-120, z=0)
- OnActive Restart, OnDisable Rewind

### Slot 구조
- ScrollView 무제한.
- 초기 사용가능 **16칸**, 나머지 잠금.

### Slot 상태
| 상태 | Glow | 상호작용 |
|---|---|---|
| 사용가능 | `#FFFFFF alpha 40` | X |
| 사용중 | 등급색 | O |
| 잠김 | `#0F0F0F`, 자물쇠 `#FFFFFF alpha 150` | X |

### 획득 순서
- 왼쪽 위 → 오른쪽, 상→하.

### [정리] 버튼
- 왼쪽 위부터 재정리.
- **Anim (DOTweenAnimation Scale)**: Duration 0.05, OutQuad, Loops 2 Yoyo, To 0.9
- NEW 표시는 버그로 명시 → 무시.

### 사용중 Slot Anim (DOTweenAnimation Punch/Scale)
- Duration 0.3 / OutQuad / Loops 1
- TO (-0.25, 0.15, 1) / Vibrato 10 / Elasticity 1

### New Text 조건
- **새로운 종류** 아이템 **최초 획득** 시.
- 위치: [가방] 버튼 + Popup 해당 Slot.
- 수량 증가는 제외.

## 작업 지시
1. **조사**
   - `UI_InventoryPopup.cs` + `UI_InvSlot.cs`.
   - Slot 3상태 시각 분기.
   - 초기 16칸/잠금 로직.
   - [정리] 버튼 + Scale DOTweenAnimation.
   - 사용중 Slot Punch/Scale DOTweenAnimation.
   - NEW 조건 로직.
   - Open Anim prefab DOTweenAnimation.

2. **체크리스트**
   - [ ] ScrollView 무제한 Slot?
   - [ ] 초기 16칸 사용가능 + 나머지 잠금?
   - [ ] 사용가능 Glow `#FFFFFF alpha 40`?
   - [ ] 사용중 Glow 등급색?
   - [ ] 잠긴 Glow `#0F0F0F` + 자물쇠 `#FFFFFF alpha 150`?
   - [ ] 획득 순서 좌→우/상→하?
   - [ ] [정리] 버튼 Scale DOTweenAnimation?
   - [ ] 사용중 Slot Punch/Scale DOTweenAnimation?
   - [ ] NEW: 최초 획득만, 수량 증가 제외?
   - [ ] Slot 클릭 → 상세 Popup?
   - [ ] Open Anim Move (from y=-120 OutBack 0.5s)?

3. **누락분만 구현**
   - Slot 상태 enum + Glow 매핑.
   - 초기 16칸 + 잠금 제어.
   - [정리] Scale DOTweenAnimation + Sort 함수.
   - 사용중 Punch/Scale DOTweenAnimation.
   - NEW: `_firstAcquiredItemIds` 집합.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_InventoryPopup.prefab`
- `Assets/@Resources/Sprite/` (BAR_Bag, Icon_inventory_bag, shop_frame_glow, Chat_Icon_Lock, gui_atlas slider_button01, T_gray_button, Stencils 32)
