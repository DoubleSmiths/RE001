using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 플레이어 스프라이트 시트의 피벗을 **프레임마다 발끝**에 맞춘다.
///
/// 왜 필요했나 — 액티브 스킬 모션에 쓰는 Player_02/attack_Sheet2는 셀 안에서 캐릭터가
/// 높이 그려져 있어서, 시트 공통 피벗을 쓰면 발끝이 피벗보다 **0.39유닛 위**에 있었다.
/// 그래서 스킬을 쓰는 동안만 플레이어가 공중에 떠 보였다.
/// (기준인 Player_01/idle_Sheet은 모든 프레임이 정확히 0.0이다 — 이 시트들만 어긋나 있었다.)
///
/// 피벗 x는 셀 가운데를 유지하고 y만 그 프레임의 실제 발끝으로 내린다.
/// 임포터 설정은 SetDirty로는 저장되지 않는다 — WriteImportSettingsIfDirty가 필요하다.
/// </summary>
public static class PlayerPivotFixer
{
    static readonly string[] DIRS =
    {
        "Assets/@Resources/Sprite/Player/Player_01",
        "Assets/@Resources/Sprite/Player/Player_02",
    };

    [MenuItem("Tools/플레이어 스프라이트 발끝 피벗 정렬")]
    public static void Fix()
    {
        int files = 0, frames = 0;
        var moved = new List<string>();

        foreach (var dir in DIRS)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { dir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null || ti.spriteImportMode != SpriteImportMode.Multiple) continue;

                var tex = LoadReadable(path);
                if (tex == null) continue;

                // LoadReadable이 재임포트를 걸었을 수 있어 임포터를 다시 잡는다(오래된 참조에 쓰면 유실).
                ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;

                var sheet = ti.spritesheet;
                bool dirty = false;
                float maxShift = 0f;

                for (int i = 0; i < sheet.Length; i++)
                {
                    var r = sheet[i].rect;
                    int feet = FeetRow(tex, r);
                    if (feet < 0) continue;                       // 빈 프레임은 그대로 둔다

                    var piv = new Vector2(0.5f, feet / r.height);
                    if (Mathf.Abs(piv.y - sheet[i].pivot.y) < 0.001f
                        && sheet[i].alignment == (int)SpriteAlignment.Custom) continue;

                    maxShift = Mathf.Max(maxShift, Mathf.Abs(piv.y - sheet[i].pivot.y) * r.height);
                    sheet[i].alignment = (int)SpriteAlignment.Custom;
                    sheet[i].pivot = piv;
                    dirty = true;
                    frames++;
                }

                if (!dirty) continue;
                ti.spritesheet = sheet;
                AssetDatabase.WriteImportSettingsIfDirty(path);   // SetDirty만으로는 저장되지 않는다
                files++;
                moved.Add($"{System.IO.Path.GetFileName(path)}(최대 {maxShift:F1}px)");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[PlayerPivot] 시트 {files}개 / 프레임 {frames}개 발끝 정렬 — {string.Join(", ", moved)}");
    }

    /// <summary>셀 안에서 불투명 픽셀이 가장 아래에 있는 행(셀 하단 기준 px). 없으면 -1.</summary>
    static int FeetRow(Texture2D tex, Rect r)
    {
        int x0 = (int)r.x, y0 = (int)r.y, w = (int)r.width, h = (int)r.height;
        var px = tex.GetPixels(x0, y0, w, h);
        for (int y = 0; y < h; y++)                 // 텍스처 y는 아래에서 위로 증가
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
                if (px[row + x].a > 0.06f) return y;
        }
        return -1;
    }

    /// <summary>GetPixels를 쓰려면 읽기 가능해야 한다. 원본 설정을 건드리지 않고 임시로만 켠다.</summary>
    static Texture2D LoadReadable(string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return null;
        if (ti.isReadable) return AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        ti.isReadable = true;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        copy.SetPixels(tex.GetPixels());
        copy.Apply();
        ti.isReadable = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return copy;
    }
}
