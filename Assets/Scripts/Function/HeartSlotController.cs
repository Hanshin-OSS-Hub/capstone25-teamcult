using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeartSlotController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static HeartSlotController instance;

    public Image heartSlotImage;
    public Sprite fireHeartSprite;
    public Sprite iceHeartSprite;
    public Sprite lightningHeartSprite;

    private string currentElement = "";

    void Awake() { instance = this; }

    public void SetHeart(string elementType)
    {
        currentElement = elementType;
        switch (elementType)
        {
            case "Fire": heartSlotImage.sprite = fireHeartSprite; break;
            case "Ice": heartSlotImage.sprite = iceHeartSprite; break;
            case "Lightning": heartSlotImage.sprite = lightningHeartSprite; break;
        }
        heartSlotImage.enabled = true;
    }

    public void ClearHeart()
    {
        currentElement = "";
        heartSlotImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentElement == "") return;
        TooltipController.instance.ShowHeartTooltip(currentElement);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.instance.HideTooltip();
    }
}