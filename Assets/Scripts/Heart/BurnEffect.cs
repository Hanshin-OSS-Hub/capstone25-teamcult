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
    private ParticleSystem firePS;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyHealth == null) { Destroy(this); return; }
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        firePS = CreateFireParticle();

        burning = true;
        StartCoroutine(BurnTick());
        StartCoroutine(BurnFlicker());
    }

    ParticleSystem CreateFireParticle()
    {
        GameObject psObj = new GameObject("FireParticle");
        psObj.transform.SetParent(transform);

        Bounds bounds = spriteRenderer != null ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.one);
        psObj.transform.position = new Vector3(
            transform.position.x,
            bounds.min.y,
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
        psr.renderMode = ParticleSystemRenderMode.Billboard;

        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.8f, 0.0f, 1f),
            new Color(1f, 0.2f, 0.0f, 1f)
        );
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.3f, -0.8f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 20f;

        float width = bounds.size.x * 0.6f;
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, 0.05f, 0.01f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1.0f);
        sizeCurve.AddKey(0.5f, 0.7f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 0.0f), 0.0f),
                new GradientColorKey(new Color(1f, 0.4f, 0.0f), 0.4f),
                new GradientColorKey(new Color(1f, 0.1f, 0.0f), 0.7f),
                new GradientColorKey(new Color(0.3f, 0f, 0.0f), 1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0.0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1.0f),
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 2f;
        noise.scrollSpeed = 1f;

        ps.Play();
        return ps;
    }

    IEnumerator BurnTick()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            if (enemyHealth != null)
                enemyHealth.TakeDamageIgnoreDefense((int)damage); // 방어력 무시!
        }
        burning = false;

        if (firePS != null)
        {
            firePS.Stop();
            Destroy(firePS.gameObject, 1f);
        }

        yield return StartCoroutine(FadeToOriginal());
        Destroy(this);
    }

    IEnumerator BurnFlicker()
    {
        float elapsed = 0f;
        float fadeTime = 0.2f;
        Color fireColor = new Color(1f, 0.4f, 0.1f, 1f);

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, fireColor, t);
            yield return null;
        }

        while (burning)
        {
            float flicker = 0.5f + 0.5f * Mathf.Sin(Time.time * 20f);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(
                    1f,
                    Mathf.Lerp(0.1f, 0.5f, flicker),
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
    }

    void OnDestroy()
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (firePS != null) Destroy(firePS.gameObject);
    }
}