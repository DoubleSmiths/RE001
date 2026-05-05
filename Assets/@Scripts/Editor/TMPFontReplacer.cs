using TMPro;
using UnityEditor;
using UnityEngine;

public class TMPFontReplacer : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/TMP/Replace Missing TMP Fonts In Selected Prefabs")]
    public static void Open()
    {
        GetWindow<TMPFontReplacer>("TMP Font Replacer");
    }

    private void OnGUI()
    {
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "New TMP Font Asset",
            newFont,
            typeof(TMP_FontAsset),
            false
        );

        if (GUILayout.Button("Replace In Selected Prefabs"))
        {
            Replace();
        }
    }

    private void Replace()
    {
        if (newFont == null)
        {
            Debug.LogError("New TMP Font Asset을 넣어주세요.");
            return;
        }

        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                continue;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

            var texts = prefabRoot.GetComponentsInChildren<TMP_Text>(true);
            int count = 0;

            foreach (var text in texts)
            {
                if (text.font == null)
                {
                    Undo.RecordObject(text, "Replace Missing TMP Font");
                    text.font = newFont;
                    EditorUtility.SetDirty(text);
                    count++;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log($"{path}: missing TMP font {count}개 교체 완료");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}