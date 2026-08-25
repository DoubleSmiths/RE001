using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// 원작 대장간 연출의 파티클을 우리 프로젝트로 옮긴다.
//
//  원작(RE001_Share)의 망치 타격은 `FX_splash_crystal_air` 를 씬에서 값만 바꿔 쓴다.
//  그 팩(Assets/IMPORTS/SkillEf)은 우리 프로젝트에도 그대로 들어와 있고,
//  **재질이 캔버스 경로에서 실제로 그려진다**(물약창이 FX_splash_spark_air 를 같은 방식으로 쓴다).
//  그동안 손으로 그린 대체품을 써 온 것은 우리 프리팹의 GlowDot_01_1 이 built-in
//  Mobile/Particles/Additive 라 UIParticle 로도 아무것도 안 그려졌기 때문이다 — 출처가 다르다.
//
//  이 메뉴는 팩 프리팹을 복제해 **원작 씬의 오버라이드만** 얹어 저장하고 어드레서블에 등록한다.
public static class ForgeFxBuilder
{
    const string SRC = "Assets/IMPORTS/SkillEf/Prefabs/Stylized_Element_Splash_vol.2/FX_splash_crystal_air.prefab";
    const string DIR = "Assets/@Resources/Prefab/FX";
    const string DST = DIR + "/FX_HammerSmash.prefab";
    const string ADDRESS = "FX_HammerSmash";
    const string GROUP = "VFX";
    const string SPRITE_GROUP = "VFX";

    [MenuItem("Tools/Forge/타격 파티클 프리팹 생성 (원작 값)")]
    public static void Build()
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(SRC);
        if (src == null) { Debug.LogError($"[ForgeFx] 원본 없음: {SRC}"); return; }

        if (!Directory.Exists(DIR)) Directory.CreateDirectory(DIR);

        var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        go.name = "FX_HammerSmash";

        // ── 원작이 쓰지 않는 갈래는 지운다 ──
        //  sharp_circle 은 아틀라스의 **초승달 칼날**이다. 아무리 줄여도 타격 뒤에 흰 조각으로
        //  남는데 원작 프레임엔 그런 게 없다(원작의 흰 줄무늬는 wind 텍스처가 이미 낸다).
        foreach (string n in new[] { "particle", "center_light", "shining_highlight", "sharp_circle" })
        {
            var t = go.transform.Find(n);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // ── 루트는 아무것도 뿜지 않는다(원작도 껍데기다) ──
        var root = go.GetComponent<ParticleSystem>();
        if (root != null)
        {
            var m = root.main;
            m.duration = 0.05f;
            m.startLifetime = 0.0001f;
            m.startColor = new Color(0.56078434f, 1f, 0.9533545f, 0f);
            // ⚠️ 알파 0 이라고 안 보이는 게 아니다 — 프리멀티플라이드 블렌딩에서는 그대로 그려져
            //    청록색 판때기가 타격 순간에 튀어나왔다. 방출 자체를 끈다.
            var rem = root.emission;
            rem.enabled = false;
        }

        // ── 원작 씬의 자식 오버라이드 ──
        // 연기 폭을 원작에 맞추면 구체가 과하게 커진다(원작 비율: 연기 342 : 구체 90).
        //  구체 계열만 상대적으로 줄인다.
        Tune(go, "center_light_circle", 1f, new Color(1f, 1f, 1f, 1f), 1.0f, ORB_SIZE_MUL);
        // sharp_circle 은 아틀라스의 **초승달 칼날** 모양이다. 연기 폭에 맞춰 통째로 키우면
        //  이게 화면을 덮는 흰 칼날 무더기가 된다(원작엔 그런 게 없다). 크게 줄인다.
        Tune(go, "twinkle",             1f, new Color(1f, 1f, 1f, 0.7058824f), 1.0f, ORB_SIZE_MUL);
        // 연기 — 팩 기본은 푸른 회색이다. 원작은 중성 회색으로 바꿔 쓴다.
        // ⚠️ 원작 저작값은 알파 1 이지만 우리 화면에선 훨씬 짙게 합성된다
        //    (폭을 원작에 맞추고 나니 타격 구역 평균이 원작의 3배가 됐다).
        //    폭·색·시간은 원작 그대로 두고 **농도와 수명만** 실측으로 눌렀다.
        Tune(go, "wind", 1f, new Color(0.6698113f, 0.6698113f, 0.6698113f, WIND_ALPHA), -1f);
        SetLifetime(go, "wind", WIND_LIFE_MUL);
        Tune(go, "shining_center",      0.5f, Color.white, -1f, SHINE_SIZE_MUL);

        // ── 재질을 갈아 끼운다 ──
        //  ⚠️ 팩의 재질(Mat_fx_elements_set_*)은 우리 프로젝트에서 **URP/Lit 으로 떨어져 있다**
        //     (_BaseColor/_Metallic/_Smoothness 가 남아 있는 게 증거다). 그 셰이더는 파티클 색을
        //     먹지 않아 텍스처의 채널 패킹이 그대로 나온다 — 타격이 형광 초록으로 터졌다.
        //     아틀라스의 **알파 채널이 모양을 온전히 담고 있으므로**, RGB를 흰색으로 채운
        //     마스크 텍스처를 만들고 파티클 셰이더로 칠한다. 그래야 원작처럼 흰/회색이 된다.
        AssignWhiteMaterials(go);

        // 전부 한 번에 자동 재생되게 (UIFx.PlayParticle 이 Play 를 부르지만 보험)
        //  수명도 함께 줄인다 — 원작은 타격 0.35초 뒤면 무대가 이미 깨끗하다.
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            m.playOnAwake = true;
            m.loop = false;
            m.startLifetimeMultiplier *= LIFE_MUL;
        }

