using System;
using System.Collections.Generic;
using static Define;

// v2 미션·출석(ADR0030, 설계서 4.5). 표는 MissionData(_tools/gen_missions.py).
//  일일은 기기 현지 00시, 주간은 월요일 00시에 비운다. 이벤트는 게임 곳곳에서 Notify 로 들어오고
//  (처치·보스·새 스테이지·훈련·포획 시도·강화·패링), 레벨·도감·돌파 수처럼 상태로 알 수 있는 것은 그때 계산한다(가이드).
//  세이브는 GameData.Missions — 세이브가 필요한 값은 GameData 에 둔다(AGENTS.md).
public class MissionManager
{
    public const string DAILY = "Daily", WEEKLY = "Weekly", GUIDE = "Guide", ATTENDANCE = "Attendance";
    public const string PASS = "Pass", ACHIEVE = "Achieve";   // 성장 패스(ADR0039)·업적(ADR0040) — 가이드처럼 누적·상태로 판정
    public const int ATTENDANCE_DAYS = 7;

    public event Action OnChanged;

    MissionSaveData Save => Managers.Game.MissionSave;
    static IEnumerable<Data.MissionData> All => Managers.Data.MissionDic.Values;

    public void Init() { }

    public void Notify(MissionEvent e, int amount = 1)
    {
        if (amount <= 0 || Managers.Game == null || Managers.Data?.MissionDic == null) return;
        RollPeriods();
        var s = Save;
        string key = e.ToString();
        s.Lifetime.TryGetValue(key, out int life);
        s.Lifetime[key] = life + amount;
        foreach (var m in All)
        {
            if ((m.Kind != DAILY && m.Kind != WEEKLY) || m.Event != key) continue;
            s.Progress.TryGetValue(m.DataId, out int cur);
            s.Progress[m.DataId] = Math.Min(m.Goal, cur + amount);
        }
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
    }

    public List<Data.MissionData> List(string kind)
    {
        RollPeriods();
        var list = new List<Data.MissionData>();
        foreach (var m in All) if (m.Kind == kind) list.Add(m);
        list.Sort((a, b) => a.Order.CompareTo(b.Order));
        return list;
    }

    public int Progress(Data.MissionData m)
    {
        var g = Managers.Game;
        switch (m.Event)
        {
            case nameof(MissionEvent.ReachLevel): return g.Level;
            case nameof(MissionEvent.Discover):   return g.DiscoveredMonsterIds.Count;
            case nameof(MissionEvent.DailyChest): return ClaimedDailyCount();
        }
        if (m.Kind == GUIDE || m.Kind == PASS || m.Kind == ACHIEVE)
        {
            if (m.Event == nameof(MissionEvent.NewStage)) return g.ClearedStageCount;
            return Save.Lifetime.TryGetValue(m.Event, out int n) ? n : 0;
        }
        return Save.Progress.TryGetValue(m.DataId, out int p) ? p : 0;
    }

    public bool IsDone(Data.MissionData m)    => Progress(m) >= m.Goal;
    public bool IsClaimed(Data.MissionData m) => Save.Claimed.Contains(m.DataId);

    // 가이드는 한 줄 사다리 — 받지 않은 첫 단계만 보인다(모두 받으면 null).
    public Data.MissionData CurrentGuide
    {
        get
        {
            foreach (var m in List(GUIDE)) if (!IsClaimed(m)) return m;
            return null;
        }
    }

    public bool TryClaim(Data.MissionData m)
    {
        if (m == null || m.Kind == ATTENDANCE) return false;
        RollPeriods();
        if (m.Kind == PASS) return TryClaimPass(m);
        if (IsClaimed(m) || !IsDone(m)) return false;
        if (m.Kind == GUIDE && m != CurrentGuide) return false;   // 사다리는 순서대로
        if (m.Kind == ACHIEVE && m != CurrentAchieve(m.Event)) return false;   // 업적도 분야마다 순서대로

        Save.Claimed.Add(m.DataId);
        Grant(m.RewardType, m.RewardValue);
        if (m.Kind == ACHIEVE && !string.IsNullOrEmpty(m.HonorKey))
        {
            if (string.IsNullOrEmpty(Save.Honor)) Save.Honor = m.HonorKey;   // 첫 칭호는 바로 단다 — HUD 에서 곧바로 보이게
            if (_batchHonors != null) _batchHonors.Add(m.HonorKey);
            else Managers.UI?.ShowToast(string.Format(Util.Localize("HonorGetFormat"), Util.Localize(m.HonorKey)));
        }
        if (m.Event == nameof(MissionEvent.DailyChest)) Notify(MissionEvent.DailyAllClear);   // 주간 '일일 완료 N일'
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }

