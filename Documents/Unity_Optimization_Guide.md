# 유니티 최적화 가이드 (RE001)

> 사용자 필기 + 일반 Unity 최적화 지식 + **이 프로젝트에 실제 적용한 작업 로그**를 종합한 학습/참조 문서.
> 작성: 2026-06-24. 적용은 Unity Editor API(unity-cli exec)로 안전하게 수행함.

---

## 0. 이 프로젝트에 실제 적용한 최적화 (작업 로그)

아래는 `unity-cli exec`로 Editor API를 통해 **이미 적용 완료**한 항목 (ProjectSettings에 저장됨):

| 항목 | 적용 값 | API |
|---|---|---|
| Physics2D Auto Sync Transforms | **off** | `Physics2D.autoSyncTransforms=false` |
| Physics2D Reuse Collision Callbacks | **on** | `Physics2D.reuseCollisionCallbacks=true` |
| Physics(3D) Auto Sync / Reuse | off / on | `Physics.autoSyncTransforms` / `reuseCollisionCallbacks` |
| Fixed Timestep | **0.04** (25Hz) | `Time.fixedDeltaTime=0.04` |
| GPU Skinning | **on** | `PlayerSettings.gpuSkinning=true` |
| Graphics Jobs | **on** | `PlayerSettings.graphicsJobs=true` |
| Strip Engine Code | **on** | `PlayerSettings.stripEngineCode=true` |
| Optimize Mesh Data | **on** | `PlayerSettings.stripUnusedMeshComponents=true` |
| Texture Mipmap Stripping | **on** | `PlayerSettings.mipStripping=true` |
| Stack Trace (Log/Warning/Assert) | **None** | `SetStackTraceLogType(..., None)` |
| Android Graphics API | Auto off → **GLES3** | `SetUseDefaultGraphicsAPIs/SetGraphicsAPIs` |
| Managed Stripping (Android/iOS) | **Low** | `SetManagedStrippingLevel(..., Low)` |
| Async Asset Upload (전 품질레벨) | TimeSlice **4** / Buffer **8** | `QualitySettings.asyncUploadTimeSlice/BufferSize` |

### 2차 적용 (2026-06-24 추가)
| 항목 | 적용 | 방법 |
|---|---|---|
| 빌트인 모듈 제거 | **cloth · xr · vr · vehicles** | `Packages/manifest.json` (코드 참조 0 확인, 컴파일 클린) |
| ParticleSystem Culling Mode | **Pause** (377개 / 39 프리팹) | AssetDatabase 일괄 exec |
| Addressables InternalIdNamingMode | **Dynamic(3)** | AddressableAssetSettings.asset |
| Addressables maxConcurrentWebRequests | **16** | 〃 |
| Addressables Json Catalog | 이미 **바이너리(off)** = 최적 | (변경 불필요) |

### 의도적으로 보수 적용/보류한 것 (이유)
- **Fixed Timestep**: 필기엔 0.333(3Hz)이지만 그건 과격해 물리가 끊겨 보일 수 있어 **0.04(25Hz)** 절충. 이 게임은 물리 의존이 적어(동행은 시뮬레이션 off, 수동 transform) 더 올려도 무방하나 안전값 선택. 더 낮은 CPU를 원하면 0.05~0.1까지 시험.
- **Managed Stripping = High 보류**: 이 프로젝트는 **Visual Scripting + Newtonsoft Json + 리플렉션(테스트 메뉴)**를 써서 High/Medium 스트리핑이 타입을 날릴 위험. `link.xml`로 보존 타입을 지정한 뒤에야 상향 가능. → **Low** 유지.
- **Android GLES3 단일화**: Vulkan을 빼면 빌드/셰이더 변형이 줄지만 일부 기기 성능은 Vulkan이 유리. 2D 모바일 호환 안전값으로 GLES3 선택. 필요 시 Vulkan 추가.
- **Always Included Shaders 정리 / Strip Post Processing**: 잘못 빼면 핑크 머티리얼 → **수동 검토 필요**(아래 3장).
- **Audio DSP Buffer / AudioClip 설정**: 에셋·AudioManager 단위라 일괄 API가 까다로움 → 수동(아래 4장). 참고로 이 프로젝트의 SoundManager는 현재 PlayOneShot이 전부 주석 처리돼 **사운드 자체가 비활성**.
- **Addressables MaxConcurrentWebRequests=16**: API 네임스페이스 이슈로 1회 실패 → 에디터에서 수동 설정 권장(아래 5장).

