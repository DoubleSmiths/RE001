using UnityEngine;

/// <summary>
/// 전투 구역에서 몬스터를 주기적으로 스폰한다.
/// 하이어라키의 BattleSpawner 오브젝트에 부착하고
/// Inspector에서 monsterIds / spawnInterval / spawnPoints 를 설정한다.
/// </summary>
public class BattleSpawner : MonoBehaviour
{
    [SerializeField] int[] monsterIds = { 2001 };
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] int   maxMonsters   = 3;
    [SerializeField] Transform[] spawnPoints;

    float _nextSpawnTime;

    void Start()
    {
        TrySpawn();
        _nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.time < _nextSpawnTime) return;
        _nextSpawnTime = Time.time + spawnInterval;
        TrySpawn();
    }

    void TrySpawn()
    {
        if (Managers.Object.MonsterTransform.childCount >= maxMonsters) return;

        int dataId = monsterIds[Random.Range(0, monsterIds.Length)];
        Vector3 pos = spawnPoints is { Length: > 0 }
            ? spawnPoints[Random.Range(0, spawnPoints.Length)].position
            : transform.position;

        Managers.Object.SpawnMonster(dataId, pos);
    }
}
