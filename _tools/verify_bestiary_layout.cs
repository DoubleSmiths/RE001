// 기존 목록의 기하 구조를 기준 파일로 저장한다. 가시 영역 생성 변경 전/후를 같은 저장에서 비교한다.
var game = Managers.Game;
if (!UnityEngine.Application.isPlaying || game.Player == null || game.Player.InBattle || Managers.UI.GetPopupCount() != 0)
    throw new System.Exception("BestiaryLayout requires town with no popups");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
System.Collections.IEnumerator Run()
{
    var original = dataField.GetValue(game);
    var originalPath = pathField.GetValue(game);
    UI_BestiaryPopup popup = null;
    try
    {
        pathField.SetValue(game, null);
        dataField.SetValue(game, Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(Newtonsoft.Json.JsonConvert.SerializeObject(original)));
        popup = Managers.UI.ShowPopupUI<UI_BestiaryPopup>();
        popup.SetInfo();
        yield return new UnityEngine.WaitForSecondsRealtime(1f);
        UnityEngine.Canvas.ForceUpdateCanvases();
        var scroll = popup.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true).Single();
        var regions = scroll.content.GetComponentsInChildren<UI_Bestiary>(true).Select(section =>
        {
            var rect = (UnityEngine.RectTransform)section.transform;
            var grid = section.GetComponentsInChildren<UnityEngine.UI.GridLayoutGroup>(true).Single();
            var group = (UnityEngine.RectTransform)grid.transform;
            return new
            {
                title = section.GetComponentsInChildren<TMPro.TMP_Text>(true).Single(t => t.name == "BestiaryAreaText").text,
                x = rect.anchoredPosition.x, y = rect.anchoredPosition.y, width = rect.rect.width, height = rect.rect.height,
                gridWidth = group.rect.width, gridHeight = group.rect.height,
                boxes = section.GetComponentsInChildren<UI_BestiaryBox>(true).Where(b => b.DataId != 0).Select(box => new
                {
                    id = box.DataId,
                    x = ((UnityEngine.RectTransform)box.transform).anchoredPosition.x,
                    y = ((UnityEngine.RectTransform)box.transform).anchoredPosition.y
                }).ToArray()
            };
        }).ToArray();
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            contentHeight = scroll.content.rect.height, viewportHeight = scroll.viewport.rect.height, regions
        }, Newtonsoft.Json.Formatting.Indented);
        string output = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../Documents/Verification-20260909-Gameplay/commercial-20260910/bestiary-layout-before.json");
        System.IO.File.WriteAllText(output, json);
        UnityEngine.Debug.Log("[BestiaryLayout] Baseline saved: " + regions.Length + " regions, " + regions.Sum(r => r.boxes.Length) + " boxes, height " + scroll.content.rect.height);
    }
    finally
    {
        if (popup != null) Managers.UI.ClosePopupUI(popup);
        dataField.SetValue(game, original);
        pathField.SetValue(game, originalPath);
    }
}
CoroutineManager.StartCoroutine(Run());
return "BestiaryLayout started; wait for baseline log";
