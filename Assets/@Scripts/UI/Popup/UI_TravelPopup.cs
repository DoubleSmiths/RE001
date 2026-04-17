using System;
using System.Collections.Generic;
using System.Linq;
using Data;
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

        GetButton((int)Buttons.MapSelectBackground)?.gameObject.BindEvent(ClosePopupUI);
        GetButton((int)Buttons.ButtonMapSelectBack_TOP_1)?.gameObject.BindEvent(OnClickBack);
        GetButton((int)Buttons.ButtonMapSelectBack_TOP)?.gameObject.BindEvent(OnClickBack);
        GetButton((int)Buttons.Buttonto_TownType)?.gameObject.BindEvent(() => OnClickRootCategory("TOWN"));
        GetButton((int)Buttons.Buttonto_FieldType)?.gameObject.BindEvent(() => OnClickRootCategory("EXPLORE"));
        GetButton((int)Buttons.MapSelectBWF_EntryBtn)?.gameObject.BindEvent(OnClickEnterSelectedDetail);
        GetButton((int)Buttons.ButtonMapSelectGO)?.gameObject.BindEvent(OnClickEnterSelectedDetail);
        GetButton((int)Buttons.ButtonMapSelectBack)?.gameObject.BindEvent(OnClickBack);

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

        _selectedRootId = GetPreferredRootId();
        _selectedGroupId = 0;
        _selectedDetailId = 0;
        _depth = TravelDepth.Root;

        BuildMiddleButtons();
        BuildDetailButtons();
        RefreshStaticTexts();
        RefreshAll();

        GameObject popupRoot = GetObject((int)GameObjects.MapSelectPopup);
        if (popupRoot != null)
            PopupOpenAnimation(popupRoot);
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
            view.Button.gameObject.BindEvent(() => OnClickDetailNode(capturedId));
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
            view.Button.gameObject.BindEvent(() => clickHandler(capturedId));
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
        RefreshVisiblePanels();
        RefreshRootCategoryButtons();
        RefreshMiddleButtons();
        RefreshDetailButtons();
        RefreshSelectionPanel();
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

        if (_depth == TravelDepth.Root)
        {
            TravelAreaData selectedRoot = TryGetNode(_selectedRootId, out TravelAreaData root) ? root : exploreRoot ?? townRoot;
            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, ResolveNodeSummary(selectedRoot, "CurrentLocationText"));
        }
    }

    void RefreshMiddleButtons()
    {
        foreach (TravelAreaData node in _allNodes.Where(node => node.Depth == 2))
        {
            if (_middleButtonViews.TryGetValue(node.DataId, out TravelNodeButtonView view) == false)
                continue;

            bool unlocked = IsNodeUnlocked(node.DataId, out _);
            bool selected = node.DataId == _selectedGroupId;

            if (view.Text != null)
            {
                view.Text.text = ResolveText(node.Name);
                view.Text.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                view.Text.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            }

            if (view.Frame != null)
                view.Frame.color = selected ? Color.white : new Color(1f, 1f, 1f, unlocked ? 0.9f : 0.45f);

            if (view.Notify != null)
                view.Notify.SetActive(HasNotifyInSubtree(node.DataId));

            if (view.Button != null)
                view.Button.interactable = unlocked && IsSelectableNode(node);

            view.Root.transform.localScale = selected ? new Vector3(1.04f, 1.04f, 1f) : Vector3.one;
        }
    }

    void RefreshDetailButtons()
    {
        foreach (TravelAreaData node in GetDetailNodes(_selectedGroupId))
        {
            if (_detailButtonViews.TryGetValue(node.DataId, out TravelNodeButtonView view) == false)
                continue;

            bool unlocked = IsNodeUnlocked(node.DataId, out _);
            bool selected = node.DataId == _selectedDetailId;

            if (view.Text != null)
            {
                view.Text.text = ResolveText(node.Name);
                view.Text.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                view.Text.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            }

            if (view.Frame != null)
                view.Frame.color = selected ? Color.white : new Color(1f, 1f, 1f, unlocked ? 0.9f : 0.45f);

            if (view.Notify != null)
                view.Notify.SetActive(HasEntryNotify(node));

            if (view.Button != null)
                view.Button.interactable = unlocked && IsSelectableNode(node);

            view.Root.transform.localScale = selected ? new Vector3(1.04f, 1.04f, 1f) : Vector3.one;
        }
    }

    void RefreshSelectionPanel()
    {
        TravelAreaData root = TryGetNode(_selectedRootId, out TravelAreaData rootNode) ? rootNode : null;
        TravelAreaData group = TryGetNode(_selectedGroupId, out TravelAreaData groupNode) ? groupNode : null;
        TravelAreaData detail = TryGetNode(_selectedDetailId, out TravelAreaData detailNode) ? detailNode : null;

        TravelAreaData displayNode = detail ?? group ?? root;
        if (displayNode == null)
            return;

        bool unlocked = IsNodeUnlocked(displayNode.DataId, out string failMessage);
        bool canEnter = detail != null && unlocked && IsEnterableNode(detail);

        SetRawText(Texts.MapSelectPopupMapNameText, ResolveText(displayNode.Name));
        SetRawText(Texts.BWFTypeTitleText, ResolveText(group?.Name ?? detail?.Name ?? root?.Name));

        if (_depth == TravelDepth.Detail)
        {
            string info = unlocked
                ? ResolveNodeSummary(detail ?? group, group?.Description ?? displayNode.Description)
                : string.IsNullOrEmpty(failMessage) ? ResolveNodeSummary(detail ?? group, displayNode.Description) : failMessage;

            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, info);
            SetRawText(Texts.MapSelectBWF_EntryBtnText, canEnter ? ResolveText("HeadOutText") : ResolveText("NextInfoText"));
        }
        else if (_depth == TravelDepth.Category)
        {
            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, ResolveNodeSummary(group ?? root, root?.Description));
            SetRawText(Texts.MapSelectBWF_EntryBtnText, ResolveText("NextInfoText"));
        }
        else
        {
            SetRawText(Texts.MapSelectPopup_CurrentLocationTxt, ResolveNodeSummary(root, "CurrentLocationText"));
        }

        Image mapImage = GetImage((int)Images.MapSelectPopupMapImage);
        if (mapImage != null)
        {
            Sprite sprite = null;
            if (string.IsNullOrEmpty(displayNode.SpriteName) == false)
                sprite = Managers.Resource.Load<Sprite>(displayNode.SpriteName);
            if (sprite != null)
                mapImage.sprite = sprite;
        }

        Button entryButton = GetButton((int)Buttons.MapSelectBWF_EntryBtn);
        if (entryButton != null)
            entryButton.interactable = canEnter;

        GetObject((int)GameObjects.MapSelectBWF_EntryVFX)?.SetActive(detail != null && HasEntryNotify(detail));

        Button goButton = GetButton((int)Buttons.ButtonMapSelectGO);
        if (goButton != null)
            goButton.gameObject.SetActive(false);
    }

    void RefreshVisiblePanels()
    {
        SetPanelActive("PlaceType", true);
        SetPanelActive("TownType", _depth >= TravelDepth.Category && IsTownRoot(_selectedRootId));
        SetPanelActive("FieldType", _depth >= TravelDepth.Category && IsExploreRoot(_selectedRootId));
        SetPanelActive("BWFType", _depth == TravelDepth.Detail);
    }

    void OnClickRootCategory(string category)
    {
        TravelAreaData root = GetRootNode(category);
        if (root == null)
            return;

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

        if (IsNodeUnlocked(node.DataId, out _) == false || IsSelectableNode(node) == false)
            return;

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

        if (IsNodeUnlocked(node.DataId, out _) == false || IsSelectableNode(node) == false)
            return;

        _selectedDetailId = node.DataId;

        if (string.IsNullOrEmpty(node.DefaultBtnId) == false)
            Managers.Quest.CheckButtonClick(node.DefaultBtnId);

        RefreshAll();
    }

    void OnClickEnterSelectedDetail()
    {
        if (TryGetNode(_selectedDetailId, out TravelAreaData detail) == false)
            return;

        if (IsNodeUnlocked(detail.DataId, out _) == false || IsEnterableNode(detail) == false)
            return;

        if (string.IsNullOrEmpty(detail.EntryBtnId) == false)
            Managers.Quest.CheckButtonClick(detail.EntryBtnId);

        Managers.Quest.CheckButtonClick(BTN_JOURNEY);
        Managers.UI.CloseAllPopupUI();

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene?.EnterAreaWithFade(AreaType.Battle);
    }

    void OnClickBack()
    {
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
            .FirstOrDefault(node => IsNodeUnlocked(node.DataId, out _));
    }

    int GetPreferredRootId()
    {
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

    void OnQuestChanged(int _)
    {
        RefreshAll();
    }

    void OnButtonUnlocked(string _)
    {
        RefreshAll();
    }

    void OnInventoryChanged(int _)
    {
        RefreshAll();
    }

    void OnLanguageChanged()
    {
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
