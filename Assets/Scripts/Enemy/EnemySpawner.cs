using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {
    [Header("Setting")]
    [SerializeField] private GameObject[] enemyPrefabs;
    // 보스 프리팹 변수 추가
    [SerializeField] private GameObject[] bossPrefabs;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private float spawnRadius = 3.0f;
    [SerializeField] private int enemiesPerTick = 1;

    private Transform player;

    void Start() {
        GameObject p = GameObject.Find("Player");
        if (p != null) player = p.transform;
        StartCoroutine(SpawnRoutine());
    }

    //private void Update() {
    //    if (Input.GetKeyDown(KeyCode.X)) {
    //        SpawnEnemy(1);
    //    }
    //}

    IEnumerator SpawnRoutine() {
        if (spawnInterval <= 0) { yield break; }
        while (player != null && player.gameObject.activeSelf) {
            SpawnEnemy(enemiesPerTick);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public List<GameObject> SpawnEnemy(int cnt) {
        List<GameObject> enemyList = new List<GameObject>();
        for (int i = 0; i < cnt; i++) {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 spawnPos = transform.position;
            spawnPos += (Vector3)randomDir * spawnRadius;
            enemyList.Add(Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity));
        }
        return enemyList;
    }

    // 보스 스폰 함수 추가
    public GameObject SpawnBoss(int index) {
        if (bossPrefabs == null || bossPrefabs.Length <= index) {
            Debug.LogWarning("보스 프리팹이 설정되지 않았거나 인덱스가 범위를 벗어났습니다.");
            return null;
        }
        
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        //Vector3 spawnPos = transform.position + (Vector3)randomDir * spawnRadius; // 일반 적과 동일하게 스폰 위치 계산 (필요 시 수정 가능)
        Vector3 spawnPos = transform.position; // 정중앙 스폰

        // 보스 생성 후 반환
        GameObject boss = Instantiate(bossPrefabs[index], spawnPos, Quaternion.identity);
        return boss;
    }
}