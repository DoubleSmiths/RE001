using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

// [능력] 시스템 (기획서 v3 슬38~39). 일반 게임의 "스킬".
//  - 기억력(Memory): 플레이어 자원. 능력마다 [기억력 점유량](MemoryCapacity)을 가짐.
//    점유량만큼 기억력을 점유해 능력을 "기억(장착)". 점유량 0 = [재능](항시 적용).
//  - MaxMemory = BASE_MEMORY + 장착된 MaxMemory 보너스(기억 확장 재능 등).
//  - 획득 경로(장비+9/전직/도감별/미니몬 공명도)는 후속 — 현재는 Learn(id) API로 추상화.
//  - 능력 효과: 장착된 능력의 StatType/StatBonus를 PlayerController 스탯에 가산(ApplyTo).
public class AbilityManager
{
    public event Action OnAbilityChanged;   // 보유/장착 변동 시

    public const int BASE_MEMORY = 10;      // 시작 기억력 (밸런스 시드 — 엑셀 후속)

    // 슬79: 미니몬 고유기술 능력 id 매핑 (creatureId + OFFSET). 예) 2001 → 6001.
    public const int MINION_SKILL_OFFSET = 4000;
    public int MinionSkillId(int creatureId) => creatureId + MINION_SKILL_OFFSET;

    List<int> Learned  => Managers.Game.LearnedAbilities;
    List<int> Equipped => Managers.Game.EquippedAbilities;

    bool TryData(int id, out SkillData d) => Managers.Data.SkillDic.TryGetValue(id, out d);

    // 슬38/42: 능력 종류(Role) — 기술/마법/수비/지원. StatType에서 파생.
    //  (기획서상 한 능력이 여러 종류를 가질 수 있으나 현재 데이터는 StatType 1개라 단일 파생)
    public enum Role { Skill, Spell, Def, Util }
    public Role GetRole(int id)
    {
        if (!TryData(id, out var d)) return Role.Util;
        switch (d.StatType)
        {
            case "MagicAtk":     return Role.Spell;                                  // 마법
            case "PhysicalAtk":
            case "CritRate":
            case "AtkSpeed":
            case "Accuracy":     return Role.Skill;                                  // 기술(물리 전투)
            case "MaxHp":
            case "Def":
            case "PhysicalResist":
            case "MagicResist":
            case "Evasion":
            case "RecoveryRate": return Role.Def;                                    // 수비(생존)
            default:             return Role.Util;                                   // 지원(MaxMemory 등 편의)
        }
    }

    // 슬2: 능력 이름/설명 한/영 — NameEng/DescEng 비어있으면 한국어(Name/Description) 폴백.
    bool IsKorean => Managers.Game == null || Managers.Game.Language == Define.Language.Korean;
    public string GetName(int id)
    {
        if (!TryData(id, out var d)) return $"능력 #{id}";
        return !IsKorean && !string.IsNullOrEmpty(d.NameEng) ? d.NameEng : d.Name;
    }
    public string GetDesc(int id)
    {
        if (!TryData(id, out var d)) return "";
        return !IsKorean && !string.IsNullOrEmpty(d.DescEng) ? d.DescEng : d.Description;
    }

    // 능력 정보창 #4: 능력 이야기(flavor) 한/영. 없으면 빈 문자열.
    public string GetStory(int id)
    {
        if (!TryData(id, out var d)) return "";
        return !IsKorean && !string.IsNullOrEmpty(d.StoryEng) ? d.StoryEng : (d.Story ?? "");
    }

    // 능력 정보창 #5: 능력 출처(획득 경로). 미니몬 고유기술(id-4000이 미니몬 범위)은 데이터 미기재여도 Minion 파생.
    public Define.AbilitySource GetSource(int id)
    {
        if (TryData(id, out var d))
        {
            // 미니몬 고유기술(6001/6002 = creatureId+OFFSET)은 데이터 기본값이어도 Minion 취급.
            if (d.Source == Define.AbilitySource.Unlimited && IsMinionSkill(id))
                return Define.AbilitySource.Minion;
            return d.Source;
        }
        return IsMinionSkill(id) ? Define.AbilitySource.Minion : Define.AbilitySource.Unlimited;
    }

    // 능력 정보창 #6: 게이팅 미니몬 CreatureId. 데이터 0이면 (id-OFFSET)로 파생.
    public int GetRequiredMinionId(int id)
    {
        if (TryData(id, out var d) && d.RequiredMinionId > 0) return d.RequiredMinionId;
        return IsMinionSkill(id) ? id - MINION_SKILL_OFFSET : 0;
    }

    // 미니몬 고유기술 id 범위(creatureId 2000번대 + OFFSET = 6000번대)에 해당하고 원본 미니몬이 존재하는지.
    bool IsMinionSkill(int id)
    {
        int creatureId = id - MINION_SKILL_OFFSET;
        return creatureId > 0 && Managers.Data.MinMonDic != null && Managers.Data.MinMonDic.ContainsKey(creatureId);
    }

