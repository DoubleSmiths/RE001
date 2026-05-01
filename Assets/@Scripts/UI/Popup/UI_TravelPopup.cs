using Data;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Util;

public class UI_TravelPopup : UI_Popup
{
    // 대 중 소를 프리펩화해서 재구성하자
    // 데이터 연결이 중요하니까 생각해야됨
    // 현재 플레이어가 들어갈수있는 지역인지 아닌지도 필요할듯.

    // UI_TownSelectButton -> 타운(마을) 선택 버튼 (중분류) TownTypeContent -> 타운(마을) 선택 버튼 그룹
    // UI_FieldTypeButton -> 필드(탐험) 선택 버튼 (중분류) FieldTypeContent -> 필드(탐험) 선택 버튼 그룹
    // UI_MapSelectButton -> 목적지 선택 버튼 (소분류) MapTypeContent -> 목적지 선택 버튼 그룹

    // 모든 버튼은 눌렀을 때 애니메이션이 있음.
    #region Enum

    enum Images
    {
        Xbutton,
        MapSelectPopupFrame,
        MapSelectPopupTitleBar,
        MapSelectPopupMapImage,
        MapSelectPopupMapImageFrame,
        MapSelectPopupMapNameFrame,
        DecorDescriptionLineImage,
        MapSelectPopupBtmFrame,
        PlaceCategoryPanel,
        TownCategoryButtonImage,
        TownCategoryButtonQuestVFXIcon,
        FieldCategoryButtonImage,
        FieldCategoryButtonQuestVFXIcon,
        TownSelectPanel,
        TownSelectPanelFrame,
        TownSelectPanelIcon,
        TownSelectButtons,
        TownSelectMask,
        FieldSelectPanel,
        FieldSelectFrame,
        FieldSelectIcon,
        FieldSelectButtons,
        FieldSelectMask,
        MapSelectPanel,
        MapSelectFrame,
        MapSelectTitle,
        MapSelectButtons,
        MapSelectMask,
        BtmGroup,
    }

    enum Texts
    {
        XbuttonText,
        MapSelectPopupTitleText,
        MapSelectPopupMapNameText,
        TownCategoryButtonText,
        FieldCategoryButtonText,
        TownSelectPanelText,
        FieldSelectText,
        MapSelectTitleText,
        ButtonMapSelectBackText,
        ButtonMapSelectGOText,
    }

    enum Buttons
    {
        MapSelectBackground,
        TownCategoryButton,
        FieldCategoryButton,
        ButtonMapSelectBack,
        ButtonMapSelectGO,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        MapSelectPopup,
        TownCategoryButtonQuestVFX,
        FieldCategoryButtonQuestVFX,
        TownSelectContent,
        FieldSelectContent,
        MapSelectContent,
    }

    #endregion



    static readonly Color COLOR_DEFAULT = new Color32(0xFF, 0xFF, 0xFF, 0xFF);          // FFFFFF
    static readonly Color COLOR_DISABLED = new Color32(0x5E, 0x5E, 0x5E, 200);          // 5E5E5E alpha 200 그외 또는 [뒤로] 버튼이 눌러졌을 때
    static readonly Color COLOR_DESTINATION = new Color32(0xFF, 0xA3, 0x00, 0xFF);      // FFA300 행선지가 선택되었을 때
    static readonly Color COLOR_CURRENT = new Color32(0x23, 0x7B, 0x00, 0xFF);          // 237B00 해당 버튼의 장소가 현재 플레이어가 있는 장소일 시
    static readonly Color COLOR_GROUP_SELECT = new Color32(0xFF, 0xC5, 0x00, 0xFF);     // FFC500 선택 되었을 때
    static readonly Color COLOR_BUTTON_DEFAULT = new Color32(0x95, 0x95, 0x95, 0xFF);   // 959595 클릭 가능 버튼들의 기본 색상
    static readonly Color COLOR_ROOT_IDLE = new Color32(0xFF, 0xFF, 0xFF, 0xFF);        // FFFFFF (대분류 버튼 본체)
    static readonly Color COLOR_ROOT_ICON = new Color32(0xFF, 0xFF, 0xFF, 80);          // FFFFFF alpha 80 (대분류 아이콘)

