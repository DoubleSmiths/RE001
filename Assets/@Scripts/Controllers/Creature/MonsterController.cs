using System.Collections;
using DG.Tweening;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    public int DataId { get; private set; }
    public event System.Action<float, float> OnHpChanged; // current, max
    public event System.Action OnEngaged; // 첫 공격 시점

    // 슬라이드 49: 전투 드롭 통지 (dataId, count) — AddItem 와 별도로 UI 리스트 용.
    public static event System.Action<int, int> OnItemDropped;

    Data.CreatureData _data;
    PlayerController  _target;
    bool _hasEngaged;

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

        MaxHp           = _data.MaxHp;
        Hp              = _data.MaxHp;
        PhysicalAtk     = _data.PhysicalAtk;
        MagicAtk        = _data.MagicAtk;
        CritRate        = _data.CritRate;
        CritDamage      = _data.CritDamage;
        Accuracy        = _data.Accuracy;
        AtkSpeed        = _data.AtkSpeed;
        DmgAmp          = _data.DmgAmp;
        Def             = _data.Def;
        PhysicalResist  = _data.PhysicalResist;
        MagicResist     = _data.MagicResist;
        Evasion         = _data.Evasion;
        RecoveryRate    = _data.RecoveryRate;
        MoveSpeed       = _data.MoveSpeed;
        MonsterAttackDelay = _data.MonsterAttackDelay;
        if (System.Enum.TryParse(_data.Rarity, true, out CreatureRarity r))
            Rarity = r;
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
        if (State == Define.State.Dead) return;
        if (_target == null) return;

        float dist = Vector2.Distance(transform.position, _target.transform.position);

        if (dist <= AtkRange)
            TryAttack();
        else
            ChaseTarget();
    }

    void ChaseTarget()
    {
        if (State == Define.State.Attack) return;
        Vector2 dir = ((Vector2)_target.transform.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = new Vector2(dir.x * MoveSpeed, _rb.linearVelocity.y);
        //FlipToward(_target.transform.position.x);
        State = Define.State.Idle;
    }

    void TryAttack()
    {
        // 슬라이드 53: 몬스터 공격속도는 Attack Anim 재생 전 대기 시간(MonsterAttackDelay)에 영향
        if (Time.time < _lastAtkTime + MonsterAttackDelay) return;
        _lastAtkTime = Time.time;
        State = Define.State.Attack;
        _rb.linearVelocity = Vector2.zero;

        BattleMath.DamageResult result = BattleMath.Resolve(this, _target, AttackType.Physical);
        _target.TakeHit(result, this);

        if (!_hasEngaged)
        {
            _hasEngaged = true;
            OnEngaged?.Invoke();
        }

        StartCoroutine(CoResetStateAfterAttack(0.4f));
    }

    IEnumerator CoResetStateAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (State != Define.State.Dead)
            State = Define.State.Moving;
    }

    // ────────────────────────────────────────
    //  피격 / 사망
    // ────────────────────────────────────────
    public override void TakeHit(BattleMath.DamageResult result, CreatureController attacker = null)
    {
        base.TakeHit(result, attacker);
        OnHpChanged?.Invoke(Hp, MaxHp);
    }

    // 슬라이드 64 — Attack Anim 도중 피격 시 OnHit 처리는 하되 OnHit Anim/색 변환 없음.
    // (OnHit 처리 = HP 차감 등은 base.TakeHit에서 이미 끝남)
    protected override void PlayHitFeedback()
    {
        if (State == Define.State.Dead) return;
        if (State == Define.State.Attack) return;
        base.PlayHitFeedback();
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
        {
            Managers.Game.AddItem(_data.DropItemId, _data.DropCount);
            OnItemDropped?.Invoke(_data.DropItemId, _data.DropCount);
        }

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
