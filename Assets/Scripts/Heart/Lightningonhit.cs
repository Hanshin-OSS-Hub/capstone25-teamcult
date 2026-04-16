using UnityEngine;

public class LightningOnHit : MonoBehaviour
{
    public ElementalManager elementalManager;
    public float chainRadius = 4f;
    public float chainDamageRatio = 0.5f;
    public float duration = 1.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasLightningHeart) return;

        if (other.GetComponent<LightningEffect>() == null)
        {
            Debug.Log("[번개 히트] 번개 효과 부여!");
            LightningEffect effect = other.gameObject.AddComponent<LightningEffect>();
            effect.elementalManager = elementalManager;
            effect.chainRadius = chainRadius;
            effect.chainDamageRatio = chainDamageRatio;
            effect.duration = duration;
        }
    }
}