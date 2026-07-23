<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# Editor

## Purpose
UnityEditor 전용 툴 디렉터리 (빌드에 미포함, 다수가 `#if UNITY_EDITOR` 가드). 핵심은 `DataTransformer` — 로컬 xlsx 또는 구글시트를 ExcelDataReader로 읽어 `Assets/@Resources/Data/JsonData/*.json`을 생성하는 시트→JSON 파이프라인이다(게임은 JSON만 읽음). 나머지는 Addressables 등록기, TMP/프리팹 일괄 변환기, 그리고 PlayMode 중 기획서 슬라이드별 UI/시스템을 검증하는 Test 메뉴(Battle/Bestiary/Popup/QuestVerify/Integration)와 여정 해금 연출 자동 녹화/이펙트 진단 도구(Record/FX)이다.

## Key Files
| File | Description |
|------|-------------|
| `DataTransformer.cs` | 시트→JSON 파서 (EditorWindow). 로컬 xlsx와 구글시트(GSHEET_* export URL) 양대 경로. `Tools/ParseExcel %#K`(로컬 7종), `Tools/ParseExcelFromGoogleSheets`, `Tools/ParseExtraData (스킬/장비/미니몬/물약/콤보)`(로컬 8종), `Tools/ParseExtraDataFromGoogleSheets`, `Tools/GenerateLocalizeKeys`(Define.cs의 LocalizeKey region 덮어쓰기), `Tools/DeleteGameData `(PlayerPrefs+SaveData.json 삭제) |
| `AddressableSpriteAddressFixer.cs` | `Tools/Addressables/Fix Sprite Addresses` — "Sprites" 그룹 엔트리 주소에 `.sprite` 접미사 일괄 부여 |
| `SoundAddressableRegistrar.cs` | `Tools/Register Sound Addressables` — `Assets/@Resources/Sound` 하위 AudioClip 전부를 주소=파일명 + "PreLoad" 라벨로 등록 |
| `EquipPrefabAddressableRegistrar.cs` | `Tools/Register Equipment Popup Addressables` — 장비 팝업 프리팹 3종(UI_EquipmentPopup/UI_EquipDetailPopup/UI_ForgePopup)을 UI 그룹+PreLoad 라벨 등록 후 `BuildPlayerContent()`로 카탈로그 재빌드 |
| `BattleTestMenu.cs` | `Test/Battle/*` — 데미지 폰트 5모드, BattleMath 100회 시뮬, Lv 리셋/강제 레벨업, `Toggle Force Player Critical %#c`(Ctrl+Shift+C) |
| `BestiaryTestMenu.cs` | `Test/Bestiary/*` — 도감/미니몬 검증. 기록도·별등급·공명도·AllSS 시드/사이클(PlayerPrefs 스텁), TamePopup·관심창고 테스트. 대상 몬스터 2001/2002 |
| `IntegrationTestMenu.cs` | `Test/Integration/*` — `Diagnose - 전체 진단 %#d`(매니저/데이터 사전/플레이어/능력/미니몬/퀘스트/콤보 종합), `Diagnose - 데이터 무결성`(MinMon↔Creature, 고유기술=creatureId+4000 매핑 등), 능력 학습/망각, 재화 치트, 세이브 저장/삭제 |
| `PopupTestMenu.cs` | `Test/*` 단축키 팝업 — CustomOrder `%#1`, PotionDetail `%#2~4`, PotionPopup `%#6`, 자동물약 해금 토글 `%#7`, 스피드버프 `%#8`, 행운물약 해금 `%#9`, 연금공방 이동 `%#0` (reflection으로 private OnClick 호출) |
| `QuestVerifyMenu.cs` | `Test/Quest Verify/*` — `A. Reset & Re-init %#r`(세이브 삭제+ISFIRST), `B. Show All & Seed %#u`(전 컨텐츠 해금/시드), part1/part2 슬라이드별 시연 메뉴 다수, `Jump/Battle %#b`·`Village1 %#v`·Quest 점프 |
| `UnlockAnimRecorder.cs` | `Test/Record/여정 해금 연출 녹화 (Documents 저장)` — 클릭 한 번으로 플레이 진입→GameScene 준비 대기(최대 30초)→Unity Recorder(MP4, 30fps, 게임뷰) 시작→여정 해금 트리거(`Managers.Quest.RaiseBtnUnlocked(UNLOCK_TRAVEL_BUTTON)`)→연출/팝업 촬영→팝업 닫기([!]+NEW 확인 구간)→`Documents/UnlockAnim_*.mp4` 저장→플레이 자동 종료. 미플레이 시 SessionState pending + playModeStateChanged 훅으로 진입 후 자동 재개 (async Task 기반) |
| `UnlockFxDebugMenu.cs` | `Test/FX/여정 버튼 이펙트 진단 (10초 유지)` — 실제 연출과 동일한 `UIFx.PlayParticle` 경로로 Star_01_Anim_13_a/GlowDot_01을 여정 버튼(TravelButton)에 10초간 스폰하는 진단 도구. raw 스폰용 `Spawn()` 헬퍼는 렌더러 바운즈/카메라 프러스텀 포함 여부를 Console에 출력 |
| `PrefabEnumGenerator.cs` | `Tools/Prefab Enum Generator` (EditorWindow) — 프리팹 자식을 Button/Image/Text/GameObject로 분류해 UI_Base 바인딩용 `enum Images/Texts/Buttons/GameObjects` 텍스트 생성·저장 |
| `PrefabChildRenamer.cs` | `Tools/Prefab Child Renamer` (EditorWindow) — 프리팹 자식 이름 정리(공백/괄호 제거, 특수문자→`_`), 미리보기 후 적용 |
| `TextToTMPConverter.cs` | `Tools/Convert Text to TMP in UI_GameScene`, `Tools/Convert Selected to TMP` + 역변환 `Tools/Convert TMP to Text in UI_GameScene`, `Tools/Convert Selected TMP to Text` — 정렬/폰트스타일/AutoSize 속성 보존 변환 |
| `TMPFontReplacer.cs` | `Tools/TMP/Replace Missing TMP Fonts In Selected Prefabs` (EditorWindow) — 선택 프리팹들의 font==null인 TMP_Text에 지정 폰트 일괄 주입 |

