<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Controllers/Creature

## Purpose
크리처(플레이어, 몬스터, 동행 미니몬)의 스탯·상태머신·전투 로직을 담는 상속 계층. `BaseController → CreatureController(abstract) → PlayerController / MonsterController / CompanionController` 구조이며, `CreatureController`가 공용 스탯(MaxHp, PhysicalAtk, CritRate, Accuracy 등 + Rate/Amp 증가율 시스템), `State` property setter(값 변경 시 Animator CrossFade 자동 전환), `TakeHit`(BattleMath.DamageResult 적용 + Damage/Miss 폰트 팝업 + 사망 처리), OnHit 색 플래시 피드백(DOTween)을 제공한다. 전투는 "명중/회피 dice → BattleMath.Resolve → TakeHit" 흐름이고, 데미지 적용 시점은 Attack 애니메이션의 키프레임 이벤트(`AttackTrigger`, Animation Event)로 동기화된다(슬라이드 61). `AbilityRunner`는 PlayerController에 런타임 부착되어 장착 능력의 확률/시간 발동을 처리하는 보조 컴포넌트다.

## Key Files
| File | Description |
|------|-------------|
| `CreatureController.cs` | abstract 베이스. 공용 스탯 필드 + `Final*` 계산 프로퍼티(`기본*(1+Rate/100)*Amp`), `virtual State` setter(Dead→"Die", Idle→"Idle", Moving→"Run", Attack→"Attack" CrossFade), `virtual TakeHit(DamageResult, attacker)`(Miss 팝업/HP 차감/데미지 폰트/Hit·Crit 이펙트/OnDead), `OnDamaged(float)` 호환 shim, DOTween 색 플래시 `PlayHitFeedback`(virtual), `FlipToward` |
| `PlayerController.cs` | `ObjectType.Player`. 자동공격(AutoAttack — engaged 몬스터 탐색, 콤보+신속물약 공속 배율), Animation Event `AttackTrigger()`에서 데미지 판정, 레벨/EXP(`GainExp`/`CheckLevelUp`/`RefreshStats` — PlayerStatusData/LevelData 조회 + StatInvest + Managers.Ability.ApplyTo), 자동물약, 동행 미니몬 스폰 동기화(`RefreshCompanion`), 사망 시 마을 리스폰. 이벤트: `OnHpChanged`, `OnExpChanged`, `OnLevelUp`, `OnHitLanded`, `OnBuffApplied/OnBuffCleared(BuffInfo)`, `OnAutoPotionUsed` |
| `MonsterController.cs` | `ObjectType.Monster`. `SetInfo(dataId)`로 CreatureData 스탯/AnimatorController 주입. engage 전엔 Ground 동속(-5) 좌측 스크롤, Player와 `OnCollisionEnter2D` 충돌 시 `_hasEngaged=true`(거리 폴링 아님). 사망 시 보상(Gold/Exp/드롭) + `Managers.MinMon.AddKill`/`RollInterest`(포획 관심) + 0.5s 후 풀 반환. 이벤트: `OnHpChanged`, `OnEngaged`, static `OnItemDropped(int,int)` |
| `CompanionController.cs` | `ObjectType.Minion`. 동행 미니몬 — 플레이어를 LateUpdate에서 Lerp 추종(FOLLOW_OFFSET, 데드존 스냅), 전투 중 engaged 몬스터 자동 공격 후 `_player.NotifyCompanionHit`로 콤보 누적. 명중률만 플레이어 추종. 피격 대상 아님(Rigidbody2D.simulated=false, Collider 전부 비활성). 스탯은 `Managers.MinMon`(공명도 반영), 펫 전용 Animator는 `MinMonData.PetAnimLabel`로 교체 |
| `AbilityRunner.cs` | PlayerController에 `GetOrAddComponent`로 런타임 부착되는 능력 발동 런너(슬39). 확률 발동(`Notify(AbilityCondition)` — OnAttack/OnHit/OnCrit/OnDamaged/SelfEvade/OnHeal/LowHp/BattleStart)과 시간 발동(Update에서 Cooldown 관리)을 처리. Effect: Heal/BonusDamage/Buff. 임시 버프는 라이브 스탯 필드에 직접 가감하며 `OnStatsReset()`으로 RefreshStats와의 이중 환불 방지 |

## For AI Agents

