// 실행: python _tools/unity_exec.py _tools/verify_battle_modal_pause.cs --allow-async
// 새 Play 세션의 마을 / 팝업 없음 / 임시 버프와 일회성 모션 종료 후 실행한다. 약 36초간 입력 금지.
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
    throw new System.Exception("BattleModalPause: missing field " + name);
}
void SetPlayer(string name, object value) => Field(typeof(PlayerController), name).SetValue(player, value);
void SetCombo(string name, object value) => Field(typeof(ComboManager), name).SetValue(combo, value);
void Require(bool pass, string message)
{
    if (!pass) throw new System.Exception("BattleModalPause prerequisite: " + message);
}
Require(UnityEngine.Application.isPlaying && player != null, "Play mode with an existing player");
Require(player.enabled && !player.InBattle && player.State == Define.State.Idle && ui.GetPopupCount() == 0 && !ui.IsActiveSoulShop,
    "Idle player in town, all popups closed");
Require(Managers.Object.MonsterTransform.childCount == 0, "No existing monsters, including pending destruction");
Require(UnityEngine.Object.FindObjectsByType<UI_MonsterHPBar>(UnityEngine.FindObjectsSortMode.None).Length == 0,
    "No existing monster HP bars");
var anim = player.GetComponent<UnityEngine.Animator>();
var rb = player.GetComponent<UnityEngine.Rigidbody2D>();
var sprite = player.GetComponent<UnityEngine.SpriteRenderer>();
var runner = player.AbilityRunner;
Require(anim != null && anim.enabled && anim.runtimeAnimatorController != null && runner != null && runner.enabled,
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
    var companions = UnityEngine.Object.FindObjectsByType<CompanionController>(UnityEngine.FindObjectsSortMode.None).ToDictionary(c => c, c => c.enabled);
    Require(companions.Keys.All(c => !(bool)Field(typeof(CompanionController), "_attacking").GetValue(c)), "No companion attack coroutine in progress");
    var runnerFields = typeof(AbilityRunner).GetFields(flags).Where(f => !f.IsInitOnly).ToArray();
    var originalRunner = runnerFields.ToDictionary(f => f, f => f.GetValue(runner));
    var uiFields = typeof(UIManager).GetFields(flags).Where(f => !f.IsInitOnly).ToArray();
    var originalUi = uiFields.ToDictionary(f => f, f => f.GetValue(ui));
    var cooldowns = (System.Collections.Generic.Dictionary<int, float>)Field(typeof(AbilityRunner), "_cooldowns").GetValue(runner);
    var originalCooldowns = new System.Collections.Generic.Dictionary<int, float>(cooldowns);
    var firedLow = (System.Collections.Generic.HashSet<int>)Field(typeof(AbilityRunner), "_firedLowHp").GetValue(runner);
    var firedEnemy = (System.Collections.Generic.HashSet<int>)Field(typeof(AbilityRunner), "_firedEnemyLowHp").GetValue(runner);
    var originalLow = new System.Collections.Generic.HashSet<int>(firedLow);
    var originalEnemy = new System.Collections.Generic.HashSet<int>(firedEnemy);
    var scrollers = UnityEngine.Object.FindObjectsByType<BackgroundScroller>(UnityEngine.FindObjectsSortMode.None).ToDictionary(x => x, x => x.IsScrolling);
    float originalTweenScale = DG.Tweening.DOTween.timeScale;
    var originalAnimMode = anim.updateMode;
    var originalRandom = UnityEngine.Random.state;
    var existingObjects = new System.Collections.Generic.HashSet<int>(UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(
        UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Select(go => go.GetInstanceID()));
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
        foreach (var field in playerFields.Where(f => f.FieldType == typeof(UnityEngine.Coroutine)))
        {
            var owned = field.GetValue(player) as UnityEngine.Coroutine;
            if (owned != null) player.StopCoroutine(owned);
        }
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
        // 기존 동행은 보존하면서 이번 플레이어 1회 타격 측정에만 개입하지 않게 한다.
        foreach (var companion in companions.Keys) companion.enabled = false;
        var isolated = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(originalJson);
        isolated.EquippedAbilities.Clear();
        isolated.LearnedAbilities.Clear();
        isolated.AbilityLevels.Clear();
        isolated.CompanionMinMonId = -1;
        isolated.LastBattleAreaId = 0;
        isolated.AutoPotionMemorized = isolated.AutoPotionOn = false;
        isolated.SpdPotionRevealed = true;
        isolated.SpdPotionPopupAnimPlayed = true;
        dataField.SetValue(game, isolated);
        // RefreshStats/StartBattle는 동행 재생성·UI·버프 초기화까지 수행하므로 호출하지 않는다.
        // 워밍업 후 InBattle=true로 바꾼다. 지역 UI 코루틴 없이 실제 Animator/Player.Update를 사용한다.
        // 실제 Update를 켠 상태로 정지 가드를 검증한다.
        runner.enabled = true;
        Field(typeof(AbilityRunner), "_inBattle").SetValue(runner, false);
        cooldowns.Clear();
        firedLow.Clear();
        firedEnemy.Clear();
        foreach (var field in runnerFields.Where(f => typeof(System.Delegate).IsAssignableFrom(f.FieldType))) field.SetValue(runner, null);
        foreach (var field in uiFields.Where(f => typeof(System.Delegate).IsAssignableFrom(f.FieldType))) field.SetValue(ui, null);
        UnityEngine.Time.timeScale = 1f;
        UnityEngine.Random.InitState(8675309);
        BattleMath.DebugForcePlayerCritical = false;
        rb.simulated = false;
        anim.cullingMode = UnityEngine.AnimatorCullingMode.AlwaysAnimate;
        anim.updateMode = UnityEngine.AnimatorUpdateMode.Normal;
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
        target.name = "QA_BattleModalPause_Target";
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

        // 첫 타격 전 도감을 열어 공격 진행과 세 종류 타이머의 동일한 정지를 검증한다.
        Prepare(100f, 0f, 0f);
        player.AtkCooldown = 100f; // 다음 공격만 막는다. 현재 클립 속도와 0.5초 이벤트는 그대로다.
        player.Hp = 500f;
        var potion = ui.ShowPopupUI<UI_PotionPopup>();
        Require(potion != null, "Actual potion popup loaded");
        var hpSlot = Field(typeof(UI_PotionPopup), "_hpSlot").GetValue(potion);
        var spdSlot = Field(typeof(UI_PotionPopup), "_spdSlot").GetValue(potion);
        Require(hpSlot != null && spdSlot != null, "Both potion slots bound");
        float PF(string name) => (float)Field(typeof(PlayerController), name).GetValue(player);
        float RF(string name) => (float)Field(typeof(AbilityRunner), name).GetValue(runner);
        float SF(object slot) => (float)Field(slot.GetType(), "cooldown").GetValue(slot);
        void SR(string name, object value) => Field(typeof(AbilityRunner), name).SetValue(runner, value);
        void SS(object slot, string name, object value) => Field(slot.GetType(), name).SetValue(slot, value);
        bool Same(float a, float b) => UnityEngine.Mathf.Abs(a - b) < 0.0001f;
        yield return new UnityEngine.WaitForSecondsRealtime(0.3f);
        UnityEngine.Time.timeScale = 0.5f;
        SetPlayer("_inBattle", true);
        SR("_inBattle", true);
        isolated.LearnedAbilities.AddRange(new[] { 6001, 5116 });
        isolated.EquippedAbilities.AddRange(new[] { 6001, 5116 });
        cooldowns[5116] = 60f;
        SR("_minionCooldown", 60f);
        SR("_minionCooldownDuration", 60f);
        SetPlayer("_autoPotionCd", 60f);
        SetPlayer("_spdPotionCd", 60f);
        foreach (var slot in new[] { hpSlot, spdSlot })
        {
            SS(slot, "cooldown", 60f);
            SS(slot, "cooldownMax", 60f);
        }
        SetCombo("<Combo>k__BackingField", 3);
        SetCombo("<NowCombo>k__BackingField", 3f);
        SetCombo("<CurrentTimer>k__BackingField", 10f);
        SetPlayer("_lastAtkTime", UnityEngine.Time.time - 200f);
        StartAttack();
        yield return new UnityEngine.WaitForSeconds(0.3f);
        Require(hits.Count == 0 && player.State == Define.State.Attack, "Open at 0.3 seconds before the authored hit");
        ui.ShowPopupUI<UI_BestiaryPopup>();
        Require(UnityEngine.Time.timeScale == 0f, "RED: battle Bestiary did not pause gameplay");
        yield return null;
        float pausedTime = UnityEngine.Time.time;
        float targetHp = target.Hp, hp = player.Hp;
        float normalized = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float comboTimer = combo.CurrentTimer;
        float hpCd = PF("_autoPotionCd"), spdCd = PF("_spdPotionCd");
        float abilityCd = RF("_minionCooldown"), timedCd = cooldowns[5116];
        float hpUiCd = SF(hpSlot), spdUiCd = SF(spdSlot);
        object attackCo = Field(typeof(PlayerController), "_attackResetCo").GetValue(player);
        float pauseStarted = UnityEngine.Time.realtimeSinceStartup;
        var detail = ui.ShowPopupUI<UI_SkillDetailPopup>();
        detail.SetInfo(6001);
        yield return new UnityEngine.WaitForSecondsRealtime(0.25f);
        Check(UnityEngine.Time.timeScale == 0f, "Non-pausing detail above Bestiary retains pause");
        ui.ClosePopupUI(detail);
        Check(UnityEngine.Time.timeScale == 0f, "Closing nested detail retains underlying pause");
        while (UnityEngine.Time.realtimeSinceStartup - pauseStarted < 30f) yield return null;
        Check(player.enabled && runner.enabled && player.InBattle, "Real Player/AbilityRunner Updates stayed enabled");
        Check(hits.Count == 0 && Same(target.Hp, targetHp) && Same(player.Hp, hp), "Thirty real seconds: no hit, damage or healing");
        Check(Same(UnityEngine.Time.time, pausedTime) && Same(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, normalized), "Scaled time and actual attack clip stay frozen");
        Check(object.ReferenceEquals(Field(typeof(PlayerController), "_attackResetCo").GetValue(player), attackCo)
            && player.State == Define.State.Attack, "Original attack coroutine remains suspended, not cancelled");
        Check(combo.Combo == 3 && Same(combo.CurrentTimer, comboTimer), "Combo count and remaining duration freeze");
        Check(Same(PF("_autoPotionCd"), hpCd) && Same(PF("_spdPotionCd"), spdCd), "Both Player potion cooldowns freeze");
        Check(Same(SF(hpSlot), hpUiCd) && Same(SF(spdSlot), spdUiCd), "Actual PotionPopup cooldowns freeze");
        Check(Same(RF("_minionCooldown"), abilityCd) && Same(cooldowns[5116], timedCd), "Shared and timed ability cooldowns freeze");
        UnityEngine.Debug.Log($"[BattleModalPause] pause={UnityEngine.Time.realtimeSinceStartup - pauseStarted:F3}s real, "
            + $"scaled={UnityEngine.Time.time - pausedTime:F6}s, normalized={normalized:F6}->{anim.GetCurrentAnimatorStateInfo(0).normalizedTime:F6}, "
            + $"hits={hits.Count}, targetHP={targetHp:F1}->{target.Hp:F1}, playerHP={hp:F1}->{player.Hp:F1}, combo={comboTimer:F6}->{combo.CurrentTimer:F6}");
        UnityEngine.Debug.Log($"[BattleModalPause] cooldowns before->after: playerHP={hpCd:F6}->{PF("_autoPotionCd"):F6}, "
            + $"playerSpeed={spdCd:F6}->{PF("_spdPotionCd"):F6}, uiHP={hpUiCd:F6}->{SF(hpSlot):F6}, uiSpeed={spdUiCd:F6}->{SF(spdSlot):F6}, "
            + $"sharedAbility={abilityCd:F6}->{RF("_minionCooldown"):F6}, timedAbility={timedCd:F6}->{cooldowns[5116]:F6}");

        // 준비된 사용을 실제 생산 진입점으로 시도한다.
        SR("_minionCooldown", 0f);
        Check(!runner.UseSkill(6001) && Same(player.Hp, hp), "Ready manual ability cannot activate while paused");
        SR("_minionCooldown", abilityCd);
        int count = (int)Field(hpSlot.GetType(), "count").GetValue(hpSlot);
        SS(hpSlot, "cooldown", 0f);
        typeof(UI_PotionPopup).GetMethod("OnClickSlot", flags).Invoke(potion, new[] { hpSlot });
        Check((int)Field(hpSlot.GetType(), "count").GetValue(hpSlot) == count && Same(player.Hp, hp), "Ready potion click consumes nothing while paused");
        SS(hpSlot, "cooldown", hpUiCd);
        isolated.AutoPotionMemorized = isolated.AutoPotionOn = true;
        isolated.AutoPotionHpRatio = 0.9f;
        SetPlayer("_autoPotionCd", 0f);
        yield return null;
        yield return null;
        Check(Same(player.Hp, hp) && Same(PF("_autoPotionCd"), 0f), "Enabled Player.Update cannot auto-drink while paused");
        isolated.AutoPotionMemorized = isolated.AutoPotionOn = false;
        SetPlayer("_autoPotionCd", hpCd);

        ui.ClosePopupUI(); // 도감만 닫음. 비정지 물약창은 여전히 아래에 남아 있다.
        Check(Same(UnityEngine.Time.timeScale, 0.5f) && ui.GetPopupCount() == 1, "Last blocking popup restores 0.5 with non-pausing popup beneath");
        ui.ClosePopupUI(potion);
        float resumed = UnityEngine.Time.time;
        float deadline = UnityEngine.Time.realtimeSinceStartup + 5f;
        while (hits.Count == 0 && UnityEngine.Time.realtimeSinceStartup < deadline) yield return null;
        Check(hits.Count == 1 && UnityEngine.Mathf.Abs(targetHp - target.Hp - damageSum) < 0.1f, "Closing resumes exactly one real attack event and matching damage");
        Check(hits.Count == 1 && hits[0] > resumed && hits[0] - resumed < 0.4f, "Remaining pre-hit portion resumes rather than restarting the attack");
        yield return new UnityEngine.WaitForSecondsRealtime(1.7f);
        Check(hits.Count == 1 && player.State == Define.State.Idle, "No duplicate hit after attack reset completes");
        UnityEngine.Debug.Log($"[BattleModalPause] resume: scale={UnityEngine.Time.timeScale:F2}, "
            + $"hitDelta={(hits.Count > 0 ? hits[0] - resumed : -1f):F6}s scaled, hits={hits.Count}, "
            + $"damage={damageSum:F1}, targetHPDelta={targetHp - target.Hp:F1}, finalState={player.State}");

        SetPlayer("_inBattle", false);
        SR("_inBattle", false);
        ui.ShowPopupUI<UI_BestiaryPopup>();
        Check(Same(UnityEngine.Time.timeScale, 0.5f), "Town Bestiary does not pause");
        ui.ClosePopupUI();
        SetPlayer("_inBattle", true);
        ui.IsActiveSoulShop = true;
        ui.ShowPopupUI<UI_BestiaryPopup>();
        ui.IsActiveSoulShop = false;
        Check(UnityEngine.Time.timeScale == 0f, "Soul shop release retains overlapping battle modal pause");
        ui.ClosePopupUI();
        Check(Same(UnityEngine.Time.timeScale, 0.5f), "Soul shop first: final modal close restores 0.5");
        ui.ShowPopupUI<UI_BestiaryPopup>();
        ui.IsActiveSoulShop = true;
        ui.ClosePopupUI();
        Check(UnityEngine.Time.timeScale == 0f, "Modal close retains overlapping soul shop pause");
        ui.IsActiveSoulShop = false;
        Check(Same(UnityEngine.Time.timeScale, 0.5f), "Modal first: final soul shop release restores 0.5");
        Check(errors.Count == 0, "No runtime errors: " + string.Join(" | ", errors));
    }
    finally
    {
        // 실패/예외에서도 원래 참조와 값으로 복원한다. 기존 세이브에 테스트 값이 기록될 틈을 만들지 않는다.
        UnityEngine.Application.logMessageReceived -= logCallback;
        try
        {
            ui.CloseAllPopupUI();
            ui.IsActiveSoulShop = false;
            ClearPlayerCoroutines();
            DG.Tweening.DOTween.Kill(sprite, false);
            if (target != null) Managers.Resource.Destroy(target.gameObject);
            // 사용 가드 실패 시 생길 수 있는 이 실행의 회복 이펙트만 정리한다.
            foreach (var go in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(
                         UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
                if (!existingObjects.Contains(go.GetInstanceID()) && go.name == "HP Potion Ef") Managers.Resource.Destroy(go);
        }
        finally
        {
            dataField.SetValue(game, originalData);
            sceneField.SetValue(ui, originalScene);
            foreach (var pair in originalPlayer) pair.Key.SetValue(player, pair.Value);
            foreach (var pair in originalCombo) pair.Key.SetValue(combo, pair.Value);
            foreach (var pair in originalRunner) pair.Key.SetValue(runner, pair.Value);
            foreach (var pair in originalUi) pair.Key.SetValue(ui, pair.Value);
            cooldowns.Clear();
            foreach (var pair in originalCooldowns) cooldowns[pair.Key] = pair.Value;
            firedLow.Clear(); foreach (int id in originalLow) firedLow.Add(id);
            firedEnemy.Clear(); foreach (int id in originalEnemy) firedEnemy.Add(id);
            ((System.Collections.IList)Field(typeof(AbilityRunner), "_buffs").GetValue(runner)).Clear();
            foreach (var pair in scrollers) if (pair.Key != null) pair.Key.IsScrolling = pair.Value;
            foreach (var pair in companions) if (pair.Key != null) pair.Key.enabled = pair.Value;
            seen.Clear();
            foreach (var kind in originalSeen) seen.Add(kind);
            player.transform.position = originalPosition;
            sprite.color = originalColor;
            rb.simulated = originalSimulated;
            rb.linearVelocity = originalVelocity;
            anim.speed = originalAnimSpeed;
            anim.cullingMode = originalCulling;
            anim.updateMode = originalAnimMode;
            DG.Tweening.DOTween.timeScale = originalTweenScale;
            anim.Play(originalAnimState.fullPathHash, 0, originalAnimState.normalizedTime);
            runner.enabled = originalRunnerEnabled;
            BattleMath.DebugForcePlayerCritical = originalCritical;
            UnityEngine.Time.timeScale = originalTimeScale;
            UnityEngine.Random.state = originalRandom;
            pathField.SetValue(game, originalPath);
        }
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
    Check(originalRunner.All(pair => object.Equals(pair.Key.GetValue(runner), pair.Value))
        && cooldowns.Count == originalCooldowns.Count
        && originalCooldowns.All(pair => cooldowns.TryGetValue(pair.Key, out float value) && value == pair.Value)
        && firedLow.SetEquals(originalLow) && firedEnemy.SetEquals(originalEnemy), "Original runner fields, delegates, cooldown dictionary and trigger sets restored");
    Check(ui.GetPopupCount() == 0 && player.enabled && runner.enabled && DG.Tweening.DOTween.timeScale == originalTweenScale,
        "No remaining owned popup; Updates and tween scale restored");
    if (failures.Count != 0)
        throw new System.Exception("[BattleModalPause] FAIL " + failures.Count + "/" + checks + "\n" + string.Join("\n", failures));
    UnityEngine.Debug.Log($"[BattleModalPause] PASS {checks} checks: 30-second attack freeze, cooldowns, input guards, single-hit resume, nested pause ownership, restoration");
}
CoroutineManager.StartCoroutine(Run());
return "BattleModalPause started (about 36 seconds); wait for [BattleModalPause] PASS/FAIL. GameData and save path restore in finally.";
