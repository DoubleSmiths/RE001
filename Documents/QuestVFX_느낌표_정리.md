# 퀘스트 느낌표(QuestVFX) 사용처 정리 — 기획자용

> 작성일: 2026-07-05 · 대상 이미지: `exclamation_mark01`
> (원본: `Assets/IMPORTS/DarkFantasyGUI/GUI/Textures/gui_atlas.psd` 안의 서브 스프라이트)
> `Assets/@Resources/Prefab` 폴더의 **모든 프리팹을 전수 스캔**한 결과입니다.

---

## 1. 느낌표가 켜지는 원리 (한 장 요약)

```
[QuestNotify 시트]  +  [SubQuestNotify 시트(서브퀘 활성 시)]
        │  현재 진행 중인 퀘스트 ID의 행들만 골라서
        ▼
   BtnID 문자열 목록 생성
        │  아래 표의 "켜는 키"와 문자열이 완전히 같으면
        ▼
   해당 느낌표 오브젝트 ON  (없으면 OFF)
```

- **BtnID는 대소문자·철자까지 완전일치**해야 합니다. 한 글자라도 다르면 느낌표가 안 뜹니다.
- 키는 두 종류입니다.
  - **`..._VFX` 계열**: 느낌표 표시 **전용** 키. 시트에서 "이 버튼에 느낌표 띄워줘"라고 지정하는 용도.
  - **`BTN_...` 계열**: 버튼 **클릭 액션** ID. 퀘스트 완료 조건(CompleteTarget) 매칭에도 쓰이고, 일부 버튼은 이 키로도 느낌표가 켜집니다(아래 표에서 "또는"으로 표기).
- 시트(xlsx)를 고친 뒤에는 **파서(Tools 메뉴)를 실행해 JSON을 재생성**해야 게임에 반영됩니다. 게임은 JSON만 읽습니다.

---

## 2. 마을 화면 — `UI_Village1BtnGroup.prefab` (담당 스크립트: `UI_Village1BtnGroup`)

### 마을 메인 버튼들 (Village1MainBtnGroup)

| 느낌표가 붙는 버튼 | 느낌표 오브젝트 이름 | 켜는 키 (BtnID) |
|---|---|---|
| 길드 버튼 | `Village1GuildVFX` | `VILLAGE1_GUILD_VFX` 또는 `BTN_GUILD` |
| 연금공방 버튼 | `Village1AlchemyVFX_Sub1` | `VILLAGE1_ALCHEMY_VFX_SUB1` 또는 `BTN_ALCHEMY` |
| 수도원 버튼 | `MonasteryVFX_Sub1` | `MONASTERY_VFX_SUB1` |
| 여정 버튼 | `Village1TravelQuestVFX` | `VILLAGE1_TRAVEL_QUEST_VFX` 또는 `BTN_JOURNEY` |

### 길드 내부 (Village1GuildBtnGroup)

| 느낌표가 붙는 버튼 | 느낌표 오브젝트 이름 | 켜는 키 (BtnID) |
|---|---|---|
| 길드 NPC 대화 버튼 | `Village1GuildTalkVFX` | `VILLAGE1_GUILD_TALK_VFX` |
| 길드 [퀘스트] 버튼 | `Village1GuildQuestVFX` | `VILLAGE1_GUILD_QUEST_VFX` 또는 `BTN_GUILD_QUEST` |
| 길드 나가기 버튼 | `GuildExitQuestVFX` | `GUILD_EXIT_QUEST_VFX` |

### 연금공방 내부 (Village1AlchemyBtnGroup)

| 느낌표가 붙는 버튼 | 느낌표 오브젝트 이름 | 켜는 키 (BtnID) |
|---|---|---|
| 연금 NPC 대화 버튼 | `Village1AlchemyTalkVFX` | `VILLAGE1_ALCHEMY_TALK_VFX` |
| 연금 NPC 대화 버튼 (서브퀘용 2번째 느낌표) | `Village1AlchemyTalkVFX_Sub1` | `VILLAGE1_ALCHEMY_TALK_VFX_SUB1` 또는 `BTN_ALCHEMY_TALK` |
| 제작의뢰(CUSTOM ORDER) 버튼 | `Village1AlchemyQuestVFX` | `VILLAGE1_ALCHEMY_QUEST_VFX` |
| 연금공방 나가기 버튼 | `AlchemyExitQuestVFX` | `ALCHEMY_EXIT_QUEST_VFX` |

### 수도원 내부 (Village1MonasteryBtnGroup)

| 느낌표가 붙는 버튼 | 느낌표 오브젝트 이름 | 켜는 키 (BtnID) |
|---|---|---|
| 수도원 NPC 대화 버튼 | `Village1MonasteryTalkVFX` | `VILLAGE1_MONASTERY_TALK_VFX` |
| 수도원 NPC 대화 버튼 (서브퀘용 2번째 느낌표) | `Village1MonasteryTalkVFX_Sub1` | `VILLAGE1_MONASTERY_TALK_VFX_SUB1` |
| 클래스 버튼 | `Village1MonasteryQuestVFX` | `VILLAGE1_MONASTERY_QUEST_VFX` |
| 수도원 나가기 버튼 | `MonasteryExitQuestVFX` | `MONASTERY_EXIT_QUEST_VFX` |

