using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SaftyZoneController : BaseController
{
    private Coroutine _coDotDamage;

    public override bool Init()
    {
        base.Init();
        return true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player.IsValid() == false)
            return;

        player.OnSafetyZoneEnter(this);

        if (_coDotDamage != null)
        {
            StopCoroutine(_coDotDamage);
            _coDotDamage = null;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player.IsValid() == false)
            return;

        player.OnSafetyZoneExit(this);

        if (_coDotDamage == null)
            _coDotDamage = StartCoroutine(CoStartDotDamage(player));
    }

    protected IEnumerator CoStartDotDamage(PlayerController target)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            target.OnSafetyZoneExit(this);
        }
    }
}
