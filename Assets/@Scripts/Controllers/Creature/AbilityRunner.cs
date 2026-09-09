using System.Collections.Generic;
using Data;
using UnityEngine;
using static Define;

// [능력 전투 발동조건] 런너 (기획서 슬39). PlayerController에 런타임 부착.
//  - 장착(기억)된 능력 중 TriggerType != None 인 것을 전투 이벤트/쿨타임에 따라 발동.
//  - 확률 발동(Probability): 조건 이벤트(공격/타격/치명타/피격/회피/회복/저체력/전투시작) 발생 →
//                            Conditions에 그 조건이 있으면 ProcChance% 굴려 Effect 적용.
//  - 시간 발동(Time): ActiveContext(InBattle/OutOfBattle/Always) 충족 시 Cooldown마다 Effect 적용.
//  - 효과(Effect): Heal(본인 회복) / BonusDamage(현재 타겟 추가피해) / Buff(본인 스탯 일시 증가).
//  - stateful 조건(연속n치명타/연속n회피 등)의 카운터는 PlayerController가 세고 param으로 전달.
//  ※ 발동조건 데이터는 SkillData(시드 JSON).
public class AbilityRunner : MonoBehaviour
{
    PlayerController _player;

    readonly Dictionary<int, float> _cooldowns = new();   // 시간발동 능력별 남은 쿨타임
    readonly HashSet<int> _firedLowHp = new();            // LowHp: 전투당 1회 발동 추적
    readonly HashSet<int> _firedEnemyLowHp = new();       // EnemyLowHp: 전투당 1회 발동 추적
    bool _inAfterNotify;                                  // AfterAbilityUse 재귀(능력→능력 무한 연쇄) 가드 — 1단만 허용

    class TempBuff { public int id; public string stat; public float value; public float expire; }
    readonly List<TempBuff> _buffs = new();               // 활성 임시 버프

    bool _inBattle;
    float _minionCooldown;
    float _minionCooldownDuration;

    public void Setup(PlayerController player) => _player = player;

    // ── 전투 시작/종료 ──
    public void OnBattleStart()
    {
        _inBattle = true;
        _firedLowHp.Clear();
        _firedEnemyLowHp.Clear();
        _cooldowns.Clear();   // 전투 시작 시 쿨 초기화(즉시 발동 가능)
        Notify(AbilityCondition.BattleStart);
    }

    public void OnBattleEnd()
    {
        _inBattle = false;
        ClearBuffs();
        _cooldowns.Clear();
    }

