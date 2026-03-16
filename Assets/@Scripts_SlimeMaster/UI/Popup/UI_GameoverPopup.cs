using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameoverPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // GameoverStageValueText : 해당 스테이지 수
    // GameoverLastWaveValueText : 죽기전 마지막 웨이브 수
    // GameoverGoldValueText : 죽기전 까지 얻은 골드
    // GameoverKillValueText : 죽기전 까지 킬 수

    // 로컬라이징 텍스트
    // GameoverPopupTitleText : 게임 오버
    // LastWaveText : 마지막 웨이브
    // ConfirmButtonText : OK

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        GameoverKillObject,
    }
    enum Texts
    {
        GameoverPopupTitleText,
        GameoverStageValueText,
        LastWaveText,
        GameoverLastWaveValueText,
        GameoverKillValueText,
        ConfirmButtonText,
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
        Managers.Sound.Play(Define.Sound.Effect, "PopupOpen_Gameover");
#if UNITY_EDITOR

        //TextBindTest();
#endif

        Refresh();
        return true;
    }

    public void SetInfo()
    {
        // GameoverStageValueText : 해당 스테이지 수
        GetText((int)Texts.GameoverStageValueText).text = $"{Managers.Game.CurrentStageData.StageIndex} STAGE";
        // GameoverLastWaveValueText : 죽기전 마지막 웨이브 수
        GetText((int)Texts.GameoverLastWaveValueText).text = $"{Managers.Game.CurrentWaveIndex + 1}";
        // GameoverKillValueText : 죽기전 까지 킬 수
        GetText((int)Texts.GameoverKillValueText).text = $"{Managers.Game.Player.KillCount}";

        Refresh();
    }

    void Refresh()
    {
        // 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.GameoverKillObject).GetComponent<RectTransform>());
    }

    void OnClickStatisticsButton() // 통계 버튼
    {
        Managers.Sound.PlayButtonClick();
        // 통계 팝업 호출p
        Managers.UI.ShowPopupUI<UI_TotalDamagePopup>().SetInfo();

    }
    void OnClickConfirmButton() // 확인 버튼
    {
        Managers.Sound.PlayButtonClick();

        StageClearInfo info;
        if (Managers.Game.DicStageClearInfo.TryGetValue(Managers.Game.CurrentStageData.StageIndex, out info))
        {
            // 기록 갱신
            if (Managers.Game.CurrentWaveIndex > info.MaxWaveIndex)
            {
                info.MaxWaveIndex = Managers.Game.CurrentWaveIndex;
                Managers.Game.DicStageClearInfo[Managers.Game.CurrentStageData.StageIndex] = info;
            }
        }

        Managers.Game.ClearContinueData();
        Managers.Scene.LoadScene(Define.Scene.LobbyScene, transform);
    }
}
