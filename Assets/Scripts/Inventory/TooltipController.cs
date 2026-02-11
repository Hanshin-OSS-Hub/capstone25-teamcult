using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TooltipController : MonoBehaviour
{
    public static TooltipController instance;

    [Header("UI 연결")]
    public GameObject tooltipPanel; // 툴팁 전체 패널
    public TextMeshProUGUI TitleText;// 아이템 이름 텍스트
    public TextMeshProUGUI DescbText;// 아이템 설명 텍스트
    public RectTransform rectTransform; // 마우스 위치 따라가기용
 
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 게임 시작 시 툴팁 숨기기
        HideTooltip();
    }

    void Update()
    {
        // 툴팁이 켜져있다면 마우스 위치를 따라다니게 함
        if (tooltipPanel.activeSelf)
        {
            // 마우스 위치에 약간의 오프셋을 주어 커서에 가려지지 않게 함
            Vector2 mousePos = Input.mousePosition;
            transform.position = new Vector3(mousePos.x + 15, mousePos.y - 15, 0f);
        }
    }

    public void ShowTooltip(Item item)
    {
        TitleText.text = item.itemName;
        DescbText.text = item.itemDesc;
        tooltipPanel.SetActive(true);

        // 맨 위에 그려지도록 순서 변경 (가려짐 방지)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        transform.SetAsLastSibling();
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}