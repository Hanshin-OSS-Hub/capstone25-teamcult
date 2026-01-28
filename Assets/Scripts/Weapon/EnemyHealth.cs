using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("상태")]
    public int currentHealth;
    private EnemyStats stats;

    [Header("보상 설정")]
    public int expReward = 10; // ★ [추가] 이 적을 죽이면 주는 경험치

    [Header("UI 연결")]
    public TMP_Text nameText;
    public Slider hpSlider;
    public TMP_Text hpText;

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        // 스탯 기반 초기화
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            if (nameText != null) nameText.text = stats.enemyName;

            // 만약 EnemyStats에 경험치 수치가 있다면 그걸 가져오고, 없으면 기본값(10) 사용
            // (EnemyStats에 exp 변수가 없다면 이 줄은 무시됩니다)
            // expReward = stats.exp; 
        }
        else
        {
            currentHealth = 30;
            if (nameText != null) nameText.text = "Unknown";
        }

        // 슬라이더 설정
        if (hpSlider != null)
        {
            hpSlider.maxValue = currentHealth;
            hpSlider.value = currentHealth;
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // 방어력 적용 데미지 공식
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
        // 1. 매니저의 킬 카운트 증가
        if (GameManager.instance != null)
        {
            GameManager.instance.killCount++;
        }

        // ★ [추가] 2. 플레이어에게 경험치 지급 (자동 이체)
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerExp expScript = player.GetComponent<PlayerExp>();
            if (expScript != null)
            {
                expScript.GetExp(expReward); // 설정한 경험치만큼 전달
            }
        }

        Destroy(gameObject);
    }
}