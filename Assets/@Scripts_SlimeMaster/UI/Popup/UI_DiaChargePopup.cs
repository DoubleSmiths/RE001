using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_DiaChargePopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        ContentObject,
    }

    enum Buttons
    {
        BackgroundButton,
        BuyADButton,
    }

    enum Texts
    {
        BackgroundText,
        BuyADText,
        UI_DiaChargePopupTitleText,
        ADChargeValueText,
        ADRemainingValueText,
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
        GetButton((int)Buttons.BuyADButton).gameObject.BindEvent(OnClickBuyADButton);
        GetButton((int)Buttons.BuyADButton).GetOrAddComponent<UI_ButtonAnimation>();

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
        GetText((int)Texts.ADRemainingValueText).text = $"오늘 남은 횟수 : {Managers.Game.DiaCountAds}";
    }

    void OnClickBackgroundButton() // 배경 눌러 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickBuyADButton() // 광고보고 구매 버튼
    {
        Managers.Sound.PlayButtonClick();

        if (Managers.Game.DiaCountAds > 0)
        {
            Managers.Ads.ShowRewardedAd(() =>
            {
                string[] spriteName = new string[1];
                int[] count = new int[1];

                spriteName[0] = Managers.Data.MaterialDic[Define.ID_DIA].SpriteName;
                count[0] = 200;

                UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
                rewardPopup.gameObject.SetActive(true);
                Managers.Game.DiaCountAds--;
                Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_DIA], 200);
                Refresh();
                rewardPopup.SetInfo(spriteName, count);

            });

        }
    }
}
