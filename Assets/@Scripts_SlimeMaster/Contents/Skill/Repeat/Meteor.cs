using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class Meteor : RepeatSkill
{
    private void Awake()
    {
        SkillType = Define.SkillType.Meteor;
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
    }

    public override void OnChangedSkillData()
    {
    }

    IEnumerator GenerateMeteor()
    {
        List<MonsterController> targets = Managers.Object.GetMonsterWithinCamera(SkillData.NumProjectiles);
        if (targets == null)
            yield break;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].IsValid() == true)
            { 
                Vector2 startPos = GetMeteorPositgion(targets[i].CenterPosition);
                GenerateProjectile(Managers.Game.Player, "MeteorProjectile", startPos, Vector3.zero, targets[i].CenterPosition, this);
                yield return new WaitForSeconds(SkillData.AttackInterval);
            }
        }
    }

    public Vector2 GetMeteorPositgion(Vector3 target)
    {
        float angleInRadians = 60f * Mathf.Deg2Rad;
        float spawnMargin = 1f;
        // 화면의 높이 절반
        float halfHeight = Camera.main.orthographicSize;
        // 화면의 너비 절반
        float halfWidth = Camera.main.aspect * halfHeight;

        float spawnX = target.x + (halfWidth + spawnMargin) * Mathf.Cos(angleInRadians);
        float spawnY = target.y + (halfHeight + spawnMargin) * Mathf.Sin(angleInRadians);
        Vector2 spawnPosition = new Vector2(spawnX, spawnY);

        return spawnPosition;
    }

    protected override void DoSkillJob()
    {
        StartCoroutine(GenerateMeteor());
    }
}
