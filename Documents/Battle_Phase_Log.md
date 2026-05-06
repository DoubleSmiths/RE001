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
| D | S55/56/58 Battle 진입 시퀀스 | ✅ 완료 | (uncommitted) |
| E1 | S61 공격판정 키프레임 | ✅ 완료 | (uncommitted) |
| E | S57/58/59 첫 몬스터 스폰·조우 | ✅ 완료 | (uncommitted) |
| F | S59 Monster HP Bar 희귀도 | ✅ 완료 | (uncommitted) |
| G | S66 몬스터 사망 | ✅ 완료 | (uncommitted) |
| H | S67 플레이어 사망·리스폰 | ✅ 완료 | (uncommitted) |
| I | S68 Level Up | ✅ 완료 | (uncommitted) |
| J | S69 도중 이탈 | ✅ 완료 | (uncommitted) |

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

### Phase C 검증 중 발견된 회귀 + 픽스 (2026-05-06)

**증상**: Attack 클립이 시각적으로 안 보임. State `Idle → Attack` 직후 같은 프레임에 `Attack → Idle`이 찍히고 `anim=Idle` 그대로.

**원인**: `CoResetStateAfterAttack`의 `IsName("Attack")` 폴링이 CrossFade 직후 transition 단계에서 false를 반환해서 두 while 루프가 1프레임 만에 둘 다 빠지면서 즉시 State=Idle을 호출. 그 결과 Animator가 Attack state에 들어가지도 못함.

**픽스**: 폴링을 시간 기반 대기로 교체.
- `Assets/@Scripts/Controllers/Creature/PlayerController.cs`:
  - `Coroutine _attackResetCo` 필드 추가 (동시 실행 가드)
  - `CoResetStateAfterAttack` 본문을 `WaitForSeconds(ATTACK_CLIP_LENGTH = 1.0f)`로 단순화 (슬라이드 65 스펙)
  - 1초 대기 후 `if (State == State.Attack)` 가드로 OnHit/Idle 등 다른 이벤트가 State 바꿨으면 건드리지 않음 → Phase C `CoReturnAfterOnHit`과 충돌 방지
  - 새 Attack 시 기존 코루틴 stop 후 새로 시작

**검증 후 로그 패턴**:
```
[Player.State] Idle → Attack  (t=30.91, anim=Attack)   ← anim=Attack: 이전 cycle Animator 상태가 자연 연속
[Player.State] Attack → Idle  (t=31.91, anim=Attack)   ← 1초 후 정상 종료, 클립 재생 완료
```

---

## Phase D — Battle 진입 시퀀스 (S55/56/58)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/BackgroundScroller.cs` | 전면 리팩토링. `_baseScrollSpeed × _parallaxFactors` → 절대 속도 배열 `_layerSpeeds = {1, 2, 3, 4, 4, 5}` (슬라이드 58 BG1~5 + Ground). `IsScrolling` 기본 `false`로 변경 (배너 종료 시점에 외부에서 true로 set). 배열 길이 초과 자식은 마지막 entry로 fallback |
| `Assets/@Scripts/UI/Scene/UI_GameScene.cs` | `EnterAreaWithFade` 타이밍을 슬라이드 55 스펙으로 교체: Fade Out 0.5s / Fade In 1.0s. Battle 진입 시 Fade In 50%(절대 t=1.0)에 기존 `Managers.UI.ShowToast` 사용해 [지역명 UI] 표시 (UI_Toast prefab이 이미 슬라이드 55 스펙대로 구성됨 — title_btn_start ×3 + DOTweenAnimation Move). 배너 Move 종료(t=2.0)에서 `StartBattleSceneScroll` + `_currentBtnGroup.PlayEnterAnimation`. 비-Battle은 기존 동작 유지. `GetCurrentAreaName` 헬퍼(`TravelNodeDic` 조회 → Localize) 추가 |
| `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs` | `Init()`의 `Player.State = Moving` + `Player.StartBattle()` 제거 → `State = Idle`로 변경 (배너 동안 Player Idle 고정). `PlayEnterAnimation`에서 `ShowToast("산들바람 숲(입구)")` 제거 (배너가 대체). `StartBattle()` 호출은 그대로 유지 (배너 종료 시점에 자연스럽게 호출됨) |

### 진입 시퀀스 타임라인 (Battle 기준)

| 시각 | 이벤트 |
|---:|---|
| t=0.0 | EnterAreaWithFade 호출 → Fade Out 시작 |
| t=0.5 | 검은 화면. SwapAreaBtnGroup → UI_BattleBtnGroup.Init → ChangeMap → CH1BG spawn (`IsScrolling = false`). Player.State = Idle. Fade In 시작 |
| t=1.0 | Fade In 50% → `UI_AreaNameBanner.Spawn` → 패널이 y=195에서 OutBack으로 내려옴 |
| t=1.5 | Fade In 완료. 배너 Move 진행 중 |
| t=2.0 | 배너 Move 종료 → onInComplete 콜백 → `BackgroundScroller.IsScrolling = true` + `Player.State = Moving` (Run anim) + `_currentBtnGroup.PlayEnterAnimation` → `StartBattle()` → CoBattleLoop 시작 |
| t=2.5 | 지역명 텍스트 페이드아웃 시작 (1s) |
| t=3.0 | 배너 패널 페이드아웃 시작 (1s) |
| t=3.5 | 텍스트 완전 사라짐 |
| t=4.0 | 배너 패널 사라짐 → 자체 Destroy |

### 슬라이드 매핑

| 기획 | 코드 |
|---|---|
| Fade Out 0.5s / In 1.0s (S55) | `EnterAreaWithFade` Sequence Fade 길이 |
| Fade In 절반 시 [지역명 UI] (S55) | `seq.InsertCallback(1.0f, ...)` |
| title_btn_start ×3 (S55) | `UI_AreaNameBanner.Build` 패널 3개 spawn |
| Move d=1 OutBack From y=195 (S55) | `rt.DOAnchorPosY(0f, 1f).SetEase(OutBack)` |
| Panel Fade d=1 delay=2 (S55) | `seq.Insert(2f, panel.DOFade(0, 1f))` |
| Text Fade d=1 delay=1.5 (S55) | `seq.Insert(1.5f, _nameText.DOFade(0, 1f))` |
| 배너 In 종료 시 Player Idle→Run + BG scroll (S55/57) | `StartBattleSceneScroll` |
| Layer 별 속도 BG1=1 ... Ground=5 (S58) | `BackgroundScroller._layerSpeeds = {1,2,3,4,4,5}` |
| 진입 직후 Player Idle + Background 정지 (S57) | Init `State = Idle` + `IsScrolling = false` |

