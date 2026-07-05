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
    const string EXPLORE_CATEGORY = "EXPLORE";

    List<TravelNodeData> _allNode = new();
    List<GameObject> _spawnedTownButtons = new();
    List<GameObject> _spawnedFieldButtons = new();
    List<GameObject> _spawnedMapButtons = new();

    int _selectedTownId = 0;
    int _selectedFieldId = 0;
    int _selectedMapId = 0;

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
        GetButton((int)Buttons.TownCategoryButton).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.TownCategoryButton)));
        GetButton((int)Buttons.FieldCategoryButton).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.FieldCategoryButton)));
        GetButton((int)Buttons.ButtonMapSelectGO).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.ButtonMapSelectGO)));
        GetButton((int)Buttons.ButtonMapSelectBack).gameObject.BindEvent(() => OnClickButtonAnim(GetButton((int)Buttons.ButtonMapSelectBack)));

        GameObject popupGo = GetObject((int)GameObjects.MapSelectPopup);
        if (popupGo != null)
            _frameRT = popupGo.GetComponent<RectTransform>();

        // 퀘스트 느낌표(VFX) 갱신: 현재 퀘스트 notify 목록 변경 시 카테고리/목적지 버튼 VFX 재평가
        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged += RefreshQuestNotify;

        if (Managers.Game != null)
        {
        //    Managers.Game.OnItemChanged += OnInventoryChanged;
            Managers.Game.OnLanguageChanged += OnLanguageChanged;
        }

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Managers.Quest != null)
            Managers.Quest.OnNotifyChanged -= RefreshQuestNotify;

        if (Managers.Game != null)
        {
        //    Managers.Game.OnItemChanged -= OnInventoryChanged;
            Managers.Game.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    public void SetInfo()
    {
        _allNode = Managers.Data.TravelNodeDic.Values.OrderBy(n => n.Id).ToList(); // 이 코드 되게 멋있다 기억해놔야지. SQL의 ORDER BY 같은 느낌이네. 데이터가 많아지면 성능 이슈 있을 수 있으니 주의하자.
        Debug.Log($"[Travel] Loaded {_allNode.Count} nodes.");

        _selectedCategory = "";
        _selectedTownId = 0;
        _selectedFieldId = 0;
        _selectedMapId = 0;
        GetImage((int)Images.TownSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.FieldSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.MapSelectPanel).gameObject.SetActive(false);

        BuildTownButtons();
        BuildFieldButtons();

        RefreshStaticTexts();
        RefreshCurrentLocationLabel();
        RefreshUI();
        OpenAnimation();
    }

    void RefreshCurrentLocationLabel()
    {
        int currentId = Managers.Game.CurrentTravelAreaId;
        if (Managers.Data.TravelNodeDic.TryGetValue(currentId, out var node))
            SetText(Texts.MapSelectPopupMapNameText, node.Name);
        else
            SetRawText(Texts.MapSelectPopupMapNameText, string.Empty);
    }

    void RefreshStaticTexts()
    {
        SetRawText(Texts.XbuttonText, "X");
        SetText(Texts.MapSelectPopupTitleText, LocalizeKey.TravelBtnText);
        SetText(Texts.TownCategoryButtonText, LocalizeKey.TownText);
        SetText(Texts.FieldCategoryButtonText, LocalizeKey.ExploreText);
        SetText(Texts.TownSelectPanelText, LocalizeKey.TownText);
        SetText(Texts.FieldSelectText, LocalizeKey.ExploreText);
        SetText(Texts.ButtonMapSelectBackText, LocalizeKey.BackBtnText);
        SetText(Texts.ButtonMapSelectGOText, LocalizeKey.HeadOutText);
    }

    void SetText(Texts textId, string key)
    {
        TMP_Text text = GetText((int)textId);
        if (text != null) text.text = Util.Localize(key);
    }

    void SetRawText(Texts textId, string value)
    {
        TMP_Text text = GetText((int)textId);
        if (text != null) text.text = value ?? string.Empty;
    }

    void OnLanguageChanged()
    {
        RefreshStaticTexts();
        RefreshCurrentLocationLabel();

        // 동적 spawn된 버튼들 텍스트 다시 localize
        foreach (var go in _spawnedTownButtons)
            go?.GetComponent<UI_TownSelectButton>()?.RefreshLanguage();
        foreach (var go in _spawnedFieldButtons)
            go?.GetComponent<UI_FieldSelectButton>()?.RefreshLanguage();
        foreach (var go in _spawnedMapButtons)
            go?.GetComponent<UI_MapSelectButton>()?.RefreshLanguage();

        RefreshUI();
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
        bool isFieldSelected = _selectedCategory == EXPLORE_CATEGORY;

        if (townFrame != null) townFrame.color = isTownSelected ? COLOR_GROUP_SELECT : COLOR_ROOT_IDLE;
        if (fieldFrame != null) fieldFrame.color = isFieldSelected ? COLOR_GROUP_SELECT : COLOR_ROOT_IDLE;

        GetButton((int)Buttons.ButtonMapSelectGO).interactable = CanGo();
        GetButton((int)Buttons.ButtonMapSelectBack).interactable = IsAnyPanelActive();

        foreach (GameObject go in _spawnedTownButtons)
        {
            go?.GetComponent<UI_TownSelectButton>()?.SetSelected(go?.GetComponent<UI_TownSelectButton>().NodeId == _selectedTownId);
        }

        foreach (GameObject go in _spawnedFieldButtons)
        {
            UI_FieldSelectButton btn = go?.GetComponent<UI_FieldSelectButton>();
            if (btn == null) continue;
            //btn.SetSelected(btn.NodeId == _selectedFieldId);
        }

        foreach (GameObject go in _spawnedMapButtons)
        {
            UI_MapSelectButton btn = go?.GetComponent<UI_MapSelectButton>();
            if (btn == null) continue;
            btn.SetSelected(btn.NodeId == _selectedMapId);
        }

        RefreshQuestNotify();
    }

    // 현재 퀘스트 notify 목록 기준으로 여정 팝업의 퀘스트 VFX를 일괄 갱신.
    // - 대분류(탐험) 카테고리 버튼: BTN_EXPLORE
    // - 동적 목적지 버튼: 각 노드의 ClickBtnId 또는 EnterBtnId가 활성 목록에 있으면 표시
    void RefreshQuestNotify()
    {
        HashSet<string> activeIds = new HashSet<string>();
        if (Managers.Quest != null)
            foreach (var n in Managers.Quest.GetCurrentNotifyList())
                activeIds.Add(n.BtnID);

        // 대분류 카테고리 고정 VFX
        GetObject((int)GameObjects.FieldCategoryButtonQuestVFX)?.SetActive(activeIds.Contains(BTN_EXPLORE));
        // TOWN 카테고리는 현재 매핑되는 BtnID 없음 (필요 시 TOWN 유도 BtnID 추가 후 여기서 처리)
        GetObject((int)GameObjects.TownCategoryButtonQuestVFX)?.SetActive(false);

        // 동적 spawn된 목적지 버튼: 노드 ClickBtnId/EnterBtnId 매칭
        foreach (GameObject go in _spawnedTownButtons)
            go?.GetComponent<UI_TownSelectButton>()?.RefreshQuestVFX(activeIds);
        foreach (GameObject go in _spawnedFieldButtons)
            go?.GetComponent<UI_FieldSelectButton>()?.RefreshQuestVFX(activeIds);
        foreach (GameObject go in _spawnedMapButtons)
            go?.GetComponent<UI_MapSelectButton>()?.RefreshQuestVFX(activeIds);
    }

    bool CanGo()
    {
        if (_selectedCategory == TOWN_CATEGORY)
        {
            return _selectedTownId != 0;
        }
        if (_selectedCategory == EXPLORE_CATEGORY)
        {
            return _selectedMapId != 0;
        }

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

        GameObject panel = GetImage((int)Images.TownSelectPanel).gameObject;
        if (!panel.activeSelf)
        {
            // 패널이 열려있지 않다면 열기
            panel.SetActive(true);
            RefreshUI();
            return;
        }
    }

    void OnClickFieldCategory()
    {
        _selectedCategory = EXPLORE_CATEGORY;
        Debug.Log("[UI_TravelPopup] Selected: FIELD");

        GameObject panel = GetImage((int)Images.FieldSelectPanel).gameObject;
        if (!panel.activeSelf)
        {
            // FieldSelectPanel 열기
            panel.SetActive(true);
            RefreshUI();
            return;
        }
    }

    void OnClickGo() // 행선지 이동 시만 
    {
        if (_selectedCategory == TOWN_CATEGORY)
        {
            if (_selectedTownId != 0)
            {
                TravelToNode(_selectedTownId);
                // 행선지로 이동
            }
        }
        else if (_selectedCategory == EXPLORE_CATEGORY)
        {
            // 맵 행선지로 이동
            if (_selectedMapId != 0)
            {
                TravelToNode(_selectedMapId);
            }
        }

        RefreshUI();
    }

    void TravelToNode(int nodeId)
    {
        if (!Managers.Data.TravelNodeDic.TryGetValue(nodeId, out var node))
        {
            Debug.LogWarning($"[Travel] Node {nodeId} not found.");
            return;
        }

        if (!System.Enum.TryParse(node.AreaType, true, out AreaType areaType))
        {
            Debug.LogWarning($"[Travel] Invalid AreaType '{node.AreaType}' for node {nodeId}.");
            return;
        }

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        if (gameScene == null)
        {
            Debug.LogWarning("[Travel] UI_GameScene is not active.");
            return;
        }

        if (!gameScene.CanEnterAreaTransition(areaType))
        {
            Debug.LogWarning($"[Travel] Area transition validation failed for {areaType}.");
            return;
        }

        // 퀘스트 트리거 (입장 버튼 ID)
        if (!string.IsNullOrEmpty(node.EnterBtnId))
            Managers.Quest.CheckButtonClick(node.EnterBtnId);

        // 씬(area) 전환 — UI_GameScene 안의 BtnGroup prefab 교체 + 페이드
        gameScene.EnterAreaWithFade(areaType);

        // 현재 위치 갱신 (저장됨)
        Managers.Game.CurrentTravelAreaId = nodeId;

        // 모든 팝업 닫기 (Travel popup도 같이 닫힘)
        Managers.UI.CloseAllPopupUI();

        Debug.Log($"[Travel] Travel accepted: node={nodeId}, area={areaType}");
    }


    void OnClickBack()
    {
        //GetButton((int)Buttons.ButtonMapSelectBack).interactable = false; // 중복 클릭 방지 위해 바로 비활성화

        if (GetImage((int)Images.MapSelectPanel).gameObject.activeSelf)
        {
            ClosePanelWithAnim(GetImage((int)Images.MapSelectPanel).gameObject);
            _selectedMapId = 0;
            RefreshUI();
            return;
        }

        if (GetImage((int)Images.TownSelectPanel).gameObject.activeSelf || GetImage((int)Images.FieldSelectPanel).gameObject.activeSelf)
        {
            ClosePanelWithAnim(GetImage((int)Images.TownSelectPanel).gameObject);
            ClosePanelWithAnim(GetImage((int)Images.FieldSelectPanel).gameObject);
            _selectedCategory = "";
            _selectedTownId = 0;
            _selectedFieldId = 0;
            _selectedMapId = 0;
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
            Debug.Log($"Current Travel Area ID: {Managers.Game.CurrentTravelAreaId}, Node ID: {node.Id}");
            townBtn.SetInfo(node, Managers.Game.CurrentTravelAreaId == node.Id);
            townBtn.OnClickTownSelect = OnClickTownNode;

            _spawnedTownButtons.Add(btn);
        }

        Debug.Log($"[UI_TravelPopup] Spawned {_spawnedTownButtons.Count} town buttons.");
    }

    void BuildFieldButtons()
    {
        foreach (var go in _spawnedFieldButtons)
            if (go != null) Destroy(go);
        _spawnedFieldButtons.Clear();

        GameObject parent = GetObject((int)GameObjects.FieldSelectContent);
        if (parent == null) return;

        GameObject prefab = Managers.Resource.Load<GameObject>("UI_FieldSelectButton");
        if (prefab == null) return;

        // 현재 위치 노드 찾기 (그룹 자식 매칭용)
        int currentNodeId = Managers.Game.CurrentTravelAreaId;
        Managers.Data.TravelNodeDic.TryGetValue(currentNodeId, out var currentNode);

        var fieldNodes = _allNode
            .Where(n => n.Category == EXPLORE_CATEGORY && n.ParentId == 0)
            .ToList();

        foreach (var node in fieldNodes)
        {
            GameObject btnGo = Instantiate(prefab, parent.transform);
            btnGo.name = $"UI_FieldSelectButton_{node.Id}";

            UI_FieldSelectButton btn = btnGo.GetComponent<UI_FieldSelectButton>();
            if (btn != null)
            {
                btn.SetInfo(node);
                btn.OnClickFieldSelect = OnClickFieldNode;
            }
            _spawnedFieldButtons.Add(btnGo);
        }
    }

    void BuildMapButtons()
    {
        foreach (GameObject go in _spawnedMapButtons)
            if (go != null) Destroy(go);
        _spawnedMapButtons.Clear();

        if (_selectedFieldId == 0) return;   // 그룹 미선택 시 spawn X

        GameObject parent = GetObject((int)GameObjects.MapSelectContent);
        if (parent == null) return;

        GameObject prefab = Managers.Resource.Load<GameObject>("UI_MapSelectButton");
        if (prefab == null)
        {
            Debug.LogError("[UI_TravelPopup] Failed to load UI_MapSelectButton prefab.");
            return;
        }

        int currentNodeId = Managers.Game.CurrentTravelAreaId;

        var mapNodes = _allNode
            .Where(n => n.ParentId == _selectedFieldId)
            .OrderBy(n => n.Id)
            .ToList();

        foreach (var node in mapNodes)
        {
            GameObject btn = Managers.Resource.Instantiate("UI_MapSelectButton", parent.transform);
            btn.name = $"UI_MapSelectButton_{node.Id}";

            UI_MapSelectButton mapBtn = btn.GetComponent<UI_MapSelectButton>();
            if (mapBtn != null)
            {
                bool isCurrent = (currentNodeId == node.Id);
                mapBtn.SetInfo(node, isCurrent);
                mapBtn.OnClickMapSelect = OnClickMapNode;
            }

            _spawnedMapButtons.Add(btn);
        }

        Debug.Log($"[Travel] Spawned {_spawnedMapButtons.Count} map buttons for field {_selectedFieldId}.");
    }


    void OnClickTownNode(int nodeId)
    {
        _selectedTownId = nodeId;
        Debug.Log($"[Travel] Town selected: {nodeId}");
        RefreshUI();
    }

    void OnClickFieldNode(int nodeId)
    {
        _selectedFieldId = nodeId;
        _selectedMapId = 0;   // ← 그룹 바뀌면 행선지 선택 리셋
        Debug.Log($"[Travel] Field selected: {nodeId}");
        BuildMapButtons();    // ← 새 그룹의 행선지로 재구성

        GameObject mapPanel = GetImage((int)Images.MapSelectPanel).gameObject;
        if (!mapPanel.activeSelf)
        {
            // Phase 2: MapSelectPanel 열기 (그룹 선택돼있을 때만)
            if (_selectedFieldId == 0) return;
            mapPanel.SetActive(true);
            RefreshUI();
        }
    }


    void OnClickMapNode(int nodeId)
    {
        _selectedMapId = nodeId;
        Debug.Log($"[Travel] Map selected: {nodeId}");
        RefreshUI();
    }

}
