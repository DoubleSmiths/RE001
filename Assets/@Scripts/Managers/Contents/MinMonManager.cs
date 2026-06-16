using System;
using System.Collections.Generic;
using UnityEngine;

public class MinMonManager
{
    public event Action<int> OnCompanionChanged;
    public event Action<int> OnMinMonAdded;
    public event Action      OnInterestChanged;   // 슬23: 포획 가능 창고 변동
    public event Action<int> OnMaxStarReached;     // 슬2: 몬스터가 보라별(3단계) 최초 달성 (완성도 등급 상승 트리거)

    // ── 미니몬 관심 / 포획 가능 창고 (슬23~27) ──
    public const int INTEREST_CAPACITY = 2;       // 창고 기본 슬롯 수 (슬23)
    // 슬23-4: 실제 창고 칸 수 = 기본 + 능력(StatType "InterestCapacity") 보너스. 능력 데이터는 후속이라 현재 0.
    public int InterestCapacity => INTEREST_CAPACITY + (Managers.Ability != null ? Managers.Ability.InterestCapacityBonus : 0);
    public enum InterestResult { NotInterested, Added, StorageFull }

    public List<int> InterestedMinMons => Managers.Game.InterestedMinMons;
    public bool HasInterest     => InterestedMinMons.Count > 0;
    public bool HasInterestRoom => InterestedMinMons.Count < InterestCapacity;
    public bool IsInterested(int dataId) => InterestedMinMons.Contains(dataId);

    // 사냥 성공 시 호출: InterestRate 굴려 관심 판정 → 창고 저장 (순서대로). 슬23~25.
    public InterestResult RollInterest(int dataId)
    {
        if (!Managers.Data.CreatureDic.TryGetValue(dataId, out var cd))
            return InterestResult.NotInterested;
        if (UnityEngine.Random.value > cd.InterestRate)
            return InterestResult.NotInterested;
        return AddInterest(dataId);
    }

    // 관심 판정 없이 창고에 직접 추가 (디버그/확정 관심용)
    public InterestResult AddInterest(int dataId)
    {
        if (InterestedMinMons.Contains(dataId))
            return InterestResult.Added;            // 이미 창고에 있음
        if (!HasInterestRoom)
            return InterestResult.StorageFull;      // 슬25: 창고 가득 → 저장 안 됨
        InterestedMinMons.Add(dataId);
        Managers.Game.SaveGame();
        OnInterestChanged?.Invoke();
        return InterestResult.Added;
    }

    public void RemoveInterest(int dataId)
    {
        if (InterestedMinMons.Remove(dataId))
        {
            Managers.Game.SaveGame();
            OnInterestChanged?.Invoke();
        }
    }

    // ── 초기화 ──
    public void Init() { }

    // ── 포획 ──
    public bool TryCatch(int dataId, bool forceSuccess = false)
    {
        if (!Managers.Data.MinMonDic.TryGetValue(dataId, out var data))
            return false;

        MinMonSaveData save = GetSaveData(dataId);

        // 천장: TameCount가 MaxTameCount 이상이면 무조건 성공
        bool success = forceSuccess
            || save.TameCount >= data.MaxTameCount
            || UnityEngine.Random.value < data.CatchChance;

        if (success)
        {
            AddMinMon(dataId);
            // 도감 등록
            if (!Managers.Game.DiscoveredMonsterIds.Contains(dataId))
                Managers.Game.DiscoveredMonsterIds.Add(dataId);
        }
        else
        {
            // 실패해도 TameCount 증가
            save.TameCount++;
            Managers.Game.SaveGame();
        }

        return success;
    }

    // 마지막 포획 결과 (UI 연출용 — 슬20 첫포획 / 슬21 재포획 향상 / 슬22 향상없음)
    public bool LastCatchWasFirst { get; private set; }
    public bool LastCatchImproved { get; private set; }
    // 슬21-e: 실제로 오른 능력치만 Anim 재생하도록 스탯별 향상 여부 보관
    public bool LastAtkImproved  { get; private set; }
    public bool LastASImproved   { get; private set; }
    public bool LastCritImproved { get; private set; }
    // 슬21-d: Anim 중 "포획 전 등급 → 새 등급" 전환 연출용 이전 등급
    public int LastAtkGradePrev  { get; private set; }
    public int LastASGradePrev   { get; private set; }
    public int LastCritGradePrev { get; private set; }

