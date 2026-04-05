using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public Item item;
    private bool isPlayerInRange = false;
    private bool hasBeenPickedUp = false; 

    void Update()
    {
        if (!hasBeenPickedUp && isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        hasBeenPickedUp = true; 

        // 원본 아이템을 제네레이터에 넣어 랜덤 옵션이 붙은 복제본을 생성
        Item newItemWithOption = OptionGenerator.GenerateDroppedItem(item);

        // 생성된 복제본을 인벤토리에 넣습니다.
        if (TabController.instance.AddItem(newItemWithOption))
        {
            Debug.Log($"{newItemWithOption.itemName} 획득 완료! (부여된 옵션 개수: {newItemWithOption.currentOptions.Count}개)");
            Destroy(gameObject);
        }
        else
        {
            hasBeenPickedUp = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = false;
    }
}