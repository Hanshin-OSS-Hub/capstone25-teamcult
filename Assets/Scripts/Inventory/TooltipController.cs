using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController instance;

    [Header("메인 툴팁 (마우스 올린 아이템)")]
    public GameObject mainTooltipPanel;
    public TextMeshProUGUI mainNameText;
    public TextMeshProUGUI mainDescText;

    [Header("비교 툴팁 (현재 장착 중인 아이템)")]
    public GameObject compareTooltipPanel;
    public TextMeshProUGUI compareNameText;
    public TextMeshProUGUI compareDescText;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        HideTooltip();
    }

    void Update()
    {
        // 툴팁 마우스 따라다니기 로직
        if (mainTooltipPanel.activeSelf)
        {
            mainTooltipPanel.transform.position = Input.mousePosition + new Vector3(20, -20, 0);
        }

        if (compareTooltipPanel.activeSelf)
        {
            RectTransform compareRect = compareTooltipPanel.GetComponent<RectTransform>();
            float realWidth = compareRect.rect.width * compareRect.lossyScale.x;
            compareTooltipPanel.transform.position = mainTooltipPanel.transform.position + new Vector3(-realWidth - 10, 0, 0);
        }
    }

    //툴팁 표시 메인 로직
    public void ShowTooltip(Item item, bool isFromInventory)
    {
        mainNameText.text = item.itemName;

        Item equippedItem = null;
        if (isFromInventory)
        {
            equippedItem = TabController.instance.GetEquippedItem(item.itemType);
        }

        // 메인 툴팁 텍스트 만들기
        string mainStatText = "";

        if (equippedItem != null) 
        {
            mainStatText += GetStatComparisonString("공격력", item.bonusAttack, equippedItem.bonusAttack);
            mainStatText += GetStatComparisonString("방어력", item.bonusDefense, equippedItem.bonusDefense);
            mainStatText += GetStatComparisonString("체력", item.bonusHealth, equippedItem.bonusHealth);
        }
        else 
        {
            mainStatText += GetRawStatString("공격력", item.bonusAttack);
            mainStatText += GetRawStatString("방어력", item.bonusDefense);
            mainStatText += GetRawStatString("체력", item.bonusHealth);
        }

     
        string mainOptionText = GetOptionsText(item);

        // 스탯, 옵션, 설명을 모두 합쳐서 출력
        mainDescText.text = mainStatText + mainOptionText + "\n" + item.itemDesc;
        mainTooltipPanel.SetActive(true);

        if (isFromInventory && equippedItem != null)
        {
            compareNameText.text = "[장착 중]\n" + equippedItem.itemName;

            string compareStatText = "";
            compareStatText += GetRawStatString("공격력", equippedItem.bonusAttack);
            compareStatText += GetRawStatString("방어력", equippedItem.bonusDefense);
            compareStatText += GetRawStatString("체력", equippedItem.bonusHealth);

            string compareOptionText = GetOptionsText(equippedItem);

            compareDescText.text = compareStatText + compareOptionText + "\n" + equippedItem.itemDesc;
            compareTooltipPanel.SetActive(true);
        }
        else
        {
            compareTooltipPanel.SetActive(false);
        }
    }

    public void HideTooltip()
    {
        mainTooltipPanel.SetActive(false);
        compareTooltipPanel.SetActive(false);
    }


    private string GetStatComparisonString(string statName, int itemStat, int equippedStat)
    {
        if (itemStat == 0 && equippedStat == 0) return "";

        int diff = itemStat - equippedStat;
        string result = $"{statName}: {itemStat} ";

        if (diff > 0) result += $"<color=#00FF00>(+{diff})</color>\n";
        else if (diff < 0) result += $"<color=#FF0000>({diff})</color>\n";
        else result += $"<color=#808080>(동일)</color>\n";

        return result;
    }

    private string GetRawStatString(string statName, int statValue)
    {
        if (statValue == 0) return "";
        return $"{statName}: <color=#00FF00>+{statValue}</color>\n";
    }

    private string GetOptionsText(Item item)
    {
        // 옵션이 하나도 없다면 빈 칸 반환
        if (item.currentOptions == null || item.currentOptions.Count == 0) return "";

        // 노란색으로 타이틀 달기 (보기 좋게 위아래로 줄바꿈 추가)
        string result = "\n<color=#FFD700>[랜덤 옵션]</color>\n";

        // 하늘색으로 각 옵션 설명 한 줄씩 추가
        foreach (ItemOption option in item.currentOptions)
        {
            result += $"<color=#00FFFF> - {option.description}</color>\n";
        }

        return result;
    }
}