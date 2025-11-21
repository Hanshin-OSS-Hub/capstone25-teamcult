using UnityEngine;

public class MusicDirector : MonoBehaviour
{
    public Transform player;
    public CodeManager codeManager;
    public float detectionRadius = 15f;

    private int lastEnemyCount = -1;
    private float lastDistance = 0f;
    
    // 최적화용 쿨타임 (이제 비동기라 1초로 줄여도 됩니다!)
    private float compileTimer = 0f;
    private float compileCooldown = 1.0f; 

    void Update()
    {
        if (player == null) return;
        compileTimer += Time.deltaTime;

        int currentEnemyCount = 0;
        float closestDist = 100f;

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

        bool enemyChanged = currentEnemyCount != lastEnemyCount;
        // 거리 변화 민감도 1.5m
        bool distChanged = Mathf.Abs(closestDist - lastDistance) > 1.5f; 

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
        float tension = Mathf.Clamp01(1.0f - (distance / detectionRadius));
        double bpm = 130.0;

        // ============================================================
        // Track 0: KICK (단단하게 고정)
        // ============================================================
        string kickCode = $@"
            double beatTime = time * ({bpm} / 60.0);
            double step = beatTime % 1.0;
            double env = Math.Exp(-step * 15.0); 
            return Math.Tanh(Math.Sin(step * 60.0 * Math.Exp(-step*25.0)) * 10.0) * env;
        ";
        codeManager.ApplyCodeToTrack(0, kickCode);


        // ============================================================
        // Track 1: BASS (적극적 활용 + 사이드체인 효과)
        // ============================================================
        string bassCode;
        if (enemyCount == 0)
        {
            bassCode = "return Math.Sin(phase * 0.5) * 0.8;"; 
        }
        else
        {
            // [테크노 베이스 핵심]
            // 1. 톱니파 2개를 살짝 엇나가게 섞음 (Detune 효과) -> 소리가 두꺼워짐
            // 2. Sidechain: 킥이 나오는 타이밍(step < 0.2)에 볼륨을 확 줄임
            bassCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double step = beatTime % 1.0;
                
                // 1. 두꺼운 톱니파 (Sawtooth) 만들기
                double saw1 = ((phase * 0.5) % 1.0) * 2.0 - 1.0;
                double saw2 = ((phase * 0.505) % 1.0) * 2.0 - 1.0; // 0.005만큼 피치 어긋나게
                double rawBass = (saw1 + saw2) * 0.5;

                // 2. 필터 효과 (Low Pass Filter) - 긴박할수록 뚜껑이 열림
                double filter = rawBass * ({tension} * 0.8 + 0.2) + Math.Sin(phase*0.5) * (1.0 - {tension});

                // 3. 사이드체인 (Sidechain Compression) - 킥이 칠때 베이스가 숨죽임
                // 박자의 앞부분(0.0~0.3)에서는 볼륨이 작았다가 커짐
                double ducking = (step < 0.3) ? (step / 0.3) : 1.0;

                return filter * ducking * 0.8;
            ";
        }
        codeManager.ApplyCodeToTrack(1, bassCode);


        // ============================================================
        // Track 2: LEAD (화려함 유지)
        // ============================================================
        string leadCode;
        if (enemyCount == 0)
        {
            leadCode = "return 0.0;";
        }
        else
        {
            string noteSpeed = (tension > 0.6f) ? "8.0" : "4.0"; 
            leadCode = $@"
                double beatTime = time * ({bpm} / 60.0);
                double seq = Math.Floor(beatTime * {noteSpeed}) % 4.0;
                
                double pitchMult = 1.0;
                if(seq == 1.0) pitchMult = 1.2; 
                if(seq == 2.0) pitchMult = 1.5; 
                if(seq == 3.0) pitchMult = 2.0; 
                
                double osc = (Math.Sin(phase * pitchMult) > 0.0) ? 1.0 : -1.0;
                double subStep = (beatTime * {noteSpeed}) % 1.0;
                double env = Math.Exp(-subStep * 10.0);
                return osc * env * 0.15; 
            ";
        }
        codeManager.ApplyCodeToTrack(2, leadCode);
    }
    
    void OnDrawGizmos() { if(player!=null) Gizmos.DrawWireSphere(player.position, detectionRadius); }
}