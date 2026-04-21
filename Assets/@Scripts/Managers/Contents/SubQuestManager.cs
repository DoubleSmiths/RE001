using System;
using System.Collections.Generic;
using Data;
using static Define;

/// <summary>
/// 메인퀘스트 진행 중 특정 타이밍에 분기되는 "서브퀘스트" 상태 관리.
/// SUBQUEST_START unlock으로 활성화 → 완료 조건 충족 시 다음 SubQuestID로 전이 →
/// 전이된 상태의 Unlock 테이블 실행. CurrentSubQuestID = 0은 비활성.
/// </summary>
public class SubQuestManager
{
    public int CurrentSubQuestID { get; private set; } = 0;

    public event Action<int> OnSubQuestChanged;    // 서브퀘스트 전이 (0 = 비활성)
    public event Action      OnNotifyChanged;      // 서브퀘스트 Notify UI 갱신 요청

    public void Init()
    {
        CurrentSubQuestID = Managers.Game.CurrentSubQuestID;

        Managers.Game.OnHPPotionChanged += OnHPPotionChanged;

        // 저장된 상태로 복귀 후 즉시 조건 검사 (앱 재시작 후에도 조건 이미 충족된 케이스)
        TryAutoComplete();

        OnNotifyChanged?.Invoke();
    }

    public void Clear()
    {
        if (Managers.Game != null)
            Managers.Game.OnHPPotionChanged -= OnHPPotionChanged;
    }

    // ────────────────────────────────────────
    //  외부 진입점
    // ────────────────────────────────────────

    /// <summary>"SUBQUEST_N" 문자열로부터 서브퀘스트 시작.</summary>
    public void StartSubQuestFromTarget(string targetId)
    {
        if (string.IsNullOrEmpty(targetId)) return;
        const string prefix = "SUBQUEST_";
        if (!targetId.StartsWith(prefix)) return;
        if (!int.TryParse(targetId.Substring(prefix.Length), out int subId)) return;
        StartSubQuest(subId);
    }

    public void StartSubQuest(int subQuestId)
    {
        if (subQuestId <= 0) return;
        if (!Managers.Data.SubQuestDic.ContainsKey(subQuestId)) return;

        SetCurrentSubQuest(subQuestId);
        ProcessUnlocks(subQuestId);
        // NOTE: 여기서 TryAutoComplete()를 호출하지 않음.
        //   시작 시점에 조건이 이미 충족돼 있으면 (예: 기본 HPPotion Rank=1)
        //   서브퀘스트가 1프레임도 살아있지 못하고 즉시 다음 상태로 넘어감 →
        //   Notify UI가 표시되지 않고, 메인퀘스트 Unlock 애니메이션도 덮어쓰기 문제 발생.
        //   조건 충족 검사는 OnHPPotionChanged 이벤트에서만 수행.
    }

    /// <summary>현재 서브퀘스트의 Notify 목록 반환. 없으면 빈 리스트.</summary>
    public List<SubQuestNotifyData> GetCurrentNotifyList()
    {
        if (CurrentSubQuestID <= 0) return new List<SubQuestNotifyData>();
        if (Managers.Data.SubQuestNotifyDic.TryGetValue(CurrentSubQuestID, out var list))
            return list;
        return new List<SubQuestNotifyData>();
    }

    /// <summary>현재 서브퀘스트의 Dialogue ID. 없으면 null.</summary>
    public string GetCurrentDialogueID()
    {
        if (!TryGetCurrent(out var q)) return null;
        return string.IsNullOrEmpty(q.DialogueID) ? null : q.DialogueID;
    }

    public bool IsActive => CurrentSubQuestID > 0;

    /// <summary>버튼 클릭 이벤트 수신 (QuestManager.CheckButtonClick에서 위임).</summary>
    public void CheckButtonClick(string btnId)
    {
        if (!TryGetCurrent(out var q)) return;
        if (q.CompleteType != SubQuestCompleteType.BTN_CLICK) return;
        if (q.CompleteParam != btnId) return;

        CompleteSubQuest(CurrentSubQuestID);
    }

    // ────────────────────────────────────────
    //  내부: 전이 / 완료 체크
    // ────────────────────────────────────────

    void OnHPPotionChanged(int dataId)
    {
        TryAutoComplete();
    }

