using System;
using System.Collections.Generic;
using UnityEngine;

public class MinMonManager
{
    public event Action<int> OnCompanionChanged;
    public event Action<int> OnMinMonAdded;

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
        save.TameCount++;
        Managers.Game.SaveGame();
        OnMinMonAdded?.Invoke(dataId);
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
    // 동기화(%) = TameCount / MaxTameCount
    public float GetSyncRate(int dataId)
    {
        if (!Managers.Data.MinMonDic.TryGetValue(dataId, out var data))
            return 0f;
        MinMonSaveData save = GetSaveData(dataId);
        if (save == null) return 0f;
        return Mathf.Clamp01((float)save.TameCount / data.MaxTameCount);
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
        return Managers.Game.OwnedMinMons.Exists(m => m.DataId == dataId && m.TameCount > 0);
    }

    public bool IsDiscovered(int dataId)
    {
        return Managers.Game.DiscoveredMonsterIds.Contains(dataId);
    }
}
