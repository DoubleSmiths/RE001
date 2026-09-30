<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# Managers/Contents

## Purpose
RE001 게임 로직 전용 매니저 계층. 계정 세이브데이터(GameData)의 단일 소스인 GameManager를 중심으로, 메인/서브 퀘스트 상태머신(Quest/SubQuestManager), 미니몬 포획·등급·공명도(MinMonManager), 능력(스킬) 학습·기억 장착(AbilityManager), 콤보 Rank/버프(ComboManager), 전투 오브젝트·FX 스폰(ObjectManager)을 담당한다. 대부분의 상태 변경은 GameData에 기록 후 `Managers.Game.SaveGame()`으로 즉시 저장하고 `event Action`으로 UI에 통지하는 구조다.

## Key Files
| File | Description |
|------|-------------|
| `GameManager.cs` | 세이브 단일 소스. `GameData`(재화/장비/인벤토리/미니몬/능력/퀘스트ID/탭 해금 비트마스크/자동물약 설정 등) + 런타임 클래스 `InventoryItem`/`Equipment`/`MinMonSaveData` 정의. property setter가 변경 즉시 `SaveGame()`(persistentDataPath/SaveData.json, Newtonsoft) + 이벤트 발행. `AddItem`/`RemoveItem`/`HasItem`, `EquipItem`/`ForgeEquipment`, HP·SPD 물약 제작(`TryCraftHPPotion` 등), `LoadGame()`은 PlayerPrefs "ISFIRST"로 초기화 여부 판단. 언어 설정만 PlayerPrefs 저장 v2(ADR0030): `BalanceConfig`는 `Managers.Data.Balance(key)`. 훈련(`TrainingLevels`, `TryTrain`/`ApplyTraining`), 스테이지·보스(`StageNodeId`·`StageKillCount`·`ClearedStages`·`BossAutoChallenge`, `AddStageKill`/`ClearStage`/`FailStage`/`TryGetStageNumber`/`NextStage`, 보스전 상태 `SetBossFight`, 이벤트 `OnStageChanged`), 가드 도움말 `GuardHintSeen`. 전투 구역 여부 `InBattleArea`(SwapAreaBtnGroup이 기록). |
| `QuestManager.cs` | 메인퀘스트 선형 진행(CurrentQuestID, 완료 시 +1). CompleteType별 체크 진입점 `CheckItemAcquire`/`CheckButtonClick`/`CheckAnimationEnd`, MULTI 조건은 `_metConditions` HashSet(세이브 동기화로 중단 복원), ITEM_COUNT 롤백 지원. 완료 시 `ProcessUnlocks`(UI_BUTTON 해금/SUBQUEST_START/ITEM_REWARD/ITEM_REMOVE — ITEM_REWARD는 `UI_ItemPopup.SetInfo(itemId, showGlow: true)`로 후광 표시, SubQuestManager도 동일). `GetCurrentNotifyList()`는 서브퀘스트 Notify를 합쳐 반환. 이벤트: OnQuestChanged/OnFollowUIChanged/OnNotifyChanged/OnBtnUnlocked |
| `SubQuestManager.cs` | 메인퀘스트에서 분기되는 서브퀘스트 상태머신(CurrentSubQuestID, 0=비활성). `StartSubQuestFromTarget("SUBQUEST_N")` 파싱 시작, BTN_CLICK/LEVEL_COUNT("HPPotion_Rank:N") 완료 조건, 다음 ID 없으면 라인 종료. 시작 직후 자동완료 검사를 의도적으로 생략(Notify UI 1프레임 미표시 버그 방지 주석 참고) |
| `MinMonManager.cs` | 미니몬(펫) 시스템. `TryCatch`(확률+천장 TameCount 확정 포획, Ability 보너스 반영), `AddMinMon`(첫 포획 등급 롤 `RollGrade` 40/30/15/8/5/2%, 재포획은 상향 갱신), 공명도(`GetSyncRate` = 포획 1회당 25%), 기록도(`AddKill`/`GetRecordRate`), 별 3단계(`GetStarGrade`) 및 보라별 최초 달성 트리거(`OnMaxStarReached`), 관심 창고(`RollInterest`, 기본 2칸+능력 보너스), 동행 설정(`SetCompanion`), 공명도 100% 시 `Managers.Ability.LearnMinionUniqueSkill` 호출 |
| `MissionManager.cs` | v2 미션·출석(ADR0030). 표 `MissionData`(`_tools/gen_missions.py`): 일일(현지 00시 초기화) 6+완료 상자, 주간(월요일 00시) 5, 가이드 40단계 사다리(받지 않은 첫 단계만), 7일 누적 출석. `Notify(MissionEvent, n)`을 처치(MonsterController·방치 TickIdle)·보스/새 스테이지(ClearStage)·훈련·강화 시도·포획 시도·패링에서 부른다. 레벨·도감·돌파 수는 상태에서 계산. 세이브 `GameData.Missions`(MissionSaveData). `HasClaimable`이 HUD 알림 점 성장 패스(ADR0039): Kind `Pass` — `PassProduct`(인앱 Kind `Pass`)·`PassPremium`·`CanClaimPass`·`HasPassClaimable`, 받기는 `TryClaim` 이 무료 줄 + (샀으면) 프리미엄 줄을 한 번에(`PremiumClaimed`). 가이드·패스·업적은 누적·상태로 판정. |
| `BondManager.cs` | v2 마을 사람들(ADR0036, `Managers.Bond`). NPC 5명(`Npc` — 이름 키 `Npc*NameText`), 호감도 점수 → 단계(`BOND_POINTS_PER_LEVEL`, 최대 `BOND_MAX_LEVEL`). 하루 배달 의뢰(`Roll` — 날짜 씨앗, 사냥하던 스테이지 기준 재료 배달/소포 전달), `OnKill(노드)`(MonsterController), `TryComplete`, `TryGift`(가장 많은 몬스터 재료). 혜택: `EnhanceCostMult`(GetEnhanceCost), `ShopPriceMult`(GameManager.ShopPrice), `ExtraProficiencyChance`(처치 숙련도), `ApplyTo`(RefreshStats — 회복력). 세이브 `GameData.Bond` |
| `DungeonManager.cs` | v2 요일 던전(ADR0034, `Managers.Dungeon`). 골드·경험치·재료(`Define.DungeonKind`), 종류별 하루 무료 `DUNGEON_FREE_PER_DAY` + 광고(`ShopManager.WatchAd`, 자리 `dungeon_*`) + 크리스탈(`SpendCrystal`). `TryEnter` 가 값을 치르고 입장을 **예약**(`CurrentTravelAreaId` = 사냥하던 스테이지)하면 새 전투 그룹의 `StartBattle` 이 `Begin()` 으로 시작 → `UI_BattleBtnGroup.CoDungeon` 이 `DUNGEON_TIME` 동안 스폰·`TimeLeft` 감소, 처치 `AddKill`. `Finish()`(시간 초과·쓰러짐·귀환·이동, 한 번만)가 처치 수 × 지역 평균 × 배수(오늘의 던전 ×`DUNGEON_TODAY_MULT`)로 골드/경험치, 재료는 잡은 몬스터 드롭 × `DUNGEON_MAT_MULT` + 만드라고라(`DUNGEON_MANDRAKE_KILLS`마다). ⚠️ `Finish` 는 예약을 지우지 않는다 — 전투→던전 전환에서 옛 그룹의 EndBattle 이 늦게 온다. 세이브 `GameData.Dungeon`(무료 사용 수·최고 기록) |
| `RelicManager.cs` | v2 유물 소환(ADR0035, `Managers.Relic`). 표 `RelicData`(`_tools/gen_relics.py`, 30종). `Summon(1|10, Pay)` — 크리스탈(`ShopManager.SpendCrystal`) 또는 광고(`WatchAd`, 자리 `relic`), 등급 확률 `RELIC_RATE_*` 뒤 그 등급 안에서 균등, 전설 천장 `RELIC_PITY`, 중복은 레벨 +1(최대 `RELIC_MAX_LEVEL`)·넘치면 크리스탈 환급. 효과: `ApplyTo`(RefreshStats — 공격·체력·방어 %, 치명 피해 %p, 경험치→`GrowthRate`, 획득률→`DropRate`), `GoldMult`(처치·방치·오프라인·던전), `InterestMult`(RollInterest). 효과 합은 세이브 객체+소환 수로 캐시. 확률 공개용 `GradeRate`/`GradeCount`/`ItemRate`. 세이브 `GameData.Relic` |
| `PromotionManager.cs` | v2 시련(승급 시험, ADR0037, `Managers.Promotion`). 화면 이름은 '시련'(퀘스트의 'N급 모험가 승급'과 구분). 단계 = `GameData.PromotionTier`(읽을 때 `MaxTier` 로 자름), N단계 요구 레벨 `PROMO_LEVEL_STEP`×N, 시험관 자리 `ExamNode` = `GameManager.StageForLevel(요구 레벨)`. `TryEnter`(예약 — 싸우는 곳은 `DungeonManager.PickNode`) → `Begin`(새 전투 그룹) → `Finish(passed)`(통과: 단계+1·크리스탈·`RefreshStats`). `ApplyTo`(RefreshStats — 공격·마법공격·최대 체력 × `PROMO_ATK_MULT/HP_MULT`^단계) |
| `AbyssManager.cs` | v2 심연(ADR0037, `Managers.Abyss`). `IsUnlocked` = `GameManager.AllStagesCleared`, 자리 = `LastStageNode`. `FloorMult(층)` = `ABYSS_START_MULT`×`ABYSS_FLOOR_MULT`^(층−1). `AddKill`(보통 층 처치 — `ABYSS_KILLS_PER_FLOOR` 채우면 `NextFloor`), `IsBossFloor`(`ABYSS_BOSS_EVERY`), `NextFloor`(처음 넘긴 층 크리스탈·`AbyssBest`), `Finish`(결과 알림, 여러 번 불려도 한 번). 전투는 `UI_BattleBtnGroup.CoAbyss` |
| `ShopManager.cs` | v2 상점·핫타임·월정액(ADR0030) + 같은 파일의 `AdService`·`IapService`(모의 — 에디터·개발 빌드만 즉시 성공, 출시 빌드는 광고·결제 줄을 숨긴다). 핫타임 골드·경험치 2배/공격력 +20%(실제 시간 30분, 광고 하루 3회 또는 크리스탈 50) — `GoldMult`/`ExpMult`는 처치(MonsterController)·방치(TickIdle)에, `ApplyHotTime`은 RefreshStats에. 무료 크리스탈·오프라인 2배 광고, 만드라고라 구매, 인앱 상품(`ProductData`: 시작 패키지·스텝업·월정액·광고 제거·크리스탈 첫 구매 2배), 월정액 매일 100·오프라인 상한 +4시간. 세이브 `GameData.Shop` |
| `AbilityManager.cs` | 능력(=스킬) 시스템. 기억력(Memory) 자원 회계: `MaxMemory`(BASE 10+장착 보너스)/`UsedMemory`/`FreeMemory`, `Learn`/`Memorize`(장착)/`Forget`, 점유량 0=재능(자동 장착·망각 불가). 데이터는 `Managers.Data.SkillDic`, 보유/장착 목록은 GameData에 저장. `ApplyTo(CreatureController)`가 장착 능력 StatBonus×Level을 스탯 가산. 미니몬 고유기술 id = creatureId+4000. 지원 보너스 조회 API: `CatchRateBonus`/`GuaranteedCatchReduce`/`ExpGainBonus`/`InterestCapacityBonus`. `GetEquipAbilityLevel(id)`는 장비 유래 능력 단계 훅 — 장비 강화 시스템 미구현으로 현재 항상 0 반환하는 명시적 스텁(기획서2 슬38·49) |
| `ComboManager.cs` | 콤보 시스템(Visual Scripting FSM의 C# 포팅). `Managers.Update`가 매 프레임 `Tick(dt)` 호출(프레임 중복 방지 가드 내장) — 타이머 감소·표시값 Smooth 보간. `AttachPlayer`로 `PlayerController.OnHitLanded` 구독, ComboStageDic(HitsRequired) 기반 Rank 산정, Rank별 `AtkSpeedBonus`/`DmgBoostBonus` 버프 노출. `GetRankColor(int rank)`가 ComboData ColorHex 파싱('#' 유무 허용, 미로드/파싱 실패 시 흰색 fallback)으로 랭크 색을 제공 — `GetCurrentRankColor()`가 이를 사용하고, 콤보 IDLE 색도 Lv0 데이터 기반(`UI_BattleBtnGroup.COMBO_IDLE_COLOR` 프로퍼티 = `GetRankColor(0)`). 이벤트: OnRankChanged/OnTimerChanged/OnComboBreak/OnComboChanged/OnNowValuesChanged/OnFirstBuffActivated. 콤보 브레이크(타이머 0) 시 슬로우모션 0.4s 실시간(timeScale 0.2 → 원복, 팝업 열림 중이면 원복 스킵 가드) + `FX_Combo_Break` 이펙트 키(에셋 미등록 시 LogWarning만) — `_slowMoInFlight` 리셋 누락으로 2회차부터 연출이 스킵되던 버그 수정됨(:266) |
| `ObjectManager.cs` | 전투 오브젝트 스폰. `SpawnMonster(dataId,pos)`/`SpawnCompanion(minionId,player)`(CreatureData.PrefabLabel 기반), `PickFieldMonsterId()`(현재 TravelArea의 Habitat 매칭 + EncounterRate 가중 랜덤, fallback 2001), 데미지 폰트(`ShowDamageFont`/`ShowMissPopup`/`ShowCriticalCallout`/`ShowHealFont` — pooling), 히트 FX(`ShowHitEffect`/`ShowCriHitEffect`, 2초 후 Destroy), 치명타 확정 연출 `ShowCritEffect(pos)`("Basic Crit" 프리팹 — 피격자 위치, CreatureController.TakeHit 치명 분기에서 호출, 기획서1 슬63), 랭크업 연출 `ShowRankUpEffectOn(Transform)`("Player Rank Up Ef" 프리팹을 플레이어 발밑 — 스프라이트 바운즈 하단 y 정렬 — 에 생성, `RANKUP_FX_LIFETIME`=4s 후 Destroy). 루트 Transform property: `@Skill`/`@Monster`/`@Companion`/`@TextFont` |
| `CoroutineManager.cs` | Managers.cs 미등록의 독립 유틸. `[RuntimeInitializeOnLoadMethod]`로 자체 생성되는 DontDestroyOnLoad MonoBehaviour — 비-MonoBehaviour 매니저가 `CoroutineManager.StartCoroutine(...)` static으로 코루틴 실행 |

## For AI Agents

### Working In This Directory
- 접근은 `Managers.Game`/`Managers.Quest`/`Managers.MinMon`/`Managers.Ability`/`Managers.SubQuest`/`Managers.Combo`/`Managers.Object` static property로만. CoroutineManager만 예외(자체 static).
- **영속 상태는 전부 GameData에**: 다른 매니저(MinMon/Ability/Quest/SubQuest)는 자체 저장 없이 `Managers.Game`의 리스트/필드를 참조하고 변경 후 `Managers.Game.SaveGame()`을 호출한다. 새 영속 필드는 GameData에 추가 → GameManager에 property 노출 순서로.
- setter가 SaveGame을 호출하는 property가 많음 — 루프 안 반복 설정은 세이브 스팸이 됨. 대량 변경은 backing 필드를 직접 만지고 마지막에 1회 저장하는 기존 패턴(`ClearAllItemNew`) 참고.
- 퀘스트 완료 처리 순서 불변: `CompleteQuest`는 **퀘스트 전환 후** `ProcessUnlocks`(보상 AddItem 이벤트가 다음 퀘스트 기준으로 체크되도록). SubQuest도 상태 갱신 → Unlock → 이벤트 순서 보장 주석 있음 — 순서 변경 금지.
- 퀘스트 BtnID/TargetID는 문자열 **완전일치** 매칭 (`quest.CompleteTarget == btnId`). 버튼 ID 오탈자는 조용히 무시되므로 데이터 시트와 코드의 문자열을 반드시 대조.
- Init/Clear에서 이벤트 구독/해제 쌍 유지 (QuestManager.Init의 `OnItemAdded += ` ↔ Clear의 `-=` 패턴). 이벤트 구독 추가 시 Clear 해제 누락 금지.
- ComboManager.Tick은 Managers.Update와 UI 양쪽에서 불릴 수 있어 `_lastTickFrame` 가드가 있음 — Tick 호출 지점을 추가해도 안전하지만 가드를 제거하면 더블틱 버그 재발.
- 게임 수치(등급 확률, 콤보 스테이지, 포획 확률 등)는 가능하면 JSON(Data 시트)으로 — 단 `RollGrade` 확률표, `SYNC_PER_CATCH`(0.25), `BASE_MEMORY`(10), `INTEREST_CAPACITY`(2) 등 일부는 현재 코드 상수로 남아있음(주석에 "밸런스 시드 — 엑셀 후속" 표기).

### Common Patterns
- `event Action` / `event Action<T>` Observer — 상태 변경 → 이벤트 발행 → UI 구독 갱신 (컨트롤러/매니저가 UI를 직접 참조하지 않음).
- 데이터 조회는 `Managers.Data.XxxDic.TryGetValue(dataId, ...)` — 실패 시 조용히 반환하거나 LogWarning.
- Find-or-Create 세이브 엔트리: `GetSaveData(dataId)`는 없으면 생성(쓰기용), `FindSave`는 읽기 전용(부작용 없음) — 조회 API에는 FindSave 계열 사용.
- 디버그 점프 API: `JumpToSubQuest` 는 `#if UNITY_EDITOR`, **`JumpToQuest` 는 런타임**(치트 K = 만능 키가 부른다). Debug 세터(`DebugSetKillCount` 등).
  ⚠️ 에디터 전용 API를 런타임에서 부르면 **에디터는 통과하고 플레이어 빌드만 CS1061 로 터진다** (Addressables 콘텐츠 빌드가 "SBP Error" 로 죽는다).
- 비트마스크 해금 상태 (`UnlockedBtmButtons`, `NewBadgeTabs` — `1 << (int)TabType`).
- 슬라이드 번호 주석("슬38", "슬23" 등) = 기획서 슬라이드 참조 — 수정 시 해당 기획 의도 유지.

### 스펙 출처 (기획서 매핑)
원전은 구글 슬라이드 기획서 2종 — **기획서1**(70장: UI/연출/전투), **기획서2**(54장: 도감/미니몬/능력) + 수정안 덱. 에디터 미러는 `Editor/QuestVerifyMenu.cs`(슬라이드별 시연 메뉴).

| 파일 | 기획서 슬라이드 |
|------|----------------|
| `GameManager.cs` | 기획서1 슬10(하단 버튼)·슬19(소지품 NEW 조건) |
| `QuestManager.cs` | 기획서1 슬24(퀘스트 유도/notify) |
| `AbilityManager.cs` | 기획서2 슬38(능력 획득 5경로)·슬46(기억력) |
| `MinMonManager.cs` | 기획서2 슬10(공명도)·슬38(공명 100% 능력) |
| `ObjectManager.cs` | 기획서1 슬63(히트/치명 이펙트), 수정안 슬4(랭크업) |
| `ComboManager.cs` | 기획서1 슬48~49(공세 단계/브레이크) |

## Dependencies

### Internal
- `Managers` (상호 참조 허브: Game↔Quest↔SubQuest, MinMon→Ability/Game, Combo→Data/Game.Player, Object→Data/Resource/Sound)
- `Data` 네임스페이스(CreatureData/QuestData/SubQuestData/ComboStageData/SkillData 등), `Define`(EquipSlotType/TabType/QuestCompleteType/DamageFontMode/ActivationKey 등, `using static`)
- Controllers: `PlayerController`, `MonsterController`, `CompanionController`, `CreatureController`
- UI: `UI_ItemPopup`(퀘스트 보상 팝업 — `SetInfo(itemId, showGlow: true)`, 후광은 퀘스트 보상 경로에서만), `DamageFont`, `Util`(Localize/HexToColor)

### External
- Newtonsoft.Json (GameManager 세이브/로드)
- DOTween (ComboManager — 콤보 브레이크 슬로우모션/FX, SoundManager 경유 페이드)
- UnityEngine (PlayerPrefs, Random, Mathf, File I/O는 System.IO)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- `ComboKeep`는 콤보 지속시간 보너스(%)다. `AbilityManager.ComboKeepBonus`를 `ComboManager.Tick`이 매번 조회하여 감소량을 `dt / (1 + bonus / 100)`으로 적용한다. 기억/망각 시 현재 게이지를 채우지 않으며 HitGain/MaxTimer/정규화는 원래 값을 유지한다. 격리 검증: `_tools/verify_combo_keep.cs`.

- 2026-09-09: 소유한 현재 동행의 고유능력은 `IsCompanionGranted`로 즉시 사용하며 영구 습득 목록에는 넣지 않는다. 공명도 100% 영구화, 로드 시 누락 보상 복원은 `RestoreMinionRewards`. 첫 지역 첫 관심/포획 보정은 MinMonData의 `FirstInterestKillCount`/`FirstCatchChance` 참조. 결정: `docs/decisions/0014-companion-ability-first-catch.md`.
