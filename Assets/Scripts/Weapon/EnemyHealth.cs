using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트 매시 프로 필수

public class EnemyHealth : MonoBehaviour
{
    [Header("상태")]
    public int currentHealth;
    private EnemyStats stats;

    [Header("UI 연결")]
    public TMP_Text nameText;    // ★ [추가됨] 이름 표시할 텍스트
    public Slider hpSlider;      // HP 바
    public TMP_Text hpText;      // HP 숫자 (50/100)

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        // 1. 체력 초기화
        if (stats != null)
        {
            currentHealth = stats.maxHealth;

            // ★ [추가됨] 이름 설정 (스탯에 있는 이름 가져오기)
            if (nameText != null)
            {
                nameText.text = stats.enemyName;
            }
        }
        else
        {
            currentHealth = 30; // 기본값
            if (nameText != null) nameText.text = "Unknown";
        }

        // 2. 슬라이더 최대치 설정
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

        if (currentHealth <= 0)
        {
            Die();
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
        Destroy(gameObject);
    }
}