---

## 1. 빌드 / 플레이어 설정 (Player Settings)

### Managed Stripping Level
- **무엇**: 빌드 시 안 쓰는 코드(IL)를 제거하는 강도. Disabled < Minimal < Low < Medium < High.
- **왜**: 빌드 크기↓, 로드/메모리↓.
- **주의**: 리플렉션·직렬화(JsonUtility/Newtonsoft)·Visual Scripting이 동적으로 참조하는 타입은 정적 분석으로 못 잡아 **런타임 누락**. → `link.xml`(보존 목록)로 방어.
- **이 프로젝트**: VS+리플렉션 사용 → **Low** 적용.

### Strip Engine Code
- 엔진 네이티브 모듈 중 안 쓰는 것 제거(IL2CPP). 빌드↓. → **on 적용**.

### Optimize Mesh Data (`stripUnusedMeshComponents`)
- 메시에서 안 쓰는 버텍스 채널(노멀/탄젠트/UV2 등) 제거.
- **주의**: 런타임에 셰이더를 스왑해 그 채널이 갑자기 필요해지면 이미 날아가 문제. 2D 스프라이트 위주면 안전. → **on 적용**.

### Texture Mipmap Stripping (`mipStripping`)
- 품질 설정에서 안 쓰는 밉맵 레벨 제거. 빌드/메모리↓. → **on 적용**.

### Graphics API (Android: Auto off)
- **무엇**: Auto Graphics API를 켜면 Vulkan·GLES3 변형을 **모두** 빌드. 끄고 원하는 API만.
- **왜**: 빌드 크기/셰이더 변형↓, 첫 셰이더 컴파일↓.
- **이 프로젝트**: Auto off + **GLES3**(2D 모바일 호환). 고사양 타깃이면 Vulkan 추가/우선.

### GPU Skinning
- 스키닝 계산을 CPU→GPU. GPU가 보통 여유. → **on 적용**. (2D라 스키닝은 적지만 무해)

### Graphics Jobs (Experimental)
- 렌더 명령 생성을 멀티스레드. CPU 렌더 병목 완화. → **on 적용**(실험적이라 빌드 후 검증 권장).

### Stack Trace Log Type
- 플레이어 빌드에서 **Log/Warning → None**(스택 추적 제거). 스택 추적은 리플렉션 비용이 큼. Error는 유지 권장. → **None 적용**.

### iOS
- **Memoryless Depth → Automatic**: 타일 GPU에서 depth를 메모리에 안 올림(대역폭↓).
- **Accelerometer Frequency → Disabled**: 안 쓰면 끔(가속도 폴링 제거). (API 버전 차로 자동 적용 실패 → 수동)

---

## 2. 물리 (Physics / Physics2D)

### Auto Sync Transforms → off
- **무엇**: Transform 변경 시마다 물리 트랜스폼을 즉시 동기화하던 것을, FixedUpdate에 **몰아서** 처리.
- **왜**: 매 Transform set마다 동기화 비용 제거. → **off 적용**.
- **주의**: 같은 프레임에 `transform` 이동 직후 `Raycast`로 결과를 즉시 읽는 코드가 있으면 한 프레임 어긋날 수 있음(레이캐스트 의존 시 체크 필요).

### Reuse Collision Callbacks → on
- 콜리전 콜백 객체를 매번 새로 만들지 않고 **현재 프레임용으로 재사용**. GC↓. → **on 적용**. (콜백 객체를 저장해 다음 프레임에 쓰면 안 됨)

### Fixed Timestep
- 물리/FixedUpdate 빈도. 기본 0.02(50Hz). 높이면(예 0.04=25Hz) 물리 CPU↓, 너무 높이면 반응이 밀려 보임. → **0.04 적용**.

---

## 3. 그래픽스 (Graphics / Shader)

### Always Included Shaders 정리
- GraphicsSettings에 항상 포함되는 셰이더 목록. 안 쓰는 것 제거 → 빌드/메모리↓.
- **주의**: 런타임에 동적으로 쓰는 셰이더를 빼면 **핑크 머티리얼**. → 수동 검토.

