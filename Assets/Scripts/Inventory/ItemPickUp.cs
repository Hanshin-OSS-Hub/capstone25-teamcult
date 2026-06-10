using UnityEngine;
using TMPro;

public class ItemPickUp : MonoBehaviour {
    public Item item;

    [Header("상점 설정")]
    public bool isShopItem = false;
    public int price = 30;
    public TextMeshPro priceText;

    private bool isPlayerInRange = false;
    private bool hasBeenPickedUp = false;

    void Start() {
        RefreshUI();
    }

    public void InitializeShopItem(int newPrice, TextMeshPro existingText = null) {
        isShopItem = true;
        price = newPrice;

        if (existingText != null) {
            priceText = existingText;
        }

        RefreshUI();
    }

    public void RefreshUI() {
        if (priceText != null) {
            priceText.text = isShopItem ? $"{price}G" : "";
            priceText.gameObject.SetActive(isShopItem);
        }
    }

    void Update() {
        if (!hasBeenPickedUp && isPlayerInRange && Input.GetKeyDown(KeyCode.Z)) {
            if (isShopItem) {
                PurchaseItem();
            }
            else {
                Pickup();
            }
        }
    }

    void PurchaseItem() {
        if (PlayerStats.instance.TryPurchase(price)) {
            Pickup();
        }
    }

    void Pickup() {
        Item newItemWithOption = OptionGenerator.GenerateDroppedItem(item);

        if (TabController.instance.AddItem(newItemWithOption)) {
            Debug.Log($"{newItemWithOption.itemName} 획득 완료!");
            LogManager.Instance.AddLog($"{newItemWithOption.itemName}을(를) 획득했습니다.");

            if (SFXManager.Instance != null) {
                SFXManager.Instance.PlaySFX(SFXType.ItemEquip); 
            }

            Destroy(gameObject); 
        }
        else {
            LogManager.Instance.AddLog("인벤토리가 가득 차서 아이템을 획득할 수 없습니다.");

            if (isShopItem) {
                PlayerStats.instance.AddGold(price);
                LogManager.Instance.AddLog($"가방 부족으로 인해 {price} 골드가 환불되었습니다.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) isPlayerInRange = false;
    }
}