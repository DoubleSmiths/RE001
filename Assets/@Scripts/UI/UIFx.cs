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
        /// <summary>대장간 망치 타격: 흰 구체 + 흩어지는 연기 + 청록 불티.
        /// 원본 영상(3:38~3:41)의 타격 순간을 옮긴 것 — 구체가 먼저 터지고,
        /// 연기가 옆으로 퍼지며, 청록 불티가 사방으로 튄다.</summary>
        ForgeSmash,
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
            case FxType.ForgeSmash: lifetime = SpawnForgeSmash(rt); break;
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

    /// <summary>**프리팹에 이미 놓여 있는** ParticleSystem을 그 자리에서 캔버스 위에 그리게 한다.
    ///
    /// 원작의 강화 연출은 빛줄기·불티·타격이 전부 진짜 ParticleSystem이다. 우리 프리팹에도
    ///  같은 노드가 이식돼 있는데(God_ray_02, Btn Embers, GlowDot_01_1) **한 번도 화면에 나온 적이 없다** —
    ///  월드 파티클은 Screen Space Camera 캔버스 위에 렌더되지 않기 때문이다.
    ///  그래서 여태 Image로 흉내를 냈고, 그게 원작과 다르다는 지적을 받았다.
    ///
    /// ⚠️ UIParticle은 **자기 GameObject의 localScale을 제멋대로 덮어쓴다**(스케일 20배 사고 전례).
    ///    그래서 파티클에 직접 붙이지 않고 전용 호스트를 만들어 그 아래로 넣는다.
    /// 반환: 호스트(없으면 null). 같은 파티클을 두 번 감싸지 않는다.</summary>
    public static UIParticle WrapInPlace(GameObject psGo)
    {
        if (psGo == null) return null;

        var ps = psGo.GetComponent<ParticleSystem>();
        if (ps == null) return null;

        var parent = psGo.transform.parent as RectTransform;
        if (parent == null) return null;

        // 이미 감싸 뒀으면 그대로 쓴다
        var existing = psGo.GetComponentInParent<UIParticle>();
        if (existing != null) return existing;

        var host = new GameObject($"UIParticle_{psGo.name}", typeof(RectTransform));
        host.transform.SetParent(parent, false);

        var hostRT = host.GetComponent<RectTransform>();
        var srcRT = psGo.transform as RectTransform;
        hostRT.anchorMin = hostRT.anchorMax = new Vector2(0.5f, 0.5f);
        hostRT.pivot = new Vector2(0.5f, 0.5f);
        hostRT.sizeDelta = Vector2.zero;
        // 파티클이 놓여 있던 자리를 그대로 물려받는다(RectTransform이 아니면 localPosition).
        hostRT.anchoredPosition = srcRT != null ? srcRT.anchoredPosition
                                                : (Vector2)psGo.transform.localPosition;
        hostRT.SetSiblingIndex(psGo.transform.GetSiblingIndex());

        var uip = host.AddComponent<UIParticle>();
        uip.SetParticleSystemInstance(psGo);      // 내부에서 host 아래로 옮기고 RefreshParticles
        psGo.transform.localScale = Vector3.one;
        return uip;
    }

    /// <summary>감싼 파티클을 처음부터 다시 튼다.</summary>
    public static void Replay(UIParticle uip, int emit = 0)
    {
        if (uip == null) return;
        uip.gameObject.SetActive(true);

        foreach (var p in uip.GetComponentsInChildren<ParticleSystem>(true))
        {
            p.gameObject.SetActive(true);
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Clear(true);
            p.Play(true);
            if (emit > 0) p.Emit(emit);
        }
    }

    // ────────────────────────────────────────
    //  Util
    // ────────────────────────────────────────

    /// <summary>대장간 타격 — 흰 구체 / 연기 / 청록 불티 세 겹.
    /// 영상에서 읽은 순서 그대로다: 구체가 가장 빠르고(0.18s), 연기가 옆으로 눕고(0.5s),
    /// 불티가 가장 오래 남는다(0.6s). 겹치는 시간이 곧 타격감이다.</summary>
    // 원본 영상의 타격 한 번은 세 겹이다(vid_hammer 004~012 프레임):
    //   ① 모루 위에 흰 구체가 번쩍 → ② 가로로 길게 퍼지는 연기 + 흰 충격파 줄무늬
    //   → ③ 청록 반짝임이 사방으로 튀며 가장 늦게까지 남는다.
    // ⚠️ 스프라이트를 안 준 Image는 **흰 사각형**이다. 예전 버전이 그래서
    //    구체가 네모난 판때기로, 연기가 회색 벽돌로 보였다. 둥근/별 텍스처를 만들어 쓴다.
    /// <summary>대장간 타격. 원작 프레임에서 잰 대로 다시 그렸다.
    ///
    /// 원작은 파티클 다섯 겹(center_light_circle / sharp_circle / shining_center / wind / twinkle)인데
    ///  월드 파티클이라 우리 캔버스에 얹히지 않는다. 그래서 같은 **크기·색·지속시간**으로 그린다.
    ///  실측: 흰 원반 지름 90 ref-px = 47유닛이 두 프레임(0.15~0.2초) 포화 상태로 번쩍이고,
    ///        그 뒤 어두운 회색(0.67) 연기가 0.6초 안에 사라지며, 속 빈 링이 한 번 퍼진다.
    ///  예전 것은 원반이 우리 화면에서 12유닛밖에 안 됐고(원작의 1/4), 연기가 밝은 흰 구름이라
    ///  무대가 하얗게 떠 버렸다.</summary>
    static float SpawnForgeSmash(RectTransform root)
    {
        Sprite dot = Dot(), star = Star(), disc = Disc(), ring = Ring();

        // ① 흰 원반 — 지름 47유닛, 가장자리가 또렷하다. 커지지 않고 **처음부터 최대**로 난다.
        var orb = CreateImage(root, "SmashOrb", Vector2.zero, new Vector2(ORB_SIZE, ORB_SIZE),
                              Color.white, disc);
        orb.transform.localScale = Vector3.one * 0.9f;
        orb.transform.DOScale(1.05f, ORB_LIFE).SetEase(Ease.OutQuad).SetUpdate(true);
        DOTween.Sequence().SetUpdate(true).SetLink(orb.gameObject)
               .AppendInterval(ORB_HOLD)
               .Append(orb.DOFade(0f, ORB_LIFE - ORB_HOLD).SetEase(Ease.InQuad));

        // ②-a 속 빈 링 — 원작 Ring_01. 원반이 꺼질 무렵 한 번 퍼진다.
        var rg = CreateImage(root, "SmashRing", Vector2.zero, new Vector2(ORB_SIZE, ORB_SIZE),
                             new Color(1f, 1f, 1f, 0.85f), ring);
        rg.transform.localScale = Vector3.one * 0.5f;
        DOTween.Sequence().SetUpdate(true).SetLink(rg.gameObject)
               .AppendInterval(RING_DELAY)
               .Append(rg.transform.DOScale(1.5f, RING_LIFE).SetEase(Ease.OutQuad))
               .Join(rg.DOFade(0f, RING_LIFE).SetEase(Ease.OutQuad));

        // ②-b 연기 — **어두운 회색**이다. 원작 wind startColor (0.6698,0.6698,0.6698),
        //      알파가 1에서 0.196으로 빠르게 죽어 검은 무대 위에서 그을음처럼 읽힌다.
        // 원작의 연기는 팝업 폭을 거의 다 덮는다(실측 반경 약 70유닛). 작게 뭉치면 얼룩으로 읽힌다.
        for (int i = 0; i < 11; i++)
        {
            float dir = i % 2 == 0 ? 1f : -1f;
            float spread = 12f + (i / 2) * 13f;
            // 원작의 연기는 가로 띠가 아니라 **울퉁불퉁한 덩어리**다 — 위로도 크게 부풀고
            //  덩어리마다 크기와 높이가 제각각이라 사이사이 구멍이 보인다.
            float lift = UnityEngine.Random.Range(-4f, 34f) * (1f - spread / 90f) + i * 0.8f;
            float w = UnityEngine.Random.Range(22f, 38f);
            var puff = CreateImage(root, $"SmashSmoke{i}", new Vector2(dir * 4f, 0f),
                                   new Vector2(w, w * UnityEngine.Random.Range(0.6f, 1.0f)),
                                   new Color(SMOKE_V, SMOKE_V, SMOKE_V, 1f), dot);
            puff.rectTransform.DOAnchorPos(new Vector2(dir * spread, lift), SMOKE_LIFE)
                .SetEase(Ease.OutQuad).SetUpdate(true);
            puff.rectTransform.DOScale(UnityEngine.Random.Range(1.7f, 2.8f), SMOKE_LIFE)
                .SetEase(Ease.OutQuad).SetUpdate(true);
            DOTween.Sequence().SetUpdate(true).SetLink(puff.gameObject)
                   .Append(puff.DOFade(0.196f, SMOKE_LIFE * 0.35f).SetEase(Ease.OutQuad))
                   .Append(puff.DOFade(0f, SMOKE_LIFE * 0.65f).SetEase(Ease.Linear));
        }

        // ②-c 충격파 줄무늬 — 연기를 가로지르는 흰 선. 이게 있어야 "때렸다"로 읽힌다.
        for (int i = 0; i < 4; i++)
        {
            float dir = i % 2 == 0 ? 1f : -1f;
            float y = (i / 2 == 0 ? 3f : -3f);
            var line = CreateImage(root, $"SmashLine{i}", new Vector2(0f, y),
                                   new Vector2(26f, 2.4f), new Color(1f, 1f, 1f, 0.95f), dot);
            line.rectTransform.DOAnchorPos(new Vector2(dir * 40f, y), 0.28f)
                .SetEase(Ease.OutQuad).SetUpdate(true);
            line.rectTransform.DOSizeDelta(new Vector2(54f, 1.2f), 0.28f).SetUpdate(true);
            line.DOFade(0f, 0.32f).SetEase(Ease.InQuad).SetUpdate(true);
        }

        // ③ 청록 반짝임(원작 twinkle) — 옆으로 퍼지며 **위로 뜬다**
        const int N = 8;
        for (int i = 0; i < N; i++)
        {
            float rad = (360f / N * i + 14f) * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad) * 0.65f);
            var sp = CreateImage(root, $"SmashSpark{i}", Vector2.zero,
                                 new Vector2(10f, 10f), new Color(0.62f, 1f, 0.94f, 1f), star);
            sp.rectTransform.DOAnchorPos(dir * (34f + (i % 3) * 9f) + new Vector2(0f, 8f), 0.6f)
              .SetEase(Ease.OutQuad).SetUpdate(true);
            sp.rectTransform.DOScale(0.35f, 0.6f).SetEase(Ease.InQuad).SetUpdate(true);
            sp.DOFade(0f, 0.6f).SetEase(Ease.InQuad).SetUpdate(true);
        }

        return 0.7f;
    }

    const float ORB_SIZE   = 47f;    // 원작 실측 90 ref-px
    const float ORB_HOLD   = 0.10f;  // 포화 상태로 버티는 시간 (두 프레임)
    const float ORB_LIFE   = 0.22f;
    const float RING_DELAY = 0.10f;
    const float RING_LIFE  = 0.30f;
    const float SMOKE_V    = 0.6698f;   // 원작 wind startColor
    const float SMOKE_LIFE = 0.70f;

    // ────────────────────────────────────────
    //  불티 — 대장간 연출 내내 흩날린다
    // ────────────────────────────────────────

    /// <summary>주황 불티 한 점. **아래에서 위로 오른다.**
    ///
    /// ⚠️ 예전 것은 떨어졌다. 원작(Btn Embers/GlowDot_02)은 화로에서 올라오는 불티라
    ///    위로(약간 왼쪽으로) 오른다 — 사용자가 "불꽃이 아래에서 위로 올라오는데 그렇지 않다"고
    ///    지적한 것이 이것이다.
    /// 실측: 방출 원반 반경 86유닛, 속도 위 32.5유닛/s · 좌 9.7유닛/s, 수명 0.5~1.0초,
    ///       크기 1.0~2.5유닛, 색 주황(1,0.34,0) 과 옅은 노랑이 반반, 등속.</summary>
    public static void SpawnEmber(RectTransform host, float halfRadius = 86f)
    {
        if (host == null) return;

        float ang = UnityEngine.Random.value * Mathf.PI * 2f;
        float rad = halfRadius * Mathf.Sqrt(UnityEngine.Random.value);   // 원 안 균등 분포
        var pos = new Vector2(Mathf.Cos(ang) * rad, EMBER_ORIGIN_Y + Mathf.Sin(ang) * rad * 0.85f);

        float sz = UnityEngine.Random.Range(1.0f, 2.5f);
        bool light = UnityEngine.Random.value < 0.5f;
        var col = light ? new Color(1f, UnityEngine.Random.Range(0.41f, 0.87f), 0f, 0.35f)
                        : new Color(1f, 0.34f, 0f, 1f);

        var img = CreateImage(host, "Ember", pos, new Vector2(sz, sz), col, Dot());
        img.rectTransform.localRotation = Quaternion.identity;   // 원작 startRotation = 0

        float life = UnityEngine.Random.Range(0.5f, 1.0f);
        var rise = pos + new Vector2(-UnityEngine.Random.Range(6f, 14f) * life,
                                      UnityEngine.Random.Range(27f, 41f) * life);

        img.color = new Color(col.r, col.g, col.b, 0f);
        var seq = DOTween.Sequence().SetUpdate(true).SetLink(img.gameObject);
        seq.Join(img.rectTransform.DOAnchorPos(rise, life).SetEase(Ease.Linear));    // 원작은 등속
        seq.Insert(0f, img.DOFade(col.a, life * 0.2f).SetEase(Ease.Linear));
        seq.Insert(life * 0.8f, img.DOFade(0f, life * 0.2f).SetEase(Ease.Linear));
        seq.OnComplete(() => { if (img != null) UnityEngine.Object.Destroy(img.gameObject); });
    }

    const float EMBER_ORIGIN_Y = -86.5f;   // 원작 Btn Embers 의 자리(무대 바닥)

    // ────────────────────────────────────────
    //  절차적 스프라이트 — 에셋을 늘리지 않고 모양을 만든다
    //   (Image에 스프라이트를 안 주면 흰 사각형이 그려진다)
    // ────────────────────────────────────────
    static Sprite s_dot, s_star, s_disc, s_ring;

    /// <summary>가장자리가 부드러운 흰 원. 구체·연기·선 전부 이걸 늘려 쓴다.</summary>
    static Sprite Dot()
    {
        if (s_dot != null) return s_dot;
        s_dot = Build("UIFx_Dot", (nx, ny) =>
        {
            float d = Mathf.Sqrt(nx * nx + ny * ny);
            return Mathf.Clamp01(1f - d);
        });
        return s_dot;
    }

    /// <summary>네 갈래 반짝임(✦). 축에 붙을수록 밝고 대각선은 파인 별 모양.</summary>
    static Sprite Star()
    {
        if (s_star != null) return s_star;
        s_star = Build("UIFx_Star", (nx, ny) =>
        {
            // astroid: |x|^(2/3)+|y|^(2/3)=1 보다 더 뾰족하게 지수를 낮춘다
            float v = 1f - (Mathf.Pow(Mathf.Abs(nx), 0.5f) + Mathf.Pow(Mathf.Abs(ny), 0.5f));
            return Mathf.Clamp01(v * 1.6f);
        });
        return s_star;
    }

    /// <summary>가장자리가 또렷한 흰 원반. 타격의 흰 구체용 — Dot은 너무 물러서
    /// 포화 화소가 거의 안 생긴다(원작 대비 1/4 크기로 측정되던 원인).</summary>
    static Sprite Disc()
    {
        if (s_disc != null) return s_disc;
        s_disc = Build("UIFx_Disc", (nx, ny) =>
        {
            float d = Mathf.Sqrt(nx * nx + ny * ny);
            return Mathf.Clamp01((0.97f - d) / 0.12f);   // 0.85까지 알파 1, 그 밖은 짧게 감쇠
        });
        return s_disc;
    }

    /// <summary>속이 빈 흰 링(원작 Ring_01).</summary>
    static Sprite Ring()
    {
        if (s_ring != null) return s_ring;
        s_ring = Build("UIFx_Ring", (nx, ny) =>
        {
            float d = Mathf.Sqrt(nx * nx + ny * ny);
            return Mathf.Clamp01(1f - Mathf.Abs(d - 0.78f) / 0.13f);
        });
        return s_ring;
    }

    /// <summary>중심 기준 -1..1 좌표로 알파를 그려 64px 스프라이트를 굽는다.</summary>
    static Sprite Build(string name, Func<float, float, float> alpha)
    {
        const int S = 64;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,   // 씬 전환에도 살아남되 저장은 안 한다
        };

        var px = new Color32[S * S];
        float half = S * 0.5f;
        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float nx = (x + 0.5f - half) / half;
                float ny = (y + 0.5f - half) / half;
                float a = Mathf.Clamp01(alpha(nx, ny));
                a = a * a * (3f - 2f * a);           // smoothstep — 계단지지 않게
                px[y * S + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply(false, false);

        var sp = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        sp.name = name;
        sp.hideFlags = HideFlags.HideAndDontSave;
        return sp;
    }

    static Image CreateImage(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color,
                             Sprite sprite = null)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;         // null이면 유니티 기본 흰 사각형
        img.color = color;
        img.raycastTarget = false;
        return img;
    }
}
