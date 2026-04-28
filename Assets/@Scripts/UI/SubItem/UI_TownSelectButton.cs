using UnityEngine;
using UnityEngine.UI;

public class UI_TownSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        TownSelectButtonIconFrame,
        TownSelectButtonIcon,
        TownSelectButtonQuestVFXIcon,
        EFrame,
    }

    enum Texts
    {
        TownSelectButtonText,
        TownSelectPopupCurrentLocationText,
    }

    enum Buttons
    {
        UI_TownSelectButton,
    }

    enum GameObjects
    {
        TownSelectButtonQuestVFX,
        TownSelectPopupActiveEf,
    }

    #endregion

    private void Awake()
    {
        Init();
    }

    override public bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // TODO : 버튼 클릭 이벤트 추가
        // TODO : 퀘스트 VFX 활성화/비활성화 로직 추가
        // TODO : 현재 위치 텍스트 업데이트 로직 추가
        // TODO : 타운 선택시만 EFrame 켜기
        // TODO : 한/영 텍스트 업데이트 로직 추가
        // TODO : 타운 선택 팝업 활성화/비활성화 로직 추가
        // TODO : 타운 선택 버튼 아이콘 업데이트 로직 추가
        // TODO : 타운 선택 버튼 프레임 업데이트 로직 추가

        return true;
    }

    public void SetInfo()
    {

    }
}
