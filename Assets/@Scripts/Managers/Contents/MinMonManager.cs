using System;
using System.Collections.Generic;
using UnityEngine;

public class MinMonManager
{
    public event Action<int> OnCompanionChanged;
    public event Action<int> OnMinMonAdded;
    public event Action      OnInterestChanged;   // 슬23: 포획 가능 창고 변동

    // ── 미니몬 관심 / 포획 가능 창고 (슬23~27) ──
    public const int INTEREST_CAPACITY = 2;       // 창고 슬롯 수 (기술로 증가 가능 — 후속)
    public enum InterestResult { NotInterested, Added, StorageFull }

    public List<int> InterestedMinMons => Managers.Game.InterestedMinMons;
    public bool HasInterest     => InterestedMinMons.Count > 0;
    public bool HasInterestRoom => InterestedMinMons.Count < INTEREST_CAPACITY;
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

    public void AddMinMon(int dataId)
    {
        MinMonSaveData save = GetSaveData(dataId);

        // 슬20/21: 능력치 등급 결정. 첫 포획은 새로 롤, 재포획은 굴려서 더 높으면 갱신.
        if (save.CaughtCount == 0)
        {
            save.AtkGrade      = RollGrade();
            save.AtkSpeedGrade = RollGrade();
            save.CritGrade     = RollGrade();
        }
        else
        {
            save.AtkGrade      = Mathf.Max(save.AtkGrade,      RollGrade());
            save.AtkSpeedGrade = Mathf.Max(save.AtkSpeedGrade, RollGrade());
            save.CritGrade     = Mathf.Max(save.CritGrade,     RollGrade());
        }

        save.CaughtCount++;
        save.TameCount++;
        // 포획 성공 시 포획 가능 창고에서 제거 (슬23: 창고의 미니몬을 잡은 것)
        Managers.Game.InterestedMinMons.Remove(dataId);
        Managers.Game.SaveGame();
        OnMinMonAdded?.Invoke(dataId);
        OnInterestChanged?.Invoke();
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
