using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using static Util;

public class UI_UnlockButtonPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        BlockImg,
        Content,
        Image4,
        Image5,
        TravelUnlockTutoIcon,
        Icon,
        TextBoxDialogueNextBtnImg,
    }

    enum Texts
    {
        IconNameText,
        TextTMP,
    }

    enum Buttons
    {
        TravelUnlockTutoCloseBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        icon,
        shineeffect,
    }

    #endregion

    Coroutine _typewriterCoroutine;
    TMP_Text  _animTMP;
    float[]   _charAlphas;
    bool      _isAnimating;

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        #endregion

        GetImage((int)Images.Content).gameObject.BindEvent(OnClickContent);

        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // enum Texts: IconNameText / TextTMP 모두 동적 (해금 버튼별 set) — 폰트만 적용.
    void RefreshTexts()
    {
        var icon = GetText((int)Texts.IconNameText);
        if (icon != null) ApplyFont(icon, Define.FontType.UI);
        var tmp = GetText((int)Texts.TextTMP);
        //if (tmp != null) ApplyFont(tmp, Define.FontType.UI);
    }

    // TMP는 매 프레임 메시를 재갱신하므로 LateUpdate에서 알파를 강제 적용
    void LateUpdate()
    {
        if (!_isAnimating || _animTMP == null || _charAlphas == null) return;

        for (int i = 0; i < _charAlphas.Length; i++)
            SetCharAlpha(_animTMP, i, _charAlphas[i]);
        _animTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // 어떤 버튼이 해금되는지에 따라 정보를 세팅하는 함수
    public void SetInfo(string iconSprite, string iconNameId, string iconDescId)
    {
        GetImage((int)Images.Icon).sprite = Managers.Resource.Load<Sprite>(iconSprite);
        GetText((int)Texts.IconNameText).text = Localize(iconNameId);

        TMP_Text tmp = GetText((int)Texts.TextTMP);
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterFade(tmp, Localize(iconDescId)));
    }

    // 글자 하나씩 알파 0→1로 페이드인
    IEnumerator TypewriterFade(TMP_Text tmp, string text, float charInterval = 0.05f, float fadeDuration = 0.35f)
    {
        _animTMP = tmp;
        _isAnimating = true;

        tmp.text = text;
        tmp.ForceMeshUpdate();

        int charCount = tmp.textInfo.characterCount;
        _charAlphas = new float[charCount]; // 기본값 0

        for (int i = 0; i < charCount; i++)
        {
            int index = i;
            DOTween.To(() => _charAlphas[index], a => _charAlphas[index] = a, 1f, fadeDuration)
                   .SetTarget(tmp)        // SkipTypewriter에서 한꺼번에 Kill 가능하도록 target 지정
                   .SetUpdate(true);

            yield return new WaitForSecondsRealtime(charInterval);
        }

        // 마지막 글자의 페이드인 완료 대기
        yield return new WaitForSecondsRealtime(fadeDuration);

        _isAnimating = false;
        _charAlphas = null;
        _typewriterCoroutine = null;
    }

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

    public void OnClickContent()
    {
        // DialoguePopup 패턴: 타이핑 진행 중에는 모든 글자 즉시 표시(스킵), 완료 상태에서만 팝업 닫기.
        if (_isAnimating)
        {
            SkipTypewriter();
            return;
        }

        ClosePopupUI();
    }

    /// <summary>타이핑 코루틴/트윈을 멈추고 모든 글자 알파를 1로 즉시 적용한다.</summary>
    void SkipTypewriter()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        // 진행 중인 글자별 DOTween.To 트윈 종료 (_charAlphas 참조 트윈들)
        DOTween.Kill(_animTMP);

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
        _charAlphas = null;
    }
}
