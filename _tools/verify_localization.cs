// 테스트 세이브에서 실행: python _tools/unity_exec.py _tools/verify_localization.cs --allow-async
System.Collections.IEnumerator Run()
{
    var language = Managers.Game.Language;
    try
    {
        foreach (var lang in new[] { Define.Language.English, Define.Language.Korean })
        {
            Managers.Game.Language = lang;
            Managers.UI.CloseAllPopupUI();
            var detail = Managers.UI.ShowPopupUI<UI_EquipDetailPopup>();
            detail.SetInfo(new Equipment(30013));
            yield return new UnityEngine.WaitForSecondsRealtime(1f);
            var button = detail.GetComponentsInChildren<TMPro.TMP_Text>().First(t => t.name == "Txt_DontHaveBtn_03");
            button.ForceMeshUpdate();
            if (button.isTextOverflowing) throw new System.Exception("Craft reason clipped: " + lang);
            var owned = Managers.Game.OwnedEquipments.First();
            Managers.UI.CloseAllPopupUI();
            var forge = Managers.UI.ShowPopupUI<UI_ForgePopup>();
            forge.SetInfo(owned);
            yield return new UnityEngine.WaitForSecondsRealtime(1f);
            Check(forge, lang);
            Managers.UI.CloseAllPopupUI();
            var enhance = Managers.UI.ShowPopupUI<UI_EnhacePopup>();
            enhance.SetInfo(owned, Define.ENHANCE_SCROLL_IDS[0]);
            yield return new UnityEngine.WaitForSecondsRealtime(4f);
            Check(enhance, lang);
            Managers.UI.CloseAllPopupUI();
            var abilities = Managers.UI.ShowPopupUI<UI_ClassPopup>();
            abilities.SetInfo();
            yield return new UnityEngine.WaitForSecondsRealtime(1f);
            var title = abilities.GetComponentsInChildren<TMPro.TMP_Text>().First(t => t.name == "title");
            if (title.text != Util.Localize(Define.LocalizeKey.Class_Ability_Text)) throw new System.Exception("Class title untranslated");
        }
        UnityEngine.Debug.Log("[LocalizationVerify] PASS: Korean/English equipment, forge, enhance and class UI");
    }
    finally
    {
        Managers.UI.CloseAllPopupUI();
        Managers.Game.Language = language;
    }
}
void Check(UI_Popup popup, Define.Language language)
{
    if (language != Define.Language.English) return;
    foreach (var text in popup.GetComponentsInChildren<TMPro.TMP_Text>())
        if (System.Text.RegularExpressions.Regex.IsMatch(text.text, "[가-힣]"))
            throw new System.Exception(popup.name + "/" + text.name + " untranslated: " + text.text);
}
Managers.Game.Player.StartCoroutine(Run());
return "Localization verification started";
