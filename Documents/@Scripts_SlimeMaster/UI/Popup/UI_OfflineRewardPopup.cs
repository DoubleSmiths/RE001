using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_OfflineRewardPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // TotalTimeValueText : 마지막 보상부터 현재까지 걸린 시간 표기 (최대 24시간, hh:mm:ss 으로 표기)
    // ResultGoldValueText : 클리어한 챕터 단계에 따라 보상으로 얻게 될 시간당 골드 ( nnnn/h 로 표기
    // ResultExpValueText : 클리어한 챕터 단계에 따라 보상으로 얻게 될 시간당 계정 경험치 ( nnnn/h 로 표기
    // RewardItemScrollContentObject : 보상으로 얻게될 아이템이 들어갈 부모 개체
    // (골드, 경헌치, 아이템, 캐릭터 강화석 등을 보상으로)

    // 로컬라이징
    // OfflineRewardPopupTitleText : 오프라인 보상
    // OfflineRewardCommentText : 더 많은 스테이지를 클리어하면 더 많은 보상을 얻습니다.
    // TotalTimeText : 총 시간
    // ButtonCommentText : 최대 24시간 보상
    // FastRewardText : 빠른 보상
    // ClaimButtonText : 얻기


    #endregion


    #region Enum
    enum GameObjects
    {
        ContentObject,
        RewardItemScrollContentObject,
        OfflineRewardGoldEffect,
    }

    enum Buttons
    {
        BackgroundButton,
        FastRewardButton,
        ClaimButton,

    }

    enum Texts
    {
        BackgroundText,
        OfflineRewardPopupTitleText,
        OfflineRewardCommentText,
        TotalTimeText,
        TotalTimeValueText,
        ResultGoldValueText,
        //ResultExpValueText,
        FastRewardText,
        ClaimButtonText,
        ButtonCommentText,
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

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.FastRewardButton).gameObject.BindEvent(OnClickFastRewardButton);
        GetButton((int)Buttons.FastRewardButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);

        GetObject((int)GameObjects.OfflineRewardGoldEffect).SetActive(false);



#if UNITY_EDITOR

        //TextBindTest();
#endif


        #endregion

        Refresh();
        StartCoroutine(CoTimeCheck());
        return true;
    }

    void Refresh()
    {
        // TotalTimeValueText : 마지막 보상부터 현재까지 걸린 시간 표기 (최대 24시간, hh:mm:ss 으로 표기)
        StopAllCoroutines();


        if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
        {
            // ResultGoldValueText : 클리어한 챕터 단계에 따라 보상으로 얻게 될 시간당 골드 ( nnnn/h 로 표기
            GetText((int)Texts.ResultGoldValueText).text = $"{offlineReward.Reward_Gold} / 시간";
            // ResultExpValueText : 클리어한 챕터 단계에 따라 보상으로 얻게 될 시간당 계정 경험치 ( nnnn/h 로 표기
        }

        GameObject container = GetObject((int)GameObjects.RewardItemScrollContentObject);
        container.DestroyChilds();
        if (Managers.Time.TimeSinceLastReward.TotalMinutes > 10)
        {
            UI_MaterialItem item = Managers.UI.MakeSubItem<UI_MaterialItem>(container.transform);
            int count = (int)Managers.Time.CalculateGoldPerMinute(offlineReward.Reward_Gold);
            item.SetInfo(Define.GOLD_SPRITE_NAME, count);
        }

    }

    IEnumerator CoTimeCheck()
    {
        while (true)
        {

            TimeSpan timeSpan = Managers.Time.TimeSinceLastReward;

            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            if (timeSpan == TimeSpan.FromHours(24))
            {
                formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", 24, 0, 0);
            }

            GetText((int)Texts.TotalTimeValueText).text = formattedTime;

            if (timeSpan.TotalMinutes < 10)
            {
                TimeSpan remainingTime = TimeSpan.FromMinutes(10) - timeSpan;

                // Display remaining time
                //남은시간 표기
                string remaining = string.Format("{0:D2}분 {1:D2}초", remainingTime.Minutes, remainingTime.Seconds);
                GetText((int)Texts.ClaimButtonText).text = remaining;
                GetButton((int)Buttons.ClaimButton).GetComponent<Image>().color = Util.HexToColor("989898");
                //버튼 비활성화,

            }
            else
            {
                GetText((int)Texts.ClaimButtonText).text = "받기";
                GetButton((int)Buttons.ClaimButton).GetComponent<Image>().color = Util.HexToColor("50D500");
                GetButton((int)Buttons.ClaimButton).GetOrAddComponent<UI_ButtonAnimation>();
                Refresh();
            }
            yield return new WaitForSeconds(1);

        }
    }
    void OnClickBackgroundButton() // 배경 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);

    }
    void OnClickFastRewardButton() // 빠른 리워드 버튼
    {
        Managers.Sound.PlayButtonClick();

        // 빠른 보상 팝업 호출
        if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
        {
            UI_FastRewardPopup popup = Managers.UI.ShowPopupUI<UI_FastRewardPopup>();
            popup.SetInfo(offlineReward);
        }

    }
    void OnClickClaimButton() // 수락 버튼 (스테미너 사용)
    {
        Managers.Sound.PlayButtonClick();
        

        if (Managers.Time.TimeSinceLastReward.TotalMinutes < 10)
        {
            return;
        }
        if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
        {
            GetObject((int)GameObjects.OfflineRewardGoldEffect).SetActive(true);
            Managers.Time.GiveOfflineReward(offlineReward);
        }
        
        Refresh();
        Managers.UI.ClosePopupUI(this);
    }
}
