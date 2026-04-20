# [슬라이드 30] 모험가 협회 (Guild)

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 구성
- `Background_Guild` 배경
- `guild_npc_idle` (접수원) + Animator `Town_NPC` / Clip `Receptionist`
- `Main_lev_build` (`Main_lev_build_53`)
- `JungleTileset` (`JungleTilset_8`)
- Masking 처리 영역

### 퀘스트 규칙
- 메인 퀘스트 수락/완료 → 항상 **[임무]** 버튼.
- **예외: 메인 퀘스트 1번** (게임 시작 직후) → [대화] 버튼으로 진행.
- 그 외 [대화] → `GuildNormalText` 랜덤 TextKey.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` OnClickVillage1GuildTalkButton 분기.
   - `Village1_Guild_BG.prefab` 구성.
   - `DialogueScriptData.json` 키 (`GuildQuest1Text`, `GuildNormalText`).
   - `Town_NPC.controller` + `Receptionist` Clip.
   - [임무] 버튼 핸들러.

2. **체크리스트**
   - [ ] BG + NPC + Building + Tileset 배치?
   - [ ] Masking 처리?
   - [ ] [대화] 퀘스트 1 → GuildQuest1Text 전체 재생?
   - [ ] [대화] 그 외 → GuildNormalText 랜덤 1줄?
   - [ ] [임무] 버튼 수락/완료 로직?
   - [ ] Animator Receptionist Clip?

3. **누락분만 구현**
   - [임무] 스텁 → QuestManager.AcceptCurrentQuest()/CompleteCurrentQuest().
   - Animator 없으면 신설 + Clip 연결.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Resources/Prefab/Map/Village1_Guild_BG.prefab`
- `Assets/@Resources/Data/DialogueScriptData.json`
- `Assets/@Resources/Animator/Town_NPC.controller`