    string _selectedCategory = "";
    const string TOWN_CATEGORY = "TOWN";
    const string FIELD_CATEGORY = "FIELD";

    List<TravelNodeData> _allNode = new();
    List<GameObject> _spawnedTownButtons = new();

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        ApplySharedFonts();

        GetButton((int)Buttons.MapSelectBackground).gameObject.BindEvent(OnClickBG);
        GetButton((int)Buttons.TownCategoryButton).gameObject.BindEvent(OnClickTownCategory);
        GetButton((int)Buttons.FieldCategoryButton).gameObject.BindEvent(OnClickFieldCategory);
        GetButton((int)Buttons.ButtonMapSelectGO).gameObject.BindEvent(OnClickGo);
        GetButton((int)Buttons.ButtonMapSelectBack).gameObject.BindEvent(OnClickBack);

        GameObject popupGo = GetObject((int)GameObjects.MapSelectPopup);
        if (popupGo != null)
            _frameRT = popupGo.GetComponent<RectTransform>();

        //if (Managers.Quest != null)
        //{
        //    Managers.Quest.OnNotifyChanged += RefreshAll;
        //    Managers.Quest.OnQuestChanged += OnQuestChanged;
        //    Managers.Quest.OnBtnUnlocked += OnButtonUnlocked;
        //}

        //if (Managers.Game != null)
        //{
        //    Managers.Game.OnItemChanged += OnInventoryChanged;
        //    Managers.Game.OnLanguageChanged += OnLanguageChanged;
        //}

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        //if (Managers.Quest != null)
        //{
        //    Managers.Quest.OnNotifyChanged -= RefreshAll;
        //    Managers.Quest.OnQuestChanged -= OnQuestChanged;
        //    Managers.Quest.OnBtnUnlocked -= OnButtonUnlocked;
        //}

