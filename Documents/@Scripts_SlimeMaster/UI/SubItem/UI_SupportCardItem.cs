using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Data;

public class UI_SupportCardItem : UI_Base
{

    #region UI 기능 리스트
    // 정보 갱신
    // CardNameText : 서포트 스킬 이름
    // SupportSkillImage : 서포트 스킬 아이콘
    // TargetDescriptionText : 서포트 스킬 설명
    // SoulValueText : 서포트 스킬 코스트
    // SoldOutObject : 서포트 카드 구매 완료시 활성화 
    // LockToggle : 토글이 활성화 되었다면 서포트 스킬이 변경되지 않음(잠금을 해제 할때까지 유지)

    // 로컬라이징
    // LockToggleText : 잠금


    #endregion

    #region Enum
    enum GameObjects
    {
        SoldOutObject,
    }

    enum Texts
    {
        CardNameText,
        SoulValueText,
        LockToggleText,
        SkillDescriptionText,
    }

    enum Images
    {
        SupportSkillImage,
        SupportSkillCardBackgroundImage,
        SupportCardTitleImage,
    }
    enum Toggles
    {
        LockToggle,
    }
    #endregion
    Data.SupportSkillData _supportSkilllData;

    private void OnEnable()
    {
    }

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
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindToggle(typeof(Toggles));
        #endregion

        GetToggle((int)Toggles.LockToggle).gameObject.BindEvent(OnClickLockToggle);
        gameObject.BindEvent(OnClickBuy);
        GetToggle((int)Toggles.LockToggle).GetOrAddComponent<UI_ButtonAnimation>();

        return true;
    }
  
    public void SetInfo(Data.SupportSkillData supportSkilll)
    {
        transform.localScale = Vector3.one;
        _supportSkilllData = supportSkilll;
        GetObject((int)GameObjects.SoldOutObject).SetActive(false);

        Refresh();
    }

    void Refresh()
    {
        // CardNameText : 서포트 스킬 이름
        GetText((int)Texts.CardNameText).text = _supportSkilllData.Name;
        // TargetDescriptionText : 서포트 스킬 설명
        GetText((int)Texts.SkillDescriptionText).text = _supportSkilllData.Description;
        // SoulValueText : 서포트 스킬 코스트
        GetText((int)Texts.SoulValueText).text = _supportSkilllData.Price.ToString();
        // SupportSkillImage : 서포트 스킬 아이콘
        GetImage((int)Images.SupportSkillImage).sprite = Managers.Resource.Load<Sprite>(_supportSkilllData.IconLabel);
        // SoldOutObject : 서포트 카드 구매 완료시 활성화 
        GetObject((int)GameObjects.SoldOutObject).SetActive(_supportSkilllData.IsPurchased);
        // LockToggle : 토글이 활성화 되었다면 서포트 스킬이 변경되지 않음(잠금을 해제 할때까지 유지)
        GetToggle((int)Toggles.LockToggle).isOn = _supportSkilllData.IsLocked;

        // 등급에 따라 배경 색상 변경
        switch (_supportSkilllData.SupportSkillGrade)
        {
            case SupportSkillGrade.Common:
                GetImage((int)Images.SupportSkillCardBackgroundImage).color = EquipmentUIColors.Common;
                GetImage((int)Images.SupportCardTitleImage).color = EquipmentUIColors.CommonNameColor;
                break;
            case SupportSkillGrade.Uncommon:
                GetImage((int)Images.SupportSkillCardBackgroundImage).color = EquipmentUIColors.Uncommon;
                GetImage((int)Images.SupportCardTitleImage).color = EquipmentUIColors.UncommonNameColor;
                break;
            case SupportSkillGrade.Rare:
                GetImage((int)Images.SupportSkillCardBackgroundImage).color = EquipmentUIColors.Rare;
                GetImage((int)Images.SupportCardTitleImage).color = EquipmentUIColors.RareNameColor;
                break;
            case SupportSkillGrade.Epic:
                GetImage((int)Images.SupportSkillCardBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.SupportCardTitleImage).color = EquipmentUIColors.EpicNameColor;
                break;
            case SupportSkillGrade.Legend:
                GetImage((int)Images.SupportSkillCardBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.SupportCardTitleImage).color = EquipmentUIColors.LegendaryNameColor;
                break;
            default:
                break;
        }
    }

    void OnClickLockToggle()
    {
        Managers.Sound.PlayButtonClick();
        if (_supportSkilllData.IsPurchased)
            return;
        if (GetToggle((int)Toggles.LockToggle).isOn == true)
        {
        _supportSkilllData.IsLocked = true;
            Managers.Game.Player.Skills.LockedSupportSkills.Add(_supportSkilllData);
        }
        else
        {
            _supportSkilllData.IsLocked = false;
            Managers.Game.Player.Skills.LockedSupportSkills.Remove(_supportSkilllData);
        }
    }

    void OnClickBuy()
    {
        if (GetObject((int)GameObjects.SoldOutObject).activeInHierarchy == true)
            return;
        if (Managers.Game.Player.SoulCount >= _supportSkilllData.Price)
        {
            Managers.Game.Player.SoulCount -= _supportSkilllData.Price;
            
            if(Managers.Game.Player.Skills.LockedSupportSkills.Contains(_supportSkilllData))
                Managers.Game.Player.Skills.LockedSupportSkills.Remove(_supportSkilllData);

            Managers.Game.Player.Skills.AddSupportSkill(_supportSkilllData);
            GetObject((int)GameObjects.SoldOutObject).SetActive(true);
            //구매완료
            GetObject((int)GameObjects.SoldOutObject).SetActive(true);
        }
    }
}
