using System;
using System.Collections.Generic;
using static Define;

[Serializable]
public class QuestProgress
{
    public int QuestId;
    public int Progress;
    public bool IsCompleted;
}

public class QuestManager
{
    Dictionary<int, QuestProgress> _activeQuests = new Dictionary<int, QuestProgress>();

    public event Action<int> OnQuestCompleted;
    public event Action<string> OnTabUnlocked;
    public event Action<int> OnAreaUnlocked;

    public void Init()
    {
        _activeQuests.Clear();

        foreach (int questId in Managers.Game.CompletedQuestIds)
        {
            if (_activeQuests.ContainsKey(questId)) continue;
            _activeQuests[questId] = new QuestProgress { QuestId = questId, IsCompleted = true };
        }
    }

    // ── 퀘스트 해금 ──
    public void UnlockQuest(int questId)
    {
        if (questId <= 0) return;
        if (_activeQuests.ContainsKey(questId)) return;
        if (!Managers.Data.QuestDic.ContainsKey(questId)) return;

        _activeQuests[questId] = new QuestProgress { QuestId = questId };
    }

    // ── 진행도 업데이트 ──
    public void OnMonsterKilled(int monsterId)
    {
        UpdateQuests(QuestType.KillMonster, monsterId);
    }

    public void OnAreaReached(int areaId)
    {
        UpdateQuests(QuestType.ReachArea, areaId);
    }

    public void OnItemGetted(int itemId)
    {
        UpdateQuests(QuestType.GetItem, itemId);
    }

    public void OnNpcTalked(int npcId)
    {
        UpdateQuests(QuestType.TalkNpc, npcId);
    }

    void UpdateQuests(QuestType type, int targetId)
    {
        foreach (var pair in _activeQuests)
        {
            QuestProgress progress = pair.Value;
            if (progress.IsCompleted) continue;

            if (!Managers.Data.QuestDic.TryGetValue(progress.QuestId, out var data)) continue;
            if (data.Type != type || data.TargetId != targetId) continue;

            progress.Progress++;
            if (progress.Progress >= data.TargetCount)
                CompleteQuest(progress.QuestId);
        }
    }

    // ── 완료 처리 ──
    public void CompleteQuest(int questId)
    {
        if (!_activeQuests.TryGetValue(questId, out var progress)) return;
        if (progress.IsCompleted) return;

        progress.IsCompleted = true;
        Managers.Game.CompletedQuestIds.Add(questId);

        if (!Managers.Data.QuestDic.TryGetValue(questId, out var data)) return;

        // 보상 지급
        if (data.RewardGold > 0)   Managers.Game.Gold    += data.RewardGold;
        if (data.RewardCrystal > 0) Managers.Game.Crystal += data.RewardCrystal;

        // 탭 해금
        if (!string.IsNullOrEmpty(data.UnlockTabName))
        {
            UnlockTab(data.UnlockTabName);
        }

        // 구역 해금
        if (data.UnlockAreaId > 0)
        {
            UnlockArea(data.UnlockAreaId);
        }

        OnQuestCompleted?.Invoke(questId);

        // 다음 퀘스트 자동 시작
        if (data.NextQuestId > 0)
            UnlockQuest(data.NextQuestId);

        Managers.Game.SaveGame();
    }

    void UnlockTab(string tabName)
    {
        if (Managers.Game.UnlockedTabs.Contains(tabName)) return;
        Managers.Game.UnlockedTabs.Add(tabName);
        OnTabUnlocked?.Invoke(tabName);
    }

    void UnlockArea(int areaId)
    {
        if (Managers.Game.UnlockedAreaIds.Contains(areaId)) return;
        Managers.Game.UnlockedAreaIds.Add(areaId);
        OnAreaUnlocked?.Invoke(areaId);
    }

    // ── 조회 ──
    public bool IsCompleted(int questId)
    {
        return Managers.Game.CompletedQuestIds.Contains(questId);
    }

    public QuestProgress GetProgress(int questId)
    {
        _activeQuests.TryGetValue(questId, out var p);
        return p;
    }

    public bool IsTabUnlocked(string tabName)
    {
        return Managers.Game.UnlockedTabs.Contains(tabName);
    }
}
