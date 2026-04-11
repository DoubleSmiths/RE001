using DG;
using DG.Tweening;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;

public abstract class UI_AreaBtnGroup : UI_Base
{
    protected UI_GameScene _gameScene;

    public void Setup(UI_GameScene gameScene)
    {
        _gameScene = gameScene;
        Init();
    }

    /// <summary>구역 전환 후 FadeIn 완료 시점에 호출. 하위 클래스에서 진입 연출을 구현한다.</summary>
    public virtual void PlayEnterAnimation() { }

    public void UnlockButtonAnim(GameObject go, Image image, TMP_Text text, System.Action onComplete = null)
    {
        // 이벤트는 Image와 연결해야한다!

        image.gameObject.SetActive(true);
        text.gameObject.SetActive(true);

        image.color = new Color(1, 1, 1, 0);
        text.color = new Color(1, 1, 1, 0);

        // 파티클 효과 추가
        Destroy(Managers.Resource.Instantiate("Star_01_Anim_13_a", go.transform), 3.0f);

        Sequence seq = DOTween.Sequence();

        seq.Join(go.transform.DOShakePosition(2f, new Vector3(0.7f, 0.7f, 1f), 27, 40.1f, false, false).SetEase(Ease.OutQuad));
        seq.Join(go.transform.DOScale(1.15f, 2f).SetEase(Ease.Linear));
        seq.Join(image.DOFade(1f, 2f).SetEase(Ease.InQuint));
        seq.Join(text.DOFade(1f, 2f).From(0.3f).SetEase(Ease.Linear));
        seq.Join(go.transform.DOScale(1f, 0.1f).SetDelay(2.01f).SetEase(Ease.InOutBack));

        seq.OnComplete(() =>
        {
            go.GetComponent<Button>().interactable = true;
            onComplete?.Invoke();
        });
    }

    public void PlaceNameAnim(RectTransform rt)
    {
        rt.DOKill();

        Sequence seq = DOTween.Sequence();

        // FROM (-150, 12) → 현재 위치
        seq.Append(
            rt.DOAnchorPos(new Vector2(0f, 12f), 0.25f)
              .From(new Vector2(-150f, 12f))
              .SetEase(Ease.OutQuad)
        );

        // 대기
        seq.AppendInterval(0.6f);

        // TO (150, 12)
        seq.Append(
            rt.DOAnchorPos(new Vector2(150f, 12f), 0.15f)
              .SetEase(Ease.OutQuad));
    }

    public void ButtonMoveAnim(RectTransform rt, float fromX, float fromY, float toX, float toY, Ease ease = Ease.OutBack)
    {
        rt.gameObject.SetActive(true);

        rt.DOKill();

        rt.DOAnchorPos(new Vector2(toX, toY), 0.3f)
          .From(new Vector2(fromX, fromY))
          .SetEase(ease);
    }
}
