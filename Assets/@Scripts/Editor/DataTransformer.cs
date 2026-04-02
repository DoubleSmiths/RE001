using Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using System.Data;
using static Define;

public class DataTransformer : EditorWindow
{
    const string EXCEL_PATH = "/@Resources/Data/Excel";
    const string JSON_PATH  = "/@Resources/Data/JsonData";

    [MenuItem("Tools/DeleteGameData ")]
    public static void DeleteGameData()
    {
        PlayerPrefs.DeleteAll();
        string path = Application.persistentDataPath + "/SaveData.json";
        if (File.Exists(path))
            File.Delete(path);
    }

    [MenuItem("Tools/ParseExcel %#K")]
    public static void ParseExcel()
    {
        ParsePlayerStatusData();
        ParseDialogueScriptData();
        ParseLocalizationData();
        ParseItemData();
        ParseQuestData();
        ParseCreatureData();
        AssetDatabase.Refresh();
        Debug.Log("[DataTransformer] ParseExcel 완료");
    }

    // -------------------------------------------------------------------
    // PlayerStatusData.csv
    //   Row 0: 영문 필드명 (헤더)
    //   Row 1: 타입
    //   Row 2: 한글 설명
    //   Row 3+: 실제 데이터
    // -------------------------------------------------------------------
    static void ParsePlayerStatusData()
    {
        PlayerStatusDataLoader loader = new PlayerStatusDataLoader();

        var rows = ReadXlsx($"{Application.dataPath}{EXCEL_PATH}/PlayerStatusData.xlsx");

        for (int y = 3; y < rows.Count; y++)
        {
            List<string> row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
                continue;

            int i = 0;
            PlayerStatusData data = new PlayerStatusData();
            data.DataId           = ConvertValue<int>(row[i++]);
            data.Class            = ConvertValue<string>(row[i++]);
            data.ClassProficiency = ConvertValue<int>(row[i++]);
            data.AdvRank          = ConvertValue<int>(row[i++]);
            data.Level            = ConvertValue<int>(row[i++]);
            data.NowExp           = ConvertValue<int>(row[i++]);
            data.StartHP          = ConvertValue<int>(row[i++]);
            data.StatusPoint      = ConvertValue<int>(row[i++]);
            data.Elixir           = ConvertValue<int>(row[i++]);
            data.StartStrength    = ConvertValue<int>(row[i++]);
            data.StartVital       = ConvertValue<int>(row[i++]);
            data.StartAgility     = ConvertValue<int>(row[i++]);
            data.StartMagic       = ConvertValue<int>(row[i++]);

            // StartSkill / StartSkill_Lv: 셀 내부 줄바꿈으로 구분된 다중값
            string skillRaw   = i < row.Count ? row[i++] : "";
            string skillLvRaw = i < row.Count ? row[i++] : "";
            data.StartSkills      = SplitLines(skillRaw).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
            data.StartSkillLevels = SplitLines(skillLvRaw).Select(s => s.Trim()).Where(s => s.Length > 0).Select(s => ConvertValue<int>(s)).ToList();

            loader.playerStatuses.Add(data);
        }

        WriteJson("PlayerStatusData", loader);
    }

    // -------------------------------------------------------------------
    // DialogueData001.xlsx
    //   시트별: Row 0 = 헤더(TextKey, Kor, Eng), Row 1+ = 데이터
    //   시트명이 대화 그룹 키가 됨 (예: GuildQuest1Text)
    // -------------------------------------------------------------------
    static void ParseDialogueScriptData()
    {
        DialogueScriptLoader loader = new DialogueScriptLoader();

        string path = $"{Application.dataPath}{EXCEL_PATH}/DialogueData.xlsx";
        var sheets = ReadAllXlsxSheets(path);

        foreach (var (sheetName, rows) in sheets)
        {
            var group = new DialogueScriptGroup { GroupKey = sheetName };

            for (int y = 1; y < rows.Count; y++)  // Row 0 = 헤더 스킵
            {
                List<string> row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
                    continue;

                group.Lines.Add(new DialogueScriptLine
                {
                    TextKey = ConvertValue<int>(row[0]),
                    Kor     = row.Count > 1 ? row[1] : "",
                    Eng     = row.Count > 2 ? row[2] : "",
                });
            }

            loader.groups.Add(group);
        }

        WriteJson("DialogueScriptData", loader);
    }

    // -------------------------------------------------------------------
    // LocalizationData001.xlsx
    //   35개 시트 전체를 병합 → TextKey(string) 기준 단일 딕셔너리
    //   Row 0 = 헤더(TextKey, Kor, Eng), Row 1+ = 데이터
    // -------------------------------------------------------------------
    static void ParseLocalizationData()
    {
        LocalizationLoader loader = new LocalizationLoader();

        string path = $"{Application.dataPath}{EXCEL_PATH}/LocalizationData.xlsx";
        var sheets = ReadAllXlsxSheets(path);

        foreach (var (sheetName, rows) in sheets)
        {
            for (int y = 1; y < rows.Count; y++)  // Row 0 = 헤더 스킵
            {
                List<string> row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
                    continue;

                loader.entries.Add(new LocalizationEntry
                {
                    TextKey = row[0],
                    Kor     = row.Count > 1 ? row[1] : "",
                    Eng     = row.Count > 2 ? row[2] : "",
                });
            }
        }

        WriteJson("LocalizationData", loader);
    }

