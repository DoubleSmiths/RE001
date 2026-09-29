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
        if (m.Kind == GUIDE)
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
        if (IsClaimed(m) || !IsDone(m)) return false;
        if (m.Kind == GUIDE && m != CurrentGuide) return false;   // 사다리는 순서대로

        Save.Claimed.Add(m.DataId);
        Grant(m.RewardType, m.RewardValue);
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
