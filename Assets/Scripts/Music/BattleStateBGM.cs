using UnityEngine;
using System.Collections;

public enum RoomState { Normal, Combat, Boss }

public class BattleStateBGM : MonoBehaviour
{
    public static BattleStateBGM Instance;

    [Header("통음원 파일")]
    public AudioClip[] normalTracks;
    public AudioClip[] combatTracks;
    public AudioClip[] bossTracks;

    [Header("효과음 설정")]
    public AudioClip attackSound;
    public AudioClip heartMonitorSound; 

    [Header("Current Status")]
    public RoomState currentState = RoomState.Normal;
    public string currentElement = ""; 

    // 스피커 설정
    private AudioSource[] musicSources = new AudioSource[2];
    private int activeIndex = 0;
    private Coroutine fadeRoutine;
    private float targetVolume = 1.0f;

    private AudioSource sfxSource;
    private AudioSource heartMonitorSource; 

    // 이펙터
    private AudioDistortionFilter distortionFilter;
    private AudioChorusFilter poisonFilter; 

    // 상태 변수
    private bool isGlitching = false;
    private float glitchTimer = 0f;
    private float glitchDuration = 0.3f;

    private float targetPitch = 1.0f;
    private float currentPitch = 1.0f; 

    private bool isLowHealth = false;
    private float beepTimer = 0f;
    private bool isGameOver = false;

    // 플레이어 체력 관찰용
    private PlayerHealth playerObserver;
    private float lastKnownHealth = -1f;

    void Awake()
    {
        Instance = this; 
    }

    void Start()
    {
        Time.timeScale = 1.0f; 

        for (int i = 0; i < 2; i++)
        {
            musicSources[i] = gameObject.AddComponent<AudioSource>();
            musicSources[i].loop = false;
            musicSources[i].volume = 0f;
            musicSources[i].ignoreListenerPause = true; 
        }

        GameObject sfxObj = new GameObject("SFX_Speaker");
        sfxObj.transform.SetParent(this.transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;

        GameObject monitorObj = new GameObject("Monitor_Speaker");
        monitorObj.transform.SetParent(this.transform);
        heartMonitorSource = monitorObj.AddComponent<AudioSource>();
        heartMonitorSource.loop = false;
        heartMonitorSource.volume = 1.0f; 
        heartMonitorSource.ignoreListenerPause = true;

        distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        distortionFilter.distortionLevel = 0f;

        poisonFilter = gameObject.AddComponent<AudioChorusFilter>();
        poisonFilter.enabled = false; 

        PlayRandomTrack(false);
    }

    void Update()
    {
        if (isGameOver) return; 

        ObservePlayerHealth();

        if (!musicSources[activeIndex].isPlaying && musicSources[activeIndex].clip != null && fadeRoutine == null)
        {
            PlayRandomTrack(false);
        }

        // ★ 문제의 F키 입력(HandleInput) 호출을 완전히 삭제했습니다! ★
        HandleLowHealthBeep();

        if (isGlitching)
        {
            glitchTimer -= Time.deltaTime;
            if (glitchTimer > 0)
            {
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
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isLowHealth = false;
        
        distortionFilter.distortionLevel = 0f;
        poisonFilter.enabled = false;

        StartCoroutine(FadeOutAllAudio());
    }

    IEnumerator FadeOutAllAudio()
    {
        float dur = 1.5f; 
        float timer = 0f;
        float s0 = musicSources[0].volume;
        float s1 = musicSources[1].volume;

        while (timer < dur)
        {
            timer += Time.unscaledDeltaTime; 
            float p = timer / dur;
            musicSources[0].volume = Mathf.Lerp(s0, 0f, p);
            musicSources[1].volume = Mathf.Lerp(s1, 0f, p);
            yield return null; 
        }

        musicSources[0].Stop();
        musicSources[1].Stop();
        heartMonitorSource.Stop(); 
    }

    public void ApplyElementalEffect(string type)
    {
        if (string.IsNullOrEmpty(type)) return;
        currentElement = type.ToLower().Trim(); 
        
        if (currentElement == "ice") { targetPitch = 0.6f; }
        else if (currentElement == "lightning") { targetPitch = 1.4f; }
        else { targetPitch = 1.0f; }

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

        if (currentElement == "fire")
        {
            distortionFilter.distortionLevel = 0.5f; 
            targetVolume = 1.0f; 
        }
        else
        {
            distortionFilter.distortionLevel = 0f;
            targetVolume = 1.0f;
        }

        if (fadeRoutine == null) musicSources[activeIndex].volume = targetVolume;
    }

    public void SetBattleState(RoomState newState)
    {
        if (currentState == newState || isGameOver) return;
        currentState = newState;
        PlayRandomTrack(true);
    }

    void PlayRandomTrack(bool doCrossfade)
    {
        AudioClip[] activeArray = normalTracks;
        if (currentState == RoomState.Combat) activeArray = combatTracks;
        else if (currentState == RoomState.Boss) activeArray = bossTracks;

        if (activeArray != null && activeArray.Length > 0)
        {
            AudioClip nextClip = activeArray[Random.Range(0, activeArray.Length)];
            int nextIndex = 1 - activeIndex; 

            musicSources[nextIndex].clip = nextClip;
            musicSources[nextIndex].time = 0f;
            musicSources[nextIndex].Play();

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            if (doCrossfade) fadeRoutine = StartCoroutine(Crossfade(nextIndex));
            else { musicSources[nextIndex].volume = targetVolume; musicSources[activeIndex].Stop(); activeIndex = nextIndex; }
        }
    }

    IEnumerator Crossfade(int nextIndex)
    {
        float timer = 0f, duration = 0.2f;
        AudioSource fadeOut = musicSources[activeIndex], fadeIn = musicSources[nextIndex];
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; 
            fadeIn.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            fadeOut.volume = Mathf.Lerp(targetVolume, 0f, timer / duration);
            yield return null;
        }
        fadeIn.volume = targetVolume; fadeOut.volume = 0f; fadeOut.Stop();
        activeIndex = nextIndex; fadeRoutine = null;
    }

    public void TriggerGlitch() { if(!isGameOver) { isGlitching = true; glitchTimer = glitchDuration; } }
    public void PlayAttackFX() { if (attackSound != null && !isGameOver) sfxSource.PlayOneShot(attackSound); }
}