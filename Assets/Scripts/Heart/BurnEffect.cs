using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    public float damage = 1f;
    public float tickInterval = 0.5f;
    public float duration = 3f;

    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool burning = false;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyHealth == null)
        {
            Destroy(this);
            return;
        }

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        burning = true;
        StartCoroutine(BurnTick());
        StartCoroutine(BurnFlicker());
    }

    IEnumerator BurnTick()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            if (enemyHealth != null)
            {
                // ? TakeDamage에서 텍스트 생성하므로 여기선 데미지만 줌
                enemyHealth.TakeDamage((int)damage);
                Debug.Log($"[화상] {damage} 데미지 / 남은시간: {duration - elapsed:F1}초");
            }
        }

        Debug.Log("[화상] 종료");
        burning = false;

        yield return StartCoroutine(FadeToOriginal());
        Destroy(this);
    }

    IEnumerator BurnFlicker()
    {
        // 페이드인: 원본 → 빨간색
        float elapsed = 0f;
        float fadeTime = 0.2f;
        Color fireColor = new Color(1f, 0.3f, 0.1f, 1f);
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, fireColor, t);
            yield return null;
        }

        // 불꽃 깜빡임: 빨강 ↔ 주황/노랑 빠르게 반복
        while (burning)
        {
            float flicker = 0.5f + 0.5f * Mathf.Sin(Time.time * 20f);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(
                    1f,
                    Mathf.Lerp(0.0f, 0.6f, flicker),
                    0f,
                    1.0f
                );
            yield return null;
        }
    }

    IEnumerator FadeToOriginal()
    {
        float elapsed = 0f;
        float fadeTime = 0.3f;
        Color currentColor = spriteRenderer != null ? spriteRenderer.color : originalColor;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(currentColor, originalColor, t);
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void OnDestroy()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}