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
            return HasPendingScene;   // 볼 차례인 이야기(ADR0042)도 마을 탭 '!'·미션 버튼 점
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

    // 좋아하는 재료(BondNpcData, ADR0042)를 가진 게 있으면 그걸 먼저 건넨다 — 호감도 × BOND_GIFT_LIKED_MULT
    public int LikedGiftItemId(Npc n)
    {
        if (Managers.Data?.BondNpcDic == null || !Managers.Data.BondNpcDic.TryGetValue((int)n, out var d)) return 0;
        int best = 0, bestCount = 0;
        foreach (int id in d.LikedItems)
        {
            int count = Managers.Game.GetItemCount(id);
            if (count >= GiftItems && count > bestCount) { best = id; bestCount = count; }
        }
        return best;
    }

    // 준 호감도(0 이면 못 줬다) — 좋아하는 재료였는지는 LastGiftLiked
    public bool LastGiftLiked { get; private set; }
    public int TryGift(Npc n)
    {
        if (GiftsLeft(n) <= 0) return 0;
        int liked = LikedGiftItemId(n);
        int id = liked > 0 ? liked : GiftItemId();
        if (id <= 0) return 0;
        Managers.Game.RemoveItem(id, GiftItems);
        Save.Gifts[(int)n]++;
        int points = (int)B(BalanceKey.BOND_GIFT_AFFECTION, 2f);
        LastGiftLiked = liked > 0;
        if (LastGiftLiked) points = Mathf.RoundToInt(points * B(BalanceKey.BOND_GIFT_LIKED_MULT, 2f));
        AddPoints(n, points);
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return points;
    }
    #endregion

    #region 이야기 장면(ADR0042) — 호감도 BOND_SCENE_LEVEL_1~3(3·6·10)단계마다 하나. 그 사람의 [대화]를 누르면 잡담 대신 나오고(한 번),
    //  다 보면 크리스탈 BOND_SCENE_CRYSTAL_1~3. 대사는 DialogueData.xlsx 의 Bond{사람}{번호}Text 시트(_tools/gen_bond.py).
    public const int SCENE_COUNT = 3;
    static readonly string[] SCENE_PREFIX = { "BondReceptionist", "BondAlchemist", "BondBlacksmith", "BondPriest", "BondShopkeeper" };   // Npc 순서
    static readonly float[] SCENE_LEVEL_DEFAULT   = { 3f, 6f, 10f };
    static readonly float[] SCENE_CRYSTAL_DEFAULT = { 30f, 60f, 150f };

    public int ScenesSeen(Npc n) => Save.ScenesSeen[(int)n];
    public int SceneLevel(int scene) => (int)B(BalanceKey.BOND_SCENE_LEVEL_PREFIX + scene, SCENE_LEVEL_DEFAULT[scene - 1]);
    public string SceneKey(Npc n, int scene) => $"{SCENE_PREFIX[(int)n]}{scene}Text";

    // 지금 볼 차례인 이야기 번호(1~3), 없으면 0
    public int PendingScene(Npc n)
    {
        if (Managers.Game == null) return 0;
        int next = ScenesSeen(n) + 1;
        return next <= SCENE_COUNT && Level(n) >= SceneLevel(next) ? next : 0;
    }

    public bool HasPendingScene
    {
        get
        {
            if (Managers.Game == null) return false;
            for (int i = 0; i < NPC_COUNT; i++) if (PendingScene((Npc)i) > 0) return true;
            return false;
        }
    }

    // 이야기를 끝까지 본 뒤(대화창 onFinished) — 본 것으로 적고 크리스탈
    public void FinishScene(Npc n, int scene)
    {
        if (scene != ScenesSeen(n) + 1) return;   // 두 번 받지 않는다
        Save.ScenesSeen[(int)n] = scene;
        int crystals = (int)B(BalanceKey.BOND_SCENE_CRYSTAL_PREFIX + scene, SCENE_CRYSTAL_DEFAULT[scene - 1]);
        Managers.Game.Crystal += crystals;
        Managers.Game.SaveGame();
        Managers.UI?.ShowToast(string.Format(Util.Localize("BondSceneDoneFormat"), Util.Localize(NAME_KEYS[(int)n]), crystals));
        OnChanged?.Invoke();
    }
    #endregion

    // 비번 일정 '마을 일'(ADR0041) — 모두에게 호감도(단계가 오르면 사람마다 알림)
    public void AddToAll(int amount)
    {
        if (amount <= 0 || Managers.Game == null) return;
        for (int i = 0; i < NPC_COUNT; i++) AddPoints((Npc)i, amount);
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
    }

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
