using UnityEngine;

public class SlashDamage : MonoBehaviour
{
    [HideInInspector] public int damage;
    [HideInInspector] public float lifeTime;

    void Start()
    {
        // PlayerSlash에서 전달받은 수명(lifeTime)이 지나면 검기 자동 삭제
        if (lifeTime > 0)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    // ?? 충돌을 감지하는 핵심 함수 (적과 상자를 모두 때립니다!)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 몬스터(Enemy)를 때렸을 때
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // 2. 기믹(나무상자 등)을 때렸을 때
        BreakableObject box = collision.GetComponent<BreakableObject>();
        if (box != null)
        {
            box.TakeDamage(damage);
        }
    }
}