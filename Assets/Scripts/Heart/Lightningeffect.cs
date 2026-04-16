using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningEffect : MonoBehaviour
{
    [Header("체인 설정")]
    public float chainRadius = 4f;
    public float damage = 10f;
    public float chainDamageRatio = 0.5f; // 체인 데미지 비율 (0.5 = 50%)
    public float duration = 1.5f;
    public bool isChained = false;

    public ElementalManager elementalManager;

    private bool applied = false;
    private static HashSet<GameObject> chainedThisFrame = new HashSet<GameObject>();

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        if (applied) return;
        applied = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        StartCoroutine(ChainLightning());
        StartCoroutine(RemoveEffect());
    }

    IEnumerator ChainLightning()
    {
        yield return null;

        StartCoroutine(FlashEffect());

        if (isChained) yield break;

        chainedThisFrame.Clear();
        chainedThisFrame.Add(this.gameObject);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chainRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            if (chainedThisFrame.Contains(hit.gameObject)) continue;

            chainedThisFrame.Add(hit.gameObject);

            if (hit.GetComponent<LightningEffect>() == null)
            {
                Debug.Log($"[번개 체인] {hit.gameObject.name} 에게 전파! (데미지 {chainDamageRatio * 100f}%)");
                LightningEffect chainEffect = hit.gameObject.AddComponent<LightningEffect>();
                chainEffect.elementalManager = elementalManager;
                chainEffect.isChained = true;
                chainEffect.duration = duration;
                chainEffect.damage = damage;
                chainEffect.chainDamageRatio = chainDamageRatio;
            }

            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(Mathf.RoundToInt(damage * chainDamageRatio));
        }
    }

    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.05f);

        spriteRenderer.color = new Color(0.0f, 1.0f, 0.9f, 1.0f);
        yield return new WaitForSeconds(0.05f);

        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.05f);

        spriteRenderer.color = originalColor;
    }

    IEnumerator RemoveEffect()
    {
        yield return new WaitForSeconds(duration);
        applied = false;
        Destroy(this);
    }

    void OnDestroy()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chainRadius);
    }
}