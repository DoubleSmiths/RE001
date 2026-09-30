using System;
using UnityEngine;
using static Define;

// v2 심연(ADR0037, 설계서 4.11) — 모든 스테이지를 돌파하면 열리는 끝없는 층. 마지막 스테이지의 몬스터를
//  ABYSS_START_MULT × ABYSS_FLOOR_MULT^(층−1) 배로 키워 층마다 ABYSS_KILLS_PER_FLOOR 마리, ABYSS_BOSS_EVERY 층마다 보스(제한 시간).
//  쓰러지거나 보스를 놓치면 끝. 처음 밟은 층마다 크리스탈(ABYSS_CRYSTAL_PER_FLOOR, 보스 층은 ABYSS_BOSS_CRYSTAL_MULT 배).
//  처치 보상(골드·경험치)도 층 배수만큼 커진다. 입장은 미션 창 던전 탭, 전투는 UI_BattleBtnGroup.CoAbyss. 세이브는 GameData.AbyssBest.
public class AbyssManager
{
    public event Action OnChanged;

    static float B(string key, float fallback) => Managers.Data.Balance(key, fallback);

    public int  Best         => Managers.Game.AbyssBest;
    public bool IsUnlocked   => Managers.Game.AllStagesCleared;
    public int  Node         => Managers.Game.LastStageNode;
    public int  KillsPerFloor => Mathf.Max(1, (int)B(BalanceKey.ABYSS_KILLS_PER_FLOOR, 5f));
    public int  BossEvery    => Mathf.Max(1, (int)B(BalanceKey.ABYSS_BOSS_EVERY, 10f));
    public float BossTime    => B(BalanceKey.ABYSS_BOSS_TIME, 30f);
    public float BossMult    => B(BalanceKey.ABYSS_BOSS_MULT, 1.5f);

    public bool IsPending { get; private set; }
    public bool IsRunning { get; private set; }
    public int  Floor     { get; private set; }
    public int  FloorKills { get; private set; }
    public bool IsBossFloor => Floor % BossEvery == 0;
    public float FloorMult(int floor) => B(BalanceKey.ABYSS_START_MULT, 1.2f) * Mathf.Pow(B(BalanceKey.ABYSS_FLOOR_MULT, 1.08f), floor - 1);

    int _runCrystals;

    public bool TryEnter()
    {
        if (!IsUnlocked || IsPending || IsRunning || Node <= 0) return false;
        if (Managers.Dungeon != null && (Managers.Dungeon.IsPending || Managers.Dungeon.IsRunning)) return false;
        if (Managers.Promotion != null && (Managers.Promotion.IsPending || Managers.Promotion.IsRunning)) return false;
        IsPending = true;
        Managers.Game.CurrentTravelAreaId = Node;
        OnChanged?.Invoke();
        return true;
    }

    public bool Begin()
    {
        if (!IsPending) return false;
        IsPending = false;
        IsRunning = true;
        Floor = 1;
        FloorKills = 0;
        _runCrystals = 0;
        return true;
    }

    // 보통 층의 처치 — 채우면 다음 층(보스 층은 CoAbyss 가 보스를 이기면 NextFloor)
    public void AddKill()
    {
        if (!IsRunning || IsBossFloor) return;
        if (++FloorKills >= KillsPerFloor) NextFloor();
    }

    public void NextFloor()
    {
        if (!IsRunning) return;
        // 처음 밟은 층을 넘기면 기록 + 크리스탈
        if (Floor > Best)
        {
            float mult = IsBossFloor ? B(BalanceKey.ABYSS_BOSS_CRYSTAL_MULT, 5f) : 1f;
            int crystals = Mathf.RoundToInt(B(BalanceKey.ABYSS_CRYSTAL_PER_FLOOR, 5f) * mult);
            Managers.Game.AbyssBest = Floor;
            Managers.Costume?.CheckNew();   // 심연 의상(ADR0043) — 목표 층을 넘기면 알림
            Managers.Game.Crystal += crystals;
            _runCrystals += crystals;
        }
        Floor++;
        FloorKills = 0;
        OnChanged?.Invoke();
    }

    // 끝 — 쓰러짐·보스 놓침·귀환·이동. 몇 층까지 갔는지와 이번 판 크리스탈을 알린다(여러 번 불려도 한 번).
    public void Finish()
    {
        if (!IsRunning) return;
        IsRunning = false;
        Managers.UI.ShowToast(string.Format(Util.Localize("AbyssResultFormat"), Floor, Best, _runCrystals));
        OnChanged?.Invoke();
    }
}
