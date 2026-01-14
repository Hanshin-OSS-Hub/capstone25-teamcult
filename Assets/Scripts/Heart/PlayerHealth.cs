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

        // ★ 디버깅 로그: 맞을 때마다 콘솔에 뜹니다.
        Debug.Log($"아야! 체력 남음: {currentHealth} / {maxHealth}");

        UpdateHeartUI();
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            int heartValue = currentHealth - (i * 2);
            int clampValue = Mathf.Clamp(heartValue, 0, 2);

            hearts[i].fillAmount = (float)clampValue / 2;

            // ★ 하트 상태 로그 (Heart3 상태 확인용)
            if (i == 2) Debug.Log($"Heart3 채움 정도: {hearts[i].fillAmount}");
        }
    }
}