using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float currentHealth;
    public float maxHealth;

    [Header("Heart UI")]
    // 게임 시작 시 자동으로 찾으므로, 인스펙터가 비어있어도 괜찮습니다.
    public Image[] hearts;

    private PlayerStats stats;

    void Start()
    {
        // =========================================================
        // ★ [1] 하트 UI 자동 연결 (전체 맵 수색 모드)
        // =========================================================
        if (hearts == null || hearts.Length == 0)
        {
            // GameObject.Find는 씬 전체를 뒤져서 이름이 일치하는 오브젝트를 찾아냅니다.
            // (주의: Hierarchy에 있는 이름과 띄어쓰기, 대소문자가 정확해야 합니다)
            GameObject container = GameObject.Find("HeartContainer");

            if (container != null)
            {
                // 찾았다면 그 안에 있는 하트 이미지들을 가져옵니다.
                hearts = container.GetComponentsInChildren<Image>();
                Debug.Log($"[성공] 'HeartContainer'를 발견하여 하트 {hearts.Length}개를 연결했습니다.");
            }
            else
            {
                // 못 찾았다면 이름이 틀린 것입니다.
                Debug.LogError("!! [오류] 'HeartContainer'를 찾을 수 없습니다 !!\n -> Hierarchy 창에서 이름이 정확한지(띄어쓰기 포함) 확인해주세요.");
            }
        }

        // =========================================================
        // ★ [2] 체력 수치 동기화
        // =========================================================
        stats = GetComponent<PlayerStats>();

        // PlayerStats에 100이 적혀있든 뭐든 무시하고, 하트 개수에 맞춰 체력을 설정합니다.
        // 하트 1개 = 체력 2 (반 칸씩 깎임)
        if (hearts != null && hearts.Length > 0)
        {
            maxHealth = hearts.Length * 2;
        }
        else
        {
            maxHealth = 6; // 하트를 못 찾았을 때를 대비한 기본값
        }

        currentHealth = maxHealth;

        // [안전장치] 이미지 설정 검사
        foreach (var heart in hearts)
        {
            if (heart != null && heart.type != Image.Type.Filled)
            {
                Debug.LogWarning($"[주의] {heart.name}의 Image Type이 'Filled'가 아닙니다! 인스펙터에서 바꿔주세요.");
            }
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"[피격] 데미지 {damage} 입음!");

        int defenseVal = (stats != null) ? stats.defense : 0;
        float reduction = 100f / (100f + defenseVal);

        float finalDamage = damage * reduction;

        // 최소 데미지 0.5 (반 칸) 보장
        if (finalDamage > 0 && finalDamage < 0.5f) finalDamage = 0.5f;
        else if (finalDamage <= 0) finalDamage = 0f;

        currentHealth -= finalDamage;

        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"[상태] 남은 체력: {currentHealth}");

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    void UpdateUI()
    {
        if (hearts == null || hearts.Length == 0) return;

        // 하트 1개당 체력 2 담당
        float healthPerHeart = 2f;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            float startThreshold = i * healthPerHeart;
            float fillValue = (currentHealth - startThreshold) / healthPerHeart;

            // 0.0 ~ 1.0 사이로 값 조절하여 이미지 채우기
            hearts[i].fillAmount = Mathf.Clamp01(fillValue);
        }
    }

    void Die()
    {
        currentHealth = 0;
        UpdateUI();
        gameObject.SetActive(false);
        if (GameManager.instance != null) GameManager.instance.GameOver();
    }

    public void GetFlameHeart(int amount = 0) { }
}