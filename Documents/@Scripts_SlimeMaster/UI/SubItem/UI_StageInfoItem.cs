using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_StageInfoItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // StageValueText : 스테이지
    // StageNameValueText : 챕터 이름
    // StageImage : 챕터 이미지
    // MaxWaveValueText : 해당 스테이지의 최대 진행 웨이브 (진행 / 최대) 
    //

    // 로컬라이징
    // MaxWaveText : 최대 웨이브 :
    #endregion

    #region Enum
    enum GameObjects
    {
        MaxWaveGroupObject,

        FirstClearRewardLockObject,
        SecondClearRewardLockObject,
        ThirdClearRewardLockObject,

        FirstClearRewardCompleteObject,
        SecondClearRewardCompleteObject,
        ThirdClearRewardCompleteObject,
    }

    enum Texts
    {
        StageValueText,
        //StageNameValueText,
        MaxWaveText,
        MaxWaveValueText,
    }

    enum Images
    {
        StageImage,
        StageLockImage,
    }
    #endregion

    StageData _stageData;

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
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        ClearRewardCompleteInit();
        #endregion

        return true;
    }

    public void SetInfo(StageData data)
    {
        _stageData = data;
        transform.localScale = Vector3.one;

        Refresh();
    }

    void Refresh()
    {
        // StageValueText : 스테이지
        GetText((int)Texts.StageValueText).text = $"{_stageData.StageIndex} 스테이지";
        // StageNameValueText : 챕터 이름
        //GetText((int)Texts.StageNameValueText).text = $"{_stageData.StageName}";
        // StageImage : 챕터 이미지
        GetImage((int)Images.StageImage).sprite = Managers.Resource.Load<Sprite>(_stageData.StageImage);
        if (Managers.Game.DicStageClearInfo.TryGetValue(_stageData.StageIndex, out StageClearInfo info) == false)
            return;

        // 1. 최대 클리어 스테이지
        if (info.MaxWaveIndex > 0) 
        {
            GetImage((int)Images.StageLockImage).gameObject.SetActive(false);
            GetImage((int)Images.StageImage).color = Color.white;

            if (info.isClear == true) // 스테이지 완료
            {
                GetText((int)Texts.MaxWaveText).gameObject.SetActive(false);
                GetText((int)Texts.MaxWaveValueText).gameObject.SetActive(true);
                GetText((int)Texts.MaxWaveValueText).color = Util.HexToColor("60FF08");
                GetText((int)Texts.MaxWaveValueText).text = "스테이지 클리어";
            }
            else // 스테이지 진행중
            {
                GetText((int)Texts.MaxWaveText).gameObject.SetActive(true);
                GetText((int)Texts.MaxWaveValueText).gameObject.SetActive(true);
                GetText((int)Texts.MaxWaveValueText).color = Util.HexToColor("FFDB08");
                GetText((int)Texts.MaxWaveValueText).text = (info.MaxWaveIndex + 1).ToString();
            }

            GetObject((int)GameObjects.FirstClearRewardLockObject).gameObject.SetActive(false);
            GetObject((int)GameObjects.SecondClearRewardLockObject).gameObject.SetActive(false);
            GetObject((int)GameObjects.ThirdClearRewardLockObject).gameObject.SetActive(false);

            GetObject((int)GameObjects.FirstClearRewardCompleteObject).SetActive(info.isOpenFirstBox);
            GetObject((int)GameObjects.SecondClearRewardCompleteObject).SetActive(info.isOpenSecondBox);
            GetObject((int)GameObjects.ThirdClearRewardCompleteObject).SetActive(info.isOpenThirdBox);
        }
        else
        {
            //게임 처음 시작하고 스테이지창을 오픈 한 경우
            if (info.StageIndex == 1 && info.MaxWaveIndex == 0)
            {
                GetImage((int)Images.StageLockImage).gameObject.SetActive(false);
                GetImage((int)Images.StageImage).color = Color.white;

                GetText((int)Texts.MaxWaveText).gameObject.SetActive(false);
                GetText((int)Texts.MaxWaveValueText).gameObject.SetActive(true);
                GetText((int)Texts.MaxWaveValueText).color = Util.HexToColor("FFDB08");
                GetText((int)Texts.MaxWaveValueText).text = "기록 없음";

                GetObject((int)GameObjects.FirstClearRewardLockObject).gameObject.SetActive(false);
                GetObject((int)GameObjects.SecondClearRewardLockObject).gameObject.SetActive(false);
                GetObject((int)GameObjects.ThirdClearRewardLockObject).gameObject.SetActive(false);

                GetObject((int)GameObjects.FirstClearRewardCompleteObject).SetActive(info.isOpenFirstBox);
                GetObject((int)GameObjects.SecondClearRewardCompleteObject).SetActive(info.isOpenSecondBox);
                GetObject((int)GameObjects.ThirdClearRewardCompleteObject).SetActive(info.isOpenThirdBox);
            }
            // 새로운 스테이지
            if (Managers.Game.DicStageClearInfo.TryGetValue(_stageData.StageIndex - 1, out StageClearInfo PrevInfo) == false)
                return;
            if (PrevInfo.isClear == true)
            {
                GetImage((int)Images.StageLockImage).gameObject.SetActive(false);
                GetImage((int)Images.StageImage).color = Color.white;

                GetText((int)Texts.MaxWaveText).gameObject.SetActive(false);
                GetText((int)Texts.MaxWaveValueText).gameObject.SetActive(true);
                GetText((int)Texts.MaxWaveValueText).color = Util.HexToColor("FFDB08");
                GetText((int)Texts.MaxWaveValueText).text = "기록 없음";

                GetObject((int)GameObjects.FirstClearRewardLockObject).gameObject.SetActive(false);
                GetObject((int)GameObjects.SecondClearRewardLockObject).gameObject.SetActive(false);
                GetObject((int)GameObjects.ThirdClearRewardLockObject).gameObject.SetActive(false);

                GetObject((int)GameObjects.FirstClearRewardCompleteObject).SetActive(info.isOpenFirstBox);
                GetObject((int)GameObjects.SecondClearRewardCompleteObject).SetActive(info.isOpenSecondBox);
                GetObject((int)GameObjects.ThirdClearRewardCompleteObject).SetActive(info.isOpenThirdBox);
            }

        }

        // 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.MaxWaveGroupObject).GetComponent<RectTransform>());
    }

    void ClearRewardCompleteInit()
    {
        GetObject((int)GameObjects.FirstClearRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.SecondClearRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.ThirdClearRewardCompleteObject).gameObject.SetActive(false);

        GetObject((int)GameObjects.FirstClearRewardLockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.SecondClearRewardLockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.ThirdClearRewardLockObject).gameObject.SetActive(true);

        GetImage((int)Images.StageLockImage).gameObject.SetActive(true);
        GetImage((int)Images.StageImage).color = Util.HexToColor("3A3A3A");
        GetText((int)Texts.MaxWaveText).gameObject.SetActive(false);
        GetText((int)Texts.MaxWaveValueText).gameObject.SetActive(false);
    }

}
