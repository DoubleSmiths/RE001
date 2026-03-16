using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UI_TotalDamagePopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // TotalDamageContentObject : 가 들어갈 부모개체

    // 로컬라이징
    // BackgroundText : 탭하여 닫기
    // TotalDamagePopupTitleText : 총 데미지
    #endregion


    enum GameObjects
    {
        ContentObject,
        TotalDamageContentObject,
    }

    enum Buttons
    {
        BackgroundButton
    }

    enum Texts
    {
        BackgroundText,
        TotalDamagePopupTitleText,
    }

    enum Images
    {

    }

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
        BindImage(typeof(Images));


        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickClosePopup);

#if UNITY_EDITOR
        // 테스트용
        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    public void SetInfo()
    {
        GetObject((int)GameObjects.TotalDamageContentObject).DestroyChilds();
        List<SkillBase> skillList = Managers.Game.Player.Skills.SkillList.ToList();
        foreach (SkillBase skill in skillList.FindAll(skill => skill.IsLearnedSkill))
        {
            UI_SkillDamageItem item = Managers.UI.MakeSubItem<UI_SkillDamageItem>(GetObject((int)GameObjects.TotalDamageContentObject).transform);
            item.SetInfo(skill);
            item.transform.localScale = Vector3.one;
        }


        Refresh();
    }

    void Refresh()
    {

    }
    public void OnClickClosePopup()
    {
        Managers.UI.ClosePopupUI(this);
    }

}
