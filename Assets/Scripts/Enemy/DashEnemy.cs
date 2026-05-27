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
    public float dashSpeed = 7f;
    public float dashCooldown = 3f;

    [Header("체력")]
    public int maxHealth = 30;
    private int currentHealth;

    private Transform player;
    private bool isDashing = false;
    private float lastDashTime;

    void Start()
    {
        currentHealth = maxHealth;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
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
                float currentSpeed = distance <= detectRange ? chaseSpeed : moveSpeed;
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    currentSpeed * Time.deltaTime
                );
            }
        }

        // 방향 전환
        Vector3 scale = transform.localScale;
        if (player.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        yield return new WaitForSeconds(0.4f);

        float dashDuration = 0.5f;
        float timer = 0f;

        while (timer < dashDuration)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= stopDistance)
                break;

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                dashSpeed * Time.deltaTime
            );
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
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