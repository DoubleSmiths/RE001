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

    // ─── Google Sheets URL ───────────────────────────────────────────────────
    // 구글 시트 → 공유 → "링크 있는 사용자 - 뷰어" 공개 공유 필요.
    // 편집 URL의 /edit?... 부분을 /export?format=xlsx 로 바꿔서 입력.
    // 예) https://docs.google.com/spreadsheets/d/{ID}/export?format=xlsx
    const string GSHEET_PLAYER_STATUS  = "https://docs.google.com/spreadsheets/d/11w2mraXSr6FyI7FH1Gx4W0EdeQxHJEeU/export?format=xlsx";
    const string GSHEET_DIALOGUE       = "https://docs.google.com/spreadsheets/d/10JL5JYylyN-e4d_p6kPb7VvvdQi6ZLYn/export?format=xlsx";
    const string GSHEET_LOCALIZATION   = "https://docs.google.com/spreadsheets/d/1ydclXzn6lWwzlzMNw9lBOPUSDeg4Ats2/export?format=xlsx";
    const string GSHEET_QUEST          = "https://docs.google.com/spreadsheets/d/15AJ-UkoIjEfQjdjmGt54MkUJBbrvSFEk/export?format=xlsx";
    const string GSHEET_CREATURE       = "https://docs.google.com/spreadsheets/d/1p60TBb00k4vpMCQJIcBvUiuqp2yzY4Lh/export?format=xlsx";
    const string GSHEET_ITEM           = "https://docs.google.com/spreadsheets/d/1W5hzYceXWPzKPUG7cSOKwQ_tflHzIr9J/export?format=xlsx";
    const string GSHEET_TRAVEL         = "https://docs.google.com/spreadsheets/d/1tGg8DlSGerZfHrF8uG_UhyqRS1yWv1zw/export?format=xlsx";

    // 추가 마스터데이터 7종 (2026-06-21 신규 구글시트화 — RE001_DataSheets 폴더)
    const string GSHEET_SKILL          = "https://docs.google.com/spreadsheets/d/1MdCPOLzHsrs152wQPYjTDnOVa6d39WDgHxlzQTynrxI/export?format=xlsx";
    const string GSHEET_EQUIPMENT      = "https://docs.google.com/spreadsheets/d/1whc3zc8QbTUPmYmOzuTg6rjT1qHhqT5p6e_5TwLheO8/export?format=xlsx";
    const string GSHEET_EQUIP_ENHANCE  = "https://docs.google.com/spreadsheets/d/1ap5hc5wd9tibBwgdXNugpOKKCDj7UO1zn1f9dXCySeo/export?format=xlsx";
    const string GSHEET_MINMON         = "https://docs.google.com/spreadsheets/d/1EHzVXcUWj91sTpkSWIZ77gHqlxRl5jHKyUdvqM6DOfM/export?format=xlsx";
    const string GSHEET_HP_POTION      = "https://docs.google.com/spreadsheets/d/1pajVmT0PUAvIFbxSHjdjHzW0Gd70X8mxxqgLawTrLA0/export?format=xlsx";
    const string GSHEET_SPEED_POTION   = "https://docs.google.com/spreadsheets/d/1rhlNcgm5_5vILUU4GWFadcdaK3Fu0y1j3WE4MifWakU/export?format=xlsx";
    const string GSHEET_COMBO          = "https://docs.google.com/spreadsheets/d/1VjtZ0xrGKncXn0-dCiYFbVN5NdxL_1gct-VmGVj4GC0/export?format=xlsx";

    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("Tools/ParseExcelFromGoogleSheets")]
    public static void ParseExcelFromGoogleSheets()
    {
        GS_ParsePlayerStatusData();
        GS_ParseDialogueScriptData();
        GS_ParseLocalizationData();
        GS_ParseItemData();
        GS_ParseQuestData();
        GS_ParseCreatureData();
        GS_ParseTravelData();
        AssetDatabase.Refresh();
        Debug.Log("[DataTransformer] ParseExcelFromGoogleSheets 완료");
    }

    // ─── LocalizeKey 자동 생성 ──────────────────────────────────────────────────
    // GSHEET_LOCALIZATION의 모든 시트에서 1열(TextKey)을 읽어
    // Define.cs의 LocalizeKey 클래스를 덮어씁니다.
    [MenuItem("Tools/GenerateLocalizeKeys")]
    public static void GenerateLocalizeKeys()
    {
        var sheets = ReadAllXlsxSheets(DownloadGoogleSheet(GSHEET_LOCALIZATION));

        var sb      = new StringBuilder();
        var allKeys = new HashSet<string>();

        sb.AppendLine("    #region LocalizeKey");
        sb.AppendLine("    // LocalizationData의 TextKey와 1:1 대응 (자동 생성 - Tools/GenerateLocalizeKeys)");
        sb.AppendLine("    public static class LocalizeKey");
        sb.AppendLine("    {");

        foreach (var (sheetName, rows) in sheets)
        {
            var sheetKeys = new List<string>();
            for (int y = 1; y < rows.Count; y++)
            {
                var row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
                string key = row[0].Trim();
                if (allKeys.Add(key))   // 중복 제거
                    sheetKeys.Add(key);
            }
            if (sheetKeys.Count == 0) continue;

            sb.AppendLine($"        // ── {sheetName} ──");
            foreach (string key in sheetKeys)
            {
                // C# 식별자로 사용할 수 없는 문자 치환 (공백·특수문자 → '_')
                string safeName = System.Text.RegularExpressions.Regex.Replace(key, @"[^A-Za-z0-9_]", "_");
                if (char.IsDigit(safeName[0])) safeName = "_" + safeName;
                sb.AppendLine($"        public const string {safeName} = \"{key}\";");
            }
            sb.AppendLine();
        }

        sb.Append("    }");
        sb.AppendLine();
        sb.Append("    #endregion");

        // Define.cs에서 #region LocalizeKey ... #endregion 블록을 교체
        string definePath = $"{Application.dataPath}/@Scripts/Utils/Define.cs";
        string original   = File.ReadAllText(definePath, Encoding.UTF8);

        const string REGION_TAG   = "    #region LocalizeKey";
        const string ENDREGION_TAG = "    #endregion";

        int regionStart = original.IndexOf(REGION_TAG, System.StringComparison.Ordinal);
        if (regionStart < 0)
        {
            Debug.LogError("[GenerateLocalizeKeys] Define.cs에서 '#region LocalizeKey'를 찾을 수 없습니다.");
            return;
        }
        int endregionStart = original.IndexOf(ENDREGION_TAG, regionStart, System.StringComparison.Ordinal);
        if (endregionStart < 0)
        {
            Debug.LogError("[GenerateLocalizeKeys] Define.cs에서 '#endregion'을 찾을 수 없습니다.");
            return;
        }
        int blockEnd = endregionStart + ENDREGION_TAG.Length;

        string newContent = original.Substring(0, regionStart)
                          + sb.ToString()
                          + original.Substring(blockEnd);

        File.WriteAllText(definePath, newContent, Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[GenerateLocalizeKeys] {allKeys.Count}개 키 생성 완료 → Define.LocalizeKey");
    }

    static void GS_ParsePlayerStatusData()
    {
        PlayerStatusDataLoader loader = new PlayerStatusDataLoader();
        var rows = ReadXlsx(DownloadGoogleSheet(GSHEET_PLAYER_STATUS));

        for (int y = 3; y < rows.Count; y++)
        {
            List<string> row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;

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
            data.StartStrength = ConvertValue<int>(row[i++]);
            data.StartVital    = ConvertValue<int>(row[i++]);
            data.StartAgility  = ConvertValue<int>(row[i++]);
            data.StartMagic    = ConvertValue<int>(row[i++]);

            string skillRaw   = i < row.Count ? row[i++] : "";
            string skillLvRaw = i < row.Count ? row[i++] : "";
            data.StartSkills      = SplitLines(skillRaw).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
            data.StartSkillLevels = SplitLines(skillLvRaw).Select(s => s.Trim()).Where(s => s.Length > 0).Select(s => ConvertValue<int>(s)).ToList();

            // 신규 컬럼 (StartSkill_Lv 뒤에 추가)
            data.StartCritRate       = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            data.StartCritDamage     = i < row.Count ? ConvertValue<float>(row[i++]) : 150f;
            data.StartAccuracy       = i < row.Count ? ConvertValue<float>(row[i++]) : 80f;
            data.StartAtkSpeed       = i < row.Count ? ConvertValue<float>(row[i++]) : 100f;
            data.StartPhysicalResist = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            data.StartMagicResist    = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            data.StartEvasion        = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            data.StartRecoveryRate   = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            data.StartGrowthRate     = i < row.Count ? ConvertValue<float>(row[i++]) : 100f;
            data.StartDropRate       = i < row.Count ? ConvertValue<float>(row[i++]) : 100f;

            loader.playerStatuses.Add(data);
        }
        WriteJson("PlayerStatusData", loader);
    }

    static void GS_ParseDialogueScriptData()
    {
        DialogueScriptLoader loader = new DialogueScriptLoader();
        var sheets = ReadAllXlsxSheets(DownloadGoogleSheet(GSHEET_DIALOGUE));

        foreach (var (sheetName, rows) in sheets)
        {
            var group = new DialogueScriptGroup { GroupKey = sheetName };
            for (int y = 1; y < rows.Count; y++)
            {
                List<string> row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
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

    static void GS_ParseLocalizationData()
    {
        LocalizationLoader loader = new LocalizationLoader();
        var sheets = ReadAllXlsxSheets(DownloadGoogleSheet(GSHEET_LOCALIZATION));

        foreach (var (sheetName, rows) in sheets)
        {
            for (int y = 1; y < rows.Count; y++)
            {
                List<string> row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
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

    static void GS_ParseQuestData()
    {
        var sheets = ReadAllXlsxSheets(DownloadGoogleSheet(GSHEET_QUEST));

        var questLoader       = new QuestDataLoader();
        var conditionLoader   = new QuestConditionLoader();
        var notifyLoader      = new QuestNotifyLoader();
        var unlockLoader      = new QuestUnlockLoader();
        var subMainLoader     = new SubQuestDataLoader();
        var subNotifyLoader   = new SubQuestNotifyLoader();
        var subUnlockLoader   = new SubQuestUnlockLoader();

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
                    case "SubQuest_Main":
                    {
                        int i = 0;
                        var d = new SubQuestData();
                        d.SubQuestID   = ConvertValue<int>(row[i++]);
                        d.SubQuestName = i < row.Count ? row[i++] : "";
                        d.CompleteType = ConvertValue<SubQuestCompleteType>(row[i++]);
                        d.CompleteParam = NormalizeNone(i < row.Count ? row[i++] : "");
                        d.DialogueID    = NormalizeNone(i < row.Count ? row[i++] : "");
                        subMainLoader.subQuests.Add(d);
                        break;
                    }
                    case "SubQuest_Notify":
                    {
                        int i = 0;
                        var d = new SubQuestNotifyData();
                        d.SubQuestID = ConvertValue<int>(row[i++]);
                        d.TargetID   = i < row.Count ? row[i++] : "";
                        d.ShowTiming = ConvertValue<SubQuestShowTiming>(i < row.Count ? row[i++] : "ON_START");
                        subNotifyLoader.notifies.Add(d);
                        break;
                    }
                    case "SubQuest_Unlock_Table":
                    {
                        int i = 0;
                        var d = new SubQuestUnlockData();
                        d.SubQuestID  = ConvertValue<int>(row[i++]);
                        d.QuestType   = i < row.Count ? row[i++] : "";
                        d.UnlockType  = ConvertValue<QuestUnlockType>(row[i++]);
                        d.TargetID    = i < row.Count ? row[i++] : "";
                        d.UnlockValue = i < row.Count ? row[i++] : "";
                        subUnlockLoader.unlocks.Add(d);
                        break;
                    }
                }
            }
        }
        WriteJson("QuestData",          questLoader);
        WriteJson("QuestConditionData", conditionLoader);
        WriteJson("QuestNotifyData",    notifyLoader);
        WriteJson("QuestUnlockData",    unlockLoader);
        WriteJson("SubQuestMainData",   subMainLoader);
        WriteJson("SubQuestNotifyData", subNotifyLoader);
        WriteJson("SubQuestUnlockData", subUnlockLoader);
    }

    /// <summary>시트의 "NONE" 값은 빈 문자열로 정규화 (IsNullOrEmpty 체크 통합).</summary>
    static string NormalizeNone(string s)
        => (s ?? "").Trim().Equals("NONE", System.StringComparison.OrdinalIgnoreCase) ? "" : s;

    static void GS_ParseCreatureData()
    {
        CreatureDataLoader loader = new CreatureDataLoader();
        var rows = ReadXlsx(DownloadGoogleSheet(GSHEET_CREATURE));

        for (int y = 3; y < rows.Count; y++)
        {
            List<string> row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;

            int i = 0;
            CreatureData d = new CreatureData();
            d.DataId      = ConvertValue<int>(row[i++]);
            d.Name        = i < row.Count ? row[i++] : "";
            d.PrefabLabel = i < row.Count ? row[i++] : "";
            d.IconLabel   = i < row.Count ? row[i++] : "";
            // 기존 컬럼 순서 유지 (Atk → PhysicalAtk 헤더 변경만)
            d.MaxHp       = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.PhysicalAtk = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.Def         = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.MoveSpeed   = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.TotalExp    = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.RewardGold  = i < row.Count ? ConvertValue<int>(row[i++])   : 0;
            d.DropItemId  = i < row.Count ? ConvertValue<int>(row[i++])   : 0;
            d.DropCount   = i < row.Count ? ConvertValue<int>(row[i++])   : 0;
            d.DropChance  = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            // 신규 컬럼 (뒤에 추가)
            d.MagicAtk       = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.CritRate       = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.CritDamage     = i < row.Count ? ConvertValue<float>(row[i++]) : 150f;
            d.Accuracy       = i < row.Count ? ConvertValue<float>(row[i++]) : 80f;
            d.AtkSpeed       = i < row.Count ? ConvertValue<float>(row[i++]) : 100f;
            d.PhysicalResist = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.MagicResist    = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.Evasion        = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.RecoveryRate   = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            // 슬라이드 79: 미니몬 고유기술 (Excel에 컬럼 추가 전엔 빈 값)
            d.UniqueSkillName = i < row.Count ? row[i++] : "";
            d.UniqueSkillDesc = i < row.Count ? row[i++] : "";
            loader.creatures.Add(d);
        }
        WriteJson("CreatureData", loader);
    }

    static void GS_ParseItemData()
    {
        var sheets = ReadAllXlsxSheets(DownloadGoogleSheet(GSHEET_ITEM));

        foreach (var (sheetName, rows) in sheets)
        {
            if (sheetName.StartsWith("_")) continue;

            switch (sheetName)
            {
                case "InventoryItem":
                case "RequisiteItem":
                case "Equipment":
                {
                    var loader = new ItemDataLoader();
                    for (int y = 1; y < rows.Count; y++)
                    {
                        var row = rows[y];
                        if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
                        int i = 0;
                        var d = new ItemData();
                        d.DataId        = ConvertValue<int>(row[i++]);
                        d.Name          = i < row.Count ? row[i++] : "";
                        d.Grade         = i < row.Count ? ConvertValue<EquipGrade>(row[i++]) : EquipGrade.Basic;
                        d.Description   = i < row.Count ? row[i++] : "";
                        d.SpriteName    = i < row.Count ? row[i++] : "";
                        d.FoundLocation = i < row.Count ? row[i++] : "";
                        loader.items.Add(d);
                    }
                    WriteJson(sheetName, loader);
                    break;
                }
                case "HPPotion":
                {
                    var loader = new HPPotionDataLoader();
                    for (int y = 1; y < rows.Count; y++)
                    {
                        var row = rows[y];
                        if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
                        int i = 0;
                        var d = new HPPotionData();
                        d.DataId        = ConvertValue<int>(row[i++]);
                        d.Name          = i < row.Count ? row[i++] : "";
                        d.Grade         = i < row.Count ? ConvertValue<EquipGrade>(row[i++]) : EquipGrade.Basic;
                        d.Description   = i < row.Count ? row[i++] : "";
                        d.SpriteName    = i < row.Count ? row[i++] : "";
                        d.WhereTo       = i < row.Count ? row[i++] : "";
                        d.Step          = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.FlatRecov     = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.MaxHPRecov    = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.Cooldown      = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.NeededItemIDs     = i < row.Count ? ParseIntList(row[i++]) : new List<int>();
                        d.NeededItemAmounts = i < row.Count ? ParseIntList(row[i++]) : new List<int>();
                        loader.potions.Add(d);
                    }
                    WriteJson(sheetName, loader);
                    break;
                }
                case "SpeedPotion":
                {
                    var loader = new SpeedPotionDataLoader();
                    for (int y = 1; y < rows.Count; y++)
                    {
                        var row = rows[y];
                        if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
                        int i = 0;
                        var d = new SpeedPotionData();
                        d.DataId        = ConvertValue<int>(row[i++]);
                        d.Name          = i < row.Count ? row[i++] : "";
                        d.Grade         = i < row.Count ? ConvertValue<EquipGrade>(row[i++]) : EquipGrade.Basic;
                        d.Description   = i < row.Count ? row[i++] : "";
                        d.SpriteName    = i < row.Count ? row[i++] : "";
                        d.WhereTo       = i < row.Count ? row[i++] : "";
                        d.Step          = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.AtkSpeedBonus = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
                        d.Duration      = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.Cooldown      = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.NeededItemIDs     = i < row.Count ? ParseIntList(row[i++]) : new List<int>();
                        d.NeededItemAmounts = i < row.Count ? ParseIntList(row[i++]) : new List<int>();
                        loader.potions.Add(d);
                    }
                    WriteJson(sheetName, loader);
                    break;
                }
                default:
                    Debug.LogWarning($"[DataTransformer] ItemData 시트 '{sheetName}' 는 파서가 없습니다.");
                    break;
            }
        }
    }

    static void GS_ParseTravelData()
    {
        var sheets = ReadAllXlsxSheets(DownloadGoogleSheet(GSHEET_TRAVEL));
        ParseTravelSheets(sheets);
    }

    static void ParseTravelSheets(List<(string name, List<List<string>> rows)> sheets)
    {
        var nodeLoader      = new TravelNodeDataLoader();
        var conditionLoader = new TravelConditionDataLoader();

        foreach (var (sheetName, rows) in sheets)
        {
            if (sheetName.StartsWith("_")) continue;

            for (int y = 1; y < rows.Count; y++)
            {
                var row = rows[y];
                if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;

                switch (sheetName)
                {
                    case "TravelNode":
                    {
                        int i = 0;
                        var d = new TravelNodeData();
                        d.Id         = ConvertValue<int>(row[i++]);
                        d.ParentId   = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.Category   = i < row.Count ? row[i++] : "";
                        d.Name       = i < row.Count ? row[i++] : "";
                        d.Sprite     = i < row.Count ? row[i++] : "";
                        d.AreaType   = i < row.Count ? row[i++] : "";
                        d.ClickBtnId = i < row.Count ? row[i++] : "";
                        d.EnterBtnId = i < row.Count ? row[i++] : "";
                        nodeLoader.nodes.Add(d);
                        break;
                    }
                    case "TravelCondition":
                    {
                        int i = 0;
                        var d = new TravelConditionData();
                        d.NodeId      = ConvertValue<int>(row[i++]);
                        d.Type        = i < row.Count ? row[i++] : "";
                        d.Target      = i < row.Count ? row[i++] : "";
                        d.Value       = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
                        d.FailMessage = i < row.Count ? row[i++] : "";
                        conditionLoader.conditions.Add(d);
                        break;
                    }
                }
            }
        }

        WriteJson("TravelNodeData", nodeLoader);
        WriteJson("TravelConditionData", conditionLoader);
    }

    /// <summary>"NONE" 또는 빈 값은 빈 리스트, 쉼표 구분 정수 목록 파싱</summary>
    static List<int> ParseIntList(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Equals("NONE", System.StringComparison.OrdinalIgnoreCase))
            return new List<int>();
        return value.Split(',').Select(s => ConvertValue<int>(s.Trim())).ToList();
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
        ParseTravelData();
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

        var questLoader       = new QuestDataLoader();
        var conditionLoader   = new QuestConditionLoader();
        var notifyLoader      = new QuestNotifyLoader();
        var unlockLoader      = new QuestUnlockLoader();
        var subMainLoader     = new SubQuestDataLoader();
        var subNotifyLoader   = new SubQuestNotifyLoader();
        var subUnlockLoader   = new SubQuestUnlockLoader();

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
                    case "SubQuest_Main":
                    {
                        int i = 0;
                        var d = new SubQuestData();
                        d.SubQuestID    = ConvertValue<int>(row[i++]);
                        d.SubQuestName  = i < row.Count ? row[i++] : "";
                        d.CompleteType  = ConvertValue<SubQuestCompleteType>(row[i++]);
                        d.CompleteParam = NormalizeNone(i < row.Count ? row[i++] : "");
                        d.DialogueID    = NormalizeNone(i < row.Count ? row[i++] : "");
                        subMainLoader.subQuests.Add(d);
                        break;
                    }
                    case "SubQuest_Notify":
                    {
                        int i = 0;
                        var d = new SubQuestNotifyData();
                        d.SubQuestID = ConvertValue<int>(row[i++]);
                        d.TargetID   = i < row.Count ? row[i++] : "";
                        d.ShowTiming = ConvertValue<SubQuestShowTiming>(i < row.Count ? row[i++] : "ON_START");
                        subNotifyLoader.notifies.Add(d);
                        break;
                    }
                    case "SubQuest_Unlock_Table":
                    {
                        int i = 0;
                        var d = new SubQuestUnlockData();
                        d.SubQuestID  = ConvertValue<int>(row[i++]);
                        d.QuestType   = i < row.Count ? row[i++] : "";
                        d.UnlockType  = ConvertValue<QuestUnlockType>(row[i++]);
                        d.TargetID    = i < row.Count ? row[i++] : "";
                        d.UnlockValue = i < row.Count ? row[i++] : "";
                        subUnlockLoader.unlocks.Add(d);
                        break;
                    }
                }
            }
        }

        WriteJson("QuestData",          questLoader);
        WriteJson("QuestConditionData", conditionLoader);
        WriteJson("QuestNotifyData",    notifyLoader);
        WriteJson("QuestUnlockData",    unlockLoader);
        WriteJson("SubQuestMainData",   subMainLoader);
        WriteJson("SubQuestNotifyData", subNotifyLoader);
        WriteJson("SubQuestUnlockData", subUnlockLoader);
    }

    static void ParseTravelData()
    {
        string path = $"{Application.dataPath}{EXCEL_PATH}/TravelData.xlsx";
        ParseTravelSheets(ReadAllXlsxSheets(path));
    }

    // -------------------------------------------------------------------
    // 공용 헬퍼
    // -------------------------------------------------------------------

    /// <summary>구글 시트 URL에서 xlsx를 다운로드해 MemoryStream으로 반환</summary>
    static MemoryStream DownloadGoogleSheet(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("[DataTransformer] 구글 시트 URL이 비어 있습니다. GSHEET_* 상수를 채워주세요.");
            return null;
        }
        Debug.Log($"[DataTransformer] 다운로드 중: {url}");
        using var client = new System.Net.WebClient();
        return new MemoryStream(client.DownloadData(url));
    }

    /// <summary>xlsx의 모든 시트를 읽어 (시트명, 행 목록) 리스트로 반환 (Stream 오버로드)</summary>
    static List<(string name, List<List<string>> rows)> ReadAllXlsxSheets(Stream stream)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var result = new List<(string, List<List<string>>)>();
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

    /// <summary>xlsx 파일을 읽어 List&lt;List&lt;string&gt;&gt; 형태로 반환 (Stream 오버로드)</summary>
    static List<List<string>> ReadXlsx(Stream stream, int sheetIndex = 0)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var rows = new List<List<string>>();
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
            });
            if (sheetIndex >= dataSet.Tables.Count)
            {
                Debug.LogError($"[DataTransformer] 시트 인덱스 {sheetIndex} 가 존재하지 않습니다.");
                return rows;
            }
            foreach (DataRow dataRow in dataSet.Tables[sheetIndex].Rows)
            {
                var row = new List<string>();
                foreach (var cell in dataRow.ItemArray)
                    row.Add(cell?.ToString() ?? "");
                rows.Add(row);
            }
        }
        return rows;
    }

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
            d.PhysicalAtk = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.Def         = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.MoveSpeed   = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.TotalExp    = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            d.RewardGold  = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
            d.DropItemId  = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
            d.DropCount   = i < row.Count ? ConvertValue<int>(row[i++]) : 0;
            d.DropChance  = i < row.Count ? ConvertValue<float>(row[i++]) : 0f;
            // 슬라이드 79: 미니몬 고유기술 (Excel에 컬럼 추가 전엔 빈 값)
            d.UniqueSkillName = i < row.Count ? row[i++] : "";
            d.UniqueSkillDesc = i < row.Count ? row[i++] : "";

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
            d.Grade         = i < row.Count ? ConvertValue<EquipGrade>(row[i++]) : EquipGrade.Basic;
            d.Description   = i < row.Count ? row[i++] : "";
            d.SpriteName    = i < row.Count ? row[i++] : "";
            d.FoundLocation = i < row.Count ? row[i++] : "";

            loader.items.Add(d);
        }

        WriteJson("ItemData", loader);
    }

    // ===================================================================
    //  추가 마스터데이터 시트 → JSON (스킬/장비/강화/미니몬/물약/콤보)
    //  포맷: Row0=필드명, Row1=타입, Row2=한글설명, Row3+=데이터. 리스트 셀은 콤마(,) 구분.
    //  enum은 가독성 위해 문자열로 직렬화(WriteJsonStr).
    //  각 Parse*Sheet은 행 소스(List<List<string>>)를 주입받아 로컬/구글 양쪽에서 재사용.
    // ===================================================================

    /// <summary>로컬 .xlsx(Assets/@Resources/Data/Excel)에서 추가 7종 파싱.</summary>
    [MenuItem("Tools/ParseExtraData (스킬/장비/미니몬/물약/콤보)")]
    public static void ParseExtraData()
    {
        string baseDir = $"{Application.dataPath}{EXCEL_PATH}";
        ParseSkillDataSheet      (ReadXlsx($"{baseDir}/SkillData.xlsx"));
        ParseEquipmentDataSheet  (ReadXlsx($"{baseDir}/EquipmentData.xlsx"));
        ParseEquipEnhanceDataSheet(ReadXlsx($"{baseDir}/EquipEnhanceData.xlsx"));
        ParseMinMonDataSheet     (ReadXlsx($"{baseDir}/MinMonData.xlsx"));
        ParseHPPotionDataSheet   (ReadXlsx($"{baseDir}/HPPotionData.xlsx"));
        ParseSpeedPotionDataSheet(ReadXlsx($"{baseDir}/SpeedPotionData.xlsx"));
        ParseComboDataSheet      (ReadXlsx($"{baseDir}/ComboData.xlsx"));
        AssetDatabase.Refresh();
        Debug.Log("[DataTransformer] ParseExtraData (로컬) 완료");
    }

    /// <summary>구글시트(RE001_DataSheets)에서 추가 7종 파싱. 시트는 "링크 있는 사용자-뷰어" 공개 필요.</summary>
    [MenuItem("Tools/ParseExtraDataFromGoogleSheets")]
    public static void ParseExtraDataFromGoogleSheets()
    {
        ParseSkillDataSheet      (ReadXlsx(DownloadGoogleSheet(GSHEET_SKILL)));
        ParseEquipmentDataSheet  (ReadXlsx(DownloadGoogleSheet(GSHEET_EQUIPMENT)));
        ParseEquipEnhanceDataSheet(ReadXlsx(DownloadGoogleSheet(GSHEET_EQUIP_ENHANCE)));
        ParseMinMonDataSheet     (ReadXlsx(DownloadGoogleSheet(GSHEET_MINMON)));
        ParseHPPotionDataSheet   (ReadXlsx(DownloadGoogleSheet(GSHEET_HP_POTION)));
        ParseSpeedPotionDataSheet(ReadXlsx(DownloadGoogleSheet(GSHEET_SPEED_POTION)));
        ParseComboDataSheet      (ReadXlsx(DownloadGoogleSheet(GSHEET_COMBO)));
        AssetDatabase.Refresh();
        Debug.Log("[DataTransformer] ParseExtraDataFromGoogleSheets 완료");
    }

    static string C(List<string> r, int i) => i < r.Count ? r[i] : "";
    static List<string> CsvStr(string s) => string.IsNullOrEmpty(s) ? new List<string>()
        : s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
    static List<T> Csv<T>(string s) => CsvStr(s).Select(x => ConvertValue<T>(x)).ToList();

    static void WriteJsonStr<T>(string filename, T loader)
    {
        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        string jsonStr = JsonConvert.SerializeObject(loader, settings);
        File.WriteAllText($"{Application.dataPath}{JSON_PATH}/{filename}.json", jsonStr, Encoding.UTF8);
        Debug.Log($"[DataTransformer] {filename}.json 생성 완료(시트)");
    }

    static void ParseSkillDataSheet(List<List<string>> rows)
    {
        var loader = new SkillDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            // 영문컬럼(NameEng/DescEng/StoryEng) 제거 — Name/Description/Story는 LocalizationData 키.
            var d = new SkillData();
            d.DataId            = ConvertValue<int>(C(row, 0));
            d.Name              = C(row, 1);
            d.Description       = C(row, 2);
            d.IconLabel         = C(row, 3);
            d.Category          = ConvertValue<SkillCategory>(C(row, 4));
            d.SkillActiveType   = ConvertValue<SkillActiveType>(C(row, 5));
            d.StatType          = C(row, 6);
            d.StatBonus         = ConvertValue<float>(C(row, 7));
            d.MaxLevel          = ConvertValue<int>(C(row, 8));
            d.MemoryCapacity    = ConvertValue<int>(C(row, 9));
            d.Story             = C(row, 10);
            d.Source            = ConvertValue<AbilitySource>(C(row, 11));
            d.RequiredMinionId  = ConvertValue<int>(C(row, 12));
            d.RequiredEquipId   = ConvertValue<int>(C(row, 13));
            d.RequiredItemId    = ConvertValue<int>(C(row, 14));
            d.RequiredItemCount = ConvertValue<int>(C(row, 15));
            d.NpcLocation       = C(row, 16);
            d.RoleTypes         = CsvStr(C(row, 17));
            d.TriggerType       = ConvertValue<AbilityTrigger>(C(row, 18));
            d.Conditions        = Csv<AbilityCondition>(C(row, 19));
            d.ProcChance        = string.IsNullOrEmpty(C(row, 20)) ? 100f : ConvertValue<float>(C(row, 20));
            d.Cooldown          = ConvertValue<float>(C(row, 21));
            d.ActiveContext     = ConvertValue<AbilityContext>(C(row, 22));
            d.ConditionValue    = ConvertValue<float>(C(row, 23));
            d.Effect            = ConvertValue<AbilityEffect>(C(row, 24));
            d.EffectStat        = C(row, 25);
            d.EffectValue       = ConvertValue<float>(C(row, 26));
            d.EffectDuration    = ConvertValue<float>(C(row, 27));
            loader.skills.Add(d);
        }
        WriteJsonStr("SkillData", loader);
    }

    static void ParseEquipmentDataSheet(List<List<string>> rows)
    {
        var loader = new EquipmentDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            var d = new EquipmentData();
            d.DataId        = ConvertValue<int>(C(row, 0));
            d.Name          = C(row, 1);
            d.SlotType      = ConvertValue<EquipSlotType>(C(row, 2));
            d.Grade         = ConvertValue<EquipGrade>(C(row, 3));
            d.SpriteName    = C(row, 4);
            d.MaxDurability = ConvertValue<int>(C(row, 5));
            d.BaseAtk       = ConvertValue<int>(C(row, 6));
            d.BaseCritRate  = ConvertValue<float>(C(row, 7));
            d.BaseMagicAtk  = ConvertValue<int>(C(row, 8));
            loader.equipments.Add(d);
        }
        WriteJsonStr("EquipmentData", loader);
    }

    static void ParseEquipEnhanceDataSheet(List<List<string>> rows)
    {
        var loader = new EquipEnhanceDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            var d = new EquipEnhanceData();
            d.DataId                = ConvertValue<int>(C(row, 0));
            d.EquipDataId           = ConvertValue<int>(C(row, 1));
            d.Level                 = ConvertValue<int>(C(row, 2));
            d.AtkBonus              = ConvertValue<int>(C(row, 3));
            d.CritBonus             = ConvertValue<float>(C(row, 4));
            d.MagicAtkBonus         = ConvertValue<int>(C(row, 5));
            d.RequiredMaterialId    = ConvertValue<int>(C(row, 6));
            d.RequiredMaterialCount = ConvertValue<int>(C(row, 7));
            loader.enhances.Add(d);
        }
        WriteJsonStr("EquipEnhanceData", loader);
    }

    static void ParseMinMonDataSheet(List<List<string>> rows)
    {
        var loader = new MinMonDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            var d = new MinMonData();
            d.DataId        = ConvertValue<int>(C(row, 0));
            d.Name          = C(row, 1);
            d.IconLabel     = C(row, 2);
            d.PrefabLabel   = C(row, 3);
            d.PetAnimLabel  = C(row, 4);
            d.MonsterDataId = ConvertValue<int>(C(row, 5));
            d.BaseAtk       = ConvertValue<float>(C(row, 6));
            d.BaseAtkSpeed  = ConvertValue<float>(C(row, 7));
            d.BaseCritRate  = ConvertValue<float>(C(row, 8));
            d.AtkGrade      = ConvertValue<MinMonGrade>(C(row, 9));
            d.AtkSpeedGrade = ConvertValue<MinMonGrade>(C(row, 10));
            d.CritGrade     = ConvertValue<MinMonGrade>(C(row, 11));
            d.MaxTameCount  = ConvertValue<int>(C(row, 12));
            d.CatchChance   = ConvertValue<float>(C(row, 13));
            loader.minMons.Add(d);
        }
        WriteJsonStr("MinMonData", loader);
    }

    static void ParseHPPotionDataSheet(List<List<string>> rows)
    {
        var loader = new HPPotionDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            var d = new HPPotionData();
            d.DataId            = ConvertValue<int>(C(row, 0));
            d.Name              = C(row, 1);
            d.Grade             = ConvertValue<EquipGrade>(C(row, 2));
            d.Description       = C(row, 3);
            d.SpriteName        = C(row, 4);
            d.WhereTo           = C(row, 5);
            d.Step              = ConvertValue<int>(C(row, 6));
            d.FlatRecov         = ConvertValue<int>(C(row, 7));
            d.MaxHPRecov        = ConvertValue<int>(C(row, 8));
            d.Cooldown          = ConvertValue<int>(C(row, 9));
            d.NeededItemIDs     = Csv<int>(C(row, 10));
            d.NeededItemAmounts = Csv<int>(C(row, 11));
            loader.potions.Add(d);
        }
        WriteJsonStr("HPPotion", loader);
    }

    static void ParseSpeedPotionDataSheet(List<List<string>> rows)
    {
        var loader = new SpeedPotionDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            var d = new SpeedPotionData();
            d.DataId            = ConvertValue<int>(C(row, 0));
            d.Name              = C(row, 1);
            d.Grade             = ConvertValue<EquipGrade>(C(row, 2));
            d.Description       = C(row, 3);
            d.SpriteName        = C(row, 4);
            d.WhereTo           = C(row, 5);
            d.Step              = ConvertValue<int>(C(row, 6));
            d.AtkSpeedBonus     = ConvertValue<float>(C(row, 7));
            d.Duration          = ConvertValue<int>(C(row, 8));
            d.Cooldown          = ConvertValue<int>(C(row, 9));
            d.NeededItemIDs     = Csv<int>(C(row, 10));
            d.NeededItemAmounts = Csv<int>(C(row, 11));
            loader.potions.Add(d);
        }
        WriteJsonStr("SpeedPotion", loader);
    }

    static void ParseComboDataSheet(List<List<string>> rows)
    {
        var loader = new ComboStageDataLoader();
        for (int y = 3; y < rows.Count; y++)
        {
            var row = rows[y];
            if (row.Count == 0 || string.IsNullOrEmpty(row[0])) continue;
            var d = new ComboStageData();
            d.Level        = ConvertValue<int>(C(row, 0));
            d.NameKey      = C(row, 1);
            d.HitsRequired = ConvertValue<int>(C(row, 2));
            d.MaxTimer     = ConvertValue<float>(C(row, 3));
            d.HitGain      = ConvertValue<float>(C(row, 4));
            d.AtkSpeedBuff = ConvertValue<float>(C(row, 5));
            d.DmgBoostBuff = ConvertValue<float>(C(row, 6));
            d.ColorHex     = C(row, 7);
            loader.stages.Add(d);
        }
        WriteJsonStr("ComboData", loader);
    }
}
