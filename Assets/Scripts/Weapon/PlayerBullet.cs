using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    // 값은 PlayerSlash에서 넣어줍니다.
    public float damage;
    public float speed;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // 받아온 속도(speed)로 날아감
        if (rb != null) rb.linearVelocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage((int)damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall")) Destroy(gameObject);
    }
}