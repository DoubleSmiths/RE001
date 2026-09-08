// 제작 완료 상태에서 부족한 강화 재료를 구매하고 실제 강화·축성으로 이어간다.
System.Collections.IEnumerator Click(string name)
{
    var go = UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsSortMode.None).First(x => x.name == name).gameObject;
    UnityEngine.EventSystems.ExecuteEvents.Execute(go, new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    yield return new UnityEngine.WaitForSecondsRealtime(2f);
}
System.Collections.IEnumerator Run()
{
    // 이전 검증 창 정리만 수행. 게임 진행도와 재화는 변경하지 않는다.
    Managers.UI.CloseAllPopupUI();
    yield return new UnityEngine.WaitForSecondsRealtime(1);
    yield return Click("ShopButtonIcon");
    yield return Click("ShopBuyButton");
    var p = UnityEngine.Object.FindAnyObjectByType<UI_ShopPopup>();
    var img = p.GetComponentsInChildren<UnityEngine.UI.Image>().First(x => x.sprite != null && x.sprite.name == Managers.Data.ItemDic[11001].SpriteName);
    img.GetComponentInParent<UI_EventHandler>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    yield return new UnityEngine.WaitForSecondsRealtime(1);
    UnityEngine.Object.FindAnyObjectByType<UI_ShopConfirmPopup>().GetComponentInChildren<UnityEngine.UI.Slider>().value = 10;
    yield return Click("ShopConfirmOkButton");
    yield return Click("ShopPopupBackground");
    yield return Click("Village1ShopExitButton");
    yield return Click("EquipmentButton");
    yield return Click("W00Btn_gen1");
    yield return Click("EnhaceBtn");
    yield return Click("Scroll1Box");
    yield return Click("Btn_BtnGroup_02");
    int scrolls = Managers.Game.GetItemCount(10011);
    yield return Click("Enhance_Btn");
    yield return new UnityEngine.WaitForSecondsRealtime(15);
    if (Managers.Game.GetItemCount(10011) != scrolls - 1) throw new System.Exception("Enhance failed to consume scroll");
    UnityEngine.Debug.Log("[NaturalProgress] enhancement resolved level=" + Managers.Game.OwnedEquipments.First(x => x.DataId == 30007).EnhanceLevel);
    Managers.UI.CloseAllPopupUI();
    yield return new UnityEngine.WaitForSecondsRealtime(1);
    yield return Click("MonasteryButtonIcon");
    yield return Click("MonasteryClassButton");
    var cls = UnityEngine.Object.FindAnyObjectByType<UI_ClassPopup>();
    var root = cls.GetComponentsInChildren<UnityEngine.Transform>().First(x => x.name == "SkillBookSkill_0");
    root.GetComponentsInChildren<UI_EventHandler>().First().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
    int id = Managers.Data.SkillDic.Values.Where(x => x.Source == Define.AbilitySource.Class).Min(x => x.DataId);
    if (!Managers.Ability.IsLearned(id) || Managers.Game.ClassProficiency != 0) throw new System.Exception("Anoint failed");
    UnityEngine.Debug.Log("[NaturalProgress] learned class ability=" + id);
}
CoroutineManager.StartCoroutine(Run());
return "finish normal progression started";
