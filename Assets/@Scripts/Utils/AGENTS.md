<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Utils

## Purpose
프로젝트 전역 공용 유틸 디렉터리. `Define.cs`가 모든 enum/상수/키(LocalizeKey·NotifyKey·UnlockKey·아이템 ID 대역·색상 HEX)의 단일 정의처이고, `BattleMath.cs`가 데미지 계산식의 단일 진입점, `Util.cs`/`Extension.cs`가 GetOrAddComponent·FindChild·Localize 등 정적 헬퍼를 제공한다. 대부분의 스크립트가 `using static Define`/`using static Util`로 이 디렉터리에 의존하므로 여기 수정은 전역 영향이다.

## Key Files
| File | Description |
|------|-------------|
| `Define.cs` | 전역 enum 30여 종 + `LocalizeKey` 자동생성 static class + NotifyKey/UnlockKey 상수 + 아이템 ID 대역(`ITEM_ID_MIN=10000` 등) + 색상 HEX 상수 |
| `BattleMath.cs` | `BattleMath.Resolve(attacker, defender, attackType, skillMul)` — 명중 dice → BASE(크리) → 저항/방어 감산 → 피해증폭×Random(0.8~1.2), floor 1. `DamageResult` struct 반환. `FromRawDamage()` 헬퍼, `DebugForcePlayerCritical` 에디터 토글(Ctrl+Shift+C) |
| `Util.cs` | `GetOrAddComponent<T>`, `FindChild<T>(recursive)`, `Localize(key)`(LocalizationDic + 현재 Language), `HexToColor`, `ParseEnum<T>`, `FormatStat`(슬18 표기규칙), `GetGradeColor(EquipGrade)`, `SetTMPOutline/SetTMPUnderlay`, `RandomPointInAnnulus` |
| `CameraShake.cs` | 타격 카메라 흔들림(ADR0033) `CameraShake.Play(세기, 시간)`. 설정 `IsShakeOn` 을 따르고, 겹치면 앞 흔들림을 끊고 원위치에서 다시(위치 누적 방지), 실시간(SetUpdate). HUD 캔버스는 Screen Space - Camera 라 월드만 흔들린다 |
| `Extension.cs` | GameObject/BaseController 확장 — `GetOrAddComponent`, `BindEvent`(UI_Base.BindEvent 위임), `IsValid`, `DestroyChilds`(Managers.Resource.Destroy 사용), `IList.Shuffle` |
| `UI_ParticlePlay.cs` | OnEnable 시 `UIParticleSystem.DOPlay()` 호출하는 소형 MonoBehaviour (unity-ui-extensions + DOTween) |

## For AI Agents

### Working In This Directory

**Define.cs 핵심 enum** (수정 시 영향 범위 함께 표기):
- `State` (Idle/Moving/Attack/OnDamaged/Dead) — 크리처 상태머신
- `ObjectType` (Player/Monster/Projectile/Minion) — BattleMath 크리 토글, ObjectManager 분기
- `CreatureRarity` (Common/Rare/Boss) — CreatureData.Rarity 문자열과 대응
- `AttackType` (Physical/Magic) — BattleMath 저항/데미지 색 분기
- `EquipSlotType` (Weapon~Accessory 6종) / `EquipGrade` (Basic~Mystic+REQUISITE 7종 — `Util.GetGradeColor`와 1:1)
- `SkillCategory` (Talent/Memory) / `SkillActiveType` (Passive/Sequence/Repeat)
- 능력(Ability) 4종: `AbilityTrigger`(None/Probability/Time), `AbilityCondition`(BattleStart~LowHp 9종), `AbilityContext`, `AbilityEffect`(Heal/BonusDamage/Buff) + `AbilitySource`(Unlimited/Class/Minion/Equip/Npc — 능력정보창 하단 UI 5분기)
- `CraftCategory` (Potion/Skill), `Scene` (GameScene/TitleScene/HubScene/BattleScene), `AreaType` (Village1/Battle), `Village1Group`, `TabType` (하단 6탭+α — GameManager.UnlockBtmButton 키)
- `MinMonGrade` (E~S), `Language` (Korean/English), `FontType` (UI/Dialogue/Battle — FontManager), `Sound` (Bgm/SubBgm/Effect), `UIEvent`, `DialogueEmotion`, `ActivationKey`
- 퀘스트 5종: `QuestCompleteType` (ITEM_ACQUIRE/ITEM_COUNT/ANIM_END/MULTI/BTN_CLICK), `QuestUnlockType` (UI_BUTTON/SUBQUEST_START/ITEM_REWARD/ITEM_REMOVE), `QuestConditionType`, `SubQuestCompleteType`, `SubQuestShowTiming`

