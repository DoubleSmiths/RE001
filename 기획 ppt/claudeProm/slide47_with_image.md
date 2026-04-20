# [슬라이드 47] 전투 UI 기획서 #1 — 구성 리소스 & Text 필드

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 47
- 첨부 이미지 (`기획 ppt/slide_images/slide47/`, 총 12개):
  - 전투 화면 예시, `Decorated_bar`, `Hp_frame_Empty`, `ComboGuage` (구글 드라이브 UI), `Decor_Title_options2`, `Skill_bar_noColbs_little`, `Icon_Frame`, `Icon_inventory`, `shop_frame_glow`, `T_gray_button`, ReturnText/TravelBtnText 등

먼저 `기획 ppt/slide_images/slide47/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 구성 리소스
- `Decorated_bar` — 상단 장식 바
- `Hp_frame_Empty` — HP 프레임 (슬라이드 6 공용)
- `ComboGuage` — 콤보 게이지 (구글 드라이브 UI 참조)
- `Decor_Title_options2` — 섹션 데코
- `Skill_bar_noColbs_little` — 스킬 바
- `Icon_Frame` — 아이콘 프레임
- `Icon_inventory` — 인벤토리 아이콘
- `shop_frame_glow`
- `T_gray_button` — 회색 버튼

### 콤보 관련 텍스트
- Text Color: `#FF001F` / Outline `#000000` (x=0.6, y=-0.6)
- `ComboText` — 현재 콤보 수
- `ComboLv0Text` ~ `ComboLv6Text` — 공세 단계 이름 (무딘, 숙련된, ... 등 슬라이드 48 연계)

### 기타 Text 필드
- `BattleLogText`
- `DmgBoostText`
- `AttackSpeedText`
- `BattleTimeText`
- `DmgDealtText`
- `DmgTakenText`
- `HPRestoredText`
- `ReturnText`, `TravelBtnText`

### 폰트
- **FONT: `RiaSans-Bold` 고정** (구글 드라이브 - FONT)
- 슬라이드 3의 폰트 예외 — 전투 UI 일부 텍스트는 RiaSans-Bold 사용.

## 작업 지시
1. **조사**
   - `UI_BattleScene.cs` Texts enum 및 바인딩 상태.
   - prefab의 Decorated_bar / Hp_frame_Empty / ComboGuage / Skill_bar / Icon_Frame / shop_frame_glow / T_gray_button 배치 여부.
   - `RiaSans-Bold` 폰트 에셋 존재 확인 (Assets/@Resources/Font 또는 Google Drive에서 다운로드 필요할 수 있음).
   - Combo 시스템 구현 (슬라이드 48에서 상세) 유무.

2. **체크리스트**
   - [ ] UI_BattleScene 바인딩 존재 + 리소스 배치?
   - [ ] 콤보 관련 Text 색 `#FF001F` + Outline `#000000 (0.6, -0.6)`?
   - [ ] ComboText + ComboLv0~Lv6Text 8개 바인딩?
   - [ ] BattleLog/DmgBoost/AttackSpeed/BattleTime/DmgDealt/DmgTaken/HPRestored 7종 바인딩?
   - [ ] Return/TravelBtn 2종 바인딩?
   - [ ] 전투 UI Text들이 `RiaSans-Bold` 폰트 고정?
   - [ ] 슬라이드 47 리소스 10종 배치?

3. **누락분만 구현**
   - 누락 Texts enum 필드 추가.
   - `RiaSans-Bold` 폰트 에셋 없으면 보고 후 중단 (구글 드라이브 다운로드 필요).
   - FontManager에 `Battle` FontType 추가 (`RiaSans-Bold` 반환) 검토.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab + 누락 에셋 목록.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_BattleScene.cs`
- `Assets/@Resources/Prefab/UI/Scene/UI_BattleScene.prefab`
- `Assets/@Resources/Font/RiaSans-Bold SDF.asset` (존재 여부 확인)
- `Assets/@Scripts/Managers/Core/FontManager.cs`
