using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// 몬스터 500여 종에 **각각** 전용 애니메이터 + 클립 5종(Idle/Run/Attack/OnHit/Die)을 생성한다.
//
// 왜 절차 생성인가
//   종별로 손으로 그린 스프라이트 시트를 500장 확보하는 것은 현실적이지 않다. 대신
//   지역별 원형 스프라이트(_tools/aether_icons.py monster)를 쓰고, 종마다 다른 모션 커브와
//   색조를 입힌 **자기만의 클립 에셋**을 만든다. 즉 "모든 몬스터가 자기 애니메이션을 가진다"를
//   실제 에셋으로 충족한다.
//
// 클립 구성 (Monster.prefab은 루트에 SpriteRenderer+Animator 단일 구조 → 바인딩 경로는 "")
//   Idle   : 위아래 부드러운 호흡 + 미세 스쿼시 (loop)
//   Run    : 빠른 상하 바운스 + 진행방향 기울기 (loop)
//   Attack : 앞으로 돌진했다 복귀 (1회)
//   OnHit  : 붉은 플래시 + 뒤로 밀림 (1회)
//   Die    : 납작해지며 알파 소멸 (1회)
//
// 상태명은 CreatureController가 CrossFade로 호출하는 이름과 정확히 같아야 한다.
public static class MonsterAnimGenerator
{
    const string OUT_ROOT = "Assets/@Resources/Animation/Monster/Generated";
    const string SPRITE_DIR = "Assets/@Resources/Sprite/Monster/Generated";

