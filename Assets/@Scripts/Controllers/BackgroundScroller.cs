using System.Collections.Generic;
using UnityEngine;

// 슬라이드 56/58 — Battle Scene Layer Scrolling.
// CH1BG 같은 BG prefab에 부착. 자식 SpriteRenderer 레이어를 슬라이드 58 절대 속도(units/sec)로 무한 스크롤.
//
// 슬라이드 58 — Layer 별 Speed (translate(DeltaTime * Speed, 0, 0)):
//   Layer1: 1, Layer2: 2, Layer3: 3, Layer4: 4, Layer5: 4, Ground: 5
//
// 자식 순서 0 → N: 뒤(layer01) → 앞(layer05) → Ground.
// _layerSpeeds 배열 길이를 초과하는 자식은 마지막 entry 값으로 fallback.
// IsScrolling = false 가 기본 (슬라이드 55 — [지역명 UI] In 종료 시점에 외부에서 true로 set).
public class BackgroundScroller : MonoBehaviour
{
    // 슬라이드 58 기본값 — BG1..5 + Ground
    [SerializeField] float[] _layerSpeeds = { 1f, 2f, 3f, 4f, 4f, 5f };

    class LayerData
    {
        public Transform A;     // 원본
        public Transform B;     // 복사본 (A 오른쪽에 배치)
        public float Width;     // 스프라이트 가로 크기 (world unit)
        public float Speed;     // 실제 스크롤 속도
    }

    readonly List<LayerData> _layers = new List<LayerData>();
    float _camLeftEdge;
    public bool IsScrolling { get; set; } = false;

    // ── 지역별 배경 ──
    // 전 던전이 CH1BG(여름 숲) 하나를 그대로 썼다. 125개 던전이 전부 같은 그림이었다는 뜻이다.
    // 노드마다 만들어 둔 그림으로 갈아끼운다. 우선순위는 셋이다.
    //   1) 겹 배경 RegionL_{Far|Mid|Near}_{id}  ← 원경/중경/근경을 다른 속도로 흘려 입체감을 낸다
    //   2) 한 장짜리 Region_{id}                ← 겹이 없는 지역 폴백
    //   3) 저작 5겹 패럴랙스(CH1BG)             ← 생성 그림이 아예 없는 경우
    //
    // ⚠️ 한동안 2)만 썼다. 지역마다 그림은 달라졌지만 **한 장을 통째로 같은 속도로** 미니
    //    원래 있던 깊이감이 사라졌다("배경이 평평해졌다"는 지적이 이것이다).
    static readonly string[] REGION_TAGS = { "Far", "Mid", "Near" };
    static readonly float[] REGION_SPEEDS = { 1f, 3f, 5f };   // 슬58의 원경 1 ~ Ground 5 범위 안에 맞춤

    readonly List<GameObject> _regionLayers = new List<GameObject>();
    int _appliedNodeId = int.MinValue;

    Sprite LoadForNode(string prefix, int nodeId)
    {
        Sprite sp = Managers.Resource?.Load<Sprite>($"{prefix}{nodeId}");
        // 던전은 소속 지역 그림으로 물러난다 — 전투는 던전에서 일어나므로 이 폴백이 실사용 경로다.
        if (sp == null && Managers.Data?.TravelNodeDic != null
            && Managers.Data.TravelNodeDic.TryGetValue(nodeId, out var tn) && tn.ParentId != 0)
            sp = Managers.Resource?.Load<Sprite>($"{prefix}{tn.ParentId}");
        return sp;
    }

