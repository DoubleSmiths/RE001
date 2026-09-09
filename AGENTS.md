# AGENTS.md

이 저장소에서 일하는 **모든 코딩 에이전트**(Codex, Claude Code 등)의 출발 문서다.
Codex 는 `AGENTS.md` 만 읽고 Claude Code 는 `CLAUDE.md` 를 읽는다 — 그래서 규칙의 원본은
여기 하나로 두고, `CLAUDE.md` 는 이 파일을 `@AGENTS.md` 로 가져가기만 한다. 고칠 때는 여기를 고친다.

RE001 — Unity 2D 사이드뷰 액션 RPG (모바일 세로 화면, 픽셀 아트). 스크립트는 전부 `Assets/@Scripts` 아래.

---

## 0. 협업 · AI 에이전트 컨벤션

브랜치는 짧게: `feat/…`, `game/…`, `data/…`, `fix/…`.
작업 단위마다 잘게 커밋한다 — 커밋 기록이 곧 작업 증빙이다.

- **`main` 직접 push 가능.** `main`이 통합 지점이다. 규모가 있거나 되돌릴 여지가 필요한 작업은
  짧은 브랜치에서 하고 검증 후 합치되, 작은 수정은 `main`에서 바로 처리해도 된다.
- **에이전트 커밋은 `Co-Authored-By` 트레일러를 유지한다.**
- **결정을 내렸으면 그 세션이 `docs/decisions/`에 ADR을 남긴다** — 다른 세션·다른 에이전트가
  같은 고민을 재발명하지 않도록. 형식과 판단 기준은 `docs/decisions/README.md`.
- **각 영역의 `AGENTS.md` 가 그 영역 에이전트의 출발 컨텍스트다.**
  구조가 바뀌면 코드보다 그 문서를 먼저 갱신한다.
- **완료 주장 전에 실측한다.** 컴파일(`unity-cli … console --type error`)과 플레이 모드 확인 없이
  "됐다"고 쓰지 않는다. 에셋은 **존재 확인만으로 부족하다** — 내용까지 본다
  (애니메이터는 클립 수, 데이터는 행 수, 스프라이트는 실제 로드).
- **무관한 워킹트리 변경은 건드리지 않는다.** 커밋은 기능 단위로 원자적으로.
- 대량 데이터는 손으로 넣지 말고 `_tools/gen_*.py` 생성기를 남겨 재현 가능하게 한다.

**시크릿**: `.env.local`(AetherForge API 키)은 절대 커밋하지 않는다. `.gitignore`에 등록돼 있다.

---

## 1. 빌드 / 검증

**자동화 테스트 스위트가 없다.** 검증은 Unity Editor + `unity-cli`로 한다.

### unity-cli (켜져 있는 에디터에 붙어서 조종)
Windows에서 프로젝트를 열 때 `_tools/open_unity.bat`를 사용한다. 프로젝트 버전의 에디터를 DX11로 실행하며,
실행 중인 Unity가 없을 때만 남은 씬 백업을 `Documents/UnityRecovery/`로 **보존 이동**한다.
`Assets/_Recovery` 사본은 그대로 둔다. Windows 기본 그래픽 API도 DX11이며 Android 설정과 별개다.
에디터 종료는 저장 상태를 확인한 뒤 실제 창 닫기(Alt+F4)를 사용한다. 플레이 후에는 창 닫기에서도
씬 백업이 잔류할 수 있어 `SceneBackupArchive`가 정상 종료 확정 시 보존 이동한다(ADR0016).
런처는 자동화 호스트에서 누락될 수 있는 `ALLUSERSPROFILE`도 Windows 공용 데이터 경로로 보완한다.
UPM이 `path ... Received undefined`로 실패하면 패키지 캐시를 지우기 전에 실행 환경을 확인한다(ADR0018).

`C:\Users\Mycom\AppData\Local\unity-cli\unity-cli.exe` (v0.3.22, 비공식 도구)

```bash
unity-cli editor refresh --compile          # 스크립트 재컴파일, 완료까지 대기
unity-cli console --type error --lines 30   # 컴파일/런타임 에러 확인  ← 컴파일 검증의 기본 경로
unity-cli menu "Tools/ParseAllLocal (로컬 전체 파싱)"   # 에디터 메뉴 실행
unity-cli exec "return Managers.Data.SkillDic.Count;"  # C# 즉시 실행 (top-level 문, 인자 방식만)
unity-cli editor play --wait                # 플레이 모드 진입
unity-cli screenshot --view game            # 게임뷰 캡처
```

