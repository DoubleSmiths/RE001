using System.Collections;
using UnityEngine;
using static Define;

// [동행 미니몬] (기획서 part2 — 나의 미니몬 #5 / 슬19)
//  · [부르기]로 동행 설정 시 플레이어 발치에 스폰되어 따라다닌다.
//  · 전투 중 자신의 공격속도(공격대기시간)에 따라 자동으로 engaged 몬스터를 공격한다. (Attack Anim)
//  · 명중률만 플레이어를 따른다. DMG TEXT는 플레이어와 동일(ObjectType.Minion → Damage 폰트 표시).
//  · 공격 또한 [타격]으로 친다(공세 게이지에 영향) → Player.NotifyCompanionHit로 콤보 누적.
//  적의 표적이 되지 않으며(몬스터는 PlayerController만 타겟), 피해를 받지 않는다.
public class CompanionController : CreatureController
{
    PlayerController _player;
    int _minionId = -1;

    // 동행 위치 오프셋 — "위치는 미니몬마다 다르다"(슬). 플레이어(발치 y≈2.0) 기준 뒤(왼쪽) + 약간 위로.
    //  (슬라임 스프라이트 pivot이 중앙이라 y=0이면 지면에 절반 묻힘 → +0.3 들어올려 발치 지면선에 맞춤.
    //   사용자 지정 월드 위치 ≈ (-2, 2.3) = player(-0.5, 2.0) + (-1.5, 0.3))
    static readonly Vector3 FOLLOW_OFFSET = new Vector3(-1.5f, 0.3f, 0f);
    const float FOLLOW_LERP = 8f;
    const float ATTACK_HIT_DELAY = 0.3f;   // Attack anim 키프레임(데미지 적용) 시점
    const float ATTACK_RECOVER   = 0.3f;   // 데미지 후 Idle 복귀까지

