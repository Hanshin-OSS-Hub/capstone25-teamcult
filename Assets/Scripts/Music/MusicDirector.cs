using UnityEngine;

public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance;

    [Header("References")]
    public Transform player;
    public DnBSynth synth; 
    // HealthBarManager 연결 부분 삭제됨

    [Header("Detection")]
    public float detectionRadius = 15f;
    
    // 최적화를 위한 변수들
    private Collider2D[] hitBuffer = new Collider2D[10];
    private ContactFilter2D contactFilter;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        
        // 충돌 필터 초기화
        contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
    }

    void Update()
    {
        if (synth == null) return;
        
        // 플레이어 찾기
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        // 1. 적 감지 (Physics2D.OverlapCircle 사용)
        int count = Physics2D.OverlapCircle(player.position, detectionRadius, contactFilter, hitBuffer);
        
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

        // 2. 음악 상태 업데이트 (체력 인자 제거됨)
        UpdateMusicState(enemyCount, closestDist);
    }

    void UpdateMusicState(int enemyCount, float distance)
    {
        // 긴장도 계산 (0.0 ~ 1.0)
        float tension = 0f;
        if (enemyCount > 0) tension = Mathf.Clamp01(1.0f - (distance / detectionRadius));

        // BPM 조절: 적이 가까울수록 빨라짐 (170 -> 180)
        double targetBPM = 170.0 + (tension * 10.0);
        synth.bpm = Mathf.Lerp((float)synth.bpm, (float)targetBPM, Time.deltaTime);

        // Tension Level 설정 (악기 패턴 변화)
        // 0: 평화 (미니멀 멜로디)
        // 1: 전투 (킥 추가, 피치 상승)
        // 2: 위기 (스네어 롤링, 옥타브 상승)
        if (enemyCount == 0) synth.tensionLevel = 0; 
        else if (tension < 0.6f) synth.tensionLevel = 1; 
        else synth.tensionLevel = 2; 

        // [체력 연동 제거에 따른 고정값 설정]
        // 빈사 상태 효과 끄기
        synth.lowHealthMode = false;
        
        // 필터는 항상 깨끗하게 열어둠
        synth.cutoffFrequency = Mathf.Lerp(synth.cutoffFrequency, 22000f, Time.deltaTime * 5.0f);
        
        // 베이스 디스토션은 여전히 긴장도에 따라 반응하도록 유지 (박진감 위해)
        synth.bassDistortion = tension * 0.3f;
    }
    
    void OnDrawGizmos() {
        if(player != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, detectionRadius);
        }
    }
}