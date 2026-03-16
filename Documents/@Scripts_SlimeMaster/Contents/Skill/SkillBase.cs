using Data;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static Define;
[Serializable]
public class SkillStat
{
    public Define.SkillType SkillType;
    public int Level;
    public float MaxHp;
    public Data.SkillData SkillData;
}
public class SkillBase : BaseController
{
    public CreatureController Owner { get; set; }
    Define.SkillType skillType;
    public Define.SkillType SkillType
    {
        get
        {
            return skillType;
        }
        set
        {
            skillType = value;
        }
    }

    #region Level
    int level = 0;
    public int Level
    {
        get { return level; }
        set { level = value; }
    }
    #endregion
    #region skillData
    [SerializeField]
    public Data.SkillData _skillData;
    public Data.SkillData SkillData 
    {
        get
        { 
            return _skillData;
        }
        set 
        { 
            _skillData = value;
        }
    }
    #endregion

    public float TotalDamage { get; set; } = 0;
    public bool IsLearnedSkill { get { return Level > 0; } }

    public Data.SkillData UpdateSkillData(int dataId = 0)
    {

        int id = 0;
        if (dataId == 0)
            id = Level < 2 ? (int)SkillType : (int)SkillType + Level - 1;
        else
            id = dataId;

        SkillData skillData = new Data.SkillData();

        if (Managers.Data.SkillDic.TryGetValue(id, out skillData) == false)
            return SkillData;

        foreach (SupportSkillData support in Managers.Game.Player.Skills.SupportSkills)
        {
            if (SkillType.ToString() == support.SupportSkillName.ToString())
            {
                skillData.ProjectileSpacing += support.ProjectileSpacing;
                skillData.Duration += support.Duration;
                skillData.NumProjectiles += support.NumProjectiles;
                skillData.AttackInterval += support.AttackInterval;
                skillData.NumBounce += support.NumBounce;
                skillData.ProjRange += support.ProjRange;
                skillData.RoatateSpeed += support.RoatateSpeed;
                skillData.ScaleMultiplier += support.ScaleMultiplier;
                skillData.NumPenerations += support.NumPenerations;
            }
        }

        SkillData = skillData;
        OnChangedSkillData();
        return SkillData;
    }

    public virtual void OnChangedSkillData() { }

    // 레벨 0에서 1될때만 실행
    public virtual void ActivateSkill()
    {
        UpdateSkillData();
    }

    public virtual void OnLevelUp()
    {
        if (Level == 0)
            ActivateSkill();
        Level++;
        UpdateSkillData();
        //Debug.Log($"@>> Skill Level up : {SkillType.ToString()} -> Level {Level}, {SkillData.DataId}");
    }

    protected virtual void GenerateProjectile(CreatureController Owner, string prefabName, Vector3 startPos, Vector3 dir, Vector3 targetPos, SkillBase skill)
    {
        ProjectileController pc = Managers.Object.Spawn<ProjectileController>(startPos, prefabName: prefabName);
        pc.SetInfo(Owner, startPos, dir, targetPos, skill);
    }

    protected void HitEvent(Collider2D collision)
    {

    }

}
