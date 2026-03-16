using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleShot : SequenceSkill
{
    public float RotateSpeed =10;

    public GameObject Bullet;
    CreatureController _owner;
    public float SpawnInterval = 0.02f;
    private Vector3 _dir;
    private void Awake()
    {
        SkillType = Define.SkillType.CircleShot;
        AnimagtionName = "Attack";
        _owner = GetComponent<CreatureController>();
    }

    public override void DoSkill(Action callback = null)
    {
        CreatureController owner = GetComponent<CreatureController>();
        if (owner.CreatureState != Define.CreatureState.Skill)
            return;

        UpdateSkillData(DataId);

        _dir = Managers.Game.Player.CenterPosition - _owner.CenterPosition;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        _coroutine = null;
        _coroutine = StartCoroutine(CoSkill(callback));
    }

    Coroutine _coroutine;
    IEnumerator CoSkill(Action callback = null)
    {
        Vector3 playerPosition = Managers.Game.Player.CenterPosition;
        float angleIncrement = 360f / SkillData.NumProjectiles;
        transform.GetChild(0).GetComponent<Animator>().Play(AnimagtionName);

        for (int i = 0; i < SkillData.NumProjectiles; i++)
        {
            // 1. 프로젝타일 발사 위치 계산하기
            float angle = i * angleIncrement;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.up;

            // 2. 프로젝타일 발사하기
            Vector3 startPos = _owner.CenterPosition + dir;
            GenerateProjectile(_owner, SkillData.PrefabLabel, startPos, dir.normalized, Vector3.zero, this);
        }
        yield return new WaitForSeconds(SkillData.AttackInterval);

        callback?.Invoke();
    }

    private void Update()
    {

    }

    public override void OnChangedSkillData()
    {
    }
}
