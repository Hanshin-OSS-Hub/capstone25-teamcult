using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private float spawnRadius = 10.0f;
    [SerializeField] private int enemiesPerTick = 1;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.Find("Player");
        if (p != null) player = p.transform;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (spawnInterval <= 0) { yield break; }
        while (player != null && player.gameObject.activeSelf)
        {
            SpawnEnemy(enemiesPerTick);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public List<GameObject> SpawnEnemy(int cnt)
    {
        List<GameObject> enemyList = new List<GameObject>();
        for (int i = 0; i < cnt; i++)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 spawnPos = transform.position;
            spawnPos += (Vector3)randomDir * spawnRadius;
            enemyList.Add(Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity));
        }
        return enemyList;
    }
}