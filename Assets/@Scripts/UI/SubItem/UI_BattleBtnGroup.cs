using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 전투 UI (슬라이드 47~52). VS COMBOSYSTEM StateMachine의 C# 포팅.
///
/// 실제 prefab GameObject:
///   COMBOSYSTEM                — 루트 컨테이너 (VS 제거됨)
///   ComboText                  — 라벨 "공세 단계:"
///   ComboTitle                 — Rank 이름 ("무딘"/"숙련된"/...)
///   ComboNum                   — NowCombo 숫자 표시
///   DMG_NUM                    — NowDmg 숫자 표시
///   BAR1                       — Slider (게이지)
///   ASBoostText / ASBoost      — 공격속도 라벨 / 값  (DOTweenAnimation: 최초 활성 / 값 변경)
///   DMGBoostText / DMGBoost    — 피해증폭 라벨 / 값  (DOTweenAnimation: 최초 활성 / 값 변경)
///   ASBoostEffect              — 공격속도 최초 활성 파티클 (prefab 내장 ParticleSystem)
///   DMGBoostEffect             — 피해증폭 최초 활성 파티클 (prefab 내장 ParticleSystem)
///   ATTACKBTN                  — (테스트용) 수동 타격 시뮬
/// </summary>
public class UI_BattleBtnGroup : UI_AreaBtnGroup
{
    #region Enum

    enum Images
    {
        // 추후 필요 시 Image 타겟 추가
    }

    enum Texts
    {
        // 콤보 영역
        ComboText,     // "공세 단계:" 라벨
        ComboTitle,    // Rank 이름 (ComboLv#Text Localize로 동적 갱신)
        ComboNum,      // NowCombo 숫자
        DMG_NUM,       // NowDmg 숫자

        // 버프 영역
        ASBoostText,   // "공격속도" 라벨
        ASBoost,       // "+15%" 값
        DMGBoostText,  // "피해증폭" 라벨
        DMGBoost,      // "+10%" 값

        // 전투 기록 (슬라이드 50 — 현재는 enum만 자리잡음, 값 갱신은 후속 작업)
        BattleLogText,
        BattleTimeText,
        DmgDealtText,
        DmgTakenText,
        HPRestoredText,

        // 버튼 텍스트 (슬라이드 51/52)
        TravelBtnText,
        ReturnText,
    }

    enum Buttons
    {
        ATTACKBTN,   // 테스트용 수동 타격. 실제 전투는 PlayerController.AutoAttack 자동.
    }

    enum GameObjects
    {
        COMBOSYSTEM,
        BAR1,             // Slider
        ASBoostEffect,
        DMGBoostEffect,
    }

    #endregion

    // ── 콤보 Text 스타일 (슬라이드 47) ──
    // 초기/IDLE 상태의 ComboText / ComboNum 기본 색 (흰색).
    static readonly Color COMBO_IDLE_COLOR = Color.white;
    static readonly Color32 COMBO_OUTLINE  = new Color32(0x00, 0x00, 0x00, 0xFF);
    const float COMBO_OUTLINE_X =  0.6f;
    const float COMBO_OUTLINE_Y = -0.6f;

    // ── Combo UI 캐시 ──
    Slider         _comboBar;
    Image          _comboBarFill;
    List<TMP_Text> _rankColorTexts;
    GameObject     _asBoostEffect;
    GameObject     _dmgBoostEffect;

    // 콤보 연출용 DOTween (prefab에 이미 붙어있는 DOTweenAnimation을 코드로 트리거)
    DOTweenAnimation _comboNumAnim;       // ComboNum    : 타격마다
    DOTweenAnimation _dmgNumAnim;         // DMG_NUM     : 타격마다
    DOTweenAnimation _comboTitleAnim;     // ComboTitle  : Rank up 시
    DOTweenAnimation _asBoostTextAnim;    // ASBoostText : AS 버프 최초 활성 시
    DOTweenAnimation _dmgBoostTextAnim;   // DMGBoostText: DMG 버프 최초 활성 시
    DOTweenAnimation _asBoostValAnim;     // ASBoost     : AS 값 변경 시
    DOTweenAnimation _dmgBoostValAnim;    // DMGBoost    : DMG 값 변경 시

    // 버프 값 diff 추적 (rank up 시 "새 값 vs 이전 값"이 달라지면 값 DOTween 실행)
    float _prevAsBuff;
    float _prevDmgBuff;

