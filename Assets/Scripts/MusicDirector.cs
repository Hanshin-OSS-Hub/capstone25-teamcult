using UnityEngine;

public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance; // 싱글톤 인스턴스

    [Header("References")]
    public Transform player;       // 플레이어 (자동으로 찾음)
    public CodeManager codeManager; // 오디오 합성 엔진

    [Header("Settings")]
    public float detectionRadius = 15f;
    private int lastEnemyCount = -1;
    private float lastDistance = 0f;
    
    // 컴파일 부하 조절용
    private float compileTimer = 0f;
    private float compileCooldown = 0.5f; // 반응 속도를 조금 높였습니다.

    void Awake()
    {
        // 1. 씬 이동 시 파괴 방지 (싱글톤)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    void Update()
    {
        // 2. 씬이 바뀌어서 플레이어 연결이 끊겼으면 다시 찾기 (중요!)
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            else return; // 플레이어 없으면 아무것도 안 함
        }

        compileTimer += Time.deltaTime;

        int currentEnemyCount = 0;
        float closestDist = 100f;

        // 적 감지 로직
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                currentEnemyCount++;
                float d = Vector2.Distance(player.position, hit.transform.position);
                if (d < closestDist) closestDist = d;
            }
        }

        // 상태 변화 감지 (적 숫자 혹은 거리 변화)
        bool enemyChanged = currentEnemyCount != lastEnemyCount;
        bool distChanged = Mathf.Abs(closestDist - lastDistance) > 1.0f; // 민감도 살짝 조정

        // 쿨타임이 찼고, 상황이 변했을 때만 코드 생성
        if (compileTimer > compileCooldown && (enemyChanged || distChanged))
        {
            lastEnemyCount = currentEnemyCount;
            lastDistance = closestDist;
            compileTimer = 0f;

            if (currentEnemyCount == 0) closestDist = 20f;
            ComposeTechno(currentEnemyCount, closestDist);
        }
    }

    void ComposeTechno(int enemyCount, float distance)
    {
        // 긴장도: 0.0 (멀음) ~ 1.0 (코앞)
        float tension = Mathf.Clamp01(1.0f - (distance / detectionRadius));
        double bpm = 130.0;

        // ============================================================
        // Track 0: KICK (리듬 변주 추가됨!)
        // ============================================================
        string kickCode;
        
        if (enemyCount == 0)
        {
            // [평화] 쿵... 쿵... (심장박동처럼 느리게, 2박자에 한번)
            // beatTime % 2.0을 사용해서 2배 느리게
            kickCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double step = beatTime % 2.0; 
                double env = Math.Exp(-step * 10.0); 
                // 소리도 부드럽게 (Tanh 강도 낮춤)
                return Math.Tanh(Math.Sin(step * 40.0) * 5.0) * env * 0.6;
            ";
        }
        else if (tension < 0.7f)
        {
            // [전투] 쿵! 쿵! 쿵! 쿵! (전형적인 테크노 4/4박자)
            kickCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double step = beatTime % 1.0;
                double env = Math.Exp(-step * 15.0); 
                // 펀치감 있는 소리
                return Math.Tanh(Math.Sin(step * 60.0 * Math.Exp(-step*25.0)) * 10.0) * env;
            ";
        }
        else
        {
            // [위기] 쿵쿵쿵쿵! (8비트로 쪼개짐, 속도 2배)
            // beatTime * 2.0을 해서 템포를 2배로
            kickCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double step = (beatTime * 2.0) % 1.0; 
                double env = Math.Exp(-step * 20.0); // 꼬리를 짧게
                // 아주 강하고 찌그러진 킥
                return Math.Tanh(Math.Sin(step * 50.0) * 20.0) * env * 0.9;
            ";
        }
        codeManager.ApplyCodeToTrack(0, kickCode);


        // ============================================================
        // Track 1: BASS (기존 로직 유지 + 긴장감 강화)
        // ============================================================
        string bassCode;
        if (enemyCount == 0)
        {
            bassCode = "return Math.Sin(phase * 0.5) * 0.5;"; 
        }
        else
        {
            // 긴장도가 높으면 베이스가 더 '와블'거리는 속도를 높임
            string lfoSpeed = (tension > 0.8) ? "8.0" : "4.0";
            
            bassCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double step = beatTime % 1.0;
                
                // 1. Detune Sawtooth
                double saw1 = ((phase * 0.5) % 1.0) * 2.0 - 1.0;
                double saw2 = ((phase * 0.505) % 1.0) * 2.0 - 1.0; 
                double rawBass = (saw1 + saw2) * 0.5;

                // 2. Filter (긴장도에 따라 더 많이 열림)
                double filterMod = Math.Sin(beatTime * {lfoSpeed}); 
                double filter = rawBass * ({tension} * 0.7 + 0.3 + filterMod * 0.1);

                // 3. Sidechain (킥 리듬이 바뀌면 사이드체인도 바뀌어야 함)
                // 위기 상황(tension > 0.7)에서는 사이드체인을 더 짧고 강하게 
                double duckLen = ({tension} > 0.7) ? 0.15 : 0.3;
                double ducking = (step < duckLen) ? (step / duckLen) : 1.0;

                return filter * ducking * 0.8;
            ";
        }
        codeManager.ApplyCodeToTrack(1, bassCode);


        // ============================================================
        // Track 2: LEAD (화려함)
        // ============================================================
        string leadCode;
        if (enemyCount == 0)
        {
            leadCode = "return 0.0;";
        }
        else
        {
            // 거리가 가까워질수록 아르페지오 속도가 미친듯이 빨라짐
            string noteSpeed = (tension > 0.6f) ? "8.0" : "4.0"; 
            if(tension > 0.9f) noteSpeed = "16.0"; // 초근접 시 16비트

            leadCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double seq = Math.Floor(beatTime * {noteSpeed}) % 4.0;
                
                double pitchMult = 1.0;
                // 랜덤성 대신 수식으로 멜로디 생성
                if(seq == 1.0) pitchMult = 1.5; // 5도
                if(seq == 2.0) pitchMult = 1.2; // 단3도
                if(seq == 3.0) pitchMult = 2.0; // 옥타브
                
                // Square Wave (게임음악 느낌)
                double osc = (Math.Sin(phase * pitchMult) > 0.0) ? 1.0 : -1.0;
                
                double subStep = (beatTime * {noteSpeed}) % 1.0;
                double env = Math.Exp(-subStep * 10.0);
                
                // 긴장도가 높으면 딜레이(Echo) 느낌 추가
                double echo = 0.0;
                if({tension} > 0.5) echo = Math.Sin(phase * pitchMult * 0.99) * 0.3;

                return (osc + echo) * env * 0.15; 
            ";
        }
        codeManager.ApplyCodeToTrack(2, leadCode);
    }
    
    // 디버그용 원 그리기
    void OnDrawGizmos() { 
        if(player!=null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, detectionRadius); 
        }
    }
}