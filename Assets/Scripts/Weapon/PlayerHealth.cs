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

    [Header("Death Settings")]
    public float deathAnimDuration = 1.5f;

    private Coroutine flickerCoroutine;
    private bool isInvincible = false;
    private bool isDead = false;

    private bool isBerserker = false;
    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();

        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (hearts == null || hearts.Length == 0)
        {
            GameObject container = GameObject.Find("HeartContainer");
            if (container != null) hearts = container.GetComponentsInChildren<Image>();
        }

        if (hearts != null && hearts.Length > 0) maxHealth = hearts.Length * 8;
        else maxHealth = 24;

        currentHealth = 24f;
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7)) TakeDamage(1);
        CheckBerserker();
    }

    void CheckBerserker()
    {
        if (stats == null || !stats.berserkerMode) return;

        if (currentHealth <= 2f && !isBerserker)
        {
            isBerserker = true;
            stats.bonusAttackPercent += 30f;
            stats.bonusAttackSpeed += 50f;
        }
        else if (currentHealth > 2f && isBerserker)
        {
            isBerserker = false;
            stats.bonusAttackPercent -= 30f;
            stats.bonusAttackSpeed -= 50f;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (isInvincible) return;

        if (stats != null && stats.damageNullifyChance > 0)
        {
            if (Random.Range(0f, 100f) < stats.damageNullifyChance) return;
        }

        // 방어력 계산: max(공격력 - 방어력, 1)
        int defenseVal = (stats != null) ? stats.GetTotalDefense() : 0;
        int finalDamage = Mathf.Max(damage - defenseVal, 1);

        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.TriggerGlitch();

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (BattleStateBGM.Instance != null)
            BattleStateBGM.Instance.SetLowHealth(currentHealth <= 8f && currentHealth > 0);

        if (playerSprite != null && currentHealth > 0)
        {
            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            flickerCoroutine = StartCoroutine(DamageFlicker());
        }

        if (currentHealth > 0)
        {
            float invincTime = flickerDuration;
            if (stats != null) invincTime += stats.invincibilityBonus;
            StartCoroutine(InvincibilityCoroutine(invincTime));
        }

        if (currentHealth <= 0) StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        if (isDead) yield break;
        isDead = true;

        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.TriggerGameOver();

        if (flickerCoroutine != null) { StopCoroutine(flickerCoroutine); flickerCoroutine = null; }

        if (GameManager.instance != null && GameManager.instance.HasRevive())
        {
            GameManager.instance.UseRevive();
            yield return StartCoroutine(ReviveSequence());
            yield break;
        }

        yield return StartCoroutine(DeathAnimation());

        if (GameManager.instance != null) GameManager.instance.GameOver();
        else gameObject.SetActive(false);
    }

    IEnumerator DeathAnimation()
    {
        if (playerSprite != null)
        {
            float timer = 0f;
            float fastInterval = 0.05f;
            while (timer < deathAnimDuration * 0.6f)
            {
                playerSprite.enabled = !playerSprite.enabled;
                timer += fastInterval;
                yield return new WaitForSeconds(fastInterval);
            }

            Color c = playerSprite.color;
            float fadeTimer = 0f;
            float fadeDuration = deathAnimDuration * 0.4f;
            playerSprite.enabled = true;
            while (fadeTimer < fadeDuration)
            {
                fadeTimer += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
                playerSprite.color = c;
                yield return null;
            }
            c.a = 0f;
            playerSprite.color = c;
        }
        else yield return new WaitForSeconds(deathAnimDuration);
    }

    IEnumerator ReviveSequence()
    {
        currentHealth = Mathf.Max(1f, maxHealth * 0.5f);
        isDead = false;
        UpdateUI();

        if (BattleStateBGM.Instance != null)
            BattleStateBGM.Instance.SetLowHealth(currentHealth <= 8f && currentHealth > 0);

        if (playerSprite != null) { playerSprite.enabled = true; Color c = playerSprite.color; c.a = 1f; playerSprite.color = c; }

        float reviveInvincDuration = 3f;
        if (stats != null) reviveInvincDuration += stats.invincibilityBonus;
        StartCoroutine(InvincibilityCoroutine(reviveInvincDuration));
        yield return StartCoroutine(ReviveFlicker(reviveInvincDuration));
    }

    IEnumerator ReviveFlicker(float duration) { if (playerSprite == null) yield break; float timer = 0f; float interval = 0.15f; while (timer < duration) { playerSprite.enabled = !playerSprite.enabled; timer += interval; yield return new WaitForSeconds(interval); } playerSprite.enabled = true; }
    IEnumerator InvincibilityCoroutine(float duration) { isInvincible = true; yield return new WaitForSeconds(duration); isInvincible = false; }
    IEnumerator DamageFlicker() { float timer = 0f; while (timer < flickerDuration) { playerSprite.enabled = !playerSprite.enabled; timer += flickerInterval; yield return new WaitForSeconds(flickerInterval); } playerSprite.enabled = true; flickerCoroutine = null; }

    public void UpdateUI()
    {
        if (hearts == null) return;
        float healthPerHeart = 8f;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            float startThreshold = i * healthPerHeart;
            float fillValue = (currentHealth - startThreshold) / healthPerHeart;
            hearts[i].fillAmount = Mathf.Clamp01(fillValue);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.SetLowHealth(currentHealth <= 8f && currentHealth > 0);
    }

    public void GetFlameHeart(int amount = 1) { ElementalManager manager = GetComponent<ElementalManager>(); if (manager != null) manager.ActivateAbility("Fire"); }
    public void GetIceHeart(int amount = 1) { ElementalManager manager = GetComponent<ElementalManager>(); if (manager != null) manager.ActivateAbility("Ice"); }
}