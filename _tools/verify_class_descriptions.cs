// 격리 마을 세션 전용. 클래스 설명의 수치와 실제 팝업 배치를 검사한다.
// python _tools/unity_exec.py _tools/verify_class_descriptions.cs --allow-async
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var game = Managers.Game;
var dataField = typeof(GameManager).GetField("_gameData", flags);
if (!UnityEditor.EditorApplication.isPlaying || game.Player == null || game.Player.InBattle
    || typeof(GameManager).GetField("_path", flags).GetValue(game) != null || Managers.UI.GetPopupCount() != 0)
    throw new System.Exception("Isolated town session with empty popup stack required");
void Check(bool pass, string message)
{
    if (!pass) throw new System.Exception("ClassDescriptions: " + message);
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
UnityEngine.Rect GlyphBounds(TMPro.TMP_Text text)
{
    var canvas = text.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
    var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    var min = UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, text.transform.TransformPoint(text.textBounds.min));
    var max = UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, text.transform.TransformPoint(text.textBounds.max));
    return UnityEngine.Rect.MinMaxRect(min.x, min.y, max.x, max.y);
}
void CheckActions(UI_SkillDetailPopup popup, TMPro.TMP_Text body, int id)
{
    var bottomTab = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsSortMode.None)
        .First(b => b.name == "CharacterButton");
    float tabTop = Bounds(bottomTab.GetComponent<UnityEngine.RectTransform>()).yMax;
    foreach (var button in popup.GetComponentsInChildren<UnityEngine.UI.Button>(true))
    {
        if (!button.gameObject.activeInHierarchy || (button.name != "MemorizeBtn" && button.name != "NextLvlBtn")) continue;
        var rect = Bounds(button.GetComponent<UnityEngine.RectTransform>());
        Check(rect.xMin >= 0 && rect.xMax <= UnityEngine.Screen.width && rect.yMin >= 0 && rect.yMax <= UnityEngine.Screen.height,
            "Action outside game screen: " + id + "/" + button.name + " " + rect + " screen=" + UnityEngine.Screen.width + "x" + UnityEngine.Screen.height);
        Check(!rect.Overlaps(GlyphBounds(body)), "Description overlaps action: " + id + "/" + button.name);
        Check(rect.yMin > tabTop, "Action covers bottom navigation: " + id + "/" + button.name);
    }
    var memory = popup.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == "Txt_MemoryDisplayGroup_02");
    memory.ForceMeshUpdate();
    Check(!memory.isActiveAndEnabled || GlyphBounds(memory).yMin > tabTop, "Memory gauge covers bottom navigation: " + id);
}
System.Collections.IEnumerator Run()
{
    var original = dataField.GetValue(game);
    var language = game.Language;
    UI_SkillDetailPopup popup = null;
    try
    {
        dataField.SetValue(game, new GameData());
        game.Language = Define.Language.English;
        string steel = ClassAbilityDescription.GetDescription(5004);
        Check(steel.Contains("Upon learning: level 1") && steel.Contains("Max HP +50") && steel.Contains("Defense +5"),
            "Unowned steel previews both level-one stats");
        Check(ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[5007], 2).Contains("Physical attack +30 (6s)"),
            "Level two doubles battle-start attack but not duration");
        Check(ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[5105], 2).Contains("Deal 180 bonus damage"),
            "Level two doubles flat conditional damage");
        string healing = ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[5113], 2);
        Check(healing.Contains("40%") && healing.Contains("Restore 400 HP") && healing.Contains("Before taking lethal damage"),
            "Fatal-trigger healing scales amount, not chance");
        string timer = ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[5116], 2);
        Check(timer.Contains("cooldown 18s") && timer.Contains("Physical attack +60 (6s)"), "Timed ability interval and effect are distinct");
        Check(ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[5142], 4).Contains("Combo duration +40%"),
            "Combo duration matches level-four runtime bonus");
        Check(ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[5139], 1).Contains("Failures needed"),
            "Capture threshold describes failure count, not an incorrect minimum attempt");

        popup = Managers.UI.ShowPopupUI<UI_SkillDetailPopup>();
        popup.SetInfo(5004);
        yield return new UnityEngine.WaitForSecondsRealtime(1f);
        var body = popup.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == "LvEffectTxt_Real");
        CheckActions(popup, body, 5004);
        var panel = popup.GetComponentsInChildren<UnityEngine.RectTransform>(true).First(t => t.name == "SubFrame");
        var ids = Managers.Data.SkillDic.Values.Where(d => d.Source == Define.AbilitySource.Class).Select(d => d.DataId).OrderBy(id => id).ToArray();
        Check(ids.Length == 12, "All current class abilities covered");
        int views = 0;
        foreach (var lang in new[] { Define.Language.Korean, Define.Language.English })
        {
            game.Language = lang;
            foreach (int id in ids)
            {
                game.LearnedAbilities.Remove(id);
                game.AbilityLevels.Remove(id);
                popup.SetInfo(id);
                // SetInfo가 매번 0.5초 진입 이동을 재생하므로 최종 배치를 검사한다.
                yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
                yield return new UnityEngine.WaitForEndOfFrame();
                body.ForceMeshUpdate();
                Check(body.isActiveAndEnabled && !string.IsNullOrWhiteSpace(body.text) && !body.text.Contains("ClassAbility"), "No missing description keys: " + id);
                if (lang == Define.Language.English)
                    Check(!System.Text.RegularExpressions.Regex.IsMatch(body.text + Managers.Ability.GetName(id), "[가-힣]"), "English class view contains Korean: " + id);
                // 저작된 TMP rect는 글자보다 작을 수 있다. 실제 glyph가 장식 패널 안에 있는지 검사한다.
                var bodyBounds = GlyphBounds(body);
                var panelBounds = Bounds(panel);
                Check(bodyBounds.xMin >= panelBounds.xMin && bodyBounds.xMax <= panelBounds.xMax && bodyBounds.yMin >= panelBounds.yMin,
                    "Acquisition glyphs exceed panel: " + id + "/" + lang);
                Check(bodyBounds.xMin >= 0 && bodyBounds.xMax <= UnityEngine.Screen.width && bodyBounds.yMin >= 0 && bodyBounds.yMax <= UnityEngine.Screen.height,
                    "Acquisition body outside game screen: " + id + "/" + lang);
                CheckActions(popup, body, id);
                game.LearnedAbilities.Add(id);
                game.AbilityLevels[id] = 1;
                popup.SetInfo(id);
                yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
                var next = popup.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == "NextLvlBtn");
                Check(next.gameObject.activeInHierarchy, "Learned ability exposes next-level preview: " + id);
                // 이 검사는 클릭 처리 후 레이아웃 대상이다. 실제 클릭 경로는 성장 UI 회귀에서 별도 검증한다.
                typeof(UI_SkillDetailPopup).GetMethod("OnClickNextLvl", flags).Invoke(popup, null);
                yield return null;
                yield return new UnityEngine.WaitForEndOfFrame();
                body.ForceMeshUpdate();
                Check(body.text.Contains(ClassAbilityDescription.GetLevelSummary(Managers.Data.SkillDic[id], 2)), "Next-level effect missing: " + id);
                var nextBounds = GlyphBounds(body);
                Check(nextBounds.yMin >= 0 && nextBounds.yMin >= Bounds(panel).yMin,
                    "Next-level glyphs exceed panel or screen: " + id + "/" + lang);
                CheckActions(popup, body, id);
                views += 2;
            }
        }
        game.AbilityLevels[5004] = 5;
        Check(!ClassAbilityDescription.GetDescription(5004, true).Contains(Util.Localize("ClassAbilityNextFormat").Split('{')[0]),
            "Maximum level has no phantom next stage");
        UnityEngine.Debug.Log("PASS: ClassDescriptions " + views + " localized acquisition/next views and numeric effect contracts");
    }
    finally
    {
        if (popup != null) Managers.UI.ClosePopupUI(popup);
        dataField.SetValue(game, original);
        game.Language = language;
    }
}
game.Player.StartCoroutine(Run());
return "Class descriptions verification started; inspect console for PASS/errors";