    public void AddMinMon(int dataId)
    {
        MinMonSaveData save = GetSaveData(dataId);

        // 슬20/21: 능력치 등급 결정. 첫 포획은 새로 롤, 재포획은 굴려서 더 높으면 갱신.
        LastCatchWasFirst = save.CaughtCount == 0;
        LastCatchImproved = false;
        LastAtkImproved = LastASImproved = LastCritImproved = false;
        if (LastCatchWasFirst)
        {
            save.AtkGrade      = RollGrade();
            save.AtkSpeedGrade = RollGrade();
            save.CritGrade     = RollGrade();
        }
        else
        {
            int a = save.AtkGrade, s = save.AtkSpeedGrade, c = save.CritGrade;
            LastAtkGradePrev = a; LastASGradePrev = s; LastCritGradePrev = c;
            save.AtkGrade      = Mathf.Max(save.AtkGrade,      RollGrade());
            save.AtkSpeedGrade = Mathf.Max(save.AtkSpeedGrade, RollGrade());
            save.CritGrade     = Mathf.Max(save.CritGrade,     RollGrade());
            LastAtkImproved    = save.AtkGrade      > a;
            LastASImproved     = save.AtkSpeedGrade > s;
            LastCritImproved   = save.CritGrade     > c;
            LastCatchImproved  = LastAtkImproved || LastASImproved || LastCritImproved;
        }

        save.CaughtCount++;
        save.TameCount++;
        // 포획 성공 시 포획 가능 창고에서 제거 (슬23: 창고의 미니몬을 잡은 것)
        Managers.Game.InterestedMinMons.Remove(dataId);
        Managers.Game.SaveGame();

        // 슬10/11/79: 공명도 100%(4회 포획) 시 고유기술을 능력으로 영구 습득.
        //  Lv.1 = 공명도 100% / Lv.2 = 추가로 모든 능력치 등급이 S(완벽획득, 슬11).
        if (GetSyncRate(dataId) >= 1f)
        {
            int lv = 1 + (IsAllSS(dataId) ? 1 : 0);
            Managers.Ability.LearnMinionUniqueSkill(dataId, lv);
        }

        OnMinMonAdded?.Invoke(dataId);
        OnInterestChanged?.Invoke();
        CheckMaxStar(dataId);   // 슬2: 포획으로 공명도/등급 상승 → 보라별 달성 시 완성도 트리거
    }

    // 슬20: 능력치 등급 확률 (0:40% 1:30% 2:15% 3:8% 4:5% 5:2%)
    static int RollGrade()
    {
        int r = UnityEngine.Random.Range(0, 100);
        if (r < 40) return 0;
        if (r < 70) return 1;
        if (r < 85) return 2;
        if (r < 93) return 3;
        if (r < 98) return 4;
        return 5;
    }

    // ── 동행 설정 ──
    public void SetCompanion(int dataId)
    {
        // 이전 동행 해제
        int prev = Managers.Game.CompanionMinMonId;
        if (prev != -1)
        {
            MinMonSaveData prevSave = GetSaveData(prev);
            if (prevSave != null)
                prevSave.IsCompanion = false;
        }

        MinMonSaveData save = GetSaveData(dataId);
        if (save != null)
            save.IsCompanion = true;

        Managers.Game.CompanionMinMonId = dataId;
        OnCompanionChanged?.Invoke(dataId);
    }

    public void RemoveCompanion()
    {
        int prev = Managers.Game.CompanionMinMonId;
        if (prev != -1)
        {
            MinMonSaveData prevSave = GetSaveData(prev);
            if (prevSave != null)
                prevSave.IsCompanion = false;
        }
        Managers.Game.CompanionMinMonId = -1;
        OnCompanionChanged?.Invoke(-1);
        Managers.Game.SaveGame();
    }

    // ── 스탯 계산 ──
    // 슬라이드 10: 공명도 = 포획 성공 1회당 +25% (4회에 100%). 천장(TameCount)과 무관.
    public const float SYNC_PER_CATCH = 0.25f;
    public float GetSyncRate(int dataId)
    {
        var save = FindSave(dataId);
        if (save == null) return 0f;
        return Mathf.Clamp01(save.CaughtCount * SYNC_PER_CATCH);
    }

    // 스탯 = BaseStat * (1 + SyncRate)
    public float GetAtk(int dataId)
    {
        if (!Managers.Data.MinMonDic.TryGetValue(dataId, out var data)) return 0f;
        return data.BaseAtk * (1f + GetSyncRate(dataId));
    }

    public float GetAtkSpeed(int dataId)
    {
        if (!Managers.Data.MinMonDic.TryGetValue(dataId, out var data)) return 0f;
        return data.BaseAtkSpeed * (1f + GetSyncRate(dataId));
    }

    public float GetCritRate(int dataId)
    {
        if (!Managers.Data.MinMonDic.TryGetValue(dataId, out var data)) return 0f;
        return data.BaseCritRate * (1f + GetSyncRate(dataId));
    }

    // ── 능력치 등급 조회 (0~5 = E~S). 미포획/미롤은 0. 엔트리 생성 부작용 없는 읽기 전용. ──
    MinMonSaveData FindSave(int dataId) => Managers.Game.OwnedMinMons.Find(m => m.DataId == dataId);

    public int GetAtkGrade(int dataId)      { var s = FindSave(dataId); return s != null ? Mathf.Max(0, s.AtkGrade)      : 0; }
    public int GetAtkSpeedGrade(int dataId) { var s = FindSave(dataId); return s != null ? Mathf.Max(0, s.AtkSpeedGrade) : 0; }
    public int GetCritGrade(int dataId)     { var s = FindSave(dataId); return s != null ? Mathf.Max(0, s.CritGrade)     : 0; }
    public const int GRADE_MAX = 5; // S등급

