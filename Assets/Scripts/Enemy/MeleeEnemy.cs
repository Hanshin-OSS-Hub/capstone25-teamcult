using UnityEngine;
public class MeleeEnemy : MonoBehaviour
{
    [Header("설정")]
    public float detectRange = 25f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    public float moveSpeed;
    public int damage;

    private Transform player;
    private float lastAttackTime;
    private PlayerHealth playerHealth;
    private EnemyStats stats;
    private bool hasSpotted = false;

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        // EnemyStats에서 스탯 가져오기
        if (stats != null)
        {
            moveSpeed = stats.moveSpeed;
            damage = stats.damage;
        }

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

        if (distance <= attackRange)
        {
            if (!hasSpotted)
            {
                hasSpotted = true;
                if (SFXManager.Instance != null)
                    SFXManager.Instance.PlaySFX(SFXType.EnemyEncounter);
            }

            // 공격 범위 안에서는 멈추고 공격
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else if (distance <= detectRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // 방향 반전
            Vector3 scale = transform.localScale;

            if (player.position.x < transform.position.x) {
                scale.x = -Mathf.Abs(scale.x);
            }
            else {
                scale.x = Mathf.Abs(scale.x);
            }

            transform.localScale = scale;
        }
    }

    void Attack()
    {
        if (playerHealth != null)
        {
            int dmg = (stats != null) ? stats.damage : damage;
            playerHealth.TakeDamage(dmg);
            Debug.Log("근거리 적 공격!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}