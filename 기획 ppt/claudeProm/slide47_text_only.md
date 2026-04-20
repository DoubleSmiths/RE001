# [슬라이드 47] 전투 UI 기획서 #1 — 구성 리소스 & Text 필드

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 리소스
- `Decorated_bar`, `Hp_frame_Empty`, `ComboGuage`
- `Decor_Title_options2`, `Skill_bar_noColbs_little`
- `Icon_Frame`, `Icon_inventory`, `shop_frame_glow`, `T_gray_button`

### 콤보 Text
- Color `#FF001F`, Outline `#000000` (0.6, -0.6)
- `ComboText`, `ComboLv0Text` ~ `ComboLv6Text`

### 기타 Text
- `BattleLogText`, `DmgBoostText`, `AttackSpeedText`, `BattleTimeText`
- `DmgDealtText`, `DmgTakenText`, `HPRestoredText`
- `ReturnText`, `TravelBtnText`

### 폰트
- **FONT: `RiaSans-Bold` 고정** (슬라이드 3 예외).

## 작업 지시
1. **조사**
   - `UI_BattleScene.cs` Texts enum.
   - prefab 리소스 배치.
   - `RiaSans-Bold` 에셋 존재 확인.
   - Combo 시스템 (슬라이드 48).

2. **체크리스트**
   - [ ] UI_BattleScene 바인딩 + 리소스?
   - [ ] 콤보 Text `#FF001F` + Outline #000000 (0.6, -0.6)?
   - [ ] ComboText + Lv0~Lv6 8개?
   - [ ] 기타 7종 Text?
   - [ ] Return/Travel 2종?
   - [ ] RiaSans-Bold 적용?

3. **누락분만 구현**
   - Texts enum 보완.
   - `RiaSans-Bold` 에셋 없으면 보고 후 중단.
   - FontManager에 Battle FontType 검토.

4. **결과 보고**
   - 체크리스트 결과 + 파일 + 누락 에셋.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_BattleScene.cs`
- `Assets/@Resources/Prefab/UI/Scene/UI_BattleScene.prefab`
- `Assets/@Resources/Font/RiaSans-Bold SDF.asset`
- `Assets/@Scripts/Managers/Core/FontManager.cs`
