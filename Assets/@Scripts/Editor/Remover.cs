using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RemoveScriptMachine
{
    // =======================================================================
    // 선택한 프리팹에서 ScriptMachine / Variables / StateMachine 제거
    // Project 창에서 프리팹을 하나 이상 선택하고 메뉴를 실행하면 됩니다.
    // 프리팹 내부의 모든 자식 오브젝트까지 검사합니다.
    // =======================================================================
    [MenuItem("Tools/Remove ScriptMachines And Variables (Selected Prefabs)")]
    static void RemoveFromSelectedPrefabs()
    {
        Object[] selected = Selection.objects;

        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("선택된 프리팹이 없습니다. Project 창에서 프리팹을 선택해 주세요.");
            return;
        }

        // 실제로 처리 가능한 프리팹 에셋만 수집
        List<GameObject> prefabAssets = new List<GameObject>();
        foreach (var obj in selected)
        {
            GameObject go = obj as GameObject;
            if (go == null)
                continue;

            string path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
                continue;

            // 프리팹 에셋인지 확인
            PrefabAssetType assetType = PrefabUtility.GetPrefabAssetType(go);
            if (assetType == PrefabAssetType.NotAPrefab ||
                assetType == PrefabAssetType.MissingAsset)
            {
                continue;
            }

            prefabAssets.Add(go);
        }

        if (prefabAssets.Count == 0)
        {
            Debug.LogWarning("선택된 오브젝트 중 프리팹 에셋이 없습니다.");
            return;
        }

        int totalScriptMachines = 0;
        int totalVariables = 0;
        int totalStateMachines = 0;
        int processedPrefabs = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < prefabAssets.Count; i++)
            {
                GameObject prefabAsset = prefabAssets[i];
                string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);

                EditorUtility.DisplayProgressBar(
                    "Remove ScriptMachines And Variables",
                    $"Processing: {prefabAsset.name}",
                    (float)i / prefabAssets.Count);

                // 프리팹 내용을 임시로 로드해서 편집
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                if (prefabRoot == null)
                {
                    Debug.LogWarning($"[Skip] 프리팹 로드 실패: {prefabPath}");
                    continue;
                }

                int smCount = 0;
                int varCount = 0;
                int stateCount = 0;

                // ScriptMachine 제거 (자식 포함)
                ScriptMachine[] machines = prefabRoot.GetComponentsInChildren<ScriptMachine>(true);
                foreach (var m in machines)
                {
                    if (m == null)
                        continue;
                    Object.DestroyImmediate(m, true);
                    smCount++;
                }

                // Variables 제거 (자식 포함)
                Variables[] vars = prefabRoot.GetComponentsInChildren<Variables>(true);
                foreach (var v in vars)
                {
                    if (v == null)
                        continue;
                    Object.DestroyImmediate(v, true);
                    varCount++;
                }

                // StateMachine 제거 (자식 포함)
                StateMachine[] sms = prefabRoot.GetComponentsInChildren<StateMachine>(true);
                foreach (var sm in sms)
                {
                    if (sm == null)
                        continue;
                    Object.DestroyImmediate(sm, true);
                    stateCount++;
                }

                // 변경사항 저장
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);

                totalScriptMachines += smCount;
                totalVariables += varCount;
                totalStateMachines += stateCount;
                processedPrefabs++;

                Debug.Log(
                    $"[{prefabAsset.name}] ScriptMachine: {smCount}, " +
                    $"Variables: {varCount}, StateMachine: {stateCount} 제거");
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log(
            $"완료: 프리팹 {processedPrefabs}개 처리 / " +
            $"ScriptMachine {totalScriptMachines}개, " +
            $"Variables {totalVariables}개, " +
            $"StateMachine {totalStateMachines}개 제거.");
    }

    [MenuItem("Tools/Remove All ScriptMachines And Variables")]
    static void RemoveAll()
    {
        ScriptMachine[] machines = Object.FindObjectsOfType<ScriptMachine>(true);

        int count = machines.Length;

        foreach (var m in machines)
        {
            Undo.DestroyObjectImmediate(m);
        }

        Debug.Log($"Removed {count} ScriptMachine components.");

        Variables[] variables = Object.FindObjectsOfType<Variables>(true);

        int count2 = variables.Length;

        foreach (var v in variables)
        {
            Undo.DestroyObjectImmediate(v);
        }

        Debug.Log($"Removed {count} variables components.");

        StateMachine[] sms = Object.FindObjectsOfType<StateMachine>(true);

        int smsCount = sms.Length;

        foreach (var sm in sms)
        {
            Undo.DestroyObjectImmediate(sm);
        }

        Debug.Log($"Removed {count} StateMachine components.");
    }


    // 필요하면 여기 기본 TMP 폰트 에셋을 넣어두면 된다.
    // 없으면 null로 둬도 컴포넌트 추가는 된다.
    private static TMP_FontAsset defaultFontAsset;

    [MenuItem("Tools/Convert/Replace Text to TextMeshProUGUI")]
    public static void ReplaceTextToTMP()
    {
        Text[] texts = Object.FindObjectsOfType<Text>(true);

        if (texts == null || texts.Length == 0)
        {
            Debug.Log("변환할 Unity Text가 없습니다.");
            return;
        }

        int convertedCount = 0;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        foreach (Text oldText in texts)
        {
            if (oldText == null)
                continue;

            GameObject go = oldText.gameObject;

            // 이미 TMP가 붙어있으면 스킵
            if (go.GetComponent<TextMeshProUGUI>() != null)
            {
                Debug.LogWarning($"[Skip] 이미 TMP가 존재함: {GetHierarchyPath(go)}");
                continue;
            }

            // 값 백업
            string oldString = oldText.text;
            Color oldColor = oldText.color;
            int oldFontSize = oldText.fontSize;
            FontStyle oldFontStyle = oldText.fontStyle;
            TextAnchor oldAlignment = oldText.alignment;
            bool oldRaycastTarget = oldText.raycastTarget;
            bool oldRichText = oldText.supportRichText;
            float oldLineSpacing = oldText.lineSpacing;
            HorizontalWrapMode oldHorizontalOverflow = oldText.horizontalOverflow;
            VerticalWrapMode oldVerticalOverflow = oldText.verticalOverflow;
            bool oldBestFit = oldText.resizeTextForBestFit;
            int oldBestFitMin = oldText.resizeTextMinSize;
            int oldBestFitMax = oldText.resizeTextMaxSize;

            Undo.RecordObject(go, "Replace Text to TMP");

            // TMP 추가
            TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(go);

            // 기본 값 복사
            tmp.text = oldString;
            tmp.color = oldColor;
            tmp.fontSize = oldFontSize;
            tmp.raycastTarget = oldRaycastTarget;
            tmp.richText = oldRichText;
            tmp.lineSpacing = oldLineSpacing;

            if (defaultFontAsset != null)
            {
                tmp.font = defaultFontAsset;
            }

            // 스타일 매핑
            ApplyFontStyle(tmp, oldFontStyle);

            // 정렬 매핑
            tmp.alignment = ConvertAlignment(oldAlignment);

            // 줄바꿈 / overflow 매핑
            ApplyOverflow(tmp, oldHorizontalOverflow, oldVerticalOverflow);

            // Best Fit -> Auto Size 대응
            if (oldBestFit)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = oldBestFitMin;
                tmp.fontSizeMax = oldBestFitMax;
            }
            else
            {
                tmp.enableAutoSizing = false;
            }

            // 기존 Text 제거
            Undo.DestroyObjectImmediate(oldText);

            EditorUtility.SetDirty(go);
            convertedCount++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"Replace Text to TextMeshProUGUI 완료: {convertedCount}개 변환");
    }

    [MenuItem("Tools/Convert/Replace Text to TextMeshProUGUI (Selected Objects Only)")]
    public static void ReplaceSelectedTextToTMP()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("선택된 오브젝트가 없습니다.");
            return;
        }

        int convertedCount = 0;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject root in selectedObjects)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);

            foreach (Text oldText in texts)
            {
                if (oldText == null)
                    continue;

                GameObject go = oldText.gameObject;

                if (go.GetComponent<TextMeshProUGUI>() != null)
                {
                    Debug.LogWarning($"[Skip] 이미 TMP가 존재함: {GetHierarchyPath(go)}");
                    continue;
                }

                string oldString = oldText.text;
                Color oldColor = oldText.color;
                int oldFontSize = oldText.fontSize;
                FontStyle oldFontStyle = oldText.fontStyle;
                TextAnchor oldAlignment = oldText.alignment;
                bool oldRaycastTarget = oldText.raycastTarget;
                bool oldRichText = oldText.supportRichText;
                float oldLineSpacing = oldText.lineSpacing;
                HorizontalWrapMode oldHorizontalOverflow = oldText.horizontalOverflow;
                VerticalWrapMode oldVerticalOverflow = oldText.verticalOverflow;
                bool oldBestFit = oldText.resizeTextForBestFit;
                int oldBestFitMin = oldText.resizeTextMinSize;
                int oldBestFitMax = oldText.resizeTextMaxSize;

                Undo.RecordObject(go, "Replace Selected Text to TMP");

                TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(go);

                tmp.text = oldString;
                tmp.color = oldColor;
                tmp.fontSize = oldFontSize;
                tmp.raycastTarget = oldRaycastTarget;
                tmp.richText = oldRichText;
                tmp.lineSpacing = oldLineSpacing;

                if (defaultFontAsset != null)
                {
                    tmp.font = defaultFontAsset;
                }

                ApplyFontStyle(tmp, oldFontStyle);
                tmp.alignment = ConvertAlignment(oldAlignment);
                ApplyOverflow(tmp, oldHorizontalOverflow, oldVerticalOverflow);

                if (oldBestFit)
                {
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = oldBestFitMin;
                    tmp.fontSizeMax = oldBestFitMax;
                }
                else
                {
                    tmp.enableAutoSizing = false;
                }

                Undo.DestroyObjectImmediate(oldText);

                EditorUtility.SetDirty(go);
                convertedCount++;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"선택 오브젝트 기준 TMP 변환 완료: {convertedCount}개");
    }

    private static void ApplyFontStyle(TextMeshProUGUI tmp, FontStyle fontStyle)
    {
        switch (fontStyle)
        {
            case FontStyle.Normal:
                tmp.fontStyle = FontStyles.Normal;
                break;

            case FontStyle.Bold:
                tmp.fontStyle = FontStyles.Bold;
                break;

            case FontStyle.Italic:
                tmp.fontStyle = FontStyles.Italic;
                break;

            case FontStyle.BoldAndItalic:
                tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
                break;
        }
    }

    private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                return TextAlignmentOptions.TopLeft;

            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;

            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;

            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.Left;

            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;

            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.Right;

            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;

            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;

            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
        }

        return TextAlignmentOptions.Center;
    }

    private static void ApplyOverflow(
        TextMeshProUGUI tmp,
        HorizontalWrapMode horizontal,
        VerticalWrapMode vertical)
    {
        // horizontal wrap
        if (horizontal == HorizontalWrapMode.Wrap)
        {
            tmp.enableWordWrapping = true;
        }
        else
        {
            tmp.enableWordWrapping = false;
        }

        // vertical overflow
        if (vertical == VerticalWrapMode.Overflow)
        {
            tmp.overflowMode = TextOverflowModes.Overflow;
        }
        else
        {
            tmp.overflowMode = TextOverflowModes.Truncate;
        }
    }

    private static string GetHierarchyPath(GameObject obj)
    {
        if (obj == null)
            return "(null)";

        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    [MenuItem("Tools/TMP/Apply Auto Height To All TextMeshProUGUI In Scene")]
    public static void ApplyAutoHeightToAllTMPInScene()
    {
        TextMeshProUGUI[] tmps = Object.FindObjectsOfType<TextMeshProUGUI>(true);

        if (tmps == null || tmps.Length == 0)
        {
            Debug.Log("현재 씬에 TextMeshProUGUI가 없습니다.");
            return;
        }

        int changedCount = 0;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        foreach (var tmp in tmps)
        {
            if (tmp == null)
                continue;

            GameObject go = tmp.gameObject;

            Undo.RecordObject(tmp, "Apply TMP Auto Height");
            Undo.RecordObject(go, "Apply TMP Auto Height");

            // 1. 줄바꿈 ON
            tmp.enableWordWrapping = true;

            // 2. Overflow 설정
            tmp.overflowMode = TextOverflowModes.Overflow;

            // 3. Auto Size는 꺼두는 편이 일반적
            tmp.enableAutoSizing = false;

            // 4. Content Size Fitter 추가 또는 가져오기
            ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = Undo.AddComponent<ContentSizeFitter>(go);
            }
            else
            {
                Undo.RecordObject(fitter, "Apply TMP Auto Height");
            }

            // 5. 세로는 고정, 가로만 Preferred Size
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; 
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            EditorUtility.SetDirty(tmp);
            EditorUtility.SetDirty(go);
            EditorUtility.SetDirty(fitter);

            changedCount++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"현재 씬의 TextMeshProUGUI {changedCount}개에 자동 높이 설정을 적용했습니다.");
    }

}

