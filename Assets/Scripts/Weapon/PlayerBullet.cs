using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("총알 설정")]
    public float damage;
    public float speed = 10f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }

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
        if (other.CompareTag("Enemy"))
        {
            PlayerStats playerStats = GameObject.Find("Player")?.GetComponent<PlayerStats>();
            if (playerStats != null && playerStats.GetEffectiveMissChance() > 0)
            {
                float roll = Random.Range(0f, 100f);
                if (roll < playerStats.GetEffectiveMissChance())
                {
                    LogManager.Instance.AddLog("[MISS] 음파 디버프로 총알 빗나감");
                    return;
                }
            }

            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
            Destroy(gameObject);
        }
        else if (other.GetComponent<BreakableObject>() != null)
        {
            BreakableObject box = other.GetComponent<BreakableObject>();
            box.TakeDamage((int)damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}