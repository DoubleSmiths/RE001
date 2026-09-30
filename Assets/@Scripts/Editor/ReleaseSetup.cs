using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 출시용 화면·설정 구성을 **재현 가능하게** 적용한다 — 씬/프리팹을 손으로 만지지 않는다.
///   1. 브랜딩 이미지 임포트 설정(픽셀 아트: Point, 무압축, 4096)
///   2. 앱 아이콘(PC 기본 + 안드로이드 적응형/원형/레거시)
///   3. 타이틀 화면(키 아트·로고·시작 띠·설정/종료·버전)
///   4. 설정창 '시스템' 줄(크레딧·게임 종료·PC 전체 화면)
///   5. 크레딧 팝업 프리팹 + 어드레서블 등록(UI 그룹, PreLoad)
/// 여러 번 실행해도 같은 결과(있으면 갱신, 없으면 생성).
/// </summary>
public static class ReleaseSetup
{
    const string BRANDING = "Assets/@Resources/Sprite/Branding/";
    const string TITLE_SCENE = "Assets/@Scenes/TitleScene.unity";
    const string SETTING_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_SettingPopup.prefab";
    const string CREDITS_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_CreditsPopup.prefab";
    const string CREDITS_TEXT = "Assets/@Resources/Data/Credits.txt";
    const string BUTTON_SPRITE = "Assets/IMPORTS/ClassicPixelRPGUI/Artwok/UI/1_orange_button.png";
    const string BAND_SPRITE = "Assets/@Resources/Sprite/UI/title_btn_start.png";

    [MenuItem("Tools/Release/전체 구성 (아이콘·타이틀·설정·크레딧)")]
    public static void AllMenu() => Debug.Log(All());

    public static string All()
    {
        var log = new List<string>
        {
            ImportBrandingSprites(),
            ApplyAppIcons(),
            BuildTitleScene(),
            ExtendSettingPopup(),
            BuildCreditsPopup(),
        };
        AssetDatabase.SaveAssets();
        return string.Join("\n", log);
    }

    #region 1. 임포트
    public static string ImportBrandingSprites()
    {
        int n = 0;
        foreach (string path in Directory.GetFiles(BRANDING, "*.png"))
        {
            string p = path.Replace('\\', '/');
            var ti = (TextureImporter)AssetImporter.GetAtPath(p);
            if (ti == null) continue;
            bool icon = Path.GetFileName(p).StartsWith("AppIcon");
            ti.textureType = icon ? TextureImporterType.Default : TextureImporterType.Sprite;
            if (!icon) ti.spriteImportMode = SpriteImportMode.Single;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 4096;
            ti.SaveAndReimport();
            n++;
        }
        return $"[Release] 브랜딩 임포트 {n}개";
    }

    static Sprite LoadSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>(BRANDING + file);
    static Texture2D LoadTexture(string file) => AssetDatabase.LoadAssetAtPath<Texture2D>(BRANDING + file);
    #endregion

    #region 2. 아이콘
    public static string ApplyAppIcons()
    {
        Texture2D icon = LoadTexture("AppIcon.png");
        Texture2D fg = LoadTexture("AppIcon_AdaptiveFG.png");
        Texture2D bg = LoadTexture("AppIcon_AdaptiveBG.png");
        if (icon == null || fg == null || bg == null) return "[Release] 아이콘 파일 없음 — 건너뜀";

        // 기본 아이콘(모든 플랫폼 기본값, PC exe 아이콘)
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

        // PC: 크기별 칸을 전부 같은 원본으로
        int standaloneSlots = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Any).Length;
        PlayerSettings.SetIcons(NamedBuildTarget.Standalone, Enumerable.Repeat(icon, standaloneSlots).ToArray(), IconKind.Any);

