# [슬라이드 21] 아이템 상세정보 Popup

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 21
- 첨부 이미지 (`기획 ppt/slide_images/slide21/`, 총 15개):
  - Popup 전체 레이아웃, `Frame_square_256_vert`(세로 프레임), `Title_frame_m`, `shop_frame_glow`, `Inventory_Slot_IconFrame`, `Decor_Title_options`, `2_options_off`, `Frame_title_dots`, `message_popup_bg_default`, 아이콘 3 레이어 + Mask 처리 레이어, `ItemNameText`, `ItemDesc` 예시

먼저 `기획 ppt/slide_images/slide21/` 내 모든 파일을 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 용도
- 아이템의 모든 정보(이름/설명/등급/획득처/보유개수 등)가 나오는 창.
- 슬라이드 18의 아이템 클릭 시 호출, 슬라이드 19/20에서 클릭 시 호출.

### 리소스
- `Frame_square_256_vert` — 메인 프레임 (세로 긴 형태)
- `Title_frame_m` — 타이틀 영역 프레임
- `shop_frame_glow`
- `Inventory_Slot_IconFrame` — 아이콘 슬롯 프레임
- `Decor_Title_options` — 타이틀 데코
- `2_options_off` — 옵션 버튼 비활성 상태
- `Frame_title_dots` — 점선 장식
- `message_popup_bg_default` — 내부 배경

### Text 필드
- `ItemNameText` — 아이템명
- `RarityText:` — 등급
- `FoundAtText:` — 획득처
- `ItemDesc` — 설명
  - Outline: `#BE7F00` (x=1, y=1), alpha `120`

### 보유 개수 표시
- 소지품 아이콘과 동일한 구조로 해당 아이템 보유 개수 표시.
- **0일 시 Text 표기 X**.

### 아이템 아이콘 Layer (슬라이드 18 연계)
- Layer1, Layer2, Layer3 — 기본 아이콘 레이어들
- **Mask 처리**
- Layer4 — 등급색/오버레이

### Popup Open Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: `(x=0, y=-120, z=0)`
- OnActive → Restart, OnDisable → Rewind

## 작업 지시
1. **조사**
   - `UI_ItemDetailPopup.cs` / `UI_InventoryDetailPopup.cs` 존재 여부 확인 (없으면 신설).
   - prefab의 Open Anim DOTweenAnimation Move 유무.
   - ItemData에 `Name`, `Rarity`, `FoundAt`, `Desc` 필드 존재 여부.
   - `UI_PotionDetailPopup`/`UI_SkillDetailPopup` 과 구조 비교 — 공용 베이스로 통합 가능한지.

2. **체크리스트**
   - [ ] Item 상세 Popup 존재?
   - [ ] Frame_square_256_vert / Title_frame_m / shop_frame_glow / Inventory_Slot_IconFrame / Decor_Title_options / Frame_title_dots 배치?
   - [ ] ItemNameText / RarityText / FoundAtText / ItemDesc 4개 TMP 바인딩?
   - [ ] ItemDesc Outline `#BE7F00` (1, 1) alpha 120?
   - [ ] 보유 개수 표시 (0일 때 숨김)?
   - [ ] Layer1/2/3/4 + Mask 아이콘 구조 (슬라이드 18 공용)?
   - [ ] Open Anim Move (from y=-120 OutBack 0.5s) prefab DOTweenAnimation?
   - [ ] OnDisable Rewind?
   - [ ] BlackBG_Layer 슬라이드 8 공통 처리 적용?

3. **누락분만 구현**
   - `UI_ItemDetailPopup.cs` + prefab 신설 시 `UI_PotionDetailPopup` 구조 참고.
   - ItemData에 누락 필드 추가 + JSON/XLSX 연계.
   - Outline은 TMP 컴포넌트 Underlay로.
   - Open Anim prefab DOTweenAnimation.

4. **결과 보고**
   - 체크리스트 결과 + 신규/수정 파일·prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (구조 참고)
- `Assets/@Scripts/UI/Popup/UI_ItemDetailPopup.cs` (존재 여부 확인)
- `Assets/@Scripts/Data/Data.Contents.cs` (ItemData)
- `Assets/@Resources/Prefab/UI/Popup/` (UI_ItemDetailPopup.prefab 존재 여부)
