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
    public int currentGold {
        get {
            return _currentGold;
        }
        private set {
            if (_currentGold != value) { // 값이 변경될때만
                _currentGold = value;
                if (TabController.instance != null) {
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
    //장비 랜덤 옵션으로 올라가는 스피드 변수
    [HideInInspector] public float itemBonusMoveSpeed = 0f;
    [HideInInspector] public float itemBonusAttackSpeed = 0f;

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

    public void AddGold(int amount) {
        if (amount <= 0) {
            Debug.Log($"AddGold : amount : {amount}, 1 이상의 정수를 매개변수로 넣어야 합니다");
            return; 
        }
        currentGold += amount;
        LogManager.Instance.AddLog($"골드를 {amount} 획득했습니다");

    }
    // 골드를 특정 값으로 강제 설정하는 함수 (관리자 기능 등)
    public void SetGold(int value) {
        // 음수로 설정되지 않도록 방지
        int finalValue = Mathf.Max(0, value);
        currentGold = finalValue;

        Debug.Log($"골드가 {finalValue}(으)로 설정되었습니다.");
    }

    // 골드를 강제로 소모하는 함수 (음수 방지)
    public void ForceConsumeGold(int amount) {
        if (amount <= 0) return;

        // 현재 골드에서 차감하되, 0 밑으로 내려가지 않도록 처리
        currentGold = Mathf.Max(0, currentGold - amount);

        LogManager.Instance.AddLog($"골드가 {amount}만큼 강제 차감되었습니다.");
    }

    // 골드가 충분할 때만 소모(구매)하는 함수
    public bool TryPurchase(int cost) {
        if (cost < 0) {
            Debug.Log("구매 비용은 0 이상이여야 합니다.");
            return false;
        }

        if (currentGold >= cost) {
            currentGold -= cost; // 차감 (UI는 프로퍼티 set에서 자동 갱신)
            LogManager.Instance.AddLog($"{cost} 골드를 사용하여 구매에 성공했습니다.");
            return true;
        }
        else {
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
    public int GetTotalHealth() => maxHealth + bonusHealth;

    public float GetTotalAttackSpeed() => (attackSpeed + itemBonusAttackSpeed) * (1f + bonusAttackSpeed / 100f);

    public float GetTotalMoveSpeed() => (moveSpeed + itemBonusMoveSpeed) * (1f + killMoveSpeedStack);

    // 장비 장착
    public void EquipStat(Item item)
    {
        if (item == null) return;

        // 장비 기본 스탯 적용
        bonusAttack += item.bonusAttack;
        bonusDefense += item.bonusDefense;
        bonusHealth += item.bonusHealth;

        // 장비 랜덤 옵션 적용
        foreach (ItemOption option in item.currentOptions)
        {
            switch (option.optionType)
            {
                case OptionType.Attack: bonusAttack += (int)option.value; break;
                case OptionType.Defense: bonusDefense += (int)option.value; break;
                case OptionType.AttackSpeed: itemBonusAttackSpeed += option.value; break;
                case OptionType.MoveSpeed: itemBonusMoveSpeed += option.value; break;
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

    // 장비 해제 
    public void UnequipStat(Item item)
    {
        if (item == null) return;

        // 장비 기본 스탯 해제
        bonusAttack -= item.bonusAttack;
        bonusDefense -= item.bonusDefense;
        bonusHealth -= item.bonusHealth;

        // 장비 랜덤 옵션 해제
        foreach (ItemOption option in item.currentOptions)
        {
            switch (option.optionType)
            {
                case OptionType.Attack: bonusAttack -= (int)option.value; break;
                case OptionType.Defense: bonusDefense -= (int)option.value; break;
                case OptionType.AttackSpeed: itemBonusAttackSpeed -= option.value; break;
                case OptionType.MoveSpeed: itemBonusMoveSpeed -= option.value; break;
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