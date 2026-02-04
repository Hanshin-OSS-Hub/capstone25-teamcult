using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float currentHealth;
    public float maxHealth;

    [Header("UI Settings")]
    public Image[] hearts;
    public Sprite redHeartSprite;

    [Header("Flicker Settings (On Hit)")]
    public SpriteRenderer playerSprite; // Drag player sprite here if auto-find fails
    public float flickerDuration = 1.0f; // How long it flickers
    public float flickerInterval = 0.1f; // How fast it blinks

    // Internal Variables
    private Coroutine flickerCoroutine;

    void Start()
    {
        // 1. Auto-find SpriteRenderer if not assigned
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
            // If the script is on a parent object, try finding it in children
            if (playerSprite == null)
            {
                playerSprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (playerSprite == null)
            {
                Debug.LogError("PlayerHealth: Could not find SpriteRenderer! Flickering won't work.");
            }
        }

        // 2. Auto-find Heart UI
        if (hearts == null || hearts.Length == 0)
        {
            GameObject container = GameObject.Find("HeartContainer");
            if (container != null)
                hearts = container.GetComponentsInChildren<Image>();
        }

        // 3. Initialize Health
        if (hearts != null && hearts.Length > 0)
            maxHealth = hearts.Length * 2;
        else
            maxHealth = 6;

        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        // Test Key 7: Take Damage
        if (Input.GetKeyDown(KeyCode.Alpha7)) TakeDamage(1);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"HP Left: {currentHealth}");
        UpdateUI();

        // ★ Start Flicker Effect
        if (playerSprite != null && currentHealth > 0)
        {
            // Stop existing flicker to prevent overlapping conflicts
            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            // Start new flicker
            flickerCoroutine = StartCoroutine(DamageFlicker());
        }

        if (currentHealth <= 0) Die();
    }

    // ★ Flicker Coroutine logic
    IEnumerator DamageFlicker()
    {
        float timer = 0f;

        while (timer < flickerDuration)
        {
            // Toggle renderer on/off
            playerSprite.enabled = !playerSprite.enabled;

            // Wait for interval
            timer += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }

        // Important: Ensure sprite is visible when finished
        playerSprite.enabled = true;
        flickerCoroutine = null;
    }

    public void UpdateUI()
    {
        if (hearts == null) return;

        float healthPerHeart = 2f;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            float startThreshold = i * healthPerHeart;
            float fillValue = (currentHealth - startThreshold) / healthPerHeart;
            hearts[i].fillAmount = Mathf.Clamp01(fillValue);
        }
    }

    void Die()
    {
        // Ensure sprite is on before disabling object
        if (playerSprite != null) playerSprite.enabled = true;
        gameObject.SetActive(false);
    }

    // =========================================================
    // Compatibility Methods (Fixed for CS7036 Error)
    // "int amount = 1" makes the parameter optional.
    // =========================================================

    public void GetFlameHeart(int amount = 1)
    {
        ElementalManager manager = GetComponent<ElementalManager>();
        if (manager != null) manager.ActivateAbility("Fire");
    }

    public void GetIceHeart(int amount = 1)
    {
        ElementalManager manager = GetComponent<ElementalManager>();
        if (manager != null) manager.ActivateAbility("Ice");
    }
}