using Data;
using DG.Tweening;
using UnityEngine;
using static Define;
using static Util;

public class UI_ItemPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        _0,
        _2,
        Image_4,
        Image_5,
        ItemPopupFrame,
        ItemImg,
        dot,
        dot_1,
        __1,
        ScrollNotHaveBtn,
        SOMNotHaveBtn,
        MORNotHaveBtn,
        DuriShowPanel,
        Flash_USETHIS,
        Xbutton,
        ItemPopupImgGroupGlow,
        ItemPopupTitleGlow,
    }

    enum Texts
    {
        AmountText,
        ItemPopupItemName,
        raretext,
        ItemPopupItemDesc_Rare,
        wherefoundtext,
        ItemPopupItemDesc_Found,
        ItemPopupItemDesc,
        Text_Legacy,
        Text_Legacy_1,
        GameObject,
    }

    enum Buttons
    {
        ItemPopupBackground,
        _1,
        ScrollUseBtn,
        SOMUseBtn,
        MORUseBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        ItemPopup,
        ItemPopupBackground_SOMGroup,
        ItemPopupMain,
        shineeffect,
        ItemPopupImgGroup,
        ItemPopupTitle,
        ButtonGroup,
        ScrollBtnGroup,
        SOMBtnGroup,
        MORBtnGroup,
        ItemPopupFlash,
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

        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.ItemPopup).GetComponent<RectTransform>();

        GetButton((int)Buttons.ItemPopupBackground).gameObject.BindEvent(() => OnClickBG());

        GetImage((int)Images.DuriShowPanel).gameObject.SetActive(false);

        OpenAnimation();

        return true;
    }

    public void SetInfo(int id)
    {
        Managers.Sound?.PlaySFX("SFX_ItemPopup");   // 아이템 팝업
        ItemData data;
        if (!Managers.Data.ItemDic.TryGetValue(id, out data) && !Managers.Data.RequisiteItemDic.TryGetValue(id, out data))
        {
            Debug.LogWarning($"[UI_ItemPopup] ItemData 없음: {id}");
            return;
        }

        // 아이템 이름
        GetText((int)Texts.ItemPopupItemName).text = Localize(data.Name);

        // 등급명 + TitleGlow는 항상 유지하되 색은 등급색(Util.GetGradeColor — 인벤/드롭/물약과 동일 컨벤션). ImgGroup 후광/shine만 제거.
        GetText((int)Texts.ItemPopupItemDesc_Rare).text  = GetGradeName(data.Grade);
        GetImage((int)Images.ItemPopupTitleGlow).color = Util.GetGradeColor(data.Grade);
        GetImage((int)Images.ItemPopupImgGroupGlow).gameObject.SetActive(false);
        var shine = GetObject((int)GameObjects.shineeffect);
        if (shine != null) shine.SetActive(false);

        // 발견 장소
        GetText((int)Texts.ItemPopupItemDesc_Found).text = Localize(data.FoundLocation);

        // 설명
        GetText((int)Texts.ItemPopupItemDesc).text = Localize(data.Description);

        // 아이템 아이콘
        Sprite sprite = Managers.Resource.Load<Sprite>(data.SpriteName);
        GetImage((int)Images.ItemImg).sprite = sprite;

        // 보유 개수 (0이면 숨김)
        int count = Managers.Game.GetItemCount(id);
        GetText((int)Texts.AmountText).text = count.ToString();
        GetText((int)Texts.AmountText).gameObject.SetActive(count > 0);

        // 소지품 아이템(재료/귀중품)은 '사용' 액션이 없으므로 하단 버튼 그룹은 항상 숨김.
        GetObject((int)GameObjects.ButtonGroup).SetActive(false);
    }

    // ── 애니메이션 ───────────────────────────────────────────

    //void OpenAnimation()
    //{
    //    _isClosing = false;
    //    if (_frameRT == null) return;

    //    _frameRT.DOKill();
    //    _frameRT.anchoredPosition = new Vector2(0f, -120f);
    //    _frameRT.DOAnchorPosY(0f, 0.5f)
    //            .SetEase(Ease.OutBack)
    //            .SetUpdate(true);
    //}

    //void OnClickBG()
    //{
    //    if (_isClosing) return;
    //    _isClosing = true;

    //    if (_frameRT == null) { ClosePopupUI(); return; }

    //    _frameRT.DOKill();
    //    _frameRT.DOAnchorPosY(-120f, 0.5f)
    //            .SetEase(Ease.InBack)
    //            .SetUpdate(true)
    //            .OnComplete(() => ClosePopupUI());
    //}

    // ── 등급 헬퍼 ────────────────────────────────────────────

    static string GetGradeName(EquipGrade grade)
    {
        bool kor = Managers.Game.Language == Language.Korean;
        return grade switch
        {
            EquipGrade.Basic       => kor ? "일반" : "BASIC",
            EquipGrade.Abnormal    => kor ? "고급" : "ABNORMAL",
            EquipGrade.Rare        => kor ? "희귀" : "RARE",
            EquipGrade.Exceptional => kor ? "진귀" : "EXCEPTIONAL",
            EquipGrade.Legendary   => kor ? "전설" : "LEGENDARY",
            EquipGrade.Mystic      => kor ? "신화" : "MYSTIC",
            EquipGrade.REQUISITE   => kor ? "귀중" : "REQUISITE",
            _                      => grade.ToString(),
        };
    }

}
