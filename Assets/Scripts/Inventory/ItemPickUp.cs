using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public Item item;
    private bool isPlayerInRange = false;
    private bool hasBeenPickedUp = false; // 중복 습득 방지용 변수 추가

    void Update()
    {
        // 이미 주운 상태라면 실행하지 않음
        if (!hasBeenPickedUp && isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        hasBeenPickedUp = true; // 줍기 시작하면 즉시 true로 변경하여 중복 방지

        if (TabController.instance.AddItem(item))
        {
            Debug.Log($"{item.itemName} 획득 완료!");
            Destroy(gameObject);
        }
        else
        {
            // 인벤토리가 꽉 차서 실패한 경우에만 다시 false로
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