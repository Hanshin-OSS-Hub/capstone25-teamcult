using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 7f;
    public float lifeTime = 3f; // 3초 뒤 자동 삭제

    private Vector2 targetDir;

    public void SetDirection(Vector2 direction)
    {
        targetDir = direction.normalized;
        // 총알이 날아가는 방향을 바라보게 회전 (선택 사항)
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 3초 뒤에 스스로 파괴 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 설정된 방향으로 계속 이동
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어에게 닿으면
        if (other.CompareTag("Player"))
        {
            // ★ 핵심 추가: 플레이어 몸에서 방금 만든 PlayerHitEffect 컴포넌트를 찾는다.
            PlayerHitEffect playerEffect = other.GetComponent<PlayerHitEffect>();

            // 찾았으면 (null이 아니면), TakeDamage 함수를 실행시킨다!
            if (playerEffect != null)
            {
                playerEffect.TakeDamage();
            }

            // 총알은 자기 임무를 다했으니 사라진다.
            Destroy(gameObject);
        }
        // 벽에 닿으면
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}