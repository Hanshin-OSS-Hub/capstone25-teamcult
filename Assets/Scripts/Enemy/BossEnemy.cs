using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("이동")]
    public float detectRange = 12f;
    public float stopDistance = 4f;
    public float moveSpeed = 1f;

    [Header("공격")]
    public GameObject bulletPrefab;
    public float attackCooldown = 2.5f;
    public float spreadAngle = 25f;

    [Header("체력 세팅 (기존 체력 스크립트가 있다면 무시하세요)")]
    public int maxHealth = 100;
    private int currentHealth;

    private float lastAttackTime;
    private Transform player;
    
    // ★ 인사말을 한 번만 하도록 체크하는 변수
    private bool hasGreeted = false; 

    void Start()
    {
        currentHealth = maxHealth;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            // ★ 1. 처음 발견했을 때 인사말(포효) 재생
            if (!hasGreeted)
            {
                hasGreeted = true;
                if (SFXManager.Instance != null) 
                    SFXManager.Instance.PlaySFX(SFXType.BossGreeting);
                
                Debug.Log("보스 조우! 인사말 재생");
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

    void ShootTriple()
    {
        if (bulletPrefab == null || player == null) return;

        // ★ 2. 보스 공격 효과음 재생
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

    // ★ 3 & 4. 피격 및 사망 로직 (플레이어 공격 스크립트 등에서 호출)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // 사망 효과음
            if (SFXManager.Instance != null) 
                SFXManager.Instance.PlaySFX(SFXType.BossDeath);
            
            Debug.Log("보스 처치!");
            Destroy(gameObject);
        }
        else
        {
            // 피격 효과음
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