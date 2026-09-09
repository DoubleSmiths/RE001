// 호출자가 만든 격리 UI 테스트 세션 전용. GameManager._path == null인 전투 상태에서 실행한다.
// 6개 능력/동행/HP/전투 상태를 테스트용으로 바꾸며 게임 데이터는 자동 원복하지 않는다.
// 언어 검사를 켠 경우 언어 설정만 finally에서 복원한다. 실제 사용자 세이브에 실행하지 않는다.
// python _tools/unity_exec.py _tools/verify_skillbar.cs --allow-async
const bool CHECK_LANGUAGE = true;
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play mode required");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var savePath = typeof(GameManager).GetField("_path", flags);
if (savePath == null || savePath.GetValue(Managers.Game) != null)
    throw new System.Exception("Caller must disable save path for this isolated UI seed session");
if (Managers.UI.GetPopupCount() != 0) throw new System.Exception("Close existing popups before verification");
var player = Managers.Game.Player;
if (player == null || !player.InBattle || player.State == Define.State.Dead || UnityEngine.Time.timeScale <= 0f)
    throw new System.Exception("Live battle at positive time scale required");
var runner = player.GetComponent<AbilityRunner>();
var bar = UnityEngine.Object.FindFirstObjectByType<UI_SkillBar>();
if (runner == null || bar == null) throw new System.Exception("Runtime AbilityRunner/SkillBar missing");

