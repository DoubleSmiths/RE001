using DG.Tweening;
using TMPro;
using UnityEngine;

// 슬라이드 63 — Damage Textpopup.
// 모드별 색상(Top/Btm gradient + Outline) + 생성 anim.
//   Normal:           Top FF0000 / Btm FFFF00 / Outline FFFFFF
//   Critical:         Top FF0000 / Btm FF00FF / Outline FFFFFF (+ critical_additive 글로우는 후속)
//   Miss:             Top 000000 / Btm E9E9E9 / Outline FFFFFF (Scale anim 없음)
//   CriticalCallout:  단색 FF0000 / Outline 000000  ("Critical!" / "치명타!")
//   Heal:             초록 (슬라이드 68 회복 텍스트 효과)
//
// Anim: Fade(0→1) + Scale(1.2→1, OutBack) Duration 0.3 → 1.5초 위로 이동 + FadeOut(InQuint).
//       Miss는 Scale 조정 없음.
public enum DamageFontMode
{
    Normal,
    Critical,
    Miss,
    CriticalCallout,
    Heal,
}

public class DamageFont : MonoBehaviour
{
    const float SPAWN_DURATION = 0.3f;
    const float RISE_DURATION  = 1.5f;
    const float RISE_HEIGHT    = 0.8f;

    static bool s_warnedCritSprite;   // 없는 스프라이트 경고는 1회만

    // 슬라이드 63 — 치명타 시 데미지 숫자 왼쪽 위에 dmg_txt_critical_additive 글로우 sprite 오버레이.
    // 위치는 텍스트 bounds 기반으로 동적 계산. 아래 offset은 좌상단 corner에서 추가 시각 보정.
    static readonly Vector3 CRIT_ADDITIVE_VISUAL_OFFSET = new Vector3(0f, 0f, 0f);

    TextMeshPro _text;
    SpriteRenderer _critAdditive;

    public void SetInfo(Vector2 pos, string display, DamageFontMode mode, Transform parent = null)
    {
        if (_text == null) _text = GetComponent<TextMeshPro>();

        // 풀에서 꺼낸 직후 잔상 정리
        DOTween.Kill(this);

        transform.position = pos;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        _text.text  = display;
        _text.color = Color.white;   // alpha multiplier (gradient 위에 곱함)
        _text.alpha = 0f;

        ApplyStyle(mode);
        DoAnimation(mode);
    }

    void ApplyStyle(DamageFontMode mode)
    {
        switch (mode)
        {
            case DamageFontMode.Normal:
                ApplyGradient("FF0000", "FFFF00");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                ShowCritAdditive(false);
                break;
            case DamageFontMode.Critical:
                ApplyGradient("FF0000", "FF00FF");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                ShowCritAdditive(true);
                break;
            case DamageFontMode.Miss:
                ApplyGradient("000000", "E9E9E9");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                ShowCritAdditive(false);
                break;
            case DamageFontMode.CriticalCallout:
                // 슬라이드 63 자료에 단색 명시 없음 → 빨강 단색 + 검정 외곽선 placeholder
                ApplyGradient("FF0000", "FF0000");
                Util.SetTMPOutline(_text, Util.HexToColor("000000"));
                ShowCritAdditive(false);
                break;
            case DamageFontMode.Heal:
                ApplyGradient("4EEE6F", "1A9600");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                ShowCritAdditive(false);
                break;
        }
    }

