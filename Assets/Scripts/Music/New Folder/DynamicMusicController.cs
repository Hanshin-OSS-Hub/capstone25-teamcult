using UnityEngine;

public class DynamicMusicController : MonoBehaviour
{
    public AudioSource audio_Kick;
    public AudioSource audio_Bass;
    public AudioSource audio_Synth;
    public AudioSource audio_HiPerc;

    public float bassThreshold = 1.0f;
    public float synthThreshold = 5.0f;
    public float hiPercThreshold = 10.0f;
    
    public float fadeSpeed = 1.5f;

    private float currentThreat = 0.0f;
    private Camera mainCamera; 

    void Start()
    {
        mainCamera = Camera.main;

        if (audio_Kick != null) audio_Kick.volume = 1.0f;
        if (audio_Bass != null) audio_Bass.volume = 0.0f;
        if (audio_Synth != null) audio_Synth.volume = 0.0f;
        if (audio_HiPerc != null) audio_HiPerc.volume = 0.0f;
    }

    void Update()
    {
        if (mainCamera == null) return; 

        currentThreat = CalculateTotalThreat();
        UpdateMusicLayers();
    }

    private float CalculateTotalThreat()
    {
        float totalThreat = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObject in enemies)
        {
            if (enemyObject.activeInHierarchy)
            {
                
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemyObject.transform.position);

                bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                 viewportPos.y >= 0 && viewportPos.y <= 1 &&
                                 viewportPos.z > 0;


                if (isVisible) 
                {
                    Vector3 scale = enemyObject.transform.localScale;
                    float threatFromSize = scale.x * scale.y;
                    totalThreat += threatFromSize;
                }
            }
        }
        return totalThreat;
    }

    private void UpdateMusicLayers()
    {
        if (audio_Kick != null) audio_Kick.volume = 1.0f;

        float targetBassVolume = (currentThreat >= bassThreshold) ? 1.0f : 0.0f;
        float targetSynthVolume = (currentThreat >= synthThreshold) ? 1.0f : 0.0f;
        float targetHiPercVolume = (currentThreat >= hiPercThreshold) ? 1.0f : 0.0f;

        if (audio_Bass != null)
            audio_Bass.volume = Mathf.Lerp(audio_Bass.volume, targetBassVolume, Time.deltaTime * fadeSpeed);
        
        if (audio_Synth != null)
            audio_Synth.volume = Mathf.Lerp(audio_Synth.volume, targetSynthVolume, Time.deltaTime * fadeSpeed);
        
        if (audio_HiPerc != null)
            audio_HiPerc.volume = Mathf.Lerp(audio_HiPerc.volume, targetHiPercVolume, Time.deltaTime * fadeSpeed);
    }
}