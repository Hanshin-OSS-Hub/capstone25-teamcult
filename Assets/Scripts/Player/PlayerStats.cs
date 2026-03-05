using UnityEngine;
using TMPro; // [추가 1] TextMeshPro를 조종하기 위해 필수!

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [Header("기존 전투 및 이동 스탯")]
    public float moveSpeed = 5f;
    public float bonusDamage = 0f;
    public float attackMultiplier = 1f;

    [Header("캐릭터 기본 스탯")]
    public int baseAttack = 10;
    public int baseDefense = 5;
    public int maxHealth = 100;

    [Header("재화 정보")]
    public int currentGold = 0;

    [Header("스탯 UI 창 전체 패널")]
    public GameObject statPanel;
    [Header("장비 보너스 스탯")]
    [SerializeField] private int bonusAttack = 0;
    [SerializeField] private int bonusDefense = 0;
    [SerializeField] private int bonusHealth = 0;

    // [추가 2] 스탯 숫자를 보여줄 텍스트 UI들
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
        // [추가 3] 게임 시작 시 현재 스탯으로 UI 갱신
        UpdateStatUI();
        if (TabController.instance != null)
        {
            TabController.instance.UpdateGoldUI(currentGold);
        }
        // [추가 2] 게임이 시작될 때 스탯창이 화면을 가리지 않게 일단 꺼둡니다.
        if (statPanel != null)
        {
            statPanel.SetActive(false);
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"골드 획득! +{amount} (현재: {currentGold})");

        // 골드를 얻었으니 화면의 숫자도 바꿔달라고 요청합니다.
        if (TabController.instance != null)
        {
            TabController.instance.UpdateGoldUI(currentGold);
        }
    }



    // --- 최종 스탯 계산 함수 ---
    public int GetTotalAttack() => baseAttack + bonusAttack;
    public int GetTotalDefense() => baseDefense + bonusDefense;
    public int GetTotalHealth() => maxHealth + bonusHealth;

    // --- 장비 장착 시 호출 ---
    public void EquipStat(Item item)
    {
        if (item == null) return;

        bonusAttack += item.bonusAttack;
        bonusDefense += item.bonusDefense;
        bonusHealth += item.bonusHealth;

        // [추가 4] 스탯이 올랐으니 화면의 글씨도 바꿔주기!
        UpdateStatUI();
    }

    // --- 장비 해제 시 호출 ---
    public void UnequipStat(Item item)
    {
        if (item == null) return;

        bonusAttack -= item.bonusAttack;
        bonusDefense -= item.bonusDefense;
        bonusHealth -= item.bonusHealth;

        // [추가 5] 스탯이 떨어졌으니 화면의 글씨도 바꿔주기!
        UpdateStatUI();
    }

    // [추가 6] 화면의 텍스트를 실제 최종 스탯으로 바꿔주는 전용 함수
    public void UpdateStatUI()
    {
        // 인스펙터에 잘 연결되어 있을 때만 텍스트 변경 (에러 방지용)
        if (attackText != null) attackText.text = $"공격력: {GetTotalAttack()}";
        if (defenseText != null) defenseText.text = $"방어력: {GetTotalDefense()}";
        if (healthText != null) healthText.text = $"최대 체력: {GetTotalHealth()}";
    }
 
    private void Update()
    {
        // 'C' 키를 누를 때마다 실행 (원하는 키가 있다면 KeyCode.C 를 다른 걸로 바꾸세요!)
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (statPanel != null)
            {
                // .activeSelf는 현재 켜져있는지(true/false) 확인하는 속성입니다.
                // 앞에 !를 붙이면 "켜져있으면 끄고, 꺼져있으면 켜라" 라는 뜻이 됩니다!
                statPanel.SetActive(!statPanel.activeSelf);
            }
        }
    }
}