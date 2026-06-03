using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryScrollButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("스크롤 설정")]
    public ScrollRect targetScrollRect; 
    public bool isUpButton;             

    [Header("속도 설정")]
    public float stepAmount = 0.1f;     
    public float scrollSpeed = 1.5f;    
    private bool isPressed = false;

    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        Scroll(stepAmount);
    }

    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    void Update()
    {
        if (isPressed)
        {
            Scroll(scrollSpeed * Time.unscaledDeltaTime);
        }
    }

    private void Scroll(float amount)
    {
        if (targetScrollRect == null) return;

        float direction = isUpButton ? 1f : -1f;
        targetScrollRect.verticalNormalizedPosition += direction * amount;
        targetScrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetScrollRect.verticalNormalizedPosition);
    }
}