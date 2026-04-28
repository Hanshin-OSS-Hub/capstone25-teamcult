using UnityEngine;

public class LightningOnHit : MonoBehaviour
{
    [Header("References")]
    public ElementalManager elementalManager;

    [Header("Chain Settings")]
    public float chainRadius = 4f;
    public float chainDamageRatio = 0.5f;
    public float duration = 1.5f;
    public int triggerEveryNHits = 3;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasLightningHeart) return;

        elementalManager.lightningHitCounter++;
        if (elementalManager.lightningHitCounter < triggerEveryNHits) return;
        elementalManager.lightningHitCounter = 0;

        TriggerLightningChain(other.gameObject);
    }

    void TriggerLightningChain(GameObject hitEnemy)
    {
        Vector3 hitPos = hitEnemy.transform.position;
        LightningVisual.Spawn(transform.position, hitPos);
        SpawnChainEffect(hitPos);
    }

    void SpawnChainEffect(Vector3 origin)
    {
        int damage = GetOriginalDamage();

        LightningEffect effect = new GameObject("LightningChainRunner").AddComponent<LightningEffect>();
        effect.elementalManager = elementalManager;
        effect.chainRadius = chainRadius;
        effect.chainDamageRatio = chainDamageRatio;
        effect.originalDamage = damage;
        effect.duration = duration;
        effect.chainOrigin = origin;
    }

    int GetOriginalDamage()
    {
        SlashDamage slash = GetComponent<SlashDamage>();
        if (slash != null) return slash.damage;

        PlayerBullet bullet = GetComponent<PlayerBullet>();
        if (bullet != null) return (int)bullet.damage;

        return 10;
    }
}