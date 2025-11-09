using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public UIManager uiManager; // UIManager 참조 (Inspector에서 연결 필요)

    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        // UIManager를 찾는 다른 방법 (태그나 FindObjectOfType 등)
        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }

        currentHealth = maxHealth;
        // 시작할 때 UI 업데이트
        uiManager.UpdateHealthUI((float)currentHealth / maxHealth);
    }

    // 데미지를 입는 함수 (다른 스크립트에서 호출)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // 체력이 변경될 때마다 UI 업데이트 요청
        float healthPercent = (float)currentHealth / maxHealth;
        uiManager.UpdateHealthUI(healthPercent);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // 사망 처리 로직
    }
}