    /// <summary>현재 서브퀘스트의 완료 조건 재평가. 충족 시 다음 SubQuestID로 전이.</summary>
    void TryAutoComplete()
    {
        if (!TryGetCurrent(out var q)) return;
        if (q.CompleteType == SubQuestCompleteType.NONE) return;

        if (IsConditionMet(q))
            CompleteSubQuest(CurrentSubQuestID);
    }

    bool IsConditionMet(SubQuestData q)
    {
        switch (q.CompleteType)
        {
            case SubQuestCompleteType.LEVEL_COUNT:
                return EvalLevelCount(q.CompleteParam);
            case SubQuestCompleteType.NONE:
            default:
                return false;
        }
    }

    /// <summary>"HPPotion_Rank:N" 형식 파싱 후 현재 HP 물약 Step >= N 체크.</summary>
    static bool EvalLevelCount(string param)
    {
        if (string.IsNullOrEmpty(param)) return false;
        int colon = param.IndexOf(':');
        if (colon <= 0) return false;
        string key = param.Substring(0, colon);
        string valStr = param.Substring(colon + 1);
        if (!int.TryParse(valStr, out int required)) return false;

        switch (key)
        {
            case "HPPotion_Rank":
                int currentDataId = Managers.Game.CurrentHPPotionDataId;
                if (Managers.Data.HPPotionDic.TryGetValue(currentDataId, out var hp))
                    return hp.Step >= required;
                return false;
            default:
                return false;
        }
    }

    void CompleteSubQuest(int subQuestId)
    {
        int nextId = subQuestId + 1;

        // 다음 SubQuestID가 정의돼 있지 않으면 서브퀘스트 라인 종료 (비활성화)
        if (!Managers.Data.SubQuestDic.ContainsKey(nextId))
        {
            SetCurrentSubQuest(0);
            return;
        }

        SetCurrentSubQuest(nextId);
        ProcessUnlocks(nextId);  // 새 상태 Unlock 처리
        TryAutoComplete();       // 연쇄 전이 (NONE 상태까지)
    }

    void SetCurrentSubQuest(int subQuestId)
    {
        CurrentSubQuestID = subQuestId;
        Managers.Game.CurrentSubQuestID = subQuestId;
        Managers.Game.SaveGame();

        OnSubQuestChanged?.Invoke(subQuestId);
        OnNotifyChanged?.Invoke();
    }

    void ProcessUnlocks(int subQuestId)
    {
        if (!Managers.Data.SubQuestUnlockDic.TryGetValue(subQuestId, out var unlocks))
            return;

        foreach (var u in unlocks)
        {
            switch (u.UnlockType)
            {
                case QuestUnlockType.UI_BUTTON:
                    // 서브퀘스트는 "조용한" 해금 — 언락 애니메이션 트리거 안 함.
                    // (메인퀘스트의 UnlockButtonAnim은 _pendingUnlockBtnId 하나만 저장하므로
                    //  서브퀘스트가 OnBtnUnlocked을 발행하면 메인 해금 애님이 덮어써짐.)
                    if (!Managers.Game.UnlockedBtnIds.Contains(u.TargetID))
                        Managers.Game.AddUnlockedBtn(u.TargetID);
                    break;

                case QuestUnlockType.ITEM_REWARD:
                    if (int.TryParse(u.TargetID, out int itemId))
                    {
                        Managers.Game.AddItem(itemId);
                        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(itemId);
                    }
                    break;

                case QuestUnlockType.ITEM_REMOVE:
                    if (int.TryParse(u.TargetID, out int removeId))
                    {
                        int have = Managers.Game.GetItemCount(removeId);
                        if (have > 0)
                            Managers.Game.RemoveItem(removeId, have);
                    }
                    break;

                case QuestUnlockType.SUBQUEST_START:
                    // 서브퀘스트 내부에서 다른 서브퀘스트를 바로 시작하는 경우
                    StartSubQuestFromTarget(u.TargetID);
                    break;
            }
        }
    }

    bool TryGetCurrent(out SubQuestData q)
    {
        q = null;
        if (CurrentSubQuestID <= 0) return false;
        return Managers.Data.SubQuestDic.TryGetValue(CurrentSubQuestID, out q);
    }

#if UNITY_EDITOR
    /// <summary>[에디터 전용] 서브퀘스트 ID 강제 변경.</summary>
    public void JumpToSubQuest(int subQuestId)
    {
        SetCurrentSubQuest(subQuestId);
    }
#endif
}
