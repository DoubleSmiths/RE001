# [슬라이드 22] QuestFollow #1 — 의뢰 진행 상태 표시

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 기획 의도
- 플레이어가 목적성을 잊지 않도록 가이드 UI.

### 진행 흐름
1. 초기 시작: UI 비활성.
2. 의뢰 수주: UI Active + Move Anim.
3. 의뢰 진행중: 보유량 실시간 갱신.
4. 의뢰 완료: Quest_Done_Effect 파티클 + `DoPlayBackward`.

### Active Move Anim (DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0
- Ease: OutCubic / Loops: 1
- TO: (x=50, y=-2, z=0)

### 완료 시
- Particle `Quest_Done_Effect` + `DOPlayBackward`.

### 호출 조건
- 모험가 협회에서 의뢰 수주 시 Active.
- 모든 임무는 **몬스터 드랍 아이템 n개 모으기**.

## 작업 지시
1. **조사**
   - QuestFollow UI 오브젝트 유무.
   - `QuestManager`/`CurrentQuestId` 시스템.
   - `Quest_Done_Effect` prefab.
   - Move DOTweenAnimation 유무.

2. **체크리스트**
   - [ ] QuestFollow prefab + 바인딩?
   - [ ] 수주/진행/완료 상태 전환?
   - [ ] Move DOTweenAnimation (OutCubic 0.5s TO(50, -2, 0))?
   - [ ] 완료 시 Quest_Done_Effect + DOPlayBackward?
   - [ ] 보유량/요구량 실시간 갱신?
   - [ ] 모험가협회 [임무] 이벤트 연결?

3. **누락분만 구현**
   - QuestFollow prefab 신설 또는 UI_GameScene 자식.
   - Quest 시작/완료 이벤트 구독.
   - prefab Move DOTweenAnimation.
   - Quest_Done_Effect Instantiate.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Resources/Prefab/UI/SubItem/QuestFollow.prefab`
- `Assets/@Resources/Prefab/FX/Quest_Done_Effect.prefab`
