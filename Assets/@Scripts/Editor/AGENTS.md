<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Editor

## Purpose
UnityEditor 전용 툴 디렉터리 (빌드에 미포함, 다수가 `#if UNITY_EDITOR` 가드). 핵심은 `DataTransformer` — 로컬 xlsx 또는 구글시트를 ExcelDataReader로 읽어 `Assets/@Resources/Data/JsonData/*.json`을 생성하는 시트→JSON 파이프라인이다(게임은 JSON만 읽음). 나머지는 Addressables 등록기, TMP/프리팹 일괄 변환기, 그리고 PlayMode 중 기획서 슬라이드별 UI/시스템을 검증하는 Test 메뉴 4종(Battle/Bestiary/Popup/QuestVerify/Integration)이다.

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
- 파싱은 **컬럼 인덱스 순서 기반**(`row[i++]` 또는 `C(row, n)`) — 시트 컬럼 순서 변경은 파괴적. 신규 컬럼은 항상 뒤에 추가하고 `i < row.Count ? ... : 기본값` 패턴으로 하위호환 유지.
- `Tools/GenerateLocalizeKeys`는 `Assets/@Scripts/Utils/Define.cs`의 `#region LocalizeKey` 블록을 찾아 **파일을 직접 재작성**한다 — Define.cs의 해당 region 수동 편집 금지.

**Test 메뉴 공통 규칙:**
- 거의 모든 항목이 `Application.isPlaying` 가드 — PlayMode 진입 후 실행 (일부는 미진입 시 자동으로 `EditorApplication.isPlaying = true`).
- 단축키 표기: `%`=Ctrl, `#`=Shift (예: `%#u` = Ctrl+Shift+U).
- QuestVerifyMenu의 `B. Show All & Seed`가 수동 QA의 표준 진입점 (재화/탭해금/인벤/능력/미니몬/물약/퀘스트 점프 일괄 시드).
- 팝업 진입 애님 완료 후 후속 팝업을 열 때 `EditorApplication.delayCall` 2중 중첩 패턴 사용.
- 일부 메뉴가 reflection으로 private 메서드(`UI_Village1BtnGroup.OnClickMain/OnClickGuild/OnClickAlchemy`) 호출 — 해당 메서드명 변경 시 테스트 메뉴가 조용히 깨진다.
- 새 Editor 파일은 UnityEditor를 참조하므로 이 폴더(Editor 어셈블리) 안에만 두고, 런타임 코드에서 참조하지 말 것. Addressables 등록은 unity-cli exec에서 Editor 어셈블리 접근 불가라 MenuItem으로 만든 관례가 있다 (SoundAddressableRegistrar/EquipPrefabAddressableRegistrar 주석).

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
- `Managers` (Data/Game/UI/Object/MinMon/Ability/Quest/Combo), `BattleMath`, `CreatureController`, `UI_*` 팝업 클래스들

### External
- UnityEditor (MenuItem, EditorWindow, PrefabUtility, Selection, EditorSceneManager, EditorUtility)
- UnityEditor.AddressableAssets (AddressableAssetSettings, Settings/Entry API)
- ExcelDataReader (+ System.Data, CodePagesEncodingProvider 등록 필수)
- Newtonsoft.Json (JsonConvert, StringEnumConverter)
- System.Net.WebClient (구글시트 xlsx 다운로드)
- TMPro / UnityEngine.UI (변환기류)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
