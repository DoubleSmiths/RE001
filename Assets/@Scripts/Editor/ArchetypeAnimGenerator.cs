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
        // Die는 0.5초. MonsterController가 사망 0.5초 뒤 풀에 반환하므로(HP Bar 역재생과 동기)
        //  더 길게 잡으면 마지막 프레임(쓰러진 자세)을 못 보고 사라진다.
        ("Die",    "Die",    0.50f, false),
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

                // 이미 정상이면 건너뛴다 — 재실행 가능하게.
                //  상태 개수뿐 아니라 **클립 길이까지** 본다. 길이 상수를 바꿨는데 개수만 보고 건너뛰면
                //  옛 길이의 클립이 그대로 남는다(Die를 0.7→0.5로 줄였을 때 실제로 그랬다).
                var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                if (existing != null && existing.layers.Length > 0 &&
                    existing.layers[0].stateMachine.states.Length >= CLIPS.Length + 1 &&
                    LengthsMatch(dir))
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

    // 이미 구워진 클립들이 현재 길이 상수와 일치하는지.
    //  AnimationClip.length는 **마지막 키 시각 + 1프레임**이라 우리가 준 총 길이와 다르다.
    //  (총 0.5s / 4프레임이면 키는 0·0.125·0.25·0.375 → length = 0.375 + 1/12 ≈ 0.46)
    //  그 계산을 그대로 재현해 비교한다. 이걸 안 맞추면 매번 전부 다시 굽는다.
    static float ExpectedLength(float total)
        => total * (FRAMES - 1) / FRAMES + 1f / 12f;   // frameRate 12

    static bool LengthsMatch(string dir)
    {
        foreach (var (state, _, len, _) in CLIPS)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{dir}/{state}.anim");
            if (clip == null) return false;
            if (Mathf.Abs(clip.length - ExpectedLength(len)) > 0.02f) return false;
        }

        // OnHit 아틀라스가 생겼는데 컨트롤러엔 아직 1프레임 폴백이 남아 있으면 다시 만들어야 한다.
        //  (CLIPS 루프엔 OnHit이 없어서 위 검사만으로는 옛 폴백이 그대로 살아남는다)
        string src = dir.Replace(OUT_ROOT, SRC_ROOT);
        bool hasHitAtlas = AssetDatabase.LoadAssetAtPath<Sprite>($"{src}/OnHit_{FRAMES - 1}.png") != null;
        var onHit = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{dir}/OnHit.anim");
        if (hasHitAtlas && (onHit == null || onHit.length < 0.3f)) return false;

        return true;
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

        // OnHit — 전용 아틀라스가 있으면 그걸 쓴다.
        //  예전엔 아틀라스가 없어 Idle 첫 프레임 한 장으로 때웠는데, 그러면 **달려오던 몬스터가
        //  맞는 순간 정지한 아이들 포즈로 스냅**해서 애니메이션이 고장난 것처럼 보였다.
        //  (아직 생성 전인 아키타입은 예전 폴백을 그대로 써서 깨지지 않게 둔다)
        var hitFrames = new Sprite[FRAMES];
        bool hasHitAtlas = true;
        for (int f = 0; f < FRAMES; f++)
        {
            hitFrames[f] = AssetDatabase.LoadAssetAtPath<Sprite>($"{srcDir}/OnHit_{f}.png");
            if (hitFrames[f] == null) { hasHitAtlas = false; break; }
        }

        if (hasHitAtlas)
            result["OnHit"] = SpriteClip(hitFrames, 0.35f, false);
        else
            result["OnHit"] = HitClip(AssetDatabase.LoadAssetAtPath<Sprite>($"{srcDir}/Idle_0.png"));

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
