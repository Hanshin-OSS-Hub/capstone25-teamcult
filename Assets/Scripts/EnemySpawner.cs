using UnityEngine;
using System.Collections; // 코루틴
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 1.0f; // 스폰주기,spawnInterval <= 0 일때 스포너 작동 X
    [SerializeField] private float spawnRadius = 10.0f; // 스폰 범위
    [SerializeField] private int enemiesPerTick = 1;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.Find("Player");
        if (p != null) player = p.transform;

        // 코루틴 시작
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (spawnInterval <= 0) { yield break; }
        // 플레이어가 존재하는 동안 무한 반복
        while (player != null && player.gameObject.activeSelf)
        {
            SpawnEnemy(enemiesPerTick);
            // spawnInterval초 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public List<GameObject> SpawnEnemy(int cnt)
    {
        List<GameObject> enemyList = new List<GameObject>();
        for (int i = 0; i < cnt; i++) {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            //Vector3 spawnPos = player.position; // 캐릭터위치
            Vector3 spawnPos = transform.position; // 스포너 위치
            spawnPos += (Vector3)randomDir * spawnRadius;

            enemyList.Add(Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity));
        }
        return enemyList;
    }
}