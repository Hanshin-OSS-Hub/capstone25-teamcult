using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    private Item currentItem;

    public void SetItem(Item newItem)
    {
        currentItem = newItem;
    }

    // 장착창의 버튼 OnClick에 연결
    public void OnEquipSlotClick()
    {
        if (currentItem != null)
        {
            TabController.instance.UnequipItem(currentItem);
            currentItem = null; // 슬롯 비우기
            GetComponent<Image>().enabled = false; // 이미지 끄기
        }
    }
}