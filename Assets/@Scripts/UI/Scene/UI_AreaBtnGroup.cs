using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG;
using static Define;
using System.Net.Http.Headers;

public abstract class UI_AreaBtnGroup : UI_Base
{
    protected UI_GameScene _gameScene;

    public void Setup(UI_GameScene gameScene)
    {
        _gameScene = gameScene;
        Init();
    }


    public void UnlockButtonAnim(GameObject go, Image image, TMP_Text text)
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

        //go.transform.DOShakePosition(2f, new Vector3(0.7f, 0.7f, 1f), 27, 40.1f, false, false).SetEase(Ease.OutQuad);
        //go.transform.DOScale(1.15f, 2f).SetEase(Ease.Linear);
        //image.DOFade(1f, 2f).SetEase(Ease.InQuint);
        //text.DOFade(1f, 2f).From(0.3f).SetEase(Ease.Linear);
        //go.transform.DOScale(1f, 0.1f).SetDelay(2.01f).SetEase(Ease.InOutBack);

    }
}
