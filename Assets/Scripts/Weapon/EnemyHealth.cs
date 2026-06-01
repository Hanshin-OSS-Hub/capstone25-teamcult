using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int currentHealth;
    protected EnemyStats stats;

    [Header("보상 설정")]
    public int expReward = 10;

    [Header("UI 설정")]
    public TMP_Text nameText;
    public Slider hpSlider;
    public TMP_Text hpText;

    [Header("마석")]
    public GameObject maSeokPrefab;
    [Range(0f, 100f)]
    public float maSeokDropChance = 50f;

    public event System.Action OnDeath;

    protected bool isDead = false;
    private bool isInvincible = false;

    protected virtual void Start()
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

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public bool IsInvincible() => isInvincible;

    protected virtual void PlayHitSound()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyHit);
    }

    protected virtual void PlayDeathSound()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyDeath);
    }

    Vector3 GetTextSpawnPos()
    {
        return hpSlider != null
            ? hpSlider.transform.position + new Vector3(0, 0.5f, 0)
            : transform.position + new Vector3(0, 1.5f, 0);
    }

    public virtual void TakeDamage(int damage, bool isCrit = false, bool isBurn = false)
    {
        if (isInvincible) return;
        if (isDead) return;

        int defenseVal = (stats != null) ? stats.defense : 0;
        int finalDamage = Mathf.Max(damage - defenseVal, 1);

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        // === 디버그 ===
        if (DamageTextSpawner.Instance != null)
        {
            Vector3 pos = GetTextSpawnPos();
            DamageTextSpawner.Instance.Spawn(finalDamage, pos, isCrit, isBurn);
            Debug.Log($"[데미지텍스트] Spawn 호출됨 - 데미지:{finalDamage}, 위치:{pos}");
        }
        else
        {
            Debug.LogWarning("[데미지텍스트] DamageTextSpawner.Instance가 null!");
        }
        // =============

        if (currentHealth <= 0)
        {
            PlayDeathSound();
            Die();
        }
        else
        {
            PlayHitSound();
        }
    }

    public virtual void TakeDamageIgnoreDefense(int damage, bool isCrit = false, bool isBurn = false)
    {
        if (isInvincible) return;
        if (isDead) return;

        if (damage < 1) damage = 1;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        if (DamageTextSpawner.Instance != null)
        {
            Vector3 pos = GetTextSpawnPos();
            DamageTextSpawner.Instance.Spawn(damage, pos, isCrit, isBurn);
            Debug.Log($"[데미지텍스트] (관통) Spawn 호출됨 - 데미지:{damage}, 위치:{pos}");
        }
        else
        {
            Debug.LogWarning("[데미지텍스트] DamageTextSpawner.Instance가 null!");
        }

        if (currentHealth <= 0)
        {
            PlayDeathSound();
            Die();
        }
        else
        {
            PlayHitSound();
        }
    }

    public void ShowMiss()
    {
        if (isDead) return;
        if (DamageTextSpawner.Instance != null)
            DamageTextSpawner.Instance.SpawnMiss(GetTextSpawnPos());
    }

    public void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
        if (hpText != null)
        {
            int max = (stats != null) ? stats.maxHealth : (int)hpSlider.maxValue;
            hpText.text = $"{currentHealth} / {max}";
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();

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
                    playerStats.moveSpeed += playerStats.killMoveSpeedStack;

                if (playerStats.killGoldChance > 0)
                {
                    float roll = Random.Range(0f, 100f);
                    if (roll < playerStats.killGoldChance)
                        playerStats.AddGold(playerStats.killGoldAmount);
                }
            }
        }

        Destroy(gameObject);
    }
}