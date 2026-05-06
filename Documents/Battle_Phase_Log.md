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

## Phase E — 첫 몬스터 스폰·조우 (S57/58/59)

(아직 시작 전 — Phase E1 키프레임 작업 후 진입)
