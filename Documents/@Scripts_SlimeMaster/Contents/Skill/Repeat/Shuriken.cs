using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Shuriken : RepeatSkill
{

    private void Awake()
    {
        SkillType = Define.SkillType.Shuriken;
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
    }

    public override void OnChangedSkillData()
    {
    }

    IEnumerator SetShuriken(Define.SkillType type)
    {
        string prefabName = SkillData.PrefabLabel;

        if (Managers.Game.Player != null)
        {
            List<MonsterController> target = Managers.Object.GetMonsterWithinCamera(SkillData.NumProjectiles);

            if (target == null) 
                yield break;
            for (int i = 0; i < target.Count; i++)
            {
                if (target != null)
                {
                    if (target[i].IsValid() == false)
                        continue;
                    Vector3 dir = target[i].CenterPosition - Managers.Game.Player.CenterPosition;
                    Vector3 startPos = Managers.Game.Player.CenterPosition;
                    GenerateProjectile(Managers.Game.Player, prefabName, startPos, dir.normalized, Vector3.zero, this);
                }

                yield return new WaitForSeconds(SkillData.ProjectileSpacing);
            }
        }
    }

    protected override void DoSkillJob()
    {
        StartCoroutine(SetShuriken(SkillType));
    }
}
