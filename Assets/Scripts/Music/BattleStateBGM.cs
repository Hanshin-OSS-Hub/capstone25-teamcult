using UnityEngine;
using System.Collections;

public class BattleStateBGM : MonoBehaviour
{
    public static BattleStateBGM Instance;

    public enum ThreatLevel { None, Normal, Tension, Combat, Boss }
    public ThreatLevel currentLevel = ThreatLevel.None;

    [Header("BGM 오디오 클립 세팅 (4단계)")]
    public AudioClip normalBGM;    
    public AudioClip tensionBGM;   
    public AudioClip combatBGM;    
    public AudioClip bossBGM;      

    [Header("믹싱(크로스페이드) 설정")]
    public float crossfadeDuration = 2.0f; 
    public float maxVolume = 1.0f;

    [Header("심박동 설정 (복구됨)")]
    public AudioClip heartMonitorSound; 

    // 카세트 플레이어 2개
    private AudioSource[] musicSources = new AudioSource[2];
    private int activeIndex = 0;
    private Coroutine fadeRoutine;

    // 복구된 오디오 소스 및 필터들
    private AudioSource heartMonitorSource; 
    private AudioDistortionFilter distortionFilter;
    private AudioChorusFilter poisonFilter; 
    private AudioLowPassFilter lowPassFilter;

    // 글리치 및 피치 변수 (오리지널 로직)
    private bool isGlitching = false;
    private float glitchTimer = 0f;
    private float glitchDuration = 0.3f;
    private float targetPitch = 1.0f;
    private float currentPitch = 1.0f; 
    private string currentElement = ""; 

    // 체력 및 상태 변수 (오리지널 로직)
    private bool isLowHealth = false;
    private float beepTimer = 0f;
    private bool isGameOver = false;

    private PlayerHealth playerObserver;
    private float lastKnownHealth = -1f;
    private Coroutine pendingPlayRoutine;

    void Awake()
    {
        Instance = this;
        PreloadClip(normalBGM);
        PreloadClip(tensionBGM);
        PreloadClip(combatBGM);
        PreloadClip(bossBGM);
        PreloadClip(heartMonitorSound);
    }

    void Start()
    {
        Time.timeScale = 1.0f; 

        // BGM용 오디오 소스 2개 세팅
        for (int i = 0; i < 2; i++)
        {
            musicSources[i] = gameObject.AddComponent<AudioSource>();
            musicSources[i].loop = true; 
            musicSources[i].volume = 0f;
            musicSources[i].ignoreListenerPause = true; 
        }

        // 심박동용 오디오 소스 세팅
        GameObject monitorObj = new GameObject("Monitor_Speaker");
        monitorObj.transform.SetParent(this.transform);
        heartMonitorSource = monitorObj.AddComponent<AudioSource>();
        heartMonitorSource.loop = false;
        heartMonitorSource.volume = 1.0f; 
        heartMonitorSource.ignoreListenerPause = true;

        // 특수 효과용 필터 세팅
        distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        poisonFilter = gameObject.AddComponent<AudioChorusFilter>();
        lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        
        poisonFilter.enabled = false;
        distortionFilter.distortionLevel = 0f;
        lowPassFilter.cutoffFrequency = 22000f;
    }

