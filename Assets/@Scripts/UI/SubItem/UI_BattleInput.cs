using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// v2 전투 입력(ADR0033) — UI_GameScene/In-Frame/BattleInput (빌더: Tools/Remake V2/Build Battle Input).
//  · 전투 화면(BattleTapArea)을 누르면 벤다. PC 는 Space·Z 도.
//  · 방패 버튼(BattleGuardBtn)을 누르면 막는다(가드·패링, ADR0030). PC 는 X·Shift 도. 재사용 대기는 어둡게 덮어 보여 준다.
//  · AUTO 버튼(BattleAutoBtn)으로 자동 전투를 켜고 끈다(GameData.AutoBattle) — 켜 두면 예전처럼 알아서 벤다.
//  세 칸은 형제다 — 버튼이 탭 칸의 자식이면 누름 이벤트가 탭 칸까지 올라가 방패 한 번이 막기+공격이 된다.
//  HUD(스테이지 막대·퀘스트·HP 바·버프)는 이 묶음보다 뒤 형제라 위에 그려지고 먼저 눌린다.
//  마을에서는 버튼을 숨기고 탭 칸의 레이캐스트를 끈다. 첫 맞붙음에 '눌러서 공격', 첫 공격 예고 '!'에 '방패로 막기' 도움말.
public class UI_BattleInput : UI_Base
{
    enum GameObjects { BattleTapArea, BattleAutoBtn, BattleGuardBtn }
    enum Images { BattleTapArea, BattleAutoBtn, BattleGuardCool }
    enum Texts { BattleAutoTxt }

    static readonly Color AUTO_ON_TEXT  = new Color32(0xFF, 0xE0, 0x6A, 0xFF);
    static readonly Color AUTO_OFF_TEXT = new Color32(0x9A, 0x9A, 0x9A, 0xFF);
    static readonly Color AUTO_OFF_TINT = new Color(0.45f, 0.45f, 0.45f, 1f);

    bool _shown;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        BindText(typeof(Texts));

        GetObject((int)GameObjects.BattleTapArea).BindEvent(Attack, type: UIEvent.PointerDown);
        GetObject((int)GameObjects.BattleGuardBtn).BindEvent(Guard, type: UIEvent.PointerDown);
        GetObject((int)GameObjects.BattleAutoBtn).BindEvent(ToggleAuto);

        MonsterController.OnAttackTelegraph += OnTelegraph;
        MonsterController.OnAnyEngaged += OnEngaged;
        if (Managers.Game != null)
        {
            Managers.Game.OnAutoBattleChanged += RefreshAuto;
            Managers.Game.OnLanguageChanged += RefreshAuto;
        }
        RefreshAuto();
        SetShown(Managers.Game != null && Managers.Game.InBattleArea);
        return true;
    }

    public override void OnDestroy()
    {
        MonsterController.OnAttackTelegraph -= OnTelegraph;
        MonsterController.OnAnyEngaged -= OnEngaged;
        if (Managers.Game != null)
        {
            Managers.Game.OnAutoBattleChanged -= RefreshAuto;
            Managers.Game.OnLanguageChanged -= RefreshAuto;
        }
        base.OnDestroy();
    }

    void Update()
    {
        bool battle = Managers.Game != null && Managers.Game.InBattleArea;
        if (battle != _shown) SetShown(battle);
        if (!battle) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z)) Attack();
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift)) Guard();

        var player = Managers.Game.Player;
        GetImage((int)Images.BattleGuardCool).fillAmount = player != null ? player.GuardCooldownNormalized : 0f;
    }

    void SetShown(bool battle)
    {
        _shown = battle;
        GetImage((int)Images.BattleTapArea).raycastTarget = battle;
        GetObject((int)GameObjects.BattleAutoBtn).SetActive(battle);
        GetObject((int)GameObjects.BattleGuardBtn).SetActive(battle);
    }

    static void Attack()
    {
        var player = Managers.Game?.Player;   // ?. 는 파괴된 오브젝트(fake null)를 걸러내지 못한다 — == 로 확인
        if (player != null) player.RequestAttack();
    }

    static void Guard()
    {
        if (Managers.UI != null && Managers.UI.IsBattlePaused) return;   // 도감 등으로 멈춘 동안은 받지 않는다
        var player = Managers.Game?.Player;
        if (player != null) player.TryGuard();
    }

    void ToggleAuto()
    {
        var g = Managers.Game;
        if (g == null) return;
        g.AutoBattle = !g.AutoBattle;
        Managers.UI.ShowToast(Util.Localize(g.AutoBattle ? "AutoBattleOnText" : "AutoBattleOffText"));
    }

    void RefreshAuto()
    {
        bool on = Managers.Game != null && Managers.Game.AutoBattle;
        GetImage((int)Images.BattleAutoBtn).color = on ? Color.white : AUTO_OFF_TINT;
        var txt = GetText((int)Texts.BattleAutoTxt);
        txt.text  = Util.Localize("AutoBattleText");
        txt.color = on ? AUTO_ON_TEXT : AUTO_OFF_TEXT;
    }

    void OnEngaged()
    {
        var g = Managers.Game;
        if (g == null || g.TapHintSeen) return;
        g.TapHintSeen = true;
        Managers.UI.ShowToast(Util.Localize("TapHintText"));
    }

    void OnTelegraph()
    {
        // 방패 버튼을 들썩여 손이 갈 곳을 알려 준다(매번 — 예고는 공격마다 뜬다)
        var btn = GetObject((int)GameObjects.BattleGuardBtn).transform;
        btn.DOKill(true);
        btn.DOPunchScale(Vector3.one * 0.18f, 0.3f, 6, 0.6f).SetUpdate(true).SetLink(btn.gameObject);

        var g = Managers.Game;
        if (g == null || g.GuardHintSeen) return;
        g.GuardHintSeen = true;
        Managers.UI.ShowToast(Util.Localize("GuardHintText"));
    }
}