UnityEngine.Rect ScreenBounds(UnityEngine.RectTransform rect)
{
    var canvas = rect.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
    var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    var corners = new UnityEngine.Vector3[4];
    rect.GetWorldCorners(corners);
    var points = corners.Select(p => UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, p)).ToArray();
    return UnityEngine.Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
}
System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> Hits(UnityEngine.Vector2 point)
{
    var system = UnityEngine.EventSystems.EventSystem.current;
    if (system == null) throw new System.Exception("EventSystem missing");
    var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    system.RaycastAll(new UnityEngine.EventSystems.PointerEventData(system) { position = point }, results);
    return results;
}
System.Collections.IEnumerator Run()
{
    var originalLanguage = Managers.Game.Language;
    UI_MinionInfoPopup popup = null;
    try
    {
        int[] ids = { 6001, 6002, 6003, 6101, 6102, 6103 };
        foreach (int id in ids)
            if (!Managers.Data.SkillDic.TryGetValue(id, out var data) || data.TriggerType != Define.AbilityTrigger.Active)
                throw new System.Exception("Active seed skill missing: " + id);

        // 6001은 영구 기억 목록에 넣지 않고 첫 동행의 부여 능력으로 확인한다.
        var companion = Managers.Game.OwnedMinMons.FirstOrDefault(x => x.DataId == 2001);
        if (companion == null)
        {
            companion = new MinMonSaveData { DataId = 2001 };
            Managers.Game.OwnedMinMons.Add(companion);
        }
        companion.CaughtCount = System.Math.Max(1, companion.CaughtCount);
        companion.IsCompanion = true;
        companion.AtkGrade = companion.AtkSpeedGrade = companion.CritGrade = 0;
        Managers.Game.CompanionMinMonId = 2001;
        Managers.Game.LearnedAbilities.Clear();
        Managers.Game.EquippedAbilities.Clear();
        foreach (int id in ids.Skip(1))
        {
            Managers.Game.LearnedAbilities.Add(id);
            Managers.Game.EquippedAbilities.Add(id);
            Managers.Game.AbilityLevels[id] = 1;
        }
        Managers.Ability.OnCompanionChanged();
        bar.Refresh();
        // 새로 활성화된 Graphic의 렌더 깊이는 화면을 한 번 그린 뒤 확정된다.
        yield return new UnityEngine.WaitForEndOfFrame();
        yield return null;
        UnityEngine.Canvas.ForceUpdateCanvases();
        if (!runner.ActiveSkillIds.SequenceEqual(ids) || Managers.Ability.GetLevel(6001) < 1)
            throw new System.Exception("Six active slots/companion-first ordering failed");

        var slots = bar.GetComponentsInChildren<UnityEngine.UI.Button>(true)
            .Where(x => x.name.StartsWith("SkillSlot_") && x.gameObject.activeSelf).OrderBy(x => x.name).ToArray();
        if (slots.Length != 6) throw new System.Exception("Expected six visible skill slots: " + slots.Length);
        var bounds = slots.Select(x => ScreenBounds((UnityEngine.RectTransform)x.transform)).ToArray();
        var controls = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsSortMode.None)
            .Where(x => x.gameObject.activeInHierarchy && (x.name == "ReturnButton" || x.name == "TravelButton" || x.name == "CharacterButton")).ToArray();
        foreach (string name in new[] { "ReturnButton", "TravelButton", "CharacterButton" })
            if (!controls.Any(x => x.name == name)) throw new System.Exception("Required visible control missing: " + name);
        for (int i = 0; i < slots.Length; i++)
        {
            var rect = bounds[i];
            if (rect.width <= 0f || rect.height <= 0f || rect.xMin < -1f || rect.yMin < -1f
                || rect.xMax > UnityEngine.Screen.width + 1f || rect.yMax > UnityEngine.Screen.height + 1f)
                throw new System.Exception("Slot outside screen: " + slots[i].name + " " + rect);
            for (int j = i + 1; j < slots.Length; j++)
                if (rect.Overlaps(bounds[j])) throw new System.Exception("Slots overlap: " + i + "/" + j);
            foreach (var control in controls)
                if (rect.Overlaps(ScreenBounds((UnityEngine.RectTransform)control.transform)))
                    throw new System.Exception(slots[i].name + " overlaps " + control.name);
            var hits = Hits(rect.center);
            var receiver = hits.Count == 0 ? null : hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>();
            if (receiver != slots[i]) throw new System.Exception("Top raycast receiver is not " + slots[i].name
                + ": " + (hits.Count == 0 ? "none" : hits[0].gameObject.name));
        }

        var group = bar.GetComponent<UnityEngine.CanvasGroup>();
        if (group == null || group.alpha <= 0f || !group.blocksRaycasts) throw new System.Exception("Battle skill bar is hidden or noninteractive");
        // 이전 테스트의 공통 쿨타임이 남아 있으면 실제 경과를 기다린다. 런너 내부 쿨타임은 조작하지 않는다.
        float deadline = UnityEngine.Time.realtimeSinceStartup + 25f;
        while (runner.GetCooldownRemain(6001) > 0f && UnityEngine.Time.realtimeSinceStartup < deadline) yield return null;
        if (runner.GetCooldownRemain(6001) > 0f) throw new System.Exception("Seed cooldown did not expire within 25 seconds");
        yield return null;
        var mask = slots[0].GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(x => x.name == "CooldownMask");
        var remain = slots[0].GetComponentsInChildren<TMPro.TMP_Text>(true).Single(x => x.name == "RemainTxt");
        if (mask.gameObject.activeInHierarchy && mask.enabled && mask.rectTransform.rect.height > 0.01f)
            throw new System.Exception("Ready cooldown mask still covers the icon");
        player.Hp = player.MaxHp * 0.5f;
        if (!runner.UseSkill(6001)) throw new System.Exception("Actual companion skill 6001 failed to activate");
        yield return null;
        UnityEngine.Canvas.ForceUpdateCanvases();
        if (runner.GetCooldownRemain(6001) <= 0f || !mask.isActiveAndEnabled || mask.rectTransform.rect.height <= 0f
            || mask.type == UnityEngine.UI.Image.Type.Filled || string.IsNullOrWhiteSpace(remain.text))
            throw new System.Exception("Active cooldown does not show geometry mask and remaining seconds");
        if (UnityEngine.Vector2.Distance(ScreenBounds(remain.rectTransform).center, bounds[0].center) > 1f)
            throw new System.Exception("Remaining seconds shifted away from slot center");

        if (CHECK_LANGUAGE)
        {
            var header = bar.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(x => x.name == "AbilityStatusText");
            Managers.Game.Language = Define.Language.Korean;
            yield return null;
            string korean = header.text;
            if (!korean.Contains(Util.Localize(Managers.Data.SkillDic[6001].Name))) throw new System.Exception("Korean ability name missing");
            Managers.Game.Language = Define.Language.English;
            yield return null;
            if (header.text == korean || !header.text.Contains(Util.Localize(Managers.Data.SkillDic[6001].Name)))
                throw new System.Exception("English ability name/state did not refresh");
        }

        player.StopBattle();
        yield return null;
        if (group.alpha > 0f || group.blocksRaycasts) throw new System.Exception("Town skill bar remained visible/interactive");
        player.StartBattle();
        yield return null;
        if (group.alpha <= 0f || !group.blocksRaycasts) throw new System.Exception("Battle skill bar did not return");
        popup = Managers.UI.ShowPopupUI<UI_MinionInfoPopup>();
        popup.SetInfo(2001);
        yield return null;
        UnityEngine.Canvas.ForceUpdateCanvases();
        if (group.blocksRaycasts) throw new System.Exception("Skill bar accepts input behind popup");
        foreach (var rect in bounds)
            if (Hits(rect.center).Any(x => x.gameObject.transform.IsChildOf(bar.transform)))
                throw new System.Exception("Skill bar appears in raycast results behind popup");
        Managers.UI.ClosePopupUI(popup);
        popup = null;
        yield return null;
        if (!group.blocksRaycasts) throw new System.Exception("Skill bar input did not recover after popup close");
        UnityEngine.Debug.Log("[SkillBar] PASS: six slots in bounds without control overlap, top raycast receivers correct, real cooldown geometry/text centered, town/battle visibility and popup input gating work; language=" + CHECK_LANGUAGE);
    }
    finally
    {
        if (popup != null) Managers.UI.ClosePopupUI(popup);
        if (CHECK_LANGUAGE) Managers.Game.Language = originalLanguage;
    }
}
CoroutineManager.StartCoroutine(Run());
return "Skill bar verification started; caller-owned test seed is not restored";
