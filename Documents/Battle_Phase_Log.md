# 전투 작업 기록

기획서: `Documents/기획서v2_05.04.pptx` 슬라이드 53~69
브랜치: `ai_test2`
시작일: 2026-05-05

각 Phase 별로 **변경 파일 / 변경 요약 / 핵심 함수 / 롤백 시 주의** 를 기록.
파일 단위 롤백을 위해 변경 파일은 풀 경로로 적고, 함수 단위 롤백을 위해 변경된 메서드 시그니처를 적는다.

---

## 진행 현황

| Phase | 범위 | 상태 | 커밋 |
|---|---|---|---|
| A | S53/54 데미지 코어 | ✅ 완료 | (uncommitted) |
| B | S62/63 Damage Textpopup | ✅ 완료 | (uncommitted) |
| C | S60/64/65 OnHit·Anim | ✅ 완료 | (uncommitted) |
| D | S55/56/58 Battle 진입 시퀀스 | 대기 | |
| E | S57/58/59 첫 몬스터 스폰·조우 | 대기 | |
| F | S59 Monster HP Bar 희귀도 | 대기 | |
| G | S66 몬스터 사망 | 대기 | |
| H | S67 플레이어 사망·리스폰 | 대기 | |
| I | S68 Level Up | 대기 | |
| J | S69 도중 이탈 | 대기 | |

---

## Phase A — 데미지 코어 (S53/54)

**완료**: 2026-05-05

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Utils/Define.cs` | `CreatureRarity` (Common/Rare/Boss), `AttackType` (Physical/Magic) enum 추가 |
| `Assets/@Scripts/Utils/BattleMath.cs` | **신설**. 기획서 #1 데미지 식의 단일 진입점 |
| `Assets/@Scripts/Data/Data.Contents.cs` | `CreatureData`에 `DmgAmp`/`Rarity`/`MonsterAttackDelay`/`EncounterRate`/`InterestRate`/`MaxKillCount` 추가 (기본값으로 후방호환) |
| `Assets/@Scripts/Controllers/Creature/CreatureController.cs` | `DmgAmp`/`Rarity`/`MonsterAttackDelay` 필드. `TakeHit(BattleMath.DamageResult)` 신설. 기존 `OnDamaged(float)`는 `BattleMath.FromRawDamage`로 감싸는 shim |
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `AutoAttack`이 `BattleMath.Resolve` 사용. 콤보 피해증폭은 `DmgAmp`에 일시 가산. `OnDamaged` override → `TakeHit` override |
| `Assets/@Scripts/Controllers/Creature/MonsterController.cs` | `SetInfo`가 새 데이터 컬럼 매핑. `TryAttack` cooldown을 `MonsterAttackDelay`로 변경 + `BattleMath.Resolve` 사용. `TakeHit` override |

### 핵심 신설 API

- `BattleMath.Resolve(attacker, defender, AttackType, skillMul = 1f)` → `DamageResult`
  - `Resolve` 한 곳에 명중 dice / BASE / DMG / FINAL 모든 식이 모임
- `BattleMath.FromRawDamage(int damage, bool isCrit = false)` → `DamageResult`
  - 사전 계산된 데미지(투사체 등)를 우회 적용용
- `CreatureController.TakeHit(BattleMath.DamageResult, CreatureController attacker = null)`
  - 결과를 받아 HP 차감/사망/시각 처리

### 슬라이드 #1 식 매핑

| 기획 단계 | 코드 위치 |
|---|---|
| 명중 dice | `BattleMath.Resolve` 첫 if |
| BASE = ATK × skillMul × (crit ? CritDmg/100 : 1) | `BattleMath.Resolve` 2-a |
| DMG = BASE − BASE×(저항/100) − 방어력 | `BattleMath.Resolve` 2-b |
| FINAL = DMG × (1 + DmgAmp/100) × Random(0.8, 1.2), floor 1 | `BattleMath.Resolve` 2-c |
| HP 차감, clamp 0, 사망 처리 | `CreatureController.TakeHit` |
| Monster 공격속도 → 공격대기시간 | `MonsterController.TryAttack`이 `MonsterAttackDelay` 사용 |

### 롤백 시 주의

- `OnDamaged(float)` → `TakeHit(DamageResult)`로 갈아탔지만, **shim은 살아있음**. ProjectileController가 여전히 `OnDamaged(float)`를 호출 → shim이 `FromRawDamage`로 감싸서 `TakeHit`로 라우팅.
- 기존 JSON에 새 필드 없어도 default value로 채워짐. JSON 깨지지 않음.
- Player/Monster의 `OnHpChanged` 이벤트는 `TakeHit` override에서만 발화. 만약 `OnDamaged(float)` 직접 호출 경로(투사체 등)에서 HP UI 갱신이 필요하면 추가 발화 필요.

### 의도적으로 안 건드린 부분

- `_anim.SetTrigger("OnDamaged")` (UpdateAnimation 안 dead code) → Phase C에서 isHit Anim 정리 시 같이 처리
- `CoFlashDamage` 0.1초 빨강 → Phase C에서 슬라이드 64 스펙(0.3초 색 변환)으로 교체
- ProjectileController는 shim 경로 → 스킬 시스템 손볼 때 `BattleMath` 직접 사용으로 마이그레이션 권장

---

## Phase B — Damage Textpopup 풀스펙 (S62/63)

**완료**: 2026-05-05

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/UI/DamageFont.cs` | 전면 재작성. `DamageFontMode` enum (Normal/Critical/Miss/CriticalCallout/Heal). 모드별 vertex gradient + outline. Spawn anim(Fade 0→1 + Scale 1.2→1 OutBack 0.3s) + Rise/FadeOut(1.5s) Sequence. Miss는 Scale anim 없음. |
| `Assets/@Scripts/Managers/Contents/ObjectManager.cs` | `ShowDamageFont` 시그니처 변경 (mode 인자 추가). `ShowMissPopup`, `ShowCriticalCallout`, `ShowHealFont` 추가. `SpawnDamageFont` 헬퍼 분리. CriticalDamageFont prefab 미사용(DamageFont 단일 prefab + mode 분기) |
| `Assets/@Scripts/Controllers/Creature/CreatureController.cs` | `TakeHit`에 Miss popup 호출 추가. 슬라이드 63 규칙(Monster 공격은 Damage Textpopup 호출 X) 적용. 치명타 시 Player 공격자 위치에 Critical callout. |

