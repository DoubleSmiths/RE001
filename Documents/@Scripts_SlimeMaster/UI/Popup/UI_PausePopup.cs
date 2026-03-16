using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class UI_PausePopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // BattleSkillSlotGroupObject에 현재 보유하고 있는 전투 스킬 표시
    // SupportSkillSlotGroupObject에 현재 보유하고 있는 서포트 스킬 표시

    // 로컬라이징
    // PauseTitleText : 일시정지
    #endregion


    #region Enum

    enum GameObjects
    {
        ContentObject,
        //BattleSkillSlotGroupObject, // 배틀 스킬 슬롯
        //SupportSkillSlotGroupObject, // 서포트 스킬 슬롯
    }
    enum Buttons
    {
        ResumeButton,
        HomeButton,
        StatisticsButton,
        SoundButton,
        SettingButton,
    }


   enum Texts
    {
        PauseTitleText,
        //OwnBattleSkillInfoText,
        //OwnSupportSkillInfoText,
        ResumeButtonText
    }

  
    #endregion

    SkillBase skill;
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
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.HomeButton).gameObject.BindEvent(OnClickHomeButton);
        GetButton((int)Buttons.HomeButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ResumeButton).gameObject.BindEvent(OnClickResumeButton);
        GetButton((int)Buttons.ResumeButton).GetOrAddComponent<UI_ButtonAnimation>(); 
        GetButton((int)Buttons.SettingButton).gameObject.BindEvent(OnClickSettingButton);
        GetButton((int)Buttons.SettingButton).GetOrAddComponent<UI_ButtonAnimation>(); 
        GetButton((int)Buttons.SoundButton).gameObject.BindEvent(OnClickSoundButton);
        GetButton((int)Buttons.SoundButton).GetOrAddComponent<UI_ButtonAnimation>(); 
        GetButton((int)Buttons.StatisticsButton).gameObject.BindEvent(OnClickStatisticsButton);
        GetButton((int)Buttons.StatisticsButton).GetOrAddComponent<UI_ButtonAnimation>();

#if UNITY_EDITOR
        // 테스트용

#endif
        #endregion
        //SetBattleSkill();
        return true;
    }


    //public void SetBattleSkill()
    //{

    //    GameObject container = GetObject((int)GameObjects.BattleSkillSlotGroupObject);
    //    GameObject container2 = GetObject((int)GameObjects.SupportSkillSlotGroupObject);
    //    //초기화
    //    foreach (Transform child in container.transform)
    //        Managers.Resource.Destroy(child.gameObject);

    //    foreach (Transform child in container2.transform)
    //        Managers.Resource.Destroy(child.gameObject);

    //    //전투스킬
    //    foreach (SkillBase skill in Managers.Game.Player.Skills.ActivatedSkills)
    //    {
    //        UI_SkillSlotItem item = Managers.UI.MakeSubItem<UI_SkillSlotItem>(container.transform);
    //        item.GetComponent<UI_SkillSlotItem>().SetUI(skill.SkillData.IconLabel, skill.Level);
    //    }
        
    //    ////서포트스킬
    //    //foreach (SupportSkillData skill in Managers.Game.Player.Skills.SupportSkills)
    //    //{
    //    //    UI_SkillSlotItem item = Managers.UI.MakeSubItem<UI_SkillSlotItem>(container2.transform);
    //    //    item.GetComponent<UI_SkillSlotItem>().SetUI(skill.IconLabel);
    //    //}
    //}

    void OnClickResumeButton() // 되돌아가기 버튼
    {
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickHomeButton() // 로비 버튼
    {
        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_BackToHomePopup>();
    }

    void OnClickSettingButton() // 설정 버튼
    {
        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_SettingPopup>();
    }
    void OnClickSoundButton() // 사운드 버튼
    {
        Managers.Sound.PlayButtonClick();
    }
    void OnClickStatisticsButton() // 통계 버튼
    {
        Managers.Sound.PlayButtonClick();
        // 통계 팝업 호출(아직 안만듬)
        Managers.UI.ShowPopupUI<UI_TotalDamagePopup>().SetInfo();
    }

}
