using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed, curveDelay, curveAngle, lifetime;
    private int damage;
    private float timer;
    private bool curved;

    public void Init(Vector2 dir, float spd, float delay, float angle, float life, int dmg)
    {
        direction = dir.normalized;
        speed = spd;
        curveDelay = delay;
        curveAngle = angle;
        lifetime = life;
        damage = dmg;
        timer = 0f;
        curved = false;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 일정 시간 지나면 한 번 꺾인다
        if (!curved && timer >= curveDelay)
        {
            float rad = curveAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            direction = new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos
            );
            curved = true;
        }

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("?? 곡선탄 명중! 플레이어 체력 감소");
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}