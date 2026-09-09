// 새 게임의 플레이 모드에서 두 협회 버튼이 같은 첫 임무 안내를 여는지 검사한다.
// 버튼 경로의 통합 검사이며 실제 터치/정상 속도 완주 검증과 별개다.
if (!UnityEditor.EditorApplication.isPlaying || Managers.Quest.CurrentQuestID != 1)
    throw new System.Exception("Fresh game at quest 1 required");
var group = UnityEngine.Object.FindFirstObjectByType<UI_Village1BtnGroup>();
if (group == null) throw new System.Exception("Village button group missing");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var linesField = typeof(UI_DialoguePopup).GetField("_lines", flags);
var finishedField = typeof(UI_DialoguePopup).GetField("_onFinished", flags);
var expected = Managers.Data.DialogueScriptDic[Managers.Quest.GetCurrentDialogueID()].Lines;
foreach (string method in new[] { "OnClickVillage1GuildTalkButton", "OnClickVillage1GuildQuestButton" })
{
    var existing = UnityEngine.Object.FindObjectsByType<UI_DialoguePopup>(UnityEngine.FindObjectsSortMode.None).Select(x => x.GetInstanceID()).ToHashSet();
    group.GetType().GetMethod(method, flags).Invoke(group, null);
    try
    {
        var popup = UnityEngine.Object.FindObjectsByType<UI_DialoguePopup>(UnityEngine.FindObjectsSortMode.None)
            .Single(x => !existing.Contains(x.GetInstanceID()));
        var actual = (System.Collections.Generic.List<Data.DialogueScriptLine>)linesField.GetValue(popup);
        if (!actual.SequenceEqual(expected)) throw new System.Exception(method + " opened idle chatter instead of first mission");
        if (finishedField.GetValue(popup) == null) throw new System.Exception(method + " has no quest completion callback");
        if (Managers.Quest.CurrentQuestID != 1) throw new System.Exception("Quest advanced before dialogue completion");
    }
    finally
    {
        Managers.UI.ClosePopupUI();
    }
}
return "PASS: Talk and Mission open the complete first briefing; progress waits for the dialogue";
