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

    [Header("재화 UI 연결")]
    public TextMeshProUGUI goldText;

    [Header("탭별 컨텐츠(Scroll View의 Content) 연결")]
    public GameObject weaponContent;

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
    private List<ItemSlot> oopartsSlotUI = new List<ItemSlot>();

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
        foreach (Transform child in weaponContent.transform) Destroy(child.gameObject);
        CreateEmptySlots(weaponContent.transform, weaponSlotUI);
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
            case Item.ItemType.Ooparts: return oopartsSlotUI;
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
            GameManager.instance.isUIOpen = mainPanel.activeSelf;

            if (!mainPanel.activeSelf)
                TooltipController.instance.HideTooltip();
        }
    }

    public void ShowWeaponTab() { SetTabActive(true, false, false); }
    public void ShowConsumableTab() { SetTabActive(false, true, false); }
    public void ShowOopartsTab() { SetTabActive(false, false, true); }

    private void SetTabActive(bool weapon, bool consumable, bool ooparts)
    {
        if (equipPanel != null) equipPanel.SetActive(weapon);
        weaponContent.SetActive(weapon);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) ToggleWindow();

       
    }
}