        PrefabUtility.SaveAsPrefabAsset(go, DST);
        Object.DestroyImmediate(go);
        AssetDatabase.Refresh();

        Register(DST, ADDRESS);
        Debug.Log($"[ForgeFx] {DST} 생성 + 어드레서블 '{ADDRESS}' 등록");
    }

    /// <summary>길이 / 시작색 / 두 번째 이후 버스트 시각을 원작 값으로.</summary>
    const float ORB_SIZE_MUL = 0.18f;
    const float SHARP_SIZE_MUL = 0.09f;
    const float SHINE_SIZE_MUL = 0.12f;   // 아틀라스의 큰 초승달 — 원작엔 이 크기로 안 나온다
    const float LIFE_MUL       = 0.40f;   // 원작은 타격 0.35초 뒤면 무대가 깨끗하다
    const float WIND_ALPHA    = 0.34f;
    const float WIND_LIFE_MUL = 0.55f;

    static void SetLifetime(GameObject root, string child, float mul)
    {
        var t = root.transform.Find(child);
        var ps = t != null ? t.GetComponent<ParticleSystem>() : null;
        if (ps == null) return;
        var m = ps.main;
        m.startLifetimeMultiplier *= mul;
    }

    static void Tune(GameObject root, string child, float duration, Color? startColor, float burstTime,
                     float sizeMul = 1f)
    {
        var t = root.transform.Find(child);
        if (t == null) { Debug.LogWarning($"[ForgeFx] 자식 없음: {child}"); return; }

        var ps = t.GetComponent<ParticleSystem>();
        if (ps == null) return;

        var m = ps.main;
        m.duration = duration;
        if (sizeMul != 1f) m.startSizeMultiplier *= sizeMul;
        if (startColor.HasValue) m.startColor = new ParticleSystem.MinMaxGradient(startColor.Value);

        // ⚠️ 팩 기본색은 **초록·파랑**이다. startColor 만 바꿔서는 안 되고
        //    ColorOverLifetime 그라디언트의 색 키까지 흰색으로 덮어야 원작 그림이 된다
        //    (그대로 두면 타격이 형광 초록으로 터진다).
        var col = ps.colorOverLifetime;
        if (col.enabled)
        {
            var g = col.color;
            if (g.mode == ParticleSystemGradientMode.Gradient || g.mode == ParticleSystemGradientMode.TwoGradients)
            {
                var src = g.gradient;
                var keys = src.colorKeys;
                for (int i = 0; i < keys.Length; i++) keys[i].color = Color.white;
                var ng = new Gradient();
                ng.SetKeys(keys, src.alphaKeys);
                col.color = new ParticleSystem.MinMaxGradient(ng);
            }
            else if (g.mode == ParticleSystemGradientMode.Color)
            {
                var c = g.color; col.color = new Color(1f, 1f, 1f, c.a);
            }
        }

        if (burstTime >= 0f)
        {
            var em = ps.emission;
            int n = em.burstCount;
            if (n > 1)
            {
                var bursts = new ParticleSystem.Burst[n];
                em.GetBursts(bursts);
                for (int i = 1; i < n; i++) bursts[i].time = burstTime;   // 첫 버스트(t=0.05)는 그대로
                em.SetBursts(bursts);
            }
        }
    }

    const string MAT_DIR = "Assets/@Resources/Material/FX";

    /// <summary>자식마다 원래 쓰던 아틀라스를 알아내 **흰색 마스크판** 재질로 바꾼다.</summary>
    static void AssignWhiteMaterials(GameObject root)
    {
        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);

        foreach (var psr in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var src = psr.sharedMaterial;
            if (src == null) continue;

            var tex = src.mainTexture;
            string atlas = tex != null ? tex.name : "Tex_fx_elements_set_01";
            var mat = GetWhiteMaterial(atlas);
            if (mat != null) psr.sharedMaterial = mat;

            // ⚠️ 재질을 갈면 **버텍스 스트림도 같이 맞춰야 한다.**
            //    팩 셰이더가 요구하던 커스텀 스트림이 남아 있으면 UV가 어긋나
            //    아틀라스의 엉뚱한 칸(해골·송곳니 따위)이 통째로 그려진다 —
            //    타격 순간에 흰 송곳니 무더기가 화면을 덮던 것의 정체다.
            var streams = new System.Collections.Generic.List<ParticleSystemVertexStream>
            {
                ParticleSystemVertexStream.Position,
                ParticleSystemVertexStream.Normal,
                ParticleSystemVertexStream.Color,
                ParticleSystemVertexStream.UV,
            };
            psr.SetActiveVertexStreams(streams);
        }
    }

    static Material GetWhiteMaterial(string atlas)
    {
        string path = $"{MAT_DIR}/{atlas}_white.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit")
              ?? Shader.Find("Universal Render Pipeline/Unlit")
              ?? Shader.Find("Sprites/Default");
        if (sh == null) { Debug.LogError("[ForgeFx] 파티클 셰이더를 못 찾았다"); return null; }
        Debug.Log($"[ForgeFx] 셰이더: {sh.name}");

        var white = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/@Resources/Sprite/FX/{atlas}_white.png");
        if (white == null) { Debug.LogWarning($"[ForgeFx] 흰 마스크 없음: {atlas}"); return null; }

        mat = new Material(sh);
        mat.name = atlas + "_white";
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", white);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", white);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        // 알파 블렌드 + 깊이 쓰기 없음 (UI 위에 겹쳐 그린다)
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
        if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
        if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        // 프리멀티플라이드는 쓰지 않는다 — 알파 0 인 입자가 그대로 그려진다.
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }

    // ── 원작 아틀라스의 낱 조각을 스프라이트로 등록한다 ──
    //  원작의 타격은 이 조각들(흰 원 / 구름 / 4각별 / 스크래치)로 이뤄져 있다.
    //  파티클로 재생하려 했으나 팩 재질이 URP/Lit 으로 떨어져 있고 UV 애니메이션이
    //  아틀라스의 엉뚱한 칸(해골·송곳니)을 골라 그리는 문제가 있어,
    //  **필요한 조각만 잘라** 쓰기로 했다(모양은 원작 그대로다).
    static readonly string[] FX_SPRITES =
    {
        "FX_Orb", "FX_Star", "FX_Cloud", "FX_Wisp1", "FX_Wisp2", "FX_Wisp3", "FX_Streak",
    };

    [MenuItem("Tools/Forge/타격 스프라이트 등록 (아틀라스 조각)")]
    public static void RegisterFxSprites()
    {
        foreach (var n in FX_SPRITES)
        {
            string path = $"Assets/@Resources/Sprite/FX/{n}.png";
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { Debug.LogWarning($"[ForgeFx] 없음: {path}"); continue; }

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();

            // ⚠️ 이 프로젝트의 스프라이트 주소 규약은 **"이름.sprite"** 다.
            //    ResourceManager.LoadSync 가 그 접미사를 보고 서브오브젝트 키로 바꿔 찾는다
            //    (FX_Orb.sprite → "FX_Orb.sprite[FX_Orb]"). 그냥 "FX_Orb" 로 넣으면
            //    Load<Sprite> 가 텍스처만 잡고 null 을 돌려준다.
            Register(path, n + ".sprite");
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[ForgeFx] 스프라이트 {FX_SPRITES.Length}종 등록");
    }

    static void Register(string path, string address)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("[ForgeFx] Addressable 설정 없음"); return; }

        var group = settings.groups.FirstOrDefault(g => g != null && g.Name == GROUP) ?? settings.DefaultGroup;
        string guid = AssetDatabase.AssetPathToGUID(path);
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = address;

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();
    }
}
