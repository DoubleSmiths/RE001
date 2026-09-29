using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// v2 요일 던전(ADR0034, 설계서 4.8) — 골드·경험치·재료. 종류마다 하루 무료 DUNGEON_FREE_PER_DAY 회 + 광고 + 크리스탈.
//  들어가면 지금 사냥하던 스테이지(처음이면 1-1)의 몬스터로 DUNGEON_TIME 초 타임어택 — 잡은 수만큼 보상을 끝에 한꺼번에 준다
//  (처치마다 나오는 보통 보상은 그대로). 탭 공격(ADR0033)이라 손이 빠를수록 많이 잡는다.
//  요일마다 한 종류가 DUNGEON_TODAY_MULT 배(오늘의 던전). 전투는 UI_BattleBtnGroup.CoDungeon, 남은 시간·처치 수는 UI_StageBar,
//  입장 줄은 UI_MissionPopup 던전 탭. 세이브는 GameData.Dungeon(무료 입장 수·최고 기록), 광고 횟수는 ShopManager 광고 자리가 센다.
public class DungeonManager
{
    public enum Pay { Free, Ad, Crystal }

    public static readonly string[] NAME_KEYS = { "DungeonGoldText", "DungeonExpText", "DungeonMatText" };   // DungeonKind 순서

    public event Action OnChanged;

    public bool        IsRunning { get; private set; }
    public DungeonKind RunKind   { get; private set; }
    public int         RunNode   { get; private set; }
    public int         RunKills  { get; private set; }
    public float       TimeLeft  { get; set; }   // CoDungeon 이 줄인다(전투 정지 중엔 멈춘다)
    public float       TimeLimit => B(BalanceKey.DUNGEON_TIME, 60f);
    public bool        IsPending => _pending;

    bool _pending;
    readonly Dictionary<int, int> _runItems = new Dictionary<int, int>();

    static float B(string key, float fallback) => Managers.Data.Balance(key, fallback);

    DungeonSaveData Save
    {
        get
        {
            var s = Managers.Game.DungeonSave;
            int today = MissionManager.DayKey(DateTime.Now);
            if (s.DayKey != today)
            {
                s.DayKey = today;
                Array.Clear(s.Used, 0, s.Used.Length);
            }
            return s;
        }
    }

    public int  FreePerDay  => (int)B(BalanceKey.DUNGEON_FREE_PER_DAY, 2f);
    public int  AdsPerDay   => (int)B(BalanceKey.DUNGEON_ADS_PER_DAY, 1f);
    public int  CrystalCost => (int)B(BalanceKey.DUNGEON_CRYSTAL_COST, 30f);
    public int  FreeLeft(DungeonKind k) => Math.Max(0, FreePerDay - Save.Used[(int)k]);
    public int  AdLeft(DungeonKind k)   => Managers.Shop.AdLeft(AdPlacement(k), AdsPerDay);
    public int  Best(DungeonKind k)     => Save.BestKills[(int)k];
    static string AdPlacement(DungeonKind k) => ShopManager.AD_DUNGEON + k;

    public bool HasFreeEntry
    {
        get
        {
            if (Managers.Game == null || Managers.Data == null) return false;
            foreach (DungeonKind k in Enum.GetValues(typeof(DungeonKind)))
                if (FreeLeft(k) > 0) return true;
            return false;
        }
    }

    // 오늘의 던전 — 요일마다 돌아간다(일 골드 · 월 경험치 · 화 재료 · 수 골드 …)
    public static DungeonKind Today => (DungeonKind)((int)DateTime.Now.DayOfWeek % 3);
    public float Mult(DungeonKind k) => k == Today ? B(BalanceKey.DUNGEON_TODAY_MULT, 1.5f) : 1f;

    // 사냥하던 스테이지(열려 있을 때). 전투 경험이 없으면 첫 스테이지.
    public int PickNode()
    {
        var g = Managers.Game;
        int node = g.LastBattleAreaId;
        if (node <= 0 || !g.IsTravelNodeUnlocked(node)) node = g.FirstStageNode;
        return node;
    }

