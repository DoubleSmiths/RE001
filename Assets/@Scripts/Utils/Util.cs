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

    // 한국어 조사 자동 선택.
    //  이름을 문장 앞에 붙이는 구조라(예: "{몬스터명}" + "이(가) 나타났다!") 화면에
    //  "여우이(가) 나타났다!" 처럼 조사 두 개가 그대로 찍혀 나왔다.
    //  로컬라이즈 원문은 "이(가)" 형태를 유지하고(번역자가 쓰기 편하다) 표시 직전에 여기서 고른다.
    //
    //  받침 판정: 한글 음절 U+AC00~U+D7A3 에서 (코드-0xAC00) % 28 != 0 이면 받침 있음.
    //  영문/숫자로 끝나면 판정이 불가능하므로 원문("이(가)")을 그대로 둔다.
    static readonly (string pair, string withJong, string withoutJong)[] JOSA =
    {
        ("이(가)", "이", "가"),
        ("은(는)", "은", "는"),
        ("을(를)", "을", "를"),
        ("와(과)", "과", "와"),   // 받침 있으면 '과'
        ("과(와)", "과", "와"),
    };

    public static string ApplyJosa(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (Managers.Game != null && Managers.Game.Language != Define.Language.Korean) return text;

        foreach (var (pair, withJong, withoutJong) in JOSA)
        {
            int at;
            while ((at = text.IndexOf(pair, System.StringComparison.Ordinal)) >= 0)
            {
                // 원문이 " 이(가) 나타났다!"처럼 앞에 공백을 달고 있는 경우가 있다.
                //  공백을 건너뛰고 이름의 마지막 글자를 본 뒤, 조사는 이름에 **붙여서** 쓴다
                //  (조사 앞 공백은 한국어에서 틀린 표기다).
                int nameEnd = at;
                while (nameEnd > 0 && char.IsWhiteSpace(text[nameEnd - 1])) nameEnd--;
                if (nameEnd == 0) break;                 // 앞 글자가 없으면 판정 불가 — 그대로 둔다

                char prev = text[nameEnd - 1];
                if (prev < 0xAC00 || prev > 0xD7A3) break;
                string josa = ((prev - 0xAC00) % 28 != 0) ? withJong : withoutJong;
                text = text.Substring(0, nameEnd) + josa + text.Substring(at + pair.Length);
            }
        }
        return text;
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

    public static void SetTMPOutline(TMP_Text tmp, Color32 color, float width = 0.5f)
    {
        tmp.outlineColor = color;
        tmp.outlineWidth = width;

        //성우 추가
        Material mat = tmp.fontMaterial;
        mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.5f);
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
