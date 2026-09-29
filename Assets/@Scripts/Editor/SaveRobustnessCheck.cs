using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 세이브 내구성 검사 — 출시 전에 막은 데이터 손실 경로를 하나씩 다시 밟는다.
///
///   1. PlayerPrefs "ISFIRST" 가 없어도 세이브를 지우지 않는다
///   2. 본 파일이 손상되면 .bak 에서 복구하고, 손상본은 지우지 않고 격리한다
///   3. 로드를 반복해도 ActivationManager 가 늘어나지 않는다(Newtonsoft 병합 버그)
///   4. 세이브 속 장비 데이터 사본이 공유 데이터표(EquipmentDic)를 덮어쓰지 않는다
///   5. SaveGame 은 모아 쓰기(즉시 기록 안 함), FlushSave 가 기록한다
///
/// 플레이 모드 전용. 실행 전 세이브 3종(.json/.bak/.tmp)을 복사해 두고 끝나면 그대로 되돌린다.
/// </summary>
public static class SaveRobustnessCheck
{
    [MenuItem("Test/Integration/Save - 내구성 검사", false, 502)]
    public static void Run() => Debug.Log(RunAndReport());

    public static string RunAndReport()
    {
        if (!Application.isPlaying || Managers.Game == null)
            return "[SaveCheck] 플레이 모드에서 실행하세요.";

        var g = Managers.Game;
        string path = GameManager.SavePath;
        string[] files = { path, path + ".bak", path + ".tmp" };
        var backup = files.ToDictionary(f => f, f => File.Exists(f) ? File.ReadAllBytes(f) : null);
        var corruptBefore = Directory.GetFiles(Path.GetDirectoryName(path), "SaveData.json*.corrupt-*").ToList();
        var fail = new List<string>();

        try
        {
            g.SaveGameNow();
            string good = File.ReadAllText(path);

            // 1. ISFIRST 없음 → 삭제하지 않고 그대로 읽는다
            PlayerPrefs.DeleteKey("ISFIRST");
            if (!g.LoadGame() || !File.Exists(path)) fail.Add("1: ISFIRST 없을 때 세이브가 사라짐");

            // 3. 로드 반복 → 칸 수 고정
            int keys = System.Enum.GetValues(typeof(Define.ActivationKey)).Length;
            for (int i = 0; i < 3; i++) { g.SaveGameNow(); g.LoadGame(); }
            if (g.ActivationManager.Count != keys) fail.Add($"3: ActivationManager {g.ActivationManager.Count}칸 ≠ {keys}");

            // 4. 옛 세이브의 장비 데이터 사본(과장된 값)이 공유 데이터표를 오염시키지 않는다
            var owned = g.OwnedEquipments.FirstOrDefault(e => e.EquipmentData != null);
            if (owned != null)
            {
                int realAtk = owned.EquipmentData.BaseAtk;
                var j = JObject.Parse(File.ReadAllText(path));
                foreach (var e in j["OwnedEquipments"])
                    e["EquipmentData"] = new JObject { ["DataId"] = e["DataId"], ["BaseAtk"] = 999999 };
                File.WriteAllText(path, j.ToString());
                g.LoadGame();
                if (Managers.Data.EquipmentDic[owned.DataId].BaseAtk != realAtk)
                {
                    fail.Add($"4: EquipmentDic 오염 BaseAtk {Managers.Data.EquipmentDic[owned.DataId].BaseAtk} ≠ {realAtk}");
                    Managers.Data.EquipmentDic[owned.DataId].BaseAtk = realAtk;
                }
                g.SaveGameNow();
                if (File.ReadAllText(path).Contains("\"EquipmentData\"")) fail.Add("4: 장비 데이터 사본이 여전히 저장됨");
            }
            else fail.Add("4: 검사할 보유 장비 없음");

            // 2. 본 파일 손상 → .bak 복구 + 격리
            g.SaveGameNow();                          // .bak = 직전 정상본
            g.SaveGameNow();
            File.WriteAllText(path, "{ \"Level\": 12, \"Gold\": ");   // 잘린 JSON
            int levelBefore = g.Level;
            if (!g.LoadGame()) fail.Add("2: 손상 시 복구 실패(LoadGame false)");
            if (g.Level != levelBefore) fail.Add($"2: 복구된 레벨 {g.Level} ≠ {levelBefore}");
            var corruptNow = Directory.GetFiles(Path.GetDirectoryName(path), "SaveData.json*.corrupt-*").Except(corruptBefore).ToList();
            if (corruptNow.Count == 0) fail.Add("2: 손상본이 격리되지 않음");
            foreach (var c in corruptNow) File.Delete(c);   // 검사가 만든 격리본만 정리

            // 5. 모아 쓰기
            g.SaveGameNow();
            var stamp = File.GetLastWriteTimeUtc(path);
            System.Threading.Thread.Sleep(20);
            g.Gold += 0;                                  // 세터 → SaveGame(표시만)
            if (File.GetLastWriteTimeUtc(path) != stamp) fail.Add("5: SaveGame 이 즉시 기록함(모아 쓰기 아님)");
            g.FlushSave();
            if (File.GetLastWriteTimeUtc(path) == stamp) fail.Add("5: FlushSave 가 기록하지 않음");
        }
        catch (System.Exception e)
        {
            fail.Add("예외: " + e);
        }
        finally
        {
            // 원래 파일 그대로 복원 후 메모리도 그 상태로
            foreach (var kv in backup)
            {
                if (kv.Value == null) { if (File.Exists(kv.Key)) File.Delete(kv.Key); }
                else File.WriteAllBytes(kv.Key, kv.Value);
            }
            g.LoadGame();
        }

        return fail.Count == 0
            ? "[SaveCheck] PASS 5/5 (ISFIRST 무관 · 손상 복구·격리 · 병합 없음 · 데이터표 보호 · 모아 쓰기)"
            : "[SaveCheck] FAIL\n  " + string.Join("\n  ", fail);
    }
}
