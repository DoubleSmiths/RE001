using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// v2 유물 소환(ADR0035, 설계서 4.10) — 30종(RelicData, _tools/gen_relics.py). 등급 확률 일반·희귀·영웅·전설 = RELIC_RATE_*
//  (70/24/5.5/0.5%), 같은 등급 안에서는 균등. 이미 가진 유물이 나오면 레벨 +1(최대 RELIC_MAX_LEVEL), 최대면 크리스탈 환급.
//  전설 천장: 전설 없이 RELIC_PITY−1 번 뽑았으면 다음 한 번은 전설 확정(전설이 나오면 0부터 다시 센다).
//  확률은 전부 소환 화면에 공개한다(게임산업법 확률형 아이템 표시 의무, 2024-03-22 시행 — UI_MissionPopup 유물 탭).
//  값: 1회 RELIC_COST_ONE, 10회 RELIC_COST_TEN 크리스탈, 하루 광고 RELIC_ADS_PER_DAY 회(1회 소환).
//  효과: 공격·체력·방어 %, 치명 피해 %p, 경험치 → GrowthRate, 획득률 → DropRate 는 PlayerController.RefreshStats 가 ApplyTo 로 받고,
//  골드는 GoldMult(처치·방치·오프라인·던전), 관심 확률은 InterestMult(MinMonManager.RollInterest). 세이브는 GameData.Relic.
public class RelicManager
{
    public enum Pay { Crystal, Ad }

    public const int GRADE_COUNT = 4;   // 0 일반 · 1 희귀 · 2 영웅 · 3 전설
    public const int LEGENDARY   = 3;
    public static readonly string[] GRADES     = { "Common", "Rare", "Epic", "Legendary" };
    public static readonly string[] GRADE_KEYS = { "RelicGradeCommonText", "RelicGradeRareText", "RelicGradeEpicText", "RelicGradeLegendaryText" };
    static readonly string[] RATE_KEYS   = { BalanceKey.RELIC_RATE_COMMON, BalanceKey.RELIC_RATE_RARE, BalanceKey.RELIC_RATE_EPIC, BalanceKey.RELIC_RATE_LEGENDARY };
    static readonly float[]  RATE_BASE   = { 70f, 24f, 5.5f, 0.5f };
    static readonly string[] REFUND_KEYS = { BalanceKey.RELIC_REFUND_COMMON, BalanceKey.RELIC_REFUND_RARE, BalanceKey.RELIC_REFUND_EPIC, BalanceKey.RELIC_REFUND_LEGENDARY };
    static readonly float[]  REFUND_BASE = { 2f, 5f, 15f, 50f };

    public struct Pull
    {
        public int  Id;
        public int  Grade;
        public bool IsNew;      // 처음 얻었다
        public int  Refund;     // 최대 레벨이라 크리스탈로 돌려받은 수(0 = 레벨이 올랐다)
    }

    public event Action OnChanged;

    RelicSaveData Save => Managers.Game.RelicSave;
    static float B(string key, float fallback) => Managers.Data.Balance(key, fallback);

    public int CostOne   => (int)B(BalanceKey.RELIC_COST_ONE, 30f);
    public int CostTen   => (int)B(BalanceKey.RELIC_COST_TEN, 270f);
    public int MaxLevel  => Mathf.Max(1, (int)B(BalanceKey.RELIC_MAX_LEVEL, 10f));
    public int PityLimit => Mathf.Max(1, (int)B(BalanceKey.RELIC_PITY, 150f));
    public int Pity      => Save.Pity;
    public int AdsPerDay => (int)B(BalanceKey.RELIC_ADS_PER_DAY, 1f);
    public int AdLeft    => Managers.Shop.AdLeft(ShopManager.AD_RELIC, AdsPerDay);
    public int Level(int id) => Save.Levels.TryGetValue(id, out int lv) ? lv : 0;
    public int OwnedCount => Save.Levels.Count;

    public static int GradeIndex(string grade) => Math.Max(0, Array.IndexOf(GRADES, grade));
    public float GradeRate(int grade) => B(RATE_KEYS[grade], RATE_BASE[grade]);   // %
    public int   Refund(int grade)    => (int)B(REFUND_KEYS[grade], REFUND_BASE[grade]);

    public int GradeCount(int grade)
    {
        int n = 0;
        foreach (var r in Managers.Data.RelicDic.Values) if (GradeIndex(r.Grade) == grade) n++;
        return n;
    }

    // 한 종류가 나올 확률(%) = 등급 확률 ÷ 그 등급 종류 수. 천장은 별도(확정).
    public float ItemRate(Data.RelicData r)
    {
        int g = GradeIndex(r.Grade), n = GradeCount(g);
        return n > 0 ? GradeRate(g) / n : 0f;
    }

    public float Value(Data.RelicData r, int level) => level <= 0 ? 0f : r.Base + r.PerLevel * (level - 1);

