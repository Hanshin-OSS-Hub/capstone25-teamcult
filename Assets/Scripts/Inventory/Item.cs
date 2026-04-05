using UnityEngine;
using System.Collections.Generic;

// [추가] 장비에 붙을 4가지 옵션 종류
public enum OptionType { Attack, AttackSpeed, Defense, MoveSpeed }

[System.Serializable]
public class ItemOption
{
    public OptionType optionType;
    public float value; // 공격력/방어력은 정수, 속도류는 소수점으로 쓰기 위해 float 사용
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
    public enum ItemTier { Tier1, Tier2, Tier3 } // 티어 시스템 추가

    [Header("기본 정보")]
    public string itemName;
    [TextArea] public string itemDesc;
    public Sprite icon;
    public ItemType itemType;
    public ItemTier tier;

    [Header("기본 장비 스탯")]
    public int bonusAttack;
    public int bonusDefense;
    public int bonusHealth;

    [Header("부여된 랜덤 옵션 (복제 시 자동 생성)")]
    public List<ItemOption> currentOptions = new List<ItemOption>();

    // [핵심] 원본 데이터를 보호하고 고유한 장비를 만들기 위한 복제 함수
    public Item Clone()
    {
        return Instantiate(this);
    }
}