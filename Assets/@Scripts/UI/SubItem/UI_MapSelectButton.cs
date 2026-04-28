using UnityEngine;

public class UI_MapSelectButton : UI_Base
{
    #region Enum

    enum Images
    {
        MapSelectButtonIconFrame,
        MapSelectButtonIcon,
        MapSelectButtonQuestVFXIcon,
        EFrame,
    }

    enum Texts
    {
        MapSelectButtonText,
    }

    enum Buttons
    {
        UI_MapSelectButton,
    }

    enum GameObjects
    {
        MapSelectButtonQuestVFX,
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

        // TODO : 퀘스트 VFX는 퀘스트가 있을 때만 활성화
        // TODO : 한/영 텍스트는 언어 설정에 따라 변경
        // TODO : 맵 아이콘은 맵에 따라 변경
        // TODO : 버튼 선택이 되어있을 때 버튼 프레임 색상 변경, 선택이 해제되었을 때 버튼 프레임 색상 원래대로
        // TODO : 버튼 선택이 되어있을 때 EFrame 활성화, 선택이 해제되었을 때 EFrame 비활성화
        // TODO : 버튼 클릭 시 맵 선택 UI에서 해당 맵으로 이동

        return true;
    }


    public void SetInfo()
    {

    }
}
