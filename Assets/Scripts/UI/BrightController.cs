using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP 필수 네임스페이스

public class BrightnessController : MonoBehaviour
{
    public Slider brightnessSlider;
    public Volume globalVolume; // Global Volume 오브젝트 연결

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // 볼륨 프로필에서 Color Adjustments 효과를 가져옵니다.
        if (globalVolume.profile.TryGet<ColorAdjustments>(out var tmp))
        {
            colorAdjustments = tmp;
        }

        // 초기값 설정 (예: -3.0 ~ 3.0 범위)
        float savedBrightness = PlayerPrefs.GetFloat("PostBrightness", 0f);
        brightnessSlider.minValue = -3f; // 너무 낮으면 검게 변함
        brightnessSlider.maxValue = 3f;  // 너무 높으면 하얗게 타버림
        brightnessSlider.value = savedBrightness;

        UpdatePostBrightness(savedBrightness);

        brightnessSlider.onValueChanged.AddListener(UpdatePostBrightness);
    }

    public void UpdatePostBrightness(float value)
    {
        if (colorAdjustments != null)
        {
            // Post Exposure 값을 슬라이더 값으로 변경
            colorAdjustments.postExposure.value = value;
        }
        PlayerPrefs.SetFloat("PostBrightness", value);
    }
}