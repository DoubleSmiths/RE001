#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// 마물 도감(슬라이드 70~75) 검증용 임시 메뉴. PlayMode 중 Test/Bestiary 에서 선택.
// 기록도/별등급은 PlayerPrefs 스텁(UI_BestiaryPopup.Get/SetRecordRate, Get/SetStarGrade).
public static class BestiaryTestMenu
{
    // 스텁 매핑(REGION_STUB)에 들어있는 검증 대상 몬스터.
    static readonly int[] MONSTER_IDS = { 2001, 2002 };

    // ─────────────────────────────────────────────
    // 팝업 열기 / 갱신
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/Open Bestiary Popup %#9", false, 400)]
    static void OpenBestiaryPopup()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        if (Managers.UI == null) { Debug.LogWarning("[BestiaryTest] Managers.UI not ready"); return; }
        Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo();
    }

    // 기록도를 바꾼 뒤 이미 열린 팝업을 즉시 다시 그린다(없으면 새로 연다).
    static void RefreshOrOpen()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BestiaryTest] PlayMode 진입 후 실행"); return; }
        var popup = Object.FindObjectOfType<UI_BestiaryPopup>();
        if (popup != null)
            popup.SetInfo();
        else
            Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo();
    }

    // ─────────────────────────────────────────────
    // 3분류 시드 (슬라이드 73)
    //  0% = "?" 잠금 / 0~10미만 = 검정 실루엣 / 10이상 = 풀컬러
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/Seed - Mixed (풀컬러+실루엣)", false, 420)]
    static void SeedMixed()
    {
        UI_BestiaryPopup.SetRecordRate(2001, 100); // 풀컬러 + Rarity 글로우
        UI_BestiaryPopup.SetStarGrade(2001, 1);    // 별 표기
        UI_BestiaryPopup.SetRecordRate(2002, 5);   // 검정 실루엣
        UI_BestiaryPopup.SetStarGrade(2002, 0);
        Debug.Log("[BestiaryTest] Mixed: 2001=100/별1(풀컬러), 2002=5/별0(실루엣)");
        RefreshOrOpen();
    }

    [MenuItem("Test/Bestiary/Seed - All Full (전부 풀컬러+별)", false, 421)]
    static void SeedAllFull()
    {
        foreach (int id in MONSTER_IDS)
        {
            UI_BestiaryPopup.SetRecordRate(id, 100);
            UI_BestiaryPopup.SetStarGrade(id, 3);
        }
        Debug.Log("[BestiaryTest] All Full: 전부 기록도 100 / 별3");
        RefreshOrOpen();
    }

    [MenuItem("Test/Bestiary/Seed - All Locked (전부 0% \"?\")", false, 422)]
    static void SeedAllLocked()
    {
        foreach (int id in MONSTER_IDS)
        {
            UI_BestiaryPopup.SetRecordRate(id, 0);
            UI_BestiaryPopup.SetStarGrade(id, 0);
        }
        Debug.Log("[BestiaryTest] All Locked: 전부 기록도 0 (미발견, Interactive X)");
        RefreshOrOpen();
    }

    [MenuItem("Test/Bestiary/Seed - All Silhouette (전부 실루엣)", false, 423)]
    static void SeedAllSilhouette()
    {
        foreach (int id in MONSTER_IDS)
        {
            UI_BestiaryPopup.SetRecordRate(id, 5); // 0 < rec < 10
            UI_BestiaryPopup.SetStarGrade(id, 0);
        }
        Debug.Log("[BestiaryTest] All Silhouette: 전부 기록도 5 (검정 실루엣)");
        RefreshOrOpen();
    }

    // ─────────────────────────────────────────────
    // 2002 기록도 단계 순환 (0 → 5 → 50 → 100) — 전환 시각 확인용
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/Cycle 2002 RecordRate (0→5→50→100)", false, 440)]
    static void Cycle2002()
    {
        int cur = UI_BestiaryPopup.GetRecordRate(2002);
        int next = cur <= 0 ? 5 : cur < 10 ? 50 : cur < 100 ? 100 : 0;
        UI_BestiaryPopup.SetRecordRate(2002, next);
        Debug.Log($"[BestiaryTest] 2002 기록도 {cur} → {next} " +
                  (next == 0 ? "(? 잠금)" : next < 10 ? "(실루엣)" : "(풀컬러)"));
        RefreshOrOpen();
    }

    // ─────────────────────────────────────────────
    // 별등급 토글 (2001, 0↔1↔2↔3)
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/Cycle 2001 StarGrade (0→1→2→3)", false, 441)]
    static void Cycle2001Star()
    {
        int cur = UI_BestiaryPopup.GetStarGrade(2001);
        int next = (cur + 1) % 4;
        UI_BestiaryPopup.SetStarGrade(2001, next);
        Debug.Log($"[BestiaryTest] 2001 별등급 {cur} → {next}");
        RefreshOrOpen();
    }

    // ─────────────────────────────────────────────
    // 슬라이드 71: 상단 4개 스킬 (0:종초월교감 1:조련사 2:오다주웠 3:기억확장)
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/Skills - Unlock All", false, 460)]
    static void SkillsUnlockAll()
    {
        for (int i = 0; i < 4; i++) UI_BestiaryPopup.SetSkillUnlocked(i, true);
        Debug.Log("[BestiaryTest] 스킬 4개 전부 해금 (idle Anim + 클릭 가능)");
        RefreshOrOpen();
    }

    [MenuItem("Test/Bestiary/Skills - Reset (기억확장만 해금)", false, 461)]
    static void SkillsReset()
    {
        for (int i = 0; i < 4; i++)
        {
            UI_BestiaryPopup.SetSkillUnlocked(i, i == 3);
            UI_BestiaryPopup.SetSkillGrade(i, 0);
        }
        Debug.Log("[BestiaryTest] 스킬 기본 상태 (idx3 기억확장만 해금, 등급 0)");
        RefreshOrOpen();
    }

    [MenuItem("Test/Bestiary/Skills - Cycle idx0 Grade (+0→+1→+2→+3)", false, 462)]
    static void SkillsCycleGrade0()
    {
        int cur = UI_BestiaryPopup.GetSkillGrade(0);
        int next = (cur + 1) % 4;
        UI_BestiaryPopup.SetSkillGrade(0, next);
        Debug.Log($"[BestiaryTest] 스킬 idx0(종초월교감) 등급 +{cur} → +{next}");
        RefreshOrOpen();
    }

    // ─────────────────────────────────────────────
    // 슬라이드 74: 별 등급 상승 연출
    // ─────────────────────────────────────────────
    static void TriggerGradeUp(int dataId, int newStar)
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BestiaryTest] PlayMode 진입 후 실행"); return; }
        var popup = Object.FindObjectOfType<UI_BestiaryPopup>();
        if (popup == null)
        {
            // 팝업이 닫혀있으면 먼저 열고 진입 애님 후 연출
            popup = Managers.UI.ShowPopupUI<UI_BestiaryPopup>();
            popup.SetInfo();
            EditorApplication.delayCall += () =>
                EditorApplication.delayCall += () =>
                {
                    var p = Object.FindObjectOfType<UI_BestiaryPopup>();
                    if (p != null) p.PlayGradeUp(dataId, newStar);
                };
            return;
        }
        popup.PlayGradeUp(dataId, newStar);
    }

    [MenuItem("Test/Bestiary/GradeUp 2001 → 1단계 (미니몬만 Anim)", false, 480)]
    static void GradeUp2001Sub3()
    {
        UI_BestiaryPopup.SetRecordRate(2001, 100); // 발견 상태 보장
        TriggerGradeUp(2001, 1);
        Debug.Log("[BestiaryTest] 2001 별 1단계 — 미니몬 아이콘만 Punch/Scale");
    }

    [MenuItem("Test/Bestiary/GradeUp 2001 → 3단계 보라 (풀 시퀀스)", false, 481)]
    static void GradeUp2001Purple()
    {
        UI_BestiaryPopup.SetRecordRate(2001, 100);
        TriggerGradeUp(2001, 3);
        Debug.Log("[BestiaryTest] 2001 보라 달성 — 기술→미니몬→완성도별→숫자 순차 + Flash");
    }

    [MenuItem("Test/Bestiary/Open Monster Detail (2001)", false, 470)]
    static void OpenMonsterDetail2001()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001);
    }

    static void ReopenMonsterDetail(int dataId)
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BestiaryTest] PlayMode 진입 후 실행"); return; }
        var p = Object.FindObjectOfType<UI_MonsterDetailPopup>();
        if (p != null) p.SetInfo(dataId);
        else Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(dataId);
    }

    // 슬라이드 78: 기록도 0(검정/???)→60(색복귀/스탯)→100(전리품 해금)
    [MenuItem("Test/Bestiary/Detail - Cycle 2001 기록도 (0→60→100)", false, 471)]
    static void CycleDetailRec()
    {
        int cur = UI_BestiaryPopup.GetRecordRate(2001);
        int next = cur <= 0 ? 60 : cur < 100 ? 100 : 0;
        UI_BestiaryPopup.SetRecordRate(2001, next);
        Debug.Log($"[BestiaryTest] 2001 기록도 {cur}→{next}");
        ReopenMonsterDetail(2001);
    }

    // 슬라이드 79: 공명도 0(FullLock)→50(Lock,interactive X)→100(해금+영구 학습)
    [MenuItem("Test/Bestiary/Detail - Cycle 2001 공명도 (0→50→100)", false, 472)]
    static void CycleDetailSync()
    {
        int cur = UI_BestiaryPopup.GetSync(2001);
        int next = cur <= 0 ? 50 : cur < 100 ? 100 : 0;
        UI_BestiaryPopup.SetSync(2001, next);
        Debug.Log($"[BestiaryTest] 2001 공명도 {cur}→{next} (learned={UI_BestiaryPopup.IsUniqueSkillLearned(2001)})");
        ReopenMonsterDetail(2001);
    }

    [MenuItem("Test/Bestiary/Detail - Toggle 2001 학습 (UniqueSkill)", false, 473)]
    static void ToggleLearned2001()
    {
        bool cur = UI_BestiaryPopup.IsUniqueSkillLearned(2001);
        UI_BestiaryPopup.SetUniqueSkillLearned(2001, !cur);
        Debug.Log($"[BestiaryTest] 2001 고유기술 학습 {cur}→{!cur}");
        ReopenMonsterDetail(2001);
    }

    [MenuItem("Test/Bestiary/Detail - Reset 2001 학습/공명도", false, 474)]
    static void ResetLearnedSync2001()
    {
        UI_BestiaryPopup.SetUniqueSkillLearned(2001, false);
        UI_BestiaryPopup.SetSync(2001, 0);
        UI_BestiaryPopup.SetAllSS(2001, false);
        UI_BestiaryPopup.RecomputeStar(2001);
        Debug.Log("[BestiaryTest] 2001 학습/공명도/AllSS/별 초기화");
        ReopenMonsterDetail(2001);
    }

    // 슬라이드 80: 모든 능력치 SS 토글 (완벽획득/+2등급 검증)
    [MenuItem("Test/Bestiary/Detail - Toggle 2001 AllSS (완벽획득)", false, 475)]
    static void ToggleAllSS2001()
    {
        bool cur = UI_BestiaryPopup.IsAllSS(2001);
        UI_BestiaryPopup.SetAllSS(2001, !cur);
        Debug.Log($"[BestiaryTest] 2001 AllSS {cur}→{!cur} (별={UI_BestiaryPopup.GetStarGrade(2001)})");
        ReopenMonsterDetail(2001);
    }

    // 슬라이드 83: 3조건 모두 만족 → 별 3단계
    [MenuItem("Test/Bestiary/Detail - 2001 별 3단계 (rec100+sync100+AllSS)", false, 476)]
    static void Star3_2001()
    {
        UI_BestiaryPopup.SetRecordRate(2001, 100);
        UI_BestiaryPopup.SetSync(2001, 100);
        UI_BestiaryPopup.SetAllSS(2001, true);
        Debug.Log($"[BestiaryTest] 2001 별={UI_BestiaryPopup.GetStarGrade(2001)} (학습={UI_BestiaryPopup.IsUniqueSkillLearned(2001)})");
        ReopenMonsterDetail(2001);
    }

    [MenuItem("Test/Bestiary/GradeUp Reset (회전/완성도/NEW 초기화)", false, 482)]
    static void GradeUpReset()
    {
        UI_BestiaryPopup.ResetSkillRotation();
        for (int i = 0; i < 4; i++)
        {
            UI_BestiaryPopup.SetSkillUnlocked(i, i == 3);
            UI_BestiaryPopup.SetSkillGrade(i, 0);
        }
        UI_BestiaryPopup.SetStarGrade(2001, 0);
        UI_BestiaryPopup.SetStarGrade(2002, 0);
        Debug.Log("[BestiaryTest] 회전 포인터/완성도/NEW/스킬등급 전부 초기화");
        RefreshOrOpen();
    }

    // ─────────────────────────────────────────────
    // 슬라이드 84~88: 나의 미니몬 (UI_MinionInfoPopup)
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/MyMinimon - Open (2001)", false, 500)]
    static void OpenMyMinimon2001()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(2001);
    }

    static void ReopenMyMinimon(int dataId)
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BestiaryTest] PlayMode 진입 후 실행"); return; }
        var p = Object.FindObjectOfType<UI_MinionInfoPopup>();
        if (p != null) p.SetInfo(dataId);
        else Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(dataId);
    }

    [MenuItem("Test/Bestiary/MyMinimon - Seed 2001 능력치 (값+등급)", false, 501)]
    static void SeedMM2001()
    {
        UI_BestiaryPopup.SetMMValue(2001, UI_BestiaryPopup.MMStat.Atk, 55);
        UI_BestiaryPopup.SetMMValue(2001, UI_BestiaryPopup.MMStat.AtkSpeed, 35);
        UI_BestiaryPopup.SetMMValue(2001, UI_BestiaryPopup.MMStat.Crit, 12);
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.Atk, 3);
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.AtkSpeed, 2);
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.Crit, 1);
        Debug.Log("[BestiaryTest] 2001 ATK=55(3등급)/AS=35(2등급)/CRIT=12(1등급)");
        ReopenMyMinimon(2001);
    }

    [MenuItem("Test/Bestiary/MyMinimon - Cycle 2001 ATK 등급 (0→5)", false, 502)]
    static void CycleATKGrade()
    {
        int cur = UI_BestiaryPopup.GetMMGrade(2001, UI_BestiaryPopup.MMStat.Atk);
        int next = (cur + 1) % 6;
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.Atk, next);
        Debug.Log($"[BestiaryTest] 2001 ATK 등급 {cur}→{next}");
        ReopenMyMinimon(2001);
    }

    [MenuItem("Test/Bestiary/MyMinimon - All 5등급 (S 모두 → AllSS 자동)", false, 503)]
    static void AllGrade5()
    {
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.Atk, 5);
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.AtkSpeed, 5);
        UI_BestiaryPopup.SetMMGrade(2001, UI_BestiaryPopup.MMStat.Crit, 5);
        Debug.Log($"[BestiaryTest] 2001 모든 등급 S(5). AllSS={UI_BestiaryPopup.IsAllSS(2001)} 별={UI_BestiaryPopup.GetStarGrade(2001)}");
        ReopenMyMinimon(2001);
    }

    [MenuItem("Test/Bestiary/MyMinimon - Toggle 동행 (2001)", false, 504)]
    static void ToggleFollow2001()
    {
        int cur = UI_BestiaryPopup.GetFollowingId();
        UI_BestiaryPopup.SetFollowing(cur == 2001 ? 0 : 2001);
        Debug.Log($"[BestiaryTest] follower {cur}→{UI_BestiaryPopup.GetFollowingId()}");
        ReopenMyMinimon(2001);
    }

    // ─────────────────────────────────────────────
    // 미니몬 잡기 (슬라이드 28~37, UI_TamePopup) — Phase 1
    // ─────────────────────────────────────────────
    [MenuItem("Test/Bestiary/Tame - Open Tame Popup (2001)", false, 520)]
    static void OpenTamePopup2001()
    {
        if (!Application.isPlaying) { EditorApplication.isPlaying = true; return; }
        Managers.UI.ShowPopupUI<UI_TamePopup>().SetInfo(2001);
    }
}
#endif
