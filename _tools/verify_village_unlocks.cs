// 실제 완료 보고 후 마을 메인으로 복귀하고 해금 연출이 끝난 뒤 실행한다. 재접속 후에도 동일.
var group = UnityEngine.Object.FindAnyObjectByType<UI_Village1BtnGroup>();
foreach (var pair in new[] {
    (Define.UNLOCK_TRAVEL_BUTTON, "TravelButtonIcon"),
    (Define.UNLOCK_ALCHEMY_BUTTON, "AlchemyButtonIcon"),
    (Define.UNLOCK_FORGE_BUTTON, "ForgeButtonIcon"),
    (Define.UNLOCK_SHOP_BUTTON, "ShopButtonIcon"),
    (Define.UNLOCK_MONASTERY_BUTTON, "MonasteryButtonIcon") })
{
    if (!Managers.Game.UnlockedBtnIds.Contains(pair.Item1)) continue;
    var icon = group.GetComponentsInChildren<UnityEngine.Transform>(true).Single(x => x.name == pair.Item2);
    if (!icon.gameObject.activeInHierarchy) throw new System.Exception("Unlocked button hidden: " + pair.Item2);
    var accepts = typeof(UI_Village1BtnGroup).GetMethod("IsUnlockAnimTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
    if (!(bool)accepts.Invoke(null, new object[] { pair.Item1 })) throw new System.Exception("Unlock event ignored: " + pair.Item1);
}
return "PASS: unlocked village icons visible and event targets accepted";
