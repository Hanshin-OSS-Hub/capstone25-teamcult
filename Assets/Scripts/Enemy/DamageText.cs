using UnityEngine;
using TMPro;
using System.Collections;
public class DamageText : MonoBehaviour
{
    public TextMeshPro tmp;
    public float duration = 1f;
    public float riseSpeed = 2f;

    public void Setup(int damage)
    {
        Setup(damage, Color.white);
    }

    public void Setup(int damage, Color color)
    {
        if (tmp == null) tmp = GetComponent<TextMeshPro>();
        tmp.text = damage.ToString();
        tmp.color = color;
        StartCoroutine(Animate());
    }

    public void SetupText(string text, Color color)
    {
        if (tmp == null) tmp = GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        float bounceHeight = 0.4f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2.5f)) * bounceHeight * (1f - t);

            transform.position = startPos + new Vector3(
                0f,
                t * riseSpeed * 0.5f + bounce,
                0f
            );

            float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);

            float scale = 1f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.3f * (1f - t);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }
        Destroy(gameObject);
    }
}