    void ShowCritAdditive(bool show)
    {
        if (!show)
        {
            if (_critAdditive != null) _critAdditive.gameObject.SetActive(false);
            return;
        }

        // lazy 생성 (첫 Critical 시점에 child SpriteRenderer 추가)
        if (_critAdditive == null)
        {
            var go = new GameObject("CritAdditive");
            go.transform.SetParent(transform, worldPositionStays: false);
            _critAdditive = go.AddComponent<SpriteRenderer>();
            // 텍스트 메시 위에 그리도록 sortingOrder 한 단계 위 
            int textOrder = GetComponent<MeshRenderer>()?.sortingOrder ?? 350;
            _critAdditive.sortingOrder = textOrder - 1;
            // 로드 줄이 주석 처리된 채 아래 경고만 남아 있었다 — 그래서 치명타가 뜰 때마다
            //  절대 꺼지지 않는 경고가 찍혔다(플레이스루 한 판에 221건). 실측해 보니
            //  주소는 멀쩡히 로드된다(접미사 유무 둘 다). 로드를 되살린다.
            _critAdditive.sprite = Managers.Resource.Load<Sprite>("dmg_txt_critical_additive");
            if (_critAdditive.sprite == null && !s_warnedCritSprite)
            {
                // 경고는 한 번만 — 타격마다 찍히면 진짜 에러가 파묻힌다.
                s_warnedCritSprite = true;
                Debug.LogWarning("[DamageFont] 'dmg_txt_critical_additive' sprite not found in Addressables.");
            }

            // 슬라이드 63 — 외곽선 FF9900. CriticalOutlineMat이 SSU Standard shader 기반 outer outline 적용.
            //Material critMat = Managers.Resource.Load<Material>("CriticalOutlineMat");
            //if (critMat != null)
            //    _critAdditive.material = critMat;
            //else
            //    Debug.LogWarning("[DamageFont] 'CriticalOutlineMat' material not found in Addressables.");
        }

        _critAdditive.gameObject.SetActive(true);
        PositionCritAdditive();
        // 텍스트와 동일하게 alpha 0에서 시작 → DoAnimation에서 페이드인
        Color c = _critAdditive.color; c.a = 0f; _critAdditive.color = c;
    }

    // 텍스트 길이/크기에 따라 좌상단 corner에 sprite 배치.
    // SetInfo에서 _text.text가 set된 직후, 매번 호출해야 textBounds가 갱신됨.
    void PositionCritAdditive()
    {
        if (_critAdditive == null || _text == null) return;

        _text.ForceMeshUpdate();
        Bounds b = _text.textBounds;

        Vector3 upperLeftLocal = new Vector3(
            b.center.x - b.extents.x,
            b.center.y + b.extents.y,
            0f);

        _critAdditive.transform.localPosition = upperLeftLocal + CRIT_ADDITIVE_VISUAL_OFFSET;
    }

    void ApplyGradient(string topHex, string btmHex)
    {
        Color top = Util.HexToColor(topHex);
        Color btm = Util.HexToColor(btmHex);
        _text.enableVertexGradient = true;
        _text.colorGradient = new VertexGradient(top, top, btm, btm);
    }

    void DoAnimation(DamageFontMode mode)
    {
        bool useScale = (mode != DamageFontMode.Miss);
        Vector3 startPos = transform.position;
        Vector3 endPos   = startPos + Vector3.up * RISE_HEIGHT;

        transform.localScale = useScale ? Vector3.one * 1.2f : Vector3.one;

        bool hasCritAdditive = (mode == DamageFontMode.Critical) && _critAdditive != null;

        Sequence seq = DOTween.Sequence().SetTarget(this);

        // 0.3s: Fade-in (0→1) + Scale(1.2→1, OutBack)
        seq.Append(_text.DOFade(1f, SPAWN_DURATION));
        if (useScale)
            seq.Join(transform.DOScale(1f, SPAWN_DURATION).SetEase(Ease.OutBack));
        if (hasCritAdditive)
            seq.Join(_critAdditive.DOFade(1f, SPAWN_DURATION));

        // 1.5s: 위로 이동 + FadeOut(InQuint)
        seq.Append(transform.DOMove(endPos, RISE_DURATION).SetEase(Ease.Linear));
        seq.Join(_text.DOFade(0f, RISE_DURATION).SetEase(Ease.InQuint));
        if (hasCritAdditive)
            seq.Join(_critAdditive.DOFade(0f, RISE_DURATION).SetEase(Ease.InQuint));

        seq.OnComplete(() => Managers.Resource.Destroy(gameObject));
    }
}
