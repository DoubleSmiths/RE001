using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_EquipItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // EquipmentGradeBackgroundImage : 합성 할 장비 등급의 테두리 (색상 변경)
    // - 일반(Common) : #AC9B83
    // - 고급(Uncommon)  : #73EC4E
    // - 희귀(Rare) : #0F84FF
    // - 유일(Epic) : #B740EA
    // - 전설(Legendary) : #F19B02
    // - 신화(Myth) : #FC2302
    // EquipmentEnforceBackgroundImage : 유일 +1 등급부터 활성화되고 등급에 따라 이미지 색깔 변경
    // - 유일(Epic) : #9F37F2
    // - 전설(Legendary) : #F67B09
    // - 신화(Myth) : #F1331A
    // EnforceValueText : 유일 +1 등의 등급 벨류
    // EquipmentImage : 장비의 아이콘
    // EquipmentLevelValueText : 장비의 현재 레벨
    // EquipmentRedDotObject : 장비가 강화가 가능할때 출력
    // NewTextObject : 장비를 처음 습득했을때 출력
    // EquippedObject : 합성 팝업에서 착용장비 표시용
    // SelectObject : 합성 팝업에서 장비 선택 표시용
    // LockObject : 합성 팝업에서 선택 불가 표시용
    #endregion

    #region Enum
    enum GameObjects
    {
        EquipmentRedDotObject,
        NewTextObject,
        EquippedObject,
        SelectObject,
        LockObject,
        SpecialImage,
        GetEffectObject,
    }

    enum Texts
    {
        EquipmentLevelValueText,
        EnforceValueText,
    }

    enum Images
    {
        EquipmentGradeBackgroundImage,
        EquipmentImage,
        EquipmentEnforceBackgroundImage,
        EquipmentTypeBackgroundImage,
        EquipmentTypeImage,
    }
    #endregion

    public Equipment Equipment;
    public Action OnClickEquipItem;
    ScrollRect _scrollRect;
    UI_ItemParentType _parentType;

    bool _isDrag = false;

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        GetObject((int)GameObjects.GetEffectObject).SetActive(false);
        gameObject.BindEvent(null, OnDrag, Define.UIEvent.Drag);
        gameObject.BindEvent(null, OnBeginDrag, Define.UIEvent.BeginDrag);
        gameObject.BindEvent(null, OnEndDrag, Define.UIEvent.EndDrag);
       
        gameObject.BindEvent(OnClickEquipItemButton);

        return true;
    }

    public void SetInfo(Equipment item, UI_ItemParentType parentType, ScrollRect scrollRect = null)
    {
        Equipment = item;
        transform.localScale= Vector3.one;
        _scrollRect = scrollRect;
        _parentType = parentType;

        #region 색상 변경
        // EquipmentGradeBackgroundImage : 합성 할 장비 등급의 테두리 (색상 변경)
        // EquipmentEnforceBackgroundImage : 유일 +1 등급부터 활성화되고 등급에 따라 이미지 색깔 변경
        switch (Equipment.EquipmentData.EquipmentGrade)
        {
            case EquipmentGrade.Common:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Common;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Common;
                break;

            case EquipmentGrade.Uncommon:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Uncommon;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Uncommon;
                break;

            case EquipmentGrade.Rare:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Rare;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Rare;
                break;

            case EquipmentGrade.Epic:
            case EquipmentGrade.Epic1:
            case EquipmentGrade.Epic2:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;
                break;

            case EquipmentGrade.Legendary:
            case EquipmentGrade.Legendary1:
            case EquipmentGrade.Legendary2:
            case EquipmentGrade.Legendary3:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                break;

            default:
                break;
        }
        #endregion

        #region 유일 +1 등의 등급 벨류
        string gradeName = Equipment.EquipmentData.EquipmentGrade.ToString();
        int num = 0;

        // Epic1 -> 1 리턴 Epic2 ->2 리턴 Common처럼 숫자가 없으면 0 리턴
        Match match = Regex.Match(gradeName, @"\d+$");
        if (match.Success)
            num = int.Parse(match.Value);

        if (num == 0)
        {
            GetText((int)Texts.EnforceValueText).text = "";
            GetImage((int)Images.EquipmentEnforceBackgroundImage).gameObject.SetActive(false);
        }
        else
        { 
            GetText((int)Texts.EnforceValueText).text = num.ToString();
            GetImage((int)Images.EquipmentEnforceBackgroundImage).gameObject.SetActive(true);
        }
        #endregion

        // EquipmentImage : 장비의 아이콘
        Sprite spr = Managers.Resource.Load<Sprite>(Equipment.EquipmentData.SpriteName);
        GetImage((int)Images.EquipmentImage).sprite = spr;
        // 장비 타입 아이콘
        Sprite tpy = Managers.Resource.Load<Sprite>($"{Equipment.EquipmentData.EquipmentType}_Icon.sprite");
        GetImage((int)Images.EquipmentTypeImage).sprite = tpy;
        // EquipmentLevelValueText : 장비의 현재 레벨
        GetText((int)Texts.EquipmentLevelValueText).text = $"Lv.{Equipment.Level}";
        // EquipmentRedDotObject : 장비가 강화가 가능할때 출력 
        GetObject((int)GameObjects.EquipmentRedDotObject).SetActive(Equipment.IsUpgradable);
        // NewTextObject : 장비를 처음 습득했을때 출력
        GetObject((int)GameObjects.NewTextObject).SetActive(!Equipment.IsConfirmed);
        // EquippedObject : 합성 팝업에서 착용장비 표시용
        GameObject obj = GetObject((int)GameObjects.EquippedObject);
        
        GetObject((int)GameObjects.EquippedObject).SetActive(Equipment.IsEquipped);
        // SelectObject : 합성 팝업에서 장비 선택 표시용
        GetObject((int)GameObjects.SelectObject).SetActive(Equipment.IsSelected);
        // LockObject : 합성 팝업에서 선택 불가 표시용
        GetObject((int)GameObjects.LockObject).SetActive(Equipment.IsUnavailable);
        // Special아이템일때 
        bool isSpecial = Equipment.EquipmentData.GachaRarity == GachaRarity.Special ? true : false;
        //케릭터 장착중인 슬롯에 있으면 "착용중" 오브젝트 끔
        GetObject((int)GameObjects.SpecialImage).SetActive(isSpecial);
        if (parentType == UI_ItemParentType.CharacterEquipmentGroup)
        {
            GetObject((int)GameObjects.EquippedObject).SetActive(false);
        }
        // 가챠 결과 팝업 획득 효과 
        if (_parentType == UI_ItemParentType.GachaResultPopup)
        {
            GetObject((int)GameObjects.GetEffectObject).SetActive(true);
        }


    }

    void OnClickEquipItemButton() // 장비 선택 시
    {
        Managers.Sound.PlayButtonClick();
        if (_isDrag) 
            return;
        if (_parentType == UI_ItemParentType.GachaResultPopup)
        {
            Managers.UI.ShowPopupUI<UI_GachaEquipmentInfoPopup>().SetInfo(Equipment);
        }
        else
        {
            Equipment.IsConfirmed = true;
            Managers.Game.SaveGame();

            // 해당 장비의 장비 정보 팝업을 호출
            UI_EquipmentInfoPopup infoPopup = (Managers.UI.SceneUI as UI_LobbyScene).EquipmentInfoPopupUI;
            if (infoPopup != null)
            {
                infoPopup.SetInfo(Equipment);
                infoPopup.gameObject.SetActive(true);
            }
        }
    }

    public void OnDrag(BaseEventData baseEventData)
    {
        if (_parentType == UI_ItemParentType.GachaResultPopup)
            return;
        _isDrag = true;
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnDrag(pointerEventData);
    }

    public void OnBeginDrag(BaseEventData baseEventData)
    {
        if (_parentType == UI_ItemParentType.GachaResultPopup)
            return;

        _isDrag = true;
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnBeginDrag(pointerEventData);
    }

    public void OnEndDrag(BaseEventData baseEventData)
    {
        if (_parentType == UI_ItemParentType.GachaResultPopup)
            return;
        _isDrag = false;
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnEndDrag(pointerEventData);
    }
}