    bool _attacking;

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = ObjectType.Minion;
        return true;
    }

    public void Setup(int minionId, PlayerController player)
    {
        Init();   // BaseController.Awake가 이미 호출했을 수 있으나 guard로 안전
        _minionId = minionId;
        _player   = player;

        ApplyStats(minionId);

        // 물리 비간섭: 시뮬레이션 자체를 끔(Kinematic rb가 수동 transform 설정과 충돌해 떨림 유발하므로).
        if (_rb != null) { _rb.bodyType = RigidbodyType2D.Kinematic; _rb.linearVelocity = Vector2.zero; _rb.simulated = false; }
        foreach (var col in GetComponentsInChildren<Collider2D>(true)) col.enabled = false;

        if (_player != null)
        {
            transform.position = _player.transform.position + FOLLOW_OFFSET;
            ApplySorting();
        }
        // 플레이어와 같이 오른쪽(적 방향)을 보게. 슬라임 아트는 flipX=false가 좌향 → 우향은 true.
        //  (적은 항상 화면 오른쪽에서 등장하므로 항시 우향 유지)
        if (_sprite != null) _sprite.flipX = true;
        State = Define.State.Idle;
    }

    void ApplyStats(int minionId)
    {
        var mm = Managers.MinMon;
        // 기본 스탯은 CreatureData, 전투 수치는 MinMonManager(공명도 반영)에서.
        if (Managers.Data.CreatureDic.TryGetValue(minionId, out var cd))
        {
            CritDamage        = cd.CritDamage > 0 ? cd.CritDamage : 150f;
            DamageFontYOffset = cd.DamageFontYOffset > 0 ? cd.DamageFontYOffset : 1.5f;
        }
        PhysicalAtk = mm != null ? mm.GetAtk(minionId)      : 10f;
        CritRate    = mm != null ? mm.GetCritRate(minionId) : 0f;
        float atkSpeedPct = mm != null ? mm.GetAtkSpeed(minionId) : 100f;   // %(100=기본)
        AtkCooldown = atkSpeedPct > 0f ? 100f / atkSpeedPct : 1.5f;          // 100%→1s
        // 명중률(Accuracy)은 공격 순간 플레이어 값으로 덮어씀(슬: 명중률만 플레이어 추종)

        ApplyPetAnimator(minionId);
    }

    // 동행(펫)은 몬스터와 다른 애니메이션을 쓴다 — MinMonData.PetAnimLabel(Addressable)로 펫 전용
    // AnimatorController로 교체. (펫 애니는 몬스터 anim 폴더 하위 Pet_* 에 제작 — 슬라임 = Pet_Slime(Green))
    //  라벨이 없거나 로드 실패 시 프리팹 기본(몬스터) 컨트롤러 유지.
    void ApplyPetAnimator(int minionId)
    {
        if (_anim == null) return;
        if (Managers.Data.MinMonDic == null || !Managers.Data.MinMonDic.TryGetValue(minionId, out var md)) return;
        if (string.IsNullOrEmpty(md.PetAnimLabel)) return;
        var ctrl = Managers.Resource.Load<RuntimeAnimatorController>(md.PetAnimLabel);
        if (ctrl != null) _anim.runtimeAnimatorController = ctrl;
    }

    void ApplySorting()
    {
        var ps = _player.GetComponentInChildren<SpriteRenderer>();
        if (ps != null && _sprite != null)
        {
            _sprite.sortingLayerID = ps.sortingLayerID;
            _sprite.sortingOrder   = ps.sortingOrder + 1;   // 플레이어 발치 앞쪽
        }
    }

    void Update()
    {
        if (_player == null) return;
        if (State == Define.State.Dead) return;

        if (_attacking) return;

        // 전투 중 + engaged 몬스터 존재 시 공격. 공격 안 하면 플레이어 이동 상태(Idle/Run) 미러링.
        if (_player.InBattle && _player.State != Define.State.Dead && TryAttack()) return;
        MirrorMoveState();
    }

    // 추종은 LateUpdate(애니메이터 적용 이후)에서. 정지 타겟에 데드존 스냅으로 sub-pixel 떨림 방지.
    void LateUpdate()
    {
        if (_player == null || State == Define.State.Dead) return;
        Vector3 desired = _player.transform.position + FOLLOW_OFFSET;
        if ((transform.position - desired).sqrMagnitude < 0.0025f)   // ≈0.05 unit 이내면 스냅(떨림 제거)
            transform.position = desired;
        else
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * FOLLOW_LERP);
    }

    // 펫 컨트롤러는 Run 상태가 있어 플레이어가 이동(Moving)하면 동행도 Run, 아니면 Idle.
    void MirrorMoveState()
    {
        if (State == Define.State.Dead || _attacking) return;
        Define.State desired = _player.State == Define.State.Moving ? Define.State.Moving : Define.State.Idle;
        if (State != desired) State = desired;
    }

    bool TryAttack()
    {
        if (Time.time < _lastAtkTime + AtkCooldown) return false;
        MonsterController target = FindEngagedMonster();
        if (target == null) return false;

        _lastAtkTime = Time.time;
        _attacking = true;
        // 적은 항상 오른쪽 → 우향 유지(FaceToward로 뒤집지 않음)
        State = Define.State.Attack;
        StartCoroutine(CoAttack(target));
        return true;
    }

    IEnumerator CoAttack(MonsterController target)
    {
        yield return new WaitForSeconds(ATTACK_HIT_DELAY);

        if (target != null && target.State != Define.State.Dead && _player != null && State != Define.State.Dead)
        {
            Accuracy = _player.Accuracy;   // 명중률만 플레이어 추종
            BattleMath.DamageResult result = BattleMath.Resolve(this, target, AttackType.Physical);
            target.TakeHit(result, this);
            if (result.Hit) _player.NotifyCompanionHit(result.FinalDamage);   // 타격 → 콤보/공세 게이지
        }

        yield return new WaitForSeconds(ATTACK_RECOVER);
        _attacking = false;
        if (State != Define.State.Dead) State = Define.State.Idle;
    }

    MonsterController FindEngagedMonster()
    {
        Transform monsterRoot = Managers.Object?.MonsterTransform;
        if (monsterRoot == null) return null;
        for (int i = 0; i < monsterRoot.childCount; i++)
        {
            var m = monsterRoot.GetChild(i).GetComponent<MonsterController>();
            if (m == null || m.State == Define.State.Dead) continue;
            if (m.HasEngaged) return m;
        }
        return null;
    }

}
