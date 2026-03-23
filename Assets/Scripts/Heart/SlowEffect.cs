using UnityEngine;
using System.Collections;

public class SlowEffect : MonoBehaviour
{
    public float slowPercent = 50f;
    public float duration = 2f;

    private EnemyStats enemyStats;
    private MeleeEnemy meleeEnemy;
    private RangedEnemy rangedEnemy;
    private float originalSpeed;
    private bool applied = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private ElementalManager elementalManager;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        meleeEnemy = GetComponent<MeleeEnemy>();
        rangedEnemy = GetComponent<RangedEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        elementalManager = FindFirstObjectByType<ElementalManager>();

        if (enemyStats == null && meleeEnemy == null && rangedEnemy == null)
        {
            Destroy(this);
            return;
        }

        if (!applied)
        {
            applied = true;

            if (meleeEnemy != null)
            {
                originalSpeed = meleeEnemy.moveSpeed;
                meleeEnemy.moveSpeed *= (1f - slowPercent / 100f);
            }
            else if (rangedEnemy != null)
            {
                originalSpeed = rangedEnemy.moveSpeed;
                rangedEnemy.moveSpeed *= (1f - slowPercent / 100f);
            }
            else if (enemyStats != null)
            {
                originalSpeed = enemyStats.moveSpeed;
                enemyStats.moveSpeed *= (1f - slowPercent / 100f);
            }

            Debug.Log($"[슬로우] 이동속도 {slowPercent}% 감소");

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;

            StartCoroutine(ApplyColorEffect());
            StartCoroutine(RemoveSlow());
        }
    }

    IEnumerator ApplyColorEffect()
    {
        // 페이드인: 원본 → 파란색
        float elapsed = 0f;
        float fadeTime = 0.3f;
        Color iceColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, iceColor, t);
            yield return null;
        }

        // 슬로우 지속 중 살짝 깜빡임
        while (applied)
        {
            // ? 얼음 하트 없어지면 즉시 슬로우 해제
            if (elementalManager != null && !elementalManager.hasIceHeart)
            {
                applied = false;
                break;
            }

            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 3.0f);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(
                    Mathf.Lerp(originalColor.r, 0.4f, pulse),
                    Mathf.Lerp(originalColor.g, 0.8f, pulse),
                    Mathf.Lerp(originalColor.b, 1.0f, pulse),
                    1.0f
                );
            yield return null;
        }

        // 루프 빠져나오면 페이드아웃
        yield return StartCoroutine(FadeOutColor());
    }

    IEnumerator RemoveSlow()
    {
        yield return new WaitForSeconds(duration);

        applied = false;

        if (meleeEnemy != null)
            meleeEnemy.moveSpeed = originalSpeed;
        else if (rangedEnemy != null)
            rangedEnemy.moveSpeed = originalSpeed;
        else if (enemyStats != null)
            enemyStats.moveSpeed = originalSpeed;

        Debug.Log("[슬로우] 해제");

        yield return new WaitForSeconds(0.4f);
        Destroy(this);
    }

    IEnumerator FadeOutColor()
    {
        // 이동속도 복원
        if (meleeEnemy != null)
            meleeEnemy.moveSpeed = originalSpeed;
        else if (rangedEnemy != null)
            rangedEnemy.moveSpeed = originalSpeed;
        else if (enemyStats != null)
            enemyStats.moveSpeed = originalSpeed;

        // 페이드아웃: 파란색 → 원본
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

        Destroy(this);
    }

    void OnDestroy()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}