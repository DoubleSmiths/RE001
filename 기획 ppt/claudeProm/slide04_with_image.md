# [슬라이드 4] UI Feedback Guide — NEW 버튼 / [!] 버튼

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 4
- 첨부 이미지:
  - `기획 ppt/slide_images/slide4/slide4_shape004_그림_3.gif` — "NEW" 텍스트 (노란 FFFF00, DOSSaemmul) 애니메이션 샘플
  - `기획 ppt/slide_images/slide4/slide4_shape005_그림_5.gif` — NEW 뱃지가 물약 아이템 우상단에 붙은 실제 적용 예시
  - `기획 ppt/slide_images/slide4/slide4_shape006_그림_7.gif` — [!] 느낌표 버튼 (황금색, Punch/Rotation 애니메이션) 샘플
  - `기획 ppt/slide_images/slide4/slide4_shape007_그림_9.gif` — [!] 뱃지가 모험가협회 아이콘 우상단에 붙은 실제 적용 예시

먼저 Read 도구로 위 4개 이미지를 모두 열어 시각적으로 확인한 뒤 작업을 시작해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 1. NEW 버튼 (뱃지)
- **용도**: 아이템 또는 기능이 새로 생겼을 때 표시. 유저가 아직 확인하지 않았음을 알림.
- **표시 위치**: 각 개별 기획서에 서술 (예: 아이템 아이콘 우상단, 메뉴 버튼 상단 등).
- **사라지는 조건**: 해당 버튼(또는 NEW가 붙은 요소)을 누르거나, 해당 Popup 창을 닫으면 사라짐.
- **폰트**: `DOSSaemmul` (언어 무관 고정 — 시각적으로 "NEW"는 영문이지만 폰트는 DOSSaemmul 픽셀 스타일로 고정).
- **색상**: `#FFFF00` (순노랑).
- **애니메이션 (DOTween Move)**:
  - Duration: `0.5s`
  - Ease: `OutQuad`
  - Loops: `-1` (무한)
  - Loop Type: `Restart`
  - TO: `(x=1, y=2, z=0)` — **Snapping 옵션 ON** (픽셀 정렬)
  - 해석: 원점에서 시작 → (1, 2, 0)으로 이동 후 Restart 반복. Snapping으로 픽셀 단위 스텝 느낌.

### 2. [!] 버튼 (느낌표 아이콘)
- **용도**: 메인/서브 퀘스트 가이드 UI. 다음 행선지를 목표로 표시.
- **표시 위치**: 각 개별 기획서에 서술 (예: 퀘스트 대상 NPC/건물 위, 맵 아이콘 위).
- **표시/비표시 제어**: 퀘스트 번호(진행 상태)에 맞춰 해당 타겟에 on/off.
- **사용 이미지**: `gui_atlas` 아틀라스의 `exclamation_mark01` 서브스프라이트.
- **색상 팔레트**: `#FFE300` (황금 노랑) 또는 `#14FF00` (녹색) — 용도에 따라 선택 (예: 메인 퀘스트/서브 퀘스트 구분일 가능성, 정확 스펙은 각 기획서에서 확인).
- **애니메이션 (DOTween Punch/Rotation)**:
  - Duration: `1.5s`
  - Ease: `OutQuad`
  - Loops: `-1` (무한)
  - Loop Type: `Restart`
  - TO: `(x=0, y=0, z=15)` — Z축 회전 Punch
  - Vibrato: `10`
  - Elasticity: `0`
  - 해석: Z축으로 15도 흔들거림을 1.5초 주기로 계속 반복, Elasticity=0이라 튕김 없이 감쇠.

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `Grep` 으로 `NewTxt`, `AlchemyButtonNewTxt` 등 NEW 관련 식별자를 모두 찾아 어느 UI에 붙어 있는지 파악.
   - 대상 prefab(예: `Village1BtnGroup`, `UI_InventoryPopup`, `UI_CustomOrderPopup` 등)에서 NEW 오브젝트에 **`DOTweenAnimation` 컴포넌트가 이미 붙어 있는지 확인** (prefab yaml `m_Component`에서 DOTweenAnimation 찾기).
   - `Grep`으로 `exclamation_mark`, `ExclamationMark`, `QuestGuide`, `[!]` 관련 구현 존재 여부 확인. 현재는 `gui_atlas.psd`만 있고 코드 구현 흔적이 거의 없을 수 있음.
   - `Assets/IMPORTS/DarkFantasyGUI/GUI/Textures/gui_atlas.psd` 의 서브스프라이트에 `exclamation_mark01`이 있는지 스프라이트 slice 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] NEW 뱃지 재사용 가능한 **prefab (예: `UI_NewBadge.prefab`)** 이 존재하는가? 아니면 매 UI마다 중복 구성되어 있는가?
   - [ ] NEW 텍스트의 폰트가 `DOSSaemmul` / 색상 `#FFFF00` 으로 일관되게 설정되어 있는가?
   - [ ] NEW 오브젝트에 **Move Loop -1 / 0.5s / OutQuad / TO(1,2,0) Snapping** DOTweenAnimation이 설정되어 있는가?
   - [ ] NEW 표시/해제 로직이 있는가? (해당 버튼 클릭 또는 Popup 닫힘 시 off)
   - [ ] [!] 아이콘 공용 prefab/컴포넌트 (예: `UI_QuestGuideMark`) 가 존재하는가?
   - [ ] [!] 아이콘이 `gui_atlas.exclamation_mark01` 스프라이트를 참조하는가?
   - [ ] [!] 오브젝트에 **Punch/Rotation Loop -1 / 1.5s / OutQuad / TO(0,0,15) Vibrato=10 Elasticity=0** DOTweenAnimation이 설정되어 있는가?
   - [ ] [!] 표시/비표시를 퀘스트 번호에 따라 제어하는 시스템 (예: `QuestManager` API 또는 `Managers.Game.CurrentQuestId` 참조) 이 있는가?

3. **누락분만 구현**
   - NEW/[!] 모두 **재사용 가능한 공용 prefab**으로 만들고, 필요한 곳에서 `SetActive`만 토글하도록 구조화 (이미 공용으로 쓰이고 있으면 유지).
   - 애니메이션은 **DOTween 규약**에 따라 prefab의 `DOTweenAnimation` 컴포넌트로 구현 (코드 Sequence 금지). prefab에 DOTweenAnimation이 이미 있으면 그 설정이 스펙과 일치하는지만 확인 후 조정.
   - NEW 자동 해제 로직: 해당 버튼/Popup에 공통 헬퍼 메서드 (예: `UI_Base.ClearNewBadge(string key)`) 를 두고 PlayerPrefs 또는 `GameData`에 본 항목의 "확인함" 플래그 저장.
   - [!] 퀘스트 가이드는 현재 `QuestManager`가 없다면 최소한 `Managers.Game.CurrentQuestId` + 퀘스트별 타겟 매핑 정도의 스텁을 먼저 설계한 뒤 사용자에게 보고.

4. **결과 보고**
   - 점검 체크리스트 결과 + 수정/신규 파일 목록 + 변경된 prefab 경로를 간결히 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs` (`AlchemyButtonNewTxt`, `MonasteryButtonNewTxt`, `TravelButtonNewTxt`, `AlchemyCustomOrderButtonNewTxt`, `MonasteryCustomOrderButtonNewTxt`)
- `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`, `UI_ImportantSlot.cs`
- `Assets/@Scripts/Utils/Define.cs`
- `Assets/IMPORTS/DarkFantasyGUI/GUI/Textures/gui_atlas.psd` (exclamation_mark01)
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (퀘스트 상태 필드 확인용)
