// 실행: python _tools/unity_exec.py _tools/verify_player_attack_flow.cs --allow-async
// 새 Play 세션의 마을 / 팝업 없음 / 임시 버프와 일회성 모션 종료 후 실행한다. 실행 중 입력 금지.
// 귀환 후 남은 종료된 Coroutine 핸들도 안전상 거부한다. 기존 실행 중 코루틴은 복원할 수 없기 때문이다.
// AttackTrigger는 직접 호출하지 않는다. 실제 Animator 이벤트가 낸 OnHitLanded와 대상 HP를 측정한다.
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var game = Managers.Game;
var player = game.Player;
var combo = Managers.Combo;
var ui = Managers.UI;
System.Reflection.FieldInfo Field(System.Type type, string name)
{
    for (; type != null; type = type.BaseType)
    {
        var field = type.GetField(name, flags | System.Reflection.BindingFlags.DeclaredOnly);
        if (field != null) return field;
    }
    throw new System.Exception("PlayerAttackFlow: missing field " + name);
}
void SetPlayer(string name, object value) => Field(typeof(PlayerController), name).SetValue(player, value);
void SetCombo(string name, object value) => Field(typeof(ComboManager), name).SetValue(combo, value);
void Require(bool pass, string message)
{
    if (!pass) throw new System.Exception("PlayerAttackFlow prerequisite: " + message);
}
Require(UnityEngine.Application.isPlaying && player != null, "Play mode with an existing player");
Require(!player.InBattle && player.State == Define.State.Idle && ui.GetPopupCount() == 0,
    "Idle player in town, all popups closed");
Require(Managers.Object.MonsterTransform.childCount == 0, "No existing monsters, including pending destruction");
Require(UnityEngine.Object.FindObjectsByType<UI_MonsterHPBar>(UnityEngine.FindObjectsSortMode.None).Length == 0,
    "No existing monster HP bars");
var anim = player.GetComponent<UnityEngine.Animator>();
var rb = player.GetComponent<UnityEngine.Rigidbody2D>();
var sprite = player.GetComponent<UnityEngine.SpriteRenderer>();
var runner = player.AbilityRunner;
Require(anim != null && anim.enabled && anim.runtimeAnimatorController != null && runner != null,
    "Player animator and ability runner initialized");
Require(((System.Collections.ICollection)Field(typeof(AbilityRunner), "_buffs").GetValue(runner)).Count == 0,
    "Wait for temporary ability buffs to expire");
Require(!DG.Tweening.DOTween.IsTweening(player.transform) && !DG.Tweening.DOTween.IsTweening(sprite),
    "Wait for player movement/color tweens to finish");
Require(!(bool)Field(typeof(ComboManager), "_slowMoInFlight").GetValue(combo), "No combo slow-motion coroutine");
var playerFields = new System.Collections.Generic.List<System.Reflection.FieldInfo>();
for (var type = typeof(PlayerController); type != null && type != typeof(UnityEngine.MonoBehaviour); type = type.BaseType)
    playerFields.AddRange(type.GetFields(flags | System.Reflection.BindingFlags.DeclaredOnly).Where(f => !f.IsInitOnly));
foreach (var field in playerFields.Where(f => f.FieldType == typeof(UnityEngine.Coroutine)))
    Require(field.GetValue(player) == null, "Wait for player coroutine " + field.Name + " to finish");
var attackClips = anim.runtimeAnimatorController.animationClips
    .Where(c => c.events.Any(e => e.functionName == "AttackTrigger")).Distinct().ToArray();
Require(attackClips.Length == 1 && attackClips[0].events.Count(e => e.functionName == "AttackTrigger") == 1,
    "Exactly one attack clip with exactly one AttackTrigger event");
// 실제 애셋 길이는 마지막 샘플 프레임을 포함한 61/60초다. 생산 코드의 1초 공격 주기 계약과 구분한다.
Require(UnityEngine.Mathf.Abs(attackClips[0].length - 61f / 60f) < 0.01f
    && UnityEngine.Mathf.Abs(attackClips[0].events.First(e => e.functionName == "AttackTrigger").time - 0.5f) < 0.01f,
    "Independent authored attack contract: 61/60-second clip including final sample, hit at 0.5 seconds");
