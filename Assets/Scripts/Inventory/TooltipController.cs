using UnityEngine;
using TMPro; // TextMeshPro 사용

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
        HideTooltip(); // 시작 시 툴팁 숨기기
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

    // --- 툴팁 표시 메인 로직 ---
    public void ShowTooltip(Item item, bool isFromInventory)
    {
        mainNameText.text = item.itemName;

        Item equippedItem = null;
        if (isFromInventory)
        {
            equippedItem = TabController.instance.GetEquippedItem(item.itemType);
        }

        // 1. 메인 툴팁 텍스트 만들기 (스탯 + 설명)
        string mainStatText = "";

        if (equippedItem != null) // 장착 중인 장비가 있어서 '비교'해야 할 때
        {
            mainStatText += GetStatComparisonString("공격력", item.bonusAttack, equippedItem.bonusAttack);
            mainStatText += GetStatComparisonString("방어력", item.bonusDefense, equippedItem.bonusDefense);
            mainStatText += GetStatComparisonString("체력", item.bonusHealth, equippedItem.bonusHealth);
        }
        else // 비교할 장비가 없거나, 장비 창에서 마우스를 올렸을 때
        {
            mainStatText += GetRawStatString("공격력", item.bonusAttack);
            mainStatText += GetRawStatString("방어력", item.bonusDefense);
            mainStatText += GetRawStatString("체력", item.bonusHealth);
        }

        // 스탯 텍스트와 아이템 설명을 합쳐서 출력
        mainDescText.text = mainStatText + "\n" + item.itemDesc;
        mainTooltipPanel.SetActive(true);

        // 2. 비교 툴팁 텍스트 만들기
        if (isFromInventory && equippedItem != null)
        {
            compareNameText.text = "[장착 중]\n" + equippedItem.itemName;

            string compareStatText = "";
            compareStatText += GetRawStatString("공격력", equippedItem.bonusAttack);
            compareStatText += GetRawStatString("방어력", equippedItem.bonusDefense);
            compareStatText += GetRawStatString("체력", equippedItem.bonusHealth);

            compareDescText.text = compareStatText + "\n" + equippedItem.itemDesc;
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

    // ========================================================
    // 아래는 글자 색상을 입혀주는 핵심 도우미 함수들입니다!
    // ========================================================

    // 두 스탯을 비교해서 색깔 태그를 붙여주는 함수
    private string GetStatComparisonString(string statName, int itemStat, int equippedStat)
    {
        // 둘 다 0이면 아예 표시하지 않음 (깔끔한 UI를 위해)
        if (itemStat == 0 && equippedStat == 0) return "";

        int diff = itemStat - equippedStat;
        string result = $"{statName}: {itemStat} "; // 예: "방어력: 15 "

        if (diff > 0)
        {
            result += $"<color=#00FF00>(+{diff})</color>\n"; // 초록색
        }
        else if (diff < 0)
        {
            result += $"<color=#FF0000>({diff})</color>\n"; // 빨간색
        }
        else
        {
            result += $"<color=#808080>(동일)</color>\n"; // 회색
        }

        return result;
    }

    // 비교 없이 기본 스탯만 보여주는 함수 (초록색으로 표시)
    private string GetRawStatString(string statName, int statValue)
    {
        if (statValue == 0) return "";
        return $"{statName}: <color=#00FF00>+{statValue}</color>\n";
    }
}