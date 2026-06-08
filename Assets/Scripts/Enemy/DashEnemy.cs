using System.Collections;
using UnityEngine;
public class DashEnemy : MonoBehaviour
{
    [Header("이동")]
    public float detectRange = 8f;
    public float dashRange = 5f;
    public float stopDistance = 1f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 5f;
    public float dashSpeed = 4f;
    public float dashCooldown = 3f;
    public float prepareTime = 0.4f;
    public float dashDuration = 0.9f;

    [Header("벽 감지")]
    public float bodyRadius = 0.5f;
    public string wallNameKeyword = "Tilemap_Wall";

    [Header("공격")]
    public int damage = 10;

    [Header("체력")]
    public int maxHealth = 30;
    private int currentHealth;
    private Transform player;
    private PlayerHealth playerHealth;
    private EnemyStats stats;
    private EnemyHealth enemyHealth;
    private Animator anim;
    private bool isDashing = false;
    private bool canHit = false;
    private bool hasHit = false;
    private float lastDashTime;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        stats = GetComponent<EnemyStats>();
        enemyHealth = GetComponent<EnemyHealth>();
        if (stats != null) damage = stats.damage;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
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
        if (player == null || isDashing) return;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            if (distance <= dashRange && Time.time > lastDashTime + dashCooldown)
            {
                StartCoroutine(Dash());
            }
            else if (!isDashing && distance > stopDistance)
            {
                float speed = chaseSpeed;
                Vector2 moveDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                float step = speed * Time.deltaTime;

                float wall = WallDistanceAhead(moveDir, step + bodyRadius);
                if (wall < 0f)
                    transform.position += (Vector3)(moveDir * step);
                else
                    transform.position += (Vector3)(moveDir * Mathf.Max(0f, wall - bodyRadius));
            }
        }

        FacePlayer();
    }

    void FacePlayer()
    {
        Vector3 scale = transform.localScale;
        if (player.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;

        if (enemyHealth != null && enemyHealth.hpSlider != null)
        {
            Vector3 hpScale = enemyHealth.hpSlider.transform.localScale;
            hpScale.x = (transform.localScale.x < 0) ? -Mathf.Abs(hpScale.x) : Mathf.Abs(hpScale.x);
            enemyHealth.hpSlider.transform.localScale = hpScale;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        hasHit = false;
        canHit = false;
        lastDashTime = Time.time;

        if (anim != null) anim.SetTrigger("PrepareAttack");
        yield return new WaitForSeconds(prepareTime);

        if (anim != null) anim.SetTrigger("ExecuteAttack");
        canHit = true;

        Vector2 dashDir = (player.position - transform.position).normalized;
        float dashTimer = 0f;
        while (dashTimer < dashDuration)
        {
            float step = dashSpeed * Time.deltaTime;
            float wall = WallDistanceAhead(dashDir, step + bodyRadius);
            if (wall >= 0f)
            {
                transform.position += (Vector3)(dashDir * Mathf.Max(0f, wall - bodyRadius));
                break;
            }

            transform.position += (Vector3)(dashDir * step);
            dashTimer += Time.deltaTime;
            yield return null;
        }

        canHit = false;
        isDashing = false;
    }

    float WallDistanceAhead(Vector2 dir, float dist)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, dist);
        float nearest = -1f;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.name.Contains(wallNameKeyword))
            {
                if (nearest < 0f || hit.distance < nearest)
                    nearest = hit.distance;
            }
        }
        return nearest;
    }

    void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    void OnTriggerStay2D(Collider2D other) { TryHit(other); }

    void TryHit(Collider2D other)
    {
        if (!canHit || hasHit) return;
        if (!other.CompareTag("Player")) return;

        if (playerHealth != null)
        {
            int dmg = (stats != null) ? stats.damage : damage;
            playerHealth.TakeDamage(dmg);
            hasHit = true;
            Debug.Log("돌진 적 공격!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}