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
        if (State != Define.State.Idle)
            State = Define.State.Idle;
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

        // 슬라이드 61 — 데미지 적용은 AttackTrigger() (Animation Event 키프레임 시점).
        // 몬스터 Attack anim에 키프레임이 아직 없으면 TryAttack이 fallback으로 즉시 호출.
        // 자산에 키프레임 추가되면 이 inline 호출은 제거.
        //AttackTrigger();

        StartCoroutine(CoResetStateAfterAttack(0.4f));
    }

    // 슬라이드 61 — 공격판정 순간. Animation Event에서 호출되어야 함.
    // 자산에 이벤트 추가되기 전까지는 TryAttack이 inline으로 호출 (시각 동기화 X).
    public void AttackTrigger()
    {
        if (_target == null) return;
        if (_target.State == Define.State.Dead) return;
        if (State != Define.State.Attack) return;

        BattleMath.DamageResult result = BattleMath.Resolve(this, _target, AttackType.Physical);
        _target.TakeHit(result, this);
    }

    IEnumerator CoResetStateAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (State != Define.State.Dead)
            State = Define.State.Idle;   // Slime Animator에 "Run" 없음 — Idle로 복귀
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
        _anim.CrossFade("OnHit", 0.05f, -1, 0f);
        base.PlayHitFeedback();
    }

    // 슬라이드 66 — 몬스터 사망 처리.
    //   1. Die Clip 재생 (base.OnDead가 State=Dead로 set → CrossFade("Die"))
    //   2. Monster HP Bar는 UI_MonsterHPBar 측에서 DOPlayBackwards (OnHpChanged의 cur<=0 분기)
    //   3. 보상(Gold/Exp/드롭) — 슬라이드 66 #2~#3
    //   4. Kill count 증가 / 관심 체크 / Die Sound — 후속 (도감/사운드 시스템 구축 후)
    //   5. Die clip 종료 후 풀 반환
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

        // 슬9: 사냥 성공 시 Kill Count 증가 (기록도/분석률 = KillCount/MaxKillCount)
        Managers.MinMon.AddKill(DataId);

        // 슬23~25: 관심 체크 — 사냥 성공 시 InterestRate로 관심 판정 → 포획 가능 창고 저장 + Side UI 알림
        if (_data != null)
        {
            var interest = Managers.MinMon.RollInterest(DataId);
            if (interest == MinMonManager.InterestResult.Added)
                Managers.UI.ShowAttractedPopup(DataId, false);          // 슬24: 창고 여유 → Side UI 알림
            else if (interest == MinMonManager.InterestResult.StorageFull)
                Managers.UI.ShowWarningPopup(DataId);                   // 슬23/25: 창고 가득 → 경고 popup
            // TODO 슬24: 몬스터 머리 위 Emoj_Shy (풀 반환 전 타이밍)
        }

        // Die clip 종료 후 풀 반환. Slime Die clip은 0.17s지만, HP Bar Playbackwards(0.5s)와
        // 동기화하기 위해 0.5s 대기.
        StartCoroutine(CoDestroyAfterDie(0.5f));
    }

    IEnumerator CoDestroyAfterDie(float delay)
    {
        yield return new WaitForSeconds(delay);
        Managers.Resource.Destroy(gameObject);
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
