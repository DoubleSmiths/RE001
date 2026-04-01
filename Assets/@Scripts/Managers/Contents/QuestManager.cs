using System;
using System.Collections.Generic;
using Data;
using static Define;

public class QuestManager
{
    // ── 현재 퀘스트 상태 ───────────────────────────────────────────────────
    public int CurrentQuestID { get; private set; } = 1;

    // MULTI 조건 충족 추적: Order → 충족 여부
    readonly HashSet<int> _metConditions = new HashSet<int>();

    // ── 이벤트 ────────────────────────────────────────────────────────────
    public event Action<int>    OnQuestChanged;    // 퀘스트 ID 변경
    public event Action<int>    OnFollowUIChanged; // Follow UI 갱신 요청 (questId)
    public event Action         OnNotifyChanged;   // 느낌표 UI 갱신 요청
    public event Action<string> OnBtnUnlocked;     // UI_BUTTON 해금 (btnId)

    // ── 초기화 ────────────────────────────────────────────────────────────
    public void Init()
    {
        CurrentQuestID = Managers.Game.CurrentQuestID;
        _metConditions.Clear();

        Managers.Game.OnItemAdded   += CheckItemAcquire;
        Managers.Game.OnItemChanged += OnInventoryChanged;

        OnNotifyChanged?.Invoke();
        OnFollowUIChanged?.Invoke(CurrentQuestID);
    }

    public void Clear()
    {
        Managers.Game.OnItemAdded   -= CheckItemAcquire;
        Managers.Game.OnItemChanged -= OnInventoryChanged;
    }

    // ── 외부 진입점 ───────────────────────────────────────────────────────

    /// <summary>아이템 최초 획득 시 호출 (GameManager.AddItem에서 발행)</summary>
    public void CheckItemAcquire(int dataId)
    {
        if (!TryGetCurrentQuest(out var quest)) return;

        if (quest.CompleteType == QuestCompleteType.ITEM_ACQUIRE)
        {
            if (int.TryParse(quest.CompleteTarget, out int target) && target == dataId)
                CompleteQuest(CurrentQuestID);
        }
        else if (quest.CompleteType == QuestCompleteType.MULTI)
        {
            MarkMultiCondition(QuestConditionType.ITEM_ACQUIRE, dataId.ToString());
        }
    }

    /// <summary>버튼 클릭 시 호출</summary>
    public void CheckButtonClick(string btnId)
    {
        if (!TryGetCurrentQuest(out var quest)) return;

        if (quest.CompleteType == QuestCompleteType.BTN_CLICK
            && quest.CompleteTarget == btnId)
            CompleteQuest(CurrentQuestID);
        else if (quest.CompleteType == QuestCompleteType.MULTI)
            MarkMultiCondition(QuestConditionType.BTN_CLICK, btnId);
    }

    /// <summary>연출 완료 시 호출</summary>
    public void CheckAnimationEnd(string animId)
    {
        if (!TryGetCurrentQuest(out var quest)) return;

        if (quest.CompleteType == QuestCompleteType.ANIM_END
            && quest.CompleteTarget == animId)
            CompleteQuest(CurrentQuestID);
    }

    // ── 현재 퀘스트 정보 조회 ─────────────────────────────────────────────

    /// <summary>현재 퀘스트의 느낌표 버튼 목록 반환 (Order 정렬)</summary>
    public List<QuestNotifyData> GetCurrentNotifyList()
    {
        if (Managers.Data.QuestNotifyDic.TryGetValue(CurrentQuestID, out var list))
            return list;
        return new List<QuestNotifyData>();
    }

    /// <summary>현재 퀘스트의 Follow UI 데이터 반환. null = 표시 안 함</summary>
    public QuestData GetCurrentFollowUI()
    {
        if (!TryGetCurrentQuest(out var quest)) return null;
        if (string.IsNullOrEmpty(quest.FollowItemID)) return null;
        return quest;
    }

    /// <summary>현재 퀘스트의 Dialogue ID 반환. null = NPC 기본 대사 사용</summary>
    public string GetCurrentDialogueID()
    {
        if (!TryGetCurrentQuest(out var quest)) return null;
        return string.IsNullOrEmpty(quest.DialogueID) ? null : quest.DialogueID;
    }

    // ── 내부 처리 ─────────────────────────────────────────────────────────

