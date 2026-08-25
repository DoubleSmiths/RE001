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
//  ⚠️ 처음엔 팩 프리팹(FX_splash_crystal_air)을 그대로 재생하려 했으나 막혔다 —
//     그 팩의 재질이 우리 프로젝트에서 **URP/Lit 으로 떨어져 있어** 파티클 색을 안 먹고
//     (텍스처의 채널 패킹이 그대로 나와 형광 초록으로 터진다), 버텍스 스트림도 옛 셰이더
//     기준이라 UV 가 어긋나 아틀라스의 엉뚱한 칸(해골·송곳니)이 통째로 그려졌다.
//     그래서 **필요한 조각만 잘라** 스프라이트로 등록해 쓴다(모양은 원작 그대로다).
public static class ForgeFxBuilder
{
    const string GROUP = "VFX";

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
