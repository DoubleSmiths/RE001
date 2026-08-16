using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 손으로 플레이하며 문제를 찾기 위한 조사 도구.
///
/// 자동 검증은 API를 직접 부른다(Managers.Quest.CheckButtonClick 등). 플레이어는 버튼을 누른다.
/// 그 사이에서 문제가 난다 — 버튼이 안 보이거나, 잠겨 있거나, 투명 막이 클릭을 가로채거나,
/// 눌러도 핸들러가 안 붙어 있거나. 그래서 **레이캐스트로 실제로 닿는지 확인한 뒤** 누른다.
/// </summary>
public static class PlaythroughProbe
{
    /// 화면에 보이는(활성) 버튼들을 이름·상태와 함께 나열. "지금 뭘 누를 수 있나"를 본다.
    public static string ListButtons()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (!b.gameObject.activeInHierarchy) continue;
            var rt = b.transform as RectTransform;
            if (rt == null) continue;
            string reach = Reachable(rt, out string blocker) ? "OK" : $"막힘({blocker})";
            sb.AppendLine($"{b.name} | interactable={b.interactable} | 클릭={reach}");
        }
        return sb.Length == 0 ? "(활성 버튼 없음)" : sb.ToString();
    }

    /// 이름으로 버튼을 눌러본다. 실제 클릭 경로(레이캐스트 최상단)로 보낸다.
    public static string Click(string name)
    {
        GameObject target = null;
        foreach (var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            if (b.gameObject.activeInHierarchy && b.name == name) { target = b.gameObject; break; }

        // Button 컴포넌트 없이 UI_EventHandler만 붙은 노드도 많다(이 프로젝트 관례).
        if (target == null)
            foreach (var h in Object.FindObjectsByType<UI_EventHandler>(FindObjectsSortMode.None))
                if (h.gameObject.activeInHierarchy && h.name == name) { target = h.gameObject; break; }

        if (target == null) return $"'{name}' 없음(비활성이거나 이름 불일치)";

        var trt = target.transform as RectTransform;
        if (trt != null && !Reachable(trt, out string blocker))
            return $"'{name}' 클릭이 '{blocker}'에 막힘";

        var btn = target.GetComponent<Button>();
        if (btn != null && !btn.interactable) return $"'{name}' 비활성(interactable=false)";

        var data = new PointerEventData(EventSystem.current);
        bool ok = ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
        return ok ? $"'{name}' 클릭됨" : $"'{name}' 클릭 핸들러 없음(반응 X)";
    }

    /// 이 RectTransform 중심으로 레이를 쏴서 자기 자신(또는 자식)이 맨 위에 잡히는지.
    static bool Reachable(RectTransform rt, out string blocker)
    {
        blocker = "";
        var es = EventSystem.current;
        if (es == null) { blocker = "EventSystem 없음"; return false; }

        var canvas = rt.GetComponentInParent<Canvas>();
        if (canvas == null) { blocker = "Canvas 없음"; return false; }
        var cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        // ⚠️ rt.position은 **피벗**이다. 이 프로젝트 버튼들은 피벗이 아래 모서리라 그 점을 찍으면
        //  버튼 바깥을 쏴서 멀쩡한 버튼이 "막힘"으로 나온다. 사각형의 중심을 써야 한다.
        Vector3 center = rt.TransformPoint(rt.rect.center);
        var ptr = new PointerEventData(es) { position = RectTransformUtility.WorldToScreenPoint(cam, center) };
        var hits = new List<RaycastResult>();
        es.RaycastAll(ptr, hits);
        if (hits.Count == 0) { blocker = "아무것도 안 잡힘"; return false; }

        var top = hits[0].gameObject.transform;
        for (var t = top; t != null; t = t.parent)
            if (t == rt) return true;

        blocker = hits[0].gameObject.name;
        return false;
    }

    /// 지금 화면 상태 요약 — 어디에 있고 뭘 가졌는지.
    public static string State()
    {
        var g = Managers.Game;
        var sb = new System.Text.StringBuilder();
        sb.Append($"Lv{g.Level} 퀘{g.CurrentQuestID}");
        if (Managers.SubQuest != null) sb.Append($" 서브퀘{Managers.SubQuest.CurrentSubQuestID}");
        sb.Append($" 골드{g.Gold} 노드{g.CurrentTravelAreaId}");
        sb.Append($" 해금버튼[{string.Join(",", g.UnlockedBtnIds)}]");

        var open = new List<string>();
        foreach (var p in Object.FindObjectsByType<UI_Popup>(FindObjectsSortMode.None))
            if (p.gameObject.activeInHierarchy) open.Add(p.GetType().Name);
        sb.Append($" 열린팝업[{string.Join(",", open)}]");
        return sb.ToString();
    }
}
