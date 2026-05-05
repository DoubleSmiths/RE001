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

    TextMeshPro _text;

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
                break;
            case DamageFontMode.Critical:
                ApplyGradient("FF0000", "FF00FF");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                // TODO: dmg_txt_critical_additive 글로우(Outline FF9900) 후속
                break;
            case DamageFontMode.Miss:
                ApplyGradient("000000", "E9E9E9");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                break;
            case DamageFontMode.CriticalCallout:
                // 슬라이드 63 자료에 단색 명시 없음 → 빨강 단색 + 검정 외곽선 placeholder
                ApplyGradient("FF0000", "FF0000");
                Util.SetTMPOutline(_text, Util.HexToColor("000000"));
                break;
            case DamageFontMode.Heal:
                ApplyGradient("4EEE6F", "1A9600");
                Util.SetTMPOutline(_text, Util.HexToColor("FFFFFF"));
                break;
        }
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

        Sequence seq = DOTween.Sequence().SetTarget(this);

        // 0.3s: Fade-in (0→1) + Scale(1.2→1, OutBack)
        seq.Append(_text.DOFade(1f, SPAWN_DURATION));
        if (useScale)
            seq.Join(transform.DOScale(1f, SPAWN_DURATION).SetEase(Ease.OutBack));

        // 1.5s: 위로 이동 + FadeOut(InQuint)
        seq.Append(transform.DOMove(endPos, RISE_DURATION).SetEase(Ease.Linear));
        seq.Join(_text.DOFade(0f, RISE_DURATION).SetEase(Ease.InQuint));

        seq.OnComplete(() => Managers.Resource.Destroy(gameObject));
    }
}
