using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1; // 하트 반 칸

    public void SetDirection(Vector3 direction)
    {
        // 리지드바디가 없으면 추가해서라도 날아감
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0; // 중력 끄기
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            // ★ 중요: 맞은 부위뿐만 아니라 부모(몸통)에서도 스크립트를 찾음
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("🔫 플레이어가 총알에 맞음!"); // 콘솔 확인용
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