### 검증 중 발견된 이슈 + 픽스 (2026-05-06)

**증상**: 처음에 신설한 `UI_AreaNameBanner.cs` (프로그램으로 RectTransform/Image/TMP 빌드) 사용 시 배너가 너무 크고 정중앙에 표시됨.

**원인**: 기존에 이미 슬라이드 55 스펙대로 만들어진 `UI_Toast.prefab`이 존재 (StageTitle / StageTitle_1 / StageTitle_2 = title_btn_start ×3겹 + StageTitle_Txt + 적절한 사이즈/포지션 + DOTweenAnimation 시퀀스). 이를 모르고 직접 빌드해서 사이즈/앵커가 어긋남.

**픽스**: 신설 `UI_AreaNameBanner.cs` 삭제 → `EnterAreaWithFade`가 기존 `Managers.UI.ShowToast(areaName)`을 fade-in 50% 시점에 호출하도록 변경. 진입 완료 콜백은 `seq.InsertCallback(2.0f, ...)`로 별도 처리 (Toast Move 1초 후).

**교훈**: 기존 prefab/시스템 먼저 살펴보고, 슬라이드 스펙이 이미 자산화되었는지 확인 후 코드 작성.

### 검증 방법

1. Unity 컴파일 (Console 에러 없음 확인)
2. 마을 → 여정 → 탐험 지역 선택 → Battle 진입
3. 시퀀스가 슬라이드 스펙대로 진행되는지 시각 확인:
   - 페이드 검 → 흰 → 기존 토스트 배너가 적절한 위치/크기로 등장 (1초)
   - 토스트 정지 후 Player가 **달리기 모션** 시작
   - 배경이 좌측으로 스크롤 시작 (BG1~Ground 속도 차이)
   - 토스트 자체 fade-out은 prefab의 OnEnable Sequence가 처리

### 의도적으로 안 건드린 부분 (Phase E·F에서 처리)

- **CoBattleLoop 첫 spawn 타이밍**: 슬라이드 58 "Run 시작 6초 후 첫 몬스터 spawn"은 Phase E
- **몬스터 Ground와 동속 스크롤**: 슬라이드 58 "몬스터도 Ground 속도로 스크롤"은 Phase E
- **Object Layer (Player 앞)**: 슬라이드 56 — 자산 측에서 layer 추가 후 BG 자식 순서 끝에 두면 자동 처리 (현재 CH1BG는 BG1~5만)
- **Ground Layer 분리**: 현재 CH1BG는 BG layers 5개만. Ground layer 자산 추가 시 6번째 자식 자리에 두면 `_layerSpeeds[5]=5` 자동 적용
- **CoResetStateAfterAttack의 Idle 복귀**: Phase E에서 "전투 중엔 Run 유지"로 변경

### 롤백 시 주의

- BackgroundScroller `_baseScrollSpeed` 필드 제거됨. CH1BG prefab의 인스펙터 값 (이전에 설정했던 값)은 무시됨 — 새 `_layerSpeeds` 배열 기본값 사용. prefab의 인스펙터 값을 보존하고 싶으면 prefab 수정 필요.
- `IsScrolling` 기본 `false`로 변경됨. 다른 비-Battle 씬에서 BackgroundScroller 사용 중이면 외부에서 명시적으로 `true` 설정 필요.
- `UI_BattleBtnGroup.Init`에서 Player.StartBattle 호출 제거. 진입 직후 _inBattle=false 유지 (배너 종료 시점에 PlayEnterAnimation이 StartBattle 호출).
- `EnterAreaWithFade` 전체 길이 1.05s → 1.5s로 늘어남 (비-Battle도 마찬가지). 빠른 전환을 원하면 시퀀스 길이 조정 필요.

---

## Phase E1 — 공격판정 키프레임 (S61)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `_attackTarget` 필드 추가 (락온 타겟). `AutoAttack`에서 데미지 계산 전체 제거 → target 락온 + State=Attack만. 빈 메서드였던 `AttackTrigger()`에 BattleMath.Resolve + TakeHit + OnHitLanded 이동 |
| `Assets/@Scripts/Controllers/Creature/MonsterController.cs` | `AttackTrigger()` 메서드 신설 (구조 일치). `TryAttack`은 키프레임 자산 부재로 inline 호출 fallback 유지 — 자산 보강 시 inline 호출 제거 가능 |

### 슬라이드 61 매핑

| 기획 항목 | 코드 |
|---|---|
| 1. 일반/스킬/몬스터공격 모두 공격판정 순간 정의 | `AttackTrigger()` 메서드 + Animation Event 패턴 |
| 2. 공격판정 순간에 명중/회피 dice | `BattleMath.Resolve` (PlayerController.AttackTrigger / MonsterController.AttackTrigger 내부) |
| 3. 공격성공 시 공격자가 적을 [타격] | `_attackTarget.TakeHit(result, this)` |
| 4. 실패 시 빗나감 처리 | `BattleMath.Resolve`의 result.Hit=false → `TakeHit`이 Miss popup 표시 (Phase B) |
| 5. 투사체는 닿았을 때 공격판정 | `ProjectileController.OnTriggerEnter2D` (변경 없음, 기존 collision-based 유지) |

### Player 공격 타임라인

| 시각 | 이벤트 |
|---:|---|
| t=0.0 | AutoAttack — cooldown OK → `_attackTarget = nearest`, State=Attack, CrossFade("Attack") 큐에 들어감 |
| t≈0.05 | Attack 클립 재생 시작 |
| t=0.5 | Attack.anim 키프레임 → Animation Event AttackTrigger() 발화 → `BattleMath.Resolve` → 명중/치명/저항/방어 dice → `_attackTarget.TakeHit(result)` → DamageFont/MissPopup/CritCallout 표시 |
| t=1.0 | `CoResetStateAfterAttack` (Phase C) → State=Idle |
| t≈1.0 | 다음 AutoAttack cooldown 충족 → 새 락온/Attack |

