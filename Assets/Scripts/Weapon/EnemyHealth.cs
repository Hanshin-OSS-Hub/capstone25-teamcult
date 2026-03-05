using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int currentHealth;
    private EnemyStats stats;

    [Header("보상 설정")]
    public int expReward = 10; // 적 처치 시 주는 경험치

    [Header("UI 연결")]
    public TMP_Text nameText;
    public Slider hpSlider;
    public TMP_Text hpText;

    [Header("이펙트 효과")]
    public GameObject damageTextPrefab; // 데미지 텍스트 프리팹

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        // 체력 및 이름 초기화
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            if (nameText != null) nameText.text = stats.enemyName;

            // 만약 EnemyStats에 경험치 수치가 따로 있다면 그걸로 덮어씌웁니다.
            // expReward = stats.exp; 
        }
        else
        {
            // 스탯 스크립트가 없을 때의 기본값
            currentHealth = 30;
            if (nameText != null) nameText.text = "Unknown";
        }

        // HP 슬라이더 세팅
        if (hpSlider != null)
        {
            hpSlider.maxValue = currentHealth;
            hpSlider.value = currentHealth;
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // 1. 방어력을 적용한 최종 데미지 계산
        int defenseVal = (stats != null) ? stats.defense : 0;
        float reduction = 100f / (100f + defenseVal);
        int finalDamage = Mathf.RoundToInt(damage * reduction);
        if (finalDamage < 1) finalDamage = 1; // 최소 데미지는 1

        // 2. 체력 깎기 및 UI 업데이트
        currentHealth -= finalDamage;
        UpdateUI();

        // ?? 3. 데미지 텍스트 팝업 띄우기 (체력바 위로 위치 수정됨!)
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos;

            // 체력바(hpSlider)가 연결되어 있다면, 체력바 위치를 기준으로 잡습니다!
            if (hpSlider != null)
            {
                // 체력바 위치보다 살짝 위쪽(Y축으로 0.5f)에 생성
                spawnPos = hpSlider.transform.position + new Vector3(0, 0.5f, 0);
            }
            else
            {
                // 만약 체력바가 없는 적이라면 기본 몸통에서 좀 더 높이 띄웁니다.
                spawnPos = transform.position + new Vector3(0, 1.5f, 0);
            }

            // 데미지 텍스트 생성
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            // 생성된 텍스트에 방금 입은 데미지 숫자 전달
            DamageText dmgTextScript = textObj.GetComponent<DamageText>();
            if (dmgTextScript != null)
            {
                dmgTextScript.Setup(finalDamage);
            }
        }

        // 4. 체력이 0 이하가 되면 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        // 슬라이더 바 업데이트
        if (hpSlider != null) hpSlider.value = currentHealth;

        // 텍스트(예: 30 / 100) 업데이트
        if (hpText != null)
        {
            int max = (stats != null) ? stats.maxHealth : (int)hpSlider.maxValue;
            hpText.text = $"{currentHealth} / {max}";
        }
    }

    void Die()
    {
        // 1. 매니저의 킬 카운트 증가 (오파츠 시스템 등에서 사용)
        if (GameManager.instance != null)
        {
            GameManager.instance.killCount++;
        }

        // 2. 플레이어에게 경험치 지급
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerExp expScript = player.GetComponent<PlayerExp>();
            if (expScript != null)
            {
                expScript.GetExp(expReward);
            }
        }

        // 3. 적 오브젝트 파괴
        Destroy(gameObject);
    }
}