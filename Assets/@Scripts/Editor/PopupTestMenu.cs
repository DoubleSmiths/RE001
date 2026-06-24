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

    [MenuItem("Test/Open Potion Popup (S41) %#6", false, 104)]
    static void OpenPotionPopup()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        Managers.UI.ShowPopupUI<UI_PotionPopup>().SetInfo();
    }

    [MenuItem("Test/Toggle 자동물약 스킬 해금 (물약주입기) %#7", false, 105)]
    static void ToggleAutoPotionSkill()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        if (Managers.Game == null) { Debug.LogWarning("[Test] Managers.Game not ready"); return; }

        // ⚠️ 과거엔 죽은 PlayerPrefs("PotionPopup.Memorized")를 토글했으나 실제 시스템은
        //  Managers.Game.AutoPotionMemorized(GameData, 저장됨)를 읽는다. → 실제 해금 플래그를 토글.
        //  이 플래그가 true여야 물약창의 자동물약 On/Off 버튼이 사용 가능(UI_PotionPopup.OnClickAutoToggle 게이팅).
        bool next = !Managers.Game.AutoPotionMemorized;
        Managers.Game.AutoPotionMemorized = next;
        if (!next)
        {
            Managers.Game.AutoPotionOn = false;             // 잠그면 자동물약도 끔
            Managers.Game.AutoPotionSkillRevealed = false;  // 제작의뢰 기술 해금 모션도 재시청 가능하게 리셋
        }

        // 열려있는 물약창이 있으면 즉시 갱신 (캐시된 _autoMemorized 재로드)
        var popup = Object.FindObjectOfType<UI_PotionPopup>();
        if (popup != null) popup.SetInfo();
        // 열려있는 제작의뢰 팝업도 갱신 (기술 그룹 잠금/해금 반영)
        var custom = Object.FindObjectOfType<UI_CustomOrderPopup>();
        if (custom != null) custom.SetInfo();

        Debug.Log($"[Test] 자동물약 스킬(물약주입기) 해금 = {(next ? "ON — On/Off 버튼 사용 가능 + 제작의뢰 기술 해금" : "OFF — 잠김")}");
    }

    [MenuItem("Test/Toggle 행운물약 컨텐츠 해금 %#9", false, 107)]
    static void ToggleLuckPotionUnlock()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        if (Managers.Game == null) { Debug.LogWarning("[Test] Managers.Game not ready"); return; }

        // 행운물약 컨텐츠 해금 플래그 토글. ON이면 제작의뢰 물약탭 첫 조회 시 슬35 해금 모션 재생.
        bool next = !Managers.Game.LuckPotionUnlocked;
        Managers.Game.LuckPotionUnlocked = next;
        if (!next) Managers.Game.LuckPotionRevealed = false;   // 잠그면 모션 재생 플래그도 리셋(재시청 가능)

        var custom = Object.FindObjectOfType<UI_CustomOrderPopup>();
        if (custom != null) custom.SetInfo();

        Debug.Log($"[Test] 행운물약 컨텐츠 해금 = {(next ? "ON — 물약탭 첫 조회 시 해금 모션" : "OFF — 잠김")}");
    }

    [MenuItem("Test/Apply 스피드 버프 (속도+버프아이콘) %#8", false, 106)]
    static void ApplySpeedBuffTest()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        var p = Object.FindObjectOfType<PlayerController>();
        if (p == null) { Debug.LogWarning("[Test] PlayerController 없음 — GameScene에서 실행하세요"); return; }
        // 실제 신속버프 적용 — 공격속도 +30%/15초 + 좌상단 버프 아이콘(지속게이지+점멸+FadeOut)
        p.ApplySpeedBuff(30f, 15f);
        Debug.Log("[Test] 스피드 버프: 공격속도 +30% / 15초 (PotionAtkSpeedBonus 적용)");
    }

    [MenuItem("Test/Navigate to Alchemy Scene %#0", false, 200)]
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
