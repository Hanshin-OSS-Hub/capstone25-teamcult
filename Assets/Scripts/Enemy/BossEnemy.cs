using System.Collections;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("이동")]
    public float detectRange = 12f;
    public float stopDistance = 4f;
    public float moveSpeed = 1f;

    [Header("일반 공격")]
    public GameObject bulletPrefab;
    public float attackCooldown = 2.5f;
    public float spreadAngle = 25f;

    [Header("마법진 패턴")]
    public GameObject magicCirclePrefab;
    public float magicCooldown = 6f;
    public float magicWarningTime = 1.5f;
    public int magicDamage = 20;
    public float predictTime = 0.8f;

    [Header("체력")]
    public int maxHealth = 100;
    private int currentHealth;

    private float lastAttackTime;
    private float lastMagicTime;
    private Transform player;
    private Rigidbody2D playerRb;
    private bool hasGreeted = false;
    private bool isMagicAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerRb = p.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            if (!hasGreeted)
            {
                hasGreeted = true;
                if (SFXManager.Instance != null)
                    SFXManager.Instance.PlaySFX(SFXType.BossGreeting);
            }

            if (!isMagicAttacking && Time.time > lastMagicTime + magicCooldown)
            {
                StartCoroutine(MagicCirclePattern());
                lastMagicTime = Time.time;
            }

            if (Time.time > lastAttackTime + attackCooldown)
            {
                ShootTriple();
                lastAttackTime = Time.time;
            }

            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    moveSpeed * Time.deltaTime
                );
            }
        }
    }

    IEnumerator MagicCirclePattern()
    {
        isMagicAttacking = true;

        // 현재 위치
        SpawnMagicCircle(player.position);

        // 예측 위치
        if (playerRb != null)
        {
            Vector3 predictedPos = player.position + (Vector3)(playerRb.linearVelocity * predictTime);
            SpawnMagicCircle(predictedPos);
        }

        yield return new WaitForSecondsRealtime(magicWarningTime + 0.5f);

        isMagicAttacking = false;
        lastAttackTime = Time.time;
    }

    void SpawnMagicCircle(Vector3 pos)
    {
        GameObject circle = Instantiate(magicCirclePrefab, pos, Quaternion.identity);
        circle.transform.localScale = Vector3.one * 3f;
        MagicCircle magicScript = circle.GetComponent<MagicCircle>();
        if (magicScript != null)
        {
            magicScript.damage = magicDamage;
            magicScript.warningTime = magicWarningTime;
        }
    }

    void ShootTriple()
    {
        if (bulletPrefab == null || player == null) return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(SFXType.BossAttack);

        Vector3 dir = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float[] angles = { baseAngle - spreadAngle, baseAngle, baseAngle + spreadAngle };

        foreach (float angle in angles)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 shootDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
                bulletScript.SetDirection(shootDir);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlaySFX(SFXType.BossDeath);
            Destroy(gameObject);
        }
        else
        {
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlaySFX(SFXType.BossHit);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}