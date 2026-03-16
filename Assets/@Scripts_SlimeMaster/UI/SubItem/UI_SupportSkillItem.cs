using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static Define;

public class UI_SupportSkillItem : UI_Base
{
    #region Enum
    enum Buttons
    {
        SupportSkillButton,
    }
    enum Images
    {
        SupportSkillImage,
        BackgroundImage,
    }
    #endregion

    SupportSkillData supportSkillData;
    Transform _makeSubItemParents;
    ScrollRect _scrollRect;
    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));

        //GetButton((int)Buttons.SupportSkillButton).gameObject.BindEvent(OnClickMaterialInfoButton, type: Define.UIEvent.Preseed);
        GetButton((int)Buttons.SupportSkillButton).gameObject.BindEvent(OnClickSupportSkillItem);
        GetButton((int)Buttons.SupportSkillButton).gameObject.BindEvent(null, OnDrag, Define.UIEvent.Drag);
        GetButton((int)Buttons.SupportSkillButton).gameObject.BindEvent(null, OnBeginDrag, Define.UIEvent.BeginDrag);
        GetButton((int)Buttons.SupportSkillButton).gameObject.BindEvent(null, OnEndDrag, Define.UIEvent.EndDrag);
        #endregion
        return true;
    }

    public void SeteInfo(Data.SupportSkillData skill, Transform makeSubItemParents, ScrollRect scrollRect)
    {
        transform.localScale = Vector3.one;
        Image img = GetImage((int)Images.SupportSkillImage);
        img.sprite = Managers.Resource.Load<Sprite>(skill.IconLabel);
        supportSkillData = skill;
        _makeSubItemParents = makeSubItemParents;
        _scrollRect = scrollRect;
        // 등급에 따른 배경 색상 변경
        switch (skill.SupportSkillGrade)
        {
            case SupportSkillGrade.Common:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Common;
                break;
            case SupportSkillGrade.Uncommon:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Uncommon;
                break;
            case SupportSkillGrade.Rare:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Rare;
                break;
            case SupportSkillGrade.Epic:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Epic;
                break;
            case SupportSkillGrade.Legend:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Legendary;
                break;
            default:
                break;
        }
    }

    // 툴팁 호출
    void OnClickSupportSkillItem()
    {
        Managers.Sound.PlayButtonClick();
        // UI_ToolTipItem 프리팹 생성
        UI_ToolTipItem item = Managers.UI.MakeSubItem<UI_ToolTipItem>(_makeSubItemParents);
        item.transform.localScale = Vector3.one;
        RectTransform TargetPos = this.gameObject.GetComponent<RectTransform>();
        RectTransform parentsCanvas = _makeSubItemParents.gameObject.GetComponent<RectTransform>();
        item.SetInfo(supportSkillData, TargetPos, parentsCanvas);
        item.transform.SetAsLastSibling();
    }

    #region 버튼 스크롤 대응
    public void OnDrag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnDrag(pointerEventData);
    }

    public void OnBeginDrag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnBeginDrag(pointerEventData);
    }

    public void OnEndDrag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnEndDrag(pointerEventData);
    }
    #endregion
}
