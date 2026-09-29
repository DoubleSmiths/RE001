<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# Controllers/Creature

## Purpose
크리처(플레이어, 몬스터, 동행 미니몬)의 스탯·상태머신·전투 로직을 담는 상속 계층. `BaseController → CreatureController(abstract) → PlayerController / MonsterController / CompanionController` 구조이며, `CreatureController`가 공용 스탯(MaxHp, PhysicalAtk, CritRate, Accuracy 등 + Rate/Amp 증가율 시스템), `State` property setter(값 변경 시 Animator CrossFade 자동 전환), `TakeHit`(BattleMath.DamageResult 적용 + Damage/Miss 폰트 팝업 + 사망 처리), OnHit 색 플래시 피드백(DOTween)을 제공한다. 전투는 "명중/회피 dice → BattleMath.Resolve → TakeHit" 흐름이고, 데미지 적용 시점은 Attack 애니메이션의 키프레임 이벤트(`AttackTrigger`, Animation Event)로 동기화된다(슬라이드 61). `AbilityRunner`는 PlayerController에 런타임 부착되어 장착 능력의 확률/시간 발동을 처리하는 보조 컴포넌트다.

## Key Files
| File | Description |
|------|-------------|
| `CreatureController.cs` | abstract 베이스. 공용 스탯 필드 + `Final*` 계산 프로퍼티(`기본*(1+Rate/100)*Amp`), `virtual State` setter(Dead→"Die", Idle→"Idle", Moving→"Run", Attack→"Attack" CrossFade), `virtual TakeHit(DamageResult, attacker)`(Miss 팝업/HP 차감/데미지 폰트/타격 성공 시 Hit Sound — `Sound_Hit` 미등록 시 `Slime_OnHit` 폴백/Crit 시 Crit 이펙트·`Sound_Crit`(미등록 시 무음)을 **피격자 위치**에 표시(슬63)/OnDead), `OnDamaged(float)` 호환 shim, DOTween 색 플래시 `PlayHitFeedback`(virtual), `FlipToward`. 타격감(ADR0033): `HitStop(초)`(애니메이터를 실시간으로 멈췄다 푼다 — 겹치면 긴 쪽 끝에서 한 번), **애니 속도는 반드시 `SetAnimSpeed`**(직접 `_anim.speed` 를 쓰면 멈칫이 풀리며 덮어쓴다), `PlayStateClip`(섞지 않고 즉시 지정 지점부터), `virtual HitPoint`(타격 이펙트 위치 — 몸 한가운데) |
| `PlayerController.cs` | `ObjectType.Player`. 공격(ADR0033 `UpdateAttack`): **탭 공격** `RequestAttack()`(UI_BattleInput) — 칼을 든 자세(TAP_SWING_START)부터 클립을 틀어 약 0.09초에 판정, 간격 `TAP_ATTACK_INTERVAL`/공속 배율, 입력 버퍼 `TAP_INPUT_BUFFER`, 적이 없으면 허공 베기 / **AUTO**(`GameManager.AutoBattle`)면 `AtkCooldown`/공속 배율 주기로 자동 — 공속 배율 = `(AtkSpeed + 콤보 + 신속물약)/100`(하한 0.1, 슬54). 판정은 `CoSwing` 이 시각을 잡고 클립 이벤트 `AttackTrigger` 도 같은 함수(한 번 베기에 한 번, `_swingHitDone`). 명중 시 `Impact`: 둘 다 히트스톱·카메라 흔들림(`CameraShake`)·적 밀림, 레벨/EXP(`GainExp`/`CheckLevelUp`/`RefreshStats` — PlayerStatusData/LevelData 조회 + StatInvest + Managers.Ability.ApplyTo), 자동물약, HP·신속물약 쿨타임 영속 보관(`_autoPotionCd`/`_spdPotionCd`, `StartHpPotionCooldown`/`StartSpdPotionCooldown`, `AutoPotionCooldownNormalized`/`SpdPotionCooldownNormalized`), 동행 미니몬 스폰 동기화(`RefreshCompanion`), 사망 시 마을 리스폰. 이벤트: `OnHpChanged`, `OnExpChanged`, `OnLevelUp`, `OnHitLanded`, `OnBuffApplied/OnBuffCleared(BuffInfo)`, `OnAutoPotionUsed` v2 가드·패링(ADR0030): `TryGuard()`(GUARD_DURATION 동안 막음, 재사용 GUARD_COOLDOWN) → 몬스터 타격 순간 `ResolveGuard()`가 처음 PARRY_WINDOW면 Parry(피해 0·`ParryCounter`=멈칫 뒤 확정 치명 반격+콤보 가산), 나머지면 Block(피해×BLOCK_DAMAGE_MULT). `OnGuard` 이벤트. |
| `MonsterController.cs` | `ObjectType.Monster`. `SetInfo(dataId)`로 CreatureData 스탯/AnimatorController 주입 후 `CacheClipLengths()`로 Attack/Die 클립 길이 캐싱 — Attack 상태 유지·사망 풀 반환 타이밍이 실제 클립 길이 기반(기본값 슬라임 0.4/0.5s, 고블린 1.35/1.52s 같은 긴 클립 대응). engage 전엔 Ground(-5) + 제 걸음(`MONSTER_WALK_SPEED`)으로 좌측 이동, Player와 `OnCollisionEnter2D` 충돌 시 `_hasEngaged=true`(거리 폴링 아님) + Idle·Kinematic·`_homeX`. ADR0033: `FitBody` 가 스프라이트 Tight 메시로 실제 몸을 재 충돌 폭·`HitPoint`·`DamageFontYOffset`(머리 위)를 맞추고 몸 면적 > `BODY_TARGET_AREA` 면 줄인다. 모든 명중에 `PlayImpactFlash`(흰 번쩍임+찌그러짐), `Knockback`(플레이어 칼, 마무리는 날아감), 공격 순간 `Nudge` 로 내딛기, 사망 후 `DEATH_FADE` 흐려짐. 피격 사운드는 상태 무관 재생(Attack 중에도 사운드는 남 — 애니/색 플래시만 억제, 슬63). 사망 시 보상: 골드는 `GoldDropChance` 확률(0=미기입, 항상 지급), 별조각(Crystal)은 `DiaDropChance`/`DiaDropCount`(0이면 드롭 없음, 슬55 #15~17), 아이템 드롭은 `DropChance × 플레이어 DropRate/100`(슬14) + Exp + `Managers.MinMon.AddKill`/`RollInterest`(포획 관심) + `max(0.5s, Die클립 길이)` 후 풀 반환. 이벤트: `OnHpChanged`, `OnEngaged`, static `OnItemDropped(int,int)` v2: 타격 ATTACK_TELEGRAPH 초 전 머리 위 `Emoj_Exclamation`(정적 이벤트 `OnAttackTelegraph`), 패링당하면 `Stun`(애니메이터 정지·받는 피해 ×STUN_DAMAGE_TAKEN_MULT). 스테이지 보스: `MakeStageBoss`(능력치·보상·크기·`BossOutline`), `Retreat`(시간 초과 퇴각, 보상 없음), `OnDied`(실제 사망에만), 처치 보상은 `_rewardExp/_rewardGold`. `ScaleStats(stat, reward)` — 시련·심연 배수(ADR0037): 최대 체력(=현재 체력)·공격·마법공격·방어 × stat, 처치 골드·경험치 × reward. |
| `CompanionController.cs` | `ObjectType.Minion`. 동행 미니몬 — 플레이어를 LateUpdate에서 Lerp 추종(기본 `FOLLOW_OFFSET` + `MinMonData.FollowOffsetX/Y` 가산 = 미니몬별 위치, 데이터 미기입(0/0)이면 기본 위치, 슬19 part2; 데드존 스냅), 전투 중 engaged 몬스터 자동 공격 후 `_player.NotifyCompanionHit`로 콤보 누적. 명중률만 플레이어 추종. 피격 대상 아님(Rigidbody2D.simulated=false, Collider 전부 비활성). 스탯은 `Managers.MinMon`(공명도 반영), 펫 전용 Animator는 `MinMonData.PetAnimLabel`로 교체 |
| `AbilityRunner.cs` | PlayerController에 `GetOrAddComponent`로 런타임 부착되는 능력 발동 런너(슬39). 확률 발동(`Notify(AbilityCondition)` — OnAttack/OnHit/OnCrit/OnDamaged/SelfEvade/OnHeal/LowHp/BattleStart)과 시간 발동(Update에서 Cooldown 관리)을 처리. Effect: Heal/BonusDamage/Buff. 임시 버프는 라이브 스탯 필드에 직접 가감하며 `OnStatsReset()`으로 RefreshStats와의 이중 환불 방지 |

## For AI Agents

### Working In This Directory
- **애니 속도는 `SetAnimSpeed`, 즉시 재생은 `PlayStateClip`**(ADR0033): 히트스톱이 `_anim.speed` 를 0 으로 잡고 있다가 기억한 값으로 되돌린다.
- **상태 전환은 반드시 `State` property setter로**: setter가 Animator CrossFade를 수행하므로 `_state` 필드 직접 변경 금지. 클립 이름은 "Idle"/"Run"/"Attack"/"Die"/"OnHit"로 하드코딩되어 있어 Animator 클립명과 일치해야 한다. 참고: Slime Animator에는 "Run"이 없어 MonsterController는 공격 후 Idle로 복귀한다.
- **데미지 흐름**: 공격자는 `BattleMath.Resolve(attacker, target, AttackType)` → `target.TakeHit(result, attacker)`. 사전 계산된 raw 데미지는 `BattleMath.FromRawDamage()` 경유(`OnDamaged` shim, AbilityRunner BonusDamage). 데미지 폰트는 Player/Minion 공격 시에만 표시되고 Monster 공격은 팝업 없음(슬63) — `TakeHit`의 `attackerIsMonster` 분기.
- **플레이어 타격 시점 = `CoSwing` 이 잡는 칼 닿는 시각**(ADR0033): `StartSwing` 이 클립 시작 지점(탭 0.235·AUTO 0)과 휘두르기 길이로 애니 속도를 정하고, `ATTACK_IMPACT_TIME`(0.5초, 클립 이벤트와 같은 시각)에 `AttackTrigger()` 를 부른다. 클립의 AttackTrigger 이벤트는 **끈다**(`fireEvents=false` — 판정 직후 새 베기를 시작한 프레임에 이전 재생의 이벤트가 새 베기의 판정으로 들어오는 경합이 있었다). `_swingHitDone` 은 한 번 베기에 한 번만 맞게 하는 가드. 휘두르기 중 맞붙은 적은 판정 순간에 다시 찾는다. AUTO 베기는 칼이 닿은 뒤 탭이 끊고 들어갈 수 있다. 일반 피격은 진행 중인 공격을 취소하지 않는다(ADR0024). 새 공격 시작 시 이전 피격 복귀 코루틴을 끝낸다. 공속·콤보·물약 합산 배율은 공격 시작 때 캡처한다. 귀환 연출 동안은 `LeaveBattle()` 로 입력을 막는다. 몬스터는 `CoAttack`의 0.25초 대기(히트스톱 동안은 멈춤) 후 앞으로 내딛으며 `AttackTrigger()`를 호출하고 `_attackHitDone`으로 Animation Event와 중복 타격을 막는다.
- **몬스터 클립 이름 규칙**: `CacheClipLengths()`는 클립 이름의 "Attack"/"Die" 포함 여부로 길이를 찾는다 — 새 몬스터 Animator의 공격/사망 클립 이름에 해당 문자열 필수(못 찾으면 슬라임 기준 0.4/0.5s 기본값으로 동작).
- **engage 판정은 collider 충돌 flag**: `MonsterController.OnCollisionEnter2D`가 `_hasEngaged`를 set하고, Player/Companion은 거리 폴링 대신 `Managers.Object.MonsterTransform` 자식을 순회하며 `HasEngaged`로 타겟을 찾는다.
- **풀 반환은 `Managers.Resource.Destroy(gameObject)`**: MonsterController 사망(`max(0.5s, Die클립)` 후), Companion 해제(`PlayerController.RefreshCompanion`) 모두 이 경로.
- **이벤트 구독 해제**: `PlayerController.OnDestroy`가 `Managers.MinMon.OnCompanionChanged -= ...`를 해제한다. 이 디렉터리의 이벤트(`OnHpChanged` 등, 특히 static `MonsterController.OnItemDropped`)를 구독하는 UI 코드는 반드시 OnDestroy에서 해제할 것.
- **스탯 갱신 순서 주의**: `PlayerController.RefreshStats()`는 base 스탯 리셋 → StatInvest → `Managers.Ability.ApplyTo` → `_abilityRunner.OnStatsReset()`(임시버프 추적 초기화) → `Hp=MaxHp` 순서. AbilityRunner 임시 버프는 이 파이프라인 밖에서 라이브 필드에 직접 가감하므로, RefreshStats 호출 시점을 바꾸면 버프 이중 적용/환불 버그가 생길 수 있다.
- **물약 쿨타임은 unscaled 시계로 항상 감소**: `_autoPotionCd`/`_spdPotionCd`는 `PlayerController.Update()` 상단에서 전투/마을 무관하게 `Time.unscaledDeltaTime`으로 감소한다 — UI_PotionPopup 쿨타임 바와 동일 시계(전투 밖에서 타이머가 멈춰 팝업 재오픈 시 쿨타임이 리셋되던 버그 수정). 수동 사용은 `StartHpPotionCooldown`/`StartSpdPotionCooldown`으로 쿨타임 공유(더 긴 값만 반영), 팝업은 `*CooldownNormalized`로 남은 비율 조회. 감소 로직을 `_inBattle` 가드 아래로 옮기지 말 것.
- **주석의 "슬라이드 N"은 기획서 슬라이드 번호** — 수치/연출 변경 시 해당 슬라이드 스펙을 근거로 한 주석을 함께 갱신할 것.
- `PlayerStatusDic`이 DataId 키라 레벨 조회에 실질적으로 안 잡혀 `RefreshStats`는 거의 항상 fallback 식(하드코딩 밸런스)을 사용한다는 주석이 있음 — 밸런스 수정 시 이 fallback 블록이 실효 코드다.

### Common Patterns
- **State property setter + CrossFade**: 상태 변경 즉시 애니메이션 전환. 코루틴(`CoSwing`, `CoReturnAfterOnHit`)으로 일정 시간 후 Idle 복귀하되 "그 사이 상태가 바뀌었으면 건드리지 않음" 가드.
- **`if (!base.Init()) return false;` + ObjectType 설정**: 모든 서브클래스 Init의 첫 줄.
- **GetOrAddComponent**: `CreatureController.Init`이 `_rb`/`_anim`/`_sprite` 확보, PlayerController가 AbilityRunner 부착.
- **데이터 드리븐 런타임 교체**: `RuntimeAnimatorController`를 `Managers.Resource.Load`로 로드해 교체 (MonsterController `_data.AnimController`, CompanionController `PetAnimLabel` — 동일 패턴, 실패 시 프리팹 기본 유지). SFX 키도 CreatureData(`AttackSfx`/`OnHitSfx`/`DieSfx`)에서.
- **C# event 통지**: 컨트롤러는 UI를 직접 참조하지 않고 `OnHpChanged`/`OnExpChanged`/`OnBuffApplied` 등으로 발화 (예외: PlayerController.OnDead가 `UI_MonsterHPBar`/`UI_GameScene`을 직접 조작하는 사망 시퀀스).
- **Time.time 쿨다운**: `_lastAtkTime + cooldown` 비교로 공격 주기 제어 (Player/Monster/Companion 공통).

### 스펙 출처(기획서 매핑)
이 디렉터리 파일의 기획서 원전(주석의 "슬N" 근거). 전투 챕터(슬61~70)는 **기획서1**, 도감/미니몬/능력은 **기획서2**가 원전이며, 스펙 검증용 에디터 미러는 `Editor/QuestVerifyMenu.cs`다.

| File | 기획서 슬라이드 |
|------|----------------|
| `CreatureController.cs` | 기획서1 슬15·54·61·63·64·65·67·68, 기획서2 슬40 |
| `PlayerController.cs` | 기획서1 슬61·63·65·66·68·69·70, 기획서2 슬40 |
| `MonsterController.cs` | 기획서1 슬14·61·63·65·67 |
| `AbilityRunner.cs` | 기획서2 슬39·40·41 |
| `CompanionController.cs` | (매핑 미등록 — 코드 주석은 기획서2 슬19 part2 참조) |

- **알려진 의도적 상충 1건**: `BattleMath`(Utils)의 최소 1 데미지 정책 vs 기획서1 슬14의 '하한 0' — 기획 확인 대기 중. 확정 전까지 양쪽 모두 수정 금지.

## Dependencies

### Internal
- `Managers.Data` — CreatureDic, SkillDic, MinMonDic, LevelDic, PlayerStatusDic, HPPotionDic, BuffDic
- `Managers.Object` — MonsterTransform, SpawnCompanion, ShowDamageFont/ShowMissPopup/ShowHitEffect/ShowCritEffect/ShowCriHitEffect/ShowCriticalCallout/ShowHpPotionEffectOn
- `Managers.Game` — Gold, Crystal(별조각 확률 보상), Exp, Level, StatInvest, EquippedAbilities, CompanionMinMonId, AddItem, 자동물약 설정
- `Managers.Resource` — Load(RuntimeAnimatorController), Destroy(풀 반환)
- `Managers.MinMon` — AddKill, RollInterest, GetAtk/GetCritRate/GetAtkSpeed, OnCompanionChanged
- `Managers.Combo` — Tick, AtkSpeedBonus, DmgBoostBonus / `Managers.Ability` — ApplyTo, ExpGainBonus
- `Managers.Sound` — PlaySFX / `Managers.UI` — SceneUI, ShowAttractedPopup, ShowWarningPopup
- `BattleMath` (Utils) — Resolve, FromRawDamage, DamageResult / `Define` — State, ObjectType, AbilityCondition/Trigger/Effect/Context, CreatureRarity, AreaType
- UI 타입 직접 참조: `UI_MonsterHPBar`, `UI_GameScene` (PlayerController 사망 시퀀스)

### External
- **DOTween** (`DG.Tweening`) — CreatureController 피격 색 플래시(`DOColor`), `DOKill`, PlayerController `transform.DOKill()`

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- 2026-09-10: 도감/포획 모달의 전투 정지는 `Managers.UI.IsBattlePaused`로 조회한다. Player의 자동공격/자동물약/물약 쿨타임과 AbilityRunner의 시간 발동/쿨타임/액티브 사용을 보류한다. StartBattle/StopBattle은 UI 정지 조건을 재평가하며, 진행 중 공격 코루틴·Animation Event·피해 처리를 취소하거나 무시하지 않는다.

- 2026-09-09: 액티브 미니몬 능력은 교체 후에도 공유 쿨타임을 유지한다. `OnStatsReset`은 아직 장착 중인 능력의 만료 전 버프를 재적용하고 해제된 능력 버프만 제거한다. `PlayerController`는 능력 변경 시 HP를 회복하지 않고 스탯을 갱신한다.