    // 스탯 재계산 후에도 사용 중인 능력의 남은 버프는 유지한다. 해제된 능력만 제거한다.
    public void OnStatsReset()
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            var buff = _buffs[i];
            if (Time.time < buff.expire && Managers.Ability.IsEquipped(buff.id))
            {
                AddStat(buff.stat, buff.value);
                continue;
            }
            _buffs.RemoveAt(i);
            if (_player != null) _player.RaiseBuffCleared(buff.id);
        }
    }

    // ── 확률 발동 조건 통지 (PlayerController 훅에서 호출) ──
    //  param 의미: LowHp/EnemyLowHp=HP% / ConsecutiveCrit·Evade=현재 연속 횟수 / AfterAbilityUse=발동한 능력 DataId
    public void Notify(AbilityCondition cond, float param = 0f)
    {
        if (Managers.Ability == null || Managers.Game == null || _player == null) return;

        foreach (int id in Managers.Ability.ActiveAbilityIds)
        {
            if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) continue;
            if (d.TriggerType != AbilityTrigger.Probability) continue;
            if (d.Conditions == null || !d.Conditions.Contains(cond)) continue;

            switch (cond)
            {
                case AbilityCondition.LowHp:
                    if (param > d.ConditionValue) continue;     // HP% 임계 초과면 미발동
                    if (_firedLowHp.Contains(id)) continue;     // 전투당 1회
                    break;
                case AbilityCondition.EnemyLowHp:
                    if (param > d.ConditionValue) continue;
                    if (_firedEnemyLowHp.Contains(id)) continue; // 전투당 1회
                    break;
                case AbilityCondition.ConsecutiveCrit:
                case AbilityCondition.ConsecutiveEvade:
                    // 연속 횟수가 정확히 n(ConditionValue)이 되는 순간 1회 발동 — 끊기면 카운터 리셋 후 재무장
                    if (Mathf.RoundToInt(param) != Mathf.Max(1, Mathf.RoundToInt(d.ConditionValue))) continue;
                    break;
                case AbilityCondition.AfterAbilityUse:
                    // ConditionValue = 대상 능력 DataId (0이면 아무 능력 발동 후)
                    if (d.ConditionValue > 0f && Mathf.RoundToInt(param) != Mathf.RoundToInt(d.ConditionValue)) continue;
                    if (Mathf.RoundToInt(param) == d.DataId) continue;   // 자기 자신 발동에는 미반응
                    break;
            }

            if (Random.value * 100f > Mathf.Max(0f, d.ProcChance)) continue;   // n% 굴림

            if (cond == AbilityCondition.LowHp)      _firedLowHp.Add(id);
            if (cond == AbilityCondition.EnemyLowHp) _firedEnemyLowHp.Add(id);
            ApplyEffect(d);
        }
    }

    void Update()
    {
        // 동행 교체나 같은 계열 능력 여러 개 기억으로 재사용 대기를 우회하지 않는다.
        _minionCooldown = Mathf.Max(0f, _minionCooldown - Time.deltaTime);
        // 임시 버프 만료
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            if (Time.time < _buffs[i].expire) continue;
            AddStat(_buffs[i].stat, -_buffs[i].value);
            int doneId = _buffs[i].id;
            _buffs.RemoveAt(i);
            _player.RaiseBuffCleared(doneId);   // 슬40: 좌측상단 버프 아이콘 제거(FadeOut)
        }

        // 시간 발동 능력
        if (Managers.Ability == null || Managers.Game == null || _player == null) return;
        if (_player.State == State.Dead) return;

        bool anyActiveReady = false;
        foreach (int id in Managers.Ability.ActiveAbilityIds)
        {
            if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) continue;

            // 액티브: 쿨만 깎는다. 발동은 UseSkill(플레이어 입력)이 한다.
            if (d.TriggerType == AbilityTrigger.Active)
            {
                float rem = _cooldowns.TryGetValue(id, out var a) ? a : 0f;
                if (rem <= 0f) continue;
                rem -= Time.deltaTime;
                _cooldowns[id] = Mathf.Max(0f, rem);
                if (rem <= 0f) anyActiveReady = true;   // 방금 쿨이 돌았다 → 스킬바 갱신
                continue;
            }

            if (d.TriggerType != AbilityTrigger.Time) continue;
            if (!ContextOk(d.ActiveContext)) continue;

            float cd = _cooldowns.TryGetValue(id, out var v) ? v : 0f;
            cd -= Time.deltaTime;
            if (cd <= 0f)
            {
                ApplyEffect(d);
                cd = Mathf.Max(0.1f, d.Cooldown);
            }
            _cooldowns[id] = cd;
        }
        if (anyActiveReady) OnActiveSkillChanged?.Invoke();
    }

    // ── 액티브 스킬 (플레이어가 직접 발동) ──
    //  확률/시간 발동과 효과 처리(ApplyEffect)를 그대로 공유한다. 다른 점은 발동 주체와
    //  쿨타임을 **누가 돌리느냐**뿐 — 시간발동은 Update가 자동으로, 액티브는 UseSkill이 찍는다.
    public event System.Action OnActiveSkillChanged;   // 슬롯 구성/쿨 상태 변동 → 스킬바 갱신

    /// 스킬바에 올라갈 능력들 (장착된 것 중 TriggerType==Active). 슬롯 수만큼만.
    public List<int> ActiveSkillIds
    {
        get
        {
            var list = new List<int>();
            if (Managers.Ability == null || Managers.Data?.SkillDic == null) return list;
            foreach (int id in Managers.Ability.ActiveAbilityIds)
            {
                if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) continue;
                if (d.TriggerType != AbilityTrigger.Active) continue;
                list.Add(id);
                if (list.Count >= ACTIVE_SKILL_SLOTS) break;
            }
            return list;
        }
    }

    /// 남은 쿨타임(초). 0이면 사용 가능.
    public float GetCooldownRemain(int id)
    {
        if (IsMinionActive(id)) return _minionCooldown;
        return _cooldowns.TryGetValue(id, out var v) ? Mathf.Max(0f, v) : 0f;
    }

    bool IsMinionActive(int id)
    {
        return Managers.Data.SkillDic.TryGetValue(id, out var d)
            && d.Source == AbilitySource.Minion && d.TriggerType == AbilityTrigger.Active;
    }

    /// 쿨 진행률 0~1 (1 = 다 참). 스킬바 필 게이지용.
    public float GetCooldownRatio(int id)
    {
        if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) return 1f;
        float total = Mathf.Max(0.1f, IsMinionActive(id) && _minionCooldownDuration > 0f
            ? _minionCooldownDuration : d.Cooldown);
        return Mathf.Clamp01(1f - GetCooldownRemain(id) / total);
    }

    public bool CanUse(int id)
    {
        if (_player == null || _player.State == State.Dead) return false;
        if (Managers.Ability == null || !Managers.Ability.IsEquipped(id)) return false;
        if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) return false;
        if (d.TriggerType != AbilityTrigger.Active) return false;
        if (!ContextOk(d.ActiveContext)) return false;
        if (d.Effect == AbilityEffect.BonusDamage
            && (_player.CurrentTarget == null || _player.CurrentTarget.State == State.Dead)) return false;
        if (d.Effect == AbilityEffect.Heal && _player.Hp >= _player.MaxHp) return false;
        return GetCooldownRemain(id) <= 0f;
    }

    /// 액티브 스킬 발동. 쓸 수 없는 상태면 false(쿨/사망/전투 밖 등) — 호출부가 사유를 물으면 CanUse로 갈라본다.
    public bool UseSkill(int id)
    {
        if (!CanUse(id)) return false;
        var d = Managers.Data.SkillDic[id];
        ApplyEffect(d);
        if (IsMinionActive(id))
        {
            _minionCooldown = Mathf.Max(0.1f, d.Cooldown);
            _minionCooldownDuration = _minionCooldown;
        }
        else
        {
            _cooldowns[id] = Mathf.Max(0.1f, d.Cooldown);
        }
        OnActiveSkillChanged?.Invoke();
        return true;
    }

    bool ContextOk(AbilityContext ctx)
        => ctx == AbilityContext.Always
        || (ctx == AbilityContext.InBattle && _inBattle)
        || (ctx == AbilityContext.OutOfBattle && !_inBattle);

    // ── 효과 적용 ── EffectValue는 능력 단계(Level)만큼 곱산.
    //  ponytail: ProcChance/EffectDuration은 단계 미반영(확률 스케일은 밸런스 급변 — per-level 컬럼 엑셀화 후속).
    void ApplyEffect(SkillData d)
    {
        int lv = Mathf.Max(1, Managers.Ability != null ? Managers.Ability.GetLevel(d.DataId) : 1);
        float value = GetEffectValue(d, lv);

        // 액티브 스킬은 전용 모션을 탄다. 효과 종류로 고른다 —
        //  때리는 스킬은 크게 휘두르고(Skill_Slash), 버프·회복은 자세를 잡는다(Skill_Cast).
        //  쿨이 긴 스킬일수록 느리고 묵직하게 재생해 체감 무게를 다르게 준다.
        if (d.TriggerType == AbilityTrigger.Active && _player != null)
        {
            string motion = d.Effect == AbilityEffect.BonusDamage ? "Skill_Slash" : "Skill_Cast";
            float speed = d.Cooldown >= 40f ? 0.7f : (d.Cooldown <= 10f ? 1.3f : 1f);
            _player.PlayMotion(motion, speed);
        }

        switch (d.Effect)
        {
            case AbilityEffect.Heal:
                _player.Heal(value);
                SpawnEffectVfx(d, _player.transform);
                break;

            case AbilityEffect.BonusDamage:
                var tgt = _player.CurrentTarget;
                if (tgt != null && tgt.State != State.Dead)
                {
                    tgt.TakeHit(BattleMath.FromRawDamage(Mathf.RoundToInt(value)), _player);
                    SpawnEffectVfx(d, tgt.transform);
                }
                break;

            case AbilityEffect.Buff:
                ApplyBuff(d, value);
                SpawnEffectVfx(d, _player.transform);
                break;
        }
        Debug.Log($"[Ability] 발동: {d.Name} Lv.{lv} ({d.TriggerType}) → {d.Effect} {value}");

        // 슬39: [특정 능력 사용 후] 조건 통지 — 능력→능력 연쇄는 1단만(무한 루프 가드)
        if (!_inAfterNotify)
        {
            _inAfterNotify = true;
            Notify(AbilityCondition.AfterAbilityUse, d.DataId);
            _inAfterNotify = false;
        }
    }

    // 신규 미니몬 능력은 성장 후에도 유효하도록 공격력/체력 비율을 사용한다.
    // EffectStat이 비어 있는 기존 공격·회복 데이터는 고정 수치 그대로 적용한다.
    public float GetEffectValue(SkillData data, int level)
    {
        float value = data.EffectValue * Mathf.Max(1, level);
        if (_player == null || data.Effect == AbilityEffect.Buff) return value;
        if (data.EffectStat == "PhysicalAtk") return value * _player.FinalPhysicalAtk / ABILITY_PERCENT_BASE;
        if (data.EffectStat == "MaxHp") return value * _player.MaxHp / ABILITY_PERCENT_BASE;
        return value;
    }

    // 슬39/능력#4: 발동 VFX — EffectVfxLabel 지정 시 그 프리팹, 비면 효과별 기본 매핑(기등록 어드레서블).
    static string DefaultVfx(AbilityEffect effect) => effect switch
    {
        AbilityEffect.Heal        => "HP Potion Ef",
        AbilityEffect.BonusDamage => "FX_hit_10",
        AbilityEffect.Buff        => "Star_01_Anim_13_a",
        _ => null,
    };

    void SpawnEffectVfx(SkillData d, Transform at)
    {
        if (at == null) return;
        string label = string.IsNullOrEmpty(d.EffectVfxLabel) ? DefaultVfx(d.Effect) : d.EffectVfxLabel;
        if (string.IsNullOrEmpty(label)) return;
        var go = Managers.Resource?.Instantiate(label);
        if (go == null) return;
        go.transform.position = at.position;
        Destroy(go, 1.5f);   // 짧은 원샷 연출 — 풀링 미사용 프리팹이라 일반 Destroy
    }

    void ApplyBuff(SkillData d, float value)
    {
        if (string.IsNullOrEmpty(d.EffectStat) || value == 0f) return;

        bool timed = d.EffectDuration > 0f;   // ③지속시간형 / false=②인게임지속(전투끝까지)
        float expire = timed ? Time.time + d.EffectDuration : float.MaxValue;

        // 동일 능력 재발동(시간발동 반복 등): 스탯 중복 가산 없이 만료시각만 갱신.
        var existing = _buffs.Find(b => b.id == d.DataId);
        if (existing != null)
        {
            existing.expire = expire;
        }
        else
        {
            AddStat(d.EffectStat, value);
            // EffectDuration 0 = 전투 종료까지 유지(OnBattleEnd에서 ClearBuffs로 환불)
            _buffs.Add(new TempBuff { id = d.DataId, stat = d.EffectStat, value = value, expire = expire });
        }
        // 슬40: 좌측상단 버프 아이콘 표시(②인게임지속=정적, ③지속시간형=게이지+점멸+FadeOut)
        _player.RaiseBuffApplied(new PlayerController.BuffInfo(d.DataId, d.IconLabel, d.EffectDuration, timed));
    }

    void ClearBuffs()
    {
        foreach (var b in _buffs)
        {
            AddStat(b.stat, -b.value);
            _player.RaiseBuffCleared(b.id);
        }
        _buffs.Clear();
    }

    // 임시 버프는 RefreshStats(base+패시브)와 충돌하지 않도록 라이브 스탯 필드에 직접 가감.
    void AddStat(string stat, float v)
    {
        switch (stat)
        {
            case "PhysicalAtk": _player.PhysicalAtk += v; break;
            case "MagicAtk":    _player.MagicAtk    += v; break;
            case "CritRate":    _player.CritRate    += v; break;
            case "AtkSpeed":    _player.AtkSpeed    += v; break;
            case "Accuracy":    _player.Accuracy    += v; break;
            case "Def":         _player.Def         += v; break;
            case "MaxHp":       _player.MaxHp        += v; break;
            case "Evasion":     _player.Evasion     += v; break;
            case "CritDamage":  _player.CritDamage  += v; break;
            case "PhysicalResist": _player.PhysicalResist += v; break;
        }
    }
}
