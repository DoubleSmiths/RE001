using UnityEngine;

// v2 HUD 재화 줄의 미션 버튼(ADR0030, 빌더: Tools/Remake V2/Build Mission Popup).
//  누르면 미션 창을 연다. 받을 것(완료한 미션·출석)이 있으면 모서리에 '!' 를 띄운다 — 0.5초마다 확인.
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
        _badge = Util.FindChild(gameObject, "MissionBadge", true);
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
        bool show = Managers.Mission != null && Managers.Mission.HasClaimable;
        if (_badge != null && _badge.activeSelf != show) _badge.SetActive(show);
    }
}
