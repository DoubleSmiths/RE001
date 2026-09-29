// 마을에서 unity-cli exec 인자로 실행한다. 실제 저장 경로를 차단한 임시 데이터만 사용한다.
var game = Managers.Game;
var player = game.Player;
if (player == null || player.AbilityRunner == null)
    throw new System.Exception("Player and AbilityRunner must be ready");
var runner = player.AbilityRunner;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
var cooldownField = typeof(AbilityRunner).GetField("_minionCooldown", flags);
var durationField = typeof(AbilityRunner).GetField("_minionCooldownDuration", flags);
var battleField = typeof(AbilityRunner).GetField("_inBattle", flags);
var playerBattleField = typeof(PlayerController).GetField("_inBattle", flags);
if ((bool)battleField.GetValue(runner) || (bool)playerBattleField.GetValue(player))
    throw new System.Exception("Run starter verification in town, outside battle");
if (((System.Collections.ICollection)typeof(AbilityRunner).GetField("_buffs", flags).GetValue(runner)).Count != 0)
    throw new System.Exception("Run starter verification after temporary ability buffs expire");
var originalData = dataField.GetValue(game);
var originalPath = pathField.GetValue(game);
float originalHp = player.Hp;
var originalState = player.State;
var originalPosition = player.transform.position;
var originalRandomState = UnityEngine.Random.state;
float originalCooldown = (float)cooldownField.GetValue(runner);
float originalDuration = (float)durationField.GetValue(runner);
var cooldowns = (System.Collections.Generic.Dictionary<int, float>)typeof(AbilityRunner).GetField("_cooldowns", flags).GetValue(runner);
var originalCooldowns = new System.Collections.Generic.Dictionary<int, float>(cooldowns);
var existingObjects = new System.Collections.Generic.HashSet<int>(
    UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(UnityEngine.FindObjectsInactive.Include,
        UnityEngine.FindObjectsSortMode.None).Select(go => go.GetInstanceID()));
