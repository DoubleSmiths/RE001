using UnityEditor;
using UnityEngine;

/// <summary>
/// 슬라이드 3 이펙트(Star/GlowDot)가 안 보이는 문제 진단용.
/// 플레이 모드에서 실행하면 여정 버튼 위에 두 이펙트를 10초간 스폰하고
/// 월드 위치/스케일/렌더러 바운즈/카메라 프러스텀 포함 여부를 Console에 출력한다.
/// Hierarchy에서 UnlockFxDebug_* 오브젝트를 선택해 Scene 뷰로 직접 확인 가능.
/// </summary>
public static class UnlockFxDebugMenu
{
    [MenuItem("Test/FX/여정 버튼 이펙트 진단 (10초 유지)")]
    static void SpawnOnTravelButton()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }

        GameObject btn = GameObject.Find("TravelButton");
        if (btn == null)
        {
            Debug.LogError("[UnlockFxDebug] TravelButton을 찾지 못함 — 마을 메인 화면에서 실행해 주세요.");
            return;
        }

        // 실제 연출과 동일 경로(UIFx.PlayParticle → UIParticle 래퍼)로 스폰
        UIFx.PlayParticle((RectTransform)btn.transform, Vector2.zero, "Star_01_Anim_13_a", 10f);
        UIFx.PlayParticle((RectTransform)btn.transform, Vector2.zero, "GlowDot_01", 10f);
        Debug.Log("[UnlockFxDebug] UIParticle 경로로 스폰 완료 — 게임 뷰에서 확인하세요. (구 방식 raw 스폰은 Spawn() 참고)");
    }

    static void Spawn(string key, Transform parent)
    {
        GameObject fx = Managers.Resource.Instantiate(key, parent);
        if (fx == null)
        {
            Debug.LogError($"[UnlockFxDebug] Instantiate 실패(null): {key} — PreLoad 캐시 미포함?");
            return;
        }
        fx.name = "UnlockFxDebug_" + key;
        fx.transform.localPosition = Vector3.zero;
        fx.transform.localScale *= 22.5f;
        Object.Destroy(fx, 10f);

        Camera cam = Camera.main;
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        foreach (var r in fx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            bool inView = GeometryUtility.TestPlanesAABB(planes, r.bounds);
            Debug.Log($"[UnlockFxDebug] {key}/{r.gameObject.name}: worldPos={r.transform.position}, " +
                      $"lossyScale={r.transform.lossyScale.x:F2}, bounds={r.bounds.size}, " +
                      $"sortingOrder={r.sortingOrder}, layer={LayerMask.LayerToName(r.gameObject.layer)}, " +
                      $"카메라프러스텀포함={inView}, 셰이더={(r.sharedMaterial != null ? r.sharedMaterial.shader.name : "null")}");
        }
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
            Debug.Log($"[UnlockFxDebug] {key}/{ps.gameObject.name}: isPlaying={ps.isPlaying}, particleCount={ps.particleCount}");

        Debug.Log($"[UnlockFxDebug] 카메라: pos={cam.transform.position}, near={cam.nearClipPlane}, far={cam.farClipPlane}, ortho={cam.orthographic}(size={cam.orthographicSize}), cullingMask={cam.cullingMask}");
    }
}
