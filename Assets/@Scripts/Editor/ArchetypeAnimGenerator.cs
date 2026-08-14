using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// aether_anim.py 가 받아온 4프레임 아틀라스를 몬스터 애니메이터로 굽는다.
//
//   입력  Assets/@Resources/Sprite/Monster/Anim/Mon_{계열}_{n}/{Idle,Move,Attack,Die}_{0..3}.png
//   출력  Assets/@Resources/Animation/Monster/Archetype/Mon_{계열}_{n}/
//           Idle / Run / Attack / Die / OnHit .anim  +  .controller
//         Addressable 주소 = 폴더명(= CreatureData.AnimController)
//
// 상태 이름은 CreatureController가 CrossFade로 부르는 이름과 **정확히** 같아야 한다
//   Idle / Run / Attack / Die / OnHit  ← 하나라도 이름이 다르면 그 동작만 조용히 죽는다.
//
// 컨트롤러는 503개(몬스터마다 1개)가 아니라 **아키타입 30개**만 만든다.
//   503개를 만들던 시절엔 시작 로딩이 30초를 넘었다.
public static class ArchetypeAnimGenerator
{
    const string SRC_ROOT = "Assets/@Resources/Sprite/Monster/Anim";
    const string OUT_ROOT = "Assets/@Resources/Animation/Monster/Archetype";
    const string GROUP_NAME = "Generated";
    const int FRAMES = 4;

    // (상태 이름, 아틀라스 접두사, 총 길이(초), 반복)
    static readonly (string state, string src, float len, bool loop)[] CLIPS =
    {
        ("Idle",   "Idle",   0.80f, true),
        ("Run",    "Move",   0.50f, true),
        ("Attack", "Attack", 0.50f, false),
        ("Die",    "Die",    0.70f, false),
    };

    [MenuItem("Tools/몬스터 아키타입 애니메이터 생성")]
    public static void Generate()
    {
        if (!Directory.Exists(SRC_ROOT))
        {
            Debug.LogError($"[ArchAnim] 원본 폴더 없음: {SRC_ROOT}");
            return;
        }

        Directory.CreateDirectory(OUT_ROOT);
        string[] dirs = Directory.GetDirectories(SRC_ROOT).OrderBy(d => d).ToArray();
        int made = 0, skipped = 0, incomplete = 0;

        // ⚠️ AssetDatabase.StartAssetEditing()으로 감싸지 않는다.
        //    감싸면 CreateAnimatorControllerAtPath가 빈 상태기계를 만든다(과거에 503개를 그렇게 날렸다).
        try
        {
            for (int i = 0; i < dirs.Length; i++)
            {
                string src = dirs[i].Replace('\\', '/');
                string name = Path.GetFileName(src);
                EditorUtility.DisplayProgressBar("아키타입 애니메이터", name, (float)i / dirs.Length);

                var clips = BuildClips(src);
                if (clips == null) { incomplete++; continue; }     // 아직 생성 안 끝난 아키타입

                string dir = $"{OUT_ROOT}/{name}";
                string ctrlPath = $"{dir}/{name}.controller";

                // 이미 정상(상태 5개)이면 건너뛴다 — 재실행 가능하게.
                var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                if (existing != null && existing.layers.Length > 0 &&
                    existing.layers[0].stateMachine.states.Length >= CLIPS.Length + 1)
                {
                    skipped++;
                    continue;
                }

                if (Directory.Exists(dir)) AssetDatabase.DeleteAsset(dir);
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();

                foreach (var kv in clips)
                {
                    kv.Value.name = kv.Key;
                    AssetDatabase.CreateAsset(kv.Value, $"{dir}/{kv.Key}.anim");
                }

                var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
                var sm = ctrl.layers[0].stateMachine;
                foreach (var kv in clips)
                {
                    var st = sm.AddState(kv.Key);
                    st.motion = kv.Value;
                    if (kv.Key == "Idle") sm.defaultState = st;
                }
                made++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        int reg = RegisterAddressables();
        Debug.Log($"[ArchAnim] 생성 {made} / 건너뜀 {skipped} / 프레임부족 {incomplete} · Addressable 등록 {reg}");
    }

    // 폴더 하나 → 클립 5종. 프레임이 하나라도 없으면 null (생성이 아직 안 끝난 것).
    static Dictionary<string, AnimationClip> BuildClips(string srcDir)
    {
        var result = new Dictionary<string, AnimationClip>();

        foreach (var (state, prefix, len, loop) in CLIPS)
        {
            var sprites = new Sprite[FRAMES];
            for (int f = 0; f < FRAMES; f++)
            {
                sprites[f] = AssetDatabase.LoadAssetAtPath<Sprite>($"{srcDir}/{prefix}_{f}.png");
                if (sprites[f] == null) return null;
            }
            result[state] = SpriteClip(sprites, len, loop);
        }

        // OnHit — 전용 아틀라스는 없다. Idle 첫 프레임을 짧게 붉게 물들여 피격을 표시한다.
        //  상태가 아예 없으면 CrossFade("OnHit")가 무시되고 피격이 안 보인다.
        var idle0 = AssetDatabase.LoadAssetAtPath<Sprite>($"{srcDir}/Idle_0.png");
        result["OnHit"] = HitClip(idle0);
        return result;
    }

    static AnimationClip SpriteClip(Sprite[] sprites, float length, bool loop)
    {
        var clip = new AnimationClip { frameRate = 12f };
        var keys = new ObjectReferenceKeyframe[sprites.Length];
        float step = length / sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * step, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(
            clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return clip;
    }

    static AnimationClip HitClip(Sprite sprite)
    {
        var clip = new AnimationClip { frameRate = 12f };
        if (sprite != null)
            AnimationUtility.SetObjectReferenceCurve(
                clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
                new[] { new ObjectReferenceKeyframe { time = 0f, value = sprite } });

        // 붉은 점멸 (0 → 0.1 → 0.2초)
        foreach (var (prop, mid) in new[] { ("m_Color.g", 0.3f), ("m_Color.b", 0.3f) })
        {
            var curve = new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(0.1f, mid), new Keyframe(0.2f, 1f));
            AnimationUtility.SetEditorCurve(
                clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), prop), curve);
        }
        return clip;
    }

    // MonsterController가 Load<RuntimeAnimatorController>(AnimController)로 주소 조회한다.
    //  스프라이트가 아니므로 ".sprite" 접미사를 붙이지 않는다(붙이면 Sprite로 적재 시도해서 null).
    //  PreLoad 라벨도 달지 않는다 — 처음 스폰될 때 동기 로드된다(시작 로딩 보호).
    static int RegisterAddressables()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogWarning("[ArchAnim] Addressable 설정 없음"); return 0; }

        var group = settings.FindGroup(GROUP_NAME) ?? settings.DefaultGroup;
        int n = 0;
        if (!Directory.Exists(OUT_ROOT)) return 0;

        foreach (string path in Directory.GetFiles(OUT_ROOT, "*.controller", SearchOption.AllDirectories))
        {
            string assetPath = path.Replace('\\', '/');
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) continue;

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (entry == null) continue;
            entry.address = Path.GetFileNameWithoutExtension(assetPath);
            entry.SetLabel("PreLoad", false, false, false);
            n++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(settings);
        return n;
    }
}
