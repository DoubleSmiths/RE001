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

        // 연출 도중 모든 UI 입력을 봉쇄.
        // (1) EventSystem 자체를 비활성화 → Button.onClick / BindEvent / 드래그 등 UGUI 이벤트 전부 차단
        // (2) 전체화면 투명 블로커도 함께 깔아 시각적/구조적 의도를 명확히 함
        var es = UnityEngine.EventSystems.EventSystem.current;
        bool prevEsEnabled = es != null && es.enabled;
        if (es != null) es.enabled = false;

        GameObject blocker = CreateInputBlocker();

        // 기획 수정안(2026-07-05, 슬라이드 GIF 기준):
        //  · 빛줄기 버스트(Star_01_Anim_13_a)는 기존처럼 페이드와 동시에 시작
        //  · Text는 Icon과 같은 방식으로 페이드 (From 0, InQuint)
        //  · Fade In이 끝난 뒤 터지는 글로우 플래시(GlowDot_01) 재생 → 플래시가 끝난 후 onComplete(해금 팝업)
        const float FADE_DURATION  = 2f;
        const float FLASH_DURATION = 1f;   // GlowDot_01 파티클 버스트 시각 길이 (기획 튜닝값)

        // 빛줄기 버스트(Star_01_Anim_13_a)는 **뺐다** — 해금 연출 가운데 뜨는 별 모양을 없애 달라는 요청.
        //  흔들림 + 확대 + 페이드인은 그대로 두고, 마무리 글로우 플래시(GlowDot_01)로 순간을 잡는다.
        //  (주의: 월드 ParticleSystem은 SSC 캔버스 위에 직접 렌더되지 않아 UIFx.PlayParticle 경유가 필요했다)

        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject);

        seq.Join(go.transform.DOShakePosition(FADE_DURATION, new Vector3(0.7f, 0.7f, 1f), 27, 40.1f, false, false).SetEase(Ease.OutQuad));
        seq.Join(go.transform.DOScale(1.15f, FADE_DURATION).SetEase(Ease.Linear));
        seq.Join(image.DOFade(1f, FADE_DURATION).SetEase(Ease.InQuint));
        seq.Join(text.DOFade(1f, FADE_DURATION).From(0f).SetEase(Ease.InQuint));
        seq.Join(go.transform.DOScale(1f, 0.1f).SetDelay(FADE_DURATION + 0.01f).SetEase(Ease.InOutBack));

        // Fade In 종료 → 터지는 글로우 플래시 재생, 끝날 때까지 대기 후 완료(팝업)
        seq.AppendCallback(() =>
            UIFx.PlayParticle((RectTransform)go.transform, Vector2.zero, "GlowDot_01", FLASH_DURATION + 0.5f));
        seq.AppendInterval(FLASH_DURATION);

        // 정상 완료와 그룹 소멸 양쪽에서 입력을 복원하되, 후속 연출의 입력 잠금은 건드리지 않는다.
        bool inputReleased = false;
        System.Action releaseInput = () =>
        {
            if (inputReleased) return;
            inputReleased = true;
            if (blocker != null) Managers.Resource.Destroy(blocker);
            if (es != null) es.enabled = prevEsEnabled;
        };
        seq.OnKill(() => releaseInput());
        seq.OnComplete(() =>
        {
            releaseInput();
            go.GetComponent<Button>().interactable = true;
            onComplete?.Invoke();
        });
    }

    /// <summary>전체화면을 덮는 투명 GraphicRaycaster를 만들어 화면 전체 입력을 일시 차단.
    /// EventSystem 비활성화와 함께 사용. 두 줄 방어선.</summary>
    static GameObject CreateInputBlocker()
    {
        // Canvas는 별도 GameObject에 두어 SSO 자동 사이즈를 신뢰성 있게 받도록 한다.
        var canvasGo = new GameObject("UnlockInputBlocker",
            typeof(Canvas), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(Managers.UI.Root.transform, false);

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9999;     // 어떤 팝업/씬 UI보다도 위

        // 자식으로 Image를 깔아 Canvas RectTransform(=화면 전체)에 stretch
        var imgGo = new GameObject("Block", typeof(RectTransform), typeof(Image));
        imgGo.transform.SetParent(canvasGo.transform, false);

        var rt = (RectTransform)imgGo.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = imgGo.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);  // 완전 투명
        img.raycastTarget = true;               // 클릭만 흡수
        return canvasGo;
    }

    public void PlaceNameAnim(RectTransform rt)
    {
        rt.DOKill();
        rt.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence().SetTarget(rt).SetLink(gameObject);

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

        // 빠져나간 자리에 그대로 켜 둔 채였다. 마스킹이 없어 폰 프레임 **바깥에** 장소명이
        //  계속 떠 있었다 — 나갔으면 꺼야 한다. 다음 재생은 위에서 다시 켠다.
        seq.OnComplete(() => rt.gameObject.SetActive(false));
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
