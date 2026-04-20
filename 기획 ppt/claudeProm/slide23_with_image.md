# [슬라이드 23] QuestFollow #2 — 보유/요구량 표시 및 색상

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 23
- 첨부 이미지: **없음** (추출 결과 0건). 기획서에 텍스트만 존재.
- 단, 기존 참고 폴더 `Documents/기획안/slide23_images/` 가 있을 경우 그쪽도 참고.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 사용 Asset & 조합
- Mask 처리 (QuestFollow 루트에 RectMask2D 또는 유사)
- 퀘스트아이템 아이콘
- 퀘스트아이템 이름 TMP
- 보유량/요구량 TMP: `(9999/9999)` 형식

### 아이템 보유량 TMP
- 플레이어가 **해당 아이템을 습득할 때마다** 갱신.

### 컬러값 (보유량 상태에 따라 동적 색 변경)
1. 보유량 `= 0` → `#FF0000` (빨강)
2. 보유량 `> 0` && 보유량 `< 요구량` → `#FEA400` (주황)
3. 보유량 `>= 요구량` → `#00FF00` (초록)

## 작업 지시
1. **조사**
   - QuestFollow prefab 또는 UI_GameScene 내 퀘스트 진행 텍스트 노드 유무.
   - `UI_GameScene.Texts` enum에 `Quest_NeedItemNameTxt` 등이 있는지.
   - 아이템 습득 이벤트 (`OnItemAcquired(int itemId, int count)`) 후크 존재 여부.

2. **체크리스트**
   - [ ] QuestFollow에 아이템 아이콘 + 이름 + `(보유/요구)` TMP 배치?
   - [ ] Mask 처리 (Long 이름 대비)?
   - [ ] 아이템 습득 시 보유량 TMP 즉시 갱신?
   - [ ] 보유량 상태별 색 분기 3가지 적용?
   - [ ] 요구량(goal) 값이 퀘스트 데이터에서 로드?

3. **누락분만 구현**
   - `UpdateQuestFollow(int itemId, int has, int need)` 헬퍼 + 색 분기.
   - `Managers.Game.OnInventoryChanged` 이벤트 구독.
   - QuestData에 `GoalItemId`, `GoalCount` 필드 확보.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (Quest_NeedItemNameTxt)
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Prefab/UI/SubItem/QuestFollow.prefab` (존재 여부 확인)
- `Assets/@Resources/Data/QuestData*.json`
