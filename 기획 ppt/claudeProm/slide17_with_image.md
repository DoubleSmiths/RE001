# [슬라이드 17] 아이템 종류 및 등급

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 17
- 첨부 이미지 (`기획 ppt/slide_images/slide17/` 폴더, 총 13개):
  - 아이템 카테고리 예시 (소지품/장비/귀중품/물약), 등급 색 샘플, 사용 방식 예시 등

먼저 `기획 ppt/slide_images/slide17/` 내 모든 png를 `ls`로 확인 후 Read로 주요 이미지(그림_10/16/22/28/30 = 카테고리, 그림_57/39/41/43/45/47/59/62 = 등급 색)를 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 아이템 카테고리 4종
| 카테고리 | 설명 |
|---|---|
| 소지품 | 물약을 제외한 모든 재료 및 사용 아이템 |
| 장비 | 장비의 **종류와 개수는 고정**. 조건적 해금 및 제작 필요 |
| 귀중품 | 컨텐츠 관련 아이템들 |
| 귀중품 (물약) | 물약 아이템들. **종류는 고정** |

※ "귀중품"이 두 번 등장하는 것은 기획서 원본 그대로 — 하나는 일반 귀중품, 하나는 물약 분류로 의도됨.

### 아이템 등급 (등급별 색상 팔레트)
기획서에 8개의 색 이미지(그림_57, 39, 41, 43, 45, 47, 59, 62)가 있으나 각 등급명과 색 코드가 본문에 명시되지 않음 → **실제 스프라이트 픽셀 색을 Read해서 추출** 후 Define/Enum으로 매핑.
일반적 패턴: E → D → C → B → A → S → S+ → 전설 등. 프로젝트의 기존 `ItemGrade` enum과 대조하여 누락 보완.

### 사용 방식
- 모든 아이템은 **상호작용 가능** → 클릭 시 해당 아이템의 상세정보 Popup 호출.

## 작업 지시
1. **조사**
   - `Data.Contents.cs` 의 `ItemData` / `ItemType` / `ItemGrade` enum 존재 여부.
   - `UI_InventoryPopup`, `UI_ImportantSlot`, `UI_InvSlot` 에서 카테고리별 필터 구현 여부.
   - 등급별 Border/Glow 스프라이트 색 매핑 (`Define.GradeColor[]` 등).
   - 아이템 클릭 → 아이템 상세정보 Popup 호출 로직 (슬라이드 18, 21 연계).

2. **체크리스트**
   - [ ] ItemType enum에 4종 카테고리 (소지품/장비/귀중품/물약)가 있는가?
   - [ ] ItemGrade enum 존재 + 등급별 색 팔레트 매핑?
   - [ ] 각 카테고리 UI Popup이 해당 아이템만 표시하도록 필터링?
   - [ ] 모든 아이템 아이콘 클릭 시 상세정보 Popup 호출? (일관 헬퍼 `OnClickItem(itemId)` 또는 이벤트)
   - [ ] 장비는 종류/개수가 고정 → 장비 데이터 정의 완료?
   - [ ] 물약 종류 고정 → 물약 데이터 정의 완료?

3. **누락분만 구현**
   - ItemType/ItemGrade enum 정비.
   - 등급별 색 팔레트 상수화: `Define.GradeColors = new Color[] { ... }`.
   - 아이템 클릭 → 공통 `ItemDetailPopup` 호출 헬퍼.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일 / enum 구조 / 등급 색 매핑 표.

## 관련 파일 힌트
- `Assets/@Scripts/Data/Data.Contents.cs` (ItemData, ItemType, ItemGrade)
- `Assets/@Scripts/Utils/Define.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`, `UI_ImportantSlot.cs`
