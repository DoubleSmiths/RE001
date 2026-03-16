using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinShot : SequenceSkill
{
    public float RotateSpeed =10;

    public GameObject Bullet;
    CreatureController _owner;
    public float SpawnInterval = 0.02f;
    private float _spawnTimer;
    private float _launchCount = 0;
    Vector3 dir;
    private void Awake()
    {
        SkillType = Define.SkillType.SpinShot;
        AnimagtionName = "Attack";
        _owner = GetComponent<CreatureController>();
    }

    public override void DoSkill(Action callback = null)
    {
        CreatureController owner = GetComponent<CreatureController>();
        if (owner.CreatureState != Define.CreatureState.Skill)
            return;

        UpdateSkillData(DataId);

        dir = Managers.Game.Player.CenterPosition - _owner.CenterPosition;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        transform.GetChild(0).GetComponent<Animator>().Play(AnimagtionName);
        _coroutine = null;
        _coroutine = StartCoroutine(CoSkill(callback));
    }

    Coroutine _coroutine;
    IEnumerator CoSkill(Action callback = null)
    {
        while (true)
        {
            dir = Quaternion.Euler(0, 0, SkillData.RoatateSpeed ) * dir;
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < SpawnInterval) continue;

            _spawnTimer = 0f;

            Vector3 startPos = _owner.CenterPosition;
            GenerateProjectile(_owner, SkillData.PrefabLabel, startPos, dir.normalized, Vector3.zero, this);
            _launchCount++;

            if (_launchCount > SkillData.NumProjectiles)
            {
                _launchCount = 0;
                break;
            }

            yield return new WaitForFixedUpdate();
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