    // -------------------------------------------------------------------
    // QuestDataSheet.xlsx
    //   시트: Quest | Quest_Condition | Quest_Notify | Quest_Unlock
    //   Row 0: 헤더, Row 1+: 데이터. 첫 컬럼 비어있으면 스킵.
    //   _로 시작하는 시트(_Ref 등)는 무시.
    // -------------------------------------------------------------------
    static void ParseQuestData()
    {
        string path = $"{Application.dataPath}{EXCEL_PATH}/QuestData.xlsx";
        var sheets = ReadAllXlsxSheets(path);

        var questLoader     = new QuestDataLoader();
        var conditionLoader = new QuestConditionLoader();
        var notifyLoader    = new QuestNotifyLoader();
        var unlockLoader    = new QuestUnlockLoader();

        foreach (var (sheetName, rows) in sheets)
        {
            if (sheetName.StartsWith("_")) continue;

            for (int y = 1; y < rows.Count; y++)
            {
                var row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;

                switch (sheetName)
                {
                    case "Quest":
                    {
                        int i = 0;
                        var d = new QuestData();
                        d.QuestID        = ConvertValue<int>(row[i++]);
                        d.QuestName      = ConvertValue<string>(row[i++]);
                        d.CompleteType   = ConvertValue<QuestCompleteType>(row[i++]);
                        d.CompleteTarget = i < row.Count ? row[i++] : "";
                        d.CompleteCount  = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.FollowItemID   = i < row.Count ? row[i++] : "";
                        d.FollowItemName = i < row.Count ? row[i++] : "";
                        d.FollowCount    = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.AnimationID    = i < row.Count ? row[i++] : "";
                        d.AnimBlocking   = i < row.Count && row[i++].ToUpper() == "TRUE";
                        d.DialogueID     = i < row.Count ? row[i++] : "";
                        questLoader.quests.Add(d);
                        break;
                    }
                    case "Quest_Condition":
                    {
                        int i = 0;
                        var d = new QuestConditionData();
                        d.QuestID       = ConvertValue<int>(row[i++]);
                        d.Order         = ConvertValue<int>(row[i++]);
                        d.ConditionType = ConvertValue<QuestConditionType>(row[i++]);
                        d.Target        = i < row.Count ? row[i++] : "";
                        d.Count         = 0;
                        if (d.ConditionType == QuestConditionType.ITEM_COUNT && d.Target.Contains(":"))
                        {
                            var parts = d.Target.Split(':');
                            d.Target = parts[0];
                            d.Count  = ConvertValue<int>(parts[1]);
                        }
                        conditionLoader.conditions.Add(d);
                        break;
                    }
                    case "Quest_Notify":
                    {
                        int i = 0;
                        var d = new QuestNotifyData();
                        d.QuestID = ConvertValue<int>(row[i++]);
                        d.Order   = ConvertValue<int>(row[i++]);
                        d.BtnID   = i < row.Count ? row[i++] : "";
                        notifyLoader.notifies.Add(d);
                        break;
                    }
                    case "Quest_Unlock":
                    {
                        int i = 0;
                        var d = new QuestUnlockData();
                        d.QuestID    = ConvertValue<int>(row[i++]);
                        d.UnlockType = ConvertValue<QuestUnlockType>(row[i++]);
                        d.TargetID   = i < row.Count ? row[i++] : "";
                        unlockLoader.unlocks.Add(d);
                        break;
                    }
                }
            }
        }

        WriteJson("QuestData",          questLoader);
        WriteJson("QuestConditionData", conditionLoader);
        WriteJson("QuestNotifyData",    notifyLoader);
        WriteJson("QuestUnlockData",    unlockLoader);
    }

    // -------------------------------------------------------------------
    // 공용 헬퍼
    // -------------------------------------------------------------------