- **Unity Editor가 켜져 있어야 한다.** 꺼져 있으면 `Error: no Unity instances running` → 사용자에게 에디터 실행을 요청한다. 일상 작업에서 임의로 batchmode를 돌리지 않는다(에디터를 닫아야 해서 작업 흐름이 깨진다).
- 코드 수정 후에는 `editor refresh --compile` → `console --type error` 순으로 반드시 확인. 정독만으로 완료를 주장하지 않는다.
- `exec` 는 **인자 방식만 동작한다** — stdin 파이프도 `--file` 플래그도 없다.

### 플레이어 빌드는 batchmode로 (`Assets/@Scripts/Editor/BuildScript.cs`)

**unity-cli 로는 플레이어 빌드를 끝까지 못 돌린다.** IL2CPP/Gradle 이 에디터 메인 스레드를
오래 잡으면 커넥터 하트비트가 끊기고(실측 40분 무응답), exec 컨텍스트가 정리되면서 진행 중이던
`BuildPlayer` 까지 함께 죽는다 — 산출물도 `Temp/StagingArea` 도 남지 않아 "조용히 실패"한다.
빌드만은 에디터를 닫고 batchmode 로 돌린다.

```bash
Unity.exe -batchmode -quit -projectPath . -logFile <로그> -executeMethod BuildScript.Android -buildOut <경로>
```

- 10분을 넘기므로 분리 실행 후 로그를 폴링한다.
- 어드레서블을 플레이어보다 **먼저** 굽는다 — 카탈로그가 플랫폼별(`aa/Windows`, `aa/Android`)이라
  순서를 어기면 런타임 로드가 통째로 깨진다. `BuildScript` 가 이 순서를 지킨다.
- 에디터에서는 `Tools/Build/*` 메뉴로도 같은 경로를 탄다.

⚠️ **에디터 전용 API를 런타임 코드가 부르면 에디터는 통과하고 플레이어 빌드만 터진다**
(`CS1061`. 어드레서블 콘텐츠 빌드는 `SBP Error` 로 죽는데, 진짜 원인은 그 위에 찍힌다).

### Editor 메뉴 (`Assets/@Scripts/Editor/`)

| 메뉴 | 용도 |
|------|------|
| `Tools/ParseAllLocal (로컬 전체 파싱)` | **xlsx → JSON 전체 재생성. 데이터 수정 후 필수** |
| `Tools/DeleteGameData` | PlayerPrefs + SaveData.json 삭제 (초기 상태 재현) |
| `Tools/GenerateLocalizeKeys` | 로컬라이즈 시트 → `Define.LocalizeKey` 자동 생성 |
| `Test/Integration/Diagnose - 전체 진단 %#d` | 데이터 무결성 등 통합 진단 |
| `Test/Battle/*` | 데미지 팝업, 치명타, 레벨업 등 전투 연출 시연 |
| `Test/Bestiary/*` | 도감/미니몬/포획/공명도 상태 시드 및 시연 |
| `QuestVerifyMenu` | 기획서 슬라이드별 퀘스트/연출 시연 |

---

## 2. 데이터 파이프라인 (가장 자주 틀리는 부분)

```
Assets/@Resources/Data/Excel/*.xlsx   ← 단일 원천(Source of Truth)
        │  Tools/ParseAllLocal  (DataTransformer.cs)
        ▼
Assets/@Resources/Data/JsonData/*.json  ← 런타임이 읽는 유일한 대상
        │  Addressables (PreLoad 그룹)
        ▼
DataManager.Init() → Dictionary<int, XxxData>  (Managers.Data.XxxDic)
```

- **게임은 JSON만 읽는다.** xlsx만 고치고 파싱을 안 하면 인게임에 반영되지 않는다.
- 구글 시트 경로는 **폐기**됨 (`DataTransformer.s_useGoogleSheets = false`). 되살리면 최신 로컬 데이터가 옛 시트로 덮어써진다.
- 신규 JSON을 추가하면 **Addressable PreLoad 그룹 등록**이 필요하다. 누락 시 `[DataManager] JSON load 실패` 로그.
- 게임 수치(데미지/쿨타임/확률/가격)는 코드 하드코딩 금지. 기본 키는 항상 `DataId`(int).
- ⚠️ 시트를 외부에서 재생성하면 코드에서 추가한 로컬라이즈 키가 소실될 수 있다 — 기존 키 재사용 우선.

---

## 3. 아키텍처

