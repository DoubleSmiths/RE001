using Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        MapSelectButtonIconFrame,
        MapSelectButtonIcon,
        MapSelectButtonQuestVFXIcon,
        EFrame,
    }

    enum Texts
    {
        MapSelectButtonText,
    }

    enum Buttons
    {
        //UI_MapSelectButton,
    }

    enum GameObjects
    {
        MapSelectButtonQuestVFX,
    }

    #endregion

    TravelNodeData _node;
    int _nodeId;
    public int NodeId => _nodeId;
    public Action<int> OnClickMapSelect;
    bool _isCurrent;

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

        // TODO : 퀘스트 VFX는 퀘스트가 있을 때만 활성화
        gameObject.BindEvent(OnClickMapSelectButton);
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

    // enum Texts: MapSelectButtonText — 동적 (TravelNodeData.Name), 폰트만.
    void RefreshTexts()
    {
        var tmp = GetText((int)Texts.MapSelectButtonText);
        if (tmp != null) ApplyFont(tmp, Define.FontType.UI);
    }


    public void SetInfo(TravelNodeData node, bool isCurrentLocation)
    {
        Init();
        if (node == null)
        {
            Debug.LogError("[UI_MapSelectButton] Invalid TravelNodeData.");
            return;
        }

        _node = node;
        _nodeId = node.Id;

        // 던전 125곳이 `*_Entry_Img` 3장을 돌려써서 아이콘이 전부 같아 보였다.
        //  노드마다 생성해 둔 Region_{Id} 그림으로 갈아끼운다.
        Util.ApplyNodeSprite(GetImage((int)Images.MapSelectButtonIcon), node);

        RefreshLanguage();

        SetCurrentLocation(isCurrentLocation);
        SetSelected(false);
        SetQuestVFX(false);
    }

    public void RefreshLanguage()
    {
        if (_node == null) return;
        var label = GetText((int)Texts.MapSelectButtonText);
        if (label != null) label.text = Util.Localize(_node.Name);
    }

    void OnClickMapSelectButton()
    {
        if (_isCurrent) return;   // 현재 위치면 클릭 무시
        OnClickMapSelect?.Invoke(_nodeId);
    }

    public void SetSelected(bool selected)
    {
        GetImage((int)Images.EFrame)?.gameObject.SetActive(selected);
    }

    public void SetCurrentLocation(bool isCurrent)
    {
        _isCurrent = isCurrent;
        if (isCurrent)
            gameObject.GetComponent<Button>().interactable = false;
        //gameObject.GetComponent<Image>().color = isCurrent ? COLOR_CURRENT : COLOR_BUTTON_DEFAULT;
    }

    public void SetQuestVFX(bool active)
    {
        GetObject((int)GameObjects.MapSelectButtonQuestVFX)?.SetActive(active);
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
