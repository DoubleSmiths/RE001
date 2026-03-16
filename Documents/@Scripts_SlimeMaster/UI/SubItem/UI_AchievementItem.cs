using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Data;
using UnityEngine.UI;
using UnityEditor;

public class UI_AchievementItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // RewardItmeIconImage : 보상 아이템의 아이콘
    // RewardItmeValueText : 보상 아이템의 수량
    // ProgressSliderObject : 미션의 진행도 슬라이더로 표시

    // AchievementNameValueText : 미션의 이름
    // AchievementValueText : 미션의 진행도 (현재 / 목표)

    #endregion


    #region Enum
    enum GameObjects
    {
        ProgressSlider,
    }

    enum Buttons
    {
        GetButton,
        //GoNowButton,
    }

    enum Texts
    {
        RewardItmeValueText,
        CompleteText,
        AchievementNameValueText,
        AchievementValueText,
        ProgressText
    }

    enum Images
    {
        RewardItmeIcon,
    }

    enum MissionState
    {
        Progress,
        Complete,
        Rewarded,
    }
    #endregion

    AchievementData _achievementData;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.GetButton).gameObject.BindEvent(OnClickGetButton);
        GetButton((int)Buttons.GetButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.GoNowButton).gameObject.BindEvent(OnClickGoNowButton);
        //GetButton((int)Buttons.GoNowButton).GetOrAddComponent<UI_ButtonAnimation>();
        AchievementContentInit();



        // 테스트용
#if UNITY_EDITOR
        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;    
    }

    public void SetInfo(AchievementData achievementData)
    {
        transform.localScale = Vector3.one;
        _achievementData = achievementData;
        Refresh();
    }

    void Refresh()
    {
        // 미션 클리어 상태에 따라 활성화
        //      - GoNowButton : 진행중
        //      - GetButton : 클리어 시
        //      - CompleteText : 보상 지급 완료
        if (_init == false)
            return;

        GetText((int)Texts.RewardItmeValueText).text = $"{_achievementData.RewardValue}";
        GetText((int)Texts.AchievementNameValueText).text = $"{_achievementData.DescriptionTextID}";
        GetObject((int)GameObjects.ProgressSlider).GetComponent<Slider>().value = 0;
     
        int progress = Managers.Achievement.GetProgressValue(_achievementData.MissionTarget);
        if (progress > 0)
            GetObject((int)GameObjects.ProgressSlider).GetComponent<Slider>().value = (float)progress / _achievementData.MissionTargetValue;

        if (progress >= _achievementData.MissionTargetValue)
        {
            SetButtonUI(MissionState.Complete);
            if (_achievementData.IsRewarded == true)
                SetButtonUI(MissionState.Rewarded);
        }
        else
        {
            SetButtonUI(MissionState.Progress);
        }
        GetText((int)Texts.AchievementValueText).text = $"{progress}/{_achievementData.MissionTargetValue}";

        string sprName = Managers.Data.MaterialDic[_achievementData.ClearRewardItmeId].SpriteName;
        GetImage((int)Images.RewardItmeIcon).sprite = Managers.Resource.Load<Sprite>(sprName);
    }

    void SetButtonUI(MissionState state)
    {
        GameObject objComplte = GetButton((int)Buttons.GetButton).gameObject;
        GameObject objProgress = GetText((int)Texts.ProgressText).gameObject;
        GameObject objRewarded = GetText((int)Texts.CompleteText).gameObject;

        switch (state)
        {
            case MissionState.Rewarded:
                objRewarded.SetActive(true);
                objComplte.SetActive(false);
                objProgress.SetActive(false);
                break;
            case MissionState.Complete:
                objRewarded.SetActive(false);
                objComplte.SetActive(true);
                objProgress.SetActive(false);
                break;
            case MissionState.Progress:
                objRewarded.SetActive(false);
                objComplte.SetActive(false);
                objProgress.SetActive(true);
                //GetText((int)Texts.ProgressText).text = $"진행중";
                break;
        }
    }

    void AchievementContentInit()
    {
        GetButton((int)Buttons.GetButton).gameObject.SetActive(true); // 임시로 활성화
        GetText((int)Texts.ProgressText).gameObject.SetActive(false);
        GetText((int)Texts.CompleteText).gameObject.SetActive(false);
    }

    void OnClickGetButton() // 보상 받기 버튼
    {
        Managers.Sound.PlayButtonClick();

        string[] spriteName = new string[1];
        int[] count = new int[1];

        spriteName[0] = Managers.Data.MaterialDic[Define.ID_DIA].SpriteName;
        count[0] = _achievementData.RewardValue;

        UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
        rewardPopup.gameObject.SetActive(true);
        Managers.Game.Dia += _achievementData.RewardValue;
        Managers.Achievement.RewardedAchievement(_achievementData.AchievementID);
        _achievementData = Managers.Achievement.GetNextAchievment(_achievementData.AchievementID);
        if(_achievementData != null)
            Refresh();
        rewardPopup.SetInfo(spriteName, count);
    }

}
