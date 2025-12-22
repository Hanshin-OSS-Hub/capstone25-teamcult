using UnityEngine;

// 이 줄이 있어야 우클릭 메뉴에 나타납니다!
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject // MonoBehaviour를 ScriptableObject로 변경
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    public enum ItemType { Head, Weapon, Armor, Shoes, Consumable, Ooparts }
}