using UnityEngine;

public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance;
    public Transform player;
    public DnBSynth synth; 

    public float detectionRadius = 10.0f; 
    
    private Collider2D[] hitBuffer = new Collider2D[20];
    private ContactFilter2D contactFilter;
    private bool isFlameActivated = false;
    
    // [NEW] 장르마다 시작 속도가 다르므로 저장해둘 변수
    private double baseBPM = 160.0;
    private bool initialized = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        detectionRadius = 10.0f;
    }

    public void SetFlameMode(bool active) { isFlameActivated = active; }
    public void TriggerDamageEffect() { if (synth != null) synth.TriggerGlitch(); }

    void Update()
    {
        if (synth == null) return;

        // [NEW] 처음 한 번, 신디사이저가 정한 장르의 BPM을 기준점으로 삼음
        if (!initialized)
        {
            baseBPM = synth.bpm; 
            initialized = true;
        }

        if (player == null) {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        int count = Physics2D.OverlapCircle(player.position, detectionRadius, contactFilter, hitBuffer);
        int enemyCount = 0;
        float closestDist = 100f;
        
        for (int i = 0; i < count; i++) {
            if (hitBuffer[i].CompareTag("Enemy")) {
                enemyCount++;
                float d = Vector2.Distance(player.position, hitBuffer[i].transform.position);
                if (d < closestDist) closestDist = d;
            }
        }
        
        UpdateAIConductor(enemyCount, closestDist);
    }

    void UpdateAIConductor(int enemyCount, float closestDist)
    {
        float distFactor = (enemyCount > 0) ? Mathf.Clamp01(1.0f - (closestDist / detectionRadius)) : 0f;

        // 1. 텐션 레벨
        int targetTension = (enemyCount == 0) ? 0 : ((enemyCount < 3) ? 1 : 2);
        synth.tensionLevel = targetTension;
        synth.flameMode = isFlameActivated;
        
        // 2. BPM 조절 (장르별 기본 BPM을 기준으로 빨라짐)
        double targetBPM = baseBPM;
        if (isFlameActivated) targetBPM = baseBPM + 20.0; // 각성 시 +20
        else targetBPM = baseBPM + (enemyCount * 2.0) + (distFactor * 5.0); // 적 많으면 빨라짐

        synth.bpm = Mathf.Lerp((float)synth.bpm, (float)targetBPM, Time.deltaTime);

        // 3. AI 파라미터 (Markov Chain)
        // 장르에 상관없이 적이 많으면 혼란스러워지는 로직은 유지
        float targetChaos = 0.2f + (enemyCount * 0.15f);
        float targetDensity = 0.5f + (distFactor * 0.4f);
        float targetPitchBias = (synth.lowHealthMode || enemyCount >= 3) ? 0.8f : 0.5f;

        synth.chaos = Mathf.Lerp(synth.chaos, Mathf.Clamp01(targetChaos), Time.deltaTime);
        synth.density = Mathf.Lerp(synth.density, Mathf.Clamp01(targetDensity), Time.deltaTime);
        synth.pitchBias = Mathf.Lerp(synth.pitchBias, Mathf.Clamp01(targetPitchBias), Time.deltaTime);
    }
}