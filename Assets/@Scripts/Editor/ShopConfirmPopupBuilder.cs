using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 상점 구매/판매 확인창 프리팹을 조립한다.
//
//  왜 코드로 만드는가: 기존 팝업을 통째로 복제하면 쓰지 않는 부속(느낌표 VFX, NEW 뱃지,
//  전용 애니메이터)이 딸려 오고, 그것들이 켜진 채 남아 버튼 위를 덮는 사고가 이미 있었다.
//
//  ⚠ 이 프로젝트의 UI 좌표계는 **180 x 440**이다(UI_WarningPopup의 FIXED 크기).
//    1080x1920으로 짰다가 확인창이 화면 폭을 넘는 가로 띠로 나왔다. 구조도 다른 팝업과 같은
//    루트 > FIXED > In_Frame > 내용 을 따른다.
//
//  슬라이더만은 물약창 것을 복사한다 — 손잡이·채움 스프라이트가 이 게임의 톤이고,
//  코드로 다시 만들면 혼자 다른 UI가 된다.
//
//  실행: Tools/Build Shop Confirm Popup  (여러 번 실행해도 안전 — 매번 새로 조립)
public static class ShopConfirmPopupBuilder
{
    const string DST = "Assets/@Resources/Prefab/UI/Popup/UI_ShopConfirmPopup.prefab";
    const string SRC_SLIDER_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_PotionPopup.prefab";
    const string SRC_FRAME_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_WarningPopup.prefab";
    // 캔버스 설정은 화면에 제대로 뜨는 팝업에서 베낀다 — 경고창은 Overlay라 기준이 못 된다.
    const string SRC_CANVAS_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_InventoryPopup.prefab";

    static readonly Vector2 FIXED_SIZE = new Vector2(180f, 440f);   // 폰 세로 화면
    static readonly Vector2 PANEL = new Vector2(150f, 124f);

