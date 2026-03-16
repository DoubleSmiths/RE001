using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_PickupGachaInfoPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // 상점에서 이 팝업을 호출 할때 픽업, 일반, 고급 가챠에 따라 내용이 달라짐 

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
    // EquipmentLevelValueText : 장비의 레벨 (1/최대 레벨)
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
    // - 신화(Myth) : MythSkillOptionObject
    // EquipmentDescriptionValueText : 대상 장비의 설명 텍스트
    // PickupEquipmentButtonGroupObject : 픽업 장비가 복수일때 리스트 연결

    // 로컬라이징
    // BackgroundText : 탭하여 닫기
    // EquipmentGradeSkillText : 등급 스킬

    #endregion

    #region Enum

    enum GameObjects
    {
        ContentObject,
        UncommonSkillOptionObject,
        RareSkillOptionObject,
        EpicSkillOptionObject,
        LegendarySkillOptionObject,
        MythSkillOptionObject,
        EquipmentInfoPopupTitleObject,
        EquipmentGradeSkillScrollContentObject,
        ButtonGroupObject,
        PickupEquipmentButtonGroupObject,
    }
    enum Buttons
    {
        BackgroundButton,
    }

    enum Toggles
    {
        PickupWeaponToggle,
        PickupChestToggle, // 파츠 이름 체크해서 변경해야함
        PickupHandToggle,
        PickupShoesToggle,
        PickupNecklaceToggle,
        PickupRingToggle,
    }

    enum Texts
    {
        BackgroundText,
        EquipmentGradeSkillText,
        EquipmentGradeValueText,
        EquipmentNameValueText,
        EquipmentLevelValueText,
        EquipmentOptionValueText,
        UncommonSkillOptionDescriptionValueText,
        RareSkillOptionDescriptionValueText,
        EpicSkillOptionDescriptionValueText,
        LegendarySkillOptionDescriptionValueText,
        MythSkillOptionDescriptionValueText,
        EquipmentDescriptionValueText,
    }
    enum Images
    {
        EquipmentGradeBackgroundImage,
        EquipmentOptionImage,
        PickupWeaponImage,
        PickupChestImage,
        PickupHandImage,
        PickupShoesImage,
        PickupNecklaceImage,
        PickupRingImage,
    }
    #endregion

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
        BindToggle(typeof(Toggles));
        BindImage(typeof(Images));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

        GetToggle((int)Toggles.PickupWeaponToggle).gameObject.BindEvent(OnClickPickupWeaponToggle);
        GetToggle((int)Toggles.PickupWeaponToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.PickupChestToggle).gameObject.BindEvent(OnClickPickupChestToggle);
        GetToggle((int)Toggles.PickupChestToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.PickupHandToggle).gameObject.BindEvent(OnClickPickupHandToggle);
        GetToggle((int)Toggles.PickupHandToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.PickupShoesToggle).gameObject.BindEvent(OnClickPickupShoesToggle);
        GetToggle((int)Toggles.PickupShoesToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.PickupNecklaceToggle).gameObject.BindEvent(OnClickPickupNecklaceToggle);
        GetToggle((int)Toggles.PickupNecklaceToggle).GetOrAddComponent<UI_ButtonAnimation>();
        GetToggle((int)Toggles.PickupRingToggle).gameObject.BindEvent(OnClickPickupRingToggle);
        GetToggle((int)Toggles.PickupRingToggle).GetOrAddComponent<UI_ButtonAnimation>();

        GetObject((int)GameObjects.PickupEquipmentButtonGroupObject).gameObject.SetActive(true);

        // 테스트용
#if UNITY_EDITOR
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

    }

    void EquipmentInfoInit() // 장비 정보 초기화
    {
        // EquipmentNameValueText
        // EquipmentGradeValueText
        // EquipmentLevelValueText
        // EquipmentOptionValueText
        // UncommonSkillOptionDescriptionValueText
        // RareSkillOptionDescriptionValueText
        // EpicSkillOptionDescriptionValueText
        // LegendarySkillOptionDescriptionValueText
        // MythSkillOptionDescriptionValueText
        // EquipmentDescriptionValueText
    }

    void PickupToggleGroupInit()
    {
        // PickupWeaponImage
        // PickupChestImage
        // PickupHandImage
        // PickupShoesImage
        // PickupNecklaceImage
        // PickupRingImage
    }

    void OnClickPickupWeaponToggle()
    {
        Managers.Sound.PlayButtonClick();

        // 픽업 무기 정보 변경
    }
    void OnClickPickupChestToggle()
    {
        Managers.Sound.PlayButtonClick();

        // 픽업 갑옷 정보 변경
    }
    void OnClickPickupHandToggle()
    {
        Managers.Sound.PlayButtonClick();
        // 픽업 장갑 정보 변경
    }
    void OnClickPickupShoesToggle()
    {
        Managers.Sound.PlayButtonClick();
        // 픽업 부츠 정보 변경
    }
    void OnClickPickupNecklaceToggle()
    {
        Managers.Sound.PlayButtonClick();
        // 픽업 목걸이 정보 변경
    }
    void OnClickPickupRingToggle()
    {
        Managers.Sound.PlayButtonClick();
        // 픽업 반지 정보 변경
    }

    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.UI.ClosePopupUI(this);
    }

    #region  Test
    void OnClickStartButton() // 버튼 클릭 테스트
    {
        Debug.Log("On click start button");
    }

    void TextBindTest() // 텍스트 연결 테스트용
    {
        string TestText = "Test";

        for (int i = 0; i < System.Enum.GetValues(typeof(Texts)).Length; i++)
        {
            GetText(i).text = TestText;
        }
    }
    #endregion
}
