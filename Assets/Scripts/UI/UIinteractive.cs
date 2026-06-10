using UnityEngine;
using UnityEngine.EventSystems;

public class UIinteractive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    public float hoverScale = 1.2f;
    public float animationSpeed = 10f; 

    private Vector3 defaultScale;
    private Vector3 targetScale;
    private bool isHovered = false;
    private bool isPressed = false;

    void Start()
    {
        defaultScale = transform.localScale;
        targetScale = defaultScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTargetScale();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTargetScale();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateTargetScale();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateTargetScale();
    }

    private void UpdateTargetScale()
    {
        if (isHovered || isPressed)
        {
            targetScale = defaultScale * hoverScale;
        }
        else 
        {
            targetScale = defaultScale;
        }
    }
}