> 이 매핑들은 코드 안의 **`s_notifyBindings` 표 한 곳**에 모여 있습니다 (`UI_Village1BtnGroup.cs`).
> 새 버튼에 느낌표를 달고 싶으면: ① 프리팹에 느낌표 오브젝트 추가 → ② 이 표에 한 줄 추가 → ③ 시트에서 BtnID 사용.
> 참고: 같은 시스템으로 켜지는 **"New" 텍스트** 3종(연금 버튼 `ALCHEMY_BUTTON_NEW_TXT`, 수도원 버튼 `MONASTERY_BUTTON_NEW_TXT`, 제작의뢰 `VILLAGE1_ALCHEMY_CUSTOM_ORDER_BUTTON_NEX_TXT` 등)도 있으나, 느낌표 이미지는 아니라서 이 문서의 표에서는 제외했습니다.

---

## 3. 여정(이동) 팝업 — `UI_TravelPopup.prefab` (담당 스크립트: `UI_TravelPopup`)

| 느낌표가 붙는 버튼 | 느낌표 오브젝트 이름 | 켜는 키 (BtnID) |
|---|---|---|
| 탐험 카테고리 버튼 | `FieldCategoryButtonQuestVFX` | `BTN_EXPLORE` |
| 마을 카테고리 버튼 | `TownCategoryButtonQuestVFX` | `BTN_TOWN` |

### 목적지 선택 버튼 (동적 생성) — 여기만 규칙이 다릅니다!

| 프리팹 | 담당 스크립트 | 느낌표 오브젝트 이름 | 켜는 키 |
|---|---|---|---|
| `UI_TownSelectButton.prefab` | `UI_TownSelectButton` | `TownSelectButtonQuestVFX` | **TravelNode 시트**의 해당 노드 `ClickBtnId` 또는 `EnterBtnId` |
| `UI_FieldSelectButton.prefab` | `UI_FieldSelectButton` | `FieldSelectButtonQuestVFX` | 위와 동일 |
| `UI_MapSelectButton.prefab` | `UI_MapSelectButton` | `MapSelectButtonQuestVFX` | 위와 동일 |

> 목적지 버튼들은 고정 키가 아니라 **TravelNode 시트의 `ClickBtnId` / `EnterBtnId` 열 값**이 곧 키입니다.
> 예: 산들바람 숲 노드의 ClickBtnId가 `BTN_MAP_BREEZING_WIND_FOREST`라면, QuestNotify 시트에 `BTN_MAP_BREEZING_WIND_FOREST`를 넣으면 그 노드 버튼에 느낌표가 뜹니다.
> (현재 Define에 등록된 예: `BTN_MAP_BREEZING_WIND_FOREST`, `BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE`)

---

## 4. 메인 게임 화면 — `UI_GameScene.prefab` (담당 스크립트: `UI_GameScene`)

| 느낌표가 붙는 버튼 | 느낌표 오브젝트 이름 | 켜는 키 (BtnID) |
|---|---|---|
| 나가기 버튼 | `ExitQuestVFX` | `EXIT_QUEST_VFX` 또는 `BTN_EXIT` |
| 전투 중 여정 버튼 | `TravelButtonOnBattleQuestVFX` | `TRAVEL_BUTTON_ON_BATTLE_QUEST_VFX` 또는 `BTN_JOURNEY` |

> 2026-07-05 연결 완료: 프리팹 오브젝트 이름의 괄호를 제거해 코드와 통일했고(`TravelButton(OnBattle)QuestVFX` → `TravelButtonOnBattleQuestVFX`), 퀘스트 notify 변경 시 자동 갱신되도록 코드를 연결했습니다.
> 참고: `BTN_JOURNEY`를 쓰면 마을 여정 버튼과 전투 중 여정 버튼에 **동시에** 느낌표가 뜹니다. 전투 중 버튼에만 띄우려면 `TRAVEL_BUTTON_ON_BATTLE_QUEST_VFX`를 쓰세요. 나가기도 같은 원리입니다(`BTN_EXIT`는 클릭 액션 겸용, `EXIT_QUEST_VFX`는 느낌표 전용).

---

## 5. 그 외 (참고)

| 항목 | 상태 |
|---|---|
| `UI/Legecy/UI_BattleBtnGroup_Legecy.prefab` | 레거시(구버전) 프리팹. 느낌표 이미지 4개가 들어있지만 현재 사용 안 함 |
| `UI_BattleBtnGroup` (현행 전투 버튼 그룹) | 코드에 느낌표 갱신 함수 자리(`RefreshNotify`)는 있으나 **비어 있음** — 현행 프리팹에 느낌표 오브젝트 없음 |
| `UI_FieldTypeButton` | 코드에 느낌표 이름(`FieldTypeButtonQuestVFX`)만 예약되어 있고 프리팹·로직 모두 미구현 (TODO 상태) |
| 도감/잡기 팝업의 미니몬 느낌표 이모지 | `Emoj_Exclamation` **애니메이션**으로, `exclamation_mark01` 이미지와는 별개 시스템 (퀘스트 느낌표 아님) |

---

## 6. 시트 작업 시 체크리스트

1. QuestNotify(또는 SubQuestNotify) 시트의 **BtnID 열**에 위 표의 키를 **철자 그대로** 입력.
2. 목적지 버튼 느낌표는 TravelNode 시트의 `ClickBtnId`/`EnterBtnId` 값과 맞출 것.
3. 시트 저장 후 **Unity 메뉴의 파서(Tools → ParseExcel 계열) 실행** → JSON 재생성 확인.
4. 느낌표가 안 뜨면: ① BtnID 철자 확인 → ② 파서 실행했는지 확인 → ③ 위 표의 "미연결" 항목인지 확인.
