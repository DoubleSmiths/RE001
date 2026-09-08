// 백업된 검증 세이브에서만 실행. 원래 GameData·퀘스트 상태를 finally에서 복원한다.
var game = Managers.Game;
var field = typeof(GameManager).GetField("_gameData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var original = field.GetValue(game);
var copy = Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(original), original.GetType());
var process = typeof(QuestManager).GetMethod("ProcessUnlocks", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
try
{
    field.SetValue(game, copy);
    foreach (int have in new[] { 10, 3 })
    {
        game.RemoveItem(11001, game.GetItemCount(11001));
        game.AddItem(11001, have);
        process.Invoke(Managers.Quest, new object[] { 9 });
        if (game.GetItemCount(11001) != have - 3) throw new System.Exception("Submission consumed surplus materials");
    }
    game.AddItem(20001, 2);
    int jelly = game.GetItemCount(10001);
    process.Invoke(Managers.Quest, new object[] { 4 });
    if (game.GetItemCount(20001) != 0 || game.GetItemCount(10001) != jelly) throw new System.Exception("Certificate exchange changed");
}
finally
{
    Managers.UI.CloseAllPopupUI();
    field.SetValue(game, original);
    game.SaveGame();
    Managers.Quest.JumpToQuest(game.CurrentQuestID);
}
return "PASS: surplus 10->7, exact 3->0, certificate removed and jelly preserved";