    #region 소환
    // 값을 치르고 count 번(1 또는 10) 뽑는다. 못 치르면 null. 광고는 1회만.
    public List<Pull> Summon(int count, Pay pay)
    {
        if (Managers.Data?.RelicDic == null || Managers.Data.RelicDic.Count == 0) return null;
        count = count >= 10 ? 10 : 1;
        bool paid = pay == Pay.Ad
            ? count == 1 && Managers.Shop.WatchAd(ShopManager.AD_RELIC, AdsPerDay, () => { })
            : ShopManager.SpendCrystal(count == 10 ? CostTen : CostOne);
        if (!paid) return null;

        var pulls = new List<Pull>(count);
        for (int i = 0; i < count; i++) pulls.Add(PullOne());
        Save.Summons += count;   // 효과 합 캐시가 이 값으로 새로 센다
        Managers.Game.SaveGame();
        var player = Managers.Game.Player;   // 공격·체력 등이 바로 오른다(== 로 파괴 여부까지)
        if (player != null) player.RefreshStats(fullHeal: false);
        OnChanged?.Invoke();
        return pulls;
    }

    Pull PullOne()
    {
        var save = Save;
        int grade = RollGrade();
        var pool = new List<Data.RelicData>();
        foreach (var r in Managers.Data.RelicDic.Values) if (GradeIndex(r.Grade) == grade) pool.Add(r);
        var pick = pool[UnityEngine.Random.Range(0, pool.Count)];

        save.Pity = grade == LEGENDARY ? 0 : save.Pity + 1;
        int lv = Level(pick.DataId);
        var pull = new Pull { Id = pick.DataId, Grade = grade, IsNew = lv == 0 };
        if (lv >= MaxLevel)
        {
            pull.Refund = Refund(grade);
            Managers.Game.Crystal += pull.Refund;
        }
        else
            save.Levels[pick.DataId] = lv + 1;
        return pull;
    }

    // 천장이면 전설, 아니면 등급 확률대로(합이 100이 아니어도 비율로). 종류가 없는 등급은 건너뛴다.
    int RollGrade()
    {
        if (Save.Pity >= PityLimit - 1 && GradeCount(LEGENDARY) > 0) return LEGENDARY;
        float total = 0f;
        for (int g = 0; g < GRADE_COUNT; g++) if (GradeCount(g) > 0) total += GradeRate(g);
        float roll = UnityEngine.Random.value * total;
        for (int g = 0; g < GRADE_COUNT; g++)
        {
            if (GradeCount(g) == 0) continue;
            roll -= GradeRate(g);
            if (roll < 0f) return g;
        }
        for (int g = GRADE_COUNT - 1; g >= 0; g--) if (GradeCount(g) > 0) return g;
        return 0;
    }
    #endregion

    #region 효과
    readonly Dictionary<string, float> _sums = new Dictionary<string, float>();
    RelicSaveData _sumsFor;
    int _sumsSummons = -1;

    // 효과 종류(StatType)별 합 — 가진 유물의 현재 레벨 값. 처치마다 불리니 캐시하고, 세이브가 바뀌었거나(불러오기·삭제)
    //  소환 수가 달라졌으면 다시 센다(플레이어 Init 이 세이브를 읽기 전에 먼저 불러 0 으로 굳는 일이 없게).
    public float Sum(string stat)
    {
        if (Managers.Game == null || Managers.Data?.RelicDic == null) return 0f;
        var save = Save;
        if (!ReferenceEquals(save, _sumsFor) || save.Summons != _sumsSummons)
        {
            _sumsFor = save;
            _sumsSummons = save.Summons;
            _sums.Clear();
            foreach (var r in Managers.Data.RelicDic.Values)
            {
                int lv = Level(r.DataId);
                if (lv <= 0) continue;
                _sums.TryGetValue(r.StatType, out float s);
                _sums[r.StatType] = s + Value(r, lv);
            }
        }
        return _sums.TryGetValue(stat, out float v) ? v : 0f;
    }

    public float GoldMult     => 1f + Sum("Gold") / 100f;
    public float InterestMult => 1f + Sum("Interest") / 100f;

    // RefreshStats 가 훈련 다음에 부른다 — 곱은 훈련까지 더한 값에 건다(훈련과 같은 방식)
    public void ApplyTo(PlayerController p)
    {
        if (p == null) return;
        float atk = 1f + Sum("Atk") / 100f;
        p.PhysicalAtk *= atk;
        p.MagicAtk    *= atk;
        p.MaxHp       *= 1f + Sum("Hp") / 100f;
        p.Def         *= 1f + Sum("Def") / 100f;
        p.CritDamage  += Sum("CritDmg");
        p.GrowthRate  += Sum("Exp");
        p.DropRate    += Sum("Drop");
    }
    #endregion
}
