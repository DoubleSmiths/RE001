# 0004. 생성 에셋의 어드레서블 등록 규칙

- 상태: 채택
- 날짜: 2026-08-14

## 맥락

`ResourceManager.Load<T>(key)`는 **캐시만 조회**한다. 게임 시작 시
`LoadAllAsync<Object>("PreLoad")`가 적재한 것만 잡힌다.
이 규칙을 모르고 에셋을 등록하면 런타임에서 **조용히 null**이 되고, 원인을 찾기 어렵다.

실제로 두 번 밟았다.
1. `PreLoad` 라벨 없이 등록 → 60개 중 1개만 로드
2. 주소에 `.sprite`를 안 붙임 → `LoadAllAsync`가 Texture2D로 적재해 `as Sprite`가 전부 null

여기에 더해, 대형 에셋을 전부 PreLoad에 넣었더니 **시작 로딩이 30초를 넘겼다**.
`Managers.Data.Init()`이 PreLoad 완료 시점에만 호출되므로 그때까지 게임이 데이터 없이 떠 있는다.

## 결정

`Editor/GeneratedIconRegistrar`가 등록을 전담한다. 규칙:

| 에셋 | 주소 | PreLoad 라벨 | 이유 |
|---|---|---|---|
| 스프라이트(아이콘·몬스터 원형) | `{이름}.sprite` | **붙인다** | UI에 즉시 나와야 함 |
| 몬스터 애니메이터 컨트롤러 | `{이름}` | **안 붙인다** | 503개(클립 2,515개 동반) → 로딩 폭증 |

PreLoad에서 뺀 에셋을 위해 `ResourceManager.Load`에 **온디맨드 동기 로드 폴백**을 넣었다.
캐시에 없으면 `Addressables.LoadAssetAsync().WaitForCompletion()` 후 캐싱하고,
실패한 주소는 `_missing`에 기록해 매 호출 재시도하지 않는다.

또한 `LoadAsync`가 로드 실패 시 **콜백을 반드시 호출하도록** 고쳤다.
예전에는 조용히 `return`해서 `LoadAllAsync`의 진행 카운터가 `totalCount`에 닿지 못했고,
그 시점에만 도는 `Data.Init()`이 통째로 스킵돼 **모든 데이터 딕셔너리가 0**이 됐다.

## 결과

- 새 에셋을 추가하면 `Tools/Register Generated Icons`를 실행한다.
- 임포트 설정도 이 메뉴가 잡는다(Sprite / Point 필터 / 무압축 / PPU 64). 픽셀아트는 기본 설정이면 뭉개진다.
- 에셋을 재생성하면 GUID가 바뀌므로 **반드시 재등록**해야 한다. 안 하면 주소가 끊긴다.
- 자주 안 쓰는 대형 에셋은 PreLoad에 넣지 않는다. 로딩 시간은 PreLoad 항목 수에 비례한다.