### Strip Unused Post Processing Variants
- URP/포스트프로세싱의 안 쓰는 변형 제거.

### Shader Variant Loading Settings
- 한 번에 로드할 셰이더 청크 크기/개수 제한. 메모리 스파이크↓.
- 필기 권장: **Default chunk size = 4, chunk count = 2**. 필요한 변형은 알아서 스왑(스왑 CPU 비용 작음).

### (추가) Sprite Atlas / Draw Call Batching
- **무엇**: 흩어진 스프라이트를 아틀라스로 묶으면 머티리얼/텍스처가 같아져 **드로우콜 배칭**.
- **왜**: 2D는 드로우콜이 성능 핵심. SpriteAtlas + 동일 머티리얼 + 같은 sortingLayer로 묶기.
- **이 프로젝트**: 몬스터/UI 스프라이트가 시트별로 흩어져 있음 → 아틀라스 정리 시 드로우콜 큰 개선 여지(권장 후속).

---

## 4. 오디오 (Audio)

### DSP Buffer Size
- 크면 CPU↓·지연↑ / 작으면 CPU↑·지연↓. 리듬게임=Best Latency, 보통=**Best Performance**.

### AudioClip Import 설정
- **Load In Background**: 로드를 백그라운드로(시작 지연 가능).
- **Load Type**:
  - *Decompress On Load* — 메모리에 압축 해제 보관. CPU↓, 메모리↑. (짧은 효과음)
  - *Compressed In Memory* — 압축 보관, 재생 시 해제. 메모리↓, CPU↑.
  - *Streaming* — 디스크 스트리밍. 메모리 최소, CPU 최대·지연. (긴 BGM)
- **Compression Format**: PCM=무압축(음질↑/CPU 0/메모리↑), Vorbis/ADPCM=압축.
- **이 프로젝트**: SoundManager가 현재 사운드 비활성 → 오디오 도입 시 적용.

---

## 5. Addressables

- **Enable Json Catalog 체크 해제**: 카탈로그를 바이너리로(역직렬화↑ 빠름). 메모리 차이 미미.
- **Internal Asset Naming Mode → Dynamic (또는 FileName)**: 주소에서 전체 경로 제거 → 카탈로그 크기↓. **권장**.
- **Max Concurrent Web Requests = 16** (저사양 8): 초기 동시 로드 처리량.
- **Include GUIDs in Catalog 체크 해제**: GUID를 키로 안 쓰면 카탈로그↓.
- **Asset Bundle CRC → Disabled**(그룹별): 무결성 검사 CPU↓.
- **Shaders 전용 그룹**: 다른 그룹들이 쓰는 셰이더를 Shaders 그룹에 명시 → 셰이더 중복 로드 방지.
- **이 프로젝트**: 데이터/스프라이트/UI/VFX 그룹 존재. 위 설정은 에디터에서 수동 적용 권장.

---

## 6. 패키지 / 빌트인 모듈 정리 (Package Manager)

- 안 쓰는 **빌트인 모듈 비활성**: Cloth, XR, VR, Vehicles 등(`Packages/manifest.json`의 `com.unity.modules.*`).
- **왜**: 빌드/도메인 리로드 부담↓.
- **이 프로젝트**: 2D 방치형 → cloth/xr/vr/vehicles 미사용 → 제거 안전(권장). ⚠️ 제거 전 참조 없는지 확인.

---

## 7. 에셋 점검

### ParticleSystem → Culling Mode = Pause
- 화면 밖 파티클이 실시간 계산 안 하고, 다시 보일 때 이어서 재생. CPU↓.
- **이 프로젝트**: VFX 프리팹들(FX_splash 등)에 일괄 적용 가능(에디터 배치 스크립트).

### Texture
- 모바일 압축(ASTC), Max Size 적정화, 안 쓰는 Read/Write Enabled 끄기, Mipmap UI는 off.

---

## 8. 런타임 코드 최적화 (이 프로젝트 코드 기준)

### Object Pooling
- 자주 생성/삭제(총알/몬스터/데미지폰트)는 풀링. → 이미 `PoolManager`/`Managers.Resource.Instantiate(pooling:true)` 보유. 신규 빈출 오브젝트도 풀 경유.

