using UnityEngine;

public class LightningOnHit : MonoBehaviour
{
    public ElementalManager elementalManager;
    public float chainRadius = 4f;
    public float chainDamageRatio = 0.5f;
    public float duration = 1.5f;

    private static float lastTriggerTime = -999f;
    private static int hitCounter = 0;
    public int triggerEveryNHits = 3;  // 인스펙터에서 조절 가능

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasLightningHeart) return;

        hitCounter++;
        if (hitCounter < triggerEveryNHits) return;
        hitCounter = 0;

        Debug.Log("[번개 히트] 번개 효과 부여!");

        // 죽어도 위치 저장
        Vector3 hitPos = other.transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, chainRadius);
        bool hasNearbyEnemy = false;
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            if (hit.gameObject == other.gameObject) continue;
            hasNearbyEnemy = true;
            break;
        }

        if (hasNearbyEnemy)
            LightningVisual.Spawn(transform.position, hitPos);

        // 원래 데미지 가져오기
        int origDamage = 10;
        SlashDamage slash = GetComponent<SlashDamage>();
        PlayerBullet bullet = GetComponent<PlayerBullet>();
        if (slash != null) origDamage = slash.damage;
        if (bullet != null) origDamage = (int)bullet.damage;

        // 적이 죽어도 체인 실행되도록 별도 오브젝트에 붙이기
        GameObject chainObj = new GameObject("LightningChainRunner");
        LightningEffect effect = chainObj.AddComponent<LightningEffect>();
        effect.elementalManager = elementalManager;
        effect.chainRadius = chainRadius;
        effect.chainDamageRatio = chainDamageRatio;
        effect.originalDamage = origDamage;
        effect.duration = duration;
        effect.chainOrigin = hitPos;
    }
}