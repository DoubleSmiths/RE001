#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Data;
using UnityEditor;
using UnityEngine;

// 기획서 v3 기반 통합 검증 메뉴 — PlayMode 중 Unity 메뉴 [Test/Integration] 에서 호출.
// 기능별 BattleTestMenu / BestiaryTestMenu / PopupTestMenu 외에 능력(Ability) /
// 미니몬 포획 천장 / 관심창고 / 데이터 정합성을 한 번에 검증.
//
// 사용법:
//   1) Unity 에디터에서 PlayMode 진입 (▶)
//   2) 상단 메뉴 [Test/Integration] 에서 항목 선택
//   3) Console에 결과 로그 출력
//
// 가장 중요한 두 항목:
//   - "Diagnose / 전체 진단" : 기획서 vs 현재 코드 정합성 종합 보고
//   - "Diagnose / 데이터 무결성" : JSON 데이터 누락/매핑 오류 체크
public static class IntegrationTestMenu
{
    // ─────────────────────────────────────────────
    // 0. 사전 체크
    // ─────────────────────────────────────────────
    static bool RequirePlayMode()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Integration] PlayMode 진입 후 실행하세요.");
            return false;
        }
        if (Managers.Game == null)
        {
            Debug.LogWarning("[Integration] Managers 아직 준비 안 됨.");
            return false;
        }
        return true;
    }

    // ─────────────────────────────────────────────
    // 1. 전체 진단 (기획서 ↔ 코드 정합성)
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Diagnose - 전체 진단 %#d", false, 1)]
    static void DiagnoseAll()
    {
        if (!RequirePlayMode()) return;

        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════════════");
        sb.AppendLine("  RE001 기획서 ↔ 코드 통합 진단");
        sb.AppendLine("══════════════════════════════════════════");

        // 매니저
        sb.AppendLine("[Managers]");
        sb.AppendLine($"  Data    = {(Managers.Data    != null ? "OK" : "NULL")}");
        sb.AppendLine($"  Game    = {(Managers.Game    != null ? "OK" : "NULL")}");
        sb.AppendLine($"  Object  = {(Managers.Object  != null ? "OK" : "NULL")}");
        sb.AppendLine($"  UI      = {(Managers.UI      != null ? "OK" : "NULL")}");
        sb.AppendLine($"  MinMon  = {(Managers.MinMon  != null ? "OK" : "NULL")}");
        sb.AppendLine($"  Ability = {(Managers.Ability != null ? "OK" : "NULL")}");
        sb.AppendLine($"  Quest   = {(Managers.Quest   != null ? "OK" : "NULL")}");
        sb.AppendLine($"  Combo   = {(Managers.Combo   != null ? "OK" : "NULL")}");

        // 데이터 사전
        sb.AppendLine("\n[Data Dictionaries]");
        sb.AppendLine($"  CreatureDic        = {Managers.Data.CreatureDic.Count}");
        sb.AppendLine($"  SkillDic           = {Managers.Data.SkillDic.Count}");
        sb.AppendLine($"  MinMonDic          = {Managers.Data.MinMonDic.Count}");
        sb.AppendLine($"  HPPotionDic        = {Managers.Data.HPPotionDic.Count}");
        sb.AppendLine($"  SpeedPotionDic     = {Managers.Data.SpeedPotionDic.Count}");
        sb.AppendLine($"  ComboStageDic      = {Managers.Data.ComboStageDic.Count}");
        sb.AppendLine($"  PlayerStatusDic    = {Managers.Data.PlayerStatusDic.Count}");
        sb.AppendLine($"  QuestDic           = {Managers.Data.QuestDic.Count}");
        sb.AppendLine($"  SubQuestDic        = {Managers.Data.SubQuestDic.Count}");
        sb.AppendLine($"  TravelNodeDic      = {Managers.Data.TravelNodeDic.Count}");
        sb.AppendLine($"  LocalizationDic    = {Managers.Data.LocalizationDic.Count}");
        sb.AppendLine($"  EquipmentDic       = {Managers.Data.EquipmentDic.Count}  ← (주석 처리됨)");
        sb.AppendLine($"  LevelDic           = {Managers.Data.LevelDic.Count}      ← (주석 처리됨)");
        sb.AppendLine($"  ForgeRecipeDic     = {Managers.Data.ForgeRecipeDic.Count} ← (주석 처리됨)");
        sb.AppendLine($"  CraftRecipeDic     = {Managers.Data.CraftRecipeDic.Count} ← (주석 처리됨)");
        sb.AppendLine($"  AreaDic            = {Managers.Data.AreaDic.Count}        ← (주석 처리됨, TravelNode로 대체)");

        // 플레이어 상태
        sb.AppendLine("\n[Player State]");
        sb.AppendLine($"  Lv.{Managers.Game.Level}  Exp={Managers.Game.Exp}  Rank={Managers.Game.AdventurerRank}");
        sb.AppendLine($"  Gold={Managers.Game.Gold}  Crystal={Managers.Game.Crystal}");
        sb.AppendLine($"  StatusPoint={Managers.Game.StatusPoint}  Elixir={Managers.Game.Elixir}");
        if (Managers.Game.Player != null)
        {
            var p = Managers.Game.Player;
            sb.AppendLine($"  HP {p.Hp}/{p.MaxHp}  Atk={p.PhysicalAtk}  Crit={p.CritRate}%  AtkSpd={p.AtkSpeed}%");
        }
        else
        {
            sb.AppendLine("  Player not spawned yet (Battle 진입 전)");
        }

        // 능력 시스템 (슬38)
        sb.AppendLine("\n[Ability — 슬38]");
        sb.AppendLine($"  Memory: {Managers.Ability.UsedMemory}/{Managers.Ability.MaxMemory}  (free={Managers.Ability.FreeMemory})");
        sb.AppendLine($"  Learned ({Managers.Game.LearnedAbilities.Count}): {string.Join(",", Managers.Game.LearnedAbilities)}");
        sb.AppendLine($"  Equipped({Managers.Game.EquippedAbilities.Count}): {string.Join(",", Managers.Game.EquippedAbilities)}");

        // 미니몬 (슬9~37)
        sb.AppendLine("\n[Minimon — 슬9~37]");
        sb.AppendLine($"  Owned({Managers.Game.OwnedMinMons.Count}):");
        foreach (var m in Managers.Game.OwnedMinMons)
        {
            sb.AppendLine($"    {m.DataId}: Kill={m.KillCount} Caught={m.CaughtCount} " +
                $"Sync={(int)(Managers.MinMon.GetSyncRate(m.DataId) * 100)}% " +
                $"Star={Managers.MinMon.GetStarGrade(m.DataId)} " +
                $"Grade(Atk/AS/Crit)={m.AtkGrade}/{m.AtkSpeedGrade}/{m.CritGrade}");
        }
        sb.AppendLine($"  Companion = {Managers.Game.CompanionMinMonId}");
        sb.AppendLine($"  Interest ({Managers.Game.InterestedMinMons.Count}/{MinMonManager.INTEREST_CAPACITY}): " +
                      $"{string.Join(",", Managers.Game.InterestedMinMons)}");

        // 퀘스트
        sb.AppendLine("\n[Quest]");
        sb.AppendLine($"  CurrentQuestID    = {Managers.Game.CurrentQuestID}");
        sb.AppendLine($"  CurrentSubQuestID = {Managers.Game.CurrentSubQuestID}");
        sb.AppendLine($"  UnlockedBtnIds({Managers.Game.UnlockedBtnIds.Count})");

        // 잠금 상태
        sb.AppendLine("\n[Btm Tab Unlock]");
        foreach (Define.TabType tab in System.Enum.GetValues(typeof(Define.TabType)))
            sb.AppendLine($"  {tab,-12} = {(Managers.Game.IsBtmButtonUnlocked(tab) ? "OPEN" : "LOCK")}");

        // 콤보
        sb.AppendLine("\n[Combo]");
        sb.AppendLine($"  Combo={Managers.Combo.Combo}  Rank={Managers.Combo.NowRank}  Timer={Managers.Combo.CurrentTimer:F2}");
        sb.AppendLine($"  AtkSpdBuff={Managers.Combo.AtkSpeedBonus}%  DmgBoost={Managers.Combo.DmgBoostBonus}%");

        sb.AppendLine("\n══════════════════════════════════════════");
        sb.AppendLine("  남은 작업(기획서 기준):");
        sb.AppendLine("    1) 장비 시스템 — EquipmentDic/EquipEnhanceDic/ForgeRecipeDic 로드 활성화 + 강화 UI(T1→T3) + 대장간 연출");
        sb.AppendLine("    2) LevelData JSON 로드(現 fallback 수식)");
        sb.AppendLine("    3) CharacterPopup 스탯 분배/엘릭서 흐름 확인");
        sb.AppendLine("    4) AreaData 폐기 여부 결정 (TravelNode로 대체된 듯)");
        sb.AppendLine("    5) SoundManager.Init() 활성화");
        sb.AppendLine("══════════════════════════════════════════");

        Debug.Log(sb.ToString());
    }

    // ─────────────────────────────────────────────
    // 2. 데이터 무결성 검사
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Diagnose - 데이터 무결성", false, 2)]
    static void DiagnoseDataIntegrity()
    {
        if (!RequirePlayMode()) return;

        var sb = new StringBuilder();
        sb.AppendLine("══════ 데이터 무결성 검사 ══════");

        int errors = 0;

        // 1) 미니몬 → 몬스터 매핑
        sb.AppendLine("\n[MinMon ↔ CreatureData]");
        foreach (var mm in Managers.Data.MinMonDic.Values)
        {
            if (!Managers.Data.CreatureDic.ContainsKey(mm.MonsterDataId))
            {
                sb.AppendLine($"  ❌ MinMon {mm.DataId}({mm.Name}) → MonsterDataId={mm.MonsterDataId} 누락");
                errors++;
            }
        }

        // 2) 미니몬 고유기술 능력 ID 매핑 (creatureId + 4000)
        sb.AppendLine("\n[Unique Skill ↔ SkillData]");
        foreach (var mm in Managers.Data.MinMonDic.Values)
        {
            int abilityId = Managers.Ability.MinionSkillId(mm.MonsterDataId);
            if (!Managers.Data.SkillDic.ContainsKey(abilityId))
            {
                sb.AppendLine($"  ⚠ MinMon {mm.DataId}({mm.Name}) 고유기술 SkillDic[{abilityId}] 없음 — 공명도 100% 도달해도 학습 불가");
                errors++;
            }
            else
            {
                sb.AppendLine($"  ✓ {mm.Name} → Ability {abilityId} ({Managers.Data.SkillDic[abilityId].Name})");
            }
        }

        // 3) 도감 4스킬 (7000, 7001, 7002, 5001)
        sb.AppendLine("\n[Bestiary Skill ↔ SkillData]");
        int[] bestiarySkills = { 7000, 7001, 7002, 5001 };
        foreach (int id in bestiarySkills)
        {
            if (!Managers.Data.SkillDic.ContainsKey(id))
            {
                sb.AppendLine($"  ❌ Bestiary Skill {id} 누락");
                errors++;
            }
            else
            {
                sb.AppendLine($"  ✓ {id} = {Managers.Data.SkillDic[id].Name}");
            }
        }

        // 4) PlayerStatusData (Level=1 시작 스탯)
        sb.AppendLine("\n[PlayerStatusData Lv.1]");
        if (Managers.Data.PlayerStatusDic.TryGetValue(1, out var ps))
            sb.AppendLine($"  ✓ Lv1 HP={ps.StartHP} Str={ps.StartStrength} Crit={ps.StartCritRate}");
        else
        {
            sb.AppendLine("  ❌ PlayerStatusData[1] 누락 — fallback 수식 사용");
            errors++;
        }

        // 5) 콤보 단계
        sb.AppendLine("\n[ComboStage]");
        if (Managers.Data.ComboStageDic.Count == 0)
        {
            sb.AppendLine("  ❌ ComboData.json 로드 실패");
            errors++;
        }
        else
        {
            foreach (var kv in Managers.Data.ComboStageDic)
                sb.AppendLine($"  ✓ Lv{kv.Key}: Hits>={kv.Value.HitsRequired} AS+{kv.Value.AtkSpeedBuff}% Dmg+{kv.Value.DmgBoostBuff}%");
        }

        // 6) 포션 체인 (HP/Speed)
        sb.AppendLine("\n[Potion Chain]");
        sb.AppendLine($"  HPPotion 시작 = {Managers.Game.CurrentHPPotionDataId}");
        sb.AppendLine($"  Speed 시작     = {Managers.Game.CurrentSpeedPotionDataId}");

        // 결과
        sb.AppendLine();
        sb.AppendLine(errors == 0 ? "✅ 데이터 무결성 OK" : $"❌ 무결성 오류 {errors}건");
        Debug.Log(sb.ToString());
    }

    // ─────────────────────────────────────────────
    // 3. 능력 시스템 검증 (슬38)
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Ability - 모든 능력 학습 시드", false, 100)]
    static void SeedAllAbilities()
    {
        if (!RequirePlayMode()) return;
        foreach (int id in Managers.Data.SkillDic.Keys)
            Managers.Ability.Learn(id);
        Debug.Log($"[Ability] 전체 학습. Learned={Managers.Game.LearnedAbilities.Count} Equipped={Managers.Game.EquippedAbilities.Count} Memory={Managers.Ability.UsedMemory}/{Managers.Ability.MaxMemory}");
    }

    [MenuItem("Test/Integration/Ability - 모든 능력 망각", false, 101)]
    static void ClearAllAbilities()
    {
        if (!RequirePlayMode()) return;
        var ids = new List<int>(Managers.Game.LearnedAbilities);
        foreach (int id in ids)
            Managers.Ability.Unlearn(id);
        Debug.Log($"[Ability] 전체 망각. Memory={Managers.Ability.UsedMemory}/{Managers.Ability.MaxMemory}");
    }

    [MenuItem("Test/Integration/Ability - 힘의 기억(5002) 기억/망각 토글", false, 102)]
    static void ToggleStrength()
    {
        if (!RequirePlayMode()) return;
        const int id = 5002;
        Managers.Ability.Learn(id);   // 보유 보장
        if (Managers.Ability.IsEquipped(id)) Managers.Ability.Forget(id);
        else Managers.Ability.Memorize(id);
        Managers.Game.Player?.RefreshStats();
        var p = Managers.Game.Player;
        Debug.Log($"[Ability] 5002 equipped={Managers.Ability.IsEquipped(id)}  PlayerAtk={p?.PhysicalAtk}  Memory={Managers.Ability.UsedMemory}/{Managers.Ability.MaxMemory}");
    }

    [MenuItem("Test/Integration/Ability - 기억확장(5001) +1 Lv", false, 103)]
    static void LevelUpMemoryExpansion()
    {
        if (!RequirePlayMode()) return;
        const int id = 5001;
        Managers.Ability.Learn(id);
        int next = Managers.Ability.GetLevel(id) + 1;
        Managers.Ability.SetLevel(id, next);
        Debug.Log($"[Ability] 5001 기억확장 Lv.{Managers.Ability.GetLevel(id)} → MaxMemory={Managers.Ability.MaxMemory}");
    }

    // ─────────────────────────────────────────────
    // 4. 미니몬 — 천장 4회 시뮬, 학습, AllSS
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Minimon - 2001 4회 강제 포획 → 고유기술 학습", false, 200)]
    static void Force4Catch2001()
    {
        if (!RequirePlayMode()) return;
        const int id = 2001;
        for (int i = 0; i < 4; i++)
            Managers.MinMon.TryCatch(id, forceSuccess: true);
        var save = Managers.MinMon.GetSaveData(id);
        Debug.Log($"[MinMon] 2001 Caught={save.CaughtCount} Sync={(int)(Managers.MinMon.GetSyncRate(id)*100)}% " +
                  $"Learned(6001)={Managers.Ability.IsLearned(6001)} Lv.{Managers.Ability.GetLevel(6001)}");
    }

    [MenuItem("Test/Integration/Minimon - 2001 모든 등급 S(AllSS) → Lv.2 학습", false, 201)]
    static void Force2001AllSS()
    {
        if (!RequirePlayMode()) return;
        const int id = 2001;
        var save = Managers.MinMon.GetSaveData(id);
        save.AtkGrade = save.AtkSpeedGrade = save.CritGrade = MinMonManager.GRADE_MAX;
        if (Managers.MinMon.GetSyncRate(id) < 1f)
        {
            for (int i = 0; i < 4; i++) Managers.MinMon.TryCatch(id, true);
        }
        else
        {
            // 이미 sync 100%면 한번 더 잡아 LearnMinionUniqueSkill을 재호출(Lv 갱신)
            Managers.MinMon.TryCatch(id, true);
        }
        Debug.Log($"[MinMon] 2001 AllSS={Managers.MinMon.IsAllSS(id)}  Ability 6001 Lv.{Managers.Ability.GetLevel(6001)}");
    }

    [MenuItem("Test/Integration/Minimon - Companion 2001 토글", false, 202)]
    static void ToggleCompanion()
    {
        if (!RequirePlayMode()) return;
        const int id = 2001;
        if (!Managers.MinMon.IsOwned(id))
        {
            Debug.LogWarning("[MinMon] 2001 미보유 — 먼저 포획하세요");
            return;
        }
        if (Managers.Game.CompanionMinMonId == id) Managers.MinMon.RemoveCompanion();
        else Managers.MinMon.SetCompanion(id);
        Debug.Log($"[MinMon] Companion = {Managers.Game.CompanionMinMonId}");
    }

    [MenuItem("Test/Integration/Minimon - 모든 미니몬 데이터 리셋", false, 209)]
    static void ResetAllMinMon()
    {
        if (!RequirePlayMode()) return;
        Managers.Game.OwnedMinMons.Clear();
        Managers.Game.InterestedMinMons.Clear();
        Managers.Game.DiscoveredMonsterIds.Clear();
        Managers.Game.CompanionMinMonId = -1;
        Managers.Game.SaveGame();
        Debug.Log("[MinMon] 모든 미니몬 데이터 초기화");
    }

    // ─────────────────────────────────────────────
    // 5. 콤보 시뮬레이션
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Combo - 10타 SimulateHit (Lv 상승)", false, 300)]
    static void SimulateCombo10()
    {
        if (!RequirePlayMode()) return;
        for (int i = 0; i < 10; i++)
            Managers.Combo.SimulateHit(50f);
        Debug.Log($"[Combo] {Managers.Combo.Combo}타 / Rank {Managers.Combo.NowRank} / Timer {Managers.Combo.CurrentTimer:F2}");
    }

    [MenuItem("Test/Integration/Combo - 50타 SimulateHit (최고 Rank)", false, 301)]
    static void SimulateCombo50()
    {
        if (!RequirePlayMode()) return;
        for (int i = 0; i < 50; i++)
            Managers.Combo.SimulateHit(100f);
        Debug.Log($"[Combo] {Managers.Combo.Combo}타 / Rank {Managers.Combo.NowRank} / AS+{Managers.Combo.AtkSpeedBonus}% Dmg+{Managers.Combo.DmgBoostBonus}%");
    }

    // ─────────────────────────────────────────────
    // 6. 재화/스탯 빠른 세팅
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Quick - Gold/Crystal +10000", false, 400)]
    static void AddCurrency()
    {
        if (!RequirePlayMode()) return;
        Managers.Game.Gold    += 10000;
        Managers.Game.Crystal += 1000;
        Managers.Game.StatusPoint += 5;
        Debug.Log($"[Quick] Gold={Managers.Game.Gold}  Crystal={Managers.Game.Crystal}  StatusPoint={Managers.Game.StatusPoint}");
    }

    [MenuItem("Test/Integration/Quick - 모든 BtmTab 해금", false, 401)]
    static void UnlockAllTabs()
    {
        if (!RequirePlayMode()) return;
        foreach (Define.TabType tab in System.Enum.GetValues(typeof(Define.TabType)))
            Managers.Game.UnlockBtmButton(tab);
        var scene = Managers.UI?.SceneUI as UI_GameScene;
        scene?.TestUnlockAll();
        Debug.Log("[Quick] 모든 하단 탭 + 마을 이동/연금공방 버튼 해금");
    }

    [MenuItem("Test/Integration/Quick - 핵심 재료 아이템 지급", false, 402)]
    static void GiveStarterItems()
    {
        if (!RequirePlayMode()) return;
        Managers.Game.AddItem(10001, 10);
        Managers.Game.AddItem(10002, 10);
        Managers.Game.AddItem(20001, 5);
        Debug.Log("[Quick] 일반/중요 재료 지급");
    }

    // ─────────────────────────────────────────────
    // 7. 세이브 / 리셋
    // ─────────────────────────────────────────────
    [MenuItem("Test/Integration/Save - 즉시 저장", false, 500)]
    static void SaveNow()
    {
        if (!RequirePlayMode()) return;
        Managers.Game.SaveGame();
        Debug.Log($"[Save] {Application.persistentDataPath}/SaveData.json 저장 완료");
    }

    [MenuItem("Test/Integration/Save - 세이브 데이터 삭제 (재시작 필요)", false, 501)]
    static void DeleteSave()
    {
        string path = Application.persistentDataPath + "/SaveData.json";
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            PlayerPrefs.SetInt("ISFIRST", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Save] 삭제 완료: {path}  — Play 다시 누르면 첫 실행 상태");
        }
        else Debug.Log("[Save] 파일 없음");
    }

    [MenuItem("Test/Integration/Save - 세이브 위치 열기", false, 502)]
    static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
#endif