### `GameObject.Find` / `FindObjectOfType` 캐싱
- **무엇**: 둘 다 씬 전체 순회(O(n)). 매 프레임/빈번 호출 금지.
- **이 프로젝트 핫스팟**: `UI_Village1BtnGroup`이 `GameObject.Find("Player")`를 여러 경로에서 반복 호출, `UI_SkillDetailPopup`/`UI_MinionInfoPopup`/`UI_BestiaryPopup`이 `FindObjectOfType<UI_*>` 사용. → **캐싱 또는 Managers/이벤트 경유**로 교체 권장. (`@Skill`/`@Monster` 등 루트 Find는 1회성이라 무방)

### GC / 할당 줄이기
- Update에서 `new`(리스트/배열/문자열 보간), LINQ(`Where/ToList`), 박싱 피하기. 문자열은 캐시/StringBuilder.
- 코루틴 `new WaitForSeconds(...)`는 캐싱.

### Update 호출 최소화
- 빈 `Update()` 제거, 상태 변할 때만 갱신(이벤트 구동). 다수 매니저의 폴링 → 이벤트로.

---

## 9. 에디터 작업 속도 (리로드 / 컴파일)

### 현재 상태 진단
- **EnterPlayModeOptions = 3** (Domain+Scene Reload 비활성) → **플레이 진입은 이미 최적**.
- 느린 건 **Ctrl+R(스크립트 변경) 시 재컴파일 + 강제 도메인 리로드**.

### asmdef로 컴파일 범위 축소 — **이 프로젝트는 차단됨**
- 모든 게임 코드가 단일 `Assembly-CSharp` → 한 파일만 고쳐도 전부 재컴파일.
- 그러나 **`Managers` ↔ `UI` 순환참조**(UIManager가 22개 UI_ 타입 참조 / UI_Base·UI_Popup이 Managers 참조)로 **asmdef 분할 불가**. 분할하려면 순환참조부터 끊는 리팩터(인터페이스/이벤트 분리)가 선행돼야 함.
- 단일 asmdef로 감싸도 재컴파일량은 그대로라 효과 미미.

### 도메인 리로드 비용
- 리로드마다 VS(Visual Scripting) 노드 DB·Addressables·InputSystem 초기화가 비용. **VS는 코드(UI_Base 등)가 의존해 제거 불가**.
- 줄일 수 있는 것: `[InitializeOnLoad]`/정적 초기화 최소화(현재 1개), Editor 폴더 스크립트 의존 최소화.

### 정리(이미 수행)
- 런타임 파일의 미사용 **Editor/Test using 제거**(`UI_TravelPopup`의 `UnityEditor.Experimental.GraphView`, `UI_Village1BtnGroup`의 `NUnit.Framework`/`UnityEditor.U2D`) — asmdef 도입 전제 + 위생.

### 권장 추가
- **Preferences → Asset Pipeline → Directory Monitoring on** (OS 파일 감시로 새로고침 감지 빠르게).
- 큰 리팩터 없이 리로드를 극적으로 줄이긴 어려움(구조 한계). 순환참조 해소 → asmdef 분할이 근본 해법.

---

## 10. 프로파일링 도구

- **Profiler**(CPU/GPU/Memory/Rendering): 병목 먼저 측정 후 최적화. "측정 없는 최적화 금지".
- **Frame Debugger**: 드로우콜 단계별 확인(배칭 깨지는 지점).
- **Memory Profiler**(패키지): 누수/상주 에셋.
- **RenderDoc**(외부, Windows): 가장 비싼 셰이더/드로우콜 분석 → 그 셰이더 최적화가 고효율. (URP/모바일 셰이더 단순화)

---

## 부록: 우선순위 요약

1. **측정**(Profiler)으로 실제 병목 확인.
2. 2D는 **드로우콜**(스프라이트 아틀라스/배칭)이 1순위.
3. **GC 할당** 제거(Update/코루틴/LINQ).
4. 빌드 설정(스트리핑/그래픽 API/스택트레이스) — *대부분 적용 완료*.
5. 에디터 리로드는 **순환참조 해소 → asmdef 분할**이 근본책(중대형 리팩터).
