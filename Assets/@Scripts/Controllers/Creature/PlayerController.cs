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

    // 슬40(능력#4 강화능력): 강화(버프) 적용/해제 표시 — 좌측상단 버프 아이콘(UI_BuffDisplay)이 구독.
    //  능력 버프(AbilityRunner)와 물약 버프(ApplySpeedBuff)를 한 경로로 통합한다.
    //  Key: 능력=SkillData.DataId / 물약=음수 상수(충돌 회피). Timed=true면 지속%게이지+종료 FadeOut(③지속시간형).
    public struct BuffInfo
    {
        public int Key;          // 식별자(능력 DataId / 물약 음수키)
        public string IconLabel; // 아이콘 스프라이트 주소(비면 프리팹 기본 아이콘 유지)
        public float Duration;   // 지속시간(초). Timed=false면 의미 없음(인게임 유지)
        public bool Timed;       // true=지속시간형(③) / false=인게임지속(②)
        public BuffInfo(int key, string iconLabel, float duration, bool timed)
        { Key = key; IconLabel = iconLabel; Duration = duration; Timed = timed; }
    }
    public event System.Action<BuffInfo> OnBuffApplied;   // 강화 버프 적용(또는 갱신)
    public event System.Action<int>      OnBuffCleared;   // 강화 버프 해제(Key 전달)
    public void RaiseBuffApplied(BuffInfo b) => OnBuffApplied?.Invoke(b);
    public void RaiseBuffCleared(int key)    => OnBuffCleared?.Invoke(key);

    public const int BUFF_KEY_POTION_SPEED = -100;   // 신속물약 버프 표시용 키(UI 식별)
    public const int BUFF_DATA_SPEED = 1;            // BuffData 신속버프 DataId(아이콘 조회)

    float _expRequired = 100f;
    public float ExpRequired => _expRequired;   // UI 초기 동기화용 read-only 노출
    bool _inBattle;
    public bool InBattle => _inBattle;   // 동행 미니몬이 전투 중 여부 참조

    // 동행 미니몬(컴패니언) — 부르기 설정 시 스폰되어 따라다니며 전투 참여(슬19/나의미니몬#5)
    CompanionController _companion;
    int _companionId = -1;

    // 동행 미니몬 타격을 콤보(공세 게이지)에 누적 — 타격으로 침(슬)
    public void NotifyCompanionHit(float damage) => OnHitLanded?.Invoke(damage);

    // 슬42: 신속물약 — 일정 시간 공격속도(공격쿨타임 단축) 버프. AutoAttack의 effectiveCooldown에 가산.
    public float PotionAtkSpeedBonus { get; private set; }
    Coroutine _potionSpeedCo;
    public void ApplySpeedBuff(float bonusPct, float duration)
    {
        PotionAtkSpeedBonus = bonusPct;
        if (_potionSpeedCo != null) StopCoroutine(_potionSpeedCo);
        if (duration > 0f) _potionSpeedCo = StartCoroutine(CoClearSpeedBuff(duration));
        // 슬40: 좌측상단 버프 아이콘 표시(지속시간형 — 게이지+점멸+FadeOut). 아이콘은 BuffData에서 조회.
        string icon = "";
        if (Managers.Data != null && Managers.Data.BuffDic.TryGetValue(BUFF_DATA_SPEED, out var bd))
            icon = bd.IconLabel;
        RaiseBuffApplied(new BuffInfo(BUFF_KEY_POTION_SPEED, icon, duration, duration > 0f));
    }
    IEnumerator CoClearSpeedBuff(float duration)
    {
        yield return new WaitForSeconds(duration);
        PotionAtkSpeedBonus = 0f;
        _potionSpeedCo = null;
        RaiseBuffCleared(BUFF_KEY_POTION_SPEED);
    }

    // 슬39: 능력 전투 발동조건 런너 (장착 능력의 확률/시간 발동 처리)
    AbilityRunner _abilityRunner;
    public AbilityRunner AbilityRunner => _abilityRunner;   // 슬40: UI 버프 표시 연동용 노출
    public MonsterController CurrentTarget => _attackTarget;   // 능력 BonusDamage 대상

    // 캐릭터창 스탯 1포인트당 증가량: 0=STR(물공) 1=VIT(HP) 2=DEX(명중) 3=MAG(마공).
    // RefreshStats 적용과 UI_CharacterPopup 미리보기가 이 값을 공유한다.
    public static readonly float[] STAT_PER_POINT = { 2f, 20f, 1f, 2f };

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
        // 슬39: 능력 발동 런너 부착 (RefreshStats 전에 준비돼야 OnStatsReset 콜백 가능)
        _abilityRunner = gameObject.GetOrAddComponent<AbilityRunner>();
        _abilityRunner.Setup(this);
        // 슬라이드 62/63 — Monster→Player Miss popup, 추후 heal popup 등이 사용할 머리 위 offset.
        // Player prefab YAML에 직렬화 안 된 경우를 대비해 명시적으로 set (CreatureController 기본 2.5).
        DamageFontYOffset = 2.5f;
        RefreshStats();
        return true;
    }

    bool _companionSubscribed;

    void OnDestroy()
    {
        if (Managers.MinMon != null) Managers.MinMon.OnCompanionChanged -= OnCompanionChanged;
    }

    void OnCompanionChanged(int id) => RefreshCompanion();

    // 동행 미니몬 스폰/해제 동기화. CompanionMinMonId가 유효하면 발치에 스폰, -1이면 해제.
    public void RefreshCompanion()
    {
        int id = Managers.Game != null ? Managers.Game.CompanionMinMonId : -1;
        if (id == _companionId && (id < 0 || _companion != null)) return;

        if (_companion != null) { Managers.Resource.Destroy(_companion.gameObject); _companion = null; }
        _companionId = id;
        if (id >= 0)
            _companion = Managers.Object?.SpawnCompanion(id, this);
    }

    // 슬39 stateful 조건 카운터 — 연속 치명타/연속 회피. 전투 시작 시 리셋.
    int _critStreak;
    int _evadeStreak;

    public void StartBattle()
    {
        _inBattle = true;
        _critStreak = 0;
        _evadeStreak = 0;
        _abilityRunner?.OnBattleStart();   // 슬39: 전투 시작 발동조건
    }

    // 능력 효과/회복 — 전투 중 HP 회복(RecoveryRate 적용). 회복 시 발동조건(OnHeal) 통지.
    public void Heal(float amount)
    {
        if (State == Define.State.Dead || amount <= 0f) return;
        float healed = amount * (1f + RecoveryRate / 100f);
        float before = Hp;
        Hp = Mathf.Min(MaxHp, Hp + healed);
        if (Hp <= before) return;
        OnHpChanged?.Invoke(Hp, MaxHp);
        _abilityRunner?.Notify(Define.AbilityCondition.OnHeal);
    }

    // 슬라이드 67/69 — 마을 복귀 시 HP 회복 + Idle + 원위치 복귀 (사망/HOME/도중이탈 공통)
    public void StopBattle()
    {
        _inBattle = false;
        _abilityRunner?.OnBattleEnd();   // 슬39: 전투 종료 — 임시 버프/쿨 정리
        Hp = MaxHp;
        OnHpChanged?.Invoke(Hp, MaxHp);
        State = Define.State.Idle;
        transform.DOKill();   // 진행 중인 DOMove(off-screen) 중단
        transform.position = _originalPos;
    }

    // 마을 내 장소 이동(길드/연금공방/모험가협회 등) — 페이드 암전 중 호출.
    //  Play("Escape")로 시작한 달리기 애니/이동을 멈추고 즉시 원위치+Idle로 되돌려,
    //  페이드 인 시 잔여 이동이 보이지 않게 한다. (전투 정리는 안 함)
    public void ReturnHomeIdle()
    {
        transform.DOKill();
        if (_onHitCo != null) StopCoroutine(_onHitCo);
        _anim.speed = 1f;
        transform.position = _originalPos;
        State = Define.State.Idle;   // setter가 CrossFade(Idle)로 Escape 애니 덮음
    }

    // ────────────────────────────────────────
    //  Update
    // ────────────────────────────────────────
    void Update()
    {
        // 콤보 Tick — 항상 활성인 Player의 Update에서 구동(Managers.Update 플레이키 대비).
        // ComboManager.Tick은 프레임 디듑이 있어 다른 Update와 중복 호출돼도 1회만 처리됨.
        Managers.Combo?.Tick(Time.deltaTime);

        // 물약 쿨타임은 전투/마을 무관하게 항상 감소.
        // (UI_PotionPopup 쿨타임 바가 unscaled 시계로 돌므로 동일 시계 사용 —
        //  전투 밖에서 타이머가 멈춰 팝업 재오픈 시 쿨타임이 다시 도는 버그 방지)
        if (_autoPotionCd > 0f) _autoPotionCd -= Time.unscaledDeltaTime;
        if (_spdPotionCd  > 0f) _spdPotionCd  -= Time.unscaledDeltaTime;

        if (State == Define.State.Dead) return;
        if (!_inBattle) return;

        UpdateEngagementVisuals();
        AutoAttack();
        UpdateAutoPotion();
    }

    // 슬42: 자동물약 — 팝업 열림과 무관하게 전투 중 상시 동작.
    //  기억 + On + HP가 설정 비율 이하 + 쿨타임 종료 시 회복물약 자동 사용.
    float _autoPotionCd;
    void UpdateAutoPotion()
    {
        // (_autoPotionCd 감소는 Update에서 전투 여부와 무관하게 처리)
        var game = Managers.Game;
        if (game == null || !game.AutoPotionMemorized || !game.AutoPotionOn) return;
        if (_autoPotionCd > 0f) return;
        if (MaxHp <= 0f || Hp / MaxHp > game.AutoPotionHpRatio) return;

        int id = game.CurrentHPPotionDataId;
        if (Managers.Data?.HPPotionDic == null || !Managers.Data.HPPotionDic.TryGetValue(id, out var hp)) return;

        Heal(hp.FlatRecov + MaxHp * (hp.MaxHPRecov / 100f));
        SpawnHpPotionFx();
        _autoPotionCd = hp.Cooldown > 0 ? hp.Cooldown : 5f;
        OnAutoPotionUsed?.Invoke();   // 팝업이 열려 있으면 쿨타임 바 동기화용
    }

    // 슬42: HP 물약 사용 이펙트(HP Potion Ef)를 플레이어 자식으로 생성(위치는 프리팹 저작값). 자동/수동 공통.
    public void SpawnHpPotionFx()
    {
        Managers.Object?.ShowHpPotionEffectOn(transform);
    }

    // 수동 사용(팝업 클릭) 시에도 자동 쿨타임을 공유 — 직후 자동 사용이 중복 발동하지 않도록.
    public void StartHpPotionCooldown(float cd) { if (cd > _autoPotionCd) _autoPotionCd = cd; }

    // 자동물약 사용 시점 통지 (열려있는 UI_PotionPopup이 쿨타임 바를 동기화).
    public event System.Action OnAutoPotionUsed;
    public float AutoPotionCooldownNormalized => _autoPotionCd <= 0f ? 0f :
        (Managers.Data != null && Managers.Data.HPPotionDic.TryGetValue(Managers.Game.CurrentHPPotionDataId, out var hp) && hp.Cooldown > 0
            ? Mathf.Clamp01(_autoPotionCd / hp.Cooldown) : 0f);

    // 신속물약 쿨타임 — 팝업 인스턴스가 파괴돼도 남은 쿨타임을 유지 (HP 물약과 동일 패턴).
    float _spdPotionCd;
    public void StartSpdPotionCooldown(float cd) { if (cd > _spdPotionCd) _spdPotionCd = cd; }
    public float SpdPotionCooldownNormalized => _spdPotionCd <= 0f ? 0f :
        (Managers.Data != null && Managers.Data.SpeedPotionDic.TryGetValue(Managers.Game.CurrentSpeedPotionDataId, out var sp) && sp.Cooldown > 0
            ? Mathf.Clamp01(_spdPotionCd / sp.Cooldown) : 0f);

    // 슬라이드 57/58 — 몬스터 AtkRange 진입 여부에 따라:
    //   · 미engaged: BG scroll on, Player Run (Moving)
    //   · engaged:   BG scroll off, Player Idle (AutoAttack가 Attack로 전환)
    //
    // 몬스터 사망 직후엔 곧장 이동하지 않고 VICTORY_PAUSE_DURATION만큼 Idle + BG 정지 유지.
    // engaged → !engaged 전환 순간을 잡아 _victoryPauseUntil을 set하는 방식.
    const float VICTORY_PAUSE_DURATION = 2f;
    bool  _wasEngaged;
    float _victoryPauseUntil = -999f;

    void UpdateEngagementVisuals()
    {
        bool rawEngaged = HasMonsterInAtkRange();

        // engaged였다가 풀린 순간 = 몬스터가 막 죽거나 사라짐 → 승리 정지 타이머 시작
        if (_wasEngaged && !rawEngaged)
            _victoryPauseUntil = Time.time + VICTORY_PAUSE_DURATION;
        _wasEngaged = rawEngaged;

        bool inVictoryPause = Time.time < _victoryPauseUntil;
        bool engaged = rawEngaged || inVictoryPause;

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
    }

    // ────────────────────────────────────────
    //  자동 공격
    // ────────────────────────────────────────
    void AutoAttack()
    {
        if (State == Define.State.Attack) return;
        // 콤보 + 신속물약 공격속도 버프: 합산 %만큼 공격 사이클 전체가 빨라짐(쿨타임 + 애니 + 상태지속).
        float speedMul = 1f + (ComboAtkSpeedBonus() + PotionAtkSpeedBonus) / 100f;
        float effectiveCooldown = AtkCooldown / speedMul;
        if (Time.time < _lastAtkTime + effectiveCooldown) return;

        // 슬라이드 59 — 콜라이더 충돌로 engaged된 monster를 찾아 공격.
        // 거리 폴링은 콜라이더 사이즈와 어긋날 수 있어 HasEngaged flag로 통일.
        MonsterController target = FindEngagedMonster();
        if (target == null) return;

        _lastAtkTime = Time.time;
        _attackTarget = target;         // 슬라이드 61 — 키프레임 시점에 데미지 적용하기 위해 락온
        State = Define.State.Attack;
        // 공격속도만큼 Attack 애니 재생속도를 올려 키프레임(AttackTrigger)과 클립 종료가 함께 빨라지도록.
        if (_anim != null) _anim.speed = speedMul;

        // 명중/회피 dice + BattleMath.Resolve + TakeHit는 Attack.anim의 키프레임 이벤트
        // (AttackTrigger, 0.5s)에서 호출됨. OnHit으로 인터럽트되면 키프레임에 도달하지
        // 못해 자연스럽게 데미지 미적용 (슬라이드 60 — 인터럽트 = 빗나감).

        if (_attackResetCo != null) StopCoroutine(_attackResetCo);
        // Attack 상태 지속도 공격속도만큼 단축 — 클립 길이가 병목이 되지 않도록.
        _attackResetCo = StartCoroutine(CoResetStateAfterAttack(ATTACK_CLIP_LENGTH / speedMul));
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

    IEnumerator CoResetStateAfterAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        _attackResetCo = null;
        if (_anim != null) _anim.speed = 1f;   // 애니 재생속도 원복

        if (State == Define.State.Dead) yield break;
        if (State == Define.State.Attack)   // 그 사이 OnHit/Idle 등으로 바뀌었으면 건드리지 않음
            State = Define.State.Idle;
    }

    // ────────────────────────────────────────
    //  HP / EXP
    // ────────────────────────────────────────
    public override void TakeHit(BattleMath.DamageResult result, CreatureController attacker = null)
    {
        bool wasHit = result.Hit;

        // 슬39: 사망에 이르는 공격 — HP 차감 직전 통지 (Heal 효과가 발동하면 생존 가능)
        if (wasHit && result.FinalDamage >= Hp)
            _abilityRunner?.Notify(Define.AbilityCondition.FatalDamage);

        float before = Hp;
        base.TakeHit(result, attacker);
        OnHpChanged?.Invoke(Hp, MaxHp);

        // 슬39: 피격/회피/저체력 발동조건
        if (State == Define.State.Dead) return;
        if (!wasHit)
        {
            if (result.Evaded)   // 회피 기여분만 회피로 취급 (순수 빗나감은 상대 실수)
            {
                _evadeStreak++;
                _abilityRunner?.Notify(Define.AbilityCondition.SelfEvade);
                _abilityRunner?.Notify(Define.AbilityCondition.ConsecutiveEvade, _evadeStreak);
            }
            return;
        }
        float taken = before - Hp;
        if (taken > 0f)
        {
            _evadeStreak = 0;   // 피격 시 연속 회피 끊김
            _abilityRunner?.Notify(Define.AbilityCondition.OnDamaged);
        }
        if (MaxHp > 0f) _abilityRunner?.Notify(Define.AbilityCondition.LowHp, Hp / MaxHp * 100f);
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

        Managers.Sound?.PlaySFX("Sound_PlayeronHit");   // 플레이어 피격
        _anim.speed = 1f;   // 공격속도 버프로 올렸던 애니 속도 원복(피격 연출은 정상 속도)
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
        // 슬38 지원능력: 경험치 획득량 보너스(%)
        float expBonus = Managers.Ability != null ? Managers.Ability.ExpGainBonus / 100f : 0f;
        Managers.Game.Exp += amount * (GrowthRate / 100f) * (1f + expBonus);
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
            // _expRequired는 위 RefreshStats()가 LevelData(없으면 fallback 식)로 이미 갱신함.
            // 여기서 또 ×1.2 하면 세션 내 연속 레벨업과 재접속 시 요구 경험치가 달라지는 버그가 있었음.
        }
    }

    // ────────────────────────────────────────
    //  스탯 갱신
    // ────────────────────────────────────────
    // PlayerStatusData(Level==DataId)를 우선 참조. 데이터 누락 시에만 fallback 식 사용.
    // GameManager.GetPlayerMaxHp가 동일 lookup 키(Level)를 쓰므로 정합성 유지.
    // fullHeal=false: 장비 착탈/강화 등 스탯만 재계산하는 경로 — 현재 HP 유지(전투 중 무한회복 악용 방지).
    public void RefreshStats(bool fullHeal = true)
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

        Data.PlayerStatusData status = null;
        if (Managers.Data != null && Managers.Data.PlayerStatusDic != null)
            Managers.Data.PlayerStatusDic.TryGetValue(lv, out status);

        if (status != null)
        {
            // 데이터 기반: 슬라이드 18 시작 스탯 풀스펙
            MaxHp          = status.StartHP;
            PhysicalAtk    = status.StartStrength;
            MagicAtk       = status.StartMagic;
            CritRate       = status.StartCritRate;
            CritDamage     = status.StartCritDamage;
            Accuracy       = status.StartAccuracy;
            AtkSpeed       = status.StartAtkSpeed;
            PhysicalResist = status.StartPhysicalResist;
            MagicResist    = status.StartMagicResist;
            Evasion        = status.StartEvasion;
            RecoveryRate   = status.StartRecoveryRate;
            GrowthRate     = status.StartGrowthRate;
            DropRate       = status.StartDropRate;
            Def            = status.StartDef;   // StartDef 컬럼(레벨별 방어력) 매핑
        }
        else
        {
            // PlayerStatusData(DataId=Level, 1~300행)가 정상 로드되면 위 데이터 경로를 타고,
            // 데이터 누락 시에만 이 공식으로 떨어진다. 값은 Lv1~40 구간 데이터와 동일하게 맞춰둠.
            // 기본 밸런스: Lv1에 슬라임을 무난히 잡되 레벨업으로 점진 강해지도록.
            MaxHp       = 100 + (lv - 1) * 20;
            PhysicalAtk = 15  + (lv - 1) * 3;
            MagicAtk    = 10  + (lv - 1) * 2;
            Def         = 0   + (lv - 1) * 1;
            Accuracy    = 90;   // 명중 90% (잦은 빗나감으로 콤보 끊김 방지)
        }

        // 캐릭터창에서 분배한 스탯 포인트 적용 (영구 — GameData.StatInvest에 저장)
        int[] inv = Managers.Game?.StatInvest;
        if (inv != null && inv.Length >= 4)
        {
            PhysicalAtk += inv[0] * STAT_PER_POINT[0];
            MaxHp       += inv[1] * STAT_PER_POINT[1];
            Accuracy    += inv[2] * STAT_PER_POINT[2];
            MagicAtk    += inv[3] * STAT_PER_POINT[3];
        }

        // 대장간: 장착 장비의 기본 스탯 + 강화 보너스 가산
        Managers.Game?.ApplyEquipmentStats(this);

        // 슬38: 장착(기억)된 능력의 스탯 보너스 가산
        Managers.Ability?.ApplyTo(this);
        // 슬39: 스탯이 base+패시브로 리셋됐으니 런너의 임시 버프 추적도 비움(중복 환불 방지)
        _abilityRunner?.OnStatsReset();

        Hp = fullHeal ? MaxHp : Mathf.Min(Hp, MaxHp);
        OnHpChanged?.Invoke(Hp, MaxHp);
        // _expRequired가 갱신됐으니 EXP 슬라이더도 같은 시점에 신호 발화.
        // (BindPlayer가 늦게 구독한 경우엔 BindPlayer 안에서 초기 1회를 별도로 호출.)
        OnExpChanged?.Invoke(Managers.Game.Exp, _expRequired);

        // 동행 미니몬: 1회 구독 + 매 RefreshStats마다 동기화.
        //  (플레이어 Init의 RefreshStats는 LoadGame 전이라 CompanionMinMonId=-1 → GameScene이 LoadGame 후 RefreshStats를
        //   재호출할 때 실제 CompanionMinMonId로 스폰돼야 함. 구독 1회 가드 안에 RefreshCompanion을 두면 재호출이 스킵돼
        //   재시작 시 동행 미니몬이 안 뜬다 → RefreshCompanion은 가드 밖에서 매번 호출. 가드가 중복 스폰 방지.)
        if (Managers.MinMon != null)
        {
            if (!_companionSubscribed)
            {
                Managers.MinMon.OnCompanionChanged += OnCompanionChanged;
                _companionSubscribed = true;
            }
            RefreshCompanion();
        }
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

        Managers.Sound?.PlaySFX("Sound_PlayerAttack");   // 플레이어 공격
        // 슬39: 공격 시 발동조건
        _abilityRunner?.Notify(Define.AbilityCondition.OnAttack);

        // 콤보 피해증폭은 DmgAmp에 일시 가산해서 BattleMath에 전달
        float prevDmgAmp = DmgAmp;
        DmgAmp += ComboDmgBoostBonus();
        BattleMath.DamageResult result = BattleMath.Resolve(this, _attackTarget, AttackType.Physical);
        DmgAmp = prevDmgAmp;

        _attackTarget.TakeHit(result, this);

        // 타격(명중) 성공 시에만 콤보 누적
        if (result.Hit)
        {
            OnHitLanded?.Invoke(result.FinalDamage);
            // 슬39: 타격/치명타 발동조건
            _abilityRunner?.Notify(Define.AbilityCondition.OnHit);
            if (result.IsCrit)
            {
                _critStreak++;
                _abilityRunner?.Notify(Define.AbilityCondition.OnCrit);
                _abilityRunner?.Notify(Define.AbilityCondition.ConsecutiveCrit, _critStreak);
            }
            else
            {
                _critStreak = 0;   // 비치명 타격 — 연속 치명타 끊김
            }

            // 슬39: 상대 HP가 임계% 이하로 내려간 순간 (전투당 1회는 러너가 추적)
            if (_attackTarget != null && _attackTarget.State != Define.State.Dead && _attackTarget.MaxHp > 0f)
                _abilityRunner?.Notify(Define.AbilityCondition.EnemyLowHp, _attackTarget.Hp / _attackTarget.MaxHp * 100f);
        }
        else if (result.Evaded)
        {
            _abilityRunner?.Notify(Define.AbilityCondition.EnemyEvade);   // 상대방 회피
        }
        else
        {
            _abilityRunner?.Notify(Define.AbilityCondition.OnMiss);       // 순수 빗나감
        }

        // 슬39: [공격 후] — 명중/회피/빗나감 판정이 모두 끝난 뒤 발동 (결과와 무관)
        _abilityRunner?.Notify(Define.AbilityCondition.AfterAttack);
    }
}
