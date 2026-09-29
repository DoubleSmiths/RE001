<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# Controllers

## Purpose
게임 월드에 존재하는 엔티티(크리처, 투사체)와 전투 씬 연출(배경 스크롤, 몬스터 스폰, 카메라 해상도)을 담당하는 컨트롤러 레이어. `BaseController`가 모든 엔티티 컨트롤러의 공통 루트로, `Awake()`에서 `Init()`을 1회 호출하는 초기화 가드(`_init` flag)와 `ObjectType` 식별자를 제공한다. 크리처 계층(`BaseController → CreatureController → Player/Monster/Companion`)은 `Creature/` 하위 디렉터리에 있고, 이 디렉터리 직속 파일들은 투사체·배경·스포너 같은 비크리처 컨트롤러와 씬 유틸을 담는다. 전투 씬은 "플레이어 위치 고정 + 배경/몬스터가 좌측으로 스크롤"하는 방식(기획서 슬라이드 58)으로 동작하며 `BackgroundScroller`와 `MonsterController.ScrollLeft`가 같은 Ground 속도(5 units/sec)를 공유한다.

## Key Files
| File | Description |
|------|-------------|
| `BaseController.cs` | 모든 컨트롤러의 루트. `Awake()→Init()` 1회 초기화 가드(`_init`), `ObjectType { get; protected set; }` 제공. `virtual bool Init()`은 이미 초기화됐으면 false 반환 — 서브클래스는 `if (!base.Init()) return false;` 패턴 필수 |
| `BackgroundScroller.cs` | 전투 배경 레이어 무한 스크롤(슬라이드 56/58). Start에서 각 자식 SpriteRenderer를 복제(A/B 2장)해 좌측 이동+랩핑. `_layerSpeeds = {1,2,3,4,4,5}`(BG1..5+Ground), 배열 초과 자식은 마지막 값 fallback. `IsScrolling` 기본 false — PlayerController가 engaged 여부에 따라 on/off 제어 |
| `BattleSpawner.cs` | 전투 구역 몬스터 주기 스폰. Inspector에서 `monsterIds`/`spawnInterval`/`maxMonsters`/`spawnPoints` 설정. `Managers.Object.MonsterTransform.childCount >= maxMonsters`면 스킵, 스폰은 `Managers.Object.SpawnMonster(dataId, pos)` 위임 |
| `BossOutline.cs` | v2 스테이지 보스의 금빛 테두리(ADR0030). `MonsterController.MakeStageBoss`가 붙인다. 본체 스프라이트를 8방향 2텍셀씩 민 금색 실루엣 8장을 뒤에 그리고 LateUpdate에서 스프라이트·flipX·본체 알파를 따라간다. 재질 `BossOutline`(셰이더 `RE001/SpriteSilhouette` — Unity 6 스프라이트는 flipX·색을 `unity_SpriteProps`·`unity_SpriteColor`로 넘기므로 URP Sprite-Unlit 뼈대를 그대로 쓴다)은 Material 그룹 PreLoad |
| `FixedResolution.cs` | 1440x3040 기준 세로 화면 letterbox/pillarbox(camera.rect). **안전 영역(`Screen.safeArea`) 안에서** 계산(안드로이드 15+ 는 노치 아래까지 그린다), 모든 씬 메인 카메라에 자동 부착, 화면·안전 영역 변경 시 재계산, 여백은 `@LetterboxCamera` 가 검게 지운다(URP 는 OnPreCull 을 부르지 않는다). 계산 검사 `Test/Integration/Letterbox - 안전 영역 계산`. ADR0028 |
| `ProjectileController.cs` | `BaseController` 상속, `ObjectType.Projectile`. `SetInfo(startPos, dir, damage, speed, range)`로 초기화 후 직선 비행. 명중 처리 2경로(슬62): `SetAttacker(attacker, attackType, skillMul)`로 공격자 주입 시 `BattleMath.Resolve → monster.TakeHit`(명중/회피/치명 주사위 판정 경유), 미주입 시 레거시 `monster.OnDamaged(_damage)` 그대로. `SetInfo`가 풀 재사용 대비 attacker를 리셋하므로 `SetAttacker`는 SetInfo 다음에 호출. **현재 발사처 배선 0건 — SetAttacker 경로는 미사용 상태**(호출부 없음, 향후 원거리 공격용). 사거리 초과/명중 시 `Managers.Resource.Destroy(gameObject)`로 풀 반환 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Creature/` | 크리처 상속 계층 (CreatureController abstract + Player/Monster/Companion + AbilityRunner). 상세는 `Creature/AGENTS.md` 참조 |

## For AI Agents

### Working In This Directory
- **Init 가드 패턴**: `BaseController.Awake()`가 `Init()`을 호출한다. 서브클래스 `Init()`은 반드시 `if (!base.Init()) return false;`로 시작하고, 안에서 `ObjectType`을 설정한다 (예: `ProjectileController`는 `ObjectType.Projectile`).
- **오브젝트 파괴는 `Managers.Resource.Destroy(gameObject)`**: `ProjectileController`가 사거리 초과/명중 시 사용. 직접 `Object.Destroy` 금지 (풀 반환 처리 포함).
- **스폰은 ObjectManager 경유**: `BattleSpawner`는 직접 Instantiate하지 않고 `Managers.Object.SpawnMonster(dataId, pos)`를 호출하며, 몬스터 수 카운트도 `Managers.Object.MonsterTransform`(몬스터 부모 Transform)의 childCount로 판단한다.
- **전투 스크롤 속도 정합성**: `BackgroundScroller`의 Ground 속도(마지막 entry, 5)와 `Creature/MonsterController.cs`의 `GROUND_SCROLL_SPEED = 5f`는 같아야 몬스터가 지면과 함께 자연스럽게 흘러온다. 한쪽만 바꾸면 안 된다.
- **`BackgroundScroller.IsScrolling`은 외부(PlayerController.UpdateEngagementVisuals)에서 제어**된다. 이 클래스는 GameObject 이름 `"CH1BG"` / `"CH1BG(Clone)"`으로 조회되므로(PlayerController.GetBgScroller) BG prefab 이름 변경 시 그쪽도 수정 필요.
- **ProjectileController의 BattleMath 판정은 opt-in**: 발사처가 `SetInfo(...)` → `SetAttacker(attacker)` 순서로 호출해야 명중/회피 판정을 탄다(SetInfo가 attacker를 리셋). 2026-07 현재 `SetAttacker` 호출부는 코드베이스에 0건 — 새 투사체 발사 로직을 붙일 때는 레거시 `OnDamaged` 경로 대신 이 경로를 사용할 것.
- 예전 파일명 `CameraController.cs`(클래스 `FixedResolution`)는 파일명≠클래스명이라 GameScene 에 저작된 컴포넌트가 스크립트를 못 찾았다 → `FixedResolution.cs` 로 이름 변경(GUID 유지, 2026-09-29).
- `using static Define;`으로 enum(`ObjectType`, `State` 등)을 짧게 참조하는 것이 이 디렉터리의 관례.

### Common Patterns
- **Init 1회 가드**: `_init` bool + `virtual bool Init()` — 중복 초기화 방지 및 서브클래스 체이닝.
- **SetInfo 초기화**: 풀에서 꺼낸 오브젝트는 `SetInfo(...)`로 런타임 파라미터 주입 (`ProjectileController.SetInfo`).
- **A/B 더블 버퍼 무한 스크롤**: 스프라이트 원본+복제본 2장을 좌측 이동시키고 카메라 왼쪽 경계를 벗어나면 반대쪽 끝으로 재배치 (`BackgroundScroller`).
- **주기 실행**: 코루틴 대신 `Time.time` 기반 `_nextSpawnTime` 비교 (`BattleSpawner`).

### 스펙 출처(기획서 매핑)
이 디렉터리 직속 파일의 기획서 원전(주석의 "슬N" 근거). 전투 챕터(슬61~70)는 **기획서1**, 도감/미니몬/능력은 **기획서2**가 원전이며, 스펙 검증용 에디터 미러는 `Editor/QuestVerifyMenu.cs`다.

| File | 기획서 슬라이드 |
|------|----------------|
| `ProjectileController.cs` | 기획서1 슬62, 기획서2 슬40 |

- **알려진 의도적 상충 1건**: `BattleMath`의 최소 1 데미지 정책 vs 기획서1 슬14의 '하한 0' — 기획 확인 대기 중. 확정 전까지 양쪽 모두 수정 금지.

## Dependencies

### Internal
- `Managers.Object` — SpawnMonster, MonsterTransform (BattleSpawner, ProjectileController 간접)
- `Managers.Resource` — Destroy (ProjectileController)
- `Utils/Define` — `ObjectType`, `State`, `AttackType` enum (`using static Define`)
- `Utils/BattleMath` — Resolve (ProjectileController SetAttacker 경로)
- `Creature/MonsterController`, `Creature/CreatureController` — ProjectileController의 피격 대상/공격자 타입

### External
- UnityEngine만 사용 (이 디렉터리 직속 파일에는 DOTween 등 외부 패키지 참조 없음 — DOTween은 `Creature/` 하위에서 사용)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
