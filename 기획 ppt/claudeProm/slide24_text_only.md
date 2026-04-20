# [슬라이드 24] Quest System — 퀘스트 번호 기반 진행 시스템

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 기획 의도
- 진행 상태에 따라 UI 해금 및 [!] 알림으로 행동 유도.

### 분류
- Main Quest / Sub-Quest.

### 초기 상태
- 게임 시작 시 MainQuestId = 1.

### 퀘스트 번호 규칙
- 이전 퀘스트의 완료조건이 아이템 소지였을 경우:
  - 소지 계수 지속 체크 → 조건 미달 시 이전 퀘스트 번호로 역행.
- 각 퀘스트 번호에 Dialogue 매핑.
- [!] UI는 현재 퀘스트 번호에 해당하는 타겟만 on, 나머지 off.

## 작업 지시
1. **조사**
   - `Managers.Game.CurrentMainQuestId/SubQuestId` property.
   - `QuestManager` 유무.
   - `QuestData` 구조 (DataId/Type/GoalItemId/GoalCount/DialogueKey/ExclamationTargets).
   - 아이템 수량 변동 역행 체크 유무.

2. **체크리스트**
   - [ ] Main/Sub 퀘스트 번호 저장?
   - [ ] 첫 시작 MainQuestId=1?
   - [ ] 역행 체크 로직?
   - [ ] 퀘스트별 Dialogue 키 참조?
   - [ ] [!] UI 타겟 제어?
   - [ ] 수주 시 Id 갱신?
   - [ ] GoalCount 충족 시 완료 이벤트?

3. **누락분만 구현**
   - `QuestManager` 신설 or GameManager 확장.
   - `OnQuestChanged(id)` 이벤트.
   - `CheckRegression()`, `CompleteCurrentQuest()`.
   - `RefreshExclamationMarks()`.
   - `QuestData.ExclamationTargets` 필드.

4. **결과 보고**
   - 체크리스트 결과 + QuestManager 설계 + 수정 파일.

## 관련 파일 힌트
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Resources/Data/QuestData*.json`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
