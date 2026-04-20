# [슬라이드 24] Quest System — 퀘스트 번호 기반 진행 시스템

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 24
- 첨부 이미지: **없음** (0건) — 텍스트 설명만.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 기획 의도
- 플레이어 진행 상태에 따라 **특정 UI 기능 해금 및 [!] 알림**을 통해 행동을 유도하는 **진행 시스템**.

### 분류
- **Main Quest**
- **Sub-Quest**

### 초기 상태
- 게임 첫 시작 시 **Main Quest 번호 1**로 세팅.

### 퀘스트 번호 규칙
- 자신의 **전 퀘스트 번호**의 완료조건이 **특정 아이템 소지**일 경우:
  - 해당 아이템 소지 계수를 지속적으로 확인.
  - 만약 해당 완료조건에 **못 미칠 경우** → 다시 해당 퀘스트 번호로 **돌아가게 됨** (역행).
- 해당 퀘스트 번호에 표기된 **Dialogue**를 설정.
- **[!] (느낌표) UI**는 현재 퀘스트 번호에 들어있는 UI만 켜지고 나머지는 다 꺼짐 (슬라이드 4 연계).

## 작업 지시
1. **조사**
   - `Managers.Game.CurrentMainQuestId`, `CurrentSubQuestId` property 유무.
   - `QuestManager` 클래스 또는 GameManager 내부 퀘스트 관리 로직 유무.
   - `QuestData` JSON/XLSX 구조: `DataId`, `Type (Main/Sub)`, `GoalItemId`, `GoalCount`, `DialogueKey`, `ExclamationTargets` 등.
   - 아이템 수량 변동 시 퀘스트 역행 체크 로직 유무.

2. **체크리스트**
   - [ ] Main/Sub 퀘스트 현재 번호 저장 (`GameData.MainQuestId`)?
   - [ ] 첫 시작 시 MainQuestId=1 기본값?
   - [ ] 전 퀘스트 조건 역행 체크 로직 (아이템 수량 감소 시)?
   - [ ] 퀘스트별 Dialogue 키 참조 구조?
   - [ ] [!] UI 제어: 현재 퀘스트 번호에 해당하는 타겟만 on, 나머지 off?
   - [ ] 퀘스트 수주(Guild [임무] 클릭) → MainQuestId 갱신?
   - [ ] 아이템 수량이 GoalCount 충족 시 퀘스트 완료 이벤트?

3. **누락분만 구현**
   - `QuestManager` 신설 or GameManager 확장:
     - `CurrentMainQuestId`, `CurrentSubQuestId` property + 이벤트 `OnQuestChanged(int id)`.
     - `CheckRegression()`: 아이템 수량 감소 시 전 퀘스트 번호로 되돌림.
     - `CompleteCurrentQuest()`: MainQuestId+=1, Dialogue/UI 갱신.
   - `ExclamationManager` 또는 QuestManager 내부 메서드 `RefreshExclamationMarks()`:
     - 모든 [!] 타겟 중 현재 퀘스트 ExclamationTargets와 일치하는 것만 on.
   - `QuestData.ExclamationTargets` string[] 필드 추가.

4. **결과 보고**
   - 체크리스트 결과 + QuestManager 구조 설계 + 수정/신규 파일 요약.

## 관련 파일 힌트
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/Data/Data.Contents.cs` (QuestData)
- `Assets/@Resources/Data/QuestData*.json`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (ExclamationVFX 노드들)
