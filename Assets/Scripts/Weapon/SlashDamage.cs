using UnityEngine;
public class SlashDamage : MonoBehaviour
{
    [HideInInspector] public int damage;
    [HideInInspector] public float lifeTime;
    void Start()
    {
        float destroyTime = lifeTime > 0 ? lifeTime : 0.5f;
        Destroy(gameObject, destroyTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            float missChance = (PlayerStats.instance != null) ? PlayerStats.instance.GetEffectiveMissChance() : 0f;
            if (missChance > 0f && Random.Range(0f, 100f) < missChance)
            {
                enemy.ShowMiss(); 
            }
            else
            {
                enemy.TakeDamage(damage);
            }
        }
        BreakableObject box = collision.GetComponent<BreakableObject>();
        if (box != null)
        {
            box.TakeDamage(damage);
        }
    }
}