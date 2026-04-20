# [슬라이드 37] 물약 상세정보 Popup #1 — 기본 화면 / Text 필드

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 37
- 첨부 이미지 (`기획 ppt/slide_images/slide37/`, 총 10개):
  - 기본화면, `Icon_inventory_bag`, `Options_lattice_3`, `Options_sq_mini`, `T_gray_button_s`, `Decor_description_line`, 재료부족/충족 Glow 샘플 (`shop_frame_glow` 빨강/초록)

먼저 `기획 ppt/slide_images/slide37/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 기본 구조
- 상단 물약 아이콘 프레임이 `Icon_inventory_bag` 인 것을 **제외**하고 전체적인 것은 **아이템 상세정보창(슬라이드 21)과 동일**.
- Glow는 해당 물약의 **등급색**으로 (예시는 빨강이지만 실제는 등급별).

### 추가 사용 UI
- `Options_lattice_3` — `#A1A1A1`
- `Options_sq_mini`
- `T_gray_button_s` — `#D6D6D6`, alpha 180
- `Decor_description_line`

### Popup Open Anim (DOTweenAnimation)
- Move / Duration 0.5s / Delay 0 / OutBack / Loops 1
- From `(x=0, y=-130, z=0)` (주의: 아이템 상세의 y=-120과 다르게 y=-130)
- OnDisable → PlayBackwards

### 등급 시스템
1. 물약은 등급별로 총 **3단계**.
2. 3단계에서 강화 시 **다음 등급의 1단계**로 전환.
3. 각 등급별로 이름/등급/설명/아이콘/능력치 변경.
4. 물약 별 능력치 정보는 `ItemData.xlsx` 참고.

### Text 필드
- `AlchemyText` — 제작 관련 라벨
- `ItemName (PotionNameTexts)` — 물약 이름
- `RankText` — 등급
- `StatText` — 능력치
- `DescriptionText` — 설명
  - Outline: `#C59600`, alpha 128
- `ItemNeededText` — `#ECCC6C` (필요 재료 제목)
- `NeedMatsText` — `#C8C8C8` (필요 재료 설명)
- `NextInfoText` — 다음 단계 정보
  - Outline: `#F5A200`, alpha 146

### 재료 부족/충족 Glow (제작의뢰 경유 시만 등장)
- **재료 부족**: `shop_frame_glow` `#000000`
- **재료 충족**: `shop_frame_glow` `#00710F`
  - `Outline: #A2FF00` alpha 128
- 이 2개의 버튼 ([재료부족/제작], [다음단계 정보])은 **제작의뢰를 통해 열었을 때만 등장**.

## 현재 구현 힌트 (memory)
- `UI_PotionDetailPopup.cs` 전면 재작성됨:
  - `SetInfo(int dataId, bool fromCustomOrder)` — fromCustomOrder=true일 때만 [재료부족/제작] / [다음단계] 버튼 노출.
  - HPPotionDic(DataId)로 바인딩, 재료 박스(ReqItemBox1_1~5), SpriteName 아이콘.
  - Localization: "설명" / "필요 재료" 오버라이드.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup.cs` 전체 읽기.
   - prefab 내 Options_lattice_3, Options_sq_mini, T_gray_button_s, Decor_description_line 존재 여부.
   - 등급/단계 시스템 데이터 구조 (PotionData.Grade, Step).
   - shop_frame_glow 색 전환 (`#000000` / `#00710F`) 로직.
   - fromCustomOrder 분기 로직.

2. **체크리스트**
   - [ ] Popup Open Move Anim (from y=-130 OutBack 0.5s)?
   - [ ] OnDisable PlayBackwards?
   - [ ] Icon_inventory_bag 프레임 사용?
   - [ ] Glow 색이 등급색 자동 매핑?
   - [ ] 추가 UI (Options_lattice_3 A1A1A1, Options_sq_mini, T_gray_button_s D6D6D6 180, Decor_description_line)?
   - [ ] AlchemyText / ItemName / RankText / StatText / DescText / ItemNeededText / NeedMatsText / NextInfoText 8종 바인딩?
   - [ ] DescText Outline `#C59600 alpha 128`?
   - [ ] NextInfoText Outline `#F5A200 alpha 146`?
   - [ ] ItemNeededText `#ECCC6C` / NeedMatsText `#C8C8C8`?
   - [ ] 등급 3단계 → 다음 등급 1단계 전환 로직?
   - [ ] 재료 충족/부족 shop_frame_glow 색 전환 + Outline?
   - [ ] [재료부족/제작] / [다음단계] 버튼 제작의뢰 경유 시만 노출?

3. **누락분만 구현**
   - Text Outline은 TMP Underlay로.
   - Glow 색 전환은 코드 또는 prefab DOTweenAnimation Color.
   - 등급색 매핑 `Define.GradeColors[grade]` 활용.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
- `Assets/@Scripts/Data/Data.Contents.cs` (PotionData)
