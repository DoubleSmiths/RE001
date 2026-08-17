using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 고블린 컨트롤러에 Run 상태를 만들어 붙인다.
///
/// 고블린은 아키타입이 아니라 자체 아트(goblin_monster.png)를 쓰는데, 컨트롤러에
/// Idle/Attack/OnHit/Die 넷만 있고 **Run이 없다**. CrossFade("Run")은 조용히 무시되므로
/// 다가오는 내내 정지한 아이들 자세로 미끄러져 온다.
///
/// 시트에 별도 걷기 프레임은 없다. 아이들 프레임을 더 빠르게 돌려 이동감을 준다 —
/// 새로 그리는 것보다 정확하고(같은 아트), 정지해 보이는 것보다 낫다.
/// </summary>
public static class GoblinRunClipGenerator
{
    const string CTRL = "Assets/@Resources/Animation/Monster/Goblin/Goblin.controller";
    const string OUT = "Assets/@Resources/Animation/Monster/Goblin/Goblin_Run.anim";

    [MenuItem("Tools/고블린 Run 클립 생성")]
    public static void Generate()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(CTRL);
        if (ctrl == null) { Debug.LogError($"[GoblinRun] 컨트롤러 없음: {CTRL}"); return; }

        // 아이들 클립의 스프라이트를 그대로 쓴다(같은 아트라 튀지 않는다).
        AnimationClip idle = null;
        foreach (var c in ctrl.animationClips)
            if (c.name == "Goblin_Idle") { idle = c; break; }
        if (idle == null) { Debug.LogError("[GoblinRun] Goblin_Idle 없음"); return; }

        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(idle);
        if (bindings.Length == 0) { Debug.LogError("[GoblinRun] Goblin_Idle에 스프라이트 커브 없음"); return; }
        var keys = AnimationUtility.GetObjectReferenceCurve(idle, bindings[0]);
        if (keys.Length == 0) { Debug.LogError("[GoblinRun] 키프레임 없음"); return; }

        // 아이들보다 빠르게 — 걷는 느낌.
        float length = Mathf.Max(0.2f, idle.length * 0.6f);
        float step = length / keys.Length;

        var run = new AnimationClip { frameRate = idle.frameRate };
        var newKeys = new ObjectReferenceKeyframe[keys.Length];
        for (int i = 0; i < keys.Length; i++)
            newKeys[i] = new ObjectReferenceKeyframe { time = i * step, value = keys[i].value };
        AnimationUtility.SetObjectReferenceCurve(run, bindings[0], newKeys);

        var settings = AnimationUtility.GetAnimationClipSettings(run);
        settings.loopTime = true;   // 다가오는 동안 계속 돈다
        AnimationUtility.SetAnimationClipSettings(run, settings);
        run.name = "Goblin_Run";

        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(OUT);
        if (existing != null) EditorUtility.CopySerialized(run, existing);
        else AssetDatabase.CreateAsset(run, OUT);
        AssetDatabase.SaveAssets();

        var saved = AssetDatabase.LoadAssetAtPath<AnimationClip>(OUT);

        // 상태 이름은 코드가 CrossFade로 부르는 "Run"과 정확히 같아야 한다.
        var sm = ctrl.layers[0].stateMachine;
        AnimatorState st = null;
        foreach (var s in sm.states)
            if (s.state.name == "Run") { st = s.state; break; }
        if (st == null) st = sm.AddState("Run");
        st.motion = saved;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[GoblinRun] Run 상태 추가 — 클립 {saved.length:F2}s ({newKeys.Length}프레임)");
    }
}
