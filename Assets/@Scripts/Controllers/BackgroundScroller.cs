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
    // 노드마다 생성해 둔 Region_{nodeId} 스프라이트가 있으면 그걸로 갈아끼운다.
    // 생성 이미지는 그 자체가 원경~근경을 품은 한 장이라 레이어를 나누지 않고 한 장으로 스크롤한다
    // (없으면 저작된 5겹 패럴랙스를 그대로 둔다 — 폴백).
    GameObject _regionLayer;
    int _appliedNodeId = int.MinValue;

    void ApplyRegionBackground()
    {
        int nodeId = Managers.Game != null ? Managers.Game.CurrentTravelAreaId : 0;
        if (nodeId == _appliedNodeId) return;
        _appliedNodeId = nodeId;

        // 던전 노드 전용 그림 → 없으면 소속 지역 그림. 전투는 던전에서 일어나므로 이 폴백이 실사용 경로다.
        Sprite sp = Managers.Resource?.Load<Sprite>($"Region_{nodeId}");
        if (sp == null && Managers.Data?.TravelNodeDic != null
            && Managers.Data.TravelNodeDic.TryGetValue(nodeId, out var tn) && tn.ParentId != 0)
            sp = Managers.Resource?.Load<Sprite>($"Region_{tn.ParentId}");

        if (sp == null)
        {
            // 전용 그림이 없는 지역 — 저작 패럴랙스로 되돌린다.
            if (_regionLayer != null) _regionLayer.SetActive(false);
            foreach (Transform c in transform)
                if (c.gameObject != _regionLayer) c.gameObject.SetActive(true);
            return;
        }

        foreach (Transform c in transform)
            if (c.gameObject != _regionLayer) c.gameObject.SetActive(false);

        if (_regionLayer == null)
        {
            _regionLayer = new GameObject("RegionBG");
            _regionLayer.transform.SetParent(transform, false);
            var sr = _regionLayer.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -1;
        }
        _regionLayer.SetActive(true);

        var rend = _regionLayer.GetComponent<SpriteRenderer>();
        rend.sprite = sp;
        FitToAuthored(_regionLayer.transform, rend);

        RebuildLayers();
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
            if (c == t || c.name.StartsWith(_regionLayer.name)) continue;
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
            float speed = SpeedForIndex(idx);

            // 복사본 생성: A(원본) 바로 오른쪽에 B 배치
            GameObject cloneGo = Instantiate(child.gameObject, transform);
            cloneGo.name = child.name + "_Clone";
            cloneGo.transform.position = child.position + Vector3.right * width;

            // 지역 배경은 **좌우가 이어지도록 그려진 그림이 아니다**(저작 패럴랙스는 이음매가 맞게
            //  만들어져 있지만 생성 배경은 아니다). 복사본을 좌우 반전하면 맞닿는 변이 같은 픽셀이
            //  되어 이음매가 사라진다 — 풍경이라 거울상이어도 티가 나지 않는다.
            if (_regionLayer != null && child.gameObject == _regionLayer)
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
