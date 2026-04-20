# [슬라이드 25] 마을 MID UI #1 — 사용 UI 리소스

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 장소 이름 Text
- Color: `#FFFFFF`
- Outline: `#CFA936` alpha `118`, (x=0.8, y=0.8)
- 현재 이동된 장소의 이름 표기.

### 사용 UI Sprites
- `Icon_inventory_bag`, `Guild_Icon`, `Alchemy_Icon`, `Travel_Icon`, `Exit_Icon`
- `pick_hero_frame_2`, `guild_npc_idle_profile_5`
- Quest Icon: `gui_atlas` > `Exclamation_mark 01`

## 작업 지시
1. **조사**
   - `UI_GameScene.cs` MID UI 바인딩.
   - `UI_Village1BtnGroup.cs` 버튼 오브젝트.
   - Addressables 등록 확인.
   - 장소 이름 TMP + Outline 설정.

2. **체크리스트**
   - [ ] 장소 이름 TMP + Outline `#CFA936 alpha 118 (0.8, 0.8)`?
   - [ ] 8종 아이콘 스프라이트 바인딩?
   - [ ] Addressables 등록?
   - [ ] pick_hero_frame_2 프레임?
   - [ ] Quest Icon 공용 prefab (슬라이드 4) 재사용?

3. **누락분만 구현**
   - Addressables 등록 + TMP Outline 교정.
   - 공용 [!] prefab 재사용/신설.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
- `Assets/@Resources/Sprite/` 관련 스프라이트
