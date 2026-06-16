using System.Collections;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [미니몬 관심] Side UI 알림 (기획서 v3 슬23~25). 사냥 성공 시 몬스터가 관심을 보이면
// 화면 우측에서 슬라이드인 → 3초 유지 → 슬라이드아웃(DoPlayBackwards 대응)되는 비모달 알림.
// 전투 중 표시되므로 게임을 멈추지 않는다(ShowPopupUI 스택/timeScale 미사용, Managers.UI.ShowAttractedPopup로 직접 생성).
//  - 슬24: 창고 여유 있음 → prefab 기본(파랑/핑크) + "{Name}(이)가 관심을 보입니다!"
//  - 슬25: 창고 가득   → 레이어 전부 빨강(B70000) + "{Name}... 창고가 가득 찼습니다!"
//  - Anim: Move FROM x+150, OutExpo 0.5s → 3s 대기 → 역슬라이드 후 파괴
public class UI_AttactedPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        AttactedPopupImage1,
        AttactedPopupImage2,
        AttactedPopupMask,
        AttactedPopupGlow,
        AttactedPopupImage3,
    }

    enum Texts
    {
        AttactedPopupText,
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        AttactedPopup,
    }

    #endregion

    static readonly Color32 FULL_RED = new Color32(0xB7, 0x00, 0x00, 255); // 슬25

    RectTransform _slideRT;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        _slideRT = GetObject((int)GameObjects.AttactedPopup).GetComponent<RectTransform>();
        return true;
    }

    // storageFull=false: 창고 여유(슬24) / true: 창고 가득(슬25)
    public void SetInfo(int dataId, bool storageFull)
    {
        CreatureData data = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out data);
        string cname = data != null && !string.IsNullOrEmpty(data.Name) ? data.Name : "미니몬";

        var txt = GetText((int)Texts.AttactedPopupText);
        if (txt != null)
        {
            // 슬24/25: {CreatureData.Name} + MinimonInterestSuccessText/FullText (로컬라이즈)
            txt.text = cname + Util.Localize(storageFull
                ? LocalizeKey.MinimonInterestFullText
                : LocalizeKey.MinimonInterestSuccessText);
            ApplyFont(txt, FontType.UI);
        }

        // 슬25: 창고 가득이면 레이어를 전부 빨강으로 (Glow는 기존 알파 유지)
        if (storageFull)
        {
            SetColor(Images.AttactedPopupImage1, FULL_RED);
            SetColor(Images.AttactedPopupImage2, FULL_RED);
            SetColor(Images.AttactedPopupImage3, FULL_RED);
            SetRGBKeepAlpha(Images.AttactedPopupGlow, FULL_RED);
        }

        StartCoroutine(CoSlide());
    }

    IEnumerator CoSlide()
    {
        if (_slideRT == null) yield break;

        // 슬24: 우측 경계에 안착(전체 가시). anchor/pivot=(1,0.5) 우측 기준 → x=0이면 팝업 우측 끝이 화면 우측에 flush.
        //  (Instantiate 시 anchoredPosition.x가 캔버스 스케일로 변형되므로 프리펩 x를 신뢰하지 않고 0으로 고정)
        Vector2 rest = new Vector2(0f, _slideRT.anchoredPosition.y);
        Vector2 from = rest + new Vector2(150f, 0f); // 화면 밖 우측에서 진입

        _slideRT.anchoredPosition = from;
        _slideRT.DOKill();
        _slideRT.DOAnchorPos(rest, 0.5f).SetEase(Ease.OutExpo).SetUpdate(true);

        // 등장 + 3초 유지
        yield return new WaitForSecondsRealtime(0.5f + 3f);

        // DoPlayBackwards 대응: 역슬라이드 후 파괴
        _slideRT.DOKill();
        _slideRT.DOAnchorPos(from, 0.5f).SetEase(Ease.InExpo).SetUpdate(true)
                .OnComplete(() => Managers.Resource.Destroy(gameObject));
    }

    void SetColor(Images id, Color c)
    {
        var img = GetImage((int)id);
        if (img != null) img.color = c;
    }

    // RGB만 교체하고 기존 알파 유지 (Glow처럼 반투명 레이어용)
    void SetRGBKeepAlpha(Images id, Color rgb)
    {
        var img = GetImage((int)id);
        if (img == null) return;
        float a = img.color.a;
        img.color = new Color(rgb.r, rgb.g, rgb.b, a);
    }
}
