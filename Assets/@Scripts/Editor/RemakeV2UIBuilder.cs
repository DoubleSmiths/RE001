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

    // 스테이지 막대: HUD(UI_GameScene) 게임 화면 왼쪽 위 — 버프 줄(위에서 24~34) 바로 아래, 퀘스트 추적기(x 122~) 왼쪽,
    //  보스 HP 바(1.3배, 위에서 50~) 위. 높이 14 안에 이름·게이지 두 줄.
    //  배경은 퀘스트 추적기의 줄 그림, 게이지는 플레이어 EXP 막대, 보스 타이머는 HP 막대 채움을 복제한다.
    const string GAME_SCENE = "Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab";
    static readonly Vector2 STAGE_BAR_POS  = new Vector2(2f, -32f);
    static readonly Vector2 STAGE_BAR_SIZE = new Vector2(104f, 14f);

    [MenuItem("Tools/Remake V2/Build Stage Bar")]
    public static void BuildStageBar()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GAME_SCENE);
        try
        {
            Transform frame = Find(root.transform, "In-Frame");
            Transform old = Find(root.transform, "StageBar", false);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var bar = new GameObject("StageBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            bar.SetParent(frame, false);
            bar.anchorMin = bar.anchorMax = bar.pivot = new Vector2(0f, 1f);
            bar.anchoredPosition = STAGE_BAR_POS;
            bar.sizeDelta = STAGE_BAR_SIZE;
            var bg = bar.GetComponent<Image>();
            bg.sprite = Find(root.transform, "Img_Main_QuestFollow_01").GetComponent<Image>().sprite;
            bg.color = new Color(0f, 0f, 0f, 0.45f);

            var name = (RectTransform)Object.Instantiate(Find(root.transform, "Quest_NeedItemNameTxt").gameObject, bar).transform;
            name.name = "StageNameTxt";
            name.anchorMin = new Vector2(0f, 1f);
            name.anchorMax = Vector2.one;
            name.pivot = new Vector2(0.5f, 1f);
            name.anchoredPosition = new Vector2(0f, -0.8f);
            name.sizeDelta = new Vector2(-6f, 5.5f);
            var nameTmp = name.GetComponent<TMP_Text>();
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.enableAutoSizing = true;   // 긴 영문 지역명(“Collapsed Tunnel Depths”)도 한 줄에
            nameTmp.fontSizeMin = 4f;
            nameTmp.fontSizeMax = 6f;

            var gauge = (RectTransform)Object.Instantiate(Find(root.transform, "PlayerEXPBarGroup").gameObject, bar).transform;
            gauge.name = "StageGauge";
            gauge.anchorMin = Vector2.zero;
            gauge.anchorMax = new Vector2(1f, 0f);
            gauge.pivot = new Vector2(0.5f, 0f);
            gauge.anchoredPosition = new Vector2(0f, 0.8f);
            gauge.sizeDelta = new Vector2(-6f, 6.2f);
            Rename(gauge, "PlayerEXPBarFrame", "StageGaugeFrame");
            Rename(gauge, "PlayerEXPBarGuageBox", "StageGaugeBox");
            Rename(gauge, "PlayerEXPBarGuage", "StageGaugeFill");
            Object.DestroyImmediate(Find(gauge, "PlayerEXPTextFlat").gameObject);

            // 보스 제한 시간 — HP 막대의 빨간 채움을 같은 칸에 겹쳐 두고 보스전에만 켠다
            Transform box = Find(gauge, "StageGaugeBox");
            var bossFill = (RectTransform)Object.Instantiate(Find(root.transform, "PlayerHPBarGuage").gameObject, box).transform;
            bossFill.name = "StageGaugeBossFill";
            bossFill.anchorMin = Vector2.zero;
            bossFill.anchorMax = Vector2.one;
            bossFill.anchoredPosition = Vector2.zero;
            bossFill.sizeDelta = Vector2.zero;
            bossFill.gameObject.SetActive(false);

            var count = (RectTransform)Find(gauge, "PlayerEXPTextPercent");
            count.name = "StageCountTxt";
            count.SetAsLastSibling();   // 채움 위에 그린다
            count.anchorMin = Vector2.zero;
            count.anchorMax = Vector2.one;
            count.pivot = new Vector2(0.5f, 0.5f);
            count.anchoredPosition = Vector2.zero;
            count.sizeDelta = Vector2.zero;
            var countTmp = count.GetComponent<TMP_Text>();
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.fontSize = 6f;

            // 원본에 딸린 동작 부품은 떼어 낸다 — ContentSizeFitter 는 폭을 제멋대로 바꾸고, Slider 는 채움 칸의
            //  앵커를 직접 움직이며(여기선 fillAmount 로 그린다), Animator 는 EXP 글자용 연출이다.
            foreach (var c in bar.GetComponentsInChildren<ContentSizeFitter>(true)) Object.DestroyImmediate(c);
            foreach (var c in bar.GetComponentsInChildren<Slider>(true)) Object.DestroyImmediate(c);
            foreach (var c in bar.GetComponentsInChildren<Animator>(true)) Object.DestroyImmediate(c);

            bar.gameObject.AddComponent<UI_StageBar>();

            PrefabUtility.SaveAsPrefabAsset(root, GAME_SCENE);
            Debug.Log("[RemakeV2] 스테이지 막대 조립 완료 — UI_GameScene/In-Frame/StageBar");
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
