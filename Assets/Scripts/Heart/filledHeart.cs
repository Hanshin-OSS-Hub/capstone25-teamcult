using UnityEngine;
using UnityEngine.UI;

// ★ 이 부분이 클래스 밖에 있어야 다른 스크립트가 알아먹습니다!
public enum HeartAttribute
{
    Normal,
    Fire,
    Ice,
    Poison,
    Electric
}

public class filledHeart : MonoBehaviour
{

    [Header("Heart Stats")]
    public int HP;

    private Image heartImage;

    [Header("Attribute Settings")]
    [SerializeField] private HeartAttribute currentAttribute = HeartAttribute.Normal;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fireColor = new Color(1f, 0.5f, 0.5f); // 붉은 계열
    [SerializeField] private Color iceColor = new Color(0.5f, 0.5f, 1f);
    [SerializeField] private Color poisonColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color electricColor = new Color(0.5f, 1f, 1f);

    void Awake()
    {
        heartImage = GetComponent<Image>();
        UpdateColorByAttribute();
    }

    private void OnValidate()
    {
        if (heartImage == null) heartImage = GetComponent<Image>();
        UpdateColorByAttribute();
    }

    public void SetAttribute(HeartAttribute newAttribute)
    {
        currentAttribute = newAttribute;
        UpdateColorByAttribute();
    }

    private void UpdateColorByAttribute()
    {
        if (heartImage == null) return;

        Color targetColor;

        switch (currentAttribute)
        {
            case HeartAttribute.Normal: targetColor = normalColor; break;
            case HeartAttribute.Fire: targetColor = fireColor; break;
            case HeartAttribute.Ice: targetColor = iceColor; break;
            case HeartAttribute.Poison: targetColor = poisonColor; break;
            case HeartAttribute.Electric: targetColor = electricColor; break;
            default: targetColor = normalColor; break;
        }

        heartImage.color = targetColor;

        // 속성 변할 때 '쿵' 효과 (선택사항)
        if (currentAttribute != HeartAttribute.Normal) StartCoroutine(PulseEffect());
    }

    private System.Collections.IEnumerator PulseEffect()
    {
        Transform t = transform;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.2f;

        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            t.localScale = Vector3.Lerp(originalScale, targetScale, timer / 0.1f);
            yield return null;
        }

        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            t.localScale = Vector3.Lerp(targetScale, originalScale, timer / 0.1f);
            yield return null;
        }
        t.localScale = originalScale;
    }
}