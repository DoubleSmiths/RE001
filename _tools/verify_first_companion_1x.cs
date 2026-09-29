// 첫 길드 안내를 끝낸 새 게임에서 실행. 진행도와 시간 배율을 변경하지 않고 실제 버튼만 누른다.
// python _tools/unity_exec.py _tools/verify_first_companion_1x.cs --allow-async
System.Collections.IEnumerator Click(string name)
{
    float end = UnityEngine.Time.realtimeSinceStartup + 20f;
    UI_EventHandler h;
    while ((h = UnityEngine.Object.FindObjectsByType<UI_EventHandler>(UnityEngine.FindObjectsSortMode.None).FirstOrDefault(x => x.name == name)) == null)
    {
        if (UnityEngine.Time.realtimeSinceStartup >= end) throw new System.Exception("Missing button: " + name);
        yield return null;
    }
    h.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1.5f);
}
System.Collections.IEnumerator Run()
{
    var game = Managers.Game;
    if (game.AllUnlocked || UnityEngine.Time.timeScale != 1f) throw new System.Exception("Requires uncheated game at 1x");
    var target = Managers.Data.TravelNodeDic.Values.Where(n => n.AreaType == "Battle" && game.IsTravelNodeUnlocked(n.Id)).OrderBy(n => n.Id).First();
    yield return Click("TravelButton");
    yield return Click("FieldCategoryButton");
    var field = UnityEngine.Object.FindObjectsByType<UI_FieldSelectButton>(UnityEngine.FindObjectsSortMode.None).Single(x => x.NodeId == target.ParentId);
    field.GetComponent<UI_EventHandler>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1f);
    var map = UnityEngine.Object.FindObjectsByType<UI_MapSelectButton>(UnityEngine.FindObjectsSortMode.None).Single(x => x.NodeId == target.Id);
    map.GetComponent<UI_EventHandler>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1f);
    yield return Click("ButtonMapSelectGO");
    float start = UnityEngine.Time.realtimeSinceStartup;
    while (!Managers.MinMon.HasInterest && UnityEngine.Time.realtimeSinceStartup - start < 180f)
        yield return new UnityEngine.WaitForSecondsRealtime(1f);
    UnityEngine.Debug.Log($"[Companion1x] interest={Managers.MinMon.HasInterest}, seconds={UnityEngine.Time.realtimeSinceStartup-start:F1}, kills={game.OwnedMinMons.Sum(m=>m.KillCount)}, level={game.Level}, scale={UnityEngine.Time.timeScale}");
    if (!Managers.MinMon.HasInterest) throw new System.Exception("No interest within 180 seconds");
    yield return Click("BestiaryButton");
}
CoroutineManager.StartCoroutine(Run());
return "first habitat test started at 1x";
