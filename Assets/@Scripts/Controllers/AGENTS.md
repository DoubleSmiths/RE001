<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Controllers

## Purpose
게임 월드에 존재하는 엔티티(크리처, 투사체)와 전투 씬 연출(배경 스크롤, 몬스터 스폰, 카메라 해상도)을 담당하는 컨트롤러 레이어. `BaseController`가 모든 엔티티 컨트롤러의 공통 루트로, `Awake()`에서 `Init()`을 1회 호출하는 초기화 가드(`_init` flag)와 `ObjectType` 식별자를 제공한다. 크리처 계층(`BaseController → CreatureController → Player/Monster/Companion`)은 `Creature/` 하위 디렉터리에 있고, 이 디렉터리 직속 파일들은 투사체·배경·스포너 같은 비크리처 컨트롤러와 씬 유틸을 담는다. 전투 씬은 "플레이어 위치 고정 + 배경/몬스터가 좌측으로 스크롤"하는 방식(기획서 슬라이드 58)으로 동작하며 `BackgroundScroller`와 `MonsterController.ScrollLeft`가 같은 Ground 속도(5 units/sec)를 공유한다.

## Key Files
| File | Description |
|------|-------------|
| `BaseController.cs` | 모든 컨트롤러의 루트. `Awake()→Init()` 1회 초기화 가드(`_init`), `ObjectType { get; protected set; }` 제공. `virtual bool Init()`은 이미 초기화됐으면 false 반환 — 서브클래스는 `if (!base.Init()) return false;` 패턴 필수 |
| `BackgroundScroller.cs` | 전투 배경 레이어 무한 스크롤(슬라이드 56/58). Start에서 각 자식 SpriteRenderer를 복제(A/B 2장)해 좌측 이동+랩핑. `_layerSpeeds = {1,2,3,4,4,5}`(BG1..5+Ground), 배열 초과 자식은 마지막 값 fallback. `IsScrolling` 기본 false — PlayerController가 engaged 여부에 따라 on/off 제어 |
| `BattleSpawner.cs` | 전투 구역 몬스터 주기 스폰. Inspector에서 `monsterIds`/`spawnInterval`/`maxMonsters`/`spawnPoints` 설정. `Managers.Object.MonsterTransform.childCount >= maxMonsters`면 스킵, 스폰은 `Managers.Object.SpawnMonster(dataId, pos)` 위임 |
| `CameraController.cs` | 파일명과 달리 실제 클래스는 `FixedResolution`. 1440x3040 기준 세로 화면 letterbox/pillarbox 처리(camera.rect 조정) + `OnPreCull`에서 검은색 GL.Clear |
| `ProjectileController.cs` | `BaseController` 상속, `ObjectType.Projectile`. `SetInfo(startPos, dir, damage, speed, range)`로 초기화 후 직선 비행, 사거리 초과 또는 `MonsterController`와 OnTriggerEnter2D 시 `monster.OnDamaged(_damage)` 호출 후 `Managers.Resource.Destroy(gameObject)`로 풀 반환 |

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
- **파일명 ≠ 클래스명 주의**: `CameraController.cs` 안의 클래스는 `FixedResolution`이다. 클래스명 검색으로 찾을 것.
- `using static Define;`으로 enum(`ObjectType`, `State` 등)을 짧게 참조하는 것이 이 디렉터리의 관례.

### Common Patterns
- **Init 1회 가드**: `_init` bool + `virtual bool Init()` — 중복 초기화 방지 및 서브클래스 체이닝.
- **SetInfo 초기화**: 풀에서 꺼낸 오브젝트는 `SetInfo(...)`로 런타임 파라미터 주입 (`ProjectileController.SetInfo`).
- **A/B 더블 버퍼 무한 스크롤**: 스프라이트 원본+복제본 2장을 좌측 이동시키고 카메라 왼쪽 경계를 벗어나면 반대쪽 끝으로 재배치 (`BackgroundScroller`).
- **주기 실행**: 코루틴 대신 `Time.time` 기반 `_nextSpawnTime` 비교 (`BattleSpawner`).

## Dependencies

### Internal
- `Managers.Object` — SpawnMonster, MonsterTransform (BattleSpawner, ProjectileController 간접)
- `Managers.Resource` — Destroy (ProjectileController)
- `Utils/Define` — `ObjectType`, `State` enum (`using static Define`)
- `Creature/MonsterController` — ProjectileController의 피격 대상 타입

### External
- UnityEngine만 사용 (이 디렉터리 직속 파일에는 DOTween 등 외부 패키지 참조 없음 — DOTween은 `Creature/` 하위에서 사용)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
