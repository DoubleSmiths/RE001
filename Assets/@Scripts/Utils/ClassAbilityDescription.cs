using System.Collections.Generic;
using System.Globalization;
using Data;
using UnityEngine;
using static Define;

/// <summary>축성 능력의 실제 단계별 효과를 로컬라이즈된 본문으로 만든다.</summary>
public static class ClassAbilityDescription
{
    public static string GetDescription(int skillId, bool previewNext = false)
    {
        if (!Managers.Data.SkillDic.TryGetValue(skillId, out var data) || data.Source != AbilitySource.Class)
            return string.Empty;

        int level = Mathf.Max(1, Managers.Ability.GetLevel(skillId));
        string header = Managers.Ability.IsLearned(skillId) ? "CurrentFormat" : "AcquisitionFormat";
        string description = Format(header, level) + "\n" + GetLevelSummary(data, level);
        if (previewNext && level < Mathf.Max(1, data.MaxLevel))
            description += "\n\n" + Format("NextFormat", level + 1) + "\n" + GetLevelSummary(data, level + 1);
        return description;
    }

    public static string GetLevelSummary(SkillData data, int level)
    {
        level = Mathf.Max(1, level);
        if (data.TriggerType == AbilityTrigger.None)
        {
            var stats = new List<string>();
            // 능력 적용과 동일하게 다중 스탯이 있으면 단일 스탯보다 우선한다.
            if (data.StatTypes != null && data.StatTypes.Count > 0)
            {
                for (int i = 0; i < data.StatTypes.Count; i++)
                {
                    float bonus = data.StatBonuses != null && i < data.StatBonuses.Count ? data.StatBonuses[i] : 0f;
                    AddLine(stats, StatLine(data.StatTypes[i], bonus * level));
                }
            }
            else
                AddLine(stats, StatLine(data.StatType, data.StatBonus * level));
            return string.Join("\n", stats);
        }

        var lines = new List<string>();
        if (data.TriggerType == AbilityTrigger.Probability)
        {
            var conditions = new List<string>();
            if (data.Conditions != null)
                foreach (var condition in data.Conditions)
                    AddLine(conditions, ConditionText(condition, data.ConditionValue));
            AddLine(lines, string.Join(Text("OrText"), conditions));
            AddLine(lines, Format("ChanceFormat", Number(Mathf.Clamp(data.ProcChance, 0f, 100f))));
        }
        else if (data.TriggerType == AbilityTrigger.Time)
            AddLine(lines, Format("IntervalFormat", ContextText(data.ActiveContext), Number(Mathf.Max(0.1f, data.Cooldown))));

        // 확률과 지속 시간은 고정이며, 효과 값만 AbilityRunner처럼 단계 배율을 적용한다.
        AddLine(lines, EffectText(data, data.EffectValue * level));
        return string.Join("\n", lines);
    }

    static string StatLine(string stat, float value)
    {
        if (stat == "ComboKeep") return Format("ComboFormat", Number(value));
        if (stat == "GuaranteedCatchReduce") return Format("CaptureFormat", Mathf.Max(0, Mathf.RoundToInt(value)));
        string name = StatName(stat);
        if (string.IsNullOrEmpty(name)) return string.Empty;
        return Format(stat == "Evasion" ? "PercentPointFormat" : "FlatStatFormat", name, Number(value));
    }

    static string StatName(string stat)
    {
        switch (stat)
        {
            case "MaxHp": return Text("MaxHpText");
            case "Def": return Text("DefText");
            case "Evasion": return Text("EvasionText");
            case "PhysicalAtk": return Text("PhysicalAtkText");
            default: return string.Empty;
        }
    }

    static string ConditionText(AbilityCondition condition, float value)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(value));
        switch (condition)
        {
            case AbilityCondition.BattleStart: return Text("BattleStartText");
            case AbilityCondition.ConsecutiveCrit: return Format("ConsecutiveCritFormat", count);
            case AbilityCondition.ConsecutiveEvade: return Format("ConsecutiveEvadeFormat", count);
            case AbilityCondition.FatalDamage: return Text("FatalDamageText");
            default: return string.Empty;
        }
    }

    static string ContextText(AbilityContext context)
    {
        switch (context)
        {
            case AbilityContext.InBattle: return Text("InBattleText");
            case AbilityContext.OutOfBattle: return Text("OutOfBattleText");
            default: return Text("AlwaysText");
        }
    }

    static string EffectText(SkillData data, float value)
    {
        bool ratio = data.EffectStat == "PhysicalAtk" || data.EffectStat == "MaxHp";
        switch (data.Effect)
        {
            case AbilityEffect.Heal:
                return ratio ? Format("HealRatioFormat", StatName(data.EffectStat), Number(value)) : Format("HealFormat", Number(value));
            case AbilityEffect.BonusDamage:
                return ratio ? Format("DamageRatioFormat", StatName(data.EffectStat), Number(value)) : Format("DamageFormat", Mathf.RoundToInt(value));
            case AbilityEffect.Buff:
                string stat = StatLine(data.EffectStat, value);
                return data.EffectDuration > 0f
                    ? Format("DurationFormat", stat, Number(data.EffectDuration))
                    : Format("UntilBattleEndFormat", stat);
            default:
                return string.Empty;
        }
    }

    static void AddLine(List<string> lines, string line)
    {
        if (!string.IsNullOrEmpty(line)) lines.Add(line);
    }

    static string Number(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    static string Text(string suffix) => Util.Localize(CLASS_ABILITY_TEXT_PREFIX + suffix);
    static string Format(string suffix, params object[] values) => string.Format(CultureInfo.InvariantCulture, Text(suffix), values);
}
