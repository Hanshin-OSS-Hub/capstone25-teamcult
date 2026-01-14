using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    private float damage; // ★ 전달받은 공격력을 저장할 곳

    // ★ 이 함수가 없어서 에러가 난 겁니다!
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    void Start()
    {
        // 총알 날아가기 (Unity 버전에 따라 velocity 또는 linearVelocity 사용)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Unity 6버전이면 linearVelocity, 구버전이면 velocity
            rb.linearVelocity = transform.right * speed;
        }

        Destroy(gameObject, 2f); // 2초 뒤 삭제
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 적의 체력 스크립트 찾기
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // ★ 저장해둔 공격력으로 적 때리기
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // 총알 삭제
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}