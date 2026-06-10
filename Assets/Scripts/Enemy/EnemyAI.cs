using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    private EnemyStats stats;

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null) return;
        float speed = (stats != null) ? stats.moveSpeed : 2f;

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        Vector3 scale = transform.localScale;

        if (target.position.x < transform.position.x) {
            scale.x = -Mathf.Abs(scale.x);
        }
        else {
            scale.x = Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
            if (player != null)
            {
                int dmg = (stats != null) ? stats.damage : 10;
                player.TakeDamage(dmg);
            }
        }
    }
}