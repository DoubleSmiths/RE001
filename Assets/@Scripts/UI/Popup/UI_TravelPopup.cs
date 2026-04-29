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

    #region Enum

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
        Img_to_TownButtonQuestVFX_02,
        AdventureImage,
        Img_to_FieldQuestVFX_01,
        TownType,
        TownTypeFrame,
        TownTypeIcon,
        TownTypeButtons,
        Mask_ScrollView_01,
        FieldType,
        FieldTypeFrame,
        FieldTypeIcon,
        FieldTypeButtons,
        Mask_ScrollView_02,
        Maps,
        MapsFrame,
        MapTypeTitle,
        MapTypeButtons,
        Mask_ScrollView_03,
        BtmGroup,
    }

    enum Texts
    {
        XbuttonText,
        MapSelectPopupTitleText,
        MapSelectPopupMapNameText,
        PlaceTypeTownText,
        AdventrueText,
        TownTypeText,
        FieldTypeText,
        MapTypeTitleText,
        ButtonMapSelectBackText,
        ButtonMapSelectGOText,
    }

    enum Buttons
    {
        MapSelectBackground,
        Buttonto_TownType,
        Buttonto_FieldType,
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
        FieldTypeContent,
        MapTypeContent,
    }

    #endregion

    static readonly Color COLOR_DEFAULT         = new Color32(0xFF, 0xFF, 0xFF, 0xFF);    // FFFFFF
    static readonly Color COLOR_DISABLED        = new Color32(0x5E, 0x5E, 0x5E, 200);     // 5E5E5E alpha 200 그외 또는 [뒤로] 버튼이 눌러졌을 때
    static readonly Color COLOR_DESTINATION     = new Color32(0xFF, 0xA3, 0x00, 0xFF);    // FFA300 행선지가 선택되었을 때
    static readonly Color COLOR_CURRENT         = new Color32(0x23, 0x7B, 0x00, 0xFF);    // 237B00 해당 버튼의 장소가 현재 플레이어가 있는 장소일 시
    static readonly Color COLOR_GROUP_SELECT    = new Color32(0xFF, 0xC5, 0x00, 0xFF);    // FFC500 선택 되었을 때
    static readonly Color COLOR_BUTTON_DEFAULT  = new Color32(0x95, 0x95, 0x95, 0xFF);    // 959595 클릭 가능 버튼들의 기본 색상
    static readonly Color COLOR_ROOT_IDLE       = new Color32(0xFF, 0xFF, 0xFF, 0xFF);    // FFFFFF (대분류 버튼 본체)
    static readonly Color COLOR_ROOT_ICON       = new Color32(0xFF, 0xFF, 0xFF, 80);      // FFFFFF alpha 80 (대분류 아이콘)

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


}
