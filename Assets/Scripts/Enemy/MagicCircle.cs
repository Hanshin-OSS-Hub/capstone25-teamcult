using System.Collections;
using UnityEngine;

public class MagicCircle : MonoBehaviour
{
    public float warningTime = 1.5f;
    public int damage = 20;
    public float maxScale = 5f;

    private ParticleSystem explosion;

    void Awake()
    {
        explosion = CreateExplosionParticle();
        transform.localScale = Vector3.zero;
    }

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return null;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / warningTime;
            transform.localScale = Vector3.one * maxScale * t;
            transform.Rotate(0f, 0f, 180f * Time.unscaledDeltaTime);
            yield return null;
        }

        for (int i = 0; i < 3; i++)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSecondsRealtime(0.1f);
            GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        explosion.Play();

        float worldRadius = transform.localScale.x * 0.5f;
        Debug.Log("worldRadius: " + worldRadius + " / scale: " + transform.localScale.x);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, worldRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }

        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSecondsRealtime(explosion.main.duration);

        Destroy(gameObject);
    }

    ParticleSystem CreateExplosionParticle()
    {
        GameObject go = new GameObject("Explosion");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop();

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.4f, 0f),
            new Color(1f, 0.1f, 0f)
        );
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 40)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.5f);
        curve.AddKey(0.3f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        return ps;
    }
}