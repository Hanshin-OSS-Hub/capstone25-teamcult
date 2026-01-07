using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    public Item.ItemType slotType; // 이 슬롯이 어떤 부위인지 설정 (Head, Weapon 등)
    private Item currentItem;
    private Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    public void SetItem(Item newItem)
    {
        currentItem = newItem;
        if (newItem != null)
        {
            slotImage.sprite = newItem.icon;
            slotImage.color = Color.white;
        }
        else
        {
            slotImage.sprite = null;
            slotImage.color = new Color(1, 1, 1, 0); // 아이템 없으면 투명하게
        }
    }

    // 장착창 슬롯을 클릭했을 때 호출 (Button 컴포넌트에 연결)
    public void OnEquipSlotClick()
    {
        if (currentItem != null)
        {
            TabController.instance.UnequipItem(currentItem);
            SetItem(null); // 슬롯 비우기
        }
    }
}