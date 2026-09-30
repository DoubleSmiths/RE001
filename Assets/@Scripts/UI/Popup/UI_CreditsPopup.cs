using UnityEngine;
using static Define;

// 크레딧·오픈소스/제3자 고지. 본문은 Data/Credits.txt(어드레서블 "Credits") — 프리팹을 건드리지 않고 고칠 수 있게.
public class UI_CreditsPopup : UI_Popup
{
    enum Texts
    {
        CreditsTitleText,
        CreditsBodyText,
    }

    enum Buttons
    {
        CreditsPopupBackground,
    }

    enum GameObjects
    {
        CreditsPopup,
        CreditsScroll,
    }

    private void Awake() { Init(); }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.CreditsPopup).GetComponent<RectTransform>();
        _openFromY = 350f;
        _closeToY = 350f;

        GetButton((int)Buttons.CreditsPopupBackground).gameObject.BindEvent(() => OnClickBG());

        var title = GetText((int)Texts.CreditsTitleText);
        ApplyFont(title, FontType.UI);
        title.text = Util.Localize("CreditsText");

        // 고지문은 원문 그대로(라이선스 문구는 번역하지 않는다). 본문 폰트는 프리팹에서 한글·영문이
        //  모두 있는 폰트로 고정해 두었으므로 언어 전환 폰트(ApplyFont)를 적용하지 않는다.
        var body = GetText((int)Texts.CreditsBodyText);
        var credits = Managers.Resource.Load<TextAsset>("Credits");
        body.text = credits != null ? credits.text.Replace("{version}", Application.version) : "";

        Canvas.ForceUpdateCanvases();
        GetObject((int)GameObjects.CreditsScroll).GetComponent<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1f;

        OpenAnimation();
        return true;
    }

    // 마지막 임무를 마쳤을 때 — 제목을 감사 인사로
    const float TITLE_MAX_WIDTH = 140f;   // 액자(160) 안쪽 폭

    // 크레딧 창을 글 보기로 — 설정의 [플레이 기록](ADR0046)
    public void ShowText(string titleText, string bodyText)
    {
        GetText((int)Texts.CreditsTitleText).text = titleText;
        GetText((int)Texts.CreditsBodyText).text = bodyText;
        Canvas.ForceUpdateCanvases();
        GetObject((int)GameObjects.CreditsScroll).GetComponent<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1f;
    }

    public void ShowAsEnding()
    {
        var title = GetText((int)Texts.CreditsTitleText);
        title.text = Util.Localize("ThanksForPlayingText");
        // 감사 인사는 '크레딧'보다 훨씬 길어 제목 띠 양쪽으로 넘쳤다 — 폭에 맞춰 글자를 줄인다.
        float w = title.GetPreferredValues(title.text).x;
        if (w > TITLE_MAX_WIDTH) title.fontSize *= TITLE_MAX_WIDTH / w;
    }
}
