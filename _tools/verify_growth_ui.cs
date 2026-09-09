// 저장 경로가 비활성화된 격리 UI 세션의 마을에서 실행한다.
// GameData 복사본에서만 숙련도/능력을 변경하고 finally에서 원래 데이터·HP·언어를 복원한다.
// python _tools/unity_exec.py _tools/verify_growth_ui.cs --allow-async
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play mode required");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var pathField = typeof(GameManager).GetField("_path", flags);
var dataField = typeof(GameManager).GetField("_gameData", flags);
if (pathField == null || dataField == null || pathField.GetValue(Managers.Game) != null)
    throw new System.Exception("Caller must disable save path for the isolated UI session");
var player = Managers.Game.Player;
if (player == null || player.InBattle) throw new System.Exception("Town player required");
if (Managers.UI.GetPopupCount() != 0) throw new System.Exception("Close existing popups before verification");

UnityEngine.Rect ScreenBounds(UnityEngine.RectTransform rect)
{
    var canvas = rect.GetComponentInParent<UnityEngine.Canvas>().rootCanvas;
    var camera = canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    var corners = new UnityEngine.Vector3[4];
    rect.GetWorldCorners(corners);
    var points = corners.Select(x => UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, x)).ToArray();
    return UnityEngine.Rect.MinMaxRect(points.Min(x => x.x), points.Min(x => x.y), points.Max(x => x.x), points.Max(x => x.y));
}
void ClickTopReceiver(UnityEngine.GameObject target)
{
    UnityEngine.Canvas.ForceUpdateCanvases();
    var system = UnityEngine.EventSystems.EventSystem.current;
    if (system == null) throw new System.Exception("EventSystem missing");
    var point = ScreenBounds(target.GetComponent<UnityEngine.RectTransform>()).center;
    var pointer = new UnityEngine.EventSystems.PointerEventData(system)
    {
        position = point,
        pressPosition = point,
        button = UnityEngine.EventSystems.PointerEventData.InputButton.Left
    };
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    system.RaycastAll(pointer, hits);
    var receiver = hits.Count == 0 ? null
        : UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(hits[0].gameObject);
    if (receiver != target) throw new System.Exception("Top click receiver mismatch for " + target.name + ": "
        + (hits.Count == 0 ? "none" : hits[0].gameObject.name));
    pointer.pointerCurrentRaycast = hits[0];
    pointer.pointerPressRaycast = hits[0];
    UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
    UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
    UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
}
UnityEngine.GameObject FirstClassIcon(UI_ClassPopup popup)
{
    var slots = (System.Collections.IList)typeof(UI_ClassPopup).GetField("_slots", flags).GetValue(popup);
    if (slots.Count == 0) throw new System.Exception("Class icon slots missing");
    return (UnityEngine.GameObject)slots[0].GetType().GetField("IconBtn").GetValue(slots[0]);
}
void CheckGuide(UI_ClassPopup popup)
{
    UnityEngine.Canvas.ForceUpdateCanvases();
    var texts = popup.GetComponentsInChildren<TMPro.TMP_Text>(true);
    var guide = texts.Single(x => x.name == "SubtitleText");
    var gauge = texts.Single(x => x.name == "MemoryAmountTxt_1");
    guide.ForceMeshUpdate();
    gauge.ForceMeshUpdate();
    if (guide.text != Util.Localize(Define.GROWTH_CLASS_GUIDE_TEXT) || guide.text == Define.GROWTH_CLASS_GUIDE_TEXT)
        throw new System.Exception("Class guide was not localized: " + Managers.Game.Language);
    if (!guide.isActiveAndEnabled || guide.isTextOverflowing || guide.textInfo.lineCount < 2)
        throw new System.Exception("Class guide is hidden/overflowing/not multiline: " + Managers.Game.Language);
    var guideRect = ScreenBounds(guide.rectTransform);
    if (guideRect.Overlaps(ScreenBounds(gauge.rectTransform))) throw new System.Exception("Class guide overlaps proficiency gauge");
    if (guideRect.xMin < -1f || guideRect.xMax > UnityEngine.Screen.width + 1f
        || guideRect.yMin < -1f || guideRect.yMax > UnityEngine.Screen.height + 1f)
        throw new System.Exception("Class guide outside screen bounds");
    if (Managers.Game.ClassProficiency == 100 && gauge.GetParsedText() != "100/100")
        throw new System.Exception("Full proficiency gauge text mismatch");
}
System.Collections.IEnumerator Run()
{
    var originalData = (GameData)dataField.GetValue(Managers.Game);
    var originalLanguage = Managers.Game.Language;
    bool hadLanguageKey = UnityEngine.PlayerPrefs.HasKey("Language");
    float originalHp = player.Hp;
    UI_ClassPopup classPopup = null;
    UI_SkillDetailPopup detail = null;
    try
    {
        var seed = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(originalData));
        // JsonIgnore인 장착 딕셔너리는 복사된 보유 장비와 동일 참조로 재구성한다.
        seed.EquippedEquipments.Clear();
        foreach (var equip in seed.OwnedEquipments)
            if (equip.IsEquipped) seed.EquippedEquipments[equip.SlotType] = equip;
        int skillId = Managers.Data.SkillDic.Values.Where(x => x.Source == Define.AbilitySource.Class)
            .OrderBy(x => x.DataId).First().DataId;
        if (Managers.Data.SkillDic[skillId].MemoryCapacity <= 0) throw new System.Exception("First class skill must expose a memory cost");
        seed.LearnedAbilities.Remove(skillId);
        seed.EquippedAbilities.Remove(skillId);
        seed.AbilityLevels.Remove(skillId);
        seed.ClassProficiency = 99;
        dataField.SetValue(Managers.Game, seed);

        classPopup = Managers.UI.ShowPopupUI<UI_ClassPopup>();
        classPopup.SetInfo();
        yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
        foreach (var language in new[] { Define.Language.Korean, Define.Language.English })
        {
            Managers.Game.Language = language;
            yield return null;
            CheckGuide(classPopup);
        }
        var beforeLearned = seed.LearnedAbilities.ToArray();
        ClickTopReceiver(FirstClassIcon(classPopup));
        yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
        detail = UnityEngine.Object.FindFirstObjectByType<UI_SkillDetailPopup>();
        if (detail == null || detail.SkillId != skillId || Managers.UI.GetPopupCount() != 2)
            throw new System.Exception("Unowned class icon did not open its skill detail");
        if (Managers.Game.ClassProficiency != 99 || !seed.LearnedAbilities.SequenceEqual(beforeLearned))
            throw new System.Exception("Inspecting an unowned class skill consumed proficiency or learned it");
        var description = detail.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(x => x.name == "LvEffectTxt_Real");
        string expectedDescription = Managers.Ability.GetDesc(skillId);
        if (!description.isActiveAndEnabled || string.IsNullOrWhiteSpace(expectedDescription) || !description.text.Contains(expectedDescription))
            throw new System.Exception("Unowned class effect description not visible");
        var memory = detail.GetComponentsInChildren<UnityEngine.Transform>(true).Single(x => x.name == "GotMEM");
        var memoryText = detail.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(x => x.name == "Txt_GotMEM_01");
        if (!memory.gameObject.activeInHierarchy || !memoryText.isActiveAndEnabled
            || memoryText.GetParsedText() != Managers.Data.SkillDic[skillId].MemoryCapacity.ToString())
            throw new System.Exception("Unowned class memory cost not visible");
        var acquire = detail.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(x => x.name == "MemorizeBtn");
        if (!acquire.gameObject.activeInHierarchy || acquire.interactable) throw new System.Exception("Anoint button not disabled at proficiency 99");

        Managers.UI.ClosePopupUI(detail);
        detail = null;
        yield return null;
        seed.ClassProficiency = 100;
        classPopup.RefreshUI();
        foreach (var language in new[] { Define.Language.Korean, Define.Language.English })
        {
            Managers.Game.Language = language;
            yield return null;
            CheckGuide(classPopup);
        }
        ClickTopReceiver(FirstClassIcon(classPopup));
        yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
        detail = UnityEngine.Object.FindFirstObjectByType<UI_SkillDetailPopup>();
        if (detail == null || detail.SkillId != skillId || Managers.Game.ClassProficiency != 100 || Managers.Ability.IsLearned(skillId))
            throw new System.Exception("Opening the full-proficiency detail changed acquisition state");
        acquire = detail.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(x => x.name == "MemorizeBtn");
        if (!acquire.interactable) throw new System.Exception("Anoint button not enabled at proficiency 100");
        ClickTopReceiver(acquire.gameObject);
        yield return new UnityEngine.WaitForSecondsRealtime(0.8f);
        if (Managers.Game.ClassProficiency != 0 || !Managers.Ability.IsLearned(skillId)
            || seed.LearnedAbilities.Count(x => x == skillId) != 1)
            throw new System.Exception("Detail anointment did not consume 100 and learn once");
        // 습득 후 버튼은 기억하기로 바뀐다. 버튼을 억지로 다시 누르지 않고 획득 API의 중복 방지를 확인한다.
        if (Managers.Ability.TryAnoint(skillId) || Managers.Game.ClassProficiency != 0
            || seed.LearnedAbilities.Count(x => x == skillId) != 1)
            throw new System.Exception("Duplicate anointment changed the acquired state");
        UnityEngine.Debug.Log("[GrowthUI] PASS: actual class-icon raycast opens preview without spending at 99/100, effect and memory cost visible, actual detail click learns once at 100, Korean/English guide fits beside 100/100");
    }
    finally
    {
        if (detail != null) Managers.UI.ClosePopupUI(detail);
        if (classPopup != null) Managers.UI.ClosePopupUI(classPopup);
        dataField.SetValue(Managers.Game, originalData);
        pathField.SetValue(Managers.Game, null);
        Managers.Game.Language = originalLanguage;
        if (!hadLanguageKey) UnityEngine.PlayerPrefs.DeleteKey("Language");
        player.RefreshStats(fullHeal: false);
        player.Hp = originalHp;
    }
}
CoroutineManager.StartCoroutine(Run());
return "Growth UI verification started; original GameData/language will be restored";
