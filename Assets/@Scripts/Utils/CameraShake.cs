using DG.Tweening;
using UnityEngine;

// 타격 카메라 흔들림(ADR0033). 설정의 '화면 흔들림'(GameManager.IsShakeOn)을 따른다.
//  HUD 캔버스는 Screen Space - Camera 라 카메라와 함께 움직여 화면 위에서는 흔들리지 않는다 — 월드만 흔들린다.
//  겹쳐 들어오면 앞 흔들림을 끊고 원위치에서 다시 흔든다(위치가 조금씩 밀려 쌓이지 않게). 전투 정지 중에도 끝나도록 실시간.
public static class CameraShake
{
    static Tween     s_tween;
    static Transform s_cam;
    static Vector3   s_base;

    public static void Play(float strength, float duration = Define.SHAKE_DURATION)
    {
        if (strength <= 0f) return;
        if (Managers.Game != null && !Managers.Game.IsShakeOn) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        if (s_tween != null && s_tween.IsActive()) s_tween.Kill();   // OnKill 이 원위치로 되돌린다
        if (s_cam != cam.transform) { s_cam = cam.transform; s_base = s_cam.position; }

        Transform t = s_cam;
        Vector3 home = s_base;
        s_tween = t.DOShakePosition(duration, new Vector3(strength, strength, 0f), 24, 90f, false, true)
            .SetUpdate(true)
            .SetLink(t.gameObject)
            .OnKill(() => { if (t != null) t.position = home; });
    }
}