### 슬라이드 색상·anim 매핑

| 모드 | Top / Btm 색상 | Outline | Scale anim |
|---|---|---|---|
| Normal | FF0000 / FFFF00 | FFFFFF | 1.2→1 OutBack |
| Critical | FF0000 / FF00FF | FFFFFF | 1.2→1 OutBack |
| Miss | 000000 / E9E9E9 | FFFFFF | **없음** |
| CriticalCallout | FF0000 단색 | 000000 | 1.2→1 OutBack |
| Heal | 4EEE6F / 1A9600 | FFFFFF | 1.2→1 OutBack |

### Damage Textpopup 호출 룰 (슬라이드 63 정확 반영)

- Monster → Player 공격 시 → **Damage Textpopup 호출 X** (HP 깎이는 것만 보임)
- Player → Monster 공격 시 → 피격자(Monster) 위치에 Damage Textpopup
- Player 치명타 → 추가로 공격자(Player) 위치에 "Critical!" / "치명타!" callout
- Miss(공격자 무관) → 피격자 위치에 "MISS" popup
- 언어: `Managers.Game.Language` 기반 자동 분기 (Critical callout)

### 의도적으로 안 건드린 부분 (Phase B 범위 외)

- `dmg_txt_critical_additive` 글로우(Outline FF9900 추가 외곽선 덧대기) → 후속
- Hit Effect / Crit Effect / Miss Sound prefab → 후속 (자산 부재)
- DamageFont prefab의 폰트 자산은 이미 `RiaSans-Bold SDF` (기획 스펙 일치) — 변경 X
- CriticalDamageFont prefab 사용 중단(미삭제) — 안전상 유지, 정리는 후속 PR에서

### 롤백 시 주의

