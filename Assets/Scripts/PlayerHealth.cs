using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 6;
    public int currentHealth;
    public Image[] hearts; // Heart1, Heart2, Heart3 넣는 곳

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHeartUI();

        if (currentHealth <= 0)
        {
            Debug.Log("게임 오버!");
            // 여기에 게임 오버 처리 코드 추가 가능
        }
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            int heartValue = currentHealth - (i * 2);
            int clampValue = Mathf.Clamp(heartValue, 0, 2);

            hearts[i].fillAmount = (float)clampValue / 2;
        }
    }
}