var autoAttack = typeof(PlayerController).GetMethod("AutoAttack", flags);
var playerUpdate = typeof(PlayerController).GetMethod("Update", flags);
Require(autoAttack != null && playerUpdate != null, "Automatic attack and Update entrypoints exist");
var dataField = Field(typeof(GameManager), "_gameData");
var pathField = Field(typeof(GameManager), "_path");
var sceneField = Field(typeof(UIManager), "_sceneUI");
var hitField = Field(typeof(PlayerController), "OnHitLanded");
var failures = new System.Collections.Generic.List<string>();
int checks = 0;
void Check(bool pass, string message)
{
    checks++;
    if (!pass) failures.Add(message);
}

System.Collections.IEnumerator Run()
{
    var originalData = (GameData)dataField.GetValue(game);
    string originalJson = Newtonsoft.Json.JsonConvert.SerializeObject(originalData);
    object originalPath = pathField.GetValue(game);
    object originalScene = sceneField.GetValue(ui);
    var originalPlayer = playerFields.ToDictionary(f => f, f => f.GetValue(player));
    var comboFields = typeof(ComboManager).GetFields(flags).Where(f => !f.IsInitOnly).ToArray();
    var originalCombo = comboFields.ToDictionary(f => f, f => f.GetValue(combo));
    var seen = (System.Collections.Generic.HashSet<ComboManager.BuffKind>)Field(typeof(ComboManager), "_seenBuffsThisSession").GetValue(combo);
    var originalSeen = new System.Collections.Generic.HashSet<ComboManager.BuffKind>(seen);
    var originalRandom = UnityEngine.Random.state;
    float originalTimeScale = UnityEngine.Time.timeScale;
    float originalAnimSpeed = anim.speed;
    var originalCulling = anim.cullingMode;
    var originalAnimState = anim.GetCurrentAnimatorStateInfo(0);
    var originalPosition = player.transform.position;
    var originalColor = sprite.color;
    bool originalSimulated = rb.simulated;
    var originalVelocity = rb.linearVelocity;
    bool originalRunnerEnabled = runner.enabled;
    bool originalCritical = BattleMath.DebugForcePlayerCritical;
    var errors = new System.Collections.Generic.List<string>();
    UnityEngine.Application.LogCallback logCallback = (message, stack, kind) =>
    {
        if (kind == UnityEngine.LogType.Error || kind == UnityEngine.LogType.Exception || kind == UnityEngine.LogType.Assert)
            errors.Add(message + "\n" + stack);
    };
    MonsterController target = null;
    var hits = new System.Collections.Generic.List<float>();
    float damageSum = 0f;
    float maxFrame = 0f;
    void ClearPlayerCoroutines()
    {
        // 시작 전에 모든 기존 핸들이 null임을 확인했다. 여기서 중단하는 것은 이 검증이 만든 것뿐이다.
        player.StopAllCoroutines();
        foreach (var field in playerFields.Where(f => f.FieldType == typeof(UnityEngine.Coroutine)))
            field.SetValue(player, null);
    }
    void Prepare(float speed, float comboBonus, float potionBonus)
    {
        ClearPlayerCoroutines();
        SetPlayer("_inBattle", false);
        SetPlayer("_attackTarget", null);
        SetPlayer("_lastAtkTime", UnityEngine.Time.time - 10f);
        SetPlayer("<PotionAtkSpeedBonus>k__BackingField", potionBonus);
        SetCombo("<AtkSpeedBonus>k__BackingField", comboBonus);
        player.AtkSpeed = speed;
        player.MaxHp = player.Hp = 1000f;
        anim.speed = 1f;
        player.State = Define.State.Idle;
        anim.Play("Idle", 0, 0f);
        target.State = Define.State.Idle;
        target.Hp = target.MaxHp = 100000f;
        hits.Clear();
        damageSum = 0f;
        maxFrame = 0f;
    }
    void StartAttack() => autoAttack.Invoke(player, null);
    void ObserveFrame() => maxFrame = UnityEngine.Mathf.Max(maxFrame, UnityEngine.Time.deltaTime);
    try
    {
        // 디스크 저장 차단이 복제본 교체보다 먼저, 복원이 모든 정리보다 마지막이다.
        pathField.SetValue(game, null);
        var isolated = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(originalJson);
        isolated.EquippedAbilities.Clear();
        isolated.LearnedAbilities.Clear();
        isolated.AbilityLevels.Clear();
        isolated.CompanionMinMonId = -1;
        isolated.LastBattleAreaId = 0;
        dataField.SetValue(game, isolated);
        // RefreshStats/StartBattle는 동행 재생성·UI·버프 초기화까지 수행하므로 호출하지 않는다.
        // 마을의 InBattle=false를 유지하고 생산 AutoAttack만 구동한다. 실제 Animator/코루틴은 정상 실행된다.
        runner.enabled = false;
        UnityEngine.Time.timeScale = 1f;
        UnityEngine.Random.InitState(8675309);
        BattleMath.DebugForcePlayerCritical = false;
        rb.simulated = false;
        anim.cullingMode = UnityEngine.AnimatorCullingMode.AlwaysAnimate;
        foreach (var field in playerFields.Where(f => typeof(System.Delegate).IsAssignableFrom(f.FieldType)))
            field.SetValue(player, null);
        foreach (var field in comboFields.Where(f => typeof(System.Delegate).IsAssignableFrom(f.FieldType)))
            field.SetValue(combo, null);
        SetCombo("_player", null);
        SetCombo("<Combo>k__BackingField", 0);
        SetCombo("<CurrentTimer>k__BackingField", 0f);
        SetCombo("<DmgBoostBonus>k__BackingField", 0f);
        player.AtkCooldown = 1f;
        player.Accuracy = 100f;
        player.CritRate = 0f;
        player.PhysicalAtk = 100f;
        player.PhysicalAtkRate = 0f;
        player.PhysicalAtkAmp = 1f;
        player.DmgAmp = 0f;
        target = Managers.Object.SpawnMonster(2003, originalPosition + UnityEngine.Vector3.right * 2f);
        Require(target != null, "Verification goblin spawned");
        target.name = "QA_PlayerAttackFlow_Target";
        target.enabled = false;
        target.GetComponent<UnityEngine.Rigidbody2D>().simulated = false;
        foreach (var collider in target.GetComponents<UnityEngine.Collider2D>()) collider.enabled = false;
        Field(typeof(MonsterController), "_hasEngaged").SetValue(target, true);
        target.Evasion = target.Def = target.PhysicalResist = 0f;
        target.DefRate = 0f;
        target.DefAmp = 1f;
        hitField.SetValue(player, (System.Action<float>)(damage =>
        {
            hits.Add(UnityEngine.Time.time);
            damageSum += damage;
        }));
        UnityEngine.Application.logMessageReceived += logCallback;
        // 리소스 최초 로드/폰트 생성 비용은 측정 전에 소진한다.
        Prepare(100f, 0f, 0f);
        StartAttack();
        yield return new UnityEngine.WaitForSeconds(1.2f);
        Require(hits.Count == 1, "Warm-up produced exactly one real animation hit; event wiring is mandatory");

        // 기대값은 생산 속도 계산을 호출하지 않고 독립 수치로 지정한다.
        var cases = new[]
        {
            new[] { 100f, 0f, 0f, 1f },
            new[] { 110f, 0f, 0f, 0.90909091f },
            new[] { 140f, 0f, 0f, 0.71428571f },
            new[] { 100f, 10f, 20f, 0.76923077f },
            new[] { 140f, 10f, 20f, 0.58823529f }
        };
        foreach (var sample in cases)
        {
            Prepare(sample[0], sample[1], sample[2]);
            yield return null;
            float started = UnityEngine.Time.time;
            float deadline = UnityEngine.Time.realtimeSinceStartup + 7f;
            int attackStarts = 0;
            while (hits.Count < 4 && UnityEngine.Time.realtimeSinceStartup < deadline)
            {
                ObserveFrame();
                float before = (float)Field(typeof(PlayerController), "_lastAtkTime").GetValue(player);
                StartAttack();
                if ((float)Field(typeof(PlayerController), "_lastAtkTime").GetValue(player) != before) attackStarts++;
                yield return null;
            }
            string label = $"speed {sample[0]} + combo {sample[1]} + potion {sample[2]}";
            Check(maxFrame <= 0.04f, label + $": timing sample requires >=25 FPS, worst frame={maxFrame:F4}");
            Check(hits.Count == 4 && attackStarts == 4, label + $": exactly one hit per attack, hits={hits.Count}, starts={attackStarts}");
            Check(UnityEngine.Mathf.Abs((target.MaxHp - target.Hp) - damageSum) < 0.1f,
                label + ": target HP loss equals actual event damage, with no companion/bonus damage");
            if (hits.Count == 4)
            {
                Check(UnityEngine.Mathf.Abs(hits[0] - started - sample[3] * 0.5f) <= 0.08f,
                    label + $": first event at half-cycle; actual={hits[0] - started:F4}");
                for (int i = 1; i < hits.Count; i++)
                    Check(UnityEngine.Mathf.Abs(hits[i] - hits[i - 1] - sample[3]) <= 0.055f,
                        label + $": interval {i}, expected={sample[3]:F4}, actual={hits[i] - hits[i - 1]:F4}");
                UnityEngine.Debug.Log($"[PlayerAttackFlow] {label}: "
                    + string.Join(",", hits.Skip(1).Select((time, i) => (time - hits[i]).ToString("F4"))));
            }
        }

        // 이미 시작한 빠른 공격 도중 스탯/물약이 만료되어도 현재 클립 속도와 종료 타이머가 갈라지지 않는다.
        Prepare(140f, 10f, 20f);
        StartAttack();
        float expiryStart = UnityEngine.Time.time;
        yield return new UnityEngine.WaitForSeconds(0.12f);
        player.AtkSpeed = 100f;
        SetPlayer("<PotionAtkSpeedBonus>k__BackingField", 0f);
        SetCombo("<AtkSpeedBonus>k__BackingField", 0f);
        yield return new UnityEngine.WaitForSeconds(0.65f);
        Check(hits.Count == 1 && UnityEngine.Mathf.Abs(hits[0] - expiryStart - 0.29411765f) < 0.08f,
            "Mid-attack buff expiry preserves the current 170-percent attack event");
        Check(player.State == Define.State.Idle, "Mid-attack buff expiry still ends the current attack");
        // 만료 다음 사이클은 기본 속도로 돌아와야 한다. 이전 사이클의 빠른 anim.speed가 남으면 실패한다.
        SetPlayer("_lastAtkTime", UnityEngine.Time.time - 10f);
        hits.Clear();
        float nextStart = UnityEngine.Time.time;
        StartAttack();
        yield return new UnityEngine.WaitForSeconds(0.8f);
        Check(hits.Count == 1 && UnityEngine.Mathf.Abs(hits[0] - nextStart - 0.5f) < 0.08f,
            "First attack after buff expiry uses the restored 100-percent speed");

        // 공격 중 반복 피격: 피해는 받되 실제 키프레임과 빠른 애니메이션은 유지되어야 한다.
        Prepare(140f, 0f, 0f);
        StartAttack();
        float hitStart = UnityEngine.Time.time;
        for (int i = 0; i < 3; i++)
        {
            yield return new UnityEngine.WaitForSeconds(0.12f);
            player.TakeHit(BattleMath.FromRawDamage(7), target);
        }
        yield return new UnityEngine.WaitForSeconds(0.5f);
        Check(player.Hp == 979f, "Nonfatal hits still apply their exact damage during Attack");
        Check(hits.Count == 1 && UnityEngine.Mathf.Abs(hits[0] - hitStart - 0.35714286f) < 0.08f,
            "Repeated hits during Attack neither cancel nor delay its real hit event");
        Check(player.State == Define.State.Idle, "Attack recovers after repeated hits");

        // Idle 피격에서 생성된 옛 CoReturnAfterOnHit이 이후 시작한 Attack을 Idle로 되돌리면 실패한다.
        Prepare(100f, 0f, 0f);
        player.TakeHit(BattleMath.FromRawDamage(7), target);
        yield return new UnityEngine.WaitForSeconds(0.1f);
        StartAttack();
        yield return new UnityEngine.WaitForSeconds(0.75f);
        Check(hits.Count == 1, "Idle hit -> new Attack survives the older hit-recovery coroutine");

        // 사망은 실제 TakeHit/OnDead를 통과한다. 화면 진입점만 격리하여 귀환 코루틴의 자연 종료를 기다린다.
        // 공격/피격 코루틴은 중단하지 않으므로 사망 후 Idle로 되돌리는 회귀도 검출한다.
        Prepare(140f, 0f, 0f);
        sceneField.SetValue(ui, null);
        StartAttack();
        yield return new UnityEngine.WaitForSeconds(0.12f);
        player.TakeHit(BattleMath.FromRawDamage(7), target);
        player.TakeHit(BattleMath.FromRawDamage(2000), target);
        Check(player.Hp == 0f && player.State == Define.State.Dead, "Lethal damage during Attack reaches Dead immediately");
        float deathEnd = UnityEngine.Time.realtimeSinceStartup + 1.5f;
        while (UnityEngine.Time.realtimeSinceStartup < deathEnd)
        {
            playerUpdate.Invoke(player, null);
            yield return null;
        }
        Check(hits.Count == 0 && player.State == Define.State.Dead && player.Hp == 0f,
            "Dead player emits no hit and old attack/hit recovery cannot revive or set Idle");
        Check(errors.Count == 0, "No runtime errors: " + string.Join(" | ", errors));
    }
    finally
    {
        // 실패/예외에서도 원래 참조와 값으로 복원한다. 기존 세이브에 테스트 값이 기록될 틈을 만들지 않는다.
        UnityEngine.Application.logMessageReceived -= logCallback;
        ClearPlayerCoroutines();
        DG.Tweening.DOTween.Kill(sprite, false);
        if (target != null) Managers.Resource.Destroy(target.gameObject);
        dataField.SetValue(game, originalData);
        sceneField.SetValue(ui, originalScene);
        foreach (var pair in originalPlayer) pair.Key.SetValue(player, pair.Value);
        foreach (var pair in originalCombo) pair.Key.SetValue(combo, pair.Value);
        seen.Clear();
        foreach (var kind in originalSeen) seen.Add(kind);
        player.transform.position = originalPosition;
        sprite.color = originalColor;
        rb.simulated = originalSimulated;
        rb.linearVelocity = originalVelocity;
        anim.speed = originalAnimSpeed;
        anim.cullingMode = originalCulling;
        anim.Play(originalAnimState.fullPathHash, 0, originalAnimState.normalizedTime);
        runner.enabled = originalRunnerEnabled;
        BattleMath.DebugForcePlayerCritical = originalCritical;
        UnityEngine.Time.timeScale = originalTimeScale;
        UnityEngine.Random.state = originalRandom;
        pathField.SetValue(game, originalPath);
    }
    Check(object.ReferenceEquals(dataField.GetValue(game), originalData)
        && Newtonsoft.Json.JsonConvert.SerializeObject(originalData) == originalJson,
        "Original GameData reference and all serialized values restored");
    Check(object.Equals(pathField.GetValue(game), originalPath) && object.Equals(sceneField.GetValue(ui), originalScene),
        "Original save path and scene entrypoint restored");
    Check(originalPlayer.All(pair => object.Equals(pair.Key.GetValue(player), pair.Value))
        && originalCombo.All(pair => object.Equals(pair.Key.GetValue(combo), pair.Value)) && seen.SetEquals(originalSeen),
        "Original player stats/delegates/coroutine handles and combo state restored");
    Check(UnityEngine.Time.timeScale == originalTimeScale && UnityEngine.Random.state.Equals(originalRandom)
        && runner.enabled == originalRunnerEnabled && rb.simulated == originalSimulated,
        "Original time scale, random state, runner and physics state restored");
    if (failures.Count != 0)
        throw new System.Exception("[PlayerAttackFlow] FAIL " + failures.Count + "/" + checks + "\n" + string.Join("\n", failures));
    UnityEngine.Debug.Log($"[PlayerAttackFlow] PASS {checks} checks: real hit cadence, buff expiry, hit races, death, restoration");
}
CoroutineManager.StartCoroutine(Run());
return "PlayerAttackFlow started (about 25 seconds); wait for [PlayerAttackFlow] PASS/FAIL. GameData and save path restore in finally.";