    public bool IsLearned(int id)  => Learned.Contains(id);
    public bool IsEquipped(int id) => Equipped.Contains(id);
    public bool IsTalent(int id)   => TryData(id, out var d) && d.MemoryCapacity <= 0; // 점유량 0 = 재능

    // ── 능력 단계(Level, 슬38) ── 보유했으면 최소 1.
    public int GetLevel(int id)
    {
        if (Managers.Game.AbilityLevels.TryGetValue(id, out int lv)) return Mathf.Max(1, lv);
        return IsLearned(id) ? 1 : 0;
    }
    public void SetLevel(int id, int lv)
    {
        int max = TryData(id, out var d) ? Mathf.Max(1, d.MaxLevel) : 99;
        Managers.Game.AbilityLevels[id] = Mathf.Clamp(lv, 1, max);
        Managers.Game.SaveGame();
        OnAbilityChanged?.Invoke();
    }

    // ── 기억력 회계 ──
    public int MaxMemory
    {
        get
        {
            int m = BASE_MEMORY;
            foreach (int id in Equipped)
                if (TryData(id, out var d) && d.StatType == "MaxMemory")
                    m += Mathf.RoundToInt(d.StatBonus * GetLevel(id));
            return m;
        }
    }

    public int UsedMemory
    {
        get
        {
            int u = 0;
            foreach (int id in Equipped)
                if (TryData(id, out var d))
                    u += Mathf.Max(0, d.MemoryCapacity);
            return u;
        }
    }

    public int FreeMemory => MaxMemory - UsedMemory;

    // ── 획득 ──
    // 슬38 획득 경로(장비+9/전직/도감별/미니몬 공명도)의 공통 진입점. 재능(점유량0)은 학습 즉시 자동 장착(항시 적용).
    public void Learn(int id)
    {
        if (!TryData(id, out var d)) return;
        if (!Learned.Contains(id)) Learned.Add(id);
        if (d.MemoryCapacity <= 0 && !Equipped.Contains(id))
            Equipped.Add(id);                       // 재능: 항시 적용
        Managers.Game.SaveGame();
        OnAbilityChanged?.Invoke();
    }

    public void Unlearn(int id)
    {
        Learned.Remove(id);
        Equipped.Remove(id);
        Managers.Game.AbilityLevels.Remove(id);
        Managers.Game.SaveGame();
        OnAbilityChanged?.Invoke();
    }

    // 슬10/11/79: 미니몬 고유기술을 능력으로 습득(영구). level 1=공명도100% / 2=모든등급 S.
    // SkillDic에 해당 능력(creatureId+OFFSET)이 있을 때만. 한 번 습득하면 사라지지 않고 단계만 갱신.
    public void LearnMinionUniqueSkill(int creatureId, int level)
    {
        int id = MinionSkillId(creatureId);
        if (level <= 0 || !TryData(id, out _)) return;
        bool firstTime = !IsLearned(id);
        Learn(id);                                   // 보유 + (재능이면 자동장착)
        if (level > GetLevel(id)) SetLevel(id, level);
        if (firstTime) Debug.Log($"[Ability] 미니몬 고유기술 습득: {id} Lv.{GetLevel(id)}");
    }

    // ── 기억(장착) / 망각 ──
    public bool CanMemorize(int id)
    {
        if (!IsLearned(id) || IsEquipped(id)) return false;
        if (!TryData(id, out var d)) return false;
        return FreeMemory >= Mathf.Max(0, d.MemoryCapacity);
    }

    public bool Memorize(int id)
    {
        if (!CanMemorize(id)) return false;
        Equipped.Add(id);
        Managers.Game.SaveGame();
        OnAbilityChanged?.Invoke();
        return true;
    }

    // 망각 시 점유량만큼 기억력 회복(목록에서 제거). 재능은 항시 적용이라 망각 불가.
    public bool Forget(int id)
    {
        if (!IsEquipped(id) || IsTalent(id)) return false;
        Equipped.Remove(id);
        Managers.Game.SaveGame();
        OnAbilityChanged?.Invoke();
        return true;
    }

    // ── 효과 적용 ── 장착된 능력의 StatBonus를 크리처 스탯에 가산. (RefreshStats 말미에서 호출)
    public void ApplyTo(CreatureController c)
    {
        if (c == null) return;
        foreach (int id in Equipped)
        {
            if (!TryData(id, out var d)) continue;
            float v = d.StatBonus * GetLevel(id);   // 슬38: 단계(Level)만큼 수치 증가
            switch (d.StatType)
            {
                case "PhysicalAtk": c.PhysicalAtk += v; break;
                case "MagicAtk":    c.MagicAtk    += v; break;
                case "CritRate":    c.CritRate    += v; break;
                case "MaxHp":       c.MaxHp       += v; break;
                case "AtkSpeed":    c.AtkSpeed    += v; break;
                case "Accuracy":    c.Accuracy    += v; break;
                case "Def":         c.Def         += v; break;
                // MaxMemory 는 기억력 자원 — 스탯 아님(MaxMemory 프로퍼티에서 처리)
            }
        }
    }
}