## For AI Agents

### Working In This Directory

**DataTransformer가 읽고 쓰는 것 (가장 중요):**
- 입력: `Assets/@Resources/Data/Excel/*.xlsx` (로컬) 또는 GSHEET_* 상수의 구글시트 export URL (`/export?format=xlsx`, "링크 있는 사용자-뷰어" 공개 필수). ExcelDataReader의 AsDataSet(UseHeaderRow=false)으로 raw 행 파싱.
- 출력: `Assets/@Resources/Data/JsonData/{이름}.json` — Newtonsoft.Json `Formatting.Indented`. 기본 7종(ParseExcel): PlayerStatusData, DialogueScriptData, LocalizationData, ItemData 계열(InventoryItem/RequisiteItem/Equipment/HPPotion/SpeedPotion 시트별), Quest 7파일(QuestData/QuestConditionData/QuestNotifyData/QuestUnlockData/SubQuestMainData/SubQuestNotifyData/SubQuestUnlockData), CreatureData, TravelNodeData+TravelConditionData. 추가 8종(ParseExtraData): SkillData, EquipmentData, EquipEnhanceData, MinMonData, HPPotion, SpeedPotion, ComboData, BuffData — 이쪽은 `WriteJsonStr`(StringEnumConverter)로 **enum을 문자열 직렬화**.
- 행 포맷 규약: 마스터데이터류는 Row0=필드명/Row1=타입/Row2=한글설명, **Row3부터 데이터** (`y=3`). Dialogue/Localization/Quest/Item/Travel류는 Row0=헤더, Row1부터 (`y=1`). 첫 컬럼 빈 행 스킵, `_`로 시작하는 시트 무시, `"NONE"` → 빈 값/빈 리스트 정규화, 리스트 셀은 콤마 구분.
- 파싱은 **컬럼 인덱스 순서 기반**(`row[i++]` / `Next()` / `C(row, n)`) — 시트 컬럼 순서 변경은 파괴적. 신규 컬럼은 항상 뒤에 추가하고 범위 밖이면 기본값 유지 패턴으로 하위호환 유지.
- **GS_ParseCreatureData(구글시트 경로)는 2026-07 시트 개편에 맞춰 27컬럼 고정 순서로 재작성됨**: DataId | CreatureID(도감 번호, 대응 필드 없어 스킵) | Name | PrefabLabel | IconLabel | MaxHp | PhysicalAtk | Def | TotalExp | RewardGold | DropItemId | DropCount | DropChance | MagicAtk | CritRate | CritDamage | Accuracy | AtkSpeed | PhysicalResist | MagicResist | Evasion | AttackDelay | **GoldDropChance(W)** | **DiaDropChance(X)** | **DiaDropCount(Y)** | Grade(→Rarity) | MaxKillCount. 개편 시트에서 MoveSpeed/RecoveryRate 컬럼 삭제(기본값 0 유지). 0/빈칸은 기본값 유지(CritDamage 150, Accuracy 80, AtkSpeed 100, AttackDelay→MonsterAttackDelay 1.5, Rarity "Common", MaxKillCount 100). **28열부터 선택 컬럼**(시트에 헤더를 이 순서로 추가하면 그대로 파싱): Habitat | EncounterRate | InterestRate | AnimController | SpawnY | DamageFontYOffset | UniqueSkillName | UniqueSkillDesc | AttackSfx | OnHitSfx | DieSfx. 로컬 xlsx 경로(`ParseCreatureData`)는 구 포맷 그대로라 **두 경로의 컬럼 순서가 다르다.**
- `ParseMinMonDataSheet`: 선택 컬럼 O/P(15~16번째, `C(row,14)/C(row,15)`) = FollowOffsetX/Y — 미니몬 동행 위치 오프셋(기획서2 슬19), 시트에 없으면 0(현행 위치).
- **파서 재실행 전 시트 선행 조건**: GS_ParseCreatureData는 개편된 27컬럼 시트를 전제한다. 재실행 전에 구글시트에 ① 고블린(2003) 행 ② (필요 시) 28열~ 선택 컬럼 헤더 ③ Localization 시트 신규 키 18종이 반영돼 있어야 한다. **구 포맷 시트에 그대로 재실행하면 컬럼이 밀려 CreatureData.json이 손상된다.**
- `Tools/GenerateLocalizeKeys`는 `Assets/@Scripts/Utils/Define.cs`의 `#region LocalizeKey` 블록을 찾아 **파일을 직접 재작성**한다 — Define.cs의 해당 region 수동 편집 금지.