### OnHit 인터럽트 시 동작 (슬라이드 60 자연스럽게 충족)

- 키프레임(0.5s) 도달 **전** 피격 → CrossFade(OnHit)로 Attack 클립 끊김 → AttackTrigger 발화 안 됨 → 데미지 미적용 (= "공격이 끊겨서 타격 실패")
- 키프레임 도달 **후** 피격 → 데미지는 이미 적용됨, OnHit 클립으로 시각 전환만 발생

### 엣지 케이스 처리 (`AttackTrigger`)

- `_attackTarget == null` → 무시 (방어적)
- `_attackTarget.State == Dead` → 락온 후 사망한 경우, 무시
- `State != Attack` → 락온 후 OnHit/Idle로 갈아탔으면 무시
- `dist > AtkRange` → 락온 후 몬스터/Player 이동으로 거리 이탈, silent miss (popup 없음)

### 의도적으로 안 건드린 부분

- **Monster Attack anim 키프레임**: 자산 측 작업 필요. 현재 `TryAttack`이 inline으로 `AttackTrigger()` 호출 → 시각 동기화는 안 되지만 데미지 로직은 동일. 자산에 Animation Event 추가 시 inline 호출 제거.
- **Projectile**: 슬라이드 61 #5 — 충돌 시 판정. 기존 `ProjectileController.OnTriggerEnter2D` 그대로.
- **Player AtkRange**: 락온 시점 + 키프레임 시점 양쪽에서 체크. 락온 시점 통과 후 키프레임에서 이탈 시 silent miss.

### 롤백 시 주의

- `_attackTarget` 필드는 PlayerController 내부 상태. 외부에서 참조 X.
- AutoAttack에서 데미지 계산 코드가 사라졌음 → 기존 코드를 유지하려면 AttackTrigger() 호출을 AutoAttack 끝에 추가하면 즉시 적용 모드로 회귀 (anim 동기화 잃음).
- 직업/스킬 추가 시: 다른 Attack anim도 같은 `AttackTrigger` 함수명 + 적절한 키프레임 시점으로 이벤트 등록 필요.

---

## Phase E — 첫 몬스터 스폰·조우 (S57/58/59)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/Creature/MonsterController.cs` | `ChaseTarget` 제거 → `ScrollLeft`로 교체 (Ground 동속 -5 좌측 이동, 슬라이드 58). `FixedUpdate`에서 첫 AtkRange 진입 시 `OnEngaged` 발화로 이동 (TryAttack에서 빠짐) — HP Bar가 첫 조우 즉시 호출되도록. `CoResetStateAfterAttack`이 `Moving` → `Idle`로 변경 (Slime Animator는 Run 상태 없음) |
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `Update`에 `UpdateEngagementVisuals` 추가. AtkRange 안 몬스터 검사 → 있으면 BG.IsScrolling=false + Player Idle (AutoAttack가 Attack로 전환), 없으면 BG.IsScrolling=true + Player Moving(Run). `HasMonsterInAtkRange` / `GetBgScroller` 헬퍼. 캐시된 `_cachedScroller`로 매 프레임 lookup 절약 |
| `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs` | `CoBattleLoop` 첫 spawn 6초 대기 + spawn 위치 `Player.x + 12` (슬라이드 58). 사망 후 다음 spawn까지도 6초 |

### 슬라이드 매핑

| 기획 | 코드 |
|---|---|
| Player 위치 고정 (S58) | PlayerController.MovePlayer 주석 처리됨, Player Y 그대로 |
| 몬스터 Ground와 동속 좌측 이동 (S58) | `MonsterController.ScrollLeft` linearVelocity -5 |
| Player Run 시작 6초 후 첫 spawn (S58) | `CoBattleLoop` 첫 `WaitForSeconds(6f)` |
| Player +12 X에 spawn (S58) | `spawnPos = Player.x + 12` |
| 몬스터 조우 시 즉시 전투 진입 (S59) | `MonsterController.FixedUpdate`에서 첫 AtkRange 진입 시 OnEngaged |
| 몬스터 조우 → BG scroll 정지 + Player Attack (S57) | `PlayerController.UpdateEngagementVisuals` |
| 몬스터 사망 → Run 복귀 + BG 재개 (S57) | UpdateEngagementVisuals가 매 프레임 자동 토글 |
| Monster HP Bar 호출 (S59) | `OnEngaged` → CoBattleLoop의 `UI_MonsterHPBar` spawn |

### 전투 진행 타임라인 (Phase D + E + E1 통합)

| 시각 | 이벤트 |
|---:|---|
| t=0.0 | Travel popup → EnterAreaWithFade(Battle) → Fade Out 0.5s |
| t=0.5 | 검은 화면 → CH1BG spawn (scroll=false), Player Idle |
| t=1.0 | Fade In 50% → ShowToast(지역명) (1초 Move 진입) |
| t=1.5 | Fade In 완료 |
| t=2.0 | Toast Move 종료 → BG scroll on, Player Run, CoBattleLoop 시작 (6초 대기 시작) |
| t=8.0 | 첫 몬스터 spawn at Player.x+12. Slime이 -5 속도로 좌측 이동 시작 |
| t≈10.5 | Slime이 Player AtkRange(1.5)에 진입 → OnEngaged → HP Bar 호출. Player Idle, BG 정지. AutoAttack 발사 (State=Attack) |
| t≈10.5 + 0.5 | Attack.anim 0.5s 키프레임 → AttackTrigger() → BattleMath.Resolve → Slime.TakeHit (데미지 popup) |
| t≈11.5 | Attack 클립 종료 → CoResetStateAfterAttack → State=Idle. UpdateEngagementVisuals: 여전히 engaged → State=Idle. AutoAttack 재발사 |
| ... | Slime HP가 0 될 때까지 반복 |
| t=X | Slime 사망 → 풀 반환. UpdateEngagementVisuals: 미engaged → BG scroll 재개, Player Run |
| t=X+6 | 다음 Slime spawn at Player.x+12 |

