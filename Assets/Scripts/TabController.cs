using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public static TabController instance;

    [Header("UI 패널 연결")]
    public GameObject mainPanel;       // 전체 인벤토리 창
    public GameObject equipPanel;      // 장비창 부분 (왼쪽)

    [Header("탭별 컨텐츠 연결")]
    public GameObject weaponContent;     // 무기 탭 내용 (오른쪽)
    public GameObject consumableContent; // 소비 탭 내용
    public GameObject oopartsContent;    // 오파츠 탭 내용

    [Header("장착 슬롯 이미지")]
    public Image weaponSlotImage;      // 장착된 무기 보여줄 이미지

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 시작할 때 인벤토리 꺼두기
        if (mainPanel != null) mainPanel.SetActive(false);

        // 기본적으로 무기 탭 보여주기
        ShowWeaponTab();
    }

    void Update()
    {
        // E키 누르면 인벤토리 껏다 켰다 하기
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleWindow();
        }
    }

    public void ToggleWindow()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
        }
    }

    // 아이템 클릭 시 장착하는 함수
    public void EquipItem(ItemSlot.ItemType type, Sprite iconSprite)
    {
        // 무기 타입이면 무기 슬롯 이미지 변경
        if (type == ItemSlot.ItemType.Weapon && weaponSlotImage != null)
        {
            weaponSlotImage.sprite = iconSprite;
            weaponSlotImage.color = Color.white; // 투명도 등 초기화
        }
    }

    // --- 탭 전환 함수들 ---
    public void ShowWeaponTab()
    {
        SetTabActive(true, false, false);
    }

    public void ShowConsumableTab()
    {
        SetTabActive(false, true, false);
    }

    public void ShowOopartsTab()
    {
        SetTabActive(false, false, true);
    }

    // 탭 켜고 끄는 거 돕는 함수
    private void SetTabActive(bool weapon, bool consumable, bool ooparts)
    {
        //  [수정됨] 무기 탭(weapon이 true)일 때만 장비창(왼쪽)을 켜라!
        // weapon이 false면(오파츠, 소비 등) 장비창도 같이 꺼짐
        if (equipPanel != null)
        {
            equipPanel.SetActive(weapon);
        }

        if (weaponContent) weaponContent.SetActive(weapon);
        if (consumableContent) consumableContent.SetActive(consumable);
        if (oopartsContent) oopartsContent.SetActive(ooparts);
    }
}