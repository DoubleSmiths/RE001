using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class PrefabChildRenamer : EditorWindow
{
    private GameObject _targetPrefab;
    private Vector2 _scroll;
    private string _preview = "";

    [MenuItem("Tools/Prefab Child Renamer")]
    public static void Open()
    {
        GetWindow<PrefabChildRenamer>("Prefab Child Renamer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("프리펩 하위 오브젝트 이름 정리", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("공백 제거 / 괄호 제거 / 나머지 기호 → _", MessageType.Info);

        EditorGUILayout.Space();
        _targetPrefab = (GameObject)EditorGUILayout.ObjectField("대상 프리펩", _targetPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        if (_targetPrefab != null)
        {
            if (GUILayout.Button("미리보기"))
                _preview = BuildPreview(_targetPrefab);

            if (!string.IsNullOrEmpty(_preview))
            {
                EditorGUILayout.LabelField("변경 미리보기:");
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(300));
                EditorGUILayout.TextArea(_preview, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("이름 변경 적용"))
                ApplyRename(_targetPrefab);
            GUI.backgroundColor = Color.white;
        }
    }

    private static string CleanName(string name)
    {
        // 괄호 제거
        name = name.Replace("(", "").Replace(")", "");
        // 공백 제거
        name = name.Replace(" ", "");
        // 영문자, 숫자, 한글, 언더스코어 외의 모든 기호를 _ 로 치환
        name = Regex.Replace(name, @"[^\w가-힣]", "_");
        // 연속된 _ 를 하나로
        name = Regex.Replace(name, @"_+", "_");
        // 앞뒤 _ 제거
        name = name.Trim('_');
        return name;
    }

    private string BuildPreview(GameObject root)
    {
        var sb = new System.Text.StringBuilder();
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t == root.transform) continue;
            string newName = CleanName(t.name);
            if (t.name != newName)
                sb.AppendLine($"{t.name}  →  {newName}");
        }
        if (sb.Length == 0)
            sb.AppendLine("변경할 이름이 없습니다.");
        return sb.ToString();
    }

    private void ApplyRename(GameObject root)
    {
        string assetPath = AssetDatabase.GetAssetPath(root);
        bool isPrefabAsset = !string.IsNullOrEmpty(assetPath);

        if (isPrefabAsset)
        {
            // 프리펩 에셋 직접 수정
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(assetPath);
            int count = RenameChildren(prefabContents);
            PrefabUtility.SaveAsPrefabAsset(prefabContents, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
            AssetDatabase.Refresh();
            _preview = "";
            Debug.Log($"[PrefabChildRenamer] {count}개 이름 변경 완료: {assetPath}");
            EditorUtility.DisplayDialog("완료", $"{count}개 오브젝트 이름을 변경했습니다.", "확인");
        }
        else
        {
            // 씬에 올라간 인스턴스
            int count = RenameChildren(root);
            _preview = "";
            Debug.Log($"[PrefabChildRenamer] {count}개 이름 변경 완료 (씬 인스턴스)");
            EditorUtility.DisplayDialog("완료", $"{count}개 오브젝트 이름을 변경했습니다.", "확인");
        }
    }

    private int RenameChildren(GameObject root)
    {
        int count = 0;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t == root.transform) continue;
            string newName = CleanName(t.name);
            if (t.name != newName)
            {
                t.name = newName;
                count++;
            }
        }
        return count;
    }
}
