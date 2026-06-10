using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningEffect : MonoBehaviour
{
    [Header("체인 설정")]
    public ElementalManager elementalManager;
    public float chainRadius = 4f;
    public float chainDamageRatio = 0.5f;
    public float duration = 1.5f;
    public int maxChainCount = 3;
    public int originalDamage = 10;
    public Vector3 chainOrigin;
    public GameObject originEnemy;
    public List<GameObject> visitedEnemies = new List<GameObject>(); 
    private bool hasChained = false;

    void Start()
    {
        if (chainOrigin == Vector3.zero)
            chainOrigin = transform.position;
        StartCoroutine(DoLightningEffect());
    }

    IEnumerator DoLightningEffect()
    {
        yield return new WaitForSeconds(0.1f);
        if (!hasChained)
        {
            hasChained = true;
            ChainToNearbyEnemies(chainOrigin, maxChainCount);
        }
        yield return new WaitForSeconds(duration);
        if (this != null) Destroy(this);
    }

    void ChainToNearbyEnemies(Vector3 origin, int remainingChains)
    {
        if (remainingChains <= 0) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, chainRadius);
        List<GameObject> chainTargets = new List<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            if (Vector3.Distance(hit.transform.position, origin) < 0.1f) continue;

            if (originEnemy != null && hit.gameObject == originEnemy) continue;

            if (visitedEnemies.Contains(hit.gameObject)) continue;

            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh != null && eh.IsInvincible()) continue;

            chainTargets.Add(hit.gameObject);
        }

        if (chainTargets.Count == 0) return;

        foreach (GameObject target in chainTargets)
            visitedEnemies.Add(target);

        foreach (GameObject target in chainTargets)
            LightningVisual.Spawn(origin, target.transform.position);

        foreach (GameObject target in chainTargets)
        {
            LightningEffect existing = target.GetComponent<LightningEffect>();
            if (existing != null) Destroy(existing);
            LightningEffect chainEffect = target.AddComponent<LightningEffect>();
            chainEffect.elementalManager = elementalManager;
            chainEffect.chainRadius = chainRadius;
            chainEffect.chainDamageRatio = chainDamageRatio;
            chainEffect.originalDamage = originalDamage;
            chainEffect.chainOrigin = target.transform.position;
            chainEffect.duration = duration * 0.5f;
            chainEffect.maxChainCount = remainingChains - 1;
            chainEffect.originEnemy = originEnemy;
            chainEffect.visitedEnemies = new List<GameObject>(visitedEnemies); 
        }

        foreach (GameObject target in chainTargets)
        {
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                int chainDamage = Mathf.RoundToInt(originalDamage * chainDamageRatio);
                if (chainDamage < 1) chainDamage = 1;
                enemyHealth.TakeDamage(chainDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chainRadius);
    }
}