    void Update()
    {
        if (isGameOver) return; 

        // 매 프레임 체력 관찰 및 심박동 재생
        ObservePlayerHealth();
        HandleLowHealthBeep();

        // 오리지널 글리치 및 피치 스무딩 로직
        if (isGlitching)
        {
            glitchTimer -= Time.deltaTime;
            if (glitchTimer > 0)
            {
                // 글리치 발동 시 피치가 랜덤으로 튀고 노이즈(Distortion)가 심해짐
                float randPitch = targetPitch + Random.Range(-0.3f, 0.3f);
                musicSources[0].pitch = randPitch;
                musicSources[1].pitch = randPitch;
                distortionFilter.distortionLevel = Random.Range(0.6f, 0.9f); 
            }
            else 
            { 
                isGlitching = false; 
                ApplyElementalFilters(); 
            }
        }
        else
        {
            // 평상시에는 타겟 피치(원소 효과 등)로 부드럽게 이동
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * 3f);
            musicSources[0].pitch = currentPitch;
            musicSources[1].pitch = currentPitch;
        }
    }


    void ObservePlayerHealth()
    {
        if (playerObserver == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerObserver = p.GetComponent<PlayerHealth>();
            return;
        }

        float currentHP = playerObserver.currentHealth;
        if (lastKnownHealth < 0) lastKnownHealth = currentHP;

        if (currentHP < lastKnownHealth)
        {
            TriggerGlitch();
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.PlayerHit);
        }

        if (currentHP <= 0 && lastKnownHealth > 0)
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.PlayerDeath);
        }

        lastKnownHealth = currentHP;
        SetLowHealth(currentHP > 0f && currentHP <= 2f);
    }

    void HandleLowHealthBeep()
    {
        if (isLowHealth && heartMonitorSound != null)
        {
            beepTimer += Time.unscaledDeltaTime; 
            if (beepTimer >= 1.0f) 
            {
                heartMonitorSource.PlayOneShot(heartMonitorSound);
                beepTimer = 0f;
            }
        }
    }

    public void SetLowHealth(bool low)
    {
        if (isGameOver) return;
        isLowHealth = low;
        if (!low) 
        { 
            beepTimer = 0f; 
            if (heartMonitorSource.isPlaying) heartMonitorSource.Stop(); 
        }
        lowPassFilter.cutoffFrequency = low ? 1200f : 22000f; // 먹먹함 효과 추가
    }

    public void TriggerGlitch() 
    { 
        if(!isGameOver) { isGlitching = true; glitchTimer = glitchDuration; } 
    }

  

    public void ApplyElementalEffect(string type)
    {
        if (string.IsNullOrEmpty(type)) return;
        currentElement = type.ToLower().Trim(); 
        
        if (currentElement == "ice") targetPitch = 0.6f; 
        else if (currentElement == "lightning") targetPitch = 1.4f; 
        else targetPitch = 1.0f;
        
        ApplyElementalFilters();
    }

    public void ClearElementalEffect() 
    { 
        currentElement = ""; 
        targetPitch = 1.0f; 
        ApplyElementalFilters(); 
    }

    void ApplyElementalFilters()
    {
        if (isGlitching) return;
        poisonFilter.enabled = (currentElement == "poison");
        // 불 하트(fire)를 먹었을 때의 이글거리는 디스토션 효과 복구
        distortionFilter.distortionLevel = (currentElement == "fire") ? 0.5f : 0f;
    }

 

    public void SetBattleState(ThreatLevel newLevel)
    {
        if (currentLevel == newLevel || isGameOver) return;
        currentLevel = newLevel;

        AudioClip nextClip = null;
        switch (newLevel)
        {
            case ThreatLevel.Normal:  nextClip = normalBGM; break;
            case ThreatLevel.Tension: nextClip = tensionBGM; break;
            case ThreatLevel.Combat:  nextClip = combatBGM; break;
            case ThreatLevel.Boss:    nextClip = bossBGM; break;
        }

        if (nextClip != null) PlayWithCrossfade(nextClip);
    }

    void PlayWithCrossfade(AudioClip nextClip)
    {
        if (nextClip.loadState != AudioDataLoadState.Loaded)
        {
            if (nextClip.loadState == AudioDataLoadState.Unloaded)
            {
                nextClip.LoadAudioData();
            }

            if (pendingPlayRoutine != null) StopCoroutine(pendingPlayRoutine);
            pendingPlayRoutine = StartCoroutine(PlayWhenLoaded(nextClip));
            return;
        }

        int nextIndex = 1 - activeIndex; 

        if (musicSources[activeIndex].clip == nextClip) return;

        musicSources[nextIndex].clip = nextClip;
        musicSources[nextIndex].time = 0f;
        musicSources[nextIndex].Play();

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade(nextIndex));
    }

    private void PreloadClip(AudioClip clip)
    {
        if (clip == null || clip.loadState != AudioDataLoadState.Unloaded) return;
        clip.LoadAudioData();
    }

    private IEnumerator PlayWhenLoaded(AudioClip clip)
    {
        while (clip != null && clip.loadState == AudioDataLoadState.Loading)
        {
            yield return null;
        }

        pendingPlayRoutine = null;

        if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
        {
            PlayWithCrossfade(clip);
        }
    }

    IEnumerator Crossfade(int nextIndex)
    {
        float timer = 0f;
        AudioSource fadeOut = musicSources[activeIndex];
        AudioSource fadeIn = musicSources[nextIndex];
        float startFadeOutVol = fadeOut.volume;

        while (timer < crossfadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            fadeIn.volume = Mathf.Lerp(0f, maxVolume, timer / crossfadeDuration);
            fadeOut.volume = Mathf.Lerp(startFadeOutVol, 0f, timer / crossfadeDuration);
            yield return null;
        }
        fadeIn.volume = maxVolume; 
        fadeOut.volume = 0f; 
        fadeOut.Stop();
        activeIndex = nextIndex; 
        fadeRoutine = null;
    }



    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isLowHealth = false;
        
        distortionFilter.distortionLevel = 0f;
        poisonFilter.enabled = false;
        lowPassFilter.cutoffFrequency = 22000f;
        
        StartCoroutine(FadeOutAllAudio());
    }

    IEnumerator FadeOutAllAudio()
    {
        float dur = 1.5f, timer = 0f;
        float s0 = musicSources[0].volume;
        float s1 = musicSources[1].volume;
        
        while (timer < dur)
        {
            timer += Time.unscaledDeltaTime; 
            musicSources[0].volume = Mathf.Lerp(s0, 0f, timer / dur);
            musicSources[1].volume = Mathf.Lerp(s1, 0f, timer / dur);
            yield return null; 
        }
        
        musicSources[0].Stop(); 
        musicSources[1].Stop(); 
        heartMonitorSource.Stop(); 
    }

  
    private Coroutine wobbleCoroutine;

    public void TriggerSonicWobble(float duration = 1.5f)
    {
        if (isGameOver) return;
        
        if (wobbleCoroutine != null) StopCoroutine(wobbleCoroutine);
        wobbleCoroutine = StartCoroutine(SonicWobbleRoutine(duration));
    }

    private IEnumerator SonicWobbleRoutine(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; 
            
            float wobblePitch = Mathf.Sin(timer * 20f) * 0.15f; 
            
            musicSources[0].pitch = targetPitch + wobblePitch;
            musicSources[1].pitch = targetPitch + wobblePitch;

            float wobbleFilter = Mathf.Abs(Mathf.Sin(timer * 20f)) * 10000f;
            lowPassFilter.cutoffFrequency = 22000f - wobbleFilter;

            yield return null;
        }

        musicSources[0].pitch = targetPitch;
        musicSources[1].pitch = targetPitch;
        
        lowPassFilter.cutoffFrequency = isLowHealth ? 1200f : 22000f; 
        wobbleCoroutine = null;
    }
}