    public static void Grant(string type, int value)
    {
        switch (type)
        {
            case "Mandrake": Managers.Game.MandrakeCount += value; break;
            case "Gold":     Managers.Game.Gold          += value; break;
            default:         Managers.Game.Crystal       += value; break;
        }
    }

    #region 성장 패스(ADR0039) — 스테이지 5곳마다 무료 줄 + (프리미엄을 샀으면) 프리미엄 줄
    //  프리미엄은 인앱 상품 Kind "Pass" 를 샀는지로 판단한다(ShopManager). 나중에 사면 이미 받은 칸의 프리미엄 줄만 다시 받을 수 있다.
    public Data.ProductData PassProduct
    {
        get
        {
            if (Managers.Data?.ProductDic == null) return null;
            foreach (var p in Managers.Data.ProductDic.Values) if (p.Kind == "Pass") return p;
            return null;
        }
    }
    public bool PassPremium { get { var p = PassProduct; return p != null && Managers.Shop != null && Managers.Shop.IsBought(p); } }
    public bool IsPremiumClaimed(Data.MissionData m) => Save.PremiumClaimed.Contains(m.DataId);
    public bool CanClaimPass(Data.MissionData m)
        => IsDone(m) && (!IsClaimed(m) || (PassPremium && m.PremiumValue > 0 && !IsPremiumClaimed(m)));
    public bool HasPassClaimable
    {
        get
        {
            foreach (var m in List(PASS)) if (CanClaimPass(m)) return true;
            return false;
        }
    }

    bool TryClaimPass(Data.MissionData m)
    {
        if (!IsDone(m)) return false;
        bool got = false;
        if (!IsClaimed(m))
        {
            Save.Claimed.Add(m.DataId);
            Grant(m.RewardType, m.RewardValue);
            got = true;
        }
        if (PassPremium && m.PremiumValue > 0 && !IsPremiumClaimed(m))
        {
            Save.PremiumClaimed.Add(m.DataId);
            Grant(m.PremiumType, m.PremiumValue);
            got = true;
        }
        if (!got) return false;
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }
    #endregion

    #region 업적·칭호(ADR0040) — 분야(이벤트)마다 단계 사다리, 칭호가 붙은 단계를 받으면 그 칭호를 얻는다
    // 분야 순서 = 표 순서(Order). 한 분야의 받지 않은 첫 단계(다 받았으면 null)
    public List<string> AchieveEvents
    {
        get
        {
            var list = new List<string>();
            foreach (var m in List(ACHIEVE)) if (!list.Contains(m.Event)) list.Add(m.Event);
            return list;
        }
    }

    public Data.MissionData CurrentAchieve(string ev)
    {
        foreach (var m in List(ACHIEVE)) if (m.Event == ev && !IsClaimed(m)) return m;
        return null;
    }

    public Data.MissionData LastAchieve(string ev)
    {
        Data.MissionData last = null;
        foreach (var m in List(ACHIEVE)) if (m.Event == ev) last = m;
        return last;
    }

    public bool HasAchieveClaimable
    {
        get
        {
            foreach (var ev in AchieveEvents)
            {
                var m = CurrentAchieve(ev);
                if (m != null && IsDone(m)) return true;
            }
            return false;
        }
    }

    // 얻은 칭호(받은 단계 순) · 전체 수 · 달고 있는 칭호(얻지 못한 키가 세이브에 있으면 무시)
    public List<string> Honors
    {
        get
        {
            var list = new List<string>();
            foreach (var m in List(ACHIEVE)) if (!string.IsNullOrEmpty(m.HonorKey) && IsClaimed(m)) list.Add(m.HonorKey);
            return list;
        }
    }
    public int HonorTotal
    {
        get
        {
            int n = 0;
            foreach (var m in List(ACHIEVE)) if (!string.IsNullOrEmpty(m.HonorKey)) n++;
            return n;
        }
    }
    public string EquippedHonorKey => !string.IsNullOrEmpty(Save.Honor) && Honors.Contains(Save.Honor) ? Save.Honor : "";

