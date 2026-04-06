using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

/// <summary>
/// UI_GameScene 하위의 레거시 Text 컴포넌트를 TextMeshProUGUI로 일괄 교체하는 에디터 툴
/// Tools > Convert Text to TMP in UI_GameScene
/// </summary>
public class TextToTMPConverter
{
    [MenuItem("Tools/Convert Text to TMP in UI_GameScene")]
    static void ConvertActiveOnly()
    {
        Convert("UI_GameScene");
    }

    [MenuItem("Tools/Convert Selected to TMP")]
    static void ConvertSelected()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("[TextToTMPConverter] 변환할 오브젝트를 선택하세요.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selected);

        if (!string.IsNullOrEmpty(assetPath))
        {
            // Project 뷰에서 선택한 프리펩 에셋
            GameObject contents = PrefabUtility.LoadPrefabContents(assetPath);
            int count = ConvertTexts(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, assetPath);
            PrefabUtility.UnloadPrefabContents(contents);
            AssetDatabase.Refresh();
            Debug.Log($"[TextToTMPConverter] '{selected.name}' Text → TMP 교체 완료: {count}개");
        }
        else
        {
            // Hierarchy에서 선택한 씬 오브젝트
            int count = ConvertTexts(selected);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[TextToTMPConverter] '{selected.name}' Text → TMP 교체 완료: {count}개");
        }
    }

    [MenuItem("Tools/Convert Selected to TMP", true)]
    static bool ConvertSelectedValidate() => Selection.activeGameObject != null;

    static void Convert(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null)
        {
            Debug.LogError($"[TextToTMPConverter] '{rootName}' 을(를) 씬에서 찾을 수 없습니다.");
            return;
        }
        int converted = ConvertTexts(root);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[TextToTMPConverter] '{rootName}' 하위 Text → TextMeshProUGUI 교체 완료: {converted}개");
    }

    static int ConvertTexts(GameObject root)
    {
        Text[] texts = root.GetComponentsInChildren<Text>(true);

        if (texts.Length == 0)
        {
            Debug.Log($"[TextToTMPConverter] '{root.name}' 하위에 레거시 Text 컴포넌트가 없습니다.");
            return 0;
        }

        int converted = 0;

        foreach (Text text in texts)
        {
            GameObject go = text.gameObject;

            // 기존 Text 속성 저장
            string content       = text.text;
            int    fontSize      = text.fontSize;
            Color  color         = text.color;
            bool   raycast       = text.raycastTarget;
            TextAnchor anchor    = text.alignment;
            FontStyle fontStyle  = text.fontStyle;
            bool   bestFit       = text.resizeTextForBestFit;
            int    minSize       = text.resizeTextMinSize;
            int    maxSize       = text.resizeTextMaxSize;
            bool   richText      = text.supportRichText;
            float  lineSpacing   = text.lineSpacing;

            // Undo 등록 후 Text 제거
            Undo.DestroyObjectImmediate(text);

            // TextMeshProUGUI 추가
            TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(go);

            // 속성 복사
            tmp.text            = content;
            tmp.fontSize        = fontSize;
            tmp.color           = color;
            tmp.raycastTarget   = raycast;
            tmp.alignment       = ConvertAlignment(anchor);
            tmp.fontStyle       = ConvertFontStyle(fontStyle);
            tmp.richText        = richText;
            tmp.lineSpacing     = lineSpacing;

            if (bestFit)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin      = minSize;
                tmp.fontSizeMax      = maxSize;
            }

            EditorUtility.SetDirty(go);
            converted++;
        }

        return converted;
    }

    static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:    return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:  return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:   return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:   return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:  return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft:    return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:  return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:   return TextAlignmentOptions.BottomRight;
            default:                      return TextAlignmentOptions.Center;
        }
    }

    static FontStyles ConvertFontStyle(FontStyle style)
    {
        switch (style)
        {
            case FontStyle.Bold:          return FontStyles.Bold;
            case FontStyle.Italic:        return FontStyles.Italic;
            case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
            default:                      return FontStyles.Normal;
        }
    }
}
