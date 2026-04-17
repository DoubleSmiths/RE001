using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

/// <summary>
/// 슬라이드 36: 제작의뢰 팝업에서 해금된 기술 클릭 시 호출되는 상세정보 팝업.
/// 진입 애니메이션(From y=-130 OutBack 0.5s)과 닫기 동작만 최소 구현.
/// 슬라이드 37~40 상세 내용은 후속 작업.
/// </summary>
public class UI_SkillDetailPopup : UI_Popup
{
    enum Images
    {
        SkillDetailPopupBackground,
    }

    enum GameObjects
    {
        SkillDetailPopup,
        SkillDetailPopupMain,
    }

    const float FRAME_FROM_Y = -130f;
    Vector2 _frameOrigin;

    public int SkillId { get; private set; } = -1;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        GetImage((int)Images.SkillDetailPopupBackground).gameObject.BindEvent(OnClickClose);

        return true;
    }

    public void SetInfo(int skillId = -1)
    {
        SkillId = skillId;

        var frameRT = GetObject((int)GameObjects.SkillDetailPopupMain).GetComponent<RectTransform>();
        _frameOrigin = frameRT.anchoredPosition;

        frameRT.DOKill();
        frameRT.anchoredPosition = _frameOrigin + new Vector2(0f, FRAME_FROM_Y);
        frameRT.DOAnchorPos(_frameOrigin, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        var bg = GetImage((int)Images.SkillDetailPopupBackground);
        if (bg != null)
        {
            bg.DOKill();
            Color c = bg.color; c.a = 0f; bg.color = c;
            bg.DOFade(220f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    void OnClickClose()
    {
        var frameRT = GetObject((int)GameObjects.SkillDetailPopupMain).GetComponent<RectTransform>();
        frameRT.DOKill();

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(frameRT.DOAnchorPos(_frameOrigin + new Vector2(0f, FRAME_FROM_Y), 0.3f).SetEase(Ease.InBack));
        seq.OnComplete(() => Managers.UI.ClosePopupUI());
    }
}
