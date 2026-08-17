using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 몬스터 애니메이션 프레임(Sprite/Monster/Anim/**)의 임포트 설정을 정리한다.
///
/// 왜 필요했나 — 프레임을 다시 자르면서 PNG 크기가 바뀌었는데, .meta에는 **옛 크기 기준의
/// 스프라이트 rect**가 남아 있었다. 범위를 벗어난 rect라 스프라이트가 아예 로드되지 않았다
/// (LoadAssetAtPath<Sprite> 가 null). 눈에는 파일이 멀쩡히 보여 더 헷갈린다.
///
/// 새 자르기는 내용을 **아래에 맞춰** 붙이므로 발밑 여백이 없다 → pivot은 bottom-center가 정답이다.
/// (예전엔 아트마다 바닥 여백이 달라 pivot으로 흡수해야 했지만, 이제 자르기가 그걸 없앤다)
/// </summary>
public static class AnimFrameImportFixer
{
    const string ROOT = "Assets/@Resources/Sprite/Monster/Anim";
    const float PPU = 64f;   // 아키타입 프레임 기준. 몬스터가 지면에서 약 2유닛 높이가 된다.

    [MenuItem("Tools/몬스터 애니 프레임 임포트 정리")]
    public static void Fix()
    {
        if (!Directory.Exists(ROOT)) { Debug.LogError($"[AnimFix] 폴더 없음: {ROOT}"); return; }

        var files = Directory.GetFiles(ROOT, "*.png", SearchOption.AllDirectories);
        int fixedCount = 0, skipped = 0;

        {
            foreach (string path in files)
            {
                string p = path.Replace('\\', '/');
                if (Path.GetFileName(p).StartsWith("_raw_")) { skipped++; continue; }   // 원본 스트립은 게임에 안 쓴다

                var ti = AssetImporter.GetAtPath(p) as TextureImporter;
                if (ti == null) { skipped++; continue; }

                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;   // 낱장이다. Multiple로 두면 옛 rect가 살아난다.
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.mipmapEnabled = false;
                ti.spritePixelsPerUnit = PPU;
                ti.spritesheet = new SpriteMetaData[0];          // 옛 크기 기준 rect를 비운다

                var st = new TextureImporterSettings();
                ti.ReadTextureSettings(st);
                st.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                st.spritePivot = new Vector2(0.5f, 0f);
                st.spriteMode = (int)SpriteImportMode.Single;
                ti.SetTextureSettings(st);

                // ⚠️ SetDirty만으로는 임포터 설정이 .meta에 안 남는다. 실제로 그 상태로 돌렸더니
                //    spriteMode가 2(Multiple)에 옛 rect가 그대로 살아 스프라이트가 계속 null이었다.
                //    WriteImportSettingsIfDirty로 .meta에 쓰고, 마지막에 한 번만 리프레시한다
                //    (파일마다 SaveAndReimport를 부르면 600개에서 몇 분씩 걸린다).
                AssetDatabase.WriteImportSettingsIfDirty(p);
                fixedCount++;
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log($"[AnimFix] 임포트 설정 {fixedCount}개 정리 / 건너뜀 {skipped} (PPU {PPU}, pivot bottom-center)");
    }
}
