using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Util;

public class UI_TravelPopup : UI_Popup
{
    enum TravelDepth
    {
        Root,
        Category,
        Detail,
    }

    enum Images
    {
        Xbutton,
        MapSelectPopupFrame,
        MapSelectPopupTitleBar,
        MapSelectPopupMapImage,
        MapSelectPopupMapImageFrame,
        MapSelectPopupMapNameFrame,
        Decor_description_line_Image,
        MapSelectPopupBtmFrame,
        PlaceType,
        PlaceTypeTownImage,
        AdventureImage,
        TownType,
        TownImage,
        EFrame,
        FieldType,
        FieldTypeAdventureImage,
        BWFType,
        BWFTypeFrame,
        BWFTypeTitle,
        TitleTextFrame,
        MapSelectBWF_EntryBtnFrame,
        MapSelectBWF_EntryBtnImage,
        MapSelectBWFBtnFrame,
        MapSelectBWFBtnImage,
        BtmGroup,
    }

    enum Texts
    {
        XbuttonText,
        MapSelectPopupTitleText,
        MapSelectPopupMapNameText,
        PlaceTypeTownText,
        AdventrueText,
        TownTypeTownText,
        MapSelectAsdalBtnText,
        MapSelectPopup_CurrentLocationTxt,
        FieldTypeAdventureText,
        ButtonBWFTypeText,
        BWFTypeTitleText,
        MapSelectBWF_EntryBtnText,
        MapSelectBWFBtnText,
        ButtonMapSelectBackText,
        ButtonMapSelectGOText,
    }

    enum Buttons
    {
        MapSelectBackground,
        Buttonto_TownType,
        Buttonto_FieldType,
        ButtonMapSelectBack_TOP_1,
        MapSelectAsdalBtn,
        ButtonMapSelectBack_TOP,
        ButtonBWFType,
        MapSelectBWF_EntryBtn,
        MapSelectBWFBtn,
        ButtonMapSelectBack,
        ButtonMapSelectGO,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        MapSelectPopup,
        to_TownButtonQuestVFX,
        to_FieldQuestVFX,
        TownTypeContent,
        AsdalButtonQuestVFX,
        MapSelectPopupActiveEf,
        Content,
        ButtontoBWFVFX,
        BWFTypeContent,
        MapSelectBWF_EntryVFX,
    }

    class TravelNodeButtonView
    {
        public int DataId;
        public GameObject Root;
        public Button Button;
        public TMP_Text Text;
        public GameObject Notify;
        public Image Frame;
    }

    readonly List<GameObject> _clonedTownButtons = new();
    readonly List<GameObject> _clonedExploreButtons = new();
    readonly List<GameObject> _clonedDetailButtons = new();

    readonly Dictionary<int, TravelNodeButtonView> _middleButtonViews = new();
    readonly Dictionary<int, TravelNodeButtonView> _detailButtonViews = new();

    List<TravelAreaData> _allNodes = new();
    List<TravelAreaData> _rootNodes = new();

    int _selectedRootId;
    int _selectedGroupId;
    int _selectedDetailId;
    TravelDepth _depth = TravelDepth.Root;
    TravelDepth _prevDepth = TravelDepth.Root;
    int _prevSelectedRootId;
    int _prevSelectedGroupId;
    bool _isEntering;
    string _selectionFeedbackMessage = string.Empty;

    RectTransform _townPanelRT;
    RectTransform _fieldPanelRT;
    RectTransform _bwfPanelRT;
    Vector2 _townPanelOrigin;
    Vector2 _fieldPanelOrigin;
    Vector2 _bwfPanelOrigin;

    static readonly Vector2 LAYER_FROM_OFFSET = new Vector2(0f, -150f);
    const float LAYER_ANIM_DURATION = 0.3f;

    static readonly Color COLOR_DEFAULT       = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // FFFFFF
    static readonly Color COLOR_DISABLED      = new Color32(0x5E, 0x5E, 0x5E, 200);   // 5E5E5E alpha 200
    static readonly Color COLOR_DESTINATION   = new Color32(0xFF, 0xA3, 0x00, 0xFF); // FFA300
    static readonly Color COLOR_CURRENT       = new Color32(0x23, 0x7B, 0x00, 0xFF); // 237B00
    static readonly Color COLOR_GROUP_SELECT  = new Color32(0xFF, 0xC5, 0x00, 0xFF); // FFC500
    static readonly Color COLOR_ROOT_IDLE     = new Color32(0x95, 0x95, 0x95, 0xFF); // 959595

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

        GameObject popupGo = GetObject((int)GameObjects.MapSelectPopup);
        if (popupGo != null)
            _frameRT = popupGo.GetComponent<RectTransform>();
        _openFromY = -120f;
        _closeToY = -120f;

        CachePanel(Images.TownType, out _townPanelRT, out _townPanelOrigin);
        CachePanel(Images.FieldType, out _fieldPanelRT, out _fieldPanelOrigin);
        CachePanel(Images.BWFType, out _bwfPanelRT, out _bwfPanelOrigin);

        // 탐험 소분류에서 TitleTextFrame("FIELD") 불필요 — 영구 비활성
        Image titleFrame = GetImage((int)Images.TitleTextFrame);
        if (titleFrame != null)
            titleFrame.gameObject.SetActive(false);

        BindClickWithAnim(Buttons.MapSelectBackground, ClosePopupUI);
        BindClickWithAnim(Buttons.ButtonMapSelectBack_TOP_1, OnClickBack);
        BindClickWithAnim(Buttons.ButtonMapSelectBack_TOP, OnClickBack);
        BindClickWithAnim(Buttons.Buttonto_TownType, () => OnClickRootCategory("TOWN"));
        BindClickWithAnim(Buttons.Buttonto_FieldType, () => OnClickRootCategory("EXPLORE"));
        BindClickWithAnim(Buttons.MapSelectBWF_EntryBtn, OnClickEnterSelectedDetail);
        BindClickWithAnim(Buttons.ButtonMapSelectGO, OnClickEnterSelectedDetail);
        BindClickWithAnim(Buttons.ButtonMapSelectBack, OnClickBack);

