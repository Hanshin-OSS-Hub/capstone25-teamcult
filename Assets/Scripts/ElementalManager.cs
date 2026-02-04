using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElementalManager : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image screenEffectImage;

    [Header("Screen Effect Settings")]
    public Material screenMaterial;

    // Internal variables
    private float savedHealth;
    private bool isAbilityActive = false;
    private string currentType = "";

    void Start()
    {
        // Initialize screen effect material
        if (screenEffectImage != null)
        {
            screenEffectImage.material = new Material(screenMaterial);
            screenEffectImage.material.SetColor("_Color", new Color(0, 0, 0, 0));
            screenEffectImage.raycastTarget = false;
        }
    }

    void Update()
    {
        // Deactivate ability if player loses 1 Full Heart (2 HP)
        if (isAbilityActive)
        {
            if (playerHealth.currentHealth <= savedHealth - 2.0f)
            {
                DeactivateAbility();
            }
        }

        // Test Input Keys
        if (Input.GetKeyDown(KeyCode.Alpha4)) ActivateAbility("Fire");
        if (Input.GetKeyDown(KeyCode.Alpha5)) ActivateAbility("Ice");
        if (Input.GetKeyDown(KeyCode.Alpha6)) ActivateAbility("Poison");
    }

    public void ActivateAbility(string type)
    {
        if (playerHealth == null) return;

        isAbilityActive = true;
        currentType = type;
        savedHealth = playerHealth.currentHealth;

        StopAllCoroutines();
        StartCoroutine(AbilityLoop(type));
    }

    public void DeactivateAbility()
    {
        if (!isAbilityActive) return;

        Debug.Log("Ability Deactivated!");
        isAbilityActive = false;
        currentType = "";

        // Reset Screen Effect
        if (screenEffectImage != null)
            screenEffectImage.material.SetColor("_Color", new Color(0, 0, 0, 0));

        // Reset Heart UI
        if (playerHealth.hearts != null)
        {
            foreach (var img in playerHealth.hearts)
            {
                if (img != null)
                {
                    img.color = Color.white;
                    img.transform.localScale = Vector3.one;
                }
            }
        }
    }

    IEnumerator AbilityLoop(string type)
    {
        Material mat = screenEffectImage.material;

        // Poison Colors (Green Only)
        Color brightGreen = new Color(0.2f, 1f, 0.2f);
        Color darkGreen = new Color(0.1f, 0.4f, 0.1f);

        while (isAbilityActive)
        {
            float t = Time.time;

            for (int i = 0; i < playerHealth.hearts.Length; i++)
            {
                Image img = playerHealth.hearts[i];
                if (img == null || img.fillAmount <= 0) continue;

                float offset = i * 0.3f;

                if (type == "Fire")
                {
                    // Fire: Fast Flicker & Jitter
                    // 1. Color: Rapidly switch between Red, Orange, Yellow
                    float colorNoise = Mathf.PerlinNoise(t * 25f + offset, 0f);
                    if (colorNoise < 0.4f) img.color = Color.red;
                    else if (colorNoise < 0.7f) img.color = new Color(1f, 0.5f, 0f);
                    else img.color = Color.yellow;

                    // 2. Motion: Jitter upwards
                    float jitterX = Mathf.PerlinNoise(t * 30f + offset, 10f) * 0.15f;
                    float jitterY = Mathf.PerlinNoise(t * 30f + offset, 50f) * 0.35f;
                    img.transform.localScale = new Vector3(1.0f + jitterX, 1.0f + Mathf.Abs(jitterY), 1f);

                    // Screen Effect: Fast
                    mat.SetColor("_Color", new Color(1f, 0.2f, 0f, 0.3f));
                    mat.SetFloat("_DistortStrength", 0.02f);
                    mat.SetFloat("_Speed", 3.0f);
                }
                else if (type == "Ice")
                {
                    // Ice: Synchronized Slow Breathing
                    // 1. Color: Cyan <-> Blue
                    img.color = Color.Lerp(Color.cyan, Color.blue, Mathf.PingPong(t * 2f, 1f));

                    // 2. Motion: Slow Sine Wave
                    float breathe = 1.0f + Mathf.Sin(t * 2f) * 0.1f;
                    img.transform.localScale = new Vector3(breathe, breathe, 1f);

                    // Screen Effect: Slow
                    mat.SetColor("_Color", new Color(0f, 1f, 1f, 0.3f));
                    mat.SetFloat("_DistortStrength", 0.002f);
                    mat.SetFloat("_Speed", 0.5f);
                }
                else if (type == "Poison")
                {
                    // Poison: Slow, Viscous, Throbbing

                    // 1. Color: Very slow transition (Dark Green <-> Bright Green)
                    img.color = Color.Lerp(darkGreen, brightGreen, Mathf.PingPong((t * 1.0f) + offset, 1f));

                    // 2. Motion: Slow swelling (like an infection)
                    // Lower noise frequency for slower changes
                    float spasmNoise = Mathf.PerlinNoise((t * 2.0f) + offset, 10f);

                    // Trigger swelling only when noise is high
                    float targetScale = (spasmNoise > 0.6f) ? 1.2f : 1.0f;

                    // Smoothly interpolate scale (Viscous feel)
                    img.transform.localScale = Vector3.Lerp(img.transform.localScale, new Vector3(targetScale, targetScale, 1f), Time.deltaTime * 2f);

                    // Screen Effect: Slow and dizzy
                    mat.SetColor("_Color", new Color(0.2f, 1f, 0.2f, 0.2f));
                    mat.SetFloat("_DistortStrength", 0.015f);
                    mat.SetFloat("_Speed", 0.5f);
                }
            }
            yield return null;
        }
    }
}