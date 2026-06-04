using System.Collections;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("설정")]
    public float detectRange = 25f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float moveSpeed;
    public int damage;

    [SerializeField] private string attackStateName = "Attack";
    public float damageDelay = 0.2f;

    private Transform player;
    private float lastAttackTime;
    private PlayerHealth playerHealth;
    private EnemyStats stats;
    private EnemyHealth enemyHealth;
    private Animator anim;
    private bool hasSpotted = false;
    private bool isAttacking = false;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        enemyHealth = GetComponent<EnemyHealth>();
        anim = GetComponent<Animator>();

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
        bool isWalking = false;

        if (distance <= attackRange)
        {
            if (!hasSpotted)
            {
                hasSpotted = true;
                if (SFXManager.Instance != null)
                    SFXManager.Instance.PlaySFX(SFXType.EnemyEncounter);
            }

            if (!isAttacking && Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else if (distance <= detectRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            isWalking = true;

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

        // 걷기/대기 전환은 컨트롤러가 isWalking 보고 알아서
        if (anim != null && !isAttacking)
            anim.SetBool("isWalking", isWalking);
    }

    void Attack()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.Play(attackStateName, 0, 0f);
        }

        yield return new WaitForSeconds(damageDelay);
        if (playerHealth != null)
        {
            int dmg = (stats != null) ? stats.damage : damage;
            playerHealth.TakeDamage(dmg);
            Debug.Log("근거리 적 공격!");
        }

        // 공격 애니 끝날 때까지 대기
        if (anim != null)
        {
            float len = anim.GetCurrentAnimatorStateInfo(0).length;
            float remaining = len - damageDelay;
            if (remaining > 0) yield return new WaitForSeconds(remaining);
        }

        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}