    // ── 기록도(분석률) / 별등급 / AllSS / 고유기술 학습 (슬9/79/80/83) ──
    // 기록도 = KillCount / MaxKillCount (0~100%). 사냥 성공마다 AddKill, 최대치에서 정지.
    public void AddKill(int dataId)
    {
        if (!Managers.Data.CreatureDic.TryGetValue(dataId, out var cd)) return;
        var save = GetSaveData(dataId);
        int max = Mathf.Max(1, cd.MaxKillCount);
        save.KillCount = Mathf.Min(save.KillCount + 1, max);
        Managers.Game.SaveGame();
        CheckMaxStar(dataId);   // 슬2: 사냥으로 기록도 상승 → 보라별 달성 시 완성도 트리거
    }

    // 슬2: 몬스터의 별 단계가 보라별(=GetStarGrade 최대)에 처음 도달하면 1회만 완성도 트리거.
    //  (도감 열림/닫힘 처리는 구독자 UI에서. 여기선 게임플레이 진척 감지 + 중복 방지만)
    public const int STAR_MAX = 3;
    void CheckMaxStar(int dataId)
    {
        if (GetStarGrade(dataId) < STAR_MAX) return;
        var granted = Managers.Game.MaxStarGranted;
        if (granted.Contains(dataId)) return;
        granted.Add(dataId);
        Managers.Game.SaveGame();
        OnMaxStarReached?.Invoke(dataId);
    }

    public int GetKillCount(int dataId) { var s = FindSave(dataId); return s != null ? s.KillCount : 0; }
    public int GetCaughtCount(int dataId) { var s = FindSave(dataId); return s != null ? s.CaughtCount : 0; }

    public int GetRecordRate(int dataId)
    {
        if (!Managers.Data.CreatureDic.TryGetValue(dataId, out var cd)) return 0;
        int max = Mathf.Max(1, cd.MaxKillCount);
        return Mathf.Clamp(Mathf.RoundToInt(GetKillCount(dataId) * 100f / max), 0, 100);
    }

    // 슬80: 잡은 미니몬의 3종 능력치 등급이 모두 S(=GRADE_MAX)
    public bool IsAllSS(int dataId)
    {
        if (!IsOwned(dataId)) return false;
        return GetAtkGrade(dataId) >= GRADE_MAX
            && GetAtkSpeedGrade(dataId) >= GRADE_MAX
            && GetCritGrade(dataId) >= GRADE_MAX;
    }

    // 슬79: 공명도 100%(= 4회 포획) 달성 시 고유기술 영구 학습. 공명도는 감소 없으므로 곧 학습 상태.
    public bool IsUniqueSkillLearned(int dataId) => GetSyncRate(dataId) >= 1f;

    // 슬83: 별 단계(0~3) = ① 기록도 100% ② 공명도 100% ③ 모든 등급 S — 만족 개수
    public int GetStarGrade(int dataId)
    {
        int star = 0;
        if (GetRecordRate(dataId) >= 100) star++;
        if (GetSyncRate(dataId) >= 1f)    star++;
        if (IsAllSS(dataId))              star++;
        return star;
    }

    // ── 디버그/시드 세터 (UI_BestiaryPopup의 테스트용 Set* 스텁이 경유 호출) ──
    public void DebugSetKillCount(int dataId, int killCount)
    {
        GetSaveData(dataId).KillCount = Mathf.Max(0, killCount);
        Managers.Game.SaveGame();
    }
    public void DebugSetCaughtCount(int dataId, int caughtCount)
    {
        GetSaveData(dataId).CaughtCount = Mathf.Max(0, caughtCount);
        Managers.Game.SaveGame();
    }
    // stat: 0=Atk 1=AtkSpeed 2=Crit
    public void DebugSetGrade(int dataId, int stat, int grade)
    {
        var s = GetSaveData(dataId);
        grade = Mathf.Clamp(grade, 0, GRADE_MAX);
        if (stat == 0) s.AtkGrade = grade;
        else if (stat == 1) s.AtkSpeedGrade = grade;
        else s.CritGrade = grade;
        Managers.Game.SaveGame();
    }

    // ── 조회 ──
    public MinMonSaveData GetSaveData(int dataId)
    {
        var save = Managers.Game.OwnedMinMons.Find(m => m.DataId == dataId);
        if (save == null)
        {
            save = new MinMonSaveData { DataId = dataId };
            Managers.Game.OwnedMinMons.Add(save);
        }
        return save;
    }

    public bool IsOwned(int dataId)
    {
        // 실제 포획 성공(CaughtCount) 기준. TameCount는 실패 시도도 포함하므로 보유 판정에 부적합.
        return Managers.Game.OwnedMinMons.Exists(m => m.DataId == dataId && m.CaughtCount > 0);
    }

    public bool IsDiscovered(int dataId)
    {
        return Managers.Game.DiscoveredMonsterIds.Contains(dataId);
    }
}
