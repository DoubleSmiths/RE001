using System.Collections;
using DG.Tweening;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    public int DataId { get; private set; }

    Data.CreatureData _data;
    PlayerController  _target;

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = ObjectType.Monster;
        return true;
    }

    public void SetInfo(int dataId)
    {
        DataId = dataId;
        if (!Managers.Data.CreatureDic.TryGetValue(dataId, out _data)) return;

        MaxHp = _data.MaxHp;
        Hp    = _data.MaxHp;
        Atk   = _data.Atk;
        Def   = _data.Def;
    }

    void Start()
    {
        _target = FindFirstObjectByType<PlayerController>();
    }

    // ────────────────────────────────────────
    //  AI
    // ────────────────────────────────────────
    void FixedUpdate()
    {
        if (State == CreatureState.Dead) return;
        if (_target == null) return;

        float dist = Vector2.Distance(transform.position, _target.transform.position);

        if (dist <= AtkRange)
            TryAttack();
        else
            ChaseTarget();
    }

    void ChaseTarget()
    {
        if (State == CreatureState.Attack) return;
        Vector2 dir = ((Vector2)_target.transform.position - (Vector2)transform.position).normalized;
        //_rb.linearVelocity = new Vector2(dir.x * MoveSpeed, _rb.linearVelocity.y);
        FlipToward(_target.transform.position.x);
        State = CreatureState.Moving;
    }

    void TryAttack()
    {
        if (Time.time < _lastAtkTime + AtkCooldown) return;
        _lastAtkTime = Time.time;
        State = CreatureState.Attack;
        _rb.linearVelocity = Vector2.zero;
        _target.OnDamaged(Atk);
        StartCoroutine(CoResetStateAfterAttack(0.4f));
    }

    IEnumerator CoResetStateAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (State != CreatureState.Dead)
            State = CreatureState.Moving;
    }

    // ────────────────────────────────────────
    //  피격 / 사망
    // ────────────────────────────────────────
    public override void OnDamaged(float damage)
    {
        base.OnDamaged(damage);
    }

    protected override void OnDead()
    {
        base.OnDead();
        _rb.linearVelocity = Vector2.zero;

        // 보상
        if (_target != null)
        {
            if (_data != null) Managers.Game.Gold += _data.RewardGold;
            _target.GainExp(_data?.TotalExp ?? 0f);
        }

        // 아이템 드롭
        if (_data != null && _data.DropItemId > 0 && Random.value <= _data.DropChance)
            Managers.Game.AddItem(_data.DropItemId, _data.DropCount);

        // 사망 연출 후 풀 반환
        transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Managers.Resource.Destroy(gameObject));
    }

    // ────────────────────────────────────────
    //  넉백
    // ────────────────────────────────────────
    public void KnockBack(Vector2 dir, float force)
    {
        StartCoroutine(CoKnockBack(dir, force));
    }

    IEnumerator CoKnockBack(Vector2 dir, float force)
    {
        _rb.AddForce(dir * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.15f);
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }
}
