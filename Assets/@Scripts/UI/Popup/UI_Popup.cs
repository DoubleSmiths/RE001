using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Popup : UI_Base
{
    public bool _isClosing = false;
    public RectTransform _frameRT;

    // 팝업마다 열릴/닫힐 위치를 다르게 설정할 수 있도록 노출
    // 기본값: 아래에서 올라옴 (-120)
    protected float _openFromY = -120f;
    protected float _closeToY  = -120f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.UI.SetCanvas(gameObject, true);

        // Camera Setting
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;

        ApplyDimBackdrop();

        return true;
    }

    // 기획서 p1-슬8: "모든 Popup은 스크린 전체 크기의 검은색 layer(BlackBG_Layer)를 지닌다"
    //   Sprite: None / Color: 000000 (alpha 220) / Fade 0.3s OutQuad
    // 프리팹들엔 전체 크기 배경이 있긴 한데 전부 **alpha 0**으로 저작돼 있어서,
    // 팝업을 겹쳐 열면 뒤 팝업이 그대로 비쳐 화면이 뒤엉켜 보였다(대장간 위에 강화창 등).
    // 배경 노드 이름이 팝업마다 제각각(EquipPopupBackground / ForgePopupBackground / Img_EnhacePopup_01 …)
    // 이라 이름으로 찾지 않고 **팝업 루트의 직계 자식 중 화면을 덮는 첫 Image**를 배경으로 본다.
    protected static readonly Color DIM_BACKDROP = new Color32(0, 0, 0, 220);
    const float DIM_FADE = 0.3f;

    // 어둡게 깔면 안 되는 팝업(토스트·알림 등)은 이걸 false로 덮어쓴다.
    protected virtual bool UseDimBackdrop => true;

    void ApplyDimBackdrop()
    {
        if (!UseDimBackdrop) return;

        Image backdrop = FindBackdrop();
        if (backdrop == null) return;
        if (backdrop.color.a > 0.5f) return;      // 이미 저작된 값이 있으면 존중

        backdrop.color = new Color(DIM_BACKDROP.r, DIM_BACKDROP.g, DIM_BACKDROP.b, 0f);
        backdrop.DOKill();
        backdrop.DOFade(DIM_BACKDROP.a, DIM_FADE).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    Image FindBackdrop()
    {
        RectTransform self = transform as RectTransform;
        if (self == null) return null;

        foreach (Transform child in transform)
        {
            var rt = child as RectTransform;
            if (rt == null) continue;
            var img = child.GetComponent<Image>();
            if (img == null || img.sprite != null) continue;          // 그림이 있는 건 배경 layer가 아니다
            if (rt.rect.width < self.rect.width * 0.9f) continue;     // 화면을 덮는 크기만
            if (rt.rect.height < self.rect.height * 0.9f) continue;
            return img;
        }
        return null;
    }

    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

    // 안드로이드 뒤로가기 / PC ESC — 플레이어가 뒤로·X·바깥을 누른 것과 **똑같이** 처리한다.
    //  닫기 규칙(분배 롤백, 제작 중 잠금 등)은 팝업마다 그 버튼 핸들러에 이미 들어 있으므로 새로 만들지 않는다.
    //  닫기 수단이 없는 팝업(강화 연출·대사 진행 등)은 false — 뒤로가기를 무시한다.
    public virtual bool OnBackPressed()
    {
        UI_EventHandler best = null;
        int bestRank = int.MaxValue;
        foreach (var h in GetComponentsInChildren<UI_EventHandler>(false))
        {
            if (h.OnClickHandler == null) continue;
            int rank = BackRank(h.name);
            if (rank < bestRank) { best = h; bestRank = rank; }
        }
        if (best == null) return false;
        best.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        return true;
    }

    // 프리팹 노드 이름 규칙(자동 바인딩과 같은 이름)으로 고른다: X/나가기 > 바깥 배경.
    //  "…Back" 버튼은 고르지 않는다 — 여정창·물약 상세의 Back 은 최상위에서 아무것도 안 해서 창이 안 닫혔다.
    static int BackRank(string name)
    {
        string n = name.ToLowerInvariant();
        if (n.Contains("xbutton") || n.Contains("exit") || n.Contains("close") || n.Contains("cancel")) return 1;
        if (n.EndsWith("background")) return 2;
        return int.MaxValue;
    }

    public virtual void RefreshUI() { }

    public void OpenAnimation()
    {
        _isClosing = false;
        if (_frameRT == null) return;

        _frameRT.DOKill();
        _frameRT.anchoredPosition = new Vector2(0f, _openFromY);
        _frameRT.DOAnchorPosY(0f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
    }

    public void OnClickBG()
    {
        if (_isClosing) return;
        _isClosing = true;

        if (_frameRT == null) { ClosePopupUI(); return; }

        _frameRT.DOKill();
        _frameRT.DOAnchorPosY(_closeToY, 0.5f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => ClosePopupUI());
    }

    public void ClosePanelWithAnim(GameObject panel)
    {
        if (panel == null || !panel.activeSelf) return;

        DOTweenAnimation anim = panel.GetComponent<DOTweenAnimation>();
        if (anim == null)
        {
            panel.SetActive(false);
            return;
        }

        anim.DOPlayBackwards();
        StartCoroutine(WaitAndDeactivate(panel, anim.duration));
    }

    IEnumerator WaitAndDeactivate(GameObject go, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (go != null)
            go.SetActive(false);
        RefreshUI();
    }

}
