using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 전투 UI (슬라이드 47~). 기존 Village1 레이아웃 잔재를 정리하고
/// 슬라이드 47 스펙의 Texts/리소스 배치를 기준으로 재구성.
///
/// 슬라이드 47 범위:
///   - Texts enum 정의 (콤보 8종 + 전투 스탯 7종 + 버튼 2종)
///   - 전 Text에 RiaSans-Bold 폰트 일괄 적용 (FontType.Battle)
///   - 콤보 Text: 색 #FF001F, Outline #000000 (0.6, -0.6)
///   - Localization 자동 적용
/// </summary>
public class UI_BattleBtnGroup : UI_AreaBtnGroup
{
    #region Enum

    enum Images
    {
        ComboGuage,             // 콤보 게이지 fill 이미지 (Image.Type=Filled, Horizontal)
        TravelButtonQuestVFX_Img, // prefab에 TravelButtonQuestVFX GameObject로 존재
    }

    enum Texts
    {
        // 슬라이드 48 콤보 시스템 Text (슬라이드 47에서 필드 정의)
        ComboText,
        ComboLv0Text,
        ComboLv1Text,
        ComboLv2Text,
        ComboLv3Text,
        ComboLv4Text,
        ComboLv5Text,
        ComboLv6Text,

        // 전투 기록 / 스탯 표기
        BattleLogText,
        DmgBoostText,
        AttackSpeedText,
        BattleTimeText,
        DmgDealtText,
        DmgTakenText,
        HPRestoredText,

        // 화면 하단 버튼
        TravelBtnText,
        ReturnText,
    }

    enum Buttons
    {
        TempButton1,
        TempButton2,
        TempButton3,
        TempButton4,
        TempButton5,
        TravelButton,
    }

    enum GameObjects
    {
        TravelButtonQuestVFX,
        ASBoostEffect,     // VS ASBoostEffect — 공격속도 버프 시 노출
        DMGBoostEffect,    // VS DMGBoostEffect — 피해증폭 버프 시 노출
    }

    #endregion

    // 콤보 Text 스타일 — 슬라이드 47 스펙
    static readonly Color COMBO_COLOR    = new Color32(0xFF, 0x00, 0x1F, 0xFF); // #FF001F
    static readonly Color COMBO_OUTLINE  = new Color32(0x00, 0x00, 0x00, 0xFF); // #000000
    const float COMBO_OUTLINE_X =  0.6f;
    const float COMBO_OUTLINE_Y = -0.6f;

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

        GetButton((int)Buttons.TravelButton)?.gameObject.BindEvent(OnClickTravel);

        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged += RefreshNotify;
        RefreshNotify();

        ChangeMap();
        if (Managers.Game.Player != null)
        {
            Managers.Game.Player.State = State.Moving;
            Managers.Game.Player.StartBattle();
        }

        for (int i = 0; i < 6; i++)
        {
            var tempBtn = GetButton(i)?.gameObject;
            if (tempBtn != null) tempBtn.SetActive(false);
        }

