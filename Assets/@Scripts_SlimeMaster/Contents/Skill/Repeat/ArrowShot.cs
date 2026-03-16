using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowShot : RepeatSkill
{
    private void Awake()
    {
        SkillType = Define.SkillType.ArrowShot;
    }

    public override void OnChangedSkillData()
    {
    }

    IEnumerator SetArrowShot()
    { 
        string prefabName = SkillData.PrefabLabel;

        if (Managers.Game.Player != null)
        {
            List<MonsterController> target = Managers.Object.GetNearestMonsters(SkillData.NumProjectiles);
            if (target == null)
                yield break;

            for (int i = 0; i < target.Count; i++)
            {
                Vector3 dir = Managers.Game.Player.PlayerDirection;
                Vector3 startPos = Managers.Game.Player.CenterPosition;
                GenerateProjectile(Managers.Game.Player, prefabName, startPos, dir, Vector3.zero, this);
                yield return new WaitForSeconds(SkillData.ProjectileSpacing);
            }
        }
    }


    protected override void DoSkillJob()
    {
        StartCoroutine(SetArrowShot());
    }
}
