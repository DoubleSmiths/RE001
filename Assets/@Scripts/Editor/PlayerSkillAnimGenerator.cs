using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 액티브 스킬 전용 플레이어 모션을 만들어 Player_01 컨트롤러에 상태로 붙인다.
///
/// 왜 새로 그리지 않는가 — Player_02 폴더에 **같은 캐릭터의 다른 모션 시트**가 이미 있다.
///   attack_Sheet2 : 크게 휘두르는 베기(4번째 프레임에 궤적) → 공격 스킬
///   onhit_Sheet2  : 도약/방어 자세                          → 버프·회복 시전
/// AI로 새로 뽑으면 기존 손저작 캐릭터와 화풍이 어긋난다. 있는 프레임을 쓰는 편이 낫고 빠르다.
///
/// 상태 이름은 코드가 CrossFade로 부르는 값과 1:1이어야 한다 — 하나라도 다르면 조용히 무시된다.
/// </summary>
public static class PlayerSkillAnimGenerator
{
    const string CTRL = "Assets/@Resources/Animation/Player/Player_01/Player_01.controller";
    const string OUT_DIR = "Assets/@Resources/Animation/Player/Player_01/Skill";

    const string SHEET_SLASH = "Assets/@Resources/Sprite/Player/Player_02/attack_Sheet2.png";
    const string SHEET_CAST = "Assets/@Resources/Sprite/Player/Player_02/onhit_Sheet2.png";

    // (상태 이름, 시트, 프레임 순서, 총 길이, 반복)
    //  프레임 순서를 직접 적는 이유: 시트에 담긴 순서가 곧 좋은 연출 순서는 아니다.
    //  시전은 "자세 잡고(0) → 버티고(1,2) → 풀기(1)"로 되짚어야 동작이 읽힌다.
    static readonly (string state, string sheet, int[] order, float len)[] MOTIONS =
    {
        ("Skill_Slash", SHEET_SLASH, new[] { 0, 1, 2, 3, 4, 5, 6 }, 0.60f),
        ("Skill_Cast",  SHEET_CAST,  new[] { 1, 0, 0, 2, 1 },       0.55f),
    };

    [MenuItem("Tools/액티브 스킬 모션 생성 (플레이어)")]
    public static void Generate()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(CTRL);
        if (ctrl == null) { Debug.LogError($"[SkillAnim] 컨트롤러 없음: {CTRL}"); return; }

        if (!System.IO.Directory.Exists(OUT_DIR))
        {
            System.IO.Directory.CreateDirectory(OUT_DIR);
            AssetDatabase.Refresh();
        }

        var sm = ctrl.layers[0].stateMachine;
        int made = 0, reused = 0;

        foreach (var (state, sheet, order, len) in MOTIONS)
        {
            var frames = LoadFrames(sheet);
            if (frames.Count == 0) { Debug.LogError($"[SkillAnim] 시트 프레임 없음: {sheet}"); continue; }

            var sprites = new Sprite[order.Length];
            for (int i = 0; i < order.Length; i++)
                sprites[i] = frames[Mathf.Clamp(order[i], 0, frames.Count - 1)];

            string clipPath = $"{OUT_DIR}/{state}.anim";
            var clip = SpriteClip(sprites, len);
            clip.name = state;

            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (existing != null) EditorUtility.CopySerialized(clip, existing);
            else AssetDatabase.CreateAsset(clip, clipPath);
            AssetDatabase.SaveAssets();

            var saved = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

            // 같은 이름 상태가 이미 있으면 모션만 갈아끼운다(중복 상태를 만들면 CrossFade가 헷갈린다).
            AnimatorState st = null;
            foreach (var s in sm.states)
                if (s.state.name == state) { st = s.state; break; }

            if (st == null) { st = sm.AddState(state); made++; }
            else reused++;
            st.motion = saved;
        }

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SkillAnim] 스킬 모션 상태 신규 {made} / 갱신 {reused} — {CTRL}");
    }

    static List<Sprite> LoadFrames(string sheetPath)
    {
        var list = new List<Sprite>();
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
            if (o is Sprite s) list.Add(s);
        // 서브스프라이트는 이름 순서가 곧 프레임 순서다(..._0, _1, ...). 로드 순서는 보장되지 않는다.
        list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return list;
    }

    static AnimationClip SpriteClip(Sprite[] sprites, float length)
    {
        var clip = new AnimationClip { frameRate = 12f };
        var keys = new ObjectReferenceKeyframe[sprites.Length];
        float step = length / sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * step, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(
            clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;   // 스킬 모션은 한 번만 — 끝나면 코드가 원래 상태로 되돌린다
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return clip;
    }
}
