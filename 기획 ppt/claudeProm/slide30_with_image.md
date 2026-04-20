# [슬라이드 30] 모험가 협회 (Guild)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 30
- 첨부 이미지 (`기획 ppt/slide_images/slide30/`, 총 7개):
  - 모험가 협회 씬 전체, Background_Guild, guild_npc_idle (접수원), Main_lev_build, JungleTileset, Masking 처리, [임무]/[대화] 버튼 예시
- 기존 참고: `Documents/기획안/slide30_images/`

먼저 두 폴더 내 이미지를 모두 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 구성 리소스
- `Background_Guild` — 배경 스프라이트
- `guild_npc_idle` + Animator: `Town_NPC`, Clip: `Receptionist`
- `Main_lev_build` (서브: `Main_lev_build_53`)
- `JungleTileset` (서브: `JungleTilset_8`)
- Masking 처리된 영역 있음 (캐릭터/건물 경계)

### 퀘스트 진행 규칙
- **메인 퀘스트 수락/완료는 항상 [임무] 버튼**으로 진행.
- **단, 메인 퀘스트 1번 (게임 시작 직후)** 시에만 [대화] 버튼으로 퀘스트 진행.
- 그 외 [대화] 버튼 클릭 시: `Dialogue - GuildNormalText` 의 TextKey를 **랜덤**하게 뽑아 잡담 진행.

### 현재 구현 힌트 (memory)
- `Village1_Guild_BG` prefab이 Addressable로 등록됨 (GuildNPC, GuildFloor 포함).
- `UI_Village1BtnGroup.OnClickVillage1GuildTalkButton` L491~507 이미 분기 구현: Quest 1 → GuildQuest1Text 전체 재생, 그 외 → GuildNormalText 랜덤 1줄.
- `UI_DialoguePopup.SetInfo`에 `bool randomSingleLine` 파라미터 지원.
- `DialogueScriptData.json`에서 `GuildOnQuestText` → `GuildNormalText` 리네임 완료.

## 작업 지시
1. **조사**
   - `UI_Village1BtnGroup.cs` L491~ `OnClickVillage1GuildTalkButton` 분기 로직 확인.
   - `Village1_Guild_BG.prefab` 내 `guild_npc_idle`, `Main_lev_build_53`, `JungleTilset_8`, Masking 영역 확인.
   - `DialogueScriptData.json` 에 `GuildQuest1Text`, `GuildNormalText` 키 존재 여부.
   - 접수원 Animator `Town_NPC` Controller + `Receptionist` Clip 존재 여부.
   - `OnClickVillage1GuildMissionButton` ([임무]) 핸들러 유무 + 퀘스트 수락/완료 로직.

2. **체크리스트**
   - [ ] Background_Guild + NPC + Building + Tileset 배치?
   - [ ] Masking 처리?
   - [ ] [대화] 버튼 퀘스트 1 분기 → GuildQuest1Text 전체 재생?
   - [ ] [대화] 버튼 그 외 → GuildNormalText 랜덤 1줄?
   - [ ] [임무] 버튼 핸들러 + 현재 퀘스트에 맞춘 수락/완료 로직?
   - [ ] Town_NPC Animator + Receptionist Clip 세팅?
   - [ ] 퀘스트 1번은 [대화] 경유, 나머지는 [임무] 경유 규칙?

3. **누락분만 구현**
   - [임무] 버튼 핸들러가 현재 스텁이면 `QuestManager.AcceptCurrentQuest()` / `CompleteCurrentQuest()` 로직 추가 (슬라이드 24 연계).
   - Town_NPC Animator Controller 없으면 신설, Receptionist Clip 연결.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs` (L491~ OnClickVillage1GuildTalkButton)
- `Assets/@Resources/Prefab/Map/Village1_Guild_BG.prefab`
- `Assets/@Resources/Data/DialogueScriptData.json` (GuildQuest1Text, GuildNormalText)
- `Assets/@Resources/Animator/Town_NPC.controller` (존재 여부)
