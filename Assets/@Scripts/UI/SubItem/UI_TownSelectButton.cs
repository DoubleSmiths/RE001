using Data;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_TownSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        TownSelectButtonIconFrame,
        TownSelectButtonIcon,
        TownSelectButtonQuestVFXIcon,
        EFrame,
    }

    enum Texts
    {
        TownSelectButtonText,
        TownSelectPopupCurrentLocationText,
    }

    enum Buttons
    {
        //UI_TownSelectButton,
    }

    enum GameObjects
    {
        TownSelectButtonQuestVFX,
        TownSelectPopupActiveEf,
    }

    #endregion

    static readonly Color COLOR_CURRENT = new Color32(0x23, 0x7B, 0x00, 0xFF);          // 237B00 해당 버튼의 장소가 현재 플레이어가 있는 장소일 시
    static readonly Color COLOR_GROUP_SELECT = new Color32(0xFF, 0xC5, 0x00, 0xFF);     // FFC500 선택 되었을 때
    static readonly Color COLOR_BUTTON_DEFAULT = new Color32(0x95, 0x95, 0x95, 0xFF);   // 959595 클릭 가능 버튼들의 기본 색상

    int _nodeId;
    bool _isCurrent = false;
    public int NodeId => _nodeId;
    public Action<int> OnClickTownSelect;

    private void Awake()
    {
        Init();
    }

    override public bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        gameObject.BindEvent(OnClickTownSelectButton);

        // TODO : 버튼 클릭 이벤트 추가
        // TODO : 퀘스트 VFX 활성화/비활성화 로직 추가
        // TODO : 현재 위치 텍스트 업데이트 로직 추가
        // TODO : 타운 선택시만 EFrame 켜기
        // TODO : 한/영 텍스트 업데이트 로직 추가
        // TODO : 타운 선택 팝업 활성화/비활성화 로직 추가
        // TODO : 타운 선택 버튼 아이콘 업데이트 로직 추가
        // TODO : 타운 선택 버튼 프레임 업데이트 로직 추가

        return true;
    }

    public void SetInfo(TravelNodeData node, bool isCurrentLocation)
    {
        Init();

        if (node == null)
        {
            Debug.LogError("[UI_TownSelectButton] Invalid TravelNodeData.");
            return;
        }

        Debug.Log($"[UI_TownSelectButton] Setting info for node: {node.Name}");

        _nodeId = node.Id;
        _isCurrent = isCurrentLocation;
        GetText((int)Texts.TownSelectButtonText).text = Util.Localize(node.Name);

        // TODO : 버튼 클릭 이벤트 추가
        // TODO : 퀘스트 VFX 활성화/비활성화 로직 추가
        // TODO : 현재 위치 텍스트 업데이트 로직 추가

        SetCurrentLocation(isCurrentLocation);
        SetSelected(false);
        SetQuestVFX(false); // 일단 false
    }

    void OnClickTownSelectButton()
    {
        if (_isCurrent) return; // 현재 위치인 경우 클릭 무시
        OnClickTownSelect?.Invoke(_nodeId);
    }

    public void SetSelected(bool selected)
    {
        GetImage((int)Images.EFrame)?.gameObject.SetActive(selected);
        if (!_isCurrent) gameObject.GetComponent<Image>().color = selected ? COLOR_GROUP_SELECT : COLOR_BUTTON_DEFAULT;
    }

    public void SetCurrentLocation(bool isCurrent)
    {
        if (isCurrent)
            gameObject.GetComponent<Button>().interactable = false;

        GetText((int)Texts.TownSelectPopupCurrentLocationText)?.gameObject.SetActive(isCurrent);
        //gameObject.GetComponent<Image>().color = isCurrent ? COLOR_CURRENT : COLOR_BUTTON_DEFAULT;
    }


    public void SetQuestVFX(bool active)
    {
        GetObject((int)GameObjects.TownSelectButtonQuestVFX)?.SetActive(active);
    }

}
