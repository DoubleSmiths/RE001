<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-05 -->

# Managers/Core

## Purpose
게임 장르와 무관하게 재사용 가능한 범용 프레임워크 매니저 계층. Addressables 기반 리소스 로드/캐시(ResourceManager), JSON 데이터 시트 로딩(DataManager), Unity IObjectPool 래핑(PoolManager), 팝업 스택·토스트 UI(UIManager), BGM 크로스페이드·SFX(SoundManager), 씬 전환(SceneManagerEx), 언어·용도별 TMP 폰트 제공(FontManager)을 담당한다. 모든 클래스는 MonoBehaviour가 아닌 순수 C# 클래스이며 `Managers.Data`/`Managers.UI` 등 static property로만 접근한다.

## Key Files
| File | Description |
|------|-------------|
| `DataManager.cs` | `ILoader<Key,Value>` 인터페이스 정의 + `Init()`에서 25종 JSON(CreatureData, QuestData, TravelNodeData, ComboData, BuffData 등)을 `LoadJson<Loader,K,V>().MakeDict()`로 Dictionary 캐싱. AreaData/ForgeRecipe/CraftRecipe/LevelData 로드는 주석 처리 상태(딕셔너리는 빈 상태로 존재). 로드 실패 시 LogError 후 빈 Loader 반환 |
| `FontManager.cs` | `GetFont(FontType)` — UI(한글 PF_Stardust / 영문 DOSSaemmul, `Managers.Game.Language`로 분기), Dialogue(Cafe24), Battle(RiaSans-Bold) TMP_FontAsset 반환. `Init()`에서 Resource.Load |
| `PoolManager.cs` | prefab 이름(string) 키의 `Dictionary<string, Pool>`. 내부 Pool 클래스가 `ObjectPool<GameObject>` 래핑(활성/비활성 토글, `@{name}Pool` 루트 부모). `Pop(prefab)` / `Push(go)`(풀 없으면 false 반환) / `Clear()` |
| `ResourceManager.cs` | Addressables 로더. `LoadAsync<T>`/`LoadAllAsync<T>(label,...)`로 프리로드 후 `_resources` Dictionary에 캐시 — `Load<T>(key)`는 **캐시 전용 동기 조회**(미프리로드 에셋은 null). Sprite는 `.sprite` 접미사 및 `key[subasset]` 주소 변환 규칙. `Instantiate(key, parent, pooling)` / `Destroy(go)`(Pool.Push 우선, 실패 시 Object.Destroy) |
| `SceneManagerEx.cs` | `LoadScene(Define.Scene)` — 현재 씬 타입(Title/Game) 분기 후 `Managers.Clear()` → `SceneManager.LoadScene`. GameScene 이탈 시 timeScale=1 복원. `CurrentScene`은 `FindObjectOfType<BaseScene>()` |
| `SoundManager.cs` | `Define.Sound` 타입별 AudioSource 배열(@SoundRoot 하위, Bgm/SubBgm은 loop). `Play(type, key/clip, pitch)`, `PlayBgm(key)`(DOTween 크로스페이드, 볼륨 0.4, `SetUpdate(true)`로 timeScale=0에서도 진행), `PushBgm`/`PopBgm`(임시 BGM 저장·복귀), `PlaySFX`, `PlayButtonClick`. `Managers.Game.IsBGSoundOn/IsSFXOn` 설정 존중 |
| `UIManager.cs` | 팝업 스택(`ShowPopupUI<T>` / `ClosePopupUI` / `CloseAllPopupUI`, sortingOrder 1000+ 자동 증가), 토스트 스택(`ShowToast` — 3초 후 CoroutineManager로 자동 닫힘, order 2000+), `ShowSceneUI<T>`, `MakeSubItem<T>`(기본 pooling=true), `MakeWorldSpaceUI<T>`, 스택 밖 알림(`ShowAttractedPopup`/`ShowWarningPopup` — 게임 비정지, 자체 소멸). `OnPopupClosed` 이벤트 발행 |

## For AI Agents

### Working In This Directory
- 리소스는 반드시 Addressables **프리로드 후** 사용 — `ResourceManager.Load<T>`는 캐시만 조회하므로 새 에셋은 Addressable 그룹(PreLoad 라벨) 등록이 선행돼야 한다. DataManager의 에러 메시지도 이를 안내함.
- 오브젝트 생성/삭제는 `Managers.Resource.Instantiate()` / `Managers.Resource.Destroy()` 경유 (Destroy가 풀 반환을 먼저 시도). 풀 키는 prefab 이름이므로 생성 시 `go.name = prefab.name` 유지가 전제 — 이름을 바꾸면 Push가 실패한다.
- 팝업은 `Managers.UI.ShowPopupUI<T>()`/`ClosePopupUI()`로만 열고 닫기. `ClosePopupUI(popup)`은 스택 top과 일치할 때만 닫힘. **주의**: CLAUDE.md 가이드와 달리 현 코드의 `RefreshTimeScale()`은 본문이 전부 주석 처리되어 팝업이 게임을 일시정지하지 않는다 (`IsActiveSoulShop` setter만 timeScale 조작).
- 팝업 Canvas는 프리팹 설정(ScreenSpaceCamera) 유지 — `SetCanvas`에서 Overlay로 강제하지 말 것(레이아웃 깨짐 이력). CanvasScaler referenceResolution은 180x440.
- 새 JSON 시트 추가 시: Data 클래스 + `ILoader` 구현 Loader 작성 → DataManager에 Dictionary property와 `Init()` 로드 한 줄 추가 → JSON을 Addressable PreLoad 그룹 등록. 게임은 JSON만 읽으므로 시트 수정 후 Tools 파서 실행 필요.
- 씬 전환은 `Managers.Scene.LoadScene()` — 내부에서 `Managers.Clear()`가 호출돼 팝업/풀/사운드가 정리됨을 전제로 코드 작성.

### Common Patterns
- `ILoader<Key,Value>.MakeDict()` — JSON 배열 → `DataId` 키 Dictionary 변환 (Newtonsoft.Json `DeserializeObject<Loader>`).
- 콜백 기반 비동기 로드: `LoadAsync<T>(key, Action<T>)`, `LoadAllAsync<T>(label, Action<key,count,total>)`.
- `Util.GetOrAddComponent<T>(go)` 로 프리팹에 UI 컴포넌트 보장.
- 이름 규약 루트 오브젝트: `@UI_Root`, `@SoundRoot`, `@{prefab}Pool` — `GameObject.Find` 후 없으면 생성.
- DOTween `SetUpdate(true)` — timeScale 무시 페이드(BGM).
- `.sprite` 접미사 규약: 데이터 시트의 스프라이트 키에 `.sprite`가 없으면 Load에서 임시로 붙여 재조회.

## Dependencies

### Internal
- `Managers` (매니저 상호 참조: Resource↔Pool, Sound→Game 설정, Font→Game.Language, UI→Resource/CoroutineManager)
- `Data` 네임스페이스(데이터 클래스), `Define`(Scene/Sound/FontType 열거형), `Util`, `BaseScene`, UI 계층(UI_Base/UI_Scene/UI_Popup/UI_Toast/UI_AttactedPopup/UI_WarningPopup)

### External
- Newtonsoft.Json (DataManager)
- Unity Addressables (`UnityEngine.AddressableAssets`, ResourceManager)
- UnityEngine.Pool (`IObjectPool<GameObject>`, PoolManager)
- DOTween (SoundManager BGM 페이드; UIManager는 using만)
- TextMeshPro (`TMP_FontAsset`, FontManager)

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
