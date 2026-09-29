// Play 중 마을, 팝업 없음. 첫 생성과 반복 생성 비용을 같은 저장에서 비교한다.
var game = Managers.Game;
if (!UnityEngine.Application.isPlaying || game.Player == null || game.Player.InBattle || Managers.UI.GetPopupCount() != 0)
    throw new System.Exception("BestiaryCost requires a town player with no popups");
if (!Managers.Data.CreatureDic.Keys.Any(id => UI_BestiaryPopup.GetRecordRate(id) > 0)
    || !Managers.Data.CreatureDic.Keys.Any(id => UI_BestiaryPopup.GetRecordRate(id) == 0)
    || !Managers.Data.CreatureDic.ContainsKey(game.CompanionMinMonId))
    throw new System.Exception("BestiaryCost requires discovered and unknown creatures plus a summoned companion");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
System.Collections.IEnumerator Run()
{
    var original = (GameData)dataField.GetValue(game);
    var originalPath = pathField.GetValue(game);
    float originalHp = game.Player.Hp;
    var failures = new System.Collections.Generic.List<string>();
    var warnings = new System.Collections.Generic.List<string>();
    var errors = new System.Collections.Generic.List<string>();
    var results = new System.Collections.Generic.List<object>();
    var tweenManager = typeof(DG.Tweening.DOTween).Assembly.GetType("DG.Tweening.Core.TweenManager");
    var capacityFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
    int tweenerCapacity = (int)tweenManager.GetField("maxTweeners", capacityFlags).GetValue(null);
    int sequenceCapacity = (int)tweenManager.GetField("maxSequences", capacityFlags).GetValue(null);
    if (tweenerCapacity != 7812 || sequenceCapacity != 625) failures.Add("Startup capacity was not configured: " + tweenerCapacity + "/" + sequenceCapacity);
    UnityEngine.Application.LogCallback onLog = (message, stack, type) =>
    {
        if (type == UnityEngine.LogType.Warning) warnings.Add(message);
        if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception) errors.Add(message);
    };
    UI_BestiaryPopup popup = null;
    var external = DG.Tweening.TweenSettingsExtensions.SetLoops(DG.Tweening.DOVirtual.Float(0f, 1f, 100f, value => {}), -1);
    try
    {
        pathField.SetValue(game, null);
        dataField.SetValue(game, Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(original)));
        UnityEngine.Application.logMessageReceived += onLog;
        for (int iteration = 0; iteration < 3; iteration++)
        {
            int before = DG.Tweening.DOTween.TotalActiveTweens();
            var gcRecorder = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory, "GC Allocated In Frame", 8);
            long allocatedBefore = System.GC.GetAllocatedBytesForCurrentThread();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            popup = Managers.UI.ShowPopupUI<UI_BestiaryPopup>();
            popup.SetInfo();
            double createMs = watch.Elapsed.TotalMilliseconds;
            long createBytes = System.GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            yield return new UnityEngine.WaitForEndOfFrame();
            double firstFrameMs = watch.Elapsed.TotalMilliseconds;
            yield return null;
            long gcPeakBytes = gcRecorder.Count > 0 ? Enumerable.Range(0, gcRecorder.Count).Max(i => gcRecorder.GetSample(i).Value) : -1;
            bool gcCounterValid = gcRecorder.Valid;
            gcRecorder.Dispose();
            var boxes = popup.GetComponentsInChildren<UI_BestiaryBox>(true).Where(b => b.DataId != 0).ToArray();
            var authored = boxes.SelectMany(b => b.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true)).ToArray();
            int activeAuthored = authored.Count(a => a.tween != null && DG.Tweening.TweenExtensions.IsActive(a.tween));
            int active = DG.Tweening.DOTween.TotalActiveTweens();
            if (boxes.Length == 0) failures.Add("No visible bestiary boxes");
            if (activeAuthored != 0) failures.Add("Unused authored tweens remain: " + activeAuthored);
            if (iteration == 2)
            {
                var discovered = boxes.First(b => UI_BestiaryPopup.GetRecordRate(b.DataId) > 0);
                var unknown = boxes.First(b => UI_BestiaryPopup.GetRecordRate(b.DataId) == 0);
                int clicks = 0;
                discovered.SetInfo(Managers.Data.CreatureDic[discovered.DataId], id => clicks++);
                unknown.SetInfo(Managers.Data.CreatureDic[unknown.DataId], id => clicks++);
                var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                UnityEngine.EventSystems.ExecuteEvents.Execute(unknown.gameObject, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                if (clicks != 0) failures.Add("Unknown box accepted a click");
                UnityEngine.EventSystems.ExecuteEvents.Execute(discovered.gameObject, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                if (clicks != 1) failures.Add("Discovered box did not accept exactly one click");
                if (!DG.Tweening.DOTween.IsTweening(discovered.transform)) failures.Add("Discovered click punch is missing");
                var applyStar = typeof(UI_BestiaryBox).GetMethod("ApplyStarVisual", flags);
                var starTween = typeof(UI_BestiaryBox).GetField("_starIdleTween", flags);
                var star = discovered.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i => i.name == "BestiaryBoxStar");
                foreach (int grade in new[] { 1, 2, 3 })
                {
                    applyStar.Invoke(discovered, new object[] { grade });
                    var expected = grade == 1 ? UnityEngine.Color.white : grade == 2 ? new UnityEngine.Color(1f, 1f, 0f, 1f) : UnityEngine.Color.magenta;
                    if (!star.gameObject.activeSelf || star.color != expected) failures.Add("Wrong star color/visibility at grade " + grade);
                    if (!DG.Tweening.TweenExtensions.IsActive((DG.Tweening.Tween)starTween.GetValue(discovered))) failures.Add("Missing star idle at grade " + grade);
                    yield return new UnityEngine.WaitForSecondsRealtime(0.2f);
                    if (UnityEngine.Quaternion.Angle(star.transform.localRotation, UnityEngine.Quaternion.identity) < 0.01f) failures.Add("Star did not animate at grade " + grade);
                }
                applyStar.Invoke(discovered, new object[] { 0 });
                if (star.gameObject.activeSelf || starTween.GetValue(discovered) != null) failures.Add("Star idle remains after hiding");
                var companion = boxes.First(b => b.DataId == game.CompanionMinMonId);
                var frame = companion.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i => i.name == "BestiaryBoxFrame");
                if (frame.color != Util.HexToColor("FF00CB")) failures.Add("Companion frame was lost");
            }
            yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
            Managers.UI.ClosePopupUI(popup);
            popup = null;
            yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
            int after = DG.Tweening.DOTween.TotalActiveTweens();
            if (!DG.Tweening.TweenExtensions.IsActive(external)) failures.Add("Unrelated tween was killed");
            results.Add(new { iteration, boxes=boxes.Length, authored=authored.Length, activeAuthored, before, active, after, createMs, createBytes, gcCounterValid, gcPeakBytes, firstFrameMs });
        }
    }
    finally
    {
        UnityEngine.Application.logMessageReceived -= onLog;
        if (popup != null) Managers.UI.ClosePopupUI(popup);
        DG.Tweening.TweenExtensions.Kill(external);
        dataField.SetValue(game, original);
        game.Player.RefreshStats(false);
        game.Player.Hp = originalHp;
        pathField.SetValue(game, originalPath);
    }
    UnityEngine.Debug.Log("[BestiaryCost] " + Newtonsoft.Json.JsonConvert.SerializeObject(new { tweenerCapacity, sequenceCapacity, results, warnings, errors, failures }));
    if (errors.Count != 0 || failures.Count != 0) throw new System.Exception("[BestiaryCost] FAIL: " + string.Join(" | ", failures.Concat(errors)));
    UnityEngine.Debug.Log("[BestiaryCost] PASS: visible boxes, zero unused authored tweens, unrelated tween preserved");
}
CoroutineManager.StartCoroutine(Run());
return "BestiaryCost started; wait for its final log before any further UI input";
