using System.Collections;
using DG.Tweening;
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
    public float          DamageFontYOffset  = 2.5f; // 데미지 폰트 띄울 위쪽 거리 (transform.position 기준)

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

            // 일회성 모션(액티브 스킬 등)이 도는 동안에는 애니메이터를 건드리지 않는다.
            //  전투 중에는 교전 여부에 따라 State가 Idle↔Moving으로 계속 오가는데, 그때마다
            //  CrossFade가 걸려 **스킬 모션이 시작하자마자 덮여 사라졌다**(마을에선 멀쩡한데
            //  전투에서만 안 나오던 이유). 상태 값은 그대로 기록해 두고, 모션이 끝나면
            //  CoEndMotion이 그 상태 모션으로 되돌린다.
            if (_motionCo != null) return;

            Animator anim = GetComponent<Animator>();
            switch (_state)
            {
                case Define.State.Dead:
                    anim.CrossFade("Die", 0.1f, -1, 0);   // 슬라이드 66/67 — 사망 시 Die clip 재생
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

    /// <summary>
    /// 지금 State에 해당하는 클립을 다시 재생한다(State 값은 그대로).
    /// 아키타입 애니메이터에는 트랜지션이 없어서, 비반복 클립(OnHit 등)이 끝난 뒤
    /// 원래 모션으로 돌아오려면 누군가 다시 CrossFade를 걸어 줘야 한다.
    /// </summary>
    public void ReplayStateAnimation() => State = _state;

    Coroutine _motionCo;

    /// <summary>
    /// State와 무관한 일회성 모션(액티브 스킬 등)을 재생하고, 끝나면 원래 상태 모션으로 되돌린다.
    /// 컨트롤러에 그 상태가 없으면 CrossFade가 조용히 무시되므로 아무 일도 일어나지 않는다(안전).
    /// </summary>
    public void PlayMotion(string stateName, float speed = 1f)
    {
        if (_anim == null || State == Define.State.Dead) return;

        float len = 0.4f;
        var rc = _anim.runtimeAnimatorController;
        if (rc != null)
            foreach (var c in rc.animationClips)
                if (c != null && c.name == stateName) { len = c.length; break; }

        _anim.speed = Mathf.Max(0.1f, speed);
        _anim.CrossFade(stateName, 0.05f, -1, 0f);

        if (_motionCo != null) StopCoroutine(_motionCo);
        _motionCo = StartCoroutine(CoEndMotion(len / Mathf.Max(0.1f, speed)));
    }

    IEnumerator CoEndMotion(float wait)
    {
        yield return new WaitForSeconds(wait);
        _motionCo = null;
        _anim.speed = 1f;
        if (State != Define.State.Dead) ReplayStateAnimation();
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        _rb     = gameObject.GetOrAddComponent<Rigidbody2D>();
        _anim   = gameObject.GetOrAddComponent<Animator>();
        _sprite = gameObject.GetOrAddComponent<SpriteRenderer>();
        return true;
    }

    // 외부 원본 슬라임 애니 클립(Slime(Green)_Attack/Run 등)에 박혀있는 AnimationEvent("TriggerUnityEvent")
    // 의 수신부. 우리 게임에선 사용하지 않으므로 빈 메서드로 둬 "has no receiver" 경고만 제거한다.
    public void TriggerUnityEvent() { }

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
        // 데미지/Miss 폰트는 발 위치(transform.position)가 아니라 머리 위(DamageFontYOffset)에 표시.
        Vector3 fontPos = transform.position + Vector3.up * DamageFontYOffset;

        if (!result.Hit)
        {
            // 슬라이드 62 — Miss Textpopup (피격자 위치). Sound는 후속.
            Managers.Object.ShowMissPopup(fontPos, transform);
            return;
        }

        Hp -= result.FinalDamage;
        if (Hp < 0) Hp = 0;

        // 슬라이드 63: Monster 공격은 Damage Textpopup 호출 안 함.
        bool attackerIsMonster = attacker != null && attacker.ObjectType == ObjectType.Monster;
        if (!attackerIsMonster)
        {
            // 슬라이드 64 — 일반 데미지 색은 속성과 무관하게 동일(FF0000→FFFF00). 치명타만 다름(→FF00FF).
            DamageFontMode mode = result.IsCrit ? DamageFontMode.Critical : DamageFontMode.Normal;
            Managers.Object.ShowDamageFont(fontPos, result.FinalDamage, mode, transform);

            // 슬라이드 62 — 타격 성공 시 피격자 위치에 Hit Effect (Player·동행 미니몬 공격 시)
            bool attackerIsPlayer = attacker != null &&
                (attacker.ObjectType == ObjectType.Player || attacker.ObjectType == ObjectType.Minion);
            if (attackerIsPlayer)
            {
                // 치명타 시 추가: Crit Effect (피격자 위치) + "Critical!" callout (공격자 위치)
                if (result.IsCrit)
                {
                    Managers.Object.ShowCriHitEffect(transform.position);

                    Managers.Object.ShowCritEffect(attacker.transform.position);
                    Vector3 attackerFontPos = attacker.transform.position + Vector3.up * attacker.DamageFontYOffset;
                    Managers.Object.ShowCriticalCallout(attackerFontPos, attacker.transform);
                }
                else
                {

                    Managers.Object.ShowHitEffect(transform.position);
                }
            }
        }

        if (Hp <= 0)
        {
            OnDead();
            return;
        }

        PlayHitFeedback();
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
        if (_onHitCo != null) { StopCoroutine(_onHitCo); _onHitCo = null; }
        if (_sprite != null)  _sprite.DOKill();
    }

    // ─── 슬라이드 60/64 OnHit 피드백 ───
    // 기본: Monster 스펙 — 0.3초간 (1, 0.2, 0.2)로 색 변환 → 0.3초 OutQuad로 흰색 복귀.
    // 서브클래스(Player 등)는 override해서 OnHit Anim 클립 재생 등으로 대체 가능.
    // 이미 진행 중인 피드백이 있으면 처음부터 재시작 (슬라이드 64: "OnHit 도중 또 피격 시 처음부터").
    protected Coroutine _onHitCo;

    protected virtual void PlayHitFeedback()
    {
        if (_sprite == null) return;
        if (_onHitCo != null) StopCoroutine(_onHitCo);
        _sprite.DOKill();
        _onHitCo = StartCoroutine(CoColorFlashOnHit());
    }

    IEnumerator CoColorFlashOnHit()
    {
        Color hit = new Color(1f, 0.2f, 0.2f);
        yield return _sprite.DOColor(hit, 0.3f).WaitForCompletion();
        yield return _sprite.DOColor(Color.white, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        _onHitCo = null;
    }

    protected void FlipToward(float targetX)
    {
        if (_sprite == null) return;
        _sprite.flipX = targetX < transform.position.x;
    }
}
