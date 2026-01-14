using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabController : MonoBehaviour
{
    public static TabController instance;
    public GameObject slotPrefab;

    [Header("UI 패널 연결")]
    public GameObject mainPanel;
    public GameObject equipPanel;

    [Header("탭별 컨텐츠(Scroll View의 Content) 연결")]
    public GameObject weaponContent;
   // public GameObject consumableContent;
   // public GameObject oopartsContent;

    [Header("장착 슬롯 이미지 (UI)")]
    public Image headSlotImage;
    public Image weaponSlotImage;
    public Image armorSlotImage;
    public Image shoesSlotImage;

    [Header("인벤토리 설정")]
    public int maxSlots = 100;
    public List<Item> inventoryItems = new List<Item>(); // 데이터 관리용

    // UI 슬롯 객체들을 미리 보관할 리스트
    private List<ItemSlot> weaponSlotUI = new List<ItemSlot>();
    private List<ItemSlot> consumableSlotUI = new List<ItemSlot>();
    private List<ItemSlot> oopartsSlotUI = new List<ItemSlot>();

    private Item equippedHead, equippedWeapon, equippedArmor, equippedShoes;

    void Awake() { instance = this; }

    void Start()
    {
        // 1. 시작 시 기존 자식들 제거 및 100개 슬롯 미리 생성
        InitInventory();

        if (mainPanel != null) mainPanel.SetActive(false);
        ShowWeaponTab();
    }

    private void InitInventory()
    {
        // 기존 슬롯 청소
        foreach (Transform child in weaponContent.transform) Destroy(child.gameObject);
        //foreach (Transform child in consumableContent.transform) Destroy(child.gameObject);
        //foreach (Transform child in oopartsContent.transform) Destroy(child.gameObject);

        // 100개씩 미리 생성
        CreateEmptySlots(weaponContent.transform, weaponSlotUI);
        //CreateEmptySlots(consumableContent.transform, consumableSlotUI);
        //CreateEmptySlots(oopartsContent.transform, oopartsSlotUI);
    }

    private void CreateEmptySlots(Transform parent, List<ItemSlot> list)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject go = Instantiate(slotPrefab, parent);
            ItemSlot slot = go.GetComponent<ItemSlot>();
            slot.ClearSlot(); // 빈 상태로 초기화
            list.Add(slot);
        }
    }

    // --- 아이템 획득 로직 (빈 슬롯 찾아 채우기) ---
    public bool AddItem(Item item)
    {
        List<ItemSlot> targetList = GetTargetUIList(item.itemType);

        foreach (ItemSlot slot in targetList)
        {
            if (slot.GetItem() == null) // 비어있는 UI 슬롯 발견 시
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

    // --- 장착 로직 (인벤토리 클릭 시 호출) ---
    public void EquipItem(Item item, ItemSlot fromSlot)
    {
        if (item == null) return;

        switch (item.itemType)
        {
            case Item.ItemType.Head: UpdateSlot(ref equippedHead, item, headSlotImage); break;
            case Item.ItemType.Weapon: UpdateSlot(ref equippedWeapon, item, weaponSlotImage); break;
            case Item.ItemType.Armor: UpdateSlot(ref equippedArmor, item, armorSlotImage); break;
            case Item.ItemType.Shoes: UpdateSlot(ref equippedShoes, item, shoesSlotImage); break;
            default: return; // 소비템 등은 장착 불가
        }

        inventoryItems.Remove(item);
        fromSlot.ClearSlot(); // 인벤토리 슬롯만 비움 (파괴X)
    }

    private void UpdateSlot(ref Item equippedItem, Item newItem, Image slotImage)
    {
        // 이미 장착 중이면 인벤토리로 되돌림
        if (equippedItem != null) AddItem(equippedItem);

        equippedItem = newItem;

        if (slotImage != null)
        {
            slotImage.sprite = newItem.icon;
            slotImage.enabled = true;
            slotImage.color = Color.white;

            // 장착 슬롯 스크립트가 있다면 데이터 동기화
            EquipSlot eSlot = slotImage.GetComponent<EquipSlot>();
            if (eSlot != null) eSlot.SetItem(newItem);
        }
    }

    // --- 해제 로직 (장착창 클릭 시 호출) ---
    public void UnequipItem(Item item)
    {
        if (item == null) return;

        switch (item.itemType)
        {
            case Item.ItemType.Head: equippedHead = null; headSlotImage.enabled = false; break;
            case Item.ItemType.Weapon: equippedWeapon = null; weaponSlotImage.enabled = false; break;
            case Item.ItemType.Armor: equippedArmor = null; armorSlotImage.enabled = false; break;
            case Item.ItemType.Shoes: equippedShoes = null; shoesSlotImage.enabled = false; break;
        }

        AddItem(item); // 인벤토리의 빈 칸으로 돌아감
    }

    // --- 탭 전환 및 윈도우 토글 ---
    public void ToggleWindow() { if (mainPanel != null) mainPanel.SetActive(!mainPanel.activeSelf); }
    public void ShowWeaponTab() { SetTabActive(true, false, false); }
    public void ShowConsumableTab() { SetTabActive(false, true, false); }
    public void ShowOopartsTab() { SetTabActive(false, false, true); }

    private void SetTabActive(bool weapon, bool consumable, bool ooparts)
    {
        if (equipPanel != null) equipPanel.SetActive(weapon);
        // ScrollView가 있다면 해당 ScrollView를 끄고 켜도록 구조 설계 권장
        weaponContent.SetActive(weapon);
        //consumableContent.SetActive(consumable);
        //oopartsContent.SetActive(ooparts);
    }

    void Update() { if (Input.GetKeyDown(KeyCode.E)) ToggleWindow(); }
}