    void OnInventoryChanged(int dataId)
    {
        int count = Managers.Game.GetItemCount(dataId);

        // 현재 퀘스트가 ITEM_COUNT이면 달성 체크
        if (TryGetCurrentQuest(out var quest)
            && quest.CompleteType == QuestCompleteType.ITEM_COUNT
            && int.TryParse(quest.CompleteTarget, out int target)
            && target == dataId
            && count >= quest.CompleteCount)
        {
            CompleteQuest(CurrentQuestID);
            return;
        }

        // 이전 퀘스트가 ITEM_COUNT였으면 롤백 체크
        int prevId = CurrentQuestID - 1;
        if (prevId >= 1
            && Managers.Data.QuestDic.TryGetValue(prevId, out var prev)
            && prev.CompleteType == QuestCompleteType.ITEM_COUNT
            && int.TryParse(prev.CompleteTarget, out int prevTarget)
            && prevTarget == dataId
            && count < prev.CompleteCount)
        {
            RollbackTo(prevId);
        }
    }

    void MarkMultiCondition(QuestConditionType type, string target)
    {
        if (!Managers.Data.QuestConditionDic.TryGetValue(CurrentQuestID, out var conditions))
            return;

        foreach (var cond in conditions)
        {
            if (cond.ConditionType == type && cond.Target == target)
            {
                _metConditions.Add(cond.Order);
                break;
            }
        }

        if (_metConditions.Count >= conditions.Count)
            CompleteQuest(CurrentQuestID);
    }

    void CompleteQuest(int questId)
    {
        int nextId = questId + 1;
        SetCurrentQuest(nextId);   // 먼저 퀘스트 전환 → CurrentQuestID = nextId

        ProcessUnlocks(questId);   // 이후 보상 지급 (AddItem 이벤트가 발행돼도 이미 다음 퀘스트 기준으로 체크)
    }

    void RollbackTo(int questId)
    {
        SetCurrentQuest(questId);
    }

    void SetCurrentQuest(int questId)
    {
        CurrentQuestID = questId;
        _metConditions.Clear();

        Managers.Game.CurrentQuestID = questId;
        Managers.Game.SaveGame();

        OnQuestChanged?.Invoke(questId);
        OnNotifyChanged?.Invoke();
        OnFollowUIChanged?.Invoke(questId);

        PlayQuestAnimation(questId);
    }

    void ProcessUnlocks(int questId)
    {
        if (!Managers.Data.QuestUnlockDic.TryGetValue(questId, out var unlocks))
            return;

        foreach (var unlock in unlocks)
        {
            switch (unlock.UnlockType)
            {
                case QuestUnlockType.UI_BUTTON:
                    Managers.Game.AddUnlockedBtn(unlock.TargetID);
                    OnBtnUnlocked?.Invoke(unlock.TargetID);
                    break;

                case QuestUnlockType.SUBQUEST_START:
                    // TODO: SubQuest 시스템 연동
                    break;

                case QuestUnlockType.ITEM_REWARD:
                    if (int.TryParse(unlock.TargetID, out int itemId))
                    {
                        Managers.Game.AddItem(itemId);
                        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(itemId);
                    }
                    break;
            }
        }
    }

    void PlayQuestAnimation(int questId)
    {
        if (!Managers.Data.QuestDic.TryGetValue(questId, out var quest)) return;
        if (string.IsNullOrEmpty(quest.AnimationID)) return;

        // AnimationManager 또는 연출 시스템 연동 지점
        // 연출 완료 후 CheckAnimationEnd(quest.AnimationID) 호출 필요
        // 예: AnimationManager.Play(quest.AnimationID, quest.AnimBlocking,
        //         onComplete: () => CheckAnimationEnd(quest.AnimationID));
    }

    bool TryGetCurrentQuest(out QuestData quest)
    {
        return Managers.Data.QuestDic.TryGetValue(CurrentQuestID, out quest);
    }

#if UNITY_EDITOR
    /// <summary>[에디터 전용] 지정한 퀘스트 ID로 즉시 이동 (이벤트 재발행)</summary>
    public void JumpToQuest(int questId)
    {
        CurrentQuestID = questId;
        _metConditions.Clear();
        Managers.Game.CurrentQuestID = questId;
        Managers.Game.SaveGame();

        OnQuestChanged?.Invoke(questId);
        OnNotifyChanged?.Invoke();
        OnFollowUIChanged?.Invoke(questId);
    }
#endif
}
