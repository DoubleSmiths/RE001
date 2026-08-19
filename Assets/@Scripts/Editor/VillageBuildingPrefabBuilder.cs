using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

// 대장간·상점의 프리팹 구조를 **수도원 것을 복제해서** 만든다.
//
//  협회·연금공방·수도원은 안에 들어가는 버튼 배치가 완전히 같다(슬29). 그래서 새 건물도
//  손으로 새로 짜는 것보다 이미 검증된 수도원 그룹을 통째로 복제하고 이름만 바꾸는 쪽이
//  안전하다 — 자동 바인딩이 **이름**으로 걸리므로, 이름 규칙만 맞으면 그대로 붙는다.
//
//  ⚠ 같은 프리팹 안에서 이름이 겹치면 바인딩이 엉킨다. 그래서 Monastery -> Forge/Shop 로
//    치환한 뒤 기능 버튼만 따로 개명한다(축성 -> 제작/구매, 빈칸1 -> 강화/판매).
//
//  실행: Tools/Build Village Buildings (대장간·상점 프리팹 생성)
//  여러 번 실행해도 안전하다 — 이미 있으면 그룹은 건드리지 않고 스프라이트만 다시 물린다.
public static class VillageBuildingPrefabBuilder
{
    const string GROUP_PREFAB = "Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab";
    const string MAP_DIR = "Assets/@Resources/Prefab/Map";
    const string SRC_GROUP = "Village1MonasteryBtnGroup";
    const string SRC_BG = MAP_DIR + "/Village1_Guild_BG.prefab";

    // 건물 하나를 만드는 데 필요한 것들.
    class Building
    {
        public string Key;              // "Forge" / "Shop"
        public string TempButton;       // 로비에서 차지할 빈칸
        public string Fn1, Fn2;         // 기능 버튼 2개의 새 이름 (축성/빈칸1 자리)
        public string IconSprite;       // 로비 버튼 아이콘
        public string Fn1Icon, Fn2Icon; // 기능 버튼 아이콘
        public string PortraitSprite;   // [대화] 버튼 그림
        public string BgSprite;         // 건물 배경
        public string NpcSprite;        // 건물 안 NPC 입상
    }

    static readonly Building[] BUILDINGS =
    {
        new Building
        {
            Key = "Forge", TempButton = "TempButton1",
            Fn1 = "ForgeCraftButton", Fn2 = "ForgeEnhanceButton",
            IconSprite = "Assets/@Resources/Sprite/Icon/Menu Icon/Forge_Icon.png",
            Fn1Icon = "Assets/@Resources/Sprite/Icon/Menu Icon/Equipment_Icon.PNG",
            Fn2Icon = "Assets/@Resources/Sprite/Icon/Menu Icon/Skill_Icon.PNG",
            PortraitSprite = "Assets/@Resources/Sprite/UI/Image/NPC/blacksmith/blacksmith_normal.png",
            BgSprite = "Assets/@Resources/Sprite/Background/Town/Background_Forge.png",
            NpcSprite = "Assets/@Resources/Sprite/NPC/Blacksmith_Standing_Idle.png",
        },
        new Building
        {
            Key = "Shop", TempButton = "TempButton2",
            Fn1 = "ShopBuyButton", Fn2 = "ShopSellButton",
            IconSprite = "Assets/@Resources/Sprite/Icon/Menu Icon/Shop_Icon.png",
            Fn1Icon = "Assets/@Resources/Sprite/UI/Image/MapSelect/Img_Potion.PNG",
            Fn2Icon = "Assets/@Resources/Sprite/Icon/Menu Icon/Icon_Inven.png",
            PortraitSprite = "Assets/@Resources/Sprite/UI/Image/NPC/shopkeeper/shopkeeper_profile.png",
            BgSprite = "Assets/@Resources/Sprite/Background/Town/Background_Shop.png",
            NpcSprite = "Assets/@Resources/Sprite/NPC/Shopkeeper_Standing_Idle.png",
        },
    };

    // 수도원은 그룹·배경이 이미 있어 새로 만들 게 없다. 다만 안에 서 있는 사람이
    //  **연금술사**였다(연금공방 프리팹을 복제해 만든 흔적) — 사람과 초상화만 갈아끼운다.
    static readonly Building MONASTERY = new Building
    {
        Key = "Monastery",
        PortraitSprite = "Assets/@Resources/Sprite/UI/Image/NPC/priestess/priestess_profile.png",
        NpcSprite = "Assets/@Resources/Sprite/NPC/Priestess_Standing_Idle.png",
    };

