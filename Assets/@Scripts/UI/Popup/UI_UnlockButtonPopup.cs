using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    string    _iconNameId;
    string    _iconDescId;

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

        // 아이콘 뒤 별 모양 광선(GlowRays 2장)은 끈다. 프리팹에서 지우지 않고 여기서 숨기는 이유는
        //  Images.Image4/Image5 바인딩이 이름으로 걸려 있어, 노드를 지우면 바인딩이 깨지기 때문.
        var shine = GetObject((int)GameObjects.shineeffect);
        if (shine != null) shine.SetActive(false);

        // 언어 변경 시 폰트 갱신이 본문 재배치보다 먼저 실행되도록 한 번만 등록한다.
        ApplyFont(GetText((int)Texts.IconNameText), Define.FontType.UI);
        ApplyFont(GetText((int)Texts.TextTMP), Define.FontType.UI);
        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        StopTypewriter();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // 해금 안내를 읽는 중이면 새 언어로 타이핑하고, 이미 읽은 안내는 즉시 표시한다.
    void RefreshTexts()
    {
        if (string.IsNullOrEmpty(_iconNameId) || string.IsNullOrEmpty(_iconDescId)) return;
        RefreshLocalizedContent(_isAnimating);
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
        SetInfo(Managers.Resource.Load<Sprite>(iconSprite), iconNameId, iconDescId);
    }

    public void SetInfo(Sprite iconSprite, string iconNameId, string iconDescId)
    {
        // 마을 버튼에 이미 로드된 실제 스프라이트를 받아 서브스프라이트 주소 추측을 피한다.
        var icon = GetImage((int)Images.Icon);
        icon.sprite = iconSprite;
        icon.enabled = iconSprite != null;
        icon.preserveAspect = true;
        _iconNameId = iconNameId;
        _iconDescId = iconDescId;
        RefreshLocalizedContent(true);
    }

    void RefreshLocalizedContent(bool animate)
    {
        StopTypewriter();
        _charAlphas = null;
        GetText((int)Texts.IconNameText).text = Localize(_iconNameId);

        TMP_Text tmp = GetText((int)Texts.TextTMP);
        if (animate)
            _typewriterCoroutine = StartCoroutine(TypewriterFade(tmp, Localize(_iconDescId)));
        else
            SetBodyText(tmp, Localize(_iconDescId));
    }

    void SetBodyText(TMP_Text tmp, string text)
    {
        _animTMP = tmp;
        tmp.text = text;
        // 프리팹의 부모 폭과 세로 PreferredSize로 줄바꿈을 확정한 뒤 글자 알파를 다룬다.
        LayoutRebuilder.ForceRebuildLayoutImmediate(tmp.rectTransform);
        tmp.ForceMeshUpdate();
    }

    void StopTypewriter()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
        if (_animTMP != null) DOTween.Kill(_animTMP, false);
        _isAnimating = false;
    }

    // 글자 하나씩 알파 0→1로 페이드인
    IEnumerator TypewriterFade(TMP_Text tmp, string text, float charInterval = 0.05f, float fadeDuration = 0.35f)
    {
        SetBodyText(tmp, text);
        _isAnimating = true;

        int charCount = tmp.textInfo.characterCount;
        _charAlphas = new float[charCount]; // 기본값 0
        var charAlphas = _charAlphas; // 언어가 바뀌어도 이전 Tween이 새 문장의 배열을 읽지 않는다.

        for (int i = 0; i < charCount; i++)
        {
            int index = i;
            DOTween.To(() => charAlphas[index], a => charAlphas[index] = a, 1f, fadeDuration)
                   .SetTarget(tmp)        // SkipTypewriter에서 한꺼번에 Kill 가능하도록 target 지정
                   .SetLink(gameObject)
                   .SetUpdate(true);

            yield return new WaitForSecondsRealtime(charInterval);
        }

        // 마지막 글자의 페이드인 완료 대기
        yield return new WaitForSecondsRealtime(fadeDuration);

        DOTween.Kill(tmp, false);
        ShowAllCharacters();
        _isAnimating = false;
        _charAlphas = null;
        _typewriterCoroutine = null;
    }

    void SetCharAlpha(TMP_Text tmp, int charIndex, float alpha)
    {
        if (charIndex >= tmp.textInfo.characterCount) return;
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

    // 뒤로가기 = 화면을 누른 것과 같다(타이핑 중이면 넘기고, 끝났으면 닫는다). 닫기 버튼이 없어 공용 탐색에 안 걸린다.
    public override bool OnBackPressed()
    {
        OnClickContent();
        return true;
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
        // 진행 중인 글자별 DOTween.To 트윈 종료 (_charAlphas 참조 트윈들)
        StopTypewriter();
        ShowAllCharacters();
        _charAlphas = null;
    }

    void ShowAllCharacters()
    {
        if (_animTMP != null && _charAlphas != null)
        {
            for (int i = 0; i < _charAlphas.Length; i++)
            {
                _charAlphas[i] = 1f;
                SetCharAlpha(_animTMP, i, 1f);
            }
            _animTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
