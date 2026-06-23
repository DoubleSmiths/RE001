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
//  ※ 발동조건 데이터는 SkillData(시드 JSON). 연속 n회/사망조건 등 stateful 조건은 후속.
public class AbilityRunner : MonoBehaviour
{
    PlayerController _player;

    readonly Dictionary<int, float> _cooldowns = new();   // 시간발동 능력별 남은 쿨타임
    readonly HashSet<int> _firedLowHp = new();            // LowHp: 전투당 1회 발동 추적

    class TempBuff { public int id; public string stat; public float value; public float expire; }
    readonly List<TempBuff> _buffs = new();               // 활성 임시 버프

    bool _inBattle;

    public void Setup(PlayerController player) => _player = player;

    // ── 전투 시작/종료 ──
    public void OnBattleStart()
    {
        _inBattle = true;
        _firedLowHp.Clear();
        _cooldowns.Clear();   // 전투 시작 시 쿨 초기화(즉시 발동 가능)
        Notify(AbilityCondition.BattleStart);
    }

    public void OnBattleEnd()
    {
        _inBattle = false;
        ClearBuffs();
        _cooldowns.Clear();
    }

    // RefreshStats가 스탯을 base+패시브로 리셋한 직후 호출 — 임시버프 추적만 비움(환불 X, 이미 리셋됨).
    public void OnStatsReset()
    {
        if (_player != null)
            foreach (var b in _buffs) _player.RaiseBuffCleared(b.id);   // 슬40: 효과가 리셋됐으니 아이콘도 제거
        _buffs.Clear();
    }

    // ── 확률 발동 조건 통지 (PlayerController 훅에서 호출) ──
    public void Notify(AbilityCondition cond, float param = 0f)
    {
        if (Managers.Ability == null || Managers.Game == null || _player == null) return;

        foreach (int id in Managers.Game.EquippedAbilities)
        {
            if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) continue;
            if (d.TriggerType != AbilityTrigger.Probability) continue;
            if (d.Conditions == null || !d.Conditions.Contains(cond)) continue;

            if (cond == AbilityCondition.LowHp)
            {
                if (param > d.ConditionValue) continue;     // HP% 임계 초과면 미발동
                if (_firedLowHp.Contains(id)) continue;     // 전투당 1회
            }

            if (Random.value * 100f > Mathf.Max(0f, d.ProcChance)) continue;   // n% 굴림

            if (cond == AbilityCondition.LowHp) _firedLowHp.Add(id);
            ApplyEffect(d);
        }
    }

    void Update()
    {
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

        foreach (int id in Managers.Game.EquippedAbilities)
        {
            if (!Managers.Data.SkillDic.TryGetValue(id, out var d)) continue;
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
    }

    bool ContextOk(AbilityContext ctx)
        => ctx == AbilityContext.Always
        || (ctx == AbilityContext.InBattle && _inBattle)
        || (ctx == AbilityContext.OutOfBattle && !_inBattle);

    // ── 효과 적용 ──
    void ApplyEffect(SkillData d)
    {
        switch (d.Effect)
        {
            case AbilityEffect.Heal:
                _player.Heal(d.EffectValue);
                break;

            case AbilityEffect.BonusDamage:
                var tgt = _player.CurrentTarget;
                if (tgt != null && tgt.State != State.Dead)
                    tgt.TakeHit(BattleMath.FromRawDamage(Mathf.RoundToInt(d.EffectValue)), _player);
                break;

            case AbilityEffect.Buff:
                ApplyBuff(d);
                break;
        }
        Debug.Log($"[Ability] 발동: {d.Name} ({d.TriggerType}) → {d.Effect} {d.EffectValue}");
    }

    void ApplyBuff(SkillData d)
    {
        if (string.IsNullOrEmpty(d.EffectStat) || d.EffectValue == 0f) return;

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
            AddStat(d.EffectStat, d.EffectValue);
            // EffectDuration 0 = 전투 종료까지 유지(OnBattleEnd에서 ClearBuffs로 환불)
            _buffs.Add(new TempBuff { id = d.DataId, stat = d.EffectStat, value = d.EffectValue, expire = expire });
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
        }
    }
}
