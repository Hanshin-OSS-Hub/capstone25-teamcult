using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class TabController : MonoBehaviour
{
    public static TabController instance;
    public GameObject slotPrefab;

    [Header("UI 패널 연결")]
    public GameObject mainPanel;       // 전체 인벤토리 창
    public GameObject equipPanel;      // 장비창 부분 (왼쪽)

    [Header("탭별 컨텐츠 연결")]
    public GameObject weaponContent;     // 무기 탭 내용
    public GameObject consumableContent; // 소비 탭 내용
    public GameObject oopartsContent;    // 오파츠 탭 내용

    [Header("장착 슬롯 이미지 (UI)")]
    public Image headSlotImage;
    public Image weaponSlotImage;
    public Image armorSlotImage;
    public Image shoesSlotImage;

    [Header("캐릭터 프리뷰 (가운데)")]
    public Image characterPreview; 

    [Header("인벤토리 데이터")]
    public List<Item> inventoryItems = new List<Item>();
    public int maxSlots = 100;

    // 현재 장착된 아이템 보관용
    private Item equippedHead, equippedWeapon, equippedArmor, equippedShoes;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 시작하자마자 각 컨텐츠 영역을 깨끗이 비웁니다.
        foreach (Transform child in weaponContent.transform) Destroy(child.gameObject);
        foreach (Transform child in consumableContent.transform) Destroy(child.gameObject);
        foreach (Transform child in oopartsContent.transform) Destroy(child.gameObject);

        if (mainPanel != null) mainPanel.SetActive(false);
        ShowWeaponTab(); // 기본 탭 설정
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleWindow();
        }
    }

  

    public void ToggleWindow()
    {
        if (mainPanel != null)
            mainPanel.SetActive(!mainPanel.activeSelf);
    }

    // --- 아이템 획득 로직 ---
    public bool AddItem(Item item)
    {
        if (inventoryItems.Count >= maxSlots)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            return false;
        }

        inventoryItems.Add(item);

        // --- 추가된 코드: UI 슬롯 생성 ---
        Transform targetContent = null;

        // 아이템 타입에 맞는 탭(Content) 결정
        switch (item.itemType)
        {
            case Item.ItemType.Weapon: targetContent = weaponContent.transform; break;
            case Item.ItemType.Consumable: targetContent = consumableContent.transform; break;
            case Item.ItemType.Ooparts: targetContent = oopartsContent.transform; break;
            default: targetContent = weaponContent.transform; break;
        }

        if (targetContent != null)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, targetContent);

            // [추가] 생성된 슬롯을 부모의 가장 첫 번째(0번) 순서로 강제 이동
            newSlotObj.transform.SetAsFirstSibling();

            ItemSlot slot = newSlotObj.GetComponent<ItemSlot>();
            slot.AddItem(item);
        }
        Debug.Log(item.itemName + " 아이템 추가됨!");
        return true;
    }

    // --- 장착 로직 (통합 및 확장) ---
    public void EquipItem(Item item)
    {
        if (item == null) return;

        switch (item.itemType)
        {
            case Item.ItemType.Head:   UpdateSlot(ref equippedHead, item, headSlotImage); break;
            case Item.ItemType.Weapon: UpdateSlot(ref equippedWeapon, item, weaponSlotImage); break;
            case Item.ItemType.Armor:  UpdateSlot(ref equippedArmor, item, armorSlotImage); break;
            case Item.ItemType.Shoes:  UpdateSlot(ref equippedShoes, item, shoesSlotImage); break;
        }
        
        inventoryItems.Remove(item); // 장착했으니 인벤토리 리스트에서 제거
        Debug.Log($"{item.itemName} 장착 완료");
    }

    private void UpdateSlot(ref Item equippedItem, Item newItem, Image slotImage)
    {
        // 이미 장착된 게 있다면 해제 로직 실행 (인벤토리로 되돌림)
        if (equippedItem != null)
        {
            AddItem(equippedItem);
        }

        equippedItem = newItem;

        if (slotImage != null)
        {
            slotImage.sprite = newItem.icon;
            slotImage.color = Color.white;
            slotImage.enabled = true;

            // 만약 장착 슬롯에 EquipSlot 스크립트가 있다면 데이터 동기화
            if (slotImage.GetComponent<EquipSlot>() != null)
            {
                slotImage.GetComponent<EquipSlot>().SetItem(newItem);
            }
        }
    }

    // --- 탭 전환 함수들 ---
    public void ShowWeaponTab() { SetTabActive(true, false, false); }
    public void ShowConsumableTab() { SetTabActive(false, true, false); }
    public void ShowOopartsTab() { SetTabActive(false, false, true); }

    public void UnequipItem(Item item)
    {
        if (item == null) return;

        // 해당 부위 변수 비우기
        switch (item.itemType)
        {
            case Item.ItemType.Head: equippedHead = null; break;
            case Item.ItemType.Weapon: equippedWeapon = null; break;
            case Item.ItemType.Armor: equippedArmor = null; break;
            case Item.ItemType.Shoes: equippedShoes = null; break;
        }

        // 인벤토리에 다시 추가 (UI 슬롯 생성 포함)
        AddItem(item);
        Debug.Log($"{item.itemName} 해제 완료");
    }

    private void SetTabActive(bool weapon, bool consumable, bool ooparts)
    {
        if (equipPanel != null) equipPanel.SetActive(weapon);
        if (weaponContent) weaponContent.SetActive(weapon);
        if (consumableContent) consumableContent.SetActive(consumable);
        if (oopartsContent) oopartsContent.SetActive(ooparts);
    }
}