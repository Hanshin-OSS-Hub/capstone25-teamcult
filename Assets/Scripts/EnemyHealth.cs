using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 3;
    public float currentHealth;
    public Slider hpSlider;
    public TMP_Text hpText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth / maxHealth;
        if (hpText != null) hpText.text = $"{Mathf.Max(0, currentHealth)} / {maxHealth}";
    }

    void Die() { Destroy(gameObject); }
}