- `ShowDamageFont` 시그니처가 바뀜: `(pos, float damage, Transform parent, bool isCritical)` → `(pos, int damage, DamageFontMode mode, Transform parent)`. 외부 호출자가 추가되면 새 시그니처 사용.
- DamageFont prefab은 변경 안 됨. 폰트 색상은 prefab의 Vertex Gradient 설정과 무관 (런타임에 덮어씀).

---

## Phase C — OnHit·Anim 정합성 (S60/64/65)

**완료**: 2026-05-05

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/Creature/CreatureController.cs` | `using DG.Tweening` 추가. 기존 `CoFlashDamage()`(0.1초 빨강 고정)를 제거하고 가상 메서드 `PlayHitFeedback()` 도입. 기본 구현 = 슬라이드 64 Monster 스펙 (`(1, 0.2, 0.2)` 0.3초 변색 → 0.3초 OutQuad 흰색 복귀). `_onHitCo` 추적 필드 + 진행 중 피드백 자동 재시작. `OnDead`에 cleanup 추가 (DOKill + Coroutine stop) |
| `Assets/@Scripts/Controllers/Creature/MonsterController.cs` | `PlayHitFeedback` override — `State == Attack`이면 색 변환 skip (슬라이드 64 "Attack 도중 OnHit Anim 변경 X") |
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `PlayHitFeedback` override — `_anim.CrossFade("OnHit", 0.05f, -1, 0)`로 OnHit 클립을 처음부터 재생. `CoReturnAfterOnHit`이 슬라이드 65의 OnHit 길이 0.35s 후 State를 Idle로 복귀 → AutoAttack이 cooldown 따라 자연 재개 |

### 슬라이드 매핑

| 기획 항목 | 코드 |
|---|---|
| Player: Idle/Attack/OnHit 어느 상태든 피격 시 OnHit 처음부터 (S60/64) | `PlayerController.PlayHitFeedback` `CrossFade("OnHit", ..., 0f)` |
| Monster: Idle 중 피격 → 색 변환 (1,0.2,0.2) 0.3s → 0.3s OutQuad 흰색 (S64) | `CreatureController.CoColorFlashOnHit` |
| Monster: Attack 중 피격 → OnHit Anim/색 변환 X (S64) | `MonsterController.PlayHitFeedback` early-return on `State.Attack` |
| OnHit 도중 또 피격 시 처음부터 (S64) | 기존 코루틴 Stop + `_sprite.DOKill()` 후 재시작 |
| Player Attack 1초 / OnHit 0.35초 (S65) | `CoReturnAfterOnHit` `WaitForSeconds(0.35f)`. Attack 1초는 Animator clip 측에서 |

### Animator 의존성 (검증 시 확인 필요)

- Player Animator에 **"OnHit" 상태**가 있어야 `CrossFade("OnHit", ...)`가 동작.
- 없으면 콘솔에 warning + 색 변환/anim 없이 무시되고 그래도 게임은 동작 (HP/Damage popup은 정상).
- Player 기존 Animator는 "Idle"/"Run"/"Attack"만 보유 가능성 높음 → Phase C 검증 시 Player prefab의 Animator Controller 열어서 OnHit 상태 추가/연결 필요.
- OnHit 클립 길이는 **0.35s 권장** (슬라이드 65). 코드의 0.35s 대기와 일치시킬 것.

### 의도적으로 안 건드린 부분

- `_anim.SetTrigger("OnDamaged")` (CreatureController.UpdateAnimation 안 dead code) — 호출 경로 없음, 정리 후순위
- `State.OnDamaged` enum 값 — 사용처 없지만 enum 자체는 유지
- AutoAttack cooldown 처리 — OnHit 인터럽트 후 cooldown은 그대로 (Time.time 기준), 자연 재개

### 롤백 시 주의

- `CoFlashDamage()`는 완전히 제거됨. 다른 곳에서 직접 호출하지 않았으므로 영향 없음.
- `PlayHitFeedback` override가 없는 신규 CreatureController 서브클래스는 자동으로 기본(Monster 스타일 색 변환)을 받음. 다른 동작이 필요하면 override 필요.
- 진행 중인 OnHit 코루틴은 OnDead 시 정리됨 — 죽은 직후 색이 빨강에 머무는 버그 없음.

---

## Phase D — Battle 진입 시퀀스 (S55/56/58)

(아직 시작 전)
