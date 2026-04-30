using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("ü�� ����")]
    public int currentHealth;
    private EnemyStats stats;

    [Header("���� ����")]
    public int expReward = 10;

    [Header("UI ����")]
    public TMP_Text nameText;
    public Slider hpSlider;
    public TMP_Text hpText;

    [Header("����Ʈ ȿ��")]
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
        {
            // ★ [여기에 추가!] 적 사망 효과음
            if (SFXManager.Instance != null) 
                SFXManager.Instance.PlaySFX(SFXType.EnemyDeath);
            
            Die();
        }
        else
        {
            // ★ [여기에 추가!] 적 피격 효과음
            if (SFXManager.Instance != null) 
                SFXManager.Instance.PlaySFX(SFXType.EnemyHit);
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
        // 1. ų ī��Ʈ
        if (GameManager.instance != null)
            GameManager.instance.killCount++;

        // 2. �÷��̾� ã��
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            PlayerExp expScript = player.GetComponent<PlayerExp>();

            // 3. ����ġ ȹ��
            if (expScript != null)
            {
                float multiplier = (playerStats != null) ? playerStats.expMultiplier : 1f;
                int finalExp = Mathf.RoundToInt(expReward * multiplier);
                expScript.GetExp(finalExp);
            }

            if (playerStats != null)
            {
                // 4. ų óġ �� �̵��ӵ� ���� ����
                if (playerStats.killMoveSpeedStack > 0)
                {
                    playerStats.moveSpeed += playerStats.killMoveSpeedStack;
                    Debug.Log($"[�̵��ӵ� ����] +{playerStats.killMoveSpeedStack} (����: {playerStats.moveSpeed})");
                }
                // 5. ų óġ �� ��� Ȯ�� ȹ��
                if (playerStats.killGoldChance > 0)
                {
                    float roll = Random.Range(0f, 100f);
                    if (roll < playerStats.killGoldChance)
                    {
                        playerStats.AddGold(playerStats.killGoldAmount);
                        Debug.Log($"[��� ȹ��] +{playerStats.killGoldAmount}");
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}