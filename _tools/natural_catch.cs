// 도감이 열린 상태에서 사냥으로 쌓인 관심만 사용해 포획·동행을 검증한다.
System.Collections.IEnumerator Run()
{
    float end = UnityEngine.Time.realtimeSinceStartup + 240f;
    while (UnityEngine.Time.realtimeSinceStartup < end)
    {
        var handlers = UnityEngine.Object.FindObjectsByType<UI_EventHandler>(UnityEngine.FindObjectsSortMode.None);
        var summon = handlers.FirstOrDefault(h => h.name == "MinionSummonButton");
        var result = UnityEngine.Object.FindAnyObjectByType<UI_MinionInfoPopup>();
        if (result != null && summon == null)
        {
            int id = (int)typeof(UI_MinionInfoPopup).GetField("_dataId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(result);
            if (Managers.MinMon.IsOwned(id))
            {
                UnityEngine.Debug.Log("[NaturalProgress] caught=" + id + "; summon via bestiary record after closing result");
                yield break;
            }
        }
        if (summon != null)
        {
            summon.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
            yield return new UnityEngine.WaitForSecondsRealtime(1f);
            if (Managers.Game.CompanionMinMonId < 0) throw new System.Exception("Companion was not set after summon");
            UnityEngine.Debug.Log("[NaturalProgress] caught and summoned=" + Managers.Game.CompanionMinMonId);
            yield break;
        }
        if (UnityEngine.Object.FindAnyObjectByType<UI_TamePopup>() == null
            && UnityEngine.Object.FindAnyObjectByType<UI_MinionInfoPopup>() == null)
        {
            var start = handlers.FirstOrDefault(h => h.name == "MinimonBtn");
            if (start != null && Managers.MinMon.HasInterest)
                start.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        }
        else
        {
            var lure = handlers.FirstOrDefault(h => h.name == "LureBtn" && h.GetComponent<UnityEngine.UI.Button>().interactable);
            if (lure != null)
                lure.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        }
        yield return new UnityEngine.WaitForSecondsRealtime(2f);
    }
    throw new System.Exception("No natural catch within 240 seconds; inspect chance/interest/animation");
}
CoroutineManager.StartCoroutine(Run());
return "natural catch and summon started";
