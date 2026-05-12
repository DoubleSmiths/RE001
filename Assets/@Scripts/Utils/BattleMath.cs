using UnityEngine;
using static Define;

// 기획서v2_05.04 슬라이드 53 — 데미지 처리 식의 단일 진입점.
// 1. 명중 dice  : (공격자 명중률 - 피격자 회피율) % 확률로 타격 성공.
// 2-a. BASE     : (atk * skillMul) * (crit ? CritDamage/100 : 1)
// 2-b. DMG      : BASE - BASE*(저항/100) - 방어력
// 2-c. FINAL    : DMG * (1 + DmgAmp/100) * Random(0.8, 1.2), 1 미만은 1로 floor
// HP 차감/사망 처리는 호출 측 (CreatureController.TakeHit) 책임.
public static class BattleMath
{
    public struct DamageResult
    {
        public bool Hit;          // false면 빗나감 (Miss)
        public bool IsCrit;       // 치명타 여부
        public int  FinalDamage;  // FINAL DMG (Miss면 0)
    }

#if UNITY_EDITOR
    // 테스트 토글 — true면 Player가 공격자인 모든 공격이 무조건 크리티컬로 적용.
    // BattleTestMenu의 단축키(Ctrl+Shift+C)에서 ON/OFF.
    public static bool DebugForcePlayerCritical = false;
#endif

    public static DamageResult Resolve(
        CreatureController attacker,
        CreatureController defender,
        AttackType attackType = AttackType.Physical,
        float skillMul = 1f)
    {
        DamageResult r = default;

        // 1. 명중 주사위
        float hitChance = Mathf.Clamp(attacker.Accuracy - defender.Evasion, 0f, 100f);
        if (Random.value * 100f >= hitChance)
            return r; // Hit=false, FinalDamage=0
        r.Hit = true;

        // 2-a. BASE
        float atk = attackType == AttackType.Physical ? attacker.FinalPhysicalAtk : attacker.FinalMagicAtk;
        bool isCrit = Random.value * 100f < Mathf.Min(attacker.CritRate, 100f);
#if UNITY_EDITOR
        // 테스트 토글: Player 공격은 항상 크리티컬
        if (DebugForcePlayerCritical && attacker.ObjectType == ObjectType.Player)
            isCrit = true;
#endif
        float baseDmg = atk * skillMul * (isCrit ? attacker.CritDamage / 100f : 1f);
        r.IsCrit = isCrit;

        // 2-b. DMG (저항 % + 방어력)
        float resist = attackType == AttackType.Physical ? defender.PhysicalResist : defender.MagicResist;
        resist = Mathf.Min(resist, 100f);
        float dmg = baseDmg - baseDmg * (resist / 100f) - defender.FinalDef;

        // 2-c. FINAL = DMG * (1+피해증폭) * Random(0.8, 1.2), floor 1
        float final = dmg * (1f + attacker.DmgAmp / 100f) * Random.Range(0.8f, 1.2f);
        if (final < 1f) final = 1f;

        r.FinalDamage = Mathf.RoundToInt(final);
        return r;
    }

    // 사전 계산된 데미지(예: 투사체)를 DamageResult 형태로 감싸는 헬퍼.
    // 명중·회피·치명타 dice를 거치지 않고 final로 바로 처리할 때 사용.
    public static DamageResult FromRawDamage(int finalDamage, bool isCrit = false)
    {
        return new DamageResult
        {
            Hit         = true,
            IsCrit      = isCrit,
            FinalDamage = Mathf.Max(1, finalDamage),
        };
    }
}
