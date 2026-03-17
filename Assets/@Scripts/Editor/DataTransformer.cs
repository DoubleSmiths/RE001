using Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

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
        ParseScriptData();
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

        var rows = ReadCsv($"{Application.dataPath}{EXCEL_PATH}/PlayerStatusData.csv");

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
    // ScriptData.csv
    //   Row 0: 영문 필드명 (헤더)
    //   Row 1+: 실제 데이터
    // -------------------------------------------------------------------
    static void ParseScriptData()
    {
        ScriptDataLoader loader = new ScriptDataLoader();

        var rows = ReadCsv($"{Application.dataPath}{EXCEL_PATH}/ScriptData.csv");

        for (int y = 1; y < rows.Count; y++)
        {
            List<string> row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
                continue;

            int i = 0;
            ScriptData data = new ScriptData();
            data.DataId = ConvertValue<int>(row[i++]);
            data.Kr     = i < row.Count ? ConvertValue<string>(row[i++]) : "";
            data.En     = i < row.Count ? ConvertValue<string>(row[i++]) : "";
            data.Jp     = i < row.Count ? ConvertValue<string>(row[i++]) : "";
            data.Ch     = i < row.Count ? ConvertValue<string>(row[i++]) : "";

            loader.scripts.Add(data);
        }

        WriteJson("ScriptData", loader);
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
}
