using System.Collections;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    public event System.Action<float, float> OnHpChanged;   // current, max
    public event System.Action<float, float> OnExpChanged;  // current, required
    public event System.Action<int>          OnLevelUp;

    float _expRequired = 100f;
    bool _inBattle;

    // ── 플레이어 전용 스탯 ──
    public float GrowthRate = 100f;  // 성장률 % (경험치 획득량에 곱함)
    public float DropRate   = 100f;  // 획득률 % (몬스터 드롭률에 곱함)

    // ── 스탯 (레벨 / 장비 반영) ──
    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = ObjectType.Player;
        State = Define.State.Idle;
        RefreshStats();
        return true;
    }

    public void StartBattle() { _inBattle = true; }
    public void StopBattle()  { _inBattle = false; State = Define.State.Moving; }

    // ────────────────────────────────────────
    //  Update
    // ────────────────────────────────────────
    void Update()
    {
        if (State == Define.State.Dead) return;
        if (_inBattle) AutoAttack();
    }

    void FixedUpdate()
    {
        if (State == Define.State.Dead) return;
        //MovePlayer();
    }

    // ────────────────────────────────────────
    //  이동 (가로 스크롤 – 오른쪽으로만 진행)
    // ────────────────────────────────────────
    void MovePlayer()
    {
        if (State == Define.State.Attack) return;
        //_rb.linearVelocity = new Vector2(MoveSpeed, _rb.linearVelocity.y);
        State = Define.State.Moving;
    }

    // ────────────────────────────────────────
    //  자동 공격
    // ────────────────────────────────────────
    void AutoAttack()
    {
        if (State == Define.State.Attack) return;
        if (Time.time < _lastAtkTime + AtkCooldown) return;

        MonsterController nearest = FindNearestMonster();
        if (nearest == null) return;

        float dist = Vector2.Distance(transform.position, nearest.transform.position);
        if (dist > AtkRange) return;

        _lastAtkTime = Time.time;
        State = Define.State.Attack;
        // 명중률 판정 (100% 이상이면 skip)
        if (Accuracy < 100f && UnityEngine.Random.value > Accuracy / 100f) return;

        // 치명타 판정
        bool isCrit = UnityEngine.Random.value < Mathf.Min(CritRate, 100f) / 100f;
        float finalAtk = PhysicalAtk * (isCrit ? CritDamage / 100f : 1f);
        nearest.OnDamaged(finalAtk);
        StartCoroutine(CoResetStateAfterAttack());
    }

    MonsterController FindNearestMonster()
    {
        MonsterController[] monsters = FindObjectsByType<MonsterController>(FindObjectsSortMode.None);
        MonsterController nearest = null;
        float minDist = float.MaxValue;
        foreach (var m in monsters)
        {
            if (m.State == Define.State.Dead) continue;
            float d = Vector2.Distance(transform.position, m.transform.position);
            if (d < minDist) { minDist = d; nearest = m; }
        }
        return nearest;
    }

    IEnumerator CoResetStateAfterAttack()
    {
        yield return null;

        // Attack 애니메이션 진입 대기 (타임아웃 1초 → Animator에 Attack 상태 없어도 탈출)
        float timeout = Time.time + 1f;
        while (_anim != null && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (Time.time >= timeout) break;
            yield return null;
        }

        // Attack 애니메이션 완료 대기
        while (_anim != null &&
               _anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
               _anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        if (State != Define.State.Dead)
            State = Define.State.Idle;
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
        Managers.Game.Exp += amount * (GrowthRate / 100f);
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
        MaxHp       = 100 + (lv - 1) * 20;
        PhysicalAtk = 10  + (lv - 1) * 2;
        MagicAtk    = 10  + (lv - 1) * 2;
        Def         = 0   + (lv - 1) * 1;

        Hp = MaxHp;
        OnHpChanged?.Invoke(Hp, MaxHp);
    }

    protected override void OnDead()
    {
        base.OnDead();
        // TODO: 게임 오버 연출
    }

    public void AttackTrigger()
    {

    }
}
