// 실행: python _tools/unity_exec.py _tools/natural_report.cs --allow-async
// 실제 버튼 이벤트로 현재 완료 보고를 진행한다. 퀘스트·재료·재화를 직접 변경하지 않는다.
System.Collections.IEnumerator Click(string name)
{
    float end = UnityEngine.Time.realtimeSinceStartup + 20f;
    UI_EventHandler handler = null;
    while (handler == null && UnityEngine.Time.realtimeSinceStartup < end)
    {
        handler = UnityEngine.Object.FindObjectsByType<UI_EventHandler>(UnityEngine.FindObjectsSortMode.None)
            .SingleOrDefault(h => h.name == name);
        if (handler == null) yield return null;
    }
    if (handler == null) throw new System.Exception("Missing visible button: " + name);
    handler.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1.5f);
}
System.Collections.IEnumerator Run()
{
    yield return Click("GuildButtonIcon");
    yield return Click(Managers.Game.CurrentQuestID == 1 ? "Village1GuildTalkButton" : "Village1GuildQuestButton");
    for (int i = 0; i < 80; i++)
    {
        var dialogue = UnityEngine.Object.FindAnyObjectByType<UI_DialoguePopup>();
        if (dialogue == null) break;
        dialogue.GetComponentsInChildren<UI_EventHandler>().Single(h => h.name == "Panel")
            .OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        yield return new UnityEngine.WaitForSecondsRealtime(0.3f);
    }
    yield return new UnityEngine.WaitForSecondsRealtime(2f);
    yield return Click("Village1GuildExitButton");
    yield return new UnityEngine.WaitForSecondsRealtime(7f);
    for (int i = 0; i < 6; i++)
    {
        var reward = UnityEngine.Object.FindObjectsByType<UI_EventHandler>(UnityEngine.FindObjectsSortMode.None)
            .FirstOrDefault(h => (h.name == "ItemPopupBackground" || h.name == "Content") && h.GetComponentInParent<UI_Popup>() != null);
        if (reward == null) break;
        reward.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        yield return new UnityEngine.WaitForSecondsRealtime(2f);
    }
    UnityEngine.Debug.Log("[NaturalProgress] report done quest=" + Managers.Game.CurrentQuestID + " rank=" + Managers.Game.AdventurerRank);
}
Managers.Game.Player.StartCoroutine(Run());
return "report started";