    [MenuItem("Tools/Build Village Buildings (대장간·상점 프리팹 생성)")]
    static void Build()
    {
        EnsureSingleSprites();
        BuildGroups();
        foreach (var b in BUILDINGS) BuildBackground(b);
        FixMonasteryNpc();
        BuildShopPopup();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[VillageBuild] 완료 — Tools/Register Village Building Addressables 를 이어서 실행하세요.");
    }

    // ── 3) 상점 팝업 — 소지품 팝업을 복제한다 ─────────────────────────────
    //  필요한 게 정확히 같다(프레임 + 세로 스크롤 + 슬롯 격자 + 닫기). 새로 저작할 이유가 없다.
    //  자식 이름만 Shop*으로 맞추면 UI_ShopPopup의 enum이 그대로 붙는다.
    const string SRC_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_InventoryPopup.prefab";
    const string DST_POPUP = "Assets/@Resources/Prefab/UI/Popup/UI_ShopPopup.prefab";

    static void BuildShopPopup()
    {
        if (!File.Exists(DST_POPUP))
        {
            if (!AssetDatabase.CopyAsset(SRC_POPUP, DST_POPUP))
            {
                Debug.LogError("[VillageBuild] 상점 팝업 복제 실패");
                return;
            }
            Debug.Log("[VillageBuild] UI_ShopPopup.prefab 생성");
        }

        GameObject root = PrefabUtility.LoadPrefabContents(DST_POPUP);
        try
        {
            root.name = "UI_ShopPopup";

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                t.name = t.name.Replace("Inventory", "Shop");

            // 소지품 전용 부속은 상점에서 의미가 없다 — 켜두면 빈 칸이 남아 고장으로 보인다.
            foreach (string n in new[] { "ImportantThingsList", "ImportantThingsBox",
                                         "ImportantBagBtn", "ShopSortBtn" })
            {
                Transform t = Find(root.transform, n);
                if (t != null) t.gameObject.SetActive(false);
            }

            // 스크립트 교체: UI_InventoryPopup -> UI_ShopPopup
            var old = root.GetComponent<UI_InventoryPopup>();
            if (old != null) UnityEngine.Object.DestroyImmediate(old, true);
            if (root.GetComponent<UI_ShopPopup>() == null) root.AddComponent<UI_ShopPopup>();

            PrefabUtility.SaveAsPrefabAsset(root, DST_POPUP);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ── 1) UI_Village1BtnGroup 안에 건물 그룹 2개를 만든다 ──────────────────
    static void BuildGroups()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GROUP_PREFAB);
        try
        {
            Transform src = Find(root.transform, SRC_GROUP);
            if (src == null) { Debug.LogError($"[VillageBuild] {SRC_GROUP} 없음"); return; }

            foreach (var b in BUILDINGS)
            {
                string groupName = $"Village1{b.Key}BtnGroup";
                Transform grp = Find(root.transform, groupName);

                if (grp == null)
                {
                    GameObject copy = UnityEngine.Object.Instantiate(src.gameObject, src.parent);
                    copy.name = groupName;
                    copy.transform.SetSiblingIndex(src.GetSiblingIndex() + 1);
                    Rename(copy.transform, b);
                    grp = copy.transform;
                    Debug.Log($"[VillageBuild] {groupName} 생성");
                }

                grp.gameObject.SetActive(false);      // 진입 연출이 켠다
                ApplyGroupSprites(grp, b);
                ApplyLobbyButton(root.transform, b);
            }

            PrefabUtility.SaveAsPrefabAsset(root, GROUP_PREFAB);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>복제본의 모든 이름에서 Monastery 흔적을 지우고 기능 버튼을 개명한다.</summary>
    static void Rename(Transform grp, Building b)
    {
        foreach (Transform t in grp.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.Replace("Monastery", b.Key);

            // 축성 버튼 -> 이 건물의 첫 기능 버튼
            n = n.Replace($"{b.Key}ClassButton", b.Fn1);
            n = n.Replace($"{b.Key}CustomOrderButtonNewTxt", b.Fn1 + "NewTxt");

            // 빈칸1 -> 두 번째 기능 버튼
            n = n.Replace($"Village1{b.Key}NothingButton1", b.Fn2);

            t.name = n;
        }
    }

    static void ApplyGroupSprites(Transform grp, Building b)
    {
        SetSprite(grp, $"Village1{b.Key}TalkButtonIcon", b.PortraitSprite);
        SetSprite(grp, $"Village1{b.Key}TalkButtonIcon2", b.PortraitSprite);
        SetSprite(grp, b.Fn1 + "Icon", b.Fn1Icon);
        SetSprite(grp, b.Fn2 + "Icon", b.Fn2Icon);

        // 수도원에서 딸려온 [NEW] 뱃지 — 해금 알림용인데 복제본은 늘 켜진 채였다.
        Transform badge = Find(grp, b.Fn1 + "NewTxt");
        if (badge != null) badge.gameObject.SetActive(false);
    }

    /// <summary>로비 빈칸(TempButtonN)을 이 건물 버튼으로 바꾼다.</summary>
    static void ApplyLobbyButton(Transform root, Building b)
    {
        string btnName = $"{b.Key}Button";
        Transform btn = Find(root, btnName) ?? Find(root, b.TempButton);
        if (btn == null) { Debug.LogWarning($"[VillageBuild] {b.TempButton} 없음"); return; }

        btn.name = btnName;

        // 빈칸의 자식은 Img_TempButton1_03 / Txt_TempButton1_02 처럼 임시 이름이다.
        //  자동 바인딩은 이름으로 걸리므로 {건물}ButtonIcon / {건물}ButtonText 로 맞춘다.
        foreach (Transform c in btn)
        {
            if (c.GetComponent<TMPro.TMP_Text>() != null) c.name = btnName + "Text";
            else if (c.GetComponent<Image>() != null) c.name = btnName + "Icon";
        }

        SetSprite(btn, btnName + "Icon", b.IconSprite);
    }

    // ── 2) 건물 배경 프리팹 ────────────────────────────────────────────────
    static void BuildBackground(Building b)
    {
        string dst = $"{MAP_DIR}/Village1_{b.Key}_BG.prefab";
        if (!File.Exists(dst))
        {
            if (!AssetDatabase.CopyAsset(SRC_BG, dst))
            {
                Debug.LogError($"[VillageBuild] 배경 프리팹 복제 실패: {dst}");
                return;
            }
            Debug.Log($"[VillageBuild] {Path.GetFileName(dst)} 생성");
        }

        GameObject root = PrefabUtility.LoadPrefabContents(dst);
        try
        {
            root.name = $"Village1_{b.Key}_BG";
            root.SetActive(true);   // 수도원 배경이 비활성으로 저작돼 있어 화면이 검게 나온 전례가 있다

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                t.name = t.name.Replace("Guild", b.Key).Replace("Receptionist", $"{b.Key}NPC");

            SetSpriteRenderer(root.transform, $"Village1_{b.Key}_BG_Img", b.BgSprite);
            SetSpriteRenderer(root.transform, $"Village1_{b.Key}_{b.Key}NPC", b.NpcSprite);
            SetSpriteRenderer(root.transform, $"Village1_{b.Key}NPC", b.NpcSprite);

            // ⚠️ 여기가 함정이었다. 복제본은 길드 접수원의 Animator(Town_NPC)를 그대로 달고 온다.
            //  Animator는 매 프레임 SpriteRenderer.sprite를 덮어써서, 프리팹에 무엇을 물리든
            //  화면에는 접수원이 나왔다 — 대장간과 상점의 NPC가 같은 사람으로 보인 이유다.
            SetupNpcAnimator(root.transform, b);

            PrefabUtility.SaveAsPrefabAsset(root, dst);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>수도원 안 사람을 연금술사에서 사제로 갈아끼운다.
    /// 그룹·배경은 이미 있으므로 NPC 노드 이름과 스프라이트, 그리고 [대화] 초상화만 손댄다.</summary>
    static void FixMonasteryNpc()
    {
        const string bgPath = MAP_DIR + "/Village1_Monastery_BG.prefab";
        if (!File.Exists(bgPath)) { Debug.LogWarning("[VillageBuild] 수도원 배경 프리팹 없음"); return; }

        GameObject root = PrefabUtility.LoadPrefabContents(bgPath);
        try
        {
            // 노드 이름부터 정직하게. 다른 건물과 같은 규칙(Village1_{Key}_{Key}NPC)으로 맞춘다.
            Transform npc = Find(root.transform, "Village1_Alchemy_Alchemist")
                            ?? Find(root.transform, "Village1_Monastery_MonasteryNPC");
            if (npc == null) { Debug.LogWarning("[VillageBuild] 수도원 NPC 노드 없음"); return; }
            npc.name = "Village1_Monastery_MonasteryNPC";

            SetSpriteRenderer(root.transform, npc.name, MONASTERY.NpcSprite);
            SetupNpcAnimator(root.transform, MONASTERY);

            PrefabUtility.SaveAsPrefabAsset(root, bgPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        // [대화] 버튼 초상화도 연금술사였다.
        GameObject grpRoot = PrefabUtility.LoadPrefabContents(GROUP_PREFAB);
        try
        {
            EnsureSingle(MONASTERY.PortraitSprite);
            SetSprite(grpRoot.transform, "Village1MonasteryTalkButtonIcon", MONASTERY.PortraitSprite);
            SetSprite(grpRoot.transform, "Village1MonasteryTalkButtonIcon2", MONASTERY.PortraitSprite);
            PrefabUtility.SaveAsPrefabAsset(grpRoot, GROUP_PREFAB);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(grpRoot);
        }
    }

    const string ANIM_DIR = "Assets/@Resources/Animation/Town_NPC";
    const float NPC_FPS = 6f;   // 길드 접수원 Idle과 같은 체감 속도

    /// <summary>건물 NPC에 자기 애니메이션을 붙인다.
    /// 시트가 여러 장이면 Idle 클립 + 전용 컨트롤러를 만들고, 한 장뿐이면 Animator를 떼어
    /// SpriteRenderer가 물린 그림을 그대로 쓰게 한다(빈 컨트롤러보다 정직하다).</summary>
    static void SetupNpcAnimator(Transform root, Building b)
    {
        Transform npc = Find(root, $"Village1_{b.Key}_{b.Key}NPC") ?? Find(root, $"Village1_{b.Key}NPC");
        if (npc == null) { Debug.LogWarning($"[VillageBuild] {b.Key} NPC 노드 없음"); return; }

        var anim = npc.GetComponent<Animator>();
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(b.NpcSprite).OfType<Sprite>()
            .OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray();

        if (frames.Length <= 1)
        {
            if (anim != null) UnityEngine.Object.DestroyImmediate(anim, true);
            Debug.Log($"[VillageBuild] {b.Key} NPC는 정지 그림 — Animator 제거");
            return;
        }

        string clipPath = $"{ANIM_DIR}/{b.Key}NPC_Idle.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip { frameRate = NPC_FPS };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite",
            };
            var keys = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / NPC_FPS, value = frames[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AssetDatabase.CreateAsset(clip, clipPath);
            Debug.Log($"[VillageBuild] {b.Key}NPC_Idle.anim 생성 ({frames.Length}프레임)");
        }

        string ctrlPath = $"{ANIM_DIR}/{b.Key}_NPC.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(ctrlPath)
                   ?? UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPathWithClip(ctrlPath, clip);

        if (anim == null) anim = npc.gameObject.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        Debug.Log($"[VillageBuild] {b.Key} NPC 컨트롤러 = {ctrl.name}");
    }

    /// <summary>[대화] 초상화는 통짜 한 장이어야 한다.
    /// Multiple(sliced)로 들어오면 예전 크기 기준 rect가 남아 새 텍스처의 **빈 구석**을 가리킨다
    /// — 실제로 대장장이 초상화가 그렇게 빈 칸이 됐다.
    /// SetDirty만으로는 임포트 설정이 저장되지 않아 WriteImportSettingsIfDirty가 필요하다.</summary>
    // 새로 만든 그림은 PPU 100(유니티 기본)으로 들어온다. 기존 에셋과 안 맞추면
    //  같은 픽셀 크기인데 화면에서 2~3배 차이가 난다 — 사제가 플레이어 절반 키로 나왔다.
    const float NPC_PPU = 35f;    // Blacksmith_Standing_Idle 기준(접수원 36, 연금술사 40과 같은 급)
    const float TOWN_BG_PPU = 40f; // Background_Guild / Background_Monastery와 동일

    static void EnsureSingleSprites()
    {
        foreach (var b in BUILDINGS)
        {
            EnsureSingle(b.PortraitSprite);
            EnsurePpu(b.NpcSprite, NPC_PPU);
            EnsurePpu(b.BgSprite, TOWN_BG_PPU);
        }
        EnsureSingle(MONASTERY.PortraitSprite);
        EnsurePpu(MONASTERY.NpcSprite, NPC_PPU);
        AssetDatabase.Refresh();
    }

    static void EnsurePpu(string path, float ppu)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        bool dirty = false;

        if (!Mathf.Approximately(ti.spritePixelsPerUnit, ppu))
        {
            ti.spritePixelsPerUnit = ppu;
            dirty = true;
            Debug.Log($"[VillageBuild] PPU {ppu}로 맞춤: {Path.GetFileName(path)}");
        }

        // 마을 NPC는 pivot이 **발끝**이다 — 길드 접수원을 재보니 발끝 Y가 노드 Y와 정확히 같았다.
        //  기본값(Center)으로 두면 그림 높이가 조금만 달라도 발이 바닥을 뚫고 내려간다(실제로 그랬다).
        //  시트(Multiple)는 alignment가 슬라이스마다 따로라 양쪽 다 손봐야 한다.
        if (path.Contains("/NPC/"))
        {
            const int BOTTOM = (int)SpriteAlignment.BottomCenter;

            if (ti.spriteImportMode == SpriteImportMode.Single)
            {
                // Single의 pivot은 TextureImporter가 아니라 TextureImporterSettings에 있다.
                var st = new TextureImporterSettings();
                ti.ReadTextureSettings(st);
                if (st.spriteAlignment != BOTTOM)
                {
                    st.spriteAlignment = BOTTOM;
                    st.spritePivot = new Vector2(0.5f, 0f);
                    ti.SetTextureSettings(st);
                    dirty = true;
                }
            }
            else
            {
                var sheet = ti.spritesheet;
                bool changed = false;
                for (int i = 0; i < sheet.Length; i++)
                {
                    if (sheet[i].alignment == BOTTOM) continue;
                    sheet[i].alignment = BOTTOM;
                    sheet[i].pivot = new Vector2(0.5f, 0f);
                    changed = true;
                }
                if (changed) { ti.spritesheet = sheet; dirty = true; }
            }

            if (dirty) Debug.Log($"[VillageBuild] pivot을 발끝으로: {Path.GetFileName(path)}");
        }

        if (!dirty) return;

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        AssetDatabase.WriteImportSettingsIfDirty(path);
    }

    static void EnsureSingle(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null || ti.spriteImportMode == SpriteImportMode.Single) return;

        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritesheet = new SpriteMetaData[0];
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        AssetDatabase.WriteImportSettingsIfDirty(path);
        Debug.Log($"[VillageBuild] 초상화를 Single로 재임포트: {Path.GetFileName(path)}");
    }

    // ── 공통 ───────────────────────────────────────────────────────────────
    static Transform Find(Transform root, string name)
        => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);

    static Sprite Load(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        // 시트로 잘린 텍스처면 첫 서브스프라이트를, 아니면 통짜 스프라이트를 쓴다.
        var all = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        return all.Length > 0 ? all[0] : AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void SetSprite(Transform root, string childName, string spritePath)
    {
        Transform t = Find(root, childName);
        if (t == null) { Debug.LogWarning($"[VillageBuild] 자식 없음: {childName}"); return; }

        var img = t.GetComponent<Image>();
        if (img == null) return;

        Sprite sp = Load(spritePath);
        if (sp == null)
        {
            Debug.LogWarning($"[VillageBuild] 스프라이트 아직 없음: {spritePath} ({childName}) — 그림 생성 후 다시 실행하세요");
            return;
        }
        img.sprite = sp;
        img.color = Color.white;
    }

    static void SetSpriteRenderer(Transform root, string childName, string spritePath)
    {
        Transform t = Find(root, childName);
        if (t == null) return;

        var sr = t.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Sprite sp = Load(spritePath);
        if (sp == null)
        {
            Debug.LogWarning($"[VillageBuild] 스프라이트 아직 없음: {spritePath} ({childName})");
            return;
        }
        sr.sprite = sp;
        sr.color = Color.white;
    }
}
