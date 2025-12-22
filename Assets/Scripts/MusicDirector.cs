using UnityEngine;

public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance;

    [Header("References")]
    public Transform player;
    public DnBSynth synth; 
    // 유저가 제공한 체력 스크립트 연결
    public HealthBarManager healthManager; 

    [Header("Detection")]
    public float detectionRadius = 10f;
    private Collider2D[] hitBuffer = new Collider2D[10];

    [Header("Music Logic")]
    [Range(0, 1)] public float currentHPPercent = 1.0f; // 디버깅용 표시

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (synth == null) return;
        
        // 플레이어 찾기
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) 
            {
                player = p.transform;
                // 플레이어 찾을 때 HealthManager도 같이 찾음
                if(healthManager == null) healthManager = p.GetComponent<HealthBarManager>();
            }
            return;
        }

        // 1. 적 감지 (Tension)
        int count = Physics2D.OverlapCircleNonAlloc(player.position, detectionRadius, hitBuffer);
        int enemyCount = 0;
        float closestDist = 100f;
        for (int i = 0; i < count; i++)
        {
            if (hitBuffer[i].CompareTag("Enemy"))
            {
                enemyCount++;
                float d = Vector2.Distance(player.position, hitBuffer[i].transform.position);
                if (d < closestDist) closestDist = d;
            }
        }

        // 2. 체력 가져오기 (Health State)
        if (healthManager != null)
        {
            currentHPPercent = healthManager.GetHealthPercentage();
        }

        // --- 음악 파라미터 제어 (매우 중요) ---
        UpdateMusicState(enemyCount, closestDist, currentHPPercent);
    }

    // MusicDirector.cs 의 UpdateMusicState 함수 내부 수정 제안
    void UpdateMusicState(int enemyCount, float distance, float hpPercent)
    {
        // ... (BPM 및 Tension 로직 동일) ...
        float tension = 0f;
        if (enemyCount > 0) tension = Mathf.Clamp01(1.0f - (distance / detectionRadius));
        double targetBPM = 170.0 + (tension * 10.0);
        synth.bpm = Mathf.Lerp((float)synth.bpm, (float)targetBPM, Time.deltaTime);

        if (enemyCount == 0) synth.tensionLevel = 0; 
        else if (tension < 0.6f) synth.tensionLevel = 1; 
        else synth.tensionLevel = 2; 

        // [체력 로직]
        if (hpPercent < 0.3f) // 체력 30% 미만
        {
            synth.lowHealthMode = true;
            
            // 소리는 더 먹먹하게 (물속 느낌 강화)
            float targetCutoff = Mathf.Lerp(400f, 22000f, hpPercent / 0.3f);
            synth.cutoffFrequency = Mathf.Lerp(synth.cutoffFrequency, targetCutoff, Time.deltaTime * 5.0f);
            
            // 빈사 상태일 때 베이스 디스토션 강화 (불안감 조성)
            synth.bassDistortion = 0.6f; 
        }
        else
        {
            synth.lowHealthMode = false;
            synth.cutoffFrequency = Mathf.Lerp(synth.cutoffFrequency, 22000f, Time.deltaTime * 5.0f);
            synth.bassDistortion = tension * 0.3f;
        }
    }
    
    void OnDrawGizmos() {
        if(player != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, detectionRadius);
        }
    }
}