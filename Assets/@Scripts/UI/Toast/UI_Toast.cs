using DG.Tweening;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class UI_Toast : UI_Base
{

    #region Enum

    enum Images
    {
        StageTitle,
        StageTitle_1,
        StageTitle_2,
    }

    enum Texts
    {
        StageTitle_Txt,
    }

    public void OnEnable()
    {
        // Tm 오픈 연출
        //OpenAnimation();
        //PopupOpenAnimation(gameObject);

        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.AppendCallback(() => gameObject.GetComponent<DOTweenAnimation>()?.DORestart());
        sequence.AppendInterval(1.1f);
        sequence.AppendCallback(() => Fade());

    }

    #endregion

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        #endregion

        Refresh();
        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // enum Texts: StageTitle_Txt — 동적 (지역명, SetInfo에서 set), 폰트만.
    void RefreshTexts()
    {
        var tmp = GetText((int)Texts.StageTitle_Txt);
        if (tmp != null) ApplyFont(tmp, Define.FontType.UI);
    }

    public void SetInfo(string msg)
    {
        // 메시지 변경
        transform.localScale = Vector3.one;
        var tmp = GetText((int)Texts.StageTitle_Txt);
        // 긴 알림(던전 결과 '골드 던전 끝! 28마리 · 골드 2,450' 등)이 화면 폭을 넘어 잘렸다 — 칸에 맞춰 줄인다(최대 = 원래 크기)
        if (!tmp.enableAutoSizing)
        {
            tmp.fontSizeMax = tmp.fontSize;
            tmp.fontSizeMin = tmp.fontSize * TOAST_MIN_FONT_RATIO;
            tmp.enableAutoSizing = true;
            // 글자 칸의 ContentSizeFitter(가로 = 선호 크기)가 칸을 글자만큼 늘려 자동 크기가 한 번도 줄이지 못했다 — 띠 폭에 묶는다(넘치면 두 줄)
            var fitter = tmp.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;
        }
        tmp.text = msg;
        Refresh();
    }

    const float TOAST_MIN_FONT_RATIO = 0.5f;

    void Refresh()
    {

    }

    void Fade()
    {
        // 토스트가 먼저 치워져도 트윈이 사라진 그림을 건드리지 않게 이 오브젝트에 묶는다
        GetImage((int)Images.StageTitle).DOFade(0, 1.5f).SetLink(gameObject);
        GetImage((int)Images.StageTitle_1).DOFade(0, 1.5f).SetLink(gameObject);
        GetImage((int)Images.StageTitle_2).DOFade(0, 1.5f).SetLink(gameObject);
        GetText((int)Texts.StageTitle_Txt).DOFade(0, 1.5f).SetLink(gameObject);
    }

}