        // 안드로이드: 적응형은 [배경, 전경] 두 겹, 원형·레거시는 완성본
        int android = 0;
        foreach (PlatformIconKind kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
        {
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (PlatformIcon pi in icons)
            {
                if (kind == AndroidPlatformIconKind.Adaptive) pi.SetTextures(bg, fg);
                else pi.SetTexture(icon);
                android++;
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }
        return $"[Release] 아이콘 적용 — PC {standaloneSlots}칸, 안드로이드 {android}칸";
    }
    #endregion

    #region 3. 타이틀
    public static string BuildTitleScene()
    {
        var scene = EditorSceneManager.OpenScene(TITLE_SCENE, OpenSceneMode.Single);
        var rootGo = scene.GetRootGameObjects().First(g => g.GetComponent<UI_TtitleScene>() != null);
        var root = rootGo.transform;
        TMP_FontAsset font = root.Find("StartButton/StartText").GetComponent<TMP_Text>().font;

        // 배경 = 키 아트(카메라 영역과 같은 1440x3040 비율)
        var panel = root.Find("Panel").GetComponent<Image>();
        panel.sprite = LoadSprite("TitleBG.png");
        panel.color = Color.white;
        panel.type = Image.Type.Simple;

        // 로고 — 위쪽 하늘 영역
        Sprite logoKo = LoadSprite("TitleLogo_KO.png"), logoEn = LoadSprite("TitleLogo_EN.png");
        var logo = FindOrCreate(root, "TitleLogo", 1).gameObject.GetOrAddComponent<Image>();
        logo.sprite = logoKo;
        logo.preserveAspect = true;
        logo.raycastTarget = false;
        float logoH = logoKo != null ? 190f * logoKo.rect.height / logoKo.rect.width : 50f;
        Place(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(190f, logoH));

        // 로딩 표시 — 아래쪽(시작 띠 자리)
        var loading = root.Find("Panel/LoadingIcon") as RectTransform;
        Place(loading, new Vector2(0.5f, 0.5f), new Vector2(0f, -112f), new Vector2(34f, 34f));

        var sliderRT = root.Find("Slider") as RectTransform;
        Place(sliderRT, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(140f, 6f));
        sliderRT.Find("Handle Slide Area").gameObject.SetActive(false);
        sliderRT.Find("Background").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        sliderRT.Find("Fill Area/Fill").GetComponent<Image>().color = new Color(1f, 0.82f, 0.35f, 1f);
        var fillArea = sliderRT.Find("Fill Area") as RectTransform;
        fillArea.offsetMin = new Vector2(0f, fillArea.offsetMin.y);
        fillArea.offsetMax = new Vector2(0f, fillArea.offsetMax.y);

        // 시작 띠 — 게임 내에 이미 있던 title_btn_start(반투명 띠 + 금색 선)
        var start = root.Find("StartButton") as RectTransform;
        Place(start, new Vector2(0.5f, 0.5f), new Vector2(0f, -112f), new Vector2(260f, 36f));
        var startImg = start.GetComponent<Image>();
        startImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BAND_SPRITE);
        startImg.type = Image.Type.Simple;
        startImg.color = Color.white;
        var startText = start.Find("StartText").GetComponent<TMP_Text>();
        startText.fontSize = 15;
        startText.color = new Color(1f, 0.95f, 0.8f, 1f);
        startText.alignment = TextAlignmentOptions.Center;

        // 설정 / 종료(PC) — 아래쪽
        MakeTextButton(root, "SettingButton", "SettingText", font, new Vector2(0.5f, 0f), new Vector2(-42f, 30f));
        MakeTextButton(root, "QuitButton", "QuitText", font, new Vector2(0.5f, 0f), new Vector2(42f, 30f));

        var version = FindOrCreate(root, "VersionText", root.childCount).gameObject.GetOrAddComponent<TextMeshProUGUI>();
        version.font = font;
        version.fontSize = 6;
        version.color = new Color(1f, 1f, 1f, 0.6f);
        version.alignment = TextAlignmentOptions.Bottom;
        version.raycastTarget = false;
        Place(version.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(120f, 10f));
        version.text = "v" + PlayerSettings.bundleVersion;

        var so = new SerializedObject(rootGo.GetComponent<UI_TtitleScene>());
        so.FindProperty("_logoKo").objectReferenceValue = logoKo;
        so.FindProperty("_logoEn").objectReferenceValue = logoEn;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "[Release] 타이틀 화면 구성 저장";
    }

    static void MakeTextButton(Transform parent, string name, string textName, TMP_FontAsset font, Vector2 anchor, Vector2 pos)
    {
        var rt = FindOrCreate(parent, name, parent.childCount);
        var img = rt.gameObject.GetOrAddComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_SPRITE);
        img.type = Image.Type.Simple;
        img.color = Color.white;
        rt.gameObject.GetOrAddComponent<Button>();
        Place(rt, anchor, pos, new Vector2(72f, 17f));

