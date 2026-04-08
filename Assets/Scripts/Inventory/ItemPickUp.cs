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
        // 프리팹 자체에 이미 설정된 값이 있을 경우를 위해 호출
        RefreshUI();
    }

    public void InitializeShopItem(int newPrice, TextMeshPro existingText = null) {
        isShopItem = true;
        price = newPrice;

        // 외부에서 TMP를 생성해서 넣어줬다면 그걸 사용
        if (existingText != null) {
            priceText = existingText;
        }

        RefreshUI();
    }

    // UI 상태를 실제 데이터에 맞게 동기화
    public void RefreshUI() {
        if (priceText != null) {
            priceText.text = isShopItem ? $"{price}G" : "";
            priceText.gameObject.SetActive(isShopItem);
        }
    }

    void Update() {
        // 획득 전 + 범위 안 + Z 키 입력
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
        // PlayerStats의 instance를 통해 currentGold에 접근
        if (PlayerStats.instance != null && PlayerStats.instance.currentGold >= price) {
            PlayerStats.instance.currentGold -= price; // 돈 차감
            Debug.Log($"{price}G를 지불했습니다. 남은 골드: {PlayerStats.instance.currentGold}G");
            Pickup();
        }
        else {
            Debug.Log("골드가 부족하여 구매할 수 없습니다.");
        }
    }

    void Pickup() {
        hasBeenPickedUp = true;

        // 옵션 생성 및 인벤토리 추가 로직
        Item newItemWithOption = OptionGenerator.GenerateDroppedItem(item);

        if (TabController.instance.AddItem(newItemWithOption)) {
            Debug.Log($"{newItemWithOption.itemName} 획득 완료!");
            Destroy(gameObject);
        }
        else {
            // 인벤토리 풀 등 획득 실패 시 (상점 아이템이었다면 돈을 다시 돌려주는 로직을 넣을 수도 있습니다)
            if (isShopItem) PlayerStats.instance.currentGold += price;
            hasBeenPickedUp = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) isPlayerInRange = false;
    }
}