    void ApplyRegionBackground()
    {
        int nodeId = Managers.Game != null ? Managers.Game.CurrentTravelAreaId : 0;
        if (nodeId == _appliedNodeId) return;
        _appliedNodeId = nodeId;

        var sprites = new List<Sprite>();
        foreach (string tag in REGION_TAGS)
            sprites.Add(LoadForNode($"RegionL_{tag}_", nodeId));

        bool layered = sprites.Exists(s => s != null);
        if (!layered)
        {
            Sprite single = LoadForNode("Region_", nodeId);
            sprites.Clear();
            if (single != null) sprites.Add(single);
        }

        if (sprites.Count == 0)
        {
            // 생성 그림이 없는 지역 — 저작 패럴랙스로 되돌린다.
            foreach (var l in _regionLayers) l.SetActive(false);
            foreach (Transform c in transform)
                if (!IsRegionLayer(c.gameObject)) c.gameObject.SetActive(true);
            return;
        }

        foreach (Transform c in transform)
            if (!IsRegionLayer(c.gameObject)) c.gameObject.SetActive(false);

        // 겹마다 오브젝트 하나. 원경이 정한 배율·바닥선을 나머지가 그대로 따른다
        //  (겹마다 따로 맞추면 중경/근경은 위가 비어 있어 엉뚱하게 확대된다).
        float scale = 0f, bottom = 0f;
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i] == null) continue;
            GameObject go = EnsureRegionLayer(i);
            go.SetActive(true);

            var rend = go.GetComponent<SpriteRenderer>();
            rend.sprite = sprites[i];
            rend.sortingOrder = -REGION_TAGS.Length + i;

            if (scale <= 0f)
            {
                FitToAuthored(go.transform, rend);
                scale = go.transform.localScale.x;
                bottom = go.transform.position.y - rend.sprite.bounds.size.y * scale * 0.5f;
            }
            else
            {
                go.transform.localScale = new Vector3(scale, scale, 1f);
                float h = rend.sprite.bounds.size.y * scale;
                go.transform.position = new Vector3(go.transform.position.x, bottom + h * 0.5f,
                                                    go.transform.position.z);
            }
        }
        for (int i = sprites.Count; i < _regionLayers.Count; i++)
            _regionLayers[i].SetActive(false);

        RebuildLayers();
    }

    bool IsRegionLayer(GameObject go) => _regionLayers.Contains(go);

    GameObject EnsureRegionLayer(int i)
    {
        while (_regionLayers.Count <= i)
        {
            var go = new GameObject($"RegionBG_{_regionLayers.Count}");
            go.transform.SetParent(transform, false);
            go.AddComponent<SpriteRenderer>();
            _regionLayers.Add(go);
        }
        return _regionLayers[i];
    }

    // 저작된 배경 레이어가 차지하던 **그 자리**에 맞춘다.
    //  카메라 기준으로 맞췄더니 화면 밖까지 번졌다 — 카메라 뷰는 세로 21.8유닛인데
    //  실제 플레이 영역(저작 배경)은 위쪽 6.75유닛뿐이고 아래는 UI 패널이 덮는다.
    //  그래서 "카메라를 채운다"가 아니라 "원본과 같은 높이·같은 위치"가 정답이다.
    void FitToAuthored(Transform t, SpriteRenderer sr)
    {
        if (sr.sprite == null) return;

        // 자기 자신과 자기 복사본은 건너뛴다 — 지역을 옮기면 이전 복사본이 남아 있어 그걸 기준으로
        // 삼으면 크기가 대물림되며 계속 줄어든다.
        SpriteRenderer model = null;
        foreach (Transform c in transform)
        {
            if (c == t || c.name.StartsWith("RegionBG")) continue;
            var s = c.GetComponent<SpriteRenderer>();
            if (s != null && s.sprite != null) { model = s; break; }
        }

        t.localScale = Vector3.one;
        // bounds는 렌더러가 꺼져 있으면 못 믿는다(이 시점엔 원본 레이어가 이미 비활성) →
        // 스프라이트 크기 × 스케일로 직접 계산한다.
        float h = sr.sprite.bounds.size.y;
        if (h <= 0f) return;

        // 저작 레이어가 없으면(프리팹이 바뀐 경우) 카메라 세로의 옛 비율로 폴백.
        float targetH = model != null
            ? model.sprite.bounds.size.y * Mathf.Abs(model.transform.lossyScale.y)
            : (Camera.main != null ? Camera.main.orthographicSize * 0.62f : h);
        float scale = targetH / h;

        // 폭도 원본이 차지하던 만큼 확보한다. 높이만 맞추면 생성 배경은 비율이 달라 좁아지고,
        //  그러면 두 복사본의 이음매가 화면 한복판에 들어온다.
        float w = sr.sprite.bounds.size.x;
        if (w > 0f)
        {
            float targetW = model != null
                ? model.sprite.bounds.size.x * Mathf.Abs(model.transform.lossyScale.x)
                : (Camera.main != null ? Camera.main.orthographicSize * Camera.main.aspect * 2f * 1.1f : w);
            scale = Mathf.Max(scale, targetW / w);
        }

        t.localScale = new Vector3(scale, scale, 1f);

        // 커진 만큼은 **아래로** 흘린다 — 위쪽이 플레이 영역이고 아래는 UI 패널이 덮는다.
        //  중심을 맞추면 늘어난 높이의 절반이 위로 삐져나와 상단 HUD를 침범한다.
        Vector3 at = model != null ? model.transform.position : t.position;
        float top = at.y + targetH * 0.5f;
        float newH = h * scale;
        t.position = new Vector3(at.x, top - newH * 0.5f, t.position.z);
    }

    void Start()
    {
        Camera cam = Camera.main;
        // 카메라 왼쪽 경계 (world 좌표), 카메라가 고정이므로 한 번만 계산
        _camLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        ApplyRegionBackground();   // 내부에서 RebuildLayers까지 한다
        if (_layers.Count == 0) RebuildLayers();
    }

    void RebuildLayers()
    {
        // 이전 클론 정리 — 배경을 갈아끼울 때마다 복사본이 쌓이면 안 된다.
        foreach (var l in _layers)
            if (l.B != null) Destroy(l.B.gameObject);
        _layers.Clear();

        // 자식 목록을 먼저 수집 (Instantiate로 추가되는 클론이 루프에 잡히지 않게)
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
            children.Add(child);

        int idx = 0;
        foreach (Transform child in children)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr == null || !child.gameObject.activeSelf)
            {
                idx++;
                continue;
            }

            float width = sr.bounds.size.x;
            // 지역 겹 배경은 자식 순서가 아니라 **겹 번호**로 속도를 정한다
            //  (겹은 나중에 붙는 자식이라 idx로 잡으면 셋 다 마지막 속도를 받는다).
            int regionIdx = _regionLayers.IndexOf(child.gameObject);
            float speed = regionIdx >= 0
                ? REGION_SPEEDS[Mathf.Min(regionIdx, REGION_SPEEDS.Length - 1)]
                : SpeedForIndex(idx);

            // 복사본 생성: A(원본) 바로 오른쪽에 B 배치
            GameObject cloneGo = Instantiate(child.gameObject, transform);
            cloneGo.name = child.name + "_Clone";
            cloneGo.transform.position = child.position + Vector3.right * width;

            // 지역 배경은 **좌우가 이어지도록 그려진 그림이 아니다**(저작 패럴랙스는 이음매가 맞게
            //  만들어져 있지만 생성 배경은 아니다). 복사본을 좌우 반전하면 맞닿는 변이 같은 픽셀이
            //  되어 이음매가 사라진다 — 풍경이라 거울상이어도 티가 나지 않는다.
            if (regionIdx >= 0)
            {
                Vector3 s = cloneGo.transform.localScale;
                cloneGo.transform.localScale = new Vector3(-s.x, s.y, s.z);
            }

            _layers.Add(new LayerData
            {
                A = child,
                B = cloneGo.transform,
                Width = width,
                Speed = speed
            });

            idx++;
        }
    }

    float SpeedForIndex(int idx)
    {
        if (_layerSpeeds == null || _layerSpeeds.Length == 0) return 0f;
        if (idx < _layerSpeeds.Length) return _layerSpeeds[idx];
        return _layerSpeeds[_layerSpeeds.Length - 1];
    }

    void Update()
    {
        // 지역 이동 감지 — int 비교라 매 프레임 돌려도 부담 없다. 바뀔 때만 실제 교체가 일어난다.
        ApplyRegionBackground();

        if (!IsScrolling) return;

        float dt = Time.deltaTime;

        foreach (LayerData layer in _layers)
        {
            // 두 복사본 모두 왼쪽으로 이동
            layer.A.position += Vector3.left * layer.Speed * dt;
            layer.B.position += Vector3.left * layer.Speed * dt;

            // 스프라이트 오른쪽 끝이 카메라 왼쪽 경계를 벗어나면 반대편으로 이동
            float halfW = layer.Width * 0.5f;

            if (layer.A.position.x + halfW < _camLeftEdge)
                layer.A.position = new Vector3(layer.B.position.x + layer.Width,
                    layer.A.position.y, layer.A.position.z);

            if (layer.B.position.x + halfW < _camLeftEdge)
                layer.B.position = new Vector3(layer.A.position.x + layer.Width,
                    layer.B.position.y, layer.B.position.z);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Reset to Slide 58 Defaults")]
    void ResetSpeed()
    {
        _layerSpeeds = new[] { 1f, 2f, 3f, 4f, 4f, 5f };
    }
#endif
}
