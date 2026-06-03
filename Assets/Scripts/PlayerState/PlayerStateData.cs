using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateData
{
    public float currentHealth = 24f;
    public float maxHealth = 32f;
    public int gold = 0;

    public List<Item> inventoryItems = new List<Item>();

    public Item equippedHelmet;
    public Item equippedWeapon;
    public Item equippedUpper;
    public Item equippedBottom;

    public void ResetToDefaults()
    {
        currentHealth = 24f;
        maxHealth = 32f;
        gold = 0;
        inventoryItems.Clear();
        equippedHelmet = null;
        equippedWeapon = null;
        equippedUpper = null;
        equippedBottom = null;
    }

    public Item GetEquippedItem(Item.ItemType itemType)
    {
        switch (itemType)
        {
            case Item.ItemType.Helmet:
                return equippedHelmet;
            case Item.ItemType.Weapon:
                return equippedWeapon;
            case Item.ItemType.Upper:
                return equippedUpper;
            case Item.ItemType.Bottom:
                return equippedBottom;
            default:
                return null;
        }
    }

    public void SetEquippedItem(Item.ItemType itemType, Item item)
    {
        switch (itemType)
        {
            case Item.ItemType.Helmet:
                equippedHelmet = item;
                break;
            case Item.ItemType.Weapon:
                equippedWeapon = item;
                break;
            case Item.ItemType.Upper:
                equippedUpper = item;
                break;
            case Item.ItemType.Bottom:
                equippedBottom = item;
                break;
        }
    }
}
