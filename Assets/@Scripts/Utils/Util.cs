using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Define;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Random = UnityEngine.Random;
using Transform = UnityEngine.Transform;

public static class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static Vector2 RandomPointInAnnulus(Vector2 origin, float minRadius = 6, float maxRadius = 12)
    {
        float randomDist = UnityEngine.Random.Range(minRadius, maxRadius);

        Vector2 randomDir = new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100)).normalized;
        //Debug.Log(randomDir);
        var point = origin + randomDir * randomDist;
        return point;
    }

    /// <summary>TextKey로 현재 언어의 텍스트를 반환. 키가 없으면 key 자체를 반환.</summary>
    public static string Localize(string key)
    {
        var dic = Managers.Data?.LocalizationDic;
        if (dic == null || !dic.TryGetValue(key, out var entry))
            return key;

        return Managers.Game.Language == Define.Language.Korean ? entry.Kor : entry.Eng;
    }

    public static Color HexToColor(string color)
    {
        Color parsedColor;
        ColorUtility.TryParseHtmlString("#"+color, out parsedColor);

        return parsedColor;
    }

    public static bool IntToBool(int value)
    {
        return value > 0;
    }

    //Enum값중 랜덤값 반환
    //public static T GetRandomEnumValue<T>() where T : struct, Enum
    //{
    //    Type type = typeof(T);

    //    if (!_enumDict.ContainsKey(type))
    //        _enumDict[type] = Enum.GetValues(type);

    //    Array values = _enumDict[type];

    //    int index = UnityEngine.Random.Range(0, values.Length);
    //    return (T)values.GetValue(index);
    //}

    //string값 으로 Enum값 찾기
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    /// <summary>
    /// 슬라이드 18 표기 규칙: decimals=0이면 정수, 양수면 해당 자리까지 반올림.
    /// percent=true이면 끝에 % 붙임.
    /// </summary>
    public static string FormatStat(float value, int decimals = 0, bool percent = false)
    {
        string text = decimals > 0 ? value.ToString($"F{decimals}") : ((int)Mathf.Round(value)).ToString();
        return percent ? text + "%" : text;
    }

    public static Color GetGradeColor(EquipGrade grade)
    {
        return grade switch
        {
            EquipGrade.Basic => new Color32(255, 255, 255, 200),
            EquipGrade.Abnormal => HexToColor("1A9600"),
            EquipGrade.Rare => HexToColor("0064C8"),
            EquipGrade.Exceptional => HexToColor("9600FF"),
            EquipGrade.Legendary => HexToColor("C58C00"),
            EquipGrade.Mystic => HexToColor("960E00"),
            EquipGrade.REQUISITE => HexToColor("DD008F"),
            _ => Color.white,
        };
    }

    public static void SetTMPOutline(TMP_Text tmp, Color32 color, float width = 0.15f)
    {
        tmp.outlineColor = color;
        tmp.outlineWidth = width;
    }

    public static void SetTMPUnderlay(TMP_Text tmp, Color32 color, float offsetX, float offsetY, float softness = 0f)
    {
        Material mat = tmp.fontMaterial;
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor(ShaderUtilities.ID_UnderlayColor, color);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, offsetX);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, offsetY);
        mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, softness);
    }

}