**Test 메뉴 공통 규칙:**
- 거의 모든 항목이 `Application.isPlaying` 가드 — PlayMode 진입 후 실행 (일부는 미진입 시 자동으로 `EditorApplication.isPlaying = true`).
- 단축키 표기: `%`=Ctrl, `#`=Shift (예: `%#u` = Ctrl+Shift+U).
- QuestVerifyMenu의 `B. Show All & Seed`가 수동 QA의 표준 진입점 (재화/탭해금/인벤/능력/미니몬/물약/퀘스트 점프 일괄 시드).
- 팝업 진입 애님 완료 후 후속 팝업을 열 때 `EditorApplication.delayCall` 2중 중첩 패턴 사용.
- 일부 메뉴가 reflection으로 private 메서드(`UI_Village1BtnGroup.OnClickMain/OnClickGuild/OnClickAlchemy`) 호출 — 해당 메서드명 변경 시 테스트 메뉴가 조용히 깨진다.
- 새 Editor 파일은 UnityEditor를 참조하므로 이 폴더(Editor 어셈블리) 안에만 두고, 런타임 코드에서 참조하지 말 것. Addressables 등록은 unity-cli exec에서 Editor 어셈블리 접근 불가라 MenuItem으로 만든 관례가 있다 (SoundAddressableRegistrar/EquipPrefabAddressableRegistrar 주석).

### 스펙 출처 (기획서 매핑)
게임 스펙의 원전은 구글 슬라이드 기획서 2종 — **기획서1**(70장: UI/연출/전투), **기획서2**(54장: 도감/미니몬/능력) + 수정안. 이 폴더의 `QuestVerifyMenu.cs`가 슬라이드별 시연 메뉴로 기획서를 미러링한다.

| 파일 | 기획서 슬라이드 |
|------|----------------|
| `DataTransformer.cs` | 기획서1 슬55 (몬스터 데이터 시트 스키마) |

### Common Patterns
- `[MenuItem("경로", false, priority)]` + 검증용 `[MenuItem("경로", true)]` 쌍 (BattleTestMenu 크리 토글, TextToTMPConverter Selected).
- `RequirePlayMode()` / `Require()` 스타일 사전 가드 후 `Managers.*` 직접 조작.
- Addressables 등록: `AddressableAssetSettingsDefaultObject.Settings` → `CreateOrMoveEntry(guid, group)` → `entry.address` 지정 → `SetLabel("PreLoad", true, true)` → `SaveAssets()`.
- 프리팹 일괄 수정: `PrefabUtility.LoadPrefabContents(path)` → 수정 → `SaveAsPrefabAsset` → `UnloadPrefabContents`.
- `ConvertValue<T>(string)` — TypeDescriptor 기반 범용 문자열→타입 변환 (enum 포함).

## Dependencies

### Internal
- `Data` 네임스페이스 (Data.Contents.cs의 모든 Loader) — DataTransformer/IntegrationTestMenu/QuestVerifyMenu
- `Define` (enum, NotifyKey/UnlockKey, ID 대역 상수) — `using static Define`
- `Managers` (Data/Game/UI/Object/MinMon/Ability/Quest/Combo), `BattleMath`, `CreatureController`, `UI_*` 팝업 클래스들, `UIFx` (UnlockFxDebugMenu)

### External
- UnityEditor (MenuItem, EditorWindow, PrefabUtility, Selection, EditorSceneManager, EditorUtility)
- UnityEditor.AddressableAssets (AddressableAssetSettings, Settings/Entry API)
- UnityEditor.Recorder (RecorderController, MovieRecorderSettings, CoreEncoderSettings, GameViewInputSettings) — UnlockAnimRecorder
- ExcelDataReader (+ System.Data, CodePagesEncodingProvider 등록 필수)
- Newtonsoft.Json (JsonConvert, StringEnumConverter)
- System.Net.WebClient (구글시트 xlsx 다운로드)
- TMPro / UnityEngine.UI (변환기류)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
