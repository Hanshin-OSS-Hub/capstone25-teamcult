using UnityEngine;
// URP 2D 조명을 제어하기 위해 꼭 필요한 줄입니다.
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D myLight; // 내 조명 컴포넌트
    public float minIntensity = 0.8f; // 최소 밝기
    public float maxIntensity = 1.2f; // 최대 밝기
    public float flickerSpeed = 10f;  // 떨림 속도

    void Start()
    {
        // 시작할 때 내 몸에 붙은 Light2D 컴포넌트를 찾아옵니다.
        myLight = GetComponent<Light2D>();
    }

    void Update()
    {
        if (myLight == null) return;

        // 펄린 노이즈(Perlin Noise)라는 함수를 써서 자연스럽게 떨리게 만듭니다.
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        // 밝기를 최소~최대 사이에서 랜덤하게 조절합니다.
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}