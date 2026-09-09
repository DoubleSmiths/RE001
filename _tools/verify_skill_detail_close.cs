// 저장 경로를 비활성화한 마을 세션에서 닫기 도중 상세 갱신이 종료 콜백을 취소하지 않는지 검사한다.
if (!UnityEditor.EditorApplication.isPlaying || Managers.UI.GetPopupCount() != 0)
    throw new System.Exception("Play mode with empty popup stack required");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
if (typeof(GameManager).GetField("_path", flags).GetValue(Managers.Game) != null)
    throw new System.Exception("Isolated save path required");
System.Collections.IEnumerator Run()
{
    foreach (string refresh in new[] { "OnClickNextLvl", "RefreshAllStates" })
    {
        var popup = Managers.UI.ShowPopupUI<UI_SkillDetailPopup>();
        popup.SetInfo(5004);
        yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
        typeof(UI_SkillDetailPopup).GetMethod("OnClickClose", flags).Invoke(popup, null);
        yield return null;
        typeof(UI_SkillDetailPopup).GetMethod(refresh, flags).Invoke(popup, null);
        yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
        if (popup != null || Managers.UI.GetPopupCount() != 0)
        {
            Managers.UI.CloseAllPopupUI();
            throw new System.Exception("SkillDetailClose: closing cancelled by " + refresh);
        }
    }
    UnityEngine.Debug.Log("PASS: SkillDetailClose next-preview and acquisition-refresh preserve pending close");
}
Managers.Game.Player.StartCoroutine(Run());
return "Skill detail close verification started";
