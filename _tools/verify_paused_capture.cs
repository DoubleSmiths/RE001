// 전투 정지 중 실제 포획 성공/실패 연출과 중첩 팝업을 검사한다. 결과만 DebugForceCatch로 고정한다.
// 새 Play의 마을에서 실행하며, 재화/포획 변경은 복제 GameData에만 남긴다.
var game = Managers.Game;
var player = game.Player;
var ui = Managers.UI;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
if (!UnityEngine.Application.isPlaying || player == null || player.InBattle || ui.GetPopupCount() != 0 || ui.IsActiveSoulShop)
    throw new System.Exception("PausedCapture requires fresh town with no popups");
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
var battleField = typeof(PlayerController).GetField("_inBattle", flags);
var areaField = typeof(GameManager).GetField("_inBattleArea", flags);
var busyField = typeof(UI_TamePopup).GetField("_busy", flags);
var failures = new System.Collections.Generic.List<string>();
var errors = new System.Collections.Generic.List<string>();
void Check(bool pass, string message) { if (!pass) failures.Add(message); }
bool Click(UnityEngine.GameObject target, UnityEngine.GameObject expectedReceiver = null)
{
    var rect = target.GetComponent<UnityEngine.RectTransform>();
    var canvas = target.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
    var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    var point = UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
    var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point };
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
    var receiver = expectedReceiver != null ? expectedReceiver : target;
    if (hits.Count == 0 || !hits[0].gameObject.transform.IsChildOf(receiver.transform))
    {
        UnityEngine.Debug.Log("[PausedCapture] Click mismatch: " + target.name + " receiver=" + receiver.name + " top=" + (hits.Count > 0 ? hits[0].gameObject.name : "none"));
        return false;
    }
    return UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler) != null;
}
void CheckAnimations(UI_Popup popup)
{
    Check(popup.GetComponentsInChildren<UnityEngine.Animator>(true).All(a => a.updateMode == UnityEngine.AnimatorUpdateMode.UnscaledTime), popup.name + ": scaled Animator");
    Check(popup.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true).All(a => !a.autoGenerate || a.isIndependentUpdate), popup.name + ": scaled authored Tween setting");
}
System.Collections.IEnumerator Run()
{
    var originalData = dataField.GetValue(game);
    string originalJson = Newtonsoft.Json.JsonConvert.SerializeObject(originalData);
    var originalPath = pathField.GetValue(game);
    var originalArea = areaField.GetValue(game);
    float originalScale = UnityEngine.Time.timeScale;
    float originalHp = player.Hp;
    int originalDebug = UI_TamePopup.DebugForceCatch;
    var originalRandom = UnityEngine.Random.state;
    var minionFields = typeof(MinMonManager).GetFields(flags).Where(f => !f.IsInitOnly && !f.IsLiteral && (f.FieldType.IsPrimitive || f.FieldType.IsEnum)).ToDictionary(f => f, f => f.GetValue(Managers.MinMon));
    UnityEngine.Application.LogCallback onLog = (message, stack, type) =>
    {
        if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception || type == UnityEngine.LogType.Warning) errors.Add(message);
    };
    try
    {
        pathField.SetValue(game, null);
        areaField.SetValue(game, true);
        UnityEngine.Application.logMessageReceived += onLog;
        foreach (int outcome in new[] { 0, 1, 2 })
        {
            bool useMandrake = outcome == 2;
            bool success = outcome != 0;
            var isolated = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(originalJson);
            dataField.SetValue(game, isolated);
            if (useMandrake) game.MandrakeCount = 2;
            int mandrakeBefore = game.MandrakeCount;
            typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
            game.InterestedMinMons.Clear();
            game.InterestedMinMons.Add(2002);
            int caughtBefore = Managers.MinMon.GetCaughtCount(2002);
            int tameBefore = Managers.MinMon.GetTameCount(2002);
            battleField.SetValue(player, true);
            UnityEngine.Time.timeScale = 1f;
            var bestiary = ui.ShowPopupUI<UI_BestiaryPopup>();
            bestiary.SetInfo();
            Check(ui.IsBattlePaused && UnityEngine.Time.timeScale == 0f, "Bestiary did not pause battle");
            CheckAnimations(bestiary);
            yield return new UnityEngine.WaitForSecondsRealtime(1.2f);
            var catchButton = bestiary.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "MinimonBtn");
            if (!Click(catchButton.gameObject)) throw new System.Exception("PausedCapture catch button failed top raycast");
            yield return null;
            var tame = UnityEngine.Object.FindAnyObjectByType<UI_TamePopup>();
            if (tame == null) throw new System.Exception("PausedCapture Tame did not open");
            CheckAnimations(tame);
            var lure = tame.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "LureBtn");
            var mandrake = tame.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "UseMandrakeBtn");
            var minionInfoButton = tame.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "MinimonInfoBtn");
            var tamer = tame.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i => i.name == "TamerImg");
            var tamerEmoj = tame.GetComponentsInChildren<UnityEngine.Animator>(true).Single(a => a.name == "TamerEmoj");
            var monsterEmoj = tame.GetComponentsInChildren<UnityEngine.Animator>(true).Single(a => a.name == "MonsterEmoj");
            var lureAnimation = tamer.GetComponents<DG.Tweening.DOTweenAnimation>().Single(a => a.id == "Tamer Lure");
            var caughtAnimation = tame.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true).Single(a => a.id == "Monster Caught");
            float setupStart = tamer.rectTransform.anchoredPosition.x;
            float setupDistance = 0f;
            float lureShown = -1f, mandrakeShown = -1f, infoShown = -1f;
            float deadline = UnityEngine.Time.realtimeSinceStartup + 25f;
            while ((bool)busyField.GetValue(tame) && UnityEngine.Time.realtimeSinceStartup < deadline)
            {
                setupDistance = Math.Max(setupDistance, Math.Abs(tamer.rectTransform.anchoredPosition.x - setupStart));
                if (lure.gameObject.activeInHierarchy && lureShown < 0f) lureShown = UnityEngine.Time.realtimeSinceStartup;
                if (mandrake.gameObject.activeInHierarchy && mandrakeShown < 0f) mandrakeShown = UnityEngine.Time.realtimeSinceStartup;
                if (minionInfoButton.gameObject.activeInHierarchy && infoShown < 0f) infoShown = UnityEngine.Time.realtimeSinceStartup;
                Check(ui.IsBattlePaused && UnityEngine.Time.timeScale == 0f, "Pause lost during setup");
                yield return null;
            }
            Check(!(bool)busyField.GetValue(tame) && setupDistance > 1f, "Tamer setup did not complete/move while paused");
            if (infoShown < 0f && minionInfoButton.gameObject.activeInHierarchy) infoShown = UnityEngine.Time.realtimeSinceStartup;
            Check(lureShown > 0f && mandrakeShown - lureShown >= 0.4f && infoShown - mandrakeShown >= 0.4f, "Capture buttons did not appear sequentially while paused");
            UI_TamePopup.DebugForceCatch = useMandrake ? 0 : success ? 1 : 2;
            if (!Click(useMandrake ? mandrake.gameObject : lure.gameObject)) throw new System.Exception("PausedCapture action button failed top raycast");
            float minLureY = float.PositiveInfinity, maxLureY = float.NegativeInfinity;
            float minCaughtY = float.PositiveInfinity, maxCaughtY = float.NegativeInfinity;
            float caughtRestY = caughtAnimation.target.transform.localPosition.y;
            bool caughtReturned = false;
            float lureEmojProgress = 0f, caughtProgress = 0f, heartsProgress = 0f;
            deadline = UnityEngine.Time.realtimeSinceStartup + 60f;
            while (tame != null && UnityEngine.Time.realtimeSinceStartup < deadline)
            {
                Check(ui.IsBattlePaused && UnityEngine.Time.timeScale == 0f, "Pause lost during capture result");
                if (lureAnimation != null && lureAnimation.tween != null && DG.Tweening.TweenExtensions.IsPlaying(lureAnimation.tween))
                {
                    float y = tamer.rectTransform.anchoredPosition.y;
                    minLureY = Math.Min(minLureY, y);
                    maxLureY = Math.Max(maxLureY, y);
                }
                if (tamerEmoj != null && tamerEmoj.gameObject.activeInHierarchy)
                {
                    var state = tamerEmoj.GetCurrentAnimatorStateInfo(0);
                    if (state.IsName("Emoj_Lure")) lureEmojProgress = Math.Max(lureEmojProgress, state.normalizedTime);
                }
                if (monsterEmoj != null && monsterEmoj.gameObject.activeInHierarchy)
                {
                    var state = monsterEmoj.GetCurrentAnimatorStateInfo(0);
                    if (state.IsName("Emoj_TwoHearts")) heartsProgress = Math.Max(heartsProgress, state.normalizedTime);
                }
                if (caughtAnimation != null && caughtAnimation.tween != null && DG.Tweening.TweenExtensions.IsActive(caughtAnimation.tween))
                {
                    caughtProgress = Math.Max(caughtProgress, DG.Tweening.TweenExtensions.Elapsed(caughtAnimation.tween));
                    float y = caughtAnimation.target.transform.localPosition.y;
                    if (DG.Tweening.TweenExtensions.IsPlaying(caughtAnimation.tween))
                    {
                        minCaughtY = Math.Min(minCaughtY, y);
                        maxCaughtY = Math.Max(maxCaughtY, y);
                    }
                    if (DG.Tweening.TweenExtensions.IsComplete(caughtAnimation.tween)) caughtReturned = Math.Abs(y - caughtRestY) < 0.05f;
                }
                yield return null;
            }
            Check(tame == null, "Capture did not finish before deadline");
            if (!useMandrake)
            {
                Check(maxLureY - minLureY > 0.5f, "Authored Tamer Lure did not move while paused");
                Check(lureEmojProgress > 0.5f, "Lure Animator did not progress while paused");
            }
            Check(game.MandrakeCount == mandrakeBefore - (useMandrake ? 1 : 0), "Mandrake was not consumed exactly once");
            Check(Managers.MinMon.GetCaughtCount(2002) == caughtBefore + (success ? 1 : 0), "Capture result counted incorrectly");
            Check(Managers.MinMon.GetTameCount(2002) == (success ? 0 : tameBefore + 1), "Pity count was not updated exactly once");
            Check(UI_TamePopup.DebugForceCatch == 0, "Forced outcome hook was not consumed");
            Check(game.InterestedMinMons.Count == 0, "Capture interest was not consumed exactly once");
            var info = UnityEngine.Object.FindAnyObjectByType<UI_MinionInfoPopup>();
            if (success)
            {
                Check(info != null && ui.GetPopupCount() == 2, "Success did not replace Tame with MinionInfo");
                Check(caughtProgress > 0.5f && heartsProgress > 0.5f, "Caught Tween/Hearts Animator did not progress");
                Check(maxCaughtY - minCaughtY > 0.5f && caughtReturned, "Caught motion did not move and return to rest");
                if (info != null)
                {
                    // 창 입장(0.5초)을 마친 뒤, 아직 결과 연출 잠금(3초)이 남은 시점에 닫기를 시도한다.
                    yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
                    CheckAnimations(info);
                    var resultPlaying = typeof(UI_MinionInfoPopup).GetField("_resultAnimPlaying", flags);
                    var buttons = info.GetComponentsInChildren<UnityEngine.RectTransform>(true).Single(r => r.name == "MinionInfoPopupButtonGroup");
                    var close = info.GetComponentsInChildren<UnityEngine.RectTransform>(true).Single(r => r.name == "Xbutton");
                    Check((bool)resultPlaying.GetValue(info) && !buttons.gameObject.activeSelf, "Result controls unlocked too early");
                    // X 이미지는 raycastTarget=false이며 같은 닫기 가드가 걸린 전체 배경이 이 위치의 실제 입력을 받는다.
                    var closeReceiver = info.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "MinionInfoPopupBackground");
                    Check(Click(close.gameObject, closeReceiver.gameObject), "Result close button failed top raycast");
                    Check(ui.GetPopupCount() == 2, "Result animation was skipped by premature close");
                }
                yield return new UnityEngine.WaitForSecondsRealtime(3.5f);
                if (info != null)
                {
                    var resultPlaying = typeof(UI_MinionInfoPopup).GetField("_resultAnimPlaying", flags);
                    var buttons = info.GetComponentsInChildren<UnityEngine.RectTransform>(true).Single(r => r.name == "MinionInfoPopupButtonGroup");
                    var sync = info.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "SYNCText");
                    Check(!(bool)resultPlaying.GetValue(info) && buttons.gameObject.activeSelf, "Result controls did not unlock while paused");
                    Check(sync.text.Contains(UnityEngine.Mathf.RoundToInt(Managers.MinMon.GetSyncRate(2002) * 100f).ToString()), "Sync number did not finish counting");
                    Check(Click(info.GetComponentsInChildren<UnityEngine.RectTransform>(true).Single(r => r.name == "Xbutton").gameObject,
                        info.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "MinionInfoPopupBackground").gameObject), "Finished result close failed top raycast");
                    yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
                    Check(info == null && ui.GetPopupCount() == 1, "Finished result did not close to Bestiary");
                }
            }
            else Check(info == null && ui.GetPopupCount() == 1, "Failed capture left result popup");
            Check(ui.IsBattlePaused && UnityEngine.Time.timeScale == 0f, "Underlying Bestiary did not keep battle paused");
            ui.CloseAllPopupUI();
            Check(!ui.IsBattlePaused && UnityEngine.Time.timeScale == 1f, "Closing all capture popups did not resume");
            battleField.SetValue(player, false);
            ui.RefreshTimeScale();
            yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
            UnityEngine.Debug.Log("[PausedCapture] " + Newtonsoft.Json.JsonConvert.SerializeObject(new { success, useMandrake, setupDistance, firstButtonGap = mandrakeShown - lureShown, secondButtonGap = infoShown - mandrakeShown, lureTravel = useMandrake ? 0f : maxLureY - minLureY, lureEmojProgress, caughtProgress, caughtTravel = success ? maxCaughtY - minCaughtY : 0f, caughtReturned, heartsProgress }));
        }
    }
    finally
    {
        UnityEngine.Application.logMessageReceived -= onLog;
        ui.CloseAllPopupUI();
        battleField.SetValue(player, false);
        ui.RefreshTimeScale();
        dataField.SetValue(game, originalData);
        areaField.SetValue(game, originalArea);
        foreach (var pair in minionFields) pair.Key.SetValue(Managers.MinMon, pair.Value);
        typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
        player.RefreshStats(false);
        player.Hp = originalHp;
        UI_TamePopup.DebugForceCatch = originalDebug;
        UnityEngine.Random.state = originalRandom;
        UnityEngine.Time.timeScale = originalScale;
        pathField.SetValue(game, originalPath);
    }
    Check(Newtonsoft.Json.JsonConvert.SerializeObject(dataField.GetValue(game)) == originalJson, "Original GameData was not restored");
    UnityEngine.Debug.Log("[PausedCapture] " + Newtonsoft.Json.JsonConvert.SerializeObject(new { failures, errors }));
    if (failures.Count > 0 || errors.Count > 0) throw new System.Exception("PausedCapture FAIL: " + string.Join(" | ", failures.Concat(errors)));
    UnityEngine.Debug.Log("[PausedCapture] PASS: real capture success/failure while paused, authored motion, nested stack and restoration");
}
CoroutineManager.StartCoroutine(Run());
return "PausedCapture started; wait for both outcomes and final PASS/FAIL before further UI input";
