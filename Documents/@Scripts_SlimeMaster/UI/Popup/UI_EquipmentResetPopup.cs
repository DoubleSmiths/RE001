using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;
using static Define;

public class UI_EquipmentResetPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // TargetEquipment ( 초기화 할 장비 )
    //      TargetEquipmentGradeBackgroundImage : : 대상 장비의 등급 표시 및 색상을 등급에 맞추어 변경
    //          - 일반(Common) : #A2A2A2
    //          - 고급(Uncommon)  : #57FF0B
    //          - 희귀(Rare) : #2471E0
    //          - 유일(Epic) : #9F37F2
    //          - 전설(Legendary) : #F67B09
    //          - 신화(Myth) : #F1331A
    //      TargetEquipmentImage : 대상 장비 아이콘
    //      TargetEquipmentLevelValueText : 대상 장비 레벨
    //      TargetEquipmentEnforceBackgroundImage : 유일 +1 등급부터 활성화되고 등급에 따라 컬러 변경
    //          - 유일(Epic) : #B740EA
    //          - 전설(Legendary) : #F19B02
    //          - 신화(Myth) : #FC2302 
    //      TargetEnforceValueText : 등급의 단계

    // ResetInfoGroup
    // ResetResultGroup ( 리턴 장비 )
    //      ResultEquipmentGradeBackgroundImage : 초기화된 장비의 등급 표시 및 색상을 등급에 맞추어 변경
    //      ResultEquipmentImage : 리턴 장비 아이콘
    //      ResultEquipmentEnforceBackgroundImage : 유일 +1 등급부터 활성화되고 등급에 따라 컬러 변경
    //      ResultEnforceValueText : 등급의 벨류
    // ResultGold ( 리턴 골드 )
    //      ResultGoldCountValueText : 리셋 후 돌려받을 골드 수 (강화 단계 역계산)
    // ResultMaterial ( 리턴 재료 )
    //      ResultMaterialCountValueText : 리셋 후 돌려받을 재료 수 (강화 단계 역계산)

    //#new
    // DowngradeTargetEquipment ( 다운그레이드 할 장비 )
    //      DowngradeTargetEquipmentGradeBackgroundImage : 대상 장비의 등급 표시 및 색상을 등급에 맞추어 변경
    //      DowngradeTargetEquipmentImage : : 대상 장비 아이콘
    //      DowngradeTargetEquipmentLevelValueText : 대상 장비 레벨
    //      DowngradeTargetEquipmentEnforceBackgroundImage : 유일 +1 등급부터 활성화되고 등급에 따라 컬러 변경
    //      DowngradeTargetEnforceValueText : 등급의 단계

    // 장비 리셋은 레벨만 초기화
    // 장비 다운 그레이드는 레벨 초기화 + 등급 다운
    // DowngradeResultGroup 
    // DowngradeEquipment ( 다운그레이드 리턴 장비 )
    //      DowngradeEquipmentGradeBackgroundImage : 초기화된 장비의 등급 표시 및 색상을 등급에 맞추어 변경
    //      DowngradeEquipmentImage : 리턴 장비 아이콘
    // DowngradEnchantStone  (리턴 강화석)
    //      DowngradEnchantStoneBackgroundImage : 강화석의 등급 표시 및 색상을 등급에 맞추어 변경
    //      DowngradEnchantStoneImage : 강화석 이미지
    //      DowngradEnchantStoneCountValueText : 강화석 개수
    // DowngradeGold ( 리턴 골드 )
    //      DowngradeResultGoldCountValueText : 리셋 후 돌려받을 골드 수 (강화 단계 역계산)
    // DowngradeMaterial ( 리턴 재료 )
    //      DowngradeResultMaterialCountValueText : 리셋 후 돌려받을 재료 수 (강화 단계 역계산)

    // 로컬라이징
    // BackgroundText : 터치하여 닫기
    // EquipmentResetPopupTitleText : 장비 리셋
    // ResetInfoCommentText : 레벨 초기화 시 레벨업 재료와 골드를 100% 돌려드립니다.
    // DowngradeCommentText : 레벨 초기화 시 레벨업 재료와 골드를 100% 돌려드립니다.
    // EquipmentResetButtonText : 레벨 초기화
    // EquipmentDowngradeButtonText : 다운그레이드
    // ResetTapToggleText : 초기화
    // DowngradeTapToggleText : 다운그레이드


    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        ToggleGroupObject,
        TargetEquipment,

        ResetInfoGroupObject,
        DowngradeGroupObject,
    }

    enum Buttons
    {
        BackgroundButton,
        EquipmentResetButton,
        EquipmentDowngradeButton,
    }

    enum Texts
    {
        BackgroundText,
        EquipmentResetPopupTitleText,
        ResetInfoCommentText,
        DowngradeCommentText,
        EquipmentResetButtonText,
        ResultGoldCountValueText,
        ResultMaterialCountValueText,
        TargetEquipmentLevelValueText,
        TargetEnforceValueText,
        ResultEnforceValueText,

        ResetTapToggleText,
        DowngradeTapToggleText,

        DowngradeTargetEquipmentLevelValueText,
        DowngradeTargetEnforceValueText,
        DowngradEnchantStoneCountValueText,
        DowngradeResultGoldCountValueText,
        DowngradeResultMaterialCountValueText,
        EquipmentDowngradeButtonText,
    }
    enum Images
    {
        TargetEquipmentGradeBackgroundImage,
        TargetEquipmentImage,
        TargetEquipmentEnforceBackgroundImage,
        ResultEquipmentGradeBackgroundImage,
        ResultEquipmentImage,
        ResultEquipmentEnforceBackgroundImage,
        ResultMaterialImage,

        DowngradeTargetEquipmentGradeBackgroundImage,
        DowngradeTargetEquipmentImage,
        DowngradeTargetEquipmentEnforceBackgroundImage,
        DowngradeEquipmentGradeBackgroundImage,
        DowngradeEquipmentImage,
        DowngradEnchantStoneBackgroundImage,
        DowngradEnchantStoneImage,
        DowngradeResultMaterialImage,
    }

    enum Toggles
    {
        ResetTapToggle,
        DowngradeTapToggle,
    }
    #endregion

    bool _isSelectedResetTapTap = false;
    bool _isSelectedDowngradeTapTap = false;
    public Equipment _equipment;

    private void Awake()
    {
        Init();
    }
    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
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
        BindToggle(typeof(Toggles));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.EquipmentResetButton).gameObject.BindEvent(OnClickEquipmentResetButton);
        GetButton((int)Buttons.EquipmentResetButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.EquipmentDowngradeButton).gameObject.BindEvent(OnClickEquipmentDowngradeButton);
        GetButton((int)Buttons.EquipmentDowngradeButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetToggle((int)Toggles.ResetTapToggle).gameObject.BindEvent(OnClickResetTapToggle);
        GetToggle((int)Toggles.DowngradeTapToggle).gameObject.BindEvent(OnClickDowngradeTapToggle);

        EquipmentResetPopupContentInit();
        OnClickResetTapToggle();

        // 테스트용
#if UNITY_EDITOR
        //TextBindTest();
#endif
        #endregion

        //Refresh();
        return true;
    }

    public void SetInfo(Equipment equipment)
    {

        _equipment = equipment;
        Refresh();
        OnClickResetTapToggle();

    }

    void Refresh()
    {
        if (_init == false)
            return;

        if (_equipment == null)
        {

            GetObject((int)GameObjects.TargetEquipment).SetActive(false);
        }
        else
        {
            EquipmentResetRefresh();
        }

        // 일반 장비 토글 처리 (다운그레이드 불가상태)
        if (_equipment.EquipmentData.EquipmentGrade == EquipmentGrade.Common)
            GetObject((int)GameObjects.ToggleGroupObject).SetActive(false);
        else if (_equipment.EquipmentData.EquipmentGrade == EquipmentGrade.Uncommon)
            GetObject((int)GameObjects.ToggleGroupObject).SetActive(false);
        else if(_equipment.EquipmentData.EquipmentGrade == EquipmentGrade.Rare)
            GetObject((int)GameObjects.ToggleGroupObject).SetActive(false);
        else if(_equipment.EquipmentData.EquipmentGrade == EquipmentGrade.Epic)
            GetObject((int)GameObjects.ToggleGroupObject).SetActive(false);
        else if(_equipment.EquipmentData.EquipmentGrade == EquipmentGrade.Legendary)
            GetObject((int)GameObjects.ToggleGroupObject).SetActive(false);
        else
        {
            EquipmentDowngradeRefresh();
            GetObject((int)GameObjects.ToggleGroupObject).SetActive(true);
        }

        OnClickResetTapToggle();
    }

    // 초기화 리프레시
    void EquipmentResetRefresh()
    {
        GetImage((int)Images.TargetEquipmentImage).sprite = Managers.Resource.Load<Sprite>(_equipment.EquipmentData.SpriteName);
        GetText((int)Texts.TargetEquipmentLevelValueText).text = $"Lv. {_equipment.Level}";

        GetImage((int)Images.ResultEquipmentImage).sprite = Managers.Resource.Load<Sprite>(_equipment.EquipmentData.SpriteName);
        GetText((int)Texts.ResultGoldCountValueText).text = $"{CalculateResetGold()}";

        GetImage((int)Images.ResultMaterialImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.MaterialDic[_equipment.EquipmentData.LevelupMaterialID].SpriteName);
        GetText((int)Texts.ResultMaterialCountValueText).text = $"{CalculateResetMaterialCount()}";

        switch (_equipment.EquipmentData.EquipmentGrade)
        {
            case EquipmentGrade.Common:
                GetImage((int)Images.TargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Common;
                GetImage((int)Images.ResultEquipmentGradeBackgroundImage).color = EquipmentUIColors.Common;
                break;
            case EquipmentGrade.Uncommon:
                GetImage((int)Images.TargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Uncommon;
                GetImage((int)Images.ResultEquipmentGradeBackgroundImage).color = EquipmentUIColors.Uncommon;
                break;
            case EquipmentGrade.Rare:
                GetImage((int)Images.TargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Rare;
                GetImage((int)Images.ResultEquipmentGradeBackgroundImage).color = EquipmentUIColors.Rare;
                break;
            case EquipmentGrade.Epic:
            case EquipmentGrade.Epic1:
            case EquipmentGrade.Epic2:
                GetImage((int)Images.TargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.TargetEquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.ResultEquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.ResultEquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                break;
            case EquipmentGrade.Legendary:
            case EquipmentGrade.Legendary1:
            case EquipmentGrade.Legendary2:
            case EquipmentGrade.Legendary3:
                GetImage((int)Images.TargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.TargetEquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.ResultEquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.ResultEquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                break;
            case EquipmentGrade.Myth:
            case EquipmentGrade.Myth1:
            case EquipmentGrade.Myth2:
            case EquipmentGrade.Myth3:
                GetImage((int)Images.TargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Myth;
                GetImage((int)Images.TargetEquipmentEnforceBackgroundImage).color = EquipmentUIColors.MythBg;
                GetImage((int)Images.ResultEquipmentGradeBackgroundImage).color = EquipmentUIColors.Myth;
                GetImage((int)Images.ResultEquipmentEnforceBackgroundImage).color = EquipmentUIColors.MythBg;
                break;
            default:
                break;
        }

        string gradeName = _equipment.EquipmentData.EquipmentGrade.ToString();
        int num = 0;

        // Epic1 -> 1 리턴 Epic2 ->2 리턴 Common처럼 숫자가 없으면 0 리턴
        Match match = Regex.Match(gradeName, @"\d+$");
        if (match.Success)
            num = int.Parse(match.Value);

        if (num == 0)
        {
            GetImage((int)Images.TargetEquipmentEnforceBackgroundImage).gameObject.SetActive(false);
            GetImage((int)Images.ResultEquipmentEnforceBackgroundImage).gameObject.SetActive(false);
        }
        else
        {
            GetText((int)Texts.TargetEnforceValueText).text = num.ToString();
            GetImage((int)Images.TargetEquipmentEnforceBackgroundImage).gameObject.SetActive(true);
            GetText((int)Texts.ResultEnforceValueText).text = num.ToString();
            GetImage((int)Images.ResultEquipmentEnforceBackgroundImage).gameObject.SetActive(true);
        }
    }

    // 다운그레이드 리프레시
    void EquipmentDowngradeRefresh()
    {
        GetImage((int)Images.DowngradeTargetEquipmentImage).sprite = Managers.Resource.Load<Sprite>(_equipment.EquipmentData.SpriteName);
        GetText((int)Texts.DowngradeTargetEquipmentLevelValueText).text = $"Lv. {_equipment.Level}";

        GetImage((int)Images.DowngradeEquipmentImage).sprite = Managers.Resource.Load<Sprite>(_equipment.EquipmentData.SpriteName);
        GetText((int)Texts.DowngradeResultGoldCountValueText).text = $"{CalculateResetGold()}";

        GetImage((int)Images.DowngradEnchantStoneImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.EquipDataDic[_equipment.EquipmentData.DowngradeMaterialCode].SpriteName);
        GetText((int)Texts.DowngradEnchantStoneCountValueText).text = $"{_equipment.EquipmentData.DowngradeMaterialCount}";
        
        GetImage((int)Images.DowngradeResultMaterialImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.MaterialDic[_equipment.EquipmentData.LevelupMaterialID].SpriteName);
        GetText((int)Texts.DowngradeResultMaterialCountValueText).text = $"{CalculateResetMaterialCount()}";

        switch (_equipment.EquipmentData.EquipmentGrade)
        {
            case EquipmentGrade.Epic1:
            case EquipmentGrade.Epic2:
                GetImage((int)Images.DowngradeTargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.DowngradeTargetEquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.DowngradeEquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.DowngradEnchantStoneBackgroundImage).color = EquipmentUIColors.Epic;
                break;
            case EquipmentGrade.Legendary1:
            case EquipmentGrade.Legendary2:
            case EquipmentGrade.Legendary3:
                GetImage((int)Images.DowngradeTargetEquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.DowngradeTargetEquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.DowngradeEquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.DowngradEnchantStoneBackgroundImage).color = EquipmentUIColors.Legendary;
                break;
            default:
                break;
        }

        string gradeName = _equipment.EquipmentData.EquipmentGrade.ToString();
        int num = 0;

        // Epic1 -> 1 리턴 Epic2 ->2 리턴 Common처럼 숫자가 없으면 0 리턴
        Match match = Regex.Match(gradeName, @"\d+$");
        if (match.Success)
            num = int.Parse(match.Value);

        if (num == 0)
        {
            GetImage((int)Images.DowngradeTargetEquipmentEnforceBackgroundImage).gameObject.SetActive(false);
        }
        else
        {
            GetText((int)Texts.DowngradeTargetEnforceValueText).text = num.ToString();
            GetImage((int)Images.DowngradeTargetEquipmentEnforceBackgroundImage).gameObject.SetActive(true);
        }
    }

    void EquipmentResetPopupContentInit()
    {
        _isSelectedResetTapTap = false;
        _isSelectedDowngradeTapTap = false;

        GetObject((int)GameObjects.ResetInfoGroupObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.DowngradeGroupObject).gameObject.SetActive(false);
    }

    int CalculateResetGold()
    {
        int gold = 0;
        for (int i = 1; i < _equipment.Level; i++)
        {
            gold += Managers.Data.EquipLevelDataDic[i].UpgradeCost;
        }
        return gold;
    }

    int CalculateResetMaterialCount()
    {
        int materialCount = 0;
        for (int i = 1; i < _equipment.Level; i++)
        {
            materialCount += Managers.Data.EquipLevelDataDic[i].UpgradeRequiredItems;
        }

        return materialCount;
    }

    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.Sound.PlayPopupClose();
        //TODO 팝업을 이렇게관리하는게 맞는지 물어보기
        OnClickResetTapToggle();
        gameObject.SetActive(false);
        //Managers.UI.ClosePopupUI(this);

    }

    // 장비 리셋 버튼
    void OnClickEquipmentResetButton()
    {
        // 1. gold data -> 지금 가지고 있는 gold에서 + gold data
        int gold = CalculateResetGold();

        //2. material data -> 지금 가지고 있는 material에서 + material data
        int materialCount = CalculateResetMaterialCount();

        //3. level data -> 실제 데이터에 연동
        _equipment.Level = 1;

        string[] spriteName = new string[2];
        int[] count = new int[2];

        spriteName[0] = Define.GOLD_SPRITE_NAME;
        count[0] = gold;

        int materialCode = _equipment.EquipmentData.LevelupMaterialID;
        spriteName[1] = Managers.Data.MaterialDic[materialCode].SpriteName;
        count[1] = materialCount;

        //RewardPopup 띄우고 ResetPopup 끄기
        UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
        rewardPopup.gameObject.SetActive(true);

        Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD], gold);
        Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[materialCode], materialCount);
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentInfoPopupUI.gameObject.SetActive(false);

        // 버튼 누를 때  EuipmentPopup refresh
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentPopupUI.SetInfo();
        gameObject.SetActive(false);

        rewardPopup.SetInfo(spriteName, count);

    }

    // 장비 다운그레이드 버튼
    void OnClickEquipmentDowngradeButton()
    {
        if (_equipment.EquipmentData.DowngradeEquipmentCode == null)
            return;

        if (Managers.Data.EquipDataDic.TryGetValue(_equipment.EquipmentData.DowngradeEquipmentCode, out Data.EquipmentData downgradedEquip) == false)
            return;

        int gold = 0, materialCount = 0;
        // 1. 레벨 초기화를 할 수 있으면 한다.
        if (_equipment.Level > 1)
        {
            gold = CalculateResetGold();
            materialCount = CalculateResetMaterialCount();
        }
        // 선택된 장비의 아랫단계 장비를 Add한다.
        Managers.Game.AddEquipment(downgradedEquip.DataId);

        // DowngradeMaterialCode를 갯수만큼 인벤토리에 넣는다.
        for (int i = 0; i < _equipment.EquipmentData.DowngradeMaterialCount; i++)
        {
            Managers.Game.AddEquipment(_equipment.EquipmentData.DowngradeMaterialCode);
        }
        //Managers.Game.AddMaterialItem(_equipment.EquipmentData.DowngradeMaterialCode, _equipment.EquipmentData.DowngradeMaterialCount);

        // 선택된 장비를 삭제한다.
        Managers.Game.OwnedEquipments.Remove(_equipment);

        List<string> spriteNameList = new List<string>();
        List<int> count = new List<int>();
        //골드, 메테리얼, 아랫단계 장비, 강화석 
        if (gold > 0) 
        {
            spriteNameList.Add(Define.GOLD_SPRITE_NAME);
            count.Add(gold);
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD], gold);
        }
        if (materialCount > 0)
        {
            int materialCode = _equipment.EquipmentData.LevelupMaterialID;
            spriteNameList.Add(Managers.Data.MaterialDic[materialCode].SpriteName);
            count.Add(materialCount);
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[materialCode], materialCount);
        }
        
        spriteNameList.Add(Managers.Data.EquipDataDic[_equipment.EquipmentData.DowngradeMaterialCode].SpriteName);
        count.Add(_equipment.EquipmentData.DowngradeMaterialCount);

        spriteNameList.Add(downgradedEquip.SpriteName);
        count.Add(1);

        //RewardPopup 띄우고 ResetPopup 끄기
        UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
        rewardPopup.gameObject.SetActive(true);
        rewardPopup.SetInfo(spriteNameList.ToArray(), count.ToArray());

        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentInfoPopupUI.gameObject.SetActive(false);


        // 장비를 다운그레이드하고 리셋결과의 장비와 아이템을 습득
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentInfoPopupUI.gameObject.SetActive(false);
        // 버튼 누를 때  EuipmentPopup refresh
        (Managers.UI.SceneUI as UI_LobbyScene).EquipmentPopupUI.SetInfo();
        gameObject.SetActive(false);

    }

    void OnClickResetTapToggle() // 리셋 토글
    {
        if (_isSelectedResetTapTap == true) // 활성화 후 토글 클릭 방지
            return;
        EquipmentResetPopupContentInit();
        _isSelectedResetTapTap = true;

        GetObject((int)GameObjects.ResetInfoGroupObject).gameObject.SetActive(true);
        GetToggle((int)Toggles.ResetTapToggle).isOn = true;
    }

    void OnClickDowngradeTapToggle() // 다운그레이드 토글
    {
        if (_isSelectedDowngradeTapTap == true) // 활성화 후 토글 클릭 방지
            return;
        EquipmentResetPopupContentInit();
        _isSelectedDowngradeTapTap = true;

        GetObject((int)GameObjects.DowngradeGroupObject).gameObject.SetActive(true);
        GetToggle((int)Toggles.DowngradeTapToggle).isOn = true;
    }
}
