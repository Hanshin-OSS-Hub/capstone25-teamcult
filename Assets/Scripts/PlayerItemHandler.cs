using UnityEngine; 
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 
public class PlayerItemHandler : MonoBehaviour
{
    [Header("효과 설정")]
    [Tooltip("Hierarchy 창의 Global Volume 오브젝트를 여기로 드래그하세요.")]
    public Volume globalPostProcessVolume;

    [Tooltip("효과가 켜질 때의 강도입니다.")]
    public float effectIntensity = 0.5f;

    // (선택 사항) 효과가 지속될 시간 (초)
    // public float effectDuration = 5.0f;

    
    private Vignette vignetteEffect;

    void Start()
    {
        
        if (globalPostProcessVolume != null)
        {
            globalPostProcessVolume.profile.TryGet<Vignette>(out vignetteEffect);
        }

        
        if (vignetteEffect != null)
        {
            vignetteEffect.intensity.value = 0f;
        }
    }

    // 아이템과 충돌했을 때 호출됩니다.
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. "FlameHeartItem" 태그를 가진 물체와 부딪혔는지 확인
        if (other.CompareTag("FlameHeartItem"))
        {
            // 2. 비네트(Vignette) 효과를 켤 수 있는지 확인
            if (vignetteEffect != null)
            {
                Debug.Log("불꽃 하트 획득! 비네트 효과 ON!");

                // 3. 비네트 색상과 강도를 설정합니다.
                vignetteEffect.color.value = Color.red; // 붉은색
                vignetteEffect.intensity.value = effectIntensity; // 설정한 강도 (예: 0.5)

                // (선택 사항) 일정 시간 뒤에 효과를 끄고 싶다면 아래 줄의 주석을 푸세요.
                // Invoke(nameof(TurnEffectOff), effectDuration);
            }

            // 4. 아이템 삭제
            Destroy(other.gameObject);
        }

        // (응용 예시) 만약 "IceHeartItem" 태그를 가진 아이템을 먹는다면
        if (other.CompareTag("IceHeartItem"))
        {
            if (vignetteEffect != null)
            {
                Debug.Log("얼음 하트 획득! 비네트 효과 ON!");
                vignetteEffect.color.value = Color.blue; // 파란색
                vignetteEffect.intensity.value = effectIntensity; // 설정한 강도
                Destroy(other.gameObject);
            }
        }
    }

    // (선택 사항) 효과를 끄는 함수
    public void TurnEffectOff()
    {
        if (vignetteEffect != null)
        {
            vignetteEffect.intensity.value = 0f;
            Debug.Log("비네트 효과 OFF");
        }
    }
}