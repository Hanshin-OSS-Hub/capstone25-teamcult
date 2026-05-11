using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int currentHealth;
    private EnemyStats stats;

    [Header("보상 설정")]
    public int expReward = 10;

    [Header("UI 설정")]
    public TMP_Text nameText;
    public Slider hpSlider;
    public TMP_Text hpText;

    [Header("플로팅 효과")]
    public GameObject damageTextPrefab;

    [Header("마석")]
    public GameObject maSeokPrefab;
    [Range(0f, 100f)]
    public float maSeokDropChance = 50f;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            if (nameText != null) nameText.text = stats.enemyName;
        }
        else
        {
            currentHealth = 30;
            if (nameText != null) nameText.text = "Unknown";
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = currentHealth;
            hpSlider.value = currentHealth;
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        int defenseVal = (stats != null) ? stats.defense : 0;

        // 새 방식: max(공격력 - 방어력, 1)
        int finalDamage = Mathf.Max(damage - defenseVal, 1);

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = hpSlider != null
                ? hpSlider.transform.position + new Vector3(0, 0.5f, 0)
                : transform.position + new Vector3(0, 1.5f, 0);

            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            DamageText dmgTextScript = textObj.GetComponent<DamageText>();
            if (dmgTextScript != null) dmgTextScript.Setup(finalDamage);
        }

        if (currentHealth <= 0)
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyDeath);
            Die();
        }
        else
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyHit);
        }
    }

    // 화염 도트딜 - 방어력 무시
    public void TakeDamageIgnoreDefense(int damage)
    {
        if (damage < 1) damage = 1;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = hpSlider != null
                ? hpSlider.transform.position + new Vector3(0, 0.5f, 0)
                : transform.position + new Vector3(0, 1.5f, 0);

            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            DamageText dmgTextScript = textObj.GetComponent<DamageText>();
            if (dmgTextScript != null) dmgTextScript.Setup(damage);
        }

        if (currentHealth <= 0)
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyDeath);
            Die();
        }
        else
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyHit);
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
        if (hpText != null)
        {
            int max = (stats != null) ? stats.maxHealth : (int)hpSlider.maxValue;
            hpText.text = $"{currentHealth} / {max}";
        }
    }

    void Die()
    {
        if (GameManager.instance != null)
            GameManager.instance.killCount++;

        if (maSeokPrefab != null)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < maSeokDropChance)
                Instantiate(maSeokPrefab, transform.position, Quaternion.identity);
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            PlayerExp expScript = player.GetComponent<PlayerExp>();

            if (expScript != null)
            {
                float multiplier = (playerStats != null) ? playerStats.expMultiplier : 1f;
                int finalExp = Mathf.RoundToInt(expReward * multiplier);
                expScript.GetExp(finalExp);
            }

            if (playerStats != null)
            {
                if (playerStats.killMoveSpeedStack > 0)
                {
                    playerStats.moveSpeed += playerStats.killMoveSpeedStack;
                }
                if (playerStats.killGoldChance > 0)
                {
                    float roll = Random.Range(0f, 100f);
                    if (roll < playerStats.killGoldChance)
                    {
                        playerStats.AddGold(playerStats.killGoldAmount);
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}