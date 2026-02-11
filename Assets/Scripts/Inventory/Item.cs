using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [TextArea] // 인스펙터에서 여러 줄 입력 가능하게 함
    public string itemDesc; // 아이템 설명 (추가됨)
    public Sprite icon;
    public Sprite equipSprite;
    public ItemType itemType;

    public enum ItemType { Head, Weapon, Armor, Shoes, Consumable, Ooparts }
}