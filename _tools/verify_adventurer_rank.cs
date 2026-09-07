// 완료 보고를 실제로 마친 세이브에서 실행. 저장값과 HUD가 함께 반영되는지 검사.
int expected = Managers.Data.QuestDic.Values.Count(q => q.QuestID < Managers.Game.CurrentQuestID
    && q.CompleteType == Define.QuestCompleteType.BTN_CLICK && q.CompleteTarget == Define.BTN_GUILD_QUEST);
if (expected == 0) throw new System.Exception("Complete a guild report before running this check");
if (Managers.Game.AdventurerRank < expected)
    throw new System.Exception($"Rank missing: completed reports={expected}, saved rank={Managers.Game.AdventurerRank}");
var rankText = (Managers.UI.SceneUI as UI_GameScene).GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "RankTxt");
if (rankText.text != Managers.Game.AdventurerRank.ToString())
    throw new System.Exception("Rank HUD is stale: " + rankText.text);
return $"PASS completed reports={expected}, saved rank={Managers.Game.AdventurerRank}, HUD={rankText.text}";
