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
    /// 대화 오프닝 시퀀스 (기획 수정안 2026-07-05, 클릭 즉시 두 페이드 병행):
    ///  · Dialogue Box(dia_box2) + Char Img(portrait): 0.3s 페이드 인 (OutQuad)
    ///  · Background(Gradient01 + Sprite None): 1.0s 페이드 인 (OutQuad)
    ///  · Box/Char 페이드가 끝나는 0.3s 시점에 이름 표시 + Text Show Start 콜백 (배경 페이드와 병행)
    /// </summary>
    public void PlayOpenSequence(
        Image gradient,
        Image panel,
        Image box,
        GameObject portrait,
        GameObject nameText,
        Action onTextShowStart)
    {
        const float BOX_FADE_DURATION = 0.3f;
        const float BG_FADE_DURATION  = 1.0f;

        // 초기 상태 (알파 0, name 비활성 / portrait는 알파 0으로 켜두고 페이드)
        SetImageAlpha(gradient, 0f);
        SetImageAlpha(panel, 0f);
        SetImageAlpha(box, 0f);
        gradient.gameObject.SetActive(true);
        panel.gameObject.SetActive(true);
        box.gameObject.SetActive(true);
        if (nameText != null) nameText.SetActive(false);

        Image portraitImg = portrait != null ? portrait.GetComponent<Image>() : null;
        if (portrait != null)
        {
            portrait.SetActive(true);
            SetImageAlpha(portraitImg, 0f);
        }

        float panelTargetAlpha = 220f / 255f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // Background: 1.0s 페이드 (시퀀스 기준 트랙)
        seq.Append(gradient.DOFade(1f, BG_FADE_DURATION).SetEase(Ease.OutQuad).SetUpdate(true));
        seq.Join(panel.DOFade(panelTargetAlpha, BG_FADE_DURATION).SetEase(Ease.OutQuad).SetUpdate(true));

        // Dialogue Box + Char Img: 클릭 즉시 0.3s 페이드 (배경과 병행)
        seq.Join(box.DOFade(1f, BOX_FADE_DURATION).SetEase(Ease.OutQuad).SetUpdate(true));
        if (portraitImg != null)
            seq.Join(portraitImg.DOFade(1f, BOX_FADE_DURATION).SetEase(Ease.OutQuad).SetUpdate(true));

        // 0.3s(Box/Char 페이드 완료) 시점: 이름 ON + Text Show Start
        seq.InsertCallback(BOX_FADE_DURATION, () =>
        {
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
        const float duration = 0.5f;
        const float minAlpha = 100f / 255f;
        const float maxAlpha = 1f;

        while (arrow != null)
        {
            //점멸
            yield return FadeArrowAlpha(arrow, maxAlpha, minAlpha, duration);
            yield return FadeArrowAlpha(arrow, minAlpha, maxAlpha, duration);

            //대기
            yield return new WaitForSeconds(0.5f);
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

    // DialogueText의 프리팹 저작 위치 (Top/Bottom 비대칭 오프셋 보존용).
    // localPosition=0으로 리셋하면 프리팹에서 조정한 위치가 매 타이핑마다 지워지므로 최초 위치를 기억해 복원한다.
    Vector2? _textOriginPos;

    void PlayEmotionEffect(TMP_Text text, DialogueEmotion emotion)
    {
        RectTransform rt = text.rectTransform;
        rt.DOKill();
        if (_textOriginPos == null) _textOriginPos = rt.anchoredPosition;
        rt.anchoredPosition = _textOriginPos.Value;

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
                rt.DOAnchorPos(_textOriginPos.Value + new Vector2(0f, -6f), 0.3f)
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