### 검증 방법

1. Unity 컴파일 (Console 에러 확인)
2. 마을 → 여정 → 탐험 진입
3. 배너 끝나고 Player Run 시작 후 6초 후 슬라임이 화면 오른쪽에서 등장
4. 슬라임이 좌측으로 이동해서 Player 근처에 도달 → Player 멈추고 검 휘두르기 시작 + BG 스크롤 정지 + 슬라임 위에 HP Bar 표시
5. 키프레임(검 휘두르는 모션 절반) 시점에 데미지 popup 등장
6. 슬라임 사망 후 BG 다시 스크롤 + Player Run 복귀
7. 6초 후 다음 슬라임 등장

### 의도적으로 안 건드린 부분

- **Monster HP Bar 희귀도 (Scale/Color/Material)**: Phase F. 현재는 일반(기본) 형태만.
- **Monster 사망 시 Die Clip + DOPlayBackwards HP Bar + 드롭 리스트 UI**: Phase G.
- **여러 몬스터 동시 spawn**: 현재 1마리씩만. 다중 몬스터 시 BG scroll 토글이 가까운 몬스터에만 반응 (가장 가까운 monster의 distance로 판정).
- **Object Layer (Player 앞)**: 자산에 layer 추가되면 BG prefab 자식 끝에 배치 → BackgroundScroller가 자동 처리 (Phase D의 fallback 인덱싱).

### 검증 중 발견된 이슈 + 픽스 (2026-05-06)

**증상**: Slime이 Player를 뚫고 지나감 (조우는 일어나는데 차단이 안 됨).

**원인**: 초기 Player/Monster prefab에 Collider2D/Rigidbody2D 없었음. 사용자가 추가했으나 **둘 다 Dynamic** Body Type → 서로 밀어내 Player가 좌측으로 슬슬 밀리고 Slime이 결과적으로 통과. 또한 코드 측 engagement는 거리 폴링이라 collider 사이즈와 어긋날 수 있었음.

**픽스**:
- `MonsterController.cs`: 거리 기반 engagement 제거 → `OnCollisionEnter2D`에서 Player 충돌 시 `_hasEngaged=true` + vel=0 + OnEngaged 발화 (슬라이드 59 의도). `HasEngaged` public getter 추가.
- `PlayerController.cs`: `HasMonsterInAtkRange`가 거리 → `MonsterController.HasEngaged` flag로 변경. Collider 사이즈 영향 안 받음.

**Inspector 설정 권장 (사용자 측)**:
- Player Rigidbody2D: Body Type = **Kinematic** (Player 고정)
- Monster Rigidbody2D: Body Type = **Dynamic + Gravity Scale = 0** (코드 vel 제어)
- 둘 다 BoxCollider2D **Is Trigger = OFF**
- Layer Collision Matrix Player ↔ Monster 켜짐 (기본 default)

### 롤백 시 주의

- Monster movement이 ChaseTarget(Player 추격)에서 ScrollLeft(절대 좌측 이동)로 바뀜 → Player가 위치 이동하면 monster가 따라가지 않음 (slide 58 spec: Player 고정 가정).
- `Update`에 매 프레임 monster 검색 + scroller 캐시 로직 추가됨. 다중 monster (10+) 환경이라면 spatial partition으로 최적화 필요.
- `_cachedScroller`는 GameObject 파괴 시 자동으로 Unity null로 해석되어 GetBgScroller가 재탐색 → 안전.
- 배틀 루프의 6초 대기는 Player가 실제로 Run 모션이든 아니든 그냥 6초 카운트. CoBattleLoop은 `StartBattle`(banner 종료 시) 호출 시점부터 카운트 시작.

---

## Phase F — Monster HP Bar 희귀도 (S59)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/UI/SubItem/UI_MonsterHPBar.cs` | `SetInfo`에 `ApplyRarity` + `PlayOpenAnimation` 호출 추가. monster.Name을 `Util.Localize`로 통과 (key든 literal이든 안전). |

### 슬라이드 59 매핑

| 등급 | Scale | Color | Material |
|---|---:|---|---|
| Common | 1.0 | FFFFFF (기본) | null (Image 기본) |
| Rare | 1.15 | FFD400 | `Path_Image_Knight` |
| Boss | 1.3 | FF4300 | `Path_Image_BeastLord` |

### Open Animation

- prefab의 **기존 DOTweenAnimation** 사용 (`GetComponent<DOTweenAnimation>().DORestart()`)
  - 슬라이드 59 스펙대로 이미 구성됨: Move 0.5s OutBack From (0,0,0), autoPlay=false
  - 희귀도 Scale은 별개로 `transform.localScale` 직접 set (DOTweenAnimation은 position 애니만 함)
- 코드 측 DOScale 시퀀스는 폐기 (메모리: feedback_dotween_animation.md 원칙 준수)

### Material 로딩

- Addressables 그룹 `Material`에 등록 + `PreLoad` 라벨 → `Managers.Resource.Load<Material>("Path_Image_Knight")` / `"Path_Image_BeastLord"`로 로드
- 로드 실패 시 (이름 오타 등) Color만 적용하고 material은 기본 유지 (방어적)

### 검증 방법

1. CreatureData JSON에서 Slime의 `Rarity` 필드를 `"Common"`/`"Rare"`/`"Boss"`로 변경하면서 테스트
2. 일반: 흰색 + 기본 사이즈
3. 희귀: 노랑 + 살짝 큰 사이즈 + Knight material 효과 (외곽선/광택 등)
4. 우두머리: 빨강 + 더 큰 사이즈 + BeastLord material 효과
5. 모든 등급에서 spawn 시 scale-in 연출 (OutBack)

### 의도적으로 안 건드린 부분

- **HP Bar Playbackwards 사망 연출**: Phase G (슬라이드 66 — 몬스터 사망 시 HP Bar DOPlayBackwards)
- **Material 적용 위치**: 현재 Frame 이미지에만 적용. 슬라이드 spec이 명확치 않아 핵심 frame에만. 다른 부품에 추가로 적용해야 한다면 `ApplyRarity` 확장 필요.

