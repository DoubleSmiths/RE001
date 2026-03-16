using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_AccountPassItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // AccountLevelValueText : 레벨 조건

    // FreePassRewardItmeImage : 무료 리워드 아이템 이미지
    // RarePassRewardItmeImage : 레어 리워드 아이템 이미지
    // EpicPassRewardItmeImage : 에픽 리워드 아이템 이미지

    // FreePassRewardItmeValueText : 무료 리워드 아이템 개수
    // RarePassRewardItmeValueText : 레어 리워드 아이템 개수
    // EpicPassRewardItmeValueText : 에픽 리워드 아이템 개수

    // FreePassRewardLockObject : 레벨 조건 (계정레벨) 달성 시 비활성화
    // RarePassRewardLockObject : 레벨 조건 (계정레벨) 달성 시 비활성화
    // EpicPassRewardLockObject : 레벨 조건 (계정레벨) 달성 시 비활성화

    #endregion


    #region Enum
    enum GameObjects
    {
        FreePassRewardCompleteObject,
        RarePassRewardCompleteObject,
        EpicPassRewardCompleteObject,

        FreePassRewardLockObject,
        RarePassRewardLockObject,
        EpicPassRewardLockObject,
    }

    enum Buttons
    {
        FreePassRewardButton,
        RarePassRewardButton,
        EpicPassRewardButton,
    }

    enum Texts
    {
        AccountLevelValueText,
        FreePassRewardItmeValueText,
        RarePassRewardItmeValueText,
        EpicPassRewardItmeValueText,
    }

    enum Images
    {
        FreePassRewardItmeImage,
        RarePassRewardItmeImage,
        EpicPassRewardItmeImage,
    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.FreePassRewardButton).gameObject.BindEvent(OnClickFreePassRewardButton);
        GetButton((int)Buttons.FreePassRewardButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.RarePassRewardButton).gameObject.BindEvent(OnClickRarePassRewardButton);
        GetButton((int)Buttons.RarePassRewardButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.EpicPassRewardButton).gameObject.BindEvent(OnClickEpicPassRewardButton);
        GetButton((int)Buttons.EpicPassRewardButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetObject((int)GameObjects.FreePassRewardLockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.RarePassRewardLockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.EpicPassRewardLockObject).gameObject.SetActive(true);

        GetObject((int)GameObjects.FreePassRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.RarePassRewardCompleteObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.EpicPassRewardCompleteObject).gameObject.SetActive(false);

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


    }

    void OnClickFreePassRewardButton()
    {
        // 무료 보상 지급 시 활성화
        // GetObject((int)GameObjects.FreePassRewardLockObject).gameObject.SetActive(true);
    }
    void OnClickRarePassRewardButton()
    {
        // 레어 보상 지급 시 활성화
        // GetObject((int)GameObjects.RarePassRewardLockObject).gameObject.SetActive(true);

    }
    void OnClickEpicPassRewardButton()
    {
        // 에픽 보상 지급 시 활성화
        // GetObject((int)GameObjects.EpicPassRewardLockObject).gameObject.SetActive(true);
    }
}