### Working In This Directory
- **상태 전환은 반드시 `State` property setter로**: setter가 Animator CrossFade를 수행하므로 `_state` 필드 직접 변경 금지. 클립 이름은 "Idle"/"Run"/"Attack"/"Die"/"OnHit"로 하드코딩되어 있어 Animator 클립명과 일치해야 한다. 참고: Slime Animator에는 "Run"이 없어 MonsterController는 공격 후 Idle로 복귀한다.
- **데미지 흐름**: 공격자는 `BattleMath.Resolve(attacker, target, AttackType)` → `target.TakeHit(result, attacker)`. 사전 계산된 raw 데미지는 `BattleMath.FromRawDamage()` 경유(`OnDamaged` shim, AbilityRunner BonusDamage). 데미지 폰트는 Player/Minion 공격 시에만 표시되고 Monster 공격은 팝업 없음(슬63) — `TakeHit`의 `attackerIsMonster` 분기.
- **데미지 적용 시점 = Animation Event**: Player Attack.anim의 0.5s 키프레임이 `AttackTrigger()`를 호출한다. OnHit로 인터럽트되면 이벤트 미발화 → 데미지 미적용이 의도된 동작(슬60). Monster는 아직 키프레임이 없어 `AttackTrigger` inline 호출이 주석 처리된 과도기 상태.
- **engage 판정은 collider 충돌 flag**: `MonsterController.OnCollisionEnter2D`가 `_hasEngaged`를 set하고, Player/Companion은 거리 폴링 대신 `Managers.Object.MonsterTransform` 자식을 순회하며 `HasEngaged`로 타겟을 찾는다.
- **풀 반환은 `Managers.Resource.Destroy(gameObject)`**: MonsterController 사망(0.5s 후), Companion 해제(`PlayerController.RefreshCompanion`) 모두 이 경로.
- **이벤트 구독 해제**: `PlayerController.OnDestroy`가 `Managers.MinMon.OnCompanionChanged -= ...`를 해제한다. 이 디렉터리의 이벤트(`OnHpChanged` 등, 특히 static `MonsterController.OnItemDropped`)를 구독하는 UI 코드는 반드시 OnDestroy에서 해제할 것.
- **스탯 갱신 순서 주의**: `PlayerController.RefreshStats()`는 base 스탯 리셋 → StatInvest → `Managers.Ability.ApplyTo` → `_abilityRunner.OnStatsReset()`(임시버프 추적 초기화) → `Hp=MaxHp` 순서. AbilityRunner 임시 버프는 이 파이프라인 밖에서 라이브 필드에 직접 가감하므로, RefreshStats 호출 시점을 바꾸면 버프 이중 적용/환불 버그가 생길 수 있다.
- **주석의 "슬라이드 N"은 기획서 슬라이드 번호** — 수치/연출 변경 시 해당 슬라이드 스펙을 근거로 한 주석을 함께 갱신할 것.
- `PlayerStatusDic`이 DataId 키라 레벨 조회에 실질적으로 안 잡혀 `RefreshStats`는 거의 항상 fallback 식(하드코딩 밸런스)을 사용한다는 주석이 있음 — 밸런스 수정 시 이 fallback 블록이 실효 코드다.

### Common Patterns
- **State property setter + CrossFade**: 상태 변경 즉시 애니메이션 전환. 코루틴(`CoResetStateAfterAttack`, `CoReturnAfterOnHit`)으로 일정 시간 후 Idle 복귀하되 "그 사이 상태가 바뀌었으면 건드리지 않음" 가드.
- **`if (!base.Init()) return false;` + ObjectType 설정**: 모든 서브클래스 Init의 첫 줄.
- **GetOrAddComponent**: `CreatureController.Init`이 `_rb`/`_anim`/`_sprite` 확보, PlayerController가 AbilityRunner 부착.
- **데이터 드리븐 런타임 교체**: `RuntimeAnimatorController`를 `Managers.Resource.Load`로 로드해 교체 (MonsterController `_data.AnimController`, CompanionController `PetAnimLabel` — 동일 패턴, 실패 시 프리팹 기본 유지). SFX 키도 CreatureData(`AttackSfx`/`OnHitSfx`/`DieSfx`)에서.
- **C# event 통지**: 컨트롤러는 UI를 직접 참조하지 않고 `OnHpChanged`/`OnExpChanged`/`OnBuffApplied` 등으로 발화 (예외: PlayerController.OnDead가 `UI_MonsterHPBar`/`UI_GameScene`을 직접 조작하는 사망 시퀀스).
- **Time.time 쿨다운**: `_lastAtkTime + cooldown` 비교로 공격 주기 제어 (Player/Monster/Companion 공통).

## Dependencies

### Internal
- `Managers.Data` — CreatureDic, SkillDic, MinMonDic, LevelDic, PlayerStatusDic, HPPotionDic, BuffDic
- `Managers.Object` — MonsterTransform, SpawnCompanion, ShowDamageFont/ShowMissPopup/ShowHitEffect/ShowCritEffect/ShowCriHitEffect/ShowCriticalCallout/ShowHpPotionEffectOn
- `Managers.Game` — Gold, Exp, Level, StatInvest, EquippedAbilities, CompanionMinMonId, AddItem, 자동물약 설정
- `Managers.Resource` — Load(RuntimeAnimatorController), Destroy(풀 반환)
- `Managers.MinMon` — AddKill, RollInterest, GetAtk/GetCritRate/GetAtkSpeed, OnCompanionChanged
- `Managers.Combo` — Tick, AtkSpeedBonus, DmgBoostBonus / `Managers.Ability` — ApplyTo, ExpGainBonus
- `Managers.Sound` — PlaySFX / `Managers.UI` — SceneUI, ShowAttractedPopup, ShowWarningPopup
- `BattleMath` (Utils) — Resolve, FromRawDamage, DamageResult / `Define` — State, ObjectType, AbilityCondition/Trigger/Effect/Context, CreatureRarity, AreaType
- UI 타입 직접 참조: `UI_MonsterHPBar`, `UI_GameScene` (PlayerController 사망 시퀀스)

### External
- **DOTween** (`DG.Tweening`) — CreatureController 피격 색 플래시(`DOColor`), `DOKill`, PlayerController `transform.DOKill()`

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