### 롤백 시 주의

- `transform.localScale = Vector3.zero`로 시작하므로, 이전에 다른 코드가 prefab 인스턴스화 직후 scale을 set하던 게 있다면 덮어씀. 이상한 사이즈로 보이면 Open Anim 시점 또는 prefab 기본 scale 충돌 의심.
- Material 변경은 인스턴스 frame.material에만 영향. 같은 material이 다른 곳에서 공유되어도 영향 없음 (Image.material은 인스턴스 별 setter).
- monster.Name이 Localization key가 아니라 literal일 경우, `Util.Localize`는 key를 그대로 반환 → 시각적 영향 없음.

---

## Phase G — 몬스터 사망 처리 (S66)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/Creature/CreatureController.cs` | State setter `case Dead`: 빈 break → `CrossFade("Die", 0.1f, -1, 0)` (Player/Monster 공통) |
| `Assets/@Scripts/Controllers/Creature/MonsterController.cs` | `OnDead`의 placeholder `transform.DOScale(Vector3.zero, ...)` 제거 → State=Dead가 Die clip 트리거. `CoDestroyAfterDie(0.5f)` 코루틴으로 풀 반환 (HP Bar Playbackwards와 동기) |
| `Assets/@Scripts/UI/SubItem/UI_MonsterHPBar.cs` | `cur<=0` 분기 — 즉시 destroy → `PlayCloseAnimation`. prefab의 DOTweenAnimation을 `DOPlayBackwards`로 역재생 + duration 후 destroy |

### 슬라이드 66 매핑

| 기획 항목 | 코드 |
|---|---|
| 1. Die Clip 재생 | `State = Dead` → CreatureController setter CrossFade("Die") |
| 1. Monster HP Bar Playbackwards | `UI_MonsterHPBar.PlayCloseAnimation` → `anim.DOPlayBackwards()` |
| 1. 대기 상태 진입 | Player.HasMonsterInAtkRange가 Dead 상태 monster를 skip → BG scroll 재개 + Player Run 자동 |
| 2. 드롭 주사위 + 아이템 리스트 UI | 기존 `MonsterController.OnDead`의 DropItemId 분기 + `OnItemDropped` 이벤트 → `UI_BattleBtnGroup.HandleMonsterItemDropped` (이미 wired) |
| 3. Exp / 레벨업 체크 | 기존 `_target.GainExp(_data.TotalExp)` |
| 4. Kill count + 도감 해금 | **(deferred)** — Bestiary 시스템 구축 후 |
| 5. 관심 체크 + 대기열 | **(deferred)** |
| 6. Die Sound | **(deferred)** — Sound 자산 미정 |

### 사망 시퀀스 타임라인

| 시각 | 이벤트 |
|---:|---|
| t=0 | Player 공격 → Monster.TakeHit → base.TakeHit → Hp<=0 → `OnDead()` |
| t=0 | base.OnDead → State=Dead → CrossFade("Die"). MonsterController.OnDead → 보상/드롭 → `CoDestroyAfterDie(0.5f)` 시작 |
| t=0 | Monster.TakeHit override가 base 후 `OnHpChanged.Invoke(Hp=0, MaxHp)` → UI_MonsterHPBar.handler → `PlayCloseAnimation` → DOPlayBackwards |
| t≈0.17 | Slime Die clip 종료 (자체 길이 0.17s) |
| t=0.5 | Monster GameObject 풀 반환. HP Bar Destroy. Player.HasMonsterInAtkRange = false (Dead skip) → BG scroll on, Player Run |
| t=0.5+6 | CoBattleLoop의 `WaitForSeconds(6f)` 만료 → 다음 Slime spawn at Player.x+12 |

### 검증 방법

1. 슬라임 처치 시 슬라임이 Die 클립 재생 (1프레임 깜빡 X 자연스러운 사망 모션)
2. HP Bar가 prefab의 OutBack Move 역방향으로 위치 (0,0,0)으로 이동하며 사라짐
3. 사망 직후 BG 스크롤 재개, Player Run 모션 시작
4. 6초 후 다음 슬라임 등장
5. 드롭 아이템이 있는 경우 (CreatureData에 DropItemId 설정) 드롭 리스트 UI에 표시

### 의도적으로 안 건드린 부분 (Bestiary phase에서 처리)

- **Kill count 누적**: GameData에 `Dictionary<int, int> MonsterKillCounts` 추가 + 도감 진행도% 계산
- **해금 요소 체크**: 슬라이드 54의 [진행도%] 골드/별조각/아이템 획득 확률 해금
- **관심 체크**: 슬라이드 54의 InterestRate dice + 대기열 추가 (도감 페이지 UI 필요)
- **Die Sound**: 자산 추가 후 `Managers.Sound.Play(...)` 호출

### 롤백 시 주의

- Monster Animator에 "Die" state 없으면 CrossFade("Die") 무시되고 단순히 0.5초 후 Destroy. 기존 placeholder DOScale 시각 효과는 사라짐.
- Player Animator에도 "Die" state 있어야 Phase H에서 같이 작동. Player_01 controller에 이미 Die 상태 존재 확인됨.
- HP Bar prefab에 DOTweenAnimation 없는 경우 PlayCloseAnimation은 즉시 destroy로 fallback.

---

## Phase H — 플레이어 사망·리스폰 (S67)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/UI/SubItem/UI_MonsterHPBar.cs` | `PlayCloseAnimation` 가시성 private → public (Player 사망 시 외부에서 강제 호출 가능) |
| `Assets/@Scripts/Controllers/Creature/MonsterController.cs` | `TryAttack` 시작에 `_target.State == Dead` 가드 추가 — Player 사망 후 monster가 시체에 계속 공격하는 버그 차단 |
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `OnDead` 사망 시퀀스 구현. `StopBattle`에 HP 회복 + State=Idle 추가 (HOME/사망/도중이탈 공통 리스폰 처리) |

### 슬라이드 67 매핑

