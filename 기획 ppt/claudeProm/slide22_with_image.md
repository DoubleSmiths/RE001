# [슬라이드 22] QuestFollow #1 — 의뢰 진행 상태 표시

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 22
- 첨부 이미지 (`기획 ppt/slide_images/slide22/`, 총 4개):
  - 초기 시작 상태, 의뢰 수주 시, 의뢰 진행중, 의뢰 목표 달성 시 각 단계의 UI

먼저 `기획 ppt/slide_images/slide22/` 내 4개 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 기획 의도
- 플레이어가 목적성을 잊지 않도록 무엇을 해야 할지 간단한 가이드.

### 진행 흐름 (4단계)
1. **초기 시작 상태** — QuestFollow UI 비활성.
2. **의뢰 수주 시** — QuestFollow UI Active + 진행 상태 Anim.
3. **의뢰 진행중** — 보유량 실시간 갱신.
4. **의뢰 목표 달성 시** — `Quest_Done_Effect` 파티클 재생 후 UI 비활성 (`DoPlayBackward`).

### Active 시 Move Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0
- Ease: OutCubic / Loops: 1
- TO: `(x=50, y=-2, z=0)` (화면 바깥에서 인 방향으로 슬라이드 인)

### 완료 시
- **Particle: `Quest_Done_Effect`** 재생
- Anim: `DOPlayBackward` → 원위치로 복귀 후 비활성

### 호출 조건
- **모험가 협회에서 의뢰 수주 시** Active.
- 모든 임무는 **몬스터 드랍 아이템 n개 모으는 방식**.

## 작업 지시
1. **조사**
   - `UI_GameScene.cs` 또는 별도 HUD Scene의 QuestFollow 오브젝트 바인딩 유무.
   - `QuestManager` 또는 `Managers.Game.CurrentQuestId` 시스템 존재 여부.
   - `Quest_Done_Effect` 파티클 prefab 유무.
   - prefab에 Move DOTweenAnimation (OutCubic 0.5s TO(50, -2, 0)) 부착 유무.

2. **체크리스트**
   - [ ] QuestFollow UI prefab + UI_GameScene 바인딩?
   - [ ] 의뢰 수주/진행/완료 상태 전환 로직?
   - [ ] Active 시 Move DOTweenAnimation (OutCubic 0.5s TO(50, -2, 0))?
   - [ ] 완료 시 Quest_Done_Effect + DOPlayBackward?
   - [ ] 보유량/요구량 실시간 갱신 (슬라이드 23 연계)?
   - [ ] 모험가 협회 [임무] 수주 이벤트와 연결?

3. **누락분만 구현**
   - QuestFollow UI (루트 오브젝트 + TMP) prefab 신설 또는 UI_GameScene 내 자식 추가.
   - `QuestManager.OnQuestStart` / `OnQuestComplete` 이벤트 구독.
   - Move DOTweenAnimation prefab 구성 (AutoPlay Off, DOPlay on Active).
   - Quest_Done_Effect Instantiate 위치 지정.

4. **결과 보고**
   - 체크리스트 결과 + 수정/신규 파일·prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (퀘스트 상태)
- `Assets/@Resources/Prefab/UI/SubItem/QuestFollow.prefab` (존재 여부 확인)
- `Assets/@Resources/Prefab/FX/Quest_Done_Effect.prefab`
