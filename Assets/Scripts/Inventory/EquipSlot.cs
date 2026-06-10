using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Item currentItem;
    public Image iconImage;

    public void SetItem(Item item)
    {
        currentItem = item;

        if (iconImage != null)
        {
            if (currentItem != null)
            {
                iconImage.sprite = currentItem.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }
        else
        {
            //Debug.LogError("EquipSlot에 Icon Image가 연결되지 않았습니다! 인스펙터를 확인해주세요.");
            Debug.Log("<color=orange>주의!</color> EquipSlot에 Icon Image가 연결되지 않았습니다! 인스펙터를 확인해주세요.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Item itemToUnequip = currentItem;

            SetItem(null); 
            TabController.instance.UnequipItem(itemToUnequip);
            TooltipController.instance.HideTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            TooltipController.instance.ShowTooltip(currentItem, false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.instance.HideTooltip();
    }
}