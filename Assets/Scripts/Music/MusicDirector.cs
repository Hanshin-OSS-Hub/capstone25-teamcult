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

    // ★★★ 다른 스크립트에서 호출하는 함수들 ★★★
    public void OnPlayerAttack()
    {
        if (synth != null) synth.TriggerAttackGlitch();
    }

    public void OnPlayerDamaged()
    {
        if (synth != null) synth.TriggerDamageGlitch();
    }

    void Update()
    {
        if (synth == null) return;
        if (!initialized) { baseBPM = synth.bpm; initialized = true; }

        if (player == null) {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        int count = Physics2D.OverlapCircle(player.position, detectionRadius, contactFilter, hitBuffer);
        int enemyCount = 0;
        float closestDist = 100f;
        int heavyCnt = 0, speedCnt = 0, weirdCnt = 0;
        
        for (int i = 0; i < count; i++) {
            if (hitBuffer[i].CompareTag("Enemy")) {
                enemyCount++;
                float d = Vector2.Distance(player.position, hitBuffer[i].transform.position);
                if (d < closestDist) closestDist = d;
                
                // 적 타입 확인 (EnemyAudioProfile이 있다면)
                var profile = hitBuffer[i].GetComponent<EnemyAudioProfile>(); // 만약 없다면 주석처리
                /* if (profile != null) {
                    if (profile.type == EnemySoundType.Heavy) heavyCnt++;
                    else if (profile.type == EnemySoundType.Speed) speedCnt++;
                    else if (profile.type == EnemySoundType.Weird) weirdCnt++;
                }
                */
            }
        }
        
        // 비율 계산
        float total = Mathf.Max(1, enemyCount);
        synth.heavyMix = Mathf.Lerp(synth.heavyMix, (float)heavyCnt / total, Time.deltaTime);
        synth.speedMix = Mathf.Lerp(synth.speedMix, (float)speedCnt / total, Time.deltaTime);
        synth.weirdMix = Mathf.Lerp(synth.weirdMix, (float)weirdCnt / total, Time.deltaTime);

        UpdateConductor(enemyCount, closestDist);
    }

    void UpdateConductor(int enemyCount, float closestDist)
    {
        float distFactor = (enemyCount > 0) ? Mathf.Clamp01(1.0f - (closestDist / detectionRadius)) : 0f;

        int targetTension = (enemyCount == 0) ? 0 : ((enemyCount < 3) ? 1 : 2);
        synth.tensionLevel = targetTension;
        synth.flameMode = isFlameActivated;
        
        double targetBPM = baseBPM;
        if (isFlameActivated) targetBPM = baseBPM + 20.0; 
        else targetBPM = baseBPM + (enemyCount * 2.0) + (distFactor * 5.0); 

        synth.bpm = Mathf.Lerp((float)synth.bpm, (float)targetBPM, Time.deltaTime);
        
        // 자동 작곡 파라미터 조절
        float targetChaos = 0.2f + (enemyCount * 0.15f);
        float targetDensity = 0.5f + (distFactor * 0.4f);
        
        synth.chaos = Mathf.Lerp(synth.chaos, Mathf.Clamp01(targetChaos), Time.deltaTime);
        synth.density = Mathf.Lerp(synth.density, Mathf.Clamp01(targetDensity), Time.deltaTime);
    }
}