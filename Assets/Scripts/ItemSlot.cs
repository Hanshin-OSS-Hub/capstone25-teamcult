using UnityEngine;
using UnityEngine.EventSystems; // 클릭 감지용

// IPointerClickHandler만 남겨둠
public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public enum ItemType { Weapon, Armor, Helmet, Consumable, Etc }

    [Header("아이템 정보")]
    public ItemType myType;
    public string myName;
    [TextArea] public string myDesc;
    public Sprite myIcon;

    // 클릭했을 때 실행 (장착)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TabController.instance != null)
        {
            // 탭 컨트롤러에게 "나 장착시켜줘" 하고 요청
            TabController.instance.EquipItem(myType, myIcon);

            Debug.Log(myName + " 장착됨!"); // 확인용 로그
        }
    }
}