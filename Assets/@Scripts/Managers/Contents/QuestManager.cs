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
        // MULTI 조건 충족 상태 복원 (대화 도중 종료 후 재시작해도 진입 조건이 소실되지 않도록).
        _metConditions.Clear();
        foreach (int order in Managers.Game.MetQuestConditions)
            _metConditions.Add(order);

        Managers.Game.OnItemAdded   += CheckItemAcquire;
        Managers.Game.OnItemChanged += OnInventoryChanged;

        // 서브퀘스트 전이 시 Notify UI 같이 갱신 (FollowUI는 서브퀘스트와 무관)
        if (Managers.SubQuest != null)
            Managers.SubQuest.OnNotifyChanged += RaiseNotifyChanged;

        OnNotifyChanged?.Invoke();
        OnFollowUIChanged?.Invoke(CurrentQuestID);
    }

    public void Clear()
    {
        Managers.Game.OnItemAdded   -= CheckItemAcquire;
        Managers.Game.OnItemChanged -= OnInventoryChanged;

        if (Managers.SubQuest != null)
            Managers.SubQuest.OnNotifyChanged -= RaiseNotifyChanged;
    }

    void RaiseNotifyChanged() => OnNotifyChanged?.Invoke();

    /// <summary>SubQuestManager가 UI_BUTTON 해금 시 동일한 해금 이벤트 재사용.</summary>
    public void RaiseBtnUnlocked(string btnId) => OnBtnUnlocked?.Invoke(btnId);

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
        // 서브퀘스트에도 같은 버튼 이벤트 전달 (메인퀘스트 완료/진행과 독립적으로 평가)
        Managers.SubQuest?.CheckButtonClick(btnId);

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

    /// <summary>현재 퀘스트의 느낌표 버튼 목록 반환 (Order 정렬).
    /// 서브퀘스트가 활성 상태면 서브퀘스트 Notify도 합쳐서 반환 (같은 UI 조회 경로 재사용).</summary>
    public List<QuestNotifyData> GetCurrentNotifyList()
    {
        List<QuestNotifyData> result;
        if (Managers.Data.QuestNotifyDic.TryGetValue(CurrentQuestID, out var list))
            result = new List<QuestNotifyData>(list);
        else
            result = new List<QuestNotifyData>();

        if (Managers.SubQuest != null && Managers.SubQuest.IsActive)
        {
            foreach (var sn in Managers.SubQuest.GetCurrentNotifyList())
            {
                result.Add(new QuestNotifyData
                {
                    QuestID = CurrentQuestID,
                    Order   = int.MaxValue,  // 메인 뒤에 오도록
                    BtnID   = sn.TargetID,
                });
            }
        }

        return result;
    }

    /// <summary>현재 퀘스트의 Follow UI 데이터 반환. null = 표시 안 함</summary>
    public QuestData GetCurrentFollowUI()
    {
        if (!TryGetCurrentQuest(out var quest)) return null;
        if (string.IsNullOrEmpty(quest.FollowItemID)) return null;
        return quest;
    }

    /// <summary>현재 퀘스트의 Dialogue ID 반환. 서브퀘스트 활성 시 서브퀘스트 Dialogue 우선.
    /// null = NPC 기본 대사 사용</summary>
    public string GetCurrentDialogueID()
    {
        if (Managers.SubQuest != null && Managers.SubQuest.IsActive)
        {
            var subDlg = Managers.SubQuest.GetCurrentDialogueID();
            if (!string.IsNullOrEmpty(subDlg)) return subDlg;
        }
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
        {
            CompleteQuest(CurrentQuestID);   // 전환 시 영속 집합도 초기화·저장됨
        }
        else
        {
            // 아직 미완료여도 충족 조건을 세이브에 보존 (도중 종료 대비).
            SyncMetConditionsToSave();
            Managers.Game.SaveGame();
        }
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
        SyncMetConditionsToSave();      // 새 퀘스트 → 충족 집합 비움(영속)
        Managers.Game.SaveGame();

        OnQuestChanged?.Invoke(questId);
        OnNotifyChanged?.Invoke();
        OnFollowUIChanged?.Invoke(questId);

        PlayQuestAnimation(questId);
    }

    // 런타임 _metConditions → GameData(세이브) 동기화. SaveGame 호출은 별도.
    void SyncMetConditionsToSave()
    {
        var save = Managers.Game.MetQuestConditions;
        save.Clear();
        save.AddRange(_metConditions);
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
                    Managers.SubQuest?.StartSubQuestFromTarget(unlock.TargetID);
                    break;

                case QuestUnlockType.ITEM_REWARD:
                    if (int.TryParse(unlock.TargetID, out int itemId))
                    {
                        Managers.Game.AddItem(itemId);
                        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(itemId, showGlow: true);
                    }
                    break;

                case QuestUnlockType.ITEM_REMOVE:
                    if (int.TryParse(unlock.TargetID, out int removeId))
                    {
                        int have = Managers.Game.GetItemCount(removeId);
                        if (have > 0)
                            Managers.Game.RemoveItem(removeId, have);
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
        SyncMetConditionsToSave();
        Managers.Game.SaveGame();

        OnQuestChanged?.Invoke(questId);
        OnNotifyChanged?.Invoke();
        OnFollowUIChanged?.Invoke(questId);
    }
#endif
}
