using DG.Tweening;
using System.Collections;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    public event System.Action<float, float> OnHpChanged;   // current, max
    public event System.Action<float, float> OnExpChanged;  // current, required
    public event System.Action<int, int>     OnLevelUp;   // newLevel, regenHp (MaxHp - prevHp)
    public event System.Action<float>        OnHitLanded;   // 타격 성공 시 데미지값 전달 (miss 제외) — 슬라이드 48

    float _expRequired = 100f;
    bool _inBattle;

    // ── 플레이어 전용 스탯 ──
    public float GrowthRate = 100f;  // 성장률 % (경험치 획득량에 곱함)
    public float DropRate   = 100f;  // 획득률 % (몬스터 드롭률에 곱함)

    Vector3 _originalPos;   // 슬라이드 69 — 도중 이탈로 화면 밖 이동 후 복귀할 원위치

    // ── 스탯 (레벨 / 장비 반영) ──
    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = ObjectType.Player;
        State = Define.State.Idle;
        _originalPos = transform.position;
        // 슬라이드 62/63 — Monster→Player Miss popup, 추후 heal popup 등이 사용할 머리 위 offset.
        // Player prefab YAML에 직렬화 안 된 경우를 대비해 명시적으로 set (CreatureController 기본 2.5).
        DamageFontYOffset = 2.5f;
        RefreshStats();
        return true;
    }

    public void StartBattle() { _inBattle = true; }

    // 슬라이드 67/69 — 마을 복귀 시 HP 회복 + Idle + 원위치 복귀 (사망/HOME/도중이탈 공통)
    public void StopBattle()
    {
        _inBattle = false;
        Hp = MaxHp;
        OnHpChanged?.Invoke(Hp, MaxHp);
        State = Define.State.Idle;
        transform.DOKill();   // 진행 중인 DOMove(off-screen) 중단
        transform.position = _originalPos;
    }

    // ────────────────────────────────────────
    //  Update
    // ────────────────────────────────────────
    void Update()
    {
        if (State == Define.State.Dead) return;
        if (!_inBattle) return;

        UpdateEngagementVisuals();
        AutoAttack();
    }

    // 슬라이드 57/58 — 몬스터 AtkRange 진입 여부에 따라:
    //   · 미engaged: BG scroll on, Player Run (Moving)
    //   · engaged:   BG scroll off, Player Idle (AutoAttack가 Attack로 전환)
    void UpdateEngagementVisuals()
    {
        bool engaged = HasMonsterInAtkRange();

        BackgroundScroller scroller = GetBgScroller();
        if (scroller != null) scroller.IsScrolling = !engaged;

        // Attack/Dead 중엔 anim state 손대지 않음
        if (State == Define.State.Attack || State == Define.State.Dead) return;

        Define.State desired = engaged ? Define.State.Idle : Define.State.Moving;
        if (State != desired) State = desired;
    }

    // 슬라이드 59 — Monster.OnCollisionEnter2D에서 _hasEngaged=true로 세팅됨.
    // 거리 폴링 대신 monster의 engaged flag로 판단해 collider 사이즈와 무관.
    bool HasMonsterInAtkRange()
    {
        Transform monsterRoot = Managers.Object?.MonsterTransform;
        if (monsterRoot == null) return false;
        for (int i = 0; i < monsterRoot.childCount; i++)
        {
            var m = monsterRoot.GetChild(i).GetComponent<MonsterController>();
            if (m == null || m.State == Define.State.Dead) continue;
            if (m.HasEngaged) return true;
        }
        return false;
    }

    BackgroundScroller _cachedScroller;
    BackgroundScroller GetBgScroller()
    {
        if (_cachedScroller != null) return _cachedScroller;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == "CH1BG" || go.name == "CH1BG(Clone)")
            {
                _cachedScroller = go.GetComponent<BackgroundScroller>();
                return _cachedScroller;
            }
        }
        return null;
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
        // 콤보 공격속도 버프 적용: AtkSpeedBuff(%) 만큼 쿨타임 단축
        float effectiveCooldown = AtkCooldown / (1f + ComboAtkSpeedBonus() / 100f);
        if (Time.time < _lastAtkTime + effectiveCooldown) return;

        // 슬라이드 59 — 콜라이더 충돌로 engaged된 monster를 찾아 공격.
        // 거리 폴링은 콜라이더 사이즈와 어긋날 수 있어 HasEngaged flag로 통일.
        MonsterController target = FindEngagedMonster();
        if (target == null) return;

        _lastAtkTime = Time.time;
        _attackTarget = target;         // 슬라이드 61 — 키프레임 시점에 데미지 적용하기 위해 락온
        State = Define.State.Attack;

        // 명중/회피 dice + BattleMath.Resolve + TakeHit는 Attack.anim의 키프레임 이벤트
        // (AttackTrigger, 0.5s)에서 호출됨. OnHit으로 인터럽트되면 키프레임에 도달하지
        // 못해 자연스럽게 데미지 미적용 (슬라이드 60 — 인터럽트 = 빗나감).

        if (_attackResetCo != null) StopCoroutine(_attackResetCo);
        _attackResetCo = StartCoroutine(CoResetStateAfterAttack());
    }

    Coroutine _attackResetCo;
    MonsterController _attackTarget;   // 현재 진행 중인 Attack의 락온 타겟

    float ComboAtkSpeedBonus() => Managers.Combo?.AtkSpeedBonus ?? 0f;
    float ComboDmgBoostBonus() => Managers.Combo?.DmgBoostBonus ?? 0f;

    // 슬라이드 59 — 콜라이더 충돌로 engaged된 monster 중 첫 번째.
    // 일반적으로 동시 engage는 1마리 (현재 spawn 로직). 다중 monster 시 가장 가까운 거 우선 등 확장 여지.
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

    // 슬라이드 65 — Attack Anim 길이 1초 고정 (Player 직업별 예외는 후속).
    // IsName 폴링 방식은 CrossFade 직후 transition 중에 false를 반환해 1프레임 만에 빠지는 케이스가 있어
    // 시간 기반 대기로 단순화한다. OnHit 등 다른 이벤트가 State를 바꿨으면 Idle로 되돌리지 않는다.
    const float ATTACK_CLIP_LENGTH = 1.0f;

    IEnumerator CoResetStateAfterAttack()
    {
        yield return new WaitForSeconds(ATTACK_CLIP_LENGTH);
        _attackResetCo = null;

        if (State == Define.State.Dead) yield break;
        if (State == Define.State.Attack)   // 그 사이 OnHit/Idle 등으로 바뀌었으면 건드리지 않음
            State = Define.State.Idle;
    }

    // ────────────────────────────────────────
    //  HP / EXP
    // ────────────────────────────────────────
    public override void TakeHit(BattleMath.DamageResult result, CreatureController attacker = null)
    {
        base.TakeHit(result, attacker);
        OnHpChanged?.Invoke(Hp, MaxHp);
    }

    // 슬라이드 60/64/65 Player —
    //  · OnHit Anim을 처음부터 재생 (어느 상태에서 피격되었든)
    //  · OnHit 클립 길이 0.35s (슬라이드 65 고정)
    //  · OnHit 끝나면 Idle로 복귀 → AutoAttack이 자연 재개 (cooldown 끝나는 대로 Attack)
    //  · OnHit 도중 또 피격 시 OnHit 처음부터 재시작 (CrossFade normalizedTime 0)
    protected override void PlayHitFeedback()
    {
        if (State == Define.State.Dead) return;
        if (_anim == null) return;

        _anim.CrossFade("OnHit", 0.05f, -1, 0f);

        if (_onHitCo != null) StopCoroutine(_onHitCo);
        _onHitCo = StartCoroutine(CoReturnAfterOnHit());
    }

    IEnumerator CoReturnAfterOnHit()
    {
        yield return new WaitForSeconds(0.35f);
        _onHitCo = null;
        if (State == Define.State.Dead) yield break;
        State = Define.State.Idle;
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

            // 슬라이드 68 #2-3 — Now HP를 Max HP로 회복. RefreshStats 전 prevHp 캡쳐해서 회복량 계산.
            float prevHp = Hp;
            RefreshStats();
            int regen = Mathf.Max(0, Mathf.RoundToInt(MaxHp - prevHp));

            OnLevelUp?.Invoke(Managers.Game.Level, regen);
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

    // 슬라이드 67 — 플레이어 사망 시퀀스
    //   1-1. HP 0으로 clamp (CreatureController.TakeHit에서 이미 처리)
    //   1-2. Monster Idle 고정
    //   1-3. Monster HP Bar DOPlayBackwards
    //   1-4. Player Die Anim (base.OnDead → State=Dead → CrossFade("Die"))
    //   1-5. 1초 후 Fade IN/OUT
    //   1-6. 마을 리스폰 ([HOME] 버튼 동등 처리)
    protected override void OnDead()
    {
        base.OnDead();   // State=Dead → Die clip

        // 1-2. 살아있는 모든 monster Idle 고정
        Transform monsterRoot = Managers.Object?.MonsterTransform;
        if (monsterRoot != null)
        {
            for (int i = 0; i < monsterRoot.childCount; i++)
            {
                var m = monsterRoot.GetChild(i).GetComponent<MonsterController>();
                if (m == null || m.State == Define.State.Dead) continue;
                m.State = Define.State.Idle;
            }
        }

        // 1-3. 활성 Monster HP Bar 모두 DOPlayBackwards
        foreach (var bar in FindObjectsByType<UI_MonsterHPBar>(FindObjectsSortMode.None))
            bar.PlayCloseAnimation();

        // 1-5/1-6. 1초 대기 후 마을 리스폰 (EnterAreaWithFade가 Fade IN/OUT 처리)
        StartCoroutine(CoRespawnInVillage(1f));
    }

    IEnumerator CoRespawnInVillage(float delay)
    {
        yield return new WaitForSeconds(delay);
        var scene = Managers.UI?.SceneUI as UI_GameScene;
        scene?.EnterAreaWithFade(Define.AreaType.Village1);
        // EnterAreaWithFade → SwapAreaBtnGroup → UI_BattleBtnGroup.OnDestroy → EndBattle → StopBattle 순으로
        // 자연스럽게 정리 + Player HP/State 회복.
    }

    // 슬라이드 61 — Animation Event에서 호출되는 공격판정 순간.
    // Attack.anim의 0.5s 키프레임에 functionName=AttackTrigger 이벤트 등록되어 있음.
    // 명중/회피 dice + BattleMath.Resolve + TakeHit를 이 시점에 처리.
    //
    // OnHit 등으로 Attack 클립이 인터럽트되면 이 이벤트가 발화되지 않아 자연스럽게
    // 데미지가 들어가지 않음 (슬라이드 60 — 공격이 끊긴 경우).
    // 디버그 — 레벨/EXP 리셋. 테스트 메뉴에서 호출.
    public void DebugResetLevel(int targetLevel = 1)
    {
        Managers.Game.Level = Mathf.Max(1, targetLevel);
        Managers.Game.Exp   = 0f;
        RefreshStats();   // _expRequired/MaxHp/Atk 등 재계산 + Hp=MaxHp
        OnLevelUp?.Invoke(Managers.Game.Level, 0);
        OnExpChanged?.Invoke(0f, _expRequired);
    }

    // 디버그 — 즉시 한 단계 레벨업 트리거.
    public void DebugForceLevelUp()
    {
        GainExp(_expRequired - Managers.Game.Exp + 1f);
    }

    public void AttackTrigger()
    {
        if (_attackTarget == null) return;
        if (_attackTarget.State == Define.State.Dead) return;
        if (State != Define.State.Attack) return;       // OnHit/Idle로 바뀌었으면 무시
        if (!_attackTarget.HasEngaged) return;          // engagement가 풀렸으면 silent miss

        // 콤보 피해증폭은 DmgAmp에 일시 가산해서 BattleMath에 전달
        float prevDmgAmp = DmgAmp;
        DmgAmp += ComboDmgBoostBonus();
        BattleMath.DamageResult result = BattleMath.Resolve(this, _attackTarget, AttackType.Physical);
        DmgAmp = prevDmgAmp;

        _attackTarget.TakeHit(result, this);

        // 타격(명중) 성공 시에만 콤보 누적
        if (result.Hit)
            OnHitLanded?.Invoke(result.FinalDamage);
    }
}
