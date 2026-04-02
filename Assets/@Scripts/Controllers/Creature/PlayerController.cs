using System.Collections;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    public event System.Action<float, float> OnHpChanged;   // current, max
    public event System.Action<float, float> OnExpChanged;  // current, required
    public event System.Action<int>          OnLevelUp;

    float _expRequired = 100f;

    // ── 스탯 (레벨 / 장비 반영) ──
    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = ObjectType.Player;
        RefreshStats();
        return true;
    }

    // ────────────────────────────────────────
    //  Update
    // ────────────────────────────────────────
    void Update()
    {
        if (State == CreatureState.Dead) return;
        AutoAttack();
    }

    void FixedUpdate()
    {
        if (State == CreatureState.Dead) return;
        MovePlayer();
    }

    // ────────────────────────────────────────
    //  이동 (가로 스크롤 – 오른쪽으로만 진행)
    // ────────────────────────────────────────
    void MovePlayer()
    {
        if (State == CreatureState.Attack) return;
        //_rb.linearVelocity = new Vector2(MoveSpeed, _rb.linearVelocity.y);
        State = CreatureState.Moving;
    }

    // ────────────────────────────────────────
    //  자동 공격
    // ────────────────────────────────────────
    void AutoAttack()
    {
        if (Time.time < _lastAtkTime + AtkCooldown) return;

        MonsterController nearest = FindNearestMonster();
        if (nearest == null) return;

        float dist = Vector2.Distance(transform.position, nearest.transform.position);
        if (dist > AtkRange) return;

        _lastAtkTime = Time.time;
        State = CreatureState.Attack;
        nearest.OnDamaged(Atk);
        StartCoroutine(CoResetStateAfterAttack(0.3f));
    }

    MonsterController FindNearestMonster()
    {
        MonsterController[] monsters = FindObjectsByType<MonsterController>(FindObjectsSortMode.None);
        MonsterController nearest = null;
        float minDist = float.MaxValue;
        foreach (var m in monsters)
        {
            if (m.State == CreatureState.Dead) continue;
            float d = Vector2.Distance(transform.position, m.transform.position);
            if (d < minDist) { minDist = d; nearest = m; }
        }
        return nearest;
    }

    IEnumerator CoResetStateAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (State != CreatureState.Dead)
            State = CreatureState.Moving;
    }

    // ────────────────────────────────────────
    //  HP / EXP
    // ────────────────────────────────────────
    public override void OnDamaged(float damage)
    {
        base.OnDamaged(damage);
        OnHpChanged?.Invoke(Hp, MaxHp);
    }

    public void GainExp(float amount)
    {
        Managers.Game.Exp += amount;
        CheckLevelUp();
        OnExpChanged?.Invoke(Managers.Game.Exp, _expRequired);
    }

    void CheckLevelUp()
    {
        while (Managers.Game.Exp >= _expRequired)
        {
            Managers.Game.Exp  -= _expRequired;
            Managers.Game.Level++;
            RefreshStats();
            OnLevelUp?.Invoke(Managers.Game.Level);
            _expRequired *= 1.2f;   // 단순 성장 곡선 (LevelData 없을 때 fallback)
        }
    }

    // ────────────────────────────────────────
    //  스탯 갱신
    // ────────────────────────────────────────
    public void RefreshStats()
    {
        int lv = Managers.Game.Level;

        // LevelData 기반 필요 경험치
        if (Managers.Data != null &&
            Managers.Data.LevelDic != null &&
            Managers.Data.LevelDic.TryGetValue(lv, out var lvData))
        {
            _expRequired = lvData.RequiredExp;
        }
        else
        {
            _expRequired = 100 * Mathf.Pow(1.2f, lv - 1);
        }

        // 기본 성장식으로 스탯 계산
        MaxHp = 100 + (lv - 1) * 20;
        Atk   = 10  + (lv - 1) * 2;
        Def   = 0   + (lv - 1) * 1;

        Hp = MaxHp;
        OnHpChanged?.Invoke(Hp, MaxHp);
    }

    protected override void OnDead()
    {
        base.OnDead();
        // TODO: 게임 오버 연출
    }
}
