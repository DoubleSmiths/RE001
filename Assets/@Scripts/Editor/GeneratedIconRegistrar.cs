using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// AetherForge로 생성한 아이콘(_tools/aether_icons.py 산출물)을 게임에서 쓸 수 있게 등록한다.
//  1) 픽셀아트용 임포트 설정 (Sprite / Point 필터 / 압축 없음) — 기본값이면 뭉개진다
//  2) 어드레서블 등록 (주소 = 파일명) — Managers.Resource.Load<Sprite>(SpriteName)이 주소로 찾기 때문
// Addressables 에디터 어셈블리는 unity-cli exec에서 접근이 안 돼 MenuItem으로 둔다.
public static class GeneratedIconRegistrar
{
    const string PRELOAD_LABEL = "PreLoad";

    static readonly string[] FOLDERS =
    {
        "Assets/@Resources/Sprite/Icon/Equipment/Generated",
        "Assets/@Resources/Sprite/Icon/Skill/Generated",
        // 몬스터 원형(128px). 기존 몬스터가 PPU 32에 64px(=2유닛)이므로 PPU 64면 크기가 맞는다.
        "Assets/@Resources/Sprite/Monster/Generated",
    };

    [MenuItem("Tools/Register Generated Icons (아이콘 임포트+어드레서블)")]
    public static void Register()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[IconReg] AddressableAssetSettings 없음");
            return;
        }

        var group = settings.FindGroup("Sprites") ?? settings.DefaultGroup;
        int imported = 0, registered = 0;
        var addresses = new List<string>();

        foreach (string folder in FOLDERS)
        {
            if (!Directory.Exists(folder)) continue;

            foreach (string path in Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
            {
                string assetPath = path.Replace('\\', '/');
                // 주소 끝에 ".sprite"가 있어야 한다. ResourceManager.LoadAllAsync는 PrimaryKey에
                // ".sprite"가 포함될 때만 Sprite로 적재하고, 아니면 Object(=Texture2D)로 적재해
                // Load<Sprite>의 캐스팅이 null이 된다. Load<Sprite>(name)은 내부에서 ".sprite"를 덧붙여 조회한다.
                string address = Path.GetFileNameWithoutExtension(assetPath) + ".sprite";

                // 1) 픽셀아트 임포트 설정
                var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti != null)
                {
                    bool dirty = false;
                    if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
                    if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                    if (ti.filterMode != FilterMode.Point) { ti.filterMode = FilterMode.Point; dirty = true; }
                    if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
                    if (ti.mipmapEnabled) { ti.mipmapEnabled = false; dirty = true; }
                    if (ti.spritePixelsPerUnit != 64f) { ti.spritePixelsPerUnit = 64f; dirty = true; }
                    if (dirty)
                    {
                        ti.SaveAndReimport();
                        imported++;
                    }
                }

                // 2) 어드레서블 등록
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) continue;

                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                if (entry != null)
                {
                    entry.address = address;
                    // ResourceManager.Load는 캐시만 조회한다. 시작 시 LoadAllAsync<Object>("PreLoad")가
                    // 적재하는 대상이어야 하므로 PreLoad 라벨이 없으면 런타임에서 절대 찾지 못한다.
                    settings.AddLabel(PRELOAD_LABEL, false);
                    entry.SetLabel(PRELOAD_LABEL, true, false, false);
                    addresses.Add(address);
                    registered++;
                }
            }
        }

        // 몬스터 애니메이터 컨트롤러 — MonsterController가 Load<RuntimeAnimatorController>(AnimController)로
        // 주소 조회한다. 스프라이트가 아니므로 ".sprite" 접미사는 붙이지 않는다(붙이면 Sprite로 적재 시도).
        int ctrlCount = 0;
        const string ANIM_ROOT = "Assets/@Resources/Animation/Monster/Generated";
        if (Directory.Exists(ANIM_ROOT))
        {
            foreach (string path in Directory.GetFiles(ANIM_ROOT, "*.controller", SearchOption.AllDirectories))
            {
                string assetPath = path.Replace('\\', '/');
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) continue;

                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                if (entry == null) continue;
                entry.address = Path.GetFileNameWithoutExtension(assetPath);
                settings.AddLabel(PRELOAD_LABEL, false);
                entry.SetLabel(PRELOAD_LABEL, true, false, false);
                ctrlCount++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(settings);
        Debug.Log($"[IconReg] 임포트 설정 {imported}개 갱신, 스프라이트 {registered}개 + 애니메이터 {ctrlCount}개 등록 (그룹 {group.Name})");
    }
}
