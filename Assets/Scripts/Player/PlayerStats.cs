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
    private int _currentGold = 0;
    public int currentGold
    {
        get
        {
            return _currentGold;
        }
        private set
        {
            if (_currentGold != value)
            {
                _currentGold = value;
                if (TabController.instance != null)
                {
                    TabController.instance.UpdateGoldUI(_currentGold);
                }
            }
        }
    }

    [Header("스탯 UI 창 전체 패널")]
    public GameObject statPanel;

    [Header("장비 보너스 스탯 (기본+랜덤옵션)")]
    [HideInInspector] public int bonusAttack = 0;
    [HideInInspector] public int bonusDefense = 0;
    [HideInInspector] public int bonusHealth = 0;
    [HideInInspector] public float itemBonusMoveSpeed = 0f;
    [HideInInspector] public float itemBonusAttackSpeed = 0f;

    [Header("오파츠 퍼센트 보너스")]
    [HideInInspector] public float bonusAttackPercent = 0f;
    [HideInInspector] public float bonusAttackSpeed = 0f;

    [Header("오파츠 특수 스탯")]
    [HideInInspector] public float bonusAttackRange = 0f;
    [HideInInspector] public float critChance = 0f;
    [HideInInspector] public float critMultiplier = 2f;
    [HideInInspector] public float expMultiplier = 1f;
    [HideInInspector] public float invincibilityBonus = 0f;
    [HideInInspector] public float damageNullifyChance = 0f;
    [HideInInspector] public float missChance = 0f;
    [HideInInspector] public float missChanceReduce = 0f;       // 장비로 얻는 명중률 보너스 (%)
    [HideInInspector] public float killMoveSpeedStack = 0f;
    [HideInInspector] public float killGoldChance = 0f;
    [HideInInspector] public int killGoldAmount = 0;
    [HideInInspector] public bool berserkerMode = false;
    [HideInInspector] public int attackCounter = 0;
    [HideInInspector] public bool everyFourthAttackBonus = false;

    [Header("스탯 UI 텍스트 연결")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI moveSpeedText;

    [Header("애니메이션 설정")]
    public Animator playerAnimator;
    public RuntimeAnimatorController defaultAnimController;

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
        if (amount <= 0)
        {
            Debug.Log($"AddGold : amount : {amount}, 1 이상의 정수를 매개변수로 넣어야 합니다");
            return;
        }
        currentGold += amount;
        LogManager.Instance.AddLog($"골드를 {amount} 획득했습니다");
    }

    public void SetGold(int value)
    {
        int finalValue = Mathf.Max(0, value);
        currentGold = finalValue;
        Debug.Log($"골드가 {finalValue}(으)로 설정되었습니다.");
    }

    public void ForceConsumeGold(int amount)
    {
        if (amount <= 0) return;
        currentGold = Mathf.Max(0, currentGold - amount);
        LogManager.Instance.AddLog($"골드가 {amount}만큼 강제 차감되었습니다.");
    }

    public bool TryPurchase(int cost)
    {
        if (cost < 0)
        {
            Debug.Log("구매 비용은 0 이상이여야 합니다.");
            return false;
        }

        if (currentGold >= cost)
        {
            currentGold -= cost;
            LogManager.Instance.AddLog($"{cost} 골드를 사용하여 구매에 성공했습니다.");
            return true;
        }
        else
        {
            LogManager.Instance.AddLog("골드가 부족하여 구매할 수 없습니다.");
            return false;
        }
    }

    public int GetTotalAttack()
    {
        float base_ = baseAttack + bonusAttack;
        return Mathf.RoundToInt(base_ * (1f + bonusAttackPercent / 100f));
    }

    public int GetTotalDefense() => baseDefense + bonusDefense;

    public float GetTotalAttackSpeed() => (attackSpeed + itemBonusAttackSpeed) * (1f + bonusAttackSpeed / 100f);

    public float GetTotalMoveSpeed() => (moveSpeed + itemBonusMoveSpeed) * (1f + killMoveSpeedStack);

    // 실제 miss 판정에 쓰이는 최종 명중률 감소값 (디버프 - 장비보너스, 최소 0)
    public float GetEffectiveMissChance() => Mathf.Max(0f, missChance - missChanceReduce);

    public void EquipStat(Item item)
    {
        if (item == null) return;

        bonusAttack += item.bonusAttack;
        bonusDefense += item.bonusDefense;
        bonusHealth += item.bonusHealth;

        foreach (ItemOption option in item.currentOptions)
        {
            switch (option.optionType)
            {
                case OptionType.Attack: bonusAttack += (int)option.value; break;
                case OptionType.Defense: bonusDefense += (int)option.value; break;
                case OptionType.AttackSpeed: itemBonusAttackSpeed += option.value; break;
                case OptionType.MoveSpeed: itemBonusMoveSpeed += option.value; break;
                case OptionType.MissChanceReduce: missChanceReduce += option.value; break;
            }
        }
        if (item.itemType == Item.ItemType.Weapon && item.weaponAnim != null)
        {
            if (playerAnimator != null)
            {
                playerAnimator.runtimeAnimatorController = item.weaponAnim;
                Debug.Log("무기 애니메이션 교체 완료!");
            }
        }
        UpdateStatUI();
    }

    public void UnequipStat(Item item)
    {
        if (item == null) return;

        bonusAttack -= item.bonusAttack;
        bonusDefense -= item.bonusDefense;
        bonusHealth -= item.bonusHealth;

        foreach (ItemOption option in item.currentOptions)
        {
            switch (option.optionType)
            {
                case OptionType.Attack: bonusAttack -= (int)option.value; break;
                case OptionType.Defense: bonusDefense -= (int)option.value; break;
                case OptionType.AttackSpeed: itemBonusAttackSpeed -= option.value; break;
                case OptionType.MoveSpeed: itemBonusMoveSpeed -= option.value; break;
                case OptionType.MissChanceReduce: missChanceReduce -= option.value; break;
            }
        }
        if (item.itemType == Item.ItemType.Weapon)
        {
            if (playerAnimator != null && defaultAnimController != null)
            {
                playerAnimator.runtimeAnimatorController = defaultAnimController;
            }
        }

        UpdateStatUI();
    }

    public void UpdateStatUI()
    {
        if (attackText != null) attackText.text = $"공격력: {GetTotalAttack()}";
        if (attackSpeedText != null) attackSpeedText.text = $"공격 속도: {GetTotalAttackSpeed()}";
        if (defenseText != null) defenseText.text = $"방어력: {GetTotalDefense()}";
        if (moveSpeedText != null) moveSpeedText.text = $"이동 속도: {GetTotalMoveSpeed()}";
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