using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Data;

public class UI_MissionPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신

    // DailyMissionProgressSliderObject : 미션 완료 시 슬라이더 상승 (최대 6, 1씩 상승)
    // DailyMissionRemainingTimeText : 데일리 미션 초기화까지 남은 시간

    // 일일 미션 보상 데이터 연결 (기본상태 -> 보상 수령 가능 상태 -> 보상 수령 완료 상태)
    //      첫번재 보상 (2개 완료)
    //      - DailyMissionFirstClearRewardItemBackgroundImage : 보상 아이템의 테두리 (색상 변경)
    //      - 일반(Common) : #AC9B83
    //      - 고급(Uncommon)  : #73EC4E
    //      - 희귀(Rare) : #0F84FF
    //      - 유일(Epic) : #B740EA
    //      - 전설(Legendary) : #F19B02
    //      - 신화(Myth) : #FC2302
    //      - DailyMissionFirstClearRewardItemImage : 보상 아이템의 아이콘
    //      - DailyMissionFirstClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - DailyMissionFirstClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - DailyMissionFirstClearOutlineObject : 보상 수령 가능할 시 활성화 (기본 비활성화) 
    // DailyMissionScrollObject : 데일리 미션용 MissionItme이 들어갈 부모 개체


    //      두번재 보상 (4개 완료)
    //      - DailyMissionSecondClearRewardItemBackgroundImage : : 보상 아이템의 테두리 (색상 변경)
    //      - DailyMissionSecondClearRewardItemImage : 보상 아이템의 아이콘
    //      - DailyMissionSecondClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - DailyMissionSecondClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - DailyMissionSecondClearOutlineObject : 보상 수령 가능할 시 활성화 (기본 비활성화)

    //      세번재 보상 (6개 완료)
    //      - DailyMissionThirdClearRewardItemBackgroundImage : : 보상 아이템의 테두리 (색상 변경)
    //      - DailyMissionThirdClearRewardItemImage : 보상 아이템의 아이콘
    //      - DailyMissionThirdClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - DailyMissionThirdClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - DailyMissionThirdClearOutlineObject : 보상 수령 가능할 시 활성화 (기본 비활성화)


    // 주간 미션 보상 데이터 연결 (기본상태 -> 보상 수령 가능 상태 -> 보상 수령 완료 상태)
    //      첫번재 보상 (2개 완료)
    //      - WeeklyMissionFirstClearRewardItemBackgroundImage : 보상 아이템의 테두리 (색상 변경)
    //      - WeeklyMissionFirstClearRewardItemImage : 보상 아이템의 아이콘
    //      - WeeklyMissionFirstClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - WeeklyMissionFirstClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - WeeklyMissionFirstClearOutlineObject : 보상 수령 가능할 시 활성화 (기본 비활성화) 

    //      두번재 보상 (4개 완료)
    //      - WeeklyMissionSecondClearRewardItemBackgroundImage : : 보상 아이템의 테두리 (색상 변경)
    //      - WeeklyMissionSecondClearRewardItemImage : 보상 아이템의 아이콘
    //      - WeeklyMissionSecondClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - WeeklyMissionSecondClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - WeeklyMissionSecondClearOutlineObject : 보상 수령 가능할 시 활성화 (기본 비활성화)

    //      세번재 보상 (6개 완료)
    //      - WeeklyMissionThirdClearRewardItemBackgroundImage : : 보상 아이템의 테두리 (색상 변경)
    //      - WeeklyMissionThirdClearRewardItemImage : 보상 아이템의 아이콘
    //      - WeeklyMissionThirdClearRewardItemCountValueText : 보상 아이템의 벨류 (개수)
    //      - WeeklyMissionThirdClearRewardUnlockObject : 보상 수령 가능할 시 비활성화 (기본 활성화) 
    //      - WeeklyMissionThirdClearOutlineObject : 보상 수령 가능할 시 활성화 (기본 비활성화)
    // WeeklyMissionScrollObject : 데일리 미션용 MissionItme이 들어갈 부모 개체


    // 로컬라이징
    // BackgroundText : 터치하여 닫기
    // DailyMissionTitleText : 일일 미션
    // WeeklyMissionTitleText : 주간 미션
    // DailyMissionCompleteText : 완료
    // DailyMissionRemainingTimeValueText : 남은 시간
    // WeeklyMissionCompleteText : 완료
    // WeeklyMissionRemainingTimeValueText : 남은 시간

    #endregion

    #region Enum
    
    enum GameObjects
    {
        ContentObject,
        DailyMissionContentObject,
        DailyMissionScrollObject,
    }
    enum Buttons
    {
        BackgroundButton,
    }

    enum Texts
    {
        BackgroundText,
        DailyMissionTitleText,
        DailyMissionCommentText,

    }

    enum Images
    {
        GradientImage,

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
        BindImage(typeof(Images));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

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

        GetObject((int)GameObjects.DailyMissionScrollObject).DestroyChilds();
        foreach(KeyValuePair<int, MissionData> data in Managers.Data.MissionDataDic)
        {
            if(data.Value.MissionType == Define.MissionType.Daily)
            {
                UI_MissionItem dailyMission = Managers.UI.MakeSubItem<UI_MissionItem>(GetObject((int)GameObjects.DailyMissionScrollObject).transform);
                dailyMission.SetInfo(data.Value);
            }
        }
    }

    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton() 
    {
        Managers.UI.ClosePopupUI(this);
    }
}
