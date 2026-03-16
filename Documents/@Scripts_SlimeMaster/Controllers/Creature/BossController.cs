    using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class BossController : MonsterController
{
    public float chargingTime = 1f;
    public float rushingTime = 2.0f;
    public float attackingTime = 1.0f;
    private Queue<SkillBase> _skillQueue;

    public Vector2 DashPoint { get; set; }

    public override bool Init()
    {
        base.Init();
        transform.localScale = new Vector3(2f, 2f, 2f);
        //SpriteRenderer = GetComponent<SpriteRenderer>();
        ObjectType = ObjectType.Boss;
        CreatureState = Define.CreatureState.Skill;

        return true;
    }

    public void Start()
    {
        Init();
        CreatureState = Define.CreatureState.Skill;
        Skills.StartNextSequenceSkill();
        InvokeMonsterData();
    }

    public override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case Define.CreatureState.Idle:
                Anim.Play("Idle");
                break;
            case Define.CreatureState.Moving:
                Anim.Play("Move");
                break;
            case Define.CreatureState.Skill:
                break;
            case Define.CreatureState.Dead:
                Skills.StopSkills();
                break;
        }
    }
    public override void InitCreatureStat(bool isFullHp = true)
    {
        //보스, 플레이어빼고  엘리트, 몬스터만
        MaxHp = (CreatureData.MaxHp + (CreatureData.MaxHpBonus * Managers.Game.CurrentStageData.StageLevel)) * CreatureData.HpRate;
        Atk = (CreatureData.Atk + (CreatureData.AtkBonus * Managers.Game.CurrentStageData.StageLevel)) * CreatureData.AtkRate;
        Hp = MaxHp;
        MoveSpeed = CreatureData.MoveSpeed * CreatureData.MoveSpeedRate;
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        PlayerController target = collision.gameObject.GetComponent<PlayerController>();

        if (target.IsValid() == false)
            return;
        if (this.IsValid() == false)
            return; ;

    }

    public override void OnCollisionExit2D(Collision2D collision)
    {
        base.OnCollisionExit2D(collision);
        PlayerController target = collision.gameObject.GetComponent<PlayerController>();

        if (target.IsValid() == false)
            return;
        if (this.IsValid() == false)
            return; ;
    }

}

