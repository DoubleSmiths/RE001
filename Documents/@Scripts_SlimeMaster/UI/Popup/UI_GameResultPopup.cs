using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameResultPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // ResultStageValueText : 해당 스테이지 수
    // ResultSurvivalTimeValueText : 스테이지 클리어 까지 걸린 시간 ( mm:ss 로 표기)
    // ResultGoldValueText : 죽기전 까지 얻은 골드
    // ResultKillValueText : 죽기전 까지 킬 수
    // ResultRewardScrollContentObject : : 보상으로 얻게될 아이템이 들어갈 부모 개체
    // (골드, 경헌치, 아이템, 캐릭터 강화석 등을 보상으로)


    // 로컬라이징 텍스트
    // GameResultPopupTitleText : 게임 결과
    // ResultSurvivalTimeText : 생존 시간
    // ConfirmButtonText : OK

    #endregion


    #region Enum
    enum GameObjects
    {
        ContentObject,
        ResultRewardScrollContentObject,
        ResultGoldObject,
        ResultKillObject,
    }

    enum Texts
    {
        GameResultPopupTitleText,
        ResultStageValueText,
        ResultSurvivalTimeText,
        ResultSurvivalTimeValueText,
        ResultKillValueText,
        ConfirmButtonText,
        ResultGoldValueText
    }

    enum Buttons
    {
        StatisticsButton,
        ConfirmButton,
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
        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.StatisticsButton).gameObject.BindEvent(OnClickStatisticsButton);
        GetButton((int)Buttons.StatisticsButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ConfirmButton).gameObject.BindEvent(OnClickConfirmButton);
        GetButton((int)Buttons.ConfirmButton).GetOrAddComponent<UI_ButtonAnimation>();


#if UNITY_EDITOR

        //TextBindTest();
#endif
        Refresh();
        return true;
    }

    public void SetInfo()
    {

        Refresh();
    }

    void Refresh()
    {
        // ResultStageValueText : 해당 스테이지 수
        GetText((int)Texts.ResultStageValueText).text = $"{Managers.Game.CurrentStageData.StageIndex} STAGE";
        // ResultKillValueText : 죽기전 까지 킬 수
        GetText((int)Texts.ResultKillValueText).text = $"{Managers.Game.Player.KillCount}";
        GetText((int)Texts.ResultGoldValueText).text = $"{Managers.Game.CurrentStageData.ClearReward_Gold}";


        Managers.Game.Gold += Managers.Game.CurrentStageData.ClearReward_Gold;
        Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_RANDOM_SCROLL], Managers.Game.CurrentStageData.ClearReward_Gold);

        Transform container = GetObject((int)GameObjects.ResultRewardScrollContentObject).transform;
        container.gameObject.DestroyChilds();

        UI_MaterialItem item = Managers.UI.MakeSubItem<UI_MaterialItem>(container);
        item.SetInfo(Managers.Data.MaterialDic[Define.ID_RANDOM_SCROLL].SpriteName, Managers.Game.CurrentStageData.ClearReward_Gold);

        // 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.ResultGoldObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.ResultKillObject).GetComponent<RectTransform>());
    }

    void OnClickStatisticsButton() // 통계 버튼
    {
        Managers.Sound.PlayButtonClick();

        // 통계 팝업 호출
    }
    void OnClickConfirmButton() // 확인 버튼
    {
        Managers.Sound.PlayButtonClick();
        StageClearInfo info;
        if (Managers.Game.DicStageClearInfo.TryGetValue(Managers.Game.CurrentStageData.StageIndex, out info))
        {

                info.MaxWaveIndex = Managers.Game.CurrentWaveIndex;
                info.isClear = true;
                Managers.Game.DicStageClearInfo[Managers.Game.CurrentStageData.StageIndex] = info;
        }
        Managers.Game.ClearContinueData();
        Managers.Game.SetNextStage();
        Managers.Scene.LoadScene(Define.Scene.LobbyScene, transform);
    }
}
