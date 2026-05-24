using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContScrollButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("연결할 스크롤바")]
    public Scrollbar targetScrollbar;

    [Header("방향 설정 ")]
    public bool isUpButton = true;

    [Header("클릭 한 번에 이동할 양")]
    [Range(0.01f, 1.0f)]
    public float clickStep = 0.1f;

    [Header("꾹 누를 때 연속 스크롤 속도")]
    public float continuousSpeed = 1.0f;

    [Header("꾹 누르기 시작할 때까지의 딜레이")]
    public float delayBeforeContinuous = 0.3f;

    private bool isPressed = false;
    private float pressTime = 0f;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressTime = Time.time; // 누른 시간 기록

        MoveScroll(clickStep);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }

    void Update()
    {
        if (isPressed && (Time.time - pressTime) > delayBeforeContinuous)
        {
            MoveScroll(continuousSpeed * Time.deltaTime);
        }
    }

    private void MoveScroll(float amount)
    {
        if (targetScrollbar != null)
        {
            float direction = isUpButton ? -1f : 1f;

            targetScrollbar.value += direction * amount;

            targetScrollbar.value = Mathf.Clamp01(targetScrollbar.value);
        }
    }
}