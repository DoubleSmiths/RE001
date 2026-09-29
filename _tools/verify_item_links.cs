// python _tools/unity_exec.py _tools/verify_item_links.cs --allow-async
// 마을의 격리 세션에서 재료 링크, 스크롤 이벤트 전달, 한영 출처 배치를 검사한다.
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
if (!UnityEditor.EditorApplication.isPlaying || Managers.Game.Player == null || Managers.Game.Player.InBattle
    || typeof(GameManager).GetField("_path", flags).GetValue(Managers.Game) != null)
    throw new System.Exception("Isolated town session with save path disabled required");
if (Managers.UI.GetPopupCount() != 0) throw new System.Exception("Close existing popups first");

void Check(bool ok, string message)
{
    if (!ok) throw new System.Exception("ItemLinks: " + message);
}
System.Collections.IEnumerator Run()
{
    var language = Managers.Game.Language;
    UI_EquipDetailPopup equipment = null;
    UI_ItemPopup item = null;
    try
    {
        Check(!Managers.Game.HasEquipment(30061), "Unowned novice sword required");
        equipment = Managers.UI.ShowPopupUI<UI_EquipDetailPopup>();
        equipment.SetInfo(new Equipment(30061));
        yield return new UnityEngine.WaitForSecondsRealtime(1f);
        yield return new UnityEngine.WaitForEndOfFrame();
        var content = equipment.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == "Content");
        var scroll = content.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        var system = UnityEngine.EventSystems.EventSystem.current;
        Check(UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IDragHandler>(content.gameObject) == scroll.gameObject,
            "Material text must leave dragging to its ScrollRect");
        var canvas = content.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
        var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        content.ForceMeshUpdate();
        var firstLink = content.textInfo.linkInfo[0];
        Check(firstLink.GetLinkID() == "item:11001", "First actual material link");
        var glyph = content.textInfo.characterInfo[firstLink.linkTextfirstCharacterIndex + 1];
        var point = UnityEngine.RectTransformUtility.WorldToScreenPoint(camera,
            content.transform.TransformPoint((glyph.bottomLeft + glyph.topRight) * 0.5f));
        var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        var pointer = new UnityEngine.EventSystems.PointerEventData(system) { position = point, pressPosition = point };
        system.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(hits[0].gameObject) == content.gameObject,
            "Material link must be the visible top click receiver");
        pointer.pointerPressRaycast = hits[0];
        UnityEngine.EventSystems.ExecuteEvents.Execute(content.gameObject, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        yield return new UnityEngine.WaitForSecondsRealtime(1f);
        item = UnityEngine.Object.FindFirstObjectByType<UI_ItemPopup>();
        Check(item != null && Managers.UI.GetPopupCount() == 2, "Link opens exactly one item detail");
        var found = item.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == "ItemPopupItemDesc_Found");
        Check(found.text == ItemSourceGuide.GetFoundLocation(11001, ""), "Link opens the requested material");
        var divider = item.GetComponentsInChildren<UnityEngine.UI.Image>(true).First(i => i.name == "__1").rectTransform;
        int checkedSources = 0;
        foreach (var lang in new[] { Define.Language.Korean, Define.Language.English })
        {
            Managers.Game.Language = lang;
            foreach (int id in Managers.Data.ItemDic.Keys.OrderBy(id => id))
            {
                if (!ItemSourceGuide.TryGetFirstSource(id, out var area, out var creature)) continue;
                item.SetInfo(id);
                UnityEngine.Canvas.ForceUpdateCanvases();
                found.ForceMeshUpdate();
                Check(!found.isTextOverflowing, "Source overflows: " + id + "/" + lang);
                var bottom = found.transform.parent.InverseTransformPoint(found.transform.TransformPoint(found.textBounds.min)).y;
                float dividerTop = divider.anchoredPosition.y + divider.rect.height * (1f - divider.pivot.y);
                Check(bottom > dividerTop, "Source overlaps divider: " + id + "/" + lang);
                checkedSources++;
            }
        }
        Check(!ItemSourceGuide.TryGetFirstSource(10003, out _, out _) && ItemSourceGuide.GetFoundLocation(10003, Managers.Data.ItemDic[10003].FoundLocation)
            == Util.Localize(Managers.Data.ItemDic[10003].FoundLocation), "Non-drop item preserves authored location");
        Managers.UI.ClosePopupUI(item);
        item = null;
        yield return null;
        yield return new UnityEngine.WaitForEndOfFrame();

        // 실제 입력 모듈이 선택하는 상위 드래그 수신기에 프레임을 나눠 이벤트를 전달한다.
        scroll.verticalNormalizedPosition = 1f;
        var drag = new UnityEngine.EventSystems.PointerEventData(system) { position = point, pressPosition = point, pointerPressRaycast = hits[0] };
        UnityEngine.EventSystems.ExecuteEvents.Execute(scroll.gameObject, drag, UnityEngine.EventSystems.ExecuteEvents.initializePotentialDrag);
        UnityEngine.EventSystems.ExecuteEvents.Execute(scroll.gameObject, drag, UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
        yield return null;
        drag.dragging = true;
        drag.position += UnityEngine.Vector2.up * 40f;
        UnityEngine.EventSystems.ExecuteEvents.Execute(scroll.gameObject, drag, UnityEngine.EventSystems.ExecuteEvents.dragHandler);
        yield return null;
        UnityEngine.EventSystems.ExecuteEvents.Execute(scroll.gameObject, drag, UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
        Check(scroll.verticalNormalizedPosition < 0.9f, "Material body scrolls through drag route");
        Check(Managers.UI.GetPopupCount() == 1 && !Managers.Game.HasEquipment(30061), "Drag neither opens an item nor crafts equipment");
        UnityEngine.Debug.Log("PASS: ItemLinks visible click, " + checkedSources + " localized drop-source layouts, fallback, drag routing");
    }
    finally
    {
        if (item != null) Managers.UI.ClosePopupUI(item);
        if (equipment != null) Managers.UI.ClosePopupUI(equipment);
        Managers.Game.Language = language;
    }
}
Managers.Game.Player.StartCoroutine(Run());
return "Item link verification started; inspect console for PASS/errors";
