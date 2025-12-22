using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HeatController : MonoBehaviour
{
    [Header("--- Settings ---")]
    [Range(0, 0.3f)] public float maxDistortion = 0.15f;
    [Range(0, 1f)] public float maxVignette = 0.4f;
    public float particleEmissionRate = 30f;

    [Header("--- References ---")]
    public Material heatMaterial;
    public ParticleSystem edgeParticles;
    public Volume globalVolume;

    // ★ uiHearts 리스트 삭제 (이제 PlayerHealth가 관리합니다)

    private bool isActive = false;
    private Vignette vignetteEffect;
    private float currentDistortion = 0f;
    private float currentVignette = 0f;

    void Start()
    {
        if (heatMaterial != null) heatMaterial.SetFloat("_DistortionStrength", 0);
        if (edgeParticles != null)
        {
            var emission = edgeParticles.emission;
            emission.rateOverTime = 0f;
        }
        if (globalVolume != null && globalVolume.profile.TryGet(out vignetteEffect))
        {
            vignetteEffect.intensity.value = 0f;
        }
    }

    void Update()
    {
        float targetDist = isActive ? maxDistortion : 0f;
        float targetVig = isActive ? maxVignette : 0f;
        float lerpSpeed = Time.deltaTime * (isActive ? 3f : 2f);

        currentDistortion = Mathf.Lerp(currentDistortion, targetDist, lerpSpeed);
        currentVignette = Mathf.Lerp(currentVignette, targetVig, lerpSpeed);

        if (heatMaterial != null) heatMaterial.SetFloat("_DistortionStrength", currentDistortion);
        if (vignetteEffect != null) vignetteEffect.intensity.value = currentVignette;
        if (edgeParticles != null)
        {
            var emission = edgeParticles.emission;
            emission.rateOverTime = (currentDistortion > 0.001f) ? particleEmissionRate : 0f;
        }
    }

    // 효과 켜기
    public void TriggerEffect()
    {
        isActive = true;
        if (edgeParticles != null && !edgeParticles.isPlaying) edgeParticles.Play();
    }

    // 효과 끄기
    public void StopEffect()
    {
        isActive = false;
    }

    void OnDestroy()
    {
        if (heatMaterial != null) heatMaterial.SetFloat("_DistortionStrength", 0);
        if (vignetteEffect != null) vignetteEffect.intensity.value = 0;
    }
}