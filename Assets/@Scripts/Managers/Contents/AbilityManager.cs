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
