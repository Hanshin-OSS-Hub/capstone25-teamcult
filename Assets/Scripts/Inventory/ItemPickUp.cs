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
        if (PlayerStats.instance.TryPurchase(price)) {
            //Debug.Log($"{price}G를 지불했습니다. 남은 골드: {PlayerStats.instance.currentGold}G");
            Pickup();
        }
        //else {
        //    Debug.Log("골드가 부족하여 구매할 수 없습니다.");
        //}
    }

    void Pickup() {
        // 옵션 생성 및 인벤토리 추가 로직
        Item newItemWithOption = OptionGenerator.GenerateDroppedItem(item);

        if (TabController.instance.AddItem(newItemWithOption)) {
            Debug.Log($"{newItemWithOption.itemName} 획득 완료!");
            LogManager.Instance.AddLog($"{newItemWithOption.itemName}을(를) 획득했습니다.");
            Destroy(gameObject);
        }
        else {
            // 인벤토리 가득 참 등 획득 실패 시 처리
            LogManager.Instance.AddLog("인벤토리가 가득 차서 아이템을 획득할 수 없습니다.");

            if (isShopItem) {
                // 직접 += price 대신, 만들어둔 AddGold 함수를 사용하여 UI 갱신 및 로그 출력
                PlayerStats.instance.AddGold(price);
                LogManager.Instance.AddLog($"구매 실패로 인해 {price} 골드가 환불되었습니다.");
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