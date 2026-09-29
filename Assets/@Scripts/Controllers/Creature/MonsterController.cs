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
    // v2 공격 예고('!')가 떴다 — UI_BattleInput 이 처음 한 번 막기 도움말을 띄우고 방패 버튼을 들썩인다(ADR0030/0033).
    public static event System.Action OnAttackTelegraph;
    // 어떤 몬스터든 플레이어와 맞붙었다 — UI_BattleInput 이 첫 전투에 '눌러서 공격' 도움말을 띄운다(ADR0033).
    public static event System.Action OnAnyEngaged;

    Data.CreatureData _data;
    PlayerController  _target;
    bool _hasEngaged;
    public bool HasEngaged => _hasEngaged;

    // 처치 보상 — 데이터 값으로 시작하고, 스테이지 보스는 MakeStageBoss 가 덮어쓴다.
    float _rewardExp;
    int   _rewardGold;

    // v2 스테이지 보스(ADR0030)
    public bool IsStageBoss { get; private set; }
    public event System.Action OnDied;   // 실제로 쓰러졌을 때만(귀환·보스 퇴각으로 치워질 때는 아님)
    bool _retreating;
    const float RETREAT_FADE = 0.6f;

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
        DamageFontYOffset  = _data.DamageFontYOffset;
        if (System.Enum.TryParse(_data.Rarity, true, out CreatureRarity r))
            Rarity = r;
        _rewardExp  = _data.TotalExp;
        _rewardGold = _data.RewardGold;

        // 범용 몬스터 프리팹: 데이터(AnimController)로 외형/애니 런타임 교체.
        //  비어있으면 프리팹 기본 컨트롤러 유지(슬라임). CompanionController.ApplyPetAnimator와 동일 패턴.
        if (_anim != null && !string.IsNullOrEmpty(_data.AnimController))
        {
            var ctrl = Managers.Resource.Load<RuntimeAnimatorController>(_data.AnimController);
            if (ctrl != null) _anim.runtimeAnimatorController = ctrl;
        }
        // 첫 프레임 스프라이트를 지금 입힌다 — 몸 크기(FitBody)를 재려면 프리팹 기본(슬라임) 그림이 아니라 이 몬스터 그림이어야 한다
        if (_anim != null && _anim.runtimeAnimatorController != null) _anim.Update(0f);
        _bodyFitted = false;
    }

    // ── 몸 크기 (ADR0033) ──
    //  아키타입 그림은 6×4 캔버스에 그려져 스프라이트 경계가 실제 몸보다 훨씬 크다. 스프라이트 메시(Tight)는 불투명 영역만 덮으므로
    //  그 꼭짓점으로 실제 몸을 잰다. 충돌 상자를 몸 폭에 맞춰야 맞붙을 때 몸이 플레이어를 파고들지 않고(예전엔 1칸 폭 고정이라
    //  큰 몬스터가 플레이어 칼·다리를 덮었다), 타격 불꽃도 발바닥이 아니라 몸 한가운데에 튄다.
    const float BODY_COLLIDER_WIDTH = 0.8f;   // 몸 폭 대비 충돌 폭 — 살짝 겹쳐야 맞닿아 보인다
    //  원형 그림은 키를 전부 2칸(128px)에 맞춰 뽑아서 납작하고 긴 것(슬라임 3.8칸, 지네 5칸, 도마뱀 4.5칸)이 화면 절반을 덮었다.
    //  몸 면적이 이 값을 넘으면 면적이 같아지도록 줄인다 — 키 큰 인간형(1.3~2칸 폭)은 그대로, 키우지는 않는다.
    const float BODY_TARGET_AREA = 3.6f;
    const float DAMAGE_FONT_ABOVE_HEAD = 0.5f;
    bool    _bodyFitted;
    Vector2 _bodyCenterLocal = new Vector2(0f, 1f);

    void FitBody()
    {
        _bodyFitted = true;
        Sprite s = _sprite != null ? _sprite.sprite : null;
        if (s == null) return;
        Vector2[] v = s.vertices;
        if (v == null || v.Length == 0) return;
        float minX = v[0].x, maxX = v[0].x, minY = v[0].y, maxY = v[0].y;
        foreach (var p in v)
        {
            minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
        }
        _bodyCenterLocal = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);

        float area = (maxX - minX) * (maxY - minY);
        if (area > BODY_TARGET_AREA) transform.localScale *= Mathf.Sqrt(BODY_TARGET_AREA / area);
        // 데미지 숫자·머리 위 이모지는 실제 머리 위에 — 데이터의 고정 높이(2.5)는 몸이 작으면 허공에 떴다
        DamageFontYOffset = maxY * transform.localScale.y + DAMAGE_FONT_ABOVE_HEAD;

        var box = GetComponent<BoxCollider2D>();
        if (box == null) return;
        float width = Mathf.Clamp((maxX - minX) * BODY_COLLIDER_WIDTH, 0.5f, 3.5f);
        box.size   = new Vector2(width, box.size.y);
        box.offset = new Vector2(LocalX(_bodyCenterLocal.x), box.offset.y);
    }

    float LocalX(float x) => _sprite != null && _sprite.flipX ? -x : x;

    public override Vector3 HitPoint => transform.TransformPoint(new Vector3(LocalX(_bodyCenterLocal.x), _bodyCenterLocal.y, 0f));

    void Start()
    {
        _target = FindFirstObjectByType<PlayerController>();
    }

    // ────────────────────────────────────────
    //  AI — 슬라이드 58: Player 위치 고정. 몬스터는 Ground와 동속(-5 units/sec)으로 좌측 스크롤.
    //  슬라이드 59 — Player BoxCollider2D + Rigidbody2D 충돌 시 전투 진입.
    // ────────────────────────────────────────
    const float GROUND_SCROLL_SPEED = 5f;   // 슬라이드 58 — Ground Layer Speed = 5

    // 스테이지 보스 능력치 — ObjectManager.SpawnStageBoss 가 노드 몬스터의 출현 가중 평균으로 채운다.
    public struct StageBossStats
    {
        public float MaxHp, PhysicalAtk, MagicAtk, Def;
        public float CritRate, CritDamage, Accuracy, Evasion, PhysicalResist, MagicResist, DmgAmp, AttackDelay;
        public float Scale, RewardExp;
        public int   RewardGold;
    }

    // 능력치·보상·크기를 보스로 바꾼다(모습·효과음·애니메이터만 원래 몬스터 것). 몬스터는 풀링하지 않아 되돌릴 필요가 없다.
    public void MakeStageBoss(StageBossStats s)
    {
        IsStageBoss        = true;
        MaxHp              = s.MaxHp;
        Hp                 = s.MaxHp;
        PhysicalAtk        = s.PhysicalAtk;
        MagicAtk           = s.MagicAtk;
        Def                = s.Def;
        CritRate           = s.CritRate;
        CritDamage         = s.CritDamage;
        Accuracy           = s.Accuracy;
        Evasion            = s.Evasion;
        PhysicalResist     = s.PhysicalResist;
        MagicResist        = s.MagicResist;
        DmgAmp             = s.DmgAmp;
        MonsterAttackDelay = s.AttackDelay;
        Rarity             = CreatureRarity.Boss;   // HP 바가 우두머리 모양(색·크기·재질)이 된다
        _rewardExp         = s.RewardExp;
        _rewardGold        = s.RewardGold;
        transform.localScale *= s.Scale;
        DamageFontYOffset    *= s.Scale;
        gameObject.AddComponent<BossOutline>();
    }

    // v2 시련·심연(ADR0037) — 능력치와 처치 보상을 배수만큼 키운다(몬스터는 풀링하지 않아 되돌릴 필요가 없다).
    //  방어는 √배수 — 피해에서 그대로 빼는 값(BattleMath)이라 같은 배수로 키우면 깊은 층에서 '피해 1' 벽이 된다.
    //  골드는 상한 — 끝없는 층에서 int 를 넘으면 음수가 되어 골드가 사라진다(핫타임·유물 배수를 곱해도 int 안).
    const float MAX_REWARD_GOLD = int.MaxValue / 16f;

    public void ScaleStats(float statMult, float rewardMult = 1f)
    {
        MaxHp       *= statMult;
        Hp           = MaxHp;
        PhysicalAtk *= statMult;
        MagicAtk    *= statMult;
        Def         *= Mathf.Sqrt(statMult);
        _rewardExp  *= rewardMult;
        _rewardGold  = Mathf.RoundToInt(Mathf.Min(_rewardGold * rewardMult, MAX_REWARD_GOLD));
    }

    // 제한 시간 초과 — 싸움을 멈추고 흐려지며 사라진다(보상 없음). 교전이 풀려 플레이어는 다시 달린다.
    public void Retreat()
    {
        if (State == Define.State.Dead || _retreating) return;
        _retreating = true;
        _hasEngaged = false;
        StopAllCoroutines();
        foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
        _rb.linearVelocity = Vector2.zero;
        State = Define.State.Idle;
        if (_sprite != null)
        {
            _sprite.DOKill();   // 피격 색 트윈이 알파를 1로 되돌리지 않게
            _sprite.DOFade(0f, RETREAT_FADE).SetLink(gameObject);   // 금빛 테두리도 본체 알파를 따라간다
        }
        StartCoroutine(CoDestroyAfterDie(RETREAT_FADE));
    }

    void FixedUpdate()
    {
        if (State == Define.State.Dead || _retreating) return;
        if (_target == null) return;

        // 플레이어 쪽을 보게 한다. FlipToward는 만들어만 두고 아무 데서도 부르지 않아서
        //  몬스터가 전부 원본 그림 방향(오른쪽)으로 서 있었다 — 플레이어는 왼쪽에 있으니
        //  다가오면서도 등을 보이는 상태였다.
        FlipToward(_target.transform.position.x);
        if (!_bodyFitted) FitBody();   // 방향이 정해진 뒤에 잰다(충돌 상자 중심이 좌우로 뒤집힌다)

        if (_hasEngaged)
            TryAttack();
        else
            ScrollLeft();
    }

    // 슬라이드 59 — Player와 충돌하는 즉시 전투 진입. 거리 폴링이 아니라 콜라이더 충돌 trigger.
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (_hasEngaged) return;
        if (State == Define.State.Dead) return;
        if (coll.gameObject.GetComponent<PlayerController>() == null) return;

        _hasEngaged = true;
        _rb.linearVelocity = Vector2.zero;     // 충돌 즉시 정지
        // 맞붙은 뒤로는 물리로 밀고 당기지 않는다 — 밀림·돌진 연출이 위치를 직접 움직인다(물리가 되밀면 떨린다)
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _homeX = transform.position.x;
        // 다가오던 걸음을 멈춘다. 예전엔 State 가 Moving 으로 남아 첫 공격까지 제자리에서 계속 걸었다.
        if (State == Define.State.Moving) State = Define.State.Idle;
        // 조우 즉시 공격하지 않고 MonsterAttackDelay만큼 대기 후 첫 공격이 발사되도록.
        _lastAtkTime = Time.time;
        OnEngaged?.Invoke();                   // HP Bar 호출 등
        OnAnyEngaged?.Invoke();
    }

    // 슬라이드 58 — 몬스터는 Ground 스크롤(-5)에 더해 제 발로 걸어온다(MONSTER_WALK_SPEED).
    //  땅과 똑같은 속도면 땅 위에 멈춰 선 채 걷는 모션만 돌아 제자리걸음(러닝머신)으로 보였다.
    // engaged 전까지만 호출됨. 충돌 시 OnCollisionEnter2D가 vel=0으로 정지시킴.
    void ScrollLeft()
    {
        if (State == Define.State.Attack) return;
        _rb.linearVelocity = new Vector2(-(GROUND_SCROLL_SPEED + MONSTER_WALK_SPEED), _rb.linearVelocity.y);
        // 다가오는 동안은 이동 모션. 예전엔 Idle로 고정했는데, 그건 슬라임 애니메이터에
        //  "Run"이 없어서 어쩔 수 없었던 것이다. 아키타입 애니메이터에는 Run이 있으므로
        //  실제로 걸어오는 게 보인다. (Run이 없는 레거시 애니메이터는 CrossFade가 무시되고
        //  직전 클립이 유지되므로 예전과 같이 보인다 — 깨지지 않는다)
        if (State != Define.State.Moving)
            State = Define.State.Moving;
    }

    // ── v2 공격 예고·기절 (ADR0030) ──
    //  타격이 닿기 ATTACK_TELEGRAPH 초 전에 머리 위 '!' — 예전엔 공격 모션 0.25초 만에 맞아 반응할 수 없었다.
    //  공격 주기는 그대로 두고, 모션보다 앞선 몫(예고 − 모션 앞부분)만큼 먼저 띄운다.
    bool  _telegraphed;
    float _stunnedUntil = -999f;
    public bool IsStunned => Time.time < _stunnedUntil;
    float TelegraphLead => Mathf.Max(0f, Managers.Data.Balance(BalanceKey.ATTACK_TELEGRAPH, 0.6f) - ATTACK_HIT_DELAY);

    void TryAttack()
    {
        // 슬라이드 67 — Player 사망 시 monster는 Idle 고정. Dead target에는 공격 중단.
        if (_target == null || _target.State == Define.State.Dead) return;
        if (IsStunned) return;   // 패링당해 비틀거리는 중
        // 슬라이드 53: 몬스터 공격속도는 Attack Anim 재생 전 대기 시간(MonsterAttackDelay)에 영향
        float attackAt = _lastAtkTime + MonsterAttackDelay;
        if (!_telegraphed && Time.time >= attackAt - TelegraphLead)
        {
            _telegraphed = true;
            PlayEmoj("Emoj_Exclamation", TelegraphLead + ATTACK_HIT_DELAY, EMOJ_ABOVE_DAMAGE);
            OnAttackTelegraph?.Invoke();
        }
        if (Time.time < attackAt) return;
        _telegraphed = false;
        _lastAtkTime = Time.time;
        State = Define.State.Attack;
        _rb.linearVelocity = Vector2.zero;

        // 슬라이드 61 — 타격 판정은 Attack 모션 중간(ATTACK_HIT_DELAY)에.
        //  예전엔 Animation Event가 붙기를 기다리며 호출을 주석 처리해 뒀는데,
        //  이벤트가 끝내 안 붙어서 **몬스터가 공격 자세만 취하고 데미지를 전혀 안 줬다**.
        //  이벤트에 의존하지 말고 코루틴 하나로 처리한다 — 레거시/생성 애니메이터 모두에서 동작한다.
        //  (혹시 클립에 이벤트가 붙어 있어도 _attackHitDone 가드로 두 번 맞지 않는다)
        StartCoroutine(CoAttack());
    }

    const float ATTACK_HIT_DELAY = 0.25f;   // Attack 모션에서 타격이 닿는 시점
    const float ATTACK_RECOVER   = 0.15f;   // 타격 후 Idle 복귀까지(클립이 더 길면 클립을 따른다)
    const float LUNGE_OUT  = 0.07f;         // 내딛는 시간 — 다 내디딘 순간이 타격
    const float LUNGE_BACK = 0.16f;
    bool _attackHitDone;

    // ── 밀림·돌진 (ADR0033) ── 맞붙은 자리(_homeX) 기준으로 dx 만큼 갔다가 돌아온다. 새로 걸면 앞의 것을 끊고 지금 자리에서 다시.
    float _homeX;
    Tween _nudgeTween, _punchTween;

    void Nudge(float dx, float outTime, float backTime)
    {
        if (!_hasEngaged || _retreating) return;
        _nudgeTween?.Kill();
        _nudgeTween = DOTween.Sequence()
            .Append(transform.DOMoveX(_homeX + dx, outTime).SetEase(Ease.OutQuad))
            .Append(transform.DOMoveX(_homeX, backTime).SetEase(Ease.InOutQuad))
            .SetLink(gameObject);
    }

    // 플레이어 칼에 밀린다. 마지막 일격이면 돌아오지 않고 그대로 날아가 쓰러진다.
    public void Knockback(float distance)
    {
        if (!_hasEngaged || _retreating) return;
        if (State == Define.State.Dead)
        {
            _nudgeTween?.Kill();
            _nudgeTween = transform.DOMoveX(transform.position.x + distance * 2f, 0.25f).SetEase(Ease.OutCubic).SetLink(gameObject);
            return;
        }
        Nudge(distance, 0.05f, 0.18f);
    }

    // 맞을 때마다 하얗게 번쩍이고 찌그러진다(치명타·일반 공통, 동행 미니몬 타격 포함). 공격 모션 중에도 한다 — 모션은 끊지 않는다.
    void PlayImpactFlash()
    {
        HitFlash.Play(gameObject, HIT_FLASH_SECONDS);
        _punchTween?.Complete();
        _punchTween = transform.DOPunchScale(new Vector3(0.12f, -0.1f, 0f), 0.16f, 1, 0f).SetLink(gameObject);
    }

    IEnumerator CoAttack()
    {
        _attackHitDone = false;
        // 타격 직전에 앞으로 내딛는다 — 제자리에서 모션만 돌면 언제 맞는지 읽히지 않는다.
        //  맞아서 멈칫한(히트스톱) 동안은 공격 시계도 멈춘다 — 안 그러면 모션은 굳어 있는데 돌진·피해만 제시간에 들어갔다.
        yield return new WaitForSeconds(ATTACK_HIT_DELAY - LUNGE_OUT);
        while (InHitStop) yield return null;
        if (State == Define.State.Attack) Nudge(-MONSTER_LUNGE, LUNGE_OUT, LUNGE_BACK);
        yield return new WaitForSeconds(LUNGE_OUT);
        while (InHitStop) yield return null;
        AttackTrigger();

        // ⚠️ 0.25+0.15=0.40초 뒤에 Idle로 넘기는데 아키타입 Attack 클립은 0.46초다.
        //    상수만 믿으면 **공격 모션 마지막 프레임이 안 보이고 중간에 잘린다**(휘두르다 만 것처럼).
        //    클립이 더 길면 클립이 끝날 때까지 기다린다.
        float rest = Mathf.Max(ATTACK_RECOVER, ClipLength("Attack") - ATTACK_HIT_DELAY);
        yield return new WaitForSeconds(rest);

        if (State != Define.State.Dead && State != Define.State.OnDamaged)
            State = Define.State.Idle;
    }

    /// 아키타입마다 클립 길이가 달라 상수로 박지 않고 컨트롤러에서 읽는다.
    float ClipLength(string clipName, float fallback = 0.4f)
    {
        var ctrl = _anim != null ? _anim.runtimeAnimatorController : null;
        if (ctrl != null)
            foreach (var clip in ctrl.animationClips)
                if (clip != null && clip.name == clipName)
                    return Mathf.Max(0.05f, clip.length);
        return fallback;
    }

    // 슬라이드 61 — 공격판정 순간. CoAttack이 호출하며, 클립에 Animation Event가 붙어 있어도
    //  같은 함수로 들어온다. 한 번의 공격에 한 번만 맞도록 _attackHitDone으로 막는다.
    public void AttackTrigger()
    {
        if (_attackHitDone) return;
        if (_target == null) return;
        if (_target.State == Define.State.Dead) return;
        if (State != Define.State.Attack) return;
        _attackHitDone = true;

        Managers.Sound?.PlaySFX(_data != null ? _data.AttackSfx : "Slime_Attack");   // 몬스터 공격 (데이터 드리븐)

        // v2 가드(ADR0030): 플레이어가 막는 중이면 패링(피해 0·기절·반격) 또는 막기(피해 감소).
        var guard = _target.ResolveGuard();
        if (guard == PlayerController.GuardResult.Parry)
        {
            Stun(Managers.Data.Balance(BalanceKey.PARRY_STUN, 1.5f));
            _target.ParryCounter(this);
            return;
        }
        BattleMath.DamageResult result = BattleMath.Resolve(this, _target, AttackType.Physical);
        if (guard == PlayerController.GuardResult.Block && result.Hit)
            result.FinalDamage = Mathf.Max(1, Mathf.RoundToInt(result.FinalDamage * Managers.Data.Balance(BalanceKey.BLOCK_DAMAGE_MULT, 0.4f)));
        _target.TakeHit(result, this);
    }

    // 패링당함 — 그 자리에 굳어(애니메이터 정지) 식은땀을 흘리고, 풀린 뒤에야 다음 공격 주기를 센다.
    void Stun(float seconds)
    {
        _stunnedUntil = Time.time + seconds;
        _lastAtkTime  = _stunnedUntil;
        _telegraphed  = false;
        SetAnimSpeed(0f);
        PlayEmoj("Emoj_Sweat", seconds, EMOJ_ABOVE_DAMAGE);
        StartCoroutine(CoEndStun(seconds));
    }

    IEnumerator CoEndStun(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (State != Define.State.Dead) SetAnimSpeed(1f);
    }

    // ────────────────────────────────────────
    //  피격 / 사망
    // ────────────────────────────────────────
    public override void TakeHit(BattleMath.DamageResult result, CreatureController attacker = null)
    {
        if (_retreating) return;   // 물러나는 보스를 동행 미니몬이 마저 잡아 보상이 나가지 않게
        if (IsStunned && result.Hit)   // 기절한 적은 더 아프게 맞는다(패링 보상)
            result.FinalDamage = Mathf.RoundToInt(result.FinalDamage * Managers.Data.Balance(BalanceKey.STUN_DAMAGE_TAKEN_MULT, 1.5f));
        base.TakeHit(result, attacker);
        OnHpChanged?.Invoke(Hp, MaxHp);
        if (result.Hit) PlayImpactFlash();   // 마지막 일격에도 번쩍인다(사망 처리는 PlayHitFeedback 을 건너뛴다)
    }

    // 슬라이드 64 — Attack Anim 도중 피격 시 OnHit 처리는 하되 OnHit Anim 없음(공격 모션을 끊지 않는다).
    //  예전 0.3+0.3초 붉은 물들임은 흰 번쩍임(PlayImpactFlash)으로 바꿨다 — 연타하면 내내 검붉게 남았다(ADR0033).
    // (OnHit 처리 = HP 차감 등은 base.TakeHit에서 이미 끝남)
    protected override void PlayHitFeedback()
    {
        if (State == Define.State.Dead) return;
        Managers.Sound?.PlaySFX(_data != null ? _data.OnHitSfx : "Slime_OnHit");   // 몬스터 피격 (데이터 드리븐)
        if (State == Define.State.Attack || IsStunned) return;
        _anim.CrossFade("OnHit", 0.05f, -1, 0f);

        if (_onHitAnimCo != null) StopCoroutine(_onHitAnimCo);
        _onHitAnimCo = StartCoroutine(CoReturnAfterOnHit());
    }

    Coroutine _onHitAnimCo;

    /// <summary>
    /// OnHit 클립이 끝나면 원래 상태 모션으로 되돌린다.
    ///
    /// ⚠️ 아키타입 애니메이터에는 **트랜지션이 하나도 없다**(코드가 CrossFade로 직접 몬다).
    ///    OnHit은 비반복 클립이라 끝나도 그 자리에 멈춰 서는데, State는 여전히 Moving이라
    ///    ScrollLeft의 `State != Moving` 가드에 걸려 아무도 되돌려 주지 않는다.
    ///    결과: **한 대 맞은 몬스터는 그때부터 애니메이션이 완전히 멈춘 채 미끄러져 온다.**
    ///    (플레이어 쪽엔 같은 복귀 코루틴이 이미 있었는데 몬스터만 빠져 있었다.)
    /// </summary>
    IEnumerator CoReturnAfterOnHit()
    {
        yield return new WaitForSeconds(OnHitClipLength());
        _onHitAnimCo = null;

        // 맞는 사이 죽었거나 공격에 들어갔으면 그쪽 모션이 우선.
        if (State == Define.State.Dead || State == Define.State.Attack) yield break;
        ReplayStateAnimation();
    }

    float OnHitClipLength() => ClipLength("OnHit", 0.2f);

    // 슬라이드 66 — 몬스터 사망 처리.
    //   1. Die Clip 재생 (base.OnDead가 State=Dead로 set → CrossFade("Die"))
    //   2. Monster HP Bar는 UI_MonsterHPBar 측에서 DOPlayBackwards (OnHpChanged의 cur<=0 분기)
    //   3. 보상(Gold/Exp/드롭) — 슬라이드 66 #2~#3
    //   4. Kill count 증가 / 관심 체크 / Die Sound — 후속 (도감/사운드 시스템 구축 후)
    //   5. Die clip 종료 후 풀 반환
    protected override void OnDead()
    {
        SetAnimSpeed(1f);   // 기절(애니메이터 정지) 중에 쓰러져도 사망 모션은 재생한다(마무리 멈칫이 풀리면)
        base.OnDead();
        Managers.Sound?.PlaySFX(_data != null ? _data.DieSfx : "Slime_Die");   // 몬스터 사망 (데이터 드리븐, 슬167)
        _rb.linearVelocity = Vector2.zero;

        // 보상
        if (_target != null)
        {
            // v2 핫타임(골드·경험치 2배)은 처치 보상에만 곱한다
            //  유물 골드 %(ADR0035)는 처치·방치·오프라인·던전 모두에 곱한다
            if (_data != null) Managers.Game.Gold += Mathf.RoundToInt(_rewardGold * (Managers.Shop?.GoldMult ?? 1f) * (Managers.Relic?.GoldMult ?? 1f));
            _target.GainExp(_rewardExp * (Managers.Shop?.ExpMult ?? 1f));
        }
        OnDied?.Invoke();
        Managers.Mission?.Notify(MissionEvent.KillMonster);

        // 전직: 처치당 직업 숙련도 +1 — 만충 시 수도원 [축성]으로 직업 능력 습득.
        //  수도원 사제 호감도(ADR0036)만큼의 확률로 +1 더.
        // ponytail: 상승량 고정 1 — 몬스터별 차등은 데이터 엑셀화 후속.
        Managers.Game.AddClassProficiency(1 + (Random.value < (Managers.Bond?.ExtraProficiencyChance ?? 0f) ? 1 : 0));
        Managers.Bond?.OnKill(Managers.Game.CurrentTravelAreaId);   // 마을 '소포 전달' 의뢰 처치 수

        // v2 스테이지 보스는 외형만 빌린 '스테이지 수호자'다 — 그 종의 드롭·도감 기록·관심(포획) 판정은 하지 않는다.
        //  (우두머리 외형은 드롭 확정·관심 40%라, 보스마다 돌리면 포획·재료 경제가 몰래 몇 배로 바뀌었다)
        if (IsStageBoss)
        {
            StartCoroutine(CoDestroyAfterDie(Mathf.Max(0.5f, ClipLength("Die", 0.5f) + 0.1f)));
            return;
        }

        // 아이템 드롭
        if (_data != null && _data.DropItemId > 0 && Random.value <= _data.DropChance)
        {
            Managers.Game.AddItem(_data.DropItemId, _data.DropCount);
            OnItemDropped?.Invoke(_data.DropItemId, _data.DropCount);
        }

        // 슬9: 사냥 성공 시 Kill Count 증가 (기록도/분석률 = KillCount/MaxKillCount)
        Managers.MinMon.AddKill(DataId);

        // 슬23~25: 관심 체크 — 사냥 성공 시 InterestRate로 관심 판정 → 포획 가능 창고 저장 + Side UI 알림
        if (_data != null)
        {
            var interest = Managers.MinMon.RollInterest(DataId);
            if (interest != MinMonManager.InterestResult.NotInterested)
            {
                PlayShyEmoj();                                          // 슬24: 관심 보임 → 머리 위 Emoj_Shy
                if (interest == MinMonManager.InterestResult.Added)
                    Managers.UI.ShowAttractedPopup(DataId, false);      // 슬24: 창고 여유 → Side UI 알림
                else
                    Managers.UI.ShowAttractedPopup(DataId, true);       // 슬23/25 D3: 창고 가득 → 같은 팝업 빨강 버전(UI_WarningPopup 폐기)
            }
        }

        // Die clip 종료 후 풀 반환. Slime Die clip은 0.17s지만, HP Bar Playbackwards(0.5s)와
        // 동기화하기 위해 0.5s 대기.
        // 사망 모션이 끝나고 나서 사라지게 한다. 0.5초로 박아 두면 Die 클립이 더 긴 아키타입에서
        //  **쓰러지다 만 채로 증발**한다. 여유 0.1초를 둬 마지막 프레임이 보이게 한다.
        StartCoroutine(CoDestroyAfterDie(Mathf.Max(0.5f, ClipLength("Die", 0.5f) + 0.1f)));
    }

    // 쓰러진 모습이 뚝 사라지지 않게 마지막 DEATH_FADE 초 동안 흐려진다(퇴각은 이미 흐려지는 중이라 그대로 둔다).
    const float DEATH_FADE = 0.25f;

    IEnumerator CoDestroyAfterDie(float delay)
    {
        if (!_retreating && _sprite != null)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delay - DEATH_FADE));
            _sprite.DOFade(0f, DEATH_FADE).SetLink(gameObject);
            yield return new WaitForSeconds(DEATH_FADE);
        }
        else
            yield return new WaitForSeconds(delay);
        Managers.Resource.Destroy(gameObject);
    }

    // 슬24: 관심을 보였을 때 몬스터 머리 위에 Emoj_Shy 재생.
    //  몬스터는 0.5초 뒤 풀 반환되므로 부모로 두지 않고 독립 오브젝트로 두어 1.2초간 자체 재생 후 파괴.
    void PlayShyEmoj() => PlayEmoj("Emoj_Shy", 1.2f);

    // 머리 위 이모지(Emoj.controller 의 상태 이름) — 관심(Shy)·공격 예고(Exclamation)·기절(Sweat).
    //  Emoj 클립이 SpriteRenderer.m_Sprite 를 구동(슬16 idle와 동일) → 월드 SpriteRenderer로 생성.
    //  높이는 데미지 글자와 같은 머리 위(DamageFontYOffset) — 스프라이트 캔버스 위쪽 빈칸 때문에 bounds 로 재면 너무 뜬다.
    const float EMOJ_ABOVE_DAMAGE = 0.8f;   // 예고·기절은 데미지 숫자가 뜨는 자리보다 위에 — 숫자에 가려졌다

    void PlayEmoj(string state, float seconds, float raise = 0f)
    {
        var ctrl = Managers.Resource.Load<RuntimeAnimatorController>("Emoj");
        if (ctrl == null) return;

        var sr = GetComponent<SpriteRenderer>();
        var go = new GameObject(state + "_FX");
        go.transform.position = transform.position + Vector3.up * (DamageFontYOffset + raise);
        go.transform.localScale = Vector3.one;

        var esr = go.AddComponent<SpriteRenderer>();
        if (sr != null) { esr.sortingLayerID = sr.sortingLayerID; esr.sortingOrder = sr.sortingOrder + 10; }

        var an = go.AddComponent<Animator>();
        an.runtimeAnimatorController = ctrl;
        an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        // Emoj_Sweat 클립에 원본 에셋의 'TriggerUnityEvent' 이벤트가 박혀 있다 — 받을 곳이 없는 빈 오브젝트라
        //  기절시킬 때마다 "has no receiver" 빨간 에러가 났다. 이모지는 이벤트를 쓰지 않는다.
        an.fireEvents = false;
        an.Play(state, 0, 0f);

        Destroy(go, seconds);
    }

}
