using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 강화 연출을 프레임 단위로 찍어 원본 영상(3:20~3:50)과 나란히 놓고 본다.
//
//  "구현했다"는 말은 스크린샷 한 장으로 증명되지 않는다 — 연출은 시간축에 있고,
//  정지 화면에서는 아이콘이 떠올랐는지, 텍스트가 차례로 붙었는지 알 수 없다.
//  그래서 0.1초 간격으로 게임뷰를 찍어 필름처럼 늘어놓는다.
//
//  캡처는 EditorApplication.update로 돈다(런타임 코드에 녹화용 훅을 심지 않기 위해).
public static class ForgeCeremonyRecorder
{
    const float INTERVAL = 0.1f;

    static string s_dir;
    static double s_next, s_end;
    static int s_idx;

    [MenuItem("Test/Forge/강화 연출 캡처 (확정 성공)")]
    static void RecordSuccess()
    {
        if (!Guard(out var popup)) return;

        // 이그누스의 인정 = 확정 강화. 확률에 맡기면 성공 연출을 볼 때까지 몇 번을 돌려야 한다.
        var f = typeof(UI_EnhacePopup).GetField("_scrollId", BindingFlags.NonPublic | BindingFlags.Instance);
        f?.SetValue(popup, Define.SCROLL_IGNUS_ID);
        Managers.Game.AddItem(Define.SCROLL_IGNUS_ID, 5);

        Begin("cer_success", 11f);
        typeof(UI_EnhacePopup).GetMethod("OnClickEnhance", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(popup, null);
    }

    [MenuItem("Test/Forge/강화창 등장 캡처")]
    static void RecordIntro()
    {
        if (!Guard(out var popup)) return;

        // 인트로는 1회용이라(_introPlayed) 다시 보려면 깃발을 내려야 한다.
        typeof(UI_EnhacePopup).GetField("_introPlayed", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(popup, false);

        Begin("cer_intro", 3f);
        typeof(UI_EnhacePopup).GetMethod("PlayIntro", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(popup, null);
    }

    static bool Guard(out UI_EnhacePopup popup)
    {
        popup = Object.FindObjectOfType<UI_EnhacePopup>();
        if (!Application.isPlaying) { Debug.LogError("[Recorder] 플레이 모드에서만 동작합니다."); return false; }
        if (popup == null) { Debug.LogError("[Recorder] 강화창이 열려 있지 않습니다."); return false; }
        return true;
    }

    static void Begin(string folder, float duration)
    {
        s_dir = Path.Combine("Screenshots", folder);
        if (Directory.Exists(s_dir)) Directory.Delete(s_dir, true);
        Directory.CreateDirectory(s_dir);

        s_idx = 0;
        s_next = EditorApplication.timeSinceStartup;
        s_end = s_next + duration;

        EditorApplication.update -= Tick;   // 겹쳐 걸리면 프레임 번호가 엉킨다
        EditorApplication.update += Tick;
        Debug.Log($"[Recorder] {folder} 캡처 시작 ({duration}초)");
    }

    static void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now >= s_end)
        {
            EditorApplication.update -= Tick;
            Debug.Log($"[Recorder] 캡처 종료 — {s_idx}장 → {s_dir}");
            return;
        }
        if (now < s_next) return;

        s_next = now + INTERVAL;
        ScreenCapture.CaptureScreenshot(Path.Combine(s_dir, $"{s_idx:000}.png"));
        s_idx++;
    }
}
