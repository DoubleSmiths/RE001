// 플레이 모드에서 실행. 실제 팝업의 열림 직후/연출 중 닫기를 검증한다.
// python _tools/unity_exec.py _tools/verify_popup_lifetime.cs --allow-async
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play mode required");
System.Collections.IEnumerator Run()
{
    var warnings = new System.Collections.Generic.List<string>();
    var warningTrace = UnityEngine.Application.GetStackTraceLogType(UnityEngine.LogType.Warning);
    UnityEngine.Application.SetStackTraceLogType(UnityEngine.LogType.Warning, UnityEngine.StackTraceLogType.ScriptOnly);
    UnityEngine.Application.LogCallback onLog = (message, trace, type) =>
    {
        if (message.Contains("DOTWEEN") && (type == UnityEngine.LogType.Warning || type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)) warnings.Add(message + "\n" + trace);
    };
    var outside = new UnityEngine.GameObject("PopupLifetimeOutside");
    var outsideTween = DG.Tweening.ShortcutExtensions.DOScale(outside.transform, 2f, 20f);
    DG.Tweening.TweenSettingsExtensions.SetLoops(outsideTween, -1, DG.Tweening.LoopType.Yoyo);
    int closed = 0;
    int lateCallbacks = 0;
    System.Action<System.Type> onClosed = type => closed++;
    Managers.UI.CloseAllPopupUI();
    UnityEngine.Application.logMessageReceived += onLog;
    Managers.UI.OnPopupClosed += onClosed;
    try
    {
        for (int i = 0; i < 3; i++)
        {
            var book = Managers.UI.ShowPopupUI<UI_SkillBookPopup>();
            book.SetInfo();
            var detail = Managers.UI.ShowPopupUI<UI_SkillDetailPopup>();
            detail.SetInfo(6101);
            var img = detail.GetComponentsInChildren<UnityEngine.UI.Image>(true).First();
            bool closing = false;
            detail.FadeInOut(img, () => { if (closing) lateCallbacks++; });
            // 같은 프레임에 닫아 첫 프레임 로딩 지연으로 연출이 먼저 끝나는 경우를 배제한다.
            closing = true;
            Managers.UI.CloseAllPopupUI();
            yield return new UnityEngine.WaitForSecondsRealtime(1.2f);
        }
        if (!DG.Tweening.TweenExtensions.IsActive(outsideTween)) throw new System.Exception("Unrelated scene tween killed");
        if (closed != 6 || Managers.UI.GetPopupCount() != 0) throw new System.Exception("Popup close events/stack corrupted: " + closed);
        if (lateCallbacks != 0) throw new System.Exception("Closed popup completion callback ran: " + lateCallbacks);
        if (warnings.Count != 0) throw new System.Exception("Destroyed UI tween warnings: " + warnings.Count);
        var normal = Managers.UI.ShowPopupUI<UI_SkillDetailPopup>();
        normal.SetInfo(6101);
        yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
        typeof(UI_SkillDetailPopup).GetMethod("OnClickClose", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(normal, null);
        yield return new UnityEngine.WaitForSecondsRealtime(0.7f);
        if (Managers.UI.GetPopupCount() != 0 || closed != 7) throw new System.Exception("Normal reopen/close failed");
        if (warnings.Count != 0) throw new System.Exception("Normal close tween warnings: " + string.Join("\n", warnings));
        for (int i = 0; i < 2; i++)
        {
            UI_Popup closingPopup;
            if (i == 0)
            {
                var custom = Managers.UI.ShowPopupUI<UI_CustomOrderPopup>();
                custom.SetInfo();
                closingPopup = custom;
            }
            else
            {
                var potion = Managers.UI.ShowPopupUI<UI_PotionPopup>();
                potion.SetInfo();
                closingPopup = potion;
            }
            yield return new UnityEngine.WaitForSecondsRealtime(0.6f);
            closingPopup.GetType().GetMethod("OnClickClose", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(closingPopup, null);
            Managers.UI.CloseAllPopupUI();
            var replacement = Managers.UI.ShowPopupUI<UI_SkillDetailPopup>();
            replacement.SetInfo(6101);
            yield return new UnityEngine.WaitForSecondsRealtime(1.5f);
            if (replacement == null || Managers.UI.GetPopupCount() != 1) throw new System.Exception("Old close callback closed replacement popup: " + i);
            Managers.UI.CloseAllPopupUI();
        }
        if (warnings.Count != 0) throw new System.Exception("Cross-popup lifetime warnings: " + warnings.Count);
        UnityEngine.Debug.Log("[PopupLifetime] PASS: 3 immediate close cycles, no stale callbacks/warnings, unrelated tween alive, reopen/close works");
    }
    finally
    {
        Managers.UI.OnPopupClosed -= onClosed;
        UnityEngine.Application.logMessageReceived -= onLog;
        UnityEngine.Application.SetStackTraceLogType(UnityEngine.LogType.Warning, warningTrace);
        Managers.UI.CloseAllPopupUI();
        if (DG.Tweening.TweenExtensions.IsActive(outsideTween)) DG.Tweening.TweenExtensions.Kill(outsideTween, false);
        Managers.Resource.Destroy(outside);
    }
}
CoroutineManager.StartCoroutine(Run());
return "Popup lifetime verification started";
