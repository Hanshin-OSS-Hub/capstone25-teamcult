using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1; // 하트 반 칸

    public void SetDirection(Vector3 direction)
    {
        // 리지드바디 없으면 추가
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0; // 중력 끄기
        // Unity 6버전은 linearVelocity, 구버전은 velocity
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            // ★ 중요: 맞은 부위(팔, 무기 등)의 부모님(몸통)에게서 스크립트를 찾습니다.
            // 이게 있어야 충돌이 씹히지 않습니다.
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("🔫 적 총알 명중! 플레이어 체력 감소");
            }

            // 맞았으면 총알 삭제
            Destroy(gameObject);
        }
        // 2. 벽에 맞으면 삭제
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}