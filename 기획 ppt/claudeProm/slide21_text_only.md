# [슬라이드 21] 아이템 상세정보 Popup

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 용도
- 아이템의 모든 정보(이름/설명/등급/획득처/보유개수) 표시.

### 리소스
- 프레임: `Frame_square_256_vert`
- 타이틀: `Title_frame_m`, `Decor_Title_options`, `Frame_title_dots`
- 글로우: `shop_frame_glow`
- 아이콘 슬롯: `Inventory_Slot_IconFrame`
- 옵션: `2_options_off`
- 배경: `message_popup_bg_default`

### Text 필드
- `ItemNameText`, `RarityText:`, `FoundAtText:`, `ItemDesc`
- `ItemDesc` Outline: `#BE7F00` (x=1, y=1), alpha `120`

### 보유 개수 표시
- 소지품 아이콘과 동일 구조.
- **0일 시 Text 표기 X**.

### 아이템 아이콘 Layer (슬라이드 18 공용)
- Layer1/Layer2/Layer3 + Mask + Layer4(등급색)

### Popup Open Anim (DOTweenAnimation)
- Move / 0.5s / OutBack / Loops 1
- From (0, -120, 0)
- OnActive Restart, OnDisable Rewind

## 작업 지시
1. **조사**
   - `UI_ItemDetailPopup.cs` / prefab 존재 여부.
   - ItemData 필드 유무.
   - `UI_PotionDetailPopup`과 공용 베이스 가능성.

2. **체크리스트**
   - [ ] Item 상세 Popup 존재?
   - [ ] 리소스 배치 (Frame/Title/Glow/Decor/Frame_title_dots)?
   - [ ] 4개 TMP 바인딩 (Name/Rarity/FoundAt/Desc)?
   - [ ] Desc Outline `#BE7F00 (1,1) alpha 120`?
   - [ ] 보유 개수 표시 (0 숨김)?
   - [ ] 아이콘 Layer 1~4 + Mask?
   - [ ] Open Anim prefab DOTweenAnimation?
   - [ ] OnDisable Rewind?
   - [ ] BlackBG_Layer 공통 처리?

3. **누락분만 구현**
   - Popup 없으면 신설 (PotionDetailPopup 구조 참고).
   - ItemData 필드 추가.
   - Outline TMP Underlay.
   - prefab DOTweenAnimation.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_ItemDetailPopup.cs` (존재 여부 확인)
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Resources/Prefab/UI/Popup/`