### Managers 싱글턴 (`Managers/Managers.cs`)
모든 시스템 접근은 `Managers.XX` static property 경유. 매니저를 직접 `new` 하지 않는다.

- **Core** (범용): `Data` `Font` `Pool` `Resource` `Scene` `Sound` `UI`
- **Contents** (게임 로직): `Game` `Object` `MinMon` `Ability` `Quest` `SubQuest` `Combo`

`Managers.Awake()`는 씬에 배치된 중복 `@Managers`를 파괴한다 (씬 재진입 시 콤보 Tick이 죽던 버그 대응). `Update()`에서 `_combo.Tick()`만 돌린다.

### 리소스 / 풀링
`Managers.Resource`는 **Addressables 기반**. 생성 `Managers.Resource.Instantiate(key, parent, pooling)`, 삭제 `Managers.Resource.Destroy(go)` (풀 반환 포함). `Object.Destroy` 직접 호출 금지.

### UI (`UI/`)
```
UI_Base ─┬─ UI_Scene    (씬 HUD: UI_GameScene, UI_BattleScene …)
         ├─ UI_Popup    (UIManager 스택 관리)
         └─ UI_SubItem  (리스트 아이템, SetInfo()로 갱신)
```
- 프리팹 자식 이름 ↔ `enum Images/Buttons/Texts` 이름이 **1:1**로 자동 바인딩된다 (`Bind<T>(typeof(...))` → `GetImage((int)Images.BG)`). 프리팹 자식 이름을 바꾸면 바인딩이 깨진다. 같은 프리팹 안에서 이름 중복 금지, 언리얼식 prefix(`Img_` `Btn_` `Txt_`) 사용.
- 팝업은 `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()`로만 열고 닫는다 (직접 SetActive 금지).
- **팝업은 게임을 멈추지 않는다** — `RefreshTimeScale()`은 현재 전부 주석 처리된 no-op다. `Time.timeScale` 관련 코드를 쓰기 전에 실제 구현을 확인할 것.
- Toast / 관심 알림(`ShowAttractedPopup`)·경고는 팝업 스택을 쓰지 않고 자체 코루틴으로 소멸한다.
- **UI 캔버스 위 파티클**: 월드 ParticleSystem은 Screen Space Camera 캔버스 위에 렌더되지 않는다 → 반드시 `UIFx.PlayParticle()` (UIParticle 래퍼) 경유.

### 컨트롤러 (`Controllers/`)
```
BaseController ─┬─ ProjectileController
                └─ CreatureController (abstract) ─┬─ PlayerController
                                                  ├─ MonsterController
                                                  └─ CompanionController (동행 미니몬)
```
부가: `AbilityRunner`(능력 발동), `BattleSpawner`, `BackgroundScroller`, `FixedResolution`.
상태 전환은 반드시 property setter 경유 (setter가 애니메이션 전환을 담당). 데미지/명중/치명타 계산은 `Utils/BattleMath.cs`에 모여 있다.

### 세이브
`GameData` 하나를 `Application.persistentDataPath + "/SaveData.json"`에 직렬화. **세이브가 필요한 값은 전부 `GameData`(GameManager)에 넣는다.** GameManager의 프로퍼티 setter들이 값 변경 시 자동으로 `SaveGame()`을 호출하는 패턴이므로, 새 저장 값도 같은 패턴을 따를 것. (`Language`처럼 일부만 PlayerPrefs)

### 이벤트
컨트롤러 ↔ UI는 C# `Action`으로 느슨하게 연결 (컨트롤러가 UI를 직접 참조하지 않음). 구독은 `OnDestroy()`에서 반드시 해제.

### 로컬라이즈
표시 문자열은 키값으로 두고 `Util.Localize(key)`로 조회. 키 상수는 `Define.LocalizeKey` (자동 생성).

---

## 4. 코드 규칙

- 인코딩: **UTF-8 (BOM 없음)**. `.meta` 파일을 직접 생성하지 않는다 (Unity가 생성).
- private/public 필드 `_camelCase`, 프로퍼티/메서드 `PascalCase`, 상수 `UPPER_SNAKE_CASE`, 코루틴 `Co` 접두사, 핸들러 `On` 접두사.
- 4 스페이스, Allman 중괄호, 단일 statement면 중괄호 생략 허용. `#region`으로 섹션 구분.
- 주석은 한국어. 의도/연출 흐름 위주. 기존 주석 처리된 디버그 코드는 함부로 지우지 않는다.
- 모든 enum/상수/매직넘버/스프라이트 이름은 `Utils/Define.cs`에 모은다. `using static Define;`.
- **기존 식별자의 오타는 그대로 유지** (`_isEquiptWeapon` 등 — 다른 곳에서 참조 중).
- 애니메이션은 **프리팹에 저작된 DOTweenAnimation 컴포넌트 우선**, 코드 Sequence는 지양.
- 새 UI/시스템을 만들기 전에 **기존 프리팹을 먼저 검색**한다 — 기획 스펙이 이미 자산화돼 있는 경우가 잦다.
- `Library/` `Temp/` `obj/` `Logs/`는 Unity 생성물 — 수정/커밋 금지.

