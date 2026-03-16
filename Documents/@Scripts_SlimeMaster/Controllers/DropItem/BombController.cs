using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : DropItemController
{
    Data.DropItemData _dropItemData;

    public override bool Init()
    {
        base.Init();
        itemType = Define.ObjectType.Bomb;
        return true;
    }

    public override void GetItem()
    {
        base.GetItem();
        if (_coroutine == null && this.IsValid())
        {
            _coroutine = StartCoroutine(CoCheckDistance());
        }
    }

    public void SetInfo(Data.DropItemData data)
    {
        _dropItemData = data;
        CollectDist = Define.BOX_COLLECT_DISTANCE;
        //GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(_dropItemData.SpriteName);
    }

    public override void CompleteGetItem()
    {
        Managers.Object.KillAllMonsters();
        Managers.Object.Despawn(this);
    }
}
