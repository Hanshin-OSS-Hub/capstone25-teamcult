using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TabController : MonoBehaviour
{
    public static TabController instance;
    public GameObject slotPrefab;

    [Header("UI 패널 연결")]
    public GameObject mainPanel;
    public GameObject equipPanel;
    public GameObject consumableEquipPanel;

    [Header("재화 UI 연결")]
    public TextMeshProUGUI goldText;

    [Header("스크롤 뷰 (화면 전환용) 연결")]
    public GameObject weaponScrollView;
    public GameObject consumableScrollView;

    [Header("탭별 컨텐츠(슬롯 생성 위치) 연결")]
    public GameObject weaponContent;
    public GameObject consumableContent;

    [Header("장착 슬롯 이미지 (UI)")]
    public Image headSlotImage;
    public Image weaponSlotImage;
    public Image armorSlotImage;
    public Image shoesSlotImage;

    [Header("인벤토리 설정")]
    public int maxSlots = 100;
    public List<Item> inventoryItems = new List<Item>();

    private List<ItemSlot> weaponSlotUI = new List<ItemSlot>();
    private List<ItemSlot> consumableSlotUI = new List<ItemSlot>();

    private Item equippedHead, equippedWeapon, equippedUpper, equippedBottom;

    void Awake() { instance = this; }

    void Start()
    {
        InitInventory();
        if (mainPanel != null) mainPanel.SetActive(false);
        ShowWeaponTab();
    }

    public void UpdateGoldUI(int currentGold)
    {
        if (goldText != null)
            goldText.text = $"{currentGold:N0} G";
    }

    private void InitInventory()
    {
        if (weaponContent != null)
        {
            foreach (Transform child in weaponContent.transform) Destroy(child.gameObject);
            CreateEmptySlots(weaponContent.transform, weaponSlotUI);
        }

        if (consumableContent != null)
        {
            foreach (Transform child in consumableContent.transform) Destroy(child.gameObject);
            CreateEmptySlots(consumableContent.transform, consumableSlotUI);
        }
    }

    private void CreateEmptySlots(Transform parent, List<ItemSlot> list)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject go = Instantiate(slotPrefab, parent);
            ItemSlot slot = go.GetComponent<ItemSlot>();
            slot.ClearSlot();
            list.Add(slot);
        }
    }

    public bool AddItem(Item item)
    {
        List<ItemSlot> targetList = GetTargetUIList(item.itemType);

        foreach (ItemSlot slot in targetList)
        {
            if (slot.GetItem() == null)
            {
                slot.AddItem(item);
                inventoryItems.Add(item);
                return true;
            }
        }
        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }

    private List<ItemSlot> GetTargetUIList(Item.ItemType type)
    {
        switch (type)
        {
            case Item.ItemType.Consumable: return consumableSlotUI;
            case Item.ItemType.Heart: return consumableSlotUI;
            default: return weaponSlotUI;
        }
    }

    public Item GetEquippedItem(Item.ItemType type)
    {
        switch (type)
        {
            case Item.ItemType.Helmet: return equippedHead;
            case Item.ItemType.Weapon: return equippedWeapon;
            case Item.ItemType.Upper: return equippedUpper;
            case Item.ItemType.Bottom: return equippedBottom;
            default: return null;
        }
    }

    public void EquipItem(Item item, ItemSlot fromSlot)
    {
        if (item == null) return;

        if (item.itemType == Item.ItemType.Heart)
        {
            ElementalManager manager = FindFirstObjectByType<ElementalManager>();
            if (manager != null)
            {
                manager.ActivateAbility(item.elementType);
                HeartSlotController.instance.SetHeart(item.elementType);
            }

            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.Heal(4f);

            inventoryItems.Remove(item);
            fromSlot.ClearSlot();
            return;
        }

        if (item.itemType == Item.ItemType.Weapon)
            PlayerSlash.instance.SetWeapon(item);

        switch (item.itemType)
        {
            case Item.ItemType.Helmet: UpdateSlot(ref equippedHead, item, headSlotImage); break;
            case Item.ItemType.Weapon: UpdateSlot(ref equippedWeapon, item, weaponSlotImage); break;
            case Item.ItemType.Upper: UpdateSlot(ref equippedUpper, item, armorSlotImage); break;
            case Item.ItemType.Bottom: UpdateSlot(ref equippedBottom, item, shoesSlotImage); break;
            default: return;
        }

        inventoryItems.Remove(item);
        fromSlot.ClearSlot();
    }

    private void UpdateSlot(ref Item equippedItem, Item newItem, Image slotImage)
    {
        if (equippedItem != null)
        {
            PlayerStats.instance.UnequipStat(equippedItem);
            AddItem(equippedItem);
        }

        equippedItem = newItem;

        if (newItem != null)
            PlayerStats.instance.EquipStat(newItem);

        if (slotImage != null)
        {
            slotImage.sprite = newItem.icon;
            slotImage.enabled = true;
            slotImage.color = Color.white;

            EquipSlot eSlot = slotImage.GetComponent<EquipSlot>();
            if (eSlot != null) eSlot.SetItem(newItem);
        }
    }

    public void UnequipItem(Item item)
    {
        if (item == null) return;

        PlayerStats.instance.UnequipStat(item);

        switch (item.itemType)
        {
            case Item.ItemType.Helmet: equippedHead = null; headSlotImage.enabled = false; break;
            case Item.ItemType.Weapon: equippedWeapon = null; weaponSlotImage.enabled = false; break;
            case Item.ItemType.Upper: equippedUpper = null; armorSlotImage.enabled = false; break;
            case Item.ItemType.Bottom: equippedBottom = null; shoesSlotImage.enabled = false; break;
        }

        AddItem(item);
    }

    public void ToggleWindow()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            bool isOpen = mainPanel.activeSelf;
            GameManager.instance.isUIOpen = isOpen;
            Time.timeScale = isOpen ? 0f : 1f;
        }
    }

    public void ShowWeaponTab() { SetTabActive(true, false); }
    public void ShowConsumableTab() { SetTabActive(false, true); }

    private void SetTabActive(bool isWeapon, bool isConsumable)
    {
        if (equipPanel != null) equipPanel.SetActive(isWeapon);
        if (consumableEquipPanel != null) consumableEquipPanel.SetActive(isConsumable);

        if (weaponScrollView != null) weaponScrollView.SetActive(isWeapon);
        if (consumableScrollView != null) consumableScrollView.SetActive(isConsumable);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) ToggleWindow();

        if (Input.GetKeyDown(KeyCode.Escape) && mainPanel.activeSelf)
            ToggleWindow();
    }
}