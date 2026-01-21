using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int currentHealth;
    public bool isInvincible = false;
    public float invincibilityDuration = 1.0f;

    [Header("References")]
    private PlayerStats stats;
    private PlayerHitEffect hitEffect;
    public Image[] hearts;

    public void GetFlameHeart() { }

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        hitEffect = GetComponent<PlayerHitEffect>();

        // 하트 UI 자동 연결
        if (hearts == null || hearts.Length == 0)
        {
            GameObject container = GameObject.Find("HeartContainer");
            if (container != null)
            {
                hearts = new Image[3];
                Transform t = container.transform;
                hearts[0] = t.Find("Heart1").GetComponent<Image>();
                hearts[1] = t.Find("Heart2").GetComponent<Image>();
                hearts[2] = t.Find("Heart3").GetComponent<Image>();
            }
        }

        // 체력 초기화
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
        }
        else
        {
            currentHealth = (hearts != null) ? hearts.Length * 2 : 6;
        }

        UpdateHeartUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        // 제산식 방어력 적용
        int defenseVal = (stats != null) ? stats.defense : 0;

        // 공식: 데미지 * (100 / (100 + 방어력))
        float reduction = 100f / (100f + defenseVal);
        int finalDamage = Mathf.RoundToInt(damage * reduction);

        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"Damage Taken: {finalDamage} (Raw: {damage}, Def: {defenseVal})");

        UpdateHeartUI();

        if (currentHealth > 0)
        {
            if (hitEffect != null) hitEffect.TakeDamage();
            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            Debug.Log("Player Died");
            gameObject.SetActive(false);

            // ★ 내일 회의 후 GameManager가 생기면 아래 주석을 푸세요! ★
            /*
            if (GameManager.instance != null)
            {
                GameManager.instance.OnPlayerDead();
            }
            */
        }
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void UpdateHeartUI()
    {
        if (hearts == null || hearts.Length == 0) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            int heartValue = (i + 1) * 2;

            if (currentHealth >= heartValue)
            {
                hearts[i].fillAmount = 1f;
            }
            else if (currentHealth >= heartValue - 1)
            {
                hearts[i].fillAmount = 0.5f;
            }
            else
            {
                hearts[i].fillAmount = 0f;
            }
        }
    }
}