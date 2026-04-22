using System;
using Data;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 슬라이드 48: 콤보 시스템.
/// - 타격(hit) 시 현재 단계의 HitGain 만큼 타이머 추가 (MaxTimer로 clamp)
/// - 타격 누적 HitsToPromote 도달 시 상위 단계로 승격 + 버프 교체
/// - 타이머 감소 → 0 도달 시 슬로우 모션 + FX + Lv0 초기화
/// 버프는 AtkSpeedBonus / DmgBoostBonus 프로퍼티로 외부에서 조회.
/// </summary>
public class ComboManager
{
    public int   CurrentLevel    { get; private set; } = 0;
    public float CurrentTimer    { get; private set; } = 0f;
    public float NormalizedTimer { get; private set; } = 0f;
    public int   CurrentHitCount { get; private set; } = 0;   // 현 단계 내 누적 타격

    public float AtkSpeedBonus { get; private set; } = 0f;    // %
    public float DmgBoostBonus { get; private set; } = 0f;    // %

    public event Action<int>   OnLevelChanged;   // new level
    public event Action<float> OnTimerChanged;   // normalized 0~1
    public event Action        OnComboReset;

    PlayerController _player;
    bool _slowMoInFlight;

    // ────────────────────────────────────────
    //  Init / Cleanup
    // ────────────────────────────────────────

    public void Init()
    {
        CurrentLevel    = 0;
        CurrentTimer    = 0f;
        CurrentHitCount = 0;
        AtkSpeedBonus   = 0f;
        DmgBoostBonus   = 0f;
        AttachPlayer(Managers.Game?.Player);
    }

    public void Clear()
    {
        DetachPlayer();
    }

    /// <summary>Player 인스턴스 교체 시 호출 (GameScene.Awake 등).</summary>
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
    //  Frame 갱신
    // ────────────────────────────────────────

    public void Tick(float dt)
    {
        if (CurrentLevel <= 0 && CurrentTimer <= 0f) return;

        CurrentTimer -= dt;
        if (CurrentTimer <= 0f)
        {
            CurrentTimer = 0f;
            TriggerComboReset();
        }
        else
        {
            FireTimerChanged();
        }
    }

    // ────────────────────────────────────────
    //  Hit 처리
    // ────────────────────────────────────────

    void HandleHit()
    {
        var stage = GetStage(CurrentLevel);
        if (stage == null) return;

        CurrentTimer = Mathf.Min(CurrentTimer + stage.HitGain, stage.MaxTimer);
        CurrentHitCount++;

        // 승격 체크 — HitsToPromote <= 0 이면 승격 불가 (최상위 단계)
        if (stage.HitsToPromote > 0 && CurrentHitCount >= stage.HitsToPromote)
        {
            Promote();
        }
        else
        {
            FireTimerChanged();
        }
    }

    void Promote()
    {
        int nextLevel = CurrentLevel + 1;
        if (!Managers.Data.ComboStageDic.ContainsKey(nextLevel))
        {
            FireTimerChanged();
            return;
        }

        CurrentLevel = nextLevel;
        CurrentHitCount = 0;

        var stage = GetStage(CurrentLevel);
        if (stage != null)
        {
            // 승격 시 게이지 절반에서 재시작 (데이터에 엔트리 시드가 필요하면 여기 조정)
            CurrentTimer  = stage.MaxTimer * 0.5f;
            AtkSpeedBonus = stage.AtkSpeedBuff;
            DmgBoostBonus = stage.DmgBoostBuff;
        }

        OnLevelChanged?.Invoke(CurrentLevel);
        FireTimerChanged();
    }

    // ────────────────────────────────────────
    //  Reset (타이머 0 → 슬로우 + FX)
    // ────────────────────────────────────────

    void TriggerComboReset()
    {
        // 실제 콤보가 있었던 경우에만 슬로우 연출
        bool hadCombo = CurrentLevel > 0 || CurrentHitCount > 0;

        CurrentLevel    = 0;
        CurrentTimer    = 0f;
        CurrentHitCount = 0;
        AtkSpeedBonus   = 0f;
        DmgBoostBonus   = 0f;

        OnLevelChanged?.Invoke(0);
        OnComboReset?.Invoke();
        FireTimerChanged();

        if (hadCombo) PlaySlowMoFX();
    }

    void PlaySlowMoFX()
    {
        if (_slowMoInFlight) return;
        _slowMoInFlight = true;

        float prevScale = Time.timeScale;
        Time.timeScale  = 0.3f;

        DG.Tweening.DOVirtual.DelayedCall(0.4f, () =>
        {
            Time.timeScale = prevScale > 0f ? prevScale : 1f;
            _slowMoInFlight = false;
        }).SetUpdate(true);

        // 파티클 FX는 Player 위치에 스폰 (에셋 없으면 무시)
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
    //  Helpers
    // ────────────────────────────────────────

    ComboStageData GetStage(int level)
    {
        if (Managers.Data?.ComboStageDic == null) return null;
        return Managers.Data.ComboStageDic.TryGetValue(level, out var s) ? s : null;
    }

    void FireTimerChanged()
    {
        var stage = GetStage(CurrentLevel);
        NormalizedTimer = (stage != null && stage.MaxTimer > 0f)
            ? Mathf.Clamp01(CurrentTimer / stage.MaxTimer)
            : 0f;
        OnTimerChanged?.Invoke(NormalizedTimer);
    }

    public ComboStageData GetCurrentStage() => GetStage(CurrentLevel);
}