| 기획 항목 | 코드 |
|---|---|
| 1-1. HP 0 이하로 안 내려가게 | `CreatureController.TakeHit`에서 이미 `if (Hp < 0) Hp = 0;` 처리 |
| 1-2. Monster Idle 고정 | `PlayerController.OnDead`에서 `monsterRoot` 자식들 중 살아있는 것 모두 `State = Idle`. `MonsterController.TryAttack`도 dead target 가드로 추가 공격 차단 |
| 1-3. Monster HP Bar DOPlayBackwards | `FindObjectsByType<UI_MonsterHPBar>()` → 각자 `PlayCloseAnimation()` |
| 1-4. Player Die Anim | `base.OnDead()` → State=Dead → CrossFade("Die") (Phase G에서 추가된 setter case) |
| 1-5. 1초 후 Fade IN/OUT | `CoRespawnInVillage(1f)` → `EnterAreaWithFade(Village1)` (Fade 0.5/1.0 자체 처리) |
| 1-6. 마을 리스폰 ([HOME] 동등) | EnterAreaWithFade → SwapAreaBtnGroup → UI_BattleBtnGroup.OnDestroy → EndBattle → `Player.StopBattle()` → HP 회복 + Idle |

### 사망 시퀀스 타임라인

| 시각 | 이벤트 |
|---:|---|
| t=0 | Player Hp ≤ 0 → `TakeHit` → `OnDead` |
| t=0 | base.OnDead → State=Dead → Player Die clip 재생 |
| t=0 | 모든 활성 monster `State = Idle`. 모든 활성 UI_MonsterHPBar `PlayCloseAnimation` (DOPlayBackwards 0.5s) |
| t=0.5 | HP Bar 사라짐 |
| t=1.0 | `CoRespawnInVillage` 만료 → `EnterAreaWithFade(Village1)` |
| t=1.0 | Fade Out 0.5s |
| t=1.5 | 검은 화면 → SwapAreaBtnGroup: UI_BattleBtnGroup destroy → OnDestroy → EndBattle → Player.StopBattle (HP 회복 + Idle). 마을 BG 스폰. UI_Village1BtnGroup 등장 |
| t=1.5 | Fade In 1.0s |
| t=2.5 | Fade 완료 → 마을 정상 동작 |

### 검증 방법

1. 전투 진입 후 일부러 죽을 때까지 Player가 공격 안 받음 (디버그용으로 Player MaxHp 낮춰서 테스트)
2. Player HP가 0이 되면:
   - Monster가 즉시 정지 (Idle 모션)
   - Monster HP Bar가 위에서 아래로 밀려나며 사라짐 (DOPlayBackwards)
   - Player가 Die 모션 재생
3. 1초 뒤 화면 검게 페이드
4. 마을 화면으로 복귀
5. 마을 진입 후 Player HP 풀 회복 + Idle 상태 (Run 모션 X)

### 의도적으로 안 건드린 부분

- **Game Over UI**: 슬라이드 67엔 명시적으로 없음. HOME 버튼처럼 그냥 마을로 돌아가는 흐름. 별도 "사망" 텍스트나 토스트 추가는 후속.
- **사망 페널티**: 골드/경험치 손실 등은 슬라이드 67에 없어서 미반영.
- **Continue 시스템**: 광고 보고 부활 같은 것도 미구현.

### 롤백 시 주의

- `StopBattle`이 항상 HP 회복 + State=Idle. 이전엔 State=Moving이었음. Village1BtnGroup이 Run 애님 기대했다면 영향. 하지만 마을 진입 시 UI_Village1BtnGroup이 자체 enter 애님으로 Player 시각 처리 → 영향 작음.
- Village 진입 시 Player 항상 HP 풀 회복. HOME 버튼으로 복귀해도 회복됨 — 슬라이드 69도 같은 의도라 문제 없음. 만약 HP 유지하고 싶으면 StopBattle에 사망 분기만 회복으로 분리.
- `MonsterController.TryAttack`이 dead target 차단 → 만약 미니몬 시스템에서 monster가 다른 monster 공격하는 케이스 추가 시 _target 타입 확장 필요.

---

## Phase I — Level Up 연출 (S68)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `OnLevelUp` 이벤트 시그니처 `Action<int>` → `Action<int, int>` (newLevel, regenHp). `CheckLevelUp`이 RefreshStats 전 prevHp 캡쳐 → 회복량 계산해서 invoke |
| `Assets/@Scripts/UI/Scene/UI_GameScene.cs` | `OnLevelUp` 핸들러에 슬라이드 68 #2-1~#2-6 전부 처리: Level Up Text/Ef prefab spawn, heal popup, [상태] 버튼 [+] icon active+Punch. 이벤트 구독 람다 시그니처 갱신 |

### 슬라이드 68 매핑

| 기획 항목 | 코드 |
|---|---|
| 1. EXP ≥ required → Level up Event | `PlayerController.CheckLevelUp` 기존 while loop |
| 2-1. Level Up Text 팝업 | `Managers.Resource.Instantiate("UI_PlayerLevelUpTextPopup")` at Player position |
| 2-2. Level Up Effect | `Managers.Resource.Instantiate("Player Level Up Ef")` at Player position |
| 2-3. HP 회복 + 초록 텍스트 | `RefreshStats`가 Hp=MaxHp + `Managers.Object.ShowHealFont(pos, regenHp)` (Phase B의 Heal mode — 4EEE6F→1A9600 그라디언트) |
| 2-4/2-5. EXP/Level UI | 기존 `OnExpChanged` 핸들러 + `PlayerLevelText` 갱신 |
| 2-6. [상태] 버튼 [+] icon | `CharacterBtnNewIcon.SetActive(true)` + `DOPunchScale((0.2,0.2,0), 0.3, 10, 1).SetEase(OutQuad)`. 진행 중 punch 있으면 `DOKill`로 정리 후 재시작 (= "Restart") |

### 작동 흐름