---

## 5. 스펙 출처

게임 스펙의 원전은 구글 슬라이드 기획서 2종 — **기획서1**(70장: UI/연출/전투), **기획서2**(54장: 도감/미니몬/능력) + 수정안. 코드 주석의 `슬N` / `슬라이드 N` 표기가 이 슬라이드 번호다.

- `Documents/` — 기획·QA 문서, 변경 기록(`Battle_Phase_Log.md`, `Part2_Gap_Backlog.md`, `VS_Macro_Port_Gap.md`), 버그 스크린샷/GIF
- `Documents/RE001_Share-main` — 이식 원본(VS/브라운에픽) 스펙 참조본
- `_tools/` — xlsx 검증/수정 Python 스크립트, enum 바인딩 검증(`verify_enum_bindings.py`), 프리팹 중복 스캔

폴더별 상세 규칙과 파일↔슬라이드 매핑은 각 디렉터리의 **`AGENTS.md`** 에 있다 (`Assets/@Scripts/AGENTS.md`, `Managers/AGENTS.md`, `UI/AGENTS.md` 등). 해당 폴더 작업 전 먼저 읽을 것.

---

## 6. 게임 구조 요약

**씬**: `TitleScene` / `GameScene` (마을·필드·전투 모두 GameScene 안에서 전환)

**화면**: 상단 재화 HUD + 퀘스트 진행도 → 게임 뷰 → 캐릭터 상태창(HP/EXP) → 퀵슬롯 6칸 → 하단 탭(상태/가방/물약/장비/기술/도감)

**주요 시스템**
| 시스템 | 진입점 |
|---|---|
| 전투 (횡스크롤 실시간, 콤보/치명타) | `CreatureController`, `BattleMath`, `ComboManager` |
| 퀘스트 / 서브퀘스트 | `QuestManager`, `SubQuestManager` |
| 마물 도감 · 미니몬 포획/관심/공명도 | `MinMonManager`, `UI_BestiaryPopup`, `UI_TamePopup` |
| 능력 / 기억력 (기억·망각, 발동 조건) | `AbilityManager`, `AbilityRunner`, `UI_SkillBookPopup` |
| 장비 / 강화 / 대장간 | `UI_EquipmentPopup`, `UI_ForgePopup`, `UI_EnhacePopup` |
| 물약 (HP 회복, 신속 버프, 자동물약) | `UI_PotionPopup`, `PlayerController` |
| 여정(지역 이동 해금) | `UI_TravelPopup` + `TravelNodeData` / `TravelConditionData` |
| NPC 대화 | `DialogueAnimator`, `UI_DialoguePopup` |

---

## 7. 디렉터리 지도 · 의존

| Directory | Purpose |
|-----------|---------|
| `Assets/@Scripts/` | 모든 게임 C# 소스 (폴더별 `AGENTS.md` 참조) |
| `Assets/@Resources/` | `Data/`(xlsx·JSON), `Prefab/`, `Sprite/`, `Sound/`, `Font/`, `Animation/`, `Particles/` |
| `Assets/@Scenes/` | `TitleScene` / `GameScene` (빌드 목록은 이 둘) |
| `Assets/AddressableAssetsData/` | Addressables 설정 — 리소스 로딩의 단일 경로 |
| `Documents/` | 기획·QA 문서, 변경 기록, 버그 스크린샷/GIF, 검증 녹화본 |
| `docs/decisions/` | ADR. 결정을 내렸으면 그 세션이 여기에 남긴다 |
| `_tools/` | 데이터 생성·검증 Python 스크립트 (`gen_*.py`, `verify_enum_bindings.py` 등) |
| `ProjectSettings/`, `Packages/` | Unity 설정 / 패키지 매니페스트 |

**외부 의존**: Unity(URP · Input System · Addressables · Visual Scripting),
Newtonsoft.Json, DOTween / DOTween Pro, DamageNumbersPro, TextMesh Pro,
Unity UI Extensions, ParticleImage, FastScriptReload.