    // ── 배틀 루프 ──
    Coroutine _battleCoroutine;

    void Awake() => Init();

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        ApplyBattleTexts();
        BindComboUI();

        // 테스트 수동 타격: ATTACKBTN 클릭 시 OnHitLanded 시뮬 (에디터 편의용)
        GetButton((int)Buttons.ATTACKBTN)?.gameObject.BindEvent(OnClickAttackBtnForTesting);

        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();

        ChangeMap();
        if (Managers.Game.Player != null)
        {
            Managers.Game.Player.State = State.Moving;
            Managers.Game.Player.StartBattle();
        }

        return true;
    }

    public override void OnDestroy()
    {
        EndBattle();
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
        UnbindComboUI();
        base.OnDestroy();
    }

    // ────────────────────────────────────────
    //  슬라이드 47 + 48: Text 초기화 + 스타일
    // ────────────────────────────────────────

    void ApplyBattleTexts()
    {
        // 모든 전투 Text에 RiaSans-Bold + (정적 Localize)
        for (int i = 0; i < System.Enum.GetValues(typeof(Texts)).Length; i++)
        {
            var tmp = GetText(i);
            if (tmp == null) continue;
            ApplyFont(tmp, FontType.Battle);

            string enumKey = System.Enum.GetName(typeof(Texts), i);
            // 동적 갱신 대상은 빈 문자열로 시작, 아닌 것은 Localize
            if (IsDynamicText((Texts)i))
                tmp.text = "";
            else
                tmp.text = Util.Localize(enumKey);
        }

        // 콤보 라벨 스타일 (슬라이드 47): 초기 색 흰색, 검정 아웃라인 underlay.
        var comboLabel = GetText((int)Texts.ComboText);
        if (comboLabel != null)
        {
            comboLabel.color = COMBO_IDLE_COLOR;
            Util.SetTMPUnderlay(comboLabel, COMBO_OUTLINE, COMBO_OUTLINE_X, COMBO_OUTLINE_Y);
        }
    }

    static bool IsDynamicText(Texts t)
    {
        switch (t)
        {
            case Texts.ComboTitle:
            case Texts.ComboNum:
            case Texts.DMG_NUM:
            case Texts.ASBoost:
            case Texts.DMGBoost:
            case Texts.ASBoostText:   // Rank에 따라 표시/비표시
            case Texts.DMGBoostText:  // 동일
            case Texts.BattleTimeText:
            case Texts.DmgDealtText:
            case Texts.DmgTakenText:
            case Texts.HPRestoredText:
                return true;
            default:
                return false;
        }
    }

    // ────────────────────────────────────────
    //  슬라이드 48: 콤보 UI 바인딩 + 이벤트 연결
    // ────────────────────────────────────────

    void BindComboUI()
    {
        // BAR1 (Slider) 해석
        var barGO = GetObject((int)GameObjects.BAR1);
        if (barGO != null)
        {
            _comboBar     = barGO.GetComponent<Slider>();
            _comboBarFill = _comboBar != null && _comboBar.fillRect != null
                ? _comboBar.fillRect.GetComponent<Image>()
                : null;
            if (_comboBar != null) _comboBar.value = 0f;
        }

        _asBoostEffect  = GetObject((int)GameObjects.ASBoostEffect);
        _dmgBoostEffect = GetObject((int)GameObjects.DMGBoostEffect);

        // Prefab에 이미 구성된 DOTweenAnimation 참조 캐시
        var comboNumTmp     = GetText((int)Texts.ComboNum);
        var dmgNumTmp       = GetText((int)Texts.DMG_NUM);
        var comboTitleTmp   = GetText((int)Texts.ComboTitle);
        var asBoostTextTmp  = GetText((int)Texts.ASBoostText);
        var dmgBoostTextTmp = GetText((int)Texts.DMGBoostText);
        var asBoostValTmp   = GetText((int)Texts.ASBoost);
        var dmgBoostValTmp  = GetText((int)Texts.DMGBoost);
        if (comboNumTmp     != null) _comboNumAnim     = comboNumTmp.GetComponent<DOTweenAnimation>();
        if (dmgNumTmp       != null) _dmgNumAnim       = dmgNumTmp.GetComponent<DOTweenAnimation>();
        if (comboTitleTmp   != null) _comboTitleAnim   = comboTitleTmp.GetComponent<DOTweenAnimation>();
        if (asBoostTextTmp  != null) _asBoostTextAnim  = asBoostTextTmp.GetComponent<DOTweenAnimation>();
        if (dmgBoostTextTmp != null) _dmgBoostTextAnim = dmgBoostTextTmp.GetComponent<DOTweenAnimation>();
        if (asBoostValTmp   != null) _asBoostValAnim   = asBoostValTmp.GetComponent<DOTweenAnimation>();
        if (dmgBoostValTmp  != null) _dmgBoostValAnim  = dmgBoostValTmp.GetComponent<DOTweenAnimation>();

        // Rank Color 반영 대상 텍스트.
        //   ComboText  : "공세 단계:" 라벨 — Rank 색 따라 변함
        //   ComboTitle : 특정 상태 이름  — Rank 색 따라 변함
        //   ComboNum   : 공세 단계 숫자  — Rank 색 따라 변함
        //   (DMG_NUM = 고정 #E00000, ASBoost*/DMGBoost* 계열은 고정 색 유지)
        _rankColorTexts = new List<TMP_Text>
        {
            GetText((int)Texts.ComboText),
            GetText((int)Texts.ComboTitle),
            GetText((int)Texts.ComboNum),
        };

        // 초기 상태: 버프 이펙트 숨김
        _asBoostEffect?.SetActive(false);
        _dmgBoostEffect?.SetActive(false);

        if (Managers.Combo != null)
        {
            Managers.Combo.OnRankChanged        += HandleRankChanged;
            Managers.Combo.OnTimerChanged       += HandleTimerChanged;
            Managers.Combo.OnComboBreak         += HandleComboBreak;
            Managers.Combo.OnComboChanged       += HandleComboChanged;
            Managers.Combo.OnNowValuesChanged   += HandleNowValues;
            Managers.Combo.OnFirstBuffActivated += HandleFirstBuff;

            // 초기 바인딩 시: IDLE(combo=0)이면 HandleComboBreak로 정리, 진행 중이면 현재 Rank 반영.
            if (Managers.Combo.Combo > 0)
                HandleRankChanged(Managers.Combo.NowRank);
            else
                HandleComboBreak();

            HandleTimerChanged(Managers.Combo.NormalizedTimer);
            HandleNowValues(Managers.Combo.NowCombo, Managers.Combo.NowDmg);
        }
    }

    void UnbindComboUI()
    {
        if (Managers.Combo != null)
        {
            Managers.Combo.OnRankChanged        -= HandleRankChanged;
            Managers.Combo.OnTimerChanged       -= HandleTimerChanged;
            Managers.Combo.OnComboBreak         -= HandleComboBreak;
            Managers.Combo.OnComboChanged       -= HandleComboChanged;
            Managers.Combo.OnNowValuesChanged   -= HandleNowValues;
            Managers.Combo.OnFirstBuffActivated -= HandleFirstBuff;
        }
    }

    // ── Rank 변경 ──
    //   - ComboTitle: Localize("ComboLv{rank}Text")
    //   - ASBoost/DMGBoost: 값 갱신 + 라벨 ON/OFF
    //   - 이펙트 GameObject SetActive
    //   - Rank Color 전체 적용
    void HandleRankChanged(int rank)
    {
        var stage = Managers.Combo?.GetCurrentStage();

        // ComboTitle: Rank Name
        var titleTmp = GetText((int)Texts.ComboTitle);
        if (titleTmp != null)
            titleTmp.text = stage != null ? Util.Localize(stage.NameKey) : "";

        // ASBoost / ASBoostText
        var asVal = GetText((int)Texts.ASBoost);
        var asLbl = GetText((int)Texts.ASBoostText);
        bool asActive = stage != null && stage.AtkSpeedBuff > 0f;
        if (asVal != null) asVal.text = asActive ? $"+{(int)stage.AtkSpeedBuff}%" : "";
        if (asLbl != null) asLbl.text = asActive ? Util.Localize("AttackSpeedText") : "";

        // DMGBoost / DMGBoostText
        var dmgVal = GetText((int)Texts.DMGBoost);
        var dmgLbl = GetText((int)Texts.DMGBoostText);
        bool dmgActive = stage != null && stage.DmgBoostBuff > 0f;
        if (dmgVal != null) dmgVal.text = dmgActive ? $"+{(int)stage.DmgBoostBuff}%" : "";
        if (dmgLbl != null) dmgLbl.text = dmgActive ? Util.Localize("DmgBoostText") : "";

        // Rank Color 일괄 적용
        Color rankColor = Managers.Combo?.GetCurrentRankColor() ?? Color.white;
        foreach (var t in _rankColorTexts)
            if (t != null) t.color = rankColor;
        if (_comboBarFill != null) _comboBarFill.color = rankColor;

        // Rank up 연출: ComboTitle DOTween
        _comboTitleAnim?.DORestart();

        // 버프 값 변경 감지: 값이 0→양수 또는 양수→다른 양수로 변하면 값 텍스트 DOTween 실행.
        float newAs  = stage != null ? stage.AtkSpeedBuff : 0f;
        float newDmg = stage != null ? stage.DmgBoostBuff : 0f;
        if (newAs > 0f && !Mathf.Approximately(newAs, _prevAsBuff))
            _asBoostValAnim?.DORestart();
        if (newDmg > 0f && !Mathf.Approximately(newDmg, _prevDmgBuff))
            _dmgBoostValAnim?.DORestart();
        _prevAsBuff  = newAs;
        _prevDmgBuff = newDmg;
    }

    void HandleTimerChanged(float normalized)
    {
        if (_comboBar != null) _comboBar.value = normalized;
    }

    // 타격 발생(= 콤보 수 변경) 시: ComboNum/DMG_NUM DOTween 트리거
    void HandleComboChanged(int combo)
    {
        _comboNumAnim?.DORestart();
        _dmgNumAnim?.DORestart();
    }

    // 콤보 종료/초기화 시: Rank 관련 표시만 비움. ComboNum/DMG_NUM은 "0" 표시 유지.
    void HandleComboBreak()
    {
        var titleTmp = GetText((int)Texts.ComboTitle); if (titleTmp != null) titleTmp.text = "";
        var asVal    = GetText((int)Texts.ASBoost);     if (asVal    != null) asVal.text    = "";
        var asLbl    = GetText((int)Texts.ASBoostText); if (asLbl    != null) asLbl.text    = "";
        var dmgVal   = GetText((int)Texts.DMGBoost);    if (dmgVal   != null) dmgVal.text   = "";
        var dmgLbl   = GetText((int)Texts.DMGBoostText);if (dmgLbl   != null) dmgLbl.text   = "";
        if (_comboBar != null) _comboBar.value = 0f;
        HideBuffParticle(_asBoostEffect);
        HideBuffParticle(_dmgBoostEffect);

        // IDLE 복귀 — ComboText / ComboNum 모두 흰색으로 초기화.
        // DMG_NUM 은 Rank 영향 없이 prefab 고정 색(#E00000) 유지.
        var comboLabel = GetText((int)Texts.ComboText);
        if (comboLabel != null) comboLabel.color = COMBO_IDLE_COLOR;
        var comboNumTmp = GetText((int)Texts.ComboNum);
        if (comboNumTmp != null) comboNumTmp.color = COMBO_IDLE_COLOR;

        _prevAsBuff  = 0f;
        _prevDmgBuff = 0f;
    }

    // Smooth 보간된 NowCombo / NowDmg → Text 표시. 0 일 때도 "0" 유지.
    void HandleNowValues(float nowCombo, float nowDmg)
    {
        var comboTmp = GetText((int)Texts.ComboNum);
        if (comboTmp != null)
            comboTmp.text = Mathf.RoundToInt(Mathf.Max(0f, nowCombo)).ToString();

        var dmgTmp = GetText((int)Texts.DMG_NUM);
        if (dmgTmp != null)
            dmgTmp.text = Mathf.RoundToInt(Mathf.Max(0f, nowDmg)).ToString();
    }

    // 슬라이드 48: 특정 버프 최초 활성 시 — 라벨 DOTween + 파티클 동시 실행
    //   AS:  ASBoostText DOTween ∥ ASBoostEffect 파티클
    //   DMG: DMGBoostText DOTween ∥ DMGBoostEffect 파티클
    void HandleFirstBuff(ComboManager.BuffKind buff)
    {
        bool isAS = buff == ComboManager.BuffKind.AttackSpeed;
        DOTweenAnimation textAnim = isAS ? _asBoostTextAnim : _dmgBoostTextAnim;
        GameObject       effect   = isAS ? _asBoostEffect   : _dmgBoostEffect;

        textAnim?.DORestart();
        ShowBuffParticle(effect);
    }

    // prefab 파티클은 playOnAwake=true, LocalScale=(0,0,0) 상태. Scale-Up으로 노출한 뒤 자동 소멸.
    void ShowBuffParticle(GameObject effect)
    {
        if (effect == null) return;
        effect.SetActive(true);
        effect.transform.DOKill();
        effect.transform.localScale = Vector3.zero;
        effect.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);

        var ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) { ps.Clear(true); ps.Play(true); }

        DOVirtual.DelayedCall(1.7f, () => HideBuffParticle(effect));
    }

    void HideBuffParticle(GameObject effect)
    {
        if (effect == null) return;
        effect.transform.DOKill();
        effect.transform.localScale = Vector3.zero;
        effect.SetActive(false);
    }

    // ── 에디터 테스트용: ATTACKBTN 클릭 시 타격 시뮬 ──
    //   - 플레이어/몬스터 없이도 콤보 흐름(누적→Rank 전이→버프→타이머→리셋)을 검증 가능.
    //   - 한 번의 클릭 = 한 번의 타격(hit) + 임의 데미지 1회.
    void OnClickAttackBtnForTesting()
    {
        if (Managers.Combo == null)
        {
            Debug.LogWarning("[UI_BattleBtnGroup] ATTACKBTN: Managers.Combo is null");
            return;
        }
        Managers.Combo.SimulateHit(100f);
    }

    // ────────────────────────────────────────
    //  Quest Notify (기존 유지)
    // ────────────────────────────────────────

    void RefreshNotify()
    {
        // 현 prefab에 TravelButtonQuestVFX가 없는 상태. 존재 여부 확인 후 없으면 skip.
    }

    // ────────────────────────────────────────
    //  Battle 루프 (기존 유지)
    // ────────────────────────────────────────

    public override void PlayEnterAnimation()
    {
        Managers.UI.ShowToast("산들바람 숲(입구)");
        StartBattle();
    }

    void StartBattle()
    {
        PlayerController player = Managers.Game.Player;
        if (player == null) return;
        player.StartBattle();
        _battleCoroutine = StartCoroutine(CoBattleLoop());
    }

    System.Collections.IEnumerator CoBattleLoop()
    {
        Transform monsterRoot = Managers.Object.MonsterTransform;
        Vector3 spawnPos = new Vector3(5, 2, 0);

        while (true)
        {
            MonsterController mc = Managers.Object.SpawnMonster(2001, spawnPos);
            if (mc != null)
            {
                mc.OnEngaged += () =>
                {
                    Managers.UI.MakeSubItem<UI_MonsterHPBar>(Managers.UI.SceneUI.transform).SetInfo(mc);
                };
            }
            yield return new WaitUntil(() => monsterRoot.childCount == 0);
            yield return new WaitForSeconds(1f);
        }
    }

    void EndBattle()
    {
        Managers.Game.Player?.StopBattle();

        if (_battleCoroutine != null)
        {
            StopCoroutine(_battleCoroutine);
            _battleCoroutine = null;
        }

        Transform monsterRoot = Managers.Object?.MonsterTransform;
        if (monsterRoot != null)
        {
            for (int i = monsterRoot.childCount - 1; i >= 0; i--)
                Managers.Resource.Destroy(monsterRoot.GetChild(i).gameObject);
        }

        if (Managers.UI?.SceneUI != null)
        {
            foreach (var bar in Managers.UI.SceneUI.GetComponentsInChildren<UI_MonsterHPBar>(true))
                Managers.Resource.Destroy(bar.gameObject);
        }
    }

    void ChangeMap()
    {
        var legacyBG = GameObject.Find("BackgroundImage");
        if (legacyBG != null)
        {
            var sr = legacyBG.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1, 1, 1, 0);
        }

        foreach (var key in new[] { "Village1_Main_BG", "Village1_Guild_BG", "Village1_Alchemy_BG" })
        {
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == key || go.name == key + "(Clone)")
                    Destroy(go);
            }
        }

        Managers.Resource.Instantiate("CH1BG");

        var villageBG = Managers.Resource.Instantiate("Village1_Main_BG");
        if (villageBG != null)
        {
            foreach (var t in villageBG.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Village1_Main_BG_Img")
                {
                    t.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }
}
