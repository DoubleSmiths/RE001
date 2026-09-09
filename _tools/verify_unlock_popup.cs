// 마을에서 저장 경로를 격리하고 실제 마을 해금 콜백으로 팝업을 연다.
// python _tools/unity_exec.py _tools/verify_unlock_popup.cs --allow-async
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var game = Managers.Game;
var pathField = typeof(GameManager).GetField("_path", flags);
var dataField = typeof(GameManager).GetField("_gameData", flags);
var scene = UnityEngine.Object.FindAnyObjectByType<UI_GameScene>();
var village = UnityEngine.Object.FindAnyObjectByType<UI_Village1BtnGroup>();
var eventSystem = UnityEngine.EventSystems.EventSystem.current;
if (!UnityEditor.EditorApplication.isPlaying || scene == null || village == null || game.Player == null
    || game.Player.InBattle || Managers.UI.GetPopupCount() != 0
    || eventSystem == null || !eventSystem.enabled
    || (bool)typeof(UI_GameScene).GetField("_isAreaTransitioning", flags).GetValue(scene))
    throw new System.Exception("Isolated idle town with empty popup stack required");
var failures = new System.Collections.Generic.List<string>();
void Check(bool pass, string message)
{
    if (!pass) failures.Add(message);
}
UnityEngine.Rect Bounds(UnityEngine.RectTransform rect)
{
    var canvas = rect.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
    var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    var corners = new UnityEngine.Vector3[4];
    rect.GetWorldCorners(corners);
    var points = corners.Select(p => UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, p)).ToArray();
    return UnityEngine.Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
}
bool Contains(UnityEngine.Rect outer, UnityEngine.Rect inner)
{
    return inner.xMin >= outer.xMin - 1f && inner.xMax <= outer.xMax + 1f
        && inner.yMin >= outer.yMin - 1f && inner.yMax <= outer.yMax + 1f;
}
void CheckBody(UI_UnlockButtonPopup popup, string descKey, string phase)
{
    UnityEngine.Canvas.ForceUpdateCanvases();
    var body = popup.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "TextTMP");
    var content = popup.GetComponentsInChildren<UnityEngine.RectTransform>(true).Single(t => t.name == "Content");
    body.ForceMeshUpdate();
    Check(body.text == Util.Localize(descKey) && body.text != descKey, phase + ": localized body mismatch");
    Check(body.isActiveAndEnabled && body.textInfo.characterCount > 0, phase + ": empty/hidden body");
    var canvas = body.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
    var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    // 타이핑 알파와 무관하게 모든 실제 글리프의 네 꼭짓점을 검사한다.
    var points = new System.Collections.Generic.List<UnityEngine.Vector2>();
    foreach (var character in body.textInfo.characterInfo.Take(body.textInfo.characterCount))
    {
        if (!character.isVisible) continue;
        foreach (var vertex in new[] { character.bottomLeft, character.topLeft, character.topRight, character.bottomRight })
            points.Add(UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, body.transform.TransformPoint(vertex)));
    }
    Check(points.Count > 0, phase + ": no visible glyph geometry");
    if (points.Count == 0) return;
    var glyphs = UnityEngine.Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
    Check(Contains(new UnityEngine.Rect(0f, 0f, UnityEngine.Screen.width, UnityEngine.Screen.height), glyphs), phase + ": glyphs outside viewport " + glyphs);
    Check(Contains(Bounds(content), glyphs), phase + ": glyphs outside Content " + glyphs);
    Check(!body.isTextOverflowing, phase + ": TMP overflow");
}
bool ClickContent(UI_UnlockButtonPopup popup, string phase)
{
    var content = popup.GetComponentsInChildren<UnityEngine.RectTransform>(true).Single(t => t.name == "Content");
    var pointer = new UnityEngine.EventSystems.PointerEventData(eventSystem)
    {
        position = Bounds(content).center,
        button = UnityEngine.EventSystems.PointerEventData.InputButton.Left
    };
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    eventSystem.RaycastAll(pointer, hits);
    var receiver = hits.Count == 0 ? null : UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(hits[0].gameObject);
    Check(receiver == content.gameObject, phase + ": Content is not the top click receiver");
    if (receiver != content.gameObject) return false;
    pointer.pointerCurrentRaycast = hits[0];
    pointer.pointerPressRaycast = hits[0];
    UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
    UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
    UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    return true;
}
void CheckFinishedAlpha(UI_UnlockButtonPopup popup, string phase)
{
    // ForceMeshUpdate로 색을 덮기 전에 스킵/자연 완료가 실제로 남긴 알파를 확인한다.
    var body = popup.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "TextTMP");
    foreach (var character in body.textInfo.characterInfo.Take(body.textInfo.characterCount))
    {
        if (!character.isVisible) continue;
        var colors = body.textInfo.meshInfo[character.materialReferenceIndex].colors32;
        for (int vertex = 0; vertex < 4; vertex++)
            Check(colors[character.vertexIndex + vertex].a == 255, phase + ": finished character stayed transparent");
    }
}
System.Collections.IEnumerator Run()
{
    var originalData = (GameData)dataField.GetValue(game);
    var originalPath = pathField.GetValue(game);
    var originalLanguage = game.Language;
    var errors = new System.Collections.Generic.List<string>();
    UnityEngine.Application.LogCallback onLog = (message, stack, type) =>
    {
        if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception
            || (type == UnityEngine.LogType.Warning && (message.IndexOf("DOTWEEN", System.StringComparison.OrdinalIgnoreCase) >= 0 || message.Contains("destroyed"))))
            errors.Add(message + "\n" + stack);
    };
    var cases = new[]
    {
        (Define.UNLOCK_SHOP_BUTTON, "ShopButtonIcon", Define.LocalizeKey.ShopText, Define.LocalizeKey.ShopUnlockText),
        (Define.UNLOCK_MONASTERY_BUTTON, "MonasteryButtonIcon", Define.LocalizeKey.MonasteryText, Define.LocalizeKey.MonasteryUnlockText),
        (Define.UNLOCK_TRAVEL_BUTTON, "TravelButtonIcon", Define.LocalizeKey.TravelBtnText, Define.LocalizeKey.TravelUnlockText),
        (Define.UNLOCK_FORGE_BUTTON, "ForgeButtonIcon", Define.LocalizeKey.ForgeText, Define.LocalizeKey.ForgeUnlockText),
        (Define.UNLOCK_ALCHEMY_BUTTON, "AlchemyButtonIcon", Define.LocalizeKey.AlchemyText, Define.LocalizeKey.AlchemyUnlockText)
    };
    var trigger = typeof(UI_Village1BtnGroup).GetMethod("TryPlayUnlockAnim", flags);
    var typingField = typeof(UI_UnlockButtonPopup).GetField("_isAnimating", flags);
    int closed = 0;
    System.Action<System.Type> onClosed = type => { if (type == typeof(UI_UnlockButtonPopup)) closed++; };
    UnityEngine.Application.logMessageReceived += onLog;
    Managers.UI.OnPopupClosed += onClosed;
    try
    {
        pathField.SetValue(game, null);
        dataField.SetValue(game, Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(originalData)));
        typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
        foreach (var item in cases)
        foreach (var language in new[] { Define.Language.Korean, Define.Language.English })
        {
            game.Language = language;
            string label = item.Item1 + "/" + language;
            var knownIcon = village.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i => i.name == item.Item2).sprite;
            Check(knownIcon != null, label + ": source village icon missing");
            trigger.Invoke(village, new object[] { item.Item1 });
            UI_UnlockButtonPopup popup = null;
            float deadline = UnityEngine.Time.realtimeSinceStartup + 10f;
            while (popup == null && UnityEngine.Time.realtimeSinceStartup < deadline)
            {
                popup = UnityEngine.Object.FindAnyObjectByType<UI_UnlockButtonPopup>();
                yield return null;
            }
            if (popup == null) throw new System.Exception(label + ": actual unlock callback did not open popup");
            yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
            Check(eventSystem.enabled && UnityEngine.GameObject.Find("UnlockInputBlocker") == null, label + ": unlock input was not restored");
            var icon = popup.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i => i.name == "Icon");
            var name = popup.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "IconNameText");
            Check(icon.sprite != null && icon.sprite == knownIcon, label + ": popup icon differs from actual village icon (or is null)");
            Check(name.text == Util.Localize(item.Item3), label + ": localized name mismatch");
            Check((bool)typingField.GetValue(popup), label + ": expected typing in progress");
            CheckBody(popup, item.Item4, label + "/typing");

            game.Language = language == Define.Language.Korean ? Define.Language.English : Define.Language.Korean;
            yield return null;
            CheckBody(popup, item.Item4, label + "/language-during-typing");
            Check(name.text == Util.Localize(item.Item3), label + ": name stale after language change");
            game.Language = language;
            yield return null;
            if (language == Define.Language.Korean)
            {
                ClickContent(popup, label + "/skip");
                yield return null;
                Check(popup != null && Managers.UI.GetPopupCount() == 1, label + ": skip closed popup");
                Check(!(bool)typingField.GetValue(popup), label + ": skip did not finish typing");
            }
            else
            {
                deadline = UnityEngine.Time.realtimeSinceStartup + 30f;
                while ((bool)typingField.GetValue(popup) && UnityEngine.Time.realtimeSinceStartup < deadline) yield return null;
                Check(!(bool)typingField.GetValue(popup), label + ": natural typing did not finish");
            }
            CheckFinishedAlpha(popup, label + "/finished");
            CheckBody(popup, item.Item4, label + "/finished");
            game.Language = language == Define.Language.Korean ? Define.Language.English : Define.Language.Korean;
            yield return null;
            CheckBody(popup, item.Item4, label + "/language-after-finish");
            Check(name.text == Util.Localize(item.Item3), label + ": finished name stale after language change");
            Check(!(bool)typingField.GetValue(popup), label + ": language change restarted completed typing");
            int beforeClose = closed;
            ClickContent(popup, label + "/close");
            yield return new UnityEngine.WaitForSecondsRealtime(0.5f);
            Check(popup == null && Managers.UI.GetPopupCount() == 0 && closed == beforeClose + 1, label + ": close event/stack incorrect");
            if (Managers.UI.GetPopupCount() != 0) Managers.UI.CloseAllPopupUI();
        }
        Check(errors.Count == 0, "Console errors/warnings: " + string.Join("\n", errors));
        if (failures.Count != 0) throw new System.Exception("UnlockPopup failed (" + failures.Count + "):\n" + string.Join("\n", failures));
        UnityEngine.Debug.Log("[UnlockPopup] PASS: 5 actual unlock callbacks x 2 languages; icons, glyph bounds, typing/skip/finish, live language and close verified");
    }
    finally
    {
        Managers.UI.CloseAllPopupUI();
        Managers.UI.OnPopupClosed -= onClosed;
        UnityEngine.Application.logMessageReceived -= onLog;
        dataField.SetValue(game, originalData);
        pathField.SetValue(game, null);
        game.Language = originalLanguage;
        typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags).Invoke(game, null);
        game.Player.RefreshStats(fullHeal: false);
        pathField.SetValue(game, originalPath);
    }
}
CoroutineManager.StartCoroutine(Run());
return "Unlock popup regression started (actual unlock animation per case); isolated GameData/language restored afterwards";
