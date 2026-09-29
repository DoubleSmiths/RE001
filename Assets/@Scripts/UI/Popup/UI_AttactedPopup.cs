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
        // CreatureData.Name은 LocalizationData 키 → 화면 표시 시 한/영 변환
        string cname = data != null && !string.IsNullOrEmpty(data.Name) ? Util.Localize(data.Name) : Util.Localize(LocalizeKey.MinimonDefaultNameText);

        var txt = GetText((int)Texts.AttactedPopupText);
        if (txt != null)
        {
            // 슬24/25: {CreatureData.Name} + MinimonInterestSuccessText/FullText (로컬라이즈)
            //  ApplyJosa — 이름 받침에 따라 "이(가)"를 골라준다("여우이(가)" → "여우가")
            txt.text = Util.ApplyJosa(cname + Util.Localize(storageFull
                ? LocalizeKey.MinimonInterestFullText
                : LocalizeKey.MinimonInterestSuccessText));
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

    // [DOTweenAnimation 규약 예외 — 의도적 코드 대체] 슬24/25 슬라이드인/아웃.
    //  prefab(Attacted Popup.prefab)엔 Move(isFrom) DOTweenAnimation + 구버전 Visual Scripting 그래프
    //  (OnEnable→Timer→DOPlayBackwards, 영어 리터럴 "got amused" 등 List/MonsterNum 변수 기반)가 얹혀 있는데,
    //  이 VS 그래프가 신 로컬라이즈 텍스트 시스템과 비호환이고, Instantiate 시 anchoredPosition.x가 캔버스
    //  스케일로 왜곡돼 authored 절대 Move가 어긋난다. → 코드로 슬라이드/타이밍/파괴를 일괄 제어한다.
    IEnumerator CoSlide()
    {
        if (_slideRT == null) yield break;

        // 슬24: 우측 끝에 안착 (프리펩 authored 위치 27,45). anchor/pivot=(1,0.5) 우측 기준 →
        //  x=27이면 팝업 우측 끝(둥근 프레임)이 화면 우측 경계에 딱 붙는다.
        //  (Instantiate 시 anchoredPosition.x가 캔버스 스케일로 변형되므로 x는 프리펩 의도값 27로 고정, y는 유지)
        Vector2 rest = new Vector2(27f, _slideRT.anchoredPosition.y);
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
