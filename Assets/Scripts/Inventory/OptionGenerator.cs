using UnityEngine;
using System.Collections.Generic;
public class OptionGenerator : MonoBehaviour
{
    public static Item GenerateDroppedItem(Item originalItem)
    {
        if (originalItem == null) return null;
        Item clonedItem = originalItem.Clone();
        int optionCount = DetermineOptionCount();
        for (int i = 0; i < optionCount; i++)
        {
            clonedItem.currentOptions.Add(GetRandomOptionByType(clonedItem.itemType));
        }
        return clonedItem;
    }

    private static int DetermineOptionCount()
    {
        int rand = Random.Range(1, 101);
        if (rand <= 75) return 1;
        if (rand <= 95) return 2;
        return 3;
    }

    private static ItemOption GetRandomOptionByType(Item.ItemType type)
    {
        List<OptionType> availableOptions = new List<OptionType>();
        if (type == Item.ItemType.Weapon)
        {
            availableOptions.Add(OptionType.Attack);
            availableOptions.Add(OptionType.AttackSpeed);
            availableOptions.Add(OptionType.MissChanceReduce);
        }
        else if (type == Item.ItemType.Helmet || type == Item.ItemType.Upper || type == Item.ItemType.Bottom)
        {
            availableOptions.Add(OptionType.Defense);
            availableOptions.Add(OptionType.MoveSpeed);
        }
        else
        {
            availableOptions.Add(OptionType.Defense);
        }
        OptionType selectedType = availableOptions[Random.Range(0, availableOptions.Count)];
        return CreateOption(selectedType);
    }

    private static ItemOption CreateOption(OptionType type)
    {
        float value = 0f;
        string desc = "";
        switch (type)
        {
            case OptionType.Attack:
                value = Random.Range(3, 11);
                desc = $"공격력 +{value}";
                break;
            case OptionType.AttackSpeed:
                value = Random.Range(0.05f, 0.20f);
                desc = $"공격속도 +{(value * 100).ToString("F0")}%";
                break;
            case OptionType.Defense:
                value = Random.Range(2, 8);
                desc = $"방어력 +{value}";
                break;
            case OptionType.MoveSpeed:
                value = Random.Range(0.2f, 1.0f);
                desc = $"이동속도 +{value.ToString("F1")}";
                break;
            case OptionType.MissChanceReduce:
                value = Random.Range(5f, 16f);
                desc = $"명중률 +{value.ToString("F0")}%";
                break;
        }
        return new ItemOption(type, value, desc);
    }
}