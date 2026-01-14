using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    private Item item;

    public void AddItem(Item newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.color = Color.white;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false; // 아이콘만 숨겨서 '빈 칸' 유지
    }

    public Item GetItem() => item;

    public void OnSlotClick()
    {
        if (item != null)
        {
            // 장착 시 '나 자신(this)'을 넘겨서 해당 칸을 비우게 함
            TabController.instance.EquipItem(item, this);
        }
    }
}