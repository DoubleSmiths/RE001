using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_FastRewardPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // RewardItemScrollContentObject 리워드 아이템 들어갈 부모개체
    // ClaimCostValueText : 빠른 보상을 받을 시 소모되는 스테미너 값
    // EemainingCountValueText : 하루 보상 리미트 횟수

    // 로컬라이징 텍스트
    // FastRewardPopupTitleText : 빠른 보상
    // FastRewardCommentText : 보상 수익 300분 즉시 적립
    // ADFreeText : Free
    #endregion



    #region Enum
    enum GameObjects
    {
        ContentObject,
        ItemContainer, 
    }

    enum Buttons
    {
        BackgroundButton,
        ADFreeButton,
        ClaimButton,
    }

    enum Texts
    {
        BackgroundText,
        FastRewardPopupTitleText,
        FastRewardCommentText,
        ADFreeText,
        ClaimCostValueText,
        EemainingCommentText,
        EemainingCountValueText,
    }

    OfflineRewardData _offlineRewardData;
    bool _isClaim = false;
    #endregion
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
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.ADFreeButton).gameObject.BindEvent(OnClickADFreeButton);
        GetButton((int)Buttons.ADFreeButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);

        GetButton((int)Buttons.ClaimButton).GetComponent<Image>().color = Color.white;

#if UNITY_EDITOR

        //TextBindTest();
#endif


        #endregion
        return true;
    }

    public void SetInfo(OfflineRewardData offlineReward)
    {
        _offlineRewardData = offlineReward;
        Refresh();
    }

    void Refresh()
    {
        GameObject container = GetObject((int)GameObjects.ItemContainer);
        container.DestroyChilds();

        if (Managers.Game.Stamina >= 15 && Managers.Game.FastRewardCountStamina > 0)
        {
            GetButton((int)Buttons.ClaimButton).GetComponent<Image>().color = Util.HexToColor("50D500");
            _isClaim = true;
            GetButton((int)Buttons.ClaimButton).GetOrAddComponent<UI_ButtonAnimation>();
            //GetButton((int)Buttons.ClaimButton).enabled = false; //버튼 활성화,
        }
        else 
        {
            GetButton((int)Buttons.ClaimButton).GetComponent<Image>().color = Util.HexToColor("989898");
            _isClaim = false;
            //GetButton((int)Buttons.ClaimButton).interactable = true; //버튼 비활성화,
        }

        UI_MaterialItem item = Managers.UI.MakeSubItem<UI_MaterialItem>(container.transform);
        int count = (_offlineRewardData.Reward_Gold) * 5;
        item.SetInfo(Define.GOLD_SPRITE_NAME, count);

        UI_MaterialItem scroll = Managers.UI.MakeSubItem<UI_MaterialItem>(container.transform);
        scroll.SetInfo("Scroll_Random_Icon", _offlineRewardData.FastReward_Scroll);

        UI_MaterialItem box = Managers.UI.MakeSubItem<UI_MaterialItem>(container.transform);
        box.SetInfo("Key_Silver_Icon", _offlineRewardData.FastReward_Box);
        //TODO

        GetText((int)Texts.EemainingCountValueText).text = Managers.Game.FastRewardCountStamina.ToString();
        // 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetButton((int)Buttons.ADFreeButton).gameObject.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetButton((int)Buttons.ClaimButton).gameObject.GetComponent<RectTransform>());
    }

    void OnClickBackgroundButton() // 배경 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickADFreeButton() // 광고보기 버튼
    {
        Managers.Sound.PlayButtonClick();

        if (Managers.Game.FastRewardCountAds > 0)
        {
            Managers.Game.FastRewardCountAds--;
            Managers.Ads.ShowRewardedAd(() =>
            {
                Managers.Time.GiveFastOfflineReward(_offlineRewardData);
                Managers.UI.ClosePopupUI(this);
            });
        }
        else
        {
            Managers.UI.ShowToast("더이상 받을 수 없습니다."); 
        }
        
    }
    void OnClickClaimButton() // 확인 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.Stamina >= 15 && Managers.Game.FastRewardCountStamina > 0 && _isClaim)
        {
            Managers.Game.Stamina -= 15;
            Managers.Game.FastRewardCountStamina--;
            // 스테미나를 소모하고 팝업을 닫기 (이후 리워드 팝업으로 보상 받기)
            Managers.Time.GiveFastOfflineReward(_offlineRewardData);
            Managers.UI.ClosePopupUI(this);
            Refresh();
        }
        else
            return;
       
    }
}
