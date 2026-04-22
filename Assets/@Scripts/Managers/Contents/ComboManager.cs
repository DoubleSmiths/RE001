using System;
using System.Collections.Generic;
using Data;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 슬라이드 48: 콤보 시스템 — VS(COMBOSYSTEM StateMachine)의 C# 포팅.
///
/// VS FSM 구조(2 State):
///   IDLE → [첫 타격] → ACTIVE → [타이머 0] → IDLE
///
/// 상태:
///   Combo (int)     — 누적 타격 수 (VS 'combo')
///   NowCombo (float)— 표시용 Smooth 보간 (VS 'now combo')
///   NowRank (int)   — 현 Rank (VS 'nowRank')
///   CurrentTimer    — 게이지 실시간 (초)
///   Dmg (float)     — 누적 데미지 (VS 'dmg')
///   NowDmg (float)  — 표시용 Smooth 보간 (VS 'now dmg')
///
/// 버프:
///   AtkSpeedBonus, DmgBoostBonus  — 외부(Creature)에서 조회
///
/// 이벤트:
///   OnRankChanged(int)       — Rank 전이 (업/다운 모두)
///   OnTimerChanged(float)    — 정규화된 타이머 0~1
///   OnComboBreak             — 타이머 0 → 초기화
///   OnComboChanged(int)      — combo count 변경
///   OnNowValuesChanged(nowCombo, nowDmg) — Smooth 보간 값 변경 (표시용)
///   OnFirstBuffActivated(BuffKind) — 특정 버프 첫 활성화 (Quest_Done_Effect 파티클용)
/// </summary>
public class ComboManager
{
    public enum BuffKind { AttackSpeed, DmgBoost }

    // ── VS 변수 대응 ──
    public int   Combo           { get; private set; } = 0;
    public float NowCombo        { get; private set; } = 0f;
    public int   NowRank         { get; private set; } = 0;
    public float CurrentTimer    { get; private set; } = 0f;
    public float NormalizedTimer { get; private set; } = 0f;
    public float Dmg             { get; private set; } = 0f;
    public float NowDmg          { get; private set; } = 0f;

    // ── 버프 ──
    public float AtkSpeedBonus { get; private set; } = 0f;
    public float DmgBoostBonus { get; private set; } = 0f;

    // ── 이벤트 ──
    public event Action<int>         OnRankChanged;
    public event Action<float>       OnTimerChanged;
    public event Action              OnComboBreak;
    public event Action<int>         OnComboChanged;
    public event Action<float,float> OnNowValuesChanged;   // (nowCombo, nowDmg)
    public event Action<BuffKind>    OnFirstBuffActivated;

    // ── 내부 ──
    PlayerController _player;
    bool _slowMoInFlight;
    List<ComboStageData> _stagesSorted;  // HitsRequired 오름차순

    // 현재 콤보 세션에서 이미 활성화된 버프 추적 (첫 활성화 감지용)
    readonly HashSet<BuffKind> _seenBuffsThisSession = new HashSet<BuffKind>();

    // ────────────────────────────────────────
    //  Init / Cleanup
    // ────────────────────────────────────────

    public void Init()
    {
        BuildStagesSorted();
        ResetState(fireEvents: false);
        AttachPlayer(Managers.Game?.Player);
    }

    public void Clear()
    {
        DetachPlayer();
    }

    void BuildStagesSorted()
    {
        _stagesSorted = new List<ComboStageData>();
        if (Managers.Data?.ComboStageDic == null) return;
        _stagesSorted.AddRange(Managers.Data.ComboStageDic.Values);
        _stagesSorted.Sort((a, b) => a.HitsRequired.CompareTo(b.HitsRequired));
    }

    public void AttachPlayer(PlayerController p)
    {
        DetachPlayer();
        _player = p;
        if (p != null) p.OnHitLanded += HandleHit;
    }

    void DetachPlayer()
    {
        if (_player != null) _player.OnHitLanded -= HandleHit;
        _player = null;
    }

    // ────────────────────────────────────────
    //  Frame 갱신 (Smooth 보간 + 타이머)
    // ────────────────────────────────────────

    public void Tick(float dt)
    {
        // VS 패턴: Mathf.SmoothStep(now, target, 0.3f) — 프레임당 약 30% 수렴
        // FPS 독립성을 위해 dt 기반으로 변환 (초당 수렴 속도)
        //   factor = 1 - exp(-k*dt) 가 정석이지만,
        //   기존 VS 느낌 유지 위해 근사 Lerp(dt*rate) 사용.
        const float SMOOTH_RATE = 12f;  // dt=1/60 기준 약 0.2 전진 → VS 0.3 과 유사한 체감
        float t = 1f - Mathf.Exp(-SMOOTH_RATE * dt);

        float prevNowCombo = NowCombo;
        float prevNowDmg   = NowDmg;
        NowCombo = Mathf.Lerp(NowCombo, Combo, t);
        NowDmg   = Mathf.Lerp(NowDmg,   Dmg,   t);

        // 아주 작은 잔차 clamp
        if (Combo == 0 && NowCombo < 0.01f) NowCombo = 0f;
        if (Dmg  == 0 && NowDmg  < 0.01f)  NowDmg  = 0f;

        if (!Mathf.Approximately(NowCombo, prevNowCombo) || !Mathf.Approximately(NowDmg, prevNowDmg))
            OnNowValuesChanged?.Invoke(NowCombo, NowDmg);

        // 타이머 감소
        if (Combo <= 0 && CurrentTimer <= 0f) return;

        CurrentTimer -= dt;
        if (CurrentTimer <= 0f)
        {
            CurrentTimer = 0f;
            TriggerComboBreak();
        }
        else
        {
            // Rank 다운 체크: 현재 Rank의 HitsRequired 이상을 유지해야 하므로
            //   combo가 줄어들 일은 없지만 만약 외부에서 Combo를 직접 조작하면 재계산 가능.
            //   여기서는 간단히 timer 변화 이벤트만 발행.
            FireTimerChanged();
        }
    }

