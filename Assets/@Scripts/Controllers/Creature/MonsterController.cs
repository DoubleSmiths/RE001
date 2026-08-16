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
    public bool HasEngaged => _hasEngaged;

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

        // 범용 몬스터 프리팹: 데이터(AnimController)로 외형/애니 런타임 교체.
        //  비어있으면 프리팹 기본 컨트롤러 유지(슬라임). CompanionController.ApplyPetAnimator와 동일 패턴.
        if (_anim != null && !string.IsNullOrEmpty(_data.AnimController))
        {
            var ctrl = Managers.Resource.Load<RuntimeAnimatorController>(_data.AnimController);
            if (ctrl != null) _anim.runtimeAnimatorController = ctrl;
        }
    }

    void Start()
    {
        _target = FindFirstObjectByType<PlayerController>();
    }

    // ────────────────────────────────────────
    //  AI — 슬라이드 58: Player 위치 고정. 몬스터는 Ground와 동속(-5 units/sec)으로 좌측 스크롤.
    //  슬라이드 59 — Player BoxCollider2D + Rigidbody2D 충돌 시 전투 진입.
    // ────────────────────────────────────────
    const float GROUND_SCROLL_SPEED = 5f;   // 슬라이드 58 — Ground Layer Speed = 5

    void FixedUpdate()
    {
        if (State == Define.State.Dead) return;
        if (_target == null) return;

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
        // 조우 즉시 공격하지 않고 MonsterAttackDelay만큼 대기 후 첫 공격이 발사되도록.
        _lastAtkTime = Time.time;
        OnEngaged?.Invoke();                   // HP Bar 호출 등
    }

    // 슬라이드 58 — 몬스터는 Ground와 같은 속도(-5)로 좌측 이동.
    // engaged 전까지만 호출됨. 충돌 시 OnCollisionEnter2D가 vel=0으로 정지시킴.
    void ScrollLeft()
    {
        if (State == Define.State.Attack) return;
        _rb.linearVelocity = new Vector2(-GROUND_SCROLL_SPEED, _rb.linearVelocity.y);
        // 다가오는 동안은 이동 모션. 예전엔 Idle로 고정했는데, 그건 슬라임 애니메이터에
        //  "Run"이 없어서 어쩔 수 없었던 것이다. 아키타입 애니메이터에는 Run이 있으므로
        //  실제로 걸어오는 게 보인다. (Run이 없는 레거시 애니메이터는 CrossFade가 무시되고
        //  직전 클립이 유지되므로 예전과 같이 보인다 — 깨지지 않는다)
        if (State != Define.State.Moving)
            State = Define.State.Moving;
    }

    void TryAttack()
    {
        // 슬라이드 67 — Player 사망 시 monster는 Idle 고정. Dead target에는 공격 중단.
        if (_target == null || _target.State == Define.State.Dead) return;
        // 슬라이드 53: 몬스터 공격속도는 Attack Anim 재생 전 대기 시간(MonsterAttackDelay)에 영향
        if (Time.time < _lastAtkTime + MonsterAttackDelay) return;
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
    bool _attackHitDone;

    IEnumerator CoAttack()
    {
        _attackHitDone = false;
        yield return new WaitForSeconds(ATTACK_HIT_DELAY);
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
        BattleMath.DamageResult result = BattleMath.Resolve(this, _target, AttackType.Physical);
        _target.TakeHit(result, this);
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
        Managers.Sound?.PlaySFX(_data != null ? _data.OnHitSfx : "Slime_OnHit");   // 몬스터 피격 (데이터 드리븐)
        _anim.CrossFade("OnHit", 0.05f, -1, 0f);

        if (_onHitAnimCo != null) StopCoroutine(_onHitAnimCo);
        _onHitAnimCo = StartCoroutine(CoReturnAfterOnHit());

        base.PlayHitFeedback();
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
        base.OnDead();
        Managers.Sound?.PlaySFX(_data != null ? _data.DieSfx : "Slime_Die");   // 몬스터 사망 (데이터 드리븐, 슬167)
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

        // 슬9: 사냥 성공 시 Kill Count 증가 (기록도/분석률 = KillCount/MaxKillCount)
        Managers.MinMon.AddKill(DataId);

        // 전직: 처치당 직업 숙련도 +1 — 만충 시 수도원 [축성]으로 직업 능력 습득.
        // ponytail: 상승량 고정 1 — 몬스터별 차등은 데이터 엑셀화 후속.
        Managers.Game.AddClassProficiency(1);

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

    IEnumerator CoDestroyAfterDie(float delay)
    {
        yield return new WaitForSeconds(delay);
        Managers.Resource.Destroy(gameObject);
    }

    // 슬24: 관심을 보였을 때 몬스터 머리 위에 Emoj_Shy 재생.
    //  Emoj.controller 클립이 SpriteRenderer.m_Sprite를 구동(슬16 idle와 동일) → 월드 SpriteRenderer로 생성.
    //  몬스터는 0.5초 뒤 풀 반환되므로 부모로 두지 않고 독립 오브젝트로 두어 1.2초간 자체 재생 후 파괴.
    void PlayShyEmoj()
    {
        var ctrl = Managers.Resource.Load<RuntimeAnimatorController>("Emoj");
        if (ctrl == null) return;

        var sr = GetComponent<SpriteRenderer>();
        float headY = sr != null ? sr.bounds.max.y + 0.4f : transform.position.y + 1f;

        var go = new GameObject("Emoj_Shy_FX");
        go.transform.position = new Vector3(transform.position.x, headY, transform.position.z);
        go.transform.localScale = Vector3.one;

        var esr = go.AddComponent<SpriteRenderer>();
        if (sr != null) { esr.sortingLayerID = sr.sortingLayerID; esr.sortingOrder = sr.sortingOrder + 10; }

        var an = go.AddComponent<Animator>();
        an.runtimeAnimatorController = ctrl;
        an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        an.Play("Emoj_Shy", 0, 0f);

        Destroy(go, 1.2f);
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
