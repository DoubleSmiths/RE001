using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Data;
using System.Linq;

public class UI_AchievementPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // AchievementScrollObject : 업적용  AchievementItme이 들어갈 부모 개체

    // 로컬라이징
    // BackgroundText : 터치하여 닫기
    // AchievementTitleText : 업적

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        AchievementScrollObject,
    }
    enum Buttons
    {
        BackgroundButton,
    }
    enum Texts
    {
        BackgroundText,
        AchievementTitleText,
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


        // 테스트용
#if UNITY_EDITOR

#endif
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
        if (_init == false)
            return;

        GetObject((int)GameObjects.AchievementScrollObject).DestroyChilds();

        foreach (AchievementData data in Managers.Achievement.GetProceedingAchievment())
        {
            UI_AchievementItem item = Managers.UI.MakeSubItem<UI_AchievementItem>(GetObject((int)GameObjects.AchievementScrollObject).transform);
            item.SetInfo(data);
        }
    }
    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.UI.ClosePopupUI(this);
    }

}
