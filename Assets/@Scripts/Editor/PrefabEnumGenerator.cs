using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class PrefabEnumGenerator : EditorWindow
{
    private GameObject selectedPrefab;
    private string outputFileName = "UIEnums";
    private string prefabFolderPath = "Assets/Prefabs";
    private Vector2 scrollPos;

    private List<string> imageNames = new List<string>();
    private List<string> textNames = new List<string>();
    private List<string> buttonNames = new List<string>();
    private List<string> gameObjectNames = new List<string>();

    private string previewText = "";

    [MenuItem("Tools/Prefab Enum Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabEnumGenerator>("Prefab Enum Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Enum Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path", prefabFolderPath);
        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", selectedPrefab, typeof(GameObject), false);
        outputFileName = EditorGUILayout.TextField("Output File Name", outputFileName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Analyze Prefab"))
        {
            if (selectedPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "프리펩을 선택해주세요.", "OK");
                return;
            }
            AnalyzePrefab();
        }

        EditorGUILayout.Space();

        if (!string.IsNullOrEmpty(previewText))
        {
            GUILayout.Label("Preview:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            EditorGUILayout.TextArea(previewText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Save to File"))
            {
                SaveToFile();
            }
        }
    }

    private void AnalyzePrefab()
    {
        imageNames.Clear();
        textNames.Clear();
        buttonNames.Clear();
        gameObjectNames.Clear();

        CollectComponents(selectedPrefab.transform);

        RemoveDuplicatesAcrossAll();

        previewText = BuildEnumText();
        Repaint();
    }

    private void RemoveDuplicatesAcrossAll()
    {
        var allLists = new List<List<string>> { imageNames, textNames, buttonNames, gameObjectNames };

        // 전체 목록에서 2번 이상 등장하는 이름 수집
        var countMap = new Dictionary<string, int>();
        foreach (var list in allLists)
            foreach (var name in list)
                countMap[name] = countMap.ContainsKey(name) ? countMap[name] + 1 : 1;

        var duplicates = new HashSet<string>();
        foreach (var kv in countMap)
            if (kv.Value > 1)
                duplicates.Add(kv.Key);

        // 중복된 이름을 모든 목록에서 제거
        foreach (var list in allLists)
            list.RemoveAll(name => duplicates.Contains(name));
    }

    private void CollectComponents(Transform transform)
    {
        GameObject go = transform.gameObject;
        string name = go.name;

        bool categorized = false;

        // Button 먼저 체크 (Button은 Image도 있으므로 우선순위 높게)
        if (go.GetComponent<Button>() != null)
        {
            buttonNames.Add(name);
            categorized = true;
        }
        else if (go.GetComponent<Image>() != null || go.GetComponent<RawImage>() != null)
        {
            imageNames.Add(name);
            categorized = true;
        }
        else if (go.GetComponent<Text>() != null
              || go.GetComponent<TextMeshProUGUI>() != null
              || go.GetComponent<TMP_Text>() != null)
        {
            textNames.Add(name);
            categorized = true;
        }

        if (!categorized && go != selectedPrefab)
        {
            gameObjectNames.Add(name);
        }

        foreach (Transform child in transform)
        {
            CollectComponents(child);
        }
    }

    private string BuildEnumText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("#region Enum");
        sb.AppendLine();

        sb.AppendLine("enum Images");
        sb.AppendLine("{");
        foreach (string name in imageNames)
            sb.AppendLine($"    {SanitizeName(name)},");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("enum Texts");
        sb.AppendLine("{");
        foreach (string name in textNames)
            sb.AppendLine($"    {SanitizeName(name)},");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("enum Buttons");
        sb.AppendLine("{");
        foreach (string name in buttonNames)
            sb.AppendLine($"    {SanitizeName(name)},");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("enum GameObjects");
        sb.AppendLine("{");
        foreach (string name in gameObjectNames)
            sb.AppendLine($"    {SanitizeName(name)},");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("#endregion");

        return sb.ToString();
    }

    private string SanitizeName(string name)
    {
        // 공백, 특수문자 제거 후 유효한 식별자로 변환
        StringBuilder sb = new StringBuilder();
        bool nextUpper = false;

        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(nextUpper ? char.ToUpper(c) : c);
                nextUpper = false;
            }
            else
            {
                nextUpper = true;
            }
        }

        string result = sb.ToString();

        // 숫자로 시작하면 앞에 _ 붙이기
        if (result.Length > 0 && char.IsDigit(result[0]))
            result = "_" + result;

        return result;
    }

    private void SaveToFile()
    {
        string savePath = EditorUtility.SaveFilePanel(
            "Save Enum File",
            Application.dataPath,
            outputFileName,
            "txt"
        );

        if (string.IsNullOrEmpty(savePath))
            return;

        File.WriteAllText(savePath, previewText, Encoding.UTF8);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"파일이 저장되었습니다:\n{savePath}", "OK");
        Debug.Log($"[PrefabEnumGenerator] Saved: {savePath}");
    }
}