    // ────────────────────────────────────────
    //  Hit 처리 (Player.OnHitLanded(damage) 위임)
    // ────────────────────────────────────────

    /// <summary>테스트/에디터용: 실제 플레이어 없이 수동 타격 시뮬레이션.</summary>
    public void SimulateHit(float damage) => HandleHit(damage);

    void HandleHit(float damage)
    {
        bool wasIdle = (Combo == 0);      // IDLE → ACTIVE 진입 감지
        Combo++;
        Dmg += Mathf.Max(0f, damage);
        OnComboChanged?.Invoke(Combo);

        // (1) Rank 확정 먼저: 첫 타격이면 Level 0 상태 강제 진입 (ComboTitle/색상 초기 반영)
        int newRank = CalcRankFromCombo(Combo);
        if (wasIdle || newRank != NowRank)
            ChangeRank(newRank);

        // (2) 확정된 Rank의 stage 기준으로 타이머 가산 (HitGain 적용, MaxTimer clamp)
        var stage = GetCurrentStage();
        if (stage != null)
            CurrentTimer = Mathf.Min(CurrentTimer + stage.HitGain, stage.MaxTimer);

        FireTimerChanged();
    }

    int CalcRankFromCombo(int combo)
    {
        int rank = 0;
        if (_stagesSorted == null) return 0;
        foreach (var s in _stagesSorted)
        {
            if (combo >= s.HitsRequired && s.Level > rank)
                rank = s.Level;
        }
        return rank;
    }

    void ChangeRank(int newRank)
    {
        int oldRank = NowRank;
        NowRank = newRank;

        var stage = GetCurrentStage();
        if (stage != null)
        {
            float oldAS  = AtkSpeedBonus;
            float oldDmg = DmgBoostBonus;
            AtkSpeedBonus = stage.AtkSpeedBuff;
            DmgBoostBonus = stage.DmgBoostBuff;

            // 첫 버프 활성화 감지 (이번 콤보 세션 내에서 처음 양수로 바뀌는 시점)
            if (stage.AtkSpeedBuff > 0f && _seenBuffsThisSession.Add(BuffKind.AttackSpeed))
                OnFirstBuffActivated?.Invoke(BuffKind.AttackSpeed);
            if (stage.DmgBoostBuff > 0f && _seenBuffsThisSession.Add(BuffKind.DmgBoost))
                OnFirstBuffActivated?.Invoke(BuffKind.DmgBoost);
        }

        OnRankChanged?.Invoke(NowRank);
        FireTimerChanged();
    }

    // ────────────────────────────────────────
    //  Combo Break (VS 'combobreak')
    // ────────────────────────────────────────

    void TriggerComboBreak()
    {
        bool hadCombo = Combo > 0 || NowRank > 0;
        ResetState(fireEvents: true);

        if (hadCombo)
        {
            OnComboBreak?.Invoke();
            PlaySlowMoFX();
        }
    }

    void ResetState(bool fireEvents)
    {
        Combo           = 0;
        NowRank         = 0;
        CurrentTimer    = 0f;
        Dmg             = 0f;
        AtkSpeedBonus   = 0f;
        DmgBoostBonus   = 0f;
        NormalizedTimer = 0f;
        _seenBuffsThisSession.Clear();
        // NowCombo, NowDmg는 Tick에서 smooth하게 0으로 수렴 (즉시 리셋 X)

        if (fireEvents)
        {
            OnComboChanged?.Invoke(0);
            OnRankChanged?.Invoke(0);
            OnTimerChanged?.Invoke(0f);
        }
    }

    void PlaySlowMoFX()
    {
        if (_slowMoInFlight) return;
        _slowMoInFlight = true;

        float prevScale = Time.timeScale;
        Time.timeScale  = 0.3f;

        DOVirtual.DelayedCall(0.4f, () =>
        {
            Time.timeScale = prevScale > 0f ? prevScale : 1f;
            _slowMoInFlight = false;
        }).SetUpdate(true);

        if (_player != null)
        {
            var fx = Managers.Resource.Load<GameObject>("FX_Combo_Break");
            if (fx != null)
            {
                var go = GameObject.Instantiate(fx, _player.transform.position, Quaternion.identity);
                GameObject.Destroy(go, 2f);
            }
        }
    }

    // ────────────────────────────────────────
    //  조회 헬퍼
    // ────────────────────────────────────────

    public ComboStageData GetCurrentStage()
    {
        if (Managers.Data?.ComboStageDic == null) return null;
        return Managers.Data.ComboStageDic.TryGetValue(NowRank, out var s) ? s : null;
    }

    public Color GetCurrentRankColor()
    {
        var stage = GetCurrentStage();
        if (stage == null || string.IsNullOrEmpty(stage.ColorHex)) return Color.white;
        return Util.HexToColor(stage.ColorHex.TrimStart('#'));
    }

    void FireTimerChanged()
    {
        var stage = GetCurrentStage();
        NormalizedTimer = (stage != null && stage.MaxTimer > 0f)
            ? Mathf.Clamp01(CurrentTimer / stage.MaxTimer)
            : 0f;
        OnTimerChanged?.Invoke(NormalizedTimer);
    }
}
