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

    // 슬41: 물약주입기 기술 — 습득 시 자동물약 [기억](AutoPotionMemorized) 활성화.
    public const int AUTO_POTION_SKILL_ID = 5009;

    // 슬79: 미니몬 고유기술 능력 id 매핑 (creatureId + OFFSET). 예) 2001 → 6001.
    public const int MINION_SKILL_OFFSET = 4000;
    public int MinionSkillId(int creatureId) => creatureId + MINION_SKILL_OFFSET;

    List<int> Learned  => Managers.Game.LearnedAbilities;
    List<int> Equipped => Managers.Game.EquippedAbilities;

    bool TryData(int id, out SkillData d) => Managers.Data.SkillDic.TryGetValue(id, out d);

    // 변동 통지 (모든 상태 변경은 이 경로로)
    void RaiseChanged() => OnAbilityChanged?.Invoke();

    // ── 다중 스탯(슬44) ── StatTypes/StatBonuses(병렬 리스트)가 있으면 그것을, 비면 StatType/StatBonus 단일 fallback.
    static readonly List<(string stat, float bonus)> s_pairBuf = new();
    static List<(string stat, float bonus)> StatPairs(SkillData d)
    {
        s_pairBuf.Clear();
        if (d.StatTypes != null && d.StatTypes.Count > 0)
        {
            for (int i = 0; i < d.StatTypes.Count; i++)
                s_pairBuf.Add((d.StatTypes[i], d.StatBonuses != null && i < d.StatBonuses.Count ? d.StatBonuses[i] : 0f));
        }
        else if (!string.IsNullOrEmpty(d.StatType))
            s_pairBuf.Add((d.StatType, d.StatBonus));
        return s_pairBuf;
    }
    static string PrimaryStat(SkillData d)
        => d.StatTypes != null && d.StatTypes.Count > 0 ? d.StatTypes[0] : d.StatType;

    // 슬38/42: 능력 종류(Role) — 기술/마법/수비/지원. 대표 스탯(다중이면 첫 번째)에서 파생.
    public enum Role { Skill, Spell, Def, Util }
    public Role GetRole(int id)
    {
        if (!TryData(id, out var d)) return Role.Util;
        switch (PrimaryStat(d))
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

    // 슬44: 능력 종류 목록(여러 개 가능). RoleTypes 지정 시 그것을, 비어있으면 StatType→단일 Role 파생.
    public List<Role> GetRoles(int id)
    {
        var list = new List<Role>();
        if (TryData(id, out var d) && d.RoleTypes != null)
            foreach (var s in d.RoleTypes)
                if (System.Enum.TryParse<Role>(s, true, out var r) && !list.Contains(r)) list.Add(r);
        if (list.Count == 0) list.Add(GetRole(id));
        return list;
    }

    // 슬2: 능력 이름/설명/이야기 — Name/Description/Story는 LocalizationData 키. Util.Localize로 한/영 전환.
    public string GetName(int id)
    {
        if (!TryData(id, out var d)) return $"능력 #{id}";
        return Util.Localize(d.Name);
    }
    public string GetDesc(int id)
    {
        if (!TryData(id, out var d)) return "";
        return Util.Localize(d.Description);
    }

    // 능력 정보창 #4: 능력 이야기(flavor). 키 비면 빈 문자열.
    public string GetStory(int id)
    {
        if (!TryData(id, out var d)) return "";
        return string.IsNullOrEmpty(d.Story) ? "" : Util.Localize(d.Story);
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

    // 착용/부여 포함 보유 판정 — 장비 착용 부여(무점유)도 보유/기억중 취급 (슬38).
    public bool IsLearned(int id)  => Learned.Contains(id) || IsEquipGranted(id);
    public bool IsEquipped(int id) => Equipped.Contains(id) || IsEquipGranted(id);
    public bool IsTalent(int id)   => TryData(id, out var d) && d.MemoryCapacity <= 0; // 점유량 0 = 재능

    // ── 슬38 장비 능력 ──
    // 장비 착용으로 부여된 능력(무점유 — 착용 중에만 유효). Source=Equip && 해당 장비 착용 중.
    public bool IsEquipGranted(int id)
    {
        if (!TryData(id, out var d) || d.Source != Define.AbilitySource.Equip) return false;
        if (d.RequiredEquipId <= 0 || Managers.Game == null) return false;
        foreach (var e in Managers.Game.EquippedEquipments.Values)
            if (e.DataId == d.RequiredEquipId) return true;
        return false;
    }

    // 효과 적용 대상 능력 = 기억(장착) + 착용 장비 부여(무점유). AbilityRunner/ApplyTo가 순회.
    //  부여 여부는 장비 상태(로드 재구성/시작 지급 등 이벤트 없는 변경 포함)에 의존하므로 캐시 없이 매번 계산.
    //  매 호출 새 리스트 반환 — Notify 중첩(AfterAbilityUse 재진입)에도 순회가 안전. (N≈20이라 비용 미미)
    public List<int> ActiveAbilityIds
    {
        get
        {
            var list = new List<int>(Equipped);
            if (Managers.Data?.SkillDic != null)
                foreach (var d in Managers.Data.SkillDic.Values)
                    if (!list.Contains(d.DataId) && IsEquipGranted(d.DataId))
                        list.Add(d.DataId);
            return list;
        }
    }

    // 착용/해제 시 기억 상태 정리 (GameManager.EquipItem/UnEquipItem에서 호출).
    //  착용 → 무점유 자동 기억: 점유 기억 중이었으면 점유 환불(EquippedAbilities에서 제거). 적용은 IsEquipGranted가 담당.
    //  해제 → 기억 해제(+9 영구화 능력 포함 — 재기억 시 점유량 소모, 슬38). 비+9는 보유 자체가 소멸.
    public void OnEquipChanged(Equipment equip, bool equippedNow)
    {
        if (equip == null || Managers.Data?.SkillDic == null) return;
        bool changed = false;
        foreach (var d in Managers.Data.SkillDic.Values)
        {
            if (d.Source != Define.AbilitySource.Equip || d.RequiredEquipId != equip.DataId) continue;
            if (Equipped.Remove(d.DataId)) changed = true;
            if (!equippedNow && !Learned.Contains(d.DataId) && Managers.Game.AbilityLevels.Remove(d.DataId)) changed = true;
        }
        if (changed) Managers.Game.SaveGame();
        RaiseChanged();   // 부여 상태(computed)가 바뀌었으므로 항상 통지
    }

    // +9 도달 — 장비 능력 영구화(해제해도 기술창 잔존, 슬38). GameManager.EnhanceEquipment에서 호출.
    public void NotifyEquipMaxEnhanced(Equipment equip)
    {
        if (equip == null || Managers.Data?.SkillDic == null) return;
        foreach (var d in Managers.Data.SkillDic.Values)
            if (d.Source == Define.AbilitySource.Equip && d.RequiredEquipId == equip.DataId && !Learned.Contains(d.DataId))
                Learn(d.DataId);
    }

    // 장비 능력 단계.
    //  슬49 능력단계표: 1 + 최대강화(+1) + 최대내구(+1) — 보유 장비 중 최고 상태 기준.
    //  슬38 ⑥      : 동일 능력을 지닌 장비를 2개 이상 +9 했을 때 그 수만큼 추가 상승.
    //  두 규칙은 가산으로 합친다(기존 동작을 낮추지 않으면서 ⑥을 얹음).
    int GetEquipAbilityLevel(int id)
    {
        if (!IsLearned(id) || !TryData(id, out var d)) return 0;
        int lv = 1;
        int maxEnhancedCount = 0;
        Equipment best = null;
        if (Managers.Game != null)
        {
            foreach (var e in Managers.Game.OwnedEquipments)
            {
                if (e.DataId != d.RequiredEquipId) continue;
                if (best == null || e.EnhanceLevel > best.EnhanceLevel) best = e;
                if (e.EnhanceLevel >= Define.ENHANCE_MAX_LEVEL) maxEnhancedCount++;
            }
        }
        if (best != null)
        {
            if (best.EnhanceLevel >= Define.ENHANCE_MAX_LEVEL) lv++;
            var ed = best.EquipmentData;
            if (ed != null && best.Durability >= ed.MaxDurability) lv++;
        }
        // 슬38 ⑥: 2개째부터 1개당 1단계 추가
        lv += Mathf.Max(0, maxEnhancedCount - 1);
        return Mathf.Min(lv, Mathf.Max(1, d.MaxLevel));
    }

    // ── 전직(축성) ── 숙련도 만충 시 수도원에서 직업(Class 출처) 능력 습득. 축성마다 숙련도 소진.
    public bool CanAnoint(int id)
        => GetSource(id) == Define.AbilitySource.Class
        && !IsLearned(id)
        && Managers.Game != null
        && Managers.Game.ClassProficiency >= Define.CLASS_PROFICIENCY_MAX;

    public bool TryAnoint(int id)
    {
        if (!CanAnoint(id)) return false;
        Managers.Game.AddClassProficiency(-Define.CLASS_PROFICIENCY_MAX);   // 축성마다 소진 — 다음 능력은 다시 채워야
        Learn(id);
        return true;
    }

    // ── NPC 습득(슬50/자동물약 정식경로) ── 재료 소모 후 습득. 5009 물약주입기의 AutoPotionMemorized는 Learn이 처리.
    public bool CanLearnFromNpc(int id)
    {
        if (!TryData(id, out var d) || d.Source != Define.AbilitySource.Npc || IsLearned(id)) return false;
        return d.RequiredItemId > 0 && Managers.Game.HasItem(d.RequiredItemId, Mathf.Max(1, d.RequiredItemCount));
    }

    public bool TryLearnFromNpc(int id)
    {
        if (!CanLearnFromNpc(id) || !TryData(id, out var d)) return false;
        Managers.Game.RemoveItem(d.RequiredItemId, Mathf.Max(1, d.RequiredItemCount));
        Learn(id);
        return true;
    }

    // ── 능력 단계(Level, 슬38) ── 보유했으면 최소 1. 장비 출처는 장비 상태(강화/내구)에서 동적 산출(슬49).
    public int GetLevel(int id)
    {
        if (GetSource(id) == Define.AbilitySource.Equip) return GetEquipAbilityLevel(id);
        if (Managers.Game.AbilityLevels.TryGetValue(id, out int lv)) return Mathf.Max(1, lv);
        return IsLearned(id) ? 1 : 0;
    }
    public void SetLevel(int id, int lv)
    {
        int max = TryData(id, out var d) ? Mathf.Max(1, d.MaxLevel) : 99;
        Managers.Game.AbilityLevels[id] = Mathf.Clamp(lv, 1, max);
        Managers.Game.SaveGame();
        RaiseChanged();
    }

    // ── 기억력 회계 ──
    public int MaxMemory
    {
        get
        {
            int m = BASE_MEMORY;
            foreach (int id in ActiveAbilityIds)
                if (TryData(id, out var d))
                    foreach (var (stat, bonus) in StatPairs(d))
                        if (stat == "MaxMemory")
                            m += Mathf.RoundToInt(bonus * GetLevel(id));
            return m;
        }
    }

    public int UsedMemory
    {
        get
        {
            int u = 0;
            foreach (int id in Equipped)
            {
                if (IsEquipGranted(id)) continue;   // 착용 부여는 무점유 (슬38)
                if (TryData(id, out var d))
                    u += Mathf.Max(0, d.MemoryCapacity);
            }
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

        // 슬41: 물약주입기 습득 시 자동물약 [기억] 활성화
        if (id == AUTO_POTION_SKILL_ID)
            Managers.Game.AutoPotionMemorized = true;

        Managers.Game.SaveGame();
        RaiseChanged();
    }

    public void Unlearn(int id)
    {
        Learned.Remove(id);
        Equipped.Remove(id);
        Managers.Game.AbilityLevels.Remove(id);
        Managers.Game.SaveGame();
        RaiseChanged();
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
        RaiseChanged();
        return true;
    }

    // 망각 시 점유량만큼 기억력 회복(목록에서 제거). 재능/장비 부여(무점유)는 망각 불가.
    public bool Forget(int id)
    {
        if (IsTalent(id) || IsEquipGranted(id)) return false;
        if (!Equipped.Remove(id)) return false;
        Managers.Game.SaveGame();
        RaiseChanged();
        return true;
    }

    // 슬23-4: 포획 가능 미니몬 창고 칸 수 확장 — "InterestCapacity" 스탯 능력의 보너스 합(단계 반영).
    //  (창고 칸은 크리처 스탯이 아니라 MinMonManager가 조회 — MaxMemory처럼 ApplyTo 스위치 밖에서 처리)
    public int InterestCapacityBonus => Mathf.Max(0, Mathf.RoundToInt(SumStatBonus("InterestCapacity")));

    // 슬38 지원(Util) 효과: 활성(기억+장비부여) 능력의 해당 스탯 보너스×단계 합. (각 시스템이 직접 조회)
    float SumStatBonus(string statType)
    {
        float sum = 0f;
        foreach (int id in ActiveAbilityIds)
            if (TryData(id, out var d))
                foreach (var (stat, bonus) in StatPairs(d))
                    if (stat == statType)
                        sum += bonus * GetLevel(id);
        return sum;
    }

    // 슬38 지원: 미니몬 잡기 성공확률 보너스(%p). StatType "CatchRate". (MinMonManager.TryCatch가 조회)
    public float CatchRateBonus => SumStatBonus("CatchRate");
    // 슬38 지원: 확정잡기(천장) 필요 시도횟수 감소. StatType "GuaranteedCatchReduce".
    public int GuaranteedCatchReduce => Mathf.Max(0, Mathf.RoundToInt(SumStatBonus("GuaranteedCatchReduce")));
    // 슬38 지원: 경험치 획득량 보너스(%). StatType "ExpGain". (PlayerController.GainExp가 조회)
    public float ExpGainBonus => SumStatBonus("ExpGain");

    // ── 효과 적용 ── 활성(기억+장비부여) 능력의 스탯 보너스를 크리처 스탯에 가산. (RefreshStats 말미에서 호출)
    public void ApplyTo(CreatureController c)
    {
        if (c == null) return;
        foreach (int id in ActiveAbilityIds)
        {
            if (!TryData(id, out var d)) continue;
            int lv = GetLevel(id);
            foreach (var (stat, bonus) in StatPairs(d))
            {
                float v = bonus * lv;   // 슬38: 단계(Level)만큼 수치 증가
                switch (stat)
                {
                    case "PhysicalAtk": c.PhysicalAtk += v; break;
                    case "MagicAtk":    c.MagicAtk    += v; break;
                    case "CritRate":    c.CritRate    += v; break;
                    case "MaxHp":       c.MaxHp       += v; break;
                    case "AtkSpeed":    c.AtkSpeed    += v; break;
                    case "Accuracy":    c.Accuracy    += v; break;
                    case "Def":         c.Def         += v; break;
                    case "Evasion":     c.Evasion     += v; break;
                    // MaxMemory/InterestCapacity/CatchRate 등은 크리처 스탯 아님 — 전용 프로퍼티에서 처리
                }
            }
        }
    }
}
