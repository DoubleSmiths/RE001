<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# RE001

## Purpose
2D 사이드뷰 액션 RPG (모바일 세로 화면, 픽셀 아트) Unity 프로젝트. Manager 싱글톤 패턴 + 데이터 드리븐(JSON) 아키텍처를 사용하며, 마을/필드 탐험, 실시간 횡스크롤 전투, 퀘스트, 마물 도감, 미니언(펫), 장비/물약 제작 시스템을 포함한다. 게임 디자인 문서와 코딩 규칙은 `CLAUDE.md` 참조.

## Key Files
| File | Description |
|------|-------------|
| `CLAUDE.md` | 코딩 가이드(Manager 패턴, UI 스택, 풀링 규칙) + 게임 기획 분석 문서 |
| `RE001.sln` | Visual Studio 솔루션 (Assembly-CSharp 등 Unity 생성 프로젝트 포함) |
| `Assembly-CSharp.csproj` | 런타임 스크립트 프로젝트 (`Assets/@Scripts` 소스) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Assets/@Scripts/` | 모든 게임 C# 소스 코드 (see `Assets/@Scripts/AGENTS.md`) |
| `Assets/@Resources/` | 게임 리소스 — `Data/`(JSON 데이터), `Prefab/`, `Sprite/`, `Sound/`, `Font/`, `Animation/`, `Particles/` |
| `Assets/@Scenes/` | Unity 씬 파일 |
| `Assets/AddressableAssetsData/` | Addressables 설정 (리소스 로딩은 Addressables 기반) |
| `_tools/` | 개발 보조 스크립트 모음 (퀘스트 xlsx 검증/수정 Python 스크립트, enum 검증, 스크린샷 등 작업 산출물) |
| `tools/` | 데이터 시트 생성 PowerShell 스크립트 |
| `ProjectSettings/`, `Packages/` | Unity 프로젝트 설정 / 패키지 매니페스트 |

## For AI Agents

### Working In This Directory
- 스크립트는 전부 `Assets/@Scripts` 아래에 작성한다. 폴더별 상세 규칙은 각 하위 `AGENTS.md` 참조.
- 게임 수치(데미지, 쿨타임, 확률 등)는 코드에 하드코딩하지 않고 `Assets/@Resources/Data`의 JSON으로 관리한다. **게임은 JSON만 읽으므로**, 데이터 시트(xlsx)를 수정한 경우 Editor의 파서(DataTransformer)를 수동 실행해 JSON을 재생성해야 반영된다.
- `Library/`, `Temp/`, `obj/`, `Logs/`, `_Recovery/`는 Unity 생성물 — 수정/커밋 금지.
- 모든 C# 스크립트 인코딩: UTF-8 (BOM 없음).
- `.meta` 파일을 직접 생성하지 않는다 (Unity가 생성).

### Testing Requirements
- 자동화 테스트 스위트 없음. 검증은 Unity Editor 플레이 모드 및 `Assets/@Scripts/Editor`의 테스트 메뉴(BattleTestMenu, PopupTestMenu, QuestVerifyMenu 등)로 수행한다.
- 컴파일 검증은 Unity Editor 리로드 또는 `RE001.sln` 빌드로 확인.

### Common Patterns
- 모든 시스템 접근은 `Managers.XX` static property 경유 (자세한 내용은 `Assets/@Scripts/Managers/AGENTS.md`).
- 오브젝트 생성/삭제는 `Managers.Resource.Instantiate()` / `Managers.Resource.Destroy()` (풀링 포함).
- 팝업은 `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()`로만 열고 닫는다.

## Dependencies

### External
- Unity (URP, Input System, Addressables, Visual Scripting)
- Newtonsoft.Json — JSON 직렬화
- DOTween / DOTween Pro — 트윈 애니메이션
- DamageNumbersPro — 데미지 폰트 연출
- TextMesh Pro, Unity UI Extensions, ParticleImage, FastScriptReload

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
