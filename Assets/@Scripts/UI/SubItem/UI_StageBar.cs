using UnityEngine;
using static Define;

// v2 스테이지 막대(ADR0030, 설계서 4.3) — UI_GameScene 의 StageBar 에 붙는다(빌더: Tools/Remake V2/Build Stage Bar).
//  전투 구역에서만 보인다. 사냥 중엔 '1-5 산들바람 숲 심층' + 처치 수, 보스전엔 남은 시간(빨간 막대).
//  보스에게 져서 자동 도전이 꺼지면 막대가 금색으로 깜빡이고, 누르면 다음 몬스터 대신 보스가 나온다.
//  이미 돌파한 스테이지는 이름이 금색이다.
public class UI_StageBar : UI_Base
{
    enum Texts
    {
        StageNameTxt,
        StageCountTxt,
    }

    enum Images
    {
        StageGaugeFill,       // 처치 수(초록 — 원래 EXP 막대 그림)
        StageGaugeBossFill,   // 보스 남은 시간(빨강 — 원래 HP 막대 그림)
    }

    enum Mode { Hunting, BossNext, ChallengeReady, BossFight }

    static readonly Color NAME_CLEARED = new Color32(0xFF, 0xE0, 0x8A, 0xFF);
    static readonly Color TEXT_READY   = new Color32(0xFF, 0xD2, 0x4A, 0xFF);
    static readonly Color FILL_WAITING = new Color(0.4f, 0.4f, 0.4f, 1f);
    const float BLINK_HZ = 2f;      // 도전 대기 깜빡임 — 켜짐/꺼짐 두 단계만(매 프레임 색을 바꾸면 HUD 캔버스가 매번 다시 배칭된다)

    CanvasGroup _group;
    int  _shownNode = -1, _shownKills = -1, _shownTenths = -1;
    Mode _shownMode = (Mode)(-1);
    bool _dirty = true;
    bool _shown = true;
    bool _blinkOn;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        ApplyFont(GetText((int)Texts.StageNameTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.StageCountTxt), FontType.UI);

        _group = Util.GetOrAddComponent<CanvasGroup>(gameObject);
        gameObject.BindEvent(OnClickBar);

        if (Managers.Game != null)
        {
            Managers.Game.OnStageChanged    += MarkDirty;
            Managers.Game.OnLanguageChanged += MarkDirty;
        }
        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null)
        {
            Managers.Game.OnStageChanged    -= MarkDirty;
            Managers.Game.OnLanguageChanged -= MarkDirty;
        }
        base.OnDestroy();
    }

    void MarkDirty() => _dirty = true;

    // 도전 대기일 때만 반응한다 — 자동 도전을 다시 켜면 지금 몬스터 다음에 보스가 나온다.
    void OnClickBar()
    {
        var g = Managers.Game;
        if (g == null || CurrentMode(g, g.CurrentTravelAreaId) != Mode.ChallengeReady) return;
        g.BossAutoChallenge = true;
        Managers.Sound?.PlaySFX("SFX_BtnSelect");
    }

    static Mode CurrentMode(GameManager g, int node)
    {
        if (g.BossFightActive) return Mode.BossFight;
        if (!g.IsBossReady(node)) return Mode.Hunting;
        return g.BossAutoChallenge ? Mode.BossNext : Mode.ChallengeReady;
    }

    // 값이 바뀐 칸만 다시 쓴다(보스전 타이머는 0.1초 단위) — 매 프레임 문자열을 만들지 않는다.
    void Update()
    {
        var g = Managers.Game;
        if (g == null) return;

        int node = g.CurrentTravelAreaId;
        bool show = g.InBattleArea && g.TryGetStageNumber(node, out _, out _);
        if (show != _shown)
        {
            _shown = show;
            _group.alpha = show ? 1f : 0f;
            _group.blocksRaycasts = show;
        }
        if (!show) return;

        Mode mode  = CurrentMode(g, node);
        int kills  = g.StageKills(node);
        int tenths = mode == Mode.BossFight ? Mathf.CeilToInt(g.BossTimeLeft * 10f) : 0;

        if (_dirty || node != _shownNode)
        {
            var name = GetText((int)Texts.StageNameTxt);
            name.text  = g.StageLabel(node);
            name.color = g.IsStageCleared(node) ? NAME_CLEARED : Color.white;
        }

        if (_dirty || node != _shownNode || mode != _shownMode || kills != _shownKills || tenths != _shownTenths)
            RefreshGauge(g, mode, kills, tenths);

        if (mode == Mode.ChallengeReady)
        {
            bool on = (int)(Time.unscaledTime * BLINK_HZ * 2f) % 2 == 0;
            if (on != _blinkOn)
            {
                _blinkOn = on;
                GetText((int)Texts.StageCountTxt).color = on ? TEXT_READY : new Color(TEXT_READY.r, TEXT_READY.g, TEXT_READY.b, 0.45f);
            }
        }

        _shownNode = node; _shownMode = mode; _shownKills = kills; _shownTenths = tenths;
        _dirty = false;
    }

    void RefreshGauge(GameManager g, Mode mode, int kills, int tenths)
    {
        int need = g.StageKillsToBoss;
        var fill = GetImage((int)Images.StageGaugeFill);
        var boss = GetImage((int)Images.StageGaugeBossFill);
        var text = GetText((int)Texts.StageCountTxt);

        bool fighting = mode == Mode.BossFight;
        bool waiting  = mode == Mode.BossNext || mode == Mode.ChallengeReady;
        fill.gameObject.SetActive(!fighting);
        boss.gameObject.SetActive(fighting);
        fill.color = waiting ? FILL_WAITING : Color.white;   // 꽉 찬 초록 위 금색 글자가 묻히지 않게 채움을 가라앉힌다
        text.color = waiting ? TEXT_READY : Color.white;

        switch (mode)
        {
            case Mode.BossFight:
                boss.fillAmount = g.BossTimeLimit > 0f ? g.BossTimeLeft / g.BossTimeLimit : 0f;
                text.text = string.Format(Util.Localize("StageBossTimerFormat"), tenths / 10f);
                break;
            case Mode.BossNext:
                fill.fillAmount = 1f;
                text.text = Util.Localize("StageBossNextText");
                break;
            case Mode.ChallengeReady:
                fill.fillAmount = 1f;
                text.text = Util.Localize("StageBossChallengeText");
                break;
            default:
                fill.fillAmount = (float)kills / need;
                text.text = string.Format(Util.Localize("StageKillFormat"), kills, need);
                break;
        }
    }
}
