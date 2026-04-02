using System.Collections;
using UnityEngine;
using static Define;

public abstract class CreatureController : BaseController
{
    protected Rigidbody2D _rb;
    protected Animator _anim;
    protected SpriteRenderer _sprite;

    [Header("Stats")]
    public float MaxHp   = 100f;
    public float Hp      = 100f;
    public float Atk     = 10f;
    public float Def     = 0f;
    //public float MoveSpeed = 3f;
    public float AtkRange  = 1.5f;
    public float AtkCooldown = 1f;

    protected float _lastAtkTime = -999f;

    protected State _state = Define.State.Idle;
    //public State State
    //{
    //    get => _state;
    //    set
    //    {
    //        if (_state == value) return;
    //        _state = value;
    //        UpdateAnimation();
    //    }
    //}

    public virtual Define.State State
    {
        get { return _state; }
        set
        {
            _state = value;

            Animator anim = GetComponent<Animator>();
            switch (_state)
            {
                case Define.State.Dead:
                    break;
                case Define.State.Idle:
                    anim.CrossFade("Idle", 0.1f);
                    break;
                case Define.State.Moving:
                    anim.CrossFade("Run", 0.1f);
                    break;
                case Define.State.Attack:
                    anim.CrossFade("Attack", 0.1f, -1, 0);
                    break;
            }
        }
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        _rb     = GetComponent<Rigidbody2D>();
        _anim   = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        return true;
    }

    protected virtual void UpdateAnimation()
    {
        if (_anim == null) return;
        _anim.SetBool("IsMoving", _state == Define.State.Moving);
        _anim.SetBool("IsDead", _state == Define.State.Dead);
        if (_state == Define.State.Attack)
            _anim.SetTrigger("Attack");
        if (_state == Define.State.OnDamaged)
            _anim.SetTrigger("OnDamaged");
    }

    public virtual void OnDamaged(float damage)
    {
        float dmg = Mathf.Max(0, damage - Def);
        Hp -= dmg;
        Managers.Object.ShowDamageFont(transform.position, dmg, transform, isCritical: false);

        if (Hp <= 0)
        {
            Hp = 0;
            OnDead();
            return;
        }

        StartCoroutine(CoFlashDamage());
    }

    protected virtual void OnDead()
    {
        State = Define.State.Dead;
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    IEnumerator CoFlashDamage()
    {
        if (_sprite == null) yield break;
        _sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _sprite.color = Color.white;
    }

    protected void FlipToward(float targetX)
    {
        if (_sprite == null) return;
        _sprite.flipX = targetX < transform.position.x;
    }
}