    // 값을 치르고 입장을 예약한다 — 전투 화면 전환은 부른 쪽(UI_MissionPopup)이 하고, 새 전투 그룹이 Begin 으로 시작한다.
    public bool TryEnter(DungeonKind k, Pay pay)
    {
        if (IsRunning || _pending) return false;
        int node = PickNode();
        if (node <= 0) return false;
        bool paid;
        switch (pay)
        {
            case Pay.Free:
                paid = FreeLeft(k) > 0;
                if (paid) Save.Used[(int)k]++;
                break;
            case Pay.Ad:
                paid = Managers.Shop.WatchAd(AdPlacement(k), AdsPerDay, () => { });
                break;
            default:
                paid = ShopManager.SpendCrystal(CrystalCost);
                break;
        }
        if (!paid) return false;

        _pending = true;
        RunKind  = k;
        RunNode  = node;
        Managers.Game.CurrentTravelAreaId = node;   // 스폰 지역은 이 값으로 정해진다 — 전환보다 먼저
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }

    // 전투 그룹이 뜰 때 — 예약된 입장이 있으면 던전을 시작한다.
    public bool Begin()
    {
        if (!_pending) return false;
        _pending  = false;
        IsRunning = true;
        RunKills  = 0;
        TimeLeft  = TimeLimit;
        _runItems.Clear();
        OnChanged?.Invoke();
        return true;
    }

    public void AddKill(int creatureId)
    {
        if (!IsRunning) return;
        RunKills++;
        if (RunKind != DungeonKind.Material) return;
        if (!Managers.Data.CreatureDic.TryGetValue(creatureId, out var c) || c.DropItemId <= 0) return;
        _runItems.TryGetValue(c.DropItemId, out int n);
        _runItems[c.DropItemId] = n + Math.Max(1, c.DropCount);
    }

    // 끝 — 시간 초과·쓰러짐·귀환·이동 모두 여기로 온다(여러 번 불려도 한 번만). 잡은 만큼 주고 결과를 한 줄로 알린다.
    //  ⚠️ 입장 예약(_pending)은 건드리지 않는다 — 전투에서 던전으로 들어가면 새 전투 그룹이 먼저 뜨고 옛 그룹의 EndBattle 이
    //     뒤늦게 여기로 오는데, 그때 예약을 지우면 값을 치르고도 보통 사냥이 시작됐다.
    public void Finish()
    {
        if (!IsRunning) return;
        IsRunning = false;

        var g = Managers.Game;
        float mult = Mult(RunKind);
        g.TryGetAreaAverage(RunNode, out float avgExp, out float avgGold);
        string reward;
        switch (RunKind)
        {
            case DungeonKind.Gold:
                int gold = Mathf.RoundToInt(RunKills * avgGold * B(BalanceKey.DUNGEON_GOLD_MULT, 4f) * mult * (Managers.Relic?.GoldMult ?? 1f));
                g.Gold += gold;
                reward = string.Format(Util.Localize("RewardGoldFormat"), gold);
                break;
            case DungeonKind.Exp:
                float exp = RunKills * avgExp * B(BalanceKey.DUNGEON_EXP_MULT, 4f) * mult;
                var player = g.Player;   // == 로 파괴 여부까지 본다(?. 금지)
                if (player != null) player.GainExp(exp);
                reward = string.Format(Util.Localize("RewardExpFormat"), exp);
                break;
            default:
                int items = 0;
                float matMult = B(BalanceKey.DUNGEON_MAT_MULT, 3f) * mult;
                foreach (var kv in _runItems)
                {
                    int n = Mathf.Max(1, Mathf.RoundToInt(kv.Value * matMult));
                    g.AddItem(kv.Key, n);
                    items += n;
                }
                int mandrakes = Mathf.FloorToInt(RunKills * mult / Mathf.Max(1f, B(BalanceKey.DUNGEON_MANDRAKE_KILLS, 20f)));
                if (mandrakes > 0) g.MandrakeCount += mandrakes;
                reward = string.Format(Util.Localize("DungeonMatRewardFormat"), items, mandrakes);
                break;
        }
        _runItems.Clear();

        var save = Save;
        if (RunKills > save.BestKills[(int)RunKind]) save.BestKills[(int)RunKind] = RunKills;
        g.SaveGame();
        if (RunKills > 0) Managers.Mission?.Notify(MissionEvent.DungeonClear);
        Managers.UI.ShowToast(string.Format(Util.Localize("DungeonResultFormat"), Util.Localize(NAME_KEYS[(int)RunKind]), RunKills, reward));
        OnChanged?.Invoke();
    }
}
