# [슬라이드 23] QuestFollow #2 — 보유/요구량 표시 및 색상

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 사용 Asset & 조합
- Mask 처리 (RectMask2D 등)
- 퀘스트아이템 아이콘 + 이름 + 보유/요구량 텍스트
- 보유/요구량 형식: `(9999/9999)` (앞: 현재 보유량, 뒤: 요구량)

### 업데이트 시점
- 플레이어가 해당 아이템을 **습득할 때마다** 갱신.

### 컬러값 (보유량 상태)
1. 보유량 = 0 → `#FF0000` (빨강)
2. 0 < 보유량 < 요구량 → `#FEA400` (주황)
3. 보유량 ≥ 요구량 → `#00FF00` (초록)

## 작업 지시
1. **조사**
   - QuestFollow UI 노드 유무.
   - Texts enum `Quest_NeedItemNameTxt` 등.
   - 아이템 습득 이벤트 존재 여부.

2. **체크리스트**
   - [ ] 아이템 아이콘 + 이름 + `(보유/요구)` TMP 배치?
   - [ ] Mask 처리?
   - [ ] 습득 시 TMP 즉시 갱신?
   - [ ] 3단계 색 분기?
   - [ ] 요구량 퀘스트 데이터에서 로드?

3. **누락분만 구현**
   - `UpdateQuestFollow(itemId, has, need)` 헬퍼 + 색 분기.
   - `OnInventoryChanged` 이벤트 구독.
   - QuestData에 `GoalItemId`, `GoalCount` 필드.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Data/QuestData*.json`