        var text = FindOrCreate(rt, textName, 0).gameObject.GetOrAddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = 8;
        text.color = new Color(1f, 0.95f, 0.85f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        var trt = text.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = new Vector2(0f, 1f);
        text.text = name;
    }
    #endregion

    #region 4. 설정창 '시스템' 줄
    const float ETC_Y = -203f;
    const float FRAME_H = 274f;   // 원래 210 + 시스템 줄(제목·버튼·전체 화면) — 이펙트 줄과 같은 간격(63)

    public static string ExtendSettingPopup()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(SETTING_POPUP);
        try
        {
            var frame = root.transform.Find("FIXED/In_Frame/SettingPopup/SettingPopupFrame") as RectTransform;
            var effects = frame.Find("SettingPopupEffects") as RectTransform;
            frame.sizeDelta = new Vector2(frame.sizeDelta.x, FRAME_H);

            var old = frame.Find("SettingPopupEtc");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var etc = Object.Instantiate(effects.gameObject, frame).transform as RectTransform;
            etc.name = "SettingPopupEtc";
            etc.anchoredPosition = new Vector2(effects.anchoredPosition.x, ETC_Y);

            // 복제한 '이펙트' 줄을 이름만 바꿔 재사용(같은 프리팹 안 이름 중복 금지 — 자동 바인딩)
            Rename(etc, "SettingPopupEffectsTitle", "SettingPopupEtcTitle");
            Rename(etc, "Img_SettingPopupEffects_04", "Img_SettingPopupEtc_01");
            Rename(etc, "SettingPopupPopupText", "SettingPopupFullscreenText");
            Rename(etc, "SettingPopupPopupONBtn", "SettingPopupFullscreenONBtn");
            foreach (string extra in new[] { "SettingPopupPopupOFFBtn", "SettingPopupShakeText", "SettingPopupShakeONBtn", "SettingPopupShakeOFFBtn" })
            {
                var t = etc.Find(extra);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }

            // 전체 화면 줄은 두 번째 줄(아래)로
            var fsText = etc.Find("SettingPopupFullscreenText") as RectTransform;
            fsText.anchoredPosition = new Vector2(fsText.anchoredPosition.x, -42f);
            var fsBtn = etc.Find("SettingPopupFullscreenONBtn") as RectTransform;
            fsBtn.anchoredPosition = new Vector2(fsBtn.anchoredPosition.x, -48.25f);

            // 첫 줄: [크레딧] [플레이 기록] [게임 종료] — 플레이 기록은 비공개 테스트용(ADR0046)
            TMP_FontAsset font = etc.Find("SettingPopupEtcTitle").GetComponent<TMP_Text>().font;
            MakeTextButton(etc, "SettingPopupCreditsBtn", "SettingPopupCreditsText", font, new Vector2(0.5f, 1f), new Vector2(-42f, -26f));
            MakeTextButton(etc, "SettingPopupPlaytestBtn", "SettingPopupPlaytestText", font, new Vector2(0.5f, 1f), new Vector2(0f, -26f));
            MakeTextButton(etc, "SettingPopupQuitBtn", "SettingPopupQuitText", font, new Vector2(0.5f, 1f), new Vector2(42f, -26f));
            foreach (string b in new[] { "SettingPopupCreditsBtn", "SettingPopupPlaytestBtn", "SettingPopupQuitBtn" })
            {
                var rt = etc.Find(b) as RectTransform;
                rt.sizeDelta = new Vector2(38f, 14f);
                var label = rt.Find(b.Replace("Btn", "Text")).GetComponent<TMP_Text>();
                label.enableAutoSizing = true;   // 영어 'PLAY LOG'·'CREDITS' 도 칸 안에
                label.fontSizeMin = 4f;
                label.fontSizeMax = 7f;
            }

            PrefabUtility.SaveAsPrefabAsset(root, SETTING_POPUP);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        return "[Release] 설정창 시스템 줄 추가";
    }

    static void Rename(Transform parent, string from, string to)
    {
        var t = parent.Find(from);
        if (t != null) t.name = to;
    }
    #endregion

