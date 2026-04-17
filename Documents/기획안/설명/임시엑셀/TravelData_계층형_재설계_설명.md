**TravelData 재설계**

`TravelData.xlsx`를 `대분류 -> 중분류 -> 소분류` 구조로 다시 설계했다. 핵심 원칙은 기존 `DataTransformer`를 당장 깨지 않도록 `기존 12개 컬럼은 앞에 유지`하고, 계층형 UI에 필요한 컬럼은 뒤에 추가하는 방식이다.

**왜 바꿨는지**

| 항목 | 기존 구조 | 문제 | 재설계 방향 |
| --- | --- | --- | --- |
| 계층 표현 | 탐험 지역 1행 = 사실상 중분류+소분류 혼합 | `마을/탐험`, `장소`, `세부 장소`를 분리해서 표현하기 어려움 | `Depth`, `ParentId`, `RootId`, `NodeType` 추가 |
| 마을 처리 | 아스달 버튼 하드코딩 | 탐험만 데이터 기반, 마을은 코드 기반으로 남음 | 마을도 `TravelArea` 행으로 포함 |
| 소분류 입장 | `SceneName`이 중분류 행에 붙음 | "장소 선택"과 "실제 입장 지점 선택"이 분리되지 않음 | 실제 입장은 `Depth=3` 상세 행에만 연결 |
| UI 단계 매핑 | 코드에서 일부 추론 | 프리팹 단계와 데이터 단계가 1:1이 아님 | `PanelType`으로 `ROOT/MIDDLE/DETAIL` 명시 |
| 확장성 | 새 지역 추가 시 코드 분기 증가 | 지역/지점이 늘수록 UI 코드가 복잡해짐 | 데이터만 추가하면 리스트를 만들 수 있는 구조로 변경 |

**시트 구성**

| 시트 | 역할 | 비고 |
| --- | --- | --- |
| `TravelArea` | 모든 노드 정의 | 대분류, 중분류, 소분류를 한 시트에 저장 |
| `TravelAreaCondition` | 잠금 조건 | 그룹 잠금과 상세 잠금을 모두 표현 |
| `TravelAreaNotify` | NEW/VFX/알림 연결 | 중분류와 입장 버튼을 분리 관리 |
| `_Ref` | 입력 규칙 문서 | 파서 호환 규칙 포함 |

**TravelArea 컬럼**

| 컬럼 | 의미 | 현재 파서 사용 여부 |
| --- | --- | --- |
| `DataId` | 노드 ID | 사용 |
| `Name` | 이름 키 또는 원문 | 사용 |
| `Description` | 설명 키 또는 원문 | 사용 |
| `SpriteName` | 썸네일 리소스명 | 사용 |
| `SceneName` | 실제 입장 씬명 | 사용 |
| `RecommendPower` | 추천 전투력 | 사용 |
| `PreviewMonsterIds` | 미리보기 몬스터 | 사용 |
| `PreviewDropItemIds` | 미리보기 드랍 | 사용 |
| `Order` | 같은 부모 내 정렬 | 사용 |
| `DefaultBtnId` | 선택 시 체크할 버튼 ID | 사용 |
| `EntryBtnId` | 입장 시 체크할 버튼 ID | 사용 |
| `UnlockSummary` | 해금/요약 문구 | 사용 |
| `Category` | `TOWN` / `EXPLORE` | 신규 |
| `Depth` | `1` 대분류, `2` 중분류, `3` 소분류 | 신규 |
| `ParentId` | 상위 노드 ID | 신규 |
| `RootId` | 같은 루트 묶음 ID | 신규 |
| `NodeType` | `CATEGORY` / `GROUP` / `DETAIL` | 신규 |
| `IsSelectable` | 선택 가능 여부 | 신규 |
| `IsEnterable` | 입장 가능 여부 | 신규 |
| `PanelType` | `ROOT` / `MIDDLE` / `DETAIL` | 신규 |
| `Note` | 작업 메모 | 신규 |

**입력 규칙**

| 규칙 | 설명 |
| --- | --- |
| 대분류 | `Depth=1`, `ParentId=0`, `NodeType=CATEGORY` |
| 중분류 | `Depth=2`, `ParentId=대분류 DataId`, `NodeType=GROUP` |
| 소분류 | `Depth=3`, `ParentId=중분류 DataId`, `NodeType=DETAIL` |
| 실제 입장 | `SceneName`, `EntryBtnId`는 원칙적으로 `Depth=3` 행에만 넣는다 |
| 중분류 카드 | `DefaultBtnId`는 중분류 선택 버튼 기준으로 넣는다 |
| 기존 파서 호환 | 기존 12개 컬럼은 순서를 유지하고 신규 컬럼은 뒤에만 추가한다 |

**샘플 계층**

| 루트 | 중분류 | 소분류 |
| --- | --- | --- |
| 마을 | 아스달 | 아스달 남문 |
| 탐험 | 산들바람 숲 | 산들바람 숲 입구 |
| 탐험 | 속삭이는 동굴 | 속삭이는 동굴 입구 |
| 탐험 | 폐허 성소 | 폐허 성소 입구 |

**생성 파일**

| 경로 | 설명 |
| --- | --- |
| `임시엑셀/TravelData_redesign.xlsx` | 재설계 원본 |
| `임시엑셀/recommended_excels/TravelData.xlsx` | 추천 구조 반영본 |

**다음 코드 작업**

| 작업 | 설명 |
| --- | --- |
| `TravelAreaData` 확장 | 신규 컬럼 필드 추가 |
| `DataTransformer` 확장 | `Category`, `Depth`, `ParentId` 등 읽기 |
| `UI_TravelPopup` 동적화 | 마을/탐험 모두 같은 리스트 생성 로직으로 통일 |
| 입장 로직 분리 | `Depth=3` 노드만 실제 입장 대상으로 처리 |
