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
    private EnemyHealth enemyHealth;
    private bool hasSpotted = false;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        enemyHealth = GetComponent<EnemyHealth>();

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

            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else if (distance <= detectRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // 방향 반전 + 체력바 보정
            if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                if (enemyHealth?.hpSlider != null)
                    enemyHealth.hpSlider.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
                if (enemyHealth?.hpSlider != null)
                    enemyHealth.hpSlider.transform.localScale = new Vector3(1, 1, 1);
            }
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