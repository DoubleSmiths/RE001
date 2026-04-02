using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CH1BG 오브젝트에 부착. 자식 레이어들을 패럴랙스 효과와 함께 무한 루프 스크롤.
/// 자식 순서: 뒤(layer01) → 앞(layer05), 앞일수록 빠르게 이동.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] float _baseScrollSpeed = 2f;

    // 자식 레이어 순서에 맞는 패럴랙스 배율 (layer01~05 순서)
    // 뒤 레이어일수록 느리게, 앞 레이어일수록 빠르게
    [SerializeField] float[] _parallaxFactors = { 0.15f, 0.3f, 0.5f, 0.75f, 1.0f };

    class LayerData
    {
        public Transform A;     // 원본
        public Transform B;     // 복사본 (A 오른쪽에 배치)
        public float Width;     // 스프라이트 가로 크기 (world unit)
        public float Speed;     // 실제 스크롤 속도
    }

    readonly List<LayerData> _layers = new List<LayerData>();
    float _camLeftEdge;

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
            float factor = idx < _parallaxFactors.Length ? _parallaxFactors[idx] : 1f;

            // 복사본 생성: A(원본) 바로 오른쪽에 B 배치
            GameObject cloneGo = Instantiate(child.gameObject, transform);
            cloneGo.name = child.name + "_Clone";
            cloneGo.transform.position = child.position + Vector3.right * width;

            _layers.Add(new LayerData
            {
                A = child,
                B = cloneGo.transform,
                Width = width,
                Speed = _baseScrollSpeed * factor
            });

            idx++;
        }
    }

    void Update()
    {
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
    [ContextMenu("Reset Scroll Speed")]
    void ResetSpeed()
    {
        _baseScrollSpeed = 2f;
        _parallaxFactors = new float[] { 0.15f, 0.3f, 0.5f, 0.75f, 1.0f };
    }
#endif
}
