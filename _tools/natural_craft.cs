// 마을에서 실제 제작·장착·강화 버튼 경로를 검증한다. 재화 직접 지급 없음.
System.Collections.IEnumerator Click(string name)
{
    var e = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
    var go = UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsSortMode.None).First(x => x.name == name).gameObject;
    UnityEngine.EventSystems.ExecuteEvents.Execute(go, e, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    yield return new UnityEngine.WaitForSecondsRealtime(1.5f);
}
System.Collections.IEnumerator Run()
{
    if (!string.IsNullOrEmpty(Managers.Game.GetCraftBlockReason(30007))) throw new System.Exception("Craft prerequisites unmet");
    int gold = Managers.Game.Gold;
    yield return Click("EquipmentButton");
    yield return Click("W00Btn_gen1");
    yield return Click("DontHaveBtn");
    var equip = Managers.Game.OwnedEquipments.FirstOrDefault(x => x.DataId == 30007);
    if (equip == null || Managers.Game.Gold != gold - Managers.Game.GetRecipeFor(30007).Gold) throw new System.Exception("Craft ownership/gold mismatch");
    yield return Click("EquipBtn");
    if (!equip.IsEquipped) throw new System.Exception("Equip failed");
    yield return Click("EnhaceBtn");
    yield return Click("Scroll1Box");
    yield return Click("Btn_BtnGroup_02");
    int scrolls = Managers.Game.GetItemCount(10011);
    yield return Click("Enhance_Btn");
    yield return new UnityEngine.WaitForSecondsRealtime(10f);
    if (Managers.Game.GetItemCount(10011) != scrolls - 1) throw new System.Exception("Enhance scroll was not consumed");
    UnityEngine.Debug.Log("[NaturalProgress] crafted/equipped/enhanced 30007 level=" + equip.EnhanceLevel + " gold=" + Managers.Game.Gold);
}
CoroutineManager.StartCoroutine(Run());
return "natural craft started";
