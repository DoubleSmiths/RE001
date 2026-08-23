using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

// "This DOTweenAnimation's target is NULL, because the animation was created with a
//  DOTween Pro version older than 0.9.255" 경고를 없앤다.
//
//  이건 단순한 잔소리가 아니다 — target이 null이면 그 애니메이션은 **재생 자체가 안 된다**.
//  대장간 로그에서 Txt_Xbutton_01 / AttText / Txt_FailTxtGroup_* 등이 그랬다.
//
//  DOTween이 안내하는 해법은 "플레이 모드를 나가서 그 오브젝트를 클릭하라"다.
//  인스펙터가 열릴 때 Validate()가 돌면서 target을 채워 주기 때문인데,
//  수십 개를 손으로 클릭할 수는 없다. 그 Validate가 쓰는 **DOTween 자신의 매핑 표**를
//  리플렉션으로 꺼내 같은 일을 프리팹 전체에 한 번에 한다
//  (표를 우리가 새로 적으면 DOTween 판올림 때 조용히 어긋난다).
//
//  여러 번 실행해도 안전하다. target이 이미 있으면 건너뛴다.
public static class DOTweenAnimTargetFixer
{
    [MenuItem("Tools/Fix DOTweenAnimation NULL Targets")]
    static void Fix()
    {
        var maps = LoadDOTweenMaps();
        if (maps.Count == 0)
        {
            Debug.LogError("[DOTweenFix] DOTweenAnimationInspector의 매핑 표를 찾지 못했습니다. " +
                           "DOTween Pro 버전이 바뀌었을 수 있습니다.");
            return;
        }

        int fixedCount = 0, skipped = 0;
        var touched = new List<string>();

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/@Resources")) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool dirty = false;
            try
            {
                foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                {
                    // 경고가 나는 조건과 똑같이 본다 (DOTweenAnimation.cs:205)
                    if (!anim.targetIsSelf || anim.target != null) continue;

                    if (Assign(anim, maps)) { dirty = true; fixedCount++; touched.Add($"{Path.GetFileNameWithoutExtension(path)} / {anim.gameObject.name} ({anim.animationType})"); }
                    else skipped++;
                }

                if (dirty) PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[DOTweenFix] target 복구 {fixedCount}건 / 대상 컴포넌트를 못 찾아 건너뜀 {skipped}건\n  "
                  + string.Join("\n  ", touched.Take(25))
                  + (touched.Count > 25 ? $"\n  … 외 {touched.Count - 25}건" : ""));
    }

    /// <summary>DOTweenAnimationInspector의 private static 매핑 표를 꺼낸다.
    /// (animationType -> 이 애니를 받을 수 있는 컴포넌트 후보들)</summary>
    static List<Dictionary<DOTweenAnimation.AnimationType, Type[]>> LoadDOTweenMaps()
    {
        var result = new List<Dictionary<DOTweenAnimation.AnimationType, Type[]>>();

        Type inspector = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .FirstOrDefault(t => t.Name == "DOTweenAnimationInspector");
        if (inspector == null) return result;

        // TMP 표를 먼저 본다 — 인스펙터도 그 순서로 찾는다(글자 컴포넌트가 우선).
        foreach (string name in new[] { "_TMPAnimationTypeToComponent", "_AnimationTypeToComponent" })
        {
            var f = inspector.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            if (f?.GetValue(null) is Dictionary<DOTweenAnimation.AnimationType, Type[]> map) result.Add(map);
        }
        return result;
    }

    /// <summary>DOTweenAnimationInspector.Validate와 같은 규칙으로 target/targetType을 채운다.</summary>
    static bool Assign(DOTweenAnimation anim, List<Dictionary<DOTweenAnimation.AnimationType, Type[]>> maps)
    {
        if (anim.animationType == DOTweenAnimation.AnimationType.None) return false;

        GameObject go = anim.gameObject;
        foreach (var map in maps)
        {
            if (!map.TryGetValue(anim.animationType, out Type[] candidates)) continue;

            foreach (Type t in candidates)
            {
                Component c = go.GetComponent(t);
                if (c == null) continue;

                anim.target = c;
                anim.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                EditorUtility.SetDirty(anim);
                return true;
            }
        }
        return false;
    }
}
