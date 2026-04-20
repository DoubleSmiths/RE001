# [슬라이드 25] 마을 MID UI #1 — 사용 UI 리소스

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 25
- 첨부 이미지 (`기획 ppt/slide_images/slide25/`, 총 11개):
  - 마을 MID UI 전체 예시, `Icon_inventory_bag`, `Guild_Icon`, `Alchemy_Icon`, `Travel_Icon`, `Exit_Icon`, `pick_hero_frame_2`, `guild_npc_idle_profile_5`, Quest Icon (`gui_atlas Exclamation_mark 01`)

먼저 `기획 ppt/slide_images/slide25/` 내 모든 png를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 장소 이름 Text (현재 이동된 장소 표기)
- Text Color: `#FFFFFF`
- Text Outline: `#CFA936` alpha `118`, (x=0.8, y=0.8)
- 현재(즉, 이동된) 장소의 이름을 표기.

### 사용 UI Sprites (버튼/아이콘)
- `Icon_inventory_bag` — 가방 아이콘
- `Guild_Icon` — 모험가 협회 아이콘
- `Alchemy_Icon` — 연금공방 아이콘
- `Travel_Icon` — 여정(부츠) 아이콘
- `Exit_Icon` — 나가기 아이콘
- `pick_hero_frame_2` — 초상화 프레임
- `guild_npc_idle_profile_5` — 길드 NPC 프로필
- Quest Icon: `gui_atlas` 의 `Exclamation_mark 01` (슬라이드 4 공용 [!] 마크와 동일)

## 작업 지시
1. **조사**
   - `UI_GameScene.cs` MID UI 영역 (퀵슬롯 패널) 바인딩 확인.
   - `UI_Village1BtnGroup.cs` 에서 Guild/Alchemy/Monastery/Travel 버튼 오브젝트 존재 여부.
   - 각 아이콘 스프라이트가 Addressables에 등록되어 있는지.
   - 장소 이름 TMP 노드 (`PlaceNameText` 등) + Outline 설정 (`#CFA936 alpha 118, 0.8, 0.8`).

2. **체크리스트**
   - [ ] 장소 이름 TMP 존재 + Outline 색/오프셋 일치?
   - [ ] 8종 아이콘 스프라이트 바인딩?
   - [ ] 각 아이콘 Addressables 등록?
   - [ ] pick_hero_frame_2 초상화 프레임 배치?
   - [ ] Quest Icon (Exclamation_mark 01) 공용 prefab (슬라이드 4) 재사용?

3. **누락분만 구현**
   - 빠진 아이콘 Addressables 등록 + TMP Outline 설정 교정.
   - 공용 [!] prefab 있으면 재사용, 없으면 신설.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`
- `Assets/@Resources/Sprite/` (Icon_inventory_bag, Guild_Icon, Alchemy_Icon, Travel_Icon, Exit_Icon, pick_hero_frame_2, guild_npc_idle_profile_5, gui_atlas/Exclamation_mark 01)
