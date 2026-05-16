using UnityEngine;
using System.Collections.Generic;

public class LightningOnHit : MonoBehaviour
{
    [Header("References")]
    public ElementalManager elementalManager;
    [Header("Chain Settings")]
    public float chainRadius = 4f;
    public float chainDamageRatio = 0.5f;
    public float duration = 1.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasLightningHeart) return;

        TriggerLightningChain(other.gameObject);
    }

    void TriggerLightningChain(GameObject hitEnemy)
    {
        Vector3 hitPos = hitEnemy.transform.position;
        LightningVisual.Spawn(transform.position, hitPos);
        SpawnChainEffect(hitPos, hitEnemy);
    }

    void SpawnChainEffect(Vector3 origin, GameObject hitEnemy)
    {
        int damage = GetOriginalDamage();
        LightningEffect effect = new GameObject("LightningChainRunner").AddComponent<LightningEffect>();
        effect.elementalManager = elementalManager;
        effect.chainRadius = chainRadius;
        effect.chainDamageRatio = chainDamageRatio;
        effect.originalDamage = damage;
        effect.duration = duration;
        effect.chainOrigin = origin;
        effect.originEnemy = hitEnemy;
        effect.visitedEnemies = new List<GameObject> { hitEnemy }; // 처음 맞은 적도 방문 목록에 추가
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