        if (Managers.Quest != null)
        {
            Managers.Quest.OnNotifyChanged += RefreshAll;
            Managers.Quest.OnQuestChanged += OnQuestChanged;
            Managers.Quest.OnBtnUnlocked += OnButtonUnlocked;
        }

        if (Managers.Game != null)
        {
            Managers.Game.OnItemChanged += OnInventoryChanged;
            Managers.Game.OnLanguageChanged += OnLanguageChanged;
        }

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Managers.Quest != null)
        {
            Managers.Quest.OnNotifyChanged -= RefreshAll;
            Managers.Quest.OnQuestChanged -= OnQuestChanged;
            Managers.Quest.OnBtnUnlocked -= OnButtonUnlocked;
        }

        if (Managers.Game != null)
        {
            Managers.Game.OnItemChanged -= OnInventoryChanged;
            Managers.Game.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    public void SetInfo()
    {
        _isEntering = false;
        _selectionFeedbackMessage = string.Empty;

        _allNodes = Managers.Data.TravelAreaDic.Values
            .OrderBy(node => node.Depth)
            .ThenBy(node => node.Order)
            .ThenBy(node => node.DataId)
            .ToList();

        _rootNodes = _allNodes
            .Where(node => node.Depth == 1)
            .OrderBy(node => node.Order)
            .ThenBy(node => node.DataId)
            .ToList();

        RestoreSelection();

        BuildMiddleButtons();
        BuildDetailButtons();
        RefreshStaticTexts();

        _prevDepth = TravelDepth.Root;
        _prevSelectedRootId = 0;
        _prevSelectedGroupId = 0;

        RefreshAll();

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

    void BuildMiddleButtons()
    {
        BuildButtonGroup(
            GetButton((int)Buttons.MapSelectAsdalBtn),
            GetObject((int)GameObjects.TownTypeContent),
            GetGroupNodesForCategory("TOWN"),
            _clonedTownButtons,
            _middleButtonViews,
            "MapSelectAsdalBtnText",
            new[] { "AsdalButtonQuestVFX", "QuestMarker" },
            OnClickGroupNode);

        BuildButtonGroup(
            GetButton((int)Buttons.ButtonBWFType),
            GetObject((int)GameObjects.Content),
            GetGroupNodesForCategory("EXPLORE"),
            _clonedExploreButtons,
            _middleButtonViews,
            "ButtonBWFTypeText",
            new[] { "ButtontoBWFVFX", "QuestMarker" },
            OnClickGroupNode);
    }

    void BuildDetailButtons()
    {
        _detailButtonViews.Clear();
        foreach (GameObject clone in _clonedDetailButtons)
        {
            if (clone != null)
                Destroy(clone);
        }
        _clonedDetailButtons.Clear();

        Button templateButton = GetButton((int)Buttons.MapSelectBWFBtn);
        GameObject detailRoot = GetObject((int)GameObjects.BWFTypeContent);
        if (templateButton == null || detailRoot == null)
            return;

        List<TravelAreaData> detailNodes = GetDetailNodes(_selectedGroupId);
        GameObject templateRoot = templateButton.gameObject;
        templateRoot.SetActive(detailNodes.Count > 0);

        for (int i = 0; i < detailNodes.Count; i++)
        {
            TravelAreaData node = detailNodes[i];
            GameObject buttonRoot;

            if (i == 0)
            {
                buttonRoot = templateRoot;
            }
            else
            {
                buttonRoot = Instantiate(templateRoot, detailRoot.transform);
                buttonRoot.name = $"TravelDetailButton_{node.DataId}";
                buttonRoot.transform.SetParent(detailRoot.transform, false);
                buttonRoot.transform.localScale = Vector3.one;
                _clonedDetailButtons.Add(buttonRoot);
            }

            TravelNodeButtonView view = CreateButtonView(
                buttonRoot,
                node.DataId,
                "MapSelectBWFBtnText",
                new[] { "QuestMarker", "MapSelectBWF_EntryVFX" });

            _detailButtonViews[node.DataId] = view;
            int capturedId = node.DataId;
            RebindClickEvent(view.Button?.gameObject, () => OnClickDetailNode(capturedId));
            buttonRoot.SetActive(true);
        }

        Transform entryButton = GetButton((int)Buttons.MapSelectBWF_EntryBtn)?.transform;
        if (entryButton != null)
            entryButton.SetAsLastSibling();
    }

    void BuildButtonGroup(
        Button templateButton,
        GameObject parentRoot,
        List<TravelAreaData> nodes,
        List<GameObject> clones,
        Dictionary<int, TravelNodeButtonView> targetViews,
        string textName,
        string[] notifyNames,
        Action<int> clickHandler)
    {
        foreach (GameObject clone in clones)
        {
            if (clone != null)
                Destroy(clone);
        }
        clones.Clear();

        if (templateButton == null || parentRoot == null)
            return;

        GameObject templateRoot = templateButton.gameObject;
        templateRoot.SetActive(nodes.Count > 0);

        for (int i = 0; i < nodes.Count; i++)
        {
            TravelAreaData node = nodes[i];
            GameObject buttonRoot;

            if (i == 0)
            {
                buttonRoot = templateRoot;
            }
            else
            {
                buttonRoot = Instantiate(templateRoot, parentRoot.transform);
                buttonRoot.name = $"{templateRoot.name}_{node.DataId}";
                buttonRoot.transform.SetParent(parentRoot.transform, false);
                buttonRoot.transform.localScale = Vector3.one;
                clones.Add(buttonRoot);
            }

            TravelNodeButtonView view = CreateButtonView(buttonRoot, node.DataId, textName, notifyNames);
            targetViews[node.DataId] = view;
            int capturedId = node.DataId;
            RebindClickEvent(view.Button?.gameObject, () => clickHandler(capturedId));
            buttonRoot.SetActive(true);
        }
    }

    TravelNodeButtonView CreateButtonView(GameObject root, int dataId, string textName, string[] notifyNames)
    {
        GameObject notify = null;
        foreach (string notifyName in notifyNames)
        {
            notify = FindChild(root, notifyName, true);
            if (notify != null)
                break;
        }

        return new TravelNodeButtonView
        {
            DataId = dataId,
            Root = root,
            Button = root.GetComponent<Button>(),
            Text = FindChild<TMP_Text>(root, textName, true) ?? root.GetComponentInChildren<TMP_Text>(true),
            Notify = notify,
            Frame = root.GetComponent<Image>(),
        };
    }

    void RefreshAll()
    {
        NormalizeSelectionState();
        RefreshVisiblePanels();
        RefreshRootCategoryButtons();
        RefreshMiddleButtons();
        RefreshDetailButtons();
        RefreshSelectionPanel();

        _prevDepth = _depth;
        _prevSelectedRootId = _selectedRootId;
        _prevSelectedGroupId = _selectedGroupId;
    }

    void BindClickWithAnim(Buttons btnId, Action action)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null)
            return;
        btn.gameObject.BindEvent(() =>
        {
            PlayButtonYoyo(btn.transform);
            action?.Invoke();
        });
    }

    static void ApplyButtonTint(Button btn, Image frame, Color color)
    {
        if (frame != null)
            frame.color = color;
        if (btn == null)
            return;
        var colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.pressedColor = color * 0.9f;
        colors.disabledColor = COLOR_DISABLED;
        btn.colors = colors;
    }

    void PlayButtonYoyo(Transform tr)
    {
        if (tr == null)
            return;
        tr.DOKill();
        tr.localScale = Vector3.one;
        tr.DOScale(0.95f, 0.07f)
          .SetEase(Ease.OutQuad)
          .SetLoops(2, LoopType.Yoyo)
          .SetUpdate(true);
    }

    void RefreshStaticTexts()
    {
        SetRawText(Texts.XbuttonText, "X");
        SetText(Texts.MapSelectPopupTitleText, LocalizeKey.TravelBtnText);
        SetText(Texts.ButtonMapSelectBackText, "BackBtnText");
        SetText(Texts.ButtonMapSelectGOText, "HeadOutText");
    }

    void RefreshRootCategoryButtons()
    {
        TravelAreaData townRoot = GetRootNode("TOWN");
        TravelAreaData exploreRoot = GetRootNode("EXPLORE");

        SetRawText(Texts.PlaceTypeTownText, ResolveNodeName(townRoot, "TownText"));
        SetRawText(Texts.AdventrueText, ResolveNodeName(exploreRoot, "ExploreText"));
        SetRawText(Texts.TownTypeTownText, ResolveNodeName(townRoot, "TownText"));
        SetRawText(Texts.FieldTypeAdventureText, ResolveNodeName(exploreRoot, "ExploreText"));

        GetObject((int)GameObjects.to_TownButtonQuestVFX)?.SetActive(townRoot != null && HasNotifyInSubtree(townRoot.DataId));
        GetObject((int)GameObjects.to_FieldQuestVFX)?.SetActive(exploreRoot != null && HasNotifyInSubtree(exploreRoot.DataId));
        GetObject((int)GameObjects.AsdalButtonQuestVFX)?.SetActive(false);

        Button townBtn = GetButton((int)Buttons.Buttonto_TownType);
        Button exploreBtn = GetButton((int)Buttons.Buttonto_FieldType);

        bool townSelected = townRoot != null && _selectedRootId == townRoot.DataId && _depth != TravelDepth.Root;
        bool exploreSelected = exploreRoot != null && _selectedRootId == exploreRoot.DataId && _depth != TravelDepth.Root;

        ApplyRootCategoryState(townBtn, GetImage((int)Images.PlaceTypeTownImage), townSelected, _depth != TravelDepth.Root && townSelected == false);
        ApplyRootCategoryState(exploreBtn, GetImage((int)Images.AdventureImage), exploreSelected, _depth != TravelDepth.Root && exploreSelected == false);

        if (_depth == TravelDepth.Root)
        {
            TravelAreaData selectedRoot = TryGetNode(_selectedRootId, out TravelAreaData root) ? root : exploreRoot ?? townRoot;
            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, ResolveNodeSummary(selectedRoot, "CurrentLocationText"));
        }
    }

    void ApplyRootCategoryState(Button btn, Image iconImage, bool selected, bool hidden)
    {
        if (btn == null)
            return;

        if (hidden)
        {
            btn.gameObject.SetActive(false);
            return;
        }

        btn.gameObject.SetActive(true);
        btn.interactable = true;

        Image frame = btn.GetComponent<Image>();
        Color target = selected ? COLOR_GROUP_SELECT : COLOR_ROOT_IDLE;
        ApplyButtonTint(btn, frame, target);
        if (iconImage != null)
            iconImage.color = selected ? Color.white : COLOR_ROOT_IDLE;

        btn.transform.localScale = Vector3.one;
    }

    void RefreshMiddleButtons()
    {
        bool townRootSelected = IsTownRoot(_selectedRootId);
        int currentAreaId = GetEffectiveCurrentAreaId();

        foreach (TravelAreaData node in _allNodes.Where(node => node.Depth == 2))
        {
            if (_middleButtonViews.TryGetValue(node.DataId, out TravelNodeButtonView view) == false)
                continue;

            bool unlocked = IsNodeUnlocked(node.DataId, out _);
            bool layerOpen = _depth >= TravelDepth.Category && node.RootId == _selectedRootId;
            bool isCurrent = node.DataId == currentAreaId;
            bool isSelected = node.DataId == _selectedGroupId && _depth == TravelDepth.Detail;

            if (view.Text != null)
            {
                view.Text.text = ResolveText(node.Name);
                view.Text.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }

            Color frameColor;
            bool interactable;
            if (layerOpen == false || unlocked == false)
            {
                frameColor = COLOR_DISABLED;
                interactable = false;
            }
            else if (isCurrent)
            {
                frameColor = COLOR_CURRENT;
                interactable = false;
            }
            else if (isSelected)
            {
                frameColor = COLOR_GROUP_SELECT;
                interactable = true;
            }
            else if (townRootSelected)
            {
                // Town middle button: clicking it selects destination (FFA300 when chosen → but 마을 type middle becomes destination on click)
                frameColor = COLOR_DEFAULT;
                interactable = IsSelectableNode(node);
            }
            else
            {
                frameColor = COLOR_DEFAULT;
                interactable = IsSelectableNode(node);
            }

            ApplyButtonTint(view.Button, view.Frame, frameColor);

            if (view.Text != null)
                view.Text.color = interactable ? Color.white : new Color(1f, 1f, 1f, 0.6f);

            if (view.Notify != null)
                view.Notify.SetActive(HasNotifyInSubtree(node.DataId));

            if (view.Button != null)
                view.Button.interactable = interactable;

            view.Root.transform.localScale = Vector3.one;
        }
    }

    void RefreshDetailButtons()
    {
        int currentAreaId = GetEffectiveCurrentAreaId();

        foreach (TravelAreaData node in GetDetailNodes(_selectedGroupId))
        {
            if (_detailButtonViews.TryGetValue(node.DataId, out TravelNodeButtonView view) == false)
                continue;

            bool unlocked = TryGetSelectionBlockReason(node, out _) == false;
            bool layerOpen = _depth == TravelDepth.Detail;
            bool isCurrent = node.DataId == currentAreaId;
            bool isSelected = node.DataId == _selectedDetailId;
            bool canEnter = CanEnterNode(node, out _);

            if (view.Text != null)
            {
                view.Text.text = ResolveText(node.Name);
                view.Text.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }

            Color frameColor;
            bool interactable;
            if (layerOpen == false || unlocked == false)
            {
                frameColor = COLOR_DISABLED;
                interactable = false;
            }
            else if (isCurrent)
            {
                frameColor = COLOR_CURRENT;
                interactable = false;
            }
            else if (isSelected)
            {
                frameColor = COLOR_DESTINATION;
                interactable = true;
            }
            else
            {
                frameColor = COLOR_DEFAULT;
                interactable = IsSelectableNode(node);
            }

            ApplyButtonTint(view.Button, view.Frame, frameColor);

            if (view.Text != null)
                view.Text.color = interactable ? Color.white : new Color(1f, 1f, 1f, 0.6f);

            if (view.Notify != null)
                view.Notify.SetActive(canEnter && HasEntryNotify(node));

            if (view.Button != null)
                view.Button.interactable = interactable;

            view.Root.transform.localScale = Vector3.one;
        }
    }

    void RefreshSelectionPanel()
    {
        TravelAreaData root = TryGetNode(_selectedRootId, out TravelAreaData rootNode) ? rootNode : null;
        TravelAreaData group = TryGetNode(_selectedGroupId, out TravelAreaData groupNode) ? groupNode : null;
        TravelAreaData detail = TryGetNode(_selectedDetailId, out TravelAreaData detailNode) ? detailNode : null;

        // MAP_IMG / PlaceName 은 항상 "현재 플레이어 위치" 기준 (스펙 슬라이드 43: 유동적)
        // 단, 상세 선택 상태에선 선택된 목적지를 preview (스크린샷 4 참고)
        TravelAreaData currentLocation = ResolveCurrentLocationNode();
        TravelAreaData displayNode = detail ?? currentLocation ?? group ?? root;
        if (displayNode == null)
            return;

        bool unlocked = IsNodeUnlocked(displayNode.DataId, out string failMessage);
        string enterFailMessage = string.Empty;
        bool canEnter = detail != null && CanEnterNode(detail, out enterFailMessage);
        string displayInfoOverride = _selectionFeedbackMessage;

        SetRawText(Texts.MapSelectPopupMapNameText, ResolveText(displayNode.Name));
        SetRawText(Texts.BWFTypeTitleText, ResolveText(group?.Name ?? detail?.Name ?? root?.Name));

        if (_depth == TravelDepth.Detail)
        {
            string info = canEnter
                ? ResolveNodeSummary(detail ?? group, group?.Description ?? displayNode.Description)
                : string.IsNullOrEmpty(enterFailMessage) ? ResolveNodeSummary(detail ?? group, displayNode.Description) : enterFailMessage;

            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, string.IsNullOrEmpty(displayInfoOverride) ? info : displayInfoOverride);
            SetRawText(Texts.MapSelectBWF_EntryBtnText, canEnter ? ResolveText("HeadOutText") : ResolveText("NextInfoText"));
        }
        else if (_depth == TravelDepth.Category)
        {
            int currentAreaId = GetEffectiveCurrentAreaId();
            string categoryInfo;
            if (group != null && group.DataId == currentAreaId)
                categoryInfo = ResolveText("CurrentLocationText");
            else
                categoryInfo = ResolveCategoryInfo(root, group);

            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, string.IsNullOrEmpty(displayInfoOverride) ? categoryInfo : displayInfoOverride);
            SetRawText(Texts.MapSelectBWF_EntryBtnText, ResolveText("NextInfoText"));
        }
        else
        {
            string rootInfo = ResolveNodeSummary(root, "CurrentLocationText");
            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, string.IsNullOrEmpty(displayInfoOverride) ? rootInfo : displayInfoOverride);
        }

        Image mapImage = GetImage((int)Images.MapSelectPopupMapImage);
        if (mapImage != null && string.IsNullOrEmpty(displayNode.SpriteName) == false)
        {
            Sprite cached = Managers.Resource.Load<Sprite>(displayNode.SpriteName);
            if (cached != null)
            {
                mapImage.sprite = cached;
            }
            else
            {
                string key = displayNode.SpriteName;
                Image target = mapImage;
                Managers.Resource.LoadAsync<Sprite>(key, (s) =>
                {
                    if (s != null && target != null)
                        target.sprite = s;
                });
            }
        }

        Button entryButton = GetButton((int)Buttons.MapSelectBWF_EntryBtn);
        if (entryButton != null)
            entryButton.interactable = canEnter;

        GetObject((int)GameObjects.MapSelectBWF_EntryVFX)?.SetActive(detail != null && canEnter && HasEntryNotify(detail));

        Button goButton = GetButton((int)Buttons.ButtonMapSelectGO);
        if (goButton != null)
        {
            goButton.gameObject.SetActive(true);
            // 이동 버튼: 행선지 선택되고 해당 위치가 현재 위치가 아닐 때만 활성화 (FFA300)
            int currentAreaId = GetEffectiveCurrentAreaId();
            bool goEnabled = canEnter && detail != null && detail.DataId != currentAreaId;
            goButton.interactable = goEnabled;
            Image goFrame = goButton.GetComponent<Image>();
            Color goColor = goEnabled ? COLOR_DESTINATION : COLOR_DISABLED;
            if (goFrame != null)
                goFrame.color = goColor;
            // Button.colors 도 함께 변경 — 그렇지 않으면 Unity Selectable이 normalColor로 Image.color를 덮어씀
            var colors = goButton.colors;
            colors.normalColor = goColor;
            colors.highlightedColor = goColor;
            colors.selectedColor = goColor;
            colors.pressedColor = goColor * 0.9f;
            colors.disabledColor = COLOR_DISABLED;
            goButton.colors = colors;
        }
    }

    void RefreshVisiblePanels()
    {
        SetPanelActive("PlaceType", true);

        bool townVisible = _depth >= TravelDepth.Category && IsTownRoot(_selectedRootId);
        bool fieldVisible = _depth >= TravelDepth.Category && IsExploreRoot(_selectedRootId);
        bool bwfVisible = _depth == TravelDepth.Detail;

        bool townWasVisible = _prevDepth >= TravelDepth.Category && IsTownRootId(_prevSelectedRootId);
        bool fieldWasVisible = _prevDepth >= TravelDepth.Category && IsExploreRootId(_prevSelectedRootId);
        bool bwfWasVisible = _prevDepth == TravelDepth.Detail;

        AnimatePanel(_townPanelRT, _townPanelOrigin, townVisible, townWasVisible);
        AnimatePanel(_fieldPanelRT, _fieldPanelOrigin, fieldVisible, fieldWasVisible);
        AnimatePanel(_bwfPanelRT, _bwfPanelOrigin, bwfVisible, bwfWasVisible);
    }

    bool IsTownRootId(int dataId)
    {
        return dataId != 0 && IsTownRoot(dataId);
    }

    bool IsExploreRootId(int dataId)
    {
        return dataId != 0 && IsExploreRoot(dataId);
    }

    void AnimatePanel(RectTransform rt, Vector2 origin, bool shouldBeVisible, bool wasVisible)
    {
        if (rt == null)
            return;

        if (shouldBeVisible && wasVisible == false)
        {
            rt.DOKill();
            rt.gameObject.SetActive(true);
            rt.anchoredPosition = origin + LAYER_FROM_OFFSET;
            rt.DOAnchorPos(origin, LAYER_ANIM_DURATION)
              .SetEase(Ease.OutQuad)
              .SetUpdate(true);
        }
        else if (shouldBeVisible == false && wasVisible)
        {
            // 뒤로: 소 → 중 순으로 DOPlayBackwards 효과 (아래로 밀려나며 사라짐)
            rt.DOKill();
            Vector2 exitPos = origin + LAYER_FROM_OFFSET;
            RectTransform target = rt;
            target.DOAnchorPos(exitPos, LAYER_ANIM_DURATION)
                  .SetEase(Ease.InQuad)
                  .SetUpdate(true)
                  .OnComplete(() =>
                  {
                      if (target != null)
                      {
                          target.gameObject.SetActive(false);
                          target.anchoredPosition = origin;
                      }
                  });
        }
        else if (shouldBeVisible && wasVisible)
        {
            // 이미 열려있음: 위치만 고정
            rt.gameObject.SetActive(true);
            if (DOTween.IsTweening(rt) == false)
                rt.anchoredPosition = origin;
        }
        else
        {
            rt.gameObject.SetActive(false);
        }
    }

    void CachePanel(Images imageId, out RectTransform rt, out Vector2 origin)
    {
        rt = null;
        origin = Vector2.zero;
        Image img = GetImage((int)imageId);
        if (img == null)
            return;
        rt = img.rectTransform;
        origin = rt.anchoredPosition;
    }

    int GetEffectiveCurrentAreaId()
    {
        int raw = Managers.Game != null ? Managers.Game.CurrentTravelAreaId : 0;
        if (raw != 0)
            return raw;
        TravelAreaData fallback = ResolveCurrentLocationNode();
        return fallback?.DataId ?? 0;
    }

    TravelAreaData ResolveCurrentLocationNode()
    {
        int currentId = Managers.Game != null ? Managers.Game.CurrentTravelAreaId : 0;
        if (currentId != 0 && TryGetNode(currentId, out TravelAreaData node))
            return node;

        // 기본 시작 위치: TOWN 루트 하위의 첫 depth==2 노드 (아스달 마을)
        TravelAreaData townRoot = GetRootNode("TOWN");
        if (townRoot == null)
            return null;
        return _allNodes
            .Where(n => n.Depth == 2 && n.ParentId == townRoot.DataId)
            .OrderBy(n => n.Order)
            .ThenBy(n => n.DataId)
            .FirstOrDefault();
    }

    void OnClickRootCategory(string category)
    {
        TravelAreaData root = GetRootNode(category);
        if (root == null)
            return;

        _selectionFeedbackMessage = string.Empty;
        _selectedRootId = root.DataId;
        _selectedGroupId = GetFirstUnlockedGroup(root.DataId)?.DataId ?? 0;
        _selectedDetailId = 0;
        _depth = TravelDepth.Category;

        if (string.Equals(category, "EXPLORE", StringComparison.OrdinalIgnoreCase))
            Managers.Quest.CheckButtonClick(BTN_EXPLORE);

        RefreshAll();
    }

    void OnClickGroupNode(int dataId)
    {
        if (TryGetNode(dataId, out TravelAreaData node) == false)
            return;

        // 현재 플레이어가 있는 장소는 Interactive=X (슬라이드 46 스펙)
        if (dataId == GetEffectiveCurrentAreaId())
            return;

        if (IsSelectableNode(node) == false)
            return;

        if (TryGetSelectionBlockReason(node, out string failMessage))
        {
            Debug.LogWarning($"[UI_TravelPopup] Group selection blocked: node={dataId}, reason={failMessage}");
            _selectionFeedbackMessage = failMessage;
            RefreshAll();
            return;
        }

        _selectionFeedbackMessage = string.Empty;
        _selectedRootId = node.RootId != 0 ? node.RootId : _selectedRootId;
        _selectedGroupId = node.DataId;
        _selectedDetailId = 0;
        _depth = TravelDepth.Detail;

        if (string.IsNullOrEmpty(node.DefaultBtnId) == false)
            Managers.Quest.CheckButtonClick(node.DefaultBtnId);

        BuildDetailButtons();
        RefreshAll();
    }

    void OnClickDetailNode(int dataId)
    {
        if (TryGetNode(dataId, out TravelAreaData node) == false)
            return;

        // 현재 플레이어가 있는 장소는 Interactive=X (슬라이드 46 스펙)
        if (dataId == GetEffectiveCurrentAreaId())
            return;

        if (IsSelectableNode(node) == false)
            return;

        if (TryGetSelectionBlockReason(node, out string failMessage))
        {
            Debug.LogWarning($"[UI_TravelPopup] Detail selection blocked: node={dataId}, reason={failMessage}");
            _selectionFeedbackMessage = failMessage;
            RefreshAll();
            return;
        }

        _selectionFeedbackMessage = string.Empty;
        _selectedDetailId = node.DataId;

        if (string.IsNullOrEmpty(node.DefaultBtnId) == false)
            Managers.Quest.CheckButtonClick(node.DefaultBtnId);

        RefreshAll();
    }

    void OnClickEnterSelectedDetail()
    {
        if (_isEntering)
        {
            Debug.LogWarning("[UI_TravelPopup] Enter blocked: transition already in progress.");
            return;
        }

        if (TryGetValidatedSelectedDetail(out TravelAreaData detail, out string failMessage) == false)
        {
            if (string.IsNullOrEmpty(failMessage) == false)
            {
                Debug.LogWarning($"[UI_TravelPopup] Enter blocked: {failMessage}");
                _selectionFeedbackMessage = failMessage;
                RefreshAll();
                Managers.UI.ShowToast(failMessage);
            }
            return;
        }

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        if (gameScene == null)
        {
            Debug.LogWarning("[UI_TravelPopup] Enter blocked: UI_GameScene is not active.");
            return;
        }

        if (TryResolveAreaType(detail, out AreaType areaType) == false)
        {
            string message = ResolveText("TravelAreaSceneMissingText");
            Debug.LogWarning($"[UI_TravelPopup] Enter blocked: invalid SceneName for travel node {detail.DataId}.");
            _selectionFeedbackMessage = message;
            RefreshAll();
            Managers.UI.ShowToast(message);
            return;
        }

        if (gameScene.CanEnterAreaTransition(areaType) == false)
        {
            string message = ResolveText("TravelAreaSceneMissingText");
            Debug.LogWarning($"[UI_TravelPopup] Enter blocked: area transition validation failed for {areaType}.");
            _selectionFeedbackMessage = message;
            RefreshAll();
            Managers.UI.ShowToast(message);
            return;
        }

        _isEntering = true;
        _selectionFeedbackMessage = string.Empty;

        if (string.IsNullOrEmpty(detail.EntryBtnId) == false)
            Managers.Quest.CheckButtonClick(detail.EntryBtnId);

        if (gameScene.EnterAreaWithFade(areaType) == false)
        {
            _isEntering = false;
            Debug.LogWarning($"[UI_TravelPopup] Enter blocked: EnterAreaWithFade rejected for {areaType}.");
            return;
        }

        Managers.Game.CurrentTravelAreaId = detail.DataId;
        Managers.Quest.CheckButtonClick(BTN_JOURNEY);
        Debug.Log($"[UI_TravelPopup] Enter accepted: node={detail.DataId}, area={areaType}");
        Managers.UI.CloseAllPopupUI();
    }

    void OnClickBack()
    {
        _selectionFeedbackMessage = string.Empty;

        if (_depth == TravelDepth.Detail)
        {
            _depth = TravelDepth.Category;
            _selectedDetailId = 0;
            RefreshAll();
            return;
        }

        if (_depth == TravelDepth.Category)
        {
            _depth = TravelDepth.Root;
            RefreshAll();
            return;
        }

        ClosePopupUI();
    }

    List<TravelAreaData> GetGroupNodesForCategory(string category)
    {
        TravelAreaData root = GetRootNode(category);
        if (root == null)
            return new List<TravelAreaData>();

        return _allNodes
            .Where(node => node.Depth == 2 && node.ParentId == root.DataId)
            .OrderBy(node => node.Order)
            .ThenBy(node => node.DataId)
            .ToList();
    }

    List<TravelAreaData> GetDetailNodes(int groupId)
    {
        if (groupId == 0)
            return new List<TravelAreaData>();

        return _allNodes
            .Where(node => node.Depth == 3 && node.ParentId == groupId)
            .OrderBy(node => node.Order)
            .ThenBy(node => node.DataId)
            .ToList();
    }

    TravelAreaData GetRootNode(string category)
    {
        return _rootNodes.FirstOrDefault(node => string.Equals(node.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    TravelAreaData GetFirstUnlockedGroup(int rootId)
    {
        return _allNodes
            .Where(node => node.Depth == 2 && node.ParentId == rootId)
            .OrderBy(node => node.Order)
            .ThenBy(node => node.DataId)
            .FirstOrDefault(node => TryGetSelectionBlockReason(node, out _) == false);
    }

    void RestoreSelection()
    {
        // 팝업은 항상 Root depth (대분류만 보이는 초기 화면)로 시작. 슬라이드 45 스펙.
        _selectedRootId = GetPreferredRootId();
        _selectedGroupId = 0;
        _selectedDetailId = 0;
        _depth = TravelDepth.Root;
    }

    int GetPreferredRootId()
    {
        int savedAreaId = Managers.Game.CurrentTravelAreaId;
        if (savedAreaId > 0 && TryGetNode(savedAreaId, out TravelAreaData savedNode))
        {
            if (savedNode.Depth == 1)
                return savedNode.DataId;

            if (savedNode.RootId != 0)
                return savedNode.RootId;
        }

        TravelAreaData town = GetRootNode("TOWN");
        if (town != null)
            return town.DataId;

        TravelAreaData explore = GetRootNode("EXPLORE");
        if (explore != null)
            return explore.DataId;

        return _rootNodes.FirstOrDefault()?.DataId ?? 0;
    }

    bool TryGetNode(int dataId, out TravelAreaData node)
    {
        return Managers.Data.TravelAreaDic.TryGetValue(dataId, out node);
    }

    bool IsTownRoot(int dataId)
    {
        return TryGetNode(dataId, out TravelAreaData node) &&
               string.Equals(node.Category, "TOWN", StringComparison.OrdinalIgnoreCase);
    }

    bool IsExploreRoot(int dataId)
    {
        return TryGetNode(dataId, out TravelAreaData node) &&
               string.Equals(node.Category, "EXPLORE", StringComparison.OrdinalIgnoreCase);
    }

    bool IsSelectableNode(TravelAreaData node)
    {
        if (node == null)
            return false;

        if (node.IsSelectable != 0)
            return true;

        return node.Depth <= 2;
    }

    bool IsEnterableNode(TravelAreaData node)
    {
        if (node == null)
            return false;

        if (node.IsEnterable != 0)
            return true;

        return node.Depth == 3;
    }

    bool IsNodeUnlocked(int dataId, out string failMessage)
    {
        failMessage = string.Empty;

        if (Managers.Data.TravelAreaConditionDic.TryGetValue(dataId, out List<TravelAreaConditionData> conditions) == false || conditions.Count == 0)
            return true;

        foreach (TravelAreaConditionData condition in conditions)
        {
            if (EvaluateCondition(condition))
                continue;

            failMessage = ResolveText(condition.FailMessage);
            return false;
        }

        return true;
    }

    bool TryGetSelectionBlockReason(TravelAreaData node, out string failMessage)
    {
        failMessage = string.Empty;

        if (node == null)
            return true;

        if (node.Depth == 3 && TryGetParentGroup(node, out TravelAreaData parentGroup))
        {
            if (IsNodeUnlocked(parentGroup.DataId, out failMessage) == false)
                return true;
        }

        if (IsNodeUnlocked(node.DataId, out failMessage) == false)
            return true;

        return false;
    }

    bool TryGetParentGroup(TravelAreaData node, out TravelAreaData parentGroup)
    {
        parentGroup = null;
        if (node == null || node.Depth != 3)
            return false;

        return TryGetNode(node.ParentId, out parentGroup) && parentGroup != null && parentGroup.Depth == 2;
    }

    bool CanEnterNode(TravelAreaData node, out string failMessage)
    {
        failMessage = string.Empty;

        if (node == null)
        {
            failMessage = "[Travel] Detail node is null.";
            return false;
        }

        if (IsSelectableNode(node) == false || IsEnterableNode(node) == false)
        {
            failMessage = "[Travel] Selected detail node is not enterable.";
            return false;
        }

        if (TryGetSelectionBlockReason(node, out failMessage))
            return false;

        if (TryResolveAreaType(node, out _) == false)
        {
            failMessage = ResolveText("TravelAreaSceneMissingText");
            return false;
        }

        return true;
    }

    bool EvaluateCondition(TravelAreaConditionData condition)
    {
        if (condition == null)
            return true;

        string conditionType = condition.ConditionType?.Trim().ToUpperInvariant() ?? "NONE";
        switch (conditionType)
        {
            case "NONE":
                return true;

            case "QUEST_CLEAR":
                return int.TryParse(condition.Target, out int questId) && Managers.Quest.CurrentQuestID > questId;

            case "BTN_UNLOCKED":
                return string.IsNullOrEmpty(condition.Target) == false && Managers.Game.UnlockedBtnIds.Contains(condition.Target);

            case "ITEM_REQUIRED":
                return int.TryParse(condition.Target, out int itemId) && Managers.Game.GetItemCount(itemId) >= Mathf.Max(1, condition.Value);

            case "LEVEL_REQUIRED":
                return Managers.Game.Level >= Mathf.Max(1, condition.Value);

            default:
                return false;
        }
    }

    bool HasNotifyInSubtree(int dataId)
    {
        if (HasNotifyForNode(dataId))
            return true;

        foreach (TravelAreaData child in _allNodes.Where(node => node.ParentId == dataId))
        {
            if (HasNotifyInSubtree(child.DataId))
                return true;
        }

        return false;
    }

    bool TryApplySavedSelection(TravelAreaData savedNode)
    {
        if (savedNode == null)
            return false;

        if (savedNode.Depth == 1)
        {
            _selectedRootId = savedNode.DataId;
            return true;
        }

        if (TryGetNode(savedNode.RootId, out TravelAreaData rootNode) == false || rootNode.Depth != 1)
            return false;

        _selectedRootId = rootNode.DataId;

        if (savedNode.Depth == 2)
        {
            if (IsSelectableNode(savedNode) == false || TryGetSelectionBlockReason(savedNode, out _) == true)
                return false;

            _selectedGroupId = savedNode.DataId;
            _depth = TravelDepth.Category;
            return true;
        }

        if (savedNode.Depth != 3)
            return false;

        if (TryGetNode(savedNode.ParentId, out TravelAreaData groupNode) == false || groupNode.Depth != 2)
            return false;

        if (groupNode.ParentId != rootNode.DataId || IsSelectableNode(groupNode) == false)
            return false;

        _selectedGroupId = groupNode.DataId;
        _depth = TravelDepth.Category;

        if (CanEnterNode(savedNode, out _) == false)
            return true;

        _selectedDetailId = savedNode.DataId;
        _depth = TravelDepth.Detail;
        return true;
    }

    bool TryGetValidatedSelectedDetail(out TravelAreaData detail, out string failMessage)
    {
        failMessage = string.Empty;
        detail = null;

        if (TryGetNode(_selectedDetailId, out detail) == false)
        {
            failMessage = "[Travel] Selected detail node is invalid.";
            return false;
        }

        return CanEnterNode(detail, out failMessage);
    }

    bool HasNotifyForNode(int dataId)
    {
        List<QuestNotifyData> currentNotifyList = Managers.Quest.GetCurrentNotifyList();

        if (TryGetNode(dataId, out TravelAreaData node) == false)
            return false;

        if (currentNotifyList.Exists(n => n.BtnID == node.DefaultBtnId || n.BtnID == node.EntryBtnId))
            return true;

        if (Managers.Data.TravelAreaNotifyDic.TryGetValue(dataId, out List<TravelAreaNotifyData> notifies) == false)
            return false;

        foreach (TravelAreaNotifyData notify in notifies)
        {
            if (currentNotifyList.Exists(item => item.BtnID == notify.BtnId))
                return true;
        }

        return false;
    }

    bool HasEntryNotify(TravelAreaData node)
    {
        if (node == null || string.IsNullOrEmpty(node.EntryBtnId))
            return false;

        return Managers.Quest.GetCurrentNotifyList().Exists(n => n.BtnID == node.EntryBtnId);
    }

    void NormalizeSelectionState()
    {
        bool changed = false;

        if (_selectedRootId != 0 && TryGetNode(_selectedRootId, out TravelAreaData rootNode) == false)
        {
            _selectedRootId = GetPreferredRootId();
            _selectedGroupId = 0;
            _selectedDetailId = 0;
            _depth = TravelDepth.Root;
            changed = true;
        }

        if (_selectedGroupId != 0)
        {
            bool isValidGroup =
                TryGetNode(_selectedGroupId, out TravelAreaData selectedGroup) &&
                selectedGroup.Depth == 2 &&
                selectedGroup.ParentId == _selectedRootId &&
                IsSelectableNode(selectedGroup) &&
                TryGetSelectionBlockReason(selectedGroup, out _) == false;

            if (isValidGroup == false)
            {
                _selectedGroupId = 0;
                _selectedDetailId = 0;
                _depth = TravelDepth.Root;
                changed = true;
            }
        }

        if (_selectedDetailId != 0)
        {
            bool isValidDetail =
                TryGetNode(_selectedDetailId, out TravelAreaData selectedDetail) &&
                selectedDetail.Depth == 3 &&
                selectedDetail.ParentId == _selectedGroupId &&
                TryGetSelectionBlockReason(selectedDetail, out _) == false;

            if (isValidDetail == false)
            {
                _selectedDetailId = 0;
                _depth = _selectedGroupId != 0 ? TravelDepth.Category : TravelDepth.Root;
                changed = true;
            }
        }

        if (changed)
        {
            _selectionFeedbackMessage = string.Empty;
            Debug.LogWarning($"[UI_TravelPopup] NormalizeSelectionState adjusted selection. root={_selectedRootId}, group={_selectedGroupId}, detail={_selectedDetailId}, depth={_depth}");
        }
    }

    void OnQuestChanged(int _)
    {
        _selectionFeedbackMessage = string.Empty;
        RefreshAll();
    }

    void OnButtonUnlocked(string _)
    {
        _selectionFeedbackMessage = string.Empty;
        RefreshAll();
    }

    void OnInventoryChanged(int _)
    {
        _selectionFeedbackMessage = string.Empty;
        RefreshAll();
    }

    void OnLanguageChanged()
    {
        _selectionFeedbackMessage = string.Empty;
        RefreshStaticTexts();
        RefreshAll();
    }

    string ResolveText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText))
            return string.Empty;

        string localized = Localize(keyOrText);
        return localized == keyOrText ? keyOrText : localized;
    }

    string ResolveNodeName(TravelAreaData node, string fallbackKey)
    {
        return node == null ? ResolveText(fallbackKey) : ResolveText(node.Name);
    }

    string ResolveNodeSummary(TravelAreaData node, string fallbackKeyOrText)
    {
        if (node == null)
            return ResolveText(fallbackKeyOrText);

        if (string.IsNullOrEmpty(node.UnlockSummary) == false)
            return ResolveText(node.UnlockSummary);

        if (string.IsNullOrEmpty(node.Description) == false)
            return ResolveText(node.Description);

        return ResolveText(fallbackKeyOrText);
    }

    string ResolveCategoryInfo(TravelAreaData root, TravelAreaData group)
    {
        if (group != null && IsNodeUnlocked(group.DataId, out string failMessage) == false && string.IsNullOrEmpty(failMessage) == false)
            return failMessage;

        return ResolveNodeSummary(group ?? root, root?.Description);
    }

    bool TryResolveAreaType(TravelAreaData node, out AreaType areaType)
    {
        areaType = default;
        if (node == null)
        {
            Debug.LogWarning("[UI_TravelPopup] TryResolveAreaType failed: node is null.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(node.SceneName))
        {
            Debug.LogWarning($"[UI_TravelPopup] TryResolveAreaType failed: SceneName is empty. node={node.DataId}, category={node.Category}");
            return false;
        }

        if (Enum.TryParse(node.SceneName, true, out areaType))
            return true;

        Debug.LogWarning($"[UI_TravelPopup] TryResolveAreaType failed: invalid SceneName '{node.SceneName}'. node={node.DataId}, category={node.Category}");
        return false;
    }

    void RebindClickEvent(GameObject target, Action action)
    {
        if (target == null)
            return;

        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(target);
        evt.OnClickHandler = null;
        Transform tr = target.transform;
        Button btn = target.GetComponent<Button>();
        evt.OnClickHandler += () =>
        {
            if (btn != null && btn.interactable == false)
                return;
            PlayButtonYoyo(tr);
            action?.Invoke();
        };
    }

    void SetText(Texts textId, string key)
    {
        TMP_Text text = GetText((int)textId);
        if (text != null)
            text.text = ResolveText(key);
    }

    void SetRawText(Texts textId, string value)
    {
        TMP_Text text = GetText((int)textId);
        if (text != null)
            text.text = value ?? string.Empty;
    }

    void SetPanelActive(string panelName, bool active)
    {
        GameObject panel = FindChild(gameObject, panelName, true);
        if (panel != null)
            panel.SetActive(active);
    }
}
