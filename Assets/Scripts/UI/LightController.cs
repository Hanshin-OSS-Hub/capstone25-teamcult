using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LightController : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("UI Elements")]
    public RectTransform ringTransform;
    public Image ringImage;

    [Header("Settings")]
    public int brightnessSteps = 5;
    public AudioSource clickSound;
    public float rotationSpeed = 10f; 

    private float targetAngle = 0f;
    private int lastStep = -1;

    void Start()
    {
        ringImage.alphaHitTestMinimumThreshold = 0.5f;
        targetAngle = ringTransform.eulerAngles.z;
    }

    void Update()
    {
        float smoothAngle = Mathf.LerpAngle(ringTransform.eulerAngles.z, targetAngle, Time.deltaTime * rotationSpeed);
        ringTransform.rotation = Quaternion.Euler(0, 0, smoothAngle);
    }

    public void OnPointerDown(PointerEventData eventData) => CalculateInteraction(eventData);
    public void OnDrag(PointerEventData eventData) => CalculateInteraction(eventData);

    private void CalculateInteraction(PointerEventData eventData)
    {
        Vector2 dir = eventData.position - (Vector2)ringTransform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (brightnessSteps > 0)
        {
            // 단계별 값 계산
            float norm = Mathf.Repeat(angle + 180f, 360f) / 360f;
            int currentStep = Mathf.RoundToInt(norm * brightnessSteps);
            float snappedNorm = (float)currentStep / brightnessSteps;

            // 단계에 맞춰 목표 각도 설정
            targetAngle = (snappedNorm * 360f) - 180f;

            // 단계 변경 시 사운드 재생 및 밝기 적용
            if (currentStep != lastStep)
            {
                if (clickSound != null) clickSound.Play();
                lastStep = currentStep;
                ApplyBrightness(snappedNorm);
            }
        }
        else
        {
            // 단계 설정이 없을 때의 자유 회전 및 밝기 적용
            targetAngle = angle;
            float norm = Mathf.Repeat(angle + 180f, 360f) / 360f;
            ApplyBrightness(norm);
        }
    }

    private void ApplyBrightness(float value)
    {
        Debug.Log($"현재 밝기: {value * 100}%");
    }
}