using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR

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
        ParseSkillData("Skill");
    }

    static void ParseSkillData(string filename)
    {
        //SkillDataLoader loader = new SkillDataLoader();

        //#region ExcelData
        //string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/Excel/{filename}Data.csv").Split("\n");

        //for (int y = 1; y < lines.Length; y++)
        //{
        //    string[] row = lines[y].Replace("\r", "").Split(',');
        //    if (row.Length == 0)
        //        continue;
        //    if (string.IsNullOrEmpty(row[0]))
        //        continue;

        //    int i = 0;
        //    SkillData skillData = new SkillData();
        //    skillData.DataId = ConvertValue<int>(row[i++]);
        //    skillData.Name = ConvertValue<string>(row[i++]);
        //    skillData.Description = ConvertValue<string>(row[i++]);
        //    loader.skills.Add(skillData);
        //}
        //#endregion

        //string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
        //File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json", jsonStr);
        //AssetDatabase.Refresh();
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

#endif
}
