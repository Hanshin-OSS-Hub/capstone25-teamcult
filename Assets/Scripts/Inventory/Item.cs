using UnityEngine;
using System.Collections.Generic;

public enum OptionType { Attack, AttackSpeed, Defense, MoveSpeed }
public enum WeaponType { None, Sword, Axe, Handgun }

[System.Serializable]
public class ItemOption
{
    public OptionType optionType;
    public float value;
    public string description;
    public ItemOption(OptionType type, float val, string desc)
    {
        optionType = type;
        value = val;
        description = desc;
    }
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public enum ItemType { Helmet, Weapon, Upper, Bottom, Consumable, Ooparts }
    public enum ItemTier { Tier1, Tier2, Tier3 }

    [Header("기본 정보")]
    public string itemName;
    [TextArea] public string itemDesc;
    public Sprite icon;

    [Header("분류")]
    public ItemType itemType;
    public WeaponType weaponType;
    public ItemTier tier;

    [Header("기본 장비 스탯")]
    public int bonusAttack;
    public int bonusDefense;
    public int bonusHealth;

    [Header("무기 전용 설정 (무기일 때만 넣으세요)")]
    public GameObject prefab;
    public float damage;
    public float speed;
    public float lifeTime;
    public float cooldown;
    public AnimatorOverrideController weaponAnim;

    [Header("부여된 랜덤 옵션")]
    public List<ItemOption> currentOptions = new List<ItemOption>();

    public Item Clone()
    {
        return Instantiate(this);
    }
}