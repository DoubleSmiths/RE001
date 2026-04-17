using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class DialogueAnimator : MonoBehaviour
{
    [Header("Typing")]
    [SerializeField] float _typingSpeed = 0.04f;

    [Header("Box")]
    [SerializeField] float _boxOpenDuration = 0.3f;
    [SerializeField] float _boxCloseDuration = 0.2f;
    [SerializeField] float _boxSlideOffset = 180f;

    [Header("Portrait")]
    [SerializeField] float _portraitSpeakScale = 1.05f;
    [SerializeField] Color _portraitActiveColor = Color.white;
    [SerializeField] Color _portraitIdleColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    Coroutine _typingCoroutine;
    Action _onTypingComplete;
    bool _isTypingDone;

    public bool IsTypingDone => _isTypingDone;

    // 글자별 알파 페이드인용
    TMP_Text _animTMP;
    float[]  _charAlphas;
    bool     _isAnimating;

    // TMP는 매 프레임 메시를 재갱신하므로 LateUpdate에서 알파를 강제 적용
    void LateUpdate()
    {
        if (!_isAnimating || _animTMP == null || _charAlphas == null) return;

        for (int i = 0; i < _charAlphas.Length; i++)
            SetCharAlpha(_animTMP, i, _charAlphas[i]);
        _animTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // ────────────────────────────────────────
    //  Dialogue Open Sequence (slide 31/32)
    // ────────────────────────────────────────

    /// <summary>
    /// 슬라이드 32 STEP 1~3 시퀀스 재생.
    /// STEP 1: Gradient01 + Sprite(None) 페이드 인 (0.5s, OutQuad)
    /// STEP 2: 0.5s 대기 후 dia_box2 페이드 인 (0.5s, OutQuad)
    /// STEP 3: 0.2s 대기 후 캐릭터 Sprite ON + 이름 + Text Show Start 콜백
    /// </summary>
    public void PlayOpenSequence(
        Image gradient,
        Image panel,
        Image box,
        GameObject portrait,
        GameObject nameText,
        Action onTextShowStart)
    {
        // 초기 상태 (알파 0, portrait/name 비활성)
        SetImageAlpha(gradient, 0f);
        SetImageAlpha(panel, 0f);
        SetImageAlpha(box, 0f);
        gradient.gameObject.SetActive(true);
        panel.gameObject.SetActive(true);
        box.gameObject.SetActive(true);
        if (portrait != null) portrait.SetActive(false);
        if (nameText != null) nameText.SetActive(false);

        float panelTargetAlpha = 220f / 255f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // STEP 1
        seq.Append(gradient.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
        seq.Join(panel.DOFade(panelTargetAlpha, 0.5f).SetEase(Ease.OutQuad));

        // STEP 2: 0.5초 뒤 dia_box2 페이드 인
        seq.Append(box.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));

        // STEP 3: 0.2초 뒤 캐릭터/이름 ON + Text Show Start
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() =>
        {
            if (portrait != null) portrait.SetActive(true);
            if (nameText != null) nameText.SetActive(true);
            onTextShowStart?.Invoke();
        });
    }

    Coroutine _arrowLoopCoroutine;

    /// <summary>
    /// Arrow_down 지속 페이드 루프 (alpha 1 ↔ 100/255, 0.2s, OutQuad, 무한 반복)
    /// </summary>
    public void PlayArrowLoop(Image arrow)
    {
        if (arrow == null)
        {
            Debug.LogWarning("[DialogueAnimator] PlayArrowLoop: arrow is null");
            return;
        }

        StopArrowLoop(arrow);

        arrow.gameObject.SetActive(true);
        SetImageAlpha(arrow, 1f);

        Debug.Log($"[DialogueAnimator] PlayArrowLoop start (active: {arrow.gameObject.activeInHierarchy}, color: {arrow.color})");

        _arrowLoopCoroutine = StartCoroutine(CoArrowLoop(arrow));
    }

    public void StopArrowLoop(Image arrow)
    {
        if (_arrowLoopCoroutine != null)
        {
            StopCoroutine(_arrowLoopCoroutine);
            _arrowLoopCoroutine = null;
        }
        if (arrow != null)
            arrow.DOKill();
    }

    IEnumerator CoArrowLoop(Image arrow)
    {
        const float duration = 0.2f;
        const float minAlpha = 100f / 255f;
        const float maxAlpha = 1f;

        while (arrow != null)
        {
            yield return FadeArrowAlpha(arrow, maxAlpha, minAlpha, duration);
            yield return FadeArrowAlpha(arrow, minAlpha, maxAlpha, duration);
        }
    }

    IEnumerator FadeArrowAlpha(Image arrow, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            // OutQuad easing
            p = 1f - (1f - p) * (1f - p);
            SetImageAlpha(arrow, Mathf.Lerp(from, to, p));
            yield return null;
        }
        SetImageAlpha(arrow, to);
    }

    static void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    // ────────────────────────────────────────
    //  Typing
    // ────────────────────────────────────────

    public void PlayTyping(TMP_Text text, string content, DialogueEmotion emotion, Action onComplete = null)
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _onTypingComplete = onComplete;
        _isTypingDone = false;

        PlayEmotionEffect(text, emotion);

        _typingCoroutine = StartCoroutine(CoTyping(text, content));
    }

    public void SkipTyping(TMP_Text text)
    {
        if (_isTypingDone) return;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        // 모든 글자 알파를 즉시 1로 직접 적용
        if (_animTMP != null && _charAlphas != null)
        {
            for (int i = 0; i < _charAlphas.Length; i++)
            {
                _charAlphas[i] = 1f;
                SetCharAlpha(_animTMP, i, 1f);
            }
            _animTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        _isAnimating = false;
        _isTypingDone = true;
        _onTypingComplete?.Invoke();
    }

    IEnumerator CoTyping(TMP_Text text, string content)
    {
        _animTMP = text;
        _isAnimating = true;

        text.text = content;
        text.ForceMeshUpdate();

        int charCount = text.textInfo.characterCount;
        _charAlphas = new float[charCount]; // 기본값 0

        for (int i = 0; i < charCount; i++)
        {
            int index = i;
            DOTween.To(() => _charAlphas[index], a => _charAlphas[index] = a, 1f, _typingSpeed * 2f)
                   .SetUpdate(true);

            yield return new WaitForSecondsRealtime(_typingSpeed);
        }

        // 마지막 글자 페이드인 완료 대기
        yield return new WaitForSecondsRealtime(_typingSpeed * 2f);

        _isAnimating = false;
        _isTypingDone = true;
        _onTypingComplete?.Invoke();
    }

    // ────────────────────────────────────────
    //  Portrait
    // ────────────────────────────────────────

    public void EnterPortrait(Image portrait)
    {
        portrait.gameObject.SetActive(true);
        portrait.color = _portraitIdleColor;

        Vector2 origin = portrait.rectTransform.anchoredPosition;
        portrait.rectTransform.anchoredPosition = origin + new Vector2(-60f, 0f);

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(portrait.rectTransform.DOAnchorPos(origin, 0.3f).SetEase(Ease.OutCubic));
        seq.Join(portrait.DOColor(_portraitActiveColor, 0.3f));
    }

    public void SetPortraitSpeaking(Image portrait, bool isSpeaking)
    {
        portrait.DOKill();
        portrait.rectTransform.DOKill();

        Color targetColor = isSpeaking ? _portraitActiveColor : _portraitIdleColor;
        float targetScale = isSpeaking ? _portraitSpeakScale : 1f;

        portrait.DOColor(targetColor, 0.2f).SetUpdate(true);
        portrait.rectTransform.DOScale(targetScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // ────────────────────────────────────────
    //  Emotion
    // ────────────────────────────────────────

    void PlayEmotionEffect(TMP_Text text, DialogueEmotion emotion)
    {
        RectTransform rt = text.rectTransform;
        rt.DOKill();
        rt.localPosition = Vector3.zero;

        switch (emotion)
        {
            case DialogueEmotion.Angry:
                rt.DOShakeAnchorPos(0.4f, strength: 10f, vibrato: 30, randomness: 45)
                  .SetUpdate(true);
                break;

            case DialogueEmotion.Scared:
                rt.DOShakeAnchorPos(0.6f, strength: 5f, vibrato: 20, randomness: 90)
                  .SetUpdate(true);
                break;

            case DialogueEmotion.Happy:
                rt.DOPunchScale(Vector3.one * 0.08f, 0.4f, vibrato: 5)
                  .SetUpdate(true);
                break;

            case DialogueEmotion.Sad:
                rt.DOAnchorPos(new Vector2(0f, -6f), 0.3f)
                  .SetEase(Ease.OutSine)
                  .SetUpdate(true);
                break;

            case DialogueEmotion.Normal:
            default:
                break;
        }
    }

    // ────────────────────────────────────────
    //  Util
    // ────────────────────────────────────────

    void SetCharAlpha(TMP_Text tmp, int charIndex, float alpha)
    {
        var charInfo = tmp.textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int meshIndex   = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Color32[] colors = tmp.textInfo.meshInfo[meshIndex].colors32;

        byte a = (byte)(alpha * 255);
        colors[vertexIndex + 0].a = a;
        colors[vertexIndex + 1].a = a;
        colors[vertexIndex + 2].a = a;
        colors[vertexIndex + 3].a = a;
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
