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

    void Start()
    {
        Camera cam = Camera.main;
        // 카메라 왼쪽 경계 (world 좌표), 카메라가 고정이므로 한 번만 계산
        _camLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        // 자식 목록을 먼저 수집 (Instantiate로 추가되는 클론이 루프에 잡히지 않게)
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
            children.Add(child);

        int idx = 0;
        foreach (Transform child in children)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr == null)
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
