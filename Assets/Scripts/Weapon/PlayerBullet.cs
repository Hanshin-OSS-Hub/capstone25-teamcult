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
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // 음파 디버프로 인한 빗나감 판정
                float missChance = (PlayerStats.instance != null) ? PlayerStats.instance.GetEffectiveMissChance() : 0f;
                if (missChance > 0f && Random.Range(0f, 100f) < missChance)
                {
                    enemy.ShowMiss(); 
                }
                else
                {
                    enemy.TakeDamage((int)damage); 
                }
            }
            Destroy(gameObject);
        }
        else if (other.GetComponent<BreakableObject>() != null)
        {
            BreakableObject box = other.GetComponent<BreakableObject>();
            box.TakeDamage((int)damage);
            Destroy(gameObject);
        }
    }
}