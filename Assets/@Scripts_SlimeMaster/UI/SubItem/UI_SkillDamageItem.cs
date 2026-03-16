using UnityEngine;
using UnityEngine.UI;

public class UI_SkillDamageItem : UI_Base
{

    #region UI 기능 리스트
    // 정보 갱신
    // TargetImage : 스킬 아이콘
    // SkillNameValueText : 스킬 이름
    // SkillDamageValueText : 스킬의 딜량
    // DamageProbabilityValueText : 전체 중 딜량의 비율
    // DamageSliderObject 전체 딜량의 비율을 나타낼 슬라이드

    #endregion

    #region Enum
    enum GameObjects
    {
        DamageSliderObject
    }

    enum Texts
    {
        SkillNameValueText,
        SkillDamageValueText,
        DamageProbabilityValueText,
    }

    enum Images
    {
        SkillImage,
    }
    #endregion
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

        #endregion

        Refresh();
        return true;
    }

    public void SetInfo(SkillBase skill)
    { 
        GetImage((int)Images.SkillImage).sprite = Managers.Resource.Load<Sprite>(skill.SkillData.IconLabel);
        GetText((int)Texts.SkillNameValueText).text = $"{skill.SkillData.Name}";
        GetText((int)Texts.SkillDamageValueText).text = $"{skill.TotalDamage}";

        float allSkillDamage = Managers.Game.GetTotalDamage();
        float percentage = skill.TotalDamage / Managers.Game.GetTotalDamage();
        
        //총 스킬 데미지가 0일때 100%로 표기
        if (allSkillDamage == 0)
            percentage = 1;

        GetText((int)Texts.DamageProbabilityValueText).text = (percentage*100).ToString("F2")+"%";       
        GetObject((int)GameObjects.DamageSliderObject).GetComponent<Slider>().value = percentage;

        //transform.localScale = Vector3.one;
        Refresh();
    }

    void Refresh()
    {


    }

}
