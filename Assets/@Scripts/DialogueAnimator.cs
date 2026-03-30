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
    //  Box
    // ────────────────────────────────────────

    public void OpenBox(RectTransform box, Action onComplete = null)
    {
        box.gameObject.SetActive(true);

        Vector2 origin = box.anchoredPosition;
        box.anchoredPosition = origin + new Vector2(0f, -_boxSlideOffset);

        CanvasGroup cg = GetOrAddCanvasGroup(box.gameObject);
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(box.DOAnchorPos(origin, _boxOpenDuration).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(1f, _boxOpenDuration * 0.8f));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void CloseBox(RectTransform box, Action onComplete = null)
    {
        Vector2 target = box.anchoredPosition + new Vector2(0f, -_boxSlideOffset);
        CanvasGroup cg = GetOrAddCanvasGroup(box.gameObject);

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(box.DOAnchorPos(target, _boxCloseDuration).SetEase(Ease.InCubic));
        seq.Join(cg.DOFade(0f, _boxCloseDuration));
        seq.OnComplete(() =>
        {
            box.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
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
