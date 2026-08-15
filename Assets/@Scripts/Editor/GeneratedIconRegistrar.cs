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
        // 드롭 재료 아이콘 — 몬스터 원형 30종과 1:1 대응(ADR 참조)
        "Assets/@Resources/Sprite/Icon/Material/Generated",
        // 몬스터 도감 카드 503종 — 원형 30종을 돌려쓰던 것을 종별 고유 그림으로 교체
        "Assets/@Resources/Sprite/Monster/Card",
        // 탐험 지역 배경 25종 — 6종을 돌려쓰던 것을 지역별 고유 그림으로 교체
        "Assets/@Resources/Sprite/Map/Region",
    };

    [MenuItem("Tools/Register Generated Icons (아이콘 임포트+어드레서블)")]
    public static void Register()
    {
        // 파이썬으로 갓 떨군 PNG는 아직 AssetDatabase에 없다 → AssetPathToGUID가 빈 문자열을 주고
        // 조용히 건너뛴다(파일은 있는데 어드레서블엔 없는 상태). 먼저 임포트시킨다.
        AssetDatabase.Refresh();

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

                    // 몬스터는 pivot을 **bottom-middle**로 둔다. center(기본값)면 스프라이트 절반이
                    // 지면 아래로 내려가 "땅에 박힌" 것처럼 보인다. 발밑이 기준점이어야
                    // 스폰 Y를 지면 높이로만 주면 정확히 서 있게 된다.
                    // 인게임에 서는 몬스터만 발밑 기준. 도감 카드(/Monster/Card)는 UI 아이콘이라 center.
                    bool isMonster = folder.EndsWith("/Monster/Generated");
                    var wanted = isMonster ? SpriteAlignment.BottomCenter : SpriteAlignment.Center;
                    var st = new TextureImporterSettings();
                    ti.ReadTextureSettings(st);
                    if (st.spriteAlignment != (int)wanted)
                    {
                        st.spriteAlignment = (int)wanted;
                        st.spritePivot = isMonster ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
                        ti.SetTextureSettings(st);
                        dirty = true;
                    }
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
                // PreLoad에 넣지 않는다. 503개(클립 2,515개 동반)를 시작 시 전부 받으면 로딩이 30초를 넘긴다.
                // ResourceManager.Load가 캐시에 없을 때 동기 로드하므로 몬스터가 처음 스폰될 때 받아온다.
                entry.SetLabel(PRELOAD_LABEL, false, false, false);
                ctrlCount++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(settings);
        Debug.Log($"[IconReg] 임포트 설정 {imported}개 갱신, 스프라이트 {registered}개 + 애니메이터 {ctrlCount}개 등록 (그룹 {group.Name})");
    }
}
