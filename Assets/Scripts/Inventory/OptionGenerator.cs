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

    // 아이템 부위에 따라 허용된 옵션 중 하나를 랜덤으로 생성
    private static ItemOption GetRandomOptionByType(Item.ItemType type)
    {
        List<OptionType> availableOptions = new List<OptionType>();

        // 부위별 옵션 풀(Pool) 설정
        if (type == Item.ItemType.Weapon)
        {
            // 무기: 공격력, 공격속도만 가능
            availableOptions.Add(OptionType.Attack);
            availableOptions.Add(OptionType.AttackSpeed);
        }
        else if (type == Item.ItemType.Helmet || type == Item.ItemType.Upper || type == Item.ItemType.Bottom)
        {
            // 방어구(머리, 상의, 신발): 방어력, 이동속도만 가능
            availableOptions.Add(OptionType.Defense);
            availableOptions.Add(OptionType.MoveSpeed);
        }
        else
        {
            // 그 외(소모품 등) 옵션이 필요 없다면 기본 방어력 반환 혹은 예외 처리
            availableOptions.Add(OptionType.Defense);
        }

        // 허용된 옵션 중 하나 랜덤 선택
        OptionType selectedType = availableOptions[Random.Range(0, availableOptions.Count)];

        return CreateOption(selectedType);
    }

    // 실제 옵션 데이터 생성 (수치 및 설명)
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
        }

        return new ItemOption(type, value, desc);
    }
}