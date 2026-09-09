using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Util;

public class UI_ItemPopup : UI_Popup
{
    #region Enum

    enum Images
    {
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

    int _itemId = -1;
    bool _showGlow;

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

        // 지역과 드롭 마물을 두 줄로 표시해도 설명 영역을 침범하지 않게 폭과 높이를 고정한다.
        var found = GetText((int)Texts.ItemPopupItemDesc_Found);
        var fitter = found.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;
        var foundRT = found.rectTransform;
        float left = foundRT.anchoredPosition.x;
        float top = foundRT.anchoredPosition.y;
        foundRT.anchorMin = new Vector2(0f, 1f);
        foundRT.anchorMax = Vector2.one;
        foundRT.offsetMin = new Vector2(left, top - ITEM_SOURCE_TEXT_HEIGHT);
        foundRT.offsetMax = new Vector2(-ITEM_SOURCE_RIGHT_INSET, top);
        found.alignment = TextAlignmentOptions.TopLeft;
        found.textWrappingMode = TextWrappingModes.Normal;
        found.enableAutoSizing = true;
        found.fontSizeMax = found.fontSize;
        found.fontSizeMin = ITEM_SOURCE_MIN_FONT_SIZE;
        // 영어 출처가 세 줄이 되어도 기존 장식선과 겹치지 않게 아래로 옮긴다.
        var dividerRT = GetImage((int)Images.__1).rectTransform;
        dividerRT.anchoredPosition = new Vector2(dividerRT.anchoredPosition.x,
            top - ITEM_SOURCE_TEXT_HEIGHT - ITEM_SOURCE_DIVIDER_GAP);
        var rarity = GetText((int)Texts.ItemPopupItemDesc_Rare).rectTransform;
        rarity.anchoredPosition = new Vector2(left, rarity.anchoredPosition.y);
        ApplyFont(found, FontType.UI);
        ApplyFont(GetText((int)Texts.raretext), FontType.UI);
        ApplyFont(GetText((int)Texts.wherefoundtext), FontType.UI);
        ApplyFont(GetText((int)Texts.ItemPopupItemName), FontType.UI);
        ApplyFont(GetText((int)Texts.ItemPopupItemDesc_Rare), FontType.UI);
        ApplyFont(GetText((int)Texts.ItemPopupItemDesc), FontType.UI);
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += OnLanguageChanged;

        OpenAnimation();

        return true;
    }

    // showGlow: 퀘스트 클리어 보상으로 열릴 때만 true — 이미지 후광/shine 표시.
    //           소지품/귀중품 슬롯 클릭 등 일반 조회는 false(기본값) — 후광 없음.
    public void SetInfo(int id, bool showGlow = false)
    {
        Managers.Sound?.PlaySFX("SFX_ItemPopup");   // 아이템 팝업
        _itemId = id;
        _showGlow = showGlow;
        RefreshItemData();
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= OnLanguageChanged;
        base.OnDestroy();
    }

    void OnLanguageChanged()
    {
        // 열린 팝업의 내용만 바꾸고 사운드와 등장 연출은 다시 재생하지 않는다.
        RefreshItemData();
    }

    void RefreshItemData()
    {
        if (_itemId < 0) return;
        GetText((int)Texts.raretext).text = Localize(GROWTH_ITEM_RARITY_LABEL_TEXT);
        GetText((int)Texts.wherefoundtext).text = Localize(GROWTH_ITEM_FOUND_LABEL_TEXT);
        int id = _itemId;
        ItemData data;
        if (!Managers.Data.ItemDic.TryGetValue(id, out data) && !Managers.Data.RequisiteItemDic.TryGetValue(id, out data))
        {
            Debug.LogWarning($"[UI_ItemPopup] ItemData 없음: {id}");
            return;
        }

        // 아이템 이름
        GetText((int)Texts.ItemPopupItemName).text = Localize(data.Name);

        // 등급명 + TitleGlow는 항상 유지하되 색은 등급색(Util.GetGradeColor — 인벤/드롭/물약과 동일 컨벤션).
        // ImgGroup 후광/shine: 퀘스트 클리어 보상으로 열릴 때(showGlow) + 귀중품(REQUISITE) 아이템은 항상 표시.
        bool glowOn = _showGlow || data.Grade == EquipGrade.REQUISITE;
        GetText((int)Texts.ItemPopupItemDesc_Rare).text  = GetGradeName(data.Grade);
        GetImage((int)Images.ItemPopupTitleGlow).color = Util.GetGradeColor(data.Grade);
        var imgGlow = GetImage((int)Images.ItemPopupImgGroupGlow);
        if (imgGlow != null)
        {
            imgGlow.color = Util.GetGradeColor(data.Grade);
            imgGlow.gameObject.SetActive(glowOn);
        }
        var shine = GetObject((int)GameObjects.shineeffect);
        if (shine != null) shine.SetActive(glowOn);

        // 발견 장소
        GetText((int)Texts.ItemPopupItemDesc_Found).text = ItemSourceGuide.GetFoundLocation(id, data.FoundLocation);

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
