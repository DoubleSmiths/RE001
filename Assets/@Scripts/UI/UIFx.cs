using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

/// <summary>
/// UI Canvas에서 동작하는 fake particle burst 시스템.
/// World-space ParticleSystem이 Screen Space Camera Canvas 위에 제대로 렌더되지 않는 구조적
/// 한계를 우회하기 위해 순수 RectTransform + Image + DOTween 조합으로 구현.
/// 호출 예: UIFx.PlayBurst(popupFrameRT, Vector2.zero, UIFx.FxType.Splash);
/// </summary>
public static class UIFx
{
    public enum FxType
    {
        /// <summary>S40 물약 상세창 중앙 splash: 흰색 원형 펀치 스케일 + 방사형 spark</summary>
        Splash,
        /// <summary>S40 물약 아이콘 위 upgrade sparkle: 바깥으로 퍼지는 작은 별빛들</summary>
        Sparkle,
        /// <summary>S42 HP 물약 사용 이펙트: 녹색 맥동 + 상승</summary>
        Heal,
    }

    /// <summary>
    /// UI 트리 위에 FX 버스트를 즉시 재생. 수명 종료 후 자동 Destroy.
    /// </summary>
    /// <param name="parent">FX가 붙을 UI Transform (Canvas 하위).</param>
    /// <param name="localPos">parent 기준 anchoredPosition.</param>
    public static GameObject PlayBurst(RectTransform parent, Vector2 localPos, FxType type)
    {
        if (parent == null) return null;

        var go = new GameObject($"UIFx_{type}", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localPos;
        rt.sizeDelta = Vector2.zero;
        rt.SetAsLastSibling(); // 형제 순서상 가장 뒤 → 최상단 렌더

        float lifetime;
        switch (type)
        {
            case FxType.Splash:  lifetime = SpawnSplash(rt); break;
            case FxType.Sparkle: lifetime = SpawnSparkle(rt); break;
            case FxType.Heal:    lifetime = SpawnHeal(rt); break;
            default:             lifetime = 0.5f; break;
        }

        UnityEngine.Object.Destroy(go, lifetime);
        return go;
    }

    // ────────────────────────────────────────
    //  프리셋: 각 타입별 burst 생성
    // ────────────────────────────────────────

    static float SpawnSplash(RectTransform root)
    {
        // 중앙 흰색 플래시: 0 → 1.5배 punch scale + fade out
        var flash = CreateImage(root, "Flash", Vector2.zero, new Vector2(70f, 70f), new Color(1f, 1f, 0.85f, 0.85f));
        flash.transform.localScale = Vector3.zero;
        flash.transform.DOScale(1.5f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        flash.DOFade(0f, 0.6f).SetEase(Ease.InQuad).SetUpdate(true);

        // 방사형 8방향 spark — 각 방향으로 이동 + 페이드
        int n = 8;
        float radius = 55f;
        for (int i = 0; i < n; i++)
        {
            float rad = (360f / n * i) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            var spark = CreateImage(root, $"Spark{i}", Vector2.zero, new Vector2(8f, 8f), new Color(1f, 0.95f, 0.6f, 1f));
            spark.rectTransform.DOAnchorPos(dir * radius, 0.55f).SetEase(Ease.OutQuad).SetUpdate(true);
            spark.rectTransform.DOScale(0.3f, 0.55f).SetEase(Ease.InQuad).SetUpdate(true);
            spark.DOFade(0f, 0.55f).SetEase(Ease.InQuad).SetUpdate(true);
        }

        return 0.7f;
    }

    static float SpawnSparkle(RectTransform root)
    {
        // 반짝이 6개 — 랜덤 각도/거리로 상승 + 페이드
        int n = 6;
        for (int i = 0; i < n; i++)
        {
            float angle = UnityEngine.Random.Range(-60f, 240f) * Mathf.Deg2Rad;
            float dist  = UnityEngine.Random.Range(20f, 40f);
            Vector2 target = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist + 10f);

            var star = CreateImage(root, $"Star{i}", Vector2.zero, new Vector2(6f, 6f), new Color(1f, 1f, 0.7f, 1f));
            star.transform.localScale = Vector3.zero;
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(star.transform.DOScale(1.6f, 0.15f).SetEase(Ease.OutBack));
            seq.Join(star.rectTransform.DOAnchorPos(target, 0.7f).SetEase(Ease.OutQuad));
            seq.Append(star.transform.DOScale(0f, 0.2f).SetEase(Ease.InQuad));
            seq.Join(star.DOFade(0f, 0.2f));
        }

        // 중앙 밝은 링 flash (잠깐)
        var ring = CreateImage(root, "RingFlash", Vector2.zero, new Vector2(30f, 30f), new Color(1f, 1f, 1f, 0.9f));
        ring.transform.localScale = Vector3.one * 0.3f;
        ring.transform.DOScale(1.8f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        ring.DOFade(0f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);

        return 0.9f;
    }

    static float SpawnHeal(RectTransform root)
    {
        // 녹색 플러스(+) 모양: 가로/세로 bar 2개
        var hBar = CreateImage(root, "HealH", Vector2.zero, new Vector2(36f, 10f), new Color(0.4f, 1f, 0.5f, 0.95f));
        var vBar = CreateImage(root, "HealV", Vector2.zero, new Vector2(10f, 36f), new Color(0.4f, 1f, 0.5f, 0.95f));

        foreach (var bar in new[] { hBar, vBar })
        {
            bar.transform.localScale = Vector3.zero;
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(bar.transform.DOScale(1.2f, 0.18f).SetEase(Ease.OutBack));
            seq.Append(bar.transform.DOScale(0.3f, 0.35f).SetEase(Ease.InQuad));
            seq.Join(bar.rectTransform.DOAnchorPosY(24f, 0.5f).SetEase(Ease.OutCubic));
            seq.Join(bar.DOFade(0f, 0.4f).SetDelay(0.1f));
        }

        // 글로우 원형
        var glow = CreateImage(root, "HealGlow", Vector2.zero, new Vector2(50f, 50f), new Color(0.5f, 1f, 0.6f, 0.7f));
        glow.transform.localScale = Vector3.one * 0.3f;
        glow.transform.DOScale(1.4f, 0.35f).SetEase(Ease.OutQuad).SetUpdate(true);
        glow.DOFade(0f, 0.5f).SetEase(Ease.InQuad).SetUpdate(true);

        return 0.7f;
    }

    // ────────────────────────────────────────
    //  UIParticle wrapper: ParticleSystem 프리팹을 UI 위에 렌더
    // ────────────────────────────────────────

    /// <summary>
    /// Addressable에 등록된 ParticleSystem prefab을 UI 위에 스폰 (Coffee.UIExtensions.UIParticle 사용).
    /// prefabKey: 예) "FX_splash_spark_floor", "FX_splash_spark_air"
    /// </summary>
    public static GameObject PlayParticle(RectTransform parent, Vector2 localPos, string prefabKey, float lifetime = 2f)
    {
        if (parent == null || string.IsNullOrEmpty(prefabKey)) return null;

        // 1) UIParticle 래퍼 GameObject 생성
        var host = new GameObject($"UIParticle_{prefabKey}", typeof(RectTransform));
        host.transform.SetParent(parent, false);
        var hostRT = host.GetComponent<RectTransform>();
        hostRT.anchorMin = hostRT.anchorMax = new Vector2(0.5f, 0.5f);
        hostRT.pivot = new Vector2(0.5f, 0.5f);
        hostRT.anchoredPosition = localPos;
        hostRT.sizeDelta = Vector2.zero;
        hostRT.SetAsLastSibling();

        var uip = host.AddComponent<UIParticle>();

        // 2) 실제 파티클 프리팹 인스턴스화 — parent는 SetParticleSystemInstance가 잡아줌
        //    (미리 host에 SetParent하면 함수가 "기존 child = 제거 대상"으로 간주해 Destroy함)
        var ps = Managers.Resource.Instantiate(prefabKey);
        if (ps == null)
        {
            Debug.LogWarning($"[UIFx] prefab '{prefabKey}' 로드 실패");
            UnityEngine.Object.Destroy(host);
            return null;
        }

        // 3) UIParticle에 연결 (내부에서 host 하위로 reparent + RefreshParticles 호출)
        uip.SetParticleSystemInstance(ps);
        ps.transform.localScale = Vector3.one;

        // 4) 명시적 Play + Emit (prefab의 rateOverTime=0, burst 미설정 방어)
        foreach (var p in ps.GetComponentsInChildren<ParticleSystem>(true))
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Clear(true);
            p.Play(true);
            p.Emit(30);
        }

        UnityEngine.Object.Destroy(host, lifetime);
        return host;
    }

    // ────────────────────────────────────────
    //  Util
    // ────────────────────────────────────────

    static Image CreateImage(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }
}
