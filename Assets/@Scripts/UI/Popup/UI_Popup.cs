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

        return true;
    }

    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
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
        yield return new WaitForSeconds(delay);
        if (go != null)
            go.SetActive(false);
        RefreshUI();
    }

}
