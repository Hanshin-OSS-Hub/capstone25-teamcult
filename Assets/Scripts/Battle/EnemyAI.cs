using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    public float detectRange = 5f;  // 추적 범위 (파란 원)
    public float attackRange = 1.5f; // 공격 범위 (빨간 원)
    public float moveSpeed = 2f;
    public float attackCooldown = 1f; // 공격 속도 (1초에 한 번)

    private Transform player;
    private float lastAttackTime;
    private PlayerHealth playerHealth;

    void Start()
    {
        // 플레이어 찾기
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. 공격 범위 안에 들어왔는가? (빨간 원)
        if (distance <= attackRange)
        {
            // 공격 쿨타임이 지났으면 공격!
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        // 2. 추적 범위 안에 있으면 쫓아가기 (파란 원)
        else if (distance <= detectRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void Attack()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1); // 하트 반 칸 깎기
            Debug.Log("🥊 근거리 적이 때렸습니다!");
        }
    }

    // 에디터에서 원 그리기 (눈에 보이게)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange); // 추적
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // 공격
    }
}