// 실제 전투를 10배 시간으로 관찰한다. 레벨·재화·처치·드롭은 게임만 변경한다.
System.Collections.IEnumerator Click(string name)
{
    float end = UnityEngine.Time.realtimeSinceStartup + 20f;
    UI_EventHandler h;
    while ((h = UnityEngine.Object.FindObjectsByType<UI_EventHandler>(UnityEngine.FindObjectsSortMode.None).SingleOrDefault(x => x.name == name)) == null)
    {
        if (UnityEngine.Time.realtimeSinceStartup >= end) throw new System.Exception("Missing button: " + name);
        yield return null;
    }
    h.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1.5f);
}
System.Collections.IEnumerator Travel(Data.TravelNodeData target)
{
    UnityEngine.Time.timeScale = 1f;
    yield return Click("TravelButton");
    yield return Click("FieldCategoryButton");
    var field = UnityEngine.Object.FindObjectsByType<UI_FieldSelectButton>(UnityEngine.FindObjectsSortMode.None).Single(x => x.NodeId == target.ParentId);
    field.GetComponent<UI_EventHandler>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1f);
    var map = UnityEngine.Object.FindObjectsByType<UI_MapSelectButton>(UnityEngine.FindObjectsSortMode.None).Single(x => x.NodeId == target.Id);
    map.GetComponent<UI_EventHandler>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1f);
    yield return Click("ButtonMapSelectGO");
    yield return new UnityEngine.WaitForSecondsRealtime(4f);
    UnityEngine.Time.timeScale = 10f;
}
System.Collections.IEnumerator Run()
{
    var game = Managers.Game;
    string path = "C:/stopresent/Unity/RE001/Documents/Verification-20260908/natural-hunt.log";
    float start = UnityEngine.Time.realtimeSinceStartup;
    float nextLog = 0;
    float originalScale = UnityEngine.Time.timeScale;
    try
    {
        if (game.AllUnlocked) throw new System.Exception("Cheat enabled; refusing natural test");
        while (game.Level < 31 && UnityEngine.Time.realtimeSinceStartup - start < 900f)
        {
            var target = Managers.Data.TravelNodeDic.Values
                .Where(n => n.AreaType == "Battle" && game.IsTravelNodeUnlocked(n.Id))
                .OrderByDescending(n => Managers.Data.CreatureDic.Values.Where(c => c.Habitat == n.Name).Average(c => c.TotalExp))
                .ThenBy(n => n.Id).First();
            if (game.CurrentTravelAreaId != target.Id || !game.Player.InBattle)
                yield return Travel(target);
            if (UnityEngine.Time.realtimeSinceStartup >= nextLog)
            {
                string line = $"elapsed={UnityEngine.Time.realtimeSinceStartup-start:F1}s gameTime={UnityEngine.Time.time:F1} level={game.Level} exp={game.Exp} gold={game.Gold} hp={game.Player.Hp} area={game.CurrentTravelAreaId} kills={game.OwnedMinMons.Sum(m=>m.KillCount)} quest={game.CurrentQuestID}\n";
                System.IO.File.AppendAllText(path, line);
                nextLog = UnityEngine.Time.realtimeSinceStartup + 15f;
            }
            yield return new UnityEngine.WaitForSecondsRealtime(1f);
        }
        System.IO.File.AppendAllText(path, "FINISHED level=" + game.Level + " elapsed=" + (UnityEngine.Time.realtimeSinceStartup-start) + "\n");
    }
    finally { UnityEngine.Time.timeScale = originalScale; }
}
CoroutineManager.StartCoroutine(Run());
return "natural hunt started; target level 31, 900 real-second limit";