    /// <summary>RFC 4180을 준수하는 CSV 전체 파싱 (따옴표 내 줄바꿈 지원)</summary>
    static List<List<string>> ReadCsv(string filePath)
    {
        string text = File.ReadAllText(filePath);
        var rows  = new List<List<string>>();
        var row   = new List<string>();
        var field = new StringBuilder();
        bool inQuote = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (inQuote)
            {
                if (c == '"')
                {
                    // 이스케이프된 따옴표 ""
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuote = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuote = true;
                }
                else if (c == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\n')
                {
                    // \r\n 대응: field 끝의 \r 제거
                    string f = field.ToString();
                    if (f.Length > 0 && f[f.Length - 1] == '\r')
                        f = f.Substring(0, f.Length - 1);
                    row.Add(f);
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else if (c != '\r')
                {
                    field.Append(c);
                }
            }
        }

        // 마지막 행 처리
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }

    /// <summary>xlsx의 모든 시트를 읽어 (시트명, 행 목록) 리스트로 반환</summary>
    static List<(string name, List<List<string>> rows)> ReadAllXlsxSheets(string filePath)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var result = new List<(string, List<List<string>>)>();

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
            });

            foreach (DataTable table in dataSet.Tables)
            {
                var rows = new List<List<string>>();
                foreach (DataRow dataRow in table.Rows)
                {
                    var row = new List<string>();
                    foreach (var cell in dataRow.ItemArray)
                        row.Add(cell?.ToString() ?? "");
                    rows.Add(row);
                }
                result.Add((table.TableName, rows));
            }
        }

        return result;
    }

    /// <summary>xlsx 파일을 읽어 List<List<string>> 형태로 반환</summary>
    /// <param name="filePath">xlsx 파일 절대 경로</param>
    /// <param name="sheetIndex">읽을 시트 인덱스 (기본값 0 = 첫 번째 시트)</param>
    static List<List<string>> ReadXlsx(string filePath, int sheetIndex = 0)
    {
        // Windows에서 코드 페이지 인코딩 등록 (ExcelDataReader 필수)
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var rows = new List<List<string>>();

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false  // 헤더를 코드에서 직접 건너뜀 (기존 CSV 방식 유지)
                }
            });

            if (sheetIndex >= dataSet.Tables.Count)
            {
                Debug.LogError($"[DataTransformer] 시트 인덱스 {sheetIndex} 가 존재하지 않습니다.");
                return rows;
            }

            DataTable table = dataSet.Tables[sheetIndex];

            foreach (DataRow dataRow in table.Rows)
            {
                var row = new List<string>();
                foreach (var cell in dataRow.ItemArray)
                    row.Add(cell?.ToString() ?? "");
                rows.Add(row);
            }
        }

        return rows;
    }
    static List<string> SplitLines(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new List<string>();
        return value.Split('\n').Select(s => s.TrimEnd('\r')).ToList();
    }

    static void WriteJson<T>(string filename, T loader)
    {
        string dir = $"{Application.dataPath}{JSON_PATH}";
        Directory.CreateDirectory(dir);
        string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
        File.WriteAllText($"{dir}/{filename}.json", jsonStr, Encoding.UTF8);
        Debug.Log($"[DataTransformer] {filename}.json 생성 완료");
    }

    public static T ConvertValue<T>(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default(T);

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
        return (T)converter.ConvertFromString(value);
    }

    public static List<T> ConvertList<T>(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new List<T>();

        return value.Split('&').Select(x => ConvertValue<T>(x)).ToList();
    }

    // -------------------------------------------------------------------
    // CreatureData.xlsx
    //   Row 0: 영문 필드명, Row 1: 타입, Row 2: 한글 설명, Row 3+: 데이터
    //   첫 컬럼 비어있으면 스킵.
    // -------------------------------------------------------------------
    static void ParseCreatureData()
    {
        CreatureDataLoader loader = new CreatureDataLoader();

        var rows = ReadXlsx($"{Application.dataPath}{EXCEL_PATH}/CreatureData.xlsx");

        for (int y = 3; y < rows.Count; y++)
        {
            List<string> row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
                continue;

            int i = 0;
            CreatureData d = new CreatureData();
            d.DataId      = ConvertValue<int>(row[i++]);
            d.Name        = i < row.Count ? row[i++] : "";
            d.PrefabLabel = i < row.Count ? row[i++] : "";
            d.IconLabel   = i < row.Count ? row[i++] : "";
            d.MaxHp       = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.Atk         = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.Def         = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.MoveSpeed   = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.TotalExp    = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.RewardGold  = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
            d.DropItemId  = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
            d.DropCount   = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
            d.DropChance  = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;

            loader.creatures.Add(d);
        }

        WriteJson("CreatureData", loader);
    }

    // -------------------------------------------------------------------
    // ItemData.xlsx
    //   Row 0: 헤더 (DataId, Name, Grade, Description, SpriteName, FoundLocation)
    //   Row 1+: 데이터. 첫 컬럼 비어있으면 스킵.
    // -------------------------------------------------------------------
    static void ParseItemData()
    {
        ItemDataLoader loader = new ItemDataLoader();

        var rows = ReadXlsx($"{Application.dataPath}{EXCEL_PATH}/ItemData.xlsx");

        for (int y = 1; y < rows.Count; y++)
        {
            List<string> row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
                continue;

            int i = 0;
            ItemData d = new ItemData();
            d.DataId        = ConvertValue<int>(row[i++]);
            d.Name          = i < row.Count ? row[i++] : "";
            d.Grade         = i < row.Count ? ConvertValue<EquipGrade>(row[i++]) : EquipGrade.Common;
            d.Description   = i < row.Count ? row[i++] : "";
            d.SpriteName    = i < row.Count ? row[i++] : "";
            d.FoundLocation = i < row.Count ? row[i++] : "";

            loader.items.Add(d);
        }

        WriteJson("ItemData", loader);
    }
}
