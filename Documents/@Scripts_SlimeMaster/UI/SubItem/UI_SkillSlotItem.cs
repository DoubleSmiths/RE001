using UnityEngine;
using static Define;

public class UI_SkillSlotItem : UI_Base
{
    #region Enums
    enum SkillLevelObjects
    {
        SkillLevel_1,
        SkillLevel_2,
        SkillLevel_3,
        SkillLevel_4,
        SkillLevel_5,
        SkillLevel_6,
    }
    enum Texts
    {
        SkillDescriptionText
    }

    enum Images
    {
        BattleSkilIImage,
    }
    #endregion

    string _iconLabel;
    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindObject(typeof(SkillLevelObjects));
        BindImage(typeof(Images));
        gameObject.GetComponent<RectTransform>().localScale = Vector3.one;


        return true;
    }

    public void SetUI(string iconLabel, int skillLevel = 1)
    {
        GetImage((int)Images.BattleSkilIImage).sprite = Managers.Resource.Load<Sprite>(iconLabel);

        //별 모두 끄기
        for (int i = 0; i < 6; i++)
        {
            GetObject(i).SetActive(false);
        }

        //스킬레벨만큼 별 켜기
        for (int i = 0; i < skillLevel; i++)
            GetObject(i).SetActive(true);


    }

}
