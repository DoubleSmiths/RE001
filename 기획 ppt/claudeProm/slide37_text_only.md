# [슬라이드 37] 물약 상세정보 Popup #1 — 기본 화면 / Text 필드

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 기본 구조
- 상단 물약 아이콘 프레임은 `Icon_inventory_bag`, 그 외는 아이템 상세정보(슬라이드 21)와 동일.
- Glow는 등급별 색.

### 추가 UI
- `Options_lattice_3` (`#A1A1A1`)
- `Options_sq_mini`
- `T_gray_button_s` (`#D6D6D6` alpha 180)
- `Decor_description_line`

### Popup Open Anim (DOTweenAnimation)
- Move / 0.5s / OutBack / Loops 1
- From (0, -130, 0)
- OnDisable PlayBackwards

### 등급 시스템
1. 물약은 등급별 3단계.
2. 3단계에서 강화 시 다음 등급 1단계로.
3. 단계별 이름/설명/아이콘/능력치 변경 (ItemData.xlsx 참고).

### Text 필드
- `AlchemyText`, `ItemName (PotionNameTexts)`, `RankText`, `StatText`
- `DescriptionText` (Outline `#C59600` alpha 128)
- `ItemNeededText` (`#ECCC6C`), `NeedMatsText` (`#C8C8C8`)
- `NextInfoText` (Outline `#F5A200` alpha 146)

### 재료 Glow (제작의뢰 경유 시만 표시)
- 부족: `shop_frame_glow` `#000000`
- 충족: `shop_frame_glow` `#00710F`, Outline `#A2FF00` alpha 128

### 제작의뢰 전용 버튼
- [재료부족/제작], [다음단계 정보] — 제작의뢰 경유 시만 노출.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup.cs` 전체.
   - prefab 추가 UI 배치.
   - 등급/단계 데이터.
   - Glow 색 전환.
   - fromCustomOrder 분기.

2. **체크리스트**
   - [ ] Move (from y=-130 OutBack 0.5s)?
   - [ ] OnDisable PlayBackwards?
   - [ ] Icon_inventory_bag 프레임?
   - [ ] Glow 등급색?
   - [ ] 추가 UI 4종?
   - [ ] 8종 Text 바인딩 + Outline/Color?
   - [ ] 등급 3단계 → 다음 등급 전환?
   - [ ] Glow 색 전환 + Outline?
   - [ ] 제작의뢰 전용 버튼 분기?

3. **누락분만 구현**
   - TMP Underlay로 Outline.
   - Glow 색 전환 DOTweenAnimation 또는 런타임.
   - 등급색 매핑.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
- `Assets/@Scripts/Data/Data.Contents.cs`
