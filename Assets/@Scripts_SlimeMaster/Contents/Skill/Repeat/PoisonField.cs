using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PoisonField : RepeatSkill
{
    public void Awake()
    {
        SkillType = Define.SkillType.PoisonField;
    }


    public override void OnLevelUp()
    {
        base.OnLevelUp();
    }

    public override void OnChangedSkillData()
    {
    }

    IEnumerator GeneratePoisonField()
    {
        Vector2 dir = Managers.Game.Player.PlayerDirection;
        for (int i = 0; i < SkillData.NumProjectiles; i++)
        {
            Vector3 pos = Quaternion.AngleAxis(SkillData.AngleBetweenProj * i, Vector3.forward) * dir * SkillData.ProjRange;

            Vector3 spawnPos = Managers.Game.Player.PlayerCenterPos;

            string prefabName = Level == 6 ? "PoisonFieldProjectile_Final" : "PoisonFieldProjectile";
            if (SkillData.NumProjectiles == 1)
                GenerateProjectile(Managers.Game.Player, prefabName, spawnPos, dir, spawnPos, this);
            else
                GenerateProjectile(Managers.Game.Player, prefabName, spawnPos, dir, spawnPos + pos, this);

            yield return new WaitForSeconds(SkillData.AttackInterval);
        }
    }

    protected override void DoSkillJob()
    {
        StartCoroutine(GeneratePoisonField());
    }
}
