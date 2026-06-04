using UnityEngine;
public class PlayerBullet : MonoBehaviour
{
    [Header("총알 설정")]
    public float damage; // 최종 데미지
    public float speed = 10f; // 총알 속도

    void Start()
    {
        // 앞으로 날아가기
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }

        // 데미지가 0이면 플레이어 스탯에서 자동 계산
        if (damage == 0)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    float baseDamage = 5f;
                    damage = (baseDamage + stats.bonusDamage) * stats.attackMultiplier;
                }
            }
        }

        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 적에 닿았을 때
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // 음파 디버프로 인한 빗나감 판정
                float missChance = (PlayerStats.instance != null) ? PlayerStats.instance.GetEffectiveMissChance() : 0f;
                if (missChance > 0f && Random.Range(0f, 100f) < missChance)
                {
                    enemy.ShowMiss(); // 빗나감
                }
                else
                {
                    enemy.TakeDamage((int)damage); // 명중
                }
            }
            Destroy(gameObject);
        }
        // 2. 기믹(나무통/상자)에 닿았을 때
        else if (other.GetComponent<BreakableObject>() != null)
        {
            BreakableObject box = other.GetComponent<BreakableObject>();
            box.TakeDamage((int)damage);
            Destroy(gameObject);
        }
    }
}