// _path == null인 마을 QA 세션에서 실행. 실제 ComboManager.Tick을 동기 호출하고 전역 상태는 원복한다.
var game = Managers.Game;
var ability = Managers.Ability;
var combo = Managers.Combo;
var player = game.Player;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
if (pathField.GetValue(game) != null || player == null || player.InBattle)
    throw new System.Exception("ComboKeep verification requires _path == null and a player in town");
if (((System.Collections.ICollection)typeof(AbilityRunner).GetField("_buffs", flags).GetValue(player.AbilityRunner)).Count != 0)
    throw new System.Exception("Wait for temporary ability buffs to expire");
var originalData = (GameData)dataField.GetValue(game);
string originalJson = Newtonsoft.Json.JsonConvert.SerializeObject(originalData);
float originalHp = player.Hp;
float originalTimeScale = UnityEngine.Time.timeScale;
var comboFields = typeof(ComboManager).GetFields(flags);
var originalFields = new System.Collections.Generic.Dictionary<System.Reflection.FieldInfo, object>();
foreach (var field in comboFields)
    if (!field.IsInitOnly) originalFields.Add(field, field.GetValue(combo));
var seen = (System.Collections.Generic.HashSet<ComboManager.BuffKind>)typeof(ComboManager).GetField("_seenBuffsThisSession", flags).GetValue(combo);
var originalSeen = new System.Collections.Generic.HashSet<ComboManager.BuffKind>(seen);
int checks = 0;
void Check(bool pass, string message)
{
    if (!pass) throw new System.Exception("ComboKeep: " + message);
    checks++;
}
bool Same(float a, float b) => UnityEngine.Mathf.Abs(a - b) < 0.0001f;
void Set(string field, object value) => typeof(ComboManager).GetField(field, flags).SetValue(combo, value);
void Seed(float timer)
{
    Set("<Combo>k__BackingField", 1);
    Set("<NowRank>k__BackingField", 0);
    Set("<CurrentTimer>k__BackingField", timer);
    Set("_lastTickFrame", -1);
}
void Tick(float dt)
{
    // 프레임 양보 없이 독립 시간 구간을 재현한다. 같은 프레임 중복 호출 검사는 아래에서 직접 한다.
    Set("_lastTickFrame", -1);
    combo.Tick(dt);
}
try
{
    var isolated = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(originalJson);
    isolated.EquippedAbilities.Clear();
    isolated.LearnedAbilities.Clear();
    isolated.AbilityLevels.Clear();
    isolated.EquippedEquipments.Clear();
    dataField.SetValue(game, isolated);
    // 원래 구독과 플레이어 연결은 finally에서 그대로 복원한다. 테스트 중 HUD/브레이크 FX를 만들지 않는다.
    foreach (var field in comboFields)
        if (typeof(System.Delegate).IsAssignableFrom(field.FieldType)) field.SetValue(combo, null);
    Set("_player", null);
    Set("_slowMoInFlight", true);
    var bonusProperty = typeof(AbilityManager).GetProperty("ComboKeepBonus");
    Check(bonusProperty != null, "ComboKeep bonus consumer API exists");
    Check(Managers.Data.SkillDic[5142].StatType == "ComboKeep" && Same(Managers.Data.SkillDic[5142].StatBonus, 10f)
        && Managers.Data.SkillDic[5142].MaxLevel == 4, "Independent 10 percent per level data contract");
    Check(Same((float)bonusProperty.GetValue(ability), 0f), "Unmemorized ability grants zero bonus");
    Seed(2f);
    Tick(0.7f);
    Check(Same(combo.CurrentTimer, 1.3f), "Zero percent loses exactly 0.7 timer units");

    ability.Learn(5142);
    Check(ability.Memorize(5142), "Ability can be memorized");
    float beforeMemoryChange = combo.CurrentTimer;
    Check(Same(beforeMemoryChange, 1.3f), "Memorization does not refill timer");
    Check(Same((float)bonusProperty.GetValue(ability), 10f), "Level one grants ten percent");
    Seed(2f);
    Tick(0.7f);
    Check(Same(combo.CurrentTimer, 1.36363636f), "Ten percent loses 0.63636364 in the same 0.7 seconds");
    float once = combo.CurrentTimer;
    combo.Tick(0.7f);
    Check(Same(combo.CurrentTimer, once), "Duplicate tick in the same frame has no effect");

    ability.SetLevel(5142, 4);
    Check(Same(combo.CurrentTimer, once), "Level changes do not refill timer");
    Check(Same((float)bonusProperty.GetValue(ability), 40f), "Level four grants forty percent");
    Seed(0f);
    Set("<Combo>k__BackingField", 0);
    combo.SimulateHit(1f);
    var hitStage = combo.GetCurrentStage();
    Check(Same(combo.CurrentTimer, UnityEngine.Mathf.Min(hitStage.HitGain, hitStage.MaxTimer)),
        "Forty percent does not increase hit gain or capacity");
    Seed(2f);
    Tick(0.7f);
    Check(Same(combo.CurrentTimer, 1.5f), "Forty percent loses exactly 0.5 in 0.7 seconds");
    var stage = combo.GetCurrentStage();
    Check(stage != null && stage.MaxTimer > 0f, "Actual combo stage exists");
    Check(Same(combo.NormalizedTimer, UnityEngine.Mathf.Clamp01(1.5f / stage.MaxTimer)), "Normalization still uses stage MaxTimer");
    Check(ability.Forget(5142), "Ability can be forgotten");
    Check(Same(combo.CurrentTimer, 1.5f), "Forgetting does not change current timer");
    Tick(0.7f);
    Check(Same(combo.CurrentTimer, 0.8f), "Forgetting immediately restores base decay");

    foreach (int level in new[] { 0, 1, 4 })
    {
        if (ability.IsEquipped(5142)) Check(ability.Forget(5142), "Reset memory for expiry case");
        if (level > 0)
        {
            ability.SetLevel(5142, level);
            Check(ability.Memorize(5142), "Memorize expiry case");
        }
        Seed(1f);
        float duration = level == 0 ? 1f : level == 1 ? 1.1f : 1.4f;
        Tick(duration - 0.01f);
        Check(combo.CurrentTimer > 0f && combo.Combo == 1, $"Level {level} survives until just before expected expiry");
        Tick(0.02f);
        Check(combo.CurrentTimer == 0f && combo.Combo == 0 && combo.NormalizedTimer == 0f, $"Level {level} expires at expected duration");
    }
    Check(Same(UnityEngine.Time.timeScale, originalTimeScale), "Expiry does not change time scale");
}
finally
{
    dataField.SetValue(game, originalData);
    foreach (var pair in originalFields) pair.Key.SetValue(combo, pair.Value);
    seen.Clear();
    foreach (var kind in originalSeen) seen.Add(kind);
    UnityEngine.Time.timeScale = originalTimeScale;
    player.RefreshStats(false);
    player.Hp = originalHp;
    typeof(AbilityManager).GetMethod("RaiseChanged", flags).Invoke(ability, null);
}
Check(pathField.GetValue(game) == null && Newtonsoft.Json.JsonConvert.SerializeObject(originalData) == originalJson,
    "Original ability/save data restored and disk writes remain disabled");
Check(originalFields.All(pair => object.Equals(pair.Key.GetValue(combo), pair.Value)) && seen.SetEquals(originalSeen),
    "All combo fields and mutable session state restored");
return $"PASS: ComboKeep {checks} checks; 0/10/40 percent decay, no refill, forget, frame guard, expiry, state restoration";
