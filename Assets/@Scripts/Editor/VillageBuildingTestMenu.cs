#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 마을 건물(수도원·대장간·상점) 검증 메뉴.
//
//  이 셋은 전부 **메인 로비 버튼 → 건물 그룹**이라는 같은 흐름을 쓴다.
//  수도원은 그룹까지 저작돼 있었는데 아이콘에 클릭 바인딩이 없어 들어갈 방법이 없었고,
//  대장간·상점은 TempButton1/TempButton2 빈칸에 새로 들어갔다.
//
//  사용법: PlayMode 진입 → Test/Village/… 순서대로.
public static class VillageBuildingTestMenu
{
    static bool Require()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Village] PlayMode 진입 후 실행하세요.");
            return false;
        }
        if (Managers.Game == null)
        {
            Debug.LogWarning("[Village] Managers 아직 준비 안 됨.");
            return false;
        }
        return true;
    }

    /// <summary>씬에 살아 있는 마을 버튼 그룹. 프리팹 인스턴스라 Find로 잡는다.</summary>
    static UI_Village1BtnGroup Group()
    {
        var g = Object.FindObjectOfType<UI_Village1BtnGroup>(true);
        if (g == null) Debug.LogWarning("[Village] UI_Village1BtnGroup을 찾지 못했습니다. 마을 화면인지 확인하세요.");
        return g;
    }

    /// <summary>private 클릭 핸들러를 직접 호출한다 (UGUI 클릭을 흉내낼 필요 없이 흐름만 검증).</summary>
    static void Invoke(string method)
    {
        var g = Group();
        if (g == null) return;

        MethodInfo mi = typeof(UI_Village1BtnGroup)
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (mi == null)
        {
            Debug.LogError($"[Village] 메서드를 찾지 못했습니다: {method}");
            return;
        }
        mi.Invoke(g, null);
        Debug.Log($"[Village] {method}() 호출 — 진입 연출은 약 1.3초 뒤 완료됩니다.");
    }

    // ─────────────────────────────────────────────────────────
    [MenuItem("Test/Village/0. 마을 건물 3종 해금 (수도원·대장간·상점)", false, 0)]
    static void UnlockAll()
    {
        if (!Require()) return;

        foreach (string id in new[] { UNLOCK_MONASTERY_BUTTON, UNLOCK_FORGE_BUTTON, UNLOCK_SHOP_BUTTON })
            Managers.Game.AddUnlockedBtn(id);

        var g = Group();
        if (g != null)
        {
            // SetInfo가 해금 상태를 보고 아이콘/라벨을 켠다.
            MethodInfo mi = typeof(UI_Village1BtnGroup)
                .GetMethod("SetInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            mi?.Invoke(g, null);
        }

        Debug.Log("[Village] 수도원/대장간/상점 해금 완료. 로비에 아이콘 3개가 더 보여야 합니다.");
    }

    [MenuItem("Test/Village/1. 수도원 진입", false, 1)]
    static void EnterMonastery() { if (Require()) Invoke("OnClickMonastery"); }

    [MenuItem("Test/Village/2. 대장간 진입", false, 2)]
    static void EnterForge() { if (Require()) Invoke("OnClickForge"); }

    [MenuItem("Test/Village/3. 상점 진입", false, 3)]
    static void EnterShop() { if (Require()) Invoke("OnClickShop"); }

    [MenuItem("Test/Village/4. 마을 로비로", false, 4)]
    static void BackToMain() { if (Require()) Invoke("OnClickMain"); }

    // ─────────────────────────────────────────────────────────
    //  느낌표는 현재 퀘스트를 그대로 비춰야 한다.
    //  노랑(FFE300) = 메인 퀘스트 / 초록(14FF00) = 서브 퀘스트.
    [MenuItem("Test/Village/8. 느낌표 검증 (현재 퀘스트 대조)", false, 19)]
    static void VerifyNotify()
    {
        if (!Require()) return;

        var g = Group();
        if (g == null) return;

        // 지금 켜져 **있어야 할** 것 — 매니저가 주는 알림 목록
        var main = new HashSet<string>();
        var sub = new HashSet<string>();
        foreach (var n in Managers.Quest.GetCurrentNotifyList())
            (n.IsSubQuest ? sub : main).Add(n.BtnID);

        var sb = new StringBuilder();
        sb.AppendLine("── 느낌표 검증 ──");
        sb.AppendLine($"  메인 퀘스트 {Managers.Quest.CurrentQuestID} / 서브 {Managers.SubQuest?.CurrentSubQuestID} (활성 {Managers.SubQuest?.IsActive})");
        sb.AppendLine($"  기대 — 메인(노랑): {(main.Count == 0 ? "없음" : string.Join(", ", main))}");
        sb.AppendLine($"  기대 — 서브(초록): {(sub.Count == 0 ? "없음" : string.Join(", ", sub))}");

        int onCount = 0, wrongColor = 0;
        foreach (var img in g.GetComponentsInChildren<Image>(true))
        {
            Transform p = img.transform.parent;
            if (p == null || !p.name.Contains("VFX")) continue;
            if (!p.gameObject.activeSelf) continue;

            onCount++;
            string hex = ColorUtility.ToHtmlStringRGB(img.color);
            bool isGreen = hex == "14FF00";
            bool nameSaysSub = p.name.EndsWith("_Sub1");

            // 이름 규약과 실제 색이 어긋나면 데이터가 아니라 프리팹이 잘못된 것이다.
            if (isGreen != nameSaysSub) { wrongColor++; sb.AppendLine($"  ⚠ 색/이름 불일치: {p.name} = #{hex}"); }

            sb.AppendLine($"  켜짐: {p.name,-40} #{hex} {(isGreen ? "서브" : "메인")}");
        }

        if (onCount == 0) sb.AppendLine("  켜진 느낌표 없음");
        sb.AppendLine($"  색/이름 불일치 {wrongColor}건");
        Debug.Log(sb.ToString());
    }

    [MenuItem("Test/Village/9. 현재 상태 덤프", false, 20)]
    static void Dump()
    {
        if (!Require()) return;

        var g = Group();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("── 마을 건물 상태 ──");
        sb.AppendLine($"  LastVillage1Group = {Managers.Game.LastVillage1Group}");
        sb.AppendLine($"  CurrentTravelAreaId = {Managers.Game.CurrentTravelAreaId}");
        sb.AppendLine($"  해금: 수도원={Managers.Game.UnlockedBtnIds.Contains(UNLOCK_MONASTERY_BUTTON)}" +
                      $" 대장간={Managers.Game.UnlockedBtnIds.Contains(UNLOCK_FORGE_BUTTON)}" +
                      $" 상점={Managers.Game.UnlockedBtnIds.Contains(UNLOCK_SHOP_BUTTON)}");

        if (g != null)
        {
            foreach (string n in new[]
            {
                "Village1MainBtnGroup", "Village1GuildBtnGroup", "Village1AlchemyBtnGroup",
                "Village1MonasteryBtnGroup", "Village1ForgeBtnGroup", "Village1ShopBtnGroup",
                "MonasteryButton", "TempButton1", "TempButton2",
            })
            {
                Transform t = FindDeep(g.transform, n);
                sb.AppendLine(t == null
                    ? $"  {n} = (없음)"
                    : $"  {n} = active {t.gameObject.activeInHierarchy}");
            }
        }

        // 배경 프리팹은 씬 루트에 스폰된다
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name.StartsWith("Village1_")) sb.AppendLine($"  BG = {go.name}");

        Debug.Log(sb.ToString());
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform r = FindDeep(root.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }
}
#endif
