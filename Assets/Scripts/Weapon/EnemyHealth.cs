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

    [Header("이펙트 효과")]
    public GameObject damageTextPrefab;

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
        float reduction = 100f / (100f + defenseVal);
        int finalDamage = Mathf.RoundToInt(damage * reduction);
        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;
        UpdateUI();

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos;
            if (hpSlider != null)
                spawnPos = hpSlider.transform.position + new Vector3(0, 0.5f, 0);
            else
                spawnPos = transform.position + new Vector3(0, 1.5f, 0);

            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            DamageText dmgTextScript = textObj.GetComponent<DamageText>();
            if (dmgTextScript != null)
                dmgTextScript.Setup(finalDamage);
        }

        if (currentHealth <= 0)
            Die();
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
        // 1. 킬 카운트
        if (GameManager.instance != null)
            GameManager.instance.killCount++;

        // 2. 플레이어 찾기
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            PlayerExp expScript = player.GetComponent<PlayerExp>();

            // 3. 경험치 획득
            if (expScript != null)
            {
                float multiplier = (playerStats != null) ? playerStats.expMultiplier : 1f;
                int finalExp = Mathf.RoundToInt(expReward * multiplier);
                expScript.GetExp(finalExp);
            }

            if (playerStats != null)
            {
                // 4. 킬 처치 후 이동속도 버프 스택
                if (playerStats.killMoveSpeedStack > 0)
                {
                    playerStats.moveSpeed += playerStats.killMoveSpeedStack;
                    Debug.Log($"[이동속도 스택] +{playerStats.killMoveSpeedStack} (현재: {playerStats.moveSpeed})");
                }
                // 5. 킬 처치 후 골드 확률 획득
                if (playerStats.killGoldChance > 0)
                {
                    float roll = Random.Range(0f, 100f);
                    if (roll < playerStats.killGoldChance)
                    {
                        playerStats.AddGold(playerStats.killGoldAmount);
                        Debug.Log($"[골드 획득] +{playerStats.killGoldAmount}");
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}