using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class UI_CheckOutItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // DayValueText : 출석 체크 일자 ( 일자 + " Day" )
    // RewardItemBackgroundImage : 출석 보상 아이템 등급에 따라 색상 변경
    // RewardItemImage : 보상 아이템 이미지
    // RewardItemCountValueText : 보상 개수

    // ClearOutlineObject : 보상을 받을 수 있다면 활성화
    // ClearRewardCompleteObject : 보상을 받았다면 활성화

    #endregion

    #region Enums
    enum GameObjects
    {
        ClearRewardCompleteObject,
    }

    enum Texts
    {
        DayValueText,
        RewardItemCountValueText,
    }

    enum Images
    {
        RewardItemBackgroundImage,
        RewardItemImage,
    }

    #endregion

    int _dayCount;
    bool _isCheckOut;
    private void OnEnable()
    {
        Init();
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetObject((int)GameObjects.ClearRewardCompleteObject).gameObject.SetActive(false);

        #endregion

        Refresh();
        return true;
    }


    public void SetInfo(int dayCount, bool isCheckOut)
    {
        transform.localScale = Vector3.one;

        _dayCount = dayCount;
        _isCheckOut = isCheckOut;
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;

        if (_dayCount == 0)
            return;

        // 출석일 정보 리프레쉬
        int rewardMaterialId = Managers.Data.CheckOutDataDic[_dayCount].RewardItemId;
        int rewardItemValue = Managers.Data.CheckOutDataDic[_dayCount].MissionTarRewardItemValuegetValue;
        GetText((int)Texts.DayValueText).text = $"{_dayCount} 일";
        GetText((int)Texts.RewardItemCountValueText).text = $"{rewardItemValue}";
        GetImage((int)Images.RewardItemImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.MaterialDic[rewardMaterialId].SpriteName);
        switch (Managers.Data.MaterialDic[rewardMaterialId].MaterialGrade)
        {
            case MaterialGrade.Common:
                GetImage((int)Images.RewardItemBackgroundImage).color = EquipmentUIColors.Common;
                break;
            case MaterialGrade.Uncommon:
                GetImage((int)Images.RewardItemBackgroundImage).color = EquipmentUIColors.Uncommon;
                break;
            case MaterialGrade.Rare:
                GetImage((int)Images.RewardItemBackgroundImage).color = EquipmentUIColors.Rare;
                break;
            case MaterialGrade.Epic:
                GetImage((int)Images.RewardItemBackgroundImage).color = EquipmentUIColors.Epic;
                break;
            case MaterialGrade.Legendary:
                GetImage((int)Images.RewardItemBackgroundImage).color = EquipmentUIColors.Legendary;
                break;

            default:
                break;
        }

        if (_isCheckOut)
        {
            GetObject((int)GameObjects.ClearRewardCompleteObject).gameObject.SetActive(true);

            if (Managers.Game.AttendanceReceived[_dayCount - 1] == false)
            {
                Managers.Game.AttendanceReceived[_dayCount - 1] = true;

                int matId = Managers.Data.CheckOutDataDic[_dayCount].RewardItemId;

                string[] spriteName = new string[1];
                int[] count = new int[1];

                spriteName[0] = Managers.Data.MaterialDic[matId].SpriteName;
                count[0] = Managers.Data.CheckOutDataDic[_dayCount].MissionTarRewardItemValuegetValue;

                UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
                rewardPopup.gameObject.SetActive(true);
                Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[matId], Managers.Data.CheckOutDataDic[_dayCount].MissionTarRewardItemValuegetValue);
                rewardPopup.SetInfo(spriteName, count);
                Managers.Game.SaveGame();
            }
        }
        else
        {
            GetObject((int)GameObjects.ClearRewardCompleteObject).gameObject.SetActive(false);
        }
    }

}
