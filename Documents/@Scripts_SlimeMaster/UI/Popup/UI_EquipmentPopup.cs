using Data;
using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using Sequence = DG.Tweening.Sequence;

public class UI_EquipmentPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // CharacterImage : 캐릭터 이미지 (애니메이션이라면 변경 필요)
    // AttackValueText : 캐릭터의 최종 공격력 표시 (숫자 변화시 연출 필요)
    // HealthValueText : 캐릭터의 최종 체력 표시 (숫자 변화시 연출 필요)
    // MergeButtonRedDotObject : 합성이 가능하다면 레드닷 출력
    // EquipInventoryObject : 보유하고 있는 장비가 들어갈 부모개체
    // (최대 150개, 인벤토리 여유 공간 없을 시 보상 수령 못하고 인벤토리 경고 팝업 호출)
    // ItemInventoryObject : 보유하고 있는 아이템이 들어갈 부모개체

    // 로컬라이징 텍스트
    // EquipInventoryTlileText : 장비
    // ItemInventoryTlileText : 아이템

    #endregion
    #region Enum
    enum GameObjects
    {
        ContentObject,
        WeaponEquipObject, //무기 장착 시 들어갈 부모개체
        GlovesEquipObject, // 장갑 장착 시 들어갈 부모개체
        RingEquipObject, // 반지 장착 시 들어갈 부모개체
        BeltEquipObject, // 헬멧 장착 시 들어갈 부모개체
        ArmorEquipObject, // 갑옷 장착 시 들어갈 부모개체
        BootsEquipObject, // 부츠 장착 시 들어갈 부모개체
        CharacterRedDotObject,
        MergeButtonRedDotObject,
        EquipInventoryObject,
        ItemInventoryObject,
        EquipInventoryGroupObject,
        ItemInventoryGroupObject,
    }

    enum Buttons
    {
        CharacterButton,
        SortButton,
        MergeButton,
    }

    enum Images
    {
        CharacterImage,
    }

    enum Texts
    {
        AttackValueText,
        HealthValueText,
        SortButtonText,
        MergeButtonText,
        EquipInventoryTlileText,
        ItemInventoryTlileText,
    }
    #endregion
    #region LifeCycle
    private void OnEnable()
    {
        //Managers.Game.EquipInfoChanged += Refresh;
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    private void OnDisable()
    {
        //Managers.Game.EquipInfoChanged -= Refresh;
    }
    #endregion
    [SerializeField]
    public ScrollRect ScrollRect;
    EquipmentSortType _equipmentSortType;

    // 정렬 버튼 텍스트
    string sortText_Level = "정렬 : 레벨";
    string sortText_Grade = "정렬 : 등급";

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetObject((int)GameObjects.CharacterRedDotObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.MergeButtonRedDotObject).gameObject.SetActive(false);
        GetButton((int)Buttons.CharacterButton).gameObject.BindEvent(OnClickCharacterButton);
        GetButton((int)Buttons.CharacterButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.CharacterButton).gameObject.SetActive(false); // 출시때 제외

        GetButton((int)Buttons.SortButton).gameObject.BindEvent(OnClickSortButton);
        GetButton((int)Buttons.SortButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.MergeButton).gameObject.BindEvent(OnClickMergeButton);
        GetButton((int)Buttons.MergeButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 정렬 기준 디폴트
        _equipmentSortType = EquipmentSortType.Level;
        GetText((int)Texts.SortButtonText).text = sortText_Level;

        //GetObject((int)GameObjects.RandomGenButton).BindEvent(() =>
        //{
        //    Managers.Sound.PlayButtonClick();
        //    Managers.Game.GenerateRandomEquipment();
        //    Refresh();
        //});

#if UNITY_EDITOR
        // 테스트용
        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    public void SetInfo()
    {
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;

        #region 초기화
        GameObject WeaponContainer = GetObject((int)GameObjects.WeaponEquipObject);
        GameObject GlovesContainer = GetObject((int)GameObjects.GlovesEquipObject);
        GameObject RingContainer = GetObject((int)GameObjects.RingEquipObject);
        GameObject BeltContainer = GetObject((int)GameObjects.BeltEquipObject);
        GameObject ArmorContainer = GetObject((int)GameObjects.ArmorEquipObject);
        GameObject BootsContainer = GetObject((int)GameObjects.BootsEquipObject);

        WeaponContainer.DestroyChilds();
        GlovesContainer.DestroyChilds();
        RingContainer.DestroyChilds();
        BeltContainer.DestroyChilds();
        ArmorContainer.DestroyChilds();
        BootsContainer.DestroyChilds();

        #endregion
        #region 장비
        //1. 장비 리스트를 불러와서 장비인벤토리에 추가
        foreach (Equipment item in Managers.Game.OwnedEquipments)
        {
            //착용중인장비 
            if (item.IsEquipped)
            {
                switch (item.EquipmentData.EquipmentType)
                {
                    case Define.EquipmentType.Weapon:
                        UI_EquipItem weapon = Managers.UI.MakeSubItem<UI_EquipItem>(WeaponContainer.transform);
                        weapon.SetInfo(item, Define.UI_ItemParentType.CharacterEquipmentGroup, ScrollRect);
                        break;
                    case Define.EquipmentType.Boots:
                        UI_EquipItem boots = Managers.UI.MakeSubItem<UI_EquipItem>(BootsContainer.transform);
                        boots.SetInfo(item, Define.UI_ItemParentType.CharacterEquipmentGroup, ScrollRect);
                        break;
                    case Define.EquipmentType.Ring:
                        UI_EquipItem ring = Managers.UI.MakeSubItem<UI_EquipItem>(RingContainer.transform);
                        ring.SetInfo(item, Define.UI_ItemParentType.CharacterEquipmentGroup, ScrollRect);
                        break;
                    case Define.EquipmentType.Belt:
                        UI_EquipItem belt = Managers.UI.MakeSubItem<UI_EquipItem>(BeltContainer.transform);
                        belt.SetInfo(item, Define.UI_ItemParentType.CharacterEquipmentGroup, ScrollRect);
                        break;
                    case Define.EquipmentType.Armor:
                        UI_EquipItem armor = Managers.UI.MakeSubItem<UI_EquipItem>(ArmorContainer.transform);
                        armor.SetInfo(item, Define.UI_ItemParentType.CharacterEquipmentGroup, ScrollRect);
                        break;
                    case Define.EquipmentType.Gloves:
                        UI_EquipItem gloves = Managers.UI.MakeSubItem<UI_EquipItem>(GlovesContainer.transform);
                        gloves.SetInfo(item, Define.UI_ItemParentType.CharacterEquipmentGroup, ScrollRect);
                        break;
                }
            }
        }
        SortEquipments();
        #endregion

        #region 캐릭터

        // 공격력,HP 설정
        var (hp, attack) = Managers.Game.GetCurrentChracterStat();
        GetText((int)Texts.AttackValueText).text = (Managers.Game.CurrentCharacter.Atk + attack).ToString();
        GetText((int)Texts.HealthValueText).text = (Managers.Game.CurrentCharacter.MaxHp + hp).ToString();

        #endregion

        SetItem();

        // 리프레시 버그 대응ItemInventoryObject 
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.EquipInventoryObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.ItemInventoryObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.EquipInventoryGroupObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.ItemInventoryGroupObject).GetComponent<RectTransform>());

        
    }


    void OnClickCharacterButton() // 캐릭터 버튼
    {
        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_CharacterSelectPopup>();
    }


    void OnClickSortButton() // 정렬 버튼
    {
        Managers.Sound.PlayButtonClick();

        // 레벨로 정렬, 등급으로 정렬 누를때마다 정렬방식 변경
        if (_equipmentSortType == EquipmentSortType.Level)
        {
            _equipmentSortType = EquipmentSortType.Grade;
            GetText((int)Texts.SortButtonText).text = sortText_Grade;
        }

        else if (_equipmentSortType == EquipmentSortType.Grade)
        {
            _equipmentSortType = EquipmentSortType.Level;
            GetText((int)Texts.SortButtonText).text = sortText_Level;
        }

        SortEquipments();
    }

    void OnClickMergeButton() // 합성 버튼
    {
        Managers.Sound.PlayButtonClick();
        UI_MergePopup mergePopupUI = (Managers.UI.SceneUI as UI_LobbyScene).MergePopupUI;
        mergePopupUI.SetInfo(null);
        mergePopupUI.gameObject.SetActive(true);
    }

    void SortEquipments()
    {
        Managers.Game.SortEquipment(_equipmentSortType);

        GetObject((int)GameObjects.EquipInventoryObject).DestroyChilds();

        foreach (Equipment item in Managers.Game.OwnedEquipments)
        {
            if (item.IsEquipped)
                continue;

            UI_EquipItem popup = Managers.Resource.Instantiate("UI_EquipItem", GetObject((int)GameObjects.EquipInventoryObject).transform, true).GetOrAddComponent<UI_EquipItem>();

            popup.transform.SetParent(GetObject((int)GameObjects.EquipInventoryObject).transform);
            popup.SetInfo(item, Define.UI_ItemParentType.EquipInventoryGroup, ScrollRect);
        }
    }

    public void SetItem()
    {
        GameObject Container = GetObject((int)GameObjects.ItemInventoryObject);
        Container.DestroyChilds();

         foreach (int id in Managers.Game.ItemDictionary.Keys)
        {
            if (Managers.Data.MaterialDic.TryGetValue(id, out MaterialData material) == true)
            {
                UI_MaterialItem item = Managers.UI.MakeSubItem<UI_MaterialItem>(Container.transform);
                int count = Managers.Game.ItemDictionary[id];
                
                item.SetInfo(material, transform, count, ScrollRect);
            }
        }
    }

}