        return true;
    }

    public override void OnDestroy()
    {
        EndBattle();
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshNotify;
        UnbindComboUI();
    }

    // ────────────────────────────────────────
    //  슬라이드 48: 콤보 UI 바인딩
    // ────────────────────────────────────────

    Image _comboGuage;
    readonly List<GameObject> _comboLvNodes = new List<GameObject>();

    void BindComboUI()
    {
        _comboGuage = GetImage((int)Images.ComboGuage);
        if (_comboGuage != null)
        {
            _comboGuage.type = Image.Type.Filled;
            _comboGuage.fillMethod = Image.FillMethod.Horizontal;
            _comboGuage.fillAmount = 0f;
        }

        // Lv0~Lv6 Text는 현 Rank만 보이도록 캐시
        _comboLvNodes.Clear();
        _comboLvNodes.Add(GetText((int)Texts.ComboLv0Text)?.gameObject);
        _comboLvNodes.Add(GetText((int)Texts.ComboLv1Text)?.gameObject);
        _comboLvNodes.Add(GetText((int)Texts.ComboLv2Text)?.gameObject);
        _comboLvNodes.Add(GetText((int)Texts.ComboLv3Text)?.gameObject);
        _comboLvNodes.Add(GetText((int)Texts.ComboLv4Text)?.gameObject);
        _comboLvNodes.Add(GetText((int)Texts.ComboLv5Text)?.gameObject);
        _comboLvNodes.Add(GetText((int)Texts.ComboLv6Text)?.gameObject);

        // 초기 상태: 모든 Rank Text / Boost Effect 숨김
        foreach (var go in _comboLvNodes)
            if (go != null) go.SetActive(false);
        GetObject((int)GameObjects.ASBoostEffect)?.SetActive(false);
        GetObject((int)GameObjects.DMGBoostEffect)?.SetActive(false);

        if (Managers.Combo != null)
        {
            Managers.Combo.OnRankChanged        += OnComboRankChanged;
            Managers.Combo.OnTimerChanged       += OnComboTimerChanged;
            Managers.Combo.OnComboBreak         += OnComboBreak;
            Managers.Combo.OnComboChanged       += OnComboValueChanged;
            Managers.Combo.OnNowValuesChanged   += OnComboNowValuesChanged;
            Managers.Combo.OnFirstBuffActivated += OnComboFirstBuffActivated;
            OnComboRankChanged(Managers.Combo.NowRank);
            OnComboTimerChanged(Managers.Combo.NormalizedTimer);
            OnComboNowValuesChanged(Managers.Combo.NowCombo, Managers.Combo.NowDmg);
        }
    }

    void UnbindComboUI()
    {
        if (Managers.Combo != null)
        {
            Managers.Combo.OnRankChanged        -= OnComboRankChanged;
            Managers.Combo.OnTimerChanged       -= OnComboTimerChanged;
            Managers.Combo.OnComboBreak         -= OnComboBreak;
            Managers.Combo.OnComboChanged       -= OnComboValueChanged;
            Managers.Combo.OnNowValuesChanged   -= OnComboNowValuesChanged;
            Managers.Combo.OnFirstBuffActivated -= OnComboFirstBuffActivated;
        }
    }

    void OnComboRankChanged(int rank)
    {
        // 현 Rank Text만 활성, 나머지 비활성
        for (int i = 0; i < _comboLvNodes.Count; i++)
        {
            if (_comboLvNodes[i] != null)
                _comboLvNodes[i].SetActive(i == rank);
        }

        // VS ASBoostEffect / DMGBoostEffect 토글 — 현재 stage의 버프가 활성이면 노출
        var stage = Managers.Combo?.GetCurrentStage();
        bool asActive  = stage != null && stage.AtkSpeedBuff > 0f;
        bool dmgActive = stage != null && stage.DmgBoostBuff > 0f;
        GetObject((int)GameObjects.ASBoostEffect)?.SetActive(asActive);
        GetObject((int)GameObjects.DMGBoostEffect)?.SetActive(dmgActive);

        // Rank별 Color — ComboText, 현 Rank Text, ComboGuage fill에 일괄 적용
        Color rankColor = Managers.Combo?.GetCurrentRankColor() ?? Color.white;
        ApplyRankColor(rankColor, rank);
    }

    void ApplyRankColor(Color color, int rank)
    {
        // ComboText (공세 단계:) 색상도 Rank에 맞춰 바뀜
        var comboTitle = GetText((int)Texts.ComboText);
        if (comboTitle != null) comboTitle.color = color;

        // 현 Rank의 Lv#Text 색상
        if (rank >= 0 && rank < _comboLvNodes.Count && _comboLvNodes[rank] != null)
        {
            var tmp = _comboLvNodes[rank].GetComponent<TMP_Text>();
            if (tmp != null) tmp.color = color;
        }

        // ComboGuage fill 색상
        if (_comboGuage != null) _comboGuage.color = color;
    }

    void OnComboTimerChanged(float normalized)
    {
        if (_comboGuage != null)
            _comboGuage.fillAmount = normalized;
    }

    void OnComboValueChanged(int combo)
    {
        // 원본 int combo 값은 직접 표시하지 않음 (Smooth 보간된 NowCombo 사용)
    }

    void OnComboNowValuesChanged(float nowCombo, float nowDmg)
    {
        // BattleLogText 등에 Smooth 보간 표시 — 현재 prefab 구조상 직접 반영 대상이 제한적.
        // 총 피해량(DmgDealtText 별도 label)을 이 값으로 갱신하는 지점.
        // TODO: 슬라이드 50에서 DmgDealtText / 전투시간 등 별도 로직으로 확장 예정.
        // 지금은 진단 로그 수준만 유지. (필요 시 여기서 TMP_Text에 nowCombo/nowDmg 표시)
    }

    void OnComboFirstBuffActivated(ComboManager.BuffKind buff)
    {
        // 슬라이드 48 스펙: 처음 특정 긍정 효과 받을 시 Quest_Done_Effect 파티클 생성
        var fx = Managers.Resource.Load<GameObject>("Quest_Done_Effect");
        if (fx == null) return;

        GameObject target = buff == ComboManager.BuffKind.AttackSpeed
            ? GetObject((int)GameObjects.ASBoostEffect)
            : GetObject((int)GameObjects.DMGBoostEffect);
        Transform parent = target != null ? target.transform : transform;

        var go = GameObject.Instantiate(fx, parent);
        go.transform.localPosition = Vector3.zero;
        GameObject.Destroy(go, 3f);
    }

    void OnComboBreak()
    {
        // VS 'combobreak' — 모든 Rank Text / Boost Effect 즉시 숨김
        foreach (var go in _comboLvNodes)
            if (go != null) go.SetActive(false);
        GetObject((int)GameObjects.ASBoostEffect)?.SetActive(false);
        GetObject((int)GameObjects.DMGBoostEffect)?.SetActive(false);
        if (_comboGuage != null)
            _comboGuage.fillAmount = 0f;
    }

    // ────────────────────────────────────────
    //  슬라이드 47: Text 필드 일괄 스타일링
    // ────────────────────────────────────────

    /// <summary>모든 전투 Text에 RiaSans-Bold 폰트 적용 + Localization 값 채우기.
    /// 콤보 Text는 #FF001F 색 + 검은 Outline 추가 적용.</summary>
    void ApplyBattleTexts()
    {
        // 기본 localization 매핑 (TextKey 문자열이 enum 이름과 동일하므로 직접 재사용)
        for (int i = 0; i < System.Enum.GetValues(typeof(Texts)).Length; i++)
        {
            var tmp = GetText(i);
            if (tmp == null) continue;

            // 폰트: 전투 UI 전용 RiaSans-Bold
            ApplyFont(tmp, FontType.Battle);

            // Localization 본문
            string key = System.Enum.GetName(typeof(Texts), i);
            tmp.text = Util.Localize(key);
        }

        // 콤보 계열(8종): 색상 + Outline 스타일
        for (int i = (int)Texts.ComboText; i <= (int)Texts.ComboLv6Text; i++)
        {
            var tmp = GetText(i);
            if (tmp == null) continue;
            tmp.color = COMBO_COLOR;
            Util.SetTMPUnderlay(tmp, COMBO_OUTLINE, COMBO_OUTLINE_X, COMBO_OUTLINE_Y);
        }
    }

    // ────────────────────────────────────────
    //  배틀 루프 / Travel 복귀
    // ────────────────────────────────────────

    public override void PlayEnterAnimation()
    {
        Managers.UI.ShowToast("산들바람 숲(입구)");

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < 6; i++)
        {
            var tempBtn = GetButton(i)?.gameObject;
            if (tempBtn == null) continue;
            seq.AppendCallback(() => tempBtn.SetActive(true));
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() => tempBtn.GetComponent<DOTweenAnimation>()?.DORestart());
        }

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
            MonsterController mc = Managers.Object.SpawnMonster(2001, spawnPos); // TODO : 몬스터 테이블 연동
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

    void RefreshNotify()
    {
        var list = Managers.Quest.GetCurrentNotifyList();
        bool travelNotify = list.Exists(n => n.BtnID == VILLAGE1_TRAVEL_QUEST_VFX);
        GetObject((int)GameObjects.TravelButtonQuestVFX)?.SetActive(travelNotify);
    }

    void OnClickTravel()
    {
        EndBattle();

        var ch1 = GameObject.Find("CH1BG") ?? GameObject.Find("CH1BG(Clone)");
        if (ch1 != null) Destroy(ch1);

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene?.EnterAreaWithFade(AreaType.Village1);

        UpdateLastTravelAreaToVillage1();
    }

    void UpdateLastTravelAreaToVillage1()
    {
        if (Managers.Data?.TravelAreaDic == null) return;

        var village = Managers.Data.TravelAreaDic.Values
            .FirstOrDefault(n => n.SceneName == "Village1"
                                 && n.IsEnterable == 1
                                 && string.Equals(n.Category, "TOWN",
                                     System.StringComparison.OrdinalIgnoreCase));
        if (village != null)
            Managers.Game.CurrentTravelAreaId = village.DataId;
    }

    void ChangeMap()
    {
        // Village1→Battle 전환 시 SwitchBG가 legacy BackgroundImage를 이미 비활성화했을 수 있음
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