    [MenuItem("Tools/Build Shop Confirm Popup")]
    static void Build()
    {
        var root = new GameObject("UI_ShopConfirmPopup", typeof(RectTransform), typeof(Canvas),
                                  typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UI_ShopConfirmPopup));
        try
        {
            var rootRt = (RectTransform)root.transform;
            rootRt.anchorMin = rootRt.anchorMax = Vector2.zero;
            rootRt.sizeDelta = Vector2.zero;

            // @UI_Root에는 Canvas도 CanvasScaler도 없다 — 팝업이 각자 들고 있어야 한다.
            //  스케일러를 빠뜨렸더니 180x440 좌표계가 화면 픽셀로 해석돼 확인창이 화면 밖까지 퍼졌다.
            //  값은 기존 팝업에서 그대로 베낀다(수치를 새로 정하면 이 창만 크기가 달라진다).
            CopyCanvasSettings(root);

            var fixedGo = new GameObject("FIXED", typeof(RectTransform));
            fixedGo.transform.SetParent(rootRt, false);
            var fixedRt = (RectTransform)fixedGo.transform;
            Center(fixedRt, FIXED_SIZE);

            var frameGo = new GameObject("In_Frame", typeof(RectTransform));
            frameGo.transform.SetParent(fixedRt, false);
            Stretch((RectTransform)frameGo.transform);
            var inFrame = (RectTransform)frameGo.transform;

            // 화면을 덮어 뒤쪽 클릭을 흡수하는 어두운 막. 확인창은 모달이어야 한다 —
            //  뒤의 상점 목록이 눌리면 창을 띄운 의미가 없다.
            var dim = NewImage("ShopConfirmDim", inFrame);
            Stretch((RectTransform)dim.transform);
            dim.color = new Color32(0, 0, 0, 200);
            dim.raycastTarget = true;

            // 창 본체
            var panel = NewImage("ShopConfirmPopup", inFrame);
            var panelRt = (RectTransform)panel.transform;
            Center(panelRt, PANEL);
            panel.sprite = BorrowFrameSprite();
            panel.type = Image.Type.Sliced;
            panel.color = panel.sprite != null ? Color.white : new Color32(34, 26, 20, 250);
            panel.raycastTarget = true;   // 창 안쪽 빈 곳 클릭이 뒤로 새지 않게

            NewText("ShopConfirmTitleText", panelRt, new Vector2(0f, 48f), new Vector2(130f, 16f), 11f);

            var icon = NewImage("ShopConfirmIcon", panelRt);
            Place((RectTransform)icon.transform, new Vector2(-50f, 22f), new Vector2(28f, 28f));
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            NewText("ShopConfirmNameText", panelRt, new Vector2(16f, 22f), new Vector2(90f, 16f), 9f,
                    TextAlignmentOptions.Left);

            CopySlider(panelRt);

            NewText("ShopConfirmCountText", panelRt, new Vector2(0f, -14f), new Vector2(110f, 14f), 9f);
            NewText("ShopConfirmTotalText", panelRt, new Vector2(0f, -30f), new Vector2(130f, 16f), 11f);

            MakeButton("ShopConfirmCancelButton", "ShopConfirmCancelText", panelRt, new Vector2(-36f, -48f));
            MakeButton("ShopConfirmOkButton", "ShopConfirmOkText", panelRt, new Vector2(36f, -48f));

            Directory.CreateDirectory(Path.GetDirectoryName(DST));
            PrefabUtility.SaveAsPrefabAsset(root, DST);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ShopConfirm] {Path.GetFileName(DST)} 생성 (기준 {FIXED_SIZE.x}x{FIXED_SIZE.y})");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    /// <summary>기존 팝업의 Canvas / CanvasScaler 설정을 그대로 옮겨 온다.</summary>
    static void CopyCanvasSettings(GameObject dst)
    {
        GameObject src = PrefabUtility.LoadPrefabContents(SRC_CANVAS_POPUP);
        try
        {
            var srcScaler = src.GetComponent<CanvasScaler>();
            var dstScaler = dst.GetComponent<CanvasScaler>();
            if (srcScaler != null && dstScaler != null)
            {
                dstScaler.uiScaleMode = srcScaler.uiScaleMode;
                dstScaler.referenceResolution = srcScaler.referenceResolution;
                dstScaler.screenMatchMode = srcScaler.screenMatchMode;
                dstScaler.matchWidthOrHeight = srcScaler.matchWidthOrHeight;
                dstScaler.referencePixelsPerUnit = srcScaler.referencePixelsPerUnit;
                dstScaler.scaleFactor = srcScaler.scaleFactor;
                Debug.Log($"[ShopConfirm] CanvasScaler 복사: {srcScaler.uiScaleMode} ref={srcScaler.referenceResolution}");
            }
            else
            {
                Debug.LogWarning("[ShopConfirm] 원본 팝업에 CanvasScaler가 없습니다 — 기본값 사용");
            }

            var srcCanvas = src.GetComponent<Canvas>();
            var dstCanvas = dst.GetComponent<Canvas>();
            if (srcCanvas != null && dstCanvas != null)
            {
                dstCanvas.renderMode = srcCanvas.renderMode;
                dstCanvas.planeDistance = srcCanvas.planeDistance;
                dstCanvas.sortingLayerName = srcCanvas.sortingLayerName;
            }

            // ⚠ Overlay로 두면 폰 프레임이 아니라 **전체 스크린** 기준으로 그려져,
            //  180x440이 6배로 커지며 화면 밖으로 밀려난다(실제로 확인창이 안 보였다).
            //  UIManager.SetCanvas 주석대로 팝업은 ScreenSpaceCamera여야 한다.
            if (dstCanvas != null && dstCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                dstCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                Debug.LogWarning("[ShopConfirm] 원본이 Overlay라 ScreenSpaceCamera로 강제했습니다");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(src);
        }
    }

    // ── 조립 도우미 ────────────────────────────────────────────────────────
    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Center(RectTransform rt, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Image NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        return go.GetComponent<Image>();
    }

    static TMP_Text NewText(string name, Transform parent, Vector2 pos, Vector2 size, float fontSize,
                            TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, pos, size);

        var t = go.GetComponent<TextMeshProUGUI>();
        t.fontSize = fontSize;
        t.alignment = align;
        t.enableWordWrapping = false;
        t.raycastTarget = false;      // 글자가 버튼 클릭을 먹지 않게
        t.color = Color.white;
        t.text = name;
        return t;
    }

