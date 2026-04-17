#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 테스트용 팝업 바로 띄우기 메뉴.
/// 에디터 상단 "Test" 메뉴에서 선택하거나 단축키로 실행.
/// PlayMode 중에만 동작.
/// </summary>
public static class PopupTestMenu
{
    // 단축키: Ctrl+Shift+1 ~ 4
    [MenuItem("Test/Open Custom Order Popup %#1", false, 100)]
    static void OpenCustomOrderPopup()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        Managers.UI.ShowPopupUI<UI_CustomOrderPopup>().SetInfo();
    }

    [MenuItem("Test/Open Potion Detail (HP Step1, from CustomOrder) %#2", false, 101)]
    static void OpenPotionDetailStep1() => OpenDetailOnTopOfCustomOrder(30001);

    [MenuItem("Test/Open Potion Detail (HP Step2, 재료 충족) %#3", false, 102)]
    static void OpenPotionDetailStep2() => OpenDetailOnTopOfCustomOrder(30002);

    [MenuItem("Test/Open Potion Detail (HP Step3, 재료부족) %#4", false, 103)]
    static void OpenPotionDetailStep3() => OpenDetailOnTopOfCustomOrder(30003);

    /// <summary>
    /// 실제 게임 플로우처럼 CustomOrder 팝업을 먼저 연 뒤 위에 상세 팝업을 열어
    /// 배경 darkening + 팝업 레이어링이 정상 보이도록 한다.
    /// </summary>
    static void OpenDetailOnTopOfCustomOrder(int dataId)
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        if (Managers.UI == null) { Debug.LogWarning("[Test] Managers.UI not ready"); return; }

        // CustomOrder가 이미 열려있지 않으면 먼저 오픈
        if (Object.FindObjectOfType<UI_CustomOrderPopup>() == null)
            Managers.UI.ShowPopupUI<UI_CustomOrderPopup>().SetInfo();

        // CustomOrder 진입 애님(0.5s) 완료 후 상세 팝업 오픈
        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += () =>
            {
                Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(dataId, fromCustomOrder: true);
            };
        };
    }

    [MenuItem("Test/Navigate to Alchemy Scene %#5", false, 200)]
    static void NavigateToAlchemy()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        var grp = Object.FindObjectOfType<UI_Village1BtnGroup>();
        if (grp == null) { Debug.LogWarning("[Test] UI_Village1BtnGroup not found"); return; }
        typeof(UI_Village1BtnGroup)
            .GetMethod("OnClickMain", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(grp, null);
        // 4초 뒤 Alchemy 자동 진입
        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += () =>
            {
                var g = Object.FindObjectOfType<UI_Village1BtnGroup>();
                if (g == null) return;
                typeof(UI_Village1BtnGroup)
                    .GetMethod("OnClickAlchemy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(g, null);
            };
        };
    }
}
#endif
