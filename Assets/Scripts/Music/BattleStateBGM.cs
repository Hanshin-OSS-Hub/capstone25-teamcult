using UnityEngine;

public enum RoomState { Normal, Combat, Boss }

public class BattleStateBGM : MonoBehaviour
{
    public static BattleStateBGM Instance;

    [Header("통음원 파일 (MP3/WAV) 여러 곡 넣기")]
    public AudioClip[] normalTracks;
    public AudioClip[] combatTracks;
    public AudioClip[] bossTracks;

    [Header("Current Status")]
    public RoomState currentState = RoomState.Normal;

    private AudioSource musicSource;
    private AudioDistortionFilter distortionFilter;

    [Header("Glitch Effect Settings")]
    private bool isGlitching = false;
    private float glitchTimer = 0f;
    private float glitchDuration = 0.3f;

    [Header("Pitch Down Settings")]
    private bool isPitchDown = false; // 현재 피치 다운 상태인지 체크
    private float targetPitch = 1.0f; // 목표 피치 (기본 1.0, 다운 시 0.6)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;

        distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        distortionFilter.distortionLevel = 0f; 

        PlayRandomTrack();
    }

    void Update()
    {
        // 1. 곡이 끝나면 다음 랜덤 곡 재생
        if (!musicSource.isPlaying && musicSource.clip != null)
        {
            PlayRandomTrack();
        }

        // ★ 2. F키: 피치 다운 토글 (누를 때마다 켜짐/꺼짐)
        if (Input.GetKeyDown(KeyCode.F))
        {
            isPitchDown = !isPitchDown; // 상태 뒤집기 (true <-> false)
            targetPitch = isPitchDown ? 0.6f : 1.0f; // 켜지면 0.6배속, 꺼지면 1배속
            
            Debug.Log($"[피치 변경] 현재 피치 다운 모드: {isPitchDown}");
        }

        // ★ 3. G키: 기존의 글리치 효과 테스트용 (G키로 변경)
        if (Input.GetKeyDown(KeyCode.G) && !isGlitching)
        {
            TriggerGlitch();
        }

        // 4. 오디오 실시간 제어 로직
        if (isGlitching)
        {
            // 글리치 모드일 때는 소리 박살내기
            glitchTimer -= Time.deltaTime;

            if (glitchTimer > 0)
            {
                // 기준 피치(targetPitch)를 중심으로 흔들어서 피치 다운 상태에서도 글리치가 자연스럽게 먹히도록 함
                musicSource.pitch = targetPitch + Random.Range(-0.3f, 0.3f);
                distortionFilter.distortionLevel = Random.Range(0.6f, 0.9f);

                if (Random.value < 0.1f)
                {
                    musicSource.time = Mathf.Max(0, musicSource.time - 0.05f);
                }
            }
            else
            {
                // 글리치 종료 시 복구
                isGlitching = false;
                distortionFilter.distortionLevel = 0f;
            }
        }
        else
        {
            // ★ 글리치 상태가 아닐 때: 목표 피치(targetPitch)를 향해 스무스하게 변화
            // Lerp를 써서 DJ가 턴테이블 속도를 서서히 줄이거나 높이는 느낌 연출
            musicSource.pitch = Mathf.Lerp(musicSource.pitch, targetPitch, Time.deltaTime * 3f);
        }
    }

    public void SetBattleState(RoomState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log("🎵 [음악 엔진] 상태 변경! 즉시 음악을 바꿉니다: " + newState);
        PlayRandomTrack();
    }

    void PlayRandomTrack()
    {
        AudioClip[] activeArray = normalTracks;
        if (currentState == RoomState.Combat) activeArray = combatTracks;
        else if (currentState == RoomState.Boss) activeArray = bossTracks;

        if (activeArray != null && activeArray.Length > 0)
        {
            AudioClip nextClip = activeArray[Random.Range(0, activeArray.Length)];
            
            musicSource.clip = nextClip;
            musicSource.time = 0f; 
            musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }

    // 외부에서 부르는 글리치 함수 (그대로 유지)
    public void TriggerGlitch()
    {
        isGlitching = true;
        glitchTimer = glitchDuration;
    }
}