**중요 규칙:**
- `#region LocalizeKey ... #endregion` 블록은 **Tools/GenerateLocalizeKeys(DataTransformer)가 구글 로컬라이즈 시트에서 자동 재생성해 통째로 덮어쓴다**. 이 블록 안을 손으로 편집하면 다음 재생성 때 유실된다 — 신규 키는 시트에 추가 후 재생성이 정석.
- NotifyKey 상수(`VILLAGE1_GUILD_VFX` 등)는 QuestNotifyData.BtnID / QuestConditionData.Target(BTN_CLICK)과 **문자열 완전일치**로 매칭된다. 오타·부분일치 함정 주의 (시트 표기 유지 사례: `BTN_COMMISION`).
- UnlockKey(`UNLOCK_INVENTORY_BUTTON` 등)는 QuestUnlockData.TargetID(UI_BUTTON)와 대응.
- 아이템 ID 대역: 일반 10000+, 귀중품 20000+, HP포션 30000+, 스피드포션 40000+, 장비 50000+ — 인벤토리 분류 로직이 이 대역에 의존.
- enum 값을 추가/재정렬할 때: JSON에 문자열로 직렬화되는 것(WriteJsonStr 경유 — EquipGrade, SkillCategory 등)은 이름 변경이 파괴적이고, 순서(int) 저장되는 곳이 있으면 재정렬이 파괴적. 시트 값(문자열)과 enum 이름이 일치해야 `ConvertValue<T>` 파싱이 성공한다.
- `BattleMath.Resolve`는 HP 차감/사망 처리를 하지 않는다 — 호출 측(`CreatureController.TakeHit`) 책임. 데미지 식 변경은 기획서 슬53 식과 BattleTestMenu의 100회 시뮬레이션 기대치 주석도 함께 갱신.
- `Util.Localize(key)`는 키 부재 시 key 자체를 반환하므로 화면에 raw key가 보이면 Localization 시트 누락 신호.

### Common Patterns
- `using static Define;` / `using static Util;` 로 상수·헬퍼를 무접두 사용.
- 확장 메서드는 Extension.cs, 정적 유틸은 Util.cs로 분리 (Extension이 Util로 위임하는 경우 있음: `GetOrAddComponent`).
- `DamageResult` struct로 Hit/IsCrit/FinalDamage/AttackType을 한 번에 반환 — 데미지 폰트 색/Miss 표기 분기용.
- `#if UNITY_EDITOR` 디버그 필드 (`BattleMath.DebugForcePlayerCritical`).

## Dependencies

### Internal
- `Managers` (Data.LocalizationDic, Game.Language, Resource.Destroy) — Util/Extension에서 참조
- `CreatureController` (Accuracy/Evasion/FinalPhysicalAtk 등 스탯 프로퍼티) — BattleMath에서 참조
- `UI_Base.BindEvent` — Extension.BindEvent 위임

### External
- UnityEngine (Mathf, Random, ColorUtility, RuleTile)
- TMPro (TMP_Text, ShaderUtilities) — Util의 TMP 헬퍼
- DG.Tweening (DOTween) — Extension using, UI_ParticlePlay
- UnityEngine.UI.Extensions (`UIParticleSystem`) — UI_ParticlePlay

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- `ItemSourceGuide`는 실제 드롭·출현 조건과 여정의 서식지 매칭을 이용해 재료의 첫 획득 지역·마물을 조회한다. 출처가 없으면 데이터에 저작된 `FoundLocation`으로 돌아간다.
- `ClassAbilityDescription`은 직업 능력의 현재·습득·다음 단계 효과를 `SkillData`와 `ClassAbility*` 번역 템플릿으로 생성한다. 다중 스탯 우선순위·단계 배율·발동 조건은 `AbilityManager`/`AbilityRunner`의 실제 적용과 일치시킨다.
