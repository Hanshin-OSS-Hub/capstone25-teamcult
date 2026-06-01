using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Item currentItem;
    public Image iconImage; // 슬롯의 이미지 아이콘

    // 아이템 정보 세팅 및 시각적 업데이트
    public void SetItem(Item item)
    {
        currentItem = item;

        // 아이콘 이미지가 인스펙터에 잘 연결되어 있는지 안전장치 추가
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

    // 장착 해제
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // 마우스 왼쪽(Left) 버튼을 클릭했을 때 작동
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Item itemToUnequip = currentItem;

            SetItem(null); 
            TabController.instance.UnequipItem(itemToUnequip);
            TooltipController.instance.HideTooltip();
        }
    }

    // 툴팁 켜기
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            TooltipController.instance.ShowTooltip(currentItem, false);
        }
    }

    // 툴팁 끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.instance.HideTooltip();
    }
}