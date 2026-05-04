using Data;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_FieldSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        FieldSelectButtonQuestVFXIcon,
        EFrame,
    }

    enum Texts
    {
        FieldSelectButtonText,
    }

    enum Buttons
    {
        //UI_FieldSelectButton,
    }

    enum GameObjects
    {
        FieldSelectButtonQuestVFX,
    }

    #endregion



    int _nodeId;
    public int NodeId { get { return _nodeId; } set { _nodeId = value; } }
    public Action<int> OnClickFieldSelect;

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

        // TODO : 버튼 클릭 이벤트 추가 -> 클릭이된 경우 내부적으로 FieldType이 변경되고, UI_TravelPopup의 ButtonMapSelectGO을 누르면 소그룹 선택이 가능한 UI가 활성화 되는 로직 추가
        // TODO : 버튼 클릭시 버튼 색상 변경 로직 추가 -> 클릭이 된 경우 버튼 색상이 변경되고, 클릭이 안된 경우 원래 색상으로 돌아오는 로직 추가
        // TODO : 퀘스트 VFX 활성화/비활성화 로직 추가
        // TODO : EFrame 활성화/비활성화 로직 추가 -> 이 버튼이 선택이 된 경우 활성화, 선택이 안된 경우 비활성화

        gameObject.BindEvent(OnClickFieldSelectButton);


        return true;
    }

    public void SetInfo(TravelNodeData node)
    {
        Init();
        if (node == null)
        {
            Debug.LogError("[UI_FieldSelectButton] Invalid TravelNodeData.");
            return;
        }

        _nodeId = node.Id;
        GetText((int)Texts.FieldSelectButtonText).text = Util.Localize(node.Name);

        SetSelected(false);
        SetQuestVFX(false);
    }

    void OnClickFieldSelectButton()
    {
        OnClickFieldSelect?.Invoke(_nodeId);
    }

    public void SetSelected(bool selected)
    {
        GetImage((int)Images.EFrame)?.gameObject.SetActive(selected);
    }

    public void SetQuestVFX(bool active)
    {
        GetObject((int)GameObjects.FieldSelectButtonQuestVFX)?.SetActive(active);
    }

}
