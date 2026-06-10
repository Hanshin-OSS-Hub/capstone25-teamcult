using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

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
            TooltipController.instance.HideTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null) 
        {
            TooltipController.instance.ShowTooltip(item, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipController.instance != null)
        {
            TooltipController.instance.HideTooltip();
        }
    }
}