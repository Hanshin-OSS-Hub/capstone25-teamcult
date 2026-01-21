using UnityEngine;

public class SlashDamage : MonoBehaviour
{
    // 값은 PlayerSlash에서 넣어주므로 여기 숫자는 의미 없습니다.
    public int damage;
    public float lifeTime;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 받아온 수명대로 삭제
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }
}