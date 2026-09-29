using UnityEngine;
using UnityEngine.UI;
using static Define;

// v2 막기 입력(ADR0030, 설계서 4.4) — UI_GameScene 의 GuardTapArea 에 붙는다(빌더: Tools/Remake V2/Build Guard Input).
//  전투 화면(게임 뷰)을 누르면 막는다. PC 는 Space 도. 한 손 세로 화면이라 따로 버튼을 두지 않고 화면 전체가 버튼이다.
//  HUD(스테이지 막대·퀘스트·몬스터 HP 바·버프)는 이 칸보다 위에 그려져 그쪽을 누르면 여기로 오지 않는다.
//  마을에서는 레이캐스트를 꺼서 화면 탭을 가로채지 않는다. 첫 공격 예고 '!' 때 한 번 도움말을 띄운다.
public class UI_GuardInput : UI_Base
{
    Image _area;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _area = GetComponent<Image>();
        gameObject.BindEvent(Guard, type: UIEvent.PointerDown);
        MonsterController.OnAttackTelegraph += OnTelegraph;
        return true;
    }

    public override void OnDestroy()
    {
        MonsterController.OnAttackTelegraph -= OnTelegraph;
        base.OnDestroy();
    }

    void Update()
    {
        bool battle = Managers.Game != null && Managers.Game.InBattleArea;
        if (_area != null && _area.raycastTarget != battle) _area.raycastTarget = battle;
        if (battle && Input.GetKeyDown(KeyCode.Space)) Guard();
    }

    static void Guard()
    {
        if (Managers.UI != null && Managers.UI.IsBattlePaused) return;   // 도감 등으로 멈춘 동안은 받지 않는다
        var player = Managers.Game?.Player;   // ?. 는 파괴된 오브젝트(fake null)를 걸러내지 못한다 — == 로 확인
        if (player != null) player.TryGuard();
    }

    void OnTelegraph()
    {
        var g = Managers.Game;
        if (g == null || g.GuardHintSeen) return;
        g.GuardHintSeen = true;
        Managers.UI.ShowToast(Util.Localize("GuardHintText"));
    }
}
