using System.Collections;
using UnityEngine;
using static Define;
using static Util;

public abstract class CreatureController : BaseController
{
    protected Rigidbody2D _rb;
    protected Animator _anim;
    protected SpriteRenderer _sprite;

    [Header("Stats")]
    public float MaxHp          = 100f;
    public float Hp             = 100f;
    // ── 공격 ──
    public float PhysicalAtk    = 10f;   // 물리공격력 [기본]
    public float MagicAtk       = 10f;   // 마법공격력 [기본]
    public float CritRate       = 0f;    // 치명타 % (표기 상한 없음, 적용은 100%까지)
    public float CritDamage     = 150f;  // 치명타피해 %
    public float Accuracy       = 80f;   // 명중률 %
    public float AtkSpeed       = 100f;  // 공격속도 %
    public float DmgAmp         = 0f;    // 피해증폭 % — 식: FINAL = DMG * (1 + DmgAmp/100)

    // ── 방어 ──
    public float Def            = 0f;    // 방어력 [기본]
    public float PhysicalResist = 0f;    // 물리저항 %
    public float MagicResist    = 0f;    // 마법저항 %
    public float Evasion        = 0f;    // 회피율 %
    public float RecoveryRate   = 0f;    // 회복력 % (체력 회복 효과에 곱함)

    // ── 메타 ──
    public CreatureRarity Rarity        = CreatureRarity.Common;
    public float          MonsterAttackDelay = 1.5f; // 몬스터 전용. Player는 사용 안 함.

    // ── 증가율 / 증폭률 (장비·버프로 누적) ──
    // 최종 = 기본 * (1 + 증가율/100) * 증폭률
    [Header("Rate & Amp")]
    public float PhysicalAtkRate = 0f;   // 물리공격력 증가율 %  (+n% 표기)
    public float PhysicalAtkAmp  = 1f;   // 물리공격력 증폭률     (xn  표기)
    public float MagicAtkRate    = 0f;
    public float MagicAtkAmp     = 1f;
    public float MaxHpRate       = 0f;
    public float MaxHpAmp        = 1f;
    public float DefRate         = 0f;
    public float DefAmp          = 1f;

    // ── 최종 능력치 프로퍼티 ──
    public float FinalPhysicalAtk => PhysicalAtk * (1f + PhysicalAtkRate / 100f) * PhysicalAtkAmp;
    public float FinalMagicAtk    => MagicAtk    * (1f + MagicAtkRate    / 100f) * MagicAtkAmp;
    public float FinalMaxHp       => MaxHp       * (1f + MaxHpRate        / 100f) * MaxHpAmp;
    public float FinalDef         => Def         * (1f + DefRate          / 100f) * DefAmp;

    // ── 이동/전투 ──
    public float MoveSpeed      = 2f;
    public float AtkRange       = 1.5f;
    public float AtkCooldown    = 1f;

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

        _rb     = gameObject.GetOrAddComponent<Rigidbody2D>();
        _anim   = gameObject.GetOrAddComponent<Animator>();
        _sprite = gameObject.GetOrAddComponent<SpriteRenderer>();
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

    // 기획서 #1 데미지 식의 결과(BattleMath.DamageResult)를 적용한다.
    // Miss/HP 감소/사망 처리 + 슬라이드 62/63 Damage Textpopup 호출.
    // 슬라이드 63: Damage Textpopup은 Player/Minion 공격 시에만. Monster 공격은 popup 호출 X.
    public virtual void TakeHit(BattleMath.DamageResult result, CreatureController attacker = null)
    {
        if (!result.Hit)
        {
            // 슬라이드 62 — Miss Textpopup (피격자 위치). Sound는 후속.
            Managers.Object.ShowMissPopup(transform.position, transform);
            return;
        }

        Hp -= result.FinalDamage;
        if (Hp < 0) Hp = 0;

        // 슬라이드 63: Monster 공격은 Damage Textpopup 호출 안 함.
        bool attackerIsMonster = attacker != null && attacker.ObjectType == ObjectType.Monster;
        if (!attackerIsMonster)
        {
            DamageFontMode mode = result.IsCrit ? DamageFontMode.Critical : DamageFontMode.Normal;
            Managers.Object.ShowDamageFont(transform.position, result.FinalDamage, mode, transform);

            // 치명타 시 공격자 위치에 "Critical!" callout (Player만 해당)
            if (result.IsCrit && attacker != null && attacker.ObjectType == ObjectType.Player)
                Managers.Object.ShowCriticalCallout(attacker.transform.position, attacker.transform);
        }

        if (Hp <= 0)
        {
            OnDead();
            return;
        }

        StartCoroutine(CoFlashDamage());
    }

    // ProjectileController 등 사전 계산된 데미지를 그대로 적용하는 호출용.
    // 기존 OnDamaged(float) 호환 shim — 명중/회피 dice 없이 바로 final로 처리.
    public virtual void OnDamaged(float damage, bool isPhysical = true)
    {
        TakeHit(BattleMath.FromRawDamage(Mathf.RoundToInt(damage)), null);
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
