using UnityEngine;
using System.Collections;

// ★ 4단계로 확장된 RoomState
public enum RoomState { Normal, Tension, Combat, Boss }

public class BattleStateBGM : MonoBehaviour
{
    public static BattleStateBGM Instance;

    [Header("통음원 파일 (4단계 변주)")]
    public AudioClip[] normalTracks;
    public AudioClip[] tensionTracks; // ★ 1~2마리 감지 시 재생될 긴장감 있는 곡
    public AudioClip[] combatTracks;
    public AudioClip[] bossTracks;

    [Header("심박동 설정")]
    public AudioClip heartMonitorSound; 

    public RoomState currentState = RoomState.Normal;
    public string currentElement = ""; 

    private AudioSource[] musicSources = new AudioSource[2];
    private int activeIndex = 0;
    private Coroutine fadeRoutine;
    private float targetVolume = 1.0f;

    private AudioSource heartMonitorSource; 
    private AudioDistortionFilter distortionFilter;
    private AudioChorusFilter poisonFilter; 

    private bool isGlitching = false;
    private float glitchTimer = 0f;
    private float glitchDuration = 0.3f;

    private float targetPitch = 1.0f;
    private float currentPitch = 1.0f; 

    private bool isLowHealth = false;
    private float beepTimer = 0f;
    private bool isGameOver = false;

    private PlayerHealth playerObserver;
    private float lastKnownHealth = -1f;

    void Awake() { Instance = this; }

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

        GameObject monitorObj = new GameObject("Monitor_Speaker");
        monitorObj.transform.SetParent(this.transform);
        heartMonitorSource = monitorObj.AddComponent<AudioSource>();
        heartMonitorSource.loop = false;
        heartMonitorSource.volume = 1.0f; 
        heartMonitorSource.ignoreListenerPause = true;

        distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        poisonFilter = gameObject.AddComponent<AudioChorusFilter>();

        PlayRandomTrack(false);
    }

    void Update()
    {
        if (isGameOver) return; 

        ObservePlayerHealth();

        if (!musicSources[activeIndex].isPlaying && musicSources[activeIndex].clip != null && fadeRoutine == null)
            PlayRandomTrack(false);

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
            else { isGlitching = false; ApplyElementalFilters(); }
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

        // 체력이 깎였을 때 (글리치 + 플레이어 피격음 자동 재생)
        if (currentHP < lastKnownHealth)
        {
            TriggerGlitch();
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.PlayerHit);
        }

        // 플레이어 사망 시 (사망음 자동 재생)
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
        if (!low) { beepTimer = 0f; if (heartMonitorSource.isPlaying) heartMonitorSource.Stop(); }
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
        float dur = 1.5f, timer = 0f, s0 = musicSources[0].volume, s1 = musicSources[1].volume;
        while (timer < dur)
        {
            timer += Time.unscaledDeltaTime; 
            musicSources[0].volume = Mathf.Lerp(s0, 0f, timer / dur);
            musicSources[1].volume = Mathf.Lerp(s1, 0f, timer / dur);
            yield return null; 
        }
        musicSources[0].Stop(); musicSources[1].Stop(); heartMonitorSource.Stop(); 
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

    public void ClearElementalEffect() { currentElement = ""; targetPitch = 1.0f; ApplyElementalFilters(); }

    void ApplyElementalFilters()
    {
        if (isGlitching) return;
        poisonFilter.enabled = (currentElement == "poison");
        distortionFilter.distortionLevel = (currentElement == "fire") ? 0.5f : 0f;
        targetVolume = 1.0f; 
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
        if (currentState == RoomState.Tension) activeArray = tensionTracks; // ★ 2단계 트랙 추가
        else if (currentState == RoomState.Combat) activeArray = combatTracks;
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
}