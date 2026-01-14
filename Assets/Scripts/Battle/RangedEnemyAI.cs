using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    public float detectRange = 7f;   // 사거리 (빨간 원)
    public float stopDistance = 3f;  // 너무 가까우면 도망가거나 멈춤
    public float moveSpeed = 1.5f;

    public GameObject bulletPrefab;  // 아까 만든 적 총알
    public float attackCooldown = 2f; // 2초마다 발사
    private float lastAttackTime;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 사거리 안에 들어왔으면?
        if (distance <= detectRange)
        {
            // 공격 쿨타임 체크
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            // 너무 가까우면 멈추고, 아니면 조금씩 다가감 (선택 사항)
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    void Shoot()
    {
        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 총알에게 날아갈 방향 알려주기
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            bulletScript.SetDirection(direction);
        }
        Debug.Log("🔫 원거리 적 발사!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}