using UnityEngine;
using UnityEngine.SceneManagement;

// 세로 화면 비율(1440x3040) 고정 — 화면비가 다르면 위아래나 좌우에 검은 여백을 둔다.
//  UI 캔버스는 전부 Screen Space - Camera(메인 카메라)라 카메라 영역 안에 같이 갇힌다.
//
//  · 씬마다 메인 카메라에 자동으로 붙는다(예전엔 GameScene 에만 있어 타이틀이 16:9 로 늘어났다).
//  · 창 크기 변경·전체화면 전환·회전 때 다시 계산한다(예전엔 Start 에서 한 번뿐).
//  · URP 는 OnPreCull 을 부르지 않아 예전 GL.Clear 여백 지우기가 돌지 않았다 — 여백엔 이전 프레임
//    찌꺼기가 남았다. 아무것도 그리지 않고 화면 전체를 검게 지우는 카메라를 뒤에 하나 둔다.
public class FixedResolution : MonoBehaviour
{
    public const float TARGET_ASPECT = 1440f / 3040f;

    Camera _camera;
    int _width, _height;
    Rect _rect;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachToMainCamera();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToMainCamera();

    static void AttachToMainCamera()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<FixedResolution>() == null)
            cam.gameObject.AddComponent<FixedResolution>();
    }

    void Awake()
    {
        _camera = GetComponent<Camera>();
        CreateBarCamera();
        Apply();
    }

    // 화면 크기가 바뀌었거나, 다른 컴포넌트가 rect 를 덮어썼으면 다시 적용한다.
    //  GameScene 메인 카메라의 PixelPerfectCamera 가 시작할 때 rect 를 전체 화면으로 되돌려 여백이 사라졌다.
    void Update()
    {
        if (Screen.width != _width || Screen.height != _height || _camera.rect != _rect)
            Apply();
    }

    void Apply()
    {
        _width = Screen.width;
        _height = Screen.height;
        if (_camera == null || _width <= 0 || _height <= 0) return;

        float scale = ((float)_width / _height) / TARGET_ASPECT;
        Rect rect = new Rect(0f, 0f, 1f, 1f);
        if (scale < 1f)
        {
            rect.height = scale;                  // 화면이 더 길다 → 위아래 여백
            rect.y = (1f - scale) / 2f;
        }
        else
        {
            rect.width = 1f / scale;              // 화면이 더 넓다(PC) → 좌우 여백
            rect.x = (1f - rect.width) / 2f;
        }
        _camera.rect = rect;
        _rect = rect;
    }

    void CreateBarCamera()
    {
        var go = new GameObject("@LetterboxCamera");
        go.transform.SetParent(transform, false);
        var bar = go.AddComponent<Camera>();
        bar.cullingMask = 0;
        bar.clearFlags = CameraClearFlags.SolidColor;
        bar.backgroundColor = Color.black;
        bar.depth = _camera.depth - 100;
        bar.useOcclusionCulling = false;
        bar.allowHDR = false;
        bar.allowMSAA = false;
    }
}
