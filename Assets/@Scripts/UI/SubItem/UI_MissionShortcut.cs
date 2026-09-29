using UnityEngine;

// v2 HUD 재화 줄의 미션·상점 버튼(ADR0030, 빌더: Tools/Remake V2/Build Mission Popup).
//  누르면 미션 창을 그 탭으로 연다. 받을 것이 있으면 모서리에 빨간 점 — 0.5초마다 확인.
public class UI_MissionShortcut : UI_Base
{
    public UI_MissionPopup.Tab _tab;

    const float POLL_INTERVAL = 0.5f;
    GameObject _badge;
    float _poll;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        gameObject.BindEvent(Open);
        // 알림 점은 이름이 ...Badge 인 자식(MissionBadge / ShopBadge — 한 프리팹 안에서 이름이 겹치지 않게)
        foreach (Transform child in transform)
            if (child.name.EndsWith("Badge")) _badge = child.gameObject;
        if (_badge != null) _badge.SetActive(false);
        return true;
    }

    void Open()
    {
        if (Managers.UI.GetPopupCount() > 0 && Object.FindAnyObjectByType<UI_MissionPopup>() != null) return;   // 연타로 두 번 열지 않게
        Managers.Sound?.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_MissionPopup>().SetInfo(_tab);
    }

    void Update()
    {
        _poll -= Time.unscaledDeltaTime;
        if (_poll > 0f) return;
        _poll = POLL_INTERVAL;
        bool show = _tab == UI_MissionPopup.Tab.Shop
            ? Managers.Shop != null && Managers.Shop.HasFreeReward          // 상점 버튼: 무료 크리스탈·월정액·오프라인 2배
            : (Managers.Mission != null && Managers.Mission.HasClaimable)   // 미션 버튼: 완료한 미션·출석
              || (Managers.Dungeon != null && Managers.Dungeon.HasFreeEntry)   //  + 오늘 무료 입장이 남은 요일 던전(ADR0034)
              || (Managers.Bond != null && Managers.Bond.HasReady)             //  + 끝낼 수 있는 마을 배달 의뢰(ADR0036)
              || (Managers.Promotion != null && Managers.Promotion.IsChallengeReady); //  + 도전할 수 있는 시련(ADR0037)
        if (_badge != null && _badge.activeSelf != show) _badge.SetActive(show);
    }
}
