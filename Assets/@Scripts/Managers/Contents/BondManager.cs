using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// v2 마을 사람들 — 호감도·배달 의뢰·선물(ADR0036, 설계서 4.9). 포스트나이트2 의 '마을로 돌아올 이유'.
//  사람 5명(접수원·연금술사·대장장이·수도원 사제·상점 주인), 호감도 0~BOND_MAX_LEVEL 단계(단계마다 BOND_POINTS_PER_LEVEL 점).
//  배달 의뢰: 하루 BOND_REQUESTS_PER_DAY 건(현지 00시에 새로) — '재료 배달'(가진 재료를 건넨다) 또는 '소포 전달'(그 스테이지에서 N마리).
//   의뢰는 그날 날짜로 씨앗을 고정해 만든다(다시 열어도 같은 의뢰 — 새로 고침으로 좋은 의뢰를 고르는 일이 없다).
//   보상 = 그 스테이지 평균 골드 × 분량 × (1 + 접수원 혜택), 호감도 BOND_REQUEST_AFFECTION, 가끔 크리스탈.
//  선물: 가장 많이 가진 재료 BOND_GIFT_ITEMS 개 → 호감도 BOND_GIFT_AFFECTION(사람마다 하루 BOND_GIFTS_PER_DAY 번).
//  혜택(단계당): 접수원 의뢰 보상 %, 연금술사 회복력 %(RecoveryRate — 물약·회복 능력), 대장장이 강화 비용 −%,
//   수도원 사제 처치당 숙련도 +1 확률 %, 상점 주인 마을 상점 가격 −%. 세이브는 GameData.Bond.
public class BondManager
{
    public enum Npc { Receptionist, Alchemist, Blacksmith, Priest, Shopkeeper }
    public const int NPC_COUNT = 5;
    public const int KIND_DELIVER = 0, KIND_HUNT = 1;

    public static readonly string[] NAME_KEYS =
        { "NpcReceptionistNameText", "NpcAlchemistNameText", "NpcBlacksmithNameText", "NpcMonasteryPriestNameText", "NpcShopkeeperNameText" };
    static readonly string[] PERK_KEYS =
        { BalanceKey.BOND_PERK_RECEPTIONIST, BalanceKey.BOND_PERK_ALCHEMIST, BalanceKey.BOND_PERK_BLACKSMITH, BalanceKey.BOND_PERK_PRIEST, BalanceKey.BOND_PERK_SHOPKEEPER };
    static readonly float[] PERK_BASE = { 10f, 3f, 3f, 10f, 2f };

    public event Action OnChanged;

    static float B(string key, float fallback) => Managers.Data.Balance(key, fallback);

    BondSaveData Save
    {
        get
        {
            var s = Managers.Game.BondSave;
            int today = MissionManager.DayKey(DateTime.Now);
            if (s.DayKey != today) Roll(s, today);
            return s;
        }
    }

    public int PointsPerLevel => Mathf.Max(1, (int)B(BalanceKey.BOND_POINTS_PER_LEVEL, 20f));
    public int MaxLevel       => Mathf.Max(1, (int)B(BalanceKey.BOND_MAX_LEVEL, 10f));
    public int Points(Npc n)  => Managers.Game == null ? 0 : Managers.Game.BondSave.Points[(int)n];
    public int Level(Npc n)   => Mathf.Min(MaxLevel, Points(n) / PointsPerLevel);
    public float PerkPerLevel(Npc n) => B(PERK_KEYS[(int)n], PERK_BASE[(int)n]);
    public float Perk(Npc n)  => Level(n) * PerkPerLevel(n);   // % (단계 × 단계당)

    public int GiftsLeft(Npc n) => Mathf.Max(0, (int)B(BalanceKey.BOND_GIFTS_PER_DAY, 3f) - Save.Gifts[(int)n]);
    public int GiftItems => Mathf.Max(1, (int)B(BalanceKey.BOND_GIFT_ITEMS, 5f));
    public IReadOnlyList<BondRequest> Requests => Save.Requests;

    public bool HasReady
    {
        get
        {
            if (Managers.Game == null || Managers.Data == null) return false;
            foreach (var r in Save.Requests) if (!r.Claimed && IsReady(r)) return true;
            return false;
        }
    }

    #region 혜택 — 다른 시스템이 묻는다
    public float EnhanceCostMult => 1f - Mathf.Clamp01(Perk(Npc.Blacksmith) / 100f);
    public float ShopPriceMult   => 1f - Mathf.Clamp01(Perk(Npc.Shopkeeper) / 100f);
    public float ExtraProficiencyChance => Mathf.Clamp01(Perk(Npc.Priest) / 100f);
    public void ApplyTo(PlayerController p)
    {
        if (p != null) p.RecoveryRate += Perk(Npc.Alchemist);
    }
    #endregion

    #region 의뢰
    public int Progress(BondRequest r) => r.Kind == KIND_DELIVER ? Managers.Game.GetItemCount(r.ItemId) : r.Progress;
    public bool IsReady(BondRequest r) => Progress(r) >= r.Goal;

    // 처치 — 소포 전달 의뢰의 그 스테이지면 센다(MonsterController.OnDead)
    public void OnKill(int nodeId)
    {
        if (Managers.Game == null) return;
        bool changed = false;
        foreach (var r in Save.Requests)
            if (!r.Claimed && r.Kind == KIND_HUNT && r.Node == nodeId && r.Progress < r.Goal) { r.Progress++; changed = true; }
        if (!changed) return;
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
    }

