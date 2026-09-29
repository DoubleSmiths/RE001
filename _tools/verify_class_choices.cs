// 실제 사용자 저장 경로를 차단한 마을에서만 실행한다.
// 같은 레벨·기억 비용으로 방어형/체력형 능력의 실제 스탯과 피해 계산을 비교한다.
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var game = Managers.Game;
var player = game.Player;
var dataField = typeof(GameManager).GetField("_gameData", flags);
if (!UnityEditor.EditorApplication.isPlaying || player == null || player.InBattle
    || typeof(GameManager).GetField("_path", flags).GetValue(game) != null)
    throw new System.Exception("Isolated town session required");
if (((System.Collections.ICollection)typeof(AbilityRunner).GetField("_buffs", flags).GetValue(player.AbilityRunner)).Count != 0)
    throw new System.Exception("No temporary buffs allowed");
var original = dataField.GetValue(game);
float hp = player.Hp;
var random = UnityEngine.Random.state;
bool forceCrit = BattleMath.DebugForcePlayerCritical;
int checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new System.Exception("ClassChoices: " + message);
    checks++;
}
bool Same(float a, float b) => UnityEngine.Mathf.Abs(a - b) < 0.001f;
int DamageAt(float attack)
{
    // 추가 엔티티를 만들지 않고 동일 난수의 가상 공격으로 실제 BattleMath 경로를 확인한다.
    player.PhysicalAtk = attack;
    player.CritRate = 0f;
    player.Accuracy = 100f;
    player.Evasion = 0f;
    player.PhysicalResist = 0f;
    UnityEngine.Random.InitState(721);
    return BattleMath.Resolve(player, player).FinalDamage;
}
try
{
    BattleMath.DebugForcePlayerCritical = false;
    dataField.SetValue(game, new GameData());
    game.Level = 5;
    player.RefreshStats();
    float baseHp = player.MaxHp, baseDef = player.Def;
    var ability = Managers.Ability;
    Check(Managers.Data.SkillDic[5004].MemoryCapacity == 2 && Managers.Data.SkillDic[5125].MemoryCapacity == 2,
        "Defense and health options use the same two memory points");
    ability.Learn(5004);
    player.RefreshStats();
    Check(Same(player.MaxHp, baseHp) && Same(player.Def, baseDef), "Learning without memorizing gives no stat bonus");
    Check(ability.Memorize(5004), "Defense ability can be memorized");
    player.RefreshStats();
    Check(ability.UsedMemory == 2 && Same(player.MaxHp, baseHp + 50f) && Same(player.Def, baseDef + 5f),
        "Level one grants HP 50 and defense 5 once, for memory two");
    float shieldHp = player.MaxHp;
    int shieldSmallHit = DamageAt(20f), shieldLargeHit = DamageAt(70f);
    ability.SetLevel(5004, 5);
    player.RefreshStats();
    Check(Same(player.MaxHp, baseHp + 250f) && Same(player.Def, baseDef + 25f) && ability.UsedMemory == 2,
        "Both stat bonuses scale to level five without increasing memory cost");
    var steelSave = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(dataField.GetValue(game)));
    dataField.SetValue(game, steelSave);
    player.RefreshStats();
    Check(ability.IsEquipped(5004) && ability.GetLevel(5004) == 5 && ability.UsedMemory == 2
        && Same(player.MaxHp, baseHp + 250f) && Same(player.Def, baseDef + 25f),
        "Existing high-level memorized steel ability restores both stats and reduced cost");
    Check(ability.Forget(5004), "Defense ability can be forgotten");
    player.RefreshStats();
    Check(Same(player.MaxHp, baseHp) && Same(player.Def, baseDef) && ability.UsedMemory == 0,
        "Forgetting removes both bonuses and refunds memory");
    ability.Learn(5125);
    Check(ability.Memorize(5125), "Health ability can be memorized");
    player.RefreshStats();
    Check(ability.UsedMemory == 2 && Same(player.MaxHp, baseHp + 120f) && Same(player.Def, baseDef),
        "Health option grants HP 120 without defense at the same memory cost");
    float healthHp = player.MaxHp;
    int healthSmallHit = DamageAt(20f), healthLargeHit = DamageAt(70f);
    Check(shieldSmallHit < healthSmallHit && shieldLargeHit < healthLargeHit, "Defense reduces actual calculated damage");
    Check(shieldHp / shieldSmallHit > healthHp / healthSmallHit, "Defense option withstands more weak repeated hits");
    Check(healthHp / healthLargeHit > shieldHp / shieldLargeHit, "Health option withstands more heavy hits");
    string json = Newtonsoft.Json.JsonConvert.SerializeObject(dataField.GetValue(game));
    var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(json);
    dataField.SetValue(game, restored);
    player.RefreshStats();
    Check(ability.IsLearned(5004) && ability.IsEquipped(5125) && ability.GetLevel(5004) == 5
        && Same(player.MaxHp, baseHp + 120f) && ability.UsedMemory == 2, "Ability choice and level survive save serialization");
    UnityEngine.Debug.Log($"ClassChoices samples: defense HP {shieldHp}, damage {shieldSmallHit}/{shieldLargeHit}; health HP {healthHp}, damage {healthSmallHit}/{healthLargeHit}");
}
finally
{
    dataField.SetValue(game, original);
    player.RefreshStats(false);
    player.Hp = hp;
    UnityEngine.Random.state = random;
    BattleMath.DebugForcePlayerCritical = forceCrit;
    typeof(AbilityManager).GetMethod("RaiseChanged", flags).Invoke(Managers.Ability, null);
}
return $"PASS: ClassChoices {checks} checks; dual stats, memory, damage tradeoff, forget, level and save restore";
