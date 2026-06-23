using System.Collections;
using Data;
using DG.Tweening;
using UnityEngine;
using static Define;

// [포획 창고 가득] 경고 popup (기획서 v3 슬23). 미니몬이 관심을 보였으나 포획 가능 창고가
// 꽉 차 저장되지 않을 때 표시되는 비모달 경고. 전투 중 표시되므로 게임을 멈추지 않는다.
//  - Anim: 스케일 등장(OutBack) + 페이드인 → 2초 유지 → 페이드아웃 후 파괴 (자동 소멸, 버튼 없음)
//  - 표시: Managers.UI.ShowWarningPopup(dataId)
public class UI_WarningPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        TameSlotMaxWarningPopupImage1,
        TameSlotMaxWarningPopupImage2,
        TameSlotMaxWarningPopupImage3,
    }

    enum Texts
    {
        TameSlotMaxWarningPopupText,
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        WarningPopup,
        TameSlotMaxWarningPopup,
    }

    #endregion

    RectTransform _frame;

    void Awake() => Init();

    override public bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        var content = GetObject((int)GameObjects.TameSlotMaxWarningPopup)
                   ?? GetObject((int)GameObjects.WarningPopup);
        if (content != null) _frame = content.GetComponent<RectTransform>();

        return true;
    }

    public void SetInfo(int dataId)
    {
        CreatureData data = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out data);
        string cname = data != null && !string.IsNullOrEmpty(data.Name) ? Util.Localize(data.Name) : "미니몬";

        var txt = GetText((int)Texts.TameSlotMaxWarningPopupText);
        if (txt != null)
        {
            // 슬25: CreatureData.Name + MinimonInterestFullText (로컬라이즈)
            txt.text = cname + Util.Localize(LocalizeKey.MinimonInterestFullText);
            ApplyFont(txt, FontType.UI);
        }

        StartCoroutine(CoShow());
    }

    // 슬25: 슬24와 같은 Side UI(빨강) → AttactedPopup과 동일하게 우측에서 슬라이드인 → 3초 → 슬라이드아웃.
    //  rest=(27,45): 바 우측 끝이 화면 우측 경계에 flush. (Instantiate 시 anchoredPosition.x 변형되므로 27 고정, y 유지)
    IEnumerator CoShow()
    {
        if (_frame == null) yield break;

        _frame.localScale = Vector3.one;
        Vector2 rest = new Vector2(27f, _frame.anchoredPosition.y);
        Vector2 from = rest + new Vector2(150f, 0f);

        _frame.anchoredPosition = from;
        _frame.DOKill();
        _frame.DOAnchorPos(rest, 0.5f).SetEase(Ease.OutExpo).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.5f + 3f);

        _frame.DOKill();
        _frame.DOAnchorPos(from, 0.5f).SetEase(Ease.InExpo).SetUpdate(true)
              .OnComplete(() => Managers.Resource.Destroy(gameObject));
    }
}