    #region 5. 크레딧 팝업
    public static string BuildCreditsPopup()
    {
        if (!File.Exists(CREDITS_TEXT)) return "[Release] Credits.txt 없음 — 크레딧 팝업 건너뜀";

        if (!File.Exists(CREDITS_POPUP)) AssetDatabase.CopyAsset(SETTING_POPUP, CREDITS_POPUP);
        GameObject root = PrefabUtility.LoadPrefabContents(CREDITS_POPUP);
        try
        {
            Object.DestroyImmediate(root.GetComponent<UI_SettingPopup>());
            root.GetOrAddComponent<UI_CreditsPopup>();

            Rename(root.transform, "FIXED/In_Frame/SettingPopupBackground", "CreditsPopupBackground");
            var inFrame = root.transform.Find("FIXED/In_Frame");
            Rename(inFrame, "SettingPopupBackground", "CreditsPopupBackground");
            Rename(inFrame, "SettingPopup", "CreditsPopup");
            var popup = inFrame.Find("CreditsPopup");
            Rename(popup, "SettingPopupFrame", "CreditsPopupFrame");
            var frame = popup.Find("CreditsPopupFrame") as RectTransform;
            frame.sizeDelta = new Vector2(160f, 340f);

            foreach (string section in new[] { "SettingPopupLanguage", "SettingPopupSound", "SettingPopupEffects", "SettingPopupEtc" })
            {
                var t = frame.Find(section);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }
            var x = popup.Find("Xbutton");
            if (x != null) Object.DestroyImmediate(x.gameObject);

            var titlePanel = frame.Find("Panel_SettingPopupFrame_01");
            titlePanel.name = "Panel_CreditsPopupFrame_01";
            Rename(titlePanel, "Txt_SettingPopupTitle_02", "CreditsTitleText");
            Rename(titlePanel, "Img_SettingPopupTitle_03", "Img_CreditsPopupTitle_03");

            // 스크롤 본문
            var old = frame.Find("CreditsScroll");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var scroll = new GameObject("CreditsScroll", typeof(RectTransform)).transform as RectTransform;
            scroll.SetParent(frame, false);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(8f, 8f); scroll.offsetMax = new Vector2(-8f, -28f);
            var hit = scroll.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);            // 드래그를 받기 위한 투명 영역
            var sr = scroll.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 12f;

            var viewport = new GameObject("CreditsViewport", typeof(RectTransform), typeof(RectMask2D)).transform as RectTransform;
            viewport.SetParent(scroll, false);
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero; viewport.offsetMax = Vector2.zero;

            var body = new GameObject("CreditsBodyText", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            var brt = body.rectTransform;
            brt.SetParent(viewport, false);
            brt.anchorMin = new Vector2(0f, 1f); brt.anchorMax = new Vector2(1f, 1f); brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(0f, 100f);
            body.font = FindKoreanFont();
            body.fontSize = 6;
            body.color = new Color(0.92f, 0.9f, 0.85f, 1f);
            body.alignment = TextAlignmentOptions.Top;
            body.textWrappingMode = TextWrappingModes.Normal;
            body.raycastTarget = false;
            var fitter = body.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = viewport;
            sr.content = brt;

            PrefabUtility.SaveAsPrefabAsset(root, CREDITS_POPUP);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Register(CREDITS_POPUP, "UI_CreditsPopup", "UI");
        Register(CREDITS_TEXT, "Credits", "Data");
        return "[Release] 크레딧 팝업 + 어드레서블 등록";
    }

    // 한글·영문이 모두 있는 UI 폰트(DOSSaemmul) — FontManager 의 한국어 UI 폰트와 같다.
    static TMP_FontAsset FindKoreanFont()
    {
        string guid = AssetDatabase.FindAssets("DOSSaemmul SDF t:TMP_FontAsset").FirstOrDefault();
        return guid == null ? null : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
    }

    static void Register(string path, string address, string groupName)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(groupName);
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group, false, false);
        entry.address = address;
        entry.SetLabel("PreLoad", true, true, false);
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
    }
    #endregion

    #region 공통
    static RectTransform FindOrCreate(Transform parent, string name, int siblingIndex)
    {
        var t = parent.Find(name) as RectTransform;
        if (t == null)
        {
            t = new GameObject(name, typeof(RectTransform)).transform as RectTransform;
            t.SetParent(parent, false);
            t.SetSiblingIndex(Mathf.Min(siblingIndex, parent.childCount - 1));
        }
        return t;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, anchor.y == 1f ? 1f : anchor.y == 0f ? 0f : 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
    #endregion
}
