using UnityEngine;

// v2 스테이지 보스의 금빛 테두리(ADR0030). 본체 스프라이트를 여덟 방향으로 2텍셀씩 밀어 금색 실루엣으로 뒤에 그린다.
//  (1텍셀·네 방향은 폰 화면에서 2px 남짓이라 보스인지 알아보기 어려웠다)
//  애니메이터가 매 프레임 스프라이트를 바꾸므로 LateUpdate 에서 따라간다. 본체 알파(퇴각 페이드)도 따른다.
//  재질 BossOutline(RE001/SpriteSilhouette)은 어드레서블 Material 그룹(PreLoad) — Tools/Register V2 Data Addressables.
public class BossOutline : MonoBehaviour
{
    static readonly Color GOLD = new Color32(0xFF, 0xD2, 0x4A, 0xFF);
    static readonly Vector2[] DIRS =
    {
        Vector2.left, Vector2.right, Vector2.up, Vector2.down,
        new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(-1f, -1f), new Vector2(1f, -1f),
    };
    const float THICKNESS_TEXELS = 2f;
    const float PULSE_SPEED     = 3f;      // 라디안/초 — 2초에 한 번쯤 숨 쉬듯 밝아진다
    const float PULSE_MIN_ALPHA = 0.55f;

    SpriteRenderer _body;
    SpriteRenderer[] _edges;

    void Awake()
    {
        _body = GetComponent<SpriteRenderer>();
        var mat = Managers.Resource.Load<Material>("BossOutline");
        if (_body == null || mat == null)
        {
            Debug.LogWarning("[BossOutline] 본체 SpriteRenderer 또는 재질 BossOutline 없음 — 테두리 없이 진행");
            enabled = false;
            return;
        }

        _edges = new SpriteRenderer[DIRS.Length];
        for (int i = 0; i < DIRS.Length; i++)
        {
            var go = new GameObject($"BossOutline_{i}");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = mat;
            sr.sortingLayerID = _body.sortingLayerID;
            sr.sortingOrder   = _body.sortingOrder - 1;
            _edges[i] = sr;
        }
    }

    void LateUpdate()
    {
        Sprite s = _body.sprite;
        float texel = s != null ? THICKNESS_TEXELS / s.pixelsPerUnit : 0f;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * PULSE_SPEED);
        var color = new Color(GOLD.r, GOLD.g, GOLD.b, Mathf.Lerp(PULSE_MIN_ALPHA, 1f, pulse) * _body.color.a);

        for (int i = 0; i < _edges.Length; i++)
        {
            var e = _edges[i];
            e.sprite = s;
            e.flipX  = _body.flipX;
            e.color  = color;
            e.transform.localPosition = DIRS[i] * texel;
        }
    }
}
