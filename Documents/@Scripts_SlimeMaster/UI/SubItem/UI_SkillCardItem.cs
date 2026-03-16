using UnityEngine;
using static Define;

public class UI_SkillCardItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // SkillCardBackgroundImage : 스킬 종류에 따라 배경 색깔 변경
    // - 배틀 스킬 : #FF4E4E
    // - 서포트 스킬 : #B7FF23
    // NewIImageObject : 새로 입수한 스킬이라면 활성화
    // EvoSkillImage : 조합할 스킬 아이콘 표시
    // StarOn_0 ~ 4 : 스킬 레벨에 따라 활성화
    #endregion

    #region Enums
    enum GameObjects
    {
        UI_SkillCardItem,
        NewIImageObject,
    }

    enum Texts
    {
        SkillDescriptionText,
        CardNameText
    }

    enum Images
    {
        SkillCardBackgroundImage,
        SkillImage,
        //EvoSkillImage,
        StarOn_0,
        StarOn_1,
        StarOn_2,
        StarOn_3,
        StarOn_4,
        StarOn_5,
    }
    #endregion

    private SkillBase _skill;

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
        gameObject.BindEvent(OnClicked);
        #endregion

        return true;
    }

    public void SetInfo(SkillBase skill)
    {
        transform.localScale = Vector3.one;
        GetObject((int)GameObjects.NewIImageObject).gameObject.SetActive(false);

        _skill = skill;
        GetImage((int)Images.SkillImage).sprite = Managers.Resource.Load<Sprite>(skill.UpdateSkillData().IconLabel);
        GetText((int)Texts.CardNameText).text = _skill.SkillData.Name;
        GetText((int)Texts.SkillDescriptionText).text = _skill.SkillData.Description;

        GetImage((int)Images.StarOn_1).gameObject.SetActive(_skill.Level + 1 >= 2);
        GetImage((int)Images.StarOn_2).gameObject.SetActive(_skill.Level + 1 >= 3);
        GetImage((int)Images.StarOn_3).gameObject.SetActive(_skill.Level + 1 >= 4);
        GetImage((int)Images.StarOn_4).gameObject.SetActive(_skill.Level + 1 >= 5);
        GetImage((int)Images.StarOn_5).gameObject.SetActive(_skill.Level + 1 >= 6);
    }

    public void OnClicked()
    {
        Managers.Game.Player.Skills.LevelUpSkill(_skill.SkillType);
        Managers.UI.ClosePopupUI();
    }
}
