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

    // [수정] 다시 Glitch 호출로 변경
    public void TriggerDamageEffect() 
    { 
        if (synth != null) synth.TriggerGlitch(); 
    }

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
        UpdateMusicState(enemyCount, closestDist);
    }

    void UpdateMusicState(int enemyCount, float closestDist)
    {
        float distFactor = (enemyCount > 0) ? Mathf.Clamp01(1.0f - (closestDist / detectionRadius)) : 0f;
        
        int targetTension = 0;
        if (enemyCount == 0) targetTension = 0; 
        else if (enemyCount < 3) targetTension = 1; 
        else targetTension = 2; 

        synth.tensionLevel = targetTension;
        synth.flameMode = isFlameActivated;

        double targetBPM = 160.0; 
        if (isFlameActivated) targetBPM = 170.0;
        else targetBPM = 150.0 + (enemyCount * 2.0) + (distFactor * 5.0);
        
        synth.bpm = Mathf.Lerp((float)synth.bpm, (float)targetBPM, Time.deltaTime);
    }
}