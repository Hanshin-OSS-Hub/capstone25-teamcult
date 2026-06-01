using UnityEngine;

public class MagicstonePickUp : MonoBehaviour
{
    [Header("마석 아이템 연결")]
    public Item magicstoneItem; // 인스펙터에서 Ooparts 타입 마석 Item 연결

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            if (magicstoneItem == null)
            {
                Debug.LogError("magicstoneItem이 연결되지 않았습니다!");
                return;
            }

            if (TabController.instance.AddItem(magicstoneItem.Clone()))
            {
                LogManager.Instance.AddLog($"{magicstoneItem.itemName}을(를) 획득했습니다.");
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}