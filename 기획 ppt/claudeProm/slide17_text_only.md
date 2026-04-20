# [슬라이드 17] 아이템 종류 및 등급

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 아이템 카테고리 4종
| 카테고리 | 설명 |
|---|---|
| 소지품 | 물약 제외 모든 재료/사용 아이템 |
| 장비 | 종류/개수 고정. 조건부 해금 + 제작 |
| 귀중품 | 컨텐츠 관련 아이템 |
| 물약 (귀중품 분류) | 물약. 종류 고정 |

### 등급
- 등급별 색상 팔레트 8종 존재 (기획서 본문에 색 코드 미명시) → 실제 스프라이트에서 색 추출 후 `Define.GradeColors`로 매핑.
- 기존 `ItemGrade` enum과 대조 후 보완.

### 사용 방식
- 모든 아이템은 상호작용 가능 → 클릭 시 상세정보 Popup 호출.

## 작업 지시
1. **조사**
   - `Data.Contents.cs` ItemType/ItemGrade enum.
   - 카테고리별 UI 필터 구현 여부.
   - 등급별 색 팔레트 상수 유무.
   - 아이템 클릭 → 상세 Popup 공통 핸들러 유무.

2. **체크리스트**
   - [ ] ItemType 4종 enum?
   - [ ] ItemGrade enum + 색 매핑?
   - [ ] 카테고리별 필터링?
   - [ ] 공통 `OnClickItem(itemId)` 핸들러?
   - [ ] 장비 데이터 정의 완료?
   - [ ] 물약 데이터 정의 완료?

3. **누락분만 구현**
   - ItemType/ItemGrade 정비.
   - `Define.GradeColors` 상수화.
   - 공통 아이템 클릭 헬퍼.

4. **결과 보고**
   - 체크리스트 결과 + enum 구조 + 등급 색 매핑.

## 관련 파일 힌트
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/Utils/Define.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`, `UI_ImportantSlot.cs`
