using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("설정")]
    // ★ [수정] 적 하나가 아니라, 여러 종류를 넣을 수 있게 '배열([])'로 변경
    public GameObject[] enemyPrefabs;

    public float spawnInterval = 1.0f; // 소환 간격
    public float spawnRadius = 10.0f;  // 소환 거리

    private Transform player;
    private float timer;

    void Start()
    {
        GameObject p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeSelf) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // ★ [추가] 등록된 적들 중에서 랜덤으로 하나 뽑기
        // 예: 0번(근접), 1번(원거리) 중 랜덤 선택
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemy = enemyPrefabs[randomIndex];

        // 위치 계산 및 소환
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = player.position + (Vector3)randomDir * spawnRadius;

        Instantiate(selectedEnemy, spawnPos, Quaternion.identity);
    }
}