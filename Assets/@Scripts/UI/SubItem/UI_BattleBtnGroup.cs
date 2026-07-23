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

        // 전투 기록 라벨 (슬라이드 50) — 정적 로컬라이즈.
        BattleLogText,     // "전투 기록"
        BattleTimeText,    // "전투 시간"
        DmgDealtText,      // "가한 피해량"
        DmgTakenText,      // "받은 피해량"
        HPRestoredText,    // "회복량"

        // 전투 기록 값 (동적) — 현재 조우한 몬스터 기준.
        BattleTimeNum,     // mm:ss.ss
        DmgDealtNum,       // Smooth 보간 합산
        DmgTakenNum,
        HPRestoredNum,

        // 버튼 텍스트 (슬라이드 51/52) — enum 이름 = prefab GameObject 이름.
        // 로컬라이즈 키는 enum 이름과 다르므로 Init 에서 명시적으로 세팅.
        TravelButtonText,    // "여정" (loc: TravelBtnText)
        ReturnButtonText,    // "귀환" / "확실해?" (loc: ReturnText / ReturnConfirmText)
    }

    enum Buttons
    {
        ATTACKBTN,      // 테스트용 수동 타격. 실제 전투는 PlayerController.AutoAttack 자동.
        ReturnButton,   // 슬라이드 51: 귀환 (2단계 확인 후 마을 복귀)
        TravelButton,   // 슬라이드 52: 여정 팝업 오픈
    }

    enum GameObjects
    {
        COMBOSYSTEM,
        BAR1,             // Slider
        ASBoostEffect,
        DMGBoostEffect,
        ITEMS,            // 슬라이드 49: 드롭 아이템 리스트 컨테이너 (HorizontalLayoutGroup)
    }

    #endregion

    // ── 콤보 Text 스타일 (슬라이드 47) ──
    // 초기/IDLE 상태의 ComboText / ComboNum 기본 색 (흰색).
    // 콤보 IDLE 색 = ComboData Lv0 ColorHex (하드코딩 금지 — 시트에서 결정)
    static Color COMBO_IDLE_COLOR => Managers.Combo?.GetRankColor(0) ?? Color.white;
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

    // ── 귀환 (슬라이드 51) ──
    const float RETURN_CONFIRM_TIMEOUT = 3f;
    bool    _returnConfirming;
    Tween   _returnRevertTween;

    // ── 드롭 아이템 리스트 (슬라이드 49) ──
    Transform _dropRoot;
    readonly Dictionary<int, UI_BattleDropItem> _dropSlots = new Dictionary<int, UI_BattleDropItem>();

    // ── 전투 기록 (슬라이드 50) ──
    const float LOG_SMOOTH_RATE = 12f;  // dt 기반 smooth 수렴 속도 (ComboManager Tick과 동일)
    MonsterController _logMonster;      // 현재 조우한 몬스터
    float _logTime;                     // mm:ss.ss
    float _logDealtTarget, _logDealtDisplay;
    float _logTakenTarget, _logTakenDisplay;
    float _logHealTarget,  _logHealDisplay;
    bool  _logFrozen = true;            // 시작 전 / 전투 종료 후
    bool  _logTimerRunning;
    float _logPrevPlayerHp = -1f;
    bool  _logPlayerHooked;

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

        // 슬라이드 51/52: 귀환/여정 버튼 바인드 + 초기 텍스트.
        GetButton((int)Buttons.ReturnButton)?.gameObject.BindEvent(OnClickReturn);
        GetButton((int)Buttons.TravelButton)?.gameObject.BindEvent(OnClickTravel);
        RefreshReturnButtonText(false);
        var travelTmp = GetText((int)Texts.TravelButtonText);
        if (travelTmp != null) travelTmp.text = Util.Localize(LocalizeKey.TravelBtnText);

        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();

        ChangeMap();
        // 슬라이드 57 — 진입 직후엔 Player Idle 고정 (배경 스크롤도 정지).
        // 배너 In Animation 종료 시점에 UI_GameScene.StartBattleSceneScroll가 Moving으로 전환 + BG scroll 시작.
        if (Managers.Game.Player != null)
            Managers.Game.Player.State = State.Idle;

        // 전투 기록: Player 이벤트 구독 + 최초 IDLE 상태로 UI 세팅.
        HookPlayerLogEvents();
        ResetBattleLog(freezeAfter: true);
        RefreshLogTexts();

        // 드롭 아이템 리스트: 컨테이너 캐시 + 프리펩 샘플 자식 제거 + 몬스터 드롭 이벤트 구독.
        BindDropListUI();

        return true;
    }

    public override void OnDestroy()
    {
        EndBattle();
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
        UnbindComboUI();
        UnhookPlayerLogEvents();
        UnbindDropListUI();
        _returnRevertTween?.Kill();
        _returnRevertTween = null;
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
            // 0, 1, 4, 6, 8, 9, 10, 11, 12, 17, 18 일단 하드로
            if (i == 0 || i == 1 || i == 4 || i == 6 || i == 8 || i == 9 || i == 10 || i == 11 || i == 12 || i == 17 || i == 18) ApplyFont(tmp, FontType.UI);
            //if (tmp.name == "ReturnButtonText") ApplyFont(tmp, FontType.UI);

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
            case Texts.BattleTimeNum: // 전투 기록 값은 실시간 갱신
            case Texts.DmgDealtNum:
            case Texts.DmgTakenNum:
            case Texts.HPRestoredNum:
            case Texts.ReturnButtonText:  // 귀환 ↔ 확실해? 동적 스왑
            case Texts.TravelButtonText:  // enum 이름과 loc 키가 달라 수동 Localize
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
        if (asLbl != null) asLbl.text = asActive ? Util.Localize(LocalizeKey.AttackSpeedText) : "";

        // DMGBoost / DMGBoostText
        var dmgVal = GetText((int)Texts.DMGBoost);
        var dmgLbl = GetText((int)Texts.DMGBoostText);
        bool dmgActive = stage != null && stage.DmgBoostBuff > 0f;
        if (dmgVal != null) dmgVal.text = dmgActive ? $"+{(int)stage.DmgBoostBuff}%" : "";
        if (dmgLbl != null) dmgLbl.text = dmgActive ? Util.Localize(LocalizeKey.DmgBoostText) : "";

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

    // ────────────────────────────────────────
    //  슬라이드 50: 전투 기록
    //    - 조우 시 전체 리셋 + 타이머 시작 (mm:ss.ss)
    //    - 몬스터 사망 / 플레이어 사망 시 freeze (수치 고정)
    //    - 장소 이동 시 UI 재생성 → 자동 리셋
    //    - 2/3/4 (가한/받은/회복량) 은 Smooth 보간 표시
    // ────────────────────────────────────────

    void HookPlayerLogEvents()
    {
        if (_logPlayerHooked) return;
        var p = Managers.Game?.Player;
        if (p == null) return;
        p.OnHitLanded += HandlePlayerHitLanded;
        p.OnHpChanged += HandlePlayerHpChanged;
        _logPlayerHooked = true;
    }

    void UnhookPlayerLogEvents()
    {
        if (!_logPlayerHooked) return;
        var p = Managers.Game?.Player;
        if (p != null)
        {
            p.OnHitLanded -= HandlePlayerHitLanded;
            p.OnHpChanged -= HandlePlayerHpChanged;
        }
        _logPlayerHooked = false;
    }

    public void StartBattleLog(MonsterController mc)
    {
        _logMonster = mc;
        ResetBattleLog(freezeAfter: false);
        _logTimerRunning = true;
        _logPrevPlayerHp = Managers.Game?.Player?.Hp ?? -1f;
        RefreshLogTexts();
    }

    void ResetBattleLog(bool freezeAfter)
    {
        _logTime          = 0f;
        _logDealtTarget   = _logDealtDisplay = 0f;
        _logTakenTarget   = _logTakenDisplay = 0f;
        _logHealTarget    = _logHealDisplay  = 0f;
        _logFrozen        = freezeAfter;
        _logTimerRunning  = !freezeAfter;
    }

    void HandlePlayerHitLanded(float dmg)
    {
        if (_logFrozen) return;
        _logDealtTarget += Mathf.Max(0f, dmg);
    }

    void HandlePlayerHpChanged(float cur, float max)
    {
        if (_logPrevPlayerHp < 0f) { _logPrevPlayerHp = cur; return; }
        float delta = cur - _logPrevPlayerHp;
        if (!_logFrozen)
        {
            if (delta < 0f)      _logTakenTarget += -delta;
            else if (delta > 0f) _logHealTarget  +=  delta;
        }
        _logPrevPlayerHp = cur;
    }

    void Update()
    {
        // 콤보 Tick 안전망 — Managers.Update가 플레이키하게 안 도는 경우 대비.
        // ComboManager.Tick은 프레임 디듑이 있어 Managers.Update와 동시 호출돼도 1회만 처리됨.
        Managers.Combo?.Tick(Time.deltaTime);

        // (1) 종료 감지 — 몬스터 사망 / 플레이어 사망 → freeze
        if (!_logFrozen)
        {
            if (_logMonster == null || _logMonster.State == State.Dead)
                _logFrozen = true;
            else if (Managers.Game?.Player != null && Managers.Game.Player.State == State.Dead)
                _logFrozen = true;
        }

        // (2) 타이머 — freeze 아닐 때만 누적 (unscaled: 슬로우 모션에도 정상 진행)
        if (_logTimerRunning && !_logFrozen)
            _logTime += Time.unscaledDeltaTime;

        // (3) Smooth 보간 (freeze 상태에서도 수렴하도록 허용)
        float t = 1f - Mathf.Exp(-LOG_SMOOTH_RATE * Time.deltaTime);
        _logDealtDisplay = Mathf.Lerp(_logDealtDisplay, _logDealtTarget, t);
        _logTakenDisplay = Mathf.Lerp(_logTakenDisplay, _logTakenTarget, t);
        _logHealDisplay  = Mathf.Lerp(_logHealDisplay,  _logHealTarget,  t);

        RefreshLogTexts();
    }

    // ────────────────────────────────────────
    //  슬라이드 49: 드롭 아이템 리스트
    //    - 몬스터 드롭 이벤트(MonsterController.OnItemDropped) 구독
    //    - 신규: UI_BattleDropItem 인스턴스화 → 등급순 정렬
    //    - 중복: 기존 슬롯 AddCount (내부 DOTween 재생)
    //    - 장소 이동(=UI_BattleBtnGroup 재생성) 시 자동 리셋
    // ────────────────────────────────────────

    void BindDropListUI()
    {
        var itemsGO = GetObject((int)GameObjects.ITEMS);
        _dropRoot = itemsGO != null ? itemsGO.transform : null;

        // 프리펩에 박힌 샘플 자식 제거 (빈 컨테이너로 시작).
        if (_dropRoot != null)
        {
            for (int i = _dropRoot.childCount - 1; i >= 0; i--)
                Destroy(_dropRoot.GetChild(i).gameObject);
        }
        _dropSlots.Clear();

        MonsterController.OnItemDropped += HandleMonsterItemDropped;
    }

    void UnbindDropListUI()
    {
        MonsterController.OnItemDropped -= HandleMonsterItemDropped;
        _dropSlots.Clear();
    }

    void HandleMonsterItemDropped(int dataId, int count)
    {
        if (_dropRoot == null || count <= 0) return;

        if (_dropSlots.TryGetValue(dataId, out var existing) && existing != null)
        {
            existing.AddCount(count);
        }
        else
        {
            var go = Managers.Resource.Instantiate("UI_BattleDropItem", _dropRoot);
            if (go == null) return;
            var slot = go.GetOrAddComponent<UI_BattleDropItem>();
            slot.SetInfo(dataId, count);
            _dropSlots[dataId] = slot;
        }

        SortDropSlotsByGrade();
    }

    // 등급순 정렬 — 높은 등급(Legendary, Mystic…)이 앞쪽(왼쪽) 에 오도록 SiblingIndex 재배치.
    void SortDropSlotsByGrade()
    {
        if (_dropRoot == null) return;
        var ordered = _dropSlots.Values
            .Where(s => s != null)
            .OrderByDescending(s => (int)s.Grade)
            .ThenBy(s => s.DataId);
        int idx = 0;
        foreach (var s in ordered)
            s.transform.SetSiblingIndex(idx++);
    }

    void RefreshLogTexts()
    {
        var timeTmp = GetText((int)Texts.BattleTimeNum);
        if (timeTmp != null)
        {
            // 센티초 단위 정수로 환산 후 분/초/센티초 분해 — float 포맷 rounding 회피.
            int centis = Mathf.Max(0, Mathf.FloorToInt(_logTime * 100f));
            int min    = centis / 6000;
            int sec    = (centis / 100) % 60;
            int cs     = centis % 100;
            timeTmp.text = $"{min:00}:{sec:00}.{cs:00}";
        }
        var dealtTmp = GetText((int)Texts.DmgDealtNum);
        if (dealtTmp != null) dealtTmp.text = Mathf.RoundToInt(Mathf.Max(0f, _logDealtDisplay)).ToString();
        var takenTmp = GetText((int)Texts.DmgTakenNum);
        if (takenTmp != null) takenTmp.text = Mathf.RoundToInt(Mathf.Max(0f, _logTakenDisplay)).ToString();
        var healTmp  = GetText((int)Texts.HPRestoredNum);
        if (healTmp  != null) healTmp.text  = Mathf.RoundToInt(Mathf.Max(0f, _logHealDisplay)).ToString();
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
    //  슬라이드 51/52: 귀환 / 여정 버튼
    //    - 두 버튼 모두 클릭마다 DOTweenAnimation 재생 (prefab 내장)
    //    - ReturnButton: 첫 클릭 → "확실해?" 3 초 카운트다운, 그 사이 재클릭 시 마을 복귀
    //    - TravelButton: UI_TravelPopup 오픈
    // ────────────────────────────────────────

    void OnClickReturn()
    {
        PlayButtonAnim(Buttons.ReturnButton);

        if (_returnConfirming)
        {
            // 확인 상태에서 재클릭 → 전투 강제 종료 + Fade 마을 복귀
            _returnRevertTween?.Kill();
            _returnRevertTween = null;
            _returnConfirming = false;
            ExecuteReturnToVillage();
            return;
        }

        // 첫 클릭 → 확인 상태 진입 + 3초 타이머 (Time.timeScale 무시: unscaled)
        _returnConfirming = true;
        RefreshReturnButtonText(true);
        _returnRevertTween?.Kill();
        _returnRevertTween = DOVirtual.DelayedCall(RETURN_CONFIRM_TIMEOUT, RevertReturnConfirm)
                                     .SetUpdate(true);
    }

    void RevertReturnConfirm()
    {
        _returnConfirming = false;
        _returnRevertTween = null;
        RefreshReturnButtonText(false);
    }

    void RefreshReturnButtonText(bool confirming)
    {
        var tmp = GetText((int)Texts.ReturnButtonText);
        if (tmp == null) return;
        tmp.text = Util.Localize(confirming ? LocalizeKey.ReturnConfirmText : LocalizeKey.ReturnText);
    }

    void ExecuteReturnToVillage()
    {
        // 슬라이드 69 — 즉시 EndBattle 하지 않음. EnterAreaWithFade가 BeginExitTransit으로
        // monster Idle 고정 + Player 화면 밖 이동을 Fade Out 동안 진행하고,
        // 검은 화면에 도달했을 때 SwapAreaBtnGroup → UI_BattleBtnGroup.OnDestroy → EndBattle 자연스럽게 정리.
        var scene = Managers.UI?.SceneUI as UI_GameScene;
        scene?.EnterAreaWithFade(AreaType.Village1);
    }

    void OnClickTravel()
    {
        PlayButtonAnim(Buttons.TravelButton);
        Managers.UI.ShowPopupUI<UI_TravelPopup>().SetInfo();
    }

    void PlayButtonAnim(Buttons btn)
    {
        var b = GetButton((int)btn);
        if (b == null) return;
        var anim = b.GetComponent<DOTweenAnimation>();
        anim?.DORestart();
    }

    // ────────────────────────────────────────
    //  Battle 루프 (기존 유지)
    // ────────────────────────────────────────

    public override void PlayEnterAnimation()
    {
        // 슬라이드 55 — 지역명은 UI_AreaNameBanner가 표시 (Toast 사용 중단).
        // 이 콜백은 EnterAreaWithFade의 banner onInComplete(t=2.0)에서 호출됨.
        StartBattle();
    }

    // 슬라이드 69 — 전투 도중 장소 이동/귀환 시:
    //   1-1. Monster Idle 고정 (Attack/Idle 등 어떤 상태든 Idle로)
    //   1-2. Player Run + 화면 밖 좌측으로 서서히 이동 (Fade Out 동안)
    //   1-3. Map Scrolling OFF
    //   1-4. Fade IN/OUT은 EnterAreaWithFade가 처리
    //   1-5. HP 회복은 StopBattle (UI_BattleBtnGroup OnDestroy → EndBattle)에서 처리
    // EnterAreaWithFade 시작 직전에 호출 → Fade Out 0.5s 동안 Player가 좌측으로 사라짐.
    public void BeginExitTransit()
    {
        // 1-1. 모든 살아있는 monster Idle 고정 (CoBattleLoop 정지는 OnDestroy → EndBattle에서)
        Transform monsterRoot = Managers.Object?.MonsterTransform;
        if (monsterRoot != null)
        {
            for (int i = 0; i < monsterRoot.childCount; i++)
            {
                var m = monsterRoot.GetChild(i).GetComponent<MonsterController>();
                if (m == null || m.State == State.Dead) continue;
                m.State = State.Idle;
                if (m.GetComponent<Rigidbody2D>() is Rigidbody2D rb)
                    rb.linearVelocity = Vector2.zero;
            }
        }

        // 1-3. Map Scrolling OFF
        BackgroundScroller scroller = null;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == "CH1BG" || go.name == "CH1BG(Clone)")
            {
                scroller = go.GetComponent<BackgroundScroller>();
                if (scroller != null) break;
            }
        }
        if (scroller != null) scroller.IsScrolling = false;

        // 1-2. Player Run + 화면 밖 좌측 이동 (Fade Out 0.5s 와 동기)
        var player = Managers.Game?.Player;
        if (player != null && player.State != State.Dead)
        {
            player.State = State.Moving;
            player.transform.DOKill();
            player.transform.DOMoveX(player.transform.position.x - 10f, 0.5f).SetEase(Ease.InQuad);
        }
    }

    void StartBattle()
    {
        PlayerController player = Managers.Game.Player;
        if (player == null) return;
        player.StartBattle();
        _battleCoroutine = StartCoroutine(CoBattleLoop());
    }

    // 슬라이드 58 — Player Run anim 시작 직후부터 4초 뒤 첫 몬스터 spawn,
    //               Player 기준 +12 X (화면 밖 오른쪽). 이후 사망 → 4초 → 다음 spawn.
    // Y는 monster prefab 별 sprite pivot에 따라 다르므로 CreatureData.SpawnY로 분리.
    const float MONSTER_SPAWN_DELAY = 2f;
    const float MONSTER_SPAWN_OFFSET_X = 12f;
    const float MONSTER_SPAWN_Y_FALLBACK = 2.3f;   // CreatureData 조회 실패 시 기본값

    System.Collections.IEnumerator CoBattleLoop()
    {
        Transform monsterRoot = Managers.Object.MonsterTransform;

        // 첫 몬스터: 4초 대기 (Player Run 시작 시점부터)
        yield return new WaitForSeconds(MONSTER_SPAWN_DELAY);

        while (true)
        {
            PlayerController player = Managers.Game.Player;
            if (player == null) yield break;

            // 데이터 드리븐: 현재 지역의 Habitat/EncounterRate로 슬라임/고블린 등 가중 랜덤 선택.
            int dataId = Managers.Object.PickFieldMonsterId(2001);
            float spawnY = MONSTER_SPAWN_Y_FALLBACK;
            if (Managers.Data.CreatureDic.TryGetValue(dataId, out var creatureData))
                spawnY = creatureData.SpawnY;

            Vector3 spawnPos = new Vector3(
                player.transform.position.x + MONSTER_SPAWN_OFFSET_X,
                spawnY,
                0f);

            MonsterController mc = Managers.Object.SpawnMonster(dataId, spawnPos);
            if (mc != null)
            {
                // 슬라이드 50/59 — 실제 조우(콜라이더 충돌) 시점에 HP Bar 호출 + 전투 기록 타이머 시작.
                // 슬라임이 스폰 후 좌측으로 스크롤하는 시간은 전투 시간이 아님 (이전엔 spawn 직후 시작 → 부정확).
                mc.OnEngaged += () =>
                {
                    Managers.UI.MakeSubItem<UI_MonsterHPBar>(Managers.UI.SceneUI.transform).SetInfo(mc);
                    StartBattleLog(mc);
                };
            }
            yield return new WaitUntil(() => monsterRoot.childCount == 0);

            // 다음 몬스터까지 6초
            yield return new WaitForSeconds(MONSTER_SPAWN_DELAY);
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

        // 전투 배경(CH1BG)만 정리. Village1_*_BG는 UI_Village1BtnGroup.SwitchBG가 자체 cleanup 하므로 건드리지 않음.
        // (Phase J 이후 EnterAreaWithFade가 Village1BtnGroup을 먼저 spawn한 뒤 BattleBtnGroup OnDestroy가 발화 →
        //  여기서 Village1_*_BG를 destroy하면 방금 스폰된 마을 BG가 같이 죽는 deferred-destroy 버그 발생함)
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == "CH1BG" || go.name == "CH1BG(Clone)")
                Destroy(go);
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
