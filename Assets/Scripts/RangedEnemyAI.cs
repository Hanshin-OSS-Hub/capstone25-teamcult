using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("설정")]
    public float detectRange = 7f;   // 사거리 (이 안에 들어오면 쏨)
    public float stopDistance = 3f;  // 너무 가까우면 멈춤 (도망가지 않게)
    public float moveSpeed = 1.5f;   // 이동 속도

    public GameObject bulletPrefab;  // ★ 적 총알 프리팹 꼭 넣으세요!
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
            // 공격 쿨타임 체크 -> 발사
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            // 너무 딱 붙지 않게, 적당한 거리까지만 다가감
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 총알에게 "플레이어 쪽으로 날아가라" 명령
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}