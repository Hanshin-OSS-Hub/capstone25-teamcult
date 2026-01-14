using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ 이게 있어야 글자를 바꿀 수 있습니다!

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 3;
    public float currentHealth;

    public Slider hpSlider;
    public TMP_Text hpText; // ★ 숫자를 표시할 텍스트 빈칸

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0; // 마이너스 안 되게 0으로 고정
            Die();
        }

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        // 1. 체력바 줄이기
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth / maxHealth;
        }

        // 2. 텍스트 숫자 바꾸기 (예: "2 / 3")
        if (hpText != null)
        {
            hpText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}