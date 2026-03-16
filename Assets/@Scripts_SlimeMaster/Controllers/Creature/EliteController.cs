using Data;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using static Define;
using static UnityEngine.GraphicsBuffer;
using Sequence = DG.Tweening.Sequence;

public class EliteController : MonsterController
{
    private void OnEnable()
    {
        if(DataId != 0)
            SetInfo(DataId);
    }

    public override bool Init()
    {
        base.Init();

        CreatureState = CreatureState.Moving;

        _rigidBody.simulated = true;
        transform.localScale = new Vector3(2f, 2f, 2f);

        ObjectType = ObjectType.EliteMonster;
        return true;
    }

    public void Start()
    {
        Init();
        InvokeMonsterData();
    }

    public override void OnDead()
    {
        base.OnDead();
        Managers.Game.Player.SkillRefreshCount++;
        Managers.Game.Player.Skills.OnEliteDeadBonus();
        if (Managers.Game.DicMission.TryGetValue(MissionTarget.EliteMonsterKill, out MissionInfo mission))
            mission.Progress++;
        Managers.Game.TotalEliteKillCount++;
        DropItem();
    }

    void DropItem()
    {
        int i = 0;
        foreach (int id in Managers.Game.CurrentWaveData.EliteDropItemId)
        {
            Data.DropItemData dropItem;
            if (Managers.Data.DropItemDataDic.TryGetValue(id, out dropItem) == true)
            {
                int dropCount = Managers.Game.CurrentWaveData.EliteDropItemId.Count;
                float angleInterval = 360f / dropCount;
                Vector3 dropPos;
                if (dropCount < 2)
                    dropPos = transform.position;
                else
                    dropPos = CalculateDropPotion(angleInterval * i);

                switch (dropItem.DropItemType)
                {
                    case DropItemType.Potion:
                        Managers.Object.Spawn<PotionController>(dropPos).SetInfo(dropItem);
                        break;
                    case DropItemType.DropBox:
                        Managers.Object.Spawn<EliteBoxController>(dropPos);
                        break;
                }
                i++;
            }
        }
    }

    Vector3 CalculateDropPotion(float angle)
    {
        float dropDistance = Random.Range(1f, 2f);

        Vector3 dropPos = transform.position;

        float x = Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = Mathf.Sin(angle * Mathf.Deg2Rad);
        Vector3 offset = new Vector3(x, y, 0f) * dropDistance;
        Vector3 pos = dropPos + offset;

        return pos;
    }
}
