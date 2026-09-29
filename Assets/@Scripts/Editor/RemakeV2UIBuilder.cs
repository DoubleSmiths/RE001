#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// v2 재설계(ADR0030) UI 를 기존 프리팹의 그림으로 조립한다 — 손으로 만든 창이 아니라 다시 돌려도 같은 결과가 나오게.
//  훈련 패널: 상태 창(UI_CharacterPopup)의 CharacterPopupFrame 을 복제해 TrainingFrame 을 만들고,
//  '능력 상세정보' 긴 버튼을 4줄로 복제한다. 자동 바인딩은 이름으로 걸리므로 복제본의 노드 이름은 전부 고유하게 바꾼다.
public static class RemakeV2UIBuilder
{
    const string CHARACTER_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab";
    static readonly string[] TRAIN_PREFIX = { "ATK", "HP", "DEF", "CRIT" };   // TrainingStat 순서
    const float ROW_WIDTH = 145f, ROW_HEIGHT = 16f, ROW_SPACING = 3f, TEXT_INSET = 6f;

    [MenuItem("Tools/Remake V2/Build Training Panel")]
    public static void BuildTrainingPanel()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(CHARACTER_POPUP);
        try
        {
            Transform panel = Find(root.transform, "Panel_CharacterPopup_01");
            Transform old = Find(root.transform, "TrainingFrame", false);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            Transform src = Find(root.transform, "CharacterPopupFrame");
            Transform frame = Object.Instantiate(src.gameObject, panel).transform;
            frame.name = "TrainingFrame";
            frame.SetSiblingIndex(src.GetSiblingIndex() + 1);

            Rename(frame, "Panel_CharacterPopupFrame_02", "Panel_TrainingFrame_02");
            Rename(frame, "Img_CharacterPopupTitle_03", "Img_TrainingTitle_03");
            Rename(frame, "Img_CharacterPopupTitle_01", "Img_TrainingTitle_01");
            Rename(frame, "title", "TrainingTitleText");

            Transform contents = Find(frame, "Contents");
            contents.name = "TrainingContents";
            for (int i = contents.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(contents.GetChild(i).gameObject);

            // 상한 표시 — 원본의 '달빛영약' 글씨(오른쪽 위, 하늘색) 모양을 그대로 쓴다
            var cap = (RectTransform)Object.Instantiate(Find(src, "ElixirText").gameObject, contents).transform;
            cap.name = "TrainingCapTxt";
            cap.anchoredPosition = new Vector2(-8f, 0.5f);

            var list = new GameObject("TrainingList", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            list.SetParent(contents, false);
            list.anchorMin = Vector2.zero;
            list.anchorMax = Vector2.one;
            list.offsetMin = new Vector2(4f, 4f);
            list.offsetMax = new Vector2(-4f, -14f);
            var vlg = list.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = ROW_SPACING;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = vlg.childControlHeight = false;
            vlg.childForceExpandWidth = vlg.childForceExpandHeight = false;

            Transform rowSrc = Find(src, "StatDetailBtn");
            foreach (string p in TRAIN_PREFIX)
            {
                var row = (RectTransform)Object.Instantiate(rowSrc.gameObject, list).transform;
                row.name = p + "TrainBtn";
                row.sizeDelta = new Vector2(ROW_WIDTH, ROW_HEIGHT);
                Rename(row, "StatDetailBtnBackgroundImage", p + "TrainBtnBg");
                Rename(row, "StatDetailBtnMask", p + "TrainBtnMask");
                Rename(row, "StatDetailBtnGrow", p + "TrainBtnGrow");
                Find(row, p + "TrainBtnMask").gameObject.SetActive(false);

                var label = (RectTransform)Find(row, "Text");
                label.name = p + "TrainTxt";
                Align(label, new Vector2(0f, 0.5f), TEXT_INSET, TextAlignmentOptions.MidlineLeft);

                var cost = (RectTransform)Object.Instantiate(label.gameObject, row).transform;
                cost.name = p + "CostTxt";
                Align(cost, new Vector2(1f, 0.5f), -TEXT_INSET, TextAlignmentOptions.MidlineRight);
            }

            if (frame.GetComponent<UI_TrainingPanel>() == null)
                frame.gameObject.AddComponent<UI_TrainingPanel>();

            PrefabUtility.SaveAsPrefabAsset(root, CHARACTER_POPUP);
            Debug.Log("[RemakeV2] 훈련 패널 조립 완료 — UI_CharacterPopup/TrainingFrame (4줄)");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void Align(RectTransform rt, Vector2 anchor, float x, TextAlignmentOptions alignment)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = new Vector2(x, 0.5f);
        rt.GetComponent<TMP_Text>().alignment = alignment;
    }

    static Transform Find(Transform root, string name, bool required = true)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        if (required) throw new System.Exception($"[RemakeV2] '{name}' 없음 (프리팹 구조가 바뀌었다)");
        return null;
    }

    static void Rename(Transform root, string from, string to) => Find(root, from).name = to;
}
#endif
