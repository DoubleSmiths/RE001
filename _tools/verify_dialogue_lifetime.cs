// 플레이 모드에서 실행. 실제 대화 팝업의 수명과 긴 줄 → 스킵 → 짧은 줄 전환을 검사한다.
// python _tools/unity_exec.py _tools/verify_dialogue_lifetime.cs --allow-async
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play mode required");
if (Managers.UI.GetPopupCount() != 0) throw new System.Exception("Close existing popups before verification");
System.Collections.IEnumerator Run()
{
    var failures = new System.Collections.Generic.List<string>();
    UnityEngine.Application.LogCallback onLog = (message, trace, type) =>
    {
        bool problem = type == UnityEngine.LogType.Warning || type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception;
        if (problem && (message.Contains("DOTWEEN") || trace.Contains("DialogueAnimator"))) failures.Add(message);
    };
    UI_DialoguePopup popup = null;
    int staleOpeningCallbacks = 0;
    int normalOpeningCallbacks = 0;
    int longCompletions = 0;
    int shortCompletions = 0;
    UnityEngine.Application.logMessageReceived += onLog;
    try
    {
        // 같은 프레임에 닫으면 0.3초 뒤의 대사 시작 콜백도 실행되면 안 된다.
        popup = Managers.UI.ShowPopupUI<UI_DialoguePopup>();
        var animator = popup.GetComponent<DialogueAnimator>();
        var images = popup.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        var texts = popup.GetComponentsInChildren<TMPro.TMP_Text>(true);
        animator.PlayOpenSequence(images.First(x => x.name == "GradientImage"), images.First(x => x.name == "Panel"),
            images.First(x => x.name == "TextBoxDialogueFrame"), images.First(x => x.name == "CharacterImage").gameObject,
            texts.First(x => x.name == "DialogueName").gameObject, () => staleOpeningCallbacks++);
        animator.EnterPortrait(images.First(x => x.name == "CharacterImage"));
        Managers.UI.ClosePopupUI(popup);
        yield return new UnityEngine.WaitForSecondsRealtime(1.1f);
        if (popup != null) throw new System.Exception("Closed dialogue popup was not destroyed");
        if (staleOpeningCallbacks != 0) throw new System.Exception("Destroyed popup opening callback ran");
        if (failures.Count != 0) throw new System.Exception("Immediate close logged tween failure: " + failures[0]);

        // 살아 있는 팝업은 오프닝을 완료하고, 다음 줄을 정상적으로 타이핑해야 한다.
        popup = Managers.UI.ShowPopupUI<UI_DialoguePopup>();
        animator = popup.GetComponent<DialogueAnimator>();
        images = popup.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        texts = popup.GetComponentsInChildren<TMPro.TMP_Text>(true);
        var text = texts.First(x => x.name == "DialogueText");
        var arrow = images.First(x => x.name == "DialogueNextImage");
        animator.PlayOpenSequence(images.First(x => x.name == "GradientImage"), images.First(x => x.name == "Panel"),
            images.First(x => x.name == "TextBoxDialogueFrame"), images.First(x => x.name == "CharacterImage").gameObject,
            texts.First(x => x.name == "DialogueName").gameObject, () => normalOpeningCallbacks++);
        yield return new UnityEngine.WaitForSecondsRealtime(1.1f);
        if (normalOpeningCallbacks != 1) throw new System.Exception("Normal opening callback count: " + normalOpeningCallbacks);

        // 최소 몇 글자의 페이드가 겹친 상태에서 새 배열을 만들도록 테스트 인스턴스만 느리게 설정한다.
        typeof(DialogueAnimator).GetField("_typingSpeed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(animator, 0.1f);
        for (int i = 0; i < 3; i++)
        {
            animator.PlayTyping(text, "ABCDEFGHIJKLMN", Define.DialogueEmotion.Normal, () => longCompletions++);
            yield return new UnityEngine.WaitForSecondsRealtime(0.35f);
            if (animator.IsTypingDone) throw new System.Exception("Long line completed before skip");
            animator.SkipTyping(text);
            if (!animator.IsTypingDone || longCompletions != i + 1) throw new System.Exception("Skip did not complete exactly once");
            animator.PlayTyping(text, "A", Define.DialogueEmotion.Normal, () => shortCompletions++);
            // 이전 줄의 Tween이 실제로 업데이트될 수 있게 여러 프레임을 넘긴다.
            yield return new UnityEngine.WaitForSecondsRealtime(0.5f);
            if (!animator.IsTypingDone || shortCompletions != i + 1 || text.text != "A")
                throw new System.Exception("Short line did not complete normally");
            if (failures.Count != 0) throw new System.Exception("Skip/new line logged tween failure: " + failures[0]);
        }

        animator.PlayArrowLoop(arrow);
        yield return new UnityEngine.WaitForSecondsRealtime(0.2f);
        float runningAlpha = arrow.color.a;
        yield return new UnityEngine.WaitForSecondsRealtime(0.15f);
        if (UnityEngine.Mathf.Approximately(runningAlpha, arrow.color.a)) throw new System.Exception("Arrow loop did not animate");
        animator.StopArrowLoop(arrow);
        float stoppedAlpha = arrow.color.a;
        yield return new UnityEngine.WaitForSecondsRealtime(0.2f);
        if (!UnityEngine.Mathf.Approximately(stoppedAlpha, arrow.color.a)) throw new System.Exception("Arrow loop did not stop");

        Managers.UI.ClosePopupUI(popup);
        yield return new UnityEngine.WaitForSecondsRealtime(0.3f);
        if (popup != null || Managers.UI.GetPopupCount() != 0) throw new System.Exception("Dialogue popup cleanup failed");
        if (failures.Count != 0) throw new System.Exception("Dialogue lifetime logged tween failure: " + failures[0]);
        UnityEngine.Debug.Log("[DialogueLifetime] PASS: destroyed opening callback suppressed, normal opening works, 3 skip-to-short cycles complete without tween warnings, arrow starts/stops");
    }
    finally
    {
        UnityEngine.Application.logMessageReceived -= onLog;
        if (popup != null) Managers.UI.ClosePopupUI(popup);
    }
}
CoroutineManager.StartCoroutine(Run());
return "Dialogue lifetime verification started";
