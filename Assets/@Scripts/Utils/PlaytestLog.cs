using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

// 비공개 테스트용 기기 안 플레이 기록(ADR0046) — 서버·SDK 없이 persistentDataPath/playtest_log.txt 에 한 줄씩 남긴다.
//  줄 = 시각(UTC) \t 누적 플레이(초) \t 사건 \t 내용. 사건: session_start/pause/resume/end, level, stage_clear(첫 돌파),
//  boss_fail, death, quest, ad, summon, iap(모의), auto_unlock. 파일이 MAX_BYTES 를 넘으면 앞 절반을 버린다.
//  설정 → [플레이 기록] 이 Summary() 를 보여 주고 클립보드로 복사한다 — 테스터가 설문지에 붙여 넣는다(Documents/Playtest).
//  기록은 기기 밖으로 나가지 않는다(수집이 아니다 — 스토어 데이터 보안 답은 그대로).
public static class PlaytestLog
{
    const long MAX_BYTES = 512 * 1024;
    const string PLAYTIME_KEY = "PLAYTEST_SECONDS";
    const int RECENT_LINES = 15;
    static readonly int[] LEVEL_MARKS = { 5, 10, 20, 30, 50, 100, 150, 200, 300 };

    static float s_sessionStart = -1f;   // 지금 세션이 시작한 realtimeSinceStartup(멈춤·시작 전이면 -1)
    static bool  s_started;              // 게임 씬에 한 번이라도 들어왔나 — 켜자마자 오는 '재개'를 세지 않게

    static string FilePath => Path.Combine(Application.persistentDataPath, "playtest_log.txt");

    public static double PlaySeconds
        => PlayerPrefs.GetFloat(PLAYTIME_KEY, 0f) + (s_sessionStart >= 0f ? Time.realtimeSinceStartup - s_sessionStart : 0f);

    public static void Event(string name, string data = "")
    {
        try
        {
            string line = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\t{(int)PlaySeconds}\t{name}\t{data?.Replace('\t', ' ').Replace('\n', ' ')}\n";
            var info = new FileInfo(FilePath);
            if (info.Exists && info.Length > MAX_BYTES)
            {
                var keep = File.ReadAllLines(FilePath);
                File.WriteAllLines(FilePath, keep.Skip(keep.Length / 2));
            }
            File.AppendAllText(FilePath, line);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PlaytestLog] 기록 실패: " + e.Message);   // 기록은 부가 기능 — 게임을 멈추지 않는다
        }
    }

    public static void SessionStart(string data)
    {
        if (s_sessionStart < 0f) s_sessionStart = Time.realtimeSinceStartup;
        s_started = true;
        Event("session_start", data);
    }

    public static void SessionPause()
    {
        if (s_sessionStart < 0f) return;
        Event("session_pause");
        Commit();
    }

    public static void SessionResume()
    {
        if (s_sessionStart >= 0f || !s_started) return;
        s_sessionStart = Time.realtimeSinceStartup;
        Event("session_resume");
    }

    public static void SessionEnd()
    {
        if (s_sessionStart < 0f) return;
        Event("session_end");
        Commit();
    }

    // 지금까지의 세션 시간을 누적(PlayerPrefs)에 넣고 세션을 닫는다
    static void Commit()
    {
        PlayerPrefs.SetFloat(PLAYTIME_KEY, (float)PlaySeconds);
        PlayerPrefs.Save();
        s_sessionStart = -1f;
    }

    struct Row { public int Play; public string Name, Data, Raw; }

    // 설문지에 붙여 넣을 요약 — 총 시간·세션, 레벨 도달 시각, 돌파·막힌 곳·쓰러짐, 광고·소환·구매, 최근 기록
    public static string Summary()
    {
        var rows = new List<Row>();
        try
        {
            if (File.Exists(FilePath))
                foreach (var raw in File.ReadAllLines(FilePath))
                {
                    var p = raw.Split('\t');
                    if (p.Length < 3) continue;
                    int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int play);
                    rows.Add(new Row { Play = play, Name = p[2], Data = p.Length > 3 ? p[3] : "", Raw = raw });
                }
        }
        catch (Exception e)
        {
            return "[PlaytestLog] " + e.Message;
        }

        var sb = new StringBuilder();
        double total = PlaySeconds;
        int sessions = Math.Max(1, rows.Count(r => r.Name == "session_start" || r.Name == "session_resume"));
        sb.AppendLine($"v{Application.version} · {SystemInfo.deviceModel} · {Managers.Game?.Language}");
        sb.AppendLine(string.Format(Util.Localize("PlaytestTotalFormat"), Clock(total), sessions, Clock(total / sessions)));

        var marks = new List<string>();
        foreach (int lv in LEVEL_MARKS)
        {
            var hit = rows.FirstOrDefault(r => r.Name == "level" && int.TryParse(r.Data, out int v) && v >= lv);
            if (hit.Name != null) marks.Add(string.Format(Util.Localize("PlaytestLevelFormat"), lv, Clock(hit.Play)));
        }
        if (marks.Count > 0) sb.AppendLine(string.Join(" · ", marks));

        var clears = rows.Where(r => r.Name == "stage_clear").ToList();
        var fails = rows.Where(r => r.Name == "boss_fail").ToList();
        sb.AppendLine(string.Format(Util.Localize("PlaytestStageFormat"), clears.Count, clears.Count > 0 ? clears[clears.Count - 1].Data : "-",
                                    fails.Count, rows.Count(r => r.Name == "death")));
        var hardest = fails.GroupBy(r => r.Data).OrderByDescending(g => g.Count()).Take(3).Select(g => $"{g.Key}×{g.Count()}").ToList();
        if (hardest.Count > 0) sb.AppendLine(string.Format(Util.Localize("PlaytestHardestFormat"), string.Join(", ", hardest)));
        sb.AppendLine(string.Format(Util.Localize("PlaytestEconomyFormat"), rows.Count(r => r.Name == "ad"), rows.Count(r => r.Name == "summon"), rows.Count(r => r.Name == "iap")));

        sb.AppendLine();
        sb.AppendLine(Util.Localize("PlaytestRecentText"));
        foreach (var r in rows.Skip(Math.Max(0, rows.Count - RECENT_LINES))) sb.AppendLine($"{Clock(r.Play)}  {r.Name} {r.Data}");
        return sb.ToString();
    }

    static string Clock(double seconds)
    {
        int m = (int)(seconds / 60);
        return string.Format(Util.Localize("PlaytestTimeFormat"), m / 60, m % 60);
    }
}
