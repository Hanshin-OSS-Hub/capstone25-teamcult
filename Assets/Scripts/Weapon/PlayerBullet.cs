using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("총알 설정")]
    public float damage; // 최종 데미지
    public float speed = 10f; // 총알 속도

    void Start()
    {
        // 1. 앞으로 날아가기 (Unity 6 최신 문법 linearVelocity 적용)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }

        // ★ [핵심] 만약 데미지가 0이라면? (Weapon에서 설정을 안 해줬다면?)
        // 스스로 플레이어를 찾아서 스탯을 읽어옵니다. (자동 연동)
        if (damage == 0)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    // 기본공격력(10) + 추가데미지(레벨업보너스) * 배율
                    float baseDamage = 5f;
                    damage = (baseDamage + stats.bonusDamage) * stats.attackMultiplier;
                }
            }
        }

        // (최적화) 3초 뒤에 스스로 사라짐 (메모리 절약)
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 적에 닿았을 때
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // float 데미지를 int로 바꿔서 전달
                enemy.TakeDamage((int)damage);
            }

            // 적을 맞췄으니 총알 삭제
            Destroy(gameObject);
        }
        // 벽에 닿았을 때
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}