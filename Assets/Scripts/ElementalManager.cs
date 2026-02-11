using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections;

public class ElementalManager : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image screenEffectImage;

    [Header("Fire Settings")]
    public Sprite[] fireHeartSprites;

    [Header("Ice Settings")]
    public Sprite[] iceHeartSprites;

    [Header("General Settings")]
    public Sprite defaultHeartSprite;
    public float animSpeed = 10f;

    private Tilemap[] allMaps;
    private float savedHealth;
    private bool isAbilityActive = false;
    private Material screenMat;

    void Start()
    {
        // 1. Setup Screen Effect Material
        if (screenEffectImage != null)
        {
            screenEffectImage.gameObject.SetActive(false);
            if (screenEffectImage.material != null)
            {
                // Create a material instance to prevent modifying the asset
                screenMat = new Material(screenEffectImage.material);
                screenEffectImage.material = screenMat;
            }
        }

        // 2. Auto-assign default sprite if missing
        if (defaultHeartSprite == null && playerHealth != null && playerHealth.hearts.Length > 0)
        {
            if (playerHealth.hearts[0] != null)
                defaultHeartSprite = playerHealth.hearts[0].sprite;
        }

        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
    }

    void Update()
    {
        // Deactivate if health drops by 2 or more
        if (isAbilityActive)
        {
            if (playerHealth.currentHealth <= savedHealth - 2.0f) DeactivateAbility();
        }

        // Input Handling
        if (Input.GetKeyDown(KeyCode.Alpha4)) ActivateAbility("Fire");
        if (Input.GetKeyDown(KeyCode.Alpha5)) ActivateAbility("Ice");
        if (Input.GetKeyDown(KeyCode.Alpha6)) ActivateAbility("Poison");
    }

    public void ActivateAbility(string type)
    {
        if (playerHealth == null) return;

        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        isAbilityActive = true;
        savedHealth = playerHealth.currentHealth;

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(AbilityLoop(type));
    }

    public void DeactivateAbility()
    {
        if (!isAbilityActive) return;
        isAbilityActive = false;

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(false);

        // Reset Hearts
        if (playerHealth.hearts != null)
        {
            foreach (var img in playerHealth.hearts)
            {
                if (img != null)
                {
                    img.color = Color.white;
                    img.transform.localScale = Vector3.one;
                    if (defaultHeartSprite != null) img.sprite = defaultHeartSprite;
                }
            }
        }

        // Reset Map Color
        if (allMaps != null)
        {
            foreach (var map in allMaps)
                if (map != null) map.color = Color.white;
        }
    }

    IEnumerator AbilityLoop(string type)
    {
        // ------------------------------------------------
        // 1. Setup Target Values
        // ------------------------------------------------
        // 화면 하단 높이 설정 (0.9f)
        float targetRadius = 0.9f;

        Color targetCore = Color.white;
        Color targetEdge = Color.white;
        Vector2 scrollSpeed = Vector2.zero;
        float targetSoftness = 0.3f;

        if (type == "Fire")
        {
            // ★ [핵심 수정] 요청하신 밝고 쨍한 황금빛 주황색 적용
            // Core: 노란색에 가까운 아주 밝은 금색 (RGB 값을 높임)
            targetCore = new Color(1.0f, 0.9f, 0.4f, 0.85f);
            // Edge: 선명하고 채도 높은 밝은 주황색
            targetEdge = new Color(1.0f, 0.6f, 0.1f, 0.85f);

            scrollSpeed = new Vector2(0.1f, 1.5f);
            targetSoftness = 0.5f;
        }
        else if (type == "Ice")
        {
            // Ice: Soft Cyan/Blue
            targetCore = new Color(0.6f, 0.9f, 1.0f, 0.6f);
            targetEdge = new Color(0.0f, 0.4f, 1.0f, 0.6f);
            scrollSpeed = new Vector2(0.02f, 0.05f);
            targetSoftness = 0.4f;
        }
        else if (type == "Poison")
        {
            // Poison: Soft Green
            targetCore = new Color(0.7f, 1.0f, 0.7f, 0.6f);
            targetEdge = new Color(0.1f, 0.6f, 0.1f, 0.6f);
            scrollSpeed = new Vector2(0.08f, 0.7f);
            targetSoftness = 0.6f;
        }

        // Apply Initial Shader Settings
        if (screenMat != null)
        {
            screenMat.SetVector("_ScrollSpeed", scrollSpeed);
            screenMat.SetFloat("_Softness", targetSoftness);
            screenMat.SetColor("_CoreColor", targetCore);
            screenMat.SetColor("_EdgeColor", targetEdge);

            // Intro Logic
            if (type == "Fire")
            {
                screenMat.SetFloat("_Radius", targetRadius);
                screenMat.SetFloat("_Progress", 0f);
            }
            else
            {
                screenMat.SetFloat("_Radius", 1.5f);
                screenMat.SetFloat("_Progress", 1.0f);
            }
        }

        // ------------------------------------------------
        // 2. Intro Animation Loop
        // ------------------------------------------------
        float duration = 0.7f;
        float timer = 0f;
        float currentScale = 1.0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            if (screenMat != null)
            {
                screenMat.SetColor("_CoreColor", targetCore);
                screenMat.SetColor("_EdgeColor", targetEdge);

                if (type == "Fire")
                {
                    screenMat.SetFloat("_Progress", progress);
                }
                else
                {
                    float currentRadius = Mathf.Lerp(1.5f, targetRadius, progress);
                    screenMat.SetFloat("_Radius", currentRadius);
                }
            }

            float targetHeartScale = (type == "Fire") ? 1.8f : (type == "Ice" ? 1.2f : 1.0f);
            currentScale = Mathf.Lerp(1.0f, targetHeartScale, progress);

            UpdateHeartVisuals(type, Time.time, currentScale);

            yield return null;
        }

        // Finalize Intro
        if (screenMat != null)
        {
            screenMat.SetFloat("_Progress", 1.0f);
            screenMat.SetFloat("_Radius", targetRadius);
        }

        // ------------------------------------------------
        // 3. Main Loop
        // ------------------------------------------------
        while (isAbilityActive)
        {
            float t = Time.time;

            if (screenMat != null)
            {
                screenMat.SetColor("_CoreColor", targetCore);
                screenMat.SetColor("_EdgeColor", targetEdge);
                screenMat.SetFloat("_Progress", 1.0f);
                screenMat.SetFloat("_Radius", targetRadius);
            }

            // Map Color Logic
            Color targetMapColor = Color.white;
            if (type == "Fire") targetMapColor = Color.Lerp(new Color(1f, 0.96f, 0.96f), new Color(1f, 0.99f, 0.97f), Mathf.PerlinNoise(t * 2.0f, 0f));
            else if (type == "Ice") targetMapColor = new Color(0.9f, 0.95f, 1.0f);
            else if (type == "Poison") targetMapColor = Color.Lerp(new Color(0.97f, 1f, 0.97f), new Color(0.99f, 1f, 0.99f), (Mathf.Sin(t * 2f) + 1f) * 0.5f);

            if (allMaps != null) { foreach (var map in allMaps) if (map != null) map.color = targetMapColor; }

            float finalTargetScale = (type == "Fire") ? 1.8f : (type == "Ice" ? 1.2f : 1.0f);
            currentScale = Mathf.Lerp(currentScale, finalTargetScale, Time.deltaTime * 5f);

            UpdateHeartVisuals(type, t, currentScale);

            yield return null;
        }
    }

    void UpdateHeartVisuals(string type, float time, float scale)
    {
        int fireIndex = 0;
        int iceIndex = 0;

        if (fireHeartSprites != null && fireHeartSprites.Length > 0)
            fireIndex = (int)(time * animSpeed) % fireHeartSprites.Length;

        if (iceHeartSprites != null && iceHeartSprites.Length > 0)
            iceIndex = (int)(time * animSpeed) % iceHeartSprites.Length;

        for (int i = 0; i < playerHealth.hearts.Length; i++)
        {
            Image img = playerHealth.hearts[i];
            if (img == null || img.fillAmount <= 0) continue;
            float offset = i * 0.3f;

            if (type == "Fire")
            {
                if (fireHeartSprites != null && fireHeartSprites.Length > 0)
                    img.sprite = fireHeartSprites[fireIndex];

                float jitter = Mathf.PerlinNoise(time * 10f + offset, 0f);
                img.transform.localScale = Vector3.one * (scale + jitter * 0.2f);
                img.color = Color.white;
            }
            else if (type == "Ice")
            {
                if (iceHeartSprites != null && iceHeartSprites.Length > 0)
                {
                    img.sprite = iceHeartSprites[iceIndex];
                    img.color = Color.white;
                }
                else
                {
                    if (defaultHeartSprite != null) img.sprite = defaultHeartSprite;
                    img.color = new Color(0.5f, 0.8f, 1.0f);
                }

                float breathe = Mathf.Sin(time * 2f + offset) * 0.05f;
                img.transform.localScale = Vector3.one * (scale + breathe);
            }
            else if (type == "Poison")
            {
                if (defaultHeartSprite != null) img.sprite = defaultHeartSprite;
                float spasm = Mathf.PerlinNoise(time * 3f + offset, 0f);
                img.transform.localScale = Vector3.one * (scale + spasm * 0.2f);
                img.color = Color.Lerp(Color.white, new Color(0.7f, 1f, 0.7f), spasm);
            }
        }
    }
}