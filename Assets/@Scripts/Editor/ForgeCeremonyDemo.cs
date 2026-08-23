using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 강화 연출 전 구간(강화창 등장 → 주문서 선택 → 망치질 → 결과)을 **한 번에 재생하면서**
//  프레임을 찍어 영상으로 만든다. 원본 영상 3:20~3:50과 나란히 놓고 보기 위한 것이다.
//
//  ForgeCeremonyRecorder가 한 토막씩 찍는 도구라면, 이쪽은 대본을 처음부터 끝까지 돌린다.
//  타이밍은 원본 영상에서 읽은 것이다(강화창 등장 → 2.5s 뒤 주문서 열기 → 1.5s 뒤 고르기
//  → 1.5s 뒤 강화).
public static class ForgeCeremonyDemo
{
    const float INTERVAL = 1f / 30f;      // 원본과 같은 30fps
    const float DURATION = 30f;

    static string s_dir;
    static double s_next, s_end;
    static int s_idx;

    [MenuItem("Test/Forge/연출 전 구간 녹화 (30초)")]
    static void Run()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("[Demo] 플레이 모드에서 실행하세요.");
            return;
        }

        s_dir = Path.Combine("Screenshots", "demo30");
        if (Directory.Exists(s_dir)) Directory.Delete(s_dir, true);
        Directory.CreateDirectory(s_dir);

        s_idx = 0;
        s_next = EditorApplication.timeSinceStartup;
        s_end = s_next + DURATION;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;

        var host = Object.FindObjectOfType<UI_GameScene>();
        if (host == null) { Debug.LogError("[Demo] GameScene을 찾지 못했습니다."); return; }
        host.StartCoroutine(CoScript());

        Debug.Log("[Demo] 30초 녹화 시작");
    }

    // 박자는 원본 영상(3:20~3:50)에서 읽었다 — 대장간 → 강화창 등장 → 주문서 고르기 →
    //  강화 → 망치질 → 결과 → 복귀. 나란히 놓고 볼 것이므로 같은 자리에서 같은 일이 일어나야 한다.
    const float T_FORGE   = 0.4f;    // 대장간 팝업 (원본 206s)
    const float T_ENHANCE = 1.6f;    // 강화창 등장 (원본 208s)
    const float T_OPEN    = 6.0f;    // 주문서 슬롯 열기 (원본 213.5s)
    const float T_PICK    = 8.2f;    // 주문서 고르기 (원본 215.5s)
    const float T_SMITH   = 10.6f;   // [강화] (원본 217.6s)
    const float T_CLOSE   = 27.0f;   // 결과창 닫기 → 복귀 (원본 228.6s)

    static IEnumerator CoScript()
    {
        // 깨끗한 상태에서 시작 — 강화도 0, 내구도 만땅, 주문서 넉넉히
        var eq = Managers.Game.OwnedEquipments.Find(e => e.EquipmentData != null);
        if (eq == null) { Debug.LogError("[Demo] 장비가 없습니다."); yield break; }

        eq.EnhanceLevel = 0;
        eq.Durability = eq.EquipmentData.MaxDurability;
        Managers.Game.Gold = 100_000_000;
        Managers.Game.AddItem(Define.SCROLL_IGNUS_ID, 20);

        Managers.UI.CloseAllPopupUI();

        double t0 = EditorApplication.timeSinceStartup;
        IEnumerator At(float sec)
        {
            while (EditorApplication.timeSinceStartup - t0 < sec) yield return null;
        }

        yield return At(T_FORGE);
        var forge = Managers.UI.ShowPopupUI<UI_ForgePopup>();

        yield return At(T_ENHANCE);
        var popup = Managers.UI.ShowPopupUI<UI_EnhacePopup>();
        popup.SetInfo(eq, Define.SCROLL_BASIC_ID);

        yield return At(T_OPEN);
        Invoke(popup, "ToggleScrollSelect");

        yield return At(T_PICK);
        // 확정 강화 주문서 — 연출을 성공으로 고정한다(비교 영상이므로 실패가 나오면 안 된다)
        var pick = typeof(UI_EnhacePopup).GetMethod("OnPickScroll", BindingFlags.NonPublic | BindingFlags.Instance);
        pick?.Invoke(popup, new object[] { System.Array.IndexOf(Define.ENHANCE_SCROLL_IDS, Define.SCROLL_IGNUS_ID) });

        yield return At(T_SMITH);
        Invoke(popup, "OnClickEnhance");

        yield return At(T_CLOSE);
        Managers.UI.ClosePopupUI();   // 결과창을 닫아 강화창으로 돌아온다
    }

    static void Invoke(object target, string method)
        => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(target, null);

    static void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now >= s_end)
        {
            EditorApplication.update -= Tick;
            Debug.Log($"[Demo] 녹화 종료 — {s_idx}장 → {s_dir}");
            return;
        }
        if (now < s_next) return;

        s_next = now + INTERVAL;
        ScreenCapture.CaptureScreenshot(Path.Combine(s_dir, $"{s_idx:0000}.png"));
        s_idx++;
    }
}