```
1. 몬스터 사망 → MonsterController.OnDead → _target.GainExp(_data.TotalExp)
2. PlayerController.GainExp → CheckLevelUp
3. CheckLevelUp while loop:
   - Exp -= required
   - Level++
   - prevHp 캡쳐
   - RefreshStats — MaxHp/Atk/Def 갱신, Hp=MaxHp 자동 회복
   - regen = MaxHp - prevHp
   - OnLevelUp.Invoke(newLevel, regen)
4. UI_GameScene.OnLevelUp(newLevel, regen):
   - PlayerLevelText 갱신
   - Level Up Text 팝업 spawn at player position
   - Level Up Effect spawn at player position
   - 회복량 > 0 → ShowHealFont
   - CharacterBtnNewIcon SetActive(true) + DOPunchScale
```

### 검증 방법

1. 슬라임 처치하면서 EXP 채워서 Level Up 트리거
2. 시각 확인:
   - Player 위치에 "LEVEL UP" 텍스트 prefab 스폰 (prefab 자체 anim 자동 재생)
   - Player 위치에 Level Up Effect 파티클
   - Player 위에 초록 회복 popup (HP 회복량 표시)
   - 하단 [상태] 버튼의 [+] 아이콘이 active되며 펀치 스케일 (0.2,0.2,0) 진동

### 의도적으로 안 건드린 부분

- **Player Level Up Text/Ef prefab의 내부 anim**: prefab 자체의 DOTweenAnimation 또는 ParticleSystem이 알아서 처리. 스폰만 하면 됨. 만약 anim이 동작하지 않으면 prefab inspector에서 autoPlay 확인 필요
- **회복 텍스트 위치**: Player position 그대로. 슬라이드는 "HP Text 초록색 효과"라 Player HP Bar의 텍스트에 직접 색상 효과를 의미할 수도 있음 → 후속에서 PlayerHPBarText 색 깜빡임 추가 가능
- **여러 레벨 동시 상승 시**: while loop 내에서 OnLevelUp이 매번 invoke됨 → 텍스트/이펙트 여러 번 spawn. UX상 1번만 표시하고 싶으면 마지막 레벨로 묶을 수 있지만 현재는 단순 누적

### 롤백 시 주의

- `OnLevelUp` 이벤트 시그니처 `Action<int>` → `Action<int, int>` 변경. 다른 subscriber 추가 시 새 시그니처 사용 필요
- prefab 이름 `UI_PlayerLevelUpTextPopup`, `Player Level Up Ef`는 Addressable address 그대로. 이름 바뀌면 Instantiate 인자 동시에 갱신
- CharacterBtnNewIcon은 기존엔 StatusPoint > 0일 때 active 됨. 이제 Level Up도 active 트리거 → StatusPoint 시스템과 충돌할 가능성. 기존 RefreshCharacterNewIcon 호출 흐름 유지되므로 다음 RefreshCharacterNewIcon 호출 시 다시 SetActive(StatusPoint > 0)로 재계산. Level Up 직후 시각 효과는 보임. 일정 시간 후 재계산 시 StatusPoint=0이면 사라짐 — 의도된 동작인지 검증 필요

---

## Phase J — 전투 도중 이탈 (S69)

**완료**: 2026-05-06

### 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/@Scripts/Controllers/Creature/PlayerController.cs` | `_originalPos` 필드 추가 (`Init`에서 캡처). `StopBattle`에 `transform.DOKill` + position 원복 추가 |
| `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs` | `BeginExitTransit()` 신설 — monster Idle 고정 + BG scroll OFF + Player Run/DOMoveX(-10, 0.5s). `ExecuteReturnToVillage`에서 즉시 EndBattle 호출 제거 (OnDestroy가 처리) |
| `Assets/@Scripts/UI/Scene/UI_GameScene.cs` | `EnterAreaWithFade` 시작 직후 `_currentBtnGroup as UI_BattleBtnGroup` 검사 → `BeginExitTransit()` 호출 |

### 슬라이드 69 매핑

| 기획 항목 | 코드 |
|---|---|
| 1-1. Monster Idle 상태 고정 | `BeginExitTransit`: 모든 살아있는 monster `State = Idle` + `linearVelocity = Vector2.zero` |
| 1-2. Player Run + 화면 밖 좌측 이동 | `BeginExitTransit`: Player `State = Moving`(Run anim) + `DOMoveX(x-10, 0.5s, InQuad)` |
| 1-3. Map Scrolling OFF | `BeginExitTransit`: `BackgroundScroller.IsScrolling = false` |
| 1-4. Fade IN/OUT | `EnterAreaWithFade`의 기존 fade 시퀀스 (Phase D — Out 0.5 / In 1.0) |
| 1-5. HP Now → Max | `StopBattle` (Phase H에서 추가). UI_BattleBtnGroup.OnDestroy → EndBattle → StopBattle 자연 호출 |
| 위치 원복 | `StopBattle`의 `transform.position = _originalPos` |

### 도중 이탈 시퀀스 (HOME 버튼 기준)

| 시각 | 이벤트 |
|---:|---|
| t=0.0 | HOME 더블클릭 (확인) → `ExecuteReturnToVillage` → `EnterAreaWithFade(Village1)` |
| t=0.0 | EnterAreaWithFade 시작 — `_currentBtnGroup as UI_BattleBtnGroup` → `BeginExitTransit` |
| t=0.0 | BeginExitTransit: monster→Idle, BG scroll OFF, Player Run + DOMoveX 시작 (0.5s) |
| t=0.0 | Fade Out 0.5s 시작 (Player tween과 병렬) |
| t=0.5 | 검은 화면 도달. `SwapAreaBtnGroup(Village1)` → UI_BattleBtnGroup destroy → `OnDestroy` → `EndBattle` → `Player.StopBattle()` → HP=Max + State=Idle + DOKill + position=originalPos |
| t=0.5 | `EndBattle`이 monsters/HP bars/CH1BG 정리 → 마을 BG 스폰 → UI_Village1BtnGroup 등장 |
| t=0.5 | Fade In 1.0s 시작 |
| t=1.5 | Fade In 완료 → 마을 정상 |

### 시각 효과

- 페이드 아웃 동안 monster는 Idle 모션으로 정지, Player는 좌측으로 달려가며 화면 밖으로 사라짐
- 검은 화면에서 모든 정리 진행 (사용자에게 안 보임)
- 페이드 인 시 마을이 등장. Player는 원위치, HP 풀, Idle