    [MenuItem("Tools/Generate Monster Animations (몬스터 개별 애니 생성)")]
    public static void Generate()
    {
        var creatures = LoadCreatureRows();
        if (creatures.Count == 0)
        {
            Debug.LogError("[MonAnim] CreatureData.json을 읽지 못했습니다");
            return;
        }

        Directory.CreateDirectory(OUT_ROOT);
        int made = 0, skipped = 0;

        // ⚠️ AssetDatabase.StartAssetEditing()으로 감싸면 CreateAnimatorControllerAtPath가
        //    레이어를 온전히 만들지 못해 "Statemachine for layer 'Base Layer' is missing" 컨트롤러가 나온다.
        //    배치 편집으로 묶지 않는다(느려도 정확).
        try
        {
            for (int i = 0; i < creatures.Count; i++)
            {
                var c = creatures[i];
                if (EditorUtility.DisplayCancelableProgressBar(
                        "몬스터 애니 생성", $"{c.name} ({i + 1}/{creatures.Count})",
                        (float)i / creatures.Count))
                    break;

                Sprite sprite = FindSprite(c.archetype);
                if (sprite == null) { skipped++; continue; }

                string dir = $"{OUT_ROOT}/{c.name}";
                string ctrlPath = $"{dir}/{c.name}.controller";
                var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                if (existing != null)
                {
                    // 상태머신이 온전한 것만 건너뛴다. 비어 있으면 삭제 후 다시 만든다(복구).
                    bool healthy = existing.layers.Length > 0
                                   && existing.layers[0].stateMachine != null
                                   && existing.layers[0].stateMachine.states.Length >= 5;
                    if (healthy) { skipped++; continue; }
                    AssetDatabase.DeleteAsset(ctrlPath);
                }
                Directory.CreateDirectory(dir);

                var tint = c.tint;
                var clips = new Dictionary<string, AnimationClip>
                {
                    ["Idle"]   = BuildIdle(sprite, tint, c.speed),
                    ["Run"]    = BuildRun(sprite, tint, c.speed),
                    ["Attack"] = BuildAttack(sprite, tint),
                    ["OnHit"]  = BuildOnHit(sprite, tint),
                    ["Die"]    = BuildDie(sprite, tint),
                };

                foreach (var kv in clips)
                {
                    kv.Value.name = $"{c.name}_{kv.Key}";
                    AssetDatabase.CreateAsset(kv.Value, $"{dir}/{c.name}_{kv.Key}.anim");
                }

                var ctrl = AnimatorController.CreateAnimatorControllerAtPath($"{dir}/{c.name}.controller");
                ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                ctrl.AddParameter("OnDamaged", AnimatorControllerParameterType.Trigger);
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

        Debug.Log($"[MonAnim] 생성 {made}종, 건너뜀 {skipped}종 → {OUT_ROOT}");
    }

    // ── 클립 빌더 ────────────────────────────────────────────────────────────
    static AnimationClip NewClip(Sprite sprite, Color tint, bool loop, float length)
    {
        var clip = new AnimationClip { frameRate = 12 };

        // 스프라이트 1프레임 고정 (종별 외형)
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding,
            new[] { new ObjectReferenceKeyframe { time = 0f, value = sprite } });

        SetColor(clip, tint, length);

        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        return clip;
    }

    static void SetColor(AnimationClip clip, Color c, float length)
    {
        Curve(clip, "m_Color.r", Flat(c.r, length));
        Curve(clip, "m_Color.g", Flat(c.g, length));
        Curve(clip, "m_Color.b", Flat(c.b, length));
        Curve(clip, "m_Color.a", Flat(1f, length));
    }

    static AnimationCurve Flat(float v, float len) => AnimationCurve.Linear(0f, v, len, v);

    static void Curve(AnimationClip clip, string prop, AnimationCurve curve)
        => AnimationUtility.SetEditorCurve(clip,
               EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), prop), curve);

    static void TransformCurve(AnimationClip clip, string prop, AnimationCurve curve)
        => AnimationUtility.SetEditorCurve(clip,
               EditorCurveBinding.FloatCurve("", typeof(Transform), prop), curve);

    // ⚠️ 위치(m_LocalPosition) 커브는 쓰지 않는다.
    //  애니메이션이 localPosition을 **절대값으로 덮어써** 스폰 좌표가 무시되고
    //  몬스터가 부모 원점(0,0)으로 끌려간다 → 화면 밖이라 "몬스터가 안 나온다"로 보였다.
    //  움직임은 스케일(스쿼시)·회전·색상으로만 표현한다.
    static AnimationClip BuildIdle(Sprite sp, Color tint, float speed)
    {
        float len = Mathf.Clamp(1.4f / speed, 0.7f, 2.2f);
        var clip = NewClip(sp, tint, true, len);
        // 호흡: 위로 살짝 떴다 내려오며 가로/세로 스쿼시
        TransformCurve(clip, "m_LocalScale.y", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(len * 0.5f, 1.05f), new Keyframe(len, 1f)));
        TransformCurve(clip, "m_LocalScale.x", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(len * 0.5f, 0.96f), new Keyframe(len, 1f)));
        return clip;
    }

    static AnimationClip BuildRun(Sprite sp, Color tint, float speed)
    {
        float len = Mathf.Clamp(0.55f / speed, 0.3f, 0.9f);
        var clip = NewClip(sp, tint, true, len);
        TransformCurve(clip, "m_LocalEulerAngles.z", new AnimationCurve(
            new Keyframe(0f, -4f), new Keyframe(len * 0.5f, 4f), new Keyframe(len, -4f)));
        return clip;
    }

    static AnimationClip BuildAttack(Sprite sp, Color tint)
    {
        const float len = 0.42f;
        var clip = NewClip(sp, tint, false, len);
        // 뒤로 살짝 당겼다가 앞으로 튀어나감
        TransformCurve(clip, "m_LocalScale.x", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.12f, 0.9f),
            new Keyframe(0.24f, 1.12f), new Keyframe(len, 1f)));
        return clip;
    }

    static AnimationClip BuildOnHit(Sprite sp, Color tint)
    {
        const float len = 0.26f;
        var clip = new AnimationClip { frameRate = 12 };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding,
            new[] { new ObjectReferenceKeyframe { time = 0f, value = sp } });

        // 붉은 플래시 → 원래 색조로 복귀
        Curve(clip, "m_Color.r", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.08f, 1f), new Keyframe(len, tint.r)));
        Curve(clip, "m_Color.g", new AnimationCurve(
            new Keyframe(0f, 0.25f), new Keyframe(0.08f, 0.35f), new Keyframe(len, tint.g)));
        Curve(clip, "m_Color.b", new AnimationCurve(
            new Keyframe(0f, 0.25f), new Keyframe(0.08f, 0.35f), new Keyframe(len, tint.b)));
        Curve(clip, "m_Color.a", Flat(1f, len));
        // 뒤로 밀림

        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        return clip;
    }

    static AnimationClip BuildDie(Sprite sp, Color tint)
    {
        const float len = 0.6f;
        var clip = new AnimationClip { frameRate = 12 };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding,
            new[] { new ObjectReferenceKeyframe { time = 0f, value = sp } });

        Curve(clip, "m_Color.r", Flat(tint.r, len));
        Curve(clip, "m_Color.g", Flat(tint.g, len));
        Curve(clip, "m_Color.b", Flat(tint.b, len));
        Curve(clip, "m_Color.a", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(len * 0.6f, 0.7f), new Keyframe(len, 0f)));
        // 납작해지며 주저앉음
        TransformCurve(clip, "m_LocalScale.y", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(len, 0.35f)));
        TransformCurve(clip, "m_LocalScale.x", new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(len, 1.15f)));

        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        return clip;
    }

    // ── 데이터 ───────────────────────────────────────────────────────────────
    class Row
    {
        public string name;        // 애니메이터 이름 = AnimController 주소
        public string archetype;   // 사용할 원형 스프라이트 이름
        public Color tint;
        public float speed;
    }

    static Sprite FindSprite(string archetype)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_DIR}/{archetype}.png");
        return sp;
    }

    static List<Row> LoadCreatureRows()
    {
        var rows = new List<Row>();
        string path = Path.Combine(Application.dataPath, "@Resources/Data/JsonData/CreatureData.json");
        if (!File.Exists(path)) return rows;

        var loader = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.CreatureDataLoader>(
            File.ReadAllText(path));
        if (loader?.creatures == null) return rows;

        foreach (var c in loader.creatures)
        {
            // 서식지 키(BreezingWindForestDepth3Text)에서 지역명과 심도를 뽑아
            // 원형 스프라이트와 티어 색조를 정한다.
            string hab = c.Habitat ?? "";
            int di = hab.IndexOf("Depth");
            if (di < 0) continue;
            string region = hab.Substring(0, di);
            int depth = 1;
            int end = hab.IndexOf("Text", di);
            if (end > di) int.TryParse(hab.Substring(di + 5, end - di - 5), out depth);

            int archIdx = Mathf.Abs(c.DataId) % 10;
            rows.Add(new Row
            {
                name = $"Mon_{c.DataId}",
                archetype = $"Mon_{region}_{archIdx}",
                tint = TierTint(depth, c.Rarity),
                speed = 1f + (depth - 1) * 0.05f,
            });
        }
        return rows;
    }

    // 심도가 깊을수록 어둡고 짙게, 희귀/보스는 더 강한 색을 준다 (한눈에 위험도가 읽히도록)
    static Color TierTint(int depth, string rarity)
    {
        float t = Mathf.Clamp01((depth - 1) / 9f);
        Color baseCol = Color.Lerp(new Color(1f, 1f, 1f), new Color(0.62f, 0.55f, 0.78f), t);
        if (rarity == "Rare") baseCol = Color.Lerp(baseCol, new Color(0.55f, 0.85f, 1f), 0.55f);
        else if (rarity == "Boss") baseCol = Color.Lerp(baseCol, new Color(1f, 0.55f, 0.45f), 0.6f);
        return baseCol;
    }
}
