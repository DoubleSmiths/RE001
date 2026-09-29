using System;
using UnityEngine;
using static Define;

// v2 훈련 패널(ADR0030, 설계서 4.2) — 상태 창 안의 TrainingFrame 에 붙는다(빌더: Tools/Remake V2/Build Training Panel).
//  줄을 누르면 1단계, 누르고 있으면 연속으로 올린다. 골드가 모자라거나 상한이면 흐리게 보인다.
public class UI_TrainingPanel : UI_Base
{
    enum Texts
    {
        TrainingTitleText,
        TrainingCapTxt,
        ATKTrainTxt, ATKCostTxt,
        HPTrainTxt,  HPCostTxt,
        DEFTrainTxt, DEFCostTxt,
        CRITTrainTxt, CRITCostTxt,
    }

    // TrainingStat 순서와 같다(0 공격 1 체력 2 방어 3 치명피해)
    enum Buttons
    {
        ATKTrainBtn,
        HPTrainBtn,
        DEFTrainBtn,
        CRITTrainBtn,
    }

    static readonly string[] NAME_KEYS = { "TrainAtkText", "TrainHpText", "TrainDefText", "TrainCritDmgText" };
    const float HOLD_DELAY = 0.35f;      // 이만큼 누르고 있으면 연속 구매 시작
    const float HOLD_INTERVAL = 0.07f;
    static readonly Color COST_OK = new Color32(0xFF, 0xC8, 0x4E, 0xFF);
    static readonly Color COST_NO = new Color32(0x8A, 0x8A, 0x8A, 0xFF);
    const float ROW_DIM_ALPHA = 0.45f;

    int _holding = -1;
    float _holdTimer;
    bool _holdFired;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        foreach (Texts t in Enum.GetValues(typeof(Texts)))
            ApplyFont(GetText((int)t), FontType.UI);

        for (int i = 0; i < GameManager.TRAINING_STAT_COUNT; i++)
        {
            int stat = i;
            GameObject row = GetButton(i).gameObject;
            row.BindEvent(() => { if (!_holdFired) Train(stat); });
            row.BindEvent(() => { _holding = stat; _holdTimer = HOLD_DELAY; _holdFired = false; }, type: UIEvent.PointerDown);
            row.BindEvent(() => { if (_holding == stat) _holding = -1; }, type: UIEvent.PointerUp);
        }

        if (Managers.Game != null)
        {
            Managers.Game.OnTrainingChanged += Refresh;
            Managers.Game.OnResourceChanged += Refresh;
            Managers.Game.OnLanguageChanged += Refresh;
        }
        Refresh();
        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null)
        {
            Managers.Game.OnTrainingChanged -= Refresh;
            Managers.Game.OnResourceChanged -= Refresh;
            Managers.Game.OnLanguageChanged -= Refresh;
        }
        base.OnDestroy();
    }

    void Update()
    {
        if (_holding < 0) return;
        _holdTimer -= Time.unscaledDeltaTime;   // 전투 정지(도감 등) 중에도 UI 는 움직인다
        if (_holdTimer > 0f) return;
        _holdTimer = HOLD_INTERVAL;
        _holdFired = true;
        if (!Train(_holding)) _holding = -1;
    }

    bool Train(int stat)
    {
        bool ok = Managers.Game.TryTrain((TrainingStat)stat);
        if (ok)
        {
            Managers.Sound?.PlaySFX("SFX_BtnSelect");
            Transform t = GetButton(stat).transform;
            t.localScale = Vector3.one * 1.04f;   // 짧은 눌림 — 연속 구매 중에도 부담 없는 크기
        }
        return ok;
    }

    void LateUpdate()
    {
        // 눌림 크기를 되돌린다(Tween 없이 — 연속 구매 때 트윈이 쌓이지 않게)
        for (int i = 0; i < GameManager.TRAINING_STAT_COUNT; i++)
        {
            Transform t = GetButton(i).transform;
            if (t.localScale.x != 1f) t.localScale = Vector3.MoveTowards(t.localScale, Vector3.one, Time.unscaledDeltaTime * 0.5f);
        }
    }

    void Refresh()
    {
        var g = Managers.Game;
        if (g == null) return;
        int cap = g.TrainingCap;
        GetText((int)Texts.TrainingTitleText).text = Util.Localize("TrainingTitleText");
        GetText((int)Texts.TrainingCapTxt).text = string.Format(Util.Localize("TrainingCapFormat"), cap);

        for (int i = 0; i < GameManager.TRAINING_STAT_COUNT; i++)
        {
            var stat = (TrainingStat)i;
            int level = g.TrainingLevel(stat);
            bool maxed = level >= cap;
            int cost = g.TrainingCost(level + 1);
            bool affordable = !maxed && g.Gold >= cost;

            GetText(RowText(i)).text = string.Format(Util.Localize("TrainingRowFormat"),
                Util.Localize(NAME_KEYS[i]), level, g.TrainingBonus(stat).ToString("0.#"));
            var costText = GetText(RowText(i) + 1);
            costText.text = maxed ? Util.Localize("TrainingMaxText") : string.Format(Util.Localize("TrainingCostFormat"), cost);
            costText.color = affordable ? COST_OK : COST_NO;

            var canvasGroup = Util.GetOrAddComponent<CanvasGroup>(GetButton(i).gameObject);
            canvasGroup.alpha = affordable ? 1f : ROW_DIM_ALPHA;
        }
    }

    // Texts 에서 각 줄 이름 칸의 인덱스(비용 칸은 +1)
    static int RowText(int stat) => (int)Texts.ATKTrainTxt + stat * 2;
}