### 죽음 케이스 호환성

Player가 죽어서 마을 복귀할 때 (Phase H의 CoRespawnInVillage):
- Player.OnDead → 1초 대기 → EnterAreaWithFade(Village1)
- BeginExitTransit 호출됨 — Player.State==Dead라 Player Run/DOMove 부분 skip (가드). Monster Idle + BG scroll OFF만 적용.
- 결과: Player Die 모션 유지, Monster Idle 멈춤, Fade Out 진행, 마을에서 부활 — Phase H에서 의도한 동작 유지

### 의도적으로 안 건드린 부분

- **Player off-screen 이동 거리 -10**: 카메라 ortho size에 따라 다를 수 있음. 카메라 left edge 직접 계산하면 더 정확하지만 일단 충분한 여유 (-10) 사용. 이슈 시 calc 변경
- **Player anim 방향**: 좌측 이동 시 sprite flip 없음. 슬라이드 spec에 spriteFlip 언급 없어 그대로 유지

### 검증 중 발견된 CurrentTravelAreaId 잔여 버그 + 픽스 (2026-05-06)

**증상**: HOME 버튼으로 마을 복귀 후 TravelPopup 재오픈 시 마지막 Battle 행선지가 초록색(현재 위치)로 표시됨.

**원인**: `Managers.Game.CurrentTravelAreaId`가 `TravelPopup.TravelToNode`에서만 set됨. HOME/사망 경로는 TravelPopup을 거치지 않아 갱신이 안 됨.

**픽스**: `EnterAreaWithFade`에서 target area가 Battle이 아닌 경우(=마을), `AreaType` enum 이름과 TravelNode의 `AreaType` 문자열을 매칭해서 `CurrentTravelAreaId`를 갱신. Village2/3 등 추후 마을 추가 시 코드 수정 없이 자동 동작.

### 검증 중 발견된 deferred-destroy 버그 + 픽스 (2026-05-06)

**증상**: HOME 버튼으로 마을 복귀 시 마을 BG가 안 보임 (Village1_Main_BG가 사라짐).

**원인**: Phase J에서 EndBattle 호출 시점이 SwapAreaBtnGroup **이후**로 옮겨졌는데, 기존 EndBattle은 cleanup loop에서 `Village1_*_BG`도 같이 destroy 하고 있었음. Unity `Object.Destroy`는 end-of-frame deferred → SwapAreaBtnGroup이 먼저 Village1BtnGroup 스폰 + SwitchBG로 새 마을 BG 스폰 → BattleBtnGroup OnDestroy가 같은 프레임 끝에 발화 → EndBattle이 방금 스폰된 마을 BG까지 destroy.

**픽스**: `EndBattle`의 cleanup loop에서 `Village1_*_BG` 항목 삭제. `CH1BG`(전투 전용 BG)만 destroy. Village BG cleanup은 이미 `UI_Village1BtnGroup.SwitchBG` 내부에서 처리되므로 중복 + 위험.

### 롤백 시 주의

- `ExecuteReturnToVillage`에서 즉시 EndBattle 호출 제거됨 → `EnterAreaWithFade`가 정상 호출되어 SwapAreaBtnGroup이 결국 EndBattle 트리거하는 흐름 의존. 만약 EnterAreaWithFade가 실패하면 EndBattle도 안 됨 → 이전엔 EndBattle 먼저 했으니 더 안전했음. 하지만 Phase J 의도(monster 유지)와 충돌해서 변경.
- Player `_originalPos`는 Init 시점 한 번만 캡처. 만약 게임 진행 중 Player position이 다른 절대값으로 옮겨져야 할 일이 생기면(예: 다른 마을 스폰 위치) `_originalPos`를 갱신하는 setter 추가 필요.
- `StopBattle`의 `transform.DOKill`은 Player의 모든 transform tween을 죽임. 다른 경로로 Player에 tween을 걸어둔 게 있으면 영향. Phase 이후 Player에 tween 추가 시 주의.

---

## 전체 Phase A~J 요약

| Phase | 슬라이드 | 핵심 변경 |
|---|---|---|
| A | 53/54 | `BattleMath.Resolve` 단일 데미지 식 + CreatureData 5개 필드 |
| B | 62/63 | `DamageFontMode` (Normal/Critical/Miss/Heal/CriticalCallout) gradient + outline + anim |
| C | 60/64/65 | `PlayHitFeedback` virtual + Player CrossFade("OnHit") + Monster 색 변환. CoResetStateAfterAttack 시간 기반 단순화 |
| D | 55/56/58 | Fade 0.5/1.0 + ShowToast 지역명 + BG Layer 절대 속도 + StartBattleSceneScroll |
| E | 57/58/59 | Player.x+12 6초 spawn + Monster 좌측 스크롤 + collision-based engagement (BoxCollider2D + Kinematic Player) |
| E1 | 61 | AttackTrigger Animation Event 키프레임 시점 데미지 적용 |
| F | 59 | Monster HP Bar 희귀도 (Common/Rare/Boss) Scale·Color·Material + DOTweenAnimation Open Anim |
| G | 66 | Die clip 재생 + HP Bar DOPlayBackwards + 0.5s 후 destroy |
| H | 67 | Player 사망: monster Idle/HP Bar close/1s 대기/Village 복귀/HP·State·position 원복 |
| I | 68 | Level Up Text/Ef prefab spawn + heal popup + [상태] 버튼 [+] icon Punch |
| J | 69 | BeginExitTransit (monster Idle + BG OFF + Player Run/화면밖) → SwapAreaBtnGroup의 EndBattle |

### 추가 도구 / 메모리

- `Assets/@Scripts/Editor/BattleTestMenu.cs` — Damage popup 5종 시각 검증 + BattleMath 100회 시뮬 + Reset Lv.1 / Force Level Up
- `Documents/Battle_Phase_Log.md` (이 문서) — 모든 phase 변경 내역
- `feedback_check_existing_prefabs_first.md` (메모리) — 기존 prefab 먼저 검색하는 습관 (UI_Toast 사례)