    static void MakeButton(string name, string labelName, Transform parent, Vector2 pos)
    {
        var img = NewImage(name, parent);
        Place((RectTransform)img.transform, pos, new Vector2(58f, 20f));
        img.sprite = BorrowButtonSprite();
        img.type = Image.Type.Sliced;
        img.color = img.sprite != null ? Color.white : new Color32(74, 60, 46, 255);
        img.raycastTarget = true;
        img.gameObject.AddComponent<Button>();

        NewText(labelName, img.transform, Vector2.zero, new Vector2(54f, 16f), 9f);
    }

    /// <summary>물약창의 Slider 서브트리를 통째로 복사한다(손잡이·채움 그림을 그대로 쓰기 위해).</summary>
    static void CopySlider(Transform parent)
    {
        GameObject src = PrefabUtility.LoadPrefabContents(SRC_SLIDER_POPUP);
        try
        {
            Transform s = src.GetComponentsInChildren<Transform>(true)
                             .FirstOrDefault(t => t.name == "Slider" && t.GetComponent<Slider>() != null);
            if (s == null)
            {
                Debug.LogWarning("[ShopConfirm] 물약창에서 Slider를 찾지 못했습니다 — 기본 슬라이더로 대체");
                MakeFallbackSlider(parent);
                return;
            }

            var copy = Object.Instantiate(s.gameObject, parent);
            copy.name = "ShopConfirmSlider";
            copy.SetActive(true);
            Place((RectTransform)copy.transform, new Vector2(0f, 2f), new Vector2(120f, 12f));

            // 물약창 전용 부속(자동물약 잠금 표시, 안내 문구)은 여기서 의미가 없다.
            foreach (Transform t in copy.GetComponentsInChildren<Transform>(true))
                if (t.name.Contains("Lock") || t.name.StartsWith("Txt_"))
                    t.gameObject.SetActive(false);

            var sl = copy.GetComponent<Slider>();
            sl.onValueChanged.RemoveAllListeners();
            sl.wholeNumbers = true;
            sl.minValue = 1;
            sl.maxValue = 1;
            sl.value = 1;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(src);
        }
    }

    static void MakeFallbackSlider(Transform parent)
    {
        var go = new GameObject("ShopConfirmSlider", typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, new Vector2(0f, 2f), new Vector2(120f, 12f));

        var bg = NewImage("Background", go.transform);
        Stretch((RectTransform)bg.transform);
        bg.color = new Color32(20, 16, 12, 255);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        Stretch((RectTransform)fillArea.transform);
        var fill = NewImage("Fill", fillArea.transform);
        Stretch((RectTransform)fill.transform);
        fill.color = new Color32(45, 255, 0, 255);

        var handle = NewImage("Handle", go.transform);
        Place((RectTransform)handle.transform, Vector2.zero, new Vector2(10f, 14f));

        var sl = go.GetComponent<Slider>();
        sl.fillRect = (RectTransform)fill.transform;
        sl.handleRect = (RectTransform)handle.transform;
        sl.targetGraphic = handle;
        sl.wholeNumbers = true;
    }

    // 기존 팝업에서 프레임/버튼 스프라이트를 빌려 톤을 맞춘다.
    //  경고창의 그림은 얇은 띠라 창틀이 못 된다 — 실제 창을 가진 소지품 팝업에서 가져온다.
    static Sprite BorrowFrameSprite() => BorrowSprite(SRC_CANVAS_POPUP, "PopupFrame", "Frame");
    static Sprite BorrowButtonSprite() => BorrowSprite(SRC_CANVAS_POPUP, "Btn", "Button", "Xbutton");

    static Sprite BorrowSprite(string prefabPath, params string[] nameHints)
    {
        if (!File.Exists(prefabPath)) return null;

        GameObject src = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            foreach (string hint in nameHints)
            {
                var img = src.GetComponentsInChildren<Image>(true)
                             .FirstOrDefault(i => i.sprite != null && i.name.Contains(hint));
                if (img != null) return img.sprite;
            }
            return src.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.sprite != null)?.sprite;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(src);
        }
    }
}
