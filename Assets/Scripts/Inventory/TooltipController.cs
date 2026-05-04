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

    public void ShowTooltip(Item item, bool isFromInventory)
    {
        mainNameText.text = item.itemName;

        Item equippedItem = null;
        if (isFromInventory)
        {
            equippedItem = TabController.instance.GetEquippedItem(item.itemType);
        }

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

    public void ShowHeartTooltip(string elementType)
    {
        switch (elementType)
        {
            case "Fire":
                mainNameText.text = "화염 하트";
                mainDescText.text = "공격에 <color=#FF4500>화염</color> 효과 부여\n" +
                                    "적에게 일정 시간 동안\n<color=#FF4500>화상 데미지</color>를 입힙니다.";
                break;
            case "Ice":
                mainNameText.text = "빙결 하트";
                mainDescText.text = "공격에 <color=#00BFFF>빙결</color> 효과 부여\n" +
                                    "적의 이동속도를\n<color=#00BFFF>80%</color> 감소시킵니다.";
                break;
            case "Lightning":
                mainNameText.text = "번개 하트";
                mainDescText.text = "공격에 <color=#FFD700>번개</color> 효과 부여\n" +
                                    "번개 체인을 만들어 처음 맞은 적의\n<color=#FFD700>50% 데미지</color>가 주변 적에게 전달됩니다.";
                break;
        }

        mainTooltipPanel.SetActive(true);
        compareTooltipPanel.SetActive(false);
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
        if (item.currentOptions == null || item.currentOptions.Count == 0) return "";

        string result = "\n<color=#FFD700>[랜덤 옵션]</color>\n";

        foreach (ItemOption option in item.currentOptions)
        {
            result += $"<color=#00FFFF> - {option.description}</color>\n";
        }

        return result;
    }
}