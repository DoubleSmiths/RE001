using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// 장비 아이콘이 "튄다"는 지적을 픽셀 추측이 아니라 **좌표 로그**로 잡는다.
//
//  아이콘은 두 창을 건너간다 — 망치질은 강화창의 CraftEquipIcon이, 결과는 결과창의 Icon이
//  맡는다. 서로 부모가 달라 화면 위 자리가 어긋나면 전환 순간에 툭 튄다.
//  그래서 둘 다 **화면 좌표(world corners → screen)** 로 환산해 같은 자에 놓고 잰다.
//
//  Test/Forge/연출 전 구간 녹화와 같이 돌리면 프레임 번호까지 맞춰 볼 수 있다.
public static class ForgeIconTracer
{
    const float DURATION = 20f;

    static StringBuilder s_log;
    static double s_t0, s_end;

    [MenuItem("Test/Forge/장비 아이콘 좌표 추적 (20초)")]
    static void Run()
    {
        if (!Application.isPlaying) { Debug.LogError("[Tracer] 플레이 모드에서 실행하세요."); return; }

        s_log = new StringBuilder("t\t어느창\t화면x\t화면y\t폭\t높이\t알파\n");
        s_t0 = EditorApplication.timeSinceStartup;
        s_end = s_t0 + DURATION;

        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Debug.Log("[Tracer] 추적 시작");
    }

    static void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now >= s_end)
        {
            EditorApplication.update -= Tick;
            string path = Path.Combine("Screenshots", "icon_trace.tsv");
            File.WriteAllText(path, s_log.ToString());
            Debug.Log($"[Tracer] 추적 종료 → {path}");
            return;
        }

        float t = (float)(now - s_t0);

        // 결과창이 열려 있으면 그쪽이 주인공이다(강화창 아이콘은 그 밑에 가린다).
        var prog = Object.FindObjectOfType<UI_EnhanceProgressPopup>();
        if (prog != null && Sample(t, "결과창", Util.FindChild(prog.gameObject, "Icon", true))) return;

        var enh = Object.FindObjectOfType<UI_EnhacePopup>();
        if (enh != null) Sample(t, "강화창", Util.FindChild(enh.gameObject, "CraftEquipIcon", true));
    }

    static bool Sample(float t, string who, GameObject go)
    {
        var rt = go != null ? go.transform as RectTransform : null;
        if (rt == null) return false;

        var c = new Vector3[4];
        rt.GetWorldCorners(c);

        var cam = rt.GetComponentInParent<Canvas>()?.worldCamera;
        Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, c[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, c[2]);

        // 흰 덮개(Flash)가 얼마나 덮고 있는지도 같이 — 백열 구간을 구분하기 위해
        float a = 1f;
        var flash = Util.FindChild(go, "Flash", true)?.GetComponent<UnityEngine.UI.Image>();
        if (flash != null && flash.gameObject.activeInHierarchy) a = flash.color.a;

        s_log.AppendLine($"{t:0.000}\t{who}\t{(bl.x + tr.x) * 0.5f:0.0}\t{(bl.y + tr.y) * 0.5f:0.0}\t"
                       + $"{tr.x - bl.x:0.0}\t{tr.y - bl.y:0.0}\t{a:0.00}");
        return true;
    }
}
