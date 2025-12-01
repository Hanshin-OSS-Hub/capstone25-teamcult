using UnityEngine;
using UnityEngine.EventSystems;

public class UIinteractive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Scale Settings")]
    public float hoverScale = 1.2f; // 커질 크기 배율 (기본 1.2배)
    public float animationSpeed = 10f; // 커지는 속도

    private Vector3 defaultScale;
    private Vector3 targetScale;
    private bool isHovered = false;
    private bool isPressed = false;

    void Start()
    {
        // 시작할 때의 원래 크기를 기억
        defaultScale = transform.localScale;
        targetScale = defaultScale;
    }

    void Update()
    {
        // 목표 크기로 부드럽게 변경 (Lerp 사용)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    // --- 이벤트 감지 로직 ---

    // 1. 마우스가 UI 위에 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTargetScale();
    }

    // 2. 마우스가 UI 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTargetScale();
    }

    // 3. 마우스를 꾹 눌렀을 때 (드래그 시작)
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateTargetScale();
    }

    // 4. 마우스를 뗐을 때 (드래그 종료)
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateTargetScale();
    }

    // 크기 결정 로직
    private void UpdateTargetScale()
    {
        // 마우스가 올려져 있거나, 누르고 있는 상태라면 -> 커짐
        if (isHovered || isPressed)
        {
            targetScale = defaultScale * hoverScale;
        }
        else // 아니면 -> 원래대로
        {
            targetScale = defaultScale;
        }
    }
}
