// verify_bestiary_layout.cs의 변경 전 기준 파일과 전체 스크롤을 비교한다.
var game = Managers.Game;
if (!UnityEngine.Application.isPlaying || game.Player == null || game.Player.InBattle || Managers.UI.GetPopupCount() != 0)
    throw new System.Exception("BestiaryVisible requires town with no popups");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
string baselinePath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../_tools/fixtures/bestiary-layout-20260910.json");
var baseline = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(baselinePath));
var regions = (Newtonsoft.Json.Linq.JArray)baseline["regions"];
var expectedIds = regions.SelectMany(r => r["boxes"]).Select(b => (int)b["id"]).OrderBy(id => id).ToArray();
System.Collections.IEnumerator Run()
{
    var original = dataField.GetValue(game);
    var originalPath = pathField.GetValue(game);
    var originalLanguage = game.Language;
    float originalHp = game.Player.Hp;
    var failures = new System.Collections.Generic.List<string>();
    var errors = new System.Collections.Generic.List<string>();
    var visited = new System.Collections.Generic.HashSet<int>();
    var measurements = new System.Collections.Generic.List<object>();
    int maxLive = 0;
    void Check(bool pass, string message) { if (!pass) failures.Add(message); }
    UnityEngine.Application.LogCallback onLog = (message, stack, type) =>
    {
        if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception || type == UnityEngine.LogType.Warning) errors.Add(message);
    };
    UI_BestiaryPopup popup = null;
    var external = DG.Tweening.TweenSettingsExtensions.SetLoops(DG.Tweening.DOVirtual.Float(0f, 1f, 100f, value => {}), -1);
    try
    {
        pathField.SetValue(game, null);
        dataField.SetValue(game, Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(original)));
        typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
        UnityEngine.Application.logMessageReceived += onLog;
        for (int round = 0; round < 3; round++)
        {
            var gc = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory, "GC Allocated In Frame", 8);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            popup = Managers.UI.ShowPopupUI<UI_BestiaryPopup>();
            popup.SetInfo();
            double createMs = watch.Elapsed.TotalMilliseconds;
            yield return new UnityEngine.WaitForEndOfFrame();
            double firstFrameMs = watch.Elapsed.TotalMilliseconds;
            yield return null;
            long gcPeak = gc.Count > 0 ? Enumerable.Range(0, gc.Count).Max(i => gc.GetSample(i).Value) : -1;
            gc.Dispose();
            var scroll = popup.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true).Single();
            var sections = scroll.content.GetComponentsInChildren<UI_Bestiary>(true);
            Check(sections.Length == regions.Count, "Region count changed");
            Check(Math.Abs(scroll.content.rect.height - (float)baseline["contentHeight"]) < 0.1f, "Initial scroll height changed: " + scroll.content.rect.height);
            int initialLive = popup.GetComponentsInChildren<UI_BestiaryBox>().Count(b => b.DataId != 0);
            Check(initialLive > 0 && initialLive <= 80, "Initial live boxes outside budget: " + initialLive);
            measurements.Add(new { round, createMs, firstFrameMs, gcPeak, initialLive });
            if (round == 1)
            {
                popup.SetInfo();
                popup.SetInfo();
                yield return null;
                yield return new UnityEngine.WaitForEndOfFrame();
                Check(scroll.content.GetComponentsInChildren<UI_Bestiary>().Length == regions.Count, "Repeated SetInfo duplicated regions");
                var refreshed = popup.GetComponentsInChildren<UI_BestiaryBox>().Where(b => b.DataId != 0).ToArray();
                Check(refreshed.Length > 0 && refreshed.Length <= 80 && refreshed.Select(b => b.DataId).Distinct().Count() == refreshed.Length, "Repeated SetInfo duplicated boxes");
            }
            if (round == 0)
            {
                // 모든 지역의 시작 위치와 마지막 끝까지 이동한 뒤 역순으로 돌아온다.
                var targets = Enumerable.Range(0, regions.Count).Concat(new[] { regions.Count - 1 }).Concat(Enumerable.Range(0, regions.Count).Reverse());
                foreach (int index in targets)
                {
                    float offset = -(float)regions[index]["y"];
                    scroll.StopMovement();
                    scroll.verticalNormalizedPosition = 1f - UnityEngine.Mathf.Clamp01(offset / (scroll.content.rect.height - scroll.viewport.rect.height));
                    yield return null;
                    yield return new UnityEngine.WaitForEndOfFrame();
                    yield return null;
                    UnityEngine.Canvas.ForceUpdateCanvases();
                    var live = popup.GetComponentsInChildren<UI_BestiaryBox>().Where(b => b.DataId != 0).ToArray();
                    maxLive = Math.Max(maxLive, live.Length);
                    Check(live.Length > 0 && live.Length <= 80, "Live box budget at region " + index + ": " + live.Length);
                    Check(live.Select(b => b.DataId).Distinct().Count() == live.Length, "Duplicate live DataId");
                    foreach (var box in live) visited.Add(box.DataId);
                    Check(Math.Abs(scroll.content.rect.height - (float)baseline["contentHeight"]) < 0.1f, "Scroll height changed at " + index);
                    var section = sections[index];
                    var rect = (UnityEngine.RectTransform)section.transform;
                    Check(Math.Abs(rect.anchoredPosition.y - (float)regions[index]["y"]) < 0.1f, "Section position changed: " + index);
                    Check(Math.Abs(rect.rect.height - (float)regions[index]["height"]) < 0.1f, "Section height changed: " + index);
                    var actualBoxes = section.GetComponentsInChildren<UI_BestiaryBox>().Where(b => b.DataId != 0).ToDictionary(b => b.DataId);
                    foreach (var expected in regions[index]["boxes"])
                    {
                        int id = (int)expected["id"];
                        if (!actualBoxes.TryGetValue(id, out var box)) { failures.Add("Missing box in visible region: " + id); continue; }
                        var boxRect = (UnityEngine.RectTransform)box.transform;
                        Check(Math.Abs(boxRect.anchoredPosition.x - (float)expected["x"]) < 0.1f && Math.Abs(boxRect.anchoredPosition.y - (float)expected["y"]) < 0.1f, "Box placement changed: " + id);
                    }
                }
                Check(visited.OrderBy(id => id).SequenceEqual(expectedIds), "Full traversal lost creatures: " + visited.Count);
                // 끝에서 처음으로 돌아온 뒤 발견한 몬스터의 실제 클릭이 상세 창으로 이어지는지 확인한다.
                var discovered = popup.GetComponentsInChildren<UI_BestiaryBox>().First(b => b.DataId != 0 && UI_BestiaryPopup.GetRecordRate(b.DataId) > 0);
                int clickedId = discovered.DataId;
                var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                UnityEngine.EventSystems.ExecuteEvents.Execute(discovered.gameObject, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
                var detail = UnityEngine.Object.FindAnyObjectByType<UI_MonsterDetailPopup>();
                Check(detail != null && Managers.UI.GetPopupCount() == 2, "Visible discovered box did not open details: " + clickedId);
                if (detail != null) Managers.UI.ClosePopupUI(detail);
                yield return new UnityEngine.WaitForSecondsRealtime(0.5f);
                int companionId = game.CompanionMinMonId;
                if (companionId > 0 && regions[0]["boxes"].Any(b => (int)b["id"] == companionId))
                {
                    scroll.verticalNormalizedPosition = 0f;
                    yield return null;
                    yield return null;
                    Managers.MinMon.RemoveCompanion();
                    scroll.verticalNormalizedPosition = 1f;
                    yield return null;
                    yield return new UnityEngine.WaitForEndOfFrame();
                    var companionBox = popup.GetComponentsInChildren<UI_BestiaryBox>().Single(b => b.DataId == companionId);
                    var frame = companionBox.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i => i.name == "BestiaryBoxFrame");
                    Check(frame.color != Util.HexToColor("FF00CB"), "Recreated box retained removed companion marker");
                    Managers.MinMon.SetCompanion(companionId);
                    yield return null;
                    Check(frame.color == Util.HexToColor("FF00CB"), "Visible companion marker did not refresh");
                }
                else failures.Add("Companion refresh case requires a companion from the first region");
                var definitions = (System.Collections.Generic.List<System.ValueTuple<string, int[]>>)typeof(UI_BestiaryPopup).GetMethod("BuildRegionSections", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
                foreach (var language in new[] { Define.Language.English, Define.Language.Korean })
                {
                    game.Language = language;
                    yield return null;
                    for (int i = 0; i < sections.Length; i++)
                    {
                        string title = sections[i].GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "BestiaryAreaText").text;
                        Check(title == Util.Localize(definitions[i].Item1), "Region name stale after language change: " + i);
                    }
                }
                game.Language = originalLanguage;
            }
            Managers.UI.ClosePopupUI(popup);
            popup = null;
            yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
            Check(UnityEngine.Object.FindObjectsByType<UI_BestiaryBox>(UnityEngine.FindObjectsSortMode.None).Length == 0, "Live boxes remain after closing");
            Check(DG.Tweening.TweenExtensions.IsActive(external), "Unrelated tween killed");
        }
        popup = Managers.UI.ShowPopupUI<UI_BestiaryPopup>();
        popup.SetInfo();
        Managers.UI.ClosePopupUI(popup);
        popup = null;
        yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
        Check(UnityEngine.Object.FindObjectsByType<UI_BestiaryBox>(UnityEngine.FindObjectsSortMode.None).Length == 0 && Managers.UI.GetPopupCount() == 0, "Immediate close left live boxes or popup");
    }
    finally
    {
        UnityEngine.Application.logMessageReceived -= onLog;
        if (popup != null) Managers.UI.CloseAllPopupUI();
        DG.Tweening.TweenExtensions.Kill(external);
        dataField.SetValue(game, original);
        game.Language = originalLanguage;
        typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
        game.Player.RefreshCompanion();
        game.Player.RefreshStats(false);
        game.Player.Hp = originalHp;
        pathField.SetValue(game, originalPath);
    }
    UnityEngine.Debug.Log("[BestiaryVisible] " + Newtonsoft.Json.JsonConvert.SerializeObject(new { measurements, maxLive, visited = visited.Count, failures, errors }));
    if (failures.Count > 0 || errors.Count > 0) throw new System.Exception("BestiaryVisible FAIL: " + failures.Count + " failures, " + errors.Count + " errors/warnings");
    UnityEngine.Debug.Log("[BestiaryVisible] PASS: full forward/back traversal, original geometry, bounded live boxes, click and cleanup");
}
CoroutineManager.StartCoroutine(Run());
return "BestiaryVisible started; wait for final log";
