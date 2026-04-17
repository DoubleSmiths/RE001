using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using Sequence = DG.Tweening.Sequence;

public class UI_CustomOrderPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        CustomOrderPopupBackground,
        Alch_Img_mask_1,
        Xbutton,
    }

    enum Texts
    {
        CustomOrderPopupTitle,
    }

    enum Buttons
    {
        CustomOrderPotionBtn,
        CustomOrderSkillBtn,
    }

    enum GameObjects
    {
        CustomOrderPopup,
        CustomOrderPopupFrame,
        TalkText,
        Btm_PotionBtnGroup,
        Btm_SkillBtnGroup,
    }

    #endregion

    DialogueAnimator _typing;
    TMP_Text _talkText;
    TMP_Text _talkEngText;

    // prefab asset 기준 고정 원위치 (런타임 Init 시점이 불안정해서 하드코딩)
    static readonly Vector2 FRAME_ORIGIN = new Vector2(0f, 0f);
    static readonly Vector2 ALCH_ORIGIN = new Vector2(70f, 1f);

    const float FRAME_FROM_Y = -120f;
    const float ALCH_SLIDE_OFFSET_X = -150f;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _typing = gameObject.GetOrAddComponent<DialogueAnimator>();

        // TalkText는 kortext/engtext 하위 구조. 한글/영어 둘 중 한쪽만 표시해야 하는데,
        // 현재 prefab은 둘 다 activeSelf=true에 텍스트가 들어있어 이중으로 보임.
        // Init에서 engtext를 비활성화하고, Korean 모드에선 kortext만 사용.
        var talkTextGO = GetObject((int)GameObjects.TalkText);
        var engTf = talkTextGO.transform.Find("engtext");
        if (engTf != null)
        {
            _talkEngText = engTf.GetComponent<TMP_Text>();
            engTf.gameObject.SetActive(false);
        }
        var korTf = talkTextGO.transform.Find("kortext");
        if (korTf != null)
        {
            _talkText = korTf.GetComponent<TMP_Text>();
            // kortext_1(중복 자식)도 존재하면 숨김 — parent kortext가 실제 표시 담당
            var korDup = korTf.Find("kortext_1");
            if (korDup != null) korDup.gameObject.SetActive(false);

            // prefab의 ContentSizeFitter + DOTweenAnimation이 위치/크기를 조작해서 위치가 밀림.
            // DestroyImmediate로 즉시 제거해야 이후 set이 덮어쓰여지지 않음.
            var csf = korTf.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (csf != null) DestroyImmediate(csf);
            var dta = korTf.GetComponent<DG.Tweening.DOTweenAnimation>();
            if (dta != null) DestroyImmediate(dta);
        }

        GetImage((int)Images.CustomOrderPopupBackground).gameObject.BindEvent(OnClickClose);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickClose);
        GetButton((int)Buttons.CustomOrderPotionBtn).gameObject.BindEvent(OnClickPotion);
        GetButton((int)Buttons.CustomOrderSkillBtn).gameObject.BindEvent(OnClickSkill);

        return true;
    }

    public void SetInfo()
    {
        // 슬라이드 35 확장 그룹은 기본 상태에서 숨김 (물약/기술 버튼 클릭 시 슬라이드 35에서 표시)
        GetObject((int)GameObjects.Btm_PotionBtnGroup).SetActive(false);
        GetObject((int)GameObjects.Btm_SkillBtnGroup).SetActive(false);

        var frameRT = GetObject((int)GameObjects.CustomOrderPopupFrame).GetComponent<RectTransform>();
        var alchRT = GetImage((int)Images.Alch_Img_mask_1).rectTransform;

        // 기존 트윈 정리
        frameRT.DOKill();
        alchRT.DOKill();

        // 팝업 진입: From y = -120, Duration 0.5, OutBack
        frameRT.anchoredPosition = FRAME_ORIGIN + new Vector2(0f, FRAME_FROM_Y);
        frameRT.DOAnchorPos(FRAME_ORIGIN, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        // 캐릭터 슬라이드 인: mask 안쪽으로 왼쪽에서 오른쪽으로. OutBack 0.4s
        // Alch_Img_mask_1이 mask의 자식이므로 mask 바깥으로 나간 부분은 자동 클리핑됨.
        alchRT.anchoredPosition = ALCH_ORIGIN + new Vector2(ALCH_SLIDE_OFFSET_X, 0f);
        alchRT.DOAnchorPos(ALCH_ORIGIN, 0.4f).SetDelay(0.2f).SetEase(Ease.OutBack).SetUpdate(true);

        // TalkText 타이핑: 캐릭터가 거의 자리 잡은 뒤 시작
        if (_talkText != null)
        {
            _talkText.text = "";
            StartCoroutine(CoTalkTextPos());
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (_talkText == null) return;
                _typing.PlayTyping(_talkText, "\"무엇을 만들어드릴까요?\"", DialogueEmotion.Normal);
            }).SetUpdate(true);
        }

        // BlackBG fade (slide 8: Duration 0.3, OutQuad, target alpha 220/255)
        var bg = GetImage((int)Images.CustomOrderPopupBackground);
        if (bg != null)
        {
            bg.DOKill();
            Color c = bg.color; c.a = 0f; bg.color = c;
            bg.DOFade(220f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    System.Collections.IEnumerator CoTalkTextPos()
    {
        if (_talkText == null) yield break;
        var rt = _talkText.rectTransform;
        // prefab 잔여 오토 레이아웃(DOTween/TMP)이 pos/size를 덮어쓰므로 매 프레임 강제 설정.
        Vector2 targetPos = new Vector2(22f, -75f);
        Vector2 targetSize = new Vector2(65f, 40f);
        while (this != null && _talkText != null)
        {
            if (rt.anchoredPosition != targetPos) rt.anchoredPosition = targetPos;
            if (rt.sizeDelta != targetSize) rt.sizeDelta = targetSize;
            yield return null;
        }
    }

    void OnClickClose()
    {
        var frameRT = GetObject((int)GameObjects.CustomOrderPopupFrame).GetComponent<RectTransform>();
        var alchRT = GetImage((int)Images.Alch_Img_mask_1).rectTransform;

        frameRT.DOKill();
        alchRT.DOKill();

        // Rewind: 진입과 반대로 y -120으로 내려가며 사라짐
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(frameRT.DOAnchorPos(FRAME_ORIGIN + new Vector2(0f, FRAME_FROM_Y), 0.3f).SetEase(Ease.InBack));
        seq.Join(alchRT.DOAnchorPos(ALCH_ORIGIN + new Vector2(ALCH_SLIDE_OFFSET_X, 0f), 0.25f).SetEase(Ease.InBack));
        seq.OnComplete(() => Managers.UI.ClosePopupUI());
    }

    void OnClickPotion()
    {
        // TODO: 슬라이드 35 — 물약 리스트 확장
        Debug.Log("[CustomOrder] Potion clicked (slide 35 expansion TODO)");
    }

    void OnClickSkill()
    {
        // TODO: 슬라이드 35 — 기술 리스트 확장
        Debug.Log("[CustomOrder] Skill clicked (slide 35 expansion TODO)");
    }
}
