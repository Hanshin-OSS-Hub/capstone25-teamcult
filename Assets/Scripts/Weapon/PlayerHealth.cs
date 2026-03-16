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
    public SpriteRenderer playerSprite;
    public float flickerDuration = 1.0f;
    public float flickerInterval = 0.1f;

    private Coroutine flickerCoroutine;
    private bool isInvincible = false;

    // 광전사 모드 상태
    private bool isBerserker = false;
    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();

        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite == null)
                playerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (hearts == null || hearts.Length == 0)
        {
            GameObject container = GameObject.Find("HeartContainer");
            if (container != null)
                hearts = container.GetComponentsInChildren<Image>();
        }

        if (hearts != null && hearts.Length > 0)
            maxHealth = hearts.Length * 2;
        else
            maxHealth = 6;

        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7)) TakeDamage(1);

        // 광전사 모드 체크
        CheckBerserker();
    }

    void CheckBerserker()
    {
        if (stats == null || !stats.berserkerMode) return;

        // 체력 1칸 (2 이하) 남으면 광전사 발동
        if (currentHealth <= 2f && !isBerserker)
        {
            isBerserker = true;
            stats.bonusAttackPercent += 30f;
            stats.bonusAttackSpeed += 50f;
            Debug.Log("[광전사] 발동! 공격력 +30%, 공격속도 +50%");
        }
        else if (currentHealth > 2f && isBerserker)
        {
            isBerserker = false;
            stats.bonusAttackPercent -= 30f;
            stats.bonusAttackSpeed -= 50f;
            Debug.Log("[광전사] 해제!");
        }
    }

    public void TakeDamage(int damage)
    {
        // 무적 상태면 데미지 무시
        if (isInvincible) return;

        // 데미지 무효 확률 체크
        if (stats != null && stats.damageNullifyChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < stats.damageNullifyChance)
            {
                Debug.Log("[데미지 무효] 발동!");
                return;
            }
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"HP Left: {currentHealth}");
        UpdateUI();

        if (playerSprite != null && currentHealth > 0)
        {
            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            flickerCoroutine = StartCoroutine(DamageFlicker());
        }

        // 피격 무적시간 적용
        if (currentHealth > 0)
        {
            float invincTime = flickerDuration;
            if (stats != null) invincTime += stats.invincibilityBonus;
            StartCoroutine(InvincibilityCoroutine(invincTime));
        }

        if (currentHealth <= 0) Die();
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    IEnumerator DamageFlicker()
    {
        float timer = 0f;
        while (timer < flickerDuration)
        {
            playerSprite.enabled = !playerSprite.enabled;
            timer += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }
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
        if (playerSprite != null) playerSprite.enabled = true;
        gameObject.SetActive(false);
    }

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