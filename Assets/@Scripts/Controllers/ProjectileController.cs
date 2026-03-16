using UnityEngine;
using static Define;

public class ProjectileController : BaseController
{
    float _damage;
    float _speed  = 8f;
    float _range  = 10f;
    Vector2 _startPos;
    Vector2 _dir;

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = ObjectType.Projectile;
        return true;
    }

    public void SetInfo(Vector2 startPos, Vector2 dir, float damage, float speed = 8f, float range = 10f)
    {
        _startPos = startPos;
        _dir      = dir.normalized;
        _damage   = damage;
        _speed    = speed;
        _range    = range;
        transform.position = startPos;

        float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        transform.Translate(_dir * _speed * Time.deltaTime, Space.World);

        if (Vector2.Distance(_startPos, transform.position) >= _range)
            Managers.Resource.Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<MonsterController>(out var monster)) return;
        monster.OnDamaged(_damage);
        Managers.Resource.Destroy(gameObject);
    }
}
