using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_EquipmentInfoPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // EquipmentGradeValueText : 대상 장비의 등급 표시 및 색상을 등급에 맞추어 변경
    // - 일반(Common) : #A2A2A2
    // - 고급(Uncommon)  : #57FF0B
    // - 희귀(Rare) : #2471E0
    // - 유일(Epic) : #9F37F2
    // - 전설(Legendary) : #F67B09
    // - 신화(Myth) : #F1331A
    // EquipmentNameValueText : 대상 장비의 이름
    // EquipmentGradeBackgroundImage : 보상 아이템의 테두리 (색상 변경)
    // - 일반(Common) : #AC9B83
    // - 고급(Uncommon)  : #73EC4E
    // - 희귀(Rare) : #0F84FF
    // - 유일(Epic) : #B740EA
    // - 전설(Legendary) : #F19B02
    // - 신화(Myth) : #FC2302
    // EquipmentLevelValueText : 장비의 레벨 (현재 레벨/최대 레벨)
    // EquipmentOptionImage : 장비 옵션의 아이콘
    // EquipmentOptionValueText : 장비 옵션 수치
    // UncommonSkillOptionDescriptionValueText : 고급 장비 옵션 설명
    // RareSkillOptionDescriptionValueText : 희귀 장비 옵션 설명
    // EpicSkillOptionDescriptionValueText : 유일 장비 옵션 설명
    // LegendarySkillOptionDescriptionValueText : 전설 장비 옵션 설명
    // MythSkillOptionDescriptionValueText : 신화 장비 옵션 설명
    // 만약 장비 데이터 테이블의 각 등급셜 옵션(스킬ID)에 스킬이 없다면 등급에 맞는 옵션 오브젝트 비활성화
    // - 고급(Uncommon)  : UncommonSkillOptionObject
    // - 희귀(Rare) : RareSkillOptionObject
    // - 유일(Epic) : EpicSkillOptionObject
    // - 전설(Legendary) : LegendarySkillOptionObject
    // - 신화(Myth) : MythSkillOptionObject // 제거
    // EquipmentDescriptionValueText : 대상 장비의 설명 텍스트 // 제거
    // CostGoldValueText : 레벨업 비용 (보유 / 필요) 만약 코스트가 부족하다면 보유량을 빨간색(#F3614D)으로 보여준다. 부족하지 않다면 흰색(#FFFFFF)
    // CostMaterialImage : 레벨업 재료의 아이콘
    // CostMaterialValueText : 레벨업 재료 (보유 / 필요) 만약 코스트가 부족하다면 보유량을 빨간색(#F3614D)으로 보여준다. 부족하지 않다면 흰색(#FFFFFF)

    // 로컬라이징 텍스트
    // BackgroundText : 탭하여 닫기
    // EquipmentGradeSkillText : 등급 스킬
    // EquipButtonText : 장착
    // UnequipButtonText : 해제
    // LevelupButtonText : 레벨업
    // MergeButtonText : 합성


    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        UncommonSkillOptionObject,
        RareSkillOptionObject,
        EpicSkillOptionObject,
        LegendarySkillOptionObject,
        EquipmentGradeSkillScrollContentObject,
        ButtonGroupObject,
        CostGoldObject,
        CostMaterialObject,
        LevelupCostGroupObject,
    }

    enum Buttons
    {
        BackgroundButton,
        EquipmentResetButton,
        EquipButton,
        UnquipButton,
        LevelupButton,
        MergeButton,
    }

    enum Texts
    {
        EquipmentGradeValueText,
        EquipmentNameValueText,
        EquipmentLevelValueText,
        EquipmentOptionValueText,
        UncommonSkillOptionDescriptionValueText,
        RareSkillOptionDescriptionValueText,
        EpicSkillOptionDescriptionValueText,
        LegendarySkillOptionDescriptionValueText,
        CostGoldValueText,
        CostMaterialValueText,
        EquipButtonText,
        UnequipButtonText,
        LevelupButtonText,
        MergeButtonText,
        EquipmentGradeSkillText,
        BackgroundText,
        EnforceValueText,
    }

    enum Images
    {
        EquipmentGradeBackgroundImage,
        EquipmentOptionImage,
        CostMaterialImage,
        EquipmentImage,
        GradeBackgroundImage,
        EquipmentEnforceBackgroundImage,
        EquipmentTypeBackgroundImage,
        EquipmentTypeImage,

        UncommonSkillLockImage,
        RareSkillLockImage,
        EpicSkillLockImage,
        LegendarySkillLockImage,
    }
    #endregion

    public Equipment _equipment;

    private void Awake()
    {
        Init();
    }
    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.EquipmentGradeSkillScrollContentObject).GetComponent<RectTransform>());

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

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.EquipmentResetButton).gameObject.BindEvent(OnClickEquipmentResetButton); 
        GetButton((int)Buttons.EquipmentResetButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.EquipButton).gameObject.BindEvent(OnClickEquipButton);
        GetButton((int)Buttons.EquipButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.UnquipButton).gameObject.BindEvent(OnClickUnequipButton);
        GetButton((int)Buttons.UnquipButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.LevelupButton).gameObject.BindEvent(OnClickLevelupButton);
        GetButton((int)Buttons.LevelupButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.MergeButton).gameObject.BindEvent(OnClickMergeButton);
        GetButton((int)Buttons.MergeButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 이미 장착하고 있는 장비라면 EquipButton을 비활성화
        // 장착하고 있지 않는 장비라면 UnequipButton을 비활성화
        #endregion


        return true;
    }

    Action _closeAction;
    public void SetInfo(Equipment equipment)
    {
        _equipment = equipment;
        Refresh();
    }

    void Refresh()
    {
        #region 정보갱신
        GetButton((int)Buttons.EquipButton).gameObject.SetActive(true);
        GetButton((int)Buttons.UnquipButton).gameObject.SetActive(true);
        if (_equipment.IsEquipped == true)
            GetButton((int)Buttons.EquipButton).gameObject.SetActive(false);
        else
            GetButton((int)Buttons.UnquipButton).gameObject.SetActive(false);

        // 장비 레벨이 1이라면 리셋 버튼 비활성화 #Neo
        if (_equipment.Level == 1)
            GetButton((int)Buttons.EquipmentResetButton).gameObject.SetActive(false);
        else
            GetButton((int)Buttons.EquipmentResetButton).gameObject.SetActive(true);

        GetImage((int)Images.EquipmentTypeImage).sprite = Managers.Resource.Load<Sprite>($"{_equipment.EquipmentData.EquipmentType}_Icon.sprite");
        GetImage((int)Images.EquipmentImage).sprite = Managers.Resource.Load<Sprite>(_equipment.EquipmentData.SpriteName);
        
        switch (_equipment.EquipmentData.EquipmentGrade)
        {
            case EquipmentGrade.Common:
                //GetText((int)Texts.EquipmentGradeValueText).text = _equipment.EquipmentData.EquipmentGrade.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "일반";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.CommonNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Common;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Common;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Common;
                break;

            case EquipmentGrade.Uncommon:
                //GetText((int)Texts.EquipmentGradeValueText).text = _equipment.EquipmentData.EquipmentGrade.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "고급";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.UncommonNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Uncommon;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Uncommon;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Uncommon;
                break;

            case EquipmentGrade.Rare:
                //GetText((int)Texts.EquipmentGradeValueText).text = _equipment.EquipmentData.EquipmentGrade.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "희귀";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.RareNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Rare;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Rare;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Rare;
                break;

            case EquipmentGrade.Epic:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Epic.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "에픽";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.EpicNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;
                break;

            case EquipmentGrade.Epic1:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Epic.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "에픽 1";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.EpicNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;
                break;

            case EquipmentGrade.Epic2:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Epic.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "에픽 2";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.EpicNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;
                break;

            case EquipmentGrade.Legendary:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Legendary.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "전설";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                break; 

            case EquipmentGrade.Legendary1:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Legendary.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "전설 1";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                break;

            case EquipmentGrade.Legendary2:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Legendary.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "전설 2";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                break;

            case EquipmentGrade.Legendary3:
                //GetText((int)Texts.EquipmentGradeValueText).text = EquipmentGrade.Legendary.ToString();
                GetText((int)Texts.EquipmentGradeValueText).text = "전설 3";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.GradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                break;

            default:
                break;
        }

        // EquipmentNameValueText : 대상 장비의 이름
        GetText((int)Texts.EquipmentNameValueText).text = _equipment.EquipmentData.NameTextID;
        // EquipmentLevelValueText : 장비의 레벨 (현재 레벨/최대 레벨)
        GetText((int)Texts.EquipmentLevelValueText).text = $"{_equipment.Level}/{_equipment.EquipmentData.MaxLevel}";
        // EquipmentOptionImage : 장비 옵션의 아이콘
        string sprName = _equipment.MaxHpBonus == 0 ? "AttackPoint_Icon.sprite" : "HealthPoint_Icon.sprite";
        GetImage((int)Images.EquipmentOptionImage).sprite = Managers.Resource.Load<Sprite>(sprName);
        // EquipmentOptionValueText : 장비 옵션 수치
        string bonusVale = _equipment.MaxHpBonus == 0 ? _equipment.AttackBonus.ToString() : _equipment.MaxHpBonus.ToString();
        GetText((int)Texts.EquipmentOptionValueText).text = $"+{bonusVale}";

        // CostGoldValueText : 레벨업 비용 (보유 / 필요) 만약 코스트가 부족하다면 보유량을 빨간색(#F3614D)으로 보여준다. 부족하지 않다면 흰색(#FFFFFF)
        if (Managers.Data.EquipLevelDataDic.ContainsKey(_equipment.Level))
        {
            GetText((int)Texts.CostGoldValueText).text = $"{Managers.Data.EquipLevelDataDic[_equipment.Level].UpgradeCost}";
            if (Managers.Game.Gold < Managers.Data.EquipLevelDataDic[_equipment.Level].UpgradeCost)
                GetText((int)Texts.CostGoldValueText).color = Util.HexToColor("F3614D");
            
            GetText((int)Texts.CostMaterialValueText).text = $"{Managers.Data.EquipLevelDataDic[_equipment.Level].UpgradeRequiredItems}";

        }

        // 레벨업 재료의 아이콘 #Neo
        GetImage((int)Images.CostMaterialImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.MaterialDic[_equipment.EquipmentData.LevelupMaterialID].SpriteName);
        #endregion

        #region 유일 +1 등의 등급 벨류
        string gradeName = _equipment.EquipmentData.EquipmentGrade.ToString();
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

        #region 장비스킬 옵션 설정
        // 만약 장비 데이터 테이블의 각 등급셜 옵션(스킬ID)에 스킬이 없다면 등급에 맞는 옵션 오브젝트 비활성화
        GetObject((int)GameObjects.UncommonSkillOptionObject).SetActive(false);
        GetObject((int)GameObjects.RareSkillOptionObject).SetActive(false);
        GetObject((int)GameObjects.EpicSkillOptionObject).SetActive(false);
        GetObject((int)GameObjects.LegendarySkillOptionObject).SetActive(false);

        if (Managers.Data.SupportSkillDic.ContainsKey(_equipment.EquipmentData.UncommonGradeSkill)) // 스킬타입에서 서포트스킬 타입 데이터로 교체 #Neo
        {
            SupportSkillData skillData = Managers.Data.SupportSkillDic[_equipment.EquipmentData.UncommonGradeSkill];
            GetText((int)Texts.UncommonSkillOptionDescriptionValueText).text = $"+{skillData.Description}";
            GetObject((int)GameObjects.UncommonSkillOptionObject).SetActive(true);
        }

        if (Managers.Data.SupportSkillDic.ContainsKey(_equipment.EquipmentData.RareGradeSkill)) 
        {
            SupportSkillData skillData = Managers.Data.SupportSkillDic[_equipment.EquipmentData.RareGradeSkill];
            GetText((int)Texts.RareSkillOptionDescriptionValueText).text = $"+{skillData.Description}";
            GetObject((int)GameObjects.RareSkillOptionObject).SetActive(true);
        }

        if (Managers.Data.SupportSkillDic.ContainsKey(_equipment.EquipmentData.EpicGradeSkill))
        {
            SupportSkillData skillData = Managers.Data.SupportSkillDic[_equipment.EquipmentData.EpicGradeSkill];
            GetText((int)Texts.EpicSkillOptionDescriptionValueText).text = $"+{skillData.Description}";
            GetObject((int)GameObjects.EpicSkillOptionObject).SetActive(true);
        }

        if (Managers.Data.SupportSkillDic.ContainsKey(_equipment.EquipmentData.LegendaryGradeSkill))
        {
            SupportSkillData skillData = Managers.Data.SupportSkillDic[_equipment.EquipmentData.LegendaryGradeSkill];
            GetText((int)Texts.LegendarySkillOptionDescriptionValueText).text = $"+{skillData.Description}";
            GetObject((int)GameObjects.LegendarySkillOptionObject).SetActive(true);
        }
        #endregion

        #region 장비스킬 옵션 색상 설정
        EquipmentGrade equipmentGrade = _equipment.EquipmentData.EquipmentGrade;

        // 공통 색상 변경
        GetText((int)Texts.UncommonSkillOptionDescriptionValueText).color = Util.HexToColor("9A9A9A");
        GetText((int)Texts.RareSkillOptionDescriptionValueText).color = Util.HexToColor("9A9A9A");
        GetText((int)Texts.EpicSkillOptionDescriptionValueText).color = Util.HexToColor("9A9A9A");
        GetText((int)Texts.LegendarySkillOptionDescriptionValueText).color = Util.HexToColor("9A9A9A");

        GetImage((int)Images.UncommonSkillLockImage).gameObject.SetActive(true);
        GetImage((int)Images.RareSkillLockImage).gameObject.SetActive(true);
        GetImage((int)Images.EpicSkillLockImage).gameObject.SetActive(true);
        GetImage((int)Images.LegendarySkillLockImage).gameObject.SetActive(true);

        // 등급별 색상 추가 및 변경
        if (equipmentGrade >= EquipmentGrade.Uncommon)
        {
            GetText((int)Texts.UncommonSkillOptionDescriptionValueText).color = EquipmentUIColors.Uncommon;
            GetImage((int)Images.UncommonSkillLockImage).gameObject.SetActive(false);
        }

        if (equipmentGrade >= EquipmentGrade.Rare)
        {
            GetText((int)Texts.RareSkillOptionDescriptionValueText).color = EquipmentUIColors.Rare;
            GetImage((int)Images.RareSkillLockImage).gameObject.SetActive(false);
        }

        if (equipmentGrade >= EquipmentGrade.Epic)
        {
            GetText((int)Texts.EpicSkillOptionDescriptionValueText).color = EquipmentUIColors.Epic;
            GetImage((int)Images.EpicSkillLockImage).gameObject.SetActive(false);
        }

        if (equipmentGrade >= EquipmentGrade.Legendary)
        {
            GetText((int)Texts.LegendarySkillOptionDescriptionValueText).color = EquipmentUIColors.Legendary;
            GetImage((int)Images.LegendarySkillLockImage).gameObject.SetActive(false);
        }
        #endregion

        #region 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.EquipmentGradeSkillScrollContentObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.ButtonGroupObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.CostGoldObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.CostMaterialObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetText((int)Texts.CostGoldValueText).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetText((int)Texts.CostMaterialValueText).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.LevelupCostGroupObject).GetComponent<RectTransform>());
        #endregion
    }

    void OnClickBackgroundButton() // 배경 버튼 (팝업 닫기)
    {
        Managers.Sound.PlayPopupClose();
        gameObject.SetActive(false);

        //Tap To Close 외부 버튼 누를 때  EuipmentPopup refresh
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentPopupUI.SetInfo();
    }

    void OnClickEquipmentResetButton() // 장비 초기화 팝업 버튼
    {
        Managers.Sound.PlayButtonClick();
        UI_EquipmentResetPopup resetPopup = (Managers.UI.SceneUI as UI_LobbyScene).EquipmentResetPopupUI;
        resetPopup.SetInfo(_equipment);
        resetPopup.gameObject.SetActive(true);
    }

    void OnClickEquipButton() // 장착 버튼
    {
        Managers.Sound.Play(Define.Sound.Effect, "Equip_Equipment");

        // 장비를 장착한다
        Managers.Game.EquipItem(_equipment.EquipmentData.EquipmentType, _equipment);
        Refresh();

        //창닫기
        gameObject.SetActive(false);

        // 버튼 누를 때  EuipmentPopup refresh
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentPopupUI.SetInfo();
    }

    void OnClickUnequipButton() // 장착 해제 버튼
    {
        Managers.Sound.PlayButtonClick();
        // 장비를 장착 헤제한다.
        Managers.Game.UnEquipItem(_equipment);
        Refresh();

        //창닫기
        gameObject.SetActive(false);

        // 버튼 누를 때  EuipmentPopup refresh
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentPopupUI.SetInfo();
    }

    void OnClickLevelupButton() // 레벨업 버튼
    {
        Managers.Sound.PlayButtonClick();

        //장비레벨이 맥스레벨보다 작아야함
        if (_equipment.Level >= _equipment.EquipmentData.MaxLevel)
            return;

        //Cost_Gold, Cost_Material 값 가져오기
     
        int UpgradeCost = Managers.Data.EquipLevelDataDic[_equipment.Level].UpgradeCost;
        int UpgradeRequiredItems = Managers.Data.EquipLevelDataDic[_equipment.Level].UpgradeRequiredItems;

        // 장비의 현재 레벨 
        // _equipment.Level

        //현재 나의 재화
        // TEMP : 재화 임시로 증가시킴 나중에 지우기
        int numMaterial = 0;
        Managers.Game.ItemDictionary.TryGetValue(_equipment.EquipmentData.LevelupMaterialID, out numMaterial);
       
        if (Managers.Game.Gold >= UpgradeCost && numMaterial >= UpgradeRequiredItems)
        {
            _equipment.LevelUp();

            Managers.Game.Gold -= UpgradeCost;

            Managers.Game.RemovMaterialItem(_equipment.EquipmentData.LevelupMaterialID, UpgradeRequiredItems);
            Managers.Sound.Play(Define.Sound.Effect, "Levelup_Equipment");

            Refresh();
        }
        else
        {
            Managers.UI.ShowToast("재화가 부족합니다.");
        }

        // 버튼 누를 때  EuipmentPopup refresh
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentPopupUI.SetInfo();
    }

    void OnClickMergeButton() // 합성 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (_equipment.IsEquipped) return;
        UI_MergePopup mergePopupUI = (Managers.UI.SceneUI as UI_LobbyScene).MergePopupUI;
        mergePopupUI.SetInfo(_equipment);
        mergePopupUI.gameObject.SetActive(true);
    }
}
