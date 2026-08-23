using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 애니메이션 클립에 박힌 AnimationEvent를 받을 컴포넌트가 없어 나는
//  "has no receiver!" 경고를 없앤다.
//
//  대장간에서 망치질 몇 번에 로그가 수십 줄로 도배됐다("HammerNormalAnim ... 'Fail'").
//  이모지(MinimonEmoj/MonsterEmoj/TamerEmoj)도 같은 증상이었다.
//
//  방식: 클립이 부르는 메서드 이름을 모아, 그 클립을 쓰는 애니메이터 오브젝트에
//  AnimEventSink(빈 수신부)를 붙인다. 클립 자체는 건드리지 않는다 — 원본을
//  다시 받아오면 이벤트가 되살아나므로 받는 쪽을 두는 편이 재발하지 않는다.
//
//  여러 번 실행해도 안전하다. 이미 받을 수 있는 오브젝트는 건너뛴다.
public static class AnimEventWarningFixer
{
    // 우리가 쓰지 않는, 외부 클립에서 넘어온 이벤트 이름.
    static readonly string[] STRAY_EVENTS = { "TriggerUnityEvent" };

    [MenuItem("Tools/Fix Animation Event Warnings")]
    static void Fix()
    {
        // 1) 문제의 이벤트를 가진 클립을 모은다
        var badClips = new HashSet<string>();   // clip GUID
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            foreach (var e in AnimationUtility.GetAnimationEvents(clip))
                if (STRAY_EVENTS.Contains(e.functionName)) { badClips.Add(path); break; }
        }

        if (badClips.Count == 0)
        {
            Debug.Log("[AnimEventFix] 문제 클립 없음.");
            return;
        }

        // 2) 그 클립을 참조하는 컨트롤러를 찾는다
        var badControllers = new HashSet<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:AnimatorController"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (string dep in AssetDatabase.GetDependencies(path, true))
                if (badClips.Contains(dep)) { badControllers.Add(path); break; }
        }

        // 3) 그 컨트롤러를 쓰는 프리팹의 Animator에 수신부를 붙인다
        int patched = 0;
        var touched = new List<string>();

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/@Resources")) continue;

            // 의존성에 문제 컨트롤러/클립이 없으면 열지도 않는다 (프리팹 로드는 느리다)
            var deps = AssetDatabase.GetDependencies(path, true);
            if (!deps.Any(d => badControllers.Contains(d) || badClips.Contains(d))) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool dirty = false;
            try
            {
                foreach (var anim in root.GetComponentsInChildren<Animator>(true))
                {
                    var ctrl = anim.runtimeAnimatorController;
                    if (ctrl == null) continue;

                    string ctrlPath = AssetDatabase.GetAssetPath(ctrl);
                    if (!badControllers.Contains(ctrlPath)) continue;

                    // 이미 받을 수 있으면 그냥 둔다. AnimationEvent는 **같은 오브젝트의
                    //  아무 컴포넌트**에서 찾으므로, CreatureController처럼 이미 빈 메서드를
                    //  가진 게 붙어 있으면 중복해 달 필요가 없다.
                    if (CanReceive(anim.gameObject)) continue;

                    anim.gameObject.AddComponent<AnimEventSink>();
                    dirty = true;
                    patched++;
                    touched.Add($"{Path.GetFileNameWithoutExtension(path)} / {anim.gameObject.name}");
                }

                if (dirty) PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AnimEventFix] 문제 클립 {badClips.Count} / 컨트롤러 {badControllers.Count} / " +
                  $"수신부 부착 {patched}건\n  " + string.Join("\n  ", touched.Take(20)));
    }

    /// <summary>이 오브젝트가 이미 stray 이벤트를 받을 수 있는가.</summary>
    static bool CanReceive(GameObject go)
    {
        foreach (var mb in go.GetComponents<MonoBehaviour>())
        {
            if (mb == null) continue;
            var t = mb.GetType();
            foreach (string name in STRAY_EVENTS)
                if (t.GetMethod(name, System.Type.EmptyTypes) != null) return true;
        }
        return false;
    }
}
