using UnityEngine;
using TMPro;
using System.Collections;
public class DamageText : MonoBehaviour
{
    public TextMeshPro tmp;
    public float duration = 1f;
    public float riseSpeed = 2f;
    // ? int만 받는 버전 (EnemyHealth에서 호출)
    public void Setup(int damage)
    {
        Setup(damage, Color.white);
    }
    // ? 색상도 받는 버전 (화상 등에서 호출)
    public void Setup(int damage, Color color)
    {
        if (tmp == null) tmp = GetComponent<TextMeshPro>();
        tmp.text = damage.ToString();
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
            // 통통 튀는 효과
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2.5f)) * bounceHeight * (1f - t);
            // 위로 올라가면서 튐
            transform.position = startPos + new Vector3(
                0f,
                t * riseSpeed * 0.5f + bounce,
                0f
            );
            // 후반부 페이드아웃
            float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
            // 크기 통통
            float scale = 1f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.3f * (1f - t);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        Destroy(gameObject);
    }
}