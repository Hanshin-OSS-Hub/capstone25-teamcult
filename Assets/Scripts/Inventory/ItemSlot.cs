using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image icon;      // 아이템 아이콘을 표시할 UI Image
    private Item item;      // 현재 이 슬롯이 가지고 있는 아이템 데이터

    // 슬롯에 아이템 정보를 채우는 함수
    public void AddItem(Item newItem)
    {
        item = newItem;
        icon.sprite = item.icon;

        // 이 두 줄을 추가하여 색상을 선명하게 만듭니다.
        icon.color = Color.white; // Alpha 값을 포함해 100% 흰색으로 설정
        icon.enabled = true;
    }

    // 슬롯을 클릭했을 때 호출될 함수 (Button 컴포넌트에 연결)
    public void OnSlotClick()
    {
        if (item != null)
        {
            Debug.Log($"{item.itemName} 클릭됨! 타입: {item.itemType}"); // 로그 추가
            // TabController의 장착 함수 호출
            TabController.instance.EquipItem(item);

            // 장착 후 인벤토리 슬롯에서 제거 (필요에 따라 설정)
            Destroy(gameObject);
        }
    }
}