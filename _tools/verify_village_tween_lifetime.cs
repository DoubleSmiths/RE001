// 격리 마을 플레이 세션(_path == null), 팝업/전환/해금 연출이 없는 상태에서 실행한다.
// 실제 EnterAreaWithFade의 암전 교체 직전에 이전 그룹의 연출을 재시작해 파괴 경합을 확정한다.
// python _tools/unity_exec.py _tools/verify_village_tween_lifetime.cs --allow-async
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var game = Managers.Game;
var pathField = typeof(GameManager).GetField("_path", flags);
var dataField = typeof(GameManager).GetField("_gameData", flags);
var scene = UnityEngine.Object.FindAnyObjectByType<UI_GameScene>();
var transitionField = typeof(UI_GameScene).GetField("_isAreaTransitioning", flags);
var groupField = typeof(UI_GameScene).GetField("_currentBtnGroup", flags);
var eventSystem = UnityEngine.EventSystems.EventSystem.current;
if (!UnityEditor.EditorApplication.isPlaying || scene == null || game.Player == null || game.Player.InBattle
    || pathField.GetValue(game) != null || Managers.UI.GetPopupCount() != 0
    || (bool)transitionField.GetValue(scene) || !(groupField.GetValue(scene) is UI_Village1BtnGroup)
    || eventSystem == null || !eventSystem.enabled)
    throw new System.Exception("Isolated idle town session with empty popup stack required");
