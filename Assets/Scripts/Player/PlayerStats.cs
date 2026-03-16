using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [Header("기존 전투 및 이동 스탯")]
    public float moveSpeed = 5f;
    public float bonusDamage = 0f;
    public float attackMultiplier = 1f;
    public float attackSpeed = 1f;

    [Header("캐릭터 기본 스탯")]
    public int baseAttack = 10;
    public int baseDefense = 5;
    public int maxHealth = 100;

    [Header("재화 정보")]
    public int currentGold = 0;

    [Header("스탯 UI 창 전체 패널")]
    public GameObject statPanel;

    [Header("장비 보너스 스탯")]
    [HideInInspector] public int bonusAttack = 0;
    [HideInInspector] public int bonusDefense = 0;
    [HideInInspector] public int bonusHealth = 0;

    [Header("오파츠 퍼센트 보너스")]
    [HideInInspector] public float bonusAttackPercent = 0f;
    [HideInInspector] public float bonusAttackSpeed = 0f;

    [Header("오파츠 특수 스탯")]
    [HideInInspector] public float bonusAttackRange = 0f;       // 사거리 보너스
    [HideInInspector] public float critChance = 0f;             // 치명타 확률 (%)
    [HideInInspector] public float critMultiplier = 2f;         // 치명타 배율 (기본 2배)
    [HideInInspector] public float expMultiplier = 1f;          // 경험치 획득 배율
    [HideInInspector] public float invincibilityBonus = 0f;     // 피격 무적시간 보너스
    [HideInInspector] public float damageNullifyChance = 0f;    // 데미지 무효 확률 (%)
    [HideInInspector] public float killMoveSpeedStack = 0f;     // 적 처치 시 이동속도 중첩
    [HideInInspector] public float killGoldChance = 0f;         // 적 처치 시 골드 획득 확률 (%)
    [HideInInspector] public int killGoldAmount = 0;            // 적 처치 시 골드 획득량
    [HideInInspector] public bool berserkerMode = false;        // 광전사 모드 활성화 여부
    [HideInInspector] public int attackCounter = 0;             // 공격 횟수 카운터 (4번째 공격용)
    [HideInInspector] public bool everyFourthAttackBonus = false; // 4번째 공격 데미지 2배

    [Header("스탯 UI 텍스트 연결")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI healthText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateStatUI();
        if (TabController.instance != null)
            TabController.instance.UpdateGoldUI(currentGold);
        if (statPanel != null)
            statPanel.SetActive(false);
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"골드 획득! +{amount} (현재: {currentGold})");
        if (TabController.instance != null)
            TabController.instance.UpdateGoldUI(currentGold);
    }

    public int GetTotalAttack()
    {
        float base_ = baseAttack + bonusAttack;
        return Mathf.RoundToInt(base_ * (1f + bonusAttackPercent / 100f));
    }

    public int GetTotalDefense() => baseDefense + bonusDefense;
    public int GetTotalHealth() => maxHealth + bonusHealth;
    public float GetTotalAttackSpeed() => attackSpeed * (1f + bonusAttackSpeed / 100f);
    public float GetTotalMoveSpeed() => moveSpeed * (1f + killMoveSpeedStack);

    public void EquipStat(Item item)
    {
        if (item == null) return;
        bonusAttack += item.bonusAttack;
        bonusDefense += item.bonusDefense;
        bonusHealth += item.bonusHealth;
        UpdateStatUI();
    }

    public void UnequipStat(Item item)
    {
        if (item == null) return;
        bonusAttack -= item.bonusAttack;
        bonusDefense -= item.bonusDefense;
        bonusHealth -= item.bonusHealth;
        UpdateStatUI();
    }

    public void UpdateStatUI()
    {
        if (attackText != null) attackText.text = $"공격력: {GetTotalAttack()}";
        if (defenseText != null) defenseText.text = $"방어력: {GetTotalDefense()}";
        if (healthText != null) healthText.text = $"최대 체력: {GetTotalHealth()}";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (statPanel != null)
                statPanel.SetActive(!statPanel.activeSelf);
        }
    }
}