        //if (Managers.Game != null)
        //{
        //    Managers.Game.OnItemChanged -= OnInventoryChanged;
        //    Managers.Game.OnLanguageChanged -= OnLanguageChanged;
        //}
    }

    public void SetInfo()
    {
        _allNode = Managers.Data.TravelNodeDic.Values.OrderBy(n => n.Id).ToList(); // 이 코드 되게 멋있다 기억해놔야지. SQL의 ORDER BY 같은 느낌이네. 데이터가 많아지면 성능 이슈 있을 수 있으니 주의하자.
        Debug.Log($"[Travel] Loaded {_allNode.Count} nodes.");

        _selectedCategory = "";
        GetImage((int)Images.TownSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.FieldSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.MapSelectPanel).gameObject.SetActive(false);

        BuildTownButtons();

        RefreshUI();
        OpenAnimation();
    }

    void ApplySharedFonts()
    {
        foreach (Texts textId in Enum.GetValues(typeof(Texts)))
        {
            TMP_Text text = GetText((int)textId);
            if (text != null)
                ApplyFont(text, FontType.UI);
        }
    }

    public override void RefreshUI()
    {
        Image townFrame = GetButton((int)Buttons.TownCategoryButton)?.GetComponent<Image>();
        Image fieldFrame = GetButton((int)Buttons.FieldCategoryButton)?.GetComponent<Image>();

        bool isTownSelected = _selectedCategory == TOWN_CATEGORY;
        bool isFieldSelected = _selectedCategory == FIELD_CATEGORY;

        if (townFrame != null) townFrame.color = isTownSelected ? COLOR_GROUP_SELECT : COLOR_ROOT_IDLE;
        if (fieldFrame != null) fieldFrame.color = isFieldSelected ? COLOR_GROUP_SELECT : COLOR_ROOT_IDLE;

        GetButton((int)Buttons.ButtonMapSelectGO).interactable = CanGo();
        GetButton((int)Buttons.ButtonMapSelectBack).interactable = IsAnyPanelActive();
    }

    bool CanGo()
    {
        if (_selectedCategory == TOWN_CATEGORY)
            return !GetImage((int)Images.TownSelectPanel).gameObject.activeSelf;
        if (_selectedCategory == FIELD_CATEGORY)
            return !GetImage((int)Images.FieldSelectPanel).gameObject.activeSelf;

        return false;
    }

    bool IsAnyPanelActive()
    {
        return GetImage((int)Images.TownSelectPanel).gameObject.activeSelf ||
               GetImage((int)Images.FieldSelectPanel).gameObject.activeSelf ||
               GetImage((int)Images.MapSelectPanel).gameObject.activeSelf;
    }

    void OnClickTownCategory()
    {
        _selectedCategory = TOWN_CATEGORY; // 일단 하드코딩인데 이거 Define으로 이동할 예정임. 아닌가 굳이 안해도 되긴하는데 흠..
        Debug.Log("[UI_TravelPopup] Selected: TOWN");

        RefreshUI();
    }

    void OnClickFieldCategory()
    {
        _selectedCategory = FIELD_CATEGORY;
        Debug.Log("[UI_TravelPopup] Selected: FIELD");

        RefreshUI();
    }

    void OnClickGo()
    {
        if (_selectedCategory == TOWN_CATEGORY)
            GetImage((int)Images.TownSelectPanel).gameObject.SetActive(true);
        else if (_selectedCategory == FIELD_CATEGORY)
            GetImage((int)Images.FieldSelectPanel).gameObject.SetActive(true);

        RefreshUI();
    }

    void OnClickBack()
    {
        //GetButton((int)Buttons.ButtonMapSelectBack).interactable = false; // 중복 클릭 방지 위해 바로 비활성화

        if (GetImage((int)Images.MapSelectPanel).gameObject.activeSelf)
        {
            ClosePanelWithAnim(GetImage((int)Images.MapSelectPanel).gameObject);

            RefreshUI();
            return;
        }

        if (GetImage((int)Images.TownSelectPanel).gameObject.activeSelf || GetImage((int)Images.FieldSelectPanel).gameObject.activeSelf)
        {
            ClosePanelWithAnim(GetImage((int)Images.TownSelectPanel).gameObject);
            ClosePanelWithAnim(GetImage((int)Images.FieldSelectPanel).gameObject);
            _selectedCategory = "";

            RefreshUI();
            return;
        }

    }

    void BuildTownButtons()
    {
        // 기존 인스턴스 정리.
        foreach (GameObject go in _spawnedTownButtons)
            Destroy(go);

        _spawnedTownButtons.Clear();

        // 부모없으면 리턴
        GameObject parent = GetObject((int)GameObjects.TownSelectContent);
        if (parent == null) return;

        GameObject prefab = Managers.Resource.Load<GameObject>("UI_TownSelectButton");
        if (prefab == null)
        {
            Debug.LogError("[UI_TravelPopup] Failed to load TownSelectButton prefab.");
            return;
        }

        // TOWN 행선지 = Category=TOWN, ParentId = 0
        var townNodes = _allNode.Where(n => n.Category == TOWN_CATEGORY && n.ParentId == 0).ToList();

        foreach (var node in townNodes)
        {
            GameObject btn = Managers.Resource.Instantiate("UI_TownSelectButton", parent.transform);
            btn.name = $"UI_TownSelectButton_{node.Id}";

            // Click Test
            UI_TownSelectButton townBtn = btn.GetComponent<UI_TownSelectButton>();
            townBtn.SetInfo(node, Managers.Game.CurrentTravelAreaId == node.Id);

            _spawnedTownButtons.Add(btn);
        }

        Debug.Log($"[UI_TravelPopup] Spawned {_spawnedTownButtons.Count} town buttons.");
    }


}
