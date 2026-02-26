using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("설정")]
    public float detectRange = 5f;   // 추적 범위 (파란 원)
    public float attackRange = 1.2f; // 공격 범위 (빨간 원 - 딱 붙어야 때림)
    public float moveSpeed = 2f;     // 이동 속도
    public float attackCooldown = 1f; // 공격 속도 (1초에 한 번)
    public int damage = 1;           // 공격력 (1 = 하트 반 칸)

    private Transform player;
    private float lastAttackTime;
    private PlayerHealth playerHealth;

    void Start()
    {
        // 게임 시작하면 플레이어 찾기
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

        // 플레이어와의 거리 계산
        float distance = Vector2.Distance(transform.position, player.position);

        // 1. 공격 범위 안에 들어왔는가?
        if (distance <= attackRange)
        {
            // 쿨타임 됐으면 공격!
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        // 2. 공격 범위는 아니지만, 추적 범위 안이라면? -> 쫓아가기
        else if (distance <= detectRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void Attack()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("🥊 근거리 적 공격!");
        }
    }

    // 에디터에서 원 그려주기 (확인용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange); // 추적
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // 공격
    }
}