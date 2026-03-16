using Data;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_CheckOutPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // 30일의 출석일을 기준으로 10일마다 보드를 초기화 하고 15일, 20일, 30일때 추가 보상을 지급한다.
    // CheckOutBoardObject : 10개의 UI_CheckOutItem이 들어갈 부모개체 (출석 보드)
    // CheckOutProgressSliderObject : 30일 기준 현재 출석일을 슬라이더로 표시


    // FirstClearRewardBackgroundImage : 보상아이템의 등급에 따라 색상변경 (15일)
    // FirstClearRewardItemImage : 보상 아이템의 이미지 (15일) 
    // FirstClearRewardCountText : 보상 아이템의 개수 (15일)

    // SecondClearRewardBackgroundImage : 보상아이템의 등급에 따라 색상변경 (20일)
    // SecondClearRewardItemImage : 보상 아이템의 이미지 (20일)
    // SecondClearRewardCountText : 보상 아이템의 개수 (20일)

    // ThirdClearRewardBackgroundImage : 보상아이템의 등급에 따라 색상변경 (30일)
    // ThirdClearRewardItemImage : 보상 아이템의 이미지 (30일)
    // ThirdClearRewardCountText : 보상 아이템의 개수 (30일)

    // FirstClearRedDotObject : 출석일수 조건 도달 시 활성화 (15일)
    // SecondClearRedDotObject : 출석일수 조건 도달 시 활성화 (20일)
    // ThirdClearRedDotObject : 출석일수 조건 도달 시 활성화 (30일)

    // FirstClearRewardCompleteObject : 보상을 지급하고 활성화 (15일)
    // SecondClearRewardCompleteObject : 보상을 지급하고 활성화 (20일)
    // ThirdClearRewardCompleteObject : 보상을 지급하고 활성화 (30일)


    // 로컬라이징
    // CheckOutPopupTitleText : 출석 체크
    // CheckOutDescriptionText : 출석일수 30일이 지나면 보드가 초기화됩니다.
    // BackgroundText : 터치하여 닫기

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        CheckOutProgressSliderObject,
        CheckOutBoardObject,
        FirstClearRewardCompleteObject,
        SecondClearRewardCompleteObject,
        ThirdClearRewardCompleteObject,
    }
    enum Buttons
    {
        BackgroundButton,
    }

    enum Texts
    {
        BackgroundText,
        CheckOutPopupTitleText,

        FirstClearRewardCountText,
        SecondClearRewardCountText,
        ThirdClearRewardCountText,
        DaysCountText,
        CheckOutDescriptionText,
    }
    enum Images
    {
        FirstClearRewardBackgroundImage,
        SecondClearRewardBackgroundImage,
        ThirdClearRewardBackgroundImage,

        FirstClearRewardItemImage,
        SecondClearRewardItemImage,
        ThirdClearRewardItemImage,
    }
    #endregion

    public int _userCheckOutDay; 
    int _monthlyCount; 
    int _dailyCount;
    Transform _makeSubItemParents;

    private void Awake()
    {
        Init();
    }
    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        //Refresh();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetObject((int)GameObjects.FirstClearRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.SecondClearRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.ThirdClearRewardCompleteObject).gameObject.SetActive(false);
        #endregion

        Refresh();
        return true;
    }


    public void SetInfo(int checkOutDay)
    {
        _userCheckOutDay = checkOutDay;
        Debug.Log(_userCheckOutDay);
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;

        if (_userCheckOutDay == 0)
            return;


        _monthlyCount = _userCheckOutDay % 30;
        _dailyCount = _monthlyCount % 10;
        // 출석판 카운트 계산
        // 나머지가 0이면 10일로 보정
        if (_dailyCount == 0)
        {
            _dailyCount = 10;
        }

        // 10일 보드판 초기화
        GetObject((int)GameObjects.CheckOutBoardObject).DestroyChilds();
        _makeSubItemParents = GetObject((int)GameObjects.CheckOutBoardObject).transform;
        // dailyCount 수에 따라 SetInfo에 true값을 넘겨줌
        for (int count = 1; count <= 10; count++)
        {
            UI_CheckOutItem item = Managers.UI.MakeSubItem<UI_CheckOutItem>(_makeSubItemParents);
            item.transform.SetAsLastSibling();
            if (_dailyCount >= count)
                item.SetInfo(count, true);
            else
                item.SetInfo(count, false);
        }

        // 갱신 보상 초기화
        if (_monthlyCount >= 10 && _monthlyCount < 20) // 10일
        {
            GetObject((int)GameObjects.FirstClearRewardCompleteObject).gameObject.SetActive(true);
        }
        else if (_monthlyCount >= 20 && _monthlyCount < 30) // 20일
        {
            GetObject((int)GameObjects.SecondClearRewardCompleteObject).gameObject.SetActive(true);
        }
        else if (_monthlyCount >= 30) // 30일
        {
            GetObject((int)GameObjects.ThirdClearRewardCompleteObject).gameObject.SetActive(true);
        }

        GetText((int)Texts.DaysCountText).text = $"{_monthlyCount}일";
        GetObject((int)GameObjects.CheckOutProgressSliderObject).GetComponent<Slider>().value = _monthlyCount;
    }

    void OnClickBackgroundButton()
    {
        Managers.Sound.PlayButtonClick();
        _userCheckOutDay = 0;
        Managers.UI.ClosePopupUI(this);
    }

    #region  Test
    public void OnClickButtonTest() // 버튼 클릭 테스트
    {
        Debug.Log("On click start button");
        //_userCheckOutDay += 1;
        //Refresh();
    }

    void TextBindTest() // 텍스트 연결 테스트용
    {
        string TestText = "Test";

        for (int i = 0; i < System.Enum.GetValues(typeof(Texts)).Length; i++)
        {
            GetText(i).text = TestText;
        }
    }
    void OnClickCheckOutTestButton() // 출석일 올리는 테스트 버튼
    {
        _monthlyCount += 1;
        _dailyCount += 1;
        GetText((int)Texts.DaysCountText).text = $"{_monthlyCount}";

        Init();
    }

    #endregion


}