    // '모두 받기' 동안 얻은 칭호 — 알림 다섯 개가 창을 덮지 않게 끝에 한 번('[사냥꾼] 외 4개')
    List<string> _batchHonors;
    public void BeginClaimBatch() => _batchHonors = new List<string>();
    public void EndClaimBatch()
    {
        var got = _batchHonors;
        _batchHonors = null;
        if (got == null || got.Count == 0) return;
        string first = Util.Localize(got[0]);
        Managers.UI?.ShowToast(got.Count == 1 ? string.Format(Util.Localize("HonorGetFormat"), first)
                                              : string.Format(Util.Localize("HonorGetManyFormat"), first, got.Count - 1));
    }

    // 없음 → 첫 칭호 → … → 마지막 → 없음
    public void CycleHonor()
    {
        var list = Honors;
        if (list.Count == 0) return;
        int i = list.IndexOf(EquippedHonorKey);
        Save.Honor = i + 1 < list.Count ? list[i + 1] : "";
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
    }
    #endregion

    #region 출석 — 하루 한 번, 7칸을 차례로 돈다(빠진 날이 있어도 다음 칸부터 이어서 받는다)
    public int  AttendanceIndex => Save.AttendanceIndex;
    public bool CanAttend { get { RollPeriods(); return Save.AttendanceKey != TodayKey; } }

    public Data.MissionData AttendanceAt(int index)
    {
        foreach (var m in All) if (m.Kind == ATTENDANCE && m.Order == index + 1) return m;
        return null;
    }

    public bool TryAttend()
    {
        if (!CanAttend) return false;
        var m = AttendanceAt(Save.AttendanceIndex);
        if (m == null) return false;
        Grant(m.RewardType, m.RewardValue);
        Save.AttendanceIndex = (Save.AttendanceIndex + 1) % ATTENDANCE_DAYS;
        Save.AttendanceKey   = TodayKey;
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
        return true;
    }
    #endregion

    // HUD 미션 버튼의 알림 표시 — 받을 것이 하나라도 있으면 true
    public bool HasClaimable
    {
        get
        {
            if (Managers.Data?.MissionDic == null || Managers.Game == null) return false;
            if (CanAttend) return true;
            if (HasPassClaimable || HasAchieveClaimable) return true;
            foreach (var m in List(DAILY))  if (IsDone(m) && !IsClaimed(m)) return true;
            foreach (var m in List(WEEKLY)) if (IsDone(m) && !IsClaimed(m)) return true;
            var guide = CurrentGuide;
            return guide != null && IsDone(guide);
        }
    }

    int ClaimedDailyCount()
    {
        int n = 0;
        foreach (var m in All)
            if (m.Kind == DAILY && m.Event != nameof(MissionEvent.DailyChest) && Save.Claimed.Contains(m.DataId)) n++;
        return n;
    }

    #region 주기 — 기기 현지 날짜 기준
    public static int DayKey(DateTime t) => t.Year * 10000 + t.Month * 100 + t.Day;
    static int TodayKey => DayKey(DateTime.Now);
    static int WeekKey(DateTime t) => DayKey(t.Date.AddDays(-(((int)t.DayOfWeek + 6) % 7)));   // 그 주 월요일

    void RollPeriods()
    {
        var s = Save;
        if (s == null || Managers.Data?.MissionDic == null) return;
        int today = TodayKey, week = WeekKey(DateTime.Now);
        bool changed = false;
        if (s.DailyKey != today)  { ClearKind(DAILY);  s.DailyKey = today; changed = true; }
        if (s.WeeklyKey != week)  { ClearKind(WEEKLY); s.WeeklyKey = week; changed = true; }
        if (!changed) return;
        Managers.Game.SaveGame();
        OnChanged?.Invoke();
    }

    void ClearKind(string kind)
    {
        foreach (var m in All)
        {
            if (m.Kind != kind) continue;
            Save.Progress.Remove(m.DataId);
            Save.Claimed.Remove(m.DataId);
        }
    }
    #endregion
}
