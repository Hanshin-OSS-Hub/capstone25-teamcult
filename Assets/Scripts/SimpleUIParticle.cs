using UnityEngine;
using UnityEngine.UI;

public class SimpleUIParticle : MonoBehaviour
{
    [Header("Particle Settings")]
    public float minLifetime = 0.5f;
    public float maxLifetime = 1.0f;
    public float moveSpeedY = 100f;

    private Image img;
    private float lifetime;
    private float timer;
    private Vector3 startScale;
    private float driftSpeed;

    void Start()
    {
        img = GetComponent<Image>();
        startScale = transform.localScale;

        // Randomize lifetime
        lifetime = Random.Range(minLifetime, maxLifetime);

        // Randomize horizontal drift (Wind effect)
        driftSpeed = Random.Range(-30f, 30f);

        // Randomize initial size
        float randomSize = Random.Range(0.5f, 1.2f);
        transform.localScale = startScale * randomSize;

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        if (progress >= 1f) return;

        // Move: Upwards + Horizontal Drift
        Vector3 moveDir = new Vector3(driftSpeed, moveSpeedY, 0f);
        transform.localPosition += moveDir * Time.deltaTime;

        // Fade Out
        if (img != null)
        {
            Color c = img.color;
            c.a = Mathf.Lerp(1f, 0f, progress);
            img.color = c;
        }

        // Shrink
        transform.localScale = Vector3.Lerp(startScale, startScale * 0.2f, progress);
    }
}