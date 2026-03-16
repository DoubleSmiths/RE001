using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class EnergyBolt : RepeatSkill
{
    private void Awake()
    {
        SkillType = Define.SkillType.EnergyBolt;
    }

    public override void OnChangedSkillData()
    {
    }

    IEnumerator SetEnergeBolt()
    {
        string prefabName = SkillData.PrefabLabel;

        if (Managers.Game.Player != null)
        {
            List<MonsterController> target = Managers.Object.GetNearestMonsters(SkillData.NumProjectiles);
            if(target == null)
                yield break;

            for (int i = 0; i < target.Count; i++)
            {
                Vector3 dir = (target[i].CenterPosition - Managers.Game.Player.CenterPosition).normalized;
                Vector3 startPos = Managers.Game.Player.CenterPosition;
                GenerateProjectile(Managers.Game.Player, prefabName, startPos, dir, Vector3.zero, this);
                yield return new WaitForSeconds(SkillData.ProjectileSpacing);
            }
        }
    }
    //Old버전 보류
    //IEnumerator SetEnergeBolt()
    //{
    //    string prefabName = SkillData.PrefabLabel;

    //    yield return new WaitForSeconds(SkillData.CoolTime);// attackspeed

    //    if (Managers.Game.Player != null)
    //    {
    //        List<MonsterController> target = Managers.Object.GetNearestMonsters(1);
    //        if (target[0] != null)
    //        {

    //            for (int i = 0; i < SkillData.NumProjectiles; i++)
    //            {
    //                Vector3 dir = target[0].CenterPosition - Managers.Game.Player.CenterPosition;
    //                Vector3 startPos = Managers.Game.Player.CenterPosition;
    //                GenerateProjectile(Managers.Game.Player, prefabName, startPos, dir, Vector3.zero, this);
    //                yield return new WaitForSeconds(SkillData.ProjectileSpacing);
    //            }
    //        }
    //    }
    //}

    protected override void DoSkillJob()
    {
        StartCoroutine(SetEnergeBolt());
    }
}