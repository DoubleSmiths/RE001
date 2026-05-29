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
        string cname = data != null && !string.IsNullOrEmpty(data.Name) ? data.Name : "미니몬";

        var txt = GetText((int)Texts.TameSlotMaxWarningPopupText);
        if (txt != null)
        {
            // 슬23/25: 로컬라이즈 키(MinimonInterestFullText) 미존재 → 한글 리터럴 (TODO: 데이터화)
            txt.text = $"{cname}(이)가 관심을 보였지만\n포획 창고가 가득 찼습니다!";
            ApplyFont(txt, FontType.UI);
        }

        StartCoroutine(CoShow());
    }

    IEnumerator CoShow()
    {
        var cg = gameObject.GetOrAddComponent<CanvasGroup>();

        if (_frame != null)
        {
            _frame.localScale = Vector3.one * 0.7f;
            _frame.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        cg.alpha = 0f;
        cg.DOFade(1f, 0.25f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(2f);

        cg.DOFade(0f, 0.3f).SetEase(Ease.InQuad).SetUpdate(true)
          .OnComplete(() => Managers.Resource.Destroy(gameObject));
    }
}
