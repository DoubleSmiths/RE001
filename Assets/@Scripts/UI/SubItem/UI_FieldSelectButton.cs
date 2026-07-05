using Data;
using System;
using System.Collections.Generic;
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



    TravelNodeData _node;
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

        // TODO : 퀘스트 VFX 활성화/비활성화 로직 추가

        gameObject.BindEvent(OnClickFieldSelectButton);
        gameObject.BindEvent(() => OnClickButtonAnim(gameObject.GetComponent<Button>()));

        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // enum Texts: FieldSelectButtonText — TravelNodeData.Name(localize 키)로 SetInfo에서 set, 폰트만 일괄 적용.
    void RefreshTexts()
    {
        var tmp = GetText((int)Texts.FieldSelectButtonText);
        if (tmp != null) ApplyFont(tmp, Define.FontType.UI);
    }

    public void SetInfo(TravelNodeData node)
    {
        Init();
        if (node == null)
        {
            Debug.LogError("[UI_FieldSelectButton] Invalid TravelNodeData.");
            return;
        }

        _node = node;
        _nodeId = node.Id;

        RefreshLanguage();

        SetSelected(false);
        SetQuestVFX(false);
    }

    public void RefreshLanguage()
    {
        if (_node == null) return;
        var label = GetText((int)Texts.FieldSelectButtonText);
        if (label != null) label.text = Util.Localize(_node.Name);
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

    // 현재 활성 퀘스트 notify 목록(activeIds)에 이 노드의 ClickBtnId/EnterBtnId가 있으면 VFX 표시.
    public void RefreshQuestVFX(HashSet<string> activeIds)
    {
        bool on = _node != null && activeIds != null &&
            ((!string.IsNullOrEmpty(_node.ClickBtnId) && activeIds.Contains(_node.ClickBtnId)) ||
             (!string.IsNullOrEmpty(_node.EnterBtnId) && activeIds.Contains(_node.EnterBtnId)));
        SetQuestVFX(on);
    }

}
