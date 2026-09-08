// 임시 GameData에서 정상 재료로 다음 물약 제작 후 원래 상태 복원.
var game = Managers.Game;
var field = typeof(GameManager).GetField("_gameData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var original = field.GetValue(game);
try
{
    field.SetValue(game, Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(original), original.GetType()));
    game.AddItem(10001, 10);
    int before = game.GetItemCount(10001);
    if (!game.TryCraftSpeedPotion(40001, out int id) || id != 40002 || game.GetItemCount(10001) != before - 10)
        throw new System.Exception("Speed potion recipe is unreachable or consumes wrong amount");
}
finally
{
    field.SetValue(game, original);
    game.SaveGame();
}
return "PASS: speed potion 40001->40002 consumes 10 valid jelly items";
