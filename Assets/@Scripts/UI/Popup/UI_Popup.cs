using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Popup : UI_Base
{
    public bool _isClosing = false;
    public RectTransform _frameRT;

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

    public void OpenAnimation()
    {
        _isClosing = false;
        if (_frameRT == null) return;

        _frameRT.DOKill();
        _frameRT.anchoredPosition = new Vector2(0f, -120f);
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
        _frameRT.DOAnchorPosY(-120f, 0.5f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => ClosePopupUI());
    }

}
