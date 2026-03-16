using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_StageRewardPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // StageScrollContentObject : UI_ChapterInfoItem가 들어가는 부모 계체
    // StageRewardProgressSliderObject : 스테이지 클리어 시 슬라이더 상승(챕터의 최대 스테이지 수, 1씩 상승)




    // 챕터 클리어 보상 데이터 연결 (기본상태 -> 보상 수령 가능 상태 -> 보상 수령 완료 상태)
    // FirstClearRewardText : 첫번째 보상 조건 
    //      첫번재 보상
    //      - FirstClearRewardItemBackgroundImage : 보상 아이템의 테두리 (색상 변경)
    //      - 일반(Common) : #AC9B83
    //      - 고급(Uncommon)  : #73EC4E
    //      - 희귀(Rare) : #0F84FF
    //      - 유일(Epic) : #B740EA
    //      - 전설(Legendary) : #F19B02
    //      - 신화(Myth) : #FC2302
    //      - FirstClearRewardItemImage : 보상 아이템의 아이콘
    //      - FirstClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - FirstClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - FirstClearRedDotObject : 보상 수령 가능할 시 활성화 (기본 비활성화) 


    //      두번재 보상
    // SecondClearRewardText : 두번째 보상 조건 
    //      - SecondClearRewardItemBackgroundImage : : 보상 아이템의 테두리 (색상 변경)
    //      - SecondClearRewardItemImage : 보상 아이템의 아이콘
    //      - SecondClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - SecondClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - SecondClearRedDotObject : 보상 수령 가능할 시 활성화 (기본 비활성화)

    //      세번재 보상
    // ThirdClearRewardText : 세번째 보상 조건 
    //      - ThirdClearRewardItemBackgroundImage : : 보상 아이템의 테두리 (색상 변경)
    //      - ThirdClearRewardItemImage : 보상 아이템의 아이콘
    //      - ThirdClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - ThirdClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - ThirdClearRedDotObject : 보상 수령 가능할 시 활성화 (기본 비활성화)

    // 로컬라이징
    // StageRewardTitleText
    // StageRewardDescriptionText

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,

        StageScrollContentObject,
        StageRewardProgressSliderObject,

        FirstClearRewardUnlockObject, 
        SecondClearRewardUnlockObject,
        ThirdClearRewardUnlockObject,
        FirstClearOutlineObject, 
        SecondClearOutlineObject,
        ThirdClearOutlineObject,
        FirstClearRewardCompleteObject,
        SecondClearRewardCompleteObject,
        ThirdClearRewardCompleteObject,

    }
    enum Buttons
    {
        BackButton,

        FirstClearRewardButton,
        SecondClearRewardButton,
        ThirdClearRewardButton,
    }

    enum Texts
    {
        StageRewardTitleText,
        StageRewardDescriptionText,

        FirstClearRewardText,
        SecondClearRewardText,
        ThirdClearRewardText,

        FirstClearRewardItemCountValueText,
        SecondClearRewardItemCountValueText,
        ThirdClearRewardItemCountValueText,
    }

    enum Images
    {
        FirstClearRewardItemBackgroundImage,
        SecondClearRewardItemBackgroundImage,
        ThirdClearRewardItemBackgroundImage,
        FirstClearRewardItemImage,
        SecondClearRewardItemImage,
        ThirdClearRewardItemImage,
    }
    #endregion

    int _stageNum = 1;

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

        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnClickBackButton);
        GetButton((int)Buttons.FirstClearRewardButton).gameObject.BindEvent(OnClickFirstClearRewardButton);
        GetButton((int)Buttons.SecondClearRewardButton).gameObject.BindEvent(OnClickSecondClearRewardButton);
        GetButton((int)Buttons.ThirdClearRewardButton).gameObject.BindEvent(OnClickThirdClearRewardButton);

        GetButton((int)Buttons.BackButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.FirstClearRewardButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SecondClearRewardButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ThirdClearRewardButton).GetOrAddComponent<UI_ButtonAnimation>();

        ChapterRewardPopupContentInit();
        #endregion

        Refresh();
        return true;
    }

    void ChapterRewardPopupContentInit()
    {
        GetObject((int)GameObjects.FirstClearRewardUnlockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.FirstClearOutlineObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.FirstClearRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.SecondClearRewardUnlockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.SecondClearOutlineObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.SecondClearRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.ThirdClearRewardUnlockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.ThirdClearOutlineObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.ThirdClearRewardCompleteObject).gameObject.SetActive(false);
    }

    public void SetInfo(int stageNum)
    {
        _stageNum = stageNum;

        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;
    }


    void OnClickBackButton()
    {
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickFirstClearRewardButton()
    {
        Managers.Sound.PlayButtonClick();

        GetObject((int)GameObjects.FirstClearRewardCompleteObject).gameObject.SetActive(true); // 보상 수령 시 활성화 (기본 비활성화)
    }

    void OnClickSecondClearRewardButton()
    {
        Managers.Sound.PlayButtonClick();
        GetObject((int)GameObjects.SecondClearRewardCompleteObject).gameObject.SetActive(true); // 보상 수령 시 활성화 (기본 비활성화)
    }

    void OnClickThirdClearRewardButton()
    {
        Managers.Sound.PlayButtonClick();
        GetObject((int)GameObjects.ThirdClearRewardCompleteObject).gameObject.SetActive(true); // 보상 수령 시 활성화 (기본 비활성화)
    }
}
