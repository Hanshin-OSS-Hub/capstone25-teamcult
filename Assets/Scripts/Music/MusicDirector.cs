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

    // ★★★ AI 지휘자 (LLM Simulation) ★★★
    // 게임 상황 데이터를 바탕으로 작곡 파라미터(Markov Weights)를 실시간 조정
    void UpdateAIConductor(int enemyCount, float closestDist)
    {
        float distFactor = (enemyCount > 0) ? Mathf.Clamp01(1.0f - (closestDist / detectionRadius)) : 0f;

        // 1. 기본 상태 (BPM 등)
        int targetTension = (enemyCount == 0) ? 0 : ((enemyCount < 3) ? 1 : 2);
        synth.tensionLevel = targetTension;
        synth.flameMode = isFlameActivated;
        
        double targetBPM = isFlameActivated ? 170.0 : (150.0 + enemyCount * 2.0 + distFactor * 5.0);
        synth.bpm = Mathf.Lerp((float)synth.bpm, (float)targetBPM, Time.deltaTime);

        // 2. [핵심] 마르코프 체인 파라미터 조절 (스타일 결정)
        // 이 부분이 나중에 LLM의 프롬프트 결과값으로 대체될 수 있습니다.
        
        float targetChaos = 0.2f;   // 기본: 안정적
        float targetDensity = 0.5f; // 기본: 여유로움
        float targetPitchBias = 0.5f; // 기본: 중음역

        if (isFlameActivated)
        {
            // 각성 모드: 매우 혼란스럽고(Chaos), 음표가 꽉 차고(Density High), 고음역(High Pitch)
            // 하지만 각성 모드는 'GenCinematicBrass'가 덮어쓰므로 멜로디 생성은 덜 중요할 수 있음
        }
        else if (enemyCount > 0)
        {
            // 적 발견:
            // - 적이 많을수록 Chaos 증가 (불규칙한 멜로디)
            // - 거리가 가까울수록 Density 증가 (급박함)
            
            targetChaos = 0.2f + (enemyCount * 0.15f); // 적 3명이면 0.65 (매우 혼란)
            targetDensity = 0.6f + (distFactor * 0.4f); // 가까우면 1.0 (쉼표 없음)
            
            // 위기 상황(체력 낮음 or 적 많음)이면 고음역대 사용
            if (synth.lowHealthMode || enemyCount >= 3) targetPitchBias = 0.8f; 
        }
        else
        {
            // 평화: 질서 정연하고(Chaos Low), 듬성듬성한(Density Low) 멜로디
            targetChaos = 0.1f;
            targetDensity = 0.3f; // 쉼표가 많아 여백의 미
            targetPitchBias = 0.4f; // 약간 저음의 차분함
        }

        // 파라미터를 부드럽게 Synth에 적용
        synth.chaos = Mathf.Lerp(synth.chaos, Mathf.Clamp01(targetChaos), Time.deltaTime);
        synth.density = Mathf.Lerp(synth.density, Mathf.Clamp01(targetDensity), Time.deltaTime);
        synth.pitchBias = Mathf.Lerp(synth.pitchBias, Mathf.Clamp01(targetPitchBias), Time.deltaTime);
        
        // *참고: 이 값들이 변하면 DnBSynth의 GenerateMarkovMelody()가 호출될 때 
        //       새로운 스타일의 멜로디가 만들어집니다.
    }
}