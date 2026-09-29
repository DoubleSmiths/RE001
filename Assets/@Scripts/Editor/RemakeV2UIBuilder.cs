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

    // 전투 입력(ADR0033): 게임 화면(재화 줄 아래 ~ 지면 아래 캐릭터 상태 줄 위)을 덮는 묶음.
    //  탭 칸(투명 Image, 누르면 공격) + 왼쪽 아래 AUTO + 오른쪽 아래 방패(막기, 재사용 대기 원형 덮개).
    //  In-Frame 의 첫 자식으로 둬서 다른 HUD 가 전부 그 위에 그려지고 먼저 눌린다. 버튼은 탭 칸의 형제다(UI_BattleInput 주석).
    //  그림은 _tools/gen_battle_buttons.py 가 만든다. 예전 막기 칸(GuardTapArea)이 있으면 지운다.
    static readonly Vector2 GUARD_AREA_POS  = new Vector2(0f, -19f);
    const float GUARD_AREA_HEIGHT = 158f;
    const string GUARD_BTN_SPRITE = "Assets/@Resources/Sprite/UI/Battle/Btn_Guard.png";
    const string AUTO_BTN_SPRITE  = "Assets/@Resources/Sprite/UI/Battle/Btn_Auto.png";
    static readonly Vector2 GUARD_BTN_POS  = new Vector2(-3f, 3f);    // 오른쪽 아래 구석 — 절반은 지면 아래 흙 띠(아래서 7까지)라 몬스터를 덜 가린다
    static readonly Vector2 GUARD_BTN_SIZE = new Vector2(30f, 30f);
    static readonly Vector2 AUTO_BTN_POS   = new Vector2(3f, 3f);     // 왼쪽 아래 구석
    static readonly Vector2 AUTO_BTN_SIZE  = new Vector2(26f, 11f);

    [MenuItem("Tools/Remake V2/Build Battle Input")]
    public static void BuildBattleInput()
    {
        PreparePixelSprite(GUARD_BTN_SPRITE);
        PreparePixelSprite(AUTO_BTN_SPRITE);

        GameObject root = PrefabUtility.LoadPrefabContents(GAME_SCENE);
        try
        {
            Transform frame = Find(root.transform, "In-Frame");
            foreach (string n in new[] { "GuardTapArea", "BattleInput" })
            {
                Transform old = Find(root.transform, n, false);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            var group = new GameObject("BattleInput", typeof(RectTransform)).GetComponent<RectTransform>();
            group.SetParent(frame, false);
            group.SetAsFirstSibling();
            group.anchorMin = new Vector2(0f, 1f);
            group.anchorMax = Vector2.one;
            group.pivot = new Vector2(0.5f, 1f);
            group.anchoredPosition = GUARD_AREA_POS;
            group.sizeDelta = new Vector2(0f, GUARD_AREA_HEIGHT);

            var tap = NewImage("BattleTapArea", group);
            tap.anchorMin = Vector2.zero;
            tap.anchorMax = Vector2.one;
            tap.offsetMin = tap.offsetMax = Vector2.zero;
            tap.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);   // 보이지 않지만 누름은 받는다

            var auto = NewImage("BattleAutoBtn", group);
            auto.anchorMin = auto.anchorMax = auto.pivot = Vector2.zero;
            auto.anchoredPosition = AUTO_BTN_POS;
            auto.sizeDelta = AUTO_BTN_SIZE;
            auto.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AUTO_BTN_SPRITE);
            var label = (RectTransform)Object.Instantiate(Find(root.transform, "MainGoldText").gameObject, auto).transform;
            label.name = "BattleAutoTxt";
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.pivot = new Vector2(0.5f, 0.5f);
            label.offsetMin = label.offsetMax = Vector2.zero;
            foreach (var c in label.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(c);
            var tmp = label.GetComponent<TMP_Text>();
            tmp.text = "AUTO";
            tmp.enableAutoSizing = false;
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            var guard = NewImage("BattleGuardBtn", group);
            guard.anchorMin = guard.anchorMax = guard.pivot = new Vector2(1f, 0f);
            guard.anchoredPosition = GUARD_BTN_POS;
            guard.sizeDelta = GUARD_BTN_SIZE;
            var guardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GUARD_BTN_SPRITE);
            guard.GetComponent<Image>().sprite = guardSprite;
            var cool = NewImage("BattleGuardCool", guard);
            cool.anchorMin = Vector2.zero;
            cool.anchorMax = Vector2.one;
            cool.offsetMin = cool.offsetMax = Vector2.zero;
            var coolImg = cool.GetComponent<Image>();
            coolImg.sprite = guardSprite;   // 같은 원 모양으로 어둡게 덮는다
            coolImg.color = new Color(0f, 0f, 0f, 0.6f);
            coolImg.type = Image.Type.Filled;
            coolImg.fillMethod = Image.FillMethod.Radial360;
            coolImg.fillOrigin = (int)Image.Origin360.Top;
            coolImg.fillClockwise = false;
            coolImg.fillAmount = 0f;
            coolImg.raycastTarget = false;

            group.gameObject.AddComponent<UI_BattleInput>();

            PrefabUtility.SaveAsPrefabAsset(root, GAME_SCENE);
            Debug.Log("[RemakeV2] 전투 입력 조립 완료 — UI_GameScene/In-Frame/BattleInput (탭·AUTO·방패)");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static RectTransform NewImage(string name, Transform parent)
    {
        var rt = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    // 도트 그림 — 흐려지지 않게 Point, 색이 뭉개지지 않게 무압축
    static void PreparePixelSprite(string path)
    {
        AssetDatabase.ImportAsset(path);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        if (ti == null) throw new System.Exception($"[RemakeV2] '{path}' 없음 — python _tools/gen_battle_buttons.py 먼저");
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.SaveAndReimport();
    }

    // ── 미션·출석 창 ──
    //  틀: 마을 상점 창(UI_ShopPopup) 복제 — 창 그림·제목·스크롤 목록·열림 연출이 그대로 필요하다.
    //  탭·줄: 상태 창의 '능력 상세정보' 긴 버튼 복제. 이미 만들어져 있으면 생성 부분만 다시 만든다(GUID·어드레서블 유지).
    const string SHOP_POPUP    = "Assets/@Resources/Prefab/UI/Popup/UI_ShopPopup.prefab";
    const string MISSION_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_MissionPopup.prefab";
    const string QUEST_ICON    = "Assets/@Resources/Sprite/Icon/Menu Icon/Quest Icon.PNG";
    static readonly string[] MISSION_TABS = { "Daily", "Weekly", "Guide", "Attendance", "Shop", "Dungeon", "Relic", "Town" };   // UI_MissionPopup.Tab 순서
    const string SHOP_ICON = "Assets/@Resources/Sprite/Icon/Menu Icon/Shop_Icon.png";
    const float MISSION_TABS_TOP = 27f, MISSION_TAB_HEIGHT = 12f, MISSION_TABS_WIDTH = 124f;
    //  탭이 8개가 되어 한 줄이면 영어 이름(DUNGEON·RELICS)이 13칸에 안 들어간다 — 4개씩 두 줄. 목록은 한 줄만큼 내린다.
    const int   MISSION_TAB_COLUMNS = 4;
    const float MISSION_TAB_SPACING = 2f;
    const float MISSION_ROW_HEIGHT = 16f, MISSION_RIGHT_WIDTH = 38f;
    const float MISSION_ROW_ICON = 13f, MISSION_ROW_ICON_INSET = 2f;

    [MenuItem("Tools/Remake V2/Build Mission Popup")]
    public static void BuildMissionPopup()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(MISSION_POPUP) == null && !AssetDatabase.CopyAsset(SHOP_POPUP, MISSION_POPUP))
            throw new System.Exception("[RemakeV2] 상점 창 복제 실패");

        GameObject charRoot = PrefabUtility.LoadPrefabContents(CHARACTER_POPUP);
        GameObject root = PrefabUtility.LoadPrefabContents(MISSION_POPUP);
        try
        {
            // 처음 복제했을 때만: 상점 전용 부품을 걷어 내고 이름을 미션 창 것으로
            var shop = root.GetComponent<UI_ShopPopup>();
            if (shop != null) Object.DestroyImmediate(shop);
            RenameIf(root.transform, "ShopPopupBackground", "MissionPopupBackground");
            RenameIf(root.transform, "ShopPopupFrame", "MissionPopupFrame");
            RenameIf(root.transform, "ShopPopupTitle", "MissionPopupTitle");
            RenameIf(root.transform, "ShopPopup", "MissionPopup");
            RenameIf(root.transform, "ShopList", "MissionList");
            RenameIf(root.transform, "Img_ShopList_01", "Img_MissionList_01");
            // 창 그림(BAR_Bag) 오른쪽 아래에 버튼 자리가 그려져 있다 — 가방의 '정리' 버튼을 '모두 받기'로 쓴다
            //  (먼저 만든 창에는 이 버튼을 지웠었다 — 없으면 상점 창에서 다시 가져온다)
            if (Find(root.transform, "ClaimAllBtn", false) == null)
            {
                Transform sort = Find(root.transform, "ShopSortBtn", false);
                if (sort == null)
                {
                    GameObject shopRoot = PrefabUtility.LoadPrefabContents(SHOP_POPUP);
                    try { sort = Object.Instantiate(Find(shopRoot.transform, "ShopSortBtn").gameObject, Find(root.transform, "MissionPopupFrame")).transform; }
                    finally { PrefabUtility.UnloadPrefabContents(shopRoot); }
                }
                sort.name = "ClaimAllBtn";
                RenameIf(sort, "Text", "ClaimAllTxt");
            }
            Find(root.transform, "ClaimAllBtn").gameObject.SetActive(true);
            foreach (var n in new[] { "ImportantBagBtn", "ImportantThingsBox" })
            {
                var t = Find(root.transform, n, false);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }

            // 목록: 격자 → 세로 줄, 샘플 칸 비우기, 탭 자리만큼 아래로
            Transform content = Find(root.transform, "Content");
            for (int i = content.childCount - 1; i >= 0; i--) Object.DestroyImmediate(content.GetChild(i).gameObject);
            var grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null) Object.DestroyImmediate(grid);
            var vlg = content.GetComponent<VerticalLayoutGroup>();   // ?? 는 Unity 가짜 null 을 못 거른다
            if (vlg == null) vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3f;
            vlg.padding = new RectOffset(2, 2, 2, 2);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var list = (RectTransform)Find(root.transform, "MissionList");
            int tabRows = (MISSION_TABS.Length + MISSION_TAB_COLUMNS - 1) / MISSION_TAB_COLUMNS;
            float extra = (tabRows - 1) * (MISSION_TAB_HEIGHT + MISSION_TAB_SPACING);
            list.sizeDelta = new Vector2(list.sizeDelta.x, -62.5f - extra);   // 위 41(+탭 줄) · 아래 21.5 여백 — 원래 위 26 에 탭 줄 15
            list.anchoredPosition = new Vector2(list.anchoredPosition.x, -9.75f - extra / 2f);

            // 이전에 만든 탭·줄 템플릿 제거 후 다시
            Transform frame = Find(root.transform, "MissionPopupFrame");
            foreach (var n in new[] { "MissionTabs", "MissionRowTemplate" })
            {
                var t = Find(root.transform, n, false);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }
            Transform rowSrc = Find(charRoot.transform, "StatDetailBtn");

            var tabs = new GameObject("MissionTabs", typeof(RectTransform), typeof(GridLayoutGroup)).GetComponent<RectTransform>();
            tabs.SetParent(frame, false);
            tabs.anchorMin = tabs.anchorMax = tabs.pivot = new Vector2(0.5f, 1f);
            tabs.anchoredPosition = new Vector2(0f, -MISSION_TABS_TOP);
            tabs.sizeDelta = new Vector2(MISSION_TABS_WIDTH, tabRows * MISSION_TAB_HEIGHT + (tabRows - 1) * MISSION_TAB_SPACING);
            var grid2 = tabs.GetComponent<GridLayoutGroup>();
            grid2.cellSize = new Vector2((MISSION_TABS_WIDTH - (MISSION_TAB_COLUMNS - 1) * MISSION_TAB_SPACING) / MISSION_TAB_COLUMNS, MISSION_TAB_HEIGHT);
            grid2.spacing = new Vector2(MISSION_TAB_SPACING, MISSION_TAB_SPACING);
            grid2.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid2.constraintCount = MISSION_TAB_COLUMNS;
            grid2.childAlignment = TextAnchor.UpperCenter;
            foreach (string name in MISSION_TABS)
            {
                var tab = Object.Instantiate(rowSrc.gameObject, tabs).transform;
                tab.name = name + "TabBtn";
                Rename(tab, "StatDetailBtnBackgroundImage", name + "TabBg");
                Rename(tab, "StatDetailBtnMask", name + "TabMask");
                Rename(tab, "StatDetailBtnGrow", name + "TabGrow");
                Find(tab, name + "TabMask").gameObject.SetActive(false);
                var label = (RectTransform)Find(tab, "Text");
                label.name = name + "TabTxt";
                label.anchorMin = Vector2.zero;   // 원본 글자 칸은 폭 0(ContentSizeFitter 몫)이라 한 글자씩 줄바꿈됐다
                label.anchorMax = Vector2.one;
                label.offsetMin = label.offsetMax = Vector2.zero;
                var tmp = label.GetComponent<TMP_Text>();
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 4f;
                tmp.fontSizeMax = 6f;
            }

            // 줄 템플릿 — 왼쪽 제목·진행, 오른쪽 상태·보상
            var row = (RectTransform)Object.Instantiate(rowSrc.gameObject, Find(root.transform, "MissionPopup")).transform;
            row.name = "MissionRowTemplate";
            row.sizeDelta = new Vector2(row.sizeDelta.x, MISSION_ROW_HEIGHT);
            Rename(row, "StatDetailBtnBackgroundImage", "MissionRowBg");
            Rename(row, "StatDetailBtnMask", "MissionRowMask");
            Rename(row, "StatDetailBtnGrow", "MissionRowGrow");
            Find(row, "MissionRowMask").gameObject.SetActive(false);
            var left = (RectTransform)Find(row, "Text");
            left.name = "MissionRowLeftTxt";
            var right = (RectTransform)Object.Instantiate(left.gameObject, row).transform;
            right.name = "MissionRowRightTxt";
            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(1f, 1f);
            left.pivot = new Vector2(0f, 0.5f);
            left.offsetMin = new Vector2(TEXT_INSET, 0f);
            left.offsetMax = new Vector2(-(MISSION_RIGHT_WIDTH + TEXT_INSET), 0f);
            right.anchorMin = new Vector2(1f, 0f);
            right.anchorMax = new Vector2(1f, 1f);
            right.pivot = new Vector2(1f, 0.5f);
            right.anchoredPosition = new Vector2(-TEXT_INSET, 0f);
            right.sizeDelta = new Vector2(MISSION_RIGHT_WIDTH, 0f);
            foreach (var (rt, align) in new[] { (left, TextAlignmentOptions.MidlineLeft), (right, TextAlignmentOptions.MidlineRight) })
            {
                var tmp = rt.GetComponent<TMP_Text>();
                tmp.alignment = align;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.enableAutoSizing = true;   // 긴 미션 이름도 한 줄에
                tmp.fontSizeMin = 3.5f;
                tmp.fontSizeMax = 6f;
                tmp.overflowMode = TextOverflowModes.Ellipsis;   // 최소 크기로도 넘치면 옆 칸을 덮지 말고 말줄임
            }
            // 줄 왼쪽 아이콘(유물 탭 등, ADR0035) — 쓰는 줄만 켠다. 켜면 UI_MissionPopup 이 왼쪽 글자를 아이콘만큼 민다
            var icon = NewImage("MissionRowIcon", row);
            icon.anchorMin = icon.anchorMax = icon.pivot = new Vector2(0f, 0.5f);
            icon.anchoredPosition = new Vector2(MISSION_ROW_ICON_INSET, 0f);
            icon.sizeDelta = new Vector2(MISSION_ROW_ICON, MISSION_ROW_ICON);
            var iconImg = icon.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            icon.gameObject.SetActive(false);

            var element = row.gameObject.GetComponent<LayoutElement>();
            if (element == null) element = row.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = MISSION_ROW_HEIGHT;
            row.gameObject.SetActive(false);

            // 복제한 동작 부품 정리(폭을 바꾸는 ContentSizeFitter, 채움 앵커를 움직이는 Slider, EXP 글자 Animator)
            foreach (var t in new Transform[] { tabs, row })
            {
                foreach (var c in t.GetComponentsInChildren<ContentSizeFitter>(true)) Object.DestroyImmediate(c);
                foreach (var c in t.GetComponentsInChildren<Animator>(true)) Object.DestroyImmediate(c);
            }

            if (root.GetComponent<UI_MissionPopup>() == null) root.AddComponent<UI_MissionPopup>();
            PrefabUtility.SaveAsPrefabAsset(root, MISSION_POPUP);
            Debug.Log("[RemakeV2] 미션 창 조립 완료 — " + MISSION_POPUP);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
            PrefabUtility.UnloadPrefabContents(charRoot);
        }

        BuildMissionShortcut();
    }

    // HUD 재화 줄에 미션·상점 버튼 — 설정 버튼을 복제해 아이콘만 바꾼다(인스펙터 클릭 연결은 비운다).
    static readonly Vector2 MISSION_BTN_POS = new Vector2(44f, 0f);
    static readonly Vector2 SHOP_BTN_POS    = new Vector2(61f, 0f);

    static void BuildMissionShortcut()
    {
        BuildShortcut("Mission", MISSION_BTN_POS, QUEST_ICON, UI_MissionPopup.Tab.Daily);
        BuildShortcut("Shop", SHOP_BTN_POS, SHOP_ICON, UI_MissionPopup.Tab.Shop);
    }

    static void BuildShortcut(string prefix, Vector2 pos, string iconPath, UI_MissionPopup.Tab tab)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GAME_SCENE);
        try
        {
            Transform bar = Find(root.transform, "CurrencyBar");
            Transform old = Find(root.transform, prefix + "Btn", false);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var btn = (RectTransform)Object.Instantiate(Find(bar, "OptionBtn").gameObject, bar).transform;
            btn.name = prefix + "Btn";
            btn.anchoredPosition = pos;
            for (int i = btn.childCount - 1; i >= 0; i--) Object.DestroyImmediate(btn.GetChild(i).gameObject);   // 설정 버튼의 아이콘 자식들
            var img = btn.GetComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            var button = btn.GetComponent<Button>();
            if (button != null) button.onClick = new Button.ButtonClickedEvent();

            // 알림 점: 빨간 원(Unity 기본 Knob) + 흰 '!' — 글자만 두면 얇은 선처럼 보여 알아보기 어려웠다
            var badge = new GameObject(prefix + "Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            badge.SetParent(btn, false);
            badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(1f, 1f);
            badge.anchoredPosition = new Vector2(1.5f, 1.5f);
            badge.sizeDelta = new Vector2(7f, 7f);
            var dot = badge.GetComponent<Image>();
            dot.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            dot.color = new Color32(0xE8, 0x2A, 0x2A, 0xFF);
            dot.raycastTarget = false;
            var mark = (RectTransform)Object.Instantiate(Find(bar, "MainGoldText").gameObject, badge).transform;
            mark.name = prefix + "BadgeTxt";
            mark.anchorMin = Vector2.zero;
            mark.anchorMax = Vector2.one;
            mark.offsetMin = mark.offsetMax = Vector2.zero;
            foreach (var c in mark.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(c);
            var tmp = mark.GetComponent<TMP_Text>();
            tmp.text = "!";
            tmp.fontSize = 6f;
            tmp.enableAutoSizing = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            badge.gameObject.SetActive(false);

            var shortcut = btn.gameObject.GetComponent<UI_MissionShortcut>();
            if (shortcut == null) shortcut = btn.gameObject.AddComponent<UI_MissionShortcut>();
            shortcut._tab = tab;

            PrefabUtility.SaveAsPrefabAsset(root, GAME_SCENE);
            Debug.Log($"[RemakeV2] HUD {prefix} 버튼 조립 완료 — CurrencyBar/{prefix}Btn");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void RenameIf(Transform root, string from, string to)
    {
        var t = Find(root, from, false);
        if (t != null) t.name = to;
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
