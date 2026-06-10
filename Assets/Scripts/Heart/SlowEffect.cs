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
    private ParticleSystem icePS;

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
            icePS = CreateIceParticle();

            StartCoroutine(ApplyColorEffect());
            StartCoroutine(RemoveSlow());
        }
    }

    ParticleSystem CreateIceParticle()
    {
        GameObject psObj = new GameObject("IceParticle");
        psObj.transform.SetParent(transform);

        Bounds bounds = spriteRenderer != null
            ? spriteRenderer.bounds
            : new Bounds(transform.position, Vector3.one);

        psObj.transform.position = new Vector3(
            transform.position.x,
            transform.position.y + bounds.size.y * 0.3f,
            transform.position.z - 0.1f
        );

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = psObj.GetComponent<ParticleSystemRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.color = Color.white;
        psr.material = mat;
        psr.sortingLayerName = "Overhead";
        psr.sortingOrder = 100;

        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.8f, 0.95f, 1.0f, 1f),  
            new Color(0.3f, 0.7f, 1.0f, 1f)   
        );
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.1f, -0.3f); 
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 15f;

        float width = bounds.size.x * 0.7f;
        float height = bounds.size.y * 0.7f;
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, height, 0.01f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.3f, 1.0f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f,   1f,   1f),   0.0f), 
                new GradientColorKey(new Color(0.6f, 0.9f, 1f),   0.3f),  
                new GradientColorKey(new Color(0.3f, 0.7f, 1f),   0.7f),  
                new GradientColorKey(new Color(0.1f, 0.4f, 0.8f), 1.0f),  
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f,   0.0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f,   1.0f),
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.1f;
        noise.frequency = 1f;
        noise.scrollSpeed = 0.5f;

        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-90f, 90f);

        ps.Play();
        return ps;
    }

    IEnumerator ApplyColorEffect()
    {
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

        while (applied)
        {
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

        yield return StartCoroutine(FadeOutColor());
    }

    IEnumerator RemoveSlow()
    {
        yield return new WaitForSeconds(duration);

        applied = false;

        if (meleeEnemy != null) meleeEnemy.moveSpeed = originalSpeed;
        else if (rangedEnemy != null) rangedEnemy.moveSpeed = originalSpeed;
        else if (enemyStats != null) enemyStats.moveSpeed = originalSpeed;

        Debug.Log("[슬로우] 해제");

        if (icePS != null)
        {
            icePS.Stop();
            Destroy(icePS.gameObject, 1f);
        }

        yield return new WaitForSeconds(0.4f);
        Destroy(this);
    }

    IEnumerator FadeOutColor()
    {
        if (meleeEnemy != null) meleeEnemy.moveSpeed = originalSpeed;
        else if (rangedEnemy != null) rangedEnemy.moveSpeed = originalSpeed;
        else if (enemyStats != null) enemyStats.moveSpeed = originalSpeed;

        float elapsed = 0f;
        float fadeTime = 0.3f;
        Color curColor = spriteRenderer != null ? spriteRenderer.color : originalColor;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(curColor, originalColor, t);
            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        Destroy(this);
    }

    void OnDestroy()
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (icePS != null) Destroy(icePS.gameObject);
    }
}