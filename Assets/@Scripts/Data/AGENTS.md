<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Data

## Purpose
`Data` 네임스페이스 아래 모든 마스터데이터 직렬화 클래스와 `ILoader<Key, Value>` 구현체를 담는 디렉터리. Editor/DataTransformer.cs가 Excel/구글시트를 파싱해 `Assets/@Resources/Data/JsonData/*.json`으로 내보낼 때의 스키마이자, 런타임에 DataManager가 Newtonsoft.Json으로 역직렬화한 뒤 `MakeDict()`로 O(1) 딕셔너리 캐시를 만들 때의 소스다. 파일은 `Data.Contents.cs` 하나뿐이며 모든 데이터 클래스가 `#region` 단위로 정리되어 있다.

## Key Files
| File | Description |
|------|-------------|
| `Data.Contents.cs` | 전 마스터데이터 클래스 + Loader 정의 (약 840줄, region별 구분). `using static Define`으로 Define의 enum(EquipGrade, SkillCategory, QuestCompleteType 등)을 필드 타입으로 직접 사용 |

## For AI Agents

### Working In This Directory

**데이터 클래스 ↔ Loader 목록** (region 순서대로):

| 데이터 클래스 | Loader | 딕셔너리 키 | 비고 |
|---|---|---|---|
| `CreatureData` | `CreatureDataLoader` | `DataId` (int) | 몬스터 스탯/드롭/SFX/AnimController/Habitat/미니몬 고유기술 필드 포함 |
| `EquipmentData` | `EquipmentDataLoader` | `DataId` | EquipSlotType/EquipGrade enum 필드 |
| `EquipEnhanceData` | `EquipEnhanceDataLoader` | `DataId` | 장비 강화 레벨별 보너스/재료 |
| `SkillData` | `SkillDataLoader` | `DataId` | 능력(Ability) 시스템 — 발동조건(AbilityTrigger/Condition/Context/Effect), 출처(AbilitySource), MemoryCapacity(0=재능) |
| `ItemData` | `ItemDataLoader` | `DataId` | InventoryItem/RequisiteItem/Equipment 시트 공용 스키마 |
| `HPPotionData` | `HPPotionDataLoader` | `DataId` | Step 체인 + NeededItemIDs/Amounts 병렬 리스트 |
| `SpeedPotionData` | `SpeedPotionDataLoader` | `DataId` | AtkSpeedBonus/Duration |
| `BuffData` | `BuffDataLoader` | `DataId` | 좌상단 버프 아이콘(UI_Buff) 카탈로그, Timed 여부 |
| `AreaData` | `AreaDataLoader` | `DataId` | (TravelNode로 대체 추세 — IntegrationTestMenu 주석 참고) |
| `TravelNodeData` | `TravelNodeDataLoader` | `Id` (int, DataId 아님 주의) | 여정 트리 노드 (Category "TOWN"/"EXPLORE") |
| `TravelConditionData` | `TravelConditionDataLoader` | `NodeId` → `List<TravelConditionData>` | 1:N 그룹핑 로더 |
| `MinMonData` | `MinMonDataLoader` | `DataId` | 미니몬 — MonsterDataId로 CreatureData 참조, MinMonGrade enum |
| `ForgeRecipeData` | `ForgeRecipeDataLoader` | `DataId` | 대장간 레시피 |
| `CraftRecipeData` (+`CraftMaterial`) | `CraftRecipeDataLoader` | `DataId` | 연금공방 제작 (CraftCategory: Potion/Skill) |
| `LevelData` | `LevelDataLoader` | `Level` (int, DataId 아님 주의) | 레벨별 요구 경험치 |
| `PlayerStatusData` | `PlayerStatusDataLoader` | `DataId` | 플레이어 초기 스탯 + StartSkills/StartSkillLevels 병렬 리스트 |
| `DialogueScriptGroup` (+`DialogueScriptLine`) | `DialogueScriptLoader` | `GroupKey` (**string** = 시트명) | 대사 그룹 |
| `LocalizationEntry` | `LocalizationLoader` | `TextKey` (**string**) | 한/영 텍스트, 중복 키는 첫 항목 유지 |
| `QuestData` | `QuestDataLoader` | `QuestID` | 완료조건/Follow UI/애니메이션/대사 연결 |
| `QuestConditionData` | `QuestConditionLoader` | `QuestID` → List (Order 정렬) | 1:N |
| `QuestNotifyData` | `QuestNotifyLoader` | `QuestID` → List (Order 정렬) | 느낌표(VFX) 유도 BtnID |
| `QuestUnlockData` | `QuestUnlockLoader` | `QuestID` → List | UI_BUTTON/ITEM_REWARD 등 |
| `SubQuestData` / `SubQuestNotifyData` / `SubQuestUnlockData` | 각 Loader | `SubQuestID` → 단일 or List | 서브퀘스트 3종 |
| `ComboStageData` | `ComboStageDataLoader` | `Level` (0~6) | 콤보 Rank별 타이머/버프/색상 |

**규칙:**
- 새 데이터 = `[Serializable]` 클래스 + `ILoader<Key, Value>` 구현 Loader 한 쌍을 이 파일에 추가. 기본 키는 `DataId`(int)지만 TravelNode(`Id`), LevelData(`Level`), Dialogue/Localization(string 키), 1:N 그룹 로더(QuestCondition 등) 예외 존재.
- 필드 추가 시 **반드시 Editor/DataTransformer.cs의 해당 Parse 함수도 같이 수정**해야 JSON에 반영된다 (컬럼 인덱스 기반 파싱). 게임은 JSON만 읽으므로 시트 수정 후 Tools 메뉴 파서 수동 실행 필요.
- 필드 기본값(예: `CritDamage = 150f`, `Accuracy = 80f`, `AttackSfx = "Slime_Attack"`)은 시트 컬럼이 비어있을 때의 fallback 역할 — 임의로 바꾸면 기존 JSON 로드 결과가 달라진다.
- `Name`/`Description`/`Story`/`NameKey` 류 string 필드 다수가 LocalizationData의 TextKey를 담는다(표시 시 `Util.Localize()` 통과). 실제 표시 문자열을 직접 넣지 말 것.
- enum 타입 필드는 모두 `Utils/Define.cs`에 정의 — 새 enum 값이 필요하면 Define.cs 수정.

### Common Patterns
- `ILoader<Key, Value>.MakeDict()` — List 필드를 순회해 Dictionary 생성. 단일 키는 `dict.Add(item.DataId, item)`, 1:N은 `Dictionary<int, List<T>>`로 그룹핑 후 `Order` 정렬.
- 병렬 리스트 패턴 — `NeededItemIDs`/`NeededItemAmounts`, `StartSkills`/`StartSkillLevels`처럼 인덱스로 짝을 맞추는 두 리스트.
- "int를 string으로 담는" 필드 — `QuestData.CompleteTarget`, `FollowItemID` 등은 용도에 따라 DataId 문자열/버튼ID 문자열을 겸용 (주석 참조).

## Dependencies

### Internal
- `Define` (Utils/Define.cs) — `using static Define`으로 enum 직접 참조
- `ILoader<Key, Value>` 인터페이스 (Managers/DataManager 쪽 정의)
- 소비자: `DataManager`(로드/캐시), `Editor/DataTransformer`(생성)

### External
- `System` / `System.Collections.Generic`만 사용 (파일 자체는 UnityEngine 미참조; 직렬화는 DataManager/DataTransformer의 Newtonsoft.Json이 담당)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