    public bool TryComplete(BondRequest r)
    {
        if (r == null || r.Claimed || !IsReady(r)) return false;
        if (r.Kind == KIND_DELIVER) Managers.Game.RemoveItem(r.ItemId, r.Goal);   // IsReady 가 개수를 이미 확인했다
        r.Claimed = true;
        Managers.Game.Gold += r.Gold;
        if (r.Crystal > 0) Managers.Game.Crystal += r.Crystal;
        AddPoints((Npc)r.Npc, (int)B(BalanceKey.BOND_REQUEST_AFFECTION, 10f));
        Managers.Mission?.Notify(MissionEvent.BondRequest);
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }
    #endregion

    #region 선물
    // 가장 많이 가진 몬스터 재료(어떤 몬스터의 드롭)를 건넨다 — 퀘스트 필수품·주문서·물약은 건드리지 않는다
    public int GiftItemId()
    {
        int best = 0, bestCount = 0;
        foreach (var item in Managers.Game.Inventory)
        {
            if (item == null || item.Count < GiftItems || !IsMaterial(item.DataId)) continue;
            if (item.Count > bestCount) { best = item.DataId; bestCount = item.Count; }
        }
        return best;
    }

    HashSet<int> _materials;
    bool IsMaterial(int itemId)
    {
        if (_materials == null)
        {
            _materials = new HashSet<int>();
            foreach (var c in Managers.Data.CreatureDic.Values)
                if (c.DropItemId > 0) _materials.Add(c.DropItemId);
            foreach (var id in Managers.Data.RequisiteItemDic.Keys) _materials.Remove(id);   // 퀘스트 필수품은 선물로 쓰지 않는다
        }
        return _materials.Contains(itemId);
    }

    public bool TryGift(Npc n)
    {
        if (GiftsLeft(n) <= 0) return false;
        int id = GiftItemId();
        if (id <= 0) return false;
        Managers.Game.RemoveItem(id, GiftItems);
        Save.Gifts[(int)n]++;
        AddPoints(n, (int)B(BalanceKey.BOND_GIFT_AFFECTION, 2f));
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }
    #endregion

    void AddPoints(Npc n, int amount)
    {
        var s = Managers.Game.BondSave;
        int before = Level(n);
        s.Points[(int)n] = Mathf.Min(s.Points[(int)n] + amount, MaxLevel * PointsPerLevel);
        int after = Level(n);
        if (after > before)
        {
            if (n == Npc.Alchemist)
            {
                var p = Managers.Game.Player;   // 회복력이 바로 오른다(== 로 파괴 여부까지)
                if (p != null) p.RefreshStats(fullHeal: false);
            }
            Managers.UI?.ShowToast(string.Format(Util.Localize("BondLevelUpFormat"), Util.Localize(NAME_KEYS[(int)n]), after));
        }
    }

    #region 하루 새 의뢰
    // 날짜로 씨앗을 고정한다 — 같은 날엔 몇 번을 열어도 같은 의뢰. 사냥하던 스테이지(처음이면 1-1) 기준으로 만든다.
    void Roll(BondSaveData s, int today)
    {
        s.DayKey = today;
        Array.Clear(s.Gifts, 0, s.Gifts.Length);
        s.Requests.Clear();
        var g = Managers.Game;
        int node = g.LastBattleAreaId;
        if (node <= 0 || !g.IsTravelNodeUnlocked(node)) node = g.FirstStageNode;
        if (node <= 0) return;
        g.TryGetAreaAverage(node, out _, out float avgGold);
        avgGold = Mathf.Max(1f, avgGold);

        var drops = new List<int>();
        foreach (var c in Managers.Object.FieldPool(node))
            if (c.DropItemId > 0 && !drops.Contains(c.DropItemId)) drops.Add(c.DropItemId);

        var rng = new System.Random(today);
        int count = Mathf.Max(1, (int)B(BalanceKey.BOND_REQUESTS_PER_DAY, 3f));
        float bonus = 1f + Perk(Npc.Receptionist) / 100f;
        float crystalChance = B(BalanceKey.BOND_REQUEST_CRYSTAL_CHANCE, 0.25f);
        int crystal = (int)B(BalanceKey.BOND_REQUEST_CRYSTAL, 10f);
        for (int i = 0; i < count; i++)
        {
            var r = new BondRequest { Npc = rng.Next(NPC_COUNT), Node = node };
            if (drops.Count > 0 && rng.Next(2) == 0)
            {
                r.Kind   = KIND_DELIVER;
                r.ItemId = drops[rng.Next(drops.Count)];
                r.Goal   = 5 + rng.Next(8);   // 5~12개
                r.Gold   = Mathf.RoundToInt(avgGold * r.Goal * B(BalanceKey.BOND_DELIVER_GOLD_PER_ITEM, 3f) * bonus);
            }
            else
            {
                r.Kind = KIND_HUNT;
                r.Goal = 15 + rng.Next(16);   // 15~30마리
                r.Gold = Mathf.RoundToInt(avgGold * r.Goal * B(BalanceKey.BOND_HUNT_GOLD_PER_KILL, 2f) * bonus);
            }
            r.Crystal = rng.NextDouble() < crystalChance ? crystal : 0;
            s.Requests.Add(r);
        }
        g.SaveGame();
    }
    #endregion
}