void Check(bool pass, string message)
{
    if (!pass) throw new System.Exception("VillageTweenLifetime: " + message);
}
System.Collections.IEnumerator Run()
{
    var originalData = (GameData)dataField.GetValue(game);
    var warnings = new System.Collections.Generic.List<string>();
    UnityEngine.Application.LogCallback onLog = (message, stack, type) =>
    {
        if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception
            || (type == UnityEngine.LogType.Warning
                && (message.IndexOf("DOTWEEN", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || message.Contains("destroyed") || message.Contains("MissingReferenceException"))))
            warnings.Add(message + "\n" + stack);
    };
    var outside = new UnityEngine.GameObject("VillageLifetimeOutside");
    var outsideTween = DG.Tweening.ShortcutExtensions.DOScale(outside.transform, 2f, 60f);
    UnityEngine.Application.logMessageReceived += onLog;
    int staleCompletions = 0;
    UI_Village1BtnGroup unlockOwner = null;
    try
    {
        dataField.SetValue(game, Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(originalData)));
        typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
        game.LastVillage1Group = Define.Village1Group.Main;
        var onMain = typeof(UI_Village1BtnGroup).GetMethod("OnClickMain", flags);
        var pendingUnlock = typeof(UI_Village1BtnGroup).GetField("_pendingUnlockBtnId", flags);
        var playPending = typeof(UI_Village1BtnGroup).GetMethod("TryPlayPendingUnlock", flags);
        for (int cycle = 0; cycle < 3; cycle++)
        {
            var oldGroup = (UI_Village1BtnGroup)groupField.GetValue(scene);
            var oldTargets = oldGroup.GetComponentsInChildren<UnityEngine.Component>(true).ToArray();
            int mode = cycle;
            Check(scene.EnterAreaWithFade(Define.AreaType.Village1, () =>
            {
                // OnClickMain의 1초 이후 버튼 등장/RefreshNotify와 장소명 종료를 파괴 뒤까지 예약한다.
                onMain.Invoke(oldGroup, null);
                if (mode == 1)
                {
                    // 지연 해금도 이전 그룹과 함께 취소되어야 한다.
                    pendingUnlock.SetValue(oldGroup, Define.UNLOCK_TRAVEL_BUTTON);
                    playPending.Invoke(oldGroup, null);
                }
                if (mode == 2)
                {
                    var button = oldGroup.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == "GuildButton");
                    var icon = oldGroup.GetComponentsInChildren<UnityEngine.UI.Image>(true).First(i => i.name == "GuildButtonIcon");
                    var label = oldGroup.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == "GuildButtonText");
                    oldGroup.UnlockButtonAnim(button.gameObject, icon, label, () => staleCompletions++);
                    Check(!eventSystem.enabled, "Unlock animation did not lock input");
                }
            }), "Town replacement rejected");
            yield return new UnityEngine.WaitForSecondsRealtime(4f);
            Check(oldGroup == null, "Old group was not destroyed");
            Check(!(bool)transitionField.GetValue(scene), "Transition did not finish");
            Check(groupField.GetValue(scene) is UI_Village1BtnGroup, "Replacement group missing");
            Check(staleCompletions == 0, "Destroyed group's unlock completion ran");
            Check(eventSystem.enabled, "Input stayed locked after group destruction");
            Check(UnityEngine.GameObject.Find("UnlockInputBlocker") == null, "Unlock blocker survived group destruction");
            foreach (var target in oldTargets)
                Check(!DG.Tweening.DOTween.IsTweening(target), "Destroyed component still owns a tween");
            Check(DG.Tweening.TweenExtensions.IsActive(outsideTween), "Unrelated tween was killed");
            Check(warnings.Count == 0, string.Join("\n", warnings));
        }
        // 교체된 그룹의 정상 등장도 완료되어 여섯 버튼을 다시 사용할 수 있어야 한다.
        var current = (UI_Village1BtnGroup)groupField.GetValue(scene);
        onMain.Invoke(current, null);
        yield return new UnityEngine.WaitForSecondsRealtime(2f);
        foreach (string name in new[] { "GuildButton", "AlchemyButton", "MonasteryButton", "ForgeButton", "ShopButton", "TravelButton" })
            Check(current.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == name).gameObject.activeInHierarchy, "Normal entrance missing: " + name);

        unlockOwner = current;
        var unlockButton = current.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == "GuildButton");
        var unlockIcon = current.GetComponentsInChildren<UnityEngine.UI.Image>(true).First(i => i.name == "GuildButtonIcon");
        var unlockLabel = current.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == "GuildButtonText");
        int normalCompletions = 0;
        current.UnlockButtonAnim(unlockButton.gameObject, unlockIcon, unlockLabel, () => normalCompletions++);
        Check(!eventSystem.enabled, "Normal unlock did not lock input");
        float deadline = UnityEngine.Time.realtimeSinceStartup + 10f;
        while (normalCompletions == 0 && UnityEngine.Time.realtimeSinceStartup < deadline) yield return null;
        yield return null; // 완료 프레임의 autoKill과 blocker Destroy까지 반영한다.
        Check(normalCompletions == 1, "Normal unlock did not complete exactly once");
        Check(eventSystem.enabled && unlockButton.interactable, "Normal unlock did not restore input/button");
        Check(UnityEngine.GameObject.Find("UnlockInputBlocker") == null, "Normal unlock blocker survived");

        // 첫 완료 콜백에서 새 해금을 시작한다. 첫 Sequence의 뒤따르는 autoKill이 새 잠금을 풀면 실패한다.
        int chainCompletions = 0;
        current.UnlockButtonAnim(unlockButton.gameObject, unlockIcon, unlockLabel, () =>
        {
            chainCompletions++;
            current.UnlockButtonAnim(unlockButton.gameObject, unlockIcon, unlockLabel, () => chainCompletions++);
        });
        deadline = UnityEngine.Time.realtimeSinceStartup + 10f;
        while (chainCompletions == 0 && UnityEngine.Time.realtimeSinceStartup < deadline) yield return null;
        yield return null;
        Check(chainCompletions == 1, "Chained unlock did not start after first completion");
        Check(!eventSystem.enabled, "First sequence autoKill released the next unlock's input lock");
        Check(UnityEngine.GameObject.Find("UnlockInputBlocker") != null, "Chained unlock blocker missing");
        deadline = UnityEngine.Time.realtimeSinceStartup + 10f;
        while (chainCompletions < 2 && UnityEngine.Time.realtimeSinceStartup < deadline) yield return null;
        yield return null;
        Check(chainCompletions == 2, "Chained unlock completion count incorrect");
        Check(eventSystem.enabled && unlockButton.interactable, "Chained unlock did not restore input/button");
        Check(UnityEngine.GameObject.Find("UnlockInputBlocker") == null, "Chained unlock blocker survived");
        Check(DG.Tweening.TweenExtensions.IsActive(outsideTween), "Unrelated tween was killed during normal/chained unlock");
        Check(warnings.Count == 0, string.Join("\n", warnings));
        UnityEngine.Debug.Log("[VillageTweenLifetime] PASS: 3 actual town replacements, normal entrance/unlock and chained unlock; callbacks/tweens cancelled, input restored, unrelated tween alive");
    }
    finally
    {
        UnityEngine.Application.logMessageReceived -= onLog;
        if (unlockOwner != null) DG.Tweening.DOTween.Kill(unlockOwner, false);
        dataField.SetValue(game, originalData);
        pathField.SetValue(game, null);
        game.Player.RefreshStats(fullHeal: false);
        if (DG.Tweening.TweenExtensions.IsActive(outsideTween)) DG.Tweening.TweenExtensions.Kill(outsideTween, false);
        Managers.Resource.Destroy(outside);
    }
}
CoroutineManager.StartCoroutine(Run());
return "Village tween lifetime verification started; GameData is restored afterwards and save path stays null";
