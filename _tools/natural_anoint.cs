// 수도원 해금 이후, 마을 메인에서 실제 축성 슬롯 클릭.
System.Collections.IEnumerator Click(string name)
{
    var e = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
    var go = UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsSortMode.None).First(x => x.name == name).gameObject;
    UnityEngine.EventSystems.ExecuteEvents.Execute(go, e, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    yield return new UnityEngine.WaitForSecondsRealtime(2f);
}
System.Collections.IEnumerator Run()
{
    yield return Click("MonasteryButtonIcon");
    yield return Click("MonasteryClassButton");
    var p = UnityEngine.Object.FindAnyObjectByType<UI_ClassPopup>();
    var root = p.GetComponentsInChildren<UnityEngine.Transform>().First(x => x.name == "SkillBookSkill_0");
    var b = root.GetComponentsInChildren<UI_EventHandler>().First();
    b.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    int id = Managers.Data.SkillDic.Values.Where(x => x.Source == Define.AbilitySource.Class).Min(x => x.DataId);
    if (!Managers.Ability.IsLearned(id) || Managers.Game.ClassProficiency != 0) throw new System.Exception("Anoint failed");
    UnityEngine.Debug.Log("[NaturalProgress] learned class ability=" + id);
}
CoroutineManager.StartCoroutine(Run());
return "natural anoint started";
