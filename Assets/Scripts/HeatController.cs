using UnityEngine;
// URP 포스트 프로세싱 제어를 위해 추가
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HeatController : MonoBehaviour
{
    [Header("--- Settings ---")]
    [Range(0, 0.3f)] public float maxDistortion = 0.15f; // 일렁임 최대 세기
    [Range(0, 1f)] public float maxVignette = 0.4f;      // 비네트 최대 세기
    public float particleEmissionRate = 30f;             // 파티클 개수

    [Header("--- References ---")]
    public Material heatMaterial;           // 1. 쉐이더 재질 (Mat_HeatFinal)
    public ParticleSystem edgeParticles;    // 2. 파티클 시스템 (EdgeParticles)
    public Volume globalVolume;             // 3. 글로벌 볼륨 (Vignette용)

    private Vignette vignetteEffect; // 실제 비네트 효과를 담을 변수

    private float currentDistortion = 0f;
    private float currentVignette = 0f;
    private bool isActive = false;

    void Start()
    {
        // 1. 쉐이더 초기화
        if (heatMaterial != null) heatMaterial.SetFloat("_DistortionStrength", 0);

        // 2. 파티클 초기화 (Emission 끄기)
        if (edgeParticles != null)
        {
            var emission = edgeParticles.emission;
            emission.rateOverTime = 0f;
        }

        // 3. 비네트 가져오기 및 초기화
        if (globalVolume != null && globalVolume.profile.TryGet(out vignetteEffect))
        {
            vignetteEffect.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("Global Volume이 없거나 Vignette 설정이 없습니다!");
        }
    }

    void Update()
    {
        // --- 부드러운 전환 (Lerp) ---
        float targetDist = isActive ? maxDistortion : 0f;
        float targetVig = isActive ? maxVignette : 0f;
        float lerpSpeed = Time.deltaTime * (isActive ? 3f : 2f); // 켜질땐 빠르게, 꺼질땐 느리게

        currentDistortion = Mathf.Lerp(currentDistortion, targetDist, lerpSpeed);
        currentVignette = Mathf.Lerp(currentVignette, targetVig, lerpSpeed);


        // --- 값 적용 ---

        // 1. 쉐이더 적용
        if (heatMaterial != null)
        {
            heatMaterial.SetFloat("_DistortionStrength", currentDistortion);
        }

        // 2. 비네트 적용
        if (vignetteEffect != null)
        {
            vignetteEffect.intensity.value = currentVignette;
        }

        // 3. 파티클 적용 (켜져있을 때만 발생)
        if (edgeParticles != null)
        {
            var emission = edgeParticles.emission;
            // 완전히 꺼졌을 때는 파티클도 0으로
            emission.rateOverTime = (currentDistortion > 0.001f) ? particleEmissionRate : 0f;
        }
    }

    // 아이템이 호출하는 함수 (영원히 켜짐)
    public void TriggerEffect()
    {
        isActive = true;
        // 파티클 시스템이 멈춰있다면 재생 시작
        if (edgeParticles != null && !edgeParticles.isPlaying) edgeParticles.Play();
    }

    // (옵션) 효과 끄기
    public void StopEffect()
    {
        isActive = false;
    }

    void OnDestroy()
    {
        // 종료 시 초기화
        if (heatMaterial != null) heatMaterial.SetFloat("_DistortionStrength", 0);
        if (vignetteEffect != null) vignetteEffect.intensity.value = 0;
    }
}