var vfxNames = new System.Collections.Generic.HashSet<string>();
foreach (int sid in new[] { 6001, 6002 })
{
    var prefab = Managers.Resource.Load<UnityEngine.GameObject>(Managers.Data.SkillDic[sid].EffectVfxLabel);
    if (prefab == null) throw new System.Exception($"Starter VFX cannot load: {sid}");
    vfxNames.Add(prefab.name);
}
UI_MinionInfoPopup popup = null;
var failures = new System.Collections.Generic.List<string>();
void Check(bool pass, string message)
{
    if (!pass) failures.Add(message);
}
bool Same(float left, float right) => UnityEngine.Mathf.Approximately(left, right);
try
{
    pathField.SetValue(game, null);
    dataField.SetValue(game, new GameData());
    player.StopBattle();
    cooldownField.SetValue(runner, 0f);
    durationField.SetValue(runner, 0f);
    player.RefreshStats();
    var ability = Managers.Ability;
    var minion = Managers.MinMon;
    float baseMaxHp = player.MaxHp;
    float baseAtkSpeed = player.AtkSpeed;
    int memory = ability.UsedMemory;
    string firstHabitat = Managers.Data.CreatureDic[2001].Habitat;
    var starters = Managers.Data.CreatureDic.Values.Where(c => c.Habitat == firstHabitat).Select(c => c.DataId).ToArray();
    Check(starters.Length == 4, "First habitat contains four minions");
    foreach (int mid in starters)
    {
        int sid = mid + AbilityManager.MINION_SKILL_OFFSET;
        minion.AddMinMon(mid);
        minion.SetCompanion(mid);
        Check(ability.IsCompanionGranted(sid) && runner.ActiveSkillIds.Contains(sid), $"First capture grants active slot: {mid}");
        Check(ability.UsedMemory == memory, $"Companion loan consumes no memory: {mid}");
        Check(!game.LearnedAbilities.Contains(sid) && !game.EquippedAbilities.Contains(sid), $"Companion loan does not persist learning or memorization: {mid}");
        minion.RemoveCompanion();
        Check(!runner.ActiveSkillIds.Contains(sid), $"Dismissal removes borrowed active slot: {mid}");
    }

    minion.SetCompanion(2001);
    int healLevel = UnityEngine.Mathf.Max(1, ability.GetLevel(6001));
    Check(Same(player.MaxHp, baseMaxHp + 30f * healLevel), "6001 preserves +30 maximum HP per level");
    player.Hp = player.MaxHp * 0.1f;
    float beforeHeal = player.Hp;
    float expectedHeal = UnityEngine.Mathf.Min(player.MaxHp,
        beforeHeal + player.MaxHp * 0.18f * healLevel * (1f + player.RecoveryRate / 100f));
    runner.OnBattleStart();
    Check(runner.UseSkill(6001), "6001 manual recovery succeeds");
    Check(player.Hp > beforeHeal && Same(player.Hp, expectedHeal), "6001 heals actual HP with level scaling");
    Check(Same(runner.GetCooldownRemain(6001), 18f), "6001 starts 18-second shared cooldown");

    minion.SetCompanion(2002);
    int hasteLevel = UnityEngine.Mathf.Max(1, ability.GetLevel(6002));
    Check(Same(player.MaxHp, baseMaxHp), "Switching away removes 6001 passive HP");
    Check(Same(player.AtkSpeed, baseAtkSpeed + 10f * hasteLevel), "6002 preserves +10 attack speed per level");
    float passiveSpeed = player.AtkSpeed;
    Check(runner.GetCooldownRemain(6002) > 0f && !runner.UseSkill(6002), "Immediate companion switch cannot bypass shared cooldown");
    Check(Same(player.AtkSpeed, passiveSpeed), "Blocked use does not apply haste");
    // 실제 18초 대기 대신 격리된 런너의 공유 쿨만 초기화한다.
    cooldownField.SetValue(runner, 0f);
    Check(runner.UseSkill(6002), "6002 manual haste succeeds after cooldown");
    Check(Same(player.AtkSpeed, passiveSpeed + 30f * hasteLevel), "6002 applies actual attack-speed buff on top of passive");
    Check(Same(runner.GetCooldownRemain(6002), 18f), "6002 starts 18-second shared cooldown");
    minion.RemoveCompanion();
    Check(Same(player.AtkSpeed, baseAtkSpeed), "Dismissal removes both 6002 buff and passive");
    Check(!ability.IsCompanionGranted(6002) && !runner.ActiveSkillIds.Contains(6002), "Dismissal revokes 6002 active access");
    Check(!runner.UseSkill(6002), "Dismissed borrowed skill cannot be used");

    popup = Managers.UI.ShowPopupUI<UI_MinionInfoPopup>();
    if (popup == null) throw new System.Exception("Minion info popup could not be instantiated");
    popup.SetInfo(2001);
    var skillName = popup.GetComponentsInChildren<TMPro.TMP_Text>(true).SingleOrDefault(t => t.name == "SkillNameText");
    Check(skillName != null && skillName.text == Util.Localize(Managers.Data.SkillDic[6001].Name),
        "Minion popup uses the actual 6001 ability name");
}
finally
{
    try
    {
        if (popup != null) Managers.UI.ClosePopupUI(popup);
        runner.OnBattleEnd();
        Managers.MinMon.RemoveCompanion();
        // 이 검증에서 생성한 원샷 효과만 리소스 매니저로 정리한다.
        foreach (var go in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(
                     UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
        {
            if (!existingObjects.Contains(go.GetInstanceID()) && vfxNames.Contains(go.name))
                Managers.Resource.Destroy(go);
        }
    }
    finally
    {
        dataField.SetValue(game, originalData);
        try
        {
            cooldownField.SetValue(runner, originalCooldown);
            durationField.SetValue(runner, originalDuration);
            cooldowns.Clear();
            foreach (var pair in originalCooldowns) cooldowns[pair.Key] = pair.Value;
            player.RefreshStats(false);
            Managers.Ability.OnCompanionChanged();
            player.Hp = UnityEngine.Mathf.Min(originalHp, player.MaxHp);
            player.State = originalState;
            player.transform.position = originalPosition;
        }
        finally
        {
            UnityEngine.Random.state = originalRandomState;
            // 복원 콜백까지 저장을 막은 뒤 마지막에 원래 경로를 돌려놓는다.
            pathField.SetValue(game, originalPath);
        }
    }
}
if (failures.Count > 0) throw new System.Exception(string.Join("; ", failures));
return "PASS: four starter active loans, free memory, preserved passive stats, real heal/haste, shared cooldown, dismissal cleanup, popup ability name";
