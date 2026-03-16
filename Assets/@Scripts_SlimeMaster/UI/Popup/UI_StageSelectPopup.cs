using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Data;
using UnityEngine.UI.Extensions;
using System.Linq;
using UnityEngine.UI;
using System;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class UI_StageSelectPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // StageScrollContentObject : UI_ChapterInfoItem이 들어갈 부모 개체
    // AppearingMonsterContentObject : UI_MonsterInfo가 들어갈 부모 개체
    // StageImage : 스테이지의 이미지 (테이블에 추가 필요)
    // StageNameValueText : 스테이지의 이름 (테이블에 추가 필요)
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
    // StageSelectTitleText : 스테이지
    // AppearingMonsterText : 등장 몬스터
    // StageSelectButtonText : 선택

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        StageScrollContentObject,
        AppearingMonsterContentObject,
        StageSelectScrollView,

    }

    enum Buttons
    {
        StageSelectButton,
        BackButton,
    }

    enum Texts
    {
        StageSelectTitleText,
        AppearingMonsterText,
        StageSelectButtonText,

    }
    
    enum Images
    {
        LArrowImage,
        RArrowImage
    }

    #endregion

    StageData _stageData;
    HorizontalScrollSnap _scrollsnap;

    public Action OnPopupClosed;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }
   
    private void OnDisable()
    {
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

        GetButton((int)Buttons.StageSelectButton).gameObject.SetActive(false);
        GetButton((int)Buttons.StageSelectButton).gameObject.BindEvent(OnClickStageSelectButton);
        GetButton((int)Buttons.StageSelectButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnClickBackButton);
        GetButton((int)Buttons.BackButton).GetOrAddComponent<UI_ButtonAnimation>();

        _scrollsnap = Util.FindChild<HorizontalScrollSnap>(gameObject, recursive : true);
        _scrollsnap.OnSelectionPageChangedEvent.AddListener(OnChangeStage);
        _scrollsnap.StartingScreen = Managers.Game.CurrentStageData.StageIndex -1;
        // 테스트용
#if UNITY_EDITOR

        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    public void SetInfo(StageData stageData)
    {
        _stageData = stageData;
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;

        if (_stageData == null)
            return;

        #region 초기화

        //AppearingMonsterContainer.DestroyChilds();
        #region 스테이지 리스트
        GameObject StageContainer = GetObject((int)GameObjects.StageScrollContentObject);
        StageContainer.DestroyChilds();

        _scrollsnap.ChildObjects = new GameObject[Managers.Data.StageDic.Count];

        foreach (StageData stageData in Managers.Data.StageDic.Values)
        {
            UI_StageInfoItem item = Managers.UI.MakeSubItem<UI_StageInfoItem>(StageContainer.transform);
            item.SetInfo(stageData);
            _scrollsnap.ChildObjects[stageData.StageIndex - 1] = item.gameObject;
        }

        #endregion
        StageInfoRefresh();
        #endregion

    }

    void StageInfoRefresh()
    {
        #region 스테이지 정보
        UIRefresh();
        #endregion

        #region 스크롤된 스테이지 등장몬스터

        List<int> monsterList = _stageData.AppearingMonsters.ToList();

        GameObject container = GetObject((int)GameObjects.AppearingMonsterContentObject);
        container.DestroyChilds();
        for (int i = 0; i < monsterList.Count; i++)
        {
            UI_MonsterInfoItem monsterInfoItemUI = Managers.UI.MakeSubItem<UI_MonsterInfoItem>(container.transform);
     
            monsterInfoItemUI.SetInfo(monsterList[i], _stageData.StageLevel, this.transform);
        }
        #endregion
    }

    void UIRefresh()
    {
        // 기본 상태
        GetImage((int)Images.LArrowImage).gameObject.SetActive(true);
        GetImage((int)Images.RArrowImage).gameObject.SetActive(true);
        GetButton((int)Buttons.StageSelectButton).gameObject.SetActive(false);

        #region 스테이지 화살표
        if (_stageData.StageIndex == 1)
        {
            GetImage((int)Images.LArrowImage).gameObject.SetActive(false);
            GetImage((int)Images.RArrowImage).gameObject.SetActive(true);
        }
        else if (_stageData.StageIndex >= 2 && _stageData.StageIndex < 50)
        {
            GetImage((int)Images.LArrowImage).gameObject.SetActive(true);
            GetImage((int)Images.RArrowImage).gameObject.SetActive(true);
        }
        else if (_stageData.StageIndex == 50)
        {
            GetImage((int)Images.LArrowImage).gameObject.SetActive(true);
            GetImage((int)Images.RArrowImage).gameObject.SetActive(false);
        }
        #endregion

        #region 스테이지 선택 버튼
        if (Managers.Game.DicStageClearInfo.TryGetValue(_stageData.StageIndex, out StageClearInfo info) == false)
            return;
        //게임 처음 시작하고 스테이지창을 오픈 한 경우
        if (info.StageIndex == 1 && info.MaxWaveIndex == 0)
        {
            GetButton((int)Buttons.StageSelectButton).gameObject.SetActive(true);
        }
        // 스테이지 진행중
        if (info.StageIndex <=_stageData.StageIndex)
            GetButton((int)Buttons.StageSelectButton).gameObject.SetActive(true);
        // 새로운 스테이지
        if (Managers.Game.DicStageClearInfo.TryGetValue(_stageData.StageIndex - 1, out StageClearInfo PrevInfo) == false)
            return;
        if (PrevInfo.isClear == true)
            GetButton((int)Buttons.StageSelectButton).gameObject.SetActive(true);
        else
            GetButton((int)Buttons.StageSelectButton).gameObject.SetActive(false);
        #endregion
    }

    void OnClickStageSelectButton()
    {
        //if (Managers.Game.DicStageClearInfo.TryGetValue(_stageData.StageIndex, out StageClearInfo info) == false)
        //    return;
        ////게임 처음 시작하고 스테이지창을 오픈 한 경우
        //if (info.StageIndex == 1 && info.MaxWaveIndex == 0)
        //{
        //    Managers.Game.CurrentStageData = _stageData;
        //    OnPopupClosed?.Invoke();
        //    Managers.UI.ClosePopupUI(this);
        //}

        //if (Managers.Game.DicStageClearInfo.TryGetValue(_stageData.StageIndex - 1, out StageClearInfo PrevInfo) == false)
        //    return;
        //if (PrevInfo.isClear == true)
        //{
        //    Managers.Game.CurrentStageData = _stageData;
        //    OnPopupClosed?.Invoke();
        //    Managers.UI.ClosePopupUI(this);
        //}

        Managers.Game.CurrentStageData = _stageData;
        OnPopupClosed?.Invoke();
        Managers.UI.ClosePopupUI(this);

    }
    
    void OnClickBackButton() // 되돌아 가기
    {
        OnPopupClosed?.Invoke();
        Managers.UI.ClosePopupUI(this);
    }



    void OnChangeStage(int index)
    {
        //현재 스테이지 설정
        _stageData = Managers.Data.StageDic[index + 1];

        int[] monsterData = _stageData.AppearingMonsters.ToArray();

        GameObject container = GetObject((int)GameObjects.AppearingMonsterContentObject);
        container.DestroyChilds();

        for (int i = 0; i < monsterData.Length; i++)
        {
            UI_MonsterInfoItem item = Managers.UI.MakeSubItem<UI_MonsterInfoItem>(GetObject((int)GameObjects.AppearingMonsterContentObject).transform);
            item.SetInfo(monsterData[i], _stageData.StageLevel, this.transform);
            //데이터 타입 물어보고 처리해야할듯
        }

        UIRefresh();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.AppearingMonsterContentObject).GetComponent<RectTransform>());
    }



}
