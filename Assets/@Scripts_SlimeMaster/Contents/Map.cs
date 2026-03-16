using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    public SpriteRenderer SpriteBackground;
    [SerializeField]
    public SpriteRenderer SpritePatten;
    [SerializeField]
    public BoxCollider2D[] BorderCollider;//왼 오 위 아래
    [SerializeField]
    public GridController Grid;//왼 오 위 아래
    [SerializeField]
    public GameObject Demarcation;
    [NonSerialized]
    public Color BackgroundColor;
    [NonSerialized]
    public Color PattenColor;
    public Vector2 MapSize
    {
        get 
        {
            return SpriteBackground.size; 
        }
        set 
        {
            SpriteBackground.size = value;
            //SpritePatten.size = value;
        }
    }

    public void Init()
    {
        Managers.Game.CurrentMap = this;
    }


    public void ChangeMapSize(float targetRate, float time = 120)
    {
        Vector3 currentSize = Vector3.one * 20f;
        if (Managers.Game.CurrentWaveIndex > 7)
            return;
        Demarcation.transform.DOScale(currentSize * (10 - Managers.Game.CurrentWaveIndex) * 0.1f, 3);
    }

    //IEnumerator CoChangeMapSize(Vector2 previousSize, Vector2 destSize, float duration = 5f)
    //{
    //    float elapsedTime = 0f;
    //    while (elapsedTime < duration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        MapSize = Vector2.Lerp(previousSize, destSize, elapsedTime/duration);
    //        SetBoundary();
    //        yield return new WaitForSeconds(Time.deltaTime);
    //        SpritePatten.size = MapSize;
    //    }
    //}
}
