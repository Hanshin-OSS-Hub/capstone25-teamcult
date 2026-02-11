using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 이 줄이 반드시 필요합니다!

// 인터페이스 2개 추가 (IPointerEnterHandler, IPointerExitHandler)
public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        icon.enabled = false;
    }

    public Item GetItem() => item;

    public void OnSlotClick()
    {
        if (item != null)
        {
            TabController.instance.EquipItem(item, this);
            // 장착해서 아이템이 사라지면 툴팁도 꺼줌
            TooltipController.instance.HideTooltip();
        }
    }

    // --- 마우스가 슬롯에 들어왔을 때 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null) // 아이템이 있을 때만 툴팁 표시
        {
            TooltipController.instance.ShowTooltip(item);
        }
    }

    // --- 마우스가 슬롯에서 나갔을 때 ---
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.